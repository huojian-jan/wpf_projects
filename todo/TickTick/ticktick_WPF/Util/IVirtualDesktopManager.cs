// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.IVirtualDesktopManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

#nullable disable
namespace ticktick_WPF.Util
{
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("a5cd92ff-29be-454c-8d04-d82879fb3f1b")]
  [SuppressUnmanagedCodeSecurity]
  [ComImport]
  public interface IVirtualDesktopManager
  {
    [MethodImpl(MethodImplOptions.PreserveSig)]
    int IsWindowOnCurrentVirtualDesktop([In] IntPtr TopLevelWindow, out int OnCurrentDesktop);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int GetWindowDesktopId([In] IntPtr TopLevelWindow, out Guid CurrentDesktop);

    [MethodImpl(MethodImplOptions.PreserveSig)]
    int MoveWindowToDesktop([In] IntPtr TopLevelWindow, [MarshalAs(UnmanagedType.LPStruct), In] Guid CurrentDesktop);
  }
}
