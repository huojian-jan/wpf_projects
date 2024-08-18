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
    public class ReaderManagementViewModel:Screen
    {
        private readonly IViewModelFactory _viewModelFactory;

        public BindableCollection<Reader> Readers { get; set; }

        public delegate ReaderManagementViewModel Factory();
        public ReaderManagementViewModel(IViewModelFactory viewModelFactory)
        {
            DisplayName = "读者管理";

            _viewModelFactory = viewModelFactory;
            PaginationViewModel = _viewModelFactory.Create<PaginationViewModel.Factory>()(0);

            Readers = new BindableCollection<Reader>();

            MockReaders();
        }

        private void MockReaders()
        {
            var names = new List<string>()
            {
                "张三",
                "lisi",
                "wangwu",
                "小明",
                "小红",
                "老王"
            };

            for (int i = 0; i < names.Count; i++)
            {
                var reader = new Reader()
                {
                    Id = i.ToString(),
                    BirthDay = DateTime.Now.AddYears(-i).ToString(),
                    Email = $"{names[i]}@exmaple.com",
                    Gender = "女",
                   CreateTime = DateTime.Now.AddDays(-(i+10)).ToString(),
                    Phone = $"12434{i + 1}1113",
                    RealName = names[i],
                    UserName = $"username_{names[i]}"
                };

                Readers.Add(reader);
            }
        }

        public PaginationViewModel PaginationViewModel { get; set; }
    }
}
