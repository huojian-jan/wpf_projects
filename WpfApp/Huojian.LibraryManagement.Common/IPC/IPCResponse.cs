using Huojian.LibraryManagement.Common.ObjectModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Huojian.LibraryManagement.Common.IPC
{
    // Schema::
    // {
    //     "code": 200,
    //     "status": "OK",
    //     "result": {
    //         "content": null,
    //         "error": {
    //             "code": 1001,
    //             "message": "control not found"
    //         }
    //     }
    // }

    public class IPCResponse
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("result")]
        public IPCResult Result { get; set; }

        internal Promise PromiseResult { get; private set; }

        public bool TryGetContent<T>(out T content, out string error)
        {
            var succ = TryGetContent(out JToken value, out error);
            if (succ)
                content = value.Resolve<T>();
            else
                content = default;
            return succ;
        }

        public bool TryGetContent(out JToken content, out string error)
        {
            if (Code == (int)IPCResponseCode.OK)
            {
                if (Result.Error == null)
                {
                    content = Result.Content;
                    error = null;
                    return true;
                }
                else
                {
                    content = null;
                    error = Result.Error.Message;
                    return false;
                }
            }
            else
            {
                content = null;
                error = Status;
                return false;
            }
        }

        public static IPCResponse InternalFailure(IPCException ex)
        {
            return new IPCResponse
            {
                Code = (int)ex.Code,
                Status = $"{ex.Code.GetDescription()}, {ex.Message}"
            };
        }

        public static IPCResponse OK(object value)
        {
            if (value is Promise promise)
            {
                return new IPCResponse
                {
                    Code = (int)IPCResponseCode.Pending,
                    Status = IPCResponseCode.Pending.GetDescription(),
                    PromiseResult = promise,
                    Result = new IPCResult
                    {
                        Content = JToken.FromObject(promise.Id)
                    }
                };
            }
            else
            {
                return new IPCResponse
                {
                    Code = (int)IPCResponseCode.OK,
                    Status = IPCResponseCode.OK.GetDescription(),
                    Result = new IPCResult
                    {
                        Content = value == null ? null : JToken.FromObject(value)
                    }
                };
            }
        }

        public static IPCResponse Failure(int code, string message)
        {
            return new IPCResponse
            {
                Code = (int)IPCResponseCode.OK,
                Status = IPCResponseCode.OK.GetDescription(),
                Result = new IPCResult
                {
                    Error = new IPCResponseError
                    {
                        Code = code,
                        Message = message
                    }
                }
            };
        }
    }
}
