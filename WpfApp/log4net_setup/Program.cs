using System.IO.Pipes;
using log4net;
using log4net.Config;
using log4net_setup;

//[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Logging4net_setup
{
    public class Program
    {
        static Program()
        {
            GlobalContext.Properties["LogName"] = "Today";
            XmlConfigurator.Configure();
        }
        static void Main(string[] args)
        {
            Logging.Info("Entering the application");
            var bar = new Bar();
            bar.DoIt();

            Logging.Info("application exit");
        }
    }

    public class Bar
    {
        public Bar() { }

        public void DoIt()
        {
            Logging.Debug("Do it again");
        }
    }
}
