
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using Huojian.LibraryManagement.Common;

namespace ShadowBot.Common.Utilities
{
    public class ClipboardHelper
    {

        public static void SetText(string value)
        {
            TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                if (string.IsNullOrEmpty(value))
                {
                    Clipboard.Clear();
                    
                }
                else
                {
                    Clipboard.SetText(value);
                }
                return true;
            }, true, "xxx");
        }

        public static bool TrySetText(string value)
        {
            try
            {
                SetText(value);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                return false;
            }
        }

        public static string GetText()
        {
            return TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                return Clipboard.GetText(TextDataFormat.UnicodeText);
            }, false, "xx");
        }

        public static bool TryGetText(out string text)
        {
            try
            {
                text = GetText();
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                text = string.Empty;
                return false;
            }
        }

        public static void SetImage(MediaTypeNames.Image image)
        {
            TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                //Clipboard.SetImage(image);
                //.Net Core中上面这个函数目前存在内存泄漏，所以我们暂时选择使用低级剪切板API(非OLE剪切板API)来临时解决
                if (!PInvoke.OpenClipboard(default(HWND)))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                if (!PInvoke.EmptyClipboard())
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                Bitmap bitmap = new Bitmap(image);
                var hBitmap_By_GetHBITMAP = bitmap.GetHbitmap();
                bitmap.Dispose();
                bitmap = null;
                HANDLE hCopiedBitmap_DDB = PInvoke.CopyImage((HANDLE)hBitmap_By_GetHBITMAP, GDI_IMAGE_TYPE.IMAGE_BITMAP, 0, 0, IMAGE_FLAGS.LR_COPYDELETEORG);
                if (hCopiedBitmap_DDB == IntPtr.Zero)
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    PInvoke.DeleteObject((HGDIOBJ)hBitmap_By_GetHBITMAP);
                    hBitmap_By_GetHBITMAP = IntPtr.Zero;
                    throw new Win32Exception(errorCode);
                }
                hBitmap_By_GetHBITMAP = IntPtr.Zero;
                if (PInvoke.SetClipboardData((uint)CLIPBOARD_FORMATS.CF_BITMAP, hCopiedBitmap_DDB) == IntPtr.Zero)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                PInvoke.CloseClipboard();
                return true;
            }, true, "");
        }

        public static bool TrySetImage(MediaTypeNames.Image image)
        {
            try
            {
                SetImage(image);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                return false;
            }
        }

        public static MediaTypeNames.Image GetImage()
        {
            return TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                return Clipboard.GetImage();
            }, false,"xx");
        }

        public static bool TryGetImage(out MediaTypeNames.Image image)
        {
            try
            {
                image = GetImage();
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                image = null;
                return false;
            }
        }

        public static void SetFile(IEnumerable<string> filePaths)
        {
            var filePathCollection = new StringCollection();
            foreach (var file in filePaths)
                filePathCollection.Add(file);

            TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                Clipboard.SetFileDropList(filePathCollection);
                return true;
            }, true,"xx");
        }

        public static bool TrySetFile(IEnumerable<string> filePaths)
        {
            try
            {
                SetFile(filePaths);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                return false;
            }
        }

        public static void SetDataObject(object data)
        {
            TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                Clipboard.SetDataObject(data);
                return true;
            }, true,"xx");
        }

        public static bool TrySetDataObject(object data)
        {
            try
            {
                SetDataObject(data);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                return false;
            }
        }

        public static IDataObject GetDataObject()
        {
            return TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                return Clipboard.GetDataObject();
            }, false,"xx");
        }

        public static bool TryGetDataObject(out IDataObject data)
        {
            try
            {
                data = GetDataObject();
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                data = null;
                return false;
            }
        }

        public static void SetData(string format, object data)
        {
            TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                System.Windows.Clipboard.SetData(format, data);
                return true;
            }, true,"xx");
        }

        public static bool TrySetData(string format, object data)
        {
            try
            {
                SetData(format, data);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                return false;
            }
        }

        public static object GetData(string format)
        {
            return TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                return Clipboard.GetData(format);

            }, false, "xx");
        }

        public static bool TryGetData(string format, out object obj)
        {
            try
            {
                obj = GetData(format);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                obj = null;
                return false;
            }
        }

        public static void Clear()
        {
            TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                Clipboard.Clear();
                return true;
            }, true, "xx");
        }

        public static bool TryClear()
        {
            try
            {
                Clear();
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                return false;
            }
        }

        // 获取剪切板中文件(夹)路径列表
        public static string[] GetFileDropList()
        {
            return TryToDoSomethingAfterTakeOwnershipOfTheClipboard(() =>
            {
                var paths = new List<string>();
                if (Clipboard.ContainsFileDropList())
                {
                    var dropList = Clipboard.GetFileDropList();
                    foreach (var item in dropList)
                    {
                        paths.Add(item);
                    }
                    return paths.ToArray();
                }
                return new string[0];
            }, false, "xx");
        }

        public static bool TryGetFileDropList(out string[] paths)
        {
            try
            {
                paths = GetFileDropList();
                return true;
            }
            catch (Exception ex)
            {
                Logging.Warn(ex.Message);
                paths = new string[0];
                return false;
            }
        }

        static bool SetClipboardOwner(bool takeOwnership)
        {
            bool result = false;
            //因为Windows没有提供取得剪切板所有权的函数，所以我们只能通过调用EmptyClipboard来尝试取得剪切板的所有权
            try
            {
                if (!PInvoke.OpenClipboard(default(HWND)))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                if (takeOwnership)
                {
                    if (!PInvoke.EmptyClipboard())
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
                PInvoke.CloseClipboard();

                result = true;
            }
            catch (Win32Exception ex)
            {
                Logging.Warn(string.Format("SetClipboardOwner invoke error\n error message：{0}", ex));
            }

            return result;
        }
        public unsafe static TResult TryToDoSomethingAfterTakeOwnershipOfTheClipboard<TResult>(Func<TResult> something, bool needTakeOwnership, string errorText)
        {
            bool result = false;
            TResult obj = default(TResult);
            uint processId = 0;
            Exception ex_lastInDoClip = null;
            for (int i = 0; i < 100; i++)//超时为10秒
            {
                processId = 0;
                ex_lastInDoClip = null;
                bool canDoSomething = true;
                if (needTakeOwnership)
                {
                    HWND windowHandle = PInvoke.GetClipboardOwner();
                    if (windowHandle != IntPtr.Zero)
                    {
                        var tid = PInvoke.GetWindowThreadProcessId(windowHandle, &processId);
                        if (tid != PInvoke.GetCurrentThreadId())    // 剪贴板被其他进程或者本进程的其他线程占用
                        {
                            if (!SetClipboardOwner(true))
                            {
                                Thread.Sleep(100);
                                canDoSomething = false;
                            }
                        }
                        else
                        {
                            processId = 0;
                        }
                    }
                }

                if (canDoSomething)
                {
                    SetClipboardOwner(false);
                    try
                    {
                        obj = something();
                        result = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        ex_lastInDoClip = ex;
                        Logging.Warn(string.Format("【Clipboard operation】Execution Something failed \n Error message:{0}", ex));
                        Thread.Sleep(100);
                    }
                }
            }

            if (!result)
            {
                if (processId != 0)
                {
                    Process process = null;
                    try
                    {
                        process = Process.GetProcessById((int)processId);
                    }
                    catch (Exception ex)
                    {
                        Logging.Warn(string.Format("Find process failed \n Error message:{0}", ex));
                    }

                    string processInfo = string.Empty;
                    if (process != null)
                    {
                        processInfo = ProcessHelper.GetProcessFileName(process, false);
                    }
                    processInfo += $"[{processId}]";

                    throw new Exception(string.Format("xx", processInfo, errorText));
                }
                else if (ex_lastInDoClip != null)
                {
                    throw ex_lastInDoClip;
                }
                else
                {
                    throw new Exception("xx");
                }
            }

            return obj;
        }
    }
}
