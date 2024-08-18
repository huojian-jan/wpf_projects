using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpf_controls.customControls
{
    /// <summary>
    /// userNameControl.xaml 的交互逻辑
    /// </summary>
    public partial class userNameControl : UserControl
    {
        public userNameControl()
        {
            InitializeComponent();
            var text = new TextBox();
        }

        private void EventSetter_OnHandler(object sender, MouseEventArgs e)
        {
            MessageBox.Show("you are in...");
        }
    }
}
