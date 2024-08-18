using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using calculator.ViewModels;
using Caliburn.Micro;

namespace calculator
{
    public class CalculatorBootstrapper : BootstrapperBase
    {

        public CalculatorBootstrapper()
        {
            Initialize();
        }
        protected async override void OnStartup(object sender, StartupEventArgs e)
        {
          await DisplayRootViewForAsync<ShellViewModel>();
        }
    }
}
