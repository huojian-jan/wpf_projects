using Autofac.Core;
using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Huojian.LibraryManagement.Extentions
{
    public static class AutofucExtension
    {
        /// <summary>
        /// 注册指定程序集中的所有ViewModel
        /// </summary>
        public static void RegisterViewModels(this ContainerBuilder builder, Assembly assembly)
        {
            builder.RegisterAssemblyTypes(assembly)
                .Where(t => t.Name.EndsWith("ViewModel") && t.IsClass && !t.IsAbstract)
                .AsSelf()
                //.PropertiesAutowired(new MefAttributePropertyFinder())
                .InstancePerDependency();
        }

        //class MefAttributePropertyFinder : IPropertySelector
        //{
        //    public bool InjectProperty(PropertyInfo propertyInfo, object instance)
        //    {
        //        return propertyInfo.GetCustomAttribute<ImportAttribute>() != null;
        //    }
        //}

    }
}
