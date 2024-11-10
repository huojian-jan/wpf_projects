// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.ProjectList.ProjectItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.MainListView.ProjectList
{
  public class ProjectItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (!(item is ProjectItemViewModel projectItemViewModel) || !(container is FrameworkElement frameworkElement))
        return (DataTemplate) null;
      if (projectItemViewModel.IsPtfAll)
        return frameworkElement.FindResource((object) "SectionGroupItemTemplate") as DataTemplate;
      if (projectItemViewModel.IsEmptyProject)
        return frameworkElement.FindResource((object) "EmptyProjectTemplate") as DataTemplate;
      if (projectItemViewModel.IsSplitLine)
        return frameworkElement.FindResource((object) "SplitLineTemplate") as DataTemplate;
      return projectItemViewModel.IsEmptySubItem ? frameworkElement.FindResource((object) "EmptySubItemTemplate") as DataTemplate : frameworkElement.FindResource((object) "ProjectItemTemplate") as DataTemplate;
    }
  }
}
