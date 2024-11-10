using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Huojian.Xaml.Behaviors
{
    public interface IAttachedObject
    {
        DependencyObject AssociatedObject
        {
            get;
        }

        void Attach();

        void Detach();
    }
}
