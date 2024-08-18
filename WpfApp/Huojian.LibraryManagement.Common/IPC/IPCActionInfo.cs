using System.Reflection;

namespace Huojian.LibraryManagement.Common.IPC
{
    /// <summary>
    /// 对MethodInfo的一层抽象
    /// </summary>
    class IPCActionInfo
    {
        private readonly MethodInfo _method;
        private readonly object _lockObj = new object();

        private bool _hasInit = false;
        private IPCActionFilter[] _actionFilters;
        private IPCActionInvoker _actionInvoker;

        public IPCActionInfo(MethodInfo method)
        {
            _method = method;
            Name = method.Name;
        }

        public string Name { get; }

        public void Execute(IPCBaseService service, IPCContext context)
        {
            EnsureInitialized();

            var filterCount = _actionFilters.Length;
            for (int i = 0; i < filterCount; i++)
                _actionFilters[i].OnActionExecuting(context);

            var actionResult = _actionInvoker.Execute(service, context.Request);
            context.Response = IPCResponse.OK(actionResult);

            for (int i = 0; i < filterCount; i++)
                _actionFilters[i].OnActionExecuted(context);
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
            _actionFilters = _method.GetCustomAttributes(typeof(IPCActionFilter), true).Cast<IPCActionFilter>().ToArray();
            _actionInvoker = new IPCActionInvoker(_method);
        }
    }
}
