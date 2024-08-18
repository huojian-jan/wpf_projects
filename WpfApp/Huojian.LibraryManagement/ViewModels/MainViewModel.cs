using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Huojian.LibraryManagement.Common;

namespace Huojian.LibraryManagement.ViewModels
{
    public class MainViewModel:Conductor<Screen>.Collection.OneActive,IHandle<ActiveteItemChangedMessage>
    {
        private readonly IViewModelFactory _viewModelFactory;
        private readonly IEventAggregator _eventAggregator;
        private readonly LayoutManager _layoutManager;

        public MainViewModel(IEventAggregator eventAggregator,
                            IViewModelFactory viewModelFactory, 
                            LayoutManager layoutManager)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnUIThread(this);
            _viewModelFactory = viewModelFactory;
            LeftViewModel = viewModelFactory.Create<LeftDockViewModel.Factory>()();
            TopViewModel = viewModelFactory.Create<TopDockViewModel.Factory>()();
            _layoutManager = layoutManager;


            Items.Add(_viewModelFactory.Create<AdminManagementViewModel.Factory>()());
            Items.Add(_viewModelFactory.Create<BookCategoryManagementViewModel.Factory>()());
            Items.Add(_viewModelFactory.Create<BookManagementViewModel.Factory>()());
            Items.Add(_viewModelFactory.Create<BorrowManagementViewModel.Factory>()());
            Items.Add(_viewModelFactory.Create<DataAnalysisViewModel.Factory>()());
            Items.Add(_viewModelFactory.Create<NoticeManagementViewModel.Factory>()());
            Items.Add(_viewModelFactory.Create<ReaderManagementViewModel.Factory>()());

            ActiveItem = Items[3];
        }

        public LeftDockViewModel LeftViewModel { get; set; }

        public TopDockViewModel TopViewModel { get; set; }

        public LayoutManager  Layout =>_layoutManager;

        public Task HandleAsync(ActiveteItemChangedMessage message, CancellationToken cancellationToken)
        {
            var changedItem = message.ActiveItemType;
            if (ActiveItem.GetType().GetHashCode() == changedItem.GetHashCode())
            {
                return Task.CompletedTask;
                ;
            }

            var item = Items.FirstOrDefault(x => x.GetType().GetHashCode() == changedItem.GetHashCode());
            ActiveItem = item;

            return Task.CompletedTask;
        }
    }


    public class ActiveteItemChangedMessage
    {
        public Type ActiveItemType { get; set; }

        public ActiveteItemChangedMessage(Type type)
        {
            ActiveItemType=type;
        }
    }

}
