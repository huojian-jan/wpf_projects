using System.Diagnostics;
using Huojian.LibraryManagement.Common.IPC;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace Huojian.LibraryManagement.Common.Utilities
{
    public class AppMutex : IDisposable
    {
        private readonly string _name;
        private readonly string _pipeNameFilePath;   // support session id
        private readonly string _pipeNameRegKey;     // support session id

        private IPCServer _ipc;

        private static Mutex _mutex = new Mutex(false, "ShadowBot");
        private static readonly string _ipcNameSuffix = ".mutex";

        public AppMutex(string name)
        {
            _name = name;
            _pipeNameFilePath = Path.Combine(Path.GetTempPath(), $"{_name}{_ipcNameSuffix}");
            _pipeNameRegKey = $"{_name}.{Process.GetCurrentProcess().SessionId}{_ipcNameSuffix}";
        }

        private string ReadPipeName()
        {
            string _regRead()
            {
                if (OperatingSystem.IsWindows())
                {
                    RegistryKey readKey = Registry.CurrentUser.OpenSubKey("Volatile Environment");
                    if (readKey != null)
                    {
                        return (string)readKey.GetValue(_pipeNameRegKey);
                    }
                }

                return string.Empty;
            }

            try
            {
                if (File.Exists(_pipeNameFilePath))
                {
                    var pipeName = File.ReadAllText(_pipeNameFilePath);
                    if (!string.IsNullOrEmpty(pipeName))
                    {
                        return pipeName;
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Read IPC pipe name from file error.", ex);
            }

            try
            {
                var regPipeName = _regRead();
                if (!string.IsNullOrEmpty(regPipeName))
                {
                    return regPipeName;
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Read IPC pipe name from reg error.", ex);
            }

            return string.Empty;
        }

        private bool WritePipeName(string value)
        {
            bool _fileWrite(string value)
            {
                try
                {
                    File.WriteAllText(_pipeNameFilePath, value);
                    return true;
                }
                catch (Exception ex)
                {
                    Logging.Error("Write IPC pipe name to file failed.", ex);
                }

                return false;
            }

            bool _regWrite(string value)
            {
                try
                {
                    if (OperatingSystem.IsWindows())
                    {

                        RegistryKey key = Registry.CurrentUser.CreateSubKey("Volatile Environment");
                        key.SetValue(_pipeNameRegKey, value);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logging.Error("Write IPC pipe name to reg failed.", ex);
                }


                return false;
            }

            var result = _fileWrite(value);
            result |= _regWrite(value);

            return result;
        }

        public bool GetProcessLock(int timeout = 1000)
        {
            // 如果在1秒内都没能获取到锁，表示锁正在被一个正在打开的影刀程序持有
            return _mutex.WaitOne(timeout);
        }

        public void ReleaseProcessLock()
        {
            _mutex.ReleaseMutex();
        }

        /// <summary>
        /// 标记本次影刀程序已完全打开
        /// </summary>
        /// <returns></returns>
        public bool RemarkOpened()
        {
            try
            {
                var pipeName = Guid.NewGuid().ToString("N");
                _ipc = new IPCServer();
                _ipc.Handler = new MutexIPCHandler(this);
                _ipc.Start(pipeName);

                WritePipeName(pipeName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool SendMessage(string method, object @params = null)
        {
            var pipeName = ReadPipeName();
            if (string.IsNullOrWhiteSpace(pipeName))
            {
                return false;
            }

            var client = new IPCClient();
            try
            {
                client.Connect(pipeName, 200);
                client.Send(method, @params);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                /* fixbug-628
                 * 如果正在运行的实例是管理员权限打开的，而当前是普通用户身份打开的
                 * 管道连接时就会出现权限不足，这种认为是有实例正在运行
                 */
                Logging.Warn("AppMutex notification failed due to insufficient permissions");
                return true;
            }
            catch (TimeoutException)
            {
                // 默认行为：当前无影刀正在运行
                return false;
            }
            catch (Exception ex)
            {
                Logging.Warn("AppMutex notification failed", ex);
                return false;
            }
            finally
            {
                client.Close();
            }
        }

        public void Dispose()
        {
            _ipc?.Stop();
        }

        private void RaiseMessageRecieved(string @method, object @params)
        {
            MessageRecieved?.Invoke(this, new MessageRecievedEventArgs(@method, @params));
        }

        public event EventHandler<MessageRecievedEventArgs> MessageRecieved;

        public class MessageRecievedEventArgs : EventArgs
        {
            public MessageRecievedEventArgs(string @method, object @params)
            {
                Method = @method;
                Params = @params;
            }
            public string Method { get; }
            public object Params { get; }
        }

        class MutexIPCHandler : IPCHandler
        {
            private readonly AppMutex _appMutex;

            public MutexIPCHandler(AppMutex appMutex)
            {
                _appMutex = appMutex;
            }

            public override void ProcessRequest(IPCContext context)
            {
                try
                {
                    _appMutex.RaiseMessageRecieved(context.Request.Method, context.Request.Params);
                    context.Response = IPCResponse.OK("Recieved");
                }
                catch (IPCException ex)
                {
                    context.Response = IPCResponse.InternalFailure(ex);
                }
                catch (Exception ex)
                {
                    context.Response = IPCResponse.InternalFailure(new IPCException(IPCResponseCode.InternalServerError, $"{Strings.AppMutex_IPCServiceFailedToProcessRequest}"));
                    Logging.Error($"IPCServer--RequestHandler Error", ex);
                }
            }
        }
    }
}
