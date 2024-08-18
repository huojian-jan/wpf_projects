using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Huojian.LibraryManagement.Common;
using Huojian.LibraryManagement.Models;
using BookCategory = Huojian.LibraryManagement.Components.Protocol.Swager.Model.BookCategory;

namespace Huojian.LibraryManagement.ViewModels
{
    public class BookCategoryManagementViewModel : Screen
    {
        private readonly IViewModelFactory _viewModelFactory;

        private List<BookCategory> _allCategories;
        private int _currentPageNum;

        public BindableCollection<BookCategory> Categories { get; set; }
        public delegate BookCategoryManagementViewModel Factory();
        public BookCategoryManagementViewModel(IViewModelFactory viewModelFactory)
        {
            DisplayName = "类型管理";

            _viewModelFactory = viewModelFactory;
            PaginationViewModel = _viewModelFactory.Create<PaginationViewModel.Factory>()(0);
            Categories = new BindableCollection<BookCategory>();
        }

        protected override void OnViewLoaded(object view)
        {
            MockData();
        }

        private void UpdateUIData(int pageNum)
        {
            Categories.Clear();
        }

        public PaginationViewModel PaginationViewModel { get; set; }
        private void MockData()
        {
            for (int i = 0; i < 20; i++)
            {
                var category1 = new BookCategory()
                {
                    Id = (i + 100).ToString(),
                    Description = "这是一段描述文本",
                    Name = "武侠小说"
                };
                Categories.Add(category1);
            }
        }
    }
}
