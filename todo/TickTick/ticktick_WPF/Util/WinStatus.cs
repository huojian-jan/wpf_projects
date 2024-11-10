// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.WinStatus
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class WinStatus
  {
    public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
    public static readonly IntPtr HWND_TOP = new IntPtr(0);
    public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
    public const uint SWP_NOSIZE = 1;
    public const uint SWP_NOMOVE = 2;
    public const uint SWP_NOZORDER = 4;
    public const uint SWP_NOREDRAW = 8;
    public const uint SWP_NOACTIVATE = 16;
    public const uint SWP_FRAMECHANGED = 32;
    public const uint SWP_SHOWWINDOW = 64;
    public const uint SWP_HIDEWINDOW = 128;
    public const uint SWP_NOCOPYBITS = 256;
    public const uint SWP_NOOWNERZORDER = 512;
    public const uint SWP_NOSENDCHANGING = 1024;
    public const uint TOPMOST_FLAGS = 3;
    public const uint ZPOS_FLAGS = 1555;
    public const uint WM_ACTIVATEAPP = 28;
    public const uint WM_ACTIVATE = 6;
    public const uint WM_SETFOCUS = 7;
    public const uint WM_NCCALCSIZE = 131;
    public const uint WM_WINDOWPOSCHANGING = 70;
  }
}
