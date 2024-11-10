// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.InterceptKeys
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

#nullable disable
namespace ticktick_WPF.Util
{
  internal static class InterceptKeys
  {
    private const int WH_KEYBOARD_LL = 13;

    public static IntPtr SetHook(InterceptKeys.LowLevelKeyboardProc proc)
    {
      using (Process currentProcess = Process.GetCurrentProcess())
      {
        using (ProcessModule mainModule = currentProcess.MainModule)
          return InterceptKeys.SetWindowsHookEx(13, proc, InterceptKeys.GetModuleHandle(mainModule.ModuleName), 0U);
      }
    }

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr SetWindowsHookEx(
      int idHook,
      InterceptKeys.LowLevelKeyboardProc lpfn,
      IntPtr hMod,
      uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr CallNextHookEx(
      IntPtr hhk,
      int nCode,
      UIntPtr wParam,
      IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern short GetKeyState(int keyCode);

    public delegate IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam);
  }
}
