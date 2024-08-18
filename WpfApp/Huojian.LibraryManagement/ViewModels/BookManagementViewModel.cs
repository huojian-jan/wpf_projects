using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Huojian.LibraryManagement.Common;
using Huojian.LibraryManagement.Models;
using Action = Caliburn.Micro.Action;

namespace Huojian.LibraryManagement.ViewModels
{
    public class BookManagementViewModel:Screen
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IViewModelFactory _viewModelFactory;
        public delegate BookManagementViewModel Factory();


        public BindableCollection<BookCategory> Categories { get; set; }
        public BindableCollection<BookRecord> Books { get; set; }
        public BookManagementViewModel(IEventAggregator eventAggregator,
                                        IViewModelFactory viewModelFactory)
        {
            DisplayName = "图书管理";
            _eventAggregator = eventAggregator;
            _viewModelFactory = viewModelFactory;
            _eventAggregator.Subscribe(this);


            Categories = new BindableCollection<BookCategory>();
            Books = new BindableCollection<BookRecord>();

            MockCategories();
            MockBooks();

            PaginationViewModel = _viewModelFactory.Create<PaginationViewModel.Factory>()(0);
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

        private void MockBooks()
        {
            for (int i = 0; i < 20; i++)
            {
                var random = new Random();
                var book = new BookRecord()
                {
                    Id = i.ToString(),
                    Name = $"书{i + 100}",
                    AuthorName = "张三",
                    Category = Categories[random.Next(0, Categories.Count)].Name,
                    Price = random.NextDouble(),
                    Language = "中文"
                };

                Books.Add(book);
            }
        }
        public void ActivateItem()
        {

        }

        public PaginationViewModel PaginationViewModel { get; set; }
    }
}
