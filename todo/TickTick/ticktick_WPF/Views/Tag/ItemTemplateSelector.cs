// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.ItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class ItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (!(item is TagLabelViewModel tagLabelViewModel) || !(container is FrameworkElement frameworkElement))
        return (DataTemplate) null;
      return tagLabelViewModel.IsAdd ? frameworkElement.FindResource((object) "TagAddTemplate") as DataTemplate : frameworkElement.FindResource((object) "TagLabelTemplate") as DataTemplate;
    }
  }
}
