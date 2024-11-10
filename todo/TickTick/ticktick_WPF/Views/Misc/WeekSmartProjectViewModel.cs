// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.WeekSmartProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class WeekSmartProjectViewModel : SmartProjectViewModel
  {
    public WeekSmartProjectViewModel()
    {
      this.Title = Utils.GetString("Next7Day");
      this.Icon = Utils.GetIconData("CalWeekIcon" + DateTime.Today.DayOfWeek.ToString().Substring(0, 2));
      this.Id = "_special_id_week";
    }
  }
}
