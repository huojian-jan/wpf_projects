using System;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

namespace ShadowBot.Common.Utilities
{    //.net core目前并没有实现Marshal.GetActiveObject https://github.com/dotnet/runtime/issues/37617 
     //https://github.com/dotnet/runtime/blob/main/src/coreclr/System.Private.CoreLib/src/System/Runtime/InteropServices/Marshal.CoreCLR.cs
     //所以我们需要移入.Net Framework中的该部分 https://github.com/microsoft/referencesource/blob/master/mscorlib/system/runtime/interopservices/marshal.cs

    /// <summary>
    /// This class contains methods that are mainly used to marshal between unmanaged
    /// and managed types.
    /// </summary>
    public static class MarshalEx
    {
        //====================================================================
        // This method gets the currently running object.
        //====================================================================
        [System.Security.SecurityCritical]  // auto-generated_required
        public static Object GetActiveObject(String progID)
        {
            Object obj = null;
            Guid clsid;

            // Call CLSIDFromProgIDEx first then fall back on CLSIDFromProgID if
            // CLSIDFromProgIDEx doesn't exist.
            try
            {
                CLSIDFromProgIDEx(progID, out clsid);
            }
            //            catch
            catch (Exception)
            {
                CLSIDFromProgID(progID, out clsid);
            }

            GetActiveObject(ref clsid, IntPtr.Zero, out obj);
            return obj;
        }

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2050:UnrecognizedReflectionPattern",
            Justification = "The calling method is annotated with RequiresUnreferencedCode")]
        [DllImport("ole32.dll", PreserveSig = false)]
        private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid);

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2050:UnrecognizedReflectionPattern",
            Justification = "The calling method is annotated with RequiresUnreferencedCode")]
        [DllImport("ole32.dll", PreserveSig = false)]
        private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] String progId, out Guid clsid);

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2050:UnrecognizedReflectionPattern",
            Justification = "The calling method is annotated with RequiresUnreferencedCode")]
        [DllImport("oleaut32.dll", PreserveSig = false)]
        private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out Object ppunk);
    }
}