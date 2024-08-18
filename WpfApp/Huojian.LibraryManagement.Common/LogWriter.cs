using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Huojian.LibraryManagement.Common
{
    public class LogCofig
    {
        // 根据实际场景替换WebApiHost，为海外私有云等不同场景提供配置便利
        public string WebApiHost = "";

        // Mute设置为True，则不会开启发送任务，随时可以设置
        public bool Mute = false;

        // 分组发送，默认开启，当关闭时，直接发送，如果发送失败，需要上层业务重发
        public bool SendLogByGroup = true;

        // 修改发送Log的分组大小，太大容易导致Http发送失败
        public uint SendGroupSize = 10;

        // HttpClient超时时间，修改次时间影响后续的发送超时
        public int HttpTimeoutInSecond = 3;
    }

    public static class LogHelper
    {
        public static LogCofig Config = new LogCofig();

        private static string _apiPath = $"api/bi/noauth/v2/log/report";

        private static LogWriter _writer = null;

        public static bool WriteInfo(string message, string exception = "")
        {
            return TryWrite("info", message, exception);
        }

        public static bool WriteError(string message, string exception = "")
        {
            return TryWrite("error", message, exception);
        }

        public static bool WriteWarn(string message, string exception = "")
        {
            return TryWrite("warn", message, exception);
        }

        public static bool WriteDebug(string message, string exception = "")
        {
            return TryWrite("debug", message, exception);
        }

        public static bool Write(string level, string message, string exception = "")
        {
            return TryWrite(level, message, exception);
        }

        public static bool ForceFlush()
        {
            if (_writer == null)
            {
                return false;
            }

            return _writer.ForceFlush(Config.HttpTimeoutInSecond);
        }

        public static void UpdateContext(string mid, string nickName)
        {
            if (_writer != null)
            {
                _writer.Mid = Base64Helper.ConvertBase64IfNotAllAscii(mid);
                _writer.NickName = Base64Helper.ConvertBase64IfNotAllAscii(nickName);
            }
        }

        private static bool TryWrite(string level, string message, string exception)
        {
            var log = new RemoteLog()
            {
                Level = level,
                Message = message,
                Domain = Assembly.GetEntryAssembly().ManifestModule.Name,
                Exception = exception,
                Date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            };


            return WriteRemoteLog(log);
        }

        public static bool WriteRemoteLog(RemoteLog remoteLog)
        {
            if (Config.Mute)
            {
                return true;
            }

            MakeSureLogWriterReady();

            return _writer.WriteRemoteLog(remoteLog, !Config.SendLogByGroup);
        }

        private static void MakeSureLogWriterReady()
        {
            var remoteUrl = string.Empty;
            if (!string.IsNullOrEmpty(Config.WebApiHost))
            {
                remoteUrl = $"{Config.WebApiHost.TrimEnd('/')}/{_apiPath}";
            }

            if (_writer == null)
            {
                _writer = new LogWriter(remoteUrl, Guid.NewGuid().ToString(), string.Empty);
            }

            _writer.Start();
            _writer.RemoteUri = remoteUrl;
            _writer.WriterTimeoutInSecond = Config.HttpTimeoutInSecond;
            _writer.SendGroupSize = Config.SendGroupSize;
        }
    }

    public class LogWriter
    {
        public string RemoteUri { get; set; } = string.Empty;
        public readonly string Version = string.Empty;
        public readonly string ModuleName = string.Empty;
        public readonly string Arch = string.Empty;
        public readonly string OS = string.Empty;
        public readonly string OSArch = string.Empty;
        public readonly string Kind = string.Empty;

        public string NickName = string.Empty;
        public string Mid = string.Empty;

        public uint SendGroupSize = 10;
        public int StatsFlushTickerInSecond = 30;
        public int WriterTimeoutInSecond = 10;

        private const int LogWriteTimeoutInSecond = 3;

        private bool _inSending = false;

        private bool _backgroundExitFlag = false;

        private Thread _sendThread = null;

        private ConcurrentQueue<RemoteLog> _logsQueue = new();

        private System.Timers.Timer _flushTimer { get; set; }

        private bool _isRunning = false;

        public LogWriter(string remoteURL, string mid, string nickName)
        {
            RemoteUri = remoteURL;
            Mid = mid;
            NickName = nickName;

            try
            {
                OS = RuntimeInformation.OSDescription.Replace(" ", "_");
                if (Environment.Is64BitOperatingSystem)
                {
                    OSArch = "x64";
                }
                else
                {
                    OSArch = "x86";
                }

                switch (RuntimeInformation.ProcessArchitecture)
                {
                    case Architecture.X64:
                    case Architecture.Arm64:
                        Arch = "x64";
                        break;
                    case Architecture.X86:
                    case Architecture.Arm:
                        Arch = "x32";
                        break;
                    default:
                        Arch = "x86";
                        break;
                }

                ModuleName = Assembly.GetEntryAssembly().ManifestModule.Name;
                Version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            }
            catch (Exception ex)
            {
                Logging.Warn("LogWrite init warn.", ex);
            }
        }

        public bool Start()
        {
            if (_isRunning) { return true; }

            try
            {
                StartTicker();
                InitBackgroundThread();
                _isRunning = true;
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn("log write start failed.", ex);
            }

            return false;
        }

        public bool Stop()
        {
            if (!_isRunning) { return true; }

            try
            {
                StopBackgroundThread();
                StopTicker();
                ForceFlush(LogWriteTimeoutInSecond);

                _isRunning = false;
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn("log write stop failed.", ex);
            }

            return false;
        }

        public bool WriteRemoteLog(RemoteLog remoteLog, bool sendImediately)
        {
            if (sendImediately)
            {
                var logs = new List<RemoteLog> { remoteLog };
                return WriteRemote(logs, LogWriteTimeoutInSecond);
            }
            else
            {
                _logsQueue.Enqueue(remoteLog);
                return true;
            }
        }

        private bool WriteRemote(List<RemoteLog> remoteLogs, int timeout)
        {
            // NOTE(huifu) 如果还未配置RemoteURI时，未发送的数据会存在缓存中，等待正确配置后发送
            // 一直未配置正确的URI，则堆积到一定程度后会清空内存
            if (string.IsNullOrEmpty(RemoteUri))
            {
                return false;
            }

            try
            {
                var logContent = JsonSerializer.Serialize(remoteLogs);
                var result = HttpRequestPost(RemoteUri, logContent, timeout);
                return !string.IsNullOrEmpty(result);
            }
            catch (Exception ex)
            {
                Logging.Warn("write remote failed.", ex);
            }

            return false;
        }

        private void InjectRuntimeContext(HttpClient httpClient)
        {
            HttpClientHeadConfigHelper(httpClient, "xbot-protocol-ver", "1.0");
            HttpClientHeadConfigHelper(httpClient, "xbot-client-ver", Version);
            HttpClientHeadConfigHelper(httpClient, "xbot-client-arch", Arch);
            HttpClientHeadConfigHelper(httpClient, "xbot-os", OS);
            HttpClientHeadConfigHelper(httpClient, "xbot-os-arch", OSArch);
            HttpClientHeadConfigHelper(httpClient, "xbot-mid", Mid);
            HttpClientHeadConfigHelper(httpClient, "xbot-nk", NickName);
            HttpClientHeadConfigHelper(httpClient, "xbot-kind", Kind);
            HttpClientHeadConfigHelper(httpClient, "xbot-module", ModuleName);

            void HttpClientHeadConfigHelper(HttpClient httpClient, string key, string value)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    httpClient.DefaultRequestHeaders.Add(key, value == null ? string.Empty : value);
                }
            }
        }

        private string HttpRequestPost(string url, string jsonData, int timeout)
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();

                // 请求体没有启用gzip压缩，需要解决
                using var client = new HttpClient(handler);
                InjectRuntimeContext(client);

                var data = new StringContent(jsonData, Encoding.UTF8, "application/json");
                client.Timeout = TimeSpan.FromSeconds(timeout);

                var responseTask = client.PostAsync(url, data);

                responseTask.Wait(TimeSpan.FromSeconds(timeout));

                var responseResult = responseTask.Result;
                if (responseResult.IsSuccessStatusCode)
                {
                    var responseDataTask = responseResult.Content.ReadAsStringAsync();
                    if (responseDataTask.Wait(TimeSpan.FromSeconds(3)))
                    {
                        return responseDataTask.Result;
                    }
                }
                else
                {
                    Logging.Debug($"Send log error. StatusCode: {responseResult.StatusCode} , {responseResult.ReasonPhrase}");
                }
            }
            catch (TaskCanceledException e)
            {
                Logging.Warn("HttpRequestPost timeout error.", e);
            }
            catch (Exception e)
            {
                Logging.Warn("HttpRequestPost error.", e);
            }

            return null;
        }

        private bool InitBackgroundThread()
        {
            if (_sendThread == null)
            {
                _sendThread = new(() =>
                {
                    while (true)
                    {
                        if (_backgroundExitFlag)
                        {
                            Logging.Info("log-thread exit.");
                            break;
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(3));

                        if (_inSending)
                        {
                            _inSending = false;
                            DoSend();
                        }
                    }
                })
                {
                    Name = "log-thread",
                    IsBackground = true
                };
            }

            _sendThread.Start();
            return true;
        }

        private void StartTicker()
        {
            if (_flushTimer == null)
            {
                var tickerCount = 1000 * StatsFlushTickerInSecond;
                _flushTimer = new System.Timers.Timer(tickerCount);
                _flushTimer.AutoReset = true;
                _flushTimer.Elapsed += FlushTimer_Tick;
            }

            _flushTimer.Start();
        }

        private void StopTicker()
        {
            if (_flushTimer != null)
            {
                _flushTimer.Stop();
                _flushTimer = null;
            }
        }

        private void FlushTimer_Tick(object sender, EventArgs e)
        {
            _inSending = true;
        }

        private void StopBackgroundThread()
        {
            _backgroundExitFlag = true;
        }

        public bool ForceFlush(int timeout)
        {
            if (_logsQueue.Count == 0) return true;

            var flushTask = Task.Run(new Action(() =>
            {
                Logging.Info("Start log write flush");
                DoSend();
            }));

            return flushTask.Wait(TimeSpan.FromSeconds(timeout));
        }

        private void DoSend()
        {
            try
            {
                if (_logsQueue.Count == 0)
                {
                    return;
                }

                // NOTE(huifu) 如果这里的消息数量太多, 直接丢弃，避免累计太多撑爆内存
                if (_logsQueue.Count > 1000)
                {
                    _logsQueue.Clear();
                    return;
                }

                while (_logsQueue.Count() > 0)
                {
                    if (!SafeRemoteLogWrite(_logsQueue, SendGroupSize))
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Warn("Send remote log error.", e);
            }
        }

        private bool SafeRemoteLogWrite(ConcurrentQueue<RemoteLog> logs, uint limitedSize)
        {
            List<RemoteLog> cached = new List<RemoteLog>();
            for (uint i = 0; i < limitedSize; i++)
            {
                if (logs.IsEmpty)
                {
                    break;
                }

                if (logs.TryDequeue(out RemoteLog result))
                {
                    if (result == null)
                    {
                        break;
                    }

                    cached.Add(result);
                }
                else
                {
                    break;
                }
            }

            if (cached.Count == 0)
            {
                return true;
            }

            if (!WriteRemote(cached, WriterTimeoutInSecond))
            {
                cached.ForEach(x => _logsQueue.Enqueue(x));
                return false;
            }

            return true;
        }
    }

    public class RemoteLog
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("level")]
        public string Level { get; set; }

        [JsonPropertyName("exception")]
        public string Exception { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        [JsonPropertyName("tid")]
        public string Tid { get; set; }

        [JsonPropertyName("jobUuid")]
        public string JobUuid { get; set; }

        [JsonPropertyName("fileName")]
        public string FileName { get; set; }

        [JsonPropertyName("fileLine")]
        public string FileLine { get; set; }
    }
}
