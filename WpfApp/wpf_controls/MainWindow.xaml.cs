using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpf_controls
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _expanded=true;
        public MainWindow()
        {
            InitializeComponent();

            var textBox=new TextBox();

        }

        private void LeftButton_OnClick(object sender, RoutedEventArgs e)
        {
            
            if (_expanded)
            {
                this.leftButton.Width = 100;
                this.leftDock.Width = 100;
                this.leftButton.Content = "Unexpanded";
            }
            else
            {
                this.leftButton.Width = 300;
                this.leftDock.Width = 300;
                this.leftButton.Content = "Expanded";
            }
            _expanded=!_expanded;
        }
    }
}