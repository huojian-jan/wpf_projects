using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Huojian.LibraryManagement.ViewModels
{
    public class AdminManagementViewModel:Screen
    {
        public delegate AdminManagementViewModel Factory();
        public AdminManagementViewModel()
        {
            DisplayName = "管理员管理";
        }
    }
}
