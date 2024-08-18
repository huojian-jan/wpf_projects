using System;
namespace Huojian.LibraryManagement.Common.IPC
{
    [Serializable]
    public class IPCException : Exception
    {
        public IPCException(IPCResponseCode code, string message)
            : base(message)
        {
            Code = code;
        }

        public IPCResponseCode Code { get; }
    }
}
