using Huojian.LibraryManagement.Common.ObjectModel;
using Microsoft.VisualBasic;

namespace Huojian.LibraryManagement.Common.IPC
{
    public class IPCServer
    {
        private readonly object _lock = new object();
        private readonly bool _allowMultiClient;
        private readonly object _promiseDictLock = new object();
        private readonly Dictionary<string, Promise> _promiseDict = new Dictionary<string, Promise>();

        //0:uninit, 1:running, 2:disposed
        enum state
        {
            uninit,
            running,
            disposed,
        };
        private state _state = state.uninit; // 0:uninit, 1:running, 2:disposed
        private ServerPipe _waitingPipe;

        public IPCServer(bool allowMultiClient = false)
        {
            _allowMultiClient = allowMultiClient;
            Handler = new IPCHandler();
        }

        public IPCHandler Handler { get; set; }

        /// <summary>
        /// 启动一个服务线程 (数据管道pipe + 操作handler)
        /// </summary>
        /// <param name="pipeName"></param>
        public void Start(string pipeName)
        {
            //提高效率，避免每次启动都抢占锁
            if (_state == state.running)
            {
                //如果服务已启动，无需创建新服务
                return;
            }
            else if (_state == state.disposed)
            {
                throw new InvalidOperationException("ipc-service is disposed");
            }
            else//double check，避免首次启动时多个线程同时进入
            {
                lock (_lock)
                {
                    if (_state == state.running)
                    {
                        return;
                    }
                    else if (_state == state.disposed)
                    {
                        throw new InvalidOperationException("ipc-service is disposed");
                    }
                    else
                    {
                        _state = state.running;

                        /* KHD-1041
                         * 这里不能使用Task.Run，因为Task.Run在线程不足时等待时长不可控，容易造成Python连接超时
                         * 暂时想到的方案是手动创建一个线程
                         * https://stackoverflow.com/a/6637964
                         */
                        var thread = new System.Threading.Thread(() =>
                        {
                            try
                            {
                                if (_allowMultiClient)
                                    WaitForMultiConnection(pipeName);
                                else
                                    WaitForConnection(pipeName);
                            }
                            catch (Exception ex)
                            {
                                Logging.Warn("ipc server wait for connection fail.", ex);
                            }
                        });
                        thread.IsBackground = true;
                        thread.Start();

                        //必须等待管道创建完成之后退出才安全
                        while (thread.ThreadState == ThreadState.Unstarted)
                        {
                            Thread.Sleep(1);
                        }
                        while (_state == state.running && _waitingPipe == null)
                        {
                            Thread.Sleep(1);
                        }
                    }
                }
            }
        }

        public void Stop()
        {
            if (_state != state.running)
            {
                return;
            }
            else
            {
                lock (_lock)
                {
                    if (_state != state.running)
                    {
                        return;
                    }
                    else
                    {
                        _state = state.disposed;
                        _waitingPipe?.Dispose();
                        Handler?.Dispose();
                    }
                }
            }
        }

        private void WaitForConnection(string pipeName)
        {
            while (_state == state.running)
            {
                ServerPipe pipe = null;
                try
                {
                    pipe = WaitServerPipe(pipeName);
                    ProcessConnection(pipe);
                }
                catch (IPCEndOfStreamException)
                {
                    ClientClosed?.Invoke(this, EventArgs.Empty);
                }
                catch
                { }
                finally
                {
                    if (pipe != null)
                        pipe.Dispose();
                }
            }
        }

        private void WaitForMultiConnection(string pipeName)
        {
            while (_state == state.running)
            {
                var pipe = WaitServerPipe(pipeName);
                Logging.Debug("pipe connected");
                Task.Run(new Action(() =>
                {
                    try
                    {
                        ProcessConnection(pipe);
                    }
                    catch
                    { }
                    finally
                    {
                        Logging.Debug("pipe disconnected");
                        if (pipe != null)
                            pipe.Dispose();
                    }
                }));
            }
        }

        private ServerPipe WaitServerPipe(string pipeName)
        {
            _waitingPipe = ServerPipe.CreateNew(pipeName);
            _waitingPipe.WaitForConnection();
            //这里waitpipe 不要置空， 否则再启动时如果就有消息进来时会阻塞住线程
            //见Start 中的while (_state == state.running && _waitingPipe == null)
            return _waitingPipe;
        }

        private void ProcessConnection(ServerPipe pipe)
        {
            while (_state == state.running)
            {
                IPCRequest request = null;
                IPCResponse response = null;
                try
                {
                    request = pipe.ReadRequest();
                }
                catch (IPCException ex)
                {
                    response = IPCResponse.InternalFailure(ex);
                }

                if (response == null)
                {
                    try
                    {
                        if (request.Method == "KeepAlive")
                        {
                            response = IPCResponse.OK("KeepAlive");
                        }
                        else if (request.Method == "ResolvePromise")
                        {
                            response = ResolvePromise(request);
                        }
                        else
                        {
                            //将返回值response通过入参context的形式返回
                            var context = new IPCContext(request);
                            Handler.ProcessRequest(context);
                            response = context.Response;

                            // 处理异步返回的(Promise)的情况
                            var promise = response?.PromiseResult;
                            if (promise != null)
                            {
                                lock (_promiseDictLock)
                                {
                                    _promiseDict[promise.Id] = promise;
                                }
                            }
                        }
                    }
                    catch (IPCException ex)
                    {
                        response = IPCResponse.InternalFailure(ex);
                    }
                    catch (Exception ex)
                    {
                        Logging.Error($"ipc-service[{pipe.PipeName}] unhandled exception", ex);
                        response = IPCResponse.InternalFailure(new IPCException(IPCResponseCode.InternalServerError, $"{Strings.IPCServer_AnUnknownExceptionOccurredInIPCService}"));
                    }
                }

                if (response == null)
                    response = IPCResponse.InternalFailure(new IPCException(IPCResponseCode.InternalServerError, $"{Strings.IPCServer_IPCServiceCannotProcessThisRequest}"));

                pipe.WriteResponse(response);
            }
        }

        private IPCResponse ResolvePromise(IPCRequest request)
        {
            lock (_promiseDictLock)
            {
                var promiseId = request.Params.Resolve<string>("promiseId");
                if (_promiseDict.TryGetValue(promiseId, out Promise promise))
                {
                    if (promise.State == PromiseStates.Pending)
                    {
                        return IPCResponse.OK(promise);
                    }
                    else
                    {
                        _promiseDict.Remove(promiseId); // 无论成功失败都需要清除缓存
                        try
                        {
                            return IPCResponse.OK(promise.Result);
                        }
                        catch (AggregateException ex)
                        {
                            Logging.Warn($"ipc resolve promise fail", ex);
                            if (ex.InnerException is IPCException ipcException)
                                return IPCResponse.InternalFailure(ipcException);
                            else
                                return IPCResponse.InternalFailure(new IPCException(
                                    IPCResponseCode.InternalServerError,
                                    ex.InnerException.Message));
                        }
                    }
                }
                else
                {
                    return IPCResponse.InternalFailure(new IPCException(IPCResponseCode.InternalServerError, $"{Strings.IPCServer_NoAsynchronousRequestFoundForTheSpecifiedID}"));
                }
            }
        }

        public event EventHandler ClientClosed;
    }
}
