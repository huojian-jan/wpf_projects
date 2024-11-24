using Caliburn.Micro;
using Todo.Common;
using Todo.Model;

namespace Todo.ViewModels
{
    public class LeftTabBarViewModel : Screen
    {
        public BindableCollection<LeftTabBarButton> LeftBarButtons { get; set; } = new();

        public LeftTabBarViewModel()
        {
            Init();
        }

        private void Init()
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

        public void ActiveItemChanged(LeftTabBarButton context)
        {
            LeftBarButtons.ToList().ForEach(x => x.IsActive = false);
            context.IsActive = true;
            LeftTabClickChanged?.Invoke(new LeftTabClickChangedEventArgs(context.Name));
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
