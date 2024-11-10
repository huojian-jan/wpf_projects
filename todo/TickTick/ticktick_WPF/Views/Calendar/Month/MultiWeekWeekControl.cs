// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Month.MultiWeekWeekControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Month
{
  public class MultiWeekWeekControl : Grid
  {
    private string _uid;
    private List<WeekEventModel> _events;
    private List<TaskBar> _taskBars;
    private List<CalLoadMore> _loadMores;
    private Dictionary<int, MultiWeekDayControl> _dayControls;
    private bool _eventsSetting;
    private readonly Dictionary<int, Line> _lines;
    private Line _hLine;
    private readonly Grid _spanItems;
    private Grid _monthGrid;
    private Storyboard _monthGridStory;
    private double _offset;
    private object _lock;
    private Border _monthTextBorder;
    private VisualBrush _borderBrush;

    public DateTime StartDate { get; set; }

    public double Offset
    {
      get => this._offset;
      set
      {
        this._offset = value;
        if (this._borderBrush == null)
          return;
        Border windowBackground = App.Window.WindowBackground;
        this._borderBrush.Viewbox = new Rect(54.0 / windowBackground.ActualWidth, (this.Offset + 90.0) / windowBackground.ActualHeight, 0.0, 0.0);
      }
    }

    public MultiWeekWeekControl()
    {
      Grid grid = new Grid();
      grid.Margin = new Thickness(0.0, 28.0, 0.0, 0.0);
      this._spanItems = grid;
      this._lock = new object();
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this._uid = Utils.GetGuid();
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      Line line = new Line();
      line.X1 = 0.0;
      line.X2 = 1.0;
      line.Stretch = Stretch.Fill;
      line.StrokeThickness = 1.0;
      line.VerticalAlignment = VerticalAlignment.Bottom;
      this._hLine = line;
      this._hLine.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
      this.Children.Add((UIElement) this._hLine);
      this.Children.Add((UIElement) this._spanItems);
      this._spanItems.SetValue(Panel.ZIndexProperty, (object) 100);
      this.SetColumns(LocalSettings.Settings.ShowCalWeekend ? 7 : 5);
      this.AddMonthText();
    }

    private void AddMonthText()
    {
      Grid grid = new Grid();
      grid.RenderTransform = (Transform) new TranslateTransform(0.0, -10.0);
      grid.Margin = new Thickness(2.0, 0.0, 0.0, 0.0);
      grid.Height = 40.0;
      grid.VerticalAlignment = VerticalAlignment.Top;
      grid.HorizontalAlignment = HorizontalAlignment.Left;
      grid.Opacity = 0.0;
      grid.IsHitTestVisible = false;
      this._monthGrid = grid;
      this._monthGrid.SetValue(Grid.ColumnSpanProperty, (object) 5);
      Border border1 = new Border();
      border1.Height = 36.0;
      border1.Effect = (Effect) new BlurEffect()
      {
        Radius = 6.0,
        RenderingBias = RenderingBias.Performance
      };
      this._monthTextBorder = border1;
      TextBlock textBlock = new TextBlock();
      textBlock.FontSize = 24.0;
      textBlock.VerticalAlignment = VerticalAlignment.Center;
      textBlock.Margin = new Thickness(16.0, 0.0, 16.0, 0.0);
      TextBlock element = textBlock;
      this._monthGrid.Children.Add((UIElement) this._monthTextBorder);
      Border border2 = new Border();
      border2.SetResourceReference(Panel.BackgroundProperty, (object) "ShowAreaBackground");
      this._monthTextBorder.Child = (UIElement) border2;
      this._monthGrid.Children.Add((UIElement) element);
      element.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity100");
      this.Children.Add((UIElement) this._monthGrid);
    }

    public void SetColumns(int count)
    {
      lock (this._lock)
      {
        if (count == this.ColumnDefinitions.Count)
          return;
        int count1 = this.ColumnDefinitions.Count;
        if (count > count1)
        {
          for (int key = count1; key < count; ++key)
          {
            this.ColumnDefinitions.Add(new ColumnDefinition()
            {
              Width = new GridLength(1.0, GridUnitType.Star)
            });
            MultiWeekDayControl element1 = new MultiWeekDayControl();
            element1.SetValue(Grid.ColumnProperty, (object) key);
            this._dayControls[key] = element1;
            this.Children.Add((UIElement) element1);
            if (key >= 1)
            {
              Line line = new Line();
              line.Y1 = 0.0;
              line.Y2 = 1.0;
              line.Stretch = Stretch.Fill;
              line.StrokeThickness = 1.0;
              line.HorizontalAlignment = HorizontalAlignment.Right;
              Line element2 = line;
              element2.SetValue(Grid.ColumnProperty, (object) (key - 1));
              element2.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity5");
              this.Children.Add((UIElement) element2);
              this._lines[key - 1] = element2;
            }
          }
        }
        else
        {
          this.ColumnDefinitions.RemoveRange(count, count1 - count);
          for (int key = count; key < count1; ++key)
          {
            MultiWeekDayControl element3;
            if (this._dayControls.TryGetValue(key, out element3))
            {
              this._dayControls.Remove(key);
              this.Children.Remove((UIElement) element3);
            }
            Line element4;
            if (this._lines.TryGetValue(key - 1, out element4))
            {
              this._lines.Remove(key - 1);
              this.Children.Remove((UIElement) element4);
            }
          }
        }
        this._hLine?.SetValue(Grid.ColumnSpanProperty, (object) count);
        this._spanItems?.SetValue(Grid.ColumnSpanProperty, (object) count);
      }
    }

    public void SetData(Dictionary<int, List<MonthDayViewModel>> modelDict, int rowIndex)
    {
      List<MonthDayViewModel> monthDayViewModelList;
      if (!modelDict.TryGetValue(rowIndex, out monthDayViewModelList))
        return;
      int index = LocalSettings.Settings.ShowCalWeekend ? 0 : monthDayViewModelList.FindIndex((Predicate<MonthDayViewModel>) (m => m.Date.DayOfWeek == DayOfWeek.Monday));
      foreach (int key in this._dayControls.Keys)
      {
        MultiWeekDayControl dayControl = this._dayControls[key];
        if (monthDayViewModelList.Count > index + key)
        {
          MonthDayViewModel monthDayViewModel = monthDayViewModelList[index + key];
          dayControl.DataContext = (object) monthDayViewModel;
        }
      }
      this.StartDate = monthDayViewModelList[0].Date;
      DateTime dateTime;
      DateTime? date;
      if (this.StartDate.Day != 1)
      {
        int month1 = this.StartDate.Month;
        dateTime = this.StartDate;
        dateTime = dateTime.AddDays(6.0);
        int month2 = dateTime.Month;
        if (month1 == month2)
        {
          date = new DateTime?();
          goto label_12;
        }
      }
      dateTime = this.StartDate;
      date = new DateTime?(dateTime.AddDays(6.0));
label_12:
      this.SetMonthText(date);
    }

    private void SetMonthText(DateTime? date)
    {
      if (date.HasValue)
      {
        this._monthGrid.Visibility = Visibility.Visible;
        Grid monthGrid = this._monthGrid;
        if ((monthGrid != null ? (monthGrid.Children.Count == 2 ? 1 : 0) : 0) != 0 && this._monthGrid.Children[1] is TextBlock child)
        {
          child.Inlines.Clear();
          InlineCollection inlines = child.Inlines;
          Run run = new Run();
          run.FontWeight = FontWeights.Bold;
          run.Text = ticktick_WPF.Util.DateUtils.FormatMonth(date.Value);
          inlines.Add((Inline) run);
          child.Inlines.Add((Inline) new Run()
          {
            Text = (" " + ticktick_WPF.Util.DateUtils.FormatYear(date.Value))
          });
        }
        if (Utils.FindParent<MainWindow>((DependencyObject) this) != null)
        {
          Border windowBackground = App.Window.WindowBackground;
          VisualBrush visualBrush = new VisualBrush();
          visualBrush.AlignmentX = AlignmentX.Left;
          visualBrush.AlignmentY = AlignmentY.Top;
          visualBrush.Visual = (Visual) windowBackground;
          visualBrush.Stretch = Stretch.None;
          visualBrush.Viewbox = new Rect(54.0 / windowBackground.ActualWidth, (this.Offset + 90.0) / windowBackground.ActualHeight, 0.0, 0.0);
          this._borderBrush = visualBrush;
          this._monthTextBorder.Background = (Brush) this._borderBrush;
        }
        this._monthGridStory = new Storyboard();
        DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(), 1.0, 40);
        DoubleAnimation doubleAnimation2 = AnimationUtils.GetDoubleAnimation(new double?(), 0.0, 300);
        doubleAnimation2.BeginTime = new TimeSpan?(TimeSpan.FromMilliseconds(500.0));
        Storyboard.SetTarget((DependencyObject) doubleAnimation1, (DependencyObject) this._monthGrid);
        Storyboard.SetTarget((DependencyObject) doubleAnimation2, (DependencyObject) this._monthGrid);
        Storyboard.SetTargetProperty((DependencyObject) doubleAnimation1, new PropertyPath((object) UIElement.OpacityProperty));
        Storyboard.SetTargetProperty((DependencyObject) doubleAnimation2, new PropertyPath((object) UIElement.OpacityProperty));
        this._monthGridStory.Children.Add((Timeline) doubleAnimation1);
        this._monthGridStory.Children.Add((Timeline) doubleAnimation2);
      }
      else
      {
        this._monthGrid.Visibility = Visibility.Collapsed;
        this._borderBrush = (VisualBrush) null;
        this._monthTextBorder.Background = (Brush) null;
        this._monthGridStory = (Storyboard) null;
      }
    }

    public void ShowMonthText() => this._monthGridStory?.Begin();

    private MultiWeekDayControl FindDayControlByIndex(int column)
    {
      MultiWeekDayControl multiWeekDayControl;
      return this._dayControls.TryGetValue(column, out multiWeekDayControl) ? multiWeekDayControl : (MultiWeekDayControl) null;
    }

    public void ShowLoadMore(int column, MouseButtonEventArgs e, bool locked)
    {
      this.FindDayControlByIndex(column)?.ShowLoadMore((MouseEventArgs) e, locked);
    }

    public void SetEvents(IEnumerable<WeekEventModel> events, bool wait = true)
    {
      this._events = events != null ? events.ToList<WeekEventModel>() : (List<WeekEventModel>) null;
      if (this._eventsSetting & wait)
        DelayActionHandlerCenter.TryDoAction(this._uid, new EventHandler(this.SetEvents), 100);
      else
        this.SetEvents();
    }

    private void SetEvents(object sender, EventArgs args)
    {
      ThreadUtil.DetachedRunOnUiThread(new Action(this.SetEvents));
    }

    private void SetEvents()
    {
      this._eventsSetting = true;
      if (this._events != null)
      {
        List<WeekEventModel> events = this._events;
        List<WeekEventModel> list1 = events.Where<WeekEventModel>((Func<WeekEventModel, bool>) (e => !e.Data.IsLoadMore)).ToList<WeekEventModel>();
        List<WeekEventModel> list2 = events.Where<WeekEventModel>((Func<WeekEventModel, bool>) (e => e.Data.IsLoadMore)).ToList<WeekEventModel>();
        this.SetTaskBar((IReadOnlyList<WeekEventModel>) list1);
        this.SetLoadMore(list2);
      }
      this._eventsSetting = false;
    }

    private void SetLoadMore(List<WeekEventModel> events)
    {
      int num = events.Count - this._loadMores.Count;
      if (num > 0)
      {
        for (int index = 0; index < num; ++index)
        {
          CalLoadMore calLoadMore = new CalLoadMore();
          calLoadMore.Visibility = Visibility.Hidden;
          CalLoadMore element = calLoadMore;
          this._loadMores.Add(element);
          this._spanItems.Children.Add((UIElement) element);
        }
      }
      for (int index = 0; index < this._loadMores.Count; ++index)
      {
        CalLoadMore loadMore = this._loadMores[index];
        if (loadMore != null)
        {
          if (index < events.Count)
          {
            double dayWidth = this.ActualWidth / (double) CalendarGeoHelper.CalColumns;
            loadMore.Margin = new Thickness(CalendarGeoHelper.GetBarLeft(dayWidth, events[index].Column) + (events[index].Data.LoadMoreWidth == 0.0 ? 0.0 : dayWidth - events[index].Data.LoadMoreWidth), (double) (events[index].Row * GridGeoAssembler.TaskBarHeight), 0.0, 0.0);
            loadMore.Width = events[index].Data.LoadMoreWidth == 0.0 ? CalendarGeoHelper.GetBarWidth(dayWidth, events[index].Column, events[index].ColumnSpan) : events[index].Data.LoadMoreWidth;
            loadMore.DataContext = (object) events[index].Data;
            loadMore.SetMoreBorderMargin(events[index].Data.LoadMoreWidth == 0.0);
            loadMore.Visibility = Visibility.Visible;
          }
          else
          {
            loadMore.DataContext = (object) null;
            if (loadMore.Visibility == Visibility.Visible)
              loadMore.Visibility = Visibility.Hidden;
          }
        }
      }
    }

    private void SetTaskBar(IReadOnlyList<WeekEventModel> events)
    {
      Visibility visibility = this.ActualWidth / (double) CalendarGeoHelper.CalColumns < 80.0 ? Visibility.Collapsed : Visibility.Visible;
      int num = events.Count - this._taskBars.Count;
      if (num > 0)
      {
        for (int index = 0; index < num; ++index)
        {
          TaskBar taskBar = new TaskBar();
          taskBar.Visibility = Visibility.Hidden;
          TaskBar element = taskBar;
          this._taskBars.Add(element);
          this._spanItems.Children.Add((UIElement) element);
        }
      }
      for (int index = 0; index < this._taskBars.Count; ++index)
      {
        TaskBar taskBar = this._taskBars[index];
        if (taskBar != null)
        {
          if (index < events.Count)
          {
            double dayWidth = this.ActualWidth / (double) CalendarGeoHelper.CalColumns;
            taskBar.Margin = new Thickness(CalendarGeoHelper.GetBarLeft(dayWidth, events[index].Column), (double) (events[index].Row * GridGeoAssembler.TaskBarHeight), 0.0, 0.0);
            taskBar.Width = Math.Max(0.0, CalendarGeoHelper.GetBarWidth(dayWidth - events[index].Data.LoadMoreWidth, events[index].Column, events[index].ColumnSpan) - 1.0);
            taskBar.DataContext = (object) events[index].Data;
            taskBar.Visibility = Visibility.Visible;
            taskBar.TimeText.Visibility = events[index].ColumnSpan > 1 ? Visibility.Visible : visibility;
          }
          else
          {
            taskBar.DataContext = (object) null;
            if (taskBar.Visibility == Visibility.Visible)
              taskBar.Visibility = Visibility.Hidden;
          }
        }
      }
    }

    public void SetDropTarget(MouseEventArgs arg)
    {
      bool flag = this.IsDragHover(arg);
      foreach (MultiWeekDayControl multiWeekDayControl in this._dayControls.Values)
      {
        if (flag && multiWeekDayControl.IsDragHover(arg))
          multiWeekDayControl.SetDropTarget();
        else
          multiWeekDayControl.ClearDropTarget();
      }
    }

    public bool IsDragHover(MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      double x = position.X;
      double y = position.Y;
      return x > 0.0 && x < this.ActualWidth && y > 0.0 && y < this.ActualHeight;
    }

    public void ClearDropTarget()
    {
      foreach (MultiWeekDayControl multiWeekDayControl in this._dayControls.Values)
        multiWeekDayControl.ClearDropTarget();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size size = e.PreviousSize;
      double width1 = size.Width;
      size = e.NewSize;
      double width2 = size.Width;
      if (Math.Abs(width1 - width2) <= 0.0)
        return;
      this.SetEvents((IEnumerable<WeekEventModel>) this._events);
    }

    public void ClearItems(bool onlyData)
    {
      this._taskBars.Clear();
      if (onlyData)
      {
        foreach (UIElement loadMore in this._loadMores)
          this._spanItems.Children.Remove(loadMore);
        if (this._spanItems.Children.Count > 14)
          this._spanItems.Children.RemoveRange(14, this._spanItems.Children.Count - 14);
        foreach (object child in this._spanItems.Children)
        {
          if (child is TaskBar taskBar)
          {
            taskBar.DataContext = (object) null;
            taskBar.Visibility = Visibility.Hidden;
            this._taskBars.Add(taskBar);
          }
        }
      }
      else
        this._spanItems.Children.Clear();
      foreach (MultiWeekDayControl multiWeekDayControl in this._dayControls.Values)
        multiWeekDayControl.ClearData();
      this._events?.Clear();
      this._loadMores.Clear();
    }

    public void TryShowAddWindow(DateTime startDate)
    {
      foreach (MultiWeekDayControl multiWeekDayControl in this._dayControls.Values)
      {
        if (multiWeekDayControl.DataContext is MonthDayViewModel dataContext && dataContext.Date == startDate)
          multiWeekDayControl.ShowAddTaskPopup((MouseEventArgs) null, startDate, startDate);
      }
    }

    public void AddDays(int days)
    {
      this.StartDate = this.StartDate.AddDays((double) days);
      foreach (FrameworkElement frameworkElement in this._dayControls.Values)
      {
        if (frameworkElement.DataContext is MonthDayViewModel dataContext)
          dataContext.AddDays(days);
      }
      DateTime dateTime = this.StartDate;
      DateTime? date;
      if (dateTime.Day != 1)
      {
        dateTime = this.StartDate;
        int month1 = dateTime.Month;
        dateTime = this.StartDate;
        dateTime = dateTime.AddDays(6.0);
        int month2 = dateTime.Month;
        if (month1 == month2)
        {
          date = new DateTime?();
          goto label_10;
        }
      }
      dateTime = this.StartDate;
      date = new DateTime?(dateTime.AddDays(6.0));
label_10:
      this.SetMonthText(date);
    }

    public void SetCurrentMonth(DateTime currentMonth)
    {
      foreach (FrameworkElement frameworkElement in this._dayControls.Values)
      {
        if (frameworkElement.DataContext is MonthDayViewModel dataContext)
          dataContext.SetCurrentMonth(currentMonth);
      }
    }

    public void FlashNaviDates(List<DateTime> navDates, bool delay = false)
    {
      foreach (MultiWeekDayControl multiWeekDayControl in this._dayControls.Values)
      {
        if (multiWeekDayControl.DataContext is MonthDayViewModel dataContext && navDates.Contains(dataContext.Date))
          multiWeekDayControl.Flash(delay);
      }
    }
  }
}
