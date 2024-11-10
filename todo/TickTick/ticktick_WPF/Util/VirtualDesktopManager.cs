// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.VirtualDesktopManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.InteropServices;

#nullable disable
namespace ticktick_WPF.Util
{
  public class VirtualDesktopManager
  {
    private static readonly VirtualDesktopManager Vdm = new VirtualDesktopManager();
    private CVirtualDesktopManager cmanager;
    private IVirtualDesktopManager manager;

    public static bool IsWindowInCurrentDeskTop(IntPtr window)
    {
      return VirtualDesktopManager.Vdm.IsWindowOnCurrentVirtualDesktop(window);
    }

    public VirtualDesktopManager()
    {
      this.cmanager = new CVirtualDesktopManager();
      this.manager = (IVirtualDesktopManager) this.cmanager;
    }

    ~VirtualDesktopManager()
    {
      this.manager = (IVirtualDesktopManager) null;
      this.cmanager = (CVirtualDesktopManager) null;
    }

    public bool IsWindowOnCurrentVirtualDesktop(IntPtr TopLevelWindow)
    {
      int OnCurrentDesktop;
      int errorCode;
      if ((errorCode = this.manager.IsWindowOnCurrentVirtualDesktop(TopLevelWindow, out OnCurrentDesktop)) != 0)
        Marshal.ThrowExceptionForHR(errorCode);
      return OnCurrentDesktop != 0;
    }

    public Guid GetWindowDesktopId(IntPtr TopLevelWindow)
    {
      Guid CurrentDesktop;
      int windowDesktopId;
      if ((windowDesktopId = this.manager.GetWindowDesktopId(TopLevelWindow, out CurrentDesktop)) != 0)
        Marshal.ThrowExceptionForHR(windowDesktopId);
      return CurrentDesktop;
    }

    public void MoveWindowToDesktop(IntPtr TopLevelWindow, Guid CurrentDesktop)
    {
      int desktop;
      if ((desktop = this.manager.MoveWindowToDesktop(TopLevelWindow, CurrentDesktop)) == 0)
        return;
      Marshal.ThrowExceptionForHR(desktop);
    }
  }
}
