// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.VirtualDesktopUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Util
{
  public class VirtualDesktopUtils
  {
    public static bool GetIsInCurrent(IntPtr handle)
    {
      return Utils.IsWindows7() || VirtualDesktopManager.IsWindowInCurrentDeskTop(handle);
    }
  }
}
