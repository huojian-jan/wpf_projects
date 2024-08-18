namespace Huojian.LibraryManagement.Common.IPC
{
    public interface IServiceContainer
    {
        TService Resolve<TService>();
    }
}
