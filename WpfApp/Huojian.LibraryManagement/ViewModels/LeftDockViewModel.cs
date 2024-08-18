using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;

namespace Huojian.LibraryManagement.ViewModels
{
    public class LeftDockViewModel:Screen
    {
        private readonly IEventAggregator _eventAggregator;
        public delegate LeftDockViewModel Factory();


        public BindableCollection<NavigatorButton> NavigatorButtons { get; set; }
        public LeftDockViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnUIThread(this);

            NavigatorButtons = new BindableCollection<NavigatorButton>();
            InitNavigationButtons();
        }

        private void InitNavigationButtons()
        {
            var borrowButton = new NavigatorButton
            {
                DisplayName = "借阅管理",
                Icon = "../resources/icon.png",
                AssociatedScreen = typeof(BorrowManagementViewModel)
            };

            NavigatorButtons.Add(borrowButton);

            var bookManagementButton = new NavigatorButton()
            {
                DisplayName = "图书管理",
                Icon = "../resources/icon.png",
                AssociatedScreen = typeof(BookManagementViewModel)
            };
            NavigatorButtons.Add(bookManagementButton);

            var readerManagemenetButton = new NavigatorButton()
            {
                DisplayName = "读者管理",
                Icon = "../resources/icon.png",
                AssociatedScreen = typeof(ReaderManagementViewModel)
            };

            NavigatorButtons.Add(readerManagemenetButton);
            var categoryButton = new NavigatorButton()
            {
                DisplayName = "类型管理",
                Icon = "../resources/icon.png",
                AssociatedScreen = typeof(BookCategoryManagementViewModel)
            };
            NavigatorButtons.Add(categoryButton);
                        var noticeButton = new NavigatorButton()
            {
                DisplayName = "公告管理",
                Icon = "../resources/icon.png",
                AssociatedScreen = typeof(NoticeManagementViewModel)
            };
            NavigatorButtons.Add(noticeButton);
                        var dataButton = new NavigatorButton()
            {
                DisplayName = "统计数据",
                Icon = "../resources/icon.png",
                AssociatedScreen = typeof(DataAnalysisViewModel)
            };
            NavigatorButtons.Add(dataButton);
        }

        public void ActivateItem(NavigatorButton button)
        {
            _eventAggregator.PublishOnUIThreadAsync(new ActiveteItemChangedMessage(button.AssociatedScreen));
        }
    }

    public class NavigatorButton
    {
        public string DisplayName { get; set; }

        public string Icon { get; set; }

        public Type AssociatedScreen { get; set; }
    }
}
