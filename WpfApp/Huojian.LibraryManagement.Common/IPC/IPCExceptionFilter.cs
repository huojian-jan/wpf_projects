namespace Huojian.LibraryManagement.Common.IPC
{
    public interface IPCExceptionFilter
    {
        void OnException(IPCExceptionContext exceptionContext);
    }
}
