using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Huojian.LibraryManagement.Common.IPC
{
    public class IPCResult
    {
        [JsonProperty("content")]
        public JToken Content { get; set; }

        [JsonProperty("error")]
        public IPCResponseError Error { get; set; }
    }
}
