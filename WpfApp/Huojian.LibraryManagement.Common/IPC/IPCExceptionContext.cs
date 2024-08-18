namespace Huojian.LibraryManagement.Common.IPC
{
    public class IPCExceptionContext
    {
        public IPCExceptionContext(IPCContext context, Exception error)
        {
            Context = context;
            Exception = error;
        }

        public IPCContext Context { get; }
        public Exception Exception { get; }
        public bool Handled { get; set; }
    }
}
