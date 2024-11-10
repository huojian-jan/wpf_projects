using System.Configuration;
using System.Data;
using System.Windows;

namespace wpf_controls
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.Resources.MergedDictionaries.Add(new ResourceDictionary()
            {
                Source = new Uri("pack://application:,,,/ControlToolKits;component/Controls/RichButton/Generic.xaml", UriKind.Absolute)
            });
        }
    }
}
