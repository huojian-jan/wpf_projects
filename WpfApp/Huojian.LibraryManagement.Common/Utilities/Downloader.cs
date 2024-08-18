using System.Diagnostics;

namespace ShadowBot.Common.Utilities
{
    public class Downloader
    {
        public Downloader() { }

        /// <summary>
        /// 下载超时时间，默认20分钟
        /// </summary>
        public int Timeout { get; set; } = 20 * 60 * 1000;

        /// <summary>
        /// 网络无响应的等待时间，默认5秒钟
        /// </summary>
        public int NoResponseTimeout { get; set; } = 10000;

        public Action<double> ProgressCallback { get; set; }

        public async Task DownloadAsync(Uri uri, string fileName, System.Threading.CancellationToken? token)
        {
            if (File.Exists(fileName))
                File.Delete(fileName);
            using (var fileStream = File.OpenWrite(fileName))
            {
                var monitorTask = DownloadMonitor(fileStream, token);
                var downloadTask = Download(uri, fileStream, token);
                var allTask = await Task.WhenAny(monitorTask, downloadTask);
                await allTask;
            }
        }

        private async Task Download(Uri uri, FileStream fileStream, System.Threading.CancellationToken? token)
        {
            var progressCallback = ProgressCallback;
            if (progressCallback == null)
                progressCallback = p => { };
            try
            {
                using (HttpClient _httpClient = new HttpClient())
                {
                    using (var response = await _httpClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead))
                    {
                        long length = response.Content.Headers.ContentLength ?? -1;
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var buffer = new byte[4096];
                            int read;
                            int totalRead = 0;
                            while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, read);
                                totalRead += read;
                                progressCallback((double)totalRead / length * 100);
                                token?.ThrowIfCancellationRequested();
                            }
                            Debug.Assert(totalRead == length || length == -1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task DownloadMonitor(FileStream fileStream, System.Threading.CancellationToken? token)
        {
            var length = 0L;
            var noResponseStopwatch = new Stopwatch();
            var timeoutStopwatch = new Stopwatch();
            timeoutStopwatch.Start();
            try
            {
                while (true)
                {
                    await Task.Delay(100);
                    token?.ThrowIfCancellationRequested();

                    // 超时检测
                    if (Timeout > 0 && (Timeout - timeoutStopwatch.ElapsedMilliseconds) <= 0)
                        throw new TimeoutException($"xx");
                    long fileLength;
                    try
                    {
                        fileLength = fileStream.Length;
                    }
                    catch (IOException)
                    {
                        // 应该是文件流已经被释放了，忽略此错误
                        break;
                    }
                    // 无响应检测(可能是断网情况)
                    if (fileLength > length)
                    {
                        length = fileLength;
                        noResponseStopwatch.Reset();
                    }
                    else
                    {
                        if (noResponseStopwatch.IsRunning)
                        {
                            if ((NoResponseTimeout - noResponseStopwatch.ElapsedMilliseconds) <= 0)
                            {
                                throw new TimeoutException($"xx");
                            }
                        }
                        else
                        {
                            noResponseStopwatch.Start();
                        }
                    }
                }
            }
            finally
            {
                timeoutStopwatch.Stop();
                noResponseStopwatch.Stop();
            }
        }
    }

}
