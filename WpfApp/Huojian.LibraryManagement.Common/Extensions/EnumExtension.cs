using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Huojian.LibraryManagement.Common.Extensions
{
    public static class EnumExtension
    {
        public static IEnumerable<T> SplitFlags<T>(this Enum @enum)
            where T : Enum
        {
            foreach (Enum value in Enum.GetValues(@enum.GetType()))
                if (@enum.HasFlag(value))
                    yield return (T)value;
        }
    }

    public static class VKeysExtension
    {
        public static string ToTitleCase(this string source)
        {
            var ti = CultureInfo.CurrentCulture.TextInfo;
            return ti.ToTitleCase(source);
        }

        public static string ToDiplayString(this VKeys source)
        {
            var key = source.ToString();
            if (key.Contains("_"))
            {
                return key.Split('_').LastOrDefault().ToTitleCase();
            }

            return key.ToTitleCase();
        }
    }

    public class EnumHelper
    {
        public static string GetEnumDescription(Enum enumObj)
        {
            var enumString = enumObj?.ToString();
            if (enumString == null)
                return string.Empty;

            var fieldInfo = enumObj.GetType().GetField(enumString);
            var descAttr = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false)
                .FirstOrDefault();

            if (descAttr == null)
                return enumString;
            else
                return ((DescriptionAttribute)descAttr).Description;
        }

        public static T GetEnumAttribute<T>(Enum enumObj)
        {
            var enumString = enumObj?.ToString();
            if (enumString == null)
                return default;

            var fieldInfo = enumObj.GetType().GetField(enumString);
            return (T)fieldInfo.GetCustomAttributes(typeof(T), false)
                .FirstOrDefault();
        }

        public static Enum GetEnumFromDescription(Type enumType, string description)
        {
            foreach (Enum enumValue in Enum.GetValues(enumType))
            {
                var fieldInfo = enumType.GetField(enumValue.ToString());
                var descAttr = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false)
                                        .FirstOrDefault();
                if (description == ((DescriptionAttribute)descAttr).Description)
                {
                    return enumValue;
                }
            }
            return null;
        }

        public static List<string> GetDescriptionsAsText(Enum enumObject)
        {
            var descriptions = new List<string>();

            foreach (Enum enumValue in Enum.GetValues(enumObject.GetType()))
            {
                if (enumObject.HasFlag(enumValue))
                {
                    descriptions.Add(GetEnumDescription(enumValue));
                }
            }
            return descriptions;
        }

        public static string GetEnumDisplay(Enum enumObj)
        {
            var enumString = enumObj?.ToString();
            if (enumString == null)
                return string.Empty;

            var fieldInfo = enumObj.GetType().GetField(enumString);
            var descAttr = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;

            if (descAttr == null)
                return enumString;
            else
                return AttributeUtils.LookupResource(descAttr.ResourceType, descAttr.Name);
        }
    }

}
