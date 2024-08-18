using System;
using System.Runtime.InteropServices;
using Windows.Win32;

namespace ShadowBot.Common.Utilities
{
    public class WineInterfaces
    {
        //https://gist.github.com/b0urb4k1/da912b4e047583fdb56af9fe37c3047d

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        delegate void wine_get_host_version_type([MarshalAs(UnmanagedType.LPStr)] out string sysname, [MarshalAs(UnmanagedType.LPStr)] out string release);

        [DllImport("Kernel32", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Unicode)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern string wine_get_dos_file_name([MarshalAs(UnmanagedType.LPStr)] string str);

        [DllImport("Kernel32", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        public static extern string wine_get_unix_file_name([MarshalAs(UnmanagedType.LPWStr)] string dosW);

        public enum WineHostType
        {
            Windows,
            Linux,
            Darwin,
            Unknown
        };

        public static WineHostType GetWineHostVersion(out string release)
        {
            release = null;
            WineHostType wineHostType = WineHostType.Unknown;
            var hmod_ntdll = PInvoke.GetModuleHandle("ntdll.dll");
            if (hmod_ntdll.IsInvalid)
            {
                //Console.WriteLine("无法获取到ntdll.dll模块的实例句柄！");
            }
            else
            {
                var pfn_wine_get_host_version = PInvoke.GetProcAddress(hmod_ntdll, "wine_get_host_version");
                if (pfn_wine_get_host_version.IsNull)
                {
                    //Console.WriteLine("当前程序不在Wine下运行！");
                    wineHostType = WineHostType.Windows;
                }
                else
                {
                    wine_get_host_version_type wine_get_host_version = (wine_get_host_version_type)Marshal.GetDelegateForFunctionPointer(pfn_wine_get_host_version.Value, typeof(wine_get_host_version_type));
                    string sysname;
                    wine_get_host_version(out sysname, out release);
                    //Console.WriteLine($"Wine host info => {sysname} : {release}");
                    wineHostType = (WineHostType)Enum.Parse(typeof(WineHostType), sysname);
                }
            }
            return wineHostType;
        }

        public static WineHostType GetWineHostType()
        {
            string release;
            return GetWineHostVersion(out release);
        }
    }
}
