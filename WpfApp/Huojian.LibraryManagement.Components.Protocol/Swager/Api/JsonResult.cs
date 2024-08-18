using Huojian.LibraryManagement.Components.Protocol.Swager.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Huojian.LibraryManagement.Components.Protocol.Swager
{
    public class JsonResult
    {

        [JsonProperty("requestId")]
        public string RequestId { get; private set; }

        [JsonProperty("code")]
        public int Code { get; private set; }

        [JsonProperty("msg")]
        public string Error { get; private set; }

        [JsonProperty("data")]
        public JToken Data { get; private set; }

        [JsonProperty("page")]
        public JsonResultPage Page { get; private set; }

        public RestResponse Response { get; set; }

        internal static JsonResult Fail(string error)
        {
            return new JsonResult { Code = -1, Error = error };
        }

        internal static JsonResult Fail(string error, int errorCode)
        {
            return new JsonResult { Code = errorCode, Error = error };
        }

        internal static JsonResult OK()
        {
            return new JsonResult { Code = 200 };
        }

        internal static JsonResult OK(JToken data)
        {
            return new JsonResult { Code = 200, Data = data };
        }
    }
}