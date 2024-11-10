// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.WinInetHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.InteropServices;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public static class WinInetHelper
  {
    public static bool SupressCookiePersist() => WinInetHelper.SetOption(81, new int?(3));

    public static bool EndBrowserSession() => WinInetHelper.SetOption(42, new int?());

    private static bool SetOption(int settingCode, int? option)
    {
      IntPtr num1 = IntPtr.Zero;
      int num2 = 0;
      if (option.HasValue)
      {
        num2 = 4;
        num1 = Marshal.AllocCoTaskMem(num2);
        Marshal.WriteInt32(num1, option.Value);
      }
      int num3 = WinInetHelper.InternetSetOption(0, settingCode, num1, num2) ? 1 : 0;
      if (!(num1 != IntPtr.Zero))
        return num3 != 0;
      Marshal.Release(num1);
      return num3 != 0;
    }

    [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool InternetSetOption(
      int hInternet,
      int dwOption,
      IntPtr lpBuffer,
      int dwBufferLength);
  }
}
