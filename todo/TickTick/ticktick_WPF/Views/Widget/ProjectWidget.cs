// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.ProjectWidget
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using KotlinModels;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sort;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomPopup;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.QuickAdd;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.TaskList;
using ticktick_WPF.Views.Undo;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class ProjectWidget : 
    UserControl,
    IToastShowWindow,
    IWidgetChild,
    INavigateProject,
    IComponentConnector
  {
    private WidgetWindow _parentWindow;
    private ProjectOrGroupPopup _popup;
    private QuickAddView _quickAddView;
    private readonly DelayActionHandler _delayReloadHandler = new DelayActionHandler(200);
    private WidgetSettings _currentWidgetSettings;
    private WidgetMorePopup _morePopup;
    private List<TaskBaseViewModel> _displayModels;
    internal ProjectWidget ProjectWindow;
    internal Border WidgetBackground;
    internal Grid WidgetGrid;
    internal Grid TitleGrid;
    internal StackPanel TitlePanel;
    internal EmjTextBlock ProjectTitle;
    internal Grid ChoseProjectGrid;
    internal Grid LockedOptionGrid;
    internal Button UnlockWidgetButton;
    internal Button SyncButton;
    internal StackPanel OptionsGrid;
    internal Button AddButton;
    internal Path SyncPath;
    internal Button MorePath;
    internal ContentControl TaskAddGrid;
    internal TextBlock EmptyText;
    internal Grid TaskListGrid;
    internal ListView TaskList;
    internal Grid UndoToastGrid;
    private bool _contentLoaded;

    public ProjectWidget(WidgetViewModel model)
    {
      this.InitSelector();
      this.InitializeComponent();
      this.DataContext = (object) model;
      this.Loaded += new RoutedEventHandler(this.OnWidgetLoaded);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      if (model.DisplayOption == "top")
      {
        this.SyncButton.SetResourceReference(FrameworkElement.ToolTipProperty, (object) "Sync");
        this.UnlockWidgetButton.SetResourceReference(FrameworkElement.ToolTipProperty, (object) "Unlock");
        this.MorePath.SetResourceReference(FrameworkElement.ToolTipProperty, (object) "More");
      }
      else
      {
        this.SyncButton.ToolTip = (object) null;
        this.UnlockWidgetButton.ToolTip = (object) null;
        this.MorePath.ToolTip = (object) null;
      }
    }

    public string ThemeId => this.Model.ThemeId;

    public ProjectIdentity ProjectIdentity { get; private set; }

    public bool IsLocked { get; private set; }

    private WidgetViewModel Model => (WidgetViewModel) this.DataContext;

    private ObservableCollection<DisplayItemModel> DisplayItemModels
    {
      get
      {
        return this.TaskList.ItemsSource is ObservableCollection<DisplayItemModel> itemsSource ? itemsSource : new ObservableCollection<DisplayItemModel>();
      }
    }

    public bool IsEditting { private get; set; }

    public void TryToastString(object obj, string toastString)
    {
      WindowToastHelper.ToastString(this.UndoToastGrid, toastString);
    }

    public async Task<bool> TryToastMoveControl(TaskModel task, ProjectModel project)
    {
      ProjectWidget navigate = this;
      if (task == null || project == null || MoveToastHelper.CheckTaskMatched(navigate.ProjectIdentity, task))
        return false;
      WindowToastHelper.ShowAndHideToast(navigate.UndoToastGrid, (FrameworkElement) new MoveToastControl(project.id, (INavigateProject) navigate, task.title, MoveToastType.Move));
      return true;
    }

    public void TryHideToast()
    {
      if (this.UndoToastGrid.Children.Count <= 0)
        return;
      if (this.UndoToastGrid.Children[0] is UndoToast child)
        child.OnFinished();
      this.UndoToastGrid.Children[0].Visibility = Visibility.Collapsed;
    }

    public async Task<bool> BatchDeleteTask(List<TaskModel> tasks)
    {
      return await TaskService.BatchDeleteTasks(tasks, undoGrid: this.UndoToastGrid);
    }

    public void TaskComplete(CloseUndoToast undo)
    {
      WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, (FrameworkElement) undo);
    }

    public async void TaskDeleted(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task != null)
      {
        task.deleted = 1;
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskOnDeletedChanged(task);
        UndoToast uiElement = new UndoToast();
        uiElement.InitTaskUndo(taskId, task.title);
        WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, (FrameworkElement) uiElement);
      }
      await Task.Delay(100);
      this.RemoveItem(taskId);
      task = (TaskModel) null;
    }

    public void ToastMoveProjectControl(string projectId, string taskName = null, MoveToastType moveType = MoveToastType.Move)
    {
    }

    public void ToastDeleteRecUndo(List<TaskDeleteRecurrenceUndoEntity> undoModels)
    {
    }

    public void Toast(FrameworkElement uiElement)
    {
      WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, uiElement);
    }

    bool IWidgetChild.IsEditing() => this.IsEditting;

    public void Save() => this.Model.Save();

    private void UnbindEvents()
    {
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      DataChangedNotifier.TaskDefaultChanged -= new EventHandler(this.OnDefaultChanged);
      DataChangedNotifier.ScheduleChanged -= new EventHandler(this.OnScheduleChanged);
      CalendarEventChangeNotifier.TitleChanged -= new EventHandler<TextExtra>(this.OnEventTitleChanged);
      CalendarEventChangeNotifier.Changed -= new EventHandler<CalendarEventModel>(this.Reload);
      CalendarEventChangeNotifier.Deleted -= new EventHandler<string>(this.Reload);
      CalendarEventChangeNotifier.Restored -= new EventHandler<string>(this.Reload);
      TimeChangeNotifier.DayChanged -= new EventHandler<EventArgs>(this.OnDayChanged);
      DataChangedNotifier.HabitCheckInChanged -= new EventHandler<HabitCheckInModel>(this.OnCheckInChanged);
      DataChangedNotifier.HabitsChanged -= new EventHandler(this.OnHabitsChanged);
      DataChangedNotifier.SyncDone -= new EventHandler<SyncResult>(this.OnSyncDone);
      DataChangedNotifier.ProjectChanged -= new EventHandler(this.OnProjectChanged);
      this._delayReloadHandler.StopAndClear();
    }

    private void BindEvents()
    {
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      DataChangedNotifier.TaskDefaultChanged += new EventHandler(this.OnDefaultChanged);
      DataChangedNotifier.ScheduleChanged += new EventHandler(this.OnScheduleChanged);
      CalendarEventChangeNotifier.TitleChanged += new EventHandler<TextExtra>(this.OnEventTitleChanged);
      CalendarEventChangeNotifier.Changed += new EventHandler<CalendarEventModel>(this.Reload);
      CalendarEventChangeNotifier.Deleted += new EventHandler<string>(this.Reload);
      CalendarEventChangeNotifier.Restored += new EventHandler<string>(this.Reload);
      TimeChangeNotifier.DayChanged += new EventHandler<EventArgs>(this.OnDayChanged);
      DataChangedNotifier.HabitCheckInChanged += new EventHandler<HabitCheckInModel>(this.OnCheckInChanged);
      DataChangedNotifier.HabitsChanged += new EventHandler(this.OnHabitsChanged);
      DataChangedNotifier.SyncDone += new EventHandler<SyncResult>(this.OnSyncDone);
      DataChangedNotifier.ProjectChanged += new EventHandler(this.OnProjectChanged);
      this._delayReloadHandler.SetAction(new EventHandler(this.TryReload));
    }

    private void OnProjectChanged(object sender, EventArgs e) => this.Reload();

    private async void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        if (e.BatchChangedIds.Any() && this.CheckChangeIds(e.BatchChangedIds.Value))
          this.LoadTasks(true);
        else if ((e.DeletedChangedIds.Any() || e.UndoDeletedIds.Any()) && (this.CheckTaskDeletedIds(e.DeletedChangedIds.Value) || this.CheckTaskDeletedIds(e.UndoDeletedIds.Value)))
          this.LoadTasks(true);
        else if (e.PinChangedIds.Any() && (this.DisplayItemModels == null || this.DisplayItemModels.Count == 0 || this.DisplayItemModels.Any<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => e.PinChangedIds.Contains(model.Id)))))
        {
          this.LoadTasks(true);
        }
        else
        {
          if (e.AddIds.Any() && (this.DisplayItemModels == null || this.DisplayItemModels.Count == 0 || this.DisplayItemModels.Any<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => !e.AddIds.Contains(model.Id)))))
          {
            List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, e.AddIds.ToList());
            // ISSUE: explicit non-virtual call
            if ((matchedTasks != null ? (__nonvirtual (matchedTasks.Count) > 0 ? 1 : 0) : 0) != 0)
            {
              this.LoadTasks(true);
              return;
            }
          }
          if (e.StatusChangedIds.Any())
          {
            List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, e.StatusChangedIds.ToList());
            // ISSUE: explicit non-virtual call
            if ((matchedTasks != null ? (__nonvirtual (matchedTasks.Count) > 0 ? 1 : 0) : 0) != 0)
            {
              this.LoadTasks(true);
              return;
            }
          }
          if (e.KindChangedIds.Any() && this.CheckKindChangedIds(e.KindChangedIds.Value))
            this.LoadTasks(true);
          else if (e.ProjectChangedIds.Any() && this.CheckProjectChangedIds(e.ProjectChangedIds.Value))
            this.LoadTasks(true);
          else if (e.PriorityChangedIds.Any() && this.CheckPriorityChangedIds(e.PriorityChangedIds.Value))
            this.LoadTasks(true);
          else if (e.TagChangedIds.Any() && this.CheckTagChangedIds(e.TagChangedIds.Value))
          {
            this.LoadTasks(true);
          }
          else
          {
            if (e.SortOrderChangedIds.Any())
            {
              ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
              List<DisplayItemModel> list = displayItemModels != null ? displayItemModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => e.SortOrderChangedIds.Contains(m.TaskId))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
              // ISSUE: explicit non-virtual call
              if ((list != null ? (__nonvirtual (list.Count) > 0 ? 1 : 0) : 0) != 0)
              {
                this.LoadTasks(true);
                return;
              }
            }
            if (e.DateChangedIds.Any() && this.CheckDateChangedIds(e.DateChangedIds.Value))
              this.LoadTasks(true);
            else if (e.AssignChangedIds.Any() && this.CheckAssignChangedIds(e.AssignChangedIds.Value))
              this.LoadTasks(true);
            else if (e.CheckItemChangedIds.Any() && this.CheckCheckItemsChanged(e.CheckItemChangedIds.Value))
            {
              this.LoadTasks(true);
            }
            else
            {
              if (!e.TasksOpenChangedIds.Any())
                return;
              this.CheckOpenChangedIds(e.TasksOpenChangedIds.Value, this.ProjectIdentity is NormalProjectIdentity || this.ProjectIdentity is GroupProjectIdentity || this.ProjectIdentity is ParentTaskIdentity || this.ProjectIdentity is MatrixQuadrantIdentity);
            }
          }
        }
      }));
    }

    private bool CheckChangeIds(HashSet<string> ids, bool checkCount = true)
    {
      ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
      List<DisplayItemModel> list = displayItemModels != null ? displayItemModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      // ISSUE: explicit non-virtual call
      if (checkCount && list != null && __nonvirtual (list.Count) > 0)
        return true;
      List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, ids.ToList<string>());
      int? count1 = list?.Count;
      int? count2 = matchedTasks?.Count;
      return !(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue);
    }

    private void CheckOpenChangedIds(HashSet<string> ids, bool useTaskOpen)
    {
      ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
      List<DisplayItemModel> models = (displayItemModels != null ? displayItemModels.ToList<DisplayItemModel>() : (List<DisplayItemModel>) null) ?? new List<DisplayItemModel>();
      List<DisplayItemModel> list = models.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>();
      if (list.Count == 0)
        return;
      HashSet<string> openedTaskIds = (HashSet<string>) null;
      if (!useTaskOpen)
        openedTaskIds = SmartListTaskFoldHelper.GetOpenedTaskIds(this.ProjectIdentity?.CatId);
      bool flag1 = false;
      foreach (DisplayItemModel displayItemModel in list)
      {
        // ISSUE: explicit non-virtual call
        bool flag2 = useTaskOpen ? displayItemModel.SourceViewModel.IsOpen : openedTaskIds != null && __nonvirtual (openedTaskIds.Contains(displayItemModel.Id));
        if (displayItemModel.IsOpen != flag2)
        {
          if (displayItemModel.IsOpen)
          {
            displayItemModel.GetChildrenModels(true).ForEach((Action<DisplayItemModel>) (m => models.Remove(m)));
          }
          else
          {
            List<DisplayItemModel> childrenModels = displayItemModel.GetChildrenModels(false, openedTaskIds);
            int num = models.IndexOf(displayItemModel);
            if (num >= 0)
            {
              for (int index = childrenModels.Count - 1; index >= 0; --index)
                models.Insert(num + 1, childrenModels[index]);
            }
          }
          flag1 = true;
          displayItemModel.IsOpen = flag2;
        }
      }
      if (!flag1)
        return;
      ItemsSourceHelper.SetItemsSource<DisplayItemModel>((ItemsControl) this.TaskList, models);
    }

    private bool CheckCheckItemsChanged(HashSet<string> ids)
    {
      if (!LocalSettings.Settings.ShowSubtasks)
        return false;
      switch (this.ProjectIdentity)
      {
        case TodayProjectIdentity _:
        case TomorrowProjectIdentity _:
        case WeekProjectIdentity _:
label_4:
          ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
          List<DisplayItemModel> list = displayItemModels != null ? displayItemModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.Id))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
          // ISSUE: explicit non-virtual call
          if ((list != null ? (__nonvirtual (list.Count) > 0 ? 1 : 0) : 0) != 0)
            return true;
          List<TaskBaseViewModel> matchedItems = TaskViewModelHelper.GetMatchedItems(this.ProjectIdentity, ids.ToList<string>());
          // ISSUE: explicit non-virtual call
          return matchedItems != null && __nonvirtual (matchedItems.Count) > 0;
        case FilterProjectIdentity filterProjectIdentity:
          if (!filterProjectIdentity.Filter.ContainsDate())
            break;
          goto label_4;
      }
      return false;
    }

    private bool CheckTaskDeletedIds(HashSet<string> ids)
    {
      if (this.ProjectIdentity is TrashProjectIdentity)
        return true;
      ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
      List<DisplayItemModel> list = displayItemModels != null ? displayItemModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      List<TaskBaseViewModel> matchedTasks = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, ids.ToList<string>());
      int? count1 = list?.Count;
      int? count2 = matchedTasks?.Count;
      return !(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue);
    }

    private bool CheckAssignChangedIds(HashSet<string> ids)
    {
      if (this.ProjectIdentity == null)
        return false;
      ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
      List<DisplayItemModel> list = displayItemModels != null ? displayItemModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      // ISSUE: explicit non-virtual call
      if (list != null && __nonvirtual (list.Count) > 0 && this.ProjectIdentity.SortOption.ContainsSortType("assignee"))
        return true;
      if (!(this.ProjectIdentity is FilterProjectIdentity) && !(this.ProjectIdentity is AssignToMeProjectIdentity) && !(this.ProjectIdentity is MatrixQuadrantIdentity))
        return false;
      int? count1 = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, ids.ToList<string>())?.Count;
      int? count2 = list?.Count;
      return !(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue);
    }

    private bool CheckDateChangedIds(HashSet<string> ids)
    {
      if (this.ProjectIdentity == null)
        return false;
      ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
      List<DisplayItemModel> list = displayItemModels != null ? displayItemModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      // ISSUE: explicit non-virtual call
      if (list != null && __nonvirtual (list.Count) > 0 && this.ProjectIdentity.SortOption.ContainsSortType("dueDate"))
        return true;
      if (!(this.ProjectIdentity is FilterProjectIdentity) && !(this.ProjectIdentity is TodayProjectIdentity) && !(this.ProjectIdentity is TomorrowProjectIdentity) && !(this.ProjectIdentity is WeekProjectIdentity) && !(this.ProjectIdentity is MatrixQuadrantIdentity))
        return false;
      int? count1 = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, ids.ToList<string>())?.Count;
      int? count2 = list?.Count;
      return !(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue);
    }

    private bool CheckTagChangedIds(HashSet<string> ids)
    {
      if (this.ProjectIdentity == null)
        return false;
      ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
      List<DisplayItemModel> list = displayItemModels != null ? displayItemModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      // ISSUE: explicit non-virtual call
      if (list != null && __nonvirtual (list.Count) > 0 && this.ProjectIdentity.SortOption.ContainsSortType("tag"))
        return true;
      if (!(this.ProjectIdentity is TagProjectIdentity))
      {
        if (this.ProjectIdentity is FilterProjectIdentity projectIdentity1)
        {
          List<string> tags = projectIdentity1.GetTags();
          // ISSUE: explicit non-virtual call
          if ((tags != null ? (__nonvirtual (tags.Count) > 0 ? 1 : 0) : 0) != 0)
            goto label_9;
        }
        if (this.ProjectIdentity is MatrixQuadrantIdentity projectIdentity2)
        {
          List<string> tags = projectIdentity2.GetTags();
          // ISSUE: explicit non-virtual call
          if ((tags != null ? (__nonvirtual (tags.Count) > 0 ? 1 : 0) : 0) != 0)
            goto label_9;
        }
        return false;
      }
label_9:
      int? count1 = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, ids.ToList<string>())?.Count;
      int? count2 = list?.Count;
      return !(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue);
    }

    private bool CheckPriorityChangedIds(HashSet<string> ids)
    {
      if (this.ProjectIdentity == null)
        return false;
      ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
      List<DisplayItemModel> list = displayItemModels != null ? displayItemModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      // ISSUE: explicit non-virtual call
      if (list != null && __nonvirtual (list.Count) > 0 && this.ProjectIdentity.SortOption.ContainsSortType("priority"))
        return true;
      if ((!(this.ProjectIdentity is FilterProjectIdentity projectIdentity1) || !projectIdentity1.GetDefaultPriority().HasValue) && (!(this.ProjectIdentity is MatrixQuadrantIdentity projectIdentity2) || !projectIdentity2.GetDefaultPriority().HasValue))
        return false;
      int? count1 = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, ids.ToList<string>())?.Count;
      int? count2 = list?.Count;
      return !(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue);
    }

    private bool CheckProjectChangedIds(HashSet<string> ids)
    {
      if (this.ProjectIdentity == null)
        return false;
      ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
      List<DisplayItemModel> list = displayItemModels != null ? displayItemModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      // ISSUE: explicit non-virtual call
      if (list != null && __nonvirtual (list.Count) > 0)
        return true;
      int? count1 = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, ids.ToList<string>())?.Count;
      int? count2 = list?.Count;
      return !(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue);
    }

    private bool CheckKindChangedIds(HashSet<string> ids)
    {
      if (this.ProjectIdentity == null)
        return false;
      ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
      List<DisplayItemModel> list = displayItemModels != null ? displayItemModels.Where<DisplayItemModel>((Func<DisplayItemModel, bool>) (m => ids.Contains(m.TaskId))).ToList<DisplayItemModel>() : (List<DisplayItemModel>) null;
      if ((this.ProjectIdentity.SortOption.groupBy == "dueDate" || this.ProjectIdentity.SortOption.groupBy == "priority" || this.ProjectIdentity.SortOption.groupBy == "createdTime" || this.ProjectIdentity.SortOption.groupBy == "modifiedTime") && list != null && list.Any<DisplayItemModel>((Func<DisplayItemModel, bool>) (m =>
      {
        if (m.Type == DisplayType.Task && m.Section is NoteSection)
          return true;
        return m.Type == DisplayType.Note && m.Section is TaskSection;
      })))
        return true;
      if (!(this.ProjectIdentity is FilterProjectIdentity) && !(this.ProjectIdentity is TodayProjectIdentity) && !(this.ProjectIdentity is TomorrowProjectIdentity) && !(this.ProjectIdentity is WeekProjectIdentity) && !(this.ProjectIdentity is MatrixQuadrantIdentity))
        return false;
      int? count1 = TaskViewModelHelper.GetMatchedTasks(this.ProjectIdentity, ids.ToList<string>())?.Count;
      int? count2 = list?.Count;
      return !(count1.GetValueOrDefault() == count2.GetValueOrDefault() & count1.HasValue == count2.HasValue);
    }

    private void OnScheduleChanged(object sender, EventArgs e)
    {
      if (!(this.ProjectIdentity is TodayProjectIdentity) && !(this.ProjectIdentity is TomorrowProjectIdentity) && !(this.ProjectIdentity is WeekProjectIdentity))
        return;
      this.Reload();
    }

    private void TryReload(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() => this.Reload()));
    }

    private void ChangeModelOpenStatus(DisplayItemModel model, bool useTaskOpen)
    {
      if (model.IsOpen)
      {
        model.GetChildrenModels(true)?.ForEach((Action<DisplayItemModel>) (child => this.DisplayItemModels.Remove(child)));
      }
      else
      {
        HashSet<string> openedTaskIds = (HashSet<string>) null;
        if (!useTaskOpen)
          openedTaskIds = SmartListTaskFoldHelper.GetOpenedTaskIds(this.ProjectIdentity?.CatId);
        List<DisplayItemModel> childrenModels = model.GetChildrenModels(false, openedTaskIds);
        int index = this.DisplayItemModels.IndexOf(model);
        if (index >= 0 && childrenModels != null)
          childrenModels.ForEach((Action<DisplayItemModel>) (child => this.DisplayItemModels.Insert(++index, child)));
      }
      model.IsOpen = !model.IsOpen;
    }

    private void OnSyncDone(object sender, SyncResult result)
    {
      if (!result.RemoteDataChanged)
        return;
      this.Reload();
    }

    private void OnHabitsChanged(object sender, EventArgs e)
    {
      if (!(this.ProjectIdentity is TodayProjectIdentity) && !(this.ProjectIdentity is WeekProjectIdentity))
        return;
      this.Reload();
    }

    private void OnCheckInChanged(object sender, HabitCheckInModel e)
    {
      if (!(e.CheckinStamp == DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture)) || !(this.ProjectIdentity is TodayProjectIdentity) && !(this.ProjectIdentity is WeekProjectIdentity))
        return;
      this.Reload();
    }

    private void OnDayChanged(object sender, EventArgs e) => this.Reload(true);

    private void OnEventTitleChanged(object sender, TextExtra extra)
    {
      this.NotifyTitleChanged(extra.Id, extra.Text);
    }

    private void Reload(object sender, CalendarEventModel model) => this.Reload();

    private void Reload(object sender, List<string> taskIds) => this.Reload();

    private void Reload(object sender, TaskModel task) => this.Reload();

    private void Reload(object sender, string itemId) => this.Reload();

    private void InitSelector()
    {
      this.Resources[(object) "WidgetListItemSelector"] = (object) new WidgetListItemSelector(this);
    }

    private async void OnWidgetLoaded(object sender, EventArgs e)
    {
      this.BindEvents();
      await this.LoadProjectIdentity();
      this.SetShowAdd(false);
      this.LoadTasks();
      this.LockWidget(this.Model.IsLocked);
    }

    private void LockWidget(bool isLocked)
    {
      if (this.IsLocked == isLocked)
        return;
      foreach (UIElement child in this.WidgetGrid.Children)
      {
        if (child.Equals((object) this.TitleGrid))
        {
          for (int index = 0; index < this.TitleGrid.Children.Count; ++index)
            this.TitleGrid.Children[index].IsHitTestVisible = !isLocked;
          this.LockedOptionGrid.IsHitTestVisible = isLocked;
        }
        else if (!child.Equals((object) this.TaskListGrid))
          child.IsHitTestVisible = !isLocked;
      }
      this.GetParentWindow().ResizeMode = isLocked ? ResizeMode.NoResize : ResizeMode.CanResize;
      this.OptionsGrid.Visibility = isLocked ? Visibility.Collapsed : Visibility.Visible;
      this.Model.IsLocked = isLocked;
      this.Model.Save();
      this.IsLocked = isLocked;
      if (isLocked)
        this.HideAddView();
      else
        this.SetShowAdd(false);
      this.LockedOptionGrid.Visibility = isLocked ? Visibility.Visible : Visibility.Collapsed;
      this.ChoseProjectGrid.Visibility = isLocked ? Visibility.Collapsed : Visibility.Visible;
      foreach (DisplayItemModel displayItemModel in (Collection<DisplayItemModel>) this.DisplayItemModels)
        displayItemModel.HitVisible = !isLocked;
    }

    private async void OnAddTaskClick(object sender, RoutedEventArgs e)
    {
      await Task.Delay(50);
      this.InitQuickAddView(true);
    }

    public void SetShowAdd(bool withAnim = true)
    {
      if (LocalSettings.Settings.ExtraSettings.PwShowAdd)
      {
        if (this.ProjectIdentity != null && this.ProjectIdentity.CanAddTask())
          this.InitQuickAddView(anim: withAnim);
        else
          this.HideAddView();
      }
      else if (this._quickAddView != null && this._quickAddView.IsLostFocus())
        this.HideAddView();
      this.SetAddButton();
    }

    private void HideAddView()
    {
      QuickAddView quickAddView = this._quickAddView;
      this._quickAddView = (QuickAddView) null;
      if (quickAddView == null)
        return;
      quickAddView.TaskAdded -= new EventHandler<TaskModel>(this.OnTaskAdded);
      quickAddView.TitleText.LostFocus -= new RoutedEventHandler(this.OnQuickAddLostFocus);
      DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(108.0), 0.0, 120);
      doubleAnimation.Completed += (EventHandler) ((o, a) =>
      {
        if (this.AddButton != null)
          this.AddButton.IsHitTestVisible = true;
        this.TaskAddGrid.Content = (object) null;
      });
      quickAddView.BeginAnimation(FrameworkElement.MaxHeightProperty, (AnimationTimeline) doubleAnimation);
    }

    private async void OnQuickAddLostFocus(object sender, RoutedEventArgs e)
    {
      await Task.Delay(100);
      if (this._quickAddView == null || this._quickAddView.TitleText.Focused)
        return;
      await this._quickAddView?.TryAddTaskOnLostFocus();
      this.HideAddView();
    }

    private void SetAddButton()
    {
      this.AddButton.Visibility = LocalSettings.Settings.ExtraSettings.PwShowAdd || !this.ProjectIdentity.CanAddTask() ? Visibility.Collapsed : Visibility.Visible;
    }

    public void ResetQuickAddView(bool keepText = true)
    {
      QuickAddView quickAddView = this._quickAddView;
      if (quickAddView == null)
        return;
      ProjectIdentity projectIdentity1 = this.ProjectIdentity;
      if (projectIdentity1 == null)
        return;
      SortOption sortOption = projectIdentity1.SortOption;
      this.ProjectIdentity = projectIdentity1.Copy(projectIdentity1);
      ProjectIdentity projectIdentity2 = this.ProjectIdentity;
      projectIdentity2.SortOption = sortOption;
      quickAddView.ResetView((IProjectTaskDefault) projectIdentity2, keepText: keepText);
    }

    private async Task InitQuickAddView(bool focus = false, bool resetIdentity = false, bool anim = true)
    {
      ProjectWidget projectWidget1 = this;
      ProjectIdentity projectIdentity = projectWidget1.ProjectIdentity;
      if (projectIdentity == null)
        return;
      if (!projectIdentity.CanAddTask() || projectWidget1.IsLocked)
      {
        projectWidget1.HideAddView();
      }
      else
      {
        if (resetIdentity)
        {
          SortOption sortOption = projectIdentity.SortOption;
          projectWidget1.ProjectIdentity = projectIdentity.Copy(projectIdentity);
          projectIdentity = projectWidget1.ProjectIdentity;
          projectIdentity.SortOption = sortOption;
        }
        if (projectWidget1._quickAddView == null)
        {
          int to = 108;
          ProjectWidget projectWidget2 = projectWidget1;
          QuickAddView quickAddView = new QuickAddView((IProjectTaskDefault) projectIdentity, QuickAddView.Scenario.Widget, projectWidget1.Model.ThemeId, focus);
          quickAddView.Margin = new Thickness(0.0, 8.0, 0.0, 8.0);
          projectWidget2._quickAddView = quickAddView;
          projectWidget1._quickAddView.VerticalAlignment = VerticalAlignment.Bottom;
          projectWidget1._quickAddView.TaskAdded += new EventHandler<TaskModel>(projectWidget1.OnTaskAdded);
          projectWidget1.TaskAddGrid.Content = (object) projectWidget1._quickAddView;
          if (anim)
          {
            projectWidget1._quickAddView.BeginAnimation(FrameworkElement.MaxHeightProperty, (AnimationTimeline) AnimationUtils.GetDoubleAnimation(new double?(0.0), (double) to, 240));
          }
          else
          {
            projectWidget1._quickAddView.BeginAnimation(FrameworkElement.MaxHeightProperty, (AnimationTimeline) null);
            projectWidget1._quickAddView.MaxHeight = (double) to;
          }
          projectWidget1._quickAddView.TitleText.EditBox.LostFocus += new RoutedEventHandler(projectWidget1.OnQuickAddLostFocus);
          if (focus)
            projectWidget1._quickAddView.FocusEnd();
        }
        if (!LocalSettings.Settings.ExtraSettings.PwShowAdd)
          return;
        projectWidget1._quickAddView.TitleText.EditBox.LostFocus -= new RoutedEventHandler(projectWidget1.OnQuickAddLostFocus);
        await Task.Delay(120);
        projectWidget1._quickAddView?.BeginAnimation(FrameworkElement.MaxHeightProperty, (AnimationTimeline) null);
        projectWidget1._quickAddView?.SetResourceReference(FrameworkElement.MaxHeightProperty, (object) 108);
      }
    }

    private void OnDefaultChanged(object sender, EventArgs e) => this.ResetQuickAddView();

    private void OnTaskAdded(object sender, TaskModel task) => this.LoadTasks();

    public void Reload(bool refreshQuickAddDate = false, bool force = false)
    {
      if (refreshQuickAddDate)
        this.ResetQuickAddView();
      this.LoadTasks(force);
    }

    private void LoadTasks(bool force = false)
    {
      ThreadUtil.DetachedRunOnUiThread((Action) (async () =>
      {
        if (this.IsEditting && !force || this.ProjectIdentity == null)
          return;
        if (this.ProjectIdentity is DeletedProjectIdentity || this.ProjectIdentity is ClosedProjectIdentity)
        {
          this.EmptyText.Visibility = Visibility.Visible;
        }
        else
        {
          WidgetViewModel widgetModel = this.Model;
          List<TaskBaseViewModel> models = await ProjectTaskDataProvider.GetDisplayModels(this.ProjectIdentity, showCompete: new bool?(!widgetModel.HideComplete));
          this._displayModels = models;
          int count1 = await TaskCountCache.TryGetCount(this.ProjectIdentity);
          widgetModel.CollapseTitle = this.ProjectTitle.Text + (count1 > 0 ? " (" + count1.ToString() + ")" : string.Empty);
          ObservableCollection<DisplayItemModel> displayModels = ProjectWidget.BuildDisplayItemModels((IEnumerable<TaskBaseViewModel>) models);
          this.TryAdjustSortType(this.ProjectIdentity is TagProjectIdentity projectIdentity3 && TagDao.IsParentTag(projectIdentity3.Tag));
          List<ColumnModel> columns = (List<ColumnModel>) null;
          if (this.ProjectIdentity is NormalProjectIdentity projectIdentity4 && projectIdentity4.SortOption.groupBy == Constants.SortType.sortOrder.ToString())
            columns = await ColumnDao.GetColumnsByProjectId(projectIdentity4.Project.id);
          ObservableCollection<DisplayItemModel> source = await SortHelper.Sort(displayModels, this.ProjectIdentity, columns: columns, showComplete: new bool?(!widgetModel.HideComplete));
          foreach (DisplayItemModel displayItemModel in (Collection<DisplayItemModel>) source)
            displayItemModel.CurrentProjectIdentity = this.ProjectIdentity;
          ItemsSourceHelper.SetItemsSource<DisplayItemModel>((ItemsControl) this.TaskList, source.ToList<DisplayItemModel>());
          this.EmptyText.Visibility = displayModels.Count > 0 ? Visibility.Collapsed : Visibility.Visible;
          await Task.Delay(1000);
          int count2 = await TaskCountCache.TryGetCount(this.ProjectIdentity);
          widgetModel.CollapseTitle = this.ProjectTitle.Text + (count2 > 0 ? " (" + count2.ToString() + ")" : string.Empty);
          widgetModel = (WidgetViewModel) null;
          models = (List<TaskBaseViewModel>) null;
          displayModels = (ObservableCollection<DisplayItemModel>) null;
        }
      }));
    }

    public void RemoveItem(DisplayItemModel model)
    {
      if (this.DisplayItemModels == null || model == null)
        return;
      this.DisplayItemModels.Remove(model);
      this._displayModels?.RemoveAll((Predicate<TaskBaseViewModel>) (t => t.Id == model.Id));
    }

    private void RemoveItem(string edittingTaskId)
    {
      if (this.DisplayItemModels != null && this.DisplayItemModels.Any<DisplayItemModel>())
      {
        DisplayItemModel displayItemModel = this.DisplayItemModels.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.IsTaskOrNote && i.Id == edittingTaskId));
        if (displayItemModel != null)
          this.DisplayItemModels.Remove(displayItemModel);
      }
      this._displayModels?.RemoveAll((Predicate<TaskBaseViewModel>) (t => t.Id == edittingTaskId));
    }

    private void TryAdjustSortType(bool isParentTag)
    {
      if (this.ProjectIdentity == null)
        return;
      if (this.ProjectIdentity is NormalProjectIdentity projectIdentity1)
      {
        if ((this.ProjectIdentity.SortOption.groupBy == Constants.SortType.project.ToString() ? 1 : (!(this.ProjectIdentity.SortOption.groupBy == Constants.SortType.assignee.ToString()) ? 0 : (!projectIdentity1.Project.IsShareList() ? 1 : 0))) != 0)
        {
          this.ProjectIdentity.SortOption.groupBy = Constants.SortType.sortOrder.ToString();
          this.ProjectIdentity.SortOption.orderBy = Constants.SortType.sortOrder.ToString();
        }
        if (projectIdentity1.Project.IsNote)
        {
          if (this.ProjectIdentity.SortOption.groupBy == Constants.SortType.priority.ToString() || this.ProjectIdentity.SortOption.groupBy == Constants.SortType.dueDate.ToString())
            this.ProjectIdentity.SortOption.groupBy = Constants.SortType.sortOrder.ToString();
          if (this.ProjectIdentity.SortOption.orderBy == Constants.SortType.priority.ToString() || this.ProjectIdentity.SortOption.orderBy == Constants.SortType.dueDate.ToString())
            this.ProjectIdentity.SortOption.orderBy = Constants.SortType.sortOrder.ToString();
        }
        if (!projectIdentity1.Project.IsNote && (this.ProjectIdentity.SortOption.orderBy == Constants.SortType.createdTime.ToString() || this.ProjectIdentity.SortOption.orderBy == Constants.SortType.modifiedTime.ToString()))
          this.ProjectIdentity.SortOption.orderBy = Constants.SortType.sortOrder.ToString();
      }
      if (this.ProjectIdentity is GroupProjectIdentity projectIdentity2)
      {
        if (projectIdentity2.DisplayKind == 1)
        {
          if (this.ProjectIdentity.SortOption.groupBy == Constants.SortType.priority.ToString() || this.ProjectIdentity.SortOption.groupBy == Constants.SortType.dueDate.ToString())
            this.ProjectIdentity.SortOption.groupBy = Constants.SortType.project.ToString();
          if (this.ProjectIdentity.SortOption.orderBy == Constants.SortType.priority.ToString() || this.ProjectIdentity.SortOption.orderBy == Constants.SortType.dueDate.ToString() || this.ProjectIdentity.SortOption.orderBy == Constants.SortType.sortOrder.ToString())
            this.ProjectIdentity.SortOption.orderBy = Constants.SortType.createdTime.ToString();
        }
        if (projectIdentity2.DisplayKind == 0 && (this.ProjectIdentity.SortOption.orderBy == Constants.SortType.createdTime.ToString() || this.ProjectIdentity.SortOption.orderBy == Constants.SortType.modifiedTime.ToString()))
          this.ProjectIdentity.SortOption.orderBy = Constants.SortType.dueDate.ToString();
      }
      if (this.ProjectIdentity is TagProjectIdentity && this.ProjectIdentity.SortOption.groupBy == Constants.SortType.tag.ToString() && !isParentTag)
        this.ProjectIdentity.SortOption.groupBy = Constants.SortType.project.ToString();
      if (this.ProjectIdentity is NormalProjectIdentity)
        return;
      if (this.ProjectIdentity.SortOption.groupBy == Constants.SortType.sortOrder.ToString())
        this.ProjectIdentity.SortOption.groupBy = Constants.SortType.project.ToString();
      if (!(this.ProjectIdentity.SortOption.orderBy == Constants.SortType.sortOrder.ToString()))
        return;
      this.ProjectIdentity.SortOption.orderBy = Constants.SortType.dueDate.ToString();
    }

    private void NotifyTitleChanged(string id, string title)
    {
      ObservableCollection<DisplayItemModel> displayItemModels = this.DisplayItemModels;
      DisplayItemModel displayItemModel = displayItemModels != null ? displayItemModels.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => model.Id == id)) : (DisplayItemModel) null;
      if (displayItemModel == null)
        return;
      displayItemModel.SourceViewModel.Title = title;
    }

    private async Task LoadProjectIdentity(bool onSelect = false)
    {
      WidgetViewModel model = this.Model;
      if (model == null)
      {
        model = (WidgetViewModel) null;
      }
      else
      {
        switch (model.Type)
        {
          case "normal":
          case "smart":
            await this.LoadProject();
            break;
          case "group":
            await this.LoadProjectGroup();
            break;
          case "filter":
            await this.LoadFilterProject();
            break;
          case "tag":
            await this.LoadTagProject();
            break;
          default:
            this.InitDefaultProject();
            break;
        }
        ProjectIdentity projectIdentity = this.ProjectIdentity;
        if (projectIdentity == null)
        {
          model = (WidgetViewModel) null;
        }
        else
        {
          this.ProjectTitle.Text = projectIdentity.GetDisplayTitle();
          if (projectIdentity is DeletedProjectIdentity)
            model = (WidgetViewModel) null;
          else if (projectIdentity is ClosedProjectIdentity)
            model = (WidgetViewModel) null;
          else if (onSelect)
          {
            model.GroupType = projectIdentity.SortOption?.groupBy ?? model.GroupType;
            model.SortType = projectIdentity.SortOption?.orderBy ?? model.SortType;
            model = (WidgetViewModel) null;
          }
          else
          {
            projectIdentity.SortOption = new SortOption()
            {
              groupBy = model.GroupType,
              orderBy = model.SortType
            };
            model = (WidgetViewModel) null;
          }
        }
      }
    }

    private void InitDefaultProject()
    {
      this.ProjectIdentity = (ProjectIdentity) ProjectIdentity.CreateInboxProject();
    }

    private async Task LoadFilterProject()
    {
      FilterModel filterById = await FilterDao.GetFilterById(this.Model.Identity);
      if (filterById != null && filterById.deleted != 1)
        this.ProjectIdentity = (ProjectIdentity) new FilterProjectIdentity(filterById);
      else
        this.LoadInvalidProject();
    }

    private async Task LoadTagProject()
    {
      TagModel tagByName = CacheManager.GetTagByName(this.Model.Identity.ToLower());
      if (tagByName == null)
        return;
      this.ProjectIdentity = (ProjectIdentity) new TagProjectIdentity(tagByName);
    }

    private async Task LoadProjectGroup()
    {
      string groupId = this.Model.Identity;
      ProjectGroupModel projectGroupById = await ProjectGroupDao.GetProjectGroupById(groupId);
      if (projectGroupById != null && projectGroupById.deleted != 1)
      {
        List<ProjectModel> projectsInGroup = CacheManager.GetProjectsInGroup(groupId);
        this.ProjectIdentity = (ProjectIdentity) new GroupProjectIdentity(projectGroupById, projectsInGroup);
        groupId = (string) null;
      }
      else
      {
        this.LoadInvalidProject();
        groupId = (string) null;
      }
    }

    private async Task LoadProject()
    {
      ProjectWidget projectWidget = this;
      string identity = projectWidget.Model.Identity;
      if (SpecialListUtils.IsSmartProject(identity))
      {
        projectWidget.ProjectIdentity = ProjectIdentity.CreateSmartIdentity(identity);
      }
      else
      {
        ProjectModel projectById = CacheManager.GetProjectById(identity);
        if (projectById == null)
        {
          if (identity.Contains("inbox"))
            projectWidget.ProjectIdentity = (ProjectIdentity) new NormalProjectIdentity(new ProjectModel()
            {
              id = Utils.GetInboxId(),
              name = Utils.GetString("Inbox")
            });
          else
            projectWidget.LoadInvalidProject();
        }
        else
        {
          bool? closed = projectById.closed;
          if (closed.HasValue)
          {
            closed = projectById.closed;
            if (closed.Value)
            {
              projectWidget.ProjectIdentity = (ProjectIdentity) new ClosedProjectIdentity(projectById.id);
              return;
            }
          }
          if (!projectById.delete_status)
            projectWidget.ProjectIdentity = (ProjectIdentity) new NormalProjectIdentity(projectById);
          else
            projectWidget.LoadInvalidProject();
        }
      }
    }

    private void LoadInvalidProject()
    {
      this.ProjectIdentity = (ProjectIdentity) new DeletedProjectIdentity();
    }

    private void OnDragBarMouseDown(object sender, MouseButtonEventArgs e)
    {
      this.TryHideAddView();
      this.GetParentWindow()?.OnDragBarMouseDown(sender, e);
    }

    private void OnDragMove(object sender, MouseEventArgs e)
    {
      this.GetParentWindow()?.OnDragMove(sender, e);
    }

    public WidgetWindow GetParentWindow()
    {
      this._parentWindow = this._parentWindow ?? Utils.FindParent<WidgetWindow>((DependencyObject) this);
      return this._parentWindow;
    }

    private static string GetHabitProgressText(HabitModel habit, HabitCheckInModel checkIn)
    {
      if (habit == null || !(habit.Type.ToLower() == "real"))
        return string.Empty;
      if (checkIn != null)
      {
        string[] strArray = new string[7];
        strArray[0] = "(";
        double goal = checkIn.Value;
        strArray[1] = goal.ToString();
        strArray[2] = "/";
        goal = checkIn.Goal;
        strArray[3] = goal.ToString();
        strArray[4] = " ";
        strArray[5] = habit.Unit;
        strArray[6] = ")";
        return string.Concat(strArray);
      }
      return "(" + 0.ToString() + "/" + habit.Goal.ToString() + " " + habit.Unit + ")";
    }

    private static ObservableCollection<DisplayItemModel> BuildDisplayItemModels(
      IEnumerable<TaskBaseViewModel> models)
    {
      ObservableCollection<DisplayItemModel> observableCollection = new ObservableCollection<DisplayItemModel>();
      foreach (TaskBaseViewModel model in models)
      {
        if (model is HabitBaseViewModel habitBaseViewModel)
        {
          DisplayItemModel displayItemModel = new DisplayItemModel(model);
          displayItemModel.ShowReminder = new bool?(HabitUtils.IsReminderValid(habitBaseViewModel.Habit.Reminder));
          TaskBaseViewModel taskBaseViewModel = model;
          taskBaseViewModel.Title = taskBaseViewModel.Title + " " + ProjectWidget.GetHabitProgressText(habitBaseViewModel.Habit, habitBaseViewModel.HabitCheckIn);
          observableCollection.Add(displayItemModel);
        }
        else if (model.Type == DisplayType.Event)
          observableCollection.Add(new DisplayItemModel(model));
        else
          observableCollection.Add(new DisplayItemModel(model, isKanbanMode: true));
      }
      return observableCollection;
    }

    private void OnProjectClick(object sender, MouseButtonEventArgs e)
    {
      e.GetPosition((IInputElement) this.TitleGrid);
      if (this._popup == null)
      {
        EscPopup escPopup = new EscPopup();
        escPopup.StaysOpen = false;
        escPopup.PopupAnimation = PopupAnimation.Fade;
        escPopup.Placement = PlacementMode.Bottom;
        escPopup.HorizontalOffset = -5.0;
        escPopup.PlacementTarget = (UIElement) this.TitlePanel;
        escPopup.VerticalOffset = -5.0;
        this._popup = new ProjectOrGroupPopup((Popup) escPopup, new ProjectExtra(), new ProjectSelectorExtra()
        {
          showAll = false,
          showFilters = true,
          showTags = true,
          showSmartProjects = true,
          batchMode = false,
          showListGroup = true
        });
        this._popup.Resources = this.GetParentWindow().Resources;
        this._popup.ItemSelect += new EventHandler<SelectableItemViewModel>(this.OnItemSelect);
        this._popup.Show();
        this._popup.Closed += new EventHandler(this.OnProjectClosed);
      }
      else
      {
        this._popup.Show();
        this._popup.SetData(new ProjectExtra());
      }
      this.IsEditting = true;
    }

    private void OnProjectClosed(object sender, EventArgs e) => this.IsEditting = false;

    private async void OnItemSelect(object sender, SelectableItemViewModel project)
    {
      this.ProjectTitle.Text = project.Title;
      this.Model.Type = project.Type;
      this.Model.Identity = project.Id;
      await this.LoadProjectIdentity(true);
      await this.Model.Save();
      this.TryAdjustSortType(true);
      WidgetConfigHelper.TryGetSettings(this.Model)?.InitSortType();
      this.ResetQuickAddView(false);
      this.LoadTasks(true);
    }

    private void OnMoreClick(object sender, RoutedEventArgs e)
    {
      if (this._morePopup == null)
      {
        this._morePopup = new WidgetMorePopup();
        this._morePopup.SetPlaceTarget((UIElement) this.MorePath);
        this._morePopup.MoreAction += new EventHandler<WidgetMoreAction>(this.OnMoreMoreAction);
      }
      this._morePopup.Show(new System.Windows.Point(-5.0, -10.0));
    }

    private void OnMoreMoreAction(object sender, WidgetMoreAction e)
    {
      switch (e)
      {
        case WidgetMoreAction.Sync:
          this.SyncTask();
          break;
        case WidgetMoreAction.Setting:
          this.AddSettingEvents();
          break;
        case WidgetMoreAction.Lock:
          this.LockWidget(true);
          this._currentWidgetSettings?.Close();
          break;
        case WidgetMoreAction.Exit:
          ProjectWidgetsHelper.CloseProject(this.Model.Id);
          this.GetParentWindow()?.CloseWidget();
          break;
      }
    }

    private void BindSettingEvents(WidgetSettings window)
    {
      this._currentWidgetSettings = window;
      window.HideCompleteChanged -= new EventHandler(this.OnSettingsHideCompleteChanged);
      window.HideCompleteChanged += new EventHandler(this.OnSettingsHideCompleteChanged);
      window.DisplayOptionChanged -= new EventHandler<string>(this.OnDisplayOptionChanged);
      window.DisplayOptionChanged += new EventHandler<string>(this.OnDisplayOptionChanged);
      window.SortTypeChanged -= new EventHandler<string>(this.OnSortTypeChanged);
      window.SortTypeChanged += new EventHandler<string>(this.OnSortTypeChanged);
      window.GroupTypeChanged -= new EventHandler<string>(this.OnGroupTypeChanged);
      window.GroupTypeChanged += new EventHandler<string>(this.OnGroupTypeChanged);
      window.OpacityChanged -= new EventHandler<float>(this.OnOpacityChanged);
      window.OpacityChanged += new EventHandler<float>(this.OnOpacityChanged);
    }

    private void OnOpacityChanged(object sender, float opacity)
    {
      this.GetParentWindow()?.EnableBlur((double) opacity > 0.0);
    }

    private async void OnGroupTypeChanged(object sender, string e)
    {
      this.Model.GroupType = e;
      await this.LoadProjectIdentity();
      this.LoadTasks();
    }

    private async void OnSortTypeChanged(object sender, string e)
    {
      this.Model.SortType = e;
      await this.LoadProjectIdentity();
      this.LoadTasks();
    }

    private async void OnDisplayOptionChanged(object sender, string option)
    {
      if (option == null)
        return;
      switch (option)
      {
        case "top":
        case "embed":
        case "bottom":
          this.GetParentWindow()?.SetTopMost();
          break;
        case "light":
        case "dark":
          this.GetParentWindow()?.SetTheme(option);
          this.TaskList.ItemsSource = (IEnumerable) null;
          if (this._quickAddView != null)
            ThemeUtil.SetTheme(option, (FrameworkElement) this._quickAddView);
          this.LoadTasks();
          break;
      }
    }

    private void OnSettingsHideCompleteChanged(object sender, EventArgs e) => this.LoadTasks();

    public void AddSettingEvents()
    {
      this.BindSettingEvents(WidgetConfigHelper.TryShowSettings(this.Model));
    }

    public void RemoveSettingEvents()
    {
      WidgetSettings settings = WidgetConfigHelper.TryGetSettings(this.Model);
      if (settings == null)
        return;
      settings.HideCompleteChanged -= new EventHandler(this.OnSettingsHideCompleteChanged);
      settings.DisplayOptionChanged -= new EventHandler<string>(this.OnDisplayOptionChanged);
      settings.SortTypeChanged -= new EventHandler<string>(this.OnSortTypeChanged);
      settings.GroupTypeChanged -= new EventHandler<string>(this.OnGroupTypeChanged);
      settings.OpacityChanged -= new EventHandler<float>(this.OnOpacityChanged);
    }

    public async Task SyncTask()
    {
      ProjectWidget projectWidget = this;
      if (!Utils.IsNetworkAvailable())
      {
        // ISSUE: explicit non-virtual call
        __nonvirtual (projectWidget.TryToastString((object) null, Utils.GetString("NoNetwork")));
      }
      projectWidget.MorePath.Visibility = Visibility.Hidden;
      projectWidget.SyncButton.Visibility = Visibility.Hidden;
      ((Storyboard) projectWidget.FindResource((object) "SyncStory")).Begin();
      SyncManager.Sync(1);
    }

    private void OnSyncStoryCompleted(object sender, EventArgs e)
    {
      this.MorePath.Visibility = Visibility.Visible;
      this.SyncButton.Visibility = Visibility.Visible;
    }

    public async void OnItemOpenClick(DisplayItemModel item)
    {
      bool useTaskOpen = this.ProjectIdentity is NormalProjectIdentity || this.ProjectIdentity is GroupProjectIdentity || this.ProjectIdentity is ParentTaskIdentity || this.ProjectIdentity is MatrixQuadrantIdentity;
      this.ChangeModelOpenStatus(item, useTaskOpen);
      if (useTaskOpen)
        await TaskService.FoldTask(item.Id, item.IsOpen);
      else
        SmartListTaskFoldHelper.UpdateStatus(item.Id, this.ProjectIdentity.CatId, !item.IsOpen);
      TaskChangeNotifier.NotifyTaskOpenChanged(item.Id);
    }

    public void SetShowCountDown(bool showCountDown)
    {
    }

    public async void NavigateProjectById(string projectId)
    {
      ProjectModel projectById = ProjectAndTaskIdsCache.GetProjectById(projectId);
      if (projectById == null)
        return;
      this.ProjectTitle.Text = projectById.name;
      this.Model.Type = "normal";
      this.Model.Identity = projectId;
      await this.Model.Save();
      await this.LoadProjectIdentity();
      this.ResetQuickAddView();
      this.LoadTasks();
    }

    public void NavigateTodayProject()
    {
    }

    public void NavigateTomorrowProject()
    {
    }

    public async void OnTaskProjectChanged(string taskId, ProjectModel project)
    {
      this.TryToastMoveControl(await TaskDao.GetThinTaskById(taskId), project);
    }

    private void OnUnlockWidgetClick(object sender, RoutedEventArgs e) => this.LockWidget(false);

    private void OnSyncButtonClick(object sender, RoutedEventArgs e) => this.SyncTask();

    public bool CanAddSubTask() => !(this.ProjectIdentity is AssignToMeProjectIdentity);

    public async void CreateSubtask(DisplayItemModel model)
    {
      ProjectWidget sender = this;
      TaskModel newTask;
      Section section;
      DisplayItemModel displayModel;
      TaskModel taskModel;
      if (sender.ProjectIdentity is AssignToMeProjectIdentity)
      {
        newTask = (TaskModel) null;
        section = (Section) null;
        displayModel = (DisplayItemModel) null;
        taskModel = (TaskModel) null;
      }
      else
      {
        if (!model.IsOpen)
          sender.OnItemOpenClick(model);
        if (await ProChecker.CheckTaskLimit(model.ProjectId))
        {
          newTask = (TaskModel) null;
          section = (Section) null;
          displayModel = (DisplayItemModel) null;
          taskModel = (TaskModel) null;
        }
        else
        {
          sender.GetCurrentIndex(model.TaskId);
          newTask = new TaskModel()
          {
            id = Utils.GetGuid(),
            isAllDay = model.IsAllDay,
            kind = "TEXT",
            title = string.Empty,
            parentId = model.TaskId,
            projectId = model.ProjectId,
            sortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(model.ProjectId, model.Id, new bool?(false)),
            status = model.Status,
            columnId = model.ColumnId,
            pinnedTimeStamp = model.IsPinned ? Utils.GetNowTimeStamp() : 0L
          };
          section = model.Section;
          newTask.startDate = (DateTime?) ((DateTime?) section?.GetStartDate() ?? sender.ProjectIdentity.GetTimeData()?.StartDate) ?? TaskDefaultDao.GetDefaultSafely().GetDefaultDateTime();
          newTask.isAllDay = new bool?(true);
          newTask.dueDate = new DateTime?();
          if (sender.ProjectIdentity is FilterProjectIdentity projectIdentity1)
          {
            newTask.tag = TagSerializer.ToJsonContent(projectIdentity1.GetTags());
            newTask.priority = projectIdentity1.GetPriority();
          }
          else
            newTask.tag = section?.GetTag();
          TaskModel taskModel1 = newTask;
          Section section1 = section;
          int num1 = section1 != null ? section1.GetPriority() : TaskDefaultDao.GetDefaultSafely().Priority;
          taskModel1.priority = num1;
          if (string.IsNullOrEmpty(newTask.tag) && sender.ProjectIdentity is TagProjectIdentity projectIdentity2)
            newTask.tag = "[\"" + projectIdentity2.Tag + "\"]";
          newTask.assignee = section?.GetAssignee();
          newTask.status = model.Status;
          if (newTask.status != 0)
            newTask.completedTime = new DateTime?(DateTime.Now);
          displayModel = new DisplayItemModel(new TaskBaseViewModel(newTask));
          displayModel.Section = section;
          displayModel.Level = model.Level + 1;
          model.AddChild(displayModel, (DisplayItemModel) null);
          taskModel = displayModel.GetTaskModel();
          taskModel.createdTime = new DateTime?(DateTime.Now);
          TaskModel taskModel2 = await TaskService.AddTask(taskModel, sender: (object) sender);
          await Task.Delay(10);
          if (taskModel.startDate.HasValue)
          {
            foreach (TaskReminderModel defaultAllDayReminder in TimeData.GetDefaultAllDayReminders())
            {
              defaultAllDayReminder.taskserverid = taskModel.id;
              int num2 = await TaskReminderDao.SaveReminders(defaultAllDayReminder);
            }
          }
          DisplayItemModel displayItemModel = displayModel;
          displayItemModel.AvatarUrl = await AvatarHelper.GetAvatarUrl(section?.GetAssignee(), newTask.projectId);
          displayItemModel = (DisplayItemModel) null;
          sender.OnSubTaskAdded(taskModel.id);
          newTask = (TaskModel) null;
          section = (Section) null;
          displayModel = (DisplayItemModel) null;
          taskModel = (TaskModel) null;
        }
      }
    }

    public async void OnSubTaskAdded(string taskModelId)
    {
      await Task.Delay(50);
      DisplayItemModel model = this.DisplayItemModels.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (i => i.Id == taskModelId));
      if (model == null)
        ;
      else
      {
        WidgetTaskItem singleVisualChildren = Utils.FindSingleVisualChildren<WidgetTaskItem>(this.TaskList.ItemContainerGenerator.ContainerFromItem((object) model));
        if (singleVisualChildren == null)
          ;
        else
        {
          this.TaskList.ScrollIntoView((object) model);
          singleVisualChildren.TryShowDisplayWindow(model, false, true);
        }
      }
    }

    private int GetCurrentIndex(string taskId)
    {
      if (this.DisplayItemModels != null && this.DisplayItemModels.Count > 0)
      {
        for (int index = 0; index < this.DisplayItemModels.Count; ++index)
        {
          if (this.DisplayItemModels[index].Id == taskId)
            return index;
        }
      }
      return -1;
    }

    public void PopupCloseTask(DisplayItemModel model, WidgetTaskItem ele)
    {
      if (!model.IsTask || !model.Enable)
        return;
      System.Windows.Point position = Mouse.GetPosition((IInputElement) ele);
      SetClosedStatusPopup instance = SetClosedStatusPopup.GetInstance((UIElement) null, model.IsCompleted, model.IsAbandoned, 15.0 - position.Y, 15.0 - position.X);
      instance.Resources = this.GetParentWindow().Resources;
      TaskCloseExtra taskCloseExtra1;
      instance.Abandoned += (EventHandler) (async (s, e) => taskCloseExtra1 = await TaskService.SetTaskStatus(model.TaskId, -1, true, (IToastShowWindow) this));
      instance.Completed += (EventHandler) (async (s, e) =>
      {
        bool playSound = await ele.TryPlayCompleteStory(model);
        TaskCloseExtra taskCloseExtra2 = await TaskService.SetTaskStatus(model.TaskId, 2, true, (IToastShowWindow) this, playSound: playSound);
      });
      instance.Show();
    }

    public void PostPoneAll()
    {
      if (!new CustomerDialog(Utils.GetString("PostponeToTodayTitle"), Utils.GetString("PostponeToTodayMsg"), Utils.GetString("Postpone"), Utils.GetString("Cancel"), (Window) this.GetParentWindow()).ShowDialog().GetValueOrDefault())
        return;
      PostponeUndoModel controller = new PostponeUndoModel();
      List<TaskBaseViewModel> allOutDateTasks = TaskCache.GetAllOutDateTasks();
      if (LocalSettings.Settings.ShowSubtasks)
        allOutDateTasks.AddRange((IEnumerable<TaskBaseViewModel>) TaskDetailItemCache.GetAllOutDateItems());
      if (!controller.InitData(allOutDateTasks))
        return;
      controller.Do();
      WindowToastHelper.ShowAndHideToast(this.UndoToastGrid, (FrameworkElement) new UndoToast((UndoController) controller));
    }

    public async Task<string> GetCompleteStoryText(DisplayItemModel model)
    {
      if (LocalSettings.Settings.ExtraSettings.LastCompleteTime < ticktick_WPF.Util.DateUtils.GetDateNum(DateTime.Today))
      {
        LocalSettings.Settings.ExtraSettings.LastCompleteTime = ticktick_WPF.Util.DateUtils.GetDateNum(DateTime.Today);
        if (TaskCache.GetAllTask().All<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
        {
          if (t.Status != 2)
            return true;
          DateTime? completedTime = t.CompletedTime;
          DateTime today = DateTime.Today;
          return completedTime.HasValue && completedTime.GetValueOrDefault() < today;
        })))
          return Utils.GetString("FirstTaskDone");
      }
      return await this.GetCompleteText(model);
    }

    private async Task<string> GetCompleteText(DisplayItemModel model)
    {
      if (this.ProjectIdentity is TodayProjectIdentity)
      {
        List<TaskBaseViewModel> list1 = this._displayModels.ToList<TaskBaseViewModel>();
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

    public void ResetProject()
    {
      this.ResetQuickAddView();
      this.ProjectTitle.Text = this.ProjectIdentity.GetDisplayTitle();
    }

    public void OpenOrCloseSection(SectionStatus status)
    {
      List<DisplayItemModel> models = this.DisplayItemModels.ToList<DisplayItemModel>();
      DisplayItemModel displayItemModel = models.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (item => item.Type == DisplayType.Section && item.Section.SectionId == status.SectionId));
      if (displayItemModel != null)
      {
        List<DisplayItemModel> displayItemModelList = new List<DisplayItemModel>();
        foreach (DisplayItemModel child in displayItemModel.Section.Children)
        {
          displayItemModelList.Add(child);
          if (child.IsOpen && child.IsTask)
          {
            List<DisplayItemModel> childrenModels = child.GetChildrenModels(false);
            displayItemModelList.AddRange((IEnumerable<DisplayItemModel>) childrenModels);
          }
        }
        string name = displayItemModel.Title;
        if (displayItemModel.Section is AssigneeSection section)
          name = section.Assingee;
        if (status.IsOpen)
        {
          SectionStatusDao.OpenProjectSection(this.ProjectIdentity.Id, name);
          int index1 = models.IndexOf(displayItemModel);
          if (index1 >= 0 && models[index1] != null)
          {
            for (int index2 = 0; index2 < displayItemModelList.Count; ++index2)
              models.Insert(index1 + 1 + index2, displayItemModelList[index2]);
          }
        }
        else
        {
          if (!string.IsNullOrEmpty(this.ProjectIdentity.Id))
            SectionStatusDao.CloseProjectSection(this.ProjectIdentity.Id, name);
          displayItemModelList.ForEach((Action<DisplayItemModel>) (child => models.Remove(child)));
          if (status.SectionId == new CompletedSection().SectionId && models[models.Count - 1].Type == DisplayType.LoadMore)
            models.RemoveAt(models.Count - 1);
        }
        ItemsSourceHelper.SetItemsSource<DisplayItemModel>((ItemsControl) this.TaskList, models);
      }
      DataChangedNotifier.NotifyListSectionOpenChanged((object) this, (this.ProjectIdentity.CatId, this.ProjectIdentity.SortOption));
    }

    private void OnAddPreviewDown(object sender, MouseButtonEventArgs e)
    {
      if (this._quickAddView == null)
        return;
      this.TryHideAddView();
      e.Handled = true;
      Mouse.Capture((IInputElement) null);
    }

    private void OnMouseClick(object sender, MouseButtonEventArgs e) => this.TryHideAddView();

    private async Task TryHideAddView()
    {
      if (LocalSettings.Settings.ExtraSettings.PwShowAdd || this._quickAddView == null || this._quickAddView.IsMouseOver || this.AddButton.IsMouseOver)
        return;
      await this._quickAddView.TryAddTaskOnLostFocus();
      this.HideAddView();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/widget/projectwidget.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.ProjectWindow = (ProjectWidget) target;
          this.ProjectWindow.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMouseClick);
          break;
        case 2:
          ((Timeline) target).Completed += new EventHandler(this.OnSyncStoryCompleted);
          break;
        case 3:
          this.WidgetBackground = (Border) target;
          break;
        case 4:
          this.WidgetGrid = (Grid) target;
          break;
        case 5:
          ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnDragBarMouseDown);
          ((UIElement) target).MouseMove += new MouseEventHandler(this.OnDragMove);
          break;
        case 6:
          this.TitleGrid = (Grid) target;
          break;
        case 7:
          this.TitlePanel = (StackPanel) target;
          this.TitlePanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnProjectClick);
          break;
        case 8:
          this.ProjectTitle = (EmjTextBlock) target;
          break;
        case 9:
          this.ChoseProjectGrid = (Grid) target;
          break;
        case 10:
          this.LockedOptionGrid = (Grid) target;
          break;
        case 11:
          this.UnlockWidgetButton = (Button) target;
          this.UnlockWidgetButton.Click += new RoutedEventHandler(this.OnUnlockWidgetClick);
          break;
        case 12:
          this.SyncButton = (Button) target;
          this.SyncButton.Click += new RoutedEventHandler(this.OnSyncButtonClick);
          break;
        case 13:
          this.OptionsGrid = (StackPanel) target;
          break;
        case 14:
          this.AddButton = (Button) target;
          this.AddButton.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnAddPreviewDown);
          this.AddButton.Click += new RoutedEventHandler(this.OnAddTaskClick);
          break;
        case 15:
          this.SyncPath = (Path) target;
          break;
        case 16:
          this.MorePath = (Button) target;
          this.MorePath.Click += new RoutedEventHandler(this.OnMoreClick);
          break;
        case 17:
          this.TaskAddGrid = (ContentControl) target;
          break;
        case 18:
          this.EmptyText = (TextBlock) target;
          break;
        case 19:
          this.TaskListGrid = (Grid) target;
          break;
        case 20:
          this.TaskList = (ListView) target;
          break;
        case 21:
          this.UndoToastGrid = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
