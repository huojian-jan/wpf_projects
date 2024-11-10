// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Week.CalendarTimelineView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Week
{
  public class CalendarTimelineView : ScrollViewer
  {
    private const int EdgeOffset = 24;
    private const int EdgeScrollStep = 36;
    private List<CalendarTimelineDayView> _dayViews;
    private readonly Timer _startScrollUpTimer;
    private readonly Storyboard _scrollStoryboard;
    private MultiDayView _parent;
    private readonly Border _topFoldBorder;
    private readonly Border _botFoldBorder;
    private readonly Grid _foldTooltipGrid;
    private readonly Grid _container;
    private readonly Grid _daysCanvas;
    private List<Line> _hLines;
    private TimeLine _timeLine;
    private double _currentOffset;
    private bool _scrollingUp;
    private bool _scrollingDown;
    private bool _scrolling;
    private List<CalendarTimelineDayViewModel> _data;
    private bool _foldBorderMouseDown;
    private Grid _timePointer;
    private TextBlock _currentTime;
    private StackPanel _currentTimeStack;

    public event EventHandler<int> MoveNextOrLast;

    public CalendarTimelineView(MultiDayView parent)
    {
      Border border1 = new Border();
      border1.Height = 32.0;
      border1.Background = (Brush) Brushes.Transparent;
      border1.Cursor = Cursors.Hand;
      border1.VerticalAlignment = VerticalAlignment.Top;
      this._topFoldBorder = border1;
      Border border2 = new Border();
      border2.Height = 32.0;
      border2.Background = (Brush) Brushes.Transparent;
      border2.Cursor = Cursors.Hand;
      border2.VerticalAlignment = VerticalAlignment.Bottom;
      this._botFoldBorder = border2;
      Grid grid1 = new Grid();
      grid1.VerticalAlignment = VerticalAlignment.Top;
      grid1.HorizontalAlignment = HorizontalAlignment.Left;
      this._foldTooltipGrid = grid1;
      this._container = new Grid();
      Grid grid2 = new Grid();
      grid2.Margin = new Thickness(61.0, 0.0, 1.0, 0.0);
      grid2.ClipToBounds = true;
      this._daysCanvas = grid2;
      this._hLines = new List<Line>();
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this._parent = parent;
      this.SetResourceReference(FrameworkElement.StyleProperty, (object) "for_scrollviewer");
      this.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
      this.Content = (object) this._container;
      TimeLine timeLine = new TimeLine();
      timeLine.HorizontalAlignment = HorizontalAlignment.Left;
      timeLine.Width = 60.0;
      this._timeLine = timeLine;
      this._container.Children.Add((UIElement) this._timeLine);
      this.PreviewMouseWheel += new MouseWheelEventHandler(this.OnPreviewMouseWheel);
      this.InitDaysCanvas();
      this.SetLines();
      CalendarGeoHelper.TopFoldedChanged += new EventHandler(this.OnTopFoldedChanged);
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      Grid grid3 = new Grid();
      grid3.Height = 19.0;
      grid3.IsHitTestVisible = false;
      grid3.VerticalAlignment = VerticalAlignment.Top;
      this._timePointer = grid3;
      this.SetPointerPosition();
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      this.InitFoldBorder();
    }

    private void BindEvents()
    {
      CalendarGeoHelper.CalendarHourHeightChanged += new EventHandler<double>(this.OnCalendarHeightChanged);
      CalendarGeoHelper.TopFoldedChanged += new EventHandler(this.OnTopFoldedChanged);
      CalendarGeoHelper.ExpandTop += new EventHandler<bool>(this.OnExpandTop);
      DataChangedNotifier.PeriodicCheck += new EventHandler(this.NotifyPointer);
      if (!CalendarGeoHelper.TopFolded)
        return;
      this._botFoldBorder.Height = Math.Max(32.0, this.ActualHeight - (CalendarGeoHelper.HourHeight * (double) (24 - CalendarGeoHelper.GetCollapsedHours()) + (CalendarGeoHelper.TopFolded ? 32.0 : 0.0)));
    }

    private void UnbindEvents()
    {
      CalendarGeoHelper.CalendarHourHeightChanged -= new EventHandler<double>(this.OnCalendarHeightChanged);
      CalendarGeoHelper.TopFoldedChanged -= new EventHandler(this.OnTopFoldedChanged);
      CalendarGeoHelper.ExpandTop -= new EventHandler<bool>(this.OnExpandTop);
      DataChangedNotifier.PeriodicCheck -= new EventHandler(this.NotifyPointer);
    }

    private void NotifyPointer(object sender, EventArgs e) => this.SetPointer();

    public void SetPointer()
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.SetPointerPosition));
    }

    private void OnExpandTop(object sender, bool e)
    {
      if (e)
        this.ScrollToHome();
      else
        this.ScrollToEnd();
    }

    public bool ShowPointer()
    {
      return this._parent.GetTimeSpan().Item1 <= DateTime.Today && this._parent.GetTimeSpan().Item2 > DateTime.Today;
    }

    private void OnTopFoldedChanged(object sender, EventArgs e)
    {
      this._topFoldBorder.Visibility = CalendarGeoHelper.TopFolded ? Visibility.Visible : Visibility.Collapsed;
      this._botFoldBorder.Visibility = CalendarGeoHelper.TopFolded ? Visibility.Visible : Visibility.Collapsed;
      this.Changed();
      this.SetPointer();
    }

    public void Changed()
    {
      this.InitBoard();
      this.SetLines();
      if (!this.IsVisible)
        return;
      this._parent?.ReloadPointEvent();
    }

    private void OnCalendarHeightChanged(object sender, double e)
    {
      int num = CalendarGeoHelper.TopFolded ? 32 : 0;
      for (int index = 0; index < this._hLines.Count; ++index)
        this._hLines[index].Margin = new Thickness(60.0, (double) num + CalendarGeoHelper.HourHeight * (double) (index + (!CalendarGeoHelper.TopFolded ? 1 : 0)), 0.0, 0.0);
      this.SetPointer();
    }

    private void InitDaysCanvas() => this._container.Children.Add((UIElement) this._daysCanvas);

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      if (Utils.IfCtrlPressed())
      {
        ZoomHelper.Zoom((double) e.Delta, this.ActualHeight, (ScrollViewer) this);
        e.Handled = true;
      }
      else
      {
        if (!PopupStateManager.CanShowAddPopup())
          e.Handled = true;
        if (!Utils.IfShiftPressed())
          return;
        this._parent?.OnShiftMouseWheel(e.Delta);
        e.Handled = true;
      }
    }

    public void SetDisplayDays() => this.SetDayViews();

    private void SetDayViews()
    {
      MultiDayView parent1 = this._parent;
      int num1 = (parent1 != null ? parent1.DisplayDays : 7) + 2;
      MultiDayView parent2 = this._parent;
      double num2 = parent2 != null ? parent2.DayViewWidth : 0.0;
      if (num1 > this._dayViews.Count)
      {
        int count = this._dayViews.Count;
        for (int index = 0; index < num1 - count; ++index)
        {
          CalendarTimelineDayView calendarTimelineDayView = new CalendarTimelineDayView();
          calendarTimelineDayView.RenderTransform = (Transform) new TranslateTransform();
          calendarTimelineDayView.Width = num2;
          calendarTimelineDayView.HorizontalAlignment = HorizontalAlignment.Left;
          CalendarTimelineDayView element = calendarTimelineDayView;
          this._dayViews.Add(element);
          this._daysCanvas.Children.Add((UIElement) element);
        }
      }
      else
      {
        List<CalendarTimelineDayView> calendarTimelineDayViewList = new List<CalendarTimelineDayView>();
        for (int index = num1; index < this._dayViews.Count; ++index)
          calendarTimelineDayViewList.Add(this._dayViews[index]);
        calendarTimelineDayViewList.ForEach((Action<CalendarTimelineDayView>) (d =>
        {
          this._dayViews.Remove(d);
          this._daysCanvas.Children.Remove((UIElement) d);
        }));
      }
      for (int index = 0; index < this._dayViews.Count; ++index)
      {
        CalendarTimelineDayView dayView = this._dayViews[index];
        dayView.RenderTransform = (Transform) new TranslateTransform()
        {
          X = ((double) (index - 1) * num2)
        };
        dayView.Width = num2;
      }
    }

    private void SetLines()
    {
      int timeBlockCount = CalendarGeoHelper.GetTimeBlockCount();
      int num = CalendarGeoHelper.TopFolded ? 32 : 0;
      if (timeBlockCount > this._hLines.Count)
      {
        int count = this._hLines.Count;
        for (int index = 0; index < timeBlockCount - count; ++index)
        {
          Line line = new Line();
          line.X1 = 0.0;
          line.X2 = 1.0;
          line.Stretch = Stretch.Fill;
          line.VerticalAlignment = VerticalAlignment.Top;
          line.StrokeThickness = 1.0;
          line.IsHitTestVisible = false;
          Line element = line;
          element.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
          this._hLines.Add(element);
          this._container.Children.Add((UIElement) element);
        }
      }
      else
      {
        List<Line> lineList = new List<Line>();
        for (int index = timeBlockCount; index < this._hLines.Count; ++index)
          lineList.Add(this._hLines[index]);
        lineList.ForEach((Action<Line>) (l =>
        {
          this._hLines.Remove(l);
          this._container.Children.Remove((UIElement) l);
        }));
      }
      for (int index = 0; index < this._hLines.Count; ++index)
        this._hLines[index].Margin = new Thickness(60.0, (double) num + CalendarGeoHelper.HourHeight * (double) (index + (!CalendarGeoHelper.TopFolded ? 1 : 0)), 0.0, 0.0);
    }

    public void Setup(List<CalendarTimelineDayViewModel> datas, double width, bool showWeekend)
    {
      DateTime dateTime = this._parent.GetTimeSpan().Item1;
      this._data = datas;
      MultiDayView parent = this._parent;
      double num = parent != null ? parent.DayViewWidth : 0.0;
      for (int index = 0; index < this._dayViews.Count; ++index)
      {
        if (this._dayViews[index].RenderTransform is TranslateTransform renderTransform)
          renderTransform.X = (double) (index - 1) * num;
      }
      DateTime date = dateTime.AddDays(-1.0);
      if (!showWeekend)
      {
        while (TickTickUtils.DateUtils.IsWeekends(date))
          date = date.AddDays(-1.0);
      }
      for (int index = 0; index < this._dayViews.Count && index < datas.Count; ++index)
      {
        if (index > 0)
          date = date.AddDays(1.0);
        CalendarTimelineDayView dayView = this._dayViews[index];
        if (!showWeekend)
        {
          while (TickTickUtils.DateUtils.IsWeekends(date))
            date = date.AddDays(1.0);
        }
        dayView.RenderTransform = (Transform) new TranslateTransform()
        {
          X = ((double) (index - 1) * num)
        };
        CalendarTimelineDayViewModel timelineDayViewModel = datas.FirstOrDefault<CalendarTimelineDayViewModel>((Func<CalendarTimelineDayViewModel, bool>) (d => d.Date == date));
        if (timelineDayViewModel != null)
        {
          dayView.DataContext = (object) timelineDayViewModel;
          dayView.Reload(new double?(width));
        }
        else
          dayView.DataContext = (object) new CalendarTimelineDayViewModel()
          {
            Date = date
          };
      }
    }

    private bool CheckColumnVisible(int columnIndex)
    {
      if (LocalSettings.Settings.ShowCalWeekend)
        return true;
      switch (LocalSettings.Settings.WeekStartFrom)
      {
        case "Sunday":
          return columnIndex != 0 && columnIndex != 6;
        case "Saturday":
          return columnIndex != 0 && columnIndex != 1;
        case "Monday":
          return columnIndex != 5 && columnIndex != 6;
        default:
          return true;
      }
    }

    public void ResetTimeLine() => this.InitBoard();

    public void NotifyEventTitleChanged(string eventId, string text)
    {
      foreach (CalendarTimelineDayView dayView in this._dayViews)
      {
        if (dayView != null && dayView.DataContext is CalendarTimelineDayViewModel dataContext)
          dataContext.Cells.Where<TaskCellViewModel>((Func<TaskCellViewModel, bool>) (i => i.EventId == eventId)).ToList<TaskCellViewModel>().ForEach((Action<TaskCellViewModel>) (item => item.SourceViewModel.Title = text));
      }
    }

    public void TryScrollOnEdge(MouseEventArgs e)
    {
      double y = e.GetPosition((IInputElement) this).Y;
      if (y > this._currentOffset)
      {
        if (this._scrollingUp)
          this.StopScrollUp();
      }
      else if (y < this._currentOffset && this._scrollingDown)
        this.StopScrollDown();
      if (this._currentOffset >= 0.0 && this._currentOffset <= 24.0)
      {
        if (y < this._currentOffset)
          this.TryStartScrollUp();
      }
      else
        this.StopScrollUp();
      if (this._currentOffset >= this.ActualHeight - 24.0 && this._currentOffset <= this.ActualHeight)
      {
        if (y > this._currentOffset)
          this.StartScrollDown();
      }
      else
        this.StopScrollDown();
      this._currentOffset = y;
    }

    private void TryStartScrollUp()
    {
      this._scrollingUp = true;
      this._startScrollUpTimer.Start();
      this._startScrollUpTimer.Elapsed -= new ElapsedEventHandler(this.OnStartScrollElapsed);
      this._startScrollUpTimer.Elapsed += new ElapsedEventHandler(this.OnStartScrollElapsed);
    }

    private void OnStartScrollElapsed(object sender, ElapsedEventArgs e)
    {
      this._startScrollUpTimer.Elapsed -= new ElapsedEventHandler(this.OnStartScrollElapsed);
      this._startScrollUpTimer.Stop();
      if (!this._scrollingUp)
        return;
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.StartScrollUp));
    }

    private void StartScrollUp()
    {
      this._scrollingUp = true;
      this.Scroll(true, new EventHandler(this.ScrollUpCompletedHandler));
    }

    public void ScrollToCurrentTime()
    {
      this.ScrollToVerticalOffset(((DateTime.Now - DateTime.Today).TotalMinutes - (double) CalendarGeoHelper.GetStartHour() * 60.0) / ((double) (CalendarGeoHelper.GetEndHour() - CalendarGeoHelper.GetStartHour()) * 60.0) * (CalendarGeoHelper.HourHeight * (double) (CalendarGeoHelper.GetEndHour() - CalendarGeoHelper.GetStartHour())) - this.ActualHeight * 0.5);
    }

    private void Scroll(bool isUp, EventHandler completedHandler)
    {
      if (this._scrolling || Mouse.LeftButton != MouseButtonState.Pressed)
        return;
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.From = new double?(this.VerticalOffset);
      doubleAnimation.To = new double?(this.VerticalOffset + (isUp ? -36.0 : 36.0));
      doubleAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(32.0));
      DoubleAnimation element = doubleAnimation;
      this._scrollStoryboard.Children.Clear();
      this._scrollStoryboard.Children.Add((Timeline) element);
      Storyboard.SetTarget((DependencyObject) element, (DependencyObject) this);
      Storyboard.SetTargetProperty((DependencyObject) element, new PropertyPath((object) ScrollAnimationBehavior.VerticalOffsetProperty));
      this._scrollStoryboard.Completed -= completedHandler;
      this._scrollStoryboard.Completed += completedHandler;
      this._scrollStoryboard.Begin();
      this._scrolling = true;
    }

    private void StopScrollUp()
    {
      this._scrollingUp = false;
      this._startScrollUpTimer.Elapsed -= new ElapsedEventHandler(this.OnStartScrollElapsed);
    }

    private void ScrollUpCompletedHandler(object sender, EventArgs e)
    {
      this._scrolling = false;
      if (!this._scrollingUp)
        return;
      this.Scroll(true, new EventHandler(this.ScrollUpCompletedHandler));
    }

    private void StartScrollDown()
    {
      this._scrollingDown = true;
      this.Scroll(false, new EventHandler(this.ScrollDownCompletedHandler));
    }

    private void ScrollDownCompletedHandler(object sender, EventArgs e)
    {
      this._scrolling = false;
      if (!this._scrollingDown)
        return;
      this.Scroll(false, new EventHandler(this.ScrollDownCompletedHandler));
    }

    private void StopScrollDown() => this._scrollingDown = false;

    public void RemoveAddCell()
    {
      foreach (CalendarTimelineDayView dayView in this._dayViews)
        dayView.RemoveAddCell();
    }

    public DateTime GetHoverDate(double currentX)
    {
      double dayViewWidth = this._parent.DayViewWidth;
      int num = (int) ((currentX - 60.0) / dayViewWidth);
      DateTime dateTime = this._parent.GetTimeSpan().Item1;
      return num >= 0 && num < this._parent.DisplayDays && this._dayViews.Count > num + 1 ? this._dayViews[num + 1].GetDate() : dateTime;
    }

    private void ScrollToTime()
    {
      if (this.VerticalOffset > 0.0)
        return;
      double num = (DateTime.Now - DateTime.Today).TotalMinutes - 150.0;
      if (num <= 0.0)
        return;
      this.ScrollToVerticalOffset(num / 1440.0 * (CalendarGeoHelper.HourHeight * 24.0));
    }

    public void ShowMoveCell(TaskCellViewModel cellModel, bool log = false)
    {
      if (!cellModel.BaseDate.HasValue)
        return;
      DateTime date1 = cellModel.BaseDate.Value.Date;
      TaskCellViewModel taskCellViewModel = (TaskCellViewModel) null;
      DateTime? nullable1 = new DateTime?();
      CalendarGeoHelper.GetCellPosition(cellModel);
      DateTime? nullable2 = cellModel.BarMode ? new DateTime?() : cellModel.DisplayDueDate;
      if (!cellModel.BarMode)
      {
        if (nullable2.HasValue && nullable2.Value.Date > date1)
        {
          taskCellViewModel = TaskCellViewModel.Copy(cellModel);
          taskCellViewModel.BaseOnStart = !cellModel.BaseOnStart;
          CalendarGeoHelper.GetCellPosition(taskCellViewModel);
          nullable1 = new DateTime?(date1.AddDays(1.0));
        }
        if (cellModel.DisplayStartDate.HasValue && cellModel.DisplayStartDate.Value < date1)
        {
          taskCellViewModel = TaskCellViewModel.Copy(cellModel);
          taskCellViewModel.BaseOnStart = !cellModel.BaseOnStart;
          CalendarGeoHelper.GetCellPosition(taskCellViewModel);
          nullable1 = new DateTime?(date1.AddDays(-1.0));
        }
      }
      foreach (CalendarTimelineDayView dayView in this._dayViews)
      {
        DateTime date2 = dayView.GetDate();
        if (date2 == date1)
        {
          if (log)
            UtilLog.Info("ShowAddCell:current");
          dayView.ShowAddCell(cellModel);
        }
        else
        {
          DateTime dateTime = date2;
          DateTime? nullable3 = nullable1;
          if ((nullable3.HasValue ? (dateTime == nullable3.GetValueOrDefault() ? 1 : 0) : 0) != 0)
          {
            if (log)
              UtilLog.Info("ShowAddCell:next");
            dayView.ShowAddCell(taskCellViewModel);
          }
          else
            dayView.RemoveAddCell();
        }
      }
    }

    public double GetHorizontalOffset(double currentX)
    {
      return CalendarGeoHelper.CalHorizontalOffset(currentX, CalendarGeoHelper.GetWeekColumnWidth(this.ActualWidth, this._parent.DisplayDays)) - 1.0;
    }

    public void ClearItems(bool onlyData)
    {
      foreach (CalendarTimelineDayView dayView in this._dayViews)
      {
        if (dayView != null)
        {
          dayView.ClearItems(onlyData);
          dayView.DataContext = (object) null;
        }
      }
    }

    public void InitBoard() => this._timeLine?.InitBoard(this.ShowPointer());

    private void InitFoldBorder()
    {
      this._container.Children.Add((UIElement) this._topFoldBorder);
      this._container.Children.Add((UIElement) this._botFoldBorder);
      this._topFoldBorder.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnFoldBorderMouseDown);
      this._botFoldBorder.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnFoldBorderMouseDown);
      this._topFoldBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTopFoldClick);
      this._botFoldBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBotFoldClick);
      Border border1 = new Border();
      border1.HorizontalAlignment = HorizontalAlignment.Left;
      border1.VerticalAlignment = VerticalAlignment.Top;
      border1.CornerRadius = new CornerRadius(2.0);
      Border element1 = border1;
      element1.SetResourceReference(Control.BackgroundProperty, (object) "ToolTipBackColor");
      Border border2 = new Border();
      border2.CornerRadius = new CornerRadius(2.0);
      border2.Effect = (Effect) new DropShadowEffect()
      {
        Opacity = 0.12,
        BlurRadius = 11.0,
        ShadowDepth = 2.0,
        Direction = 280.0
      };
      Border border3 = border2;
      border3.SetResourceReference(Control.BackgroundProperty, (object) "ToolTipTopColor");
      element1.Child = (UIElement) border3;
      TextBlock textBlock = new TextBlock()
      {
        Padding = new Thickness(6.0),
        TextTrimming = TextTrimming.CharacterEllipsis,
        Text = Utils.GetString("DragToAdjustHiddenTime"),
        TextAlignment = TextAlignment.Center
      };
      textBlock.SetResourceReference(FrameworkElement.StyleProperty, (object) "Tag01");
      textBlock.Foreground = (Brush) Brushes.White;
      border3.Child = (UIElement) textBlock;
      this._foldTooltipGrid.Children.Add((UIElement) element1);
      Geometry geometry = Geometry.Parse("M0,4 4,8,4,0z");
      Path path1 = new Path();
      path1.Data = geometry;
      path1.Margin = new Thickness(-4.0, 0.0, 0.0, 0.0);
      path1.Height = 8.0;
      path1.VerticalAlignment = VerticalAlignment.Center;
      path1.HorizontalAlignment = HorizontalAlignment.Left;
      Path element2 = path1;
      element2.SetResourceReference(Shape.FillProperty, (object) "ToolTipBackColor");
      Path path2 = new Path();
      path2.Data = geometry;
      path2.Margin = new Thickness(-4.0, 0.0, 0.0, 0.0);
      path2.Height = 8.0;
      path2.VerticalAlignment = VerticalAlignment.Center;
      path2.HorizontalAlignment = HorizontalAlignment.Left;
      Path element3 = path2;
      element3.SetResourceReference(Shape.FillProperty, (object) "ToolTipTopColor");
      this._foldTooltipGrid.Children.Add((UIElement) element3);
      this._foldTooltipGrid.Children.Add((UIElement) element2);
      this._foldTooltipGrid.Visibility = Visibility.Collapsed;
      this._foldTooltipGrid.SetValue(Panel.ZIndexProperty, (object) 100);
      this._container.Children.Add((UIElement) this._foldTooltipGrid);
    }

    public void ShowOrHideFoldTooltip(bool show)
    {
      if (show && (this._timeLine.TopFoldHover || this._timeLine.BottomFoldHover))
      {
        this._foldTooltipGrid.Margin = new Thickness(66.0, (this._timeLine.TopFoldHover ? (double) LocalSettings.Settings.CollapsedStart : (double) LocalSettings.Settings.CollapsedEnd) * LocalSettings.Settings.CalendarHourHeight - 12.0, 0.0, 0.0);
        this._foldTooltipGrid.Visibility = Visibility.Visible;
      }
      else
        this._foldTooltipGrid.Visibility = Visibility.Collapsed;
    }

    private void OnFoldBorderMouseDown(object sender, MouseButtonEventArgs e)
    {
      this._foldBorderMouseDown = true;
    }

    private void OnTopFoldClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._foldBorderMouseDown)
        return;
      this._foldBorderMouseDown = false;
      CalendarGeoHelper.TopFolded = false;
      CalendarGeoHelper.NotifyTopUnFolded(true);
    }

    private void OnBotFoldClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._foldBorderMouseDown)
        return;
      this._foldBorderMouseDown = false;
      CalendarGeoHelper.TopFolded = false;
      CalendarGeoHelper.NotifyTopUnFolded(false);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (e.HeightChanged && CalendarGeoHelper.HourHeight * (double) (24 - CalendarGeoHelper.GetCollapsedHours()) + (CalendarGeoHelper.TopFolded ? 64.0 : 0.0) < this.ActualHeight)
        ZoomHelper.Zoom(0.0, this.ActualHeight, (ScrollViewer) this);
      if (!CalendarGeoHelper.TopFolded)
        return;
      this._botFoldBorder.Height = Math.Max(32.0, this.ActualHeight - (CalendarGeoHelper.HourHeight * (double) (24 - CalendarGeoHelper.GetCollapsedHours()) + (CalendarGeoHelper.TopFolded ? 32.0 : 0.0)));
    }

    public void OnTouchScroll(double offset, bool showWeekend)
    {
      if (this._data == null)
        return;
      bool flag = false;
      MultiDayView parent1 = this._parent;
      double num1 = parent1 != null ? parent1.DayViewWidth : this.ActualWidth;
      MultiDayView parent2 = this._parent;
      int displayDays = parent2 != null ? parent2.DisplayDays : 0;
      foreach (CalendarTimelineDayView dayView in this._dayViews)
      {
        if (dayView.RenderTransform is TranslateTransform renderTransform)
        {
          double num2 = renderTransform.X + offset;
          DateTime date = dayView.GetDate();
          if (!Utils.IsEmptyDate(date))
          {
            DateTime newDate = date;
            if (num2 <= -1.5 * num1)
            {
              num2 += (double) (displayDays + 2) * num1;
              flag = true;
              newDate = date.AddDays((double) (displayDays + 2));
              if (!showWeekend)
                newDate = newDate.AddDays(date.DayOfWeek == DayOfWeek.Friday || date.DayOfWeek == DayOfWeek.Thursday ? 4.0 : 2.0);
            }
            if (num2 >= ((double) displayDays + 0.5) * num1 + 2.0)
            {
              num2 -= (double) (displayDays + 2) * num1;
              flag = true;
              newDate = date.AddDays((double) (-2 - displayDays));
              if (!showWeekend)
                newDate = newDate.AddDays(date.DayOfWeek == DayOfWeek.Monday || date.DayOfWeek == DayOfWeek.Tuesday ? -4.0 : -2.0);
            }
            dayView.Offset = num2;
            renderTransform.X = num2;
            if (newDate != date)
            {
              CalendarTimelineDayViewModel timelineDayViewModel = this._data.FirstOrDefault<CalendarTimelineDayViewModel>((Func<CalendarTimelineDayViewModel, bool>) (d => d.Date == newDate));
              if (timelineDayViewModel != null)
              {
                dayView.DataContext = (object) timelineDayViewModel;
                dayView.Reload(new double?(num1));
              }
              else
              {
                dayView.DataContext = (object) new CalendarTimelineDayViewModel()
                {
                  Date = newDate
                };
                dayView.Reload(new double?(num1));
                this.LoadModelsWhenScrolling(newDate, newDate > date);
              }
            }
          }
        }
      }
      if (!flag)
        return;
      this._dayViews.Sort((Comparison<CalendarTimelineDayView>) ((a, b) => a.Offset.CompareTo(b.Offset)));
    }

    private async Task LoadModelsWhenScrolling(DateTime newDate, bool next)
    {
      List<CalendarTimelineDayViewModel> timelineDayViewModelList1;
      if (this._parent == null)
        timelineDayViewModelList1 = (List<CalendarTimelineDayViewModel>) null;
      else
        timelineDayViewModelList1 = await this._parent.LoadModelsWhenScrolling(newDate, next);
      List<CalendarTimelineDayViewModel> timelineDayViewModelList2 = timelineDayViewModelList1;
      if (timelineDayViewModelList2 == null)
        return;
      this._data = timelineDayViewModelList2;
      foreach (CalendarTimelineDayView dayView in this._dayViews)
      {
        CalendarTimelineDayViewModel m = dayView.DataContext as CalendarTimelineDayViewModel;
        if (m != null)
        {
          CalendarTimelineDayViewModel timelineDayViewModel = this._data.FirstOrDefault<CalendarTimelineDayViewModel>((Func<CalendarTimelineDayViewModel, bool>) (d => d.Date == m.Date));
          if (timelineDayViewModel != m && timelineDayViewModel != null)
          {
            dayView.DataContext = (object) timelineDayViewModel;
            dayView.Reload(this._parent?.DayViewWidth);
          }
        }
      }
    }

    private void SetPointerPosition()
    {
      int pointOffset = CalendarGeoHelper.GetPointOffset();
      int num;
      if (pointOffset >= 32)
      {
        MultiDayView parent = this._parent;
        num = parent != null ? (parent.ShowTimePoint() ? 1 : 0) : 0;
      }
      else
        num = 0;
      bool checkNow = num != 0;
      this._timeLine?.InitBoard(checkNow);
      if (checkNow)
      {
        if (this._timePointer.Children.Count == 0)
        {
          this._timePointer.SetValue(Panel.ZIndexProperty, (object) 10);
          this._timePointer.ColumnDefinitions.Add(new ColumnDefinition()
          {
            Width = new GridLength(60.0)
          });
          this._timePointer.ColumnDefinitions.Add(new ColumnDefinition()
          {
            Width = new GridLength(1.0, GridUnitType.Star)
          });
          Line line = new Line();
          line.X1 = 0.0;
          line.X2 = 1.0;
          line.Stretch = Stretch.Fill;
          line.StrokeThickness = 1.0;
          line.Stroke = (Brush) ThemeUtil.GetColorInString("#33E03131");
          line.VerticalAlignment = VerticalAlignment.Top;
          line.Margin = new Thickness(0.0, 9.0, 0.0, 0.0);
          line.IsHitTestVisible = false;
          Line element1 = line;
          element1.SetValue(Grid.ColumnProperty, (object) 1);
          this._timePointer.Children.Add((UIElement) element1);
          this._container.Children.Add((UIElement) this._timePointer);
          StackPanel stackPanel = new StackPanel();
          stackPanel.Height = 18.0;
          stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
          stackPanel.VerticalAlignment = VerticalAlignment.Center;
          stackPanel.Orientation = Orientation.Horizontal;
          this._currentTimeStack = stackPanel;
          TextBlock textBlock = new TextBlock();
          textBlock.Foreground = (Brush) ThemeUtil.GetColorInString("#E03131");
          textBlock.FontSize = 10.0;
          textBlock.VerticalAlignment = VerticalAlignment.Center;
          textBlock.HorizontalAlignment = HorizontalAlignment.Center;
          textBlock.Margin = new Thickness(4.0, 0.0, 4.0, 0.0);
          this._currentTime = textBlock;
          Border border = new Border();
          border.Height = 18.0;
          border.CornerRadius = new CornerRadius(4.0);
          border.Background = (Brush) ThemeUtil.GetColorInString("#1AE03131");
          border.Child = (UIElement) this._currentTime;
          border.MinWidth = 36.0;
          Border element2 = border;
          Path path = new Path();
          path.Data = Geometry.Parse("F0 M4,10z M0,0z M0,0.283029556291895L0,9.71701004669189C0,8.33785004669189,0.71048,7.05597004669189,1.88,6.32502004669189L4,5.00002004669189 1.88,3.67502004669189C0.71048,2.94407004669189,0,1.66219004669189,0,0.283029556291895z");
          path.Width = 4.0;
          path.Height = 10.0;
          path.Fill = element2.Background;
          Path element3 = path;
          this._currentTimeStack.Children.Add((UIElement) element2);
          this._currentTimeStack.Children.Add((UIElement) element3);
          this._timePointer.Children.Add((UIElement) this._currentTimeStack);
        }
        this._timePointer.Visibility = Visibility.Visible;
        this._timePointer.Margin = new Thickness(0.0, (double) (pointOffset - 11), 0.0, 0.0);
        this._currentTime.Text = LocalSettings.Settings.TimeFormat == "24Hour" ? ticktick_WPF.Util.DateUtils.FormatHourMinute(DateTime.Now, true) : DateTime.Now.ToString("h:mm");
        this._currentTimeStack.Margin = new Thickness(0.0, 0.0, LocalSettings.Settings.TimeFormat == "24Hour" ? 2.0 : 6.0, 0.0);
      }
      else
      {
        if (this._timePointer == null)
          return;
        this._timePointer.Visibility = Visibility.Collapsed;
      }
    }

    public void SetTimePointVisible(bool b)
    {
      if (b)
        b = CalendarGeoHelper.GetPointOffset() >= 32;
      if (this._timePointer != null)
        this._timePointer.Visibility = b ? Visibility.Visible : Visibility.Collapsed;
      this.SetPointerPosition();
    }
  }
}
