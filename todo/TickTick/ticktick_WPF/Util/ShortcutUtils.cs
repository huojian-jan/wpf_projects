// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ShortcutUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using IWshRuntimeLibrary;
using Microsoft.CSharp.RuntimeBinder;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ticktick_WPF.Resource;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ShortcutUtils
  {
    public static bool CreatShortCut(string path = "", string arg = "")
    {
      try
      {
        if (string.IsNullOrWhiteSpace(path))
          path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string location = Assembly.GetExecutingAssembly().Location;
        ShortcutUtils.DeleteShutCut(path, "滴答清单");
        ShortcutUtils.DeleteShutCut(path, "TickTick");
        string str = Utils.IsDida() ? "滴答清单" : "TickTick";
        // ISSUE: variable of a compiler-generated type
        WshShell instance = (WshShell) Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
        // ISSUE: reference to a compiler-generated field
        if (ShortcutUtils.\u003C\u003Eo__0.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          ShortcutUtils.\u003C\u003Eo__0.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, IWshShortcut>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof (IWshShortcut), typeof (ShortcutUtils)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated method
        // ISSUE: variable of a compiler-generated type
        IWshShortcut wshShortcut = ShortcutUtils.\u003C\u003Eo__0.\u003C\u003Ep__0.Target((CallSite) ShortcutUtils.\u003C\u003Eo__0.\u003C\u003Ep__0, instance.CreateShortcut(path + "\\" + str + ".lnk"));
        wshShortcut.TargetPath = location;
        wshShortcut.WorkingDirectory = baseDirectory;
        wshShortcut.WindowStyle = 1;
        wshShortcut.Description = str;
        wshShortcut.IconLocation = AppPaths.AppIconDir + (string.IsNullOrEmpty(LocalSettings.Settings.Common.AppIconKey) ? AppIconKey.Default.ToString() : LocalSettings.Settings.Common.AppIconKey) + ".ico";
        wshShortcut.Arguments = arg;
        // ISSUE: reference to a compiler-generated method
        wshShortcut.Save();
        return true;
      }
      catch (Exception ex)
      {
        UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
        return false;
      }
    }

    public static void DeleteShutCut(string path = "", string name = "")
    {
      try
      {
        if (string.IsNullOrWhiteSpace(path))
          path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        if (!File.Exists(path + "\\" + name + ".lnk"))
          return;
        File.Delete(path + "\\" + name + ".lnk");
      }
      catch (Exception ex)
      {
      }
    }

    public static string GetShutcutArgs(string path = "")
    {
      try
      {
        if (string.IsNullOrWhiteSpace(path))
          path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        string str = "滴答清单";
        if (!File.Exists(path + "\\" + str + ".lnk"))
        {
          str = "TickTick";
          if (!File.Exists(path + "\\" + str + ".lnk"))
            return "";
        }
        // ISSUE: variable of a compiler-generated type
        WshShell instance = (WshShell) Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8")));
        // ISSUE: reference to a compiler-generated field
        if (ShortcutUtils.\u003C\u003Eo__2.\u003C\u003Ep__0 == null)
        {
          // ISSUE: reference to a compiler-generated field
          ShortcutUtils.\u003C\u003Eo__2.\u003C\u003Ep__0 = CallSite<Func<CallSite, object, IWshShortcut>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.Convert(CSharpBinderFlags.ConvertExplicit, typeof (IWshShortcut), typeof (ShortcutUtils)));
        }
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated method
        // ISSUE: variable of a compiler-generated type
        IWshShortcut wshShortcut = ShortcutUtils.\u003C\u003Eo__2.\u003C\u003Ep__0.Target((CallSite) ShortcutUtils.\u003C\u003Eo__2.\u003C\u003Ep__0, instance.CreateShortcut(path + "\\" + str + ".lnk"));
        return wshShortcut != null ? wshShortcut.Arguments : "";
      }
      catch (Exception ex)
      {
        return "";
      }
    }

    public static bool IsShortcutExit(string path = "")
    {
      if (string.IsNullOrWhiteSpace(path))
        path = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
      string str1 = "滴答清单";
      if (!File.Exists(path + "\\" + str1 + ".lnk"))
      {
        string str2 = "TickTick";
        if (!File.Exists(path + "\\" + str2 + ".lnk"))
          return false;
      }
      return true;
    }
  }
}
