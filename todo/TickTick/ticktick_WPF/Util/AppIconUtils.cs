// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.AppIconUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using IWshRuntimeLibrary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ticktick_WPF.Resource;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public class AppIconUtils
  {
    private const int MAX_PATH = 260;
    private const int CSIDL_COMMON_DESKTOPDIRECTORY = 25;

    public static async Task SetAppIcons(string key, bool checkDefault = false)
    {
      await Task.Yield();
      AppIconUtils.TryDeleteCommonProgramsLink();
      if (checkDefault && !string.IsNullOrEmpty(LocalSettings.Settings.Common.AppIconKey) && LocalSettings.Settings.Common.AppIconKey == AppIconKey.Default.ToString())
        ;
      else
      {
        if (string.IsNullOrEmpty(key))
          key = AppIconKey.Default.ToString();
        App.Instance.SetNotifyIcon(false);
        Task.Run((Action) (() => Utils.LogActionTimes((Action) (() => AppIconUtils.SetLinks(key)), 2000, (string) null, "SetLinks")));
        SystemToastUtils.SetLogoUrl();
      }
    }

    public static void SetLinks(string key)
    {
      string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
      HashSet<string> checkedDir1 = new HashSet<string>();
      HashSet<string> checkedDir2 = checkedDir1;
      string key1 = key;
      AppIconUtils.SetLnkIcons(folderPath, checkedDir2, key1);
      if (App.IsAdmin)
      {
        AppIconUtils.SetLnkIcons(AppIconUtils.GetAllUsersDesktopFolderPath(), checkedDir1, key);
        AppIconUtils.SetLnkIcons(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), checkedDir1, key);
      }
      AppIconUtils.SetLnkIcons(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), checkedDir1, key, true);
      AppIconUtils.SetLnkIcons(Environment.GetFolderPath(Environment.SpecialFolder.Startup), checkedDir1, key);
    }

    private static int SetLnkIcons(
      string foldPath,
      HashSet<string> checkedDir,
      string key,
      bool ignoreChildFold = false,
      int limit = 200)
    {
      try
      {
        if (Directory.Exists(foldPath))
        {
          if (!checkedDir.Contains(foldPath))
          {
            checkedDir.Add(foldPath);
            string[] files = Directory.GetFiles(foldPath, "*.lnk");
            if (files.Length != 0)
            {
              foreach (string PathLink in files)
              {
                if (limit < 0)
                  return limit;
                // ISSUE: variable of a compiler-generated type
                WshShell instance = (WshShell) Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
                // ISSUE: reference to a compiler-generated field
                if (AppIconUtils.\u003C\u003Eo__2.\u003C\u003Ep__0 == null)
                {
                  // ISSUE: reference to a compiler-generated field
                  AppIconUtils.\u003C\u003Eo__2.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, IWshShortcut>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof (IWshShortcut), typeof (AppIconUtils)));
                }
                // ISSUE: reference to a compiler-generated field
                // ISSUE: reference to a compiler-generated field
                // ISSUE: reference to a compiler-generated method
                // ISSUE: variable of a compiler-generated type
                IWshShortcut wshShortcut = AppIconUtils.\u003C\u003Eo__2.\u003C\u003Ep__0.Target((CallSite) AppIconUtils.\u003C\u003Eo__2.\u003C\u003Ep__0, instance.CreateShortcut(PathLink));
                if (wshShortcut.TargetPath.Contains(AppPaths.ExeDir))
                {
                  string str = AppPaths.AppIconDir + key + ".ico";
                  if (!wshShortcut.IconLocation.Contains(str))
                    wshShortcut.IconLocation = str;
                  // ISSUE: reference to a compiler-generated method
                  wshShortcut.Save();
                }
                --limit;
              }
            }
            if (!ignoreChildFold)
            {
              string[] directories = Directory.GetDirectories(foldPath);
              if (directories.Length != 0)
              {
                foreach (string foldPath1 in directories)
                {
                  if (limit < 0)
                    return limit;
                  limit = AppIconUtils.SetLnkIcons(foldPath1, checkedDir, key, true, limit);
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        UtilLog.Info(nameof (SetLnkIcons) + foldPath);
      }
      return limit;
    }

    private static void TryDeleteCommonProgramsLink()
    {
      try
      {
        if (string.IsNullOrEmpty(AppIconUtils.GetLnkPath(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu))))
          return;
        string lnkPath = AppIconUtils.GetLnkPath(Environment.GetFolderPath(Environment.SpecialFolder.CommonPrograms), true);
        if (!File.Exists(lnkPath))
          return;
        File.Delete(lnkPath);
      }
      catch (Exception ex)
      {
        UtilLog.Info("TryDeleteCommonProgramsLink   " + ex.Message);
      }
    }

    private static string GetLnkPath(string foldPath, bool checkChild = false)
    {
      if (Directory.Exists(foldPath))
      {
        string[] files1 = Directory.GetFiles(foldPath, "*.lnk");
        if (files1.Length != 0)
        {
          foreach (string PathLink in files1)
          {
            // ISSUE: variable of a compiler-generated type
            WshShell instance = (WshShell) Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
            // ISSUE: reference to a compiler-generated field
            if (AppIconUtils.\u003C\u003Eo__4.\u003C\u003Ep__0 == null)
            {
              // ISSUE: reference to a compiler-generated field
              AppIconUtils.\u003C\u003Eo__4.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, IWshShortcut>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof (IWshShortcut), typeof (AppIconUtils)));
            }
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated method
            if (AppIconUtils.\u003C\u003Eo__4.\u003C\u003Ep__0.Target((CallSite) AppIconUtils.\u003C\u003Eo__4.\u003C\u003Ep__0, instance.CreateShortcut(PathLink)).TargetPath.Contains(AppPaths.ExeDir))
              return PathLink;
          }
        }
        if (checkChild)
        {
          string[] directories = Directory.GetDirectories(foldPath);
          if (directories.Length != 0)
          {
            foreach (string path in directories)
            {
              string str = "滴答清单";
              if (path.Contains(str))
              {
                string[] files2 = Directory.GetFiles(path, "*.lnk");
                if (files2.Length != 0)
                {
                  foreach (string PathLink in files2)
                  {
                    // ISSUE: variable of a compiler-generated type
                    WshShell instance = (WshShell) Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
                    // ISSUE: reference to a compiler-generated field
                    if (AppIconUtils.\u003C\u003Eo__4.\u003C\u003Ep__1 == null)
                    {
                      // ISSUE: reference to a compiler-generated field
                      AppIconUtils.\u003C\u003Eo__4.\u003C\u003Ep__1 = CallSite<Func<CallSite, object, IWshShortcut>>.Create(Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof (IWshShortcut), typeof (AppIconUtils)));
                    }
                    // ISSUE: reference to a compiler-generated field
                    // ISSUE: reference to a compiler-generated field
                    // ISSUE: reference to a compiler-generated method
                    if (AppIconUtils.\u003C\u003Eo__4.\u003C\u003Ep__1.Target((CallSite) AppIconUtils.\u003C\u003Eo__4.\u003C\u003Ep__1, instance.CreateShortcut(PathLink)).TargetPath.Contains(AppPaths.ExeDir))
                      return PathLink;
                  }
                }
              }
            }
          }
        }
      }
      return (string) null;
    }

    [DllImport("shfolder.dll", CharSet = CharSet.Auto)]
    private static extern int SHGetFolderPath(
      IntPtr hwndOwner,
      int nFolder,
      IntPtr hToken,
      int dwFlags,
      StringBuilder lpszPath);

    public static string GetAllUsersDesktopFolderPath()
    {
      StringBuilder lpszPath = new StringBuilder(260);
      AppIconUtils.SHGetFolderPath(IntPtr.Zero, 25, IntPtr.Zero, 0, lpszPath);
      return lpszPath.ToString();
    }

    public static void TrySetToastIcon()
    {
      string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
      string appIconKey = LocalSettings.Settings.Common.AppIconKey;
      if (string.IsNullOrEmpty(appIconKey))
        appIconKey = AppIconKey.Default.ToString();
      HashSet<string> checkedDir = new HashSet<string>();
      string key = appIconKey;
      AppIconUtils.SetLnkIcons(folderPath, checkedDir, key, true);
    }

    public static BitmapImage GetIconImage()
    {
      AppIconKey result;
      Enum.TryParse<AppIconKey>(LocalSettings.Settings.Common.AppIconKey, out result);
      return result == AppIconKey.Default ? new BitmapImage(new Uri("pack://application:,,,/Resources/Default.ico")) : new BitmapImage(new Uri("pack://application:,,,/Resources/" + result.ToString() + ".ico"));
    }
  }
}
