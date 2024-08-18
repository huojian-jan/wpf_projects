using Huojian.LibraryManagement.Common;
using Microsoft.VisualBasic;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.RemoteDesktop;
using Windows.Win32.UI.Shell;


namespace ShadowBot.Common.Utilities
{
    public static class ChildSessionHelper
    {
        #region func
        // 是否启用
        public static bool IsChildSessionsEnabled()
        {
            if (IsSupportedOsVersion())
            {
                PInvoke.WTSIsChildSessionsEnabled(out BOOL isEnable);
                return isEnable;
            }

            return false;
        }

        // 启用
        public static void EnableChildSessions()
        {
            if (IsSupportedOsVersion())
            {
                if (!PInvoke.WTSEnableChildSessions(true))
                {
                    throw new InvalidOperationException($"xx");
                }
            }

            SuppressWhenMinimized();
        }

        public static void SuppressWhenMinimized()
        {
            try
            {
                // 最小化运行：https://support.smartbear.com/testcomplete/docs/testing-with/running/via-rdp/in-minimized-window.html
                using (var cuKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Terminal Server Client", true))
                {
                    cuKey.SetValue("RemoteDesktop_SuppressWhenMinimized", 2);
                }
            }
            catch (Exception e)
            {
                Logging.Warn($"The current virtual desktop cannot be minimized： {e.Message}");
            }
        }

        // 禁用
        public static void DisableChildSession()
        {
            if (IsSupportedOsVersion())
            {
                if (!PInvoke.WTSEnableChildSessions(false))
                {
                    throw new InvalidOperationException("Failed to （disable） child session. Beginning with Windows Server 2012 and Windows 8");
                }
            }
        }

        // 断开连接
        // https://docs.microsoft.com/en-us/windows/win32/api/wtsapi32/nf-wtsapi32-wtsdisconnectsession
        // Disconnects the logged-on user from the specified Remote Desktop Services session without closing the session.
        public static void DisconnectChildSession()
        {
            if (IsSupportedOsVersion())
            {
                if (PInvoke.WTSGetChildSessionId(out UInt32 sessionId)
                && !PInvoke.WTSDisconnectSession(default(HANDLE), sessionId, true))
                {
                    throw new Exception($"Failed to disconnect child session {sessionId}: {Marshal.GetLastWin32Error()}");
                }
            }
        }

        // https://docs.microsoft.com/en-us/windows/win32/api/wtsapi32/nf-wtsapi32-wtsconnectsessiona
        // todo: 子会话复用研究  看WTSDisconnectSession文档，应该是可以复用的
        public static void ConnectChildSession()
        {
            //PInvoke.WTSConnectSession()
        }

        // 注销
        // https://docs.microsoft.com/en-us/windows/win32/api/wtsapi32/nf-wtsapi32-wtslogoffsession
        // 注销子会话方式：The child session can be terminated by logging off directly from it, or it will be terminated when its parent session terminates.
        public static void LogOffChildSession()
        {
            if (IsSupportedOsVersion())
            {
                if (PInvoke.WTSGetChildSessionId(out UInt32 sessionId))
                    PInvoke.WTSLogoffSession(default(HANDLE), sessionId, true);
            }
        }

        /// <summary>
        /// 获取子会话 ID, Windows API不支持的系统时返回当前进程 SessionId
        /// </summary>
        /// <returns>失败返回 -1</returns>
        public static Int32 GetChildSessionId()
        {
            // Windows 8、Windows Server 2012以上才可以正常调用 WTSGetChildSessionId
            if (IsSupportedOsVersion())
            {
                if (PInvoke.WTSGetChildSessionId(out UInt32 sessionId))
                {
                    Logging.Debug($"invoke api sucess: {sessionId}");
                    return (Int32)sessionId;
                }
                else
                {
                    // 当前会话无子会话时会获取失败（1168：找不到元素），此时应该获取当前 SessionId (main session Id)
                    Logging.Debug($"get child sessionID error code: {Marshal.GetLastWin32Error()}");
                    if (Marshal.GetLastWin32Error() == 1168)
                    {
                        Logging.Debug($"get current: {ProcessHelper.GetCurrentProcessSessionId()}");
                        return ProcessHelper.GetCurrentProcessSessionId();
                    }
                }
            }

            // Windows Api 调用失败时，通过 WTSEnumerateSessions 获取
            var sessionIdSet = GetCurrentUserSessionIdSet(); // 当前用户下的所有 sessionId
            if (sessionIdSet.Count > 0)
            {
                var activeSessionId = sessionIdSet.First();
                Logging.Debug($"activeSessionId: {activeSessionId}");

                sessionIdSet.Remove(activeSessionId);

                if (sessionIdSet.Count > 0)
                {
                    Logging.Debug($"result {sessionIdSet.First()}");
                    return sessionIdSet.First();
                }
            }

            // 失败返回 -1
            Logging.Debug("final return -1");
            return -1;
        }

        /// <summary>
        /// 获取当前 WTS_CONNECTSTATE_CLASS.WTSActive 的 SessionId
        /// </summary>
        /// <returns>获取失败时返回 -1</returns>
        public static Int32 GetActiveSessionId()
        {
            HashSet<int> activeSessionIdSet = GetCurrentUserSessionIdSet();

            if (activeSessionIdSet.Count > 0)
                return activeSessionIdSet.First();
            else
                return -1;
        }

        /// <summary>
        /// 获取当前用户下的 SessionId 哈希Set
        /// </summary>
        /// <returns></returns>
        public unsafe static HashSet<int> GetCurrentUserSessionIdSet()
        {
            HashSet<int> userSessionIdSet = new HashSet<int>();
            WTS_SESSION_INFOW* pSessionInfo;
            uint sessionCount = 0;

            if (PInvoke.WTSEnumerateSessions(default(SafeHandle), 0, 1, out pSessionInfo, out sessionCount) != false)
            {
                var arrayElementSize = Marshal.SizeOf(typeof(WTS_SESSION_INFOW));
                IntPtr current = (IntPtr)pSessionInfo;

                for (var i = 0; i < sessionCount; i++)
                {
                    var si = (WTS_SESSION_INFOW)Marshal.PtrToStructure(current, typeof(WTS_SESSION_INFOW));
                    current += arrayElementSize;

                    if (si.State == WTS_CONNECTSTATE_CLASS.WTSActive)
                    {
                        var sessionId = si.SessionId;
                        if (GetUsernameBySessionId((int)sessionId) == Environment.UserName)
                            userSessionIdSet.Add((int)si.SessionId);
                    }
                }
            }

            Logging.Debug($"--- sesssion set --- ");
            foreach (var item in userSessionIdSet)
            {
                Logging.Debug(item);
            }
            Logging.Debug("---");

            return userSessionIdSet;
        }

        /// <summary>
        /// 根据 SessionId 获取用户名
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="prependDomain"></param>
        /// <returns></returns>
        public unsafe static string GetUsernameBySessionId(int sessionId, bool prependDomain = false)
        {
            PWSTR buffer;
            uint bufferLength;
            string username = "SYSTEM";
            if (PInvoke.WTSQuerySessionInformation(default(SafeHandle), (uint)sessionId, WTS_INFO_CLASS.WTSUserName, out buffer, out bufferLength) && bufferLength > 1)
            {
                username = buffer.ToString();
                PInvoke.WTSFreeMemory(buffer);

                if (prependDomain)
                {
                    if (PInvoke.WTSQuerySessionInformation(default(SafeHandle), (uint)sessionId, WTS_INFO_CLASS.WTSDomainName, out buffer, out bufferLength) && bufferLength > 1)
                    {
                        username = buffer.ToString() + "\\" + username;
                        PInvoke.WTSFreeMemory(buffer);
                    }
                }
            }
            else
                Logging.Warn($"Failed to get username：{Marshal.GetLastWin32Error()} - {sessionId}");

            Logging.Debug($"user name: {username}");
            return username;
        }

        public static bool IsSupportedOsVersion()
        {
            // https://stackoverflow.com/questions/2819934/detect-windows-version-in-net
            // https://stackoverflow.com/questions/13620223/how-to-detect-windows-8-operating-system-using-c-sharp-4-0/17796139
            // https://stackoverflow.com/questions/49597836/check-if-windows-server-above-2012
            var curPlatForm = Environment.OSVersion.Platform;
            var curOsVersion = Environment.OSVersion.Version;
            var isWin8OrLater = curPlatForm == PlatformID.Win32NT && curOsVersion >= new Version(6, 2, 9200, 0);
            if (isWin8OrLater)
                return true;

            var isWinServer2012OrLater = PInvoke.IsOS(OS.OS_ANYSERVER) &&
                (curOsVersion.Major > 6 || (curOsVersion.Major == 6 && curOsVersion.Minor >= 2));
            if (isWinServer2012OrLater)
                return true;

            return false;
        }
        #endregion

        #region 断开后复用 注释不删除

        //public static bool ConnectToChildSessionIfExit()
        //{
        //    bool result = false;
        //    IntPtr buffer = IntPtr.Zero;
        //    int count = 0;

        //    int activeSessId = -1;
        //    int targetSessId = -1;
        //    if (PInvoke.WTSEnumerateSessions(IntPtr.Zero, 0, 1, ref buffer, ref count))
        //    {
        //        WTS_SESSION_INFO[] sessionInfo = new WTS_SESSION_INFO[count];

        //        for (int index = 0; index < count; index++)
        //            sessionInfo[index] = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)((int)buffer +
        //            (Marshal.SizeOf(new WTS_SESSION_INFO()) * index)), typeof(WTS_SESSION_INFO));


        //        for (int i = 1; i < count; i++)
        //        {
        //            if (sessionInfo[i].State == WTS_CONNECTSTATE_CLASS.WTSDisconnected)
        //                targetSessId = sessionInfo[i].SessionId;
        //            else if (sessionInfo[i].State == WTS_CONNECTSTATE_CLASS.WTSActive)
        //                activeSessId = sessionInfo[i].SessionId;
        //        }
        //    }

        //    if ((activeSessId > 0) && (targetSessId > 0))
        //    {
        //        result = PInvoke.WTSConnectSession(Convert.ToUInt64(targetSessId), Convert.ToUInt64(activeSessId), "", false);
        //    }
        //    return result;
        //}
        #endregion
    }
}
