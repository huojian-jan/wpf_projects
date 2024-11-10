// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Kanban.Item.ListItemAddView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.QuickAdd;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Kanban.Item
{
  public class ListItemAddView : Border
  {
    private QuickAddView _quickAddView;
    private TaskListView _listParent;

    public ListItemAddView()
    {
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      this._listParent = (TaskListView) null;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      ListItemAddView child = this;
      child._listParent = Utils.FindParent<TaskListView>((DependencyObject) child);
      if (child._listParent == null)
        m = (DisplayItemModel) null;
      else if (!(child.DataContext is DisplayItemModel m))
        m = (DisplayItemModel) null;
      else if (m.AddViewModel == null)
        m = (DisplayItemModel) null;
      else if (!(child._listParent?.ViewModel.ProjectIdentity is ColumnProjectIdentity projectIdentity))
      {
        TaskListView listParent = child._listParent;
        if (listParent == null)
        {
          m = (DisplayItemModel) null;
        }
        else
        {
          listParent.RemoveAddItem(m);
          m = (DisplayItemModel) null;
        }
      }
      else
      {
        ListItemAddView listItemAddView = child;
        QuickAddView quickAddView = new QuickAddView((IProjectTaskDefault) new ColumnProjectIdentity(projectIdentity.Project, projectIdentity.ColumnId), QuickAddView.Scenario.Kanban, section: m.Section);
        quickAddView.Margin = new Thickness(12.0, 4.0, 12.0, 4.0);
        quickAddView.MaxHeight = 0.0;
        listItemAddView._quickAddView = quickAddView;
        child._quickAddView.TaskAdded += new EventHandler<TaskModel>(child.OnTaskAdded);
        child._quickAddView.TitleText.TextChanged += new EventHandler<string>(child.OnTextChanged);
        child._quickAddView.BatchTaskAdded += new EventHandler<List<TaskModel>>(child.OnBatchTaskAdded);
        child._quickAddView.BeginAnimation(FrameworkElement.MaxHeightProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(0.0), 108.0, 240));
        child._quickAddView.TitleText.EditBox.LostFocus += new RoutedEventHandler(child.OnQuickAddLostFocus);
        if (m.AddViewModel.Text != null)
          child._quickAddView.TitleText.EditBox.Text = m.AddViewModel.Text;
        child._quickAddView.FocusEnd();
        child.Child = (UIElement) child._quickAddView;
        await Task.Delay(20);
        child._listParent.ScrollToItem(m);
        m = (DisplayItemModel) null;
      }
    }

    private void OnTextChanged(object sender, string e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || dataContext.AddViewModel == null)
        return;
      dataContext.AddViewModel.Text = e;
    }

    private async void OnQuickAddLostFocus(object sender, RoutedEventArgs e)
    {
      ListItemAddView child = this;
      if (child._quickAddView == null)
        return;
      await Task.Delay(100);
      if (!child._quickAddView.IsLostFocus())
        return;
      child._quickAddView.TryAddTaskOnLostFocus();
      if (child.DataContext is DisplayItemModel dataContext)
        Utils.FindParent<TaskListView>((DependencyObject) child)?.RemoveAddItem(dataContext, true);
      child._quickAddView.TitleText.EditBox.LostFocus -= new RoutedEventHandler(child.OnQuickAddLostFocus);
      child._quickAddView.BeginAnimation(FrameworkElement.MaxHeightProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(), 0.0, 180));
    }

    private void OnBatchTaskAdded(object sender, List<TaskModel> e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || dataContext.AddViewModel == null)
        return;
      dataContext.AddViewModel.Text = (string) null;
    }

    private void OnTaskAdded(object sender, TaskModel e)
    {
      if (!(this.DataContext is DisplayItemModel dataContext) || dataContext.AddViewModel == null)
        return;
      dataContext.AddViewModel.Text = (string) null;
    }
  }
}
