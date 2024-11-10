// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.FocusStatisticsOverviewItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class FocusStatisticsOverviewItem : FocusStatisticsPanelItemViewModel
  {
    public int TodayPomos { get; set; }

    public int TotalPomos { get; set; }

    public long TodayDuration { get; set; }

    public long TotalDuration { get; set; }

    public FocusStatisticsOverviewItem(StatisticsModel model)
    {
      this.TodayPomos = model != null ? model.todayPomoCount : 0;
      this.TotalPomos = model != null ? model.totalPomoCount : 0;
      this.TodayDuration = model != null ? model.todayPomoDuration : 0L;
      this.TotalDuration = model != null ? model.totalPomoDuration : 0L;
      this.IsOverview = true;
    }
  }
}
