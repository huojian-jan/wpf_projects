// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.IdleTimeFinder
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.InteropServices;

#nullable disable
namespace ticktick_WPF.Util
{
  public class IdleTimeFinder
  {
    [DllImport("User32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    [DllImport("Kernel32.dll")]
    private static extern uint GetLastError();

    public static uint GetIdleTime()
    {
      LASTINPUTINFO plii = new LASTINPUTINFO();
      plii.cbSize = (uint) Marshal.SizeOf<LASTINPUTINFO>(plii);
      IdleTimeFinder.GetLastInputInfo(ref plii);
      return (uint) Environment.TickCount - plii.dwTime;
    }

    public static long GetLastInputTime()
    {
      LASTINPUTINFO plii = new LASTINPUTINFO();
      plii.cbSize = (uint) Marshal.SizeOf<LASTINPUTINFO>(plii);
      return IdleTimeFinder.GetLastInputInfo(ref plii) ? (long) plii.dwTime : throw new Exception(IdleTimeFinder.GetLastError().ToString());
    }
  }
}
