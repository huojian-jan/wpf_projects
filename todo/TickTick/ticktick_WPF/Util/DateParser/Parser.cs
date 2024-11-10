// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.Parser
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Runtime.InteropServices;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  public static class Parser
  {
    [DllImport("SmartDateDLL.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr parse(
      IntPtr content,
      int conLen,
      IntPtr defaultDateStr,
      int dateLen,
      bool isRecognizeDuratio,
      IntPtr localStr,
      int localLen);

    [DllImport("SmartDateDLL.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void setStartDay(int startDay);

    [DllImport("SmartDateDLL.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void setIsUsOrUkDateFormat(bool isUsOrUkDateFormat);

    [DllImport("SmartDateDLL.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void setDefaultTimeSecond(long defaultTime);

    [DllImport("SmartDateDLL.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void setIs24DateFormat(bool is24Format);

    [DllImport("SmartDateDLL.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void setCustomizeHour(
      int morningHour,
      int morningMin,
      int afternoonHour,
      int afternoonMin,
      int eveningHour,
      int eveningMin,
      int nightHour,
      int nightMin);
  }
}
