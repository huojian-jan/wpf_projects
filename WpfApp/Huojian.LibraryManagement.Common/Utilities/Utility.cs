using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace ShadowBot.Common.Utilities
{
    public static class Utility
    {
        private static readonly HashSet<string> _pythonKeyWord = new HashSet<string> { "True","False","if","elif","else","in","del",
                                                                                 "for","while","break","and","or","not",
                                                                                 "def", "return","yield","class","from","import","as",
                                                                                 "assert","is","pass","None","try","except","else",
                                                                                 "finally","with","as","global","nonlocal","lambda",
                                                                                 "await","async" };

        public static byte[] ReadBytes(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 提供一个将通配符表达式转换为正则的方法
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string WildcardToRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern))
                return "^$";

            string escapeStr = Regex.Escape(pattern);
            return "^"
                    + escapeStr.Replace("\\*", ".*").Replace("\\?", ".")
                    + "$";
        }

        /// <summary>
        /// 指示所指定的通配符表达式在指定的输入字符串中是否找到了匹配项。
        /// </summary>
        /// <param name="input"></param>
        /// <param name="wildcards"></param>
        /// <returns></returns>
        public static bool IsStringMatchWildcards(string input, string wildcards)
        {
            string strWc2Reg = WildcardToRegex(wildcards);
            return Regex.IsMatch(input, strWc2Reg, RegexOptions.Multiline | RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 获取匹配两个字符串的通配符表达式
        /// '*'匹配多个字符
        /// '?'匹配一个字符  但是自动匹配不会生成？ 用户手动修改通配符为？是可以的
        /// </summary>
        /// <param name="pattern1">模板1</param>
        /// <param name="pattern2">模板2</param>
        /// <returns>
        /// 1.成功，返回通配符表达式
        /// 2.失败返回null （失败情况下uipath是用添加属性idx来解决的todo...）
        /// </returns>
        public static string GetWildcardsMatch2Str(string pattern1, string pattern2)
        {
            if (string.IsNullOrEmpty(pattern1) || string.IsNullOrEmpty(pattern2))
                return null;

            if (IsStringMatchWildcards(pattern2, pattern1))
                return pattern1;
            if (IsStringMatchWildcards(pattern1, pattern2))
                return pattern2;

            //获取产生wildcard的模板
            string template = pattern1.Length <= pattern2.Length ? pattern1 : pattern2;
            StringBuilder sb = new StringBuilder(template);
            for (int i = 0; i < sb.Length + 1; i++)
            {
                sb.Insert(i, '*');
                if (IsStringMatchWildcards(pattern1, sb.ToString()) && IsStringMatchWildcards(pattern2, sb.ToString()))
                    return sb.ToString();
                else
                    sb.Remove(i, 1);
            }
            return null;
        }

        /// <summary>
        /// 替换文本中的最后一个匹配项
        /// <para>
        /// https://stackoverflow.com/questions/14825949/replace-the-last-occurrence-of-a-word-in-a-string-c-sharp
        /// </para>
        /// </summary>
        /// <returns></returns>
        public static string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);

            if (place == -1)
                return source;

            string result = source.Remove(place, find.Length).Insert(place, replace);
            return result;
        }

        /// <summary>
        /// 拆分命令行字符串
        /// <para>https://stackoverflow.com/questions/298830/split-string-containing-command-line-parameters-into-string-in-c-sharp</para>
        /// </summary>
        /// <param name="commandLine"></param>
        /// <returns></returns>
        public unsafe static string[] SplitCommandLine(string commandLine)
        {
            var argv = PInvoke.CommandLineToArgv(commandLine, out int argc);
            if (argv == null)
                throw new System.ComponentModel.Win32Exception();
            try
            {
                var args = new string[argc];
                for (var i = 0; i < args.Length; i++)
                {
                    var p = argv[i];
                    args[i] = p.ToString();
                }
                return args;
            }
            finally
            {
                PInvoke.LocalFree((nint)argv);
            }
        }

        public static bool IsValidPythonVariableName(string value, bool allowChinese = true, bool canEmpty = true)
        {
            if (string.IsNullOrEmpty(value))
                return canEmpty ? true : false;
            if (!allowChinese)
            {
                if (CommonRegex.Chinese.IsMatch(value))
                    return false;
            }
            return CommonRegex.PythonVariable.IsMatch(value);
        }

        public static bool IsPythonKeyWord(string value)
        {
            return _pythonKeyWord.Contains(value);
        }

        public static int GetAvailablePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }

        public static T GetObjectPropertyValue<T>(object obj, string name, BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
        {
            var pi = obj.GetType().GetProperty(name, bindingAttr);
            T value = (T)pi.GetValue(obj);
            return value;
        }
    }
}
