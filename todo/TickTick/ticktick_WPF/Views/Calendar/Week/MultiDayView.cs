// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Week.MultiDayView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;
using ticktick_WPF.Util.KotlinUtils;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar.Month;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Print;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Week
{
  public class MultiDayView : Grid, INavigate, IDragBarEvent, IDragCellEvent
  {
    public int DisplayDays;
    public double DayViewWidth;
    private bool _cellHandling;
    private readonly RowDefinition _firstRow;
    private readonly GridSplitter _splitter;
    private WeekGridViewModel _model;
    private List<CalendarDisplayModel> _tasks;
    private CalendarTimelineView _timelineView;
    private MultiDayAllDayView _alldayView;
    private List<CalendarTimelineDayViewModel> _pointVms;
    private CalendarControl _calendarControl;
    private List<CalendarDisplayViewModel> _assembles;
    private TextBlock _weekNum;
    private string _uid;
    private Border _flashBorder;
    private bool _leftScroll;
    private Grid _linesGrid;
    private List<WeekEventModel> _allDayVms;
    private List<MultiDayLine> _vLines;
    private bool _loading;
    private bool _dragFromArrange;
    private double _dragStartY;
    private bool _cellMoving;
    private TaskCellViewModel _dragCellModel;
    private DateTime? _dragDueTime;
    private DateTime? _dragStartTime;
    private Border _dragPopup;
    private TaskDragCell _dragCell;
    private Window _parentWindow;
    private Path _popTriangle;
    private string _copyId;
    private bool _scrolling;
    private bool _showWeekend;
    private BlockingSet<DateTime> _scrollLoadedDate;
    private string _weeks;
    private string _mode;

    public bool IsLocked { get; set; }

    public event EventHandler<(DateTime, DateTime)> DateRangeChanged;

    public MultiDayView(CalendarControl calendarControl)
    {
      Grid grid = new Grid();
      grid.Margin = new Thickness(61.0, 0.0, 1.0, 0.0);
      grid.ClipToBounds = true;
      this._linesGrid = grid;
      this._vLines = new List<MultiDayLine>();
      this._showWeekend = true;
      this._scrollLoadedDate = new BlockingSet<DateTime>();
      // ISSUE: explicit constructor call
      base.\u002Ector();
      this._uid = Utils.GetGuid();
      this._calendarControl = calendarControl;
      this._firstRow = new RowDefinition()
      {
        MinHeight = 88.0,
        Height = new GridLength(LocalSettings.Settings.WeekAllDayHeight)
      };
      this.RowDefinitions.Add(this._firstRow);
      this.RowDefinitions.Add(new RowDefinition());
      Line line1 = new Line();
      line1.X1 = 0.0;
      line1.X2 = 1.0;
      line1.Stretch = Stretch.Fill;
      line1.StrokeThickness = 1.0;
      line1.VerticalAlignment = VerticalAlignment.Top;
      line1.Margin = new Thickness(0.0, 40.0, 0.0, 0.0);
      Line element1 = line1;
      element1.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity8");
      this.Children.Add((UIElement) element1);
      Line line2 = new Line();
      line2.X1 = 0.0;
      line2.X2 = 1.0;
      line2.Stretch = Stretch.Fill;
      line2.StrokeThickness = 1.0;
      line2.VerticalAlignment = VerticalAlignment.Bottom;
      Line element2 = line2;
      element2.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity8");
      this.Children.Add((UIElement) element2);
      Line line3 = new Line();
      line3.Y1 = 0.0;
      line3.Y2 = 1.0;
      line3.Stretch = Stretch.Fill;
      line3.StrokeThickness = 1.0;
      line3.HorizontalAlignment = HorizontalAlignment.Left;
      line3.Margin = new Thickness(60.0, 40.0, 0.0, 0.0);
      Line element3 = line3;
      element3.SetValue(Grid.RowSpanProperty, (object) 2);
      element3.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity8");
      this.Children.Add((UIElement) element3);
      GridSplitter gridSplitter = new GridSplitter();
      gridSplitter.Height = 8.0;
      gridSplitter.Background = (Brush) Brushes.Transparent;
      gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
      gridSplitter.VerticalAlignment = VerticalAlignment.Bottom;
      gridSplitter.IsTabStop = false;
      gridSplitter.FocusVisualStyle = (Style) null;
      this._splitter = gridSplitter;
      this._splitter.DragCompleted += new DragCompletedEventHandler(this.OnSplitDragCompleted);
      this._splitter.SetValue(Panel.ZIndexProperty, (object) 16);
      this.Children.Add((UIElement) this._splitter);
      this.InitAddDayView();
      this.InitTimelineView();
      this._linesGrid.SetValue(Grid.RowSpanProperty, (object) 2);
      this.Children.Add((UIElement) this._linesGrid);
      TextBlock textBlock = new TextBlock();
      textBlock.HorizontalAlignment = HorizontalAlignment.Left;
      textBlock.VerticalAlignment = VerticalAlignment.Top;
      textBlock.Margin = new Thickness(0.0, 20.0, 0.0, 0.0);
      textBlock.Width = 60.0;
      textBlock.TextAlignment = TextAlignment.Center;
      textBlock.FontSize = 10.0;
      this._weekNum = textBlock;
      this._weekNum.SetResourceReference(TextBlock.ForegroundProperty, (object) "BaseColorOpacity60");
      this.Children.Add((UIElement) this._weekNum);
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
      this.InitEvents();
      this.Loaded += (RoutedEventHandler) ((s, e) =>
      {
        this.SetDisplayDays(this.DisplayDays, this._showWeekend);
        this.BindEvents();
      });
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size size = e.PreviousSize;
      double width1 = size.Width;
      size = e.NewSize;
      double width2 = size.Width;
      if (Math.Abs(width1 - width2) > 1.0 && !Utils.IsEmptyDate(this._model?.StartDate))
        this.SetDisplayDays(this.DisplayDays, this._showWeekend);
      double newSizeHeight = e.NewSize.Height * 0.5;
      if (Math.Abs(newSizeHeight - this._firstRow.MaxHeight) <= 4.0)
        return;
      this._firstRow.MaxHeight = newSizeHeight;
      this._alldayView?.SetScrollMaxHeight(newSizeHeight);
    }

    public void SetDisplayDays(int days, bool showWeekend = true)
    {
      bool flag = this.DisplayDays != days;
      this.DisplayDays = days;
      this._showWeekend = showWeekend;
      if (this.ActualWidth < 62.0)
        return;
      this.DayViewWidth = Math.Max((this.ActualWidth - 60.0) / (double) days, 0.0);
      this.SetDayLines();
      this._timelineView.SetDisplayDays();
      this._alldayView.SetDisplayDays();
      this._model = WeekGridViewModel.Build(this._model.StartDate, this.DisplayDays, showWeekend);
      for (int index = 0; index < this._vLines.Count; ++index)
        this._vLines[index].Date = this._model.Models.Count <= index ? this._model.StartDate.AddDays((double) index) : this._model.Models[index].Date;
      this._alldayView.SetData(this._model.Models);
      if (!flag)
        return;
      this.LoadAllDayEvents();
      this.LoadPointEvents();
    }

    private void SetDayLines()
    {
      int num1 = this.DisplayDays + 2;
      if (num1 > this._vLines.Count)
      {
        int count = this._vLines.Count;
        for (int index = 0; index < num1 - count; ++index)
        {
          MultiDayLine element = new MultiDayLine();
          this._vLines.Add(element);
          this._linesGrid.Children.Add((UIElement) element);
        }
      }
      else
      {
        List<MultiDayLine> multiDayLineList = new List<MultiDayLine>();
        for (int index = num1; index < this._vLines.Count; ++index)
          multiDayLineList.Add(this._vLines[index]);
        multiDayLineList.ForEach((Action<MultiDayLine>) (l =>
        {
          this._vLines.Remove(l);
          this._linesGrid.Children.Remove((UIElement) l);
        }));
      }
      for (int index = 0; index < this._vLines.Count; ++index)
      {
        MultiDayLine vLine = this._vLines[index];
        double num2 = (double) (index - 1) * this.DayViewWidth;
        vLine.RenderTransform = (Transform) new TranslateTransform()
        {
          X = num2
        };
        vLine.Tag = (object) num2;
        vLine.Width = this.DayViewWidth;
      }
    }

    private void UnbindEvents()
    {
      CalendarEventChangeNotifier.TitleChanged -= new EventHandler<TextExtra>(this.NotifyEventTitleChanged);
      DataChangedNotifier.TimeFormatChanged -= new EventHandler(this.ReloadOnTimeFormatChanged);
      LocalSettings.Settings.PropertyChanged -= new PropertyChangedEventHandler(this.OnSettingsChanged);
      this._timelineView.ScrollChanged -= new ScrollChangedEventHandler(this.OnScrollChanged);
    }

    private void BindEvents()
    {
      CalendarEventChangeNotifier.TitleChanged += new EventHandler<TextExtra>(this.NotifyEventTitleChanged);
      DataChangedNotifier.TimeFormatChanged += new EventHandler(this.ReloadOnTimeFormatChanged);
      LocalSettings.Settings.PropertyChanged += new PropertyChangedEventHandler(this.OnSettingsChanged);
      CalendarTimelineView timelineView = this._timelineView;
      CalendarControl parent = this.GetParent();
      double offset = parent != null ? parent.TimeScrollVerticalOffset : 0.0;
      timelineView.ScrollToVerticalOffset(offset);
      this._timelineView.ScrollChanged += new ScrollChangedEventHandler(this.OnScrollChanged);
    }

    private void TryDelayReload(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.TryReload));
    }

    private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
      CalendarControl parent = this.GetParent();
      if (parent == null)
        return;
      parent.TimeScrollVerticalOffset = e.VerticalOffset;
    }

    public void ScrollToVerticalOffset(double vOffset)
    {
      this._timelineView?.ScrollToVerticalOffset(vOffset);
    }

    public bool OnSelection() => false;

    private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == "StartWeekOfYear" || e.PropertyName == "ShowWeek")
        this.SetWeekNum();
      if (e.PropertyName == "EnableLunar" || e.PropertyName == "EnableHoliday")
      {
        this._model = WeekGridViewModel.Build(this._model.StartDate, this.DisplayDays, this._showWeekend);
        this._alldayView.SetData(this._model.Models);
      }
      if (!(e.PropertyName == "TimeFormat"))
        return;
      this._timelineView.SetPointer();
    }

    private void ReloadOnTimeFormatChanged(object sender, EventArgs e)
    {
      this._timelineView?.ResetTimeLine();
      this.LoadPointEvents();
    }

    private void NotifyEventTitleChanged(object sender, TextExtra extra)
    {
      this._timelineView.NotifyEventTitleChanged(extra.Id, extra.Text);
      if (this._tasks == null || !this._tasks.Any<CalendarDisplayModel>())
        return;
      this._tasks.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (m => m.SourceViewModel.Id == extra.Id && m.SourceViewModel.Type == DisplayType.Event)).ToList<CalendarDisplayModel>().ForEach((Action<CalendarDisplayModel>) (m => m.SourceViewModel.Title = extra.Text));
    }

    public DateTime Today() => this.GoTo(DateTime.Today, true, false);

    public DateTime Next()
    {
      int num = this.DisplayDays + (this._showWeekend ? 0 : 2);
      this.RemoveFlashBorder();
      DateTime startDate = this._model.StartDate.AddDays(num <= 7 || num >= 14 ? (double) num : 7.0);
      this.Load(startDate);
      this._alldayView.ScrollToTop();
      HabitSyncService.SyncHabitCheckInCalendar(startDate);
      this._cellHandling = false;
      return startDate;
    }

    public DateTime Last()
    {
      int num = this.DisplayDays + (this._showWeekend ? 0 : 2);
      this.RemoveFlashBorder();
      DateTime startDate = this._model.StartDate.AddDays((double) (-1 * (num <= 7 || num >= 14 ? num : 7)));
      this.Load(startDate);
      this._alldayView.ScrollToTop();
      HabitSyncService.SyncHabitCheckInCalendar(startDate);
      this._cellHandling = false;
      return startDate;
    }

    public DateTime GoTo(DateTime dateTime, bool checkStart = true, bool isScrollToNow = false)
    {
      bool flag = dateTime == DateTime.Today;
      this.RemoveFlashBorder();
      dateTime = checkStart ? this.GetStartDate(dateTime) : dateTime;
      this.Load(dateTime, flag | isScrollToNow);
      this._alldayView.ScrollToTop();
      HabitSyncService.SyncHabitCheckInCalendar(dateTime);
      this._cellHandling = false;
      return dateTime;
    }

    private DateTime GetStartDate(DateTime dateTime)
    {
      if (this.DisplayDays >= 7 || !this._showWeekend)
        dateTime = Utils.GetWeekStart(dateTime);
      return dateTime;
    }

    public void SetCellHandling(bool value) => this._cellHandling = value;

    private async void TryReload()
    {
      MultiDayView multiDayView = this;
      if (multiDayView._scrolling || !multiDayView.IsVisible)
        return;
      if (multiDayView._loading)
      {
        // ISSUE: explicit non-virtual call
        __nonvirtual (multiDayView.Reload(false, 50, false));
      }
      else
      {
        try
        {
          multiDayView._loading = true;
          await multiDayView.Load(multiDayView._model.StartDate);
        }
        finally
        {
          multiDayView._loading = false;
        }
      }
    }

    public void Reload(bool force = false, int delay = 50, bool setWeekend = false)
    {
      if (this.Visibility != Visibility.Visible)
        return;
      DelayActionHandlerCenter.TryDoAction(this._uid, new EventHandler(this.TryDelayReload), delay);
    }

    private void InitEvents()
    {
      this._timelineView.MoveNextOrLast -= new EventHandler<int>(this.OnMoveNextOrLast);
      this._timelineView.MoveNextOrLast += new EventHandler<int>(this.OnMoveNextOrLast);
      this._alldayView.MoveNextOrLast -= new EventHandler<int>(this.OnMoveNextOrLast);
      this._alldayView.MoveNextOrLast += new EventHandler<int>(this.OnMoveNextOrLast);
    }

    private CalendarControl GetParent()
    {
      if (this._calendarControl == null)
        this._calendarControl = Utils.FindParent<CalendarControl>((DependencyObject) this);
      return this._calendarControl;
    }

    private void InitAddDayView()
    {
      this._alldayView = new MultiDayAllDayView(this);
      this.Children.Add((UIElement) this._alldayView);
    }

    private void InitTimelineView()
    {
      this._timelineView = new CalendarTimelineView(this);
      this._timelineView.SetValue(Grid.RowProperty, (object) 1);
      this.Children.Add((UIElement) this._timelineView);
    }

    private void OnMoveNextOrLast(object sender, int e) => this.MoveDay(e < 0 ? 1 : -1);

    private async Task MoveDay(int addDays)
    {
      if (this._scrolling)
        return;
      this._scrolling = true;
      this.SetStartDate(this._model.StartDate.AddDays((double) addDays));
      double diff = (double) addDays * this.DayViewWidth;
      diff /= 6.0;
      int times = 6;
      while (times > 0)
      {
        this.TouchScroll(-1.0 * diff);
        this._timelineView.OnTouchScroll(-1.0 * diff, this._showWeekend);
        this._alldayView.OnTouchScroll(-1.0 * diff, this._showWeekend);
        --times;
        await Task.Delay(10);
      }
      this._scrolling = false;
      this.Reload(delay: 200);
    }

    public async Task Load(DateTime startDate, bool positionNow = false)
    {
      MultiDayView multiDayView = this;
      if (multiDayView._scrolling)
        return;
      if (PopupStateManager.IsInAdd() || multiDayView._cellHandling)
      {
        await Task.Delay(500);
        if (PopupStateManager.IsInAdd() || multiDayView._cellHandling)
          return;
      }
      multiDayView._scrollLoadedDate.Clear();
      multiDayView._model = WeekGridViewModel.Build(startDate, multiDayView.DisplayDays, multiDayView._showWeekend);
      multiDayView._timelineView.SetTimePointVisible(DateTime.Today >= multiDayView._model.StartDate && DateTime.Today <= multiDayView._model.EndDate);
      DateTime startDate1;
      for (int index = 0; index < multiDayView._vLines.Count; ++index)
      {
        MultiDayLine vLine = multiDayView._vLines[index];
        if (multiDayView._vLines[index].RenderTransform is TranslateTransform renderTransform)
        {
          renderTransform.X = (double) (index - 1) * multiDayView.DayViewWidth;
          vLine.Tag = (object) renderTransform.X;
        }
        if (multiDayView._model.Models.Count > index)
        {
          vLine.Date = multiDayView._model.Models[index].Date;
        }
        else
        {
          MultiDayLine multiDayLine = vLine;
          startDate1 = multiDayView._model.StartDate;
          DateTime dateTime = startDate1.AddDays((double) index);
          multiDayLine.Date = dateTime;
        }
      }
      EventHandler<(DateTime, DateTime)> dateRangeChanged = multiDayView.DateRangeChanged;
      if (dateRangeChanged != null)
        dateRangeChanged((object) null, (multiDayView._model.StartDate, multiDayView._model.EndDate));
      multiDayView.SetWeekNum();
      multiDayView.InitTimeLineBoard();
      multiDayView._alldayView.SetData(multiDayView._model.Models);
      startDate1 = multiDayView._model.StartDate;
      DateTime startDate2 = startDate1.AddDays(-1.0);
      startDate1 = multiDayView._model.StartDate;
      DateTime endDate = startDate1.AddDays((double) (multiDayView.DisplayDays + 2));
      List<CalendarDisplayModel> displayModels = await CalendarDisplayService.GetDisplayModels(startDate2, endDate, true);
      multiDayView._tasks = displayModels;
      for (int waitTimes = 200; multiDayView.ActualWidth == 0.0 && waitTimes > 0; --waitTimes)
        await Task.Delay(5);
      multiDayView.LoadAllDayEvents();
      multiDayView.LoadPointEvents();
      if (multiDayView._cellMoving)
        multiDayView.MovePopup();
      if (!positionNow)
        return;
      multiDayView._timelineView.ScrollToCurrentTime();
    }

    private void LoadPointEvents()
    {
      this._pointVms = new List<CalendarTimelineDayViewModel>();
      if (this._tasks == null)
        this._tasks = new List<CalendarDisplayModel>();
      if (this._tasks != null)
      {
        for (int index = -1; index < this.DisplayDays + 1; ++index)
        {
          DateTime currentDate = this._model.StartDate.AddDays((double) index);
          List<CalendarDisplayModel> source = new List<CalendarDisplayModel>();
          if (this._tasks.Count > 0)
            source = this._tasks.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (task => DateUtils.IsTaskInDay(currentDate, task.DisplayStartDate, task.DisplayDueDate, task.IsAllDay))).ToList<CalendarDisplayModel>();
          List<TaskCellViewModel> taskCellViewModelList = new List<TaskCellViewModel>();
          if (source.Count > 0)
            taskCellViewModelList.AddRange(source.Select<CalendarDisplayModel, TaskCellViewModel>((Func<CalendarDisplayModel, TaskCellViewModel>) (task => TaskCellViewModel.Build(task, currentDate))));
          CalendarTimelineDayViewModel timelineDayViewModel = new CalendarTimelineDayViewModel()
          {
            Date = currentDate,
            Cells = taskCellViewModelList,
            InWeekControl = true
          };
          if (CalendarGeoHelper.TopFolded)
          {
            List<CalendarDisplayModel> topTasks = source.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (task =>
            {
              if (!task.DisplayStartDate.HasValue || task.DisplayStartDate.Value.Hour >= CalendarGeoHelper.GetStartHour() && !(task.DisplayStartDate.Value.Date < currentDate))
                return false;
              return !task.DisplayDueDate.HasValue || (task.DisplayDueDate.Value - currentDate.Date).TotalHours <= (double) CalendarGeoHelper.GetStartHour();
            })).OrderBy<CalendarDisplayModel, DateTime?>((Func<CalendarDisplayModel, DateTime?>) (t => t.DisplayDueDate)).ToList<CalendarDisplayModel>();
            List<string> extraTasks1 = MultiDayView.GetExtraTasks((IReadOnlyList<CalendarDisplayModel>) topTasks);
            timelineDayViewModel.Extra1 = extraTasks1[0];
            timelineDayViewModel.Extra2 = extraTasks1[1];
            timelineDayViewModel.TopTasks = topTasks;
            List<CalendarDisplayModel> list = source.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (task => !topTasks.Contains(task) && task.DisplayStartDate.HasValue && task.DisplayStartDate.Value.Hour >= CalendarGeoHelper.GetEndHour() && task.DisplayStartDate.Value.Date >= currentDate)).OrderBy<CalendarDisplayModel, DateTime?>((Func<CalendarDisplayModel, DateTime?>) (t => t.DisplayStartDate)).ToList<CalendarDisplayModel>();
            List<string> extraTasks2 = MultiDayView.GetExtraTasks((IReadOnlyList<CalendarDisplayModel>) list);
            timelineDayViewModel.Extra3 = extraTasks2[0];
            timelineDayViewModel.Extra4 = extraTasks2[1];
            timelineDayViewModel.BotTasks = list;
          }
          this._pointVms.Add(timelineDayViewModel);
        }
      }
      this.SetCellEvents();
    }

    private static List<string> GetExtraTasks(IReadOnlyList<CalendarDisplayModel> tasks)
    {
      if (tasks.Any<CalendarDisplayModel>())
      {
        int count = tasks.Count;
        switch (count)
        {
          case 1:
            return new List<string>()
            {
              MultiDayView.GetDisplayTitle(tasks[0].DisplayStartDate, tasks[0].Title),
              ""
            };
          case 2:
            return new List<string>()
            {
              MultiDayView.GetDisplayTitle(tasks[0].DisplayStartDate, tasks[0].Title),
              MultiDayView.GetDisplayTitle(tasks[1].DisplayStartDate, tasks[1].Title)
            };
          default:
            if (count > 2)
              return new List<string>()
              {
                MultiDayView.GetDisplayTitle(tasks[0].DisplayStartDate, tasks[0].Title),
                string.Format(Utils.GetString("MoreTasks"), (object) (count - 1).ToString())
              };
            break;
        }
      }
      return new List<string>() { "", "" };
    }

    private static string GetDisplayTitle(DateTime? startDate, string title)
    {
      string empty = string.Empty;
      if (startDate.HasValue)
        empty += DateUtils.FormatHourMinuteText(startDate.Value);
      return empty + " " + title;
    }

    private void SetCellEvents()
    {
      this._timelineView.Setup(this._pointVms, this.DayViewWidth, this._showWeekend);
    }

    private void SetWeekNum()
    {
      if (!LocalSettings.Settings.ShowWeek)
        this._weekNum.Text = string.Empty;
      string weekNumStrOfYear = DateUtils.GetWeekNumStrOfYear(this._model.StartDate);
      if (weekNumStrOfYear == this._weeks)
        return;
      if (this._scrolling && this._mode == "1")
        DelayActionHandlerCenter.TryDoAction("AddWeekScrollAction", (EventHandler) ((sender, args) => UserActCollectUtils.AddClickEvent("calendar", "week_action", "swipe_horizontal")), 3000);
      this._weeks = weekNumStrOfYear;
      if (!LocalSettings.Settings.ShowWeek)
        return;
      this._weekNum.Text = this._weeks;
    }

    public (DateTime, DateTime) GetTimeSpan()
    {
      return (this._model.StartDate, this._model.EndDate.AddDays(1.0));
    }

    public void RemoveItem(CalendarDisplayViewModel model)
    {
      if (model == null)
        return;
      this._tasks.RemoveAll((Predicate<CalendarDisplayModel>) (m => m.Id == model.SourceViewModel.Id && Math.Abs(m.repeatDiff - model.RepeatDiff) < 0.1));
      this.LoadAllDayEvents();
      this.LoadPointEvents();
    }

    public List<CalendarDisplayModel> ExistTasks(HashSet<string> taskIds)
    {
      List<CalendarDisplayModel> tasks = this._tasks;
      return (tasks != null ? tasks.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (model => taskIds.Contains(model.Id))).ToList<CalendarDisplayModel>() : (List<CalendarDisplayModel>) null) ?? new List<CalendarDisplayModel>();
    }

    public void ShowAddWindow(DateTime dateTime) => this._alldayView.TryShowAddWindow(dateTime);

    private void LoadAllDayEvents()
    {
      if (this._tasks == null)
        return;
      this._assembles = GridGeoAssembler.AssemblyMultiDayEvents(this._tasks.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (t =>
      {
        if (!GridGeoAssembler.IsValidSpanModel(t, true))
        {
          bool? isAllDay = t.IsAllDay;
          if (isAllDay.HasValue)
          {
            isAllDay = t.IsAllDay;
            if (isAllDay.Value)
              goto label_6;
          }
          DateTime? nullable = t.CompletedTime;
          if (!nullable.HasValue)
            return false;
          nullable = t.StartDate;
          return !nullable.HasValue;
        }
label_6:
        return true;
      })).ToList<CalendarDisplayModel>(), this._model.StartDate, this._showWeekend).ToList<CalendarDisplayViewModel>();
      this._allDayVms = this._assembles.Select<CalendarDisplayViewModel, WeekEventModel>((Func<CalendarDisplayViewModel, WeekEventModel>) (item => new WeekEventModel()
      {
        Row = item.Row,
        Column = item.Column,
        ColumnSpan = item.ColumnSpan,
        Data = item,
        TargetDate = this._model.StartDate
      })).ToList<WeekEventModel>();
      if (LocalSettings.Settings.WeekAllDayHeight == 0.0)
      {
        int num = 0;
        foreach (CalendarDisplayViewModel assemble in this._assembles)
        {
          if (assemble.Row > num && num < 6)
            num = assemble.Row;
          LocalSettings.Settings.WeekAllDayHeight = (double) (63 + (num + 1) * 20);
          this.GetParent()?.NotifyAllDayHeightChanged();
        }
      }
      this._alldayView.ScrollStartDate = this._model.StartDate;
      this.SetAllDayEvents();
    }

    private void SetAllDayEvents() => this._alldayView.SetEvents(this._allDayVms);

    public void Resize()
    {
      this.LoadAllDayEvents();
      this._timelineView.Changed();
    }

    private void OnSplitDragCompleted(object sender, DragCompletedEventArgs e)
    {
      LocalSettings.Settings.WeekAllDayHeight = this._firstRow.ActualHeight;
      this.GetParent()?.NotifyAllDayHeightChanged();
    }

    public void PrintWeekCalendar(bool isAll)
    {
      new PrintPreviewWindow(this.GetPrintAllDayModel(isAll), this._pointVms.Where<CalendarTimelineDayViewModel>((Func<CalendarTimelineDayViewModel, bool>) (d => d.Date >= this._model.StartDate && d.Date <= this._model.EndDate)).Select<CalendarTimelineDayViewModel, CalendarTimelineDayViewModel>(new Func<CalendarTimelineDayViewModel, CalendarTimelineDayViewModel>(CalendarTimelineDayViewModel.Copy)).ToList<CalendarTimelineDayViewModel>(), this._model.StartDate, this.DisplayDays + (this._showWeekend ? 0 : 2)).Show();
    }

    private List<WeekEventModel> GetPrintAllDayModel(bool isAll)
    {
      List<WeekEventModel> list = GridGeoAssembler.AssemblyMultiDayEvents(this._tasks.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (t =>
      {
        if (!GridGeoAssembler.IsValidSpanModel(t, true))
        {
          bool? isAllDay = t.IsAllDay;
          if (isAllDay.HasValue)
          {
            isAllDay = t.IsAllDay;
            if (isAllDay.Value)
              goto label_6;
          }
          DateTime? nullable = t.CompletedTime;
          if (!nullable.HasValue)
            return false;
          nullable = t.StartDate;
          return !nullable.HasValue;
        }
label_6:
        return true;
      })).ToList<CalendarDisplayModel>(), this._model.StartDate, true).ToList<CalendarDisplayViewModel>().Where<CalendarDisplayViewModel>((Func<CalendarDisplayViewModel, bool>) (item => item.Row <= 8 | isAll)).Select<CalendarDisplayViewModel, WeekEventModel>((Func<CalendarDisplayViewModel, WeekEventModel>) (item => new WeekEventModel()
      {
        Row = item.Row,
        Column = item.Column,
        ColumnSpan = item.ColumnSpan,
        Data = item
      })).ToList<WeekEventModel>();
      if (!isAll)
      {
        for (int j = 0; j < 7; j++)
        {
          CalendarDisplayViewModel displayViewModel = this._assembles.Where<CalendarDisplayViewModel>((Func<CalendarDisplayViewModel, bool>) (data => data.Column == j)).OrderByDescending<CalendarDisplayViewModel, int>((Func<CalendarDisplayViewModel, int>) (task => task.Row)).FirstOrDefault<CalendarDisplayViewModel>();
          if (displayViewModel != null)
          {
            int row = displayViewModel.Row;
            if (row > 9)
              list.Add(new WeekEventModel()
              {
                Column = j,
                Row = 10,
                ColumnSpan = 1,
                Data = new CalendarDisplayViewModel()
                {
                  Column = j,
                  Row = 9,
                  ColumnSpan = 1,
                  IsLoadMore = true,
                  SourceViewModel = new TaskBaseViewModel()
                  {
                    Type = DisplayType.LoadMore,
                    Title = string.Format(Utils.GetString("MoreItems"), (object) (row - 9))
                  }
                }
              });
          }
        }
      }
      return list;
    }

    public async Task ReloadPointEvent()
    {
      this.LoadPointEvents();
      this.SetCellEvents();
    }

    public void ClearItems(bool onlyData = false)
    {
      this._alldayView.ClearItems(onlyData);
      this._timelineView.ClearItems(onlyData);
      this._allDayVms?.Clear();
      this._assembles?.Clear();
      this._pointVms?.Clear();
      this._tasks?.Clear();
    }

    public void OnShowWeekChanged(bool? showWeekend = null)
    {
      if (showWeekend.HasValue)
        this.SetDisplayDays(showWeekend.Value ? 7 : 5, showWeekend.Value);
      this.LoadAllDayEvents();
      this.LoadPointEvents();
    }

    public void OnDragStart(CalendarDisplayViewModel model, MouseEventArgs e, bool fromArrange)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      this.InitDragPopup();
      this._dragFromArrange = fromArrange;
      this._dragStartY = (double) CalendarGeoHelper.GetStartHour() * CalendarGeoHelper.HourHeight * -1.0 + 6.0;
      UtilLog.Info("TBDSMDV:" + this._cellMoving.ToString());
      if (this._cellMoving)
        return;
      TaskCellViewModel taskCellViewModel = new TaskCellViewModel();
      taskCellViewModel.RepeatDiff = model.RepeatDiff;
      taskCellViewModel.Color = model.Color;
      taskCellViewModel.Height = 20.0;
      taskCellViewModel.Width = this.DayViewWidth - 8.0;
      taskCellViewModel.BarMode = true;
      taskCellViewModel.BaseOnStart = true;
      taskCellViewModel.Icon = model.Icon;
      TaskCellViewModel cellModel = taskCellViewModel;
      cellModel.SetSourceModel(model.SourceViewModel.Copy());
      this.StartDrag(e, cellModel);
      this._dragCellModel = cellModel;
      this._dragDueTime = new DateTime?();
      DateTime? displayStartDate = cellModel.DisplayStartDate;
      ref DateTime? local = ref displayStartDate;
      this._dragStartTime = local.HasValue ? new DateTime?(local.GetValueOrDefault().Date) : new DateTime?();
      this.RegisterPanelDrag();
      this._cellMoving = true;
    }

    public void OnDragStart(TaskCellViewModel model, MouseEventArgs e, double startHeight)
    {
      if (e.LeftButton != MouseButtonState.Pressed)
        return;
      this.InitDragPopup();
      this._dragFromArrange = false;
      this._dragStartY = startHeight;
      UtilLog.Info("TCDSMDV:" + this._cellMoving.ToString());
      if (this._cellMoving)
        return;
      TaskCellViewModel taskCellViewModel = new TaskCellViewModel();
      taskCellViewModel.RepeatDiff = model.RepeatDiff;
      taskCellViewModel.Color = model.Color;
      taskCellViewModel.Height = model.Height;
      taskCellViewModel.Width = this.DayViewWidth - 8.0;
      taskCellViewModel.Icon = model.Icon;
      TaskCellViewModel cellModel = taskCellViewModel;
      cellModel.SetSourceModel(model.SourceViewModel.Copy());
      cellModel.BaseOnStart = model.BaseOnStart;
      cellModel.DragMode = true;
      this.StartDrag(e, cellModel);
      this._dragCellModel = cellModel;
      this._dragDueTime = cellModel.DisplayDueDate;
      this._dragStartTime = cellModel.DisplayStartDate;
      this._timelineView.ShowMoveCell(this._dragCellModel, true);
      this.RegisterPanelDrag();
      this._cellMoving = true;
    }

    private void InitDragPopup()
    {
      if (this._dragPopup != null)
        return;
      Border border = new Border();
      border.HorizontalAlignment = HorizontalAlignment.Left;
      border.VerticalAlignment = VerticalAlignment.Top;
      this._dragPopup = border;
      StackPanel stackPanel = new StackPanel();
      this._dragPopup.Child = (UIElement) stackPanel;
      Path path = new Path();
      path.Data = Geometry.Parse("M 6 0 0 6 L 12 6 Z");
      path.HorizontalAlignment = HorizontalAlignment.Center;
      path.Visibility = Visibility.Collapsed;
      this._popTriangle = path;
      this._popTriangle.SetBinding(Shape.FillProperty, "Color");
      TaskDragCell taskDragCell = new TaskDragCell();
      taskDragCell.HorizontalAlignment = HorizontalAlignment.Stretch;
      this._dragCell = taskDragCell;
      stackPanel.Children.Add((UIElement) this._popTriangle);
      stackPanel.Children.Add((UIElement) this._dragCell);
      this._dragPopup.Visibility = Visibility.Visible;
      this._calendarControl?.AddPopupBorder(this._dragPopup);
    }

    private void RegisterPanelDrag()
    {
      UtilLog.Info("MultiDayViewDragStart");
      Window parentWindow = this.GetParentWindow();
      parentWindow.CaptureMouse();
      parentWindow.MouseMove -= new MouseEventHandler(this.OnMouseMove);
      parentWindow.MouseMove += new MouseEventHandler(this.OnMouseMove);
      parentWindow.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnDrop);
      parentWindow.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDrop);
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (!this.IsVisible)
        return;
      this.TryScrollOnEdge(e);
      this.MovePopup();
      CalendarControl parent = this.GetParent();
      if ((parent != null ? (parent.CalendarSide.CheckHoverDayCell(10.0) ? 1 : 0) : 0) != 0)
      {
        if (this._popTriangle.Visibility != Visibility.Collapsed)
          return;
        this._popTriangle.Visibility = Visibility.Visible;
      }
      else
      {
        if (this._popTriangle.Visibility != Visibility.Visible)
          return;
        this._popTriangle.Visibility = Visibility.Collapsed;
      }
    }

    private void MovePopup()
    {
      TaskCellViewModel dragCellModel = this._dragCellModel;
      if (dragCellModel == null)
        return;
      System.Windows.Point position = Mouse.GetPosition((IInputElement) this);
      if (!this._dragFromArrange && dragCellModel.IsTaskOrNote)
        this.GetParent()?.ArrangePanel?.OnPopupMove(position, this.ActualWidth);
      double x = position.X;
      double? actualWidth = this.GetParent()?.ActualWidth;
      double valueOrDefault = actualWidth.GetValueOrDefault();
      if (x > valueOrDefault & actualWidth.HasValue || position.X < 0.0 || position.Y > this.ActualHeight || position.Y < 0.0)
      {
        this._dragPopup.Visibility = Visibility.Collapsed;
        Mouse.OverrideCursor = (Cursor) null;
      }
      else
      {
        Mouse.OverrideCursor = Cursors.Hand;
        if (!this._cellMoving || this.Visibility != Visibility.Visible || Mouse.LeftButton != MouseButtonState.Pressed)
          return;
        if (position.X > this.ActualWidth)
        {
          this._dragPopup.Visibility = Visibility.Visible;
          this._timelineView.RemoveAddCell();
          this._dragPopup.Width = 160.0;
          this._dragPopup.Margin = new Thickness(position.X - 80.0, position.Y - 10.0, 0.0, 0.0);
        }
        else
        {
          this._dragPopup.Width = double.NaN;
          if (Mouse.GetPosition((IInputElement) this._alldayView).Y - this._alldayView.ActualHeight > -12.0)
          {
            this._dragPopup.Visibility = Visibility.Collapsed;
            double verticalOffset = this.GetVerticalOffset(position.Y - this._dragStartY + CalendarGeoHelper.QuarterHourHeight);
            DateTime hoverDate = this._timelineView.GetHoverDate(position.X);
            if (Math.Abs(dragCellModel.VerticalOffset - verticalOffset) <= 0.001)
              return;
            this.SetDateOnPopMove(verticalOffset, hoverDate);
            this._timelineView.ShowMoveCell(dragCellModel);
          }
          else
          {
            if (!this._dragPopup.IsVisible)
              UtilLog.Info("MovePopup:RemoveAddCell");
            this._dragPopup.Visibility = Visibility.Visible;
            this._timelineView.RemoveAddCell();
            this._dragCell.Width = this.DayViewWidth;
            this._dragPopup.Margin = new Thickness(CalendarGeoHelper.CalHorizontalOffset(position.X, this.DayViewWidth) - 1.0, position.Y, 0.0, 0.0);
          }
        }
      }
    }

    private void SetDateOnPopMove(double vOffset, DateTime hoverDate)
    {
      int num1 = CalendarGeoHelper.TranslateMinute(vOffset - this._alldayView.ActualHeight + this._timelineView.VerticalOffset);
      int num2 = 0;
      DateTime? nullable1 = this._dragCellModel.BarMode ? new DateTime?() : this._dragCellModel.DueDate;
      DateTime? nullable2;
      if (nullable1.HasValue)
      {
        nullable2 = this._dragCellModel.StartDate;
        if (nullable2.HasValue)
        {
          DateTime dateTime1 = nullable1.Value;
          nullable2 = this._dragCellModel.StartDate;
          DateTime dateTime2 = nullable2.Value;
          num2 = Math.Max((int) (dateTime1 - dateTime2).TotalMinutes, 0);
        }
      }
      if (this._dragDueTime.HasValue && !this._dragCellModel.BaseOnStart)
      {
        DateTime dateTime = hoverDate.AddDays(-1.0 * this._dragCellModel.RepeatDiff).AddHours((double) this._dragDueTime.Value.Hour).AddMinutes((double) (this._dragDueTime.Value.Minute + num1));
        if (CalendarGeoHelper.GetStartHourForTask() > 0)
          dateTime = dateTime.AddHours(-1.0);
        this._dragCellModel.SourceViewModel.DueDate = new DateTime?(dateTime);
        nullable2 = this._dragCellModel.StartDate;
        if (!nullable2.HasValue || num2 == 0)
          return;
        this._dragCellModel.SourceViewModel.StartDate = new DateTime?(dateTime.AddMinutes((double) (-1 * num2)));
      }
      else
      {
        int num3 = CalendarGeoHelper.TopFolded ? CalendarGeoHelper.GetStartHourForTask() + 1 : 0;
        int num4;
        if (this._dragStartTime.HasValue)
        {
          DateTime dateTime = this._dragStartTime.Value;
          if (dateTime.Hour < num3)
          {
            dateTime = this._dragStartTime.Value;
            int num5 = (dateTime.Hour - 1) * 60;
            dateTime = this._dragStartTime.Value;
            int minute = dateTime.Minute;
            num4 = num5 + minute;
            goto label_12;
          }
        }
        num4 = !this._dragStartTime.HasValue ? (CalendarGeoHelper.GetStartHourForTask() > 0 ? -60 : 0) : CalendarGeoHelper.GetStartHourForTask() * 60;
label_12:
        DateTime dateTime3 = hoverDate.AddMinutes((double) (num1 + num4));
        if (num3 > 0 && dateTime3.Hour < num3)
          dateTime3 = hoverDate.AddHours((double) num3);
        dateTime3 = dateTime3.AddDays(-1.0 * this._dragCellModel.RepeatDiff);
        this._dragCellModel.SourceViewModel.StartDate = new DateTime?(dateTime3);
        nullable2 = this._dragCellModel.StartDate;
        if (nullable2.HasValue && num2 != 0)
        {
          this._dragCellModel.SourceViewModel.DueDate = new DateTime?(dateTime3.AddMinutes((double) num2));
        }
        else
        {
          TaskBaseViewModel sourceViewModel = this._dragCellModel.SourceViewModel;
          nullable2 = new DateTime?();
          DateTime? nullable3 = nullable2;
          sourceViewModel.DueDate = nullable3;
        }
      }
    }

    private double GetVerticalOffset(double currentY)
    {
      currentY -= this._alldayView.ActualHeight;
      currentY += this._timelineView.VerticalOffset;
      currentY += CalendarGeoHelper.GetTopFoldDiff();
      return CalendarGeoHelper.GetRoundOffset(currentY) + this._alldayView.ActualHeight - this._timelineView.VerticalOffset - CalendarGeoHelper.QuarterHourHeight;
    }

    private void TryScrollOnEdge(MouseEventArgs e)
    {
      if (!this._cellMoving)
        return;
      this._timelineView.TryScrollOnEdge(e);
    }

    private async void OnDrop(object sender, MouseEventArgs e)
    {
      MultiDayView element = this;
      UtilLog.Info(string.Format("TryDropCell {0},{1},{2}", (object) (element._dragCellModel == null), (object) element._cellMoving, (object) (element._calendarControl == null)));
      if (sender is Window window)
      {
        window.MouseLeftButtonUp -= new MouseButtonEventHandler(element.OnDrop);
        window.MouseMove -= new MouseEventHandler(element.OnMouseMove);
        window.ReleaseMouseCapture();
      }
      Mouse.OverrideCursor = (Cursor) null;
      PopupStateManager.StopSelection();
      CalendarControl parent = element.GetParent();
      object obj = (object) null;
      int num = 0;
      try
      {
        if (element._cellMoving && parent != null)
        {
          if (element._dragCellModel != null)
          {
            TaskCellViewModel model = element._dragCellModel;
            if (Utils.IfCtrlPressed())
            {
              string str = await parent.TryCopy((CalendarDisplayViewModel) model);
              element._copyId = str;
            }
            model = element.TryAddCopyModel(model);
            DateTime? hoverDate1 = parent.CalendarSide.DayPicker.GetHoverDate();
            parent.CalendarSide.DayPicker.ClearHoverCell();
            TimeDataModel original = (TimeDataModel) element._dragCell.Tag;
            bool flag = Utils.IsMouseOver(e, (FrameworkElement) element);
            UtilLog.Info(string.Format("DropCell {0},{1},{2}{3}", (object) flag, (object) element._dragPopup.IsVisible, (object) original.StartDate.HasValue, (object) hoverDate1.HasValue));
            if (hoverDate1.HasValue && original.StartDate.HasValue)
            {
              int num1 = 0;
              if (original.DueDate.HasValue)
                num1 = (int) (original.DueDate.Value - original.StartDate.Value).TotalMinutes;
              TaskBaseViewModel sourceViewModel = model.SourceViewModel;
              DateTime original1 = original.StartDate.Value;
              DateTime date = hoverDate1.Value;
              date = date.Date;
              DateTime modify = date.AddDays(-1.0 * model.RepeatDiff);
              DateTime? nullable = new DateTime?(DateUtils.SetDateOnly(original1, modify));
              sourceViewModel.StartDate = nullable;
              model.SourceViewModel.IsAllDay = original.IsAllDay;
              model.SourceViewModel.DueDate = new DateTime?(model.SourceViewModel.StartDate.Value.AddMinutes((double) num1));
            }
            else if (flag && element._dragPopup.IsVisible)
            {
              DateTime hoverDate2 = element._timelineView.GetHoverDate(element._dragPopup.Margin.Left + 12.0);
              if (model.BarMode && original.StartDate.HasValue)
              {
                int num2 = 0;
                if (original.DueDate.HasValue)
                  num2 = (int) (original.DueDate.Value - original.StartDate.Value).TotalMinutes;
                model.SourceViewModel.StartDate = new DateTime?(DateUtils.SetDateOnly(original.StartDate.Value, hoverDate2.Date.AddDays(-1.0 * model.RepeatDiff)));
                model.SourceViewModel.IsAllDay = original.IsAllDay;
                model.SourceViewModel.DueDate = new DateTime?(model.SourceViewModel.StartDate.Value.AddMinutes((double) num2));
              }
              else
              {
                model.SourceViewModel.StartDate = new DateTime?(hoverDate2.AddDays(-1.0 * model.RepeatDiff));
                model.SourceViewModel.DueDate = new DateTime?();
                model.SourceViewModel.IsAllDay = new bool?(true);
              }
            }
            if ((flag ? 1 : (hoverDate1.HasValue ? 1 : 0)) != 0)
            {
              if (model.IsCalendarEvent)
                await element.NotifyEventDrop(model);
              else
                await element.NotifyTaskOrItemDrop(model);
            }
            if (model.IsTaskOrNote && !element._dragFromArrange)
              await parent.ArrangePanel.OnDragDrop(original);
            element._dragCellModel = (TaskCellViewModel) null;
            model = (TaskCellViewModel) null;
            original = (TimeDataModel) null;
          }
        }
        else
          num = 1;
      }
      catch (object ex)
      {
        obj = ex;
      }
      element._cellMoving = false;
      if (element._dragFromArrange)
      {
        element._dragFromArrange = false;
        await Task.Delay(200);
        parent?.ArrangePanel.NotifyModelDrop();
      }
      element.LoadAllDayEvents();
      element.LoadPointEvents();
      element.GetParent()?.SetEditting(false);
      element._dragPopup.Visibility = Visibility.Collapsed;
      element._timelineView.RemoveAddCell();
      object obj1 = obj;
      if (obj1 != null)
      {
        if (!(obj1 is Exception source))
          throw obj1;
        ExceptionDispatchInfo.Capture(source).Throw();
      }
      if (num == 1)
      {
        parent = (CalendarControl) null;
      }
      else
      {
        obj = (object) null;
        parent = (CalendarControl) null;
      }
    }

    private TaskCellViewModel TryAddCopyModel(TaskCellViewModel model)
    {
      if (string.IsNullOrEmpty(this._copyId))
        return model;
      TaskCellViewModel taskCellViewModel = TaskCellViewModel.Copy(model);
      TimeDataModel tag = (TimeDataModel) this._dragCell.Tag;
      if (model.IsCalendarEvent)
      {
        taskCellViewModel.SourceViewModel.Id = this._copyId;
        tag.EventId = this._copyId;
      }
      else if (!string.IsNullOrEmpty(model.ItemId))
      {
        taskCellViewModel.SourceViewModel = TaskDetailItemCache.GetCheckItemById(this._copyId);
        tag.ItemId = this._copyId;
      }
      else
      {
        taskCellViewModel.SourceViewModel = TaskCache.GetTaskById(this._copyId);
        tag.TaskId = this._copyId;
      }
      taskCellViewModel.SourceViewModel.StartDate = model.DisplayStartDate;
      taskCellViewModel.SourceViewModel.DueDate = model.DisplayDueDate;
      taskCellViewModel.SourceViewModel.IsAllDay = model.SourceViewModel.IsAllDay;
      taskCellViewModel.SourceViewModel.RepeatFlag = (string) null;
      taskCellViewModel.SourceViewModel.ExDates = (string) null;
      this._copyId = (string) null;
      return taskCellViewModel;
    }

    private async Task NotifyEventDrop(TaskCellViewModel model)
    {
      TimeDataModel delta = new TimeDataModel()
      {
        TaskId = model.GetTaskId(),
        ItemId = model.ItemId,
        EventId = model.EventId,
        IsAllDay = model.IsAllDay,
        StartDate = model.StartDate,
        DueDate = model.DueDate,
        RepeatFlag = model.RepeatFlag,
        RepeatFrom = model.RepeatFrom
      };
      TimeDataModel tag = (TimeDataModel) this._dragCell.Tag;
      TimeDataModel mergedData = CalendarService.GetMergedData(tag, delta);
      bool? isAllDay = mergedData.IsAllDay;
      if (isAllDay.HasValue)
      {
        isAllDay = mergedData.IsAllDay;
        if (!isAllDay.Value && !mergedData.DueDate.HasValue && mergedData.StartDate.HasValue)
          mergedData.DueDate = new DateTime?(mergedData.StartDate.Value.AddHours(1.0));
      }
      TimeData time = new TimeData()
      {
        StartDate = mergedData.StartDate,
        DueDate = mergedData.DueDate,
        IsAllDay = mergedData.IsAllDay,
        RepeatFlag = tag.RepeatFlag
      };
      AllDayChangeState allDayChangeState = AllDayChangeState.NotChanged;
      isAllDay = tag.IsAllDay;
      bool flag1 = ((int) isAllDay ?? 1) != 0;
      isAllDay = delta.IsAllDay;
      bool flag2 = ((int) isAllDay ?? 1) != 0;
      if (flag1 != flag2)
        allDayChangeState = flag1 ? AllDayChangeState.AllDay2Point : AllDayChangeState.Point2AllDay;
      if (allDayChangeState != AllDayChangeState.NotChanged)
        time.Reminders = allDayChangeState == AllDayChangeState.Point2AllDay ? TimeData.GetDefaultAllDayReminders() : TimeData.GetDefaultTimeReminders();
      await CalendarService.SaveEventTime(model.EventId, time);
    }

    private async Task NotifyTaskOrItemDrop(TaskCellViewModel model)
    {
      TimeDataModel delta = new TimeDataModel()
      {
        TaskId = model.GetTaskId(),
        ItemId = model.ItemId,
        IsAllDay = model.IsAllDay,
        StartDate = model.DisplayStartDate,
        DueDate = model.DisplayDueDate,
        RepeatFlag = model.RepeatFlag,
        RepeatFrom = model.RepeatFrom
      };
      TimeDataModel original = (TimeDataModel) this._dragCell.Tag;
      TimeDataModel merged = CalendarService.GetMergedData(original, delta);
      if (!string.IsNullOrEmpty(model.RepeatFlag) && model.StartDate.HasValue)
      {
        TaskModel task = await TaskDao.GetThinTaskById(model.GetTaskId());
        if (task != null && model.StartDate.HasValue && task.startDate.HasValue)
        {
          List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task.id);
          TimeData reviseData = new TimeData()
          {
            StartDate = merged.StartDate,
            DueDate = merged.DueDate,
            IsAllDay = merged.IsAllDay,
            Reminders = remindersByTaskId,
            RepeatFrom = model.RepeatFrom,
            RepeatFlag = model.RepeatFlag,
            ExDates = task.exDate != null ? ((IEnumerable<string>) task.exDate).ToList<string>() : new List<string>(),
            TimeZone = new TimeZoneViewModel(task.isFloating.GetValueOrDefault(), task.timeZone)
          };
          if (await ModifyRepeatHandler.TryUpdateDueDate(model.GetTaskId(), original.StartDate, original.DueDate, reviseData, 1, 0))
          {
            delta = (TimeDataModel) null;
            original = (TimeDataModel) null;
            merged = (TimeDataModel) null;
            return;
          }
          this._alldayView.ClearDragging();
          delta = (TimeDataModel) null;
          original = (TimeDataModel) null;
          merged = (TimeDataModel) null;
          return;
        }
        task = (TaskModel) null;
      }
      if (model.StartDate.HasValue)
      {
        TimeData changedData = TaskService.GetChangedData(original.StartDate, original.DueDate, original.IsAllDay, model.StartDate.Value, original.RepeatFlag, original.RepeatFrom);
        original.RepeatFlag = changedData.RepeatFlag;
        original.RepeatFrom = changedData.RepeatFrom;
      }
      if (!string.IsNullOrEmpty(original.ItemId))
        await CalendarService.NotifyCheckItemDrop(delta, model);
      else
        await CalendarService.NotifyTaskDrop(original, model, merged);
      CalendarControl parent = this.GetParent();
      if (parent == null)
      {
        delta = (TimeDataModel) null;
        original = (TimeDataModel) null;
        merged = (TimeDataModel) null;
      }
      else
      {
        parent.ArrangePanel.NotifyModelChanged();
        delta = (TimeDataModel) null;
        original = (TimeDataModel) null;
        merged = (TimeDataModel) null;
      }
    }

    private Window GetParentWindow()
    {
      this._parentWindow = this._parentWindow ?? Window.GetWindow((DependencyObject) this);
      return this._parentWindow;
    }

    private void StartDrag(MouseEventArgs arg, TaskCellViewModel cellModel)
    {
      this._dragPopup.DataContext = (object) cellModel;
      this.GetParent()?.SetEditting(true);
      this._dragCell.Tag = (object) new TimeDataModel()
      {
        TaskId = cellModel.GetTaskId(),
        ItemId = cellModel.ItemId,
        EventId = cellModel.EventId,
        StartDate = cellModel.DisplayStartDate,
        DueDate = cellModel.DisplayDueDate,
        IsAllDay = cellModel.IsAllDay,
        RepeatFlag = cellModel.RepeatFlag,
        RepeatFrom = cellModel.RepeatFrom
      };
    }

    public void InitTimeLineBoard() => this._timelineView?.InitBoard();

    public void TryShowTooltip()
    {
    }

    public void SetFirstRowHeight()
    {
      this._firstRow.Height = new GridLength(LocalSettings.Settings.WeekAllDayHeight);
    }

    public void SetEditing(bool editing) => this._calendarControl?.SetEditting(editing);

    private async void OnStopInputScroll()
    {
      if (!this._scrolling)
        return;
      if (this._vLines.Count > 1)
      {
        double diff = this._vLines[1].Offset;
        if (Math.Abs(diff) < 0.5)
        {
          this._scrolling = false;
          this.Reload(delay: 10);
          return;
        }
        if (Math.Abs(diff) > 20.0)
          diff = diff >= 0.0 ? (this._leftScroll ? diff : diff - this.DayViewWidth) : (this._leftScroll ? diff + this.DayViewWidth : diff);
        diff /= -10.0;
        int times = 10;
        while (times > 0)
        {
          this.TouchScroll(diff);
          this._timelineView.OnTouchScroll(diff, this._showWeekend);
          this._alldayView.OnTouchScroll(diff, this._showWeekend);
          --times;
          await Task.Delay(10);
        }
        DateTime start = this._alldayView.GetStartDate() ?? this._model.StartDate;
        if (start != this._model.StartDate)
          this.SetStartDate(start);
      }
      this._scrolling = false;
      this.Reload(delay: 10);
    }

    public void OnTouchScroll(double offset)
    {
      if (!this.IsMouseOver || this._loading)
        return;
      if (!this._scrolling)
        this._scrolling = true;
      this.RemoveFlashBorder();
      this.OnDoubleFingerTouch();
      this._leftScroll = offset < 0.0;
      this.TouchScroll(offset);
      this._timelineView.OnTouchScroll(offset, this._showWeekend);
      this._alldayView.OnTouchScroll(offset, this._showWeekend);
      DateTime start = this._alldayView.GetStartDate() ?? this._model.StartDate;
      if (start != this._model.StartDate)
        this.SetStartDate(start);
      if (!this._cellMoving)
        return;
      this.MovePopup();
    }

    private void TouchScroll(double offset)
    {
      TaskDetailWindow.TryCloseWindow();
      bool flag = false;
      for (int index = 0; index < this._vLines.Count; ++index)
      {
        MultiDayLine vLine = this._vLines[index];
        if (vLine.RenderTransform is TranslateTransform renderTransform)
        {
          double num1 = renderTransform.X + offset;
          renderTransform.X = num1;
          DateTime date;
          if (renderTransform.X >= ((double) this.DisplayDays + 0.5) * this.DayViewWidth)
          {
            flag = true;
            renderTransform.X -= (double) (this.DisplayDays + 2) * this.DayViewWidth;
            date = vLine.Date;
            DateTime dateTime = date.AddDays((double) (-2 - this.DisplayDays));
            if (!this._showWeekend)
            {
              ref DateTime local = ref dateTime;
              date = vLine.Date;
              int num2;
              if (date.DayOfWeek != DayOfWeek.Monday)
              {
                date = vLine.Date;
                if (date.DayOfWeek != DayOfWeek.Tuesday)
                {
                  num2 = -2;
                  goto label_8;
                }
              }
              num2 = -4;
label_8:
              double num3 = (double) num2;
              dateTime = local.AddDays(num3);
            }
            vLine.Date = dateTime;
          }
          if (renderTransform.X <= -1.5 * this.DayViewWidth)
          {
            flag = true;
            renderTransform.X += (double) (this.DisplayDays + 2) * this.DayViewWidth;
            date = vLine.Date;
            DateTime dateTime = date.AddDays((double) (2 + this.DisplayDays));
            if (!this._showWeekend)
            {
              ref DateTime local = ref dateTime;
              date = vLine.Date;
              int num4;
              if (date.DayOfWeek != DayOfWeek.Friday)
              {
                date = vLine.Date;
                if (date.DayOfWeek != DayOfWeek.Thursday)
                {
                  num4 = 2;
                  goto label_16;
                }
              }
              num4 = 4;
label_16:
              double num5 = (double) num4;
              dateTime = local.AddDays(num5);
            }
            vLine.Date = dateTime;
          }
          vLine.Offset = renderTransform.X;
        }
      }
      if (!flag)
        return;
      this._vLines.Sort((Comparison<MultiDayLine>) ((a, b) => a.Offset.CompareTo(b.Offset)));
    }

    private void SetStartDate(DateTime start)
    {
      this._model.StartDate = start;
      this._model.EndDate = this._model.StartDate.AddDays((double) (this.DisplayDays - 1 + (this._showWeekend ? 0 : 2)));
      EventHandler<(DateTime, DateTime)> dateRangeChanged = this.DateRangeChanged;
      if (dateRangeChanged != null)
        dateRangeChanged((object) null, (this._model.StartDate, this._model.EndDate));
      this.SetWeekNum();
    }

    public void OnDoubleFingerTouch()
    {
      if (!this._scrolling)
        return;
      DelayActionHandlerCenter.TryDoAction("CalendarInputScroll", new EventHandler(this.TryStopInputScroll), 100);
    }

    private void TryStopInputScroll(object sender, EventArgs e)
    {
      this.Dispatcher.Invoke(new Action(this.OnStopInputScroll));
    }

    public async Task<List<CalendarTimelineDayViewModel>> LoadModelsWhenScrolling(
      DateTime newDate,
      bool next)
    {
      if (this._scrollLoadedDate.Contains(newDate))
        return (List<CalendarTimelineDayViewModel>) null;
      for (int index = next ? 0 : -3; index <= (next ? 3 : 0); ++index)
        this._scrollLoadedDate.Add(newDate);
      List<CalendarDisplayModel> displayModels = await CalendarDisplayService.GetDisplayModels(next ? newDate : newDate.AddDays(-3.0), next ? newDate.AddDays(4.0) : newDate.AddDays(1.0), true);
      if (displayModels != null)
      {
        for (int index = next ? 0 : -3; index <= (next ? 3 : 0); ++index)
        {
          DateTime date = newDate.AddDays((double) index);
          if (!this._pointVms.Any<CalendarTimelineDayViewModel>((Func<CalendarTimelineDayViewModel, bool>) (m => m.Date == date)))
          {
            List<CalendarDisplayModel> list1 = displayModels.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (task => DateUtils.IsTaskInDay(date, task.DisplayStartDate, task.DisplayDueDate, task.IsAllDay))).ToList<CalendarDisplayModel>();
            List<TaskCellViewModel> taskCellViewModelList = new List<TaskCellViewModel>();
            if (list1.Count > 0)
              taskCellViewModelList.AddRange(list1.Select<CalendarDisplayModel, TaskCellViewModel>((Func<CalendarDisplayModel, TaskCellViewModel>) (task => TaskCellViewModel.Build(task, date))));
            CalendarTimelineDayViewModel timelineDayViewModel = new CalendarTimelineDayViewModel()
            {
              Date = date,
              Cells = taskCellViewModelList,
              InWeekControl = true
            };
            if (CalendarGeoHelper.TopFolded)
            {
              List<CalendarDisplayModel> topTasks = list1.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (task =>
              {
                if (!task.DisplayStartDate.HasValue || task.DisplayStartDate.Value.Hour >= CalendarGeoHelper.GetStartHour() && !(task.DisplayStartDate.Value.Date < date))
                  return false;
                return !task.DisplayDueDate.HasValue || (task.DisplayDueDate.Value - date.Date).TotalHours <= (double) CalendarGeoHelper.GetStartHour();
              })).OrderBy<CalendarDisplayModel, DateTime?>((Func<CalendarDisplayModel, DateTime?>) (t => t.DisplayDueDate)).ToList<CalendarDisplayModel>();
              List<string> extraTasks1 = MultiDayView.GetExtraTasks((IReadOnlyList<CalendarDisplayModel>) topTasks);
              timelineDayViewModel.Extra1 = extraTasks1[0];
              timelineDayViewModel.Extra2 = extraTasks1[1];
              timelineDayViewModel.TopTasks = topTasks;
              List<CalendarDisplayModel> list2 = list1.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (task => !topTasks.Contains(task) && task.DisplayStartDate.HasValue && task.DisplayStartDate.Value.Hour >= CalendarGeoHelper.GetEndHour() && task.DisplayStartDate.Value.Date >= date)).OrderBy<CalendarDisplayModel, DateTime?>((Func<CalendarDisplayModel, DateTime?>) (t => t.DisplayStartDate)).ToList<CalendarDisplayModel>();
              List<string> extraTasks2 = MultiDayView.GetExtraTasks((IReadOnlyList<CalendarDisplayModel>) list2);
              timelineDayViewModel.Extra3 = extraTasks2[0];
              timelineDayViewModel.Extra4 = extraTasks2[1];
              timelineDayViewModel.BotTasks = list2;
            }
            this._pointVms.Add(timelineDayViewModel);
          }
        }
        foreach (CalendarDisplayModel calendarDisplayModel in displayModels)
        {
          CalendarDisplayModel task = calendarDisplayModel;
          if (!this._tasks.Any<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (t =>
          {
            if (!(t.Id == task.Id))
              return false;
            DateTime? displayStartDate1 = t.DisplayStartDate;
            DateTime? displayStartDate2 = task.DisplayStartDate;
            if (displayStartDate1.HasValue != displayStartDate2.HasValue)
              return false;
            return !displayStartDate1.HasValue || displayStartDate1.GetValueOrDefault() == displayStartDate2.GetValueOrDefault();
          })))
            this._tasks.Add(task);
        }
        this.SetScrollingAllDayEvents();
        return this._pointVms;
      }
      this._pointVms.Add(new CalendarTimelineDayViewModel()
      {
        Date = newDate,
        InWeekControl = true
      });
      return this._pointVms;
    }

    private async Task SetScrollingAllDayEvents()
    {
      MultiDayView multiDayView = this;
      List<CalendarDisplayModel> list = multiDayView._tasks.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (t =>
      {
        if (!GridGeoAssembler.IsValidSpanModel(t, true))
        {
          bool? isAllDay = t.IsAllDay;
          if (isAllDay.HasValue)
          {
            isAllDay = t.IsAllDay;
            if (isAllDay.Value)
              goto label_6;
          }
          DateTime? nullable = t.CompletedTime;
          if (!nullable.HasValue)
            return false;
          nullable = t.StartDate;
          return !nullable.HasValue;
        }
label_6:
        return true;
      })).ToList<CalendarDisplayModel>();
      multiDayView._assembles = GridGeoAssembler.AssemblyMultiDayEvents(list, multiDayView._alldayView.ScrollStartDate, multiDayView._showWeekend).ToList<CalendarDisplayViewModel>();
      // ISSUE: reference to a compiler-generated method
      multiDayView._allDayVms = multiDayView._assembles.Select<CalendarDisplayViewModel, WeekEventModel>(new Func<CalendarDisplayViewModel, WeekEventModel>(multiDayView.\u003CSetScrollingAllDayEvents\u003Eb__115_1)).ToList<WeekEventModel>();
      if (LocalSettings.Settings.WeekAllDayHeight == 0.0)
      {
        int num = 0;
        foreach (CalendarDisplayViewModel assemble in multiDayView._assembles)
        {
          if (assemble.Row > num && num < 6)
            num = assemble.Row;
          LocalSettings.Settings.WeekAllDayHeight = (double) (63 + (num + 1) * 20);
          multiDayView.GetParent()?.NotifyAllDayHeightChanged();
        }
      }
      multiDayView._alldayView.SetEvents(multiDayView._allDayVms, false);
    }

    public async Task FlashNaviDate(DateTime date, bool delay = false)
    {
      MultiDayView multiDayView = this;
      Border border;
      if (multiDayView.DisplayDays < 7 && multiDayView._showWeekend)
      {
        border = (Border) null;
      }
      else
      {
        if (!multiDayView.IsLoaded)
          await Task.Delay(240);
        Border border1 = new Border();
        border1.Width = multiDayView.DayViewWidth;
        border1.Margin = new Thickness(60.0 + (date - multiDayView._model.StartDate).TotalDays * multiDayView.DayViewWidth, 40.0, 0.0, 0.0);
        border1.VerticalAlignment = VerticalAlignment.Stretch;
        border1.HorizontalAlignment = HorizontalAlignment.Left;
        border1.Opacity = 0.0;
        border1.IsHitTestVisible = false;
        border = border1;
        border.SetResourceReference(Border.BackgroundProperty, (object) "BaseColorOpacity5");
        border.SetValue(Grid.RowSpanProperty, (object) 2);
        multiDayView._flashBorder = border;
        multiDayView.Children.Add((UIElement) multiDayView._flashBorder);
        int i;
        for (i = 0; i < 10; ++i)
        {
          await Task.Delay(delay ? 20 : 5);
          border.Opacity = (double) i / 10.0;
        }
        for (i = 0; i < 25; ++i)
        {
          await Task.Delay(20);
          border.Opacity = 1.0 - (double) i / 25.0;
        }
        multiDayView.Children.Remove((UIElement) border);
        if (multiDayView._flashBorder != border)
        {
          border = (Border) null;
        }
        else
        {
          multiDayView.RemoveFlashBorder();
          border = (Border) null;
        }
      }
    }

    private void RemoveFlashBorder()
    {
      this.Children.Remove((UIElement) this._flashBorder);
      this._flashBorder = (Border) null;
    }

    public void SetMode(string mode) => this._mode = mode;

    public bool ShowTimePoint()
    {
      return DateTime.Today >= this._model.StartDate && DateTime.Today <= this._model.EndDate;
    }

    public void OnShiftMouseWheel(int eDelta)
    {
      this.OnTouchScroll((eDelta > 0 ? 1.0 : -1.0) * Math.Max(30.0, this.DayViewWidth / 2.0));
      DelayActionHandlerCenter.TryDoAction("CalendarInputScroll", new EventHandler(this.TryStopInputScroll), 100);
    }
  }
}
