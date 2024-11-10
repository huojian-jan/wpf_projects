using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ControlToolKits.Controls
{
    public class RichButton:Button
    {
        private readonly string PrimaryRichButtonStyle = nameof(PrimaryRichButtonStyle);


        public static readonly DependencyProperty IsPrimaryProperty;
        static RichButton()
        {
            IsPrimaryProperty = DependencyProperty.Register(nameof(IsPrimary), typeof(bool), typeof(RichButton), new PropertyMetadata(false));
        }

        public RichButton()
        {
            Loaded += (sender,e) => LoadStyle();
        }

        private void LoadStyle()
        {
            if (IsPrimary)
            {
                var templateKey = PrimaryRichButtonStyle;
                var res = this.TryFindResource(templateKey);
                SetValue(TemplateProperty,this.TryFindResource(templateKey) as ControlTemplate);
            }
        }

        public bool IsPrimary
        {
            get => (bool)GetValue(IsPrimaryProperty);
            set=> SetValue(IsPrimaryProperty, value);
        }

    }

    public enum SizeMode
    {
        Large,
        Normal,
        Small
    }
}
