// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TimerDetail.TimerDetailItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.Views.Pomo.FocusStatistics;

#nullable disable
namespace ticktick_WPF.Views.Pomo.TimerDetail
{
  public class TimerDetailItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item is FocusStatisticsPanelItemViewModel panelItemViewModel && container is FrameworkElement frameworkElement)
      {
        if (panelItemViewModel.IsRecord)
          return frameworkElement.FindResource((object) "FocusRecordTemplate") as DataTemplate;
        if (panelItemViewModel.IsLoadMore)
          return frameworkElement.FindResource((object) "FocusLoadMoreTemplate") as DataTemplate;
        if (panelItemViewModel is TimerDetailItemViewModel detailItemViewModel)
        {
          if (detailItemViewModel.TimerTitle)
            return frameworkElement.FindResource((object) "TimerDetailTitleItemTemplate") as DataTemplate;
          if (detailItemViewModel.TimerTimeline)
            return frameworkElement.FindResource((object) "TimerStatisticsTimelineTemplate") as DataTemplate;
          if (detailItemViewModel.TimerOverview)
            return frameworkElement.FindResource((object) "TimerStatisticsOverviewTemplate") as DataTemplate;
        }
      }
      return (DataTemplate) null;
    }
  }
}
