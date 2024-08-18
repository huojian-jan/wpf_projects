using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class VariableHelper
    {
        public static string Any = "any";
        public static string Int = "int";
        public static string Float = "float";
        public static string Bool = "bool";
        public static string Str = "str";
        public static string File = "file";
        public static string SList = "list";
        public static string ListAny = "list<any>";
        public static string ListWeb = "list<xbot._web.element.WebElement>";
        public static string ListStr = "list<str>";
        public static string ListWin32 = "list<xbot.win32.element.Win32Element>";
        public static string Dict = "dict";
        private static Dictionary<string, string> defaultValueDict = new Dictionary<string, string>
        {
            { Int, @"0"},
            { Float, @"0.0"},
            { Bool, @"False"},
            { SList, @"[]"},
            { ListAny, @"[]"},
            { ListWeb, @"[]"},
            { ListWin32, @"[]"},
            { Dict, @"{}"},
            { ListStr, "[]"}
        };
        

        public static string GetDefaultValue(string key)
        {
            if (defaultValueDict.ContainsKey(key))
                return defaultValueDict[key];
            return "None";
        }
    }
}
