using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Resources;

namespace ShadowBot.Common.Utilities
{
    public class AttributeUtils
    {
        private static Dictionary<string, ResourceManager> resourceManCache = new Dictionary<string, ResourceManager>();
        public static string LookupResource(Type resourceManagerProvider, string resourceKey)
        {
            if (resourceManagerProvider == null)
                return resourceKey;

            ResourceManager resourceManager = null;
            resourceManCache.TryGetValue(resourceManagerProvider.FullName,out resourceManager);
            if (resourceManager == null)
            {
                var staticProperty = resourceManagerProvider.GetProperties(BindingFlags.Static | BindingFlags.Public).FirstOrDefault(s => s.PropertyType == typeof(ResourceManager));
                if (staticProperty != null)
                {
                    resourceManager = (ResourceManager)staticProperty.GetValue(null, null);
                    resourceManCache.Add(resourceManagerProvider.FullName, resourceManager);
                    return resourceManager.GetString(resourceKey);
                }
            }
            else
                return resourceManager.GetString(resourceKey);
            return resourceKey;
        }
    }
}
