namespace Huojian.LibraryManagement.Common.IPC
{
    public class IPCContext
    {
        public IPCContext()
        { }

        public IPCContext(IPCRequest request)
        {
            Request = request;
        }

        public IPCRequest Request { get; }

        public IPCResponse Response { get; set; }

        public IServiceContainer ServiceContainer { get; set; }
    }
}
