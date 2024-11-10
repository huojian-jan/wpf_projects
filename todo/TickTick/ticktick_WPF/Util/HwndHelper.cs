// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.HwndHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Util
{
  public class HwndHelper
  {
    [DllImport("User32.dll")]
    public static extern IntPtr SetFocus(IntPtr hWnd);

    public static void SetFocus(Popup popup, bool getKeyFocus = false)
    {
      if (popup?.Child == null)
        return;
      HwndSource hwndSource = (HwndSource) PresentationSource.FromVisual((Visual) popup.Child);
      if (hwndSource != null)
        HwndHelper.SetFocus(hwndSource.Handle);
      if (!getKeyFocus)
        return;
      Keyboard.Focus((IInputElement) popup);
      FocusManager.SetFocusedElement((DependencyObject) popup, (IInputElement) popup);
    }

    public static void SetFocus(UIElement element)
    {
      if (element == null)
        return;
      HwndSource hwndSource = (HwndSource) PresentationSource.FromVisual((Visual) element);
      if (hwndSource == null)
        return;
      HwndHelper.SetFocus(hwndSource.Handle);
    }
  }
}
