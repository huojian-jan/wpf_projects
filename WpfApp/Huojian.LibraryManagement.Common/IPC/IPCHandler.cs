namespace Huojian.LibraryManagement.Common.IPC
{
    public class IPCHandler : IDisposable
    {
        private readonly Dictionary<string, IPCServiceInfo> _serviceDict = new Dictionary<string, IPCServiceInfo>();
        private RequestHandler _requestHandler;

        public IServiceContainer ServiceContainer { get; set; }

        public ApartmentState ApartmentState { get; set; } = ApartmentState.MTA;

        protected IPCServiceInfo ResolveServiceDescriptor(IPCContext context)
        {
            var request = context.Request;
            if (!_serviceDict.TryGetValue(request.ServiceName, out IPCServiceInfo descriptor))
                throw new IPCException(IPCResponseCode.NotFound, $"ipc-service not found, {request.ServiceName}");
            return descriptor;
        }

        public void RegisterService(IEnumerable<Type> serviceTypes)
        {
            foreach (var serviceType in serviceTypes)
            {
                if (!typeof(IPCBaseService).IsAssignableFrom(serviceType))
                    throw new ArgumentException($"ipc-service type error, {serviceType.FullName}");
                var descriptor = new IPCServiceInfo(serviceType);
                _serviceDict.Add(descriptor.Name, descriptor);
            }
        }

        /// <summary>
        /// 处理管道数据
        /// </summary>
        /// <param name="context"></param>
        public virtual void ProcessRequest(IPCContext context)
        {
            if (_requestHandler == null)
                _requestHandler = new RequestHandler(_serviceDict, ApartmentState);

            context.ServiceContainer = ServiceContainer;
            _requestHandler.Process(context);   //ipcHandler转交给requestHandler处理
            var timeout = context.Request.Options.Resolve("timeout", -1);
            if (timeout > 0)
                timeout *= 1000;
            else
                timeout = -1;
            if (!_requestHandler.Wait(timeout))
            {
                context.Response = IPCResponse.InternalFailure(new IPCException(IPCResponseCode.RequestTimeout, $"{Strings.IPCHandler_ProcessingTimeout}"));
                _requestHandler.Destroy();
                _requestHandler = null;
                Logging.Warn($"{context.Request.ServiceName}.{context.Request.ActionName}{"xxx"}");
            }
        }

        public void Dispose()
        {
            if (_requestHandler != null)
            {
                _requestHandler.Destroy();
                _requestHandler = null;
            }
        }

        class RequestHandler
        {
            private readonly ManualResetEventSlim _startEvent = new ManualResetEventSlim();
            private readonly ManualResetEventSlim _endEvent = new ManualResetEventSlim();
            private readonly Dictionary<string, IPCServiceInfo> _serviceDict;
            private readonly Thread _thread;

            private bool _running = true;
            private IPCContext _context;

            public RequestHandler(Dictionary<string, IPCServiceInfo> serviceDict, ApartmentState apartmentState)
            {
                _serviceDict = serviceDict;
                _running = true;
                _thread = new Thread(StartWorkerAsync);
                _thread.IsBackground = true;
                _thread.SetApartmentState(apartmentState);
                _thread.Start();
            }

            public void Process(IPCContext context)
            {
                _context = context;
                _startEvent.Set();  //手动唤醒，类似于Notify
            }

            public bool Wait(int timeout)
            {
                if (_endEvent.Wait(timeout))
                {
                    _endEvent.Reset();
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void Destroy()
            {
                _running = false;
                _startEvent.Set();
            }

            private void StartWorkerAsync()
            {
                while (_running)
                {
                    _startEvent.Wait(); //阻塞，等待唤醒
                    if (!_running)
                        break;
                    _startEvent.Reset();

                    try
                    {
                        ProcessRequestCore(_context);
                    }
                    catch (IPCException ex)
                    {
                        _context.Response = IPCResponse.InternalFailure(ex);
                    }
                    catch (Exception ex)
                    {
                        _context.Response = IPCResponse.InternalFailure(new IPCException(IPCResponseCode.InternalServerError, $"{Strings.IPCHandler_AnUnknownExceptionOccurredInIPCService}"));
                        Logging.Error($"ipc-service unknown exception", ex);
                    }

                    _endEvent.Set();
                }
                _startEvent.Dispose();
                _endEvent.Dispose();
            }

            protected void ProcessRequestCore(IPCContext context)
            {
                //来自客户端的请求：context.Request.ServiceName，ServiceName是不带"Service"结尾的
                if (!_serviceDict.TryGetValue(context.Request.ServiceName, out IPCServiceInfo descriptor))
                    throw new IPCException(IPCResponseCode.NotFound, $"ipc-service not found, {context.Request.ServiceName}");
                descriptor.Execute(context);
            }
        }

    }
}
