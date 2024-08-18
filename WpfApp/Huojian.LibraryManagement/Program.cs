using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace Huojian.LibraryManagement
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var app = new Application();
            BootstrapperBase bootstrapper = new Bootstrapper();

            app.Resources.Add("bootstrapper", bootstrapper);
            app.Run();
        }
    }
}
