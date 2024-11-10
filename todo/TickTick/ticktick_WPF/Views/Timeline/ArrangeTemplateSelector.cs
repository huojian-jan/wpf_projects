// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.ArrangeTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class ArrangeTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (container is FrameworkElement frameworkElement)
      {
        switch (item)
        {
          case TimelineCellViewModel _:
            return frameworkElement.FindResource((object) "CellDataTemplate") as DataTemplate;
          case TimelineGroupViewModel _:
            return frameworkElement.FindResource((object) "GroupDataTemplate") as DataTemplate;
        }
      }
      return base.SelectTemplate(item, container);
    }
  }
}
