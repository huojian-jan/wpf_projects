using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Security;
using Windows.Win32.System.Threading;
using Windows.Win32.System.Diagnostics.ToolHelp;
using System.ComponentModel;
using System.Globalization;
using System.Threading;

namespace ShadowBot.Common.Utilities
{
    /// <summary>
    /// 提供进程管理功能
    /// </summary>
    public static class ProcessHelper
    {
        public unsafe static Process CreateProcessWithoutJob(string fileName, string arguments = null, string workingDirectory = null)
        {
            if (!File.Exists(fileName))
            {
                throw new ArgumentException("File does not exist");
            }
            PROCESS_INFORMATION proc = new PROCESS_INFORMATION();
            try
            {
                var inf = new STARTUPINFOW
                {
                    cb = (uint)Marshal.SizeOf(typeof(STARTUPINFOW))
                };
                var cmdline = fileName;
                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    cmdline += " " + arguments;
                }
                fixed (char* pcmdline = cmdline)
                {
                    fixed (char* lpCurrentDirectoryLocal = workingDirectory)
                    {
                        PWSTR pcmdlineW = new PWSTR(pcmdline);
                        if (!PInvoke.CreateProcess(null, pcmdlineW, null, null, false,
                            PROCESS_CREATION_FLAGS.CREATE_BREAKAWAY_FROM_JOB | PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW | PROCESS_CREATION_FLAGS.CREATE_NEW_CONSOLE,
                            null, lpCurrentDirectoryLocal, &inf, &proc))
                        {
                            // 子会话中，CreateProcessFlags.CREATE_BREAKAWAY_FROM_JOB会导致创建进程失败
                            if ((WIN32_ERROR)Marshal.GetLastWin32Error() == WIN32_ERROR.ERROR_ACCESS_DENIED)
                            {
                                Logging.Debug("createProcess ERROR_ACCESS_DENIED");
                                if (!PInvoke.CreateProcess(null, pcmdlineW, null, null, false,
                                 PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW | PROCESS_CREATION_FLAGS.CREATE_NEW_CONSOLE,
                                 null, lpCurrentDirectoryLocal, &inf, &proc))
                                {
                                    throw new InvalidOperationException("Couldn't create process when access denied");
                                }
                            }
                            else
                                throw new InvalidOperationException("Couldn't create process");
                        }
                    }
                }

                return Process.GetProcessById((int)proc.dwProcessId);
            }
            finally
            {
                if (proc.hProcess != IntPtr.Zero)
                {
                    PInvoke.CloseHandle(proc.hProcess);
                }
            }
        }

        public static int GetCurrentProcessId()
        {
            using (var process = Process.GetCurrentProcess())
                return process.Id;
        }

        public static int GetCurrentProcessSessionId()
        {
            using (var process = Process.GetCurrentProcess())
                return process.SessionId;
        }

        public static void Kill(int processId)
        {
            Process process = null;
            try
            {
                process = Process.GetProcessById(processId);
                process.Kill();
            }
            catch
            { }
            finally
            {
                process?.Dispose();
            }
        }

        /// <summary>
        /// 获取当前第一个匹配指定进程名的进程, 默认根据当前 SessionId筛选
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="filterByUserName">根据当前登录用户筛选</param>
        /// <returns></returns>
        public static Process GetProcessByName(string processName, bool filterByUserName = false)
        {
            #region 注释不删除  原获取进程方式
            /* 1. 根据进程名称获取进程的时候，如果是WindowsServer需要区分进程所有者
             * 2. WMI.GetOwner在某些机器上会抛出“不支持”的异常，如果不是WindowsServer的机器就直接使用Process.Get即可
             */
            //    string query = "Select * from Win32_Process Where Name = \"" + processName + "\"";
            //    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            //    {
            //        ManagementObjectCollection processList = searcher.Get();
            //        try
            //        {
            //            foreach (ManagementObject obj in processList)
            //            {
            //                string[] argList = new string[] { string.Empty, string.Empty };
            //                int returnVal = -1;
            //                try
            //                {
            //                    returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
            //                }
            //                catch (ManagementException ex)
            //                {
            //                    // 批量关闭网页时，这里会报"找不到"的异常，忽略它 FIX#ISSUE-234
            //                    Logging.Warn(ex);
            //                }
            //                if (returnVal == 0)
            //                {
            //                    if (Environment.UserName == argList[0] && Environment.UserDomainName == argList[1])
            //                    {
            //                        var processProperties = obj.Properties;
            //                        var pid = int.Parse(processProperties["ProcessID"].Value.ToString());
            //                        processes.Add(Process.GetProcessById(pid));
            //                    }
            //                }
            //            }
            //        }
            //        finally
            //        {
            //            // bugfix 25: 报错Provider load failure，但是无法重现
            //            processList?.Dispose();
            //            //if (processList != null)
            //            //    foreach (var process in processList)
            //            //        process.Dispose();
            //        }
            //    }
            //    return processes.ToArray();
            #endregion

            // 过滤掉.exe的后缀名
            if (processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                processName = processName.Substring(0, processName.Length - 4);


            var processArray = Process.GetProcessesByName(processName);

            if (filterByUserName) // 使用当前用户名
            {
                return processArray.FirstOrDefault(p => ChildSessionHelper.GetUsernameBySessionId(p.SessionId) == Environment.UserName);
            }
            else // 使用当前进程 SessionId 过滤
            {
                var curSessionId = GetCurrentProcessSessionId();
                return processArray.FirstOrDefault(p => p.SessionId == curSessionId);
            }
        }


        /// <summary>
        /// 获取当前所有匹配指定进程名的所有进程, 默认根据当前 SessionId筛选
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="filterByUserName">根据当前登录用户筛选</param>
        /// <returns></returns>
        public static Process[] GetAllProcessesByName(string processName, bool filterByUserName = false)
        {
            // 过滤掉.exe的后缀名
            if (processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                processName = processName.Substring(0, processName.Length - 4);

            var processArray = Process.GetProcessesByName(processName);

            if (filterByUserName) // 使用当前用户名
            {
                var result = processArray.Where(p => ChildSessionHelper.GetUsernameBySessionId(p.SessionId) == Environment.UserName);
                return result.ToArray();
            }
            else // 使用当前进程 SessionId 过滤
            {
                var curSessionId = GetCurrentProcessSessionId();
                var result = processArray.Where(p => p.SessionId == curSessionId);
                return result.ToArray();
            }
        }

        /// <summary>
        /// 根据进程名，检查是否存在当前用户名下，不同的 SessionId (主会话、子会话)中是否已存在运行的进程实例
        /// </summary>
        /// <returns></returns>
        public static bool IsProcessOccupied(string processName)
        {
            // 过滤掉.exe的后缀名
            if (processName.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                processName = processName.Substring(0, processName.Length - 4);

            // 使用当前用户 SessionId过滤
            var processArray = Process.GetProcessesByName(processName);

            // 判断进程是否存在相同用户，不同 SessionId 下的占用情况
            var currentSessionId = GetCurrentProcessSessionId();
            Logging.Debug($"Occupied currentSessionId: {currentSessionId}   userName: {Environment.UserName}");

            foreach (var process in processArray)
            {
                var sessionId = process.SessionId;

                if (sessionId != currentSessionId
                    && ChildSessionHelper.GetUsernameBySessionId(sessionId) == Environment.UserName)
                {
                    Logging.Debug("Occupied Reutrn True");
                    return true;
                }
            }

            return false;
        }

        public static void SettingThreadCulture(string culture, bool setEnvironmentVariable = false)
        {
            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo(culture.Replace("_", "-"));
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
                if (setEnvironmentVariable)
                {
                    Environment.SetEnvironmentVariable("SHADOWBOT_CULTURE", culture.Replace("_", "-"));
                }
            }
            catch (Exception ex)
            {
                Logging.Warn(ex);
            }
        }

        const string ShadowBotNativeHelperDllPath = "ShadowBot.NativeHelper.dll";
        public enum PH_PEB_OFFSET
        {
            PhpoCurrentDirectory,
            PhpoDllPath,
            PhpoImagePathName,
            PhpoCommandLine,
            PhpoWindowTitle,
            PhpoDesktopInfo,
            PhpoShellInfo,
            PhpoRuntimeData,
            PhpoTypeMask = 0xffff,
            PhpoWow64 = 0x10000,
            Phpo64From32 = 0x20000
        };
        [DllImport(ShadowBotNativeHelperDllPath, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        extern static bool GetProcessPebString(IntPtr ProcessHandle, PH_PEB_OFFSET Offset, [MarshalAs(UnmanagedType.BStr)] out string pbstrResult);
        public static unsafe string? CallHelper__GetProcessPebString(uint pid, PH_PEB_OFFSET Offset)
        {
            BOOL bCurProcessIsWow64;
            PInvoke.IsWow64Process(PInvoke.GetCurrentProcess(), &bCurProcessIsWow64);
            string strResult = null;
            var ProcessHandle = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_INFORMATION | PROCESS_ACCESS_RIGHTS.PROCESS_VM_READ, false, pid);
            if (ProcessHandle != IntPtr.Zero)
            {
                GetProcessPebString(ProcessHandle, (PH_PEB_OFFSET)(Offset | (bCurProcessIsWow64 ? PH_PEB_OFFSET.Phpo64From32 : 0)), out strResult);
                //Debug.Print("{0}[{1}] ImagePathName={2}", process.ProcessName, process.Id, strResult);
                PInvoke.CloseHandle(ProcessHandle);
            }
            return strResult;
        }

        public static unsafe string? GetProcessFileName(Process process, bool fullFileName)
        {
            string strResult = CallHelper__GetProcessPebString((uint)process.Id, PH_PEB_OFFSET.PhpoImagePathName);
            if (strResult != null)
            {
                return fullFileName ? strResult : Path.GetFileName(strResult);
            }
            else
            {
                return null;
            }
        }
        public static string? GetProcessCommandLine(Process process)
        {
            string strResult = CallHelper__GetProcessPebString((uint)process.Id, PH_PEB_OFFSET.PhpoCommandLine);
            return strResult;
        }

        internal static bool EnumProcesses(Func<PROCESSENTRY32, bool> predicate)
        {
            try
            {
                // Get a list of all processes
                var snapshotHandle = PInvoke.CreateToolhelp32Snapshot_SafeHandle(CREATE_TOOLHELP_SNAPSHOT_FLAGS.TH32CS_SNAPPROCESS, 0);
                if (snapshotHandle.IsInvalid)
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                PROCESSENTRY32 procEntry = new PROCESSENTRY32();
                procEntry.dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32));
                if (PInvoke.Process32First(snapshotHandle, ref procEntry))
                {
                    do
                    {
                        if (predicate(procEntry) == false)
                        {
                            return true;
                        }
                    }
                    while (PInvoke.Process32Next(snapshotHandle, ref procEntry));
                }
                else
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }
            }
            catch (Exception e)
            {
                Debug.Write(e);
            }

            return false;
        }

        public static int GetParentProcess(int processId)
        {
            int parent_process = 0;
            EnumProcesses(p =>
            {
                if (p.th32ProcessID == processId)
                {
                    parent_process = (int)p.th32ParentProcessID;
                    return false;
                }
                else
                {
                    return true;
                }
            });

            return parent_process;
        }

        public static void OpenWebBrowser(string url)
        {
            try
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
                }
                catch (Win32Exception)
                {
                    Process.Start("explorer.exe", url);
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Open browser Fail", ex);
            }
        }

        public static void RunCommandLine(string commandLine)
        {
            using (var process = new Process())
            {
                var startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = $"/C {commandLine}";
                process.StartInfo = startInfo;
                process.Start();
            }
        }

        public static void KillProcessAndChildren(int pid)
        {
            var processCollection = GetChildProcesses(pid);
            try
            {
                using (var proc = Process.GetProcessById(pid))
                {
                    if (!proc.HasExited)
                        proc.Kill();
                }
            }
            catch (Exception ex)
            {
                Logging.Warn($"kill process:{pid} error", ex);
            }

            if (processCollection != null)
            {
                foreach (var mo in processCollection)
                {
                    KillProcessAndChildren(mo); //kill child processes(also kills childrens of childrens etc.)
                }
            }

        }

        //来自notepad2的https://github.com/zufuliu/notepad2/blob/main/src/Helpers.c
        public unsafe static bool IsProcessElevated(int pid)
        {
            bool bIsElevated = false;

            var processHandle = PInvoke.OpenProcess(PROCESS_ACCESS_RIGHTS.PROCESS_QUERY_LIMITED_INFORMATION, false, (uint)pid);
            if (!processHandle.IsNull)
            {
                HANDLE hToken = default(HANDLE);

                if (PInvoke.OpenProcessToken(processHandle, TOKEN_ACCESS_MASK.TOKEN_QUERY, &hToken))
                {
                    TOKEN_ELEVATION te;
                    uint dwReturnLength = 0;

                    uint cb = (uint)System.Runtime.InteropServices.Marshal.SizeOf(typeof(TOKEN_ELEVATION));
                    if (PInvoke.GetTokenInformation(hToken, TOKEN_INFORMATION_CLASS.TokenElevation, &te, cb, &dwReturnLength))
                    {
                        if (dwReturnLength == cb)
                        {
                            bIsElevated = te.TokenIsElevated != 0;
                        }
                    }
                    PInvoke.CloseHandle(hToken);
                }

                PInvoke.CloseHandle(processHandle);
            }
            return bIsElevated;
        }

        public static int[] GetChildProcesses(int pid, Func<int, bool> predicate = null)
        {
            if (predicate == null)
                predicate = m => true;

            var pids = new List<int>();

            EnumProcesses(p =>
            {
                if (p.th32ParentProcessID == pid)
                {
                    if (predicate((int)p.th32ProcessID))
                        pids.Add((int)p.th32ProcessID);
                }

                return true;
            });
            return pids.ToArray();
        }

        public static string GetProductName(int processId)
        {
            var process = Process.GetProcessById(processId);
            var path = GetProcessFileName(process, true);
            FileVersionInfo processInfo = FileVersionInfo.GetVersionInfo(path);
            return processInfo.ProductName ?? "";
        }
    }
}
