namespace Huojian.LibraryManagement.Common.PInvoke
{
    // https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey
    [Flags]
    public enum ModifierKeys
    {
        None = 0,
        Alt = 0x0001,
        Control = 0x0002,
        Shift = 0x0004,
        Win = 0x0008,
        NoRepeat = 0x4000
    }
}
