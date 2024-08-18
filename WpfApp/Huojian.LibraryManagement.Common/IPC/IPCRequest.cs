using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Huojian.LibraryManagement.Common.IPC
{
    // Schema::
    // {
    //     "command": "XXX.YYY",
    //     "timeout": 100,
    //     "params": {
    //         "name": "sanco",
    //         "age": 29
    //     }
    // }

    public class IPCRequest
    {
        private string _method;
        [JsonProperty("method")]
        public string Method
        {
            get { return _method; }
            set
            {
                _method = value;
                if (_method.IndexOf('.') > -1)
                {
                    var tokens = _method.Split(new char[] { '.' }, 2);
                    ServiceName = tokens[0];
                    ActionName = tokens[1];
                }
            }
        }

        [JsonProperty("options")]
        public JObject Options { get; set; }

        [JsonProperty("params")]
        public JToken Params { get; set; }

        [JsonIgnore]
        internal string ServiceName { get; private set; }

        [JsonIgnore]
        internal string ActionName { get; private set; }

        internal bool TryResolve(string name, Type type, out object value)
        {
            if (Params == null || Params.Type == JTokenType.Null)
            {
                value = null;
                return false;
            }
            else
            {
                if (Params is JObject jObject
                    && jObject.TryGetValue(name, out JToken jValue))
                {
                    value = jValue.Resolve(type);
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }
        }

        // 为了方便日志记录
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Method: {Method}");
            if (Params != null)
                builder.Append($"Params: {Params}");
            return builder.ToString();
        }
    }
}
