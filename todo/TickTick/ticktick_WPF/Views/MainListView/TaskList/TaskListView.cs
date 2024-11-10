// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.TaskList.TaskListView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Editing;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Event;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.Sync.ClosedLoader;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Eisenhower;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Kanban;
using ticktick_WPF.Views.Kanban.Item;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;
using ticktick_WPF.Views.TaskList.Item;
using ticktick_WPF.Views.Undo;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.MainListView.TaskList
{
  public class TaskListView : Grid, IBatchAddable, ITaskList, ISectionList
  {
    private int _caretIndex = -1;
    private bool _splitting;
    private ListView _taskList = new ListView();
    private LoadMoreItemControl _loadMoreItem;
    private bool _isExpandOrFolding;
    private Popup _quickSetPopup;
    private bool _canReload = true;
    private double _dragStartX;
    private int _dragStartIndex;
    private int _dragCurrentIndex;
    private Popup _taskDragPopup;
    private System.Windows.Point _dragPoint;
    private DisplayItemModel _dragCheckItem;
    private int _parentLevel;
    private DisplayItemModel _frontItem;
    private DateTime _autoScrollTime;
    private DisplayItemModel _dragModel;
    private Popup _recordPopup;
    private Popup _checkInPopup;

    public event EventHandler ItemsCountChanged;

    public event EventHandler MoveUpCaret;

    public event EventHandler NeedReload;

    public event EventHandler<List<string>> BatchSelect;

    public event EventHandler<DragMouseEvent> DragOver;

    public event EventHandler<string> DragDropped;

    public event EventHandler<List<string>> BatchDragDropped;

    public event EventHandler<string> NavigateTask;

    public TaskListViewModel ViewModel => this.DataContext as TaskListViewModel;

    public ObservableCollection<DisplayItemModel> DisplayModels => this.ViewModel.Items;

    public TaskListView()
    {
      this.DataContext = (object) new TaskListViewModel();
      this.InitListView();
      this.MouseMove += new MouseEventHandler(this.TaskListMouseMove);
      this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.TaskListMouseUp);
      this.LostFocus += (RoutedEventHandler) ((sender, args) => this._caretIndex = -1);
      this.Loaded += new RoutedEventHandler(this.OnControlLoaded);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    private void OnControlLoaded(object sender, RoutedEventArgs e) => this.BindEvents();

    public void SetIdentity(ProjectIdentity identity, TaskListDisplayType displayType = TaskListDisplayType.Normal)
    {
      this.ViewModel.SetIdentity(identity, displayType);
    }

    private void InitListView()
    {
      this._taskList.SetBinding(ItemsControl.ItemsSourceProperty, "Items");
      this._taskList.SetResourceReference(FrameworkElement.StyleProperty, (object) "ListViewStyle");
      this._taskList.ItemTemplateSelector = (DataTemplateSelector) new TaskItemTemplateSelector();
      this._taskList.PreviewMouseRightButtonUp += new MouseButtonEventHandler(this.ShowBatchContextMenu);
      this._taskList.PreviewKeyDown += new KeyEventHandler(this.OnKeyDown);
      this._taskList.MouseMove += new MouseEventHandler(this.TaskListMouseMove);
      this._taskList.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.TaskListMouseUp);
      this._taskList.FocusVisualStyle = (Style) null;
      this.Children.Add((UIElement) this._taskList);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      if (this._taskList.Template.FindName("ScrollViewer", (FrameworkElement) this._taskList) is ScrollViewer name1)
        name1.PreviewMouseWheel += new MouseWheelEventHandler(this.OnPreviewMouseWheel);
      if (!(this._taskList.Template.FindName("ItemsPresenter", (FrameworkElement) this._taskList) is ItemsPresenter name2))
        return;
      name2.RequestBringIntoView += new RequestBringIntoViewEventHandler(this.OnItemBringIntoView);
    }

    private void OnItemBringIntoView(object sender, RequestBringIntoViewEventArgs e)
    {
      if (!this.ViewModel.InMatrix)
        return;
      e.Handled = true;
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (this.ViewModel.AddingTask)
      {
        e.Handled = true;
      }
      else
      {
        if (!this.IsMouseOver || e.Handled || !(sender is SmoothScrollViewer smoothScrollViewer) || smoothScrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
          return;
        if (Math.Abs(smoothScrollViewer.VerticalOffset - (double) e.Delta - smoothScrollViewer.ScrollableHeight) < 0.05)
          Utils.FindSingleVisualChildren<LoadMoreItemControl>((DependencyObject) this)?.OnLoadMore(false);
        smoothScrollViewer.HandleSmoothMouseWheel(e);
      }
    }

    private void TaskListMouseUp(object sender, MouseButtonEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this._taskList);
      if (this._taskDragPopup != null && this._taskDragPopup.IsOpen)
        e.Handled = true;
      this.StopDragging(position.X >= (this.ViewModel.InDetail ? -20.0 : 0.0) && position.X <= (this.ViewModel.InDetail ? this.ActualWidth + 20.0 : this.ActualWidth) && (!this.ViewModel.InDetail || position.Y > 0.0), (MouseEventArgs) e);
      if (this._taskDragPopup == null)
        return;
      this._taskDragPopup.IsOpen = false;
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
    }

    private void ShowBatchContextMenu(object sender, MouseButtonEventArgs e)
    {
      if (this.ViewModel.ProjectIdentity != null && !this.ViewModel.ProjectIdentity.CanEdit)
      {
        if (!(this.ViewModel.ProjectIdentity is TrashProjectIdentity))
          return;
        this.ShowTaskOperationInTrash(e);
      }
      else
      {
        IBatchEditable batchEditor = this.ViewModel.BatchEditor;
        if (batchEditor == null || !batchEditor.GetSelectedTaskIds().Any<string>() || batchEditor.GetSelectedTaskIds().Count <= 1)
          return;
        batchEditor.ShowBatchOperationDialog();
        e.Handled = true;
      }
    }

    private void ShowTaskOperationInTrash(MouseButtonEventArgs e)
    {
      List<string> selectedIds = this.GetSelectedTaskIds();
      if (selectedIds.Count <= 1)
        return;
      bool flag = TaskService.IsTeamTask(selectedIds[0]);
      string empty = string.Empty;
      List<OperationItemViewModel> types;
      if (!flag)
      {
        types = new List<OperationItemViewModel>()
        {
          new OperationItemViewModel(ActionType.Restore),
          new OperationItemViewModel(ActionType.DeleteForever)
        };
      }
      else
      {
        types = new List<OperationItemViewModel>();
        types.Add(new OperationItemViewModel(ActionType.Restore));
      }
      OperationDialog operationDialog = new OperationDialog(empty, types);
      operationDialog.SetPlaceTarget((UIElement) this);
      operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) (async (o, kv) =>
      {
        switch (kv.Value)
        {
          case ActionType.DeleteForever:
            if (!new CustomerDialog(Utils.GetString("DeleteForever"), Utils.GetString("BatchDeleteForeverHint"), Utils.GetString("Delete"), Utils.GetString("Cancel"))
            {
              Owner = Window.GetWindow((DependencyObject) this),
              Topmost = false
            }.ShowDialog().GetValueOrDefault())
              break;
            int num = await TaskService.BatchDeleteTaskByIds(selectedIds, false, 2) ? 1 : 0;
            break;
          case ActionType.Restore:
            await TaskService.BatchRestoreProject(selectedIds);
            break;
        }
      });
      operationDialog.Show();
      e.Handled = true;
    }

    public async Task Load(
      bool forceRefresh = false,
      bool loadMore = true,
      bool restoreSelected = true,
      bool ignoreFocus = false,
      bool setSelect = true,
      bool scroll = false,
      bool refreshData = true)
    {
      this.LoadAsync(forceRefresh, loadMore, restoreSelected, ignoreFocus, setSelect, scroll, refreshData);
    }

    public async Task LoadAsync(
      bool forceRefresh = false,
      bool loadMore = true,
      bool restoreSelected = true,
      bool ignoreFocus = false,
      bool setSelect = true,
      bool scroll = false,
      bool refreshData = true)
    {
      await this.Dispatcher.Invoke<Task>((Func<Task>) (async () =>
      {
        // ISSUE: unable to decompile the method.
      }));
    }

    private void RestoreSelected(bool restore = false)
    {
      if (!(this.DataContext is TaskListViewModel dataContext))
        return;
      if (!restore)
      {
        dataContext.InBatchSelect = false;
        dataContext.SetSelectedTaskIds(new List<string>());
        EventHandler<List<string>> batchSelect = this.BatchSelect;
        if (batchSelect == null)
          return;
        batchSelect((object) this, dataContext.SelectedTaskIds);
      }
      else
      {
        List<DisplayItemModel> source = new List<DisplayItemModel>();
        foreach (DisplayItemModel displayItemModel in dataContext.Items.ToList<DisplayItemModel>())
        {
          if (displayItemModel.Selected && !displayItemModel.IsSection)
            source.Add(displayItemModel);
          if (!displayItemModel.IsOpen)
          {
            foreach (DisplayItemModel childrenModel in displayItemModel.GetChildrenModels(true))
            {
              if (childrenModel.Selected)
                source.Add(childrenModel);
            }
          }
        }
        if (source.Count == 0)
          return;
        if (source.Count > 1 || dataContext.InBatchSelect)
        {
          dataContext.InBatchSelect = true;
          source.ForEach((Action<DisplayItemModel>) (item => item.InBatchSelected = item.IsSection || item.Selected));
        }
        List<string> list = source.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (t => t.Id)).ToList<string>();
        bool flag = dataContext.SelectedTaskIds == null || list.Count != dataContext.SelectedTaskIds.Count || list.Union<string>((IEnumerable<string>) dataContext.SelectedTaskIds).Count<string>() != dataContext.SelectedTaskIds.Count;
        dataContext.SetSelectedTaskIds(list);
        if (!dataContext.InKanban)
        {
          DisplayItemModel displayItemModel1 = (DisplayItemModel) null;
          for (int index = 0; index < dataContext.Items.Count; ++index)
          {
            DisplayItemModel displayItemModel2 = this.ViewModel.Items[index];
            if (displayItemModel2.Selected)
            {
              if (displayItemModel1 == null)
              {
                displayItemModel1 = displayItemModel2;
                displayItemModel2.ShowTopMargin = true;
              }
              else
              {
                displayItemModel2.ShowTopMargin = false;
                displayItemModel1.ShowBottomMargin = false;
                displayItemModel1 = displayItemModel2;
              }
            }
            else if (displayItemModel1 != null)
            {
              displayItemModel1.ShowBottomMargin = true;
              displayItemModel1 = (DisplayItemModel) null;
            }
          }
          if (displayItemModel1 != null)
            displayItemModel1.ShowBottomMargin = true;
        }
        if (!flag)
          return;
        if (dataContext.InBatchSelect)
        {
          EventHandler<List<string>> batchSelect = this.BatchSelect;
          if (batchSelect == null)
            return;
          batchSelect((object) this, dataContext.SelectedTaskIds);
        }
        else
        {
          if (list.Count <= 0)
            return;
          this.NotifyItemSelect(new ListItemSelectModel(list[0], (string) null, DisplayType.Task, TaskSelectType.Click));
        }
      }
    }

    private void NotifyMultipleSelected()
    {
      if (!(this.DataContext is TaskListViewModel dataContext) || !dataContext.OnBatchSelectChanged())
        return;
      EventHandler<List<string>> batchSelect = this.BatchSelect;
      if (batchSelect == null)
        return;
      batchSelect((object) this, dataContext.SelectedTaskIds);
    }

    public async void SetItemSelected(List<DisplayItemModel> items, string itemId, bool scroll = true)
    {
      if (items != null)
      {
        DisplayItemModel displayItemModel1 = (DisplayItemModel) null;
        for (int index = 0; index < items.Count; ++index)
        {
          DisplayItemModel displayItemModel2 = items[index];
          if (displayItemModel2.Selected)
            displayItemModel2.Selected = false;
          if (displayItemModel2.Id == itemId)
          {
            displayItemModel2.Selected = true;
            if (index > 0)
              items[index - 1].LineVisible = false;
            displayItemModel1 = displayItemModel2;
          }
        }
        if (displayItemModel1 != null)
        {
          this.ViewModel.SetSelectedTaskIds(new List<string>()
          {
            displayItemModel1.Id
          });
          if (scroll)
            this.TryFocusAndSelectModelById(displayItemModel1.Id);
        }
      }
      this.SelectedId = itemId;
    }

    public async void ToggleSelectedItemCompleted()
    {
      List<string> selectedTaskIds = this.GetSelectedTaskIds();
      if (selectedTaskIds.Count > 0)
      {
        int num = await TaskService.BatchCompleteTasks(selectedTaskIds) ? 1 : 0;
        this.LoadAsync(true);
      }
      else
      {
        ListViewItem parentObj = (ListViewItem) this._taskList.ItemContainerGenerator.ContainerFromItem((object) this.GetSelectedModel());
        if (parentObj == null)
          return;
        Utils.FindSingleVisualChildren<TaskListItem>((DependencyObject) parentObj)?.ToggleTaskCompleted();
      }
    }

    private ITaskOperation GetFocusingItem()
    {
      ITaskOperation focusedItemInWindow = this.GetFocusedItemInWindow();
      TaskListView parentList = focusedItemInWindow?.GetParentList();
      return parentList == null || !parentList.Equals((object) this) ? (ITaskOperation) null : focusedItemInWindow;
    }

    private ITaskOperation GetFocusedItemInWindow()
    {
      Window window = Window.GetWindow((DependencyObject) this);
      if (window == null || !window.IsActive)
        return (ITaskOperation) null;
      switch (FocusManager.GetFocusedElement((DependencyObject) window))
      {
        case ITaskOperation focusedItemInWindow:
          return focusedItemInWindow;
        case TextArea child:
          ITaskOperation parent = Utils.FindParent<ITaskOperation>((DependencyObject) child);
          if (parent != null)
            return parent;
          break;
      }
      return (ITaskOperation) null;
    }

    public void ScrollToTop()
    {
      if (!(this._taskList.Template.FindName("ScrollViewer", (FrameworkElement) this._taskList) is ScrollViewer name) || name.VerticalOffset == 0.0)
        return;
      name.ScrollToVerticalOffset(0.0);
    }

    public bool CanBatchAdd()
    {
      return this.DataContext is TaskListViewModel dataContext && !(dataContext.ProjectIdentity is AssignToMeProjectIdentity) && !(dataContext.ProjectIdentity is BindAccountCalendarProjectIdentity);
    }

    public int QuadrantLevel { get; set; }

    public string SelectedId
    {
      get
      {
        List<string> selectedTaskIds = this.ViewModel.BatchEditor?.GetSelectedTaskIds();
        return selectedTaskIds == null || __nonvirtual (selectedTaskIds.Count) != 1 ? (string) null : selectedTaskIds[0];
      }
      set
      {
        if (string.IsNullOrEmpty(value))
          return;
        this.ViewModel.SetSelectedTaskIds(new List<string>()
        {
          value
        });
      }
    }

    public bool IsLocked { get; set; }

    public bool SectionEditing { get; set; }

    public bool Editable()
    {
      return this.DataContext is TaskListViewModel dataContext && dataContext.ProjectIdentity.CanEdit;
    }

    public bool Copyable() => !(this.ViewModel.ProjectIdentity is AssignToMeProjectIdentity);

    public void OnDetailTaskNavigated(string taskId)
    {
      this.SetItemSelected(this.DisplayModels.ToList<DisplayItemModel>(), taskId);
    }

    public void SelectTask(string taskId, TaskSelectType type, bool ignoreBatch = false)
    {
      if (TaskDragHelpModel.DragHelp.IsDragging)
        return;
      if (type == TaskSelectType.Click)
      {
        DisplayItemModel displayItemModel = this.ViewModel.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.Id == taskId));
        if (displayItemModel != null && displayItemModel.IsTaskOrNote && !ignoreBatch)
        {
          if (Utils.IfCtrlPressed())
          {
            this.BatchCtrlSelect(taskId);
            return;
          }
          if (Utils.IfShiftPressed())
          {
            this.BatchShiftSelect(taskId);
            return;
          }
        }
        this.SelectItem(new ListItemSelectModel(taskId, string.Empty, DisplayType.Task, type));
        this.ViewModel.SetSelectedTaskIds(this.GetSelectedTaskIds());
      }
      else
      {
        this.SelectItem(taskId, type);
        this.ScrollToItemById(taskId);
      }
    }

    public async void TrySetTitleReadonly(string taskId)
    {
      DisplayItemModel selected = this.ViewModel.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.Id == taskId));
      if (selected == null)
      {
        selected = (DisplayItemModel) null;
      }
      else
      {
        switch (selected.Type)
        {
          case DisplayType.CheckItem:
          case DisplayType.Agenda:
            AgendaHelper.IAgenda agenda = (AgendaHelper.IAgenda) selected;
            if (selected.Type == DisplayType.CheckItem)
            {
              agenda = (AgendaHelper.IAgenda) await TaskDao.GetThinTaskById(selected.TaskId);
              if (agenda.GetAttendId() == null)
              {
                selected = (DisplayItemModel) null;
                break;
              }
            }
            if (AgendaHelper.CanAccessAgenda(agenda))
            {
              selected = (DisplayItemModel) null;
              break;
            }
            TaskTitleBox taskTitleTextBox1 = this.GetTaskTitleTextBox(selected);
            if (taskTitleTextBox1 == null)
            {
              selected = (DisplayItemModel) null;
              break;
            }
            taskTitleTextBox1.IsReadOnly = true;
            IToastShowWindow toastParent1 = this.GetToastParent();
            if (toastParent1 == null)
            {
              selected = (DisplayItemModel) null;
              break;
            }
            toastParent1.TryToastString((object) null, Utils.GetString(selected.Type == DisplayType.CheckItem ? "OnlyOwnerCanChangeSubtask" : "AttendeeModifyContent"));
            selected = (DisplayItemModel) null;
            break;
          case DisplayType.Event:
            TaskTitleBox taskTitleTextBox2 = this.GetTaskTitleTextBox(selected);
            if (taskTitleTextBox2 == null)
            {
              selected = (DisplayItemModel) null;
              break;
            }
            taskTitleTextBox2.IsReadOnly = true;
            if (UserDao.IsUserValid())
            {
              string calendarId = selected.CalendarId;
              BindCalendarModel bindCalendarModel = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == calendarId));
              if (bindCalendarModel == null)
              {
                selected = (DisplayItemModel) null;
                break;
              }
              if (!bindCalendarModel.Accessible)
              {
                selected = (DisplayItemModel) null;
                break;
              }
              taskTitleTextBox2.IsReadOnly = false;
              selected = (DisplayItemModel) null;
              break;
            }
            IToastShowWindow toastParent2 = this.GetToastParent();
            if (toastParent2 == null)
            {
              selected = (DisplayItemModel) null;
              break;
            }
            toastParent2.TryToastString((object) null, Utils.GetString("CannotEditGoogleEvents"));
            selected = (DisplayItemModel) null;
            break;
          default:
            selected = (DisplayItemModel) null;
            break;
        }
      }
    }

    public async void CopyTask(string taskId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      if (await ProChecker.CheckTaskLimit(thinTaskById.GetProjectId()))
        return;
      TaskModel taskModel = await TaskService.CopyTask(taskId);
      UtilLog.Info("ListItem.Copy task " + taskId + ",copy " + taskModel.id);
      this.NotifyItemSelect(new ListItemSelectModel(taskModel.id, (string) null, DisplayType.Task, TaskSelectType.Navigate));
    }

    public void DeleteTask(string taskId, TaskDeleteType type)
    {
      UtilLog.Info(string.Format("TaskList.Delete id {0} type {1}", (object) taskId, (object) type));
      this.TryDeleteTask(taskId, type);
    }

    private async Task TryDeleteTask(string taskId, TaskDeleteType type)
    {
      TaskListView taskListView = this;
      DisplayItemModel item = taskListView.DisplayModels.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (p => p.Id == taskId));
      if (item != null)
      {
        List<TaskModel> subTasks = await TaskService.GetAllSubTasksByIdAsync(taskId, item.ProjectId);
        if (type == TaskDeleteType.DeleteButton)
        {
          TaskModel task = await TaskDao.GetThinTaskById(taskId);
          if (task == null)
          {
            item = (DisplayItemModel) null;
            return;
          }
          if (!task.CheckEnable())
          {
            item = (DisplayItemModel) null;
            return;
          }
          if (!string.IsNullOrEmpty(task?.attendId))
          {
            if (await TaskOperationHelper.CheckIfAllowDeleteAgenda(task, (DependencyObject) taskListView))
            {
              if (taskListView.ViewModel.InDetail && !string.IsNullOrEmpty(task.repeatFlag) && task.status == 0)
              {
                TaskDetailPopup parent = Utils.FindParent<TaskDetailPopup>((DependencyObject) taskListView);
                if ((parent != null ? (parent.ShowInCal ? 1 : 0) : 0) != 0)
                {
                  // ISSUE: explicit non-virtual call
                  ModifyRepeatHandler.TryDeleteRecurrence(task.id, task.startDate, task.dueDate, __nonvirtual (taskListView.GetToastParent()));
                  item = (DisplayItemModel) null;
                  return;
                }
              }
              await taskListView.DeleteAgenda(task);
            }
          }
          else
          {
            if (taskListView.ViewModel.InDetail && !string.IsNullOrEmpty(task.repeatFlag) && task.status == 0)
            {
              TaskDetailPopup parent = Utils.FindParent<TaskDetailPopup>((DependencyObject) taskListView);
              if ((parent != null ? (parent.ShowInCal ? 1 : 0) : 0) != 0)
              {
                // ISSUE: explicit non-virtual call
                ModifyRepeatHandler.TryDeleteRecurrence(task.id, task.startDate, task.dueDate, __nonvirtual (taskListView.GetToastParent()));
                item = (DisplayItemModel) null;
                return;
              }
            }
            List<TaskModel> taskModelList = subTasks;
            // ISSUE: explicit non-virtual call
            if ((taskModelList != null ? (__nonvirtual (taskModelList.Count) > 0 ? 1 : 0) : 0) != 0)
            {
              subTasks.Add(task);
              // ISSUE: explicit non-virtual call
              if (!await __nonvirtual (taskListView.GetToastParent()).BatchDeleteTask(subTasks))
              {
                item = (DisplayItemModel) null;
                return;
              }
            }
            else
            {
              // ISSUE: explicit non-virtual call
              __nonvirtual (taskListView.GetToastParent())?.TaskDeleted(taskId);
            }
          }
          item.SourceViewModel.Deleted = 1;
          // ISSUE: explicit non-virtual call
          __nonvirtual (taskListView.RemoveItemById(taskId));
          task = (TaskModel) null;
        }
        else if ((!await TaskService.InEmptyTask(taskId) ? 0 : (subTasks == null ? 1 : (subTasks.Count == 0 ? 1 : 0))) != 0)
        {
          bool focusFirst = taskListView.IsFirstTaskInList(taskId);
          if (type == TaskDeleteType.RemoveText && !focusFirst)
            taskListView.MoveUp(taskId);
          item.SourceViewModel.Deleted = 1;
          // ISSUE: explicit non-virtual call
          __nonvirtual (taskListView.RemoveItemById(taskId));
          await TaskService.DeleteTask(taskId, 2);
          if (focusFirst && type == TaskDeleteType.RemoveText)
            taskListView.TryFocusFirstItem();
        }
        subTasks = (List<TaskModel>) null;
      }
      if (!taskListView.ViewModel.InDetail)
        item = (DisplayItemModel) null;
      else if (taskListView.DisplayModels.Count != 0)
      {
        item = (DisplayItemModel) null;
      }
      else
      {
        await Task.Delay(100);
        EventHandler needReload = taskListView.NeedReload;
        if (needReload == null)
        {
          item = (DisplayItemModel) null;
        }
        else
        {
          needReload((object) taskListView, (EventArgs) null);
          item = (DisplayItemModel) null;
        }
      }
    }

    public void TryFocusItemById(string id)
    {
      DisplayItemModel displayItem = this.ViewModel.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.Id == id));
      if (displayItem == null)
        return;
      this.TryFocusItem(displayItem);
    }

    public void TryFocusItem(DisplayItemModel displayItem)
    {
      this.FocusTitle(displayItem, 100);
      this.SelectTaskOrItem(displayItem);
      displayItem.Selected = true;
    }

    public void TryFocusFirstItem()
    {
      foreach (DisplayItemModel displayModel in (Collection<DisplayItemModel>) this.DisplayModels)
      {
        if (displayModel.IsTaskOrNote || displayModel.IsItem)
        {
          this.SelectTaskOrItem(displayModel);
          this.FocusItemTitle(displayModel);
          break;
        }
      }
    }

    private bool IsFirstTaskInList(string taskId)
    {
      switch (this.GetCurrentIndex(taskId))
      {
        case 0:
          return true;
        case 1:
          if (this.DisplayModels[0].IsSection)
            return true;
          break;
      }
      return false;
    }

    private async Task DeleteAgenda(TaskModel task)
    {
      await TaskService.DeleteAgenda(task.id, task.projectId, task.attendId);
    }

    public async void DeleteSelectedTasks()
    {
      List<string> selectedTaskIds = this.GetSelectedTaskIds();
      UtilLog.Info("TaskList.DeleteTasks from:shortCut");
      if (this.ViewModel.ProjectIdentity is TrashProjectIdentity)
      {
        // ISSUE: explicit non-virtual call
        if (selectedTaskIds == null || __nonvirtual (selectedTaskIds.Count) <= 0)
          return;
        int num = await TaskService.BatchDeleteTaskByIds(selectedTaskIds, false, 2) ? 1 : 0;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        if (selectedTaskIds != null && __nonvirtual (selectedTaskIds.Count) > 1)
        {
          int num = await TaskService.BatchDeleteTaskByIds(selectedTaskIds) ? 1 : 0;
          this.NotifyBatchDeleted();
        }
        else
          this.TryDeleteTask(this.SelectedId, TaskDeleteType.DeleteButton);
      }
    }

    private void NotifyBatchDeleted()
    {
      this.ViewModel.ExitBatchSelect();
      this.LoadAsync(true);
    }

    public async void CompleteCheckitem(string itemId, bool playSound = true)
    {
      DisplayItemModel displayItemModel = this.DisplayModels.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (t => t.Id == itemId));
      if (displayItemModel != null && playSound && displayItemModel.Status != 1)
        Utils.PlayCompletionSound();
      await TaskDetailItemService.CompleteCheckItem(itemId, needUndo: true, window: this.GetToastParent());
    }

    public async Task TaskTitleChanged(string taskId, string text)
    {
      await TaskService.SaveTaskTitle(taskId, text);
      if (!(this.ViewModel.ProjectIdentity.SortOption.orderBy == Constants.SortType.modifiedTime.ToString()))
        ;
      else
      {
        DisplayItemModel model = this.DisplayModels.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (t => t.Id == taskId));
        if (model == null)
          ;
        else
          this.CheckSortByModifiedTime(model);
      }
    }

    public void EventTitleChanged(string eventId, string text)
    {
      string str;
      if (eventId == null)
        str = (string) null;
      else
        str = ((IEnumerable<string>) eventId.Split('_')).FirstOrDefault<string>();
      eventId = str;
      if (string.IsNullOrEmpty(eventId))
        return;
      CalendarService.SaveEventTitle(eventId, text);
    }

    public async void SubtaskTitleChanged(string taskId, string checkItemId, string text)
    {
      await TaskService.SetItemTitle(taskId, checkItemId, text);
    }

    private void MoveDown(string taskId) => this.MoveDown(taskId, string.Empty);

    private void MoveUp(string taskId) => this.MoveUp(taskId, string.Empty);

    public void MoveUp(string taskId, string itemId)
    {
      DisplayItemModel lastDisplayModel = this.GetLastDisplayModel(!string.IsNullOrEmpty(itemId) ? itemId : taskId);
      if (lastDisplayModel != null)
      {
        this.SelectTaskOrItem(lastDisplayModel);
        this.FocusItemTitle(lastDisplayModel);
      }
      else
      {
        EventHandler moveUpCaret = this.MoveUpCaret;
        if (moveUpCaret == null)
          return;
        moveUpCaret((object) this, (EventArgs) null);
      }
    }

    private DisplayItemModel GetLastDisplayModel(string id)
    {
      int currentIndex = this.GetCurrentIndex(id);
      if (currentIndex > 0 && currentIndex < this.ViewModel.Items.Count)
      {
        for (int index = currentIndex - 1; index >= 0; --index)
        {
          DisplayItemModel lastDisplayModel = this.ViewModel.Items[index];
          if (!lastDisplayModel.IsSection && !lastDisplayModel.IsLoadMore)
            return lastDisplayModel;
        }
      }
      return (DisplayItemModel) null;
    }

    public void MoveDown(string taskId, string itemId)
    {
      DisplayItemModel nextDisplayModel = this.GetNextDisplayModel(!string.IsNullOrEmpty(itemId) ? itemId : taskId);
      if (nextDisplayModel == null)
        return;
      this.SelectTaskOrItem(nextDisplayModel);
      this.FocusItemTitle(nextDisplayModel);
    }

    private DisplayItemModel GetNextDisplayModel(string id)
    {
      int currentIndex = this.GetCurrentIndex(id);
      if (currentIndex >= 0 && currentIndex < this.ViewModel.Items.Count)
      {
        for (int index = currentIndex + 1; index < this.ViewModel.Items.Count; ++index)
        {
          DisplayItemModel nextDisplayModel = this.ViewModel.Items[index];
          if (!nextDisplayModel.IsSection && !nextDisplayModel.IsLoadMore)
            return nextDisplayModel;
        }
      }
      return (DisplayItemModel) null;
    }

    private DisplayItemModel GetModelById(string id)
    {
      return this.DisplayModels.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.Id == id));
    }

    public void TryFocusSelectedItem()
    {
      DisplayItemModel selectedModel = this.GetSelectedModel();
      if (selectedModel == null)
        return;
      this.FocusItemTitle(selectedModel);
    }

    private void TryFocusAndSelectModelById(string id)
    {
      DisplayItemModel model = this.ViewModel.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (t => t.Id == id));
      if (model == null)
        return;
      this.FocusItemTitle(model);
    }

    public ListViewItem GetItemByModel(DisplayItemModel model)
    {
      return this._taskList.ItemContainerGenerator.ContainerFromItem((object) model) as ListViewItem;
    }

    private TaskTitleBox GetTaskTitleTextBox(DisplayItemModel model)
    {
      return Utils.GetDescendantByType((Visual) this._taskList.ItemContainerGenerator.ContainerFromItem((object) model), typeof (TaskTitleBox), "EditBox") as TaskTitleBox;
    }

    private async void FocusItemTitle(DisplayItemModel model)
    {
      this._taskList.ScrollIntoView((object) model);
      TaskTitleBox titleTextbox = this.GetTaskTitleTextBox(model);
      if (titleTextbox == null)
        titleTextbox = (TaskTitleBox) null;
      else if (titleTextbox.KeyboardFocused)
      {
        titleTextbox = (TaskTitleBox) null;
      }
      else
      {
        titleTextbox.Select(titleTextbox.Text.Length, 0);
        titleTextbox.Focus();
        await Task.Delay(10);
        titleTextbox.CaretOffset = this._caretIndex >= titleTextbox.Text.Length || this._caretIndex < 0 ? titleTextbox.Text.Length : this._caretIndex;
        titleTextbox.ScrollToHorizontalOffset(10000.0);
        titleTextbox = (TaskTitleBox) null;
      }
    }

    public async Task SplitDisplayItem(string id)
    {
      DisplayItemModel model;
      if (this._splitting)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        this._splitting = true;
        int currentIndex = this.GetCurrentIndex(id);
        model = this.GetModelById(id);
        if (model != null && TaskListItemFocusHelper.FocusingItem != null && TaskListItemFocusHelper.FocusingItem.TitleTextBox.ParsingDate)
        {
          TaskListItemFocusHelper.FocusingItem.RemoveFocus();
          await Task.Delay(300);
          this.TryFocusAndSelectModelById(model.Id);
          this._splitting = false;
          model = (DisplayItemModel) null;
        }
        else
        {
          if (model != null && !model.IsNote)
          {
            if (this.ViewModel.ProjectIdentity is BindAccountCalendarProjectIdentity)
              await this.SplitNewEvent(id, model, currentIndex);
            else if (Utils.IfShiftPressed() && model.IsTask && !string.IsNullOrEmpty(model.Id) && TaskDao.GetTaskLevel(model.Id, model.ProjectId) < 4)
              await this.CreateSubTask(model);
            else
              await this.SplitNewTask(id, model, currentIndex);
          }
          this._splitting = false;
          model = (DisplayItemModel) null;
        }
      }
    }

    private async Task SplitNewEvent(string eventId, DisplayItemModel model, int index)
    {
      string str1 = model.CalendarType == 5 ? Guid.NewGuid().ToString() : Utils.GetGuid();
      string str2 = str1 + "@" + model.CalendarId;
      TaskBaseViewModel vm = new TaskBaseViewModel()
      {
        Type = DisplayType.Event,
        Id = str2,
        IsAllDay = new bool?(true),
        Kind = "TEXT",
        Title = string.Empty,
        ProjectName = string.Empty,
        CreatedTime = new DateTime?(DateTime.Now),
        ModifiedTime = new DateTime?(DateTime.Now)
      };
      Section section = this.GetSection(eventId);
      if (!section.GetStartDate().HasValue)
        return;
      string calendarId = model.CalendarId;
      BindCalendarModel calendar = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == calendarId));
      if (calendar != null)
      {
        BindCalendarAccountModel calendarAccountModel = CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (acc => acc.Id == calendar.AccountId));
        if (calendarAccountModel != null && calendarAccountModel.Calendars.Any<BindCalendarModel>())
          calendarId = calendarAccountModel.Calendars[0].Id;
      }
      DateTime? startDate = section.GetStartDate();
      if (startDate.HasValue)
      {
        double num = DateTimeOffset.Now.Offset.TotalMinutes;
        if (num < 0.0)
          num = 0.0;
        ref DateTime? local = ref startDate;
        DateTime date = startDate.Value.Date;
        DateTime dateTime = date.AddMinutes(num);
        local = new DateTime?(dateTime);
        vm.StartDate = startDate;
        TaskBaseViewModel taskBaseViewModel = vm;
        date = startDate.Value;
        DateTime? nullable = new DateTime?(date.AddDays(1.0));
        taskBaseViewModel.DueDate = nullable;
      }
      vm.CalendarId = calendarId;
      vm.ProjectName = string.Empty;
      DisplayItemModel displayItemModel = new DisplayItemModel(vm)
      {
        ShowReminder = new bool?(true)
      };
      this.ViewModel.Items.Insert(index + 1, displayItemModel);
      this.ViewModel.SourceModels.Add(model.SourceViewModel);
      this.FocusTitle(displayItemModel, 100);
      this.SelectTaskOrItem(displayItemModel);
      displayItemModel.Selected = true;
      CalendarEventModel calEvent = new CalendarEventModel()
      {
        Id = str2,
        UserId = Utils.GetCurrentUserIdInt().ToString(),
        EventId = str1,
        Uid = str1,
        Title = string.Empty,
        CalendarId = vm.CalendarId,
        Type = 0,
        DueStart = vm.StartDate,
        DueEnd = vm.DueDate,
        IsAllDay = true
      };
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      if (!string.IsNullOrEmpty(defaultSafely.AllDayReminders))
        calEvent.Reminders = JsonConvert.SerializeObject((object) ((IEnumerable<string>) defaultSafely.AllDayReminders.Split(',')).Select<string, int>(new Func<string, int>(TriggerUtils.TriggerToReminder)));
      await CalendarService.AddEvent(calEvent);
    }

    private async Task SplitNewTask(string taskId, DisplayItemModel task, int index)
    {
      TaskListView sender = this;
      bool flag = string.IsNullOrEmpty(task.Title);
      if (flag)
        flag = await TaskService.InEmptyTask(taskId);
      TaskBaseViewModel newTask;
      ProjectModel project;
      string parentId;
      DisplayItemModel displayModel;
      TaskModel taskModel;
      if (flag)
      {
        newTask = (TaskBaseViewModel) null;
        project = (ProjectModel) null;
        parentId = (string) null;
        displayModel = (DisplayItemModel) null;
        taskModel = (TaskModel) null;
      }
      else
      {
        switch (sender.ViewModel.ProjectIdentity)
        {
          case CompletedProjectIdentity _:
            newTask = (TaskBaseViewModel) null;
            project = (ProjectModel) null;
            parentId = (string) null;
            displayModel = (DisplayItemModel) null;
            taskModel = (TaskModel) null;
            break;
          case AbandonedProjectIdentity _:
            newTask = (TaskBaseViewModel) null;
            project = (ProjectModel) null;
            parentId = (string) null;
            displayModel = (DisplayItemModel) null;
            taskModel = (TaskModel) null;
            break;
          case TrashProjectIdentity _:
            newTask = (TaskBaseViewModel) null;
            project = (ProjectModel) null;
            parentId = (string) null;
            displayModel = (DisplayItemModel) null;
            taskModel = (TaskModel) null;
            break;
          case AssignToMeProjectIdentity _:
            newTask = (TaskBaseViewModel) null;
            project = (ProjectModel) null;
            parentId = (string) null;
            displayModel = (DisplayItemModel) null;
            taskModel = (TaskModel) null;
            break;
          case SearchProjectIdentity _:
            newTask = (TaskBaseViewModel) null;
            project = (ProjectModel) null;
            parentId = (string) null;
            displayModel = (DisplayItemModel) null;
            taskModel = (TaskModel) null;
            break;
          default:
            UserActCollectUtils.AddShortCutEvent(nameof (task), "add_task_below");
            newTask = new TaskBaseViewModel()
            {
              Type = DisplayType.Task,
              Id = Utils.GetGuid(),
              IsAllDay = task.IsAllDay,
              Kind = "TEXT",
              Title = string.Empty,
              ColumnId = task.ColumnId,
              PinnedTime = task.IsPinned ? Utils.GetNowTimeStamp() : 0L
            };
            Section section = task.Section;
            string projectId = section?.GetProjectId();
            if (!string.IsNullOrEmpty(task.ParentId) && task.Level > 0 || sender.ViewModel.SortOption.groupBy == "assignee")
            {
              projectId = task.ProjectId;
            }
            else
            {
              switch (sender.ViewModel.ProjectIdentity)
              {
                case FilterProjectIdentity _:
                  projectId = task.ProjectId;
                  break;
                case NormalProjectIdentity _:
                  projectId = sender.ViewModel.ProjectIdentity.Id;
                  break;
                case GroupProjectIdentity groupProjectIdentity:
                  if (string.IsNullOrEmpty(projectId))
                  {
                    projectId = groupProjectIdentity.GetProjectId();
                    break;
                  }
                  break;
                case ParentTaskIdentity _:
                  projectId = task.ProjectId;
                  break;
              }
              if (string.IsNullOrEmpty(projectId))
                projectId = TaskDefaultDao.GetDefaultSafely().ProjectId;
            }
            project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
            if (project == null)
            {
              newTask = (TaskBaseViewModel) null;
              project = (ProjectModel) null;
              parentId = (string) null;
              displayModel = (DisplayItemModel) null;
              taskModel = (TaskModel) null;
              break;
            }
            if (await ProChecker.CheckTaskLimit(projectId))
            {
              newTask = (TaskBaseViewModel) null;
              project = (ProjectModel) null;
              parentId = (string) null;
              displayModel = (DisplayItemModel) null;
              taskModel = (TaskModel) null;
              break;
            }
            int level = 0;
            FilterTaskDefault taskDefault = level > 0 ? FilterViewModel.CalculateTaskDefault(MatrixManager.GetQuadrantByLevel(level).rule) : (FilterTaskDefault) null;
            DateTime? nullable1 = (DateTime?) section?.GetStartDate();
            if (!nullable1.HasValue)
            {
              if (taskDefault != null && taskDefault.DefaultDate.HasValue)
              {
                nullable1 = taskDefault.DefaultDate;
              }
              else
              {
                TimeData timeData = sender.ViewModel.ProjectIdentity.GetTimeData();
                if (timeData != null && (!timeData.IsDefault || !task.IsNote))
                  nullable1 = timeData.StartDate;
              }
            }
            DateTime? nullable2 = nullable1;
            DateTime? date = nullable2 ?? (task.IsNote ? new DateTime?() : TaskDefaultDao.GetDefaultSafely().GetDefaultDateTime());
            if (Utils.IsEmptyDate(date))
              date = new DateTime?();
            newTask.StartDate = date;
            newTask.IsAllDay = new bool?(true);
            TaskBaseViewModel taskBaseViewModel1 = newTask;
            nullable2 = new DateTime?();
            DateTime? nullable3 = nullable2;
            taskBaseViewModel1.DueDate = nullable3;
            TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
            if (sender.ViewModel.ProjectIdentity is FilterProjectIdentity projectIdentity3)
            {
              newTask.SetTags(projectIdentity3.GetTags());
              newTask.Priority = projectIdentity3.GetPriority();
            }
            else if (sender.ViewModel.ProjectIdentity is MatrixQuadrantIdentity projectIdentity2)
            {
              newTask.SetTags(projectIdentity2.GetTags());
              newTask.Priority = projectIdentity2.GetPriority();
            }
            else if (sender.ViewModel.ProjectIdentity is TagProjectIdentity projectIdentity1)
              newTask.SetTags(projectIdentity1.GetTags());
            else if (taskDefault != null)
            {
              newTask.SetTags(taskDefault.DefaultTags);
              newTask.Priority = taskDefault.Priority ?? defaultSafely.Priority;
            }
            else
              newTask.Priority = defaultSafely.Priority;
            if (!(section is TagSection tagSection))
            {
              if (section is NoPrioritySection || section is LowPrioritySection || section is MediumPrioritySection || section is HighPrioritySection)
                newTask.Priority = section.GetPriority();
            }
            else
            {
              TaskBaseViewModel taskBaseViewModel2 = newTask;
              List<string> tags;
              if (!(tagSection.Name == Utils.GetString("NoTags")))
                tags = new List<string>()
                {
                  tagSection.Name
                };
              else
                tags = (List<string>) null;
              taskBaseViewModel2.SetTags(tags);
            }
            if (task.IsNote)
            {
              newTask.Kind = "NOTE";
              newTask.Priority = 0;
            }
            newTask.ProjectId = !string.IsNullOrEmpty(task.ParentId) ? task.ProjectId : project.id;
            newTask.ProjectName = project.name;
            newTask.ProjectOrder = project.sortOrder;
            newTask.Assignee = section?.GetAssignee();
            newTask.Color = project.color;
            parentId = task.Level != 0 || sender.ViewModel.InDetail ? task.ParentId : "";
            newTask.ParentId = "";
            TaskBaseViewModel taskBaseViewModel3 = newTask;
            Section section1 = section;
            int taskStatus = section1 != null ? section1.GetTaskStatus() : 0;
            taskBaseViewModel3.Status = taskStatus;
            long specialSortOrder = task.SpecialOrder;
            newTask.SortOrder = ProjectSortOrderDao.GetNextTaskSortOrderInProject(task.ProjectId, task.SortOrder, task.ParentId);
            if (newTask.IsPinned)
            {
              string groupId = newTask.ProjectId;
              if (sender.ViewModel.ProjectIdentity != null)
                groupId = sender.ViewModel.ProjectIdentity.CatId;
              SyncSortOrderModel syncSortOrderModel = await TaskSortOrderService.InsertAfterAsync("taskPinned", groupId, newTask.Id, columnId: task.ColumnId, targetId: task.TaskId);
            }
            else if (sender.ViewModel.ProjectIdentity.SortOption.orderBy != "sortOrder")
            {
              string currentProject = sender.ViewModel.ProjectIdentity.GetSortProjectId();
              string sortTypeKey = sender.ViewModel.ProjectIdentity.SortOption.GetSortKey();
              if (!string.IsNullOrEmpty(sortTypeKey) && !string.IsNullOrEmpty(section?.SectionEntityId))
                sortTypeKey = string.Format(sortTypeKey, (object) section.SectionEntityId);
              if (!string.IsNullOrEmpty(section?.SectionEntityId))
              {
                long specialOrder = task.SpecialOrder;
                if (specialOrder == long.MaxValue || specialOrder == long.MinValue)
                {
                  await TaskSortOrderService.BatchResetOrders(sender.GetSiblings(task.Section), sortTypeKey, currentProject);
                  specialOrder = task.SpecialOrder;
                }
                if (specialOrder != long.MaxValue && specialOrder != long.MinValue)
                {
                  DisplayItemModel displayModel1 = sender.DisplayModels.Count > index + 1 ? sender.DisplayModels[index + 1] : (DisplayItemModel) null;
                  long num1 = displayModel1 == null || displayModel1.IsSection ? long.MaxValue : displayModel1.SpecialOrder;
                  long num2;
                  switch (num1)
                  {
                    case long.MinValue:
                    case long.MaxValue:
                      num2 = specialOrder + 268435456L;
                      break;
                    default:
                      num2 = specialOrder + (num1 - specialOrder) / 2L;
                      break;
                  }
                  specialSortOrder = num2;
                  SyncSortOrderModel syncSortOrderModel = await TaskSortOrderService.InsertOrUpdateAsync(sortTypeKey, currentProject, newTask.Id, sortOrder: new long?(specialSortOrder));
                }
              }
              currentProject = (string) null;
              sortTypeKey = (string) null;
            }
            if (!string.IsNullOrEmpty(parentId))
            {
              TaskModel thinTaskById = await TaskDao.GetThinTaskById(parentId);
              if (thinTaskById != null && thinTaskById.projectId == newTask.ProjectId)
              {
                newTask.ParentId = thinTaskById.id;
                newTask.Status = thinTaskById.status;
              }
            }
            if (newTask.Status != 0)
            {
              newTask.CompletedTime = new DateTime?(DateTime.Now);
              newTask.CompletedUser = LocalSettings.Settings.LoginUserId;
            }
            TaskCache.AddToDict(newTask);
            displayModel = new DisplayItemModel(newTask, !(sender.ViewModel.ProjectIdentity is NormalProjectIdentity) && !(sender.ViewModel.ProjectIdentity is ParentTaskIdentity));
            displayModel.Level = task.Level;
            displayModel.InDetail = task.InDetail;
            displayModel.InSticky = task.InSticky;
            displayModel.ShowDragBar = task.ShowDragBar;
            displayModel.Section = section;
            displayModel.IsNewAdd = true;
            displayModel.SpecialOrder = specialSortOrder;
            taskModel = displayModel.GetTaskModel();
            taskModel.createdTime = new DateTime?(DateTime.Now);
            taskModel.modifiedTime = new DateTime?(DateTime.Now);
            int index1 = index + 1;
            while (index1 < sender.ViewModel.Items.Count && sender.ViewModel.Items[index1].Level > task.Level)
              ++index1;
            sender.ViewModel.Items.Insert(index1, displayModel);
            sender.ViewModel.SourceModels.Add(displayModel.SourceViewModel);
            EventHandler itemsCountChanged = sender.ItemsCountChanged;
            if (itemsCountChanged != null)
              itemsCountChanged((object) sender, (EventArgs) null);
            task.Parent?.AddChild(displayModel, task);
            displayModel.Parent = task.Parent;
            if (section != null)
            {
              DisplayItemModel lastItem = CollectionUtils.GetLastItem<DisplayItemModel>((Collection<DisplayItemModel>) sender.ViewModel.Items, index, (Func<DisplayItemModel, bool>) (item => item.IsSection && item.Section.SectionId == section.SectionId));
              if (lastItem != null)
                ++lastItem.Num;
            }
            sender.SelectedId = displayModel.Id;
            displayModel.Selected = true;
            task.Selected = false;
            UtilLog.Info("TaskList.AddTask " + taskModel.id + " from:splitNewTask");
            TaskModel taskModel1 = await TaskService.AddTask(taskModel, sender: (object) sender);
            await sender.TryAddTaskOrderBySortType(task, displayModel);
            TaskListItemFocusHelper.ParseDateId = displayModel.Id;
            sender.NotifyItemSelect(new ListItemSelectModel(taskModel.id, (string) null, DisplayType.Task, TaskSelectType.Click));
            sender.UpdateLayout();
            sender.TryFocusAndSelectModelById(displayModel.Id);
            nullable2 = taskModel.startDate;
            if (nullable2.HasValue && !task.IsNote)
            {
              foreach (TaskReminderModel defaultAllDayReminder in TimeData.GetDefaultAllDayReminders())
              {
                defaultAllDayReminder.taskserverid = taskModel.id;
                int num = await TaskReminderDao.SaveReminders(defaultAllDayReminder);
                TaskItemLoadHelper.LoadShowReminder(displayModel);
              }
            }
            DisplayItemModel displayItemModel = displayModel;
            displayItemModel.AvatarUrl = await AvatarHelper.GetAvatarUrl(section?.GetAssignee(), newTask.ProjectId);
            displayItemModel = (DisplayItemModel) null;
            newTask = (TaskBaseViewModel) null;
            project = (ProjectModel) null;
            parentId = (string) null;
            displayModel = (DisplayItemModel) null;
            taskModel = (TaskModel) null;
            break;
        }
      }
    }

    public async Task CreateSubTask(DisplayItemModel model, bool addToLastOne = false)
    {
      TaskListView taskListView = this;
      ProjectIdentity identity = taskListView.ViewModel.ProjectIdentity;
      DisplayItemModel task;
      TaskBaseViewModel newTask;
      DisplayItemModel displayModel;
      TaskModel taskModel;
      switch (identity)
      {
        case CompletedProjectIdentity _:
          identity = (ProjectIdentity) null;
          task = (DisplayItemModel) null;
          newTask = (TaskBaseViewModel) null;
          displayModel = (DisplayItemModel) null;
          taskModel = (TaskModel) null;
          break;
        case AbandonedProjectIdentity _:
          identity = (ProjectIdentity) null;
          task = (DisplayItemModel) null;
          newTask = (TaskBaseViewModel) null;
          displayModel = (DisplayItemModel) null;
          taskModel = (TaskModel) null;
          break;
        case TrashProjectIdentity _:
          identity = (ProjectIdentity) null;
          task = (DisplayItemModel) null;
          newTask = (TaskBaseViewModel) null;
          displayModel = (DisplayItemModel) null;
          taskModel = (TaskModel) null;
          break;
        case AssignToMeProjectIdentity _:
          identity = (ProjectIdentity) null;
          task = (DisplayItemModel) null;
          newTask = (TaskBaseViewModel) null;
          displayModel = (DisplayItemModel) null;
          taskModel = (TaskModel) null;
          break;
        case SearchProjectIdentity _:
          identity = (ProjectIdentity) null;
          task = (DisplayItemModel) null;
          newTask = (TaskBaseViewModel) null;
          displayModel = (DisplayItemModel) null;
          taskModel = (TaskModel) null;
          break;
        default:
          if (!model.IsOpen)
          {
            // ISSUE: explicit non-virtual call
            await __nonvirtual (taskListView.OnTaskOpenClick(model));
          }
          if (await ProChecker.CheckTaskLimit(model.ProjectId))
          {
            identity = (ProjectIdentity) null;
            task = (DisplayItemModel) null;
            newTask = (TaskBaseViewModel) null;
            displayModel = (DisplayItemModel) null;
            taskModel = (TaskModel) null;
            break;
          }
          int index = taskListView.GetCurrentIndex(model.TaskId);
          if (addToLastOne)
          {
            ++index;
            while (index < taskListView.DisplayModels.Count && taskListView.DisplayModels[index].Level > model.Level)
              ++index;
          }
          UserActCollectUtils.AddShortCutEvent("task", "add_subtask");
          task = taskListView.GetModelById(model.TaskId);
          newTask = new TaskBaseViewModel()
          {
            Type = DisplayType.Task,
            Id = Utils.GetGuid(),
            IsAllDay = task.IsAllDay,
            Kind = "TEXT",
            Title = string.Empty,
            ParentId = model.TaskId,
            ProjectId = model.ProjectId,
            ProjectName = model.ProjectName,
            ProjectOrder = model.ProjectOrder,
            Color = model.Color,
            SortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(model.ProjectId, task.Id, new bool?(!addToLastOne)),
            Status = model.Status,
            ColumnId = model.ColumnId,
            PinnedTime = model.IsPinned ? Utils.GetNowTimeStamp() : 0L,
            CreatedTime = new DateTime?(DateTime.Now),
            ModifiedTime = new DateTime?(DateTime.Now)
          };
          Section section = model.Section;
          int level = 0;
          FilterTaskDefault taskDefault = level > 0 ? FilterViewModel.CalculateTaskDefault(MatrixManager.GetQuadrantByLevel(level).rule) : (FilterTaskDefault) null;
          newTask.StartDate = ((DateTime?) section?.GetStartDate() ?? (taskDefault != null ? taskDefault.DefaultDate : identity.GetTimeData()?.StartDate)) ?? TaskDefaultDao.GetDefaultSafely().GetDefaultDateTime();
          newTask.IsAllDay = new bool?(true);
          newTask.DueDate = new DateTime?();
          switch (identity)
          {
            case FilterProjectIdentity filterProjectIdentity:
              newTask.SetTags(filterProjectIdentity.GetTags());
              newTask.Priority = filterProjectIdentity.GetPriority();
              break;
            case MatrixQuadrantIdentity quadrantIdentity:
              newTask.SetTags(quadrantIdentity.GetTags());
              newTask.Priority = quadrantIdentity.GetPriority();
              break;
            case TagProjectIdentity tagProjectIdentity1:
              newTask.SetTags(tagProjectIdentity1.GetTags());
              break;
            default:
              if (taskDefault != null)
              {
                newTask.SetTags(taskDefault.DefaultTags);
                newTask.Priority = taskDefault.Priority ?? TaskDefaultDao.GetDefaultSafely().Priority;
                break;
              }
              newTask.Priority = TaskDefaultDao.GetDefaultSafely().Priority;
              break;
          }
          if (!(section is TagSection tagSection))
          {
            if (section is NoPrioritySection || section is LowPrioritySection || section is MediumPrioritySection || section is HighPrioritySection)
              newTask.Priority = section.GetPriority();
          }
          else
          {
            TaskBaseViewModel taskBaseViewModel = newTask;
            List<string> tags;
            if (!(tagSection.Name == Utils.GetString("NoTags")))
              tags = new List<string>() { tagSection.Name };
            else
              tags = (List<string>) null;
            taskBaseViewModel.SetTags(tags);
          }
          if (string.IsNullOrEmpty(newTask.Tag) && identity is TagProjectIdentity tagProjectIdentity2)
            newTask.SetTags(new List<string>()
            {
              tagProjectIdentity2.Tag
            });
          newTask.Assignee = section?.GetAssignee();
          newTask.Status = model.Status;
          if (newTask.Status != 0)
          {
            newTask.CompletedTime = new DateTime?(DateTime.Now);
            newTask.CompletedUser = LocalSettings.Settings.LoginUserId;
          }
          if (taskListView.ViewModel.InDetail)
          {
            TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
            newTask.StartDate = defaultSafely.GetDefaultDateTime();
            newTask.Priority = defaultSafely.Priority;
          }
          TaskCache.AddToDict(newTask);
          displayModel = new DisplayItemModel(newTask, !(identity is NormalProjectIdentity) && !(identity is ParentTaskIdentity));
          displayModel.Section = section;
          displayModel.Level = task.Level + 1;
          displayModel.InDetail = task.InDetail;
          displayModel.InSticky = task.InSticky;
          displayModel.ShowDragBar = task.ShowDragBar;
          displayModel.InMatrix = taskListView.ViewModel.InMatrix;
          displayModel.InKanban = taskListView.ViewModel.InKanban;
          displayModel.ShowProject = task.ShowProject;
          task.AddChild(displayModel, (DisplayItemModel) null);
          if (taskListView.ViewModel.InKanban)
          {
            model.ShowBottomMargin = false;
            displayModel.ShowTopMargin = false;
          }
          taskModel = displayModel.GetTaskModel();
          taskModel.createdTime = new DateTime?(DateTime.Now);
          taskModel.modifiedTime = new DateTime?(DateTime.Now);
          UtilLog.Info("TaskList.AddTask " + taskModel.id + " from:" + (addToLastOne ? "CreatSubTask" : "SplitSubTask"));
          TaskModel taskModel1 = await TaskService.AddTask(taskModel, sender: (object) taskListView);
          TaskListItemFocusHelper.ParseDateId = displayModel.Id;
          taskListView.NotifyItemSelect(new ListItemSelectModel(taskModel.id, (string) null, DisplayType.Task, TaskSelectType.Click));
          if (section != null)
          {
            DisplayItemModel lastItem = CollectionUtils.GetLastItem<DisplayItemModel>((Collection<DisplayItemModel>) taskListView.DisplayModels, index, (Func<DisplayItemModel, bool>) (item => item.IsSection && item.Section == section));
            if (lastItem != null)
              ++lastItem.Num;
          }
          displayModel.IsNewAdd = !taskListView.ViewModel.InKanban;
          taskListView.DisplayModels.Insert(addToLastOne ? index : index + 1, displayModel);
          taskListView.ViewModel.SourceModels.Add(displayModel.SourceViewModel);
          displayModel.Selected = true;
          taskListView.SelectedId = displayModel.Id;
          task.Selected = false;
          displayModel.Parent = task;
          taskListView.UpdateLayout();
          EventHandler itemsCountChanged = taskListView.ItemsCountChanged;
          if (itemsCountChanged != null)
            itemsCountChanged((object) taskListView, (EventArgs) null);
          if (taskListView.ViewModel.InKanban)
            Utils.FindParent<KanbanColumnView>((DependencyObject) taskListView)?.TryShowTaskDetail(taskModel.id, true);
          else if (taskListView.ViewModel.InMatrix)
            Utils.FindSingleVisualChildren<TaskListItem>(taskListView._taskList.ItemContainerGenerator.ContainerFromItem((object) displayModel))?.ShowDetailWindow(true);
          else
            taskListView.TryFocusAndSelectModelById(displayModel.Id);
          if (taskModel.startDate.HasValue)
          {
            foreach (TaskReminderModel defaultAllDayReminder in TimeData.GetDefaultAllDayReminders())
            {
              defaultAllDayReminder.taskserverid = taskModel.id;
              int num = await TaskReminderDao.SaveReminders(defaultAllDayReminder);
              TaskItemLoadHelper.LoadShowReminder(displayModel);
            }
          }
          if (LocalSettings.Settings.ShowDetails)
            model.CompletionRate = TaskCompletionRateDao.GetRateStrByIdInDb(model.TaskId, model.ProjectId);
          DisplayItemModel displayItemModel = displayModel;
          displayItemModel.AvatarUrl = await AvatarHelper.GetAvatarUrl(section?.GetAssignee(), newTask.ProjectId);
          displayItemModel = (DisplayItemModel) null;
          identity = (ProjectIdentity) null;
          task = (DisplayItemModel) null;
          newTask = (TaskBaseViewModel) null;
          displayModel = (DisplayItemModel) null;
          taskModel = (TaskModel) null;
          break;
      }
    }

    private async Task TryAddTaskOrderBySortType(
      DisplayItemModel frontTask,
      DisplayItemModel insertTask)
    {
      if (frontTask == null || insertTask == null || !this.ViewModel.ProjectIdentity.SortOption.SpecialSort() || frontTask.SpecialOrder == long.MaxValue || frontTask.SpecialOrder == long.MinValue || insertTask.Level > 0)
        return;
      await TaskSortOrderService.NewTaskAdded(insertTask.Id, this.ViewModel.ProjectIdentity.CatId, this.ViewModel.ProjectIdentity.SortOption, frontTask.Id);
    }

    public async void AddNewSection(DisplayItemModel sectionItem, bool below)
    {
      TaskListView taskListView = this;
      if (sectionItem == null)
        return;
      CustomizedSection customizedSection1 = new CustomizedSection();
      customizedSection1.SectionId = Utils.GetGuid();
      customizedSection1.Customized = true;
      customizedSection1.Name = "";
      customizedSection1.ProjectId = sectionItem.ProjectId;
      CustomizedSection customizedSection2 = customizedSection1;
      ObservableCollection<DisplayItemModel> displayModels = taskListView.DisplayModels;
      List<DisplayItemModel> list1 = displayModels != null ? displayModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.IsCustomizedSection)).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      if (list1 == null || !list1.Contains(sectionItem))
        return;
      List<long> list2 = list1.Select<DisplayItemModel, long>((Func<DisplayItemModel, long>) (s => s.Section.Ordinal)).ToList<long>();
      if ((long) list2.Count >= LimitCache.GetLimitByKey(Constants.LimitKind.KanbanNumber))
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("ExceedMaxSectionCountMessage"), MessageBoxButton.OK);
        customerDialog.Owner = Window.GetWindow((DependencyObject) taskListView);
        customerDialog.ShowDialog();
      }
      else
      {
        list2.Sort();
        int num1 = list2.IndexOf(sectionItem.Section.Ordinal);
        if (below)
          customizedSection2.Ordinal = num1 < list2.Count - 1 ? (list2[num1 + 1] + sectionItem.Section.Ordinal) / 2L : sectionItem.Section.Ordinal + 268435456L;
        else
          customizedSection2.Ordinal = num1 > 0 ? (list2[num1 - 1] + sectionItem.Section.Ordinal) / 2L : sectionItem.Section.Ordinal - 268435456L;
        int index = taskListView.DisplayModels.IndexOf(sectionItem);
        DisplayItemModel displayItemModel = DisplayItemModel.BuildSection((Section) customizedSection2);
        displayItemModel.NewAdd = true;
        if (below)
        {
          int num2 = index + 1;
          while (num2 < taskListView.DisplayModels.Count && !taskListView.DisplayModels[num2].IsSection)
            ++num2;
          if (num2 > index)
            taskListView.DisplayModels.Insert(Math.Min(num2, taskListView.DisplayModels.Count), displayItemModel);
        }
        else
          taskListView.DisplayModels.Insert(index, displayItemModel);
        taskListView._taskList.ScrollIntoView((object) displayItemModel);
      }
    }

    public async void AddNewSectionAtLast()
    {
      TaskListView taskListView = this;
      if (!(taskListView.ViewModel.ProjectIdentity is NormalProjectIdentity projectIdentity) || projectIdentity.Project == null)
        return;
      string projectId = projectIdentity.Project.id;
      List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(projectId);
      if (columnsByProjectId == null || !columnsByProjectId.Any<ColumnModel>())
      {
        ProjectModel projectById = await ProjectDao.GetProjectById(projectId);
        if (projectById != null)
        {
          if (projectById.sync_status != Constants.SyncStatus.SYNC_NEW.ToString())
            projectById.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
          projectById.viewMode = "list";
          int num1 = await ProjectDao.TryUpdateProject(projectById);
          int num2 = await ColumnDao.TryInitColumns(projectId) ? 1 : 0;
        }
        columnsByProjectId = await ColumnDao.GetColumnsByProjectId(projectId);
      }
      if ((long) columnsByProjectId.Count >= LimitCache.GetLimitByKey(Constants.LimitKind.KanbanNumber))
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("ExceedMaxSectionCountMessage"), MessageBoxButton.OK);
        customerDialog.Owner = Window.GetWindow((DependencyObject) taskListView);
        customerDialog.ShowDialog();
      }
      else
      {
        // ISSUE: explicit non-virtual call
        if (taskListView.DisplayModels.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Section is CustomizedSection)) == null && columnsByProjectId != null && __nonvirtual (columnsByProjectId.Count) > 0)
        {
          CustomizedSection customizedSection = new CustomizedSection(columnsByProjectId[0]);
          List<DisplayItemModel> list = taskListView.DisplayModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.IsTaskOrNote && m.Section == null && m.Level == 0)).ToList<DisplayItemModel>();
          foreach (DisplayItemModel displayItemModel in list)
            displayItemModel.Section = (Section) customizedSection;
          customizedSection.Children = list;
          DisplayItemModel displayItemModel1 = DisplayItemModel.BuildSection((Section) customizedSection);
          taskListView.DisplayModels.Insert(0, displayItemModel1);
        }
        CustomizedSection customizedSection1 = new CustomizedSection();
        customizedSection1.SectionId = Utils.GetGuid();
        customizedSection1.Customized = true;
        customizedSection1.Name = "";
        customizedSection1.ProjectId = projectId;
        long? nullable = columnsByProjectId.Select<ColumnModel, long?>((Func<ColumnModel, long?>) (c => c.sortOrder)).Max();
        customizedSection1.Ordinal = (nullable.HasValue ? new long?(nullable.GetValueOrDefault() + 268435456L) : new long?()).GetValueOrDefault();
        DisplayItemModel displayItemModel2 = DisplayItemModel.BuildSection((Section) customizedSection1);
        displayItemModel2.NewAdd = true;
        int num;
        for (num = taskListView.DisplayModels.Count - 1; num >= 0; --num)
        {
          DisplayItemModel displayModel = taskListView.DisplayModels[num];
          if (!(displayModel.Section is CompletedSection) && !(displayModel.Section is NoteSection))
          {
            ++num;
            break;
          }
        }
        taskListView.DisplayModels.Insert(Math.Max(0, Math.Min(taskListView.DisplayModels.Count, num)), displayItemModel2);
        try
        {
          taskListView._taskList.ScrollIntoView((object) displayItemModel2);
        }
        catch (Exception ex)
        {
        }
        projectId = (string) null;
      }
    }

    public async void AddTaskInSection(DisplayItemModel model)
    {
      TaskListView sender = this;
      TaskBaseViewModel newTask;
      DisplayItemModel displayModel;
      TaskModel taskModel;
      if (model?.Section == null)
      {
        newTask = (TaskBaseViewModel) null;
        displayModel = (DisplayItemModel) null;
        taskModel = (TaskModel) null;
      }
      else
      {
        newTask = new TaskBaseViewModel()
        {
          Id = Utils.GetGuid(),
          Kind = sender.ViewModel.ProjectIdentity.IsNote ? "NOTE" : "TEXT",
          Title = string.Empty,
          CreatedTime = new DateTime?(DateTime.Now),
          ModifiedTime = new DateTime?(DateTime.Now)
        };
        string projectId = model.ProjectId;
        if (await ProChecker.CheckTaskLimit(projectId))
        {
          newTask = (TaskBaseViewModel) null;
          displayModel = (DisplayItemModel) null;
          taskModel = (TaskModel) null;
        }
        else
        {
          newTask.StartDate = newTask.IsNote ? new DateTime?() : TaskDefaultDao.GetDefaultSafely().GetDefaultDateTime();
          newTask.IsAllDay = new bool?(true);
          newTask.DueDate = new DateTime?();
          newTask.Priority = TaskDefaultDao.GetDefaultSafely().Priority;
          if (CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId)) == null)
          {
            newTask = (TaskBaseViewModel) null;
            displayModel = (DisplayItemModel) null;
            taskModel = (TaskModel) null;
          }
          else
          {
            newTask.ProjectId = projectId;
            newTask.ParentId = "";
            newTask.Status = 0;
            newTask.ColumnId = model.Section.SectionId;
            bool top = TaskDefaultDao.GetDefaultSafely().AddTo == 0 || sender.ViewModel.SortOption.orderBy == "createdTime" || sender.ViewModel.SortOption.orderBy == "modifiedTime";
            DisplayItemModel displayItemModel;
            if (!top)
            {
              List<DisplayItemModel> children = model.Section.Children;
              displayItemModel = children != null ? children.LastOrDefault<DisplayItemModel>() : (DisplayItemModel) null;
            }
            else
            {
              List<DisplayItemModel> children = model.Section.Children;
              displayItemModel = children != null ? children.FirstOrDefault<DisplayItemModel>() : (DisplayItemModel) null;
            }
            newTask.SortOrder = ProjectSortOrderDao.GetNextTaskSortOrderInProject(projectId, displayItemModel != null ? displayItemModel.SortOrder : 0L, "", top);
            TaskCache.AddToDict(newTask);
            displayModel = new DisplayItemModel(newTask, !(sender.ViewModel.ProjectIdentity is NormalProjectIdentity) && !(sender.ViewModel.ProjectIdentity is ParentTaskIdentity));
            if (sender.ViewModel.ProjectIdentity.SortOption.orderBy != "sortOrder")
            {
              displayModel.SpecialOrder = top ? long.MinValue : long.MaxValue;
              string sortProjectId = sender.ViewModel.ProjectIdentity.GetSortProjectId();
              string str = sender.ViewModel.ProjectIdentity.SortOption.GetSortKey();
              if (!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(newTask.ColumnId))
                str = string.Format(str, (object) newTask.ColumnId);
              if (!string.IsNullOrEmpty(newTask.ColumnId))
              {
                List<DisplayItemModel> siblings = sender.GetSiblings(model.Section);
                // ISSUE: explicit non-virtual call
                long num = siblings == null || __nonvirtual (siblings.Count) <= 0 ? long.MinValue : (top ? siblings.Min<DisplayItemModel>((Func<DisplayItemModel, long>) (m => m.SpecialOrder)) : siblings.Max<DisplayItemModel>((Func<DisplayItemModel, long>) (m => m.SpecialOrder)));
                switch (num)
                {
                  case long.MinValue:
                  case long.MaxValue:
                    break;
                  default:
                    displayModel.SpecialOrder = top ? num - 268435456L : num + 268435456L;
                    SyncSortOrderModel syncSortOrderModel = await TaskSortOrderService.InsertOrUpdateAsync(str, sortProjectId, newTask.Id, sortOrder: new long?(displayModel.SpecialOrder));
                    break;
                }
              }
            }
            displayModel.Level = 0;
            displayModel.Section = model.Section;
            displayModel.IsNewAdd = true;
            taskModel = displayModel.GetTaskModel();
            taskModel.createdTime = new DateTime?(DateTime.Now);
            taskModel.modifiedTime = new DateTime?(DateTime.Now);
            int num1 = sender.DisplayModels.IndexOf(model);
            sender.ViewModel.SourceModels.Add(newTask);
            displayModel.Parent = model;
            if (!top)
            {
              int num2 = num1 + 1;
              while (num2 < sender.DisplayModels.Count && !sender.DisplayModels[num2].IsSection)
                ++num2;
              if (num2 > num1)
              {
                sender.DisplayModels.Insert(Math.Min(num2, sender.DisplayModels.Count), displayModel);
                if (model.Section.Children != null)
                  model.Section.Children.Add(displayModel);
                else
                  model.Section.Children = new List<DisplayItemModel>()
                  {
                    displayModel
                  };
              }
            }
            else
            {
              sender.DisplayModels.Insert(num1 + 1, displayModel);
              if (model.Section.Children != null)
                model.Section.Children.Insert(0, displayModel);
              else
                model.Section.Children = new List<DisplayItemModel>()
                {
                  displayModel
                };
            }
            sender.ViewModel.SourceModels.Add(displayModel.SourceViewModel);
            sender.DisableReloadInSecond();
            TaskModel taskModel1 = await TaskService.AddTask(taskModel, sender: (object) sender);
            ++model.Num;
            sender.SelectedId = displayModel.Id;
            displayModel.Selected = true;
            EventHandler itemsCountChanged = sender.ItemsCountChanged;
            if (itemsCountChanged != null)
              itemsCountChanged((object) sender, (EventArgs) null);
            sender.UpdateLayout();
            UtilLog.Info("TaskList.AddTask " + taskModel.id + " from:AddTaskInSection");
            await Task.Delay(20);
            sender.ScrollToItemById(displayModel.Id);
            sender.ViewModel.ExitBatchSelect(displayModel.Id);
            sender.TryFocusAndSelectModelById(displayModel.Id);
            if (!taskModel.startDate.HasValue)
            {
              newTask = (TaskBaseViewModel) null;
              displayModel = (DisplayItemModel) null;
              taskModel = (TaskModel) null;
            }
            else
            {
              foreach (TaskReminderModel defaultAllDayReminder in TimeData.GetDefaultAllDayReminders())
              {
                defaultAllDayReminder.taskserverid = taskModel.id;
                int num3 = await TaskReminderDao.SaveReminders(defaultAllDayReminder);
                TaskItemLoadHelper.LoadShowReminder(displayModel);
              }
              newTask = (TaskBaseViewModel) null;
              displayModel = (DisplayItemModel) null;
              taskModel = (TaskModel) null;
            }
          }
        }
      }
    }

    private async Task DisableReloadInSecond()
    {
      this._canReload = false;
      await Task.Delay(300);
      this._canReload = true;
    }

    private void FocusTitle(DisplayItemModel itemModel, int delay = 0, int caretIndex = -1)
    {
      if (delay > 0)
        this.UpdateLayout();
      if (!(Utils.GetDescendantByType((Visual) this._taskList.ItemContainerGenerator.ContainerFromItem((object) itemModel), typeof (TaskTitleBox), "EditBox") is TaskTitleBox descendantByType))
        return;
      Utils.Focus((UIElement) descendantByType);
      this.UpdateLayout();
      descendantByType.CaretOffset = caretIndex == -1 ? descendantByType.Text.Length : Math.Min(caretIndex, descendantByType.Text.Length);
      descendantByType.ScrollToHorizontalOffset(10000.0);
      this.UpdateLayout();
    }

    private void SelectTaskOrItem(DisplayItemModel model)
    {
      if (model.IsTaskOrNote)
        this.SelectTask(model.Id, TaskSelectType.Navigate, false);
      else if (model.IsItem)
        this.SelectSubtask(new IdExtra(model.Id, model.TaskId), TaskSelectType.Navigate);
      else
        this.SelectItem(model.Id, model.Type);
    }

    public void SetTitleCaretIndex(int caretIndex) => this._caretIndex = caretIndex;

    public async void MultipleTextPaste(string text)
    {
      DisplayItemModel task = this.GetModelById(this.SelectedId);
      Section section;
      TimeData timeData;
      List<string> titles;
      if (task == null)
      {
        task = (DisplayItemModel) null;
        section = (Section) null;
        timeData = (TimeData) null;
        titles = (List<string>) null;
      }
      else
      {
        section = this.GetSection(this.SelectedId);
        if (string.IsNullOrEmpty(text))
        {
          task = (DisplayItemModel) null;
          section = (Section) null;
          timeData = (TimeData) null;
          titles = (List<string>) null;
        }
        else
        {
          DateTime? nullable1 = (DateTime?) (section.GetStartDate() ?? this.ViewModel.ProjectIdentity.GetTimeData()?.StartDate) ?? TaskDefaultDao.GetDefaultSafely().GetDefaultDateTime();
          timeData = (TimeData) null;
          if (nullable1.HasValue)
            timeData = new TimeData()
            {
              StartDate = nullable1,
              IsAllDay = new bool?(true),
              DueDate = new DateTime?(),
              Reminders = TimeData.GetDefaultAllDayReminders()
            };
          titles = ((IEnumerable<string>) text.Trim().Split('\n')).ToList<string>();
          if (!await ProChecker.CheckTaskLimit(task.ProjectId, titles.Count) && titles.Any<string>())
          {
            task.IsNewAdd = false;
            TaskModel taskById = await TaskDao.GetTaskById(this.SelectedId);
            if (string.IsNullOrEmpty(task.Title))
            {
              string str = titles.First<string>().Trim();
              taskById.title = str;
              TaskService.UpdateTask(taskById);
              titles.RemoveAt(0);
            }
            List<string> list = titles.ToList<string>();
            string projectId = task.ProjectId;
            TimeData time = timeData;
            int priority = task.IsNote ? 0 : section.GetPriority();
            List<string> tags = TagSerializer.ToTags(section.GetTag());
            int status = task.Status;
            string assignee = section.GetAssignee();
            string parentId = task.ParentId;
            int num1 = task.IsNote ? 1 : 0;
            bool isPinned = task.IsPinned;
            bool? nullable2 = new bool?(false);
            long? nullable3 = new long?(taskById.sortOrder);
            DateTime? defaultDate = new DateTime?();
            int num2 = isPinned ? 1 : 0;
            bool? addTop = nullable2;
            long? targetSortOrder = nullable3;
            List<TaskModel> source = await TaskService.BatchAddTasks(list, projectId, time, priority, tags, status, assignee, parentId: parentId, isNote: num1 != 0, defaultDate: defaultDate, isPin: num2 != 0, addTop: addTop, targetSortOrder: targetSortOrder);
            TaskChangeNotifier.NotifyTaskBatchAdded(source != null ? source.Select<TaskModel, string>((Func<TaskModel, string>) (model => model.id)).ToList<string>() : (List<string>) null);
          }
          this.Load(true, ignoreFocus: true);
          task = (DisplayItemModel) null;
          section = (Section) null;
          timeData = (TimeData) null;
          titles = (List<string>) null;
        }
      }
    }

    public async Task RemoveSection(string modelId)
    {
      this.RemoveItemById(modelId);
      if (this.DisplayModels.Count<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.IsCustomizedSection)) > 1)
        return;
      this.LoadAsync(true);
    }

    public void AfterTaskChanged(DisplayItemModel model, bool tryFocused = false)
    {
      if (!tryFocused)
        this._caretIndex = -1;
      else
        this.FocusCurrent();
    }

    public void OnItemArchived() => this.Load();

    public void OnHabitSkipped(string habitId)
    {
      if (this.ViewModel.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (p => p.Id == habitId)) != null)
        this.RemoveItemById(habitId);
      if (!this.ViewModel.InKanban)
        return;
      EventHandler itemsCountChanged = this.ItemsCountChanged;
      if (itemsCountChanged == null)
        return;
      itemsCountChanged((object) this, (EventArgs) null);
    }

    public void ReLoad(string id)
    {
      if (!string.IsNullOrEmpty(id))
        this.SelectedId = id;
      this.Load();
    }

    public bool IsCompletedList() => this.ViewModel.ProjectIdentity is CompletedProjectIdentity;

    public bool CanAddSubTask()
    {
      return !(this.ViewModel.ProjectIdentity is CompletedProjectIdentity) && !(this.ViewModel.ProjectIdentity is AssignToMeProjectIdentity) && !(this.ViewModel.ProjectIdentity is SearchProjectIdentity);
    }

    private async void FocusCurrent()
    {
      await Task.Delay(200);
      DisplayItemModel selectedModel = this.GetSelectedModel();
      if (selectedModel == null || selectedModel.IsNewAdd)
        return;
      this.FocusItemTitle(selectedModel);
    }

    private DisplayItemModel GetSelectedModel()
    {
      List<DisplayItemModel> list = this.ViewModel.Items.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (t => t.Selected)).ToList<DisplayItemModel>();
      return list.Count != 1 ? (DisplayItemModel) null : list[0];
    }

    public void BatchSelectOnMove(bool isUp)
    {
      int num1 = -1;
      int num2 = -1;
      int batchSelectIndex = this.ViewModel.GetFirstBatchSelectIndex();
      if (batchSelectIndex < 0)
        return;
      for (int index = 0; index < this.DisplayModels.Count; ++index)
      {
        if (this.DisplayModels[index].Selected)
        {
          if (num1 < 0)
          {
            num1 = index;
            num2 = index;
          }
          else
            num2 = index;
        }
      }
      int index1 = batchSelectIndex < num2 ? num2 : num1;
      if (index1 < 0 || index1 >= this.DisplayModels.Count)
        return;
      int index2 = index1;
      while (index2 >= 0 && index2 < this.DisplayModels.Count)
      {
        if (isUp)
          --index2;
        else
          ++index2;
        if (index2 >= 0 && index2 < this.DisplayModels.Count)
        {
          DisplayItemModel displayModel = this.DisplayModels[index2];
          if (displayModel.IsTaskOrNote && displayModel.CanBatchSelect)
          {
            index1 = index2;
            break;
          }
        }
      }
      if (index1 == batchSelectIndex)
      {
        this.SelectTask(this.DisplayModels[batchSelectIndex].Id, TaskSelectType.Navigate, false);
      }
      else
      {
        DisplayItemModel displayModel = this.DisplayModels[index1];
        if (!displayModel.IsTaskOrNote)
          return;
        this.BatchShiftSelect(displayModel.TaskId, batchSelectIndex);
      }
    }

    public async Task OnCheckBoxRightMouseUp(UIElement element, DisplayItemModel model)
    {
    }

    public void OnNavigateTask(DisplayItemModel model)
    {
      EventHandler<string> navigateTask = this.NavigateTask;
      if (navigateTask == null)
        return;
      navigateTask((object) this, model?.TaskId);
    }

    public void OnLineVisibleChanged(DisplayItemModel model, bool visible)
    {
      int num = this.DisplayModels.IndexOf(model);
      if (num <= 0)
        return;
      this.DisplayModels[num - 1].LineVisible = visible;
    }

    public async void SetDetailInOperation(bool inOperate, bool active = true)
    {
      TaskListView child = this;
      if (!child.ViewModel.InDetail)
        return;
      TaskDetailView parentDetailControl = Utils.FindParent<TaskDetailView>((DependencyObject) child);
      parentDetailControl?.SetPopupShowing(inOperate);
      if (((inOperate ? 0 : (parentDetailControl != null ? 1 : 0)) & (active ? 1 : 0)) != 0)
      {
        await Task.Delay(50);
        Utils.FindParent<TaskDetailWindow>((DependencyObject) parentDetailControl)?.Activate();
      }
      parentDetailControl = (TaskDetailView) null;
    }

    public void RemoveItemByIds(List<string> taskIds)
    {
      this.ViewModel.RemoveItemByIds(taskIds);
      EventHandler itemsCountChanged = this.ItemsCountChanged;
      if (itemsCountChanged == null)
        return;
      itemsCountChanged((object) this, (EventArgs) null);
    }

    public void RemoveItemById(string id)
    {
      this.ViewModel.RemoveItemById(id);
      EventHandler itemsCountChanged = this.ItemsCountChanged;
      if (itemsCountChanged == null)
        return;
      itemsCountChanged((object) this, (EventArgs) null);
    }

    public IToastShowWindow GetToastParent()
    {
      return Utils.FindParent<IToastShowWindow>((DependencyObject) this) ?? (IToastShowWindow) App.Window;
    }

    public async Task OnLostFocus(bool reload)
    {
      this._caretIndex = -1;
      if (!reload)
        return;
      await Task.Delay(150);
      this._canReload = true;
      this.LoadAsync(true);
    }

    public async void AfterTaskProjectChanged(DisplayItemModel model)
    {
      if (model == null)
        return;
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.TaskId);
      if (this.ViewModel.InDetail || thinTaskById == null || this.ViewModel.InMatrix)
        return;
      TaskService.TryToastMoveControl(this.ViewModel.ProjectIdentity, thinTaskById, thinTaskById.projectId, true);
    }

    public TimeData GetDefaultTimeData()
    {
      TimeData timeData1 = this.ViewModel.ProjectIdentity.GetTimeData();
      TimeData defaultTimeData = TimeData.BuildDefaultStartAndEnd();
      if (timeData1 == null)
        return defaultTimeData;
      if (defaultTimeData.StartDate.HasValue)
      {
        if (defaultTimeData.DueDate.HasValue)
        {
          DateTime? nullable1 = defaultTimeData.DueDate;
          DateTime? startDate = defaultTimeData.StartDate;
          double totalMinutes = (nullable1.HasValue & startDate.HasValue ? new TimeSpan?(nullable1.GetValueOrDefault() - startDate.GetValueOrDefault()) : new TimeSpan?()).Value.TotalMinutes;
          TimeData timeData2 = defaultTimeData;
          nullable1 = defaultTimeData.StartDate;
          DateTime? nullable2 = new DateTime?(nullable1.Value.AddMinutes(totalMinutes));
          timeData2.DueDate = nullable2;
        }
        defaultTimeData.StartDate = timeData1.StartDate;
      }
      else
      {
        defaultTimeData.StartDate = timeData1.StartDate;
        defaultTimeData.IsAllDay = timeData1.IsAllDay;
      }
      return defaultTimeData;
    }

    public void FocusItem(string modelId)
    {
      DisplayItemModel modelById = this.GetModelById(modelId);
      if (modelById == null || modelById.IsNewAdd)
        return;
      this.FocusItemTitle(modelById);
    }

    public void ResetModel(DisplayItemModel model)
    {
      int index = this.DisplayModels.IndexOf(model);
      if (index < 0)
        return;
      this.DisplayModels[index] = model.Clone();
    }

    public void OnSectionStatusChanged(SectionStatus status)
    {
      this.ViewModel.OpenOrCloseSection(status, true);
      DataChangedNotifier.NotifyListSectionOpenChanged((object) this, (this.ViewModel.ProjectIdentity.CatId, this.ViewModel.ProjectIdentity.SortOption));
    }

    public void SelectOrDeselectAll(DisplayItemModel model, bool selectAll)
    {
      if (!this.ViewModel.InBatchSelect || model == null || !model.IsSection || model.Section.Children == null)
        return;
      foreach (DisplayItemModel child in model.Section.Children)
      {
        if (child.CanBatchSelect)
        {
          child.Selected = selectAll;
          child.InBatchSelected = selectAll;
          foreach (DisplayItemModel childrenModel in child.GetChildrenModels(true))
          {
            childrenModel.Selected = selectAll;
            childrenModel.InBatchSelected = selectAll;
          }
        }
      }
      this.NotifyMultipleSelected();
    }

    public async Task OnTaskOpenClick(DisplayItemModel model)
    {
      ProjectIdentity projectIdentity = this.ViewModel.ProjectIdentity;
      if (projectIdentity is ColumnProjectIdentity columnProjectIdentity)
        projectIdentity = columnProjectIdentity.Project;
      if (model == null)
        return;
      int num;
      switch (projectIdentity)
      {
        case NormalProjectIdentity _:
        case GroupProjectIdentity _:
        case ParentTaskIdentity _:
        case MatrixQuadrantIdentity _:
        case CompletedProjectIdentity _:
          num = 1;
          break;
        default:
          num = projectIdentity is ClosedProjectIdentity ? 1 : 0;
          break;
      }
      bool useTaskOpen = num != 0;
      this.ViewModel.ChangeModelOpenStatus(model, useTaskOpen);
      if (useTaskOpen)
        await TaskService.FoldTask(model.Id, model.IsOpen);
      else
        SmartListTaskFoldHelper.UpdateStatus(model.Id, projectIdentity.CatId, !model.IsOpen);
      TaskChangeNotifier.NotifyTaskOpenChanged(model.Id);
    }

    public async Task OnAddTaskInSectionClick(DisplayItemModel model)
    {
      TaskListView child = this;
      SectionAddTaskViewModel addingTaskModel = Utils.FindParent<KanbanContainer>((DependencyObject) child)?.GetAddingTaskModel(child.ViewModel.ProjectIdentity, model.Section);
      int num = child.DisplayModels.IndexOf(model);
      child.DisplayModels.Insert(num + 1, new DisplayItemModel()
      {
        SourceViewModel = new TaskBaseViewModel()
        {
          Id = string.Empty
        },
        AddViewModel = addingTaskModel,
        Section = model.Section
      });
      child.SetAddingTask(true);
    }

    public void RemoveSelected(string id) => this.ViewModel.RemoveSelectedId(id);

    public void SetSelected(List<string> ids)
    {
      ids = ids ?? new List<string>();
      bool flag = ids.Count > 1;
      foreach (DisplayItemModel displayModel in (Collection<DisplayItemModel>) this.DisplayModels)
      {
        if (this.ViewModel.InMatrix)
        {
          displayModel.ShowTopMargin = true;
          displayModel.ShowBottomMargin = true;
        }
        if (ids.Contains(displayModel.Id))
        {
          displayModel.Selected = true;
          displayModel.InBatchSelected = flag;
        }
        else
        {
          displayModel.Selected = false;
          displayModel.InBatchSelected = false;
        }
      }
      this.ViewModel.SetSelectedTaskIds(ids);
    }

    public void ScrollToItemById(string id)
    {
      if (this.ViewModel.Items == null || !this.ViewModel.Items.Any<DisplayItemModel>())
        return;
      DisplayItemModel displayItemModel = this.ViewModel.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.Id == id));
      if (displayItemModel == null)
        return;
      this._taskList.ScrollIntoView((object) displayItemModel);
    }

    public void ScrollToItem(DisplayItemModel item) => this._taskList.ScrollIntoView((object) item);

    public void HideLoadMore()
    {
      if (this._loadMoreItem == null)
        return;
      this._loadMoreItem.Visibility = Visibility.Collapsed;
    }

    public void UnregisterLoadMore(LoadMoreItemControl item)
    {
      if (this._loadMoreItem == item)
        this._loadMoreItem = (LoadMoreItemControl) null;
      item.LoadMore -= new EventHandler<string>(this.OnLoadMoreClick);
    }

    public void RegisterLoadMore(LoadMoreItemControl item)
    {
      item.LoadMore -= new EventHandler<string>(this.OnLoadMoreClick);
      item.LoadMore += new EventHandler<string>(this.OnLoadMoreClick);
      this._loadMoreItem = item;
    }

    private async void OnLoadMoreClick(object sender, string parentId)
    {
      if (this.ViewModel.InDetail)
      {
        this.LoadCompleteChildren(parentId);
        trash = (TrashProjectIdentity) null;
      }
      else if (this.ViewModel.ProjectIdentity is TrashProjectIdentity trash)
      {
        if (!await this.ViewModel.TrashTaskLoader.TryLoadTrashTasks(!(this.ViewModel.ProjectIdentity is TrashProjectIdentity projectIdentity) || projectIdentity.IsPerson))
        {
          trash = (TrashProjectIdentity) null;
        }
        else
        {
          await this.LoadAsync(true, !TrashSyncService.IsDrainOff(trash.IsPerson));
          trash = (TrashProjectIdentity) null;
        }
      }
      else if (this.ViewModel.ProjectData.IsCompleted)
      {
        Dictionary<string, DateTime> dictionary1 = new Dictionary<string, DateTime>();
        foreach (DisplayItemModel displayModel in (Collection<DisplayItemModel>) this.DisplayModels)
        {
          if (displayModel.IsTask && !string.IsNullOrEmpty(displayModel.ProjectId))
          {
            DateTime? completedTime;
            if (dictionary1.ContainsKey(displayModel.ProjectId))
            {
              completedTime = displayModel.CompletedTime;
              DateTime dateTime = dictionary1[displayModel.ProjectId];
              if ((completedTime.HasValue ? (completedTime.GetValueOrDefault() < dateTime ? 1 : 0) : 0) == 0)
                continue;
            }
            Dictionary<string, DateTime> dictionary2 = dictionary1;
            string projectId = displayModel.ProjectId;
            completedTime = displayModel.CompletedTime;
            DateTime dateTime1 = completedTime ?? DateTime.Now;
            dictionary2[projectId] = dateTime1;
          }
        }
        if (this.ViewModel.ProjectIdentity is CompletedProjectIdentity)
          ClosedTaskWithFilterLoader.CompletionLoader.TryLoadTasks((DateTime?) this.DisplayModels.LastOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (t => t.IsTask))?.CompletedTime, true);
        if (!(this.ViewModel.ProjectIdentity is AbandonedProjectIdentity))
        {
          trash = (TrashProjectIdentity) null;
        }
        else
        {
          ClosedTaskWithFilterLoader.AbandonedLoader.TryLoadTasks((DateTime?) this.DisplayModels.LastOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (t => t.IsTask))?.CompletedTime, true);
          trash = (TrashProjectIdentity) null;
        }
      }
      else
      {
        DisplayItemModel displayItemModel = this.DisplayModels.LastOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.IsTask && m.Status != 0));
        this.ViewModel.CompletedTaskLoader.AddLimit(this.ViewModel.ProjectIdentity);
        int num = await this.ViewModel.CompletedTaskLoader.TryLoadCompletedTasks(this.ViewModel.ProjectIdentity, localEarliestDate: (DateTime?) displayItemModel?.CompletedTime) ? 1 : 0;
        this.Load();
        trash = (TrashProjectIdentity) null;
      }
    }

    private async void LoadCompleteChildren(string taskId)
    {
      TaskListView sender = this;
      await TaskService.TryLoadTaskChildren(taskId);
      await Task.Delay(200);
      EventHandler needReload = sender.NeedReload;
      if (needReload == null)
        return;
      needReload((object) sender, (EventArgs) null);
    }

    public bool IsAllTaskOpen() => this.ViewModel.IsAllTaskOpen();

    public bool IsAllSectionOpen() => this.ViewModel.IsAllSectionOpen();

    public async Task ExpandOrFoldAllTask(bool? open = null)
    {
      if (this._isExpandOrFolding)
        return;
      this._isExpandOrFolding = true;
      bool isOpen = open.GetValueOrDefault();
      ProjectIdentity projectIdentity = this.ViewModel.ProjectIdentity;
      List<DisplayItemModel> source = new List<DisplayItemModel>();
      foreach (DisplayItemModel displayModel in (Collection<DisplayItemModel>) this.DisplayModels)
      {
        if (displayModel.IsTask && displayModel.Level == 0 && displayModel.HasChildren)
        {
          source.Add(displayModel);
          source.AddRange(displayModel.GetChildrenModels(true).Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (c => c.HasChildren)));
        }
        if (!open.HasValue && !isOpen && !displayModel.IsOpen && displayModel.HasChildren)
          isOpen = true;
      }
      if (source.Count > 0)
      {
        List<string> list = source.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (m => m.Id)).ToList<string>();
        if (projectIdentity is ColumnProjectIdentity columnProjectIdentity)
          projectIdentity = columnProjectIdentity.Project;
        if (projectIdentity is NormalProjectIdentity || projectIdentity is GroupProjectIdentity || projectIdentity is ParentTaskIdentity)
          await TaskDao.FoldOrOpenTasks(list, isOpen);
        else
          SmartListTaskFoldHelper.ResetStatus(projectIdentity.CatId, isOpen ? list : (List<string>) null);
      }
      if (!this.ViewModel.InKanban)
        this.LoadAsync(true, ignoreFocus: true);
      await Task.Delay(300);
      this._isExpandOrFolding = false;
    }

    public async Task ExpandOrFoldAllSection(bool? open = null)
    {
      TaskListView sender = this;
      if (sender._isExpandOrFolding)
        return;
      sender._isExpandOrFolding = true;
      bool isOpen = open.GetValueOrDefault();
      List<DisplayItemModel> sections = new List<DisplayItemModel>();
      foreach (DisplayItemModel displayModel in (Collection<DisplayItemModel>) sender.DisplayModels)
      {
        if (displayModel.IsSection)
        {
          sections.Add(displayModel);
          if (!open.HasValue && !isOpen && !displayModel.IsOpen)
            isOpen = true;
        }
      }
      if (sections.Count > 0)
      {
        await sender.ViewModel.OpenOrCloseAllSections(sections, isOpen);
        if (sender.ViewModel.InKanban)
        {
          sender._isExpandOrFolding = false;
          return;
        }
        DataChangedNotifier.NotifyListSectionOpenChanged((object) sender, (sender.ViewModel.ProjectIdentity.CatId, sender.ViewModel.ProjectIdentity.SortOption));
      }
      await Task.Delay(300);
      sender._isExpandOrFolding = false;
    }

    public List<string> GetSelectedTaskIds()
    {
      List<string> selectedTaskIds = new List<string>();
      selectedTaskIds.AddRange(this.DisplayModels.Cast<DisplayItemModel>().Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (task => task.IsTaskOrNote && task.Selected)).Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (task => task.Id)));
      return selectedTaskIds;
    }

    public void SetQuickSetPopup(Popup popup) => this._quickSetPopup = popup;

    public void Toast(string toastString)
    {
      this.GetToastParent()?.TryToastString((object) null, toastString);
    }

    public void SetMatrixInOperation(bool inOperation)
    {
      if (!this.ViewModel.InMatrix)
        return;
      Utils.FindParent<QuadrantControl>((DependencyObject) this)?.SetMatrixInOperation(inOperation);
    }

    public async Task<string> GetCompleteText(DisplayItemModel model)
    {
      if (this.ViewModel.ProjectIdentity is TodayProjectIdentity)
      {
        List<TaskBaseViewModel> list1 = this.ViewModel.SourceModels.ToList<TaskBaseViewModel>();
        if (list1.Count<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m => m.Type != DisplayType.Habit && m.Status == 2)) >= 3)
        {
          bool showText = false;
          if (model.IsTask)
          {
            List<TaskBaseViewModel> list2 = list1.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m =>
            {
              if (m.Status != 0 || !(m.GetTaskId() != model.Id))
                return false;
              return m.IsTaskOrNote && m.IsTask || m.IsCheckItem;
            })).ToList<TaskBaseViewModel>();
            if (list2.Count == 0 || list2.All<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m =>
            {
              if (m.IsCheckItem && TaskCache.FindParent(m.ParentId, model.Id))
                return true;
              return m.IsTaskOrNote && m.IsTask && TaskCache.FindParent(m.Id, model.Id);
            })))
              showText = true;
          }
          if (model.Type == DisplayType.CheckItem)
          {
            List<TaskBaseViewModel> list3 = list1.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m =>
            {
              if (m.Status != 0 || !(m.Id != model.Id))
                return false;
              return m.IsTaskOrNote && m.IsTask || m.IsCheckItem;
            })).ToList<TaskBaseViewModel>();
            TaskBaseViewModel parent = list3.FirstOrDefault<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (m => m.Id == model.SourceViewModel.ParentId));
            if (list3.Count > 0)
            {
              if (list3.Count == 1 && parent != null)
              {
                if ((await TaskDetailItemDao.GetCheckItemsByTaskId(parent.Id)).All<TaskDetailItemModel>((Func<TaskDetailItemModel, bool>) (c => c.id == model.Id || c.status == 2)) && TaskCache.CanTaskCompletedByCheckItem(parent.Id))
                  showText = true;
              }
            }
            else
              showText = true;
            parent = (TaskBaseViewModel) null;
          }
          if (showText)
            return Utils.GetString(model.IsOutDate() ? "OverdueTasksAllDone" : "TodayTasksAllDone");
        }
      }
      return (string) null;
    }

    public ProjectIdentity GetIdentity() => this.ViewModel.ProjectIdentity;

    public async void TryLoadCompletedTasks(ProjectIdentity identity)
    {
      ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader loader = this.ViewModel?.CompletedTaskLoader;
      bool flag1 = loader != null;
      if (flag1)
        flag1 = await loader.NeedPullCompletedTasks(identity);
      bool flag2 = flag1;
      if (flag2)
        flag2 = await loader.TryLoadCompletedTasks(identity);
      if (!flag2)
      {
        loader = (ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader) null;
      }
      else
      {
        this.LoadAsync();
        loader = (ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoader) null;
      }
    }

    public async Task SortItemsAndLoad(
      List<TaskBaseViewModel> source,
      SectionAddTaskViewModel addingModel = null)
    {
      this.ViewModel.SourceModels = source;
      await this.ViewModel.LoadSortedItems(true, addingModel);
      List<string> selectedTaskIds = this.ViewModel.SelectedTaskIds;
      // ISSUE: explicit non-virtual call
      if ((selectedTaskIds != null ? (__nonvirtual (selectedTaskIds.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      foreach (DisplayItemModel displayItemModel in (Collection<DisplayItemModel>) this.ViewModel.Items)
      {
        if (this.ViewModel.SelectedTaskIds.Contains(displayItemModel.Id))
        {
          displayItemModel.Selected = true;
          displayItemModel.InBatchSelected = true;
        }
        else
        {
          displayItemModel.Selected = false;
          displayItemModel.InBatchSelected = false;
        }
      }
    }

    public void Dispose()
    {
      this.ViewModel.Dispose();
      this._taskList.ItemsSource = (IEnumerable) null;
      this.Children.Clear();
      this.ItemsCountChanged = (EventHandler) null;
      this.MoveUpCaret = (EventHandler) null;
      this.NeedReload = (EventHandler) null;
      this.BatchSelect = (EventHandler<List<string>>) null;
      this.DragOver = (EventHandler<DragMouseEvent>) null;
      this.DragDropped = (EventHandler<string>) null;
      this.BatchDragDropped = (EventHandler<List<string>>) null;
      this.NavigateTask = (EventHandler<string>) null;
      this.ItemSelect = (TaskListView.SelectDelegate) null;
    }

    public async void RemoveAddItem(DisplayItemModel model, bool delay = false)
    {
      TaskListView child = this;
      KanbanContainer kanban = Utils.FindParent<KanbanContainer>((DependencyObject) child);
      if (delay)
      {
        kanban?.StartRemoveAddModel();
        await Task.Delay(150);
      }
      child.SetAddingTask(false);
      kanban?.RemoveAddingTaskModel();
      ObservableCollection<DisplayItemModel> displayModels = child.DisplayModels;
      if (displayModels == null)
      {
        kanban = (KanbanContainer) null;
      }
      else
      {
        // ISSUE: explicit non-virtual call
        __nonvirtual (displayModels.Remove(model));
        kanban = (KanbanContainer) null;
      }
    }

    public void SetAddingTask(bool adding) => this.ViewModel.AddingTask = adding;

    public void RemoveSelectedId(string id)
    {
      TaskListViewModel viewModel = this.ViewModel;
      DisplayItemModel displayItemModel1;
      if (viewModel == null)
      {
        displayItemModel1 = (DisplayItemModel) null;
      }
      else
      {
        ObservableCollection<DisplayItemModel> items = viewModel.Items;
        displayItemModel1 = items != null ? items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Id == id)) : (DisplayItemModel) null;
      }
      DisplayItemModel displayItemModel2 = displayItemModel1;
      if (displayItemModel2 == null)
        return;
      displayItemModel2.Selected = false;
    }

    public bool ExistId(string id)
    {
      TaskListViewModel viewModel = this.ViewModel;
      DisplayItemModel displayItemModel;
      if (viewModel == null)
      {
        displayItemModel = (DisplayItemModel) null;
      }
      else
      {
        ObservableCollection<DisplayItemModel> items = viewModel.Items;
        displayItemModel = items != null ? items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Id == id)) : (DisplayItemModel) null;
      }
      return displayItemModel > null;
    }

    public void SetSelectedIdOnShowDetail(string taskId) => this.SelectedId = taskId;

    private void TaskListMouseMove(object sender, MouseEventArgs e)
    {
      if (this._taskDragPopup == null || !this._taskDragPopup.IsOpen)
        return;
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        this.MoveDragPopup(e, (DisplayItemModel) this._taskDragPopup.DataContext);
        e.Handled = true;
      }
      else
      {
        System.Windows.Point position = e.GetPosition((IInputElement) this._taskList);
        this.StopDragging(position.X >= (this.ViewModel.InDetail ? -20.0 : 0.0) && position.X <= (this.ViewModel.InDetail ? this.ActualWidth + 20.0 : this.ActualWidth) && (!this.ViewModel.InDetail || position.Y > 0.0), e);
      }
    }

    public void StartDrag(DisplayItemModel model, MouseEventArgs args)
    {
      this.OnStartDrag(model, args);
    }

    private async void OnStartDrag(DisplayItemModel model, MouseEventArgs arg)
    {
      if (model == null)
        return;
      if (model.IsItem && model.Status != 0)
      {
        Utils.Toast(Utils.GetString("CannotDragCompletedSubTask"));
      }
      else
      {
        if (!this.ViewModel.InDetail)
          TaskDetailWindow.TryCloseWindow(true);
        List<DisplayItemModel> models = this.ViewModel.Items.ToList<DisplayItemModel>();
        List<string> selectedTaskIds = this.ViewModel.SelectedTaskIds;
        DisplayItemModel displayItemModel1;
        // ISSUE: explicit non-virtual call
        if ((selectedTaskIds != null ? (__nonvirtual (selectedTaskIds.Count) > 1 ? 1 : 0) : 0) != 0)
        {
          if (!this.ViewModel.SelectedTaskIds.Contains(model.Id))
            this.ViewModel.SelectedTaskIds.Add(model.Id);
          List<string> treeTopIds = TaskDao.GetTreeTopIds(this.ViewModel.SelectedTaskIds, (this.ViewModel.ProjectIdentity is NormalProjectIdentity projectIdentity1 ? projectIdentity1.Id : (string) null) ?? (this.ViewModel.ProjectIdentity is ParentTaskIdentity projectIdentity2 ? projectIdentity2.ProjectId : (string) null));
          bool flag = this.ViewModel.ProjectIdentity is NormalProjectIdentity || this.ViewModel.ProjectIdentity is GroupProjectIdentity || this.ViewModel.ProjectIdentity is ParentTaskIdentity;
          displayItemModel1 = new DisplayItemModel()
          {
            SourceViewModel = new TaskBaseViewModel()
            {
              Type = DisplayType.Task,
              Title = string.Format(Utils.GetString("MultiTasks"), (object) this.ViewModel.SelectedTaskIds.Count),
              Id = model.Id
            },
            BatchMode = true
          };
          List<DisplayItemModel> list = models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => treeTopIds.Contains(m.Id))).ToList<DisplayItemModel>();
          if (!flag && list.Count > 1 && list.Exists((Predicate<DisplayItemModel>) (s => s.Level > 0)))
          {
            Utils.Toast(Utils.GetString("CannotDrag"));
            return;
          }
          List<DisplayItemModel> displayItemModelList = new List<DisplayItemModel>();
          foreach (DisplayItemModel displayItemModel2 in list)
          {
            displayItemModel2.OriginLevel = displayItemModel2.Level;
            displayItemModelList.Add(displayItemModel2);
            displayItemModelList.AddRange(displayItemModel2.IsOpen ? (IEnumerable<DisplayItemModel>) displayItemModel2.GetChildrenModels(false) : (IEnumerable<DisplayItemModel>) new List<DisplayItemModel>());
          }
          displayItemModel1.BatchModels = list;
          if (model.Level != 0 && !flag)
            return;
          displayItemModelList.Remove(model);
          displayItemModelList.ForEach((Action<DisplayItemModel>) (m => models.Remove(m)));
        }
        else
        {
          displayItemModel1 = DisplayItemModel.Copy(model);
          if (model.IsOpen)
            model.GetChildrenModels(true)?.ForEach((Action<DisplayItemModel>) (m => models.Remove(m)));
        }
        ItemsSourceHelper.CopyTo<DisplayItemModel>(models, this.ViewModel.Items);
        this._taskList.UpdateLayout();
        model.Dragging = true;
        model.Selected = false;
        model.OriginLevel = model.Level;
        Section section = this.GetSection(model.Id);
        this._dragStartX = arg.GetPosition((IInputElement) this._taskList).X;
        this._dragStartIndex = models.IndexOf(model);
        this._dragModel = model;
        this._dragCurrentIndex = this._dragStartIndex;
        this.SetDragPopup(false);
        this._taskDragPopup.DataContext = (object) displayItemModel1;
        this._taskDragPopup.IsOpen = true;
        this._taskDragPopup.Tag = (object) section;
        Mouse.OverrideCursor = Cursors.Hand;
        TaskDragHelpModel.DragHelp.IsDragging = true;
      }
    }

    private void SetDragPopup(bool isSection)
    {
      if (this._taskDragPopup == null)
      {
        Popup popup = new Popup();
        popup.AllowsTransparency = true;
        popup.Cursor = Cursors.Hand;
        popup.Placement = PlacementMode.Relative;
        popup.PlacementTarget = (UIElement) this._taskList;
        popup.HorizontalAlignment = HorizontalAlignment.Stretch;
        popup.IsHitTestVisible = false;
        this._taskDragPopup = popup;
        this._taskDragPopup.Closed += (EventHandler) ((o, e) =>
        {
          Mouse.Capture((IInputElement) null);
          this._taskDragPopup.Child = (UIElement) null;
        });
      }
      this._taskDragPopup.HorizontalOffset = this._taskList.ActualWidth;
      this._taskDragPopup.Width = this._taskList.ActualWidth;
      if (isSection)
      {
        Border border1 = new Border();
        border1.CornerRadius = new CornerRadius(3.0);
        border1.BorderThickness = new Thickness(1.0);
        border1.Effect = (Effect) new DropShadowEffect()
        {
          BlurRadius = 8.0,
          Opacity = 0.1,
          ShadowDepth = 2.0
        };
        Border border2 = border1;
        border2.SetResourceReference(Panel.BackgroundProperty, (object) "TaskDragPopupBackground");
        border2.SetResourceReference(Border.BorderBrushProperty, (object) "BaseColorOpacity5");
        border2.Child = (UIElement) new SectionItemControl();
        border2.IsHitTestVisible = false;
        this._taskDragPopup.Child = (UIElement) border2;
      }
      else
      {
        Popup taskDragPopup = this._taskDragPopup;
        TaskPopupItem taskPopupItem = new TaskPopupItem();
        taskPopupItem.IsHitTestVisible = false;
        taskDragPopup.Child = (UIElement) taskPopupItem;
      }
    }

    private void MoveDragPopup(MouseEventArgs e, DisplayItemModel model)
    {
      System.Windows.Point position1 = e.GetPosition((IInputElement) this._taskList);
      if (this.ViewModel.InMatrix)
      {
        if (this.QuadrantLevel > 2 && position1.Y < -40.0 || this.QuadrantLevel < 3 && position1.Y > this._taskList.ActualHeight)
          Mouse.Capture((IInputElement) this);
        else
          Mouse.Capture((IInputElement) this._taskList);
      }
      double num = position1.X - this._dragStartX + 10.0;
      this._taskDragPopup.VerticalOffset = position1.Y - (model.IsSection ? 16.0 : 24.0);
      this._taskDragPopup.HorizontalOffset = num;
      if (model.IsTaskOrNote)
      {
        if (!this.ViewModel.InMatrix)
        {
          System.Windows.Point position2 = e.GetPosition((IInputElement) Application.Current?.MainWindow);
          long x = (long) position2.X;
          long y = (long) position2.Y;
          if (position1.X < this._taskList.ActualWidth || model.IsTask)
          {
            EventHandler<DragMouseEvent> dragOver = this.DragOver;
            if (dragOver != null)
              dragOver((object) this, new DragMouseEvent((double) x, (double) y));
          }
        }
        else
        {
          EventHandler<DragMouseEvent> dragOver = this.DragOver;
          if (dragOver != null)
            dragOver((object) this, new DragMouseEvent(e));
        }
      }
      if (model.IsSection)
      {
        this.TrySwitchSection(e, position1);
        System.Windows.Point position3 = e.GetPosition((IInputElement) Application.Current?.MainWindow);
        long x = (long) position3.X;
        long y = (long) position3.Y;
        if (position1.X >= this._taskList.ActualWidth && !model.IsTask)
          return;
        EventHandler<DragMouseEvent> dragOver = this.DragOver;
        if (dragOver == null)
          return;
        dragOver((object) this, new DragMouseEvent((double) x, (double) y)
        {
          Data = (object) model
        });
      }
      else
        this.TrySwitchModel(e, position1, false);
    }

    private void TrySwitchSection(MouseEventArgs e, System.Windows.Point point)
    {
      if (point.X <= 0.0 || point.X >= this._taskList.ActualWidth)
        return;
      ListBoxItem mousePointElement = Utils.GetMousePointElement<ListBoxItem>(e, (FrameworkElement) this);
      if (mousePointElement == null || !(mousePointElement.DataContext is DisplayItemModel dataContext) || dataContext.Section is PinnedSection || dataContext.Section is CompletedSection)
        return;
      int num = this.DisplayModels.IndexOf(dataContext);
      if (this._dragCurrentIndex < 0)
      {
        for (int index = 0; index < this.DisplayModels.Count; ++index)
        {
          if (this.DisplayModels[index].Dragging)
          {
            this._dragCurrentIndex = index;
            break;
          }
        }
        if (this._dragCurrentIndex < 0)
          return;
        if (this._dragStartIndex < 0)
          this._dragStartIndex = this._dragCurrentIndex;
      }
      DisplayItemModel displayModel1 = this.DisplayModels[this._dragCurrentIndex];
      this.HideFrontLine(this._dragCurrentIndex);
      System.Windows.Point position = e.GetPosition((IInputElement) mousePointElement);
      if (num == this._dragCurrentIndex)
        return;
      int index1 = -1;
      if (this._dragCurrentIndex < num && position.Y > mousePointElement.ActualHeight / 2.0)
      {
        index1 = num;
        DisplayItemModel displayModel2 = index1 + 1 < this.DisplayModels.Count ? this.DisplayModels[index1 + 1] : (DisplayItemModel) null;
        if (displayModel2 != null && !displayModel2.IsSection)
          return;
      }
      else if (this._dragCurrentIndex > num && position.Y < mousePointElement.ActualHeight / 2.0)
      {
        index1 = num;
        if (!dataContext.IsSection)
          return;
      }
      if (index1 == this._dragCurrentIndex || index1 < 0 || index1 >= this.DisplayModels.Count)
        return;
      List<DisplayItemModel> list = this.DisplayModels.ToList<DisplayItemModel>();
      list.Remove(displayModel1);
      list.Insert(index1, displayModel1);
      this.ViewModel.SetItems(list);
      this._dragCurrentIndex = index1;
    }

    private async void TrySwitchModel(MouseEventArgs e, System.Windows.Point point, bool item2Task)
    {
      TaskListView element = this;
      System.Windows.Point dragPoint = element._dragPoint;
      element._dragPoint = point;
      if (point.X > 0.0 && point.X < element._taskList.ActualWidth && (!item2Task || point.Y > 0.0 && point.Y < element._taskList.ActualHeight))
      {
        ListBoxItem mousePointItem = Utils.GetMousePointItem<ListBoxItem>(e.GetPosition((IInputElement) element._taskList) with
        {
          X = element._taskList.ActualWidth - 40.0
        }, (FrameworkElement) element);
        if (mousePointItem == null || !(mousePointItem.DataContext is DisplayItemModel dataContext))
          return;
        int num1 = element.DisplayModels.IndexOf(dataContext);
        if (!dataContext.CanHoverSwitch(num1))
          return;
        DisplayItemModel dragModel = element.GetDragModel(item2Task);
        if (dragModel == null || dragModel.IsCourse && (dataContext.IsSection || dataContext.Section != dragModel.Section) || dragModel.IsItem && element.ViewModel.ProjectIdentity.SortOption.groupBy != "dueDate" && (dataContext.IsSection || dataContext.Section != dragModel.Section))
          return;
        if (dragModel.IsNote && (element.ViewModel.ProjectIdentity.SortOption.groupBy == "dueDate" || element.ViewModel.ProjectIdentity.SortOption.groupBy == "priority" || element.ViewModel.ProjectIdentity.Id == "note"))
        {
          bool flag = dataContext.IsSection && num1 > element._dragCurrentIndex && (dataContext.Section is NoteSection || element.ViewModel.ProjectIdentity.Id == "note");
          if (!flag && dataContext.IsSection && num1 < element._dragCurrentIndex && num1 > 0)
            flag = element.DisplayModels[num1 - 1].IsPinned;
          if (!(dataContext.Section is PinnedSection | flag))
            return;
        }
        if (dragModel.IsTask && dataContext.Section is NoteSection && (!dataContext.IsSection || num1 > element._dragCurrentIndex))
          return;
        System.Windows.Point position = e.GetPosition((IInputElement) mousePointItem);
        element.HideFrontLine(element._dragCurrentIndex);
        Section section1 = (Section) null;
        if (num1 != element._dragCurrentIndex)
        {
          if (element._dragCurrentIndex < num1 && position.Y < mousePointItem.ActualHeight / 2.0 || element._dragCurrentIndex > num1 && position.Y > mousePointItem.ActualHeight / 2.0 || num1 < 0 || num1 >= element.DisplayModels.Count || element._dragCurrentIndex >= element.DisplayModels.Count)
            return;
          DisplayItemModel displayItemModel1 = num1 > element._dragCurrentIndex ? dataContext : (num1 > 0 ? element.DisplayModels[num1 - 1] : (DisplayItemModel) null);
          DisplayItemModel displayItemModel2 = num1 < element._dragCurrentIndex ? dataContext : (num1 < element.DisplayModels.Count - 1 ? element.DisplayModels[num1 + 1] : (DisplayItemModel) null);
          if (displayItemModel2 != null && !displayItemModel2.Enable && displayItemModel2.Level > 0 || !dragModel.IsTask && displayItemModel2 != null && displayItemModel2.Level > 0)
            return;
          Section section2 = num1 < element._dragCurrentIndex ? displayItemModel1?.Section : dataContext.Section;
          if ((section2 == null ? 1 : (section2.CanSwitch(dragModel.Type) ? 1 : 0)) == 0)
            return;
          if (!element.DisplayModels.Contains(dragModel))
            element.DisplayModels.Insert(num1, dragModel);
          else
            element.SwitchModel(element._dragCurrentIndex, num1);
          element._dragCurrentIndex = num1;
          element.HideFrontLine(element._dragCurrentIndex);
          if (element._dragCurrentIndex == element._dragStartIndex)
            dragModel.Level = dragModel.OriginLevel;
          int level = displayItemModel2 == null || !displayItemModel2.IsTask ? 0 : displayItemModel2.Level;
          dragModel.Level = level;
          if (!element.ViewModel.InKanban)
            return;
          element.ViewModel.SetKanbanProperty(element.DisplayModels.ToList<DisplayItemModel>());
        }
        else
        {
          if (!dragModel.IsTask || dragPoint == new System.Windows.Point() || Math.Abs(point.X - dragPoint.X) <= Math.Abs(point.Y - dragPoint.Y))
            return;
          position.X -= (element.ViewModel.InKanban ? 120.0 : element._dragStartX - 15.0) - (double) (dragModel.OriginLevel * 20);
          DisplayItemModel displayModel1 = element._dragCurrentIndex > 0 ? element.DisplayModels[element._dragCurrentIndex - 1] : (DisplayItemModel) null;
          DisplayItemModel displayModel2 = element._dragCurrentIndex < element.DisplayModels.Count - 1 ? element.DisplayModels[element._dragCurrentIndex + 1] : (DisplayItemModel) null;
          section1 = displayModel1?.Section;
          int num2;
          if (displayModel1 != null && displayModel1.Enable && displayModel1.IsTask && (element.ViewModel.InDetail || displayModel1.Status == 0))
          {
            if (!displayModel1.IsOpen)
            {
              List<DisplayItemModel> children = displayModel1.Children;
              // ISSUE: explicit non-virtual call
              if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
              {
                num2 = displayModel1.Level;
                goto label_37;
              }
            }
            num2 = displayModel1.Level + 1;
          }
          else
            num2 = 0;
label_37:
          int num3 = num2;
          int level1 = displayModel2 == null || !displayModel2.IsTask ? 0 : displayModel2.Level;
          int val1_1 = Math.Min(num3, level1);
          int val2 = dragModel.Level;
          if ((int) position.X < val2 * 20 + 12)
            val2 = ((int) position.X - 12) / 20;
          else if ((int) position.X > val2 * 20 + 32)
            val2 = ((int) position.X - 32) / 20;
          int val1_2 = Math.Max(0, Math.Min(4, val2));
          int level2 = dragModel.Level;
          if (dragModel.Level <= 4)
          {
            int num4 = Math.Max(val1_1, Math.Min(val1_2, num3));
            dragModel.Level = num4 + element._parentLevel > 4 ? 4 - element._parentLevel : num4;
          }
          if (!element.ViewModel.InKanban || dragModel.Level == level2)
            return;
          element.ViewModel.SetKanbanProperty(element.DisplayModels.ToList<DisplayItemModel>());
        }
      }
      else
      {
        if (!item2Task || element._dragCheckItem == null)
          return;
        element.DisplayModels.Remove(element._dragCheckItem);
        // ISSUE: explicit non-virtual call
        __nonvirtual (element.ResetDrag());
      }
    }

    private bool CanChangeSection(SortOption option, DisplayType dragType)
    {
      if (option.groupBy == "tag")
        return false;
      return !(option.groupBy == "priority") || dragType != DisplayType.Note;
    }

    private bool CanSort(SortOption option, Section dropSection)
    {
      if (option == null)
        return false;
      bool flag = option.orderBy != "title" && option.orderBy != "assignee" && option.orderBy != "modifiedTime" && option.orderBy != "createdTime";
      if (flag)
      {
        if (this.ViewModel.InKanban)
        {
          flag = ColumnViewModel.CanTaskSortInColumn(this.ViewModel.ProjectIdentity.Id);
          if (flag && option.groupBy == "none")
            flag = dropSection is PinnedSection || this.ViewModel.ProjectIdentity.Id != "date:-1";
          if (flag && dropSection is OutdatedSection)
            flag = false;
        }
        else
          flag = dropSection == null || dropSection.CanSort(option.orderBy);
      }
      return flag;
    }

    private DisplayItemModel GetDragModel(bool item2Task)
    {
      DisplayItemModel dragModel;
      if (item2Task)
      {
        if (this._dragCheckItem == null)
          return (DisplayItemModel) null;
        dragModel = this._dragCheckItem;
      }
      else
      {
        if (this._dragModel == null)
          return (DisplayItemModel) null;
        int num = this.DisplayModels.IndexOf(this._dragModel);
        if (num < 0)
          return (DisplayItemModel) null;
        this._dragCurrentIndex = num;
        dragModel = this._dragModel;
      }
      return dragModel;
    }

    private Section GetSection(string taskId)
    {
      int currentIndex = this.GetCurrentIndex(taskId);
      if (currentIndex >= 1)
      {
        for (int index = currentIndex - 1; index >= 0; --index)
        {
          DisplayItemModel displayItemModel = this.ViewModel.Items[index];
          if (displayItemModel.IsSection)
            return displayItemModel.Section;
        }
      }
      return this.InitEmptySection();
    }

    private Section InitEmptySection()
    {
      return this.ViewModel.SortOption.groupBy == "sortOrder" ? new Section(true, true) : new Section();
    }

    private void HideFrontLine(int index)
    {
      if (index > 0)
      {
        DisplayItemModel displayModel = this.DisplayModels[Math.Min(this.DisplayModels.Count, index - 1)];
        if (!displayModel.IsSection)
          displayModel.LineVisible = false;
        if (this._frontItem == displayModel)
          return;
        if (this._frontItem != null && !this._frontItem.IsSection)
          this._frontItem.LineVisible = true;
        this._frontItem = displayModel;
      }
      else
      {
        if (this._frontItem == null || this._frontItem.IsSection)
          return;
        this._frontItem.LineVisible = true;
        this._frontItem = (DisplayItemModel) null;
      }
    }

    public void ResetDrag()
    {
      this._dragCurrentIndex = -1;
      this._dragStartIndex = -1;
      this._dragPoint = new System.Windows.Point();
      if (this._dragModel != null)
      {
        this._dragModel.Dragging = false;
        this._dragModel = (DisplayItemModel) null;
      }
      this._dragCheckItem = (DisplayItemModel) null;
    }

    private void RestoreIndex(DisplayItemModel dragModel)
    {
      if (this._dragCurrentIndex == this._dragStartIndex || this._dragStartIndex < 0)
        return;
      this.SwitchModel(this._dragCurrentIndex, this._dragStartIndex);
      this._dragCurrentIndex = this._dragStartIndex;
      dragModel.Level = dragModel.OriginLevel;
      this.HideFrontLine(this._dragCurrentIndex);
      int second = DateTime.Now.Second;
    }

    private void SwitchModel(int start, int end)
    {
      if (start < 0 || end < 0 || start >= this.DisplayModels.Count || end >= this.DisplayModels.Count)
        return;
      DisplayItemModel displayModel = this.DisplayModels[start];
      if (end < start)
      {
        for (int index = start; index > end; --index)
          this.DisplayModels[index] = this.DisplayModels[index - 1];
        this.DisplayModels[end] = displayModel;
      }
      else
      {
        for (int index = start; index < end; ++index)
          this.DisplayModels[index] = this.DisplayModels[index + 1];
        this.DisplayModels[end] = displayModel;
      }
    }

    private async void StopDragging(bool drop, MouseEventArgs e)
    {
      TaskListView taskListView = this;
      if (taskListView._taskDragPopup == null || !taskListView._taskDragPopup.IsOpen)
        return;
      taskListView._taskDragPopup.IsOpen = false;
      TaskDragHelpModel.DragHelp.IsDragging = false;
      DisplayItemModel dragModel = taskListView._taskDragPopup.DataContext as DisplayItemModel;
      bool needReload = true;
      if (taskListView.ViewModel.InMatrix && dragModel != null)
      {
        MatrixContainer parent = Utils.FindParent<MatrixContainer>((DependencyObject) taskListView);
        if (parent != null)
        {
          List<string> taskIds;
          if (dragModel.BatchModels != null && dragModel.BatchModels.Any<DisplayItemModel>())
          {
            taskIds = dragModel.BatchModels.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (c => c.Id)).ToList<string>();
            foreach (DisplayItemModel batchModel in dragModel.BatchModels)
            {
              List<DisplayItemModel> childrenModels = batchModel.GetChildrenModels(false);
              if (childrenModels != null)
                taskIds.AddRange(childrenModels.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (t => t.Id)));
            }
          }
          else
          {
            taskIds = new List<string>() { dragModel.Id };
            List<DisplayItemModel> childrenModels = dragModel.GetChildrenModels(false);
            if (childrenModels != null)
              taskIds.AddRange(childrenModels.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (t => t.Id)));
          }
          TaskDragHelpModel.DragHelp.IsDragging = false;
          // ISSUE: explicit non-virtual call
          if (await parent.OnBatchTaskDrop(__nonvirtual (taskListView.QuadrantLevel), taskIds, e))
          {
            needReload = false;
            drop = false;
          }
        }
      }
      if (drop)
      {
        DisplayItemModel displayItemModel = dragModel;
        if ((displayItemModel != null ? (displayItemModel.IsSection ? 1 : 0) : 0) != 0)
          await taskListView.OnSectionDrop(dragModel);
        else if (taskListView.ViewModel.SelectedTaskIds != null && taskListView.ViewModel.SelectedTaskIds.Count > 1)
          await taskListView.OnBatchTaskDrop(dragModel);
        else
          await taskListView.OnTaskDrop(dragModel);
      }
      else if (!taskListView.ViewModel.InMatrix)
      {
        if (taskListView.ViewModel.SelectedTaskIds != null && taskListView.ViewModel.SelectedTaskIds.Count > 1)
        {
          EventHandler<List<string>> batchDragDropped = taskListView.BatchDragDropped;
          if (batchDragDropped != null)
          {
            TaskListView sender = taskListView;
            DisplayItemModel displayItemModel = dragModel;
            List<string> e1;
            if (displayItemModel == null)
            {
              e1 = (List<string>) null;
            }
            else
            {
              List<DisplayItemModel> batchModels = displayItemModel.BatchModels;
              e1 = batchModels != null ? batchModels.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (c => c.Id)).ToList<string>() : (List<string>) null;
            }
            batchDragDropped((object) sender, e1);
          }
        }
        else
        {
          ListViewContainer parent = Utils.FindParent<ListViewContainer>((DependencyObject) taskListView);
          DisplayItemModel displayItemModel = dragModel;
          if ((displayItemModel != null ? (displayItemModel.IsSection ? 1 : 0) : 0) != 0)
          {
            bool flag = parent != null;
            if (flag)
              flag = await parent.OnDragSectionDroppedOnProject(dragModel.Id, dragModel.ProjectId);
            if (flag)
              needReload = true;
          }
          else
          {
            bool flag = parent != null;
            if (flag)
              flag = await parent.OnDragTaskDroppedOnProject(dragModel?.Id);
            if (flag)
            {
              // ISSUE: explicit non-virtual call
              __nonvirtual (taskListView.RemoveItemById(dragModel?.Id));
            }
            EventHandler<string> dragDropped = taskListView.DragDropped;
            if (dragDropped != null)
              dragDropped((object) taskListView, dragModel?.Id);
            await Task.Delay(100);
          }
        }
      }
      // ISSUE: explicit non-virtual call
      __nonvirtual (taskListView.ResetDrag());
      taskListView.ClearDropTarget();
      Mouse.OverrideCursor = (Cursor) null;
      if (Window.GetWindow((DependencyObject) taskListView) is MainWindow window)
        window.TryFocus();
      if (needReload)
        taskListView.LoadAsync(true);
      dragModel = (DisplayItemModel) null;
    }

    private async Task OnSectionDrop(DisplayItemModel dragModel)
    {
      DisplayItemModel model = this._dragCurrentIndex >= this.DisplayModels.Count || this._dragCurrentIndex < 0 ? (DisplayItemModel) null : this.DisplayModels[this._dragCurrentIndex];
      if (model == null || model.Id != dragModel.Id || !model.Dragging)
      {
        model = (DisplayItemModel) null;
        for (int index = 0; index < this.DisplayModels.Count; ++index)
        {
          DisplayItemModel displayModel = this.DisplayModels[index];
          if (displayModel.Dragging && displayModel.IsSection)
          {
            model = this.DisplayModels[index];
            this._dragCurrentIndex = index;
            break;
          }
        }
      }
      if (model == null)
      {
        model = (DisplayItemModel) null;
      }
      else
      {
        if (this._dragCurrentIndex < this.DisplayModels.Count - 1 && this._dragCurrentIndex >= 0)
        {
          DisplayItemModel displayModel = this.DisplayModels[this._dragCurrentIndex + 1];
          if (displayModel.IsCustomizedSection)
          {
            long? nullable = await ColumnDao.SaveSortOrder(dragModel.Id, displayModel.Id, true);
            DataChangedNotifier.NotifyColumnChanged(this.ViewModel?.ProjectIdentity?.Id);
            SyncManager.TryDelaySync();
            model.Dragging = false;
            model = (DisplayItemModel) null;
            return;
          }
        }
        await ColumnDao.SaveAsLastSortOrder(dragModel.Id);
        DataChangedNotifier.NotifyColumnChanged(this.ViewModel?.ProjectIdentity?.Id);
        SyncManager.TryDelaySync();
        model.Dragging = false;
        model = (DisplayItemModel) null;
      }
    }

    public async Task OnBatchTaskDrop(DisplayItemModel dragModel)
    {
      await this.OnTaskDrop(dragModel, true);
    }

    public async Task OnTaskDrop(DisplayItemModel dragModel, bool inBatch = false)
    {
      DisplayItemModel model = (DisplayItemModel) null;
      for (int index = 0; index < this.DisplayModels.Count; ++index)
      {
        if (this.DisplayModels[index].Id == dragModel.Id)
        {
          model = this.DisplayModels[index];
          this._dragCurrentIndex = index;
          break;
        }
      }
      DisplayItemModel frontItem;
      DisplayItemModel nextItem;
      List<string> dropIds;
      if (model == null)
      {
        frontItem = (DisplayItemModel) null;
        nextItem = (DisplayItemModel) null;
        dropIds = (List<string>) null;
      }
      else
      {
        model.Dragging = false;
        List<DisplayItemModel> displayItemModelList;
        if (!inBatch)
          displayItemModelList = new List<DisplayItemModel>()
          {
            model
          };
        else
          displayItemModelList = dragModel.BatchModels;
        List<DisplayItemModel> dropModels = displayItemModelList;
        if (dropModels == null)
        {
          frontItem = (DisplayItemModel) null;
          nextItem = (DisplayItemModel) null;
          dropIds = (List<string>) null;
        }
        else if (model.OriginLevel == model.Level && !inBatch && this._dragCurrentIndex == this._dragStartIndex)
        {
          frontItem = (DisplayItemModel) null;
          nextItem = (DisplayItemModel) null;
          dropIds = (List<string>) null;
        }
        else
        {
          int dragCurrentIndex = this._dragCurrentIndex;
          frontItem = (DisplayItemModel) null;
          nextItem = (DisplayItemModel) null;
          if (dragCurrentIndex >= 0)
          {
            for (int index = dragCurrentIndex - 1; index >= 0; --index)
            {
              DisplayItemModel displayModel = this.DisplayModels[index];
              if (displayModel.Level <= model.Level && displayModel.IsTask || !displayModel.IsTask)
              {
                frontItem = displayModel;
                break;
              }
            }
            for (int index = dragCurrentIndex + 1; index < this.DisplayModels.Count; ++index)
            {
              DisplayItemModel displayModel = this.DisplayModels[index];
              if (!displayModel.IsSection && displayModel.Level >= model.Level)
              {
                if (displayModel.Level == model.Level)
                {
                  nextItem = displayModel;
                  break;
                }
              }
              else
                break;
            }
          }
          List<DisplayItemModel> insertModels = new List<DisplayItemModel>();
          dropModels.ForEach((Action<DisplayItemModel>) (m =>
          {
            m.Level = model.Level;
            insertModels.Add(m);
            m.Selected = true;
            m.InBatchSelected = dropModels.Count > 1;
            List<DisplayItemModel> childrenModels = m.IsOpen ? m.GetChildrenModels(false) : (List<DisplayItemModel>) null;
            childrenModels?.ForEach((Action<DisplayItemModel>) (c => c.Level += m.Level - m.OriginLevel));
            if (childrenModels == null)
              return;
            insertModels.AddRange((IEnumerable<DisplayItemModel>) childrenModels);
          }));
          List<DisplayItemModel> list1 = this.DisplayModels.ToList<DisplayItemModel>();
          if (this._dragCurrentIndex >= 0 && this._dragCurrentIndex < list1.Count)
          {
            list1.Remove(model);
            list1.InsertRange(this._dragCurrentIndex, (IEnumerable<DisplayItemModel>) insertModels);
          }
          this.ViewModel.SetItems(list1);
          this._taskList.UpdateLayout();
          await Task.Delay(1);
          bool isUndo = model.Level == 0 && !inBatch && TaskDragUndoModel.TryUndo(model.Id);
          TaskDragUndoModel.TryFinishAll();
          Section section1 = frontItem?.Section ?? nextItem?.Section;
          if (model.Type == DisplayType.CheckItem)
          {
            string str = section1?.SectionId ?? string.Empty;
            if (!(this._taskDragPopup?.Tag is Section section2))
              section2 = model.Section;
            if (section2.SectionId != str)
            {
              if (section1 is CompletedSection)
              {
                await TaskDetailItemService.CompleteCheckItem(model.Id);
                frontItem = (DisplayItemModel) null;
                nextItem = (DisplayItemModel) null;
                dropIds = (List<string>) null;
                return;
              }
              if (this.ViewModel.SortOption.groupBy == "priority")
              {
                Utils.Toast(Utils.GetString("CannotDragSubTasksToPrioritySection"));
                frontItem = (DisplayItemModel) null;
                nextItem = (DisplayItemModel) null;
                dropIds = (List<string>) null;
                return;
              }
              if (this.ViewModel.SortOption.groupBy == "project")
              {
                Utils.Toast(Utils.GetString("CannotDragSubTasksToProjectSection"));
                frontItem = (DisplayItemModel) null;
                nextItem = (DisplayItemModel) null;
                dropIds = (List<string>) null;
                return;
              }
              TaskModel thinTaskById = await TaskDao.GetThinTaskById(model.TaskId);
              if (!string.IsNullOrEmpty(thinTaskById.attendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) thinTaskById))
              {
                Utils.Toast(Utils.GetString("OnlyOwnerCanChangeSubtask"));
                frontItem = (DisplayItemModel) null;
                nextItem = (DisplayItemModel) null;
                dropIds = (List<string>) null;
                return;
              }
            }
          }
          dropIds = dropModels.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (m => m.Id)).ToList<string>();
          if (frontItem?.Section is CompletedSection || nextItem?.Section is CompletedSection)
          {
            UtilLog.Info("TaskList.CompleteTasks " + dropIds.Join<string>(";") + ", from:TaskDragDrop");
            int num = await TaskService.BatchCompleteTasks(dropIds, justComplete: true) ? 1 : 0;
            frontItem = (DisplayItemModel) null;
            nextItem = (DisplayItemModel) null;
            dropIds = (List<string>) null;
          }
          else
          {
            List<DisplayItemModel> list2 = dropModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Status != 0)).ToList<DisplayItemModel>();
            if (!this.ViewModel.InDetail && model.Level == 0 && list2.Count > 0 && !(frontItem?.Section is PinnedSection))
            {
              List<string> ids = list2.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (m => m.Id)).ToList<string>();
              int num = await TaskService.BatchCompleteTasks(ids) ? 1 : 0;
              UtilLog.Info(string.Format("TaskList.UnCompleteTasks {0}, from:TaskDragDrop", (object) ids.Count));
              ids = (List<string>) null;
            }
            if (!isUndo)
              await this.SetModelParent(dropModels, frontItem, model.Level);
            bool flag = await this.ChangeSortOrder(frontItem, nextItem, model, dropModels);
            UtilLog.Info(string.Format("TaskList.ChangeSortOrder {0}, sort {1}, result {2}", (object) dropIds.Join<string>(";"), (object) this.ViewModel?.SortOption, (object) flag));
            if (flag)
              TaskChangeNotifier.NotifyTaskSortChanged((IEnumerable<string>) dropIds);
            this.ResetDrag();
            SyncManager.TryDelaySync();
            frontItem = (DisplayItemModel) null;
            nextItem = (DisplayItemModel) null;
            dropIds = (List<string>) null;
          }
        }
      }
    }

    private async Task<bool> ChangeSortOrder(
      DisplayItemModel frontItem,
      DisplayItemModel nextItem,
      DisplayItemModel dragModel,
      List<DisplayItemModel> models)
    {
      if (models == null || models.Count == 0)
        return false;
      Section dropSection = frontItem?.Section ?? (nextItem == null || nextItem.IsSection ? (Section) null : nextItem.Section);
      bool flag1 = false;
      if (models.Count > 1)
      {
        DisplayItemModel firstModel = models.First<DisplayItemModel>();
        flag1 = models.Count<DisplayItemModel>((Func<DisplayItemModel, bool>) (task => task.IsPinned != firstModel.IsPinned)) > 0;
      }
      if (dropSection is PinnedSection && dragModel.IsPinned && !flag1)
      {
        await TaskDragHelper.ChangeOrderInType("taskPinned", frontItem, nextItem, models, this.DisplayModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => !i.IsSection && i.IsPinned)).ToList<DisplayItemModel>(), this.ViewModel.ProjectIdentity);
        return true;
      }
      if (dropSection is PinnedSection && !dragModel.IsPinned | flag1)
      {
        foreach (DisplayItemModel model in models)
          await TaskService.TogglesStarred(model.Id, this.ViewModel.ProjectIdentity.CatId, new bool?(true), notify: false);
        await TaskDragHelper.ChangeOrderInType("taskPinned", frontItem, nextItem, models, this.DisplayModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => !i.IsSection && i.IsPinned)).ToList<DisplayItemModel>(), this.ViewModel.ProjectIdentity);
        return true;
      }
      if (dragModel.IsPinned && dropSection != null && !(dropSection is PinnedSection))
      {
        foreach (DisplayItemModel model in models)
          await TaskService.TogglesStarred(model.Id, this.ViewModel.ProjectIdentity.CatId, new bool?(false), notify: false);
      }
      List<DisplayItemModel> siblings = this.GetSiblings(dropSection);
      bool keepAssignee = this.ViewModel.ProjectIdentity is AssignToMeProjectIdentity;
      List<DisplayItemModel> source = siblings;
      siblings = source != null ? source.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => !models.Contains(m))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      switch (this.ViewModel.SortOption.groupBy)
      {
        case "sortOrder":
          string columnId = frontItem == null ? string.Empty : (frontItem.IsSection ? frontItem.Id : frontItem.ColumnId);
          if (!string.IsNullOrEmpty(columnId) && dragModel.ColumnId != columnId)
          {
            await TaskService.BatchSetColumn(models.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (m => m.Id)).ToList<string>(), columnId);
            break;
          }
          break;
        case "project":
          ProjectModel projectById = CacheManager.GetProjectById(dropSection?.GetProjectId());
          if (dragModel.IsTaskOrNote && projectById != null)
          {
            await TaskService.BatchMoveParentTaskProject(models.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (m => m.Id)).ToList<string>(), projectById, keepAssignee);
            break;
          }
          break;
        case "dueDate":
          if (dragModel.Level == 0)
          {
            foreach (DisplayItemModel model in models)
            {
              if (model.Section?.SectionId != dropSection?.SectionId && !model.IsNote)
                await TaskDragHelper.ChangeNewDate(dropSection, model, "");
            }
            break;
          }
          break;
        case "priority":
          if (dragModel.Level == 0)
          {
            Section section = dropSection;
            int priority = section != null ? section.GetPriority() : 0;
            foreach (DisplayItemModel model in models)
            {
              if (model.IsTask && model.Priority != priority)
                await TaskService.SetPriority(model.Id, priority, notify: false);
            }
            break;
          }
          break;
        case "assignee":
          if (dragModel.Level == 0)
          {
            string assignee = dropSection?.GetAssignee() ?? "";
            if (!string.IsNullOrEmpty(assignee) && assignee != "-1")
            {
              List<string> list = models.Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (m => m.ProjectId)).Distinct<string>().ToList<string>();
              bool flag2 = true;
              foreach (string projectId in list)
              {
                List<ShareUserModel> projectUsers = AvatarHelper.GetProjectUsers(projectId);
                if (projectUsers == null || projectUsers.Count == 0 || projectUsers.All<ShareUserModel>((Func<ShareUserModel, bool>) (a => (a.userId.ToString() ?? "") != assignee)))
                {
                  flag2 = false;
                  break;
                }
              }
              if (!flag2)
              {
                this.Toast(Utils.GetString("ChangeAssigneeError"));
                return false;
              }
            }
            foreach (DisplayItemModel model in models)
            {
              if (model.IsTaskOrNote && model.Assignee != assignee)
                await TaskService.SetAssignee(model.Id, assignee, false);
            }
            break;
          }
          break;
      }
      if (frontItem == null && nextItem == null)
        return false;
      if (this.ViewModel.SortOption.orderBy == "sortOrder")
      {
        await this.ChangeTaskSortOrderInProject(frontItem, nextItem, dragModel, models, dropSection, siblings);
        return true;
      }
      if (dropSection != dragModel.Section && !this.CanChangeSection(this.ViewModel.SortOption, dragModel.Type) || !this.CanSort(this.ViewModel.SortOption, dropSection))
        return false;
      string catId = this.ViewModel.ProjectIdentity.GetSortProjectId();
      string sortKey = this.ViewModel.SortOption.GetSortKey();
      if (this.ViewModel.ProjectIdentity is ColumnProjectIdentity projectIdentity)
      {
        sortKey = ColumnViewModel.GetTaskSortKey(projectIdentity.GetRealSortOption(), projectIdentity.ColumnId);
        if (this.DisplayModels != null && this.DisplayModels.All<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.IsSection || m.Status != 0 || m.IsPinned || m.SpecialOrder == long.MaxValue || m.SpecialOrder == long.MinValue)))
          await this.InitSortModels(sortKey, catId);
      }
      else if (!string.IsNullOrEmpty(sortKey))
      {
        string str = dropSection?.SectionEntityId;
        if (dropSection == null)
          str = "none";
        sortKey = string.Format(sortKey, (object) str);
      }
      if (siblings != null && (siblings.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.SpecialOrder == long.MaxValue || m.SpecialOrder == long.MinValue)).Any<DisplayItemModel>() || frontItem != null && !frontItem.IsSection && nextItem != null && !nextItem.IsSection && nextItem.SpecialOrder - frontItem.SpecialOrder < (long) (models.Count * 16)))
        await TaskSortOrderService.BatchResetOrders(siblings, sortKey, catId);
      if (this.ViewModel.SortOption.orderBy == "title" || this.ViewModel.SortOption.orderBy == "assignee")
        return false;
      await TaskDragHelper.ChangeOrderInType(sortKey, frontItem == null || frontItem.IsSection || frontItem.Level != dragModel.Level ? new long?() : new long?(frontItem.SpecialOrder), nextItem == null || nextItem.IsSection || nextItem.Level != dragModel.Level ? new long?() : new long?(nextItem.SpecialOrder), models, catId);
      return true;
    }

    private async Task InitSortModels(string sortKey, string cId)
    {
      List<DisplayItemModel> list = this.DisplayModels.ToList<DisplayItemModel>();
      List<DisplayItemModel> allModels = new List<DisplayItemModel>();
      foreach (DisplayItemModel displayItemModel in list)
      {
        if (!displayItemModel.IsPinned)
        {
          if (displayItemModel.IsSection && !displayItemModel.IsOpen && !(displayItemModel.Section is CompletedSection) && !(displayItemModel.Section is PinnedSection) && displayItemModel.Children != null)
          {
            foreach (DisplayItemModel child in displayItemModel.Children)
              allModels.Add(child);
          }
          else if (!displayItemModel.IsSection && !displayItemModel.IsPinned)
          {
            if (displayItemModel.Status == 0)
              allModels.Add(displayItemModel);
            else
              break;
          }
        }
      }
      await TaskDragHelper.InitSortModels(sortKey, cId, allModels);
    }

    private async Task ChangeTaskSortOrderInProject(
      DisplayItemModel front,
      DisplayItemModel next,
      DisplayItemModel dragModel,
      List<DisplayItemModel> models,
      Section dropSection,
      List<DisplayItemModel> siblings)
    {
      string columnId;
      if (models == null)
        columnId = (string) null;
      else if (models.Count == 0)
      {
        columnId = (string) null;
      }
      else
      {
        UtilLog.Info("TaskListChangeSortOrder : Front " + front?.Id + " " + front?.SortOrder.ToString() + " next " + next?.Id + " " + next?.SortOrder.ToString() + " drag " + dragModel.Id + " " + dragModel.SortOrder.ToString());
        long? frontSortOrder = front == null || front.Level < dragModel.Level || front.IsSection ? new long?() : new long?(front.SortOrder);
        long? nextSortOrder = next == null || next.IsSection ? new long?() : next?.SortOrder;
        if (frontSortOrder.HasValue && nextSortOrder.HasValue && nextSortOrder.Value - frontSortOrder.Value < (long) (16 * models.Count))
        {
          await this.ResetTaskSortOrders(siblings);
          frontSortOrder = new long?(front.SortOrder);
          nextSortOrder = new long?(next.SortOrder);
        }
        if (!frontSortOrder.HasValue && !nextSortOrder.HasValue)
        {
          frontSortOrder = new long?(0L);
          nextSortOrder = new long?(268435456L * (long) (models.Count + 1));
        }
        if (!frontSortOrder.HasValue)
        {
          long? nullable = nextSortOrder;
          frontSortOrder = nullable.HasValue ? new long?(nullable.GetValueOrDefault() - 268435456L) : new long?();
        }
        if (!nextSortOrder.HasValue)
        {
          long? nullable = frontSortOrder;
          nextSortOrder = nullable.HasValue ? new long?(nullable.GetValueOrDefault() + 268435456L) : new long?();
        }
        columnId = this.ViewModel.ProjectIdentity is ColumnProjectIdentity projectIdentity ? projectIdentity.ColumnId : (dropSection == null || !dropSection.Customized ? "" : dropSection.SectionId);
        for (int i = models.Count - 1; i >= 0; --i)
        {
          long sortOrder = frontSortOrder.Value + (nextSortOrder.Value - frontSortOrder.Value) * (long) (i + 1) / (long) (models.Count + 1);
          await TaskService.SetSortOrder(models[i].Id, sortOrder, columnId);
          UtilLog.Info("TaskListChangeSortOrder : Task " + models[i].Id + " order " + sortOrder.ToString());
          if (dragModel.Level > dragModel.OriginLevel && dragModel.IsTask)
            await TaskDao.CheckChildrenLevel(dragModel.Id, dragModel.Level + this._parentLevel);
        }
        columnId = (string) null;
      }
    }

    private async Task ResetTaskSortOrders(List<DisplayItemModel> siblings)
    {
      long sortOrder = 0;
      foreach (DisplayItemModel sibling in siblings)
      {
        await TaskService.SetSortOrder(sibling.Id, sortOrder);
        sortOrder += 268435456L;
      }
    }

    private List<DisplayItemModel> GetSiblings(Section section)
    {
      if (section == null)
        return this.DisplayModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.Section == null)).ToList<DisplayItemModel>();
      List<DisplayItemModel> siblings = new List<DisplayItemModel>();
      foreach (DisplayItemModel displayItemModel in this.DisplayModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => !i.IsSection && i.Section?.SectionId == section.SectionId)).ToList<DisplayItemModel>())
      {
        siblings.Add(displayItemModel);
        siblings.AddRange((IEnumerable<DisplayItemModel>) displayItemModel.GetChildrenModels(true));
      }
      return siblings;
    }

    private async Task SetModelParent(
      List<DisplayItemModel> models,
      DisplayItemModel front,
      int dropLevel)
    {
      if (models == null)
        ;
      else
      {
        string parentId = "";
        if (front == null || !front.IsTask || dropLevel == 0)
        {
          if (this.ViewModel.ProjectIdentity is ParentTaskIdentity projectIdentity1)
            parentId = projectIdentity1.Id;
        }
        else
        {
          parentId = front.Level == dropLevel ? front.ParentId : front.Id;
          int parentStatus = 0;
          if (parentId == front.Id)
          {
            if (TaskCache.GetTaskLevel(parentId) >= 4)
            {
              UtilLog.Info("TaskList.SetParent error, parent.level = 4");
              return;
            }
            parentStatus = front.Status;
          }
          else
          {
            TaskModel thinTaskById = await TaskDao.GetThinTaskById(parentId);
            if (thinTaskById != null)
              parentStatus = thinTaskById.status;
          }
          if (parentStatus != 0)
          {
            int num = await TaskService.BatchCompleteTasks(models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Status == 0)).Select<DisplayItemModel, string>((Func<DisplayItemModel, string>) (m => m.Id)).ToList<string>(), true) ? 1 : 0;
          }
          ProjectIdentity projectIdentity2 = this.ViewModel.ProjectIdentity;
          if (projectIdentity2 is ColumnProjectIdentity columnProjectIdentity)
            projectIdentity2 = columnProjectIdentity.Project;
          if (!(projectIdentity2 is NormalProjectIdentity) && !(projectIdentity2 is GroupProjectIdentity))
            SmartListTaskFoldHelper.UpdateStatus(parentId, this.ViewModel.ProjectIdentity.CatId, false);
          else
            await TaskService.FoldTask(parentId, true);
        }
        List<string> parentChangedIds = new List<string>();
        bool canUndo = true;
        models.ForEach((Action<DisplayItemModel>) (m =>
        {
          if (m.IsNote)
            return;
          if (!Utils.IsEqualString(m.ParentId, parentId) && (m.OriginLevel != 0 || m.Level != 0))
          {
            DisplayItemModel parent = m.Parent;
            if ((parent != null ? (parent.IsTask ? 1 : 0) : 0) != 0)
              m.Parent?.RemoveItem(m);
            m.SourceViewModel.ParentId = parentId;
            parentChangedIds.Add(m.Id);
          }
          if (!(m.Section?.SectionId != front?.Section?.SectionId))
            return;
          canUndo = false;
        }));
        if (!string.IsNullOrEmpty(parentId))
          await TaskDragHelper.CheckChildrenLevelAndSortOrder(parentId);
        if (parentChangedIds.Count <= 0)
          ;
        else
        {
          UtilLog.Info("TaskList.SetParent " + parentChangedIds.Join<string>(";") + ",parent " + parentId + " from:TaskDragDrop");
          await TaskDao.BatchUpdateParent(parentChangedIds, parentId, canUndo: canUndo);
          TaskChangeNotifier.NotifyTaskParentChanged(parentChangedIds);
        }
      }
    }

    private void OnCheckItemDragging(object sender, DragMouseEvent mouseEvent)
    {
      bool flag = false;
      TaskView parent = Utils.FindParent<TaskView>((DependencyObject) this);
      if (parent == null)
        return;
      double x = mouseEvent.MouseArg.GetPosition((IInputElement) parent).X;
      double width = parent.Width;
      double? actualWidth = parent.GetTaskDetail()?.ActualWidth;
      double? nullable = actualWidth.HasValue ? new double?(width - actualWidth.GetValueOrDefault()) : new double?();
      double valueOrDefault = nullable.GetValueOrDefault();
      if (x > valueOrDefault & nullable.HasValue)
        flag = true;
      MouseEventArgs mouseArg = mouseEvent.MouseArg;
      TaskDragHelpModel.DragHelp.IsDragging = true;
      if (this._dragCheckItem == null)
        this._dragCheckItem = new DisplayItemModel()
        {
          SourceViewModel = new TaskBaseViewModel()
          {
            Type = DisplayType.Task,
            Id = "checkItem2task"
          },
          Dragging = true,
          Level = 0,
          InDetail = this.ViewModel.InDetail
        };
      this.TrySwitchModel(mouseArg, flag ? new System.Windows.Point(-10.0, -10.0) : mouseArg.GetPosition((IInputElement) this._taskList), true);
    }

    public bool OnCheckItemDrop(string itemId)
    {
      if (this._dragCheckItem == null || !this.DisplayModels.Contains(this._dragCheckItem))
        return false;
      this.OnCheckItemDrop((object) null, itemId);
      return true;
    }

    private async void OnCheckItemDrop(object sender, string itemId)
    {
      TaskListView taskListView = this;
      DisplayItemModel dragCheckItem;
      DisplayItemModel frontItem;
      DisplayItemModel nextItem;
      Section dropSection;
      if (taskListView.ViewModel.ProjectIdentity is AssignToMeProjectIdentity)
      {
        dragCheckItem = (DisplayItemModel) null;
        frontItem = (DisplayItemModel) null;
        nextItem = (DisplayItemModel) null;
        dropSection = (Section) null;
      }
      else if (taskListView.DisplayModels == null)
      {
        dragCheckItem = (DisplayItemModel) null;
        frontItem = (DisplayItemModel) null;
        nextItem = (DisplayItemModel) null;
        dropSection = (Section) null;
      }
      else
      {
        dragCheckItem = taskListView._dragCheckItem;
        if (dragCheckItem == null)
        {
          dragCheckItem = (DisplayItemModel) null;
          frontItem = (DisplayItemModel) null;
          nextItem = (DisplayItemModel) null;
          dropSection = (Section) null;
        }
        else if (!taskListView.DisplayModels.Contains(dragCheckItem))
        {
          dragCheckItem = (DisplayItemModel) null;
          frontItem = (DisplayItemModel) null;
          nextItem = (DisplayItemModel) null;
          dropSection = (Section) null;
        }
        else
        {
          frontItem = (DisplayItemModel) null;
          nextItem = (DisplayItemModel) null;
          int index = taskListView.DisplayModels.IndexOf(dragCheckItem);
          if (index >= 0)
          {
            for (int index1 = index - 1; index1 >= 0; --index1)
            {
              DisplayItemModel displayModel = taskListView.DisplayModels[index1];
              if (displayModel.Level <= dragCheckItem.Level && displayModel.IsTask || !displayModel.IsTask)
              {
                frontItem = displayModel;
                break;
              }
            }
            for (int index2 = index + 1; index2 < taskListView.DisplayModels.Count; ++index2)
            {
              DisplayItemModel displayModel = taskListView.DisplayModels[index2];
              if (!displayModel.IsSection && displayModel.Level >= dragCheckItem.Level)
              {
                if (displayModel.Level == dragCheckItem.Level && displayModel.IsTask)
                {
                  nextItem = displayModel;
                  break;
                }
              }
              else
                break;
            }
          }
          dropSection = frontItem?.Section ?? nextItem?.Section;
          if (frontItem != null || nextItem != null)
          {
            TaskDetailItemModel checkItem = await TaskDetailItemDao.GetChecklistItemById(itemId);
            if (checkItem == null)
            {
              dragCheckItem = (DisplayItemModel) null;
              frontItem = (DisplayItemModel) null;
              nextItem = (DisplayItemModel) null;
              dropSection = (Section) null;
              return;
            }
            dragCheckItem.SourceViewModel.Title = checkItem.title;
            TaskModel primaryTask = await TaskDao.GetTaskById(checkItem.TaskServerId);
            if (primaryTask == null)
            {
              dragCheckItem = (DisplayItemModel) null;
              frontItem = (DisplayItemModel) null;
              nextItem = (DisplayItemModel) null;
              dropSection = (Section) null;
              return;
            }
            TimeData timeData1 = new TimeData();
            if (!Utils.IsEmptyDate(checkItem.startDate))
            {
              timeData1.StartDate = checkItem.startDate;
              timeData1.IsAllDay = checkItem.isAllDay;
              timeData1.RepeatFlag = primaryTask.repeatFlag;
              timeData1.RepeatFrom = primaryTask.repeatFrom;
              if (checkItem.isAllDay.HasValue && checkItem.isAllDay.Value)
                timeData1.Reminders = new List<TaskReminderModel>()
                {
                  new TaskReminderModel() { trigger = "TRIGGER:P0DT9H0M0S" }
                };
              else
                timeData1.Reminders = new List<TaskReminderModel>()
                {
                  new TaskReminderModel() { trigger = "TRIGGER:PT0S" }
                };
            }
            else if (!Utils.IsEmptyDate(primaryTask.startDate))
            {
              timeData1.StartDate = primaryTask.startDate;
              timeData1.DueDate = primaryTask.dueDate;
              timeData1.IsAllDay = primaryTask.isAllDay;
              timeData1.RepeatFlag = primaryTask.repeatFlag;
              timeData1.RepeatFrom = primaryTask.repeatFrom;
              TimeData timeData2 = timeData1;
              timeData2.Reminders = await TaskReminderDao.GetRemindersByTaskId(primaryTask.id);
              timeData2 = (TimeData) null;
            }
            timeData1.TimeZone = !(dropSection?.SectionId == "nodate") ? new TimeZoneViewModel((!timeData1.IsAllDay.HasValue || !timeData1.IsAllDay.Value) && primaryTask.Floating, primaryTask.timeZone) : TimeZoneData.LocalTimeZoneModel;
            string str = "";
            if (frontItem == null || !frontItem.IsTask || dragCheckItem.Level == 0)
            {
              if (taskListView.ViewModel.ProjectIdentity is ParentTaskIdentity projectIdentity)
                str = projectIdentity.Id;
            }
            else
              str = frontItem.Level < dragCheckItem.Level ? frontItem.Id : frontItem.ParentId;
            TaskPrimaryProperty primaryTaskProperty = new TaskPrimaryProperty();
            primaryTaskProperty.Priority = new int?(primaryTask.priority);
            primaryTaskProperty.ProjectId = primaryTask.projectId;
            primaryTaskProperty.Tags = TagSerializer.ToTags(primaryTask.tag);
            primaryTaskProperty.TimeData = timeData1;
            primaryTaskProperty.ParentId = str;
            bool below = frontItem != null && frontItem.Level == dragCheckItem.Level;
            TaskPrimaryProperty taskProperty = await SubtaskToTaskHelper.GetTaskPrimaryProperty(primaryTaskProperty, taskListView.ViewModel.ProjectIdentity, below ? frontItem : nextItem, below, dropSection, dragCheckItem.Level == 0);
            taskListView.ClearDropSection();
            if (!await ProChecker.CheckTaskLimit(taskProperty.ProjectId))
            {
              TaskModel task = await TaskService.SubtaskToTask(itemId, taskProperty, dropSection == null || !dropSection.Customized ? "" : dropSection.SectionId);
              if (task != null)
              {
                DisplayItemModel dragModel = new DisplayItemModel(TaskCache.SafeGetTaskViewModel(task));
                dragModel.Level = dragCheckItem.Level;
                dragModel.InDetail = taskListView.ViewModel.InDetail;
                taskListView.DisplayModels[index] = dragModel;
                taskListView.ChangeSortOrder(frontItem, nextItem, dragModel, new List<DisplayItemModel>()
                {
                  dragModel
                });
              }
            }
            SyncManager.TryDelaySync();
            checkItem = (TaskDetailItemModel) null;
            primaryTask = (TaskModel) null;
            timeData1 = (TimeData) null;
            taskProperty = (TaskPrimaryProperty) null;
          }
          taskListView._dragCheckItem = (DisplayItemModel) null;
          dragCheckItem = (DisplayItemModel) null;
          frontItem = (DisplayItemModel) null;
          nextItem = (DisplayItemModel) null;
          dropSection = (Section) null;
        }
      }
    }

    public void ClearDropTarget() => this.ClearDropSection();

    private void ClearDropSection()
    {
      if (this.DisplayModels == null)
        return;
      foreach (DisplayItemModel displayModel in (Collection<DisplayItemModel>) this.DisplayModels)
      {
        displayModel.Selected = false;
        displayModel.Dragging = false;
      }
    }

    public void OnKanbanItemDragMove(DisplayItemModel draggingTask, MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this._taskList);
      if (position.Y < 0.0)
        this.AutoScrollOnDrag(true);
      else if (position.Y > this._taskList.ActualHeight)
        this.AutoScrollOnDrag(false);
      if (this._dragModel == null || this._dragModel.Id != draggingTask.Id)
      {
        this._dragModel = this.DisplayModels.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.Id == draggingTask.Id));
        if (this._dragModel != null)
          this._dragStartIndex = this.DisplayModels.IndexOf(this._dragModel);
      }
      this.TrySwitchModel(e, position, false);
    }

    private void AutoScrollOnDrag(bool isTop)
    {
      if ((DateTime.Now - this._autoScrollTime).TotalMilliseconds <= 150.0)
        return;
      ScrollViewer singleVisualChildren = Utils.FindSingleVisualChildren<ScrollViewer>((DependencyObject) this._taskList);
      singleVisualChildren.ScrollToVerticalOffset(singleVisualChildren.VerticalOffset + (isTop ? -1.0 : 1.0));
      this._autoScrollTime = DateTime.Now;
    }

    public void StartDragSection(DisplayItemModel model)
    {
      this.SetDragPopup(true);
      ObservableCollection<DisplayItemModel> displayModels = this.DisplayModels;
      // ISSUE: explicit non-virtual call
      if ((displayModels != null ? (__nonvirtual (displayModels.Contains(model)) ? 1 : 0) : 0) == 0)
        return;
      if (model.IsOpen)
        this.ViewModel.OpenOrCloseSection(new SectionStatus()
        {
          SectionId = model.Section.SectionId,
          IsOpen = false
        }, false);
      this._dragCurrentIndex = this.DisplayModels.IndexOf(model);
      DisplayItemModel displayItemModel = DisplayItemModel.Copy(model);
      displayItemModel.Dragging = false;
      displayItemModel.UnderTaskItem = false;
      this._taskDragPopup.DataContext = (object) displayItemModel;
      this._taskDragPopup.HorizontalOffset = 0.0;
      this._dragStartX = 0.0;
      this._taskDragPopup.IsOpen = true;
      Mouse.OverrideCursor = Cursors.Hand;
      TaskDragHelpModel.DragHelp.IsDragging = true;
    }

    public void SetParentLevel(int level) => this._parentLevel = level;

    private void BindEvents()
    {
      DragEventManager.CheckItemDragEvent += new EventHandler<DragMouseEvent>(this.OnCheckItemDragging);
      DragEventManager.CheckItemDrop += new EventHandler<string>(this.OnCheckItemDrop);
      DataChangedNotifier.ProjectChanged += new EventHandler(this.OnProjectChanged);
      CalendarEventChangeNotifier.TitleChanged += new EventHandler<TextExtra>(this.OnCalendarTitleChanged);
      CalendarEventChangeNotifier.SummaryChanged += new EventHandler<TextExtra>(this.OnCalendarSummaryChanged);
      CalendarEventChangeNotifier.Deleted += new EventHandler<string>(this.OnEventChanged);
      CalendarEventChangeNotifier.Restored += new EventHandler<string>(this.OnEventChanged);
      CalendarEventChangeNotifier.Changed += new EventHandler<CalendarEventModel>(this.OnEventChanged);
      CalendarEventChangeNotifier.RemoteChanged += new EventHandler(this.OnEventChanged);
      DataChangedNotifier.HabitSkip += new EventHandler<string>(this.OnHabitSkip);
      if (!(this.DataContext is TaskListViewModel dataContext) || !dataContext.IsList)
        return;
      DataChangedNotifier.HabitCheckInChanged += new EventHandler<HabitCheckInModel>(this.OnCheckInChanged);
      DataChangedNotifier.HabitsChanged += new EventHandler(this.OnHabitsChanged);
    }

    private void UnbindEvents()
    {
      this.ViewModel.Items.Clear();
      DragEventManager.CheckItemDragEvent -= new EventHandler<DragMouseEvent>(this.OnCheckItemDragging);
      DragEventManager.CheckItemDrop -= new EventHandler<string>(this.OnCheckItemDrop);
      DataChangedNotifier.ProjectChanged -= new EventHandler(this.OnProjectChanged);
      CalendarEventChangeNotifier.TitleChanged -= new EventHandler<TextExtra>(this.OnCalendarTitleChanged);
      CalendarEventChangeNotifier.SummaryChanged -= new EventHandler<TextExtra>(this.OnCalendarSummaryChanged);
      CalendarEventChangeNotifier.Deleted -= new EventHandler<string>(this.OnEventChanged);
      CalendarEventChangeNotifier.Restored -= new EventHandler<string>(this.OnEventChanged);
      CalendarEventChangeNotifier.Changed -= new EventHandler<CalendarEventModel>(this.OnEventChanged);
      CalendarEventChangeNotifier.RemoteChanged -= new EventHandler(this.OnEventChanged);
      DataChangedNotifier.HabitCheckInChanged -= new EventHandler<HabitCheckInModel>(this.OnCheckInChanged);
      DataChangedNotifier.HabitsChanged -= new EventHandler(this.OnHabitsChanged);
      DataChangedNotifier.HabitSkip -= new EventHandler<string>(this.OnHabitSkip);
    }

    private void OnHabitSkip(object sender, string e) => this.OnHabitSkipped(e);

    private void OnProjectChanged(object sender, EventArgs e)
    {
      if (!(this.DataContext is TaskListViewModel dataContext) || dataContext.SortOption == null || !dataContext.SortOption.ContainsSortType("project"))
        return;
      this.LoadAsync();
    }

    private void OnCalendarTitleChanged(object sender, TextExtra e)
    {
      List<TaskBaseViewModel> sourceModels = this.ViewModel.SourceModels;
      List<TaskBaseViewModel> list = sourceModels != null ? sourceModels.ToList<TaskBaseViewModel>() : (List<TaskBaseViewModel>) null;
      if (list == null)
        return;
      foreach (TaskBaseViewModel taskBaseViewModel in list)
      {
        if (taskBaseViewModel.EntityId == e.Id)
          taskBaseViewModel.Title = e.Text;
      }
    }

    private void OnCalendarSummaryChanged(object sender, TextExtra e)
    {
      List<TaskBaseViewModel> sourceModels = this.ViewModel.SourceModels;
      List<TaskBaseViewModel> list = sourceModels != null ? sourceModels.ToList<TaskBaseViewModel>() : (List<TaskBaseViewModel>) null;
      if (list == null)
        return;
      foreach (TaskBaseViewModel taskBaseViewModel in list)
      {
        if (taskBaseViewModel.EntityId == e.Id)
          taskBaseViewModel.Content = e.Text;
      }
    }

    private void CheckSortByModifiedTime(DisplayItemModel model)
    {
      if (model == null || !(this.ViewModel.SortOption.orderBy == "modifiedTime") || model.IsPinned)
        return;
      int index1 = this.ViewModel.Items.IndexOf(model);
      if (index1 <= 0)
        return;
      int index2 = 0;
      DisplayItemModel displayItemModel = this.ViewModel.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => m.IsSection && model.Section == m.Section));
      if (displayItemModel != null)
        index2 = this.ViewModel.Items.IndexOf(displayItemModel) + 1;
      if (index1 == index2)
        return;
      ITaskOperation focusingItem = this.GetFocusingItem();
      int caretIndex = this._caretIndex;
      this.ViewModel.Items.RemoveAt(index1);
      model.CaretIndex = focusingItem == null ? new int?() : new int?(caretIndex);
      this.ViewModel.Items.Insert(index2, model);
      this._taskList.ScrollIntoView((object) model);
    }

    private void OnEventChanged(object sender, object e) => this.LoadAsync(ignoreFocus: true);

    private void OnCheckInChanged(object sender, HabitCheckInModel checkInModel)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (!(checkInModel.CheckinStamp == DateTime.Today.ToString("yyyyMMdd")) || !(this.DataContext is TaskListViewModel dataContext2) || !(dataContext2.ProjectIdentity is TodayProjectIdentity) && !(dataContext2.ProjectIdentity is WeekProjectIdentity))
          return;
        this.LoadAsync(ignoreFocus: true);
      }));
    }

    private void OnHabitsChanged(object sender, EventArgs e)
    {
      this.Dispatcher.Invoke((Action) (() =>
      {
        if (!(this.DataContext is TaskListViewModel dataContext2) || !(dataContext2.ProjectIdentity is TodayProjectIdentity) && !(dataContext2.ProjectIdentity is WeekProjectIdentity))
          return;
        this.LoadAsync(ignoreFocus: true);
      }));
    }

    public async void ShowRecordPopup(UIElement element, double step, string unit)
    {
      if (this._recordPopup != null)
      {
        this._recordPopup.BeginAnimation(Popup.VerticalOffsetProperty, (AnimationTimeline) null);
        this._recordPopup.IsOpen = false;
        this._recordPopup = (Popup) null;
      }
      if (element == null)
        return;
      Popup popup1 = new Popup();
      popup1.Placement = PlacementMode.Top;
      popup1.VerticalOffset = 0.0;
      popup1.StaysOpen = true;
      popup1.HorizontalOffset = -5.0;
      popup1.AllowsTransparency = true;
      popup1.Opacity = 0.0;
      Popup popup2 = popup1;
      popup2.PlacementTarget = element;
      StackPanel stackPanel = new StackPanel();
      Border border1 = new Border();
      border1.Height = 30.0;
      border1.CornerRadius = new CornerRadius(4.0);
      border1.Effect = (Effect) new DropShadowEffect()
      {
        BlurRadius = 10.0,
        Direction = 300.0,
        ShadowDepth = 2.0,
        Opacity = 0.2
      };
      Border element1 = border1;
      element1.SetResourceReference(Panel.BackgroundProperty, (object) "PrimaryColor");
      Border border2 = element1;
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 13.0;
      textBlock.Foreground = (Brush) Brushes.White;
      textBlock.Opacity = 0.8;
      textBlock.VerticalAlignment = VerticalAlignment.Center;
      textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
      textBlock.HorizontalAlignment = HorizontalAlignment.Center;
      textBlock.Margin = new Thickness(8.0, 0.0, 8.0, 0.0);
      textBlock.Text = "+" + step.ToString() + " " + HabitUtils.GetUnitText(unit);
      border2.Child = (UIElement) textBlock;
      stackPanel.Children.Add((UIElement) element1);
      Path path = UiUtils.CreatePath("IcHabitAddStep", "PrimaryColor", "");
      path.Stretch = Stretch.None;
      path.HorizontalAlignment = HorizontalAlignment.Center;
      path.Margin = new Thickness(0.0, -2.0, 0.0, 0.0);
      stackPanel.Children.Add((UIElement) path);
      popup2.Child = (UIElement) stackPanel;
      popup2.IsOpen = true;
      this._recordPopup = popup2;
      this.BeginRecordAnimation(popup2);
    }

    private void BeginRecordAnimation(Popup popup)
    {
      popup.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(0.0), 1.0, 100));
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(1.0), 0.0, 800);
      doubleAnimation1.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(800.0));
      popup.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) doubleAnimation1);
      DoubleAnimation doubleAnimation2 = AnimationUtils.GetDoubleAnimation(new double?(0.0), -30.0, 1000);
      doubleAnimation2.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(800.0));
      doubleAnimation2.Completed += (EventHandler) ((sender, args) =>
      {
        popup.IsOpen = false;
        this._recordPopup = (Popup) null;
      });
      popup.BeginAnimation(Popup.VerticalOffsetProperty, (AnimationTimeline) doubleAnimation2);
    }

    public void ManuallyRecordCheckIn(
      UIElement element,
      DisplayItemModel model,
      EventHandler<double> onCheckInPopupSave)
    {
      if (this._checkInPopup != null)
      {
        this._checkInPopup.IsOpen = false;
      }
      else
      {
        this._checkInPopup = new Popup()
        {
          PlacementTarget = element,
          Placement = PlacementMode.Top,
          VerticalOffset = 8.0,
          StaysOpen = false,
          AllowsTransparency = true
        };
        this._checkInPopup.Closed += (EventHandler) ((o, e) => this._checkInPopup = (Popup) null);
        ManualRecordCheckinControl recordCheckinControl = new ManualRecordCheckinControl();
        this._checkInPopup.Child = (UIElement) recordCheckinControl;
        this._checkInPopup.IsOpen = true;
        this._checkInPopup.Focus();
        recordCheckinControl.Init(model.Habit.Unit);
        recordCheckinControl.Save += onCheckInPopupSave;
        recordCheckinControl.Save += (EventHandler<double>) ((o, e) =>
        {
          onCheckInPopupSave(o, e);
          if (this._checkInPopup == null)
            return;
          this._checkInPopup.IsOpen = false;
        });
        recordCheckinControl.Cancel += (EventHandler) ((o, e) =>
        {
          if (this._checkInPopup == null)
            return;
          this._checkInPopup.IsOpen = false;
        });
      }
    }

    public event TaskListView.SelectDelegate ItemSelect;

    private void UpdateFocus()
    {
      Window window = Window.GetWindow((DependencyObject) this);
      if (window != null && window.Equals((object) App.Window))
        App.Window.TryFocus();
      else
        window?.Focus();
    }

    public void BatchShiftSelected(string taskId) => this.BatchShiftSelect(taskId);

    public void BatchCtrlSelected(string taskId) => this.BatchCtrlSelect(taskId);

    private void BatchShiftSelect(string taskId, int currentIndex = -1)
    {
      this.ViewModel.SetFirstBatchSelectId(taskId);
      currentIndex = currentIndex < 0 ? this.ViewModel.GetFirstBatchSelectIndex() : currentIndex;
      int currentIndex1 = this.GetCurrentIndex(taskId);
      if (currentIndex < 0)
        currentIndex = currentIndex1;
      if (this.DataContext is TaskListViewModel dataContext && dataContext.ProjectIdentity is NormalProjectIdentity projectIdentity)
      {
        ProjectModel project = projectIdentity.Project;
        if (project != null && project.closed.GetValueOrDefault())
          return;
      }
      int num1 = Math.Min(currentIndex, currentIndex1);
      int num2 = Math.Max(currentIndex, currentIndex1);
      for (int index = 0; index < this.ViewModel.Items.Count; ++index)
      {
        DisplayItemModel displayItemModel = this.ViewModel.Items[index];
        if (index >= num1 && index <= num2 && displayItemModel.CanBatchSelect)
        {
          displayItemModel.Selected = true;
          displayItemModel.InBatchSelected = true;
        }
        else
        {
          displayItemModel.Selected = false;
          displayItemModel.InBatchSelected = false;
        }
      }
      this.NotifyMultipleSelected();
      if (!this.ViewModel.InDetail)
        TaskDetailWindow.TryCloseWindow(true);
      this.ScrollToItemById(taskId);
      this.UpdateFocus();
    }

    private void BatchCtrlSelect(string taskId)
    {
      if (this.ViewModel.ProjectIdentity is NormalProjectIdentity projectIdentity)
      {
        ProjectModel project = projectIdentity.Project;
        if (project != null && project.closed.GetValueOrDefault())
          return;
      }
      this.ViewModel.SetFirstBatchSelectId(taskId);
      DisplayItemModel selectedModel = this.GetSelectedModel();
      foreach (DisplayItemModel displayItemModel in this.ViewModel.Items.ToList<DisplayItemModel>())
      {
        if (displayItemModel.Id == taskId)
        {
          if (selectedModel != null && selectedModel.Id == displayItemModel.Id && selectedModel.CanBatchSelect)
          {
            displayItemModel.Selected = true;
            if (this.ViewModel.InBatchSelect)
            {
              this.SelectItem(new ListItemSelectModel(selectedModel.Id, (string) null, selectedModel.Type, TaskSelectType.Click));
              return;
            }
            break;
          }
          if (displayItemModel.CanBatchSelect)
          {
            displayItemModel.Selected = !displayItemModel.Selected;
            if (selectedModel != null)
            {
              if (!selectedModel.CanBatchSelect)
              {
                if (selectedModel.Id != taskId)
                {
                  selectedModel.Selected = false;
                  break;
                }
                break;
              }
              break;
            }
            break;
          }
          break;
        }
      }
      this.NotifyMultipleSelected();
      if (!this.ViewModel.InDetail)
        TaskDetailWindow.TryCloseWindow(true);
      this.UpdateFocus();
    }

    private void SelectItem(string taskId, TaskSelectType type)
    {
      this.SelectItem(new ListItemSelectModel(taskId, string.Empty, DisplayType.Task, type));
    }

    private async void SelectItem(ListItemSelectModel selectModel)
    {
      DisplayItemModel displayItemModel = this.ViewModel.Items.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => !string.IsNullOrEmpty(item.Id) && item.Id == selectModel.Id));
      if (displayItemModel != null)
        displayItemModel.Selected = true;
      if (selectModel.Type == TaskSelectType.Navigate)
        this.DelaySelectItem(selectModel);
      else
        this.NotifyItemSelect(selectModel);
    }

    private void DelaySelectItem(ListItemSelectModel model)
    {
      DelayActionHandlerCenter.TryDoAction("TaskListDelaySelectItem", (EventHandler) ((o, e) => Utils.RunOnUiThread(this.Dispatcher, (Action) (() => this.NotifyItemSelect(model)))));
    }

    private void NotifyItemSelect(ListItemSelectModel model)
    {
      this.ViewModel.ExitBatchSelect(model?.Id);
      TaskListView.SelectDelegate itemSelect = this.ItemSelect;
      if (itemSelect == null)
        return;
      itemSelect(model);
    }

    private int GetCurrentIndex(string taskId) => this.ViewModel.GetCurrentIndex(taskId);

    public void SelectSubtask(IdExtra id, TaskSelectType type)
    {
      if (Utils.IfCtrlPressed() || Utils.IfShiftPressed())
        return;
      this.SelectItem(new ListItemSelectModel(id.TaskId, id.ItemId, DisplayType.Task, type));
    }

    public void SelectItem(string id, DisplayType itemType)
    {
      this.SelectItem(new ListItemSelectModel(id, string.Empty, itemType, TaskSelectType.Click));
    }

    public void OnTasksChanged(TasksChangeEventArgs e)
    {
      if (!(this.DataContext is TaskListViewModel dataContext) || dataContext.InKanban || dataContext.InDetail)
        return;
      if (dataContext.InMatrix && dataContext.SortOption.orderBy == "modifiedTime")
      {
        DisplayItemModel selectedModel = this.GetSelectedModel();
        if (selectedModel != null)
          this.CheckSortByModifiedTime(selectedModel);
      }
      e.DeletedChangedIds.AddRange(e.UndoDeletedIds);
      List<DisplayItemModel> list = dataContext.Items.ToList<DisplayItemModel>();
      if (this.CheckNeedReloadOnTasksChanged(e, dataContext, list))
      {
        this.LoadAsync(ignoreFocus: true);
      }
      else
      {
        if (e.SortOrderChangedIds.Any())
        {
          if (list.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => e.SortOrderChangedIds.Contains(m.TaskId))).ToList<DisplayItemModel>().Count > 0)
          {
            this.LoadAsync(ignoreFocus: true);
            return;
          }
          if (dataContext.InDetail)
          {
            ParentTaskIdentity parent = dataContext.ProjectIdentity as ParentTaskIdentity;
            if (parent != null && e.SortOrderChangedIds.ToList().Any<string>((Func<string, bool>) (id => TaskCache.ExistParent(id, parent.Id))))
            {
              this.LoadAsync(ignoreFocus: true);
              return;
            }
          }
        }
        dataContext.CheckTaskChanged(e);
      }
    }

    private bool CheckNeedReloadOnTasksChanged(
      TasksChangeEventArgs e,
      TaskListViewModel vm,
      List<DisplayItemModel> items)
    {
      if ((!e.BatchChangedIds.Any() || !CheckChangedIds(e.BatchChangedIds.Value)) && (!e.DeletedChangedIds.Any() || !(vm.ProjectIdentity is TrashProjectIdentity) && !CheckChangedIds(e.DeletedChangedIds.Value, false)) && (!e.PinChangedIds.Any() || vm.ProjectData.IsCompleted || vm.ProjectData.IsProjectClosed || vm.ProjectData.IsTrash || !CheckChangedIds(e.PinChangedIds.Value)))
      {
        if (e.AddIds.Any())
        {
          if (!(vm.ProjectIdentity is SearchProjectIdentity) && !(vm.ProjectIdentity is DateProjectIdentity))
          {
            if (this.ViewModel.Items.Count == 0 || this.ViewModel.Items.Any<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => !e.AddIds.Contains(model.Id))))
            {
              List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(vm.ProjectIdentity, e.AddIds.ToList());
              // ISSUE: explicit non-virtual call
              if ((matchedTasks != null ? (__nonvirtual (matchedTasks.Count) > 0 ? 1 : 0) : 0) != 0)
                goto label_9;
            }
          }
          else
            goto label_9;
        }
        if ((!e.StatusChangedIds.Any() || !vm.ProjectData.IsCompleted && !CheckChangedIds(e.StatusChangedIds.Value)) && (!e.KindChangedIds.Any() || !this.CheckKindChangedIds(e.KindChangedIds.Value, vm.ProjectIdentity, items)) && (!e.ProjectChangedIds.Any() || !CheckChangedIds(e.ProjectChangedIds.Value)) && (!e.PriorityChangedIds.Any() || !CheckChangedIds(e.PriorityChangedIds.Value)) && (!e.TagChangedIds.Any() || !CheckChangedIds(e.TagChangedIds.Value)) && (!e.DateChangedIds.Any() || !CheckChangedIds(e.DateChangedIds.Value)) && (!e.AssignChangedIds.Any() || !(vm.ProjectIdentity is FilterProjectIdentity) && !(vm.ProjectIdentity is AssignToMeProjectIdentity) && !(vm.ProjectIdentity is MatrixQuadrantIdentity) && !vm.ProjectIdentity.SortOption.ContainsSortType("assignee") || !CheckChangedIds(e.AssignChangedIds.Value, vm.ProjectIdentity.SortOption.ContainsSortType("assignee"))))
          return e.CheckItemChangedIds.Any() && LocalSettings.Settings.ShowSubtasks && (vm.ProjectIdentity is TodayProjectIdentity || vm.ProjectIdentity is TomorrowProjectIdentity || vm.ProjectIdentity is WeekProjectIdentity || vm.ProjectIdentity is MatrixQuadrantIdentity projectIdentity1 && Parser.ContainsDate(projectIdentity1.Quadrant.rule, true) || vm.ProjectIdentity is FilterProjectIdentity projectIdentity2 && Parser.ContainsDate(projectIdentity2.Filter.rule)) && CheckChangedIds(e.CheckItemChangedIds.Value);
      }
label_9:
      return true;

      bool CheckChangedIds(HashSet<string> ids, bool checkExist = true)
      {
        List<DisplayItemModel> list = items.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>();
        if (checkExist && list.Count > 0)
          return true;
        List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(vm.ProjectIdentity, ids.ToList<string>());
        int? count1 = list?.Count;
        int? count2 = matchedTasks?.Count;
        return !(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue);
      }
    }

    private bool CheckKindChangedIds(
      HashSet<string> ids,
      ProjectIdentity identity,
      List<DisplayItemModel> items)
    {
      List<DisplayItemModel> list = items.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>();
      if ((identity.SortOption.ContainsSortType("dueDate") || identity.SortOption.ContainsSortType("priority") || identity.SortOption.orderBy == "createdTime" || identity.SortOption.orderBy == "modifiedTime") && list.Any<DisplayItemModel>((Func<DisplayItemModel, bool>) (m =>
      {
        if (m.Type == DisplayType.Task && !(m.Section is TaskSection))
          return true;
        return m.Type == DisplayType.Note && !(m.Section is NoteSection);
      })))
        return true;
      switch (identity)
      {
        case FilterProjectIdentity _:
        case TodayProjectIdentity _:
        case TomorrowProjectIdentity _:
        case WeekProjectIdentity _:
        case MatrixQuadrantIdentity _:
          int? count1 = TaskViewModelHelper.GetMatchedTasks(identity, ids.ToList<string>())?.Count;
          int count2 = list.Count;
          return !(count1.GetValueOrDefault() == count2 & count1.HasValue);
        default:
          return false;
      }
    }

    public delegate void SelectDelegate(ListItemSelectModel model);
  }
}
