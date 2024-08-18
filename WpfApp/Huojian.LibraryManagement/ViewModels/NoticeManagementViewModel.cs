using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Huojian.LibraryManagement.Common;
using Huojian.LibraryManagement.Models;

namespace Huojian.LibraryManagement.ViewModels
{
    public class NoticeManagementViewModel:Screen
    {
        private readonly IViewModelFactory _viewModelFactory;

        public BindableCollection<Notice> Notices { get; set; }
        public delegate NoticeManagementViewModel Factory();
        public NoticeManagementViewModel(IViewModelFactory viewModelFactory)
        {
            DisplayName = "公告管理";

            _viewModelFactory = viewModelFactory;
            PaginationViewModel = _viewModelFactory.Create<PaginationViewModel.Factory>()(0);

            Notices = new BindableCollection<Notice>();
            MockNotices();
        }

        private void MockNotices()
        {
            for (int i = 0; i < 10; i++)
            {
                var notice = new Notice()
                {
                    Title = "Title",
                    Content = "这个是内容",
                    Publisher = $"发布者{i + 1}",
                    PublishTime = DateTime.Now.AddDays(-(i + 10)).ToString()
                };

                Notices.Add(notice);
            }
        }

        public PaginationViewModel PaginationViewModel { get; set; }
    }
}
