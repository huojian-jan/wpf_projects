namespace Huojian.LibraryManagement.Common.IPC
{
    public enum IPCResponseCode
    {
        Pending = 100,
        OK = 200,
        BadRequest = 400,
        NotFound = 404,
        RequestTimeout = 408,
        InternalServerError = 500
    }

    public static class IPCExceptionCodeExtension
    {
        public static string GetDescription(this IPCResponseCode code)
        {
            switch (code)
            {
                case IPCResponseCode.Pending:
                    return "Pending";
                case IPCResponseCode.OK:
                    return "OK";
                case IPCResponseCode.BadRequest:
                    return "Bad Request";
                case IPCResponseCode.NotFound:
                    return "Not Found";
                case IPCResponseCode.RequestTimeout:
                    return "Request Timeout";
                case IPCResponseCode.InternalServerError:
                    return "Internal Server Error";
                default:
                    return "Unknown";
            }
        }
    }
}
