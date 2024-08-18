using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public static class CommonRegex
    {
        public static readonly Regex Email = new Regex("^[A-Za-z0-9_-\u4e00-\u9fa5\\.]+@[a-zA-Z0-9_-]+(\\.[a-zA-Z0-9_-]+)+$");
        public static readonly Regex Phone = new Regex("^[1]([3-9])[0-9]{9}$");
        public static readonly Regex PythonModule = new Regex("^[a-zA-Z_][a-zA-Z0-9_]*$");
        public static readonly Regex PythonVariable = new Regex(@"^[a-zA-Z_\u4e00-\u9fa5][a-zA-Z0-9_\u4e00-\u9fa5]*$");
        public static readonly Regex Chinese = new Regex(@"[\u4e00-\u9fa5]");
        public static readonly Regex GlobalVariableInSelector = new Regex(@"^{{([a-zA-Z_\u4e00-\u9fa5][a-zA-Z0-9_\u4e00-\u9fa5]*)}}$");
    }
}
