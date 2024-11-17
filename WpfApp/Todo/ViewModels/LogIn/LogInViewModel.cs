using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using WPF.Common;

namespace Todo.ViewModels.LogIn
{
    public class LogInViewModel:Screen
    {
        private readonly IViewModelFactory _viewModelFactory;
        public LogInViewModel(IViewModelFactory viewModelFactory)
        {
            _viewModelFactory =viewModelFactory;

            LogInCarouselView = _viewModelFactory.Create<LogInCarouselControlViewModel>();
        }
        public LogInCarouselControlViewModel LogInCarouselView { get; set; }
    }
}
