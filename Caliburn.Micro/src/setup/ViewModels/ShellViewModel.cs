using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;

namespace setup.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        public ShellViewModel()
        {
            UploadViewModel=new UploadViewModel();
        }

        public ShellViewModel(string name)
        {
            
        }
        public UploadViewModel UploadViewModel { get; set; }
    }
}
