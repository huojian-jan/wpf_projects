using System.Collections.ObjectModel;
using System.IO;
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

namespace My_Todo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext=this;

            Students = new ObservableCollection<Student>();
            Students.Add(new Student("zhangsan"));
            Students.Add(new Student("xiaowang"));
            Students.Add(new Student("lisi"));

            Test1();
        }
        public ObservableCollection<Student> Students { get; set; }



        public void Test1()
        {
            try
            {
                var path = @"C:\Users\33008\Downloads\客户端 5.10-5.14\general";
                var info = new FileInfo(path);
            }
            catch (Exception ex)
            {
                var a = 10;
                var msg = ex.Message;
            }

        }
    }


    public class Student
    {
        public string Name { get; set; }

        public Student(string name)
        {
            Name=name;
        }
    }
}