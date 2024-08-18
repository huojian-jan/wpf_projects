using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.JobObjects;

namespace ShadowBot.Common.Utilities
{
    // https://tulisanlain.blogspot.com/2016/08/kill-child-process-when-parent-exit-c.html
    class WindowsJobObject : IDisposable
    {
        private HANDLE handle;
        private bool disposed;

        public unsafe WindowsJobObject(WindowsJobCreateOptions options)
        {
            handle = PInvoke.CreateJobObject(null, (PCWSTR)(null));

            //https://learn.microsoft.com/zh-cn/windows/win32/procthread/job-objects  中管理使用作业对象的进程树, 修改子进程默认被添加进job 的行为
            // JOB_OBJECT_LIMIT_BREAKAWAY_OK = 0x00000800
            // JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000
            JOB_OBJECT_LIMIT limitFlags;
            if (options.ChildProcessAttachOption == ChildProcessAttachOption.Silent)
            {
                limitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_SILENT_BREAKAWAY_OK | JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
            }
            else
            {
                limitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_BREAKAWAY_OK | JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;
            }
            var info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                LimitFlags = limitFlags
            };

            var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = info
            };

            int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

            if (!PInvoke.SetInformationJobObject(handle, JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation, (void*)extendedInfoPtr, (uint)length))
                throw new Exception(string.Format("Unable to set information.  Error: {0}", Marshal.GetLastWin32Error()));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing) { }

            Close();
            disposed = true;
        }

        public void Close()
        {
            PInvoke.CloseHandle(handle);
            handle = (HANDLE)IntPtr.Zero;
        }

        public bool AddProcess(IntPtr processHandle)
        {
            return PInvoke.AssignProcessToJobObject(handle, (HANDLE)processHandle);
        }

        public bool AddProcess(int processId)
        {
            try
            {
                return AddProcess(Process.GetProcessById(processId).Handle);
            }
            catch (Exception ex)  //如果进程运行很快结束,抛异常
            {
                Logging.Warn("AssignProcessToJobObject ", ex);
                return false;
            }
        }
    }

    public class WindowsJobCreateOptions
    {
        public ChildProcessAttachOption ChildProcessAttachOption { get; set; }

        public static WindowsJobCreateOptions Default => new()
        {
            ChildProcessAttachOption = ChildProcessAttachOption.Breakaway
        };

        public static WindowsJobCreateOptions Silent => new()
        {
            ChildProcessAttachOption = ChildProcessAttachOption.Silent
        };
    }

    //管理使用作业对象的进程树, 修改子进程默认被添加进job 的行为
    public enum ChildProcessAttachOption
    {
        //默认不加入
        Silent,
        //进程创建时如果没有带上 CREATE_BREAKAWAY_FROM_JOB 选项, 则加入， 否则不加入
        Breakaway
    }
}
