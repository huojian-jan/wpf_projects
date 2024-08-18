using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32.SafeHandles;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace ShadowBot.Common.Utilities
{
    public class ProcessDump
    {
        public enum UserRespondForDumpProcessKind
        {
            onlyDump,
            accessToken,
            userName,
        }
        public delegate object CallbackEventQueryUserInfoForDumpProcess(UserRespondForDumpProcessKind userRespondForDumpProcessKind);
        delegate void CallbackEventLaunchCrashReporterForDumpProcess(int pid, int errorCode, [MarshalAs(UnmanagedType.LPWStr)] string errorStack, DumpType dumpType);

        const string ShadowBotNativeHelperDllPath = "ShadowBot.NativeHelper.dll";
        [DllImport(ShadowBotNativeHelperDllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        extern static bool EnableCrashDumperOnce(CallbackEventLaunchCrashReporterForDumpProcess callback);

        public enum DumpType
        {
            normal,
            withheap,
            triage,
            full,
        }
        public static bool Dump(int pid, string dumpName = "%tmp%/dump", DumpType dumpType = DumpType.withheap, bool diag = true)
        {
            var assembly = Assembly.GetAssembly(typeof(string));
            FileInfo fileInfo = new FileInfo(assembly.Location);
            var DumpGeneratorName = fileInfo.DirectoryName + "\\createdump.exe";
            if (!File.Exists(DumpGeneratorName))
            {
                Debug.Print("createdump.exe 不存在！", DumpGeneratorName);
                return false;
            }

            StringBuilder commandLine = new StringBuilder();
            commandLine.AppendFormat("{0}", pid);

            if (dumpName != null)
            {
                var expandedDumpName = Environment.ExpandEnvironmentVariables(dumpName);
                var dirName = Path.GetDirectoryName(expandedDumpName);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
                commandLine.AppendFormat(" --name \"{0}\"", expandedDumpName);
            }

            commandLine.AppendFormat(" --{0}", dumpType);

            if (diag)
            {
                commandLine.AppendFormat(" --diag");
            }

            var processStartInfo = new ProcessStartInfo(DumpGeneratorName, commandLine.ToString());
            processStartInfo.WorkingDirectory = AppContext.BaseDirectory;
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.CreateNoWindow = true;
            var process = Process.Start(processStartInfo);
            process.WaitForExit();

            return process.ExitCode == 0;
        }

        static CallbackEventQueryUserInfoForDumpProcess _CallbackEventQueryUserInfoForDumpProcess = null;
        public static void SetCallbackEventQueryUserInfoForDumpProcess(CallbackEventQueryUserInfoForDumpProcess callbackEventQueryUserInfoForDumpProcess)
        {
            _CallbackEventQueryUserInfoForDumpProcess = callbackEventQueryUserInfoForDumpProcess;
        }
        static int _HideActiveWindowStateByLaunchCrashReporterForDumpProcess = 0;
        public static void LaunchCrashReporterForDumpProcess(int pid, int errorCode, string errorStack, DumpType dumpType, bool willTerminateCurProcess)
        {
            HWND hWnd = default(HWND);
            List<KeyboardHook> UninstalledAll_K = null;
            List<MouseHook> UninstalledAll_M = null;
            try
            {
                //隐藏可能的蒙层窗口，不然会因为其他窗口在下面而无法操作到
                if (_HideActiveWindowStateByLaunchCrashReporterForDumpProcess == 0)
                {
                    hWnd = PInvoke.GetActiveWindow();
                    if (hWnd != IntPtr.Zero)
                    {
                        _HideActiveWindowStateByLaunchCrashReporterForDumpProcess = 1;
                        //在一些异常发生时如DUCE::SyncFlush错误或者配额不足时，ShowWindow调用内部会再次造成WPF抛异常，进而造成递归重复
                        PInvoke.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_HIDE);
                        _HideActiveWindowStateByLaunchCrashReporterForDumpProcess = 2;
                    }
                }

                //调试测试时，可以取消注释下面这段代b
                //Thread t = new Thread(() => MessageBox.Show("LaunchCrashReporterForDumpProcess"));
                //t.Start();
                //while (t.IsAlive)
                //{
                //    Thread.Sleep(1);
                //}

                //如果本进程将被终止的话，则先取消注册所有的热键，不然如果应用程序重启后，本进程被终止前将导致热键无法被新启动的进程实例所注册成功
                if (willTerminateCurProcess)
                {
                    HotKeyRegistry.UnRegisterAllHotKeys();
                }

                //执行创建进程的Dump文件前需要将本进程中的所有键盘和鼠标钩子全部临时卸载掉，不然将导致系统在Dump期间失去响应（卡死）
                UninstalledAll_K = KeyboardHook.UninstallAll();
                UninstalledAll_M = MouseHook.UninstallAll();
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                try
                {
                    Logging.Info(e);
                }
                catch { }
            }

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(AppContext.BaseDirectory + "ShadowBot.CrashReporter.exe", $"{pid}");
                startInfo.Environment.Add("LaunchCrashReporterForDumpProcess_ErrorCode", errorCode.ToString());
                startInfo.Environment.Add("LaunchCrashReporterForDumpProcess_ErrorStack", errorStack);
                startInfo.Environment.Add("LaunchCrashReporterForDumpProcess_DumpType", dumpType.ToString());
                var LaunchCrashReporterForDumpProcess_WaitEventName = "LaunchCrashReporterForDumpProcess_{" + Guid.NewGuid() + "}";
                var LaunchCrashReporterForDumpProcess_WaitEvent = new EventWaitHandle(false, EventResetMode.AutoReset, LaunchCrashReporterForDumpProcess_WaitEventName);
                startInfo.Environment.Add("LaunchCrashReporterForDumpProcess_WaitEventName", LaunchCrashReporterForDumpProcess_WaitEventName);
                startInfo.Environment.Add("LaunchCrashReporterForDumpProcess_StartCommandLine", string.Join(" ", Environment.GetCommandLineArgs()));
                bool onlyDump = false;
                string accessToken = null;
                string userName = null;
                if (_CallbackEventQueryUserInfoForDumpProcess != null)
                {
                    onlyDump = (bool)_CallbackEventQueryUserInfoForDumpProcess(UserRespondForDumpProcessKind.onlyDump);
                    accessToken = (string)_CallbackEventQueryUserInfoForDumpProcess(UserRespondForDumpProcessKind.accessToken);
                    userName = (string)_CallbackEventQueryUserInfoForDumpProcess(UserRespondForDumpProcessKind.userName);
                }
                startInfo.Environment.Add("LaunchCrashReporterForDumpProcess_OnlyDump", onlyDump.ToString());
                startInfo.Environment.Add("LaunchCrashReporterForDumpProcess_AccessToken", accessToken);
                startInfo.Environment.Add("LaunchCrashReporterForDumpProcess_UserName", userName);
                var process = Process.Start(startInfo);
                var waits = new HANDLE[2];
                waits[0] = new HANDLE(process.Handle);
                waits[1] = new HANDLE(LaunchCrashReporterForDumpProcess_WaitEvent.Handle);
                PInvoke.WaitForMultipleObjects(waits, false, PInvoke.INFINITE);
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                try
                {
                    Logging.Error(e);
                }
                catch { }
            }

            try
            {
                //Dump执行结束可以将钩子恢复回去，以恢复钩子相关的用户功能
                if (UninstalledAll_K != null)
                {
                    KeyboardHook.ReinstallAll(UninstalledAll_K);
                }
                if (UninstalledAll_M != null)
                {
                    MouseHook.ReinstallAll(UninstalledAll_M);
                }

                //恢复显示可能的蒙层窗口
                if (_HideActiveWindowStateByLaunchCrashReporterForDumpProcess == 2)//仅在首次隐藏活动窗口成功时才尝试恢复显示活动窗口
                {
                    if (hWnd != IntPtr.Zero && PInvoke.IsWindow(hWnd))
                    {
                        //在一些异常发生时如DUCE::SyncFlush错误或者配额不足时，ShowWindow调用内部会再次造成WPF抛异常，进而造成递归重复
                        PInvoke.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_SHOW);
                        _HideActiveWindowStateByLaunchCrashReporterForDumpProcess = 3;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                try
                {
                    Logging.Info(e);
                }
                catch { }
            }
        }

        public delegate void CallbackEventReleaseManagedResourcesOnNativeCrash();
        public static event CallbackEventReleaseManagedResourcesOnNativeCrash callbackEventReleaseManagedResourcesOnNativeCrash;
        public static void LaunchCrashReporterForDumpProcess_Native(int pid, int errorCode, string errorStack, DumpType dumpType)
        {
            //调试测试时，可以取消注释下面这段代码

            ////隐藏可能的蒙层窗口，不然会因为其他窗口在下面而无法操作到
            //var hWnd = PInvoke.GetActiveWindow();
            //if (!hWnd.IsNull)
            //{
            //    //在一些异常发生时如DUCE::SyncFlush错误或者配额不足时，ShowWindow调用内部会再次造成WPF抛异常，进而造成递归重复
            //    PInvoke.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_HIDE);
            //}

            //Thread t = new Thread(() => MessageBox.Show("LaunchCrashReporterForDumpProcess_Native"));
            //t.Start();
            //while (t.IsAlive)
            //{
            //    Thread.Sleep(1);
            //}

            if (callbackEventReleaseManagedResourcesOnNativeCrash != null)
            {
                callbackEventReleaseManagedResourcesOnNativeCrash.Invoke();
            }
            LaunchCrashReporterForDumpProcess(pid, errorCode, errorStack, dumpType, true);
        }
        static CallbackEventLaunchCrashReporterForDumpProcess _CallbackEventLaunchCrashReporterForDumpProcess;
        public static bool StartCrashDumperOnce()
        {
            _CallbackEventLaunchCrashReporterForDumpProcess = new CallbackEventLaunchCrashReporterForDumpProcess(LaunchCrashReporterForDumpProcess_Native);
            return EnableCrashDumperOnce(_CallbackEventLaunchCrashReporterForDumpProcess);
        }

        public static DumpConfig GetDumpConfig()
        {
            var onlyDump = false;
            string onlyDumpString = Environment.GetEnvironmentVariable("LaunchCrashReporterForDumpProcess_OnlyDump");
            if (!string.IsNullOrEmpty(onlyDumpString))
            {
                onlyDump = Convert.ToBoolean(onlyDumpString);
            }

            DumpType dumpType = DumpType.withheap;
            string strDumpType = Environment.GetEnvironmentVariable("LaunchCrashReporterForDumpProcess_DumpType");
            if (!string.IsNullOrEmpty(strDumpType))
            {
                dumpType = Enum.Parse<DumpType>(strDumpType); ;
            }


            return new DumpConfig()
            {
                DumpType = dumpType,
                OnlyDump = onlyDump,
                StartCommandLine = Environment.GetEnvironmentVariable("LaunchCrashReporterForDumpProcess_StartCommandLine"),
            };
        }

        public class DumpConfig
        {
            public bool OnlyDump { get; set; }

            public string StartCommandLine { get; set; }

            public DumpType DumpType { get; set; }
        }
    }
}
