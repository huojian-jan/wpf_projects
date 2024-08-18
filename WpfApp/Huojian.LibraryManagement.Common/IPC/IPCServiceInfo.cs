using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Huojian.LibraryManagement.Common.IPC
{
    public class IPCServiceInfo
    {
        private readonly Type _serviceType;
        private readonly object _lockObj = new object();

        private bool _hasInit = false;
        private IPCActionFilter[] _actionFilters;
        private IPCExceptionFilter[] _exceptionFilters;
        private Dictionary<string, IPCActionInfo> _actionDict;  //某个Service服务中的所有方法

        public IPCServiceInfo(Type serviceType)
        {
            _serviceType = serviceType;
            if (serviceType.Name.EndsWith("Service"))
            {
                //客户端请求格式是 ServiceName + "." + "ActionName"，所以进行转换与客户端请求指令格式相匹配
                Name = serviceType.Name.Substring(0, serviceType.Name.Length - "Service".Length);
            }
            else
            {
                Name = serviceType.Name;
            }
        }

        public string Name { get; }

        public void Execute(IPCContext context)
        {
            EnsureInitialized();

            var request = context.Request;
            //request.ActionName -> 用户请求调用的方法，根据方法名在字典中获取方法的详情，保存至actionDescriptor中
            if (!_actionDict.TryGetValue(request.ActionName, out IPCActionInfo actionDescriptor))
                throw new IPCException(IPCResponseCode.NotFound, $"ipc-service not found, {request.ServiceName}.{request.ActionName}");

            try
            {
                var filterCount = _actionFilters.Length;
                for (int i = 0; i < filterCount; i++)
                    _actionFilters[i].OnActionExecuting(context);

                var service = (IPCBaseService)Activator.CreateInstance(_serviceType);
                service.ServiceContainer = context.ServiceContainer;
                actionDescriptor.Execute(service, context);

                for (int i = 0; i < filterCount; i++)
                    _actionFilters[i].OnActionExecuted(context);
            }
            catch (Exception ex)
            {
                var exceptionContext = new IPCExceptionContext(context, ex);
                foreach (var exceptionFilter in _exceptionFilters)
                {
                    exceptionFilter.OnException(exceptionContext);
                    if (exceptionContext.Handled)
                        break;
                }

                if (exceptionContext.Handled)
                {
                    if (exceptionContext.Context.Response == null)
                        throw new InvalidOperationException("unknown exception from IPCExceptionFilter");
                }
                else
                {
                    // 保持原有异常点
                    ExceptionDispatchInfo.Capture(ex).Throw();
                    throw;
                }
            }

        }

        private void EnsureInitialized()
        {
            if (!_hasInit)
            {
                lock (_lockObj)
                {
                    if (!_hasInit)
                    {
                        InitializeCore();
                        _hasInit = true;
                    }
                }
            }
        }

        private void InitializeCore()
        {
            _actionFilters = _serviceType.GetCustomAttributes(typeof(IPCActionFilter), true).Cast<IPCActionFilter>().ToArray();
            _exceptionFilters = _serviceType.GetCustomAttributes(typeof(IPCExceptionFilter), true).Cast<IPCExceptionFilter>().ToArray();
            _actionDict = new Dictionary<string, IPCActionInfo>();
            //将service中包含的方法保存到字典中
            var methods = _serviceType.GetMethods(BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.Public);
            foreach (var method in methods)
            {
                var action = new IPCActionInfo(method);
                _actionDict.Add(action.Name, action);
            }
        }

    }
}
