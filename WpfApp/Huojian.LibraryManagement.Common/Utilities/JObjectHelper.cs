using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadowBot.Common.Utilities
{
    public static class JObjectHelper
    {
        private static Encoding UTF8Encoding = new UTF8Encoding(false);

        public static JToken FromFile(string fileName)
        {
            return FromFile(fileName, UTF8Encoding);
        }

        public static JToken FromFile(string fileName, Encoding encoding)
        {
            var json = File.ReadAllText(fileName, encoding);
            return JToken.Parse(json);
        }

        public static T FromFile<T>(string fileName)
        {
            return FromFile<T>(fileName, UTF8Encoding);
        }

        public static T FromFile<T>(string fileName, Encoding encoding)
        {
            var json = File.ReadAllText(fileName, encoding);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public static void SaveToFile(object value, string fileName)
        {
            SaveToFile(value, fileName, UTF8Encoding);
        }

        public static void SaveToFile(object value, string fileName, Encoding encoding)
        {
            string json;
            if (value is JToken jValue)
                json = jValue.ToString(Formatting.Indented);
            else
                json = JsonConvert.SerializeObject(value, Formatting.Indented);
            File.WriteAllText(fileName, json, encoding);
        }
    }
}
