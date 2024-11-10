using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Huojian.Xaml.Behaviors
{



    /// <summary>
    /// Represets an attachable object that encapsulates a unit of functionality
    /// </summary>
    public abstract class TriggerAction:Animatable,IAttachedObject
    {
        public DependencyObject AssociatedObject { get; }
        public void Attach()
        {
            throw new NotImplementedException();
        }

        public void Detach()
        {
            throw new NotImplementedException();
        }
    }
}
