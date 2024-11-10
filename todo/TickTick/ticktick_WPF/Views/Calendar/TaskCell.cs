// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TaskCell
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TaskCell : Canvas, IShowTaskDetailWindow
  {
    private System.Windows.Point _cellDragStart = new System.Windows.Point(0.0, 0.0);
    private bool _draggingBottomHandle;
    private bool _draggingTopHandle;
    private IToastShowWindow _parentWindow;
    private bool _press;
    private string _taskId;
    private DateTime? _dragStartDate;
    private DateTime? _dragDueDate;
    private bool _isWaitingDoubleClick;
    private DateTime _lastClickTime;
    private System.Windows.Point _firstClickPoint;
    private bool _detailShowing;
    private Path _icon;
    private EmjTextBlock _text;
    private Path _repeatIcon;
    private TextBlock _timeText;
    private Border _dragBorder;

    public TaskCell(TaskCellViewModel model)
    {
      this.DataContext = (object) model;
      this.SetEvent();
      this.VerticalAlignment = VerticalAlignment.Top;
      this.HorizontalAlignment = HorizontalAlignment.Left;
      this.Cursor = Cursors.Hand;
      this.UseLayoutRounding = true;
      this.Background = (Brush) Brushes.Transparent;
      this.SetBinding(FrameworkElement.WidthProperty, "Width");
      this.SetBinding(FrameworkElement.HeightProperty, "Height");
      this.SetBinding(FrameworkElement.MarginProperty, "Margin");
      this.ClipToBounds = true;
      model?.SetTaskCell(this);
      model?.SetSourceModel(model.SourceViewModel, true);
      this.SetChildren(model);
    }

    private async void SetChildren(TaskCellViewModel model)
    {
      TaskCell taskCell = this;
      if (model == null)
      {
        taskCell.Visibility = Visibility.Collapsed;
      }
      else
      {
        model.SetTheme((bool) taskCell.FindResource((object) "IsDarkTheme"));
        model.SetTimeText(false, true);
        taskCell.SetIcon(model);
        taskCell.SetTitle(model);
        await Task.Delay(10);
        taskCell.UpdateLayout();
        bool showTime = model.Height >= 30.0 || !model.ShowIcon && string.IsNullOrEmpty(model.Title);
        taskCell.SetRepeat(model, showTime);
        taskCell.SetTimeText(model, showTime);
        taskCell.SetDragHandler(!model.ShowInFocus && model.ShowDragHandle);
      }
    }

    private void SetDragHandler(bool showDrag)
    {
      if (showDrag && this._dragBorder == null)
      {
        Border border = new Border();
        border.Cursor = Cursors.SizeNS;
        this._dragBorder = border;
        this._dragBorder.SetBinding(FrameworkElement.WidthProperty, "Width");
        this._dragBorder.SetBinding(FrameworkElement.HeightProperty, "Height");
        this._dragBorder.BorderThickness = new Thickness(0.0, 3.0, 0.0, 3.0);
        this._dragBorder.BorderBrush = (Brush) Brushes.Transparent;
        this._dragBorder.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnHandleMouseDown);
        this.Children.Add((UIElement) this._dragBorder);
      }
      else
      {
        if (showDrag || this._dragBorder == null)
          return;
        this.Children.Remove((UIElement) this._dragBorder);
        this._dragBorder = (Border) null;
      }
    }

    private void SetRepeat(TaskCellViewModel model, bool showTime)
    {
      bool flag = model.IsDerivative & showTime && model.Width >= 11.0;
      if (flag && this._repeatIcon == null)
      {
        Path path = new Path();
        path.Width = 12.0;
        path.Height = 12.0;
        path.Stretch = Stretch.Uniform;
        this._repeatIcon = path;
        this._repeatIcon.SetBinding(Shape.FillProperty, "TitleColor");
        this._repeatIcon.Data = Utils.GetIcon("RepeatPath");
        this.Children.Add((UIElement) this._repeatIcon);
      }
      else if (!flag && this._repeatIcon != null)
      {
        this.Children.Remove((UIElement) this._repeatIcon);
        this._repeatIcon = (Path) null;
      }
      if (this._repeatIcon == null)
        return;
      Canvas.SetLeft((UIElement) this._repeatIcon, model.Width > 16.0 ? 4.0 : (model.Width >= 11.0 ? model.Width - 12.0 : 0.0));
      double length = model.Height > 22.0 ? 3.0 : (model.Height - 12.0) / 2.0;
      if (this._icon != null || !string.IsNullOrEmpty(model.Title))
        length += Math.Min(this._text.ActualHeight, this._text.MaxHeight - 2.0);
      Canvas.SetTop((UIElement) this._repeatIcon, length);
    }

    private void SetTitle(TaskCellViewModel model)
    {
      if (this._text == null)
      {
        EmjTextBlock emjTextBlock = new EmjTextBlock();
        emjTextBlock.FontSize = 12.0;
        emjTextBlock.LineHeight = 14.0;
        emjTextBlock.VerticalAlignment = VerticalAlignment.Top;
        emjTextBlock.IsHitTestVisible = false;
        this._text = emjTextBlock;
        this._text.SetBinding(TextBlock.ForegroundProperty, "TitleColor");
        this._text.TextWrapping = TextWrapping.Wrap;
        this.Children.Add((UIElement) this._text);
      }
      this._text.FontSize = model.Height <= 14.0 ? 10.0 : 12.0;
      int length = Math.Max(4, Math.Max(1, (int) (model.Height / 14.0)) * (int) model.Width / 3);
      string str = TaskUtils.EscapeLinkContent(model.Title) ?? string.Empty;
      if (str.Length > length)
        str = str.Substring(0, length);
      this._text.Text = str;
      this._text.Width = Math.Max(0.0, model.Width - (model.ShowIcon ? 24.0 : 8.0));
      Canvas.SetLeft((UIElement) this._text, model.ShowIcon ? 20.0 : 4.0);
      Canvas.SetTop((UIElement) this._text, model.Height > 18.0 ? 2.0 : -1.0);
      double num = (model.Height - 16.0) / 15.0;
      if (model.Height >= 48.0 && !string.IsNullOrEmpty(model.Course?.Room))
        --num;
      this._text.MaxHeight = Math.Max(model.Height > 16.0 ? 16.0 : model.Height + 2.0, (double) ((int) num * 15 + 2));
    }

    private void SetIcon(TaskCellViewModel model)
    {
      bool flag = model.ShowIcon && model.Width >= 11.0 && model.Height > 15.0;
      if (flag && this._icon == null)
      {
        Path path = new Path();
        path.Width = 12.0;
        path.Height = 12.0;
        path.Stretch = Stretch.Uniform;
        this._icon = path;
        this._icon.SetBinding(Path.DataProperty, "Icon");
        this._icon.SetBinding(Shape.FillProperty, "TitleColor");
        this.Children.Add((UIElement) this._icon);
      }
      else if (!flag && this._icon != null)
      {
        this.Children.Remove((UIElement) this._icon);
        this._icon = (Path) null;
      }
      if (this._icon == null)
        return;
      Canvas.SetLeft((UIElement) this._icon, model.Width > 16.0 ? 4.0 : (model.Width >= 11.0 ? model.Width - 12.0 : 0.0));
      Canvas.SetTop((UIElement) this._icon, model.Height > 22.0 ? 4.0 : (model.Height - 12.0) / 2.0);
    }

    private void SetTimeText(TaskCellViewModel model, bool showTime)
    {
      if (showTime)
        model.SetTimeText(false, true);
      if (showTime && this._timeText == null)
      {
        TextBlock textBlock = new TextBlock();
        textBlock.FontSize = 11.0;
        textBlock.LineHeight = 14.0;
        textBlock.IsHitTestVisible = false;
        this._timeText = textBlock;
        this._timeText.SetBinding(TextBlock.ForegroundProperty, "TitleColor");
        this._timeText.SetBinding(TextBlock.TextProperty, "TimeText");
        this._timeText.SetResourceReference(FrameworkElement.MarginProperty, (object) "TaskCellTimeMargin");
        this.Children.Add((UIElement) this._timeText);
      }
      else if (!showTime && this._timeText != null)
      {
        this.Children.Remove((UIElement) this._timeText);
        this._timeText = (TextBlock) null;
      }
      if (this._timeText == null)
        return;
      Canvas.SetLeft((UIElement) this._timeText, model.IsDerivative ? 20.0 : 4.0);
      double length = model.Height > 20.0 ? 3.0 : (model.Height - 16.0) / 2.0;
      if (this._icon != null || !string.IsNullOrEmpty(model.Title))
        length += Math.Min(this._text.ActualHeight, this._text.MaxHeight - 2.0);
      if (model.Height >= 48.0 && !string.IsNullOrEmpty(model.Course?.Room))
      {
        TaskCellViewModel taskCellViewModel = model;
        taskCellViewModel.TimeText = taskCellViewModel.TimeText + "\n" + model.Course.Room;
      }
      Canvas.SetTop((UIElement) this._timeText, length);
      this._timeText.Width = Math.Max(0.0, model.Width - (model.IsDerivative ? 24.0 : 8.0));
    }

    private void SetEvent()
    {
      this.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCellClick);
      this.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnCellMouseDown);
      this.MouseRightButtonUp += new MouseButtonEventHandler(this.OnShowOperation);
      this.DataContextChanged += new DependencyPropertyChangedEventHandler(this.OnDataBind);
      this.Unloaded += (RoutedEventHandler) ((o, e) => this.DataContext = (object) null);
    }

    private void OnDataBind(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is TaskCellViewModel oldValue)
        oldValue.SetTaskCell((TaskCell) null);
      if (!(e.NewValue is TaskCellViewModel newValue))
        return;
      newValue.SetTaskCell(this);
      this.Opacity = 1.0;
      this.SetChildren(newValue);
      newValue.SetSourceModel(newValue.SourceViewModel, true);
    }

    public void OnTaskPropertyChanged(string propertyName)
    {
      if (propertyName == null)
        return;
      switch (propertyName.Length)
      {
        case 4:
          if (!(propertyName == "Icon"))
            return;
          break;
        case 5:
          switch (propertyName[0])
          {
            case 'T':
              if (!(propertyName == "Title"))
                return;
              break;
            case 'W':
              if (!(propertyName == "Width"))
                return;
              break;
            default:
              return;
          }
          break;
        case 6:
          if (!(propertyName == "Height"))
            return;
          break;
        case 7:
          return;
        case 8:
          if (!(propertyName == "Dragging"))
            return;
          this.Opacity = (this.DataContext is TaskCellViewModel dataContext ? (dataContext.Dragging ? 1 : 0) : 0) != 0 ? 0.5 : 1.0;
          return;
        case 9:
          if (!(propertyName == "BackColor"))
            return;
          goto label_23;
        case 10:
          return;
        case 11:
          return;
        case 12:
          if (!(propertyName == "IsDerivative"))
            return;
          break;
        case 13:
          return;
        case 14:
          return;
        case 15:
          if (!(propertyName == "BackBorderColor"))
            return;
          goto label_23;
        case 19:
          if (!(propertyName == "BackBorderThickness"))
            return;
          goto label_23;
        default:
          return;
      }
      this.SetChildren(this.DataContext as TaskCellViewModel);
      return;
label_23:
      this.InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
      base.OnRender(dc);
      if (!(this.DataContext is TaskCellViewModel dataContext) || dataContext.Width <= 1.0 || dataContext.Height <= 1.0)
        return;
      dc.DrawRoundedRectangle((Brush) dataContext.BackColor, new Pen((Brush) dataContext.BackBorderColor, dataContext.BackBorderThickness.Top), new Rect(0.0, 0.0, dataContext.Width - 1.0, dataContext.Height - 1.0), 3.0, 3.0);
    }

    private void InitEvents()
    {
      Window parentWindow = Utils.GetParentWindow((DependencyObject) this);
      if (parentWindow == null)
        return;
      parentWindow.CaptureMouse();
      parentWindow.PreviewMouseMove -= new MouseEventHandler(this.OnWeekDayCellMove);
      parentWindow.PreviewMouseMove += new MouseEventHandler(this.OnWeekDayCellMove);
      parentWindow.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(this.StopDraggingHandle);
      parentWindow.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.StopDraggingHandle);
    }

    private async void StopDraggingHandle(object sender, MouseButtonEventArgs e)
    {
      TaskCell taskCell = this;
      if (!(sender is Window window))
        return;
      taskCell.GetDragCell()?.SetCellHandling(false);
      window.ReleaseMouseCapture();
      window.PreviewMouseMove -= new MouseEventHandler(taskCell.OnWeekDayCellMove);
      window.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(taskCell.StopDraggingHandle);
      await taskCell.OnHandleDrop(taskCell._dragStartDate, taskCell._dragDueDate, taskCell._draggingBottomHandle);
      taskCell._press = false;
      taskCell._draggingBottomHandle = false;
      taskCell._draggingTopHandle = false;
      taskCell._dragDueDate = new DateTime?();
      taskCell._dragStartDate = new DateTime?();
      Mouse.OverrideCursor = (Cursor) null;
    }

    private void OnWeekDayCellMove(object sender, MouseEventArgs e)
    {
      CalendarControl calendarParent = this.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0)
        return;
      this.TryMoveHandle(e);
    }

    private void OnCellMouseDown(object sender, MouseButtonEventArgs e)
    {
      CalendarControl calendarParent = this.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0)
        return;
      this.MouseMove -= new MouseEventHandler(this.OnCellMove);
      this.MouseMove += new MouseEventHandler(this.OnCellMove);
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      e.Handled = true;
      this._cellDragStart = e.GetPosition((IInputElement) this);
      UtilLog.Info("TCDS:" + this._cellDragStart.X.ToString() + "," + this._cellDragStart.Y.ToString());
      this._press = true;
    }

    private void OnHandleMouseDown(object sender, MouseButtonEventArgs e)
    {
      CalendarControl calendarParent = this.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0 || e.LeftButton != MouseButtonState.Pressed || !(this.DataContext is TaskCellViewModel dataContext))
        return;
      e.Handled = true;
      if (!(sender is Border relativeTo))
        return;
      System.Windows.Point position = e.GetPosition((IInputElement) relativeTo);
      this._draggingBottomHandle = position.Y > 4.0;
      this._draggingTopHandle = position.Y <= 4.0;
      this._dragStartDate = dataContext.DisplayStartDate;
      this._dragDueDate = dataContext.DisplayDueDate;
      dataContext.SourceViewModel = dataContext.SourceViewModel.Copy();
      this.InitEvents();
      this.GetDragCell()?.SetCellHandling(true);
    }

    private void TryHandleCellDrop(MouseButtonEventArgs e)
    {
    }

    private async void TryMoveHandle(MouseEventArgs e)
    {
      TaskCell relativeTo = this;
      if (e.LeftButton != MouseButtonState.Pressed || !relativeTo._draggingBottomHandle && !relativeTo._draggingTopHandle || relativeTo.DataContext == null || !(relativeTo.DataContext is TaskCellViewModel dataContext) || !string.IsNullOrEmpty(dataContext.AttendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) dataContext))
        return;
      double roundOffset = CalendarGeoHelper.GetRoundOffset(e.GetPosition((IInputElement) relativeTo).Y + CalendarGeoHelper.GetTopFoldDiff());
      if (!dataContext.DisplayStartDate.HasValue)
        return;
      if (relativeTo._draggingBottomHandle)
      {
        DateTime? nullable = dataContext.BaseDate;
        DateTime dateTime1 = nullable ?? dataContext.DisplayStartDate.Value;
        DateTime dateTime2 = CalendarGeoHelper.TranslateVerticalOffset(dateTime1.Date, roundOffset + dataContext.VerticalOffset);
        Mouse.OverrideCursor = Cursors.SizeNS;
        DateTime dateTime3 = dateTime2;
        nullable = dataContext.DisplayStartDate;
        DateTime dateTime4 = nullable.Value.AddMinutes(15.0);
        if (dateTime3 <= dateTime4)
        {
          nullable = dataContext.DisplayStartDate;
          dateTime2 = nullable.Value.AddMinutes(15.0);
        }
        dataContext.SourceViewModel.DueDate = new DateTime?(dateTime2.AddDays(-1.0 * dataContext.RepeatDiff));
        CalendarGeoHelper.ResetCellHeightAndVertical(dataContext, new DateTime?(dateTime1));
      }
      else
      {
        DateTime? nullable = dataContext.DisplayDueDate;
        if (!nullable.HasValue)
          dataContext.SourceViewModel.DueDate = dataContext.StartDate;
        nullable = dataContext.DisplayStartDate;
        if (nullable.HasValue)
        {
          nullable = dataContext.DisplayDueDate;
          if (nullable.HasValue)
          {
            nullable = relativeTo._dragStartDate;
            DateTime dateTime5 = CalendarGeoHelper.TranslateVerticalOffsetToStart((nullable ?? dataContext.DisplayStartDate).Value, dataContext.VerticalOffset + roundOffset);
            int num = LocalSettings.Settings.CalendarHourHeight / 3.0 >= CalendarGeoHelper.MinHeight ? -15 : -30;
            DateTime dateTime6 = dateTime5;
            nullable = dataContext.DisplayDueDate;
            DateTime dateTime7 = nullable.Value.AddMinutes((double) num);
            if (dateTime6 >= dateTime7)
            {
              nullable = dataContext.DisplayDueDate;
              dateTime5 = nullable.Value.AddMinutes((double) num);
            }
            Mouse.OverrideCursor = Cursors.SizeNS;
            dataContext.SourceViewModel.StartDate = new DateTime?(dateTime5.AddDays(-1.0 * dataContext.RepeatDiff));
            CalendarGeoHelper.ResetCellHeightAndVertical(dataContext, relativeTo._dragStartDate);
          }
        }
      }
      dataContext.SetTimeText(false, true);
    }

    private void OnCellMove(object sender, MouseEventArgs e)
    {
      e.Handled = true;
      if (e.LeftButton != MouseButtonState.Pressed || !this._press)
        return;
      if (this.DataContext == null)
        return;
      TaskCellViewModel model = this.DataContext as TaskCellViewModel;
      if (model == null || model.NewAdd || model.ShowInFocus)
        return;
      if (model.CanDrag)
        this.TryDragCell(e);
      else if (model.IsCourse)
      {
        this.GetToastWindow()?.TryToastString((object) null, Utils.GetString("OperationNotSupport"));
      }
      else
      {
        this.TryToastUnableString(CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.ProjectId)));
        this.MouseMove -= new MouseEventHandler(this.OnCellMove);
      }
    }

    private async void TryDragCell(MouseEventArgs e)
    {
      TaskCell taskCell = this;
      if (!Utils.CheckMoveDelta((FrameworkElement) taskCell, e, taskCell._cellDragStart, 2))
        return;
      if (taskCell.DataContext != null && taskCell.DataContext is CalendarDisplayViewModel dataContext)
      {
        bool canDrag;
        bool needToast;
        string toastStr;
        CalendarEventDragHelper.CheckIfEventCanDrag(dataContext, out canDrag, out needToast, out toastStr);
        if (canDrag)
        {
          dataContext.Dragging = true;
          taskCell.GetDragCell()?.OnDragStart((TaskCellViewModel) taskCell.DataContext, e, e.GetPosition((IInputElement) taskCell).Y);
        }
        else if (needToast)
          taskCell.GetToastWindow()?.TryToastString((object) null, toastStr);
        taskCell._press = false;
      }
      // ISSUE: explicit non-virtual call
      taskCell.MouseMove -= new MouseEventHandler(taskCell.OnCellMove);
    }

    private IDragCellEvent GetDragCell()
    {
      return Utils.FindParent<IDragCellEvent>((DependencyObject) this);
    }

    private async Task OnHandleDrop(DateTime? originStart, DateTime? originDue, bool dragBottom)
    {
      TaskCell taskCell = this;
      if (taskCell.DataContext == null)
        model = (TaskCellViewModel) null;
      else if (!(taskCell.DataContext is TaskCellViewModel model))
      {
        model = (TaskCellViewModel) null;
      }
      else
      {
        switch (model.Type)
        {
          case DisplayType.Task:
          case DisplayType.Derivative:
            DateTime? nullable1 = model.DisplayStartDate;
            DateTime? nullable2 = originStart;
            if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
            {
              nullable2 = model.DisplayDueDate;
              nullable1 = originDue;
              if ((nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() == nullable1.GetValueOrDefault() ? 1 : 0) : 1) : 0) != 0)
              {
                model.SetSourceModel(TaskCache.GetTaskById(model.SourceViewModel.Id));
                model = (TaskCellViewModel) null;
                return;
              }
            }
            if (!string.IsNullOrEmpty(model.RepeatFlag))
            {
              TaskModel task = await TaskDao.GetThinTaskById(model.TaskId);
              if (task != null)
              {
                nullable1 = model.StartDate;
                if (nullable1.HasValue)
                {
                  nullable1 = task.startDate;
                  if (nullable1.HasValue)
                  {
                    List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task.id);
                    TimeData reviseData = new TimeData();
                    reviseData.StartDate = model.DisplayStartDate;
                    reviseData.DueDate = model.DisplayDueDate;
                    nullable1 = model.DisplayStartDate;
                    reviseData.IsAllDay = new bool?(!nullable1.HasValue || ((int) model.IsAllDay ?? 1) != 0);
                    reviseData.Reminders = remindersByTaskId;
                    reviseData.RepeatFrom = model.RepeatFrom;
                    reviseData.RepeatFlag = model.RepeatFlag;
                    reviseData.ExDates = task.exDate != null ? ((IEnumerable<string>) task.exDate).ToList<string>() : new List<string>();
                    reviseData.TimeZone = new TimeZoneViewModel(task.isFloating.GetValueOrDefault(), task.timeZone);
                    ModifyRepeatHandler.TryUpdateDueDate(task.id, originStart, originDue, reviseData, 1, 0);
                  }
                }
              }
              task = (TaskModel) null;
            }
            else
            {
              nullable1 = model.DisplayStartDate;
              if (nullable1.HasValue)
              {
                nullable1 = model.DisplayDueDate;
                if (nullable1.HasValue)
                {
                  string taskId = model.TaskId;
                  nullable1 = model.DisplayStartDate;
                  DateTime startDate = nullable1.Value;
                  nullable1 = model.DisplayDueDate;
                  DateTime? dueDate = new DateTime?(nullable1.Value);
                  int num = dragBottom ? 1 : 0;
                  await TaskService.SetTaskDateAfterDropHandle(taskId, startDate, dueDate, num != 0);
                }
              }
            }
            model.SetSourceModel(TaskCache.GetTaskById(model.SourceViewModel.Id));
            break;
          case DisplayType.Event:
            if (model.StartDate.HasValue && model.DueDate.HasValue)
            {
              string eventId = model.EventId;
              DateTime? nullable3 = model.StartDate;
              DateTime startDate = nullable3.Value;
              nullable3 = model.DueDate;
              DateTime endDate = nullable3.Value;
              int num = dragBottom ? 1 : 0;
              await CalendarService.SetEventDate(eventId, startDate, endDate, num != 0);
              break;
            }
            break;
        }
        SyncManager.TryDelaySync();
        model = (TaskCellViewModel) null;
      }
    }

    private async void OnCellClick(object sender, MouseButtonEventArgs e)
    {
      TaskCell taskCell = this;
      CalendarControl calendarParent = taskCell.GetCalendarParent();
      TaskCellViewModel model;
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0)
      {
        model = (TaskCellViewModel) null;
      }
      else
      {
        taskCell.TryHandleCellDrop(e);
        // ISSUE: explicit non-virtual call
        taskCell.MouseMove -= new MouseEventHandler(taskCell.OnCellMove);
        if (!PopupStateManager.CanShowDragPopup())
          model = (TaskCellViewModel) null;
        else if (taskCell._draggingBottomHandle)
          model = (TaskCellViewModel) null;
        else if (taskCell._draggingTopHandle)
          model = (TaskCellViewModel) null;
        else if (PopupStateManager.IsInAdd())
        {
          model = (TaskCellViewModel) null;
        }
        else
        {
          e.Handled = true;
          if (taskCell._detailShowing)
          {
            taskCell._detailShowing = false;
            model = (TaskCellViewModel) null;
          }
          else
          {
            model = taskCell.DataContext as TaskCellViewModel;
            if (taskCell.DataContext == null)
              model = (TaskCellViewModel) null;
            else if (model == null)
            {
              model = (TaskCellViewModel) null;
            }
            else
            {
              if (model.IsCalendarEvent)
                await taskCell.ShowEventDetail((MouseEventArgs) e, model);
              else if (model.IsHabit)
                taskCell.ShowHabitDetail((MouseEventArgs) e, (CalendarDisplayViewModel) model);
              else if (model.IsPomo)
                taskCell.ShowPomoDetail(model);
              else if (model.Course != null)
                taskCell.ShowCourseDetail(model);
              else if (model.ShowInFocus)
              {
                model.Selected = true;
                taskCell._firstClickPoint = PopupLocationCalculator.GetMousePoint(false);
                if (await taskCell.ShowTaskDetailPopup((CalendarDisplayViewModel) model, true) == null)
                {
                  model = (TaskCellViewModel) null;
                  return;
                }
              }
              else
                taskCell.TryShowTaskDetail(model);
              if (!model.ShowInFocus)
              {
                model = (TaskCellViewModel) null;
              }
              else
              {
                UserActCollectUtils.AddClickEvent("focus", TickFocusManager.GetActCType(), "click_calendar_dialog");
                model = (TaskCellViewModel) null;
              }
            }
          }
        }
      }
    }

    private async void ShowCourseDetail(TaskCellViewModel model)
    {
      TaskCell target = this;
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
          this.GetCalendarParent()?.SetEditting(false);
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

    private async void ShowPomoDetail(TaskCellViewModel model)
    {
      TaskCell target = this;
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
        PomoDetailWindow pomoDetailWindow = new PomoDetailWindow(new PomoDisplayViewModel(pomo, pomo.Tasks), (CalendarDisplayViewModel) model);
        pomoDetailWindow.Closed += (EventHandler) ((obj, e) =>
        {
          model.Selected = false;
          this.GetCalendarParent()?.SetEditting(false);
        });
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

    private async void TryShowTaskDetail(TaskCellViewModel model)
    {
      this._isWaitingDoubleClick = false;
      bool firstClick = (DateTime.Now - this._lastClickTime).TotalMilliseconds > 300.0;
      this._lastClickTime = DateTime.Now;
      if (firstClick)
      {
        this._isWaitingDoubleClick = true;
        model.Selected = true;
        this._firstClickPoint = PopupLocationCalculator.GetMousePoint(false);
        TaskDetailPopup window = await this.ShowTaskDetailPopup((CalendarDisplayViewModel) model, true);
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
      TaskCell target = this;
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

    private void OnCheckInAction(object sender, EventArgs e) => this.HandleOnWindowClosed();

    private async Task ShowEventDetail(MouseEventArgs e, TaskCellViewModel calendarDisplayViewModel)
    {
      TaskCell target = this;
      string eventId = calendarDisplayViewModel.EventId;
      if (string.IsNullOrEmpty(eventId))
        return;
      CalendarEventModel eventById = await CalendarEventDao.GetEventById(ArchivedDao.GetOriginalId(eventId));
      if (eventById == null)
        return;
      if (target.DataContext != null && target.DataContext is TaskCellViewModel dataContext)
      {
        eventById.DueStart = dataContext.DisplayStartDate;
        eventById.DueEnd = dataContext.DisplayDueDate;
        dataContext.Selected = true;
      }
      CalendarDetailWindow calendarDetailWindow = new CalendarDetailWindow(target.GetWidgetThemeId());
      calendarDetailWindow.Disappear += new EventHandler<string>(target.OnDetailClosed);
      calendarDetailWindow.EventArchiveChanged += new EventHandler<bool>(target.OnEventArchiveChanged);
      calendarDetailWindow.Show((UIElement) target, target.ActualWidth, 0.0, target.ActualWidth > 350.0, new CalendarDetailViewModel(eventById));
      target.GetCalendarParent()?.SetEditting(true);
    }

    private void OnEventArchiveChanged(object sender, bool archive)
    {
      TaskCellViewModel model = this.GetModel();
      if (model == null)
        return;
      model.SourceViewModel.Status = archive ? 2 : 0;
    }

    private string GetWidgetThemeId()
    {
      CalendarWidget parent = Utils.FindParent<CalendarWidget>((DependencyObject) this);
      return parent != null ? parent.ThemeId : string.Empty;
    }

    private async Task<TaskDetailPopup> ShowTaskDetailPopup(
      CalendarDisplayViewModel model,
      bool show)
    {
      TaskCell target = this;
      if (model == null)
        return (TaskDetailPopup) null;
      TaskDetailViewModel taskDetailViewModel = await TaskDetailViewModel.Build(model.GetTaskId());
      if (taskDetailViewModel == null)
        return (TaskDetailPopup) null;
      taskDetailViewModel.RepeatDiff = string.IsNullOrEmpty(model.RepeatFlag) ? new double?() : new double?(model.RepeatDiff);
      TaskDetailPopup taskDetailPopup1 = new TaskDetailPopup();
      IToastShowWindow toastWindow = target.GetToastWindow();
      if (toastWindow != null)
      {
        if (PopupStateManager.LastTarget == target)
          return (TaskDetailPopup) null;
        taskDetailPopup1.DependentWindow = toastWindow;
      }
      model.Selected = true;
      taskDetailPopup1.Disappear -= new EventHandler<string>(target.OnDetailClosed);
      taskDetailPopup1.Disappear += new EventHandler<string>(target.OnDetailClosed);
      TaskDetailPopup taskDetailPopup2 = taskDetailPopup1;
      TaskDetailViewModel model1 = taskDetailViewModel;
      string itemId = model.ItemId;
      TaskWindowDisplayArgs args = new TaskWindowDisplayArgs((UIElement) target, target.ActualWidth, target.ActualWidth >= 350.0 || target.ActualHeight >= 350.0 ? target._firstClickPoint : new System.Windows.Point(), 0.0);
      bool flag = show;
      System.Windows.Point point = new System.Windows.Point();
      int num = flag ? 1 : 0;
      taskDetailPopup2.Show(model1, itemId, args, point, num != 0);
      target.GetCalendarParent()?.SetEditting(true);
      return taskDetailPopup1;
    }

    private void OnDetailClosed(object sender, string e)
    {
      this.HandleOnWindowClosed();
      if (!(sender is TaskDetailWindow taskDetailWindow))
        return;
      taskDetailWindow.Disappear -= new EventHandler<string>(this.OnDetailClosed);
    }

    private void HandleOnWindowClosed()
    {
      if (this.DataContext != null && this.DataContext is TaskCellViewModel dataContext)
        dataContext.Selected = false;
      this.GetCalendarParent()?.SetEditting(false);
    }

    private CalendarControl GetCalendarParent()
    {
      return Utils.FindParent<CalendarControl>((DependencyObject) this);
    }

    private bool TryToastUnableString(ProjectModel model)
    {
      if (model == null || string.IsNullOrEmpty(model.permission))
        return false;
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
        default:
          return false;
      }
    }

    private async void OnShowOperation(object sender, MouseButtonEventArgs e)
    {
      TaskCell taskCell = this;
      if (taskCell.DataContext == null)
        return;
      TaskCellViewModel model = taskCell.DataContext as TaskCellViewModel;
      if (model == null || model.NewAdd || model.ShowInFocus)
        return;
      if (model.IsHabit)
        taskCell.ShowHabitOperation((CalendarDisplayViewModel) model);
      else if (model.Type == DisplayType.Event)
        taskCell.ShowEventOperation(model);
      else if (model.Type == DisplayType.Course)
      {
        taskCell.ShowCourseOperation(model);
      }
      else
      {
        ProjectModel model1 = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == model.ProjectId));
        if (taskCell.TryToastUnableString(model1))
          model.Selected = false;
        else
          taskCell.ShowTaskOperation(model);
      }
    }

    private async Task ShowTaskOperation(TaskCellViewModel model)
    {
      TaskCell element = this;
      element._taskId = model.Type == DisplayType.Task || model.Type == DisplayType.Derivative || model.Type == DisplayType.Note ? model.TaskId : (string) null;
      if (string.IsNullOrEmpty(element._taskId))
        ;
      else
      {
        element.GetToastWindow();
        OperationExtra taskAccessInfo = await TaskOperationHelper.GetTaskAccessInfo(element._taskId, false);
        if (taskAccessInfo?.TimeModel == null)
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
          dialog.LinkCopied += new EventHandler(element.CopyTaskLink);
          dialog.Copied += new EventHandler(element.CopyTask);
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
            await this.ClearDate();
            dialog.Dismiss();
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
          PopupStateManager.OnViewPopupOpened();
          CalendarControl calendarParent = element.GetCalendarParent();
          if (calendarParent == null)
            ;
          else
            calendarParent.SetEditting(true);
        }
      }
    }

    private void ShowCourseOperation(TaskCellViewModel model)
    {
      DateTime? startDate = model.StartDate;
      DateTime today = DateTime.Today;
      if ((startDate.HasValue ? (startDate.GetValueOrDefault() < today ? 1 : 0) : 0) != 0)
        return;
      model.Selected = true;
      ArchiveOperationDialog archiveOperationDialog = new ArchiveOperationDialog(model.Course.UniqueId, ArchiveKind.Course, model.Course.Archived);
      archiveOperationDialog.SetPlaceTarget((UIElement) this);
      archiveOperationDialog.Closed += new EventHandler(this.OnDialogClosed);
      this.GetCalendarParent()?.SetEditting(true);
      archiveOperationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) ((arg, obj) =>
      {
        model.Course.Archived = obj.Value == ActionType.Archive;
        model.SourceViewModel.Status = model.Course.Archived || model.Course.CourseStart.Date < DateTime.Today ? 2 : 0;
      });
      archiveOperationDialog.Show();
    }

    private void ShowEventOperation(TaskCellViewModel model)
    {
      DateTime? nullable;
      if (model.DueDate.HasValue)
      {
        nullable = model.DueDate;
        if (nullable.Value <= DateTime.Today)
          return;
      }
      nullable = model.DueDate;
      if (!nullable.HasValue)
      {
        nullable = model.StartDate;
        if (nullable.HasValue)
        {
          nullable = model.StartDate;
          if (nullable.Value < DateTime.Today)
            return;
        }
      }
      model.Selected = true;
      CalendarOperationDialog calendarOperationDialog = new CalendarOperationDialog(new EventArchiveArgs(model.SourceViewModel), model.Status != 0);
      calendarOperationDialog.SetPlaceTarget((UIElement) this);
      this.GetCalendarParent()?.SetEditting(true);
      calendarOperationDialog.Closed += new EventHandler(this.OnDialogClosed);
      calendarOperationDialog.Operated += (EventHandler<KeyValuePair<string, ActionType>>) ((arg, obj) => model.SourceViewModel.Status = obj.Value == ActionType.Archive ? 2 : 0);
      calendarOperationDialog.Show();
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

    private void OnTaskOperationToast(object sender, string e)
    {
      this.GetToastWindow()?.TryToastString((object) null, e);
    }

    private async void OnAbandonOrReopenTask(object sender, EventArgs e)
    {
      TaskCell taskCell = this;
      if (taskCell.DataContext == null)
        model = (CalendarDisplayViewModel) null;
      else if (!(taskCell.DataContext is CalendarDisplayViewModel model))
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
            if (!await ModifyRepeatHandler.CompleteOrSkipRecurrence(model.TaskId, model.DisplayStartDate, taskCell.GetToastWindow()))
            {
              model = (CalendarDisplayViewModel) null;
              return;
            }
          }
        }
        int closeStatus = model.IsAbandoned ? 0 : -1;
        TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(model.TaskId, closeStatus, closeStatus != 0);
        model = (CalendarDisplayViewModel) null;
      }
    }

    private async void OnSwitchTaskOrNoteClick(object sender, EventArgs e)
    {
      TaskCell taskCell = this;
      if (taskCell.DataContext == null || !(taskCell.DataContext is CalendarDisplayViewModel dataContext))
        return;
      await TaskService.SwitchTaskOrNote(dataContext.TaskId);
      SyncManager.TryDelaySync();
    }

    private async Task ClearDate()
    {
      TaskCell parent = this;
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
      TaskCellViewModel model = this.GetModel();
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
      (string str1, string str2) = e.GetProjectAndColumnId();
      string taskId = this._taskId;
      string projectId = str1;
      string str3 = str2;
      bool? isTop = new bool?();
      string columnId = str3;
      await TaskService.MoveProject(taskId, projectId, isTop, columnId);
      SyncManager.TryDelaySync();
    }

    private async void DeleteTask(object sender, EventArgs e)
    {
      TaskCell parent = this;
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
          if (!string.IsNullOrEmpty(task.repeatFlag) && task.startDate.HasValue && task.status == 0)
          {
            if (parent.DataContext == null)
            {
              task = (TaskModel) null;
              return;
            }
            if (!(parent.DataContext is CalendarDisplayViewModel dataContext))
            {
              task = (TaskModel) null;
              return;
            }
            if (!(dataContext.TaskId == task.id))
            {
              task = (TaskModel) null;
              return;
            }
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
          SyncManager.TryDelaySync();
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
      SyncManager.TryDelaySync();
    }

    private async void CopyTaskLink(object sender, EventArgs e)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(this._taskId);
      if (thinTaskById == null)
        return;
      TaskUtils.CopyTaskLink(thinTaskById.id, thinTaskById.projectId, thinTaskById.title);
    }

    private async void SetPriority(object sender, int priority)
    {
      await TaskService.SetPriority(this._taskId, priority);
      SyncManager.TryDelaySync();
    }

    private void OnDialogClosed(object sender, EventArgs e)
    {
      TaskCellViewModel model = this.GetModel();
      if (model != null)
        model.Selected = false;
      this.GetCalendarParent()?.SetEditting(false);
      PopupStateManager.OnViewPopupClosed(false);
    }

    private TaskCellViewModel GetModel()
    {
      return this.DataContext != null && this.DataContext is TaskCellViewModel dataContext ? dataContext : (TaskCellViewModel) null;
    }

    private IToastShowWindow GetToastWindow()
    {
      this._parentWindow = this._parentWindow ?? Utils.FindParent<IToastShowWindow>((DependencyObject) this);
      return this._parentWindow;
    }
  }
}
