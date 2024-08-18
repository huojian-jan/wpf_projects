using ShadowBot.Common.ErrorHandling;
using ShadowBot.Common.LocalizationResources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ShadowBot.Common.Utilities
{
    public static class Zip
    {
        private static readonly string _exeFilePath;

        static Zip()
        {
            _exeFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7za.exe");
            if (!File.Exists(_exeFilePath))
            {
                string text = string.Format($"{Strings.Zip_FoundThatTheNecessaryFileDoesNotExist}!", _exeFilePath);
                MessageBox.Show(text, $"{Strings.Zip_Hint}", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        public static void Pack(string dstCompressedFile, string srcFolderOrFile, bool deleteSourceFiles = false,
            string[] excludes = null, bool createZipType = true, int compressionLevel = 0, string password = null)
        {
            bool isDir = File.GetAttributes(srcFolderOrFile).HasFlag(FileAttributes.Directory);
            var args = new List<string>
            {
                "a",
                createZipType?"-tzip":string.Empty,
                "-mx"+compressionLevel.ToString(),
                string.Format("\"{0}\"", dstCompressedFile),
                isDir?string.Format("\"{0}\"", Path.Combine(srcFolderOrFile, "*")) : srcFolderOrFile,
                deleteSourceFiles ? "-sdel" : string.Empty,
                !string.IsNullOrEmpty(password) ? $"-p{password}" : string.Empty,
            };

            if (excludes != null)
            {
                foreach (var exclude in excludes)
                    args.Add($"-x!{exclude}");
            }

            if (!Fork7z(args))
                throw new Exception($"{Strings.Zip_ZipFileCreationFailed}");
        }

        public static void UnPack(string srcCompressedFile, string targetFolder, string password = null)
        {
            string[] args = new string[]
            {
                "x",
                string.Format("\"{0}\"", srcCompressedFile),
                string.Format("-o\"{0}\"", targetFolder),
                "-y",
                !string.IsNullOrEmpty(password) ? $"-p{password}" : string.Empty
            };
            if (!Fork7z(args))
                throw new Exception($"{Strings.Zip_ZipFileDecompressionFailed}");
        }

        public static string ExtractTextFile(string zipFilePath, string relativePath, Encoding encoding, string password = null)
        {
            // ./7za.exe e xbot_robot.zip -so package.json
            string[] args = new string[]
            {
                "e",
                string.Format("\"{0}\"", zipFilePath),
                "-so",
                relativePath,
                !string.IsNullOrEmpty(password) ? $"-p{password}" : string.Empty
            };

            using (var process = new Process())
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _exeFilePath,
                    Arguments = string.Join(" ", args),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = encoding
                };
                process.StartInfo = startInfo;

                var output = new StringBuilder();
                var error = new StringBuilder();
                using (var outputWaitHandle = new AutoResetEvent(false))
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    var timeout = int.MaxValue - 1;
                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        if (process.ExitCode != 0)
                        {
                            StringBuilder err = new StringBuilder();
                            err.AppendLine($"7za fail, {string.Join(" ", args)}");
                            err.AppendLine(output.ToString());
                            err.AppendLine(error.ToString());
                            Logging.Error(err.ToString());
                            throw new Exception($"{Strings.Zip_ZipFileDecompressionFailed}");
                        }
                        return output.ToString();
                    }
                    else
                    {
                        throw new Exception("7za timeout");
                    }
                }
            }
        }

        private static bool Fork7z(IEnumerable<string> args)
        {
            using (var process = new Process())
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = _exeFilePath,
                    Arguments = string.Join(" ", args),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                process.StartInfo = startInfo;

                var output = new StringBuilder();
                var error = new StringBuilder();
                using (var outputWaitHandle = new AutoResetEvent(false))
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            outputWaitHandle.Set();
                        }
                        else
                        {
                            output.AppendLine(e.Data);
                        }
                    };
                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (e.Data == null)
                        {
                            errorWaitHandle.Set();
                        }
                        else
                        {
                            error.AppendLine(e.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    var timeout = int.MaxValue - 1;
                    if (process.WaitForExit(timeout) &&
                        outputWaitHandle.WaitOne(timeout) &&
                        errorWaitHandle.WaitOne(timeout))
                    {
                        if (process.ExitCode != 0)
                        {
                            StringBuilder err = new StringBuilder();
                            err.AppendLine($"7za fail, {string.Join(" ", args)}");
                            err.AppendLine(output.ToString());
                            var errorStr = error.ToString();
                            err.AppendLine(errorStr);
                            Logging.Error(err.ToString());
                            var arr = errorStr.Split(Environment.NewLine);
                            ErrorCenter.ProcessFileError(arr);
                            return false;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        throw new Exception("7za timeout");
                    }
                }
            }
        }
    }
}
