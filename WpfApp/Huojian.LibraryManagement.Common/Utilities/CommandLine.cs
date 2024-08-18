using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public class CommandLine : IEnumerable<KeyValuePair<string, string>>
    {
        #region static

        static CommandLine()
        {
            Current = new CommandLine(Environment.GetCommandLineArgs());
        }

        public static CommandLine Current { get; }

        #endregion


        const string _pattern = @"^-{1,2}([^=]+)(=(.*))?$";

        List<KeyValuePair<string, string>> _args = new List<KeyValuePair<string, string>>();

        public CommandLine(string[] args)
        {
            RawArgs = args;
            var regex = new Regex(_pattern);
            foreach (var arg in args)
            {
                var match = regex.Match(arg);
                string key = null, value = null;
                if (match.Success)
                {
                    key = match.Groups[1].Value.ToLower();
                    value = match.Groups[3].Value.Trim('"');
                }
                else
                {
                    value = arg.Trim('"');
                }
                _args.Add(new KeyValuePair<string, string>(key, value));
            }
        }

        public string[] RawArgs { get; private set; }

        public string this[string key]
        {
            get
            {
                foreach (var arg in _args)
                    if (arg.Key == key)
                        return arg.Value;
                return null;
            }
        }

        public string GetString(string key)
        {
            return this[key];
        }

        public bool HasKey(string key)
        {
            return this[key] != null;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _args.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_args).GetEnumerator();
        }
    }
}
