// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MainListView.TaskView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
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
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Coaches;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Kanban;
using ticktick_WPF.Views.MainListView.DetailView;
using ticktick_WPF.Views.MainListView.TaskList;
using ticktick_WPF.Views.Summary;
using ticktick_WPF.Views.TaskList;
using ticktick_WPF.Views.Timeline;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.MainListView
{
  public class TaskView : Grid
  {
    private TaskDetailView _taskDetailView;
    private BatchDetailView _batchDetail;
    private DetailHintPanel _hintPanel;
    private HabitDetailControl _habitDetailControl;
    private CourseDetailControl _courseDetail;
    private CalendarDetailControl _calendarDetail;
    private ProjectActivityPanel _projectActivityPanel;
    private KanbanContainer _kanban;
    private SummaryControl _summary;
    private ProjectTaskListView _taskListView;
    private TimelineContainer _timeline;
    private ColumnDefinition _firstColumn;
    private ColumnDefinition _secondColumn;
    private bool _hideSecondColumn;
    private readonly Border _listContent;
    private readonly Border _detailContent;
    private GridSplitter _rightSplit;
    public ProjectIdentity ProjectIdentity;
    private bool _needHideDetail;
    private bool _hideMenu;
    private bool _showProjectMenu;

    private void SetTaskDetailView()
    {
      if (this._taskDetailView == null || !object.Equals((object) this._detailContent.Child, (object) this._taskDetailView))
      {
        this.RemoveDetailEvent();
        this._taskDetailView = new TaskDetailView(Constants.DetailMode.Page);
        this._detailContent.Child = (UIElement) this._taskDetailView;
        this._taskDetailView.ShowUndoOnTaskDeleted += new EventHandler<string>(this.OnTaskDeletedInDetail);
        this._taskDetailView.CheckItemsDeleted += new EventHandler<TaskDetailItemModel>(this.OnCheckItemsDeleted);
        this._taskDetailView.CheckItemDragDrop += new EventHandler<string>(this.OnCheckItemDragDrop);
        this._taskDetailView.NavigateTask += new EventHandler<ProjectTask>(this.OnNavigateTask);
        this._taskDetailView.SubtaskDragOver += new EventHandler<DragMouseEvent>(this.OnTaskDragging);
        this._taskDetailView.TaskCopied += new EventHandler<string>(this.OnTaskCopied);
        this._taskDetailView.FocusList += new EventHandler(this.TryFocusSelectedItem);
        this._taskDetailView.TaskNavigated += new EventHandler<string>(this.OnDetailTaskNavigated);
        this._taskDetailView.EnterImmersive += new EnterImmersiveDelegate(this.OnEnterImmersive);
      }
      if (!this._hideSecondColumn)
        return;
      this._taskDetailView.ShowBackMenu();
    }

    public async void ShowTaskDetail(string taskId, string itemId = null, bool showWithAnim = true)
    {
      TaskView taskView = this;
      taskView.SetTaskDetailView();
      taskView._taskDetailView.TaskSelect(taskId, itemId);
      if (!taskView.IsLoaded)
        await Task.Delay(200);
      if (!showWithAnim)
        return;
      taskView.SetDetailVisibleWithAnimation(true);
    }

    private void ShowBatchSelectView(List<string> selected)
    {
      if (this._batchDetail == null || !object.Equals((object) this._detailContent.Child, (object) this._batchDetail))
      {
        this.RemoveDetailEvent();
        this._batchDetail = new BatchDetailView(this);
        this._detailContent.Child = (UIElement) this._batchDetail;
      }
      this._batchDetail.OnBatchSelect(selected);
    }

    private void RemoveDetailEvent()
    {
      this._taskDetailView?.Dispose();
      this._taskDetailView?.RemoveKeyBinding();
      this._taskDetailView = (TaskDetailView) null;
      this._habitDetailControl?.ClearEvent();
      this._habitDetailControl = (HabitDetailControl) null;
      this._courseDetail?.ClearEvent();
      this._courseDetail = (CourseDetailControl) null;
      this._calendarDetail?.ClearEvent();
      this._calendarDetail = (CalendarDetailControl) null;
      this._projectActivityPanel?.ClearEvent();
      this._projectActivityPanel = (ProjectActivityPanel) null;
      this._batchDetail?.Dispose();
      this._batchDetail = (BatchDetailView) null;
      this._detailContent.Child = (UIElement) null;
    }

    private async void OnNavigateTask(object sender, ProjectTask task)
    {
      await TaskDetailWindow.NavigateProjectTask(task, this.GetToastWindow());
    }

    private void OnCheckItemsDeleted(object sender, TaskDetailItemModel model)
    {
      UndoToast uiElement = new UndoToast();
      uiElement.InitSubtaskUndo(model);
      this.GetToastWindow()?.Toast((FrameworkElement) uiElement);
    }

    private async void OnTaskDeletedInDetail(object sender, string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(taskId, task.projectId);
        // ISSUE: explicit non-virtual call
        if (subTasksByIdAsync != null && __nonvirtual (subTasksByIdAsync.Count) > 0)
        {
          subTasksByIdAsync.Add(task);
          IToastShowWindow toastWindow = this.GetToastWindow();
          if (toastWindow == null)
          {
            task = (TaskModel) null;
          }
          else
          {
            toastWindow.BatchDeleteTask(subTasksByIdAsync);
            task = (TaskModel) null;
          }
        }
        else
        {
          IToastShowWindow toastWindow = this.GetToastWindow();
          if (toastWindow == null)
          {
            task = (TaskModel) null;
          }
          else
          {
            toastWindow.TaskDeleted(taskId);
            task = (TaskModel) null;
          }
        }
      }
    }

    private void OnTaskCopied(object sender, string taskId)
    {
      this._taskListView?.SetSelectedId(taskId);
    }

    private void OnDetailTaskNavigated(object sender, string taskId)
    {
      this._taskListView?.OnDetailTaskNavigated(taskId);
    }

    private void OnEnterImmersive(string taskId, int caretIndex)
    {
      Utils.FindParent<IListViewParent>((DependencyObject) this)?.EnterImmersiveMode(taskId, caretIndex);
    }

    private void TryFocusSelectedItem(object sender, EventArgs e)
    {
      this._taskListView?.TryFocusSelectedItem();
    }

    private async void NavigateHabit(string habitId, bool pullRemote = true, bool force = true)
    {
      this.SetHabitDetailView();
      this._habitDetailControl.Load(habitId, DateTime.Today, pullRemote, force);
      this._habitDetailControl.ScrollToTop();
      this.SetDetailVisibleWithAnimation(true);
    }

    private void SetHabitDetailView()
    {
      if (this._habitDetailControl != null && object.Equals((object) this._detailContent.Child, (object) this._habitDetailControl))
        return;
      this.RemoveDetailEvent();
      HabitDetailControl habitDetailControl = new HabitDetailControl();
      habitDetailControl.Margin = new Thickness(0.0, 25.0, 0.0, 0.0);
      this._habitDetailControl = habitDetailControl;
      this._habitDetailControl.HideDetail += new EventHandler(this.OnHideDetail);
      this._detailContent.Child = (UIElement) this._habitDetailControl;
      if (!this._hideSecondColumn)
        return;
      this._habitDetailControl.ShowBackMenu();
    }

    private void OnHideDetail(object sender, EventArgs e) => this.TryExtractDetail();

    public async Task ReloadHabit()
    {
      if (this._habitDetailControl == null)
        this.SetHabitDetailView();
      this._habitDetailControl.Reload();
    }

    private async void NavigateCourse(string courseId)
    {
      if (string.IsNullOrEmpty(courseId))
        return;
      this.RemoveDetailEvent();
      this.SetCourseDetail();
      this._courseDetail.NavigateCourse(courseId);
      this.SetDetailVisibleWithAnimation(true);
    }

    private void SetCourseDetail()
    {
      if (this._courseDetail != null && object.Equals((object) this._detailContent.Child, (object) this._courseDetail))
        return;
      this.RemoveDetailEvent();
      CourseDetailControl courseDetailControl = new CourseDetailControl();
      courseDetailControl.Margin = new Thickness(0.0, 25.0, 0.0, 0.0);
      this._courseDetail = courseDetailControl;
      this._courseDetail.HideDetail += new EventHandler(this.OnHideDetail);
      this._detailContent.Child = (UIElement) this._courseDetail;
      if (!this._hideSecondColumn)
        return;
      this._courseDetail.ShowBackMenu();
    }

    private async void OnScheduleChanged(object sender, EventArgs e)
    {
      TaskView taskView = this;
      if (taskView._courseDetail != null)
      {
        if (await ScheduleDao.GetCoursedByIdAsync(taskView._courseDetail.CourseId) != null)
          taskView._courseDetail.Reload();
        else
          taskView.TryExtractDetail();
      }
      // ISSUE: reference to a compiler-generated method
      taskView.Dispatcher.Invoke(new Action(taskView.\u003COnScheduleChanged\u003Eb__22_0));
    }

    private void SetCalendarDetail()
    {
      if (this._calendarDetail != null && object.Equals((object) this._detailContent.Child, (object) this._calendarDetail))
        return;
      this.RemoveDetailEvent();
      CalendarDetailControl calendarDetailControl = new CalendarDetailControl();
      calendarDetailControl.Margin = new Thickness(0.0, 25.0, 0.0, 0.0);
      this._calendarDetail = calendarDetailControl;
      this._calendarDetail.HideDetail += new EventHandler(this.OnHideDetail);
      this._detailContent.Child = (UIElement) this._calendarDetail;
      if (!this._hideSecondColumn)
        return;
      this._calendarDetail.ShowBackMenu();
    }

    private void ShowEventDetail(string eventId) => this.NavigateEvent(eventId);

    public async Task NavigateEvent(string eventId)
    {
      if (string.IsNullOrEmpty(eventId))
        return;
      this.SetCalendarDetail();
      string displayEventId = eventId;
      if (eventId.Contains("_time_"))
        eventId = eventId.Remove(eventId.IndexOf("_time_", StringComparison.Ordinal));
      CalendarEventModel calendarEventModel = await CalendarEventDao.GetEventById(eventId);
      if (calendarEventModel == null)
        calendarEventModel = await CalendarEventDao.GetEventByEventId(eventId);
      CalendarEventModel model1 = calendarEventModel;
      if (model1 == null)
        return;
      this.SwitchTaskList();
      this._taskListView?.SetSelectedId(eventId);
      IEnumerable<DisplayItemModel> items = this._taskListView?.GetItems();
      if (items != null)
      {
        List<DisplayItemModel> list = items.ToList<DisplayItemModel>();
        if (list.Any<DisplayItemModel>())
        {
          DisplayItemModel displayItemModel = list.FirstOrDefault<DisplayItemModel>((Func<DisplayItemModel, bool>) (model => model.Id == displayEventId));
          if (displayItemModel != null)
          {
            model1.DueStart = displayItemModel.StartDate;
            model1.DueEnd = displayItemModel.DueDate;
          }
        }
      }
      if (this._calendarDetail != null)
        this._calendarDetail.DataContext = (object) new CalendarDetailViewModel(model1);
      this.SetDetailVisibleWithAnimation(true);
    }

    private void SetProjectActivity()
    {
      if (this._projectActivityPanel != null && object.Equals((object) this._detailContent.Child, (object) this._projectActivityPanel))
        return;
      this.RemoveDetailEvent();
      this._projectActivityPanel = new ProjectActivityPanel(this._hideSecondColumn);
      this._projectActivityPanel.HideDetail += new EventHandler(this.OnHideDetail);
      this._detailContent.Child = (UIElement) this._projectActivityPanel;
      if (!this._hideSecondColumn)
        return;
      this._projectActivityPanel.SetBachPathVisible(true);
    }

    private async void ToggleProjectActivityPanel(string projectId)
    {
      if (!ProChecker.CheckPro(ProType.ListActivities))
        return;
      if (!Utils.IsNetworkAvailable())
      {
        this.TryToast(Utils.GetString("NoNetwork"));
      }
      else
      {
        this.SetProjectActivity();
        this._projectActivityPanel.Init(projectId);
        this.SetDetailVisibleWithAnimation(true);
      }
    }

    public void HideDetailOnTasksDelete(List<string> taskIds)
    {
      if (taskIds.Contains(this._taskDetailView?.TaskId))
        this.HideDetail();
      else
        this._taskDetailView?.OnTasksDeleted(taskIds);
    }

    public bool DetailMouseOver()
    {
      return this._taskDetailView != null && this._taskDetailView.IsVisible && this._taskDetailView.CheckMouseOver();
    }

    private void OnHabitArchived(object sender, string e)
    {
      if (this._habitDetailControl == null || !(this._habitDetailControl.GetHabitId() == e))
        return;
      this.TryExtractDetail();
    }

    public void ClearDetailParseDate() => this._taskDetailView?.ClearParseDate();

    public bool DetailTitleFocus()
    {
      TaskDetailView taskDetailView = this._taskDetailView;
      return taskDetailView != null && taskDetailView.TitleFocused;
    }

    public TaskDetailView GetTaskDetail() => this._taskDetailView;

    public void SetEmptyImageVisible(Visibility visibility, bool isTask, bool isNote)
    {
      this._hintPanel.Visibility = visibility;
      this._hintPanel.SetEmptyImageVisible(isTask, isNote);
    }

    public void ShowDetailBackMenu()
    {
      this._taskDetailView?.ShowBackMenu();
      this._calendarDetail?.ShowBackMenu();
      this._habitDetailControl?.ShowBackMenu();
      this._courseDetail?.ShowBackMenu();
      this._projectActivityPanel?.SetBachPathVisible(true);
    }

    public void HideDetailBackMenu()
    {
      this._taskDetailView?.HideBackMenu();
      this._calendarDetail?.HideBackMenu();
      this._habitDetailControl?.HideBackMenu();
      this._courseDetail?.HideBackMenu();
      this._projectActivityPanel?.SetBachPathVisible(false);
    }

    public bool TaskDetailFocus()
    {
      TaskDetailView taskDetailView = this._taskDetailView;
      return taskDetailView != null && taskDetailView.TextFocus();
    }

    private void ClearDetail()
    {
      this.RemoveDetailEvent();
      this.SetDetailHintVisible(true);
      this._detailContent.Child = (UIElement) this._hintPanel;
    }

    private string GetDetailId()
    {
      if (this._detailContent.Child == this._hintPanel)
        return (string) null;
      if (this._taskDetailView != null)
        return this._taskDetailView.TaskId;
      if (this._calendarDetail != null)
        return this._calendarDetail.EventId;
      if (this._habitDetailControl != null)
        return this._habitDetailControl.GetHabitId();
      return this._courseDetail != null ? this._courseDetail.CourseId : (string) null;
    }

    public event EventHandler<List<string>> KanbanBatchTaskDrop;

    public async Task SwitchKanbanAndLoad(
      ProjectIdentity identity,
      bool cancelOperation = true,
      bool projectSelect = false,
      string taskId = null)
    {
      TaskView child = this;
      child.SetDisplayMode("kanban");
      Utils.FindParent<MainWindow>((DependencyObject) child)?.SetMainWindowsMinSize(false);
      if (child._kanban == null)
        child.InitKanban(identity);
      else if (!child.Children.Contains((UIElement) child._kanban))
      {
        child._kanban.BatchTaskDrop += new EventHandler<List<string>>(child.OnKanbanBatchTaskDropped);
        child.Children.Add((UIElement) child._kanban);
      }
      if (cancelOperation)
        child._kanban.CancelOperation();
      child._kanban.Load(identity, projectSelect, true, taskId);
      if (!(identity is NormalProjectIdentity normalProjectIdentity) || normalProjectIdentity.Project == null)
        return;
      if (!await TaskService.AdjustKanbanData(normalProjectIdentity.Project.id))
        return;
      child._kanban.Load(identity, needPull: true);
    }

    private void OnKanbanBatchTaskDropped(object sender, List<string> e)
    {
      EventHandler<List<string>> kanbanBatchTaskDrop = this.KanbanBatchTaskDrop;
      if (kanbanBatchTaskDrop == null)
        return;
      kanbanBatchTaskDrop(sender, e);
    }

    private void InitKanban(ProjectIdentity project)
    {
      this._kanban = new KanbanContainer(project);
      this._kanban.SetFoldMenuIcon(this._hideMenu);
      if (!this._showProjectMenu)
        this._kanban.HideFoldMenuIcon();
      this._kanban.BatchTaskDrop += new EventHandler<List<string>>(this.OnKanbanBatchTaskDropped);
      this.Children.Add((UIElement) this._kanban);
      this._kanban.SetValue(Grid.ColumnProperty, (object) 0);
      this._kanban.SetValue(Grid.ColumnSpanProperty, (object) 2);
      this._kanban.SetValue(Panel.ZIndexProperty, (object) 10);
    }

    public void ReloadKanban()
    {
      if (this._kanban == null || !this._kanban.IsVisible)
        return;
      this._kanban.CancelOperation();
      this._kanban.Reload(false, true);
    }

    private void ClearKanban()
    {
      if (this._kanban != null)
        this._kanban.BatchTaskDrop -= new EventHandler<List<string>>(this.OnKanbanBatchTaskDropped);
      this.Children.Remove((UIElement) this._kanban);
      this._kanban?.RemoveAddingTaskModel();
      this._kanban?.Clear();
    }

    public void SwitchSummary()
    {
      this.SetDisplayMode("Summary");
      if (this._summary == null)
        this.InitSummary();
      else
        this._summary.LoadData();
    }

    private void InitSummary()
    {
      this._summary = new SummaryControl();
      this._summary.SetResourceReference(Panel.BackgroundProperty, (object) "ShowAreaBackground");
      this._summary.SetFoldMenuIcon(this._hideMenu);
      this.Children.Add((UIElement) this._summary);
      this._summary.SetValue(Grid.ColumnProperty, (object) 0);
      this._summary.SetValue(Grid.ColumnSpanProperty, (object) 2);
      this._summary.SetValue(Panel.ZIndexProperty, (object) 8);
    }

    private void ClearSummary()
    {
      this.Children.Remove((UIElement) this._summary);
      this._summary = (SummaryControl) null;
    }

    public event EventHandler<List<string>> BatchTaskDrop;

    public event EventHandler<DragMouseEvent> TaskDragging;

    public void SwitchTaskList() => this.SetDisplayMode("list");

    private void OpenProjectActivity(object sender, string projectId)
    {
      this.ToggleProjectActivityPanel(projectId);
    }

    private void OnTaskListTaskDrop(object sender, string e)
    {
      this._taskDetailView?.OnTaskDrop(sender, e);
    }

    private void OnTaskTaskListBatchChanged(object sender, List<string> taskIds)
    {
      this.HideDetail();
    }

    private void OnSearchFilterChanged(object sender, EventArgs e) => this.HideDetail();

    private void OnTaskListTaskAdded(object sender, TaskModel task) => this.ShowTaskDetail(task.id);

    private void OnBatchTaskDropped(object sender, List<string> e)
    {
      EventHandler<List<string>> batchTaskDrop = this.BatchTaskDrop;
      if (batchTaskDrop == null)
        return;
      batchTaskDrop(sender, e);
    }

    public void OnTaskDragging(object sender, DragMouseEvent e)
    {
      EventHandler<DragMouseEvent> taskDragging = this.TaskDragging;
      if (taskDragging != null)
        taskDragging(sender, e);
      this._taskDetailView?.OnTaskDragging(e);
    }

    private void OnBatchSelect(object sender, List<string> selected)
    {
      if (selected == null || selected.Count == 0)
        this.HideDetail();
      else
        this.ShowBatchSelectView(selected);
    }

    private void EventAdded(object sender, string e) => this.ShowEventDetail(e);

    public void TaskSelect(ListItemSelectModel model)
    {
      if (model.Type != TaskSelectType.Click && this._hideSecondColumn)
        return;
      switch (model.ItemType)
      {
        case DisplayType.Task:
        case DisplayType.CheckItem:
        case DisplayType.Agenda:
        case DisplayType.Note:
          this.ShowTaskDetail(model.Id, model.ChildId);
          break;
        case DisplayType.Event:
          this.ShowEventDetail(model.Id);
          break;
        case DisplayType.Habit:
          this.NavigateHabit(model.Id);
          break;
        case DisplayType.Course:
          this.NavigateCourse(model.Id);
          break;
      }
    }

    public async Task SwitchListAndLoad(ProjectIdentity identity, bool hideDetail = false)
    {
      if (identity is NormalProjectIdentity normalProjectIdentity)
      {
        ProjectModel projectById = CacheManager.GetProjectById(normalProjectIdentity.Id);
        if (projectById != null && projectById.sortType == Constants.SortType.sortOrder.ToString())
          ColumnBatchHandler.MergeWithServer(projectById.id);
      }
      this.SwitchTaskList();
      await this._taskListView.LoadProject(identity, forceLoad: true);
      if (!hideDetail)
        return;
      this.HideDetail();
    }

    public void FocusQuickAdd() => this._taskListView?.FocusQuickAdd();

    public bool CheckTaskInList(string taskId) => this._taskListView.ExistTask(taskId);

    public void ReloadTaskListAndSelect(bool forceReload = false, string taskId = "", bool forceFocus = true)
    {
      this._taskListView.ReloadTask(forceReload, taskId, forceFocus);
      this.ShowTaskDetail(taskId);
    }

    public void ReSearch()
    {
      this.Dispatcher.Invoke((Action) (() => this._taskListView?.ReSearch()));
    }

    private void OnCheckItemDragDrop(object sender, string e)
    {
      this._taskListView?.OnCheckItemDragDrop(e);
    }

    public void SelectId(string id)
    {
      if (string.IsNullOrEmpty(id))
        return;
      DisplayItemModel displayItemModel = this._taskListView?.SetSelectedId(id);
      if (displayItemModel == null)
        return;
      if (displayItemModel.Type == DisplayType.Event)
        this.NavigateEvent(displayItemModel.Id);
      else
        this.ShowTaskDetail(displayItemModel.Id);
    }

    public ProjectTaskListView GetTaskListView() => this._taskListView;

    public void ClearBatchSelect()
    {
      this._taskListView?.ViewModel?.ClearBatchSelect();
      this.HideDetail();
    }

    private void InitTimeline()
    {
      App.Instance.LoadTimelineStyle();
      TimelineContainer timelineContainer = new TimelineContainer();
      timelineContainer.HorizontalAlignment = HorizontalAlignment.Stretch;
      this._timeline = timelineContainer;
      this._timeline.SetInitWidth(this.ActualWidth - 90.0 - this._firstColumn.ActualWidth);
      this.Children.Add((UIElement) this._timeline);
      if (!this._showProjectMenu)
        this._timeline.HideFoldMenuIcon();
      this._timeline.SetValue(Grid.ColumnProperty, (object) 0);
      this._timeline.SetValue(Grid.ColumnSpanProperty, (object) 2);
      this._timeline.SetValue(Panel.ZIndexProperty, (object) 10);
    }

    public void SwitchTimelineAndLoad(ProjectIdentity project)
    {
      if (!UserDao.IsPro())
        this.SwitchListAndLoad(project, true);
      else
        this.SwitchTimeline(project);
    }

    public async Task SwitchTimeline(ProjectIdentity projectProject, string taskId = null)
    {
      TaskView taskView = this;
      taskView.SetDisplayMode("timeline");
      if (taskView._timeline == null)
        taskView.InitTimeline();
      else if (!taskView.Children.Contains((UIElement) taskView._timeline))
        taskView.Children.Add((UIElement) taskView._timeline);
      await taskView._timeline.LoadProject(projectProject, taskId);
      taskView.ShowTimelineGuide(projectProject.CatId);
    }

    private async Task ShowTimelineGuide(string pId)
    {
      if (!LocalSettings.Settings.UserPreference.Timeline.ShowGuide)
        ;
      else
      {
        LocalSettings.Settings.SetTimelineShowGuide();
        SettingsHelper.PushLocalPreference();
        LocalSettings.Settings.Save();
        if (!LocalSettings.Settings.ExtraSettings.TLUsed)
        {
          await Task.Delay(750);
          LocalSettings.Settings.ExtraSettings.TLUsed = true;
          LocalSettings.Settings.Save();
          UserActCollectUtils.AddClickEvent("timeline", "user_guide", "feature_introduction");
          await FeatureInfoWindow.Show(Utils.GetString("TimeLine"), Utils.GetString("TimeLineProSummary"), Utils.GetString("TimeLineExtraProSummary"), "TimeLine" + (Utils.IsCn() ? "_cn" : "_en"), Utils.GetString("EnjoyNow"), (Action) null);
        }
        if (CacheManager.GetProjects().Any<ProjectModel>((Func<ProjectModel, bool>) (p => pId != p.id && p.viewMode == "timeline")))
          ;
        else if (CacheManager.GetProjectGroups().Any<ProjectGroupModel>((Func<ProjectGroupModel, bool>) (g => pId != g.id && g.viewMode == "timeline")))
          ;
        else if (CacheManager.GetFilters().Any<FilterModel>((Func<FilterModel, bool>) (f => pId != f.id && f.viewMode == "timeline")))
          ;
        else
        {
          MainWindow mainWindow = App.Window;
          mainWindow.BeginCoach((UIElement) new CoachControl(await CoachModel.GetTimeLineCoach()));
          mainWindow = (MainWindow) null;
        }
      }
    }

    private void ClearTimeline() => this.Children.Remove((UIElement) this._timeline);

    public TaskView(bool showProjectMenu = true)
    {
      Border border = new Border();
      border.RenderTransform = (Transform) new TranslateTransform();
      this._detailContent = border;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this._showProjectMenu = showProjectMenu;
      this._firstColumn = new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star),
        MinWidth = 340.0
      };
      this._secondColumn = new ColumnDefinition()
      {
        Width = new GridLength(1.0, GridUnitType.Star),
        MinWidth = 320.0
      };
      this.ColumnDefinitions.Add(this._firstColumn);
      this.ColumnDefinitions.Add(this._secondColumn);
      this._listContent.SetValue(Grid.ColumnProperty, (object) 0);
      this.Children.Add((UIElement) this._listContent);
      this._detailContent.SetValue(Grid.ColumnProperty, (object) 1);
      this.Children.Add((UIElement) this._detailContent);
      this._listContent.SetResourceReference(Panel.BackgroundProperty, (object) "ShowAreaBackground");
      this._detailContent.SetResourceReference(Panel.BackgroundProperty, (object) "ShowAreaBackground");
      Line line = new Line();
      line.Y1 = 0.0;
      line.Y2 = 1.0;
      line.Stretch = Stretch.Fill;
      line.StrokeThickness = 1.0;
      line.HorizontalAlignment = HorizontalAlignment.Left;
      Line element = line;
      element.SetValue(Grid.ColumnProperty, (object) 1);
      element.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
      this.Children.Add((UIElement) element);
      GridSplitter gridSplitter = new GridSplitter();
      gridSplitter.Width = 3.0;
      gridSplitter.IsTabStop = false;
      gridSplitter.Background = (Brush) Brushes.Transparent;
      gridSplitter.FocusVisualStyle = (Style) null;
      gridSplitter.HorizontalAlignment = HorizontalAlignment.Left;
      gridSplitter.VerticalAlignment = VerticalAlignment.Stretch;
      this._rightSplit = gridSplitter;
      this._rightSplit.SetValue(Grid.ColumnProperty, (object) 1);
      this._rightSplit.SetValue(Panel.ZIndexProperty, (object) 1000);
      this.Children.Add((UIElement) this._rightSplit);
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Unloaded += new RoutedEventHandler(this.OnUnloaded);
      this._rightSplit.DragCompleted += new DragCompletedEventHandler(this.OnSplitDragCompleted);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
      DataChangedNotifier.ScheduleChanged -= new EventHandler(this.OnScheduleChanged);
      DataChangedNotifier.HabitArchived -= new EventHandler<string>(this.OnHabitArchived);
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      TimeChangeNotifier.DayChanged -= new EventHandler<EventArgs>(this.OnDayChanged);
      DataChangedNotifier.EventArchivedChanged -= new EventHandler(this.OnEventArchivedChanged);
      DataChangedNotifier.SortOptionChanged -= new EventHandler<string>(this.OnSortOptionChanged);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      DataChangedNotifier.ScheduleChanged += new EventHandler(this.OnScheduleChanged);
      DataChangedNotifier.HabitArchived += new EventHandler<string>(this.OnHabitArchived);
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      TimeChangeNotifier.DayChanged += new EventHandler<EventArgs>(this.OnDayChanged);
      DataChangedNotifier.EventArchivedChanged += new EventHandler(this.OnEventArchivedChanged);
      DataChangedNotifier.SortOptionChanged += new EventHandler<string>(this.OnSortOptionChanged);
      if (!this._hideSecondColumn)
      {
        IListViewParent parent = Utils.FindParent<IListViewParent>((DependencyObject) this);
        double num = parent != null ? parent.GetDetailWidth() : 1.0;
        this._secondColumn.Width = new GridLength(num < 0.0 ? 1.0 : num, GridUnitType.Star);
      }
      this.Reload();
    }

    private void OnSortOptionChanged(object sender, string e)
    {
      if (!(this.ProjectIdentity.CatId == e))
        return;
      this._kanban?.ReloadIdentity();
      this._taskListView?.ReloadIdentity();
      this._timeline?.ReloadSortOption();
    }

    private async void OnEventArchivedChanged(object sender, EventArgs e)
    {
      this.Reload();
      bool flag = this._calendarDetail != null;
      if (flag)
        flag = await this._calendarDetail.IsArchived();
      if (!flag)
        return;
      this.TryExtractDetail();
    }

    public void OnSplitDragCompleted(object sender, DragCompletedEventArgs e)
    {
      if (this._hideSecondColumn)
        return;
      double num = this._secondColumn.ActualWidth / this._firstColumn.ActualWidth;
      Utils.FindParent<IListViewParent>((DependencyObject) this)?.OnDetailWidthChanged(num);
      this._secondColumn.Width = new GridLength(double.IsNaN(num) || double.IsInfinity(num) ? 1.0 : num, GridUnitType.Star);
      this._firstColumn.Width = new GridLength(1.0, GridUnitType.Star);
    }

    private async void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      try
      {
        if (!e.DeletedChangedIds.Any() && !e.UndoDeletedIds.Any() || this._taskDetailView == null)
          return;
        await Task.Delay(30);
        if (!e.DeletedChangedIds.Contains(this._taskDetailView?.TaskId) && !e.UndoDeletedIds.Contains(this._taskDetailView?.TaskId))
          return;
        this.HideDetail();
      }
      catch (Exception ex)
      {
      }
    }

    public void HideDetail(bool checkColumn = false)
    {
      if (checkColumn && !this._hideSecondColumn)
        return;
      this._taskListView?.SetSelectedId(string.Empty);
      this.ClearDetail();
    }

    private void OnDayChanged(object sender, EventArgs e)
    {
      this._taskListView?.ReloadDataAndQuickAddView();
      this._kanban?.ReloadDataAndColumnQuickAddView();
      this._timeline?.Reload();
      this._taskDetailView?.Reload();
    }

    public new async void OnKeyUp(KeyEventArgs e)
    {
      switch (e.Key)
      {
        case Key.Up:
        case Key.Down:
          await Task.Delay(50);
          if (e.Handled)
            return;
          if (Utils.IfShiftPressed())
          {
            ProjectTaskListView taskListView = this._taskListView;
            if (taskListView != null)
            {
              taskListView.BatchSelectOnMove(e.Key == Key.Up);
              break;
            }
            break;
          }
          ProjectTaskListView taskListView1 = this._taskListView;
          if (taskListView1 != null)
          {
            taskListView1.TryFocusSelectedItem();
            break;
          }
          break;
      }
      this._timeline?.KeyDown(e.Key);
    }

    public void SetListColumnSpan(int span, double firstMinWidth, double secondMinWidth)
    {
      this._listContent.SetValue(Grid.ColumnSpanProperty, (object) span);
      this._firstColumn.MinWidth = firstMinWidth;
      this._secondColumn.MinWidth = secondMinWidth;
    }

    private void SetDisplayMode(string mode)
    {
      Utils.FindParent<MainWindow>((DependencyObject) this)?.SetMainWindowsMinSize(true);
      IListViewParent parent = Utils.FindParent<IListViewParent>((DependencyObject) this);
      switch (mode)
      {
        case "list":
          this._listContent.Visibility = Visibility.Visible;
          this._detailContent.Visibility = Visibility.Visible;
          this._rightSplit.SetValue(Panel.ZIndexProperty, (object) 1000);
          this.ClearKanban();
          this.ClearTimeline();
          this.ClearSummary();
          parent?.SetMinSize(390, 400);
          if (this._taskListView == null)
          {
            this._taskListView = new ProjectTaskListView(this);
            if (!this._showProjectMenu)
              this._taskListView.HideFoldMenuIcon();
            this._listContent.Child = (UIElement) this._taskListView;
            this._taskListView.TaskSelect += new TaskSelectDelegate(this.TaskSelect);
            this._taskListView.EventAdded += new EventHandler<string>(this.EventAdded);
            this._taskListView.BatchTaskSelected += new EventHandler<List<string>>(this.OnBatchSelect);
            this._taskListView.TaskDragging += new EventHandler<DragMouseEvent>(this.OnTaskDragging);
            this._taskListView.BatchTaskDrop += new EventHandler<List<string>>(this.OnBatchTaskDropped);
            this._taskListView.TaskAdded += new EventHandler<TaskModel>(this.OnTaskListTaskAdded);
            this._taskListView.OnSearchFilterChanged += new EventHandler(this.OnSearchFilterChanged);
            this._taskListView.BatchTaskChanged += new EventHandler<List<string>>(this.OnTaskTaskListBatchChanged);
            this._taskListView.TaskDrop += new EventHandler<string>(this.OnTaskListTaskDrop);
            this._taskListView.OnProjectActivityClick += new EventHandler<string>(this.OpenProjectActivity);
            break;
          }
          this._listContent.Child = (UIElement) this._taskListView;
          break;
        case "kanban":
          this._listContent.Visibility = Visibility.Collapsed;
          this._detailContent.Visibility = Visibility.Collapsed;
          this._rightSplit.SetValue(Panel.ZIndexProperty, (object) 0);
          this.ClearTimeline();
          this.ClearList();
          this.ClearSummary();
          parent?.SetMinSize(630, 462);
          break;
        case "timeline":
          this._listContent.Visibility = Visibility.Collapsed;
          this._detailContent.Visibility = Visibility.Collapsed;
          this.ClearKanban();
          this.ClearList();
          this.ClearSummary();
          parent?.SetMinSize(630, 462);
          break;
        case "Summary":
          this._listContent.Visibility = Visibility.Collapsed;
          this._detailContent.Visibility = Visibility.Collapsed;
          this._rightSplit.SetValue(Panel.ZIndexProperty, (object) 0);
          this.ClearTimeline();
          this.ClearList();
          this.ClearKanban();
          parent?.SetMinSize(630, 462);
          break;
      }
    }

    private void ClearList() => this._listContent.Child = (UIElement) null;

    public void SetFoldMenuIcon(bool hideMenu)
    {
      this._hideMenu = hideMenu;
      this._taskListView?.SetFoldMenuIcon(hideMenu);
      this._kanban?.SetFoldMenuIcon(hideMenu);
      this._timeline?.SetFoldMenuIcon(hideMenu);
      this._summary?.SetFoldMenuIcon(hideMenu);
    }

    public void ReloadView(bool hideDetail = false)
    {
      if (this.ProjectIdentity == null)
        return;
      string taskId = this._taskListView == null || !this._taskListView.IsVisible ? (string) null : (hideDetail ? (string) null : this._taskListView?.GetSelectedId() ?? this.GetDetailId());
      this.OnProjectSelect(this.ProjectIdentity.Copy(this.ProjectIdentity), taskId);
    }

    public bool IsMenuPathMouseOver()
    {
      return this._taskListView != null && this._taskListView.MenuPathBorder.IsMouseOver;
    }

    public void Clear()
    {
      this._kanban?.ClearEvent();
      this._taskListView?.Dispose();
      this.ClearDetail();
      if (this._detailContent != null)
        this._detailContent.Child = (UIElement) null;
      this._batchDetail?.Dispose();
      this._batchDetail = (BatchDetailView) null;
      this._timeline?.Dispose();
      this._timeline = (TimelineContainer) null;
      this._kanban?.Dispose();
      this._kanban = (KanbanContainer) null;
    }

    public async void OnProjectSelect(ProjectIdentity identity, string taskId = null, bool showDetailAnim = false)
    {
      if (identity == null)
        return;
      bool flag = identity.Id != this.ProjectIdentity?.Id || identity.CatId != this.ProjectIdentity?.CatId;
      this.ProjectIdentity = identity;
      if (identity is GroupProjectIdentity groupProjectIdentity)
        groupProjectIdentity.CheckSortOption();
      if (identity is NormalProjectIdentity normalProjectIdentity)
      {
        if (normalProjectIdentity.IsNote && identity.ViewMode == "timeline")
          identity.ViewMode = "list";
        if (normalProjectIdentity.Project.userCount >= 2)
          this.TryPullRemoteTasks(normalProjectIdentity.Id);
      }
      if (identity.ViewMode == "kanban")
        this.SwitchKanbanAndLoad(identity, false, true, taskId);
      else if (identity.ViewMode == "timeline" && UserDao.IsPro())
      {
        this.SwitchTimelineAndLoad(identity);
        if (string.IsNullOrEmpty(taskId))
          return;
        TaskDetailWindows.ShowTaskWindows(taskId);
      }
      else if (identity is SummaryProjectIdentity)
      {
        this.SwitchSummary();
      }
      else
      {
        this.SwitchTaskList();
        ProjectTaskListView taskListView = this._taskListView;
        if (taskListView == null)
          return;
        bool inBatch = taskListView.ViewModel?.ProjectIdentity?.Id == identity.Id && this._batchDetail != null;
        bool currentSelected = !string.IsNullOrEmpty(taskId) && this._taskDetailView?.TaskId == taskId;
        if (!string.IsNullOrEmpty(taskId))
        {
          if (!currentSelected)
            taskListView.SetSelectedId(taskId);
        }
        else if (!inBatch | flag)
        {
          taskListView.SetSelectedId((string) null);
          if (flag)
            this.HideDetail();
        }
        await taskListView.LoadProject(identity, forceLoad: true);
        if (inBatch)
          return;
        this.SelectAndShowDetail(taskId, showDetailAnim, !currentSelected);
      }
    }

    private async void SelectAndShowDetail(string taskId, bool showDetailAnim, bool scroll = true)
    {
      if (string.IsNullOrEmpty(taskId))
        return;
      await Task.Delay(100);
      this._taskListView?.SetSelectedId(taskId, scroll);
      this.ShowTaskDetail(taskId, showWithAnim: showDetailAnim);
    }

    private async void TryPullRemoteTasks(string projectId)
    {
      if (!await TaskService.PullTasksOfProject(projectId))
        return;
      this.ReloadView();
    }

    public void TryHideHabitDetail(string habitId = null, bool force = false)
    {
      if (this._habitDetailControl == null)
      {
        this.HideDetail();
      }
      else
      {
        if (!force && !(this._habitDetailControl.GetHabitId() == habitId))
          return;
        this.HideDetail();
        this._habitDetailControl.Load((string) null, DateTime.Today, force: force);
      }
    }

    public string GetSelectedHabitId()
    {
      HabitDetailControl habitDetailControl = this._habitDetailControl;
      return (habitDetailControl != null ? (habitDetailControl.Visibility == Visibility.Visible ? 1 : 0) : 0) == 0 ? string.Empty : this._habitDetailControl.GetHabitId();
    }

    public bool IsEditable()
    {
      if (this._taskDetailView != null)
        return this._taskDetailView.Enable();
      return this._calendarDetail != null && this._calendarDetail != null && this._calendarDetail.Editable;
    }

    public IToastShowWindow GetToastWindow()
    {
      return Utils.FindParent<IToastShowWindow>((DependencyObject) this);
    }

    public void TryToast(string getString)
    {
      this.GetToastWindow()?.TryToastString((object) null, getString);
    }

    public void TryFoldDetail()
    {
      if (!this._hideSecondColumn)
        return;
      this._taskListView?.ForbidShowMenu();
      this.SetDetailVisibleWithAnimation(false);
    }

    public void TryExtractDetail()
    {
      if (this._hideSecondColumn)
      {
        this._taskListView?.ForbidShowMenu();
        this.SetDetailVisibleWithAnimation(false);
      }
      else
        this.HideDetail();
    }

    public void ResetSearchFilterControl() => this._taskListView?.ResetSearchFilterControl();

    public bool OnEsc()
    {
      if (this._detailContent.Visibility != Visibility.Visible)
        return false;
      this.SetDetailVisibleWithAnimation(false);
      return true;
    }

    public void TryPrint(bool isDetail)
    {
      if (this._taskListView == null || !this._taskListView.CanPrint())
        return;
      this._taskListView?.ShowPrintPreview(isDetail);
    }

    public void TryBatchSetPriority(int priority)
    {
      this._taskListView?.TryBatchSetPriority(priority).ContinueWith(new Action<Task>(UtilRun.LogTask));
      this._kanban?.TryBatchSetPriority(priority).ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    public void BatchOpenSticky()
    {
      this._taskListView?.BatchOpenSticky();
      this._kanban?.BatchOpenSticky();
    }

    public void TryBatchSetDate(DateTime? date)
    {
      this._taskListView?.TryBatchSetDate(date).ContinueWith(new Action<Task>(UtilRun.LogTask));
      this._kanban?.TryBatchSetDate(date).ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    public void BatchPinTask()
    {
      this._taskListView?.BatchPinTask();
      this._kanban?.BatchPinTask();
    }

    public void BatchDeleteTask()
    {
      this._taskListView?.DeleteSelectedTasks();
      this._kanban?.BatchDeleteTask();
    }

    public bool TabListAndDetail(IInputElement focusItem)
    {
      if (focusItem is FrameworkElement child)
      {
        TaskDetailView parent1 = Utils.FindParent<TaskDetailView>((DependencyObject) child);
        TaskListView parent2 = Utils.FindParent<TaskListView>((DependencyObject) child);
        if (parent1 != null)
        {
          parent1.TryTabInputBox();
          return true;
        }
        if (parent2 != null)
        {
          this._taskDetailView?.TryFocusTitle();
          return true;
        }
      }
      return false;
    }

    public async Task ExpandOrFoldAllTask(bool isOpen)
    {
      if (this._kanban != null && this._kanban.IsVisible)
        this._kanban.ExpandOrFoldAllTask();
      if (this._taskListView == null || !this._taskListView.IsVisible)
        return;
      this._taskListView.ExpandOrFoldAllTask(isOpen);
    }

    public async Task ExpandOrFoldAllSection()
    {
      if (this._timeline != null && this._timeline.IsVisible)
        this._timeline.ToggleGroupsOpen();
      if (this._kanban != null && this._kanban.IsVisible)
        this._kanban.ExpandOrFoldAllSection();
      if (this._taskListView == null || !this._taskListView.IsVisible)
        return;
      this._taskListView.ExpandOrFoldAllSection();
    }

    public void TabSelectItem() => this._taskListView?.TabSelectItem();

    public void TabKanbanSelect()
    {
      if (this._kanban == null || !this._kanban.IsVisible)
        return;
      this._kanban.TabSelect();
    }

    public void OnTouchScroll(int offset)
    {
      if (this._kanban != null && this._kanban.IsVisible)
        this._kanban.OnScroll(offset);
      if (this._timeline == null || this._timeline.Visibility != Visibility.Visible)
        return;
      this._timeline.OnTouchPadHorizontalScroll(offset);
    }

    public void Reload()
    {
      this._kanban?.Reload(true, true);
      this._timeline?.Reload();
      this._taskListView?.ReloadData();
    }

    public void FoldDetail()
    {
      this._hideSecondColumn = true;
      if (this._detailContent.Tag == (object) "Float")
        return;
      this.ShowDetailBackMenu();
      this._detailContent.Tag = (object) "Float";
      this._detailContent.SetValue(Panel.ZIndexProperty, (object) 100);
      this._secondColumn.MaxWidth = 0.0;
      this._secondColumn.MinWidth = 0.0;
      this.SetFoldStyle(true);
      this.SetDetailVisibleWithAnimation(false);
      this._rightSplit.Visibility = Visibility.Collapsed;
    }

    public void ExpandDetail()
    {
      this._hideSecondColumn = false;
      if (this._detailContent.Tag != (object) "Float")
        return;
      this.HideDetailBackMenu();
      this._detailContent.Tag = (object) "Normal";
      this.SetFoldStyle(false);
      this._secondColumn.MinWidth = 0.0;
      this._secondColumn.MaxWidth = double.PositiveInfinity;
      this.SetColumnWidth();
      this._rightSplit.Visibility = Visibility.Visible;
    }

    private void SetColumnWidth()
    {
      double num1 = this._secondColumn.ActualWidth + this._secondColumn.ActualWidth;
      double num2 = num1 - 302.0;
      double num3 = num1 - 258.0;
      double num4 = num2 > 0.0 ? num2 : num3;
      this._secondColumn.Width = new GridLength((num3 > 0.0 ? num3 : num4) / num4, GridUnitType.Star);
      this._firstColumn.Width = new GridLength(1.0, GridUnitType.Star);
    }

    private void SetFoldStyle(bool fold)
    {
      if (fold)
      {
        this._detailContent.SetValue(Grid.ColumnProperty, (object) 0);
        this._detailContent.SetValue(Grid.ColumnSpanProperty, (object) 2);
        this._detailContent.MaxWidth = 340.0;
        this._detailContent.MinWidth = 340.0;
        this._detailContent.HorizontalAlignment = HorizontalAlignment.Right;
        this._detailContent.RenderTransform = (Transform) new TranslateTransform()
        {
          X = 340.0
        };
        this._detailContent.SetResourceReference(Panel.BackgroundProperty, (object) "FoldAreaBackground");
      }
      else
      {
        this._detailContent.MinWidth = 0.0;
        this._detailContent.MaxWidth = double.PositiveInfinity;
        this._detailContent.SetValue(Grid.ColumnProperty, (object) 1);
        this._detailContent.SetValue(Grid.ColumnSpanProperty, (object) 1);
        this._detailContent.SetValue(Panel.ZIndexProperty, (object) 0);
        this._detailContent.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) null);
        this._detailContent.RenderTransform = (Transform) new TranslateTransform()
        {
          X = 0.0
        };
        this._detailContent.HorizontalAlignment = HorizontalAlignment.Stretch;
        this._detailContent.SetResourceReference(Panel.BackgroundProperty, (object) "ShowAreaBackground");
        this._detailContent.Effect = (Effect) null;
      }
    }

    public void TryFoldDetailOnMouseDown(MouseButtonEventArgs e)
    {
      if (this._detailContent.IsMouseOver || !this._hideSecondColumn)
        return;
      TaskDetailView taskDetailView = this._taskDetailView;
      if ((taskDetailView != null ? (taskDetailView.PopupShowing ? 1 : 0) : 0) != 0 || e.GetPosition((IInputElement) this).X > this.ActualWidth - this._detailContent.ActualWidth || this._detailContent.Visibility != Visibility.Visible)
        return;
      this._needHideDetail = true;
      DelayActionHandlerCenter.TryDoAction("MainWindowDelayHideDetail", new EventHandler(this.TryHideDetail), 200);
    }

    private void TryHideDetail(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() =>
      {
        if (!this._needHideDetail)
          return;
        this.SetDetailVisibleWithAnimation(false);
      }));
    }

    public void SetDetailVisibleWithAnimation(bool show)
    {
      if (!this._hideSecondColumn)
        return;
      if (show)
      {
        this._needHideDetail = false;
        if (this._detailContent.Effect == null)
          this._detailContent.Effect = (Effect) new DropShadowEffect()
          {
            Opacity = 0.12,
            BlurRadius = 8.0,
            ShadowDepth = 4.0,
            Direction = 180.0
          };
        if (!(this._detailContent?.RenderTransform is TranslateTransform renderTransform))
          return;
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(renderTransform.X), 0.0, 240);
        renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation);
      }
      else
      {
        this._detailContent.Effect = (Effect) null;
        if (!(this._detailContent?.RenderTransform is TranslateTransform renderTransform))
          return;
        DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(renderTransform.X), 340.0, 240);
        renderTransform.BeginAnimation(TranslateTransform.XProperty, (AnimationTimeline) doubleAnimation);
      }
    }

    private void SetDetailHintVisible(bool showHint)
    {
      this._hintPanel.Visibility = showHint ? Visibility.Visible : Visibility.Collapsed;
    }

    public ProjectIdentity GetSelectedProject()
    {
      ProjectTaskListView taskListView = this._taskListView;
      return (taskListView != null ? (taskListView.InSearch() ? 1 : 0) : 0) != 0 ? (ProjectIdentity) new SearchProjectIdentity() : this.ProjectIdentity;
    }

    public void ResetColumnWidth(double width)
    {
      if (this._hideSecondColumn || !this.IsLoaded || this._secondColumn.ActualWidth - 8.0 > this._secondColumn.MinWidth && this._firstColumn.ActualWidth - 8.0 > this._firstColumn.MinWidth)
        return;
      double num1 = this._firstColumn.ActualWidth - 8.0 <= this._firstColumn.MinWidth ? this._firstColumn.MinWidth + 12.0 : this._firstColumn.ActualWidth;
      double num2 = this._secondColumn.ActualWidth - 8.0 <= this._secondColumn.MinWidth ? this._secondColumn.MinWidth + 12.0 : this._secondColumn.ActualWidth;
      if (num1 + num2 > width)
      {
        if (num1 > num2)
          num1 = width - num2;
        else
          num2 = width - num1;
      }
      double d = num2 / num1;
      if (d <= 0.0)
        return;
      this._secondColumn.Width = new GridLength(double.IsNaN(d) || double.IsInfinity(d) ? 1.0 : d, GridUnitType.Star);
      this._firstColumn.Width = new GridLength(1.0, GridUnitType.Star);
    }

    public void SetColumns()
    {
      this._firstColumn.Width = new GridLength(1.0, GridUnitType.Star);
      this._secondColumn.Width = new GridLength(1.0, GridUnitType.Star);
    }
  }
}
