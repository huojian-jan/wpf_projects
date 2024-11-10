using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Autofac;
using Todo.ViewModels;
using WPF.Common;
using WPF.Common.Extentions;

namespace Todo
{
    public class Bootstrapper:HuojianBootstrapperBase
    {

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            base.Configure();
        }

        protected override void BuildContainer(ContainerBuilder builder)
        {
            base.BuildContainer(builder);
            builder.RegisterViewModels(GetType().Assembly);
        }

        protected async override void OnStartup(object sender, StartupEventArgs e)
        {
            await DisplayRootViewForAsync<LogInViewModel>();
        }
    }
}
