// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.GlobalHotkey
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using ticktick_WPF.Views.Config;

#nullable disable
namespace ticktick_WPF.Util
{
  public class GlobalHotkey : IDisposable
  {
    private static GlobalHotkey instance;
    private InterceptKeys.LowLevelKeyboardProc hookedLowLevelKeyboardProc;
    private IntPtr hookId = IntPtr.Zero;
    private const int VK_SHIFT = 16;
    private const int VK_CONTROL = 17;
    private const int VK_ALT = 18;
    private const int VK_WIN = 91;

    public event GlobalHotkey.KeyboardCallback hookedKeyboardCallback;

    public static GlobalHotkey Instance
    {
      get
      {
        if (GlobalHotkey.instance == null)
          GlobalHotkey.instance = new GlobalHotkey();
        return GlobalHotkey.instance;
      }
    }

    private GlobalHotkey()
    {
      this.hookedLowLevelKeyboardProc = new InterceptKeys.LowLevelKeyboardProc(this.LowLevelKeyboardProc);
      this.hookId = InterceptKeys.SetHook(this.hookedLowLevelKeyboardProc);
    }

    public SpecialKeyState CheckModifiers()
    {
      SpecialKeyState specialKeyState = new SpecialKeyState();
      if (((int) InterceptKeys.GetKeyState(16) & 32768) != 0)
        specialKeyState.ShiftPressed = true;
      if (((int) InterceptKeys.GetKeyState(17) & 32768) != 0)
        specialKeyState.CtrlPressed = true;
      if (((int) InterceptKeys.GetKeyState(18) & 32768) != 0)
        specialKeyState.AltPressed = true;
      if (((int) InterceptKeys.GetKeyState(91) & 32768) != 0)
        specialKeyState.WinPressed = true;
      return specialKeyState;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private IntPtr LowLevelKeyboardProc(int nCode, UIntPtr wParam, IntPtr lParam)
    {
      bool flag = true;
      if (nCode >= 0 && (wParam.ToUInt32() == 256U || wParam.ToUInt32() == 257U || wParam.ToUInt32() == 260U || wParam.ToUInt32() == 261U) && this.hookedKeyboardCallback != null)
        flag = this.hookedKeyboardCallback((KeyEvent) wParam.ToUInt32(), Marshal.ReadInt32(lParam), this.CheckModifiers());
      return flag ? InterceptKeys.CallNextHookEx(this.hookId, nCode, wParam, lParam) : (IntPtr) 1;
    }

    ~GlobalHotkey() => this.Dispose();

    public void Dispose() => InterceptKeys.UnhookWindowsHookEx(this.hookId);

    public delegate bool KeyboardCallback(KeyEvent keyEvent, int vkCode, SpecialKeyState state);
  }
}
