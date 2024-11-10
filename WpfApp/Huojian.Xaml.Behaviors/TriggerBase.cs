using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace Huojian.Xaml.Behaviors
{
    public class PreviewInvokeEventArgs : EventArgs
    {
        public bool IsCancelling { get; set; }
    }

    [ContentProperty("Actions")]//TODO(搞明白这个attribute)
   public abstract class TriggerBase:Animatable,IAttachedObject
   {
       private DependencyObject associatedObject;
       private Type associatedObjectTypeConstraint;

       private static readonly DependencyPropertyKey ActionsPropertyKey = DependencyProperty.RegisterReadOnly("Actions",
           typeof(TriggerActionCollection),
           typeof(TriggerBase),
           new FrameworkPropertyMetadata());


       internal TriggerBase(Type associatedObjectTypeConstraint)
       {
           this.associatedObjectTypeConstraint= associatedObjectTypeConstraint;
           var newCollection = new TriggerActionCollection();
            this.SetValue(ActionsPropertyKey,newCollection);
       }


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
