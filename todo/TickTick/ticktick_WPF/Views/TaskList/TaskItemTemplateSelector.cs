// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskList.TaskItemTemplateSelector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows;
using System.Windows.Controls;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.TaskList
{
  public class TaskItemTemplateSelector : DataTemplateSelector
  {
    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if (item is DisplayItemModel displayItemModel)
      {
        if (container is FrameworkElement frameworkElement)
        {
          try
          {
            if (displayItemModel.IsLoadMore)
              return frameworkElement.FindResource((object) "LoadMoreTemplate") as DataTemplate;
            if (displayItemModel.AddViewModel != null)
              return frameworkElement.FindResource((object) "AddViewTemplate") as DataTemplate;
            if (displayItemModel.IsSection)
              return displayItemModel.Section.Customized ? frameworkElement.FindResource((object) "CustomSectionTemplate") as DataTemplate : frameworkElement.FindResource((object) "SectionTemplate") as DataTemplate;
            if (displayItemModel.IsNormalItem)
            {
              if (displayItemModel.InDetail)
                return frameworkElement.FindResource((object) "SubTaskItemTemplate") as DataTemplate;
              if (displayItemModel.InKanban)
                return frameworkElement.FindResource((object) "KanbanItemTemplate") as DataTemplate;
              if (displayItemModel.InMatrix)
                return frameworkElement.FindResource((object) "MatrixItemTemplate") as DataTemplate;
              return !displayItemModel.HitVisible ? frameworkElement.FindResource((object) "PreviewItem") as DataTemplate : frameworkElement.FindResource((object) "TaskItemTemplate") as DataTemplate;
            }
          }
          catch (Exception ex)
          {
            return (DataTemplate) null;
          }
        }
      }
      return (DataTemplate) null;
    }
  }
}
