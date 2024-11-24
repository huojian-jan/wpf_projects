using Caliburn.Micro;

namespace Todo.Model
{
    public class LeftTabBarButton: PropertyChangedBase
    {
        private bool _isActive;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive= value;
                NotifyOfPropertyChange(nameof(IsActive));
            }
        }

        public string ToolTips { get; set; }

        public string Icon { get; set; }

        public string Name { get; set; }
    }
}
