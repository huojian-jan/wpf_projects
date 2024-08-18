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
    public class BorrowManagementViewModel : Screen
    {
        private readonly IViewModelFactory _viewModelFactory;
        public BindableCollection<BorrowRecord> Records { get; set; }
        public BindableCollection<BookCategory> Categories { get; set; }


        public delegate BorrowManagementViewModel Factory();

        public BorrowManagementViewModel(IViewModelFactory viewModelFactory)
        {

            DisplayName = "借阅管理";
            Records = new BindableCollection<BorrowRecord>();
            Categories = new BindableCollection<BookCategory>();
            _viewModelFactory =viewModelFactory;
            PaginationViewModel = _viewModelFactory.Create<PaginationViewModel.Factory>()(0);

            MockRecords();
            MockCategories();
        }

        private void MockCategories()
        {
            var names = new List<string>()
            {
                "小说",
                "财经",
                "都市",
                "IT",
                "政治",
                "经济",
                "文化",
                "社会科学"
            };

            for (var i = 0; i < names.Count; i++)
            {
                var category = new BookCategory();
                category.Name = names[i];
                category.Id = (100 + i).ToString();

                Categories.Add(category);
            }
        }

        private void MockRecords()
        {
            for (int i = 0; i < 20; i++)
            {
                var record = new BorrowRecord();
                record.BookName = "book1";
                record.ReaderId = (0 + 1000).ToString();
                record.BookStatus = "在借";
                record.BorrowTime = DateTime.Now.AddDays(-(i + 1)).ToString();
                record.ReaderName = $"张{i}";

                Records.Add(record);
            }
        }

        public PaginationViewModel PaginationViewModel { get; set; }
    }
}
