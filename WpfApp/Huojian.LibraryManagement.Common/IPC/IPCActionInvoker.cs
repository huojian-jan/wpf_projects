using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace Huojian.LibraryManagement.Common.IPC
{
    class IPCActionInvoker
    {
        private delegate object ActionExecutor(IPCBaseService service, object[] parameters);
        private delegate void VoidActionExecutor(IPCBaseService service, object[] parameters);

        private readonly ActionExecutor _executor;      //Action方法
        private readonly ParameterInfo[] _parameters;   //方法的入参
        private readonly ValidationAttribute[][] _validations;  //参数的注解式校验
        private readonly MethodInfo _method;

        public IPCActionInvoker(MethodInfo method)
        {
            _executor = GetExecutor(method);
            _parameters = method.GetParameters();
            _validations = new ValidationAttribute[_parameters.Length][];
            for (int i = 0; i < _parameters.Length; i++)
                _validations[i] = _parameters[i].GetCustomAttributes<ValidationAttribute>().ToArray();
            _method = method;
        }

        public object Execute(IPCBaseService service, IPCRequest request)
        {
            //遍历Action方法的所有参数
            object[] @params = new object[_parameters.Length];
            for (int i = 0; i < _parameters.Length; i++)
            {
                var paramInfo = _parameters[i];
                var paramType = paramInfo.ParameterType;
                //从request中根据参数名称获取参数值，并转化为paramType类型 (原因在于用户传递来的参数可能不全)
                if (request.TryResolve(paramInfo.Name, paramType, out object value))
                {
                    @params[i] = value;
                }
                //对于那些用户没传过来的参数
                else
                {
                    if (paramInfo.HasDefaultValue)
                    {
                        @params[i] = paramInfo.DefaultValue;
                    }
                    else
                    {
                        bool canBeNull = !paramType.IsValueType || Nullable.GetUnderlyingType(paramType) != null;
                        if (!canBeNull)
                            throw new ValidationException($"{String.Format(Strings.IPCActionInvoker_ParameterCannotBeEmpty, paramInfo.Name)}");
                    }
                }
                //校验参数是否合法
                foreach (var validation in _validations[i])
                    validation.Validate(@params[i], paramInfo.Name);
            }

            return _executor(service, @params);
        }

        /// <summary>
        /// 执行Service.Action方法
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static ActionExecutor GetExecutor(MethodInfo methodInfo)
        {
            var controllerParameter = Expression.Parameter(typeof(IPCBaseService), "service");
            var parametersParameter = Expression.Parameter(typeof(object[]), "parameters");

            var parameters = new List<Expression>();

            var paramInfos = methodInfo.GetParameters();
            for (int i = 0; i < paramInfos.Length; i++)
            {
                var paramInfo = paramInfos[i];
                var valueObj = Expression.ArrayIndex(parametersParameter, Expression.Constant(i));
                var valueCast = Expression.Convert(valueObj, paramInfo.ParameterType);

                parameters.Add(valueCast);
            }

            var instanceCast = !methodInfo.IsStatic ? Expression.Convert(controllerParameter, methodInfo.ReflectedType) : null;
            MethodCallExpression methodCall = methodCall = Expression.Call(instanceCast, methodInfo, parameters);

            if (methodCall.Type == typeof(void))
            {
                var lambda = Expression.Lambda<VoidActionExecutor>(methodCall, controllerParameter, parametersParameter);
                var voidDispatcher = lambda.Compile();
                return WrapVoidAction(voidDispatcher);
            }
            else
            {
                var castMethodCall = Expression.Convert(methodCall, typeof(object));
                var lambda = Expression.Lambda<ActionExecutor>(castMethodCall, controllerParameter, parametersParameter);
                return lambda.Compile();
            }
        }

        private static ActionExecutor WrapVoidAction(VoidActionExecutor executor)
        {
            return delegate (IPCBaseService service, object[] parameters)
            {
                executor(service, parameters);
                return null;
            };
        }
    }
}
