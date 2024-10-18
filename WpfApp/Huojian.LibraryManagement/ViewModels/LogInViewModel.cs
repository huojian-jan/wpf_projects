using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Huojian.LibraryManagement.Common;
using Huojian.LibraryManagement.ViewModels.controls;

namespace Huojian.LibraryManagement.ViewModels
{
    public class LogInViewModel:Screen
    {
        private readonly IViewModelFactory _viewModelFactory;
        public LogInViewModel(IViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
            StudentInfoViewModel = _viewModelFactory.Create<StudentInfoViewModel.Factory>()();
        }

        public string Red { get; set; } = "this is red" + Environment.NewLine;
        public string Blue { get; set; } = "this is blue";


        public StudentInfoViewModel StudentInfoViewModel { get; set; }
    }
}
