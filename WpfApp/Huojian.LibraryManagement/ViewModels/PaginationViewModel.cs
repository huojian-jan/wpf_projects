using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Huojian.LibraryManagement.Common;
using Huojian.LibraryManagement.Components.Protocol.Swager;

namespace Huojian.LibraryManagement.ViewModels
{
    public  class PaginationViewModel:Screen
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IApiClient _apiClient;
        private readonly int _currentPage;

        public delegate PaginationViewModel Factory(int currentPage);

        public PaginationViewModel(IViewModelFactory viewModelFactory,
                                    int currentPage)
        {
             _viewModelFactory=viewModelFactory;
             _currentPage=currentPage;
        }


        public async Task NextPage()
        {
            throw new NotImplementedException();
        }

        public async Task PreviousPage()
        {
            throw new NotImplementedException();
        }

        public string GoToPageNum { get; set; }

        public async Task GoToPageAction()
        {
            throw new NotImplementedException();
        }

        public string AllPageNum { get; set; }
    }
}
