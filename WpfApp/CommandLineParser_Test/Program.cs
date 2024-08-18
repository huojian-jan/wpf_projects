using System.Globalization;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Microsoft.Win32.SafeHandles;

namespace CommandLineParser_Test
{
    internal class Program
    {
        static void Main(string[] args)
        {

            if (!PInvoke.OpenClipboard(HWND.Null))
            {
                Console.WriteLine("failed to open clipboard");
            }

            SafeFileHandle handle = PInvoke.GetClipboardData_SafeHandle(1);
           if(handle!= new SafeFileHandle(IntPtr.Zero,false))
           {
               unsafe
               {
              var textPtr= (char*)PInvoke.GlobalLock(handle);
             var text= Marshal.PtrToStringAnsi((IntPtr)textPtr);
             var a = 100;
               }
           }
            Console.ReadKey();

        }
    }
}
