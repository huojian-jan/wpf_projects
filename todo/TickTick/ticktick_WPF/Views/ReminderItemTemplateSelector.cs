// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.ReminderItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views
{
  public class ReminderItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (!(item is AdvanceDateModel advanceDateModel) || !(container is FrameworkElement frameworkElement))
        return (DataTemplate) null;
      return advanceDateModel.IsSplit ? frameworkElement.FindResource((object) "SplitTemplate") as DataTemplate : frameworkElement.FindResource((object) "ItemDataTemplate") as DataTemplate;
    }
  }
}
