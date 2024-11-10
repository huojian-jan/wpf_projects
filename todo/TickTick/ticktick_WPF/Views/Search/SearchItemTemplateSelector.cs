// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (container is FrameworkElement frameworkElement)
      {
        switch (item)
        {
          case SearchTaskItemViewModel _:
            return frameworkElement.FindResource((object) "SearchTaskItemTemplate") as DataTemplate;
          case SearchTagAndProjectModel _:
            return frameworkElement.FindResource((object) "SearchProjectDataTemplate") as DataTemplate;
          case SearchLoadMoreModel _:
            return frameworkElement.FindResource((object) "SearchLoadMoreTemplate") as DataTemplate;
          case SearchTitleModel _:
            return frameworkElement.FindResource((object) "SearchTitleTemplate") as DataTemplate;
          case SearchSplitModel _:
            return frameworkElement.FindResource((object) "SearchSplitTemplate") as DataTemplate;
        }
      }
      return (DataTemplate) null;
    }
  }
}
