// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusStatistics.FocusStatisticsPanelItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Views.Pomo.FocusStatistics
{
  public class FocusStatisticsPanelItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item is FocusStatisticsPanelItemViewModel panelItemViewModel && container is FrameworkElement frameworkElement)
      {
        if (panelItemViewModel.IsTitle)
          return frameworkElement.FindResource((object) "FocusStatisticsTitleTemplate") as DataTemplate;
        if (panelItemViewModel.IsOverview)
          return frameworkElement.FindResource((object) "FocusStatisticsTemplate") as DataTemplate;
        if (panelItemViewModel.IsRecord)
          return frameworkElement.FindResource((object) "FocusRecordTemplate") as DataTemplate;
        if (panelItemViewModel.IsLoadMore)
          return frameworkElement.FindResource((object) "FocusLoadMoreTemplate") as DataTemplate;
        if (panelItemViewModel.IsEmpty)
          return frameworkElement.FindResource((object) "FocusRecordEmptyTemplate") as DataTemplate;
      }
      return (DataTemplate) null;
    }
  }
}
