// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WindowSinker
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class WindowSinker
  {
    private const uint SWP_NOSIZE = 1;
    private const uint SWP_NOMOVE = 2;
    private const uint SWP_NOACTIVATE = 16;
    private const uint SWP_NOZORDER = 4;
    private const int WM_ACTIVATEAPP = 28;
    private const int WM_ACTIVATE = 6;
    private const int WM_SETFOCUS = 7;
    private const int WM_NCCALCSIZE = 131;
    private const int WM_WINDOWPOSCHANGING = 70;
    private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
    private Window Window;

    public WindowSinker(Window Window) => this.Window = Window;

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(
      IntPtr hWnd,
      IntPtr hWndInsertAfter,
      int X,
      int Y,
      int cx,
      int cy,
      uint uFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr DeferWindowPos(
      IntPtr hWinPosInfo,
      IntPtr hWnd,
      IntPtr hWndInsertAfter,
      int x,
      int y,
      int cx,
      int cy,
      uint uFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr BeginDeferWindowPos(int nNumWindows);

    [DllImport("user32.dll")]
    private static extern bool EndDeferWindowPos(IntPtr hWinPosInfo);

    private void OnClosing(object sender, CancelEventArgs e)
    {
      HwndSource.FromHwnd(new WindowInteropHelper(this.Window).Handle).RemoveHook(new HwndSourceHook(this.WndProc));
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      WindowSinker.SetWindowPos(new WindowInteropHelper(this.Window).Handle, WindowSinker.HWND_BOTTOM, 0, 0, 0, 0, 19U);
      HwndSource.FromHwnd(new WindowInteropHelper(this.Window).Handle).AddHook(new HwndSourceHook(this.WndProc));
    }

    private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
      if (msg == 7 || msg == 131)
      {
        hWnd = new WindowInteropHelper(this.Window).Handle;
        WindowSinker.SetWindowPos(hWnd, WindowSinker.HWND_BOTTOM, 0, 0, 0, 0, 19U);
        handled = true;
      }
      return IntPtr.Zero;
    }

    public void Sink()
    {
      this.Window.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Window.Closing += new CancelEventHandler(this.OnClosing);
    }

    public void Unsink()
    {
      this.Window.Loaded -= new RoutedEventHandler(this.OnLoaded);
      this.Window.Closing -= new CancelEventHandler(this.OnClosing);
    }
  }
}
