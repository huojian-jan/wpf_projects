// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using System.Windows;
using Autofac;
using WpfApp1.ViewModels;
using Caliburn.Micro.Autofac;
using WpfApp1.Views;

namespace WpfApp1
{
    public class Bootstrapper: HuojianBootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }


        protected override void Configure()
        {
        }

        protected override void BuildContainer(ContainerBuilder builder)
        {
            base.BuildContainer(builder);
            builder.RegisterType<StudentInfoViewModel>().SingleInstance();
            builder.RegisterType<EditStudentInfoViewModel>().SingleInstance();
            builder.RegisterType<AddStudentInfoViewModel>().SingleInstance();
        }

        protected async  override void OnStartup(object sender, StartupEventArgs e)
        {
          await  DisplayRootViewForAsync<StudentInfoViewModel>();
        }
    }
}
