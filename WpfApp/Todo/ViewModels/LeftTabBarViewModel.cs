using Caliburn.Micro;
using Todo.Common;
using Todo.Model;

namespace Todo.ViewModels
{
    public class LeftTabBarViewModel : Screen
    {
        public BindableCollection<LeftTabBarButton> LeftBarButtons { get; set; } = new();
        public BindableCollection<LeftTabBarButton> FunctionButtons { get; set; } = new();

        public LeftTabBarViewModel()
        {
            Init();
        }

        private void Init()
        {
            InitUpButtons();
            InitFunctionButtons();
        }

        private void InitUpButtons()
        {
            var task = new LeftTabBarButton()
            {
                Name = Constants.LeftTabBar_TaskName,
                Icon = Constants.LeftTabBar_TaskIcon,
                IsActive = false,
                ToolTips = Constants.LeftTabBar_TaskName
            };

            var focus = new LeftTabBarButton()
            {
                Name = Constants.LeftTabBar_FocusName,
                Icon = Constants.LeftTabBar_FocusIcon,
                IsActive = false,
                ToolTips = Constants.LeftTabBar_FocusName
            };

            var calendar = new LeftTabBarButton()
            {
                Name = Constants.LeftTabBar_CalendarName,
                Icon = Constants.LeftTabBar_CalendarIcon,
                IsActive = false,
                ToolTips = Constants.LeftTabBar_CalendarName
            };

            var behavior = new LeftTabBarButton()
            {
                Name = Constants.LeftTabBar_BehaviorName,
                Icon = Constants.LeftTabBar_BehaviroIcon,
                IsActive = false,
                ToolTips = Constants.LeftTabBar_BehaviorName
            };

            var search = new LeftTabBarButton()
            {
                Name = Constants.LeftTabBar_SearchName,
                Icon = Constants.LeftTabBar_SearchIcon,
                IsActive = false,
                ToolTips = Constants.LeftTabBar_SearchName
            };

            var quadrants = new LeftTabBarButton()
            {
                Name = Constants.LeftTabBar_QuadrantsName,
                Icon = Constants.LeftTabBar_QuadrantsIcon,
                IsActive = false,
                ToolTips = Constants.LeftTabBar_QuadrantsName
            };

            LeftBarButtons.Add(task);
            LeftBarButtons.Add(focus);
            LeftBarButtons.Add(calendar);
            LeftBarButtons.Add(behavior);
            LeftBarButtons.Add(search);
            LeftBarButtons.Add(quadrants);
        }

        private void InitFunctionButtons()
        {
            var sync = new LeftTabBarButton()
            {
                Name = Constants.LeftTabFunctionBtn_SyncName,
                Icon = Constants.LeftTabFunctionBtn_SyncIcon,
                IsActive = false,
                ToolTips = Constants.LeftTabFunctionBtn_SyncName
            };

            var notice = new LeftTabBarButton()
            {
                Name = Constants.LeftTabFunctionBtn_NoticeName,
                Icon = Constants.LeftTabFunctionBtn_NoticeIcon,
                IsActive = false,
                ToolTips = Constants.LeftTabFunctionBtn_NoticeName
            };
            
            var more = new LeftTabBarButton()
            {
                Name = Constants.LeftTabFunctionBtn_MoreName,
                Icon = Constants.LeftTabFunctionBtn_MoreIcon,
                IsActive = false,
                ToolTips = Constants.LeftTabFunctionBtn_MoreName
            };

            FunctionButtons.Add(sync);
            FunctionButtons.Add(notice);
            FunctionButtons.Add(more);
        }

        public void ActiveItemChanged(LeftTabBarButton context)
        {
            LeftBarButtons.ToList().ForEach(x => x.IsActive = false);
            context.IsActive = true;

            if (context.Name == Constants.LeftTabFunctionBtn_SyncName ||
                context.Name == Constants.LeftTabFunctionBtn_NoticeName ||
                context.Name == Constants.LeftTabFunctionBtn_MoreName)
            {
                HandleFunctionButtons(context);
                return;
            }
            LeftTabClickChanged?.Invoke(new LeftTabClickChangedEventArgs(context.Name));
        }

        private void HandleFunctionButtons(LeftTabBarButton context)
        {
            if (context.Name == Constants.LeftTabFunctionBtn_SyncName)
            {
                Sync(context);
            }else if (context.Name == Constants.LeftTabFunctionBtn_NoticeName)
            {
                ShowNoticeView();
            }
            else if(context.Name== Constants.LeftTabFunctionBtn_MoreName)
            {
                ShowMoreView();
            }
            else
            {
                throw new InvalidOperationException("位置的功能按钮");
            }
        }

        private void ShowMoreView()
        {
        }

        private void ShowNoticeView()
        {

        }

        private void Sync(LeftTabBarButton context)
        {
            FunctionButtons.ToList().ForEach(x => x.IsRotate = false);
            context.IsRotate = true;
            FunctionButtons.FirstOrDefault(x=>x.Name==context.Name).IsRotate=true;
        }

        public event Action<LeftTabClickChangedEventArgs> LeftTabClickChanged;
    }

    public class LeftTabClickChangedEventArgs : EventArgs
    {
        public string Name { get; set; }

        public LeftTabClickChangedEventArgs(string name)
        {
            Name = name;
        }
    }
}
