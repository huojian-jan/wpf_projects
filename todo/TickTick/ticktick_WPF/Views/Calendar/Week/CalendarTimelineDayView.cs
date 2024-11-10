// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Week.CalendarTimelineDayView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Week
{
  public class CalendarTimelineDayView : Grid
  {
    private bool _addPopupShowed;
    private bool _createCellShowed;
    private bool _doubleClick;
    private double _draggingStartY = -1.0;
    private bool _isDuration;
    private bool _mouseClick;
    private bool _loaded;
    private IToastShowWindow _parentWindow;
    private Grid _timePointer;
    private BlockingList<TaskCell> _cells = new BlockingList<TaskCell>();
    private TaskCell _addTaskCell;
    public double Offset;
    private TextBlock _topExtra1;
    private TextBlock _topExtra2;
    private TextBlock _botExtra1;
    private TextBlock _botExtra2;

    public CalendarTimelineDayView()
    {
      this.Background = (Brush) Brushes.Transparent;
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      this.SetPointerPosition();
      this.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnLeftButtonDown);
    }

    private void UnbindEvents()
    {
      DataChangedNotifier.PeriodicCheck -= new EventHandler(this.NotifyPointer);
      CalendarGeoHelper.CalendarHourHeightChanged -= new EventHandler<double>(this.OnCalendarHeightChanged);
    }

    private void BindEvents()
    {
      DataChangedNotifier.PeriodicCheck += new EventHandler(this.NotifyPointer);
      CalendarGeoHelper.CalendarHourHeightChanged += new EventHandler<double>(this.OnCalendarHeightChanged);
    }

    private void OnCalendarHeightChanged(object sender, double height)
    {
      this.ReloadCells(this.ActualWidth);
      this.NotifyPointer(sender, (EventArgs) null);
    }

    private void NotifyPointer(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.SetPointerPosition));
    }

    public void Reload(double? width = null)
    {
      ref double? local = ref width;
      double? nullable = width;
      double num1 = nullable ?? this.ActualWidth;
      local = new double?(num1);
      nullable = width;
      double num2 = 0.0;
      if (nullable.GetValueOrDefault() > num2 & nullable.HasValue)
        this.SetupCells(width.Value);
      this.SetPointerPosition();
    }

    public void SetData()
    {
      if (this.DataContext is CalendarTimelineDayViewModel dataContext)
        this.SetupData(dataContext.Cells);
      this.SetPointerPosition();
    }

    private async void SetPointerPosition()
    {
      CalendarTimelineDayView calendarTimelineDayView = this;
      if (!(calendarTimelineDayView.DataContext is CalendarTimelineDayViewModel dataContext))
        return;
      calendarTimelineDayView.SetTimePointer(dataContext.IsToday);
      List<TaskCellViewModel> list1 = dataContext.Cells.Where<TaskCellViewModel>((Func<TaskCellViewModel, bool>) (c => c.IsHabit)).ToList<TaskCellViewModel>();
      bool reloadCell = false;
      calendarTimelineDayView.SetTopBotTask(dataContext);
      foreach (TaskCellViewModel taskCellViewModel in list1)
      {
        TaskCellViewModel cell = taskCellViewModel;
        DateTime? startDate = cell.StartDate;
        DateTime dateTime1 = DateTime.Now;
        if ((startDate.HasValue ? (startDate.GetValueOrDefault() >= dateTime1 ? 1 : 0) : 0) == 0)
        {
          startDate = cell.StartDate;
          dateTime1 = DateTime.Now.AddMinutes(-5.0);
          if ((startDate.HasValue ? (startDate.GetValueOrDefault() < dateTime1 ? 1 : 0) : 0) == 0)
          {
            HabitModel habitById = await HabitDao.GetHabitById(cell.HabitId);
            string[] reminders = habitById.Reminders;
            if ((reminders != null ? (reminders.Length != 0 ? 1 : 0) : 0) != 0)
            {
              List<string> list2 = ((IEnumerable<string>) habitById.Reminders).ToList<string>();
              list2.Sort();
              string str = list2.FirstOrDefault<string>((Func<string, bool>) (r => string.Compare(r, DateTime.Now.ToString("HH:mm"), StringComparison.Ordinal) >= 0)) ?? list2.LastOrDefault<string>();
              int result1;
              int result2;
              if (str != null && int.TryParse(str.Substring(0, 2), out result1) && int.TryParse(str.Substring(3), out result2))
              {
                dateTime1 = DateTime.Today;
                dateTime1 = dateTime1.AddHours((double) result1);
                DateTime dateTime2 = dateTime1.AddMinutes((double) result2);
                startDate = cell.StartDate;
                dateTime1 = dateTime2;
                if ((startDate.HasValue ? (startDate.HasValue ? (startDate.GetValueOrDefault() != dateTime1 ? 1 : 0) : 0) : 1) != 0)
                {
                  cell.SourceViewModel.StartDate = new DateTime?(dateTime2);
                  reloadCell = true;
                }
              }
            }
            cell = (TaskCellViewModel) null;
          }
        }
      }
      if (!reloadCell)
        return;
      calendarTimelineDayView.ReloadCells(calendarTimelineDayView.ActualWidth);
    }

    private void SetTopBotTask(CalendarTimelineDayViewModel model)
    {
      if (!string.IsNullOrEmpty(model.Extra1))
      {
        if (this._topExtra1 == null)
        {
          EmjTextBlock emjTextBlock = new EmjTextBlock();
          emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
          emjTextBlock.FontSize = 12.0;
          emjTextBlock.VerticalAlignment = VerticalAlignment.Top;
          emjTextBlock.Margin = new Thickness(4.0, 0.0, 4.0, 0.0);
          this._topExtra1 = (TextBlock) emjTextBlock;
          this._topExtra1.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity80");
          this.Children.Add((UIElement) this._topExtra1);
        }
        this._topExtra1.Text = model.Extra1;
      }
      else if (this._topExtra1 != null)
        this._topExtra1.Text = string.Empty;
      if (!string.IsNullOrEmpty(model.Extra2))
      {
        if (this._topExtra2 == null)
        {
          EmjTextBlock emjTextBlock = new EmjTextBlock();
          emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
          emjTextBlock.FontSize = 12.0;
          emjTextBlock.VerticalAlignment = VerticalAlignment.Top;
          emjTextBlock.Margin = new Thickness(4.0, 16.0, 4.0, 0.0);
          this._topExtra2 = (TextBlock) emjTextBlock;
          this._topExtra2.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity80");
          this.Children.Add((UIElement) this._topExtra2);
        }
        this._topExtra2.Text = model.Extra2;
      }
      else if (this._topExtra2 != null)
        this._topExtra2.Text = string.Empty;
      if (!string.IsNullOrEmpty(model.Extra3))
      {
        if (this._botExtra1 == null)
        {
          EmjTextBlock emjTextBlock = new EmjTextBlock();
          emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
          emjTextBlock.FontSize = 12.0;
          emjTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
          emjTextBlock.Margin = new Thickness(4.0, 0.0, 4.0, 16.0);
          this._botExtra1 = (TextBlock) emjTextBlock;
          this._botExtra1.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity80");
          this.Children.Add((UIElement) this._botExtra1);
        }
        this._botExtra1.Text = model.Extra3;
      }
      else if (this._botExtra1 != null)
        this._botExtra1.Text = string.Empty;
      if (!string.IsNullOrEmpty(model.Extra4))
      {
        if (this._botExtra2 == null)
        {
          EmjTextBlock emjTextBlock = new EmjTextBlock();
          emjTextBlock.TextTrimming = TextTrimming.CharacterEllipsis;
          emjTextBlock.FontSize = 12.0;
          emjTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
          emjTextBlock.Margin = new Thickness(4.0, 0.0, 4.0, 0.0);
          this._botExtra2 = (TextBlock) emjTextBlock;
          this._botExtra2.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity80");
          this.Children.Add((UIElement) this._botExtra2);
        }
        this._botExtra2.Text = model.Extra4;
      }
      else
      {
        if (this._botExtra2 == null)
          return;
        this._botExtra2.Text = string.Empty;
      }
    }

    private void SetTimePointer(bool show)
    {
      int num = show ? CalendarGeoHelper.GetPointOffset() : -1;
      show = show && num >= 32;
      if (show)
      {
        if (this._timePointer == null)
        {
          Grid grid = new Grid();
          grid.Height = 9.0;
          grid.IsHitTestVisible = false;
          grid.VerticalAlignment = VerticalAlignment.Top;
          this._timePointer = grid;
          this._timePointer.SetValue(Panel.ZIndexProperty, (object) 100);
          SolidColorBrush colorInString = ThemeUtil.GetColorInString("#ED6655");
          UIElementCollection children1 = this._timePointer.Children;
          Line element1 = new Line();
          element1.X1 = 0.0;
          element1.X2 = 1.0;
          element1.Stretch = Stretch.Fill;
          element1.StrokeThickness = 1.0;
          element1.Stroke = (Brush) colorInString;
          element1.VerticalAlignment = VerticalAlignment.Top;
          element1.Margin = new Thickness(2.0, 4.0, 0.0, 0.0);
          children1.Add((UIElement) element1);
          UIElementCollection children2 = this._timePointer.Children;
          Ellipse element2 = new Ellipse();
          element2.Height = 9.0;
          element2.Width = 9.0;
          element2.StrokeThickness = 1.0;
          element2.Fill = (Brush) colorInString;
          element2.VerticalAlignment = VerticalAlignment.Center;
          element2.HorizontalAlignment = HorizontalAlignment.Left;
          children2.Add((UIElement) element2);
          this.Children.Add((UIElement) this._timePointer);
        }
        if (num < 0)
          return;
        this._timePointer.Margin = new Thickness(0.0, (double) (num - 6), 0.0, 0.0);
      }
      else
      {
        this.Children.Remove((UIElement) this._timePointer);
        this._timePointer = (Grid) null;
      }
    }

    private void SetupCells(double width) => this.ReloadCells(width);

    private void ReloadCells(double width)
    {
      if (!(this.DataContext is CalendarTimelineDayViewModel dataContext) || width <= 0.0)
        return;
      CalendarGeoHelper.AssemblyCells(dataContext.Cells, dataContext.InWeekControl ? width : width - 10.0, dataContext.Date);
      this.SetupData(dataContext.Cells);
    }

    private void ShowAddTaskPopup()
    {
      if (this.Tag == null || !(this.Tag is TaskCellViewModel tag))
        return;
      DateTime? nullable1 = tag.StartDate;
      DateTime dateTime;
      if (nullable1.HasValue && CalendarGeoHelper.TopFolded)
      {
        nullable1 = tag.StartDate;
        dateTime = nullable1.Value;
        int hour = dateTime.Hour;
        if (hour <= CalendarGeoHelper.GetStartHourForTask() || hour >= CalendarGeoHelper.GetEndHour())
          return;
      }
      nullable1 = tag.DueDate;
      if (!nullable1.HasValue)
      {
        nullable1 = tag.StartDate;
        if (nullable1.HasValue)
        {
          TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
          if (defaultSafely.DateMode == 1 && defaultSafely.Duration > 0)
          {
            TaskBaseViewModel sourceViewModel = tag.SourceViewModel;
            nullable1 = tag.StartDate;
            dateTime = nullable1.Value;
            DateTime? nullable2 = new DateTime?(dateTime.AddMinutes((double) defaultSafely.Duration));
            sourceViewModel.DueDate = nullable2;
            TaskCellViewModel taskCellViewModel = tag;
            nullable1 = tag.DueDate;
            DateTime? startDate = tag.StartDate;
            double num = (nullable1.HasValue & startDate.HasValue ? new TimeSpan?(nullable1.GetValueOrDefault() - startDate.GetValueOrDefault()) : new TimeSpan?()).Value.TotalMinutes / CalendarGeoHelper.MinMinute * CalendarGeoHelper.MinHeight;
            taskCellViewModel.Height = num;
          }
        }
      }
      this.ShowAddCell(tag);
      TaskDetailViewModel model = TaskDetailViewModel.BuildInitModel(tag.SourceViewModel, false);
      TaskDetailPopup taskDetailPopup = new TaskDetailPopup();
      IToastShowWindow parentWindow = this.GetParentWindow();
      UIElement target = (UIElement) ((Panel) this._addTaskCell ?? (Panel) this);
      if (parentWindow != null)
      {
        if (PopupStateManager.LastTarget == target)
        {
          this.RemoveAddCell();
          return;
        }
        taskDetailPopup.DependentWindow = parentWindow;
      }
      taskDetailPopup.Disappear -= new EventHandler<string>(this.OnAddPopupClosed);
      taskDetailPopup.Disappear += new EventHandler<string>(this.OnAddPopupClosed);
      taskDetailPopup.TaskSaved += new EventHandler<string>(this.OnTaskAdded);
      taskDetailPopup.Show(model, string.Empty, new TaskWindowDisplayArgs(target, this.ActualWidth - 6.0, PopupLocationCalculator.GetMousePoint(this.ActualWidth <= 400.0), false, this.ActualWidth > 400.0 ? 0.0 : this._draggingStartY, 0));
      this._mouseClick = false;
      this.GetCalendarParent()?.SetEditting(true);
      this._addPopupShowed = true;
      taskDetailPopup.FocusTitle();
    }

    private async void OnTaskAdded(object sender, string taskId)
    {
      CalendarTimelineDayView calendarTimelineDayView = this;
      if (sender is TaskDetailWindow taskDetailWindow)
        taskDetailWindow.TaskSaved -= new EventHandler<string>(calendarTimelineDayView.OnTaskAdded);
      string e = await CalendarUtils.CheckAddTaskCanShown(taskId);
      if (string.IsNullOrEmpty(e))
        return;
      calendarTimelineDayView.GetParentWindow()?.TryToastString((object) null, e);
    }

    private IToastShowWindow GetParentWindow()
    {
      this._parentWindow = this._parentWindow ?? Utils.FindParent<IToastShowWindow>((DependencyObject) this);
      return this._parentWindow;
    }

    public void OnLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
      CalendarControl calendarParent = this.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0)
        return;
      if (sender is Window window)
      {
        window.ReleaseMouseCapture();
        window.PreviewMouseMove -= new MouseEventHandler(this.OnMouseMove);
        window.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(this.OnLeftButtonUp);
      }
      if (this._doubleClick || !this._mouseClick || !PopupStateManager.CanShowAddPopup())
        return;
      this.ShowAddTaskPopup();
    }

    private void OnLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
      CalendarControl calendarParent = this.GetCalendarParent();
      if ((calendarParent != null ? (calendarParent.IsLocked ? 1 : 0) : 0) != 0 || !PopupStateManager.CanShowAddPopup())
        return;
      this._draggingStartY = e.GetPosition((IInputElement) this).Y;
      if (this.DataContext == null || !(this.DataContext is CalendarTimelineDayViewModel dataContext))
        return;
      string defaultAddProjectId = CalendarUtils.GetCalendarDefaultAddProjectId();
      TaskCellViewModel model = CalendarGeoHelper.BuildAddTaskModel(dataContext.Date, e.GetPosition((IInputElement) this).Y);
      model.SourceViewModel.ProjectId = defaultAddProjectId;
      model.GetColor((bool) this.FindResource((object) "IsDarkTheme"));
      model.Width = this.ActualWidth - 9.0;
      this.Tag = (object) model;
      this._isDuration = false;
      this._mouseClick = true;
      this.ShowAddCell(model);
      Window parentWindow = Utils.GetParentWindow((DependencyObject) this);
      if (parentWindow == null)
        return;
      parentWindow.CaptureMouse();
      parentWindow.PreviewMouseMove -= new MouseEventHandler(this.OnMouseMove);
      parentWindow.PreviewMouseMove += new MouseEventHandler(this.OnMouseMove);
      parentWindow.PreviewMouseLeftButtonUp -= new MouseButtonEventHandler(this.OnLeftButtonUp);
      parentWindow.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnLeftButtonUp);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || !this._mouseClick || this._addPopupShowed || this.Tag == null || !(this.Tag is TaskCellViewModel tag))
        return;
      DateTime? startDate = tag.StartDate;
      if (startDate.HasValue)
      {
        startDate = tag.StartDate;
        if (startDate.Value.Hour < CalendarGeoHelper.GetStartHour())
          return;
      }
      double currentY = e.GetPosition((IInputElement) this).Y + CalendarGeoHelper.GetTopFoldDiff();
      if (!this._isDuration && currentY - this._draggingStartY > 2.0)
        this._isDuration = true;
      if (this._isDuration)
      {
        double roundOffset = CalendarGeoHelper.GetRoundOffset(currentY);
        startDate = tag.StartDate;
        DateTime dateTime1 = CalendarGeoHelper.TranslateVerticalOffset(startDate.GetValueOrDefault(), roundOffset);
        startDate = tag.StartDate;
        if (startDate.HasValue)
        {
          DateTime dateTime2 = dateTime1;
          startDate = tag.StartDate;
          DateTime dateTime3 = startDate.Value;
          DateTime dateTime4 = dateTime3.AddMinutes(15.0);
          if (dateTime2 <= dateTime4)
          {
            startDate = tag.StartDate;
            dateTime3 = startDate.Value;
            dateTime1 = dateTime3.AddMinutes(15.0);
          }
          tag.SourceViewModel.DueDate = new DateTime?(dateTime1);
          TaskCellViewModel taskCellViewModel = tag;
          dateTime3 = dateTime1;
          startDate = tag.StartDate;
          double num = (startDate.HasValue ? new TimeSpan?(dateTime3 - startDate.GetValueOrDefault()) : new TimeSpan?()).Value.TotalMinutes / 15.0 * CalendarGeoHelper.QuarterHourHeight;
          taskCellViewModel.Height = num;
        }
      }
      this.ShowAddCell(tag);
    }

    private async void OnAddPopupClosed(object sender, string e)
    {
      CalendarTimelineDayView calendarTimelineDayView = this;
      if (calendarTimelineDayView.DataContext is CalendarTimelineDayViewModel dataContext)
        dataContext.State = MonthCellState.Normal;
      if (sender is TaskDetailWindow taskDetailWindow)
        taskDetailWindow.Disappear -= new EventHandler<string>(calendarTimelineDayView.OnAddPopupClosed);
      calendarTimelineDayView.GetCalendarParent()?.SetEditting(false);
      calendarTimelineDayView.RemoveCreateCell();
      calendarTimelineDayView._addPopupShowed = false;
    }

    private CalendarControl GetCalendarParent()
    {
      return Utils.FindParent<CalendarControl>((DependencyObject) this);
    }

    private void RemoveCreateCell() => this.RemoveAddCell();

    public void SetupData(List<TaskCellViewModel> data)
    {
      int count = data.Count - this._cells.Count;
      if (count > 0)
        this.BatchInitCells(count);
      for (int index = 0; index < this._cells.Count; ++index)
      {
        if (index < data.Count)
        {
          TaskCellViewModel taskCellViewModel = data[index];
          TaskCell cell = this._cells[index];
          if (cell != null)
          {
            if (taskCellViewModel.StartDate.HasValue)
            {
              cell.DataContext = (object) taskCellViewModel;
              if (cell.Visibility != Visibility.Visible)
                cell.Visibility = Visibility.Visible;
            }
            else if (cell.Visibility != Visibility.Collapsed)
              cell.Visibility = Visibility.Collapsed;
          }
        }
        else
        {
          TaskCell cell = this._cells[index];
          if (cell != null && cell.Visibility == Visibility.Visible)
            cell.Visibility = Visibility.Collapsed;
        }
      }
      this.RemoveAddCell();
    }

    private void BatchInitCells(int count)
    {
      for (int index = 0; index < count; ++index)
      {
        TaskCell taskCell1 = new TaskCell((TaskCellViewModel) null);
        taskCell1.Visibility = Visibility.Collapsed;
        TaskCell taskCell2 = taskCell1;
        this._cells.Add(taskCell2);
        this.Children.Add((UIElement) taskCell2);
      }
    }

    public void ShowAddCell(TaskCellViewModel model)
    {
      if (model == null)
        return;
      if (this._addTaskCell == null)
      {
        this._addTaskCell = new TaskCell(model);
        this.Children.Add((UIElement) this._addTaskCell);
      }
      else
        this._addTaskCell.DataContext = (object) model;
    }

    public void RemoveAddCell()
    {
      if (this._addTaskCell == null)
        return;
      this.Children.Remove((UIElement) this._addTaskCell);
      this._addTaskCell = (TaskCell) null;
    }

    public void ClearItems(bool onlyData)
    {
      if (onlyData)
      {
        if (this._cells.Count > 5)
        {
          List<TaskCell> taskCellList = new List<TaskCell>();
          for (int index = 4; index < this._cells.Count; ++index)
            taskCellList.Add(this._cells[index]);
          foreach (TaskCell taskCell in taskCellList)
          {
            this._cells.Remove(taskCell);
            this.Children.Remove((UIElement) taskCell);
          }
        }
        foreach (TaskCell taskCell in this._cells.Value)
        {
          taskCell.DataContext = (object) null;
          taskCell.Visibility = Visibility.Collapsed;
        }
      }
      else
      {
        foreach (UIElement element in this._cells.Value)
          this.Children.Remove(element);
        this._cells.Clear();
      }
    }

    public DateTime GetDate()
    {
      return !(this.DataContext is CalendarTimelineDayViewModel dataContext) ? new DateTime() : dataContext.Date;
    }
  }
}
