using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huojian.Xaml.Behaviors
{
    [CLSCompliant(false)]
    [AttributeUsage( AttributeTargets.Class| AttributeTargets.Property,AllowMultiple = true)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "FxCop is complaining about our single parameter override")]
    public class DefaultTriggerAttribute:Attribute
    {
        private Type targetType;//触发的目标类型
        private Type triggerType;
        private object[] parameters;

        public Type TargeType=>targetType;

        public Type TriggerType =>triggerType;

        public IEnumerable Parameters=>parameters;

        public DefaultTriggerAttribute(Type targetType,Type triggerType,object parameters):this
        {
            
        }

        public DefaultTriggerAttribute(Type targetTYpe,Type triggerType,params object[] parameters)
        {
            if(!)
        }


    }
}
