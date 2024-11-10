// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.FocusTimeline
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class FocusTimeline : Grid
  {
    private List<FocusTimelineViewModel> _timelines;
    private List<Border> _pauseBorder;
    private DateTime _startTime;
    private Line _currentLine;
    private Ellipse _currentEllipse;
    private double _leftOffset;
    private readonly List<TaskCell> _taskCells;
    private bool _taskLoading;
    private readonly Border _topBorder;
    private readonly PauseText _pauseText;
    private readonly Border _bottomBorder;
    private readonly TextBlock _timeText;
    private List<CalendarDisplayModel> _validTasks;

    private double HourHeight => Math.Max(60.0, (this.Height - 20.0) / 4.0);

    public FocusTimeline()
    {
      Border border1 = new Border();
      border1.CornerRadius = new CornerRadius(2.0, 2.0, 0.0, 0.0);
      border1.HorizontalAlignment = HorizontalAlignment.Left;
      border1.VerticalAlignment = VerticalAlignment.Top;
      border1.MinWidth = 10.0;
      this._topBorder = border1;
      PauseText pauseText = new PauseText();
      pauseText.VerticalAlignment = VerticalAlignment.Top;
      pauseText.HorizontalAlignment = HorizontalAlignment.Right;
      this._pauseText = pauseText;
      Border border2 = new Border();
      border2.CornerRadius = new CornerRadius(0.0, 0.0, 2.0, 2.0);
      border2.HorizontalAlignment = HorizontalAlignment.Left;
      border2.VerticalAlignment = VerticalAlignment.Top;
      border2.MinWidth = 10.0;
      border2.BorderThickness = new Thickness(1.0);
      this._bottomBorder = border2;
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 12.0;
      textBlock.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock.Opacity = 0.8;
      this._timeText = textBlock;
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this.InitTimePoint();
      this.InitCurrentTimeLine();
      this.InitCurrentFocusRect();
      this.Height = 260.0;
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
      this.Loaded += new RoutedEventHandler(this.BindEvent);
      this.Unloaded += new RoutedEventHandler(this.UnbindEvent);
    }

    private void BindEvent(object sender, RoutedEventArgs e)
    {
      DataChangedNotifier.PeriodicCheck += new EventHandler(this.NotifyPointer);
      DataChangedNotifier.IsDarkChanged += new EventHandler(this.OnIsDarkChanged);
      ticktick_WPF.Notifier.GlobalEventManager.TimeFormatChanged += new EventHandler(this.OnFormatChanged);
      DataChangedNotifier.HabitCheckInChanged += new EventHandler<HabitCheckInModel>(this.OnCheckInChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusIdChanged), "NoTask");
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this.Loaded -= new RoutedEventHandler(this.OnLoaded);
      this._timelines.ForEach((Action<FocusTimelineViewModel>) (t => t.LineWidth = this.ActualWidth - t.TimeWidth - 12.0));
      this._leftOffset = LocalSettings.Settings.TimeFormat == "24Hour" ? 48.0 : 70.0;
      this._currentLine.X1 = this._leftOffset;
      this._currentLine.X2 = Math.Max(60.0, this.ActualWidth);
      this._topBorder.Margin = new Thickness(this._leftOffset, 10.0, 0.0, 0.0);
      this.SetRectColor();
      this.DrawCurrentFocus();
    }

    private void UnbindEvent(object sender, RoutedEventArgs e)
    {
      DataChangedNotifier.PeriodicCheck -= new EventHandler(this.NotifyPointer);
      DataChangedNotifier.IsDarkChanged -= new EventHandler(this.OnIsDarkChanged);
      ticktick_WPF.Notifier.GlobalEventManager.TimeFormatChanged -= new EventHandler(this.OnFormatChanged);
      DataChangedNotifier.HabitCheckInChanged -= new EventHandler<HabitCheckInModel>(this.OnCheckInChanged);
      PropertyChangedEventManager.RemoveHandler((INotifyPropertyChanged) TickFocusManager.Config.FocusVModel, new EventHandler<PropertyChangedEventArgs>(this.OnFocusIdChanged), "NoTask");
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
    }

    private void OnCheckInChanged(object sender, HabitCheckInModel e) => this.DrawCurrentFocus();

    private void OnIsDarkChanged(object sender, EventArgs e) => this.DrawCurrentFocus();

    private void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      if ((!this.IsVisible ? 0 : (e.BatchChangedIds.Any() || e.DeletedChangedIds.Any() || e.UndoDeletedIds.Any() || e.AddIds.Any() || e.StatusChangedIds.Any() || e.KindChangedIds.Any() || e.DateChangedIds.Any() || e.CheckItemChangedIds.Any() || e.ProjectChangedIds.Any() ? 1 : (e.TagChangedIds.Any() ? 1 : 0))) == 0)
        return;
      this.DrawCurrentFocus();
    }

    private void OnFocusIdChanged(object sender, PropertyChangedEventArgs e) => this.SetRectColor();

    private void OnFormatChanged(object sender, EventArgs e)
    {
      this._leftOffset = LocalSettings.Settings.TimeFormat == "24Hour" ? 48.0 : 70.0;
      this.SetStartTime(this._startTime);
      this._timelines.ForEach((Action<FocusTimelineViewModel>) (t => t.LineWidth = this.ActualWidth - t.TimeWidth - 12.0));
      this._currentLine.X1 = this._leftOffset;
      this.DrawCurrentFocus();
    }

    public void SetSize(double width, double height)
    {
      this.Height = height;
      this._timelines.ForEach((Action<FocusTimelineViewModel>) (t => t.LineWidth = width - t.TimeWidth - 12.0));
      this._currentLine.X2 = Math.Max(40.0, width);
      this._timelines.ForEach((Action<FocusTimelineViewModel>) (t => t.SetMargin((this.Height - 100.0) / 4.0)));
      this.DrawCurrentFocus(false);
    }

    private void InitTimePoint()
    {
      ItemsControl element = new ItemsControl();
      element.SetResourceReference(ItemsControl.ItemTemplateProperty, (object) "FocusTimelineDataTemplate");
      DateTime now = DateTime.Now;
      this._startTime = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0);
      this._timelines = FocusTimelineViewModel.InitModels(this._startTime);
      element.ItemsSource = (IEnumerable) this._timelines;
      this.Children.Add((UIElement) element);
    }

    private void InitCurrentTimeLine()
    {
      Line line = new Line();
      line.X2 = 50.0;
      line.Stroke = (Brush) Brushes.Red;
      line.StrokeThickness = 1.0;
      line.VerticalAlignment = VerticalAlignment.Top;
      this._currentLine = line;
      this._currentLine.SetValue(Panel.ZIndexProperty, (object) 100);
      this.Children.Add((UIElement) this._currentLine);
      Ellipse ellipse = new Ellipse();
      ellipse.Width = 8.0;
      ellipse.Height = 8.0;
      ellipse.Fill = (Brush) Brushes.Red;
      ellipse.VerticalAlignment = VerticalAlignment.Top;
      ellipse.HorizontalAlignment = HorizontalAlignment.Left;
      this._currentEllipse = ellipse;
      this._currentEllipse.SetValue(Panel.ZIndexProperty, (object) 100);
      this.Children.Add((UIElement) this._currentEllipse);
    }

    private void SetCurrentLineOffset()
    {
      double top = (DateTime.Now - this._startTime).TotalHours * this.HourHeight + 10.0;
      this._currentLine.Margin = new Thickness(0.0, top, 0.0, 0.0);
      this._currentEllipse.Margin = new Thickness(this._leftOffset - 6.0, top - 4.0, 0.0, 0.0);
    }

    private void NotifyPointer(object sender, EventArgs e) => this.DrawCurrentFocus();

    private void InitCurrentFocusRect()
    {
      this._topBorder.SetResourceReference(Panel.BackgroundProperty, (object) "PrimaryColor80");
      this._topBorder.SetValue(Panel.ZIndexProperty, (object) 50);
      this._topBorder.Cursor = Cursors.Hand;
      this.Children.Add((UIElement) this._topBorder);
      this._bottomBorder.SetResourceReference(Panel.BackgroundProperty, (object) "PrimaryColor10");
      this._bottomBorder.SetResourceReference(Border.BorderBrushProperty, (object) "PrimaryColor30");
      this._bottomBorder.SetValue(Panel.ZIndexProperty, (object) 10);
      this.Children.Add((UIElement) this._bottomBorder);
      this._timeText.SetValue(Panel.ZIndexProperty, (object) 60);
      this._timeText.IsHitTestVisible = false;
      this.Children.Add((UIElement) this._timeText);
      this._pauseText.SetValue(Panel.ZIndexProperty, (object) 60);
      this._pauseText.Visibility = Visibility.Collapsed;
      this.Children.Add((UIElement) this._pauseText);
      this._topBorder.MouseEnter += (MouseEventHandler) (async (o, e) =>
      {
        await Task.Delay(200);
        if (!this._topBorder.IsMouseOver)
          return;
        this._pauseText.Visibility = string.IsNullOrEmpty(this._pauseText.Text) ? Visibility.Collapsed : Visibility.Visible;
      });
      this._topBorder.MouseLeave += (MouseEventHandler) ((o, e) => this._pauseText.Visibility = Visibility.Collapsed);
    }

    public async Task DrawCurrentFocus(bool reloadTasks = true)
    {
      await this.Dispatcher.Invoke<Task>((Func<Task>) (async () =>
      {
        if (!this.IsVisible)
          return;
        if (this._taskLoading)
        {
          this.SetCurrentLineOffset();
        }
        else
        {
          this._taskLoading = true;
          try
          {
            DateTime pomoStart = Utils.IsEmptyDate(TickFocusManager.Config.StartTime) || TickFocusManager.Status == PomoStatus.WaitingWork ? DateTime.Now : TickFocusManager.Config.StartTime;
            DateTime dateTime1 = pomoStart;
            DateTime dateTime2 = new DateTime(dateTime1.Year, dateTime1.Month, dateTime1.Day);
            DateTime time = dateTime2.AddHours((double) (dateTime1.Hour - 1));
            bool pomoing = TickFocusManager.IsPomo && !TickFocusManager.Config.AutoStopTime.HasValue;
            dateTime2 = DateTime.Now;
            DateTime dateTime3 = dateTime2.AddSeconds(pomoing ? TickFocusManager.Config.CurrentSeconds : 0.0);
            if ((dateTime3 - time).TotalHours > 3.0)
            {
              dateTime2 = new DateTime(dateTime3.Year, dateTime3.Month, dateTime3.Day);
              dateTime2 = dateTime2.AddHours((double) dateTime3.Hour);
              time = dateTime2.AddHours(-3.0);
            }
            if (!time.Equals(this._startTime))
              this.SetStartTime(time);
            this.SetCurrentLineOffset();
            DateTime focusDisplayStart = pomoStart > this._startTime ? pomoStart : this._startTime;
            DateTime start = focusDisplayStart;
            dateTime2 = DateTime.Now;
            DateTime end = dateTime2.AddMinutes(pomoing ? TickFocusManager.Config.CurrentSeconds / 60.0 : 0.0);
            int num1 = reloadTasks ? 1 : 0;
            TaskCellViewModel taskCellViewModel = await this.LoadTasks(start, end, num1 != 0);
            if (TickFocusManager.Status == PomoStatus.WaitingWork)
            {
              this._topBorder.Height = 0.0;
              this._bottomBorder.Height = 0.0;
              this._timeText.Text = "";
              this._pauseText.SetText((string) null);
            }
            else
            {
              TimeSpan timeSpan;
              double num2;
              if (!(pomoStart > this._startTime))
              {
                num2 = 10.0;
              }
              else
              {
                timeSpan = pomoStart - this._startTime;
                num2 = timeSpan.TotalHours * this.HourHeight + 10.0;
              }
              double top1 = num2;
              double num3 = Math.Max(2.0, taskCellViewModel.Width <= 0.0 ? this.ActualWidth - this._leftOffset : taskCellViewModel.Width - 2.0);
              this._topBorder.Margin = new Thickness(taskCellViewModel.HorizontalOffset, top1, 0.0, 0.0);
              this._topBorder.Width = num3;
              bool flag = TickFocusManager.IsPomo && TickFocusManager.Config.AutoStopTime.HasValue;
              DateTime dateTime4 = flag ? TickFocusManager.Config.AutoStopTime.Value : DateTime.Now;
              Border topBorder = this._topBorder;
              timeSpan = dateTime4 - focusDisplayStart;
              double num4 = Math.Max(0.0, timeSpan.TotalHours * this.HourHeight);
              topBorder.Height = num4;
              this._topBorder.CornerRadius = flag ? new CornerRadius(2.0) : new CornerRadius(2.0, 2.0, 0.0, 0.0);
              this._bottomBorder.Height = Math.Max(0.0, pomoing ? TickFocusManager.Config.CurrentSeconds / 3600.0 * this.HourHeight : 0.0);
              Border bottomBorder = this._bottomBorder;
              double horizontalOffset = taskCellViewModel.HorizontalOffset;
              Thickness margin1 = this._currentLine.Margin;
              double top2 = margin1.Top;
              Thickness thickness1 = new Thickness(horizontalOffset, top2, 0.0, 0.0);
              bottomBorder.Margin = thickness1;
              this._bottomBorder.Width = num3;
              this._timeText.Text = DateUtils.GetTimeText(pomoStart) + " - " + DateUtils.GetTimeText(DateTime.Now);
              if (this._topBorder.Height >= 15.0)
              {
                TextBlock timeText = this._timeText;
                margin1 = this._topBorder.Margin;
                double left = margin1.Left + 2.0;
                margin1 = this._topBorder.Margin;
                double top3 = margin1.Top + 2.0;
                Thickness thickness2 = new Thickness(left, top3, 0.0, 0.0);
                timeText.Margin = thickness2;
                this._timeText.Foreground = (Brush) Brushes.White;
                this._timeText.Opacity = 0.8;
                this._timeText.MaxWidth = Math.Max(10.0, this._topBorder.Width - 6.0);
              }
              else
                this._timeText.Opacity = 0.0;
              List<(DateTime, DateTime)> pauseSpans = TickFocusManager.Config.GetPauseSpans();
              double num5 = 0.0;
              foreach ((DateTime, DateTime) valueTuple in pauseSpans)
              {
                timeSpan = valueTuple.Item2 - valueTuple.Item1;
                double totalMinutes = timeSpan.TotalMinutes;
                num5 += totalMinutes;
              }
              if (num5 >= 1.0)
              {
                this._pauseText.SetText(Utils.GetString("Pause") + " " + Utils.GetShortDurationString((long) (num5 * 60.0)));
                PauseText pauseText = this._pauseText;
                Thickness margin2 = this._topBorder.Margin;
                double top4 = margin2.Top - 22.0;
                double actualWidth = this.ActualWidth;
                margin2 = this._topBorder.Margin;
                double left = margin2.Left;
                double right = actualWidth - left - this._topBorder.Width;
                Thickness thickness3 = new Thickness(0.0, top4, right, 0.0);
                pauseText.Margin = thickness3;
              }
              else
                this._pauseText.SetText((string) null);
            }
          }
          finally
          {
            this._taskLoading = false;
          }
        }
      }));
    }

    private void SetStartTime(DateTime time)
    {
      this._startTime = time;
      foreach (FocusTimelineViewModel timeline in this._timelines)
      {
        timeline.SetHour(time);
        time = time.AddHours(1.0);
      }
    }

    private async void SetRectColor()
    {
      string color = string.Empty;
      FocusViewModel focusVmodel = TickFocusManager.Config.FocusVModel;
      if (!string.IsNullOrEmpty(focusVmodel.FocusId))
      {
        if (!focusVmodel.IsHabit)
          color = CacheManager.GetProjectById(TaskCache.GetTaskById(focusVmodel.FocusId)?.ProjectId)?.color;
        else
          color = (await HabitDao.GetHabitById(focusVmodel.FocusId))?.Color;
      }
      if (string.IsNullOrEmpty(color) || color.ToLower() == "transparent")
      {
        this._topBorder.SetResourceReference(Panel.BackgroundProperty, (object) "PrimaryColor80");
        this._bottomBorder.SetResourceReference(Panel.BackgroundProperty, (object) "PrimaryColor10");
        this._bottomBorder.SetResourceReference(Border.BorderBrushProperty, (object) "PrimaryColor30");
      }
      else
      {
        this._topBorder.Background = (Brush) ThemeUtil.GetColorInDict(color, 80);
        this._bottomBorder.Background = (Brush) ThemeUtil.GetColorInDict(color, 10);
        this._bottomBorder.BorderBrush = (Brush) ThemeUtil.GetColorInDict(color, 30);
      }
    }

    private async Task<TaskCellViewModel> LoadTasks(DateTime start, DateTime end, bool reloadTasks)
    {
      FocusTimeline focusTimeline = this;
      TaskCellViewModel taskCellViewModel1 = new TaskCellViewModel();
      taskCellViewModel1.SourceViewModel = new TaskBaseViewModel()
      {
        StartDate = new DateTime?(start),
        DueDate = new DateTime?(end)
      };
      taskCellViewModel1.BaseOnStart = true;
      TaskCellViewModel result = taskCellViewModel1;
      if (reloadTasks)
      {
        List<CalendarDisplayModel> displayModels = await CalendarDisplayService.GetDisplayModels(focusTimeline._startTime, focusTimeline._startTime.AddHours(4.0), true);
        // ISSUE: reference to a compiler-generated method
        focusTimeline._validTasks = displayModels != null ? displayModels.Where<CalendarDisplayModel>(new Func<CalendarDisplayModel, bool>(focusTimeline.\u003CLoadTasks\u003Eb__33_0)).ToList<CalendarDisplayModel>() : (List<CalendarDisplayModel>) null;
      }
      if (focusTimeline._validTasks != null && focusTimeline._validTasks.Any<CalendarDisplayModel>())
      {
        List<TaskCellViewModel> cells = new List<TaskCellViewModel>();
        if (focusTimeline._validTasks.Count > 0)
        {
          foreach (CalendarDisplayModel validTask in focusTimeline._validTasks)
          {
            if (!(validTask.Id == TickFocusManager.Config.Id))
              cells.Add(TaskCellViewModel.Build(validTask, focusTimeline._startTime));
          }
        }
        if (TickFocusManager.Status != PomoStatus.WaitingWork)
          cells.Add(result);
        CalendarGeoHelper.AssemblyCells(cells, focusTimeline.ActualWidth - focusTimeline._leftOffset + 9.0 + 5.0, focusTimeline._startTime, new DateTime?(focusTimeline._startTime), new double?(focusTimeline.HourHeight));
        foreach (TaskCellViewModel taskCellViewModel2 in cells)
        {
          if (taskCellViewModel2 != result)
          {
            taskCellViewModel2.HorizontalOffset += focusTimeline._leftOffset;
            taskCellViewModel2.VerticalOffset += 10.0;
            taskCellViewModel2.Height -= 0.5;
            taskCellViewModel2.Width -= 0.5;
            taskCellViewModel2.ShowInFocus = true;
          }
        }
        cells.Remove(result);
        int num = Math.Max(focusTimeline._taskCells.Count, cells.Count);
        for (int index = 0; index < num; ++index)
        {
          if (index < cells.Count)
          {
            if (index < focusTimeline._taskCells.Count)
            {
              focusTimeline._taskCells[index].DataContext = (object) cells[index];
            }
            else
            {
              TaskCell element = new TaskCell(cells[index]);
              element.SetValue(Panel.ZIndexProperty, (object) 40);
              focusTimeline._taskCells.Add(element);
              focusTimeline.Children.Add((UIElement) element);
            }
          }
          else
          {
            TaskCell taskCell = focusTimeline._taskCells[cells.Count];
            focusTimeline._taskCells.Remove(taskCell);
            focusTimeline.Children.Remove((UIElement) taskCell);
          }
        }
      }
      else
      {
        foreach (TaskCell taskCell in focusTimeline._taskCells)
          focusTimeline.Children.Remove((UIElement) taskCell);
        focusTimeline._taskCells.Clear();
      }
      result.HorizontalOffset += focusTimeline._leftOffset;
      TaskCellViewModel taskCellViewModel3 = result;
      result = (TaskCellViewModel) null;
      return taskCellViewModel3;
    }
  }
}
