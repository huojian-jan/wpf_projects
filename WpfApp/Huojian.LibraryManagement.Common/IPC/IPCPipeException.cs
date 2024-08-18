namespace Huojian.LibraryManagement.Common.IPC
{
    [Serializable]
    public class IPCPipeException : Exception
    {
        public IPCPipeException() { }
        public IPCPipeException(string message) : base(message) { }
        public IPCPipeException(string message, Exception inner) : base(message, inner) { }
        protected IPCPipeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }


    [Serializable]
    public class IPCPipeTimeoutException : IPCPipeException
    {
        public IPCPipeTimeoutException() { }
        public IPCPipeTimeoutException(string message) : base(message) { }
        public IPCPipeTimeoutException(string message, Exception inner) : base(message, inner) { }
        protected IPCPipeTimeoutException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }

    [Serializable]
    public class IPCEndOfStreamException : IPCPipeException
    {
        public IPCEndOfStreamException() { }
        public IPCEndOfStreamException(string message) : base(message) { }
        public IPCEndOfStreamException(string message, Exception inner) : base(message, inner) { }
        protected IPCEndOfStreamException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
