// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomControl.MenuItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Views.CustomControl
{
  public class MenuItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (!(item is CustomMenuItemViewModel menuItemViewModel) || !(container is FrameworkElement frameworkElement))
        return (DataTemplate) null;
      if (menuItemViewModel.IsImage)
        return frameworkElement.FindResource((object) "ImageItem") as DataTemplate;
      if (menuItemViewModel.IsMessage)
        return frameworkElement.FindResource((object) "MessageItem") as DataTemplate;
      if (menuItemViewModel.IsSplit)
        return frameworkElement.FindResource((object) "SplitLineItem") as DataTemplate;
      return menuItemViewModel.IsCenterText ? frameworkElement.FindResource((object) "TextCenterItem") as DataTemplate : frameworkElement.FindResource((object) "IconItem") as DataTemplate;
    }
  }
}
