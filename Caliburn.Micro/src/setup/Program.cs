using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace setup
{
    public class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var application = new Application();
            application.Resources.Add("bootstrapper", new Bootstrapper());
            application.Run();
        }
    }
}
