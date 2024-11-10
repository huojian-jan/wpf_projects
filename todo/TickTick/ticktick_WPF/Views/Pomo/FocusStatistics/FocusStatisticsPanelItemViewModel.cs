// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.FocusStatisticsPanelItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class FocusStatisticsPanelItemViewModel : BaseViewModel
  {
    public bool IsTitle { get; set; }

    public bool IsOverview { get; set; }

    public bool IsRecord { get; set; }

    public bool IsLoadMore { get; set; }

    public bool IsEmpty { get; set; }
  }
}
