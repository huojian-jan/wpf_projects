using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Common;

namespace Todo.ViewModels
{
    public class MainViewModel:Screen
    {
        IViewModelFactory _viewmodelFactoty;
        public MainViewModel(IViewModelFactory viewModelFactory)
        {
            _viewmodelFactoty = viewModelFactory;

            LeftTabBar = _viewmodelFactoty.Create<LeftTabBarViewModel>();
            TaskView = _viewmodelFactoty.Create<TaskViewModel>();
        }


        public LeftTabBarViewModel LeftTabBar { get; set; }

        public TaskViewModel TaskView { get; set; }
    }
}
