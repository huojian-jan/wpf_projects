namespace Huojian.LibraryManagement.Common.IPC
{
    public interface IPCActionFilter
    {
        void OnActionExecuting(IPCContext context);

        void OnActionExecuted(IPCContext context);
    }
}
