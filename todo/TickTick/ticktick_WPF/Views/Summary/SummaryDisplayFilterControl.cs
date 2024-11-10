// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Summary.SummaryDisplayFilterControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Summary
{
  public class SummaryDisplayFilterControl : UserControl, IComponentConnector, IStyleConnector
  {
    private SummaryFilterViewModel _model;
    private System.Windows.Point _draggingStartPoint;
    private int _startDragIdx;
    internal Image SortByProgressImage;
    internal Image SortByListImage;
    internal Image SortByCompletedDateImage;
    internal Image SortByTaskDateImage;
    internal Image SortByPriorityImage;
    internal Image SortByAssigneeImage;
    internal Image SortByTagImage;
    internal EscPopup SortPopup;
    internal OptionCheckBox SortByProgressButton;
    internal OptionCheckBox SortByListButton;
    internal OptionCheckBox SortByCompletedDateButton;
    internal OptionCheckBox SortByTaskDateButton;
    internal OptionCheckBox SortByTagButton;
    internal OptionCheckBox SortByPriorityButton;
    internal OptionCheckBox SortByAssigneeButton;
    internal Image DisplayFilterImage;
    internal EscPopup FilterPopup;
    internal ItemsControl DisplayItems;
    internal TextBlock ProText;
    internal EscPopup DragItemPopup;
    internal Grid DragContent;
    internal EscPopup ItemStylePopup;
    internal ItemsControl ItemStyleItems;
    private bool _contentLoaded;

    public event EventHandler FilterChanged;

    public SummaryDisplayFilterControl()
    {
      this.InitializeComponent();
      this.ItemStylePopup.Closed += (EventHandler) ((sender, args) =>
      {
        this.FilterPopup.LostFocus -= new RoutedEventHandler(this.FilterPopup_OnLostFocus);
        this.FilterPopup.LostFocus += new RoutedEventHandler(this.FilterPopup_OnLostFocus);
      });
    }

    private void TryShowProDialog()
    {
      if (string.IsNullOrEmpty(this._model.ProItemsText))
        return;
      ProChecker.ShowUpgradeDialog(ProType.SummaryStyle);
      this._model.RemoveProItems();
      this._model.SaveTemplate();
    }

    private void OnDisplayItemMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || !(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is SummaryDisplayItemViewModel dataContext) || this.DisplayItems.Items.IndexOf((object) dataContext) != this._startDragIdx)
        return;
      System.Windows.Point position = Mouse.GetPosition((IInputElement) frameworkElement);
      if (Math.Abs(this._draggingStartPoint.X - position.X) <= 2.0 && Math.Abs(this._draggingStartPoint.Y - position.Y) <= 2.0 || !dataContext.Draggable || dataContext.IsDragging)
        return;
      dataContext.IsDragging = true;
      int num = (int) DragDrop.DoDragDrop((DependencyObject) frameworkElement, (object) dataContext, DragDropEffects.Move);
    }

    private void OnDisplayItemDown(object sender, RoutedEventArgs e)
    {
      if (!(sender is FrameworkElement relativeTo) || !(relativeTo.DataContext is SummaryDisplayItemViewModel dataContext))
        return;
      this._draggingStartPoint = Mouse.GetPosition((IInputElement) relativeTo);
      if (!dataContext.Draggable)
        this._startDragIdx = -1;
      else
        this._startDragIdx = this.DisplayItems.Items.IndexOf((object) dataContext);
    }

    private void OnDisplayItemClick(object sender, RoutedEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement))
        return;
      SummaryDisplayItemViewModel model = frameworkElement.DataContext as SummaryDisplayItemViewModel;
      if (model == null)
        return;
      if (model.Key == "title")
        Utils.Toast(Utils.GetString("task_name_cannot_be_hidden"));
      if (model.ForceEnabled)
      {
        model.Enabled = true;
      }
      else
      {
        model.Enabled = !model.Enabled;
        if (this._model?.DisplayItems != null)
          this._model.SaveSelectedTemplate((Action<SummaryTemplate>) (it =>
          {
            List<SummaryDisplayItem> displayItems = it.displayItems;
            SummaryDisplayItem summaryDisplayItem = displayItems != null ? displayItems.FirstOrDefault<SummaryDisplayItem>((Func<SummaryDisplayItem, bool>) (i => i.key == model.Key)) : (SummaryDisplayItem) null;
            if (summaryDisplayItem == null)
              return;
            summaryDisplayItem.enabled = model.Enabled;
          }));
        EventHandler filterChanged = this.FilterChanged;
        if (filterChanged == null)
          return;
        filterChanged((object) null, (EventArgs) null);
      }
    }

    private void OnSortByClick(object sender, EventArgs e)
    {
      if (sender is FrameworkElement frameworkElement)
      {
        SummarySortType sortBy = this._model.SortBy;
        SummarySortType type;
        if (Enum.TryParse<SummarySortType>((string) frameworkElement.Tag, out type))
        {
          this._model.SortBy = type;
          this.SetDisplaySortIcon(sortBy, Visibility.Collapsed);
          this.SetDisplaySortIcon(this._model.SortBy, Visibility.Visible);
          this._model.SaveSelectedTemplate((Action<SummaryTemplate>) (it => it.sortType = type.ToString()));
          SummaryDisplayFilterControl.CollectSortByAct(type);
        }
      }
      EventHandler filterChanged = this.FilterChanged;
      if (filterChanged != null)
        filterChanged(sender, e);
      this.SortPopup.IsOpen = false;
    }

    private static void CollectSortByAct(SummarySortType sortBy)
    {
      switch (sortBy)
      {
        case SummarySortType.progress:
          UserActCollectUtils.AddClickEvent("summary", "sort", "completion_status");
          break;
        case SummarySortType.project:
          UserActCollectUtils.AddClickEvent("summary", "sort", "list");
          break;
        case SummarySortType.completedTime:
          UserActCollectUtils.AddClickEvent("summary", "sort", "completed_date");
          break;
        case SummarySortType.dueDate:
          UserActCollectUtils.AddClickEvent("summary", "sort", "task_date");
          break;
        case SummarySortType.priority:
          UserActCollectUtils.AddClickEvent("summary", "sort", "priority");
          break;
        case SummarySortType.assignee:
          UserActCollectUtils.AddClickEvent("summary", "sort", "assignee");
          break;
        case SummarySortType.tag:
          UserActCollectUtils.AddClickEvent("summary", "sort", "tag");
          break;
      }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      this.CollectDisplayAct();
      EventHandler filterChanged = this.FilterChanged;
      if (filterChanged != null)
        filterChanged((object) null, (EventArgs) null);
      this.FilterPopup.IsOpen = false;
    }

    private void CollectDisplayAct()
    {
      if (this._model.ShowCompleteDate)
        UserActCollectUtils.AddClickEvent("summary", "view_options", "completed_time_default");
      if (this._model.ShowStatus)
        UserActCollectUtils.AddClickEvent("summary", "view_options", "status_prem");
      if (this._model.ShowTaskDate)
        UserActCollectUtils.AddClickEvent("summary", "view_options", "task_time_prem");
      if (this._model.ShowProgress)
        UserActCollectUtils.AddClickEvent("summary", "view_options", "progress_default");
      if (this._model.ShowDetail)
        UserActCollectUtils.AddClickEvent("summary", "view_options", "detail");
      if (this._model.ShowPomo)
        UserActCollectUtils.AddClickEvent("summary", "view_options", "focus_data");
      if (this._model.ShowProject)
        UserActCollectUtils.AddClickEvent("summary", "view_options", "list");
      UserActCollectUtils.AddClickEvent("summary", "view_options", "task_title_default");
      if (this._model.ShowParent)
        UserActCollectUtils.AddClickEvent("summary", "view_options", "parent_task_prem");
      if (!this._model.ShowTag)
        return;
      UserActCollectUtils.AddClickEvent("summary", "view_options", "tag_prem");
    }

    private void ShowSortPopupClick(object sender, MouseButtonEventArgs e)
    {
      this.SortByProgressButton.Selected = this._model.SortBy == SummarySortType.progress;
      this.SortByCompletedDateButton.Selected = this._model.SortBy == SummarySortType.completedTime;
      this.SortByListButton.Selected = this._model.SortBy == SummarySortType.project;
      this.SortByPriorityButton.Selected = this._model.SortBy == SummarySortType.priority;
      this.SortByTaskDateButton.Selected = this._model.SortBy == SummarySortType.dueDate;
      this.SortByAssigneeButton.Selected = this._model.SortBy == SummarySortType.assignee;
      this.SortByTagButton.Selected = this._model.SortBy == SummarySortType.tag;
      this.SortPopup.IsOpen = true;
    }

    private void ShowFilterPopupClick(object sender, MouseButtonEventArgs e)
    {
      this.FilterPopup.IsOpen = true;
    }

    public void InitModel(SummaryFilterViewModel model)
    {
      this._model = model;
      this.SortByProgressImage.Visibility = Visibility.Collapsed;
      this.SortByListImage.Visibility = Visibility.Collapsed;
      this.SortByCompletedDateImage.Visibility = Visibility.Collapsed;
      this.SortByTaskDateImage.Visibility = Visibility.Collapsed;
      this.SortByPriorityImage.Visibility = Visibility.Collapsed;
      this.SortByAssigneeImage.Visibility = Visibility.Collapsed;
      this.SortByTagImage.Visibility = Visibility.Collapsed;
      this.SetDisplaySortIcon(this._model.SortBy, Visibility.Visible);
      this.DataContext = (object) this._model;
      if (model.DisplayItems == null || model.DisplayItems.Count == 0)
        model.SetupDefaultItems();
      this.DisplayItems.ItemsSource = (IEnumerable) model.DisplayItems;
    }

    private void SetDisplaySortIcon(SummarySortType sortBy, Visibility visible)
    {
      switch (sortBy)
      {
        case SummarySortType.progress:
          this.SortByProgressImage.Visibility = visible;
          break;
        case SummarySortType.project:
          this.SortByListImage.Visibility = visible;
          break;
        case SummarySortType.completedTime:
          this.SortByCompletedDateImage.Visibility = visible;
          break;
        case SummarySortType.dueDate:
          this.SortByTaskDateImage.Visibility = visible;
          break;
        case SummarySortType.priority:
          this.SortByPriorityImage.Visibility = visible;
          break;
        case SummarySortType.assignee:
          this.SortByAssigneeImage.Visibility = visible;
          break;
        case SummarySortType.tag:
          this.SortByTagImage.Visibility = visible;
          break;
      }
    }

    private void OnFilterPopupClosed(object sender, EventArgs e)
    {
      this.OnSaveClick(sender, (RoutedEventArgs) null);
    }

    private void OnMoreItemClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement))
        return;
      SummaryDisplayItemViewModel model = frameworkElement.DataContext as SummaryDisplayItemViewModel;
      if (model == null || model.SupportedStyles == null || model.SupportedStyles.Count <= 0)
        return;
      if (model.Style == null)
        model.Style = model.SupportedStyles.FirstOrDefault<string>();
      if (!model.Enabled)
        model.Style = (string) null;
      this.ItemStyleItems.ItemsSource = (IEnumerable) new ObservableCollection<SummaryDisplayStyleItemModel>(model.SupportedStyles.Select<string, SummaryDisplayStyleItemModel>((Func<string, SummaryDisplayStyleItemModel>) (it => new SummaryDisplayStyleItemModel()
      {
        Key = it,
        Name = SummaryDisplayItemViewModel.GetStyleName(it),
        Selected = model.Style == it,
        Parent = model
      })));
      this.ItemStylePopup.PlacementTarget = (UIElement) frameworkElement;
      this.ItemStylePopup.IsOpen = true;
      this.FilterPopup.LostFocus -= new RoutedEventHandler(this.FilterPopup_OnLostFocus);
    }

    private void ClearDraggingOver()
    {
      if (!(this.DisplayItems.ItemsSource is ObservableCollection<SummaryDisplayItemViewModel> source))
        source = new ObservableCollection<SummaryDisplayItemViewModel>();
      source.ToList<SummaryDisplayItemViewModel>().ForEach((Action<SummaryDisplayItemViewModel>) (it => it.IsDraggingOver = false));
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
      if (!((sender is FrameworkElement frameworkElement ? frameworkElement.DataContext : (object) null) is SummaryDisplayItemViewModel dataContext) || this.DisplayItems.Items.IndexOf((object) dataContext) >= this.DisplayItems.Items.Count - 1)
        return;
      this.ClearDraggingOver();
      dataContext.IsDraggingOver = true;
    }

    private void OnDrop(object sender, DragEventArgs e)
    {
      SummaryDisplayItemViewModel data = e.Data.GetData(typeof (SummaryDisplayItemViewModel)) as SummaryDisplayItemViewModel;
      SummaryDisplayItemViewModel dataContext = (sender is FrameworkElement frameworkElement ? frameworkElement.DataContext : (object) null) as SummaryDisplayItemViewModel;
      int index1 = this.DisplayItems.Items.IndexOf((object) data);
      int index2 = this.DisplayItems.Items.IndexOf((object) dataContext);
      this.ResetDragStatus();
      if (index1 == index2 || index2 == this.DisplayItems.Items.Count - 1)
        return;
      ObservableCollection<SummaryDisplayItemViewModel> itemsSource = this.DisplayItems.ItemsSource as ObservableCollection<SummaryDisplayItemViewModel>;
      if (index1 < index2)
      {
        itemsSource.RemoveAt(index1);
        itemsSource.Insert(index2, data);
      }
      else
      {
        itemsSource.RemoveAt(index1);
        itemsSource.Insert(index2 + 1, data);
      }
      foreach (SummaryDisplayItemViewModel displayItemViewModel in (Collection<SummaryDisplayItemViewModel>) itemsSource)
        displayItemViewModel.SortOrder = (long) itemsSource.IndexOf(displayItemViewModel);
      this._model.DisplayItems = itemsSource;
      this._model.SaveTemplate();
      EventHandler filterChanged = this.FilterChanged;
      if (filterChanged == null)
        return;
      filterChanged((object) null, (EventArgs) null);
    }

    private void ResetDragStatus()
    {
      if (!(this.DisplayItems.ItemsSource is ObservableCollection<SummaryDisplayItemViewModel> source))
        source = new ObservableCollection<SummaryDisplayItemViewModel>();
      source.ToList<SummaryDisplayItemViewModel>().ForEach((Action<SummaryDisplayItemViewModel>) (it =>
      {
        it.IsDragging = false;
        it.IsDraggingOver = false;
      }));
    }

    private void FilterPopup_OnLostFocus(object sender, RoutedEventArgs e)
    {
      this.FilterPopup.IsOpen = false;
      if (!(this.DisplayItems.ItemsSource is ObservableCollection<SummaryDisplayItemViewModel> source))
        source = new ObservableCollection<SummaryDisplayItemViewModel>();
      source.ToList<SummaryDisplayItemViewModel>().ForEach((Action<SummaryDisplayItemViewModel>) (it =>
      {
        it.IsDragging = false;
        it.IsDraggingOver = false;
      }));
      this.TryShowProDialog();
    }

    private void OnStyleClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is FrameworkElement frameworkElement && frameworkElement.DataContext is SummaryDisplayStyleItemModel dataContext)
      {
        dataContext.Selected = true;
        dataContext.Parent.Style = dataContext.Key;
        dataContext.Parent.Enabled = true;
        this.ItemStylePopup.IsOpen = false;
        this._model.SaveTemplate();
        EventHandler filterChanged = this.FilterChanged;
        if (filterChanged != null)
          filterChanged((object) null, (EventArgs) null);
      }
      e.Handled = true;
    }

    private void OnDragLeave(object sender, DragEventArgs e) => this.ClearDraggingOver();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/summary/summarydisplayfiltercontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowSortPopupClick);
          break;
        case 2:
          this.SortByProgressImage = (Image) target;
          break;
        case 3:
          this.SortByListImage = (Image) target;
          break;
        case 4:
          this.SortByCompletedDateImage = (Image) target;
          break;
        case 5:
          this.SortByTaskDateImage = (Image) target;
          break;
        case 6:
          this.SortByPriorityImage = (Image) target;
          break;
        case 7:
          this.SortByAssigneeImage = (Image) target;
          break;
        case 8:
          this.SortByTagImage = (Image) target;
          break;
        case 9:
          this.SortPopup = (EscPopup) target;
          break;
        case 10:
          this.SortByProgressButton = (OptionCheckBox) target;
          break;
        case 11:
          this.SortByListButton = (OptionCheckBox) target;
          break;
        case 12:
          this.SortByCompletedDateButton = (OptionCheckBox) target;
          break;
        case 13:
          this.SortByTaskDateButton = (OptionCheckBox) target;
          break;
        case 14:
          this.SortByTagButton = (OptionCheckBox) target;
          break;
        case 15:
          this.SortByPriorityButton = (OptionCheckBox) target;
          break;
        case 16:
          this.SortByAssigneeButton = (OptionCheckBox) target;
          break;
        case 17:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ShowFilterPopupClick);
          break;
        case 18:
          this.DisplayFilterImage = (Image) target;
          break;
        case 19:
          this.FilterPopup = (EscPopup) target;
          break;
        case 20:
          this.DisplayItems = (ItemsControl) target;
          break;
        case 22:
          this.ProText = (TextBlock) target;
          break;
        case 23:
          this.DragItemPopup = (EscPopup) target;
          break;
        case 24:
          this.DragContent = (Grid) target;
          break;
        case 25:
          this.ItemStylePopup = (EscPopup) target;
          break;
        case 26:
          this.ItemStyleItems = (ItemsControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 21)
        return;
      ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDisplayItemClick);
      ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnDisplayItemDown);
      ((UIElement) target).MouseMove += new MouseEventHandler(this.OnDisplayItemMouseMove);
      ((UIElement) target).Drop += new DragEventHandler(this.OnDrop);
      ((UIElement) target).DragOver += new DragEventHandler(this.OnDragOver);
      ((UIElement) target).DragLeave += new DragEventHandler(this.OnDragLeave);
    }
  }
}
