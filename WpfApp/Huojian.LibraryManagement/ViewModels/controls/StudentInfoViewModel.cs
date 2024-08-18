using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Huojian.LibraryManagement.ViewModels.controls
{
    public class StudentInfoViewModel:Screen
    {
        public string Name { get; set; } = "Charles";

        public int Id { get; set; } = 101;

        public double Score { get; set; } = 200;

        public delegate StudentInfoViewModel Factory();
        public StudentInfoViewModel()
        { 
            
        }

        public void Validate()
        {
            MessageBox.Show("this is a test msg");
        }
    }
}
