// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TaskBar
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Widget;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TaskBar : Border, IShowTaskDetailWindow, IComponentConnector
  {
    public static readonly DependencyProperty BarMarginProperty = DependencyProperty.Register(nameof (BarMargin), typeof (Thickness), typeof (TaskBar), new PropertyMetadata((object) new Thickness(4.0, 1.0, 4.0, 1.0), new PropertyChangedCallback(TaskBar.OnMarginChangeCallback)));
    private CalendarControl _calendarControl;
    private HabitCheckInWindow _checkInWindow;
    private System.Windows.Point _dragStart = new System.Windows.Point(0.0, 0.0);
    private IDragBarEvent _dragTarget;
    private IToastShowWindow _parentWindow;
    private bool _pressed;
    private bool _inArrange;
    private string _taskId;
    private bool _isWaitingDoubleClick;
    private DateTime _lastClickTime;
    private System.Windows.Point _firstClickPoint;
    private bool _detailShowing;
    private Path _foldPath;
    private Path _repeatIcon;
    internal Border Container;
    internal DockPanel TaskBarContainer;
    internal TextBlock TimeText;
    internal EmjTextBlock TitleText;
    private bool _contentLoaded;

    public TaskBar()
    {
      this.InitializeComponent();
      this.SetEvent();
    }

    private void SetEvent()
    {
      this.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTaskClick);
      this.MouseRightButtonUp += new MouseButtonEventHandler(this.OnShowOperation);
      this.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnTaskMouseDown);
    }

    public Thickness BarMargin
    {
      get => (Thickness) this.GetValue(TaskBar.BarMarginProperty);
      set => this.SetValue(TaskBar.BarMarginProperty, (object) value);
    }

    public bool InArrange
    {
      get => this._inArrange;
      set
      {
        this._inArrange = value;
        this.AddFoldPath();
      }
    }

    public event EventHandler PopupClosed;

    private static void OnMarginChangeCallback(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(sender is TaskBar taskBar) || e.NewValue == null)
        return;
      taskBar.Container.Margin = (Thickness) e.NewValue;
    }

    private IToastShowWindow GetToastWindow()
    {
      this._parentWindow = this._parentWindow ?? Utils.FindParent<IToastShowWindow>((DependencyObject) this);
      return this._parentWindow;
    }

    private void OnMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || !this._pressed)
        return;
      if (this.DataContext == null)
        return;
      CalendarDisplayViewModel model = this.DataContext as CalendarDisplayViewModel;
      if (model == null)
        return;
      if (model.CanDrag)
      {
        this.TryStartDrag(e);
      }
      else
      {
        this.TryToastUnableString(CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.ProjectId)));
        this.TaskBarContainer.MouseMove -= new MouseEventHandler(this.OnMove);
      }
    }

    private async void TryStartDrag(MouseEventArgs e)
    {
      TaskBar element = this;
      if (!Utils.CheckMoveDelta((FrameworkElement) element, e, element._dragStart, 1) || element.DataContext == null || !(element.DataContext is CalendarDisplayViewModel dataContext))
        return;
      bool canDrag;
      bool needToast;
      string toastStr;
      CalendarEventDragHelper.CheckIfEventCanDrag(dataContext, out canDrag, out needToast, out toastStr);
      if (canDrag)
      {
        dataContext.Dragging = true;
        element.GetDragTarget()?.OnDragStart((CalendarDisplayViewModel) element.DataContext, e, false);
      }
      else if (needToast)
        element.GetToastWindow()?.TryToastString((object) null, toastStr);
      element._pressed = false;
    }

    private IDragBarEvent GetDragTarget()
    {
      this._dragTarget = this._dragTarget ?? Utils.FindParent<IDragBarEvent>((DependencyObject) this);
      return this._dragTarget;
    }

    private async void OnTaskClick(object sender, MouseButtonEventArgs e)
    {
      TaskBar taskBar = this;
      e.Handled = true;
      CalendarControl calendarParent = taskBar.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0 && (!(taskBar.DataContext is CalendarDisplayViewModel dataContext1) || !dataContext1.IsLoadMore))
        return;
      IDragBarEvent dragTarget = taskBar.GetDragTarget();
      if ((dragTarget != null ? (dragTarget.OnSelection() ? 1 : 0) : 0) != 0 || PopupStateManager.IsInSelection() || PopupStateManager.IsInAdd())
        return;
      if (taskBar._detailShowing)
      {
        taskBar._detailShowing = false;
      }
      else
      {
        taskBar.TaskBarContainer.MouseMove -= new MouseEventHandler(taskBar.OnMove);
        if (taskBar.DataContext != null && taskBar.DataContext is CalendarDisplayViewModel dataContext2)
        {
          if (dataContext2.IsCalendarEvent)
            await taskBar.ShowEventDetail((MouseEventArgs) e, dataContext2);
          else if (dataContext2.IsHabit)
            await taskBar.ShowHabitDetail((MouseEventArgs) e, dataContext2);
          else if (dataContext2.IsPomo)
            taskBar.ShowPomoDetail(dataContext2);
          else if (dataContext2.Course != null)
            taskBar.ShowCourseDetail(dataContext2);
          else
            taskBar.TryShowTaskDetail(dataContext2);
        }
        taskBar._pressed = false;
      }
    }

    private async void ShowPomoDetail(CalendarDisplayViewModel model)
    {
      TaskBar target = this;
      PomodoroModel pomo = await PomoDao.GetPomoById(model.PomoId);
      if (pomo == null)
      {
        pomo = (PomodoroModel) null;
      }
      else
      {
        model.Selected = true;
        PomodoroModel pomodoroModel = pomo;
        pomodoroModel.Tasks = await PomoTaskDao.GetByPomoId(pomo.Id);
        pomodoroModel = (PomodoroModel) null;
        PomoDetailWindow pomoDetailWindow = new PomoDetailWindow(new PomoDisplayViewModel(pomo, pomo.Tasks), model);
        pomoDetailWindow.Closed += (EventHandler) (async (obj, e) =>
        {
          model.Selected = false;
          this.OnDetailClosed(obj, string.Empty);
          await Task.Delay(200);
          this._detailShowing = false;
        });
        target._detailShowing = true;
        pomoDetailWindow.Show((UIElement) target, target.ActualWidth, target.ActualWidth > 300.0);
        CalendarControl calendarParent = target.GetCalendarParent();
        if (calendarParent == null)
        {
          pomo = (PomodoroModel) null;
        }
        else
        {
          calendarParent.SetEditting(true);
          pomo = (PomodoroModel) null;
        }
      }
    }

    private async void TryShowTaskDetail(CalendarDisplayViewModel model)
    {
      this._isWaitingDoubleClick = false;
      bool firstClick = (DateTime.Now - this._lastClickTime).TotalMilliseconds > 300.0;
      this._lastClickTime = DateTime.Now;
      if (firstClick)
      {
        this._isWaitingDoubleClick = true;
        model.Selected = true;
        this._firstClickPoint = PopupLocationCalculator.GetMousePoint(false);
        TaskDetailPopup window = await this.ShowTaskDetail(model);
        if (window != null)
          window.Opacity = 0.0;
        await Task.Delay(150);
        if (!this._isWaitingDoubleClick)
        {
          window?.Clear();
          return;
        }
        if (window != null)
          window.Opacity = 1.0;
        window = (TaskDetailPopup) null;
      }
      if (firstClick)
      {
        UserActCollectUtils.AddClickEvent("calendar", "action", "click_task");
      }
      else
      {
        UserActCollectUtils.AddClickEvent("calendar", "action", "double_click_task");
        model.Selected = false;
        TaskDetailWindows.ShowTaskWindows(model.TaskId);
      }
    }

    private async Task ShowHabitDetail(MouseEventArgs e, CalendarDisplayViewModel model)
    {
      TaskBar target = this;
      HabitModel habitById = await HabitDao.GetHabitById(model.HabitId);
      if (habitById == null)
        ;
      else
      {
        HabitCheckInWindow window = new HabitCheckInWindow(habitById, model.Status, model.StartDate, target.GetToastWindow());
        window.Show((UIElement) target, target.ActualWidth, 0.0, target.ActualWidth >= 350.0);
        target.GetCalendarParent()?.SetEditting(true);
        window.OnAction += new EventHandler(target.OnCheckInAction);
        window.Closed += (EventHandler) ((sender, args) => window.OnAction -= new EventHandler(this.OnCheckInAction));
        model.Selected = true;
      }
    }

    private async void OnCheckInAction(object sender, EventArgs extra)
    {
      this.HandleOnWindowClosed();
    }

    private async void ShowCourseDetail(CalendarDisplayViewModel model)
    {
      TaskBar target = this;
      if (model.Course == null)
        ;
      else
      {
        CourseDetailViewModel viewModel = await CourseDetailViewModel.Build(model.Course);
        model.Selected = true;
        CourseDetailWindow courseDetailWindow = new CourseDetailWindow(viewModel);
        courseDetailWindow.Closed += (EventHandler) (async (obj, e) =>
        {
          model.Selected = false;
          this.OnDetailClosed(obj, string.Empty);
          await Task.Delay(200);
          this._detailShowing = false;
        });
        target._detailShowing = true;
        courseDetailWindow.Show((UIElement) target, target.ActualWidth, target.ActualWidth > 300.0);
        CalendarControl calendarParent = target.GetCalendarParent();
        if (calendarParent == null)
          ;
        else
          calendarParent.SetEditting(true);
      }
    }

    private async Task ShowEventDetail(
      MouseEventArgs e,
      CalendarDisplayViewModel calendarDisplayViewModel)
    {
      TaskBar target = this;
      string eventId = calendarDisplayViewModel.EventId;
      if (string.IsNullOrEmpty(eventId))
        return;
      calendarDisplayViewModel.Selected = true;
      CalendarEventModel eventById = await CalendarEventDao.GetEventById(ArchivedDao.GetOriginalId(eventId));
      if (eventById == null)
        return;
      if (target.DataContext != null && target.DataContext is CalendarDisplayViewModel dataContext)
      {
        eventById.DueStart = dataContext.StartDate;
        eventById.DueEnd = dataContext.DueDate;
      }
      CalendarDetailWindow calendarDetailWindow = new CalendarDetailWindow(target.GetWidgetThemeId());
      calendarDetailWindow.Disappear += new EventHandler<string>(target.OnDetailClosed);
      calendarDetailWindow.EventArchiveChanged += new EventHandler<bool>(target.OnEventArchiveChanged);
      calendarDetailWindow.Show((UIElement) target, target.ActualWidth, 0.0, target.ActualWidth > 350.0, new CalendarDetailViewModel(eventById));
      target.GetCalendarParent()?.SetEditting(true);
    }

    private void OnEventArchiveChanged(object sender, bool archive)
    {
      CalendarDisplayViewModel model = this.GetModel();
      if (model == null)
        return;
      model.SourceViewModel.Status = archive ? 2 : 0;
    }

    private async Task<TaskDetailPopup> ShowTaskDetail(CalendarDisplayViewModel model)
    {
      TaskBar target = this;
      if (model == null)
        return (TaskDetailPopup) null;
      TaskDetailViewModel model1 = await TaskDetailViewModel.Build(model.GetTaskId());
      if (model1 == null)
        return (TaskDetailPopup) null;
      model1.RepeatDiff = string.IsNullOrEmpty(model.RepeatFlag) ? new double?() : new double?(model.RepeatDiff);
      TaskDetailPopup taskDetailPopup = new TaskDetailPopup();
      IToastShowWindow toastWindow = target.GetToastWindow();
      if (toastWindow != null)
      {
        if (PopupStateManager.LastTarget == target)
          return (TaskDetailPopup) null;
        taskDetailPopup.DependentWindow = toastWindow;
      }
      model.Selected = true;
      taskDetailPopup.Disappear -= new EventHandler<string>(target.OnDetailClosed);
      taskDetailPopup.Disappear += new EventHandler<string>(target.OnDetailClosed);
      taskDetailPopup.Show(model1, model.ItemId, new TaskWindowDisplayArgs((UIElement) target, target.ActualWidth, target.ActualWidth < 350.0 ? new System.Windows.Point() : target._firstClickPoint, 0.0));
      target.GetCalendarParent()?.SetEditting(true);
      return taskDetailPopup;
    }

    private string GetWidgetThemeId()
    {
      CalendarWidget parent = Utils.FindParent<CalendarWidget>((DependencyObject) this);
      return parent != null ? parent.ThemeId : string.Empty;
    }

    private bool CheckIfDayMode() => this.ActualWidth > 245.0;

    private void OnDetailClosed(object sender, string e)
    {
      this.HandleOnWindowClosed();
      if (!(sender is TaskDetailWindow taskDetailWindow))
        return;
      taskDetailWindow.Disappear -= new EventHandler<string>(this.OnDetailClosed);
    }

    private void HandleOnWindowClosed()
    {
      if (this.DataContext is CalendarDisplayViewModel dataContext)
        dataContext.Selected = false;
      this.GetCalendarParent()?.SetEditting(false);
      this.GetCalendarParent()?.GetFocus(true);
      EventHandler popupClosed = this.PopupClosed;
      if (popupClosed == null)
        return;
      popupClosed((object) this, (EventArgs) null);
    }

    private CalendarControl GetCalendarParent()
    {
      this._calendarControl = this._calendarControl ?? Utils.FindParent<CalendarControl>((DependencyObject) this);
      return this._calendarControl;
    }

    private void OnTaskMouseDown(object sender, MouseButtonEventArgs e)
    {
      CalendarControl calendarParent = this.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0)
        return;
      this._pressed = true;
      this._dragStart = e.GetPosition((IInputElement) this);
      this.TaskBarContainer.MouseMove -= new MouseEventHandler(this.OnMove);
      this.TaskBarContainer.MouseMove += new MouseEventHandler(this.OnMove);
    }

    private bool TryToastUnableString(ProjectModel model)
    {
      if (model != null && !string.IsNullOrEmpty(model.permission))
      {
        IToastShowWindow toastWindow = this.GetToastWindow();
        switch (model.permission)
        {
          case "read":
            string str1 = Utils.GetString("ReadOnly");
            toastWindow?.TryToastString((object) null, string.Format(Utils.GetString("NoPermissionToEdit"), (object) str1));
            return true;
          case "comment":
            string str2 = Utils.GetString("CanComment");
            toastWindow?.TryToastString((object) null, string.Format(Utils.GetString("NoPermissionToEdit"), (object) str2));
            return true;
        }
      }
      return false;
    }

    private async void OnShowOperation(object sender, MouseButtonEventArgs e)
    {
      TaskBar ele = this;
      CalendarControl calendarParent = ele.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0 || ele.DataContext == null)
        return;
      CalendarDisplayViewModel model = ele.DataContext as CalendarDisplayViewModel;
      if (model == null)
        return;
      if (model.Type == DisplayType.Task || model.Type == DisplayType.Note || model.Type == DisplayType.Derivative)
      {
        model.Selected = true;
        ele._taskId = model.TaskId;
      }
      else
        ele._taskId = (string) null;
      if (model.Type == DisplayType.Event)
      {
        if (model.SourceViewModel.OutDate())
          return;
        model.Selected = true;
        CalendarOperationDialog calendarOperationDialog = new CalendarOperationDialog(new EventArchiveArgs(model.SourceViewModel), model.Status != 0);
        calendarOperationDialog.SetPlaceTarget((UIElement) ele);
        calendarOperationDialog.Closed += new EventHandler(ele.OnDialogClosed);
        ele.GetCalendarParent()?.SetEditting(true);
        calendarOperationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) ((arg, obj) => model.SourceViewModel.Status = obj.Value == ActionType.Archive ? 2 : 0);
        calendarOperationDialog.Show();
      }
      else if (model.Type == DisplayType.Course)
      {
        DateTime? startDate = model.StartDate;
        DateTime today = DateTime.Today;
        if ((startDate.HasValue ? (startDate.GetValueOrDefault() < today ? 1 : 0) : 0) != 0)
          return;
        model.Selected = true;
        ArchiveOperationDialog archiveOperationDialog = new ArchiveOperationDialog(model.Course.UniqueId, ArchiveKind.Course, model.Course.Archived);
        archiveOperationDialog.SetPlaceTarget((UIElement) ele);
        archiveOperationDialog.Closed += new EventHandler(ele.OnDialogClosed);
        ele.GetCalendarParent()?.SetEditting(true);
        archiveOperationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) ((arg, obj) =>
        {
          model.Course.Archived = obj.Value == ActionType.Archive;
          model.SourceViewModel.Status = model.Course.Archived || model.Course.CourseStart.Date < DateTime.Today ? 2 : 0;
        });
        archiveOperationDialog.Show();
      }
      else
      {
        ProjectModel model1 = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.ProjectId));
        if (model.Type == DisplayType.Habit && model.Status == 2 || ele.TryToastUnableString(model1))
          model.Selected = false;
        else if (model.Type == DisplayType.Habit)
          ele.ShowHabitOperation(model);
        else
          await ele.ShowTaskOperation(model);
      }
    }

    private void ShowHabitOperation(CalendarDisplayViewModel model)
    {
      if (model.Status != 0)
        return;
      List<OperationItemViewModel> types = new List<OperationItemViewModel>()
      {
        new OperationItemViewModel(ActionType.Skip)
      };
      if (LocalSettings.Settings.EnableFocus)
        types.Insert(0, new OperationItemViewModel(ActionType.StartFocus)
        {
          SubActions = new List<OperationItemViewModel>()
          {
            new OperationItemViewModel(ActionType.StartPomo)
            {
              Enable = !TickFocusManager.Working || LocalSettings.Settings.PomoType == FocusConstance.Focus
            },
            new OperationItemViewModel(ActionType.StartTiming)
            {
              Enable = !TickFocusManager.Working || LocalSettings.Settings.PomoType == FocusConstance.Timing
            }
          }
        });
      OperationDialog operationDialog = new OperationDialog(model.HabitId, types);
      operationDialog.SetPlaceTarget((UIElement) this);
      operationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) (async (obj, kv) =>
      {
        switch (kv.Value)
        {
          case ActionType.Skip:
            await HabitService.SkipHabit(model.HabitId);
            this.GetCalendarParent()?.RemoveItem(model);
            Utils.FindParent<LoadMoreWindow>((DependencyObject) this)?.RemoveItem(this);
            this.GetToastWindow()?.TryToastString((object) null, Utils.GetString("Skipped"));
            break;
          case ActionType.StartTiming:
            TickFocusManager.TryStartFocusHabit(kv.Key, false);
            break;
          case ActionType.StartPomo:
            TickFocusManager.TryStartFocusHabit(kv.Key, true);
            break;
        }
      });
      operationDialog.Show();
    }

    private async Task ShowTaskOperation(CalendarDisplayViewModel model)
    {
      TaskBar element = this;
      if (string.IsNullOrEmpty(element._taskId))
        ;
      else
      {
        element.GetToastWindow();
        OperationExtra taskAccessInfo = await TaskOperationHelper.GetTaskAccessInfo(element._taskId, false);
        if (taskAccessInfo.TimeModel == null)
          ;
        else
        {
          taskAccessInfo.TimeModel.StartDate = model.DisplayStartDate ?? model.StartDate;
          taskAccessInfo.TimeModel.DueDate = model.DisplayDueDate ?? model.DueDate;
          TaskOperationDialog dialog = new TaskOperationDialog(taskAccessInfo, (UIElement) element);
          if (Window.GetWindow((DependencyObject) element) is WidgetWindow window)
            dialog.Resources = window.Resources;
          dialog.Closed += new EventHandler(element.OnDialogClosed);
          dialog.PrioritySelect += new EventHandler<int>(element.SetPriority);
          dialog.Copied += new EventHandler(element.CopyTask);
          dialog.LinkCopied += new EventHandler(element.CopyTaskLink);
          dialog.Deleted += new EventHandler(element.DeleteTask);
          dialog.AbandonOrReopen += new EventHandler(element.OnAbandonOrReopenTask);
          dialog.ProjectSelect += new EventHandler<SelectableItemViewModel>(element.OnProjectSelect);
          dialog.AssigneeSelect += new EventHandler<AvatarInfo>(element.OnAssigneeSelect);
          dialog.TagsSelect += new EventHandler<TagSelectData>(element.OnTagsSelect);
          dialog.SkipCurrentRecurrence += new EventHandler(element.OnSkipRecurrence);
          dialog.SwitchTaskOrNote += new EventHandler(element.OnSwitchTaskOrNoteClick);
          dialog.Toast += new EventHandler<string>(element.OnTaskOperationToast);
          dialog.CompleteDateChanged += (EventHandler<DateTime>) (async (o, date) =>
          {
            await TaskService.ChangeCompleteDate(model.TaskId, date);
            SyncManager.TryDelaySync();
          });
          dialog.TimeClear += (EventHandler) (async (arg, obj) =>
          {
            dialog.Dismiss();
            await this.ClearDate();
          });
          dialog.TimeSelect += (EventHandler<TimeData>) (async (arg, data) =>
          {
            dialog.Dismiss();
            if (!string.IsNullOrEmpty(model.RepeatFlag) && model.StartDate.HasValue && model.Status == 0)
              ModifyRepeatHandler.TryUpdateDueDate(model.TaskId, model.DisplayStartDate, model.DisplayDueDate, data, 1, 1);
            else
              await TaskService.SetDate(this._taskId, data);
          });
          dialog.QuickDateSelect += (EventHandler<DateTime>) (async (arg, date) =>
          {
            dialog.Dismiss();
            if (!string.IsNullOrEmpty(model.RepeatFlag) && model.Status == 0)
            {
              TaskModel task = await TaskDao.GetThinTaskById(model.TaskId);
              if (task == null)
                return;
              DateTime? startDate = model.StartDate;
              if (!startDate.HasValue)
                return;
              startDate = task.startDate;
              if (!startDate.HasValue)
                return;
              List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task.id);
              TimeData timeData1 = new TimeData();
              timeData1.StartDate = model.StartDate;
              timeData1.DueDate = model.DueDate;
              startDate = model.StartDate;
              timeData1.IsAllDay = new bool?(!startDate.HasValue || ((int) model.IsAllDay ?? 1) != 0);
              timeData1.Reminders = remindersByTaskId;
              timeData1.RepeatFrom = model.RepeatFrom;
              timeData1.RepeatFlag = model.RepeatFlag;
              timeData1.ExDates = task.exDate != null ? ((IEnumerable<string>) task.exDate).ToList<string>() : new List<string>();
              TimeData timeData2 = timeData1;
              ModifyRepeatHandler.TryUpdateDueDateOnlyDate(model.TaskId, model.DisplayStartDate, model.DisplayDueDate, timeData2, date, 1, 1);
            }
            else
            {
              TaskModel taskModel = await TaskService.SetStartDate(this._taskId, date);
              SyncManager.TryDelaySync();
            }
          });
          dialog.Show();
          CalendarControl calendarParent = element.GetCalendarParent();
          if (calendarParent == null)
            ;
          else
            calendarParent.SetEditting(true);
        }
      }
    }

    private void OnTaskOperationToast(object sender, string e)
    {
      this.GetToastWindow()?.TryToastString((object) null, e);
    }

    private async void OnAbandonOrReopenTask(object sender, EventArgs e)
    {
      TaskBar taskBar = this;
      if (taskBar.DataContext == null)
        model = (CalendarDisplayViewModel) null;
      else if (!(taskBar.DataContext is CalendarDisplayViewModel model))
      {
        model = (CalendarDisplayViewModel) null;
      }
      else
      {
        if (model.DisplayStartDate.HasValue)
        {
          DateTime? startDate = model.StartDate;
          DateTime? displayStartDate = model.DisplayStartDate;
          if ((startDate.HasValue == displayStartDate.HasValue ? (startDate.HasValue ? (startDate.GetValueOrDefault() != displayStartDate.GetValueOrDefault() ? 1 : 0) : 0) : 1) != 0 && model.Status == 0)
          {
            if (!await ModifyRepeatHandler.CompleteOrSkipRecurrence(model.TaskId, model.DisplayStartDate, taskBar.GetToastWindow()))
            {
              model = (CalendarDisplayViewModel) null;
              return;
            }
          }
        }
        int closeStatus = model.IsAbandoned ? 0 : -1;
        TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(model.TaskId, closeStatus, closeStatus != 0, taskBar.GetToastWindow());
        model = (CalendarDisplayViewModel) null;
      }
    }

    private async void OnSwitchTaskOrNoteClick(object sender, EventArgs e)
    {
      TaskBar taskBar = this;
      if (taskBar.DataContext == null || !(taskBar.DataContext is CalendarDisplayViewModel dataContext))
        return;
      await TaskService.SwitchTaskOrNote(dataContext.TaskId);
      SyncManager.TryDelaySync();
    }

    private async void CopyTaskLink(object sender, EventArgs e)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(this._taskId);
      if (thinTaskById == null)
        return;
      TaskUtils.CopyTaskLink(thinTaskById.id, thinTaskById.projectId, thinTaskById.title);
    }

    private async Task ClearDate()
    {
      TaskBar parent = this;
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(parent._taskId);
      if (thinTaskById == null)
        return;
      if (string.IsNullOrEmpty(thinTaskById.attendId))
      {
        await TaskService.ClearDate(parent._taskId);
        SyncManager.TryDelaySync();
      }
      else
      {
        if (!await TaskOperationHelper.CheckIfTaskAllowClearDate(parent._taskId, (DependencyObject) parent))
          return;
        await TaskService.ClearAgendaDate(parent._taskId);
        SyncManager.TryDelaySync();
      }
    }

    private async void OnSkipRecurrence(object sender, EventArgs e)
    {
      CalendarDisplayViewModel model = this.GetModel();
      if (model != null && model.DisplayStartDate.HasValue)
      {
        DateTime? displayStartDate = model.DisplayStartDate;
        DateTime? nullable = model.StartDate;
        if ((displayStartDate.HasValue == nullable.HasValue ? (displayStartDate.HasValue ? (displayStartDate.GetValueOrDefault() != nullable.GetValueOrDefault() ? 1 : 0) : 0) : 1) != 0)
        {
          string taskId = this._taskId;
          nullable = model.DisplayStartDate;
          DateTime date = nullable.Value.Date;
          IToastShowWindow toastWindow = this.GetToastWindow();
          await TaskService.SkipSelectRecurrence(taskId, date, toastWindow);
          goto label_6;
        }
      }
      TaskModel taskModel = await TaskService.SkipCurrentRecurrence(this._taskId, toastWindow: this.GetToastWindow());
label_6:
      SyncManager.TryDelaySync();
    }

    private async void OnTagsSelect(object sender, TagSelectData tags)
    {
      await TaskService.SetTags(this._taskId, tags.OmniSelectTags);
      SyncManager.TryDelaySync();
    }

    private async void OnAssigneeSelect(object sender, AvatarInfo assignee)
    {
      await TaskService.SetAssignee(this._taskId, assignee.UserId);
      SyncManager.TryDelaySync();
    }

    private async void OnProjectSelect(object sender, SelectableItemViewModel e)
    {
      (string projectId1, string str1) = e.GetProjectAndColumnId();
      string taskId = this._taskId;
      string projectId2 = projectId1;
      string str2 = str1;
      bool? isTop = new bool?();
      string columnId = str2;
      await TaskService.MoveProject(taskId, projectId2, isTop, columnId);
      SyncManager.TryDelaySync();
      if (string.IsNullOrEmpty(await CalendarUtils.CheckAddTaskCanShown(this._taskId)))
      {
        projectId1 = (string) null;
      }
      else
      {
        IToastShowWindow toastWindow = this.GetToastWindow();
        if (toastWindow == null)
        {
          projectId1 = (string) null;
        }
        else
        {
          toastWindow.ToastMoveProjectControl(projectId1);
          projectId1 = (string) null;
        }
      }
    }

    private async void DeleteTask(object sender, EventArgs e)
    {
      TaskBar parent = this;
      PopupStateManager.OnViewPopupClosed(false);
      TaskModel task = await TaskDao.GetThinTaskById(parent._taskId);
      if (task == null)
      {
        task = (TaskModel) null;
      }
      else
      {
        if (string.IsNullOrEmpty(task.attendId))
        {
          if (!string.IsNullOrEmpty(task.repeatFlag) && task.startDate.HasValue && task.status == 0 && parent.DataContext != null && parent.DataContext is CalendarDisplayViewModel dataContext && dataContext.TaskId == task.id)
          {
            ModifyRepeatHandler.TryDeleteRecurrence(task.id, dataContext.DisplayStartDate, dataContext.DisplayDueDate, parent.GetToastWindow());
            task = (TaskModel) null;
            return;
          }
          List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(parent._taskId, task.projectId);
          // ISSUE: explicit non-virtual call
          if (subTasksByIdAsync != null && __nonvirtual (subTasksByIdAsync.Count) > 0)
          {
            subTasksByIdAsync.Add(task);
            parent.GetToastWindow()?.BatchDeleteTask(subTasksByIdAsync);
          }
          else
            parent.GetToastWindow()?.TaskDeleted(parent._taskId);
        }
        else if (await TaskOperationHelper.CheckIfAllowDeleteAgenda(task, (DependencyObject) parent))
        {
          if (!string.IsNullOrEmpty(task.repeatFlag) && task.startDate.HasValue && task.status == 0)
          {
            ModifyRepeatHandler.TryDeleteRecurrence(task.id, task.startDate, task.dueDate, parent.GetToastWindow());
            task = (TaskModel) null;
            return;
          }
          await TaskService.DeleteAgenda(parent._taskId, task.projectId, task.attendId);
        }
        if (parent.DataContext == null)
          task = (TaskModel) null;
        else if (!(parent.DataContext is CalendarDisplayViewModel dataContext1))
        {
          task = (TaskModel) null;
        }
        else
        {
          CalendarControl calendarParent = parent.GetCalendarParent();
          if (calendarParent == null)
          {
            task = (TaskModel) null;
          }
          else
          {
            calendarParent.RemoveItem(dataContext1);
            task = (TaskModel) null;
          }
        }
      }
    }

    private async void CopyTask(object sender, EventArgs e)
    {
      TaskModel taskModel = await TaskService.CopyTask(this._taskId);
    }

    private async void SetPriority(object sender, int priority)
    {
      await TaskService.SetPriority(this._taskId, priority);
    }

    private void OnDialogClosed(object sender, EventArgs e)
    {
      CalendarDisplayViewModel model = this.GetModel();
      if (model != null)
        model.Selected = false;
      this.GetCalendarParent()?.SetEditting(false);
      PopupStateManager.OnViewPopupClosed();
    }

    private CalendarDisplayViewModel GetModel()
    {
      return this.DataContext != null && this.DataContext is CalendarDisplayViewModel dataContext ? dataContext : (CalendarDisplayViewModel) null;
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!(this.DataContext is CalendarDisplayViewModel dataContext) || !(this.Tag is bool tag))
        return;
      dataContext.SetTheme(tag);
      dataContext.SetTimeText(this.InArrange, false);
      if (this._foldPath != null)
      {
        Path foldPath = this._foldPath;
        List<CalendarDisplayViewModel> children = dataContext.Children;
        // ISSUE: explicit non-virtual call
        int num = (children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0 ? 0 : 2;
        foldPath.Visibility = (Visibility) num;
        ((RotateTransform) this._foldPath.RenderTransform).Angle = dataContext.IsOpened ? 0.0 : 90.0;
      }
      this.SetRepeat(dataContext);
      dataContext.SetSourceModel(dataContext.SourceViewModel, true);
    }

    private void SetRepeat(CalendarDisplayViewModel model)
    {
      bool isDerivative = model.IsDerivative;
      if (isDerivative && this._repeatIcon == null)
      {
        Path path = new Path();
        path.Width = 12.0;
        path.Height = 12.0;
        path.Margin = new Thickness(3.0, 0.0, 0.0, 0.0);
        path.Stretch = Stretch.Uniform;
        path.VerticalAlignment = VerticalAlignment.Center;
        this._repeatIcon = path;
        this._repeatIcon.SetBinding(Shape.FillProperty, "TitleColor");
        this._repeatIcon.Data = Utils.GetIcon("RepeatPath");
        this._repeatIcon.SetValue(DockPanel.DockProperty, (object) Dock.Left);
        this.TaskBarContainer.Children.Insert(0, (UIElement) this._repeatIcon);
      }
      else
      {
        if (isDerivative || this._repeatIcon == null)
          return;
        this.TaskBarContainer.Children.Remove((UIElement) this._repeatIcon);
        this._repeatIcon = (Path) null;
      }
    }

    private void AddFoldPath()
    {
      Border element = new Border();
      element.SetValue(DockPanel.DockProperty, (object) Dock.Right);
      element.SetResourceReference(FrameworkElement.StyleProperty, (object) "SmoothHoverBorderStyle40_60");
      element.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnOpenClick);
      this.TaskBarContainer.Children.Insert(1, (UIElement) element);
      this._foldPath = new Path();
      this._foldPath.Margin = new Thickness(2.0, 0.0, 2.0, 0.0);
      this._foldPath.SetResourceReference(Shape.FillProperty, (object) "BaseColorOpacity100");
      this._foldPath.SetResourceReference(FrameworkElement.StyleProperty, (object) "ArrowPathStyle");
      this._foldPath.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
      this._foldPath.RenderTransform = (Transform) new RotateTransform()
      {
        Angle = 0.0
      };
      element.Child = (UIElement) this._foldPath;
    }

    private async void OnOpenClick(object sender, MouseButtonEventArgs e)
    {
      TaskBar taskBar = this;
      e.Handled = true;
      taskBar._pressed = false;
      if (!(taskBar.DataContext is CalendarDisplayViewModel dataContext))
        return;
      dataContext.IsOpened = !dataContext.IsOpened;
      ((RotateTransform) taskBar._foldPath.RenderTransform).Angle = dataContext.IsOpened ? 0.0 : 90.0;
      if (dataContext.IsOpened)
        ArrangeSectionStatusDao.DeleteSectionStatusModel(new ArrangeSectionStatusModel()
        {
          Name = dataContext.TaskId,
          Type = LocalSettings.Settings.ArrangeDisplayType
        });
      else
        ArrangeSectionStatusDao.AddSectionStatusModel(new ArrangeSectionStatusModel()
        {
          Name = dataContext.TaskId,
          Type = LocalSettings.Settings.ArrangeDisplayType,
          UserId = LocalSettings.Settings.LoginUserId
        });
      CalendarControl calendarParent = taskBar.GetCalendarParent();
      if (calendarParent == null)
        return;
      await calendarParent.ArrangePanel.OnSectionStatusChanged(dataContext.IsOpened, dataContext);
    }

    public void SetTitlePosition(int column, int columnSpan, double dayWidth)
    {
      if (column < 0 && column + columnSpan > 0)
        this.TitleText.Margin = new Thickness((double) (-1 * column) * dayWidth, 0.0, 0.0, 0.0);
      else
        this.TitleText.Margin = new Thickness(0.0);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/taskbar.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
          break;
        case 2:
          this.Container = (Border) target;
          break;
        case 3:
          this.TaskBarContainer = (DockPanel) target;
          this.TaskBarContainer.MouseMove += new MouseEventHandler(this.OnMove);
          break;
        case 4:
          this.TimeText = (TextBlock) target;
          break;
        case 5:
          this.TitleText = (EmjTextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
