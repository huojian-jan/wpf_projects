// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WidgetListItemSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class WidgetListItemSelector : DataTemplateSelector
  {
    private readonly ProjectWidget _parent;

    public WidgetListItemSelector(ProjectWidget parent) => this._parent = parent;

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (!(item is DisplayItemModel displayItemModel) || !(container is FrameworkElement frameworkElement))
        return (DataTemplate) null;
      string resourceKey1 = this._parent.ThemeId == "light" ? "LightTaskItemTemplate" : "DarkTaskItemTemplate";
      string resourceKey2 = this._parent.ThemeId == "light" ? "LightSectionTemplate" : "DarkSectionTemplate";
      if (displayItemModel.IsTask || displayItemModel.IsItem)
        return frameworkElement.FindResource((object) resourceKey1) as DataTemplate;
      return displayItemModel.IsSection ? frameworkElement.FindResource((object) resourceKey2) as DataTemplate : frameworkElement.FindResource((object) resourceKey1) as DataTemplate;
    }
  }
}
