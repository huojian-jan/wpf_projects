// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.ItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class ItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (!(item is CalendarDisplayViewModel displayViewModel) || !(container is FrameworkElement frameworkElement))
        return (DataTemplate) null;
      return displayViewModel.Type == DisplayType.Section ? frameworkElement.FindResource((object) "SectionTemplate") as DataTemplate : frameworkElement.FindResource((object) "TaskTemplate") as DataTemplate;
    }
  }
}
