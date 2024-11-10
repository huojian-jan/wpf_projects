// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TimerDetail.TimerStatisticsOverviewItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo.TimerDetail
{
  public class TimerStatisticsOverviewItem : TimerDetailItemViewModel
  {
    public int TotalDays { get; set; }

    public long TodayDuration { get; set; }

    public long TotalDuration { get; set; }

    public TimerStatisticsOverviewItem(TimerOverviewModel data)
    {
      this.TotalDays = data != null ? data.Days : 0;
      this.TodayDuration = data != null ? data.Today : 0L;
      this.TotalDuration = data != null ? data.Total : 0L;
      this.TimerOverview = true;
    }
  }
}
