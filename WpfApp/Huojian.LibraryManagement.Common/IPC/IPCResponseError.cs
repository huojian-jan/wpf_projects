using Newtonsoft.Json;

namespace Huojian.LibraryManagement.Common.IPC
{
    public class IPCResponseError
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
