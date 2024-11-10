// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitListItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitListItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (container is FrameworkElement frameworkElement)
      {
        try
        {
          switch (item)
          {
            case HabitItemViewModel _:
              return frameworkElement.FindResource((object) "HabitListItemTemplate") as DataTemplate;
            case HabitSectionListViewModel _:
              return frameworkElement.FindResource((object) "HabitListSectionTemplate") as DataTemplate;
          }
        }
        catch
        {
          return (DataTemplate) null;
        }
      }
      return (DataTemplate) null;
    }
  }
}
