using System.Reflection;
using Autofac;
using Huojian.LibraryManagement.Components.Protocol.Swager.Api;

namespace Huojian.LibraryManagement.Components.Swagger
{
    public class SwaggerAutofacModule : Autofac.Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // 注册WebService
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => typeof(IRestApi).IsAssignableFrom(t))
                .AsImplementedInterfaces();
        }
    }
}