// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarSettingModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarSettingModel
  {
    public int SideBar = 2;
    public int HeadSwitch;
    public int MultiNum;

    public string GetSwitchMode()
    {
      switch (this.HeadSwitch)
      {
        case 0:
        case 1:
        case 2:
          return this.HeadSwitch.ToString();
        case 3:
          return "D" + Math.Min(14, Math.Max(1, this.MultiNum)).ToString();
        case 4:
          return "W" + Math.Min(6, Math.Max(2, this.MultiNum)).ToString();
        default:
          return "0";
      }
    }

    public string GetSwitchText()
    {
      switch (this.HeadSwitch)
      {
        case 0:
          return "month";
        case 1:
          return "week";
        case 2:
          return "day";
        case 3:
          return "multi_day";
        case 4:
          return "multi_week";
        default:
          return "month";
      }
    }
  }
}
