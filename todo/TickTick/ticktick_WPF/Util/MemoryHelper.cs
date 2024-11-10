// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.MemoryHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class MemoryHelper
  {
    private static Timer _timer = new Timer();

    [DllImport("kernel32.dll")]
    private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

    public static void FlushMemory()
    {
      try
      {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
          return;
        MemoryHelper.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, 52428800, 125829120);
      }
      catch (Exception ex)
      {
        UtilLog.Warn("FlushMemory Error :" + ExceptionUtils.BuildExceptionMessage(ex));
      }
    }

    public static void Cracker(int sleepSpan = 3600)
    {
      MemoryHelper._timer.Interval = (double) (sleepSpan * 1000);
      MemoryHelper._timer.Elapsed += (ElapsedEventHandler) ((o, e) =>
      {
        UtilLog.Info("MemoryHelper Begin FlushMemory");
        MemoryHelper.FlushMemory();
      });
    }

    public static string GetCurrentSize()
    {
      try
      {
        using (Process currentProcess = Process.GetCurrentProcess())
        {
          int guiResources1 = MemoryHelper.GetGuiResources(currentProcess.Handle, 0);
          int guiResources2 = MemoryHelper.GetGuiResources(currentProcess.Handle, 2);
          int guiResources3 = MemoryHelper.GetGuiResources(currentProcess.Handle, 1);
          int guiResources4 = MemoryHelper.GetGuiResources(currentProcess.Handle, 4);
          return string.Format("current:{0},available:{1},gui:{2}/{3},userObject:{4}/{5},handleCount:{6}", (object) MemoryUtils.FormatSize((double) currentProcess.PrivateMemorySize64), (object) MemoryUtils.FormatSize((double) MemoryUtils.GetAvailPhys()), (object) guiResources1, (object) guiResources2, (object) guiResources3, (object) guiResources4, (object) currentProcess.HandleCount);
        }
      }
      catch (Exception ex)
      {
        return "," + ex.Message;
      }
    }

    public static string GetSize()
    {
      try
      {
        using (Process currentProcess = Process.GetCurrentProcess())
          return MemoryUtils.FormatSize((double) currentProcess.PrivateMemorySize64);
      }
      catch (Exception ex)
      {
        return string.Empty;
      }
    }

    [DllImport("User32")]
    private static extern int GetGuiResources(IntPtr hProcess, int uiFlags);
  }
}
