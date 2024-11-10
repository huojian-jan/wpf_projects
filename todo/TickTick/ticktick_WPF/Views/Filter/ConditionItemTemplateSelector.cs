// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.ConditionItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class ConditionItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (!(item is FilterListItemViewModel listItemViewModel) || !(container is FrameworkElement frameworkElement))
        return (DataTemplate) null;
      if (listItemViewModel.IsSplit)
        return frameworkElement.FindResource((object) "SplitTemplate") as DataTemplate;
      if (listItemViewModel.DisplayType == FilterItemDisplayType.Span)
        return frameworkElement.FindResource((object) "SpanDateItemTemplate") as DataTemplate;
      return listItemViewModel.DisplayType != FilterItemDisplayType.Normal ? frameworkElement.FindResource((object) "DateItemTemplate") as DataTemplate : frameworkElement.FindResource((object) "NormalItemTemplate") as DataTemplate;
    }
  }
}
