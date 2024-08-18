using Newtonsoft.Json.Linq;

namespace Huojian.LibraryManagement.Common.IPC
{
    public class IPCClient
    {
        private readonly object _lock = new object();

        private int _state = 0; // 0:dis-connected, 1:connected
        private ClientPipe _pipe;

        public int State => _state;

        public int ReadTimeout { get; set; } = -1;

        public int WriteTimeout { get; set; } = -1;

        public JObject Options { get; } = new JObject();

        public void Connect(string pipeName, int timeout)
        {
            if (_state == 1)
            {
                return;
            }
            else
            {
                lock (_lock)
                {
                    if (_state == 1)
                    {
                        return;
                    }
                    else
                    {
                        _pipe = ClientPipe.Connect(pipeName, timeout);
                        _state = 1;
                    }
                }
            }
        }

        public IPCResponse Send(string command, object @params = null)
        {
            if (_state == 0)
            {
                throw new InvalidOperationException("ipc-client is not connected");
            }
            else
            {
                var request = new IPCRequest
                {
                    Method = command,
                    Params = @params == null ? null : JToken.FromObject(@params),
                    Options = Options
                };
                lock (_lock)
                {
                    if (_state == 0)
                    {
                        throw new InvalidOperationException("ipc-client is not connected");
                    }
                    else
                    {
                        _pipe.WriteRequest(request, WriteTimeout);
                        var response = _pipe.ReadResponse(ReadTimeout);
                        return response;
                    }
                }
            }
        }

        public T Send<T>(string command, object @params = null)
        {
            var response = Send(command, @params);
            if (!response.TryGetContent(out T content, out string error))
                throw new IPCException(IPCResponseCode.BadRequest, error);
            else
                return content;
        }

        public bool TestConnection()
        {
            try
            {
                return Send<string>("KeepAlive") == "KeepAlive";
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Close()
        {
            if (_state == 0)
            {
                return;
            }
            else
            {
                lock (_lock)
                {
                    if (_state == 0)
                    {
                        return;
                    }
                    else
                    {
                        if (_pipe != null)
                        {
                            try
                            {
                                //当processExplorer 中关闭这个管道句柄时， 会导致这里的dispose 报错
                                _pipe.Dispose();
                            }
                            catch (Exception e)
                            {
                                Logging.Error("pipe dispose error" + e.Message, e);
                            }

                            _pipe = null;
                        }
                        _state = 0;
                    }
                }
            }
        }
    }
}
