// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.TodaySmartProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class TodaySmartProjectViewModel : SmartProjectViewModel
  {
    public TodaySmartProjectViewModel()
    {
      this.Title = Utils.GetString("Today");
      this.Icon = Utils.GetIconData("CalDayIcon" + DateTime.Today.Day.ToString());
      this.Id = "_special_id_today";
    }
  }
}
