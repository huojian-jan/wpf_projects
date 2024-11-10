// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Month.MultiWeekControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
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
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Print;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Month
{
  public class MultiWeekControl : UserControl, INavigate, IDragBarEvent, IComponentConnector
  {
    private CalendarControl _calendarControl;
    private string _copyId;
    private MultiWeekDayControl _dayControl;
    private int _depth = 4;
    private CalendarDisplayViewModel _draggingModel;
    private DateTime? _selectedFirstDate;
    private int _maxCount;
    private Window _parentWindow;
    private List<WeekEventModel> _rowEvents = new List<WeekEventModel>();
    private List<CalendarDisplayModel> _tasks = new List<CalendarDisplayModel>();
    private List<MultiWeekWeekControl> _weekControls = new List<MultiWeekWeekControl>();
    private Dictionary<DateTime, List<WeekEventModel>> _rowData;
    private bool _showWeekend;
    private bool _dragFromArrange;
    private MonthGridViewModel _model;
    private bool _isMultiWeek;
    private bool _touchScrolling;
    private DateTime _lastScrollTime;
    private string _uid;
    private bool _upScroll;
    private DateTime _currentMonth;
    private string _mode;
    internal MultiWeekControl Root;
    internal WeekDayNameView DayNameView;
    internal Grid WeekPanel;
    internal Popup DragBarPopup;
    internal Grid PopTriangle;
    internal TaskDragBar DragBar;
    private bool _contentLoaded;

    public int DisplayWeeks { get; set; } = 5;

    public double WeekHeight { get; set; }

    public MultiWeekControl()
    {
      this.InitializeComponent();
      this._uid = Utils.GetGuid();
      this.Loaded += (RoutedEventHandler) ((s, e) => this.BindEvents());
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      this.SetGridModel(DateTime.Today);
      this.Margin = new Thickness(1.0, 0.0, 0.0, 0.0);
    }

    public void SetWeeks(int weeks, bool isMultiWeek)
    {
      this.DisplayWeeks = weeks;
      this._isMultiWeek = isMultiWeek;
      if (this.ActualHeight < 42.0 || weeks < 1)
        return;
      this.WeekHeight = Math.Max((this.ActualHeight - 42.0) / (double) weeks, 0.0);
      this.SetWeekViews();
    }

    private void SetWeekViews()
    {
      int num = this.DisplayWeeks + 2;
      double weekHeight = this.WeekHeight;
      if (num > this._weekControls.Count)
      {
        int count = this._weekControls.Count;
        for (int index = 0; index < num - count; ++index)
        {
          MultiWeekWeekControl multiWeekWeekControl = new MultiWeekWeekControl();
          multiWeekWeekControl.VerticalAlignment = VerticalAlignment.Top;
          MultiWeekWeekControl element = multiWeekWeekControl;
          this._weekControls.Add(element);
          this.WeekPanel.Children.Add((UIElement) element);
        }
      }
      else
      {
        List<MultiWeekWeekControl> multiWeekWeekControlList = new List<MultiWeekWeekControl>();
        for (int index = num; index < this._weekControls.Count; ++index)
          multiWeekWeekControlList.Add(this._weekControls[index]);
        multiWeekWeekControlList.ForEach((Action<MultiWeekWeekControl>) (d =>
        {
          this._weekControls.Remove(d);
          this.WeekPanel.Children.Remove((UIElement) d);
        }));
      }
      for (int index = 0; index < this._weekControls.Count; ++index)
      {
        MultiWeekWeekControl weekControl = this._weekControls[index];
        weekControl.RenderTransform = (Transform) new TranslateTransform()
        {
          Y = ((double) (index - 1) * weekHeight)
        };
        weekControl.Height = weekHeight;
        weekControl.Offset = (double) (index - 1) * weekHeight;
      }
    }

    public void OnDragStart(CalendarDisplayViewModel model, MouseEventArgs e, bool fromArrange)
    {
      CalendarControl parent = this.GetParent();
      if ((parent != null ? (parent.IsLocked ? 1 : 0) : 0) != 0 || this._draggingModel != null || e.LeftButton != MouseButtonState.Pressed)
        return;
      this._draggingModel = model;
      CalendarDisplayViewModel displayViewModel = model.Clone();
      TimeDataModel timeDataModel = new TimeDataModel()
      {
        TaskId = model.GetTaskId(),
        ItemId = model.ItemId,
        StartDate = model.DisplayStartDate,
        DueDate = model.DisplayDueDate,
        IsAllDay = model.IsAllDay,
        RepeatFlag = model.RepeatFlag,
        RepeatFrom = model.RepeatFrom
      };
      if (Utils.IsEmptyDate(timeDataModel.StartDate))
        timeDataModel.IsAllDay = new bool?(true);
      this.DragBar.DataContext = (object) displayViewModel;
      this.DragBar.Tag = (object) timeDataModel;
      this.DragBarPopup.Width = CalendarGeoHelper.GetMonthDragBarWidth(this.ActualWidth, CalendarGeoHelper.CalColumns);
      System.Windows.Point safePopupPosition = this.GetSafePopupPosition(e);
      this.DragBarPopup.PlacementRectangle = new Rect(safePopupPosition.X, safePopupPosition.Y, 0.0, 0.0);
      this.DragBarPopup.HorizontalOffset = Math.Min(this.DragBarPopup.Width, 120.0) * -0.5;
      this.GetParent()?.SetEditting(true);
      this.DragBarPopup.IsOpen = true;
      this.RegisterPanelDrag(fromArrange);
    }

    public bool OnSelection()
    {
      CalendarControl parent = this.GetParent();
      if ((parent != null ? (parent.IsLocked ? 1 : 0) : 0) != 0 || this._dayControl == null)
        return false;
      KeyValuePair<DateTime, DateTime> selectDateSpan = this.GetSelectDateSpan();
      this._dayControl.ShowAddTaskPopup((MouseEventArgs) null, selectDateSpan.Key, selectDateSpan.Value);
      return true;
    }

    public DateTime GoTo(DateTime dateTime, bool checkStart = true, bool isScrollToNow = false)
    {
      if (this.DisplayWeeks >= 5 & checkStart)
      {
        dateTime = DateUtils.GetMonthStart(dateTime);
        DateTime dateTime1 = dateTime.AddMonths(1);
        dateTime = Utils.GetWeekStart(dateTime);
        int weeks = this._isMultiWeek ? this.DisplayWeeks : ((dateTime1 - dateTime).TotalDays > 35.0 ? 6 : 5);
        if (weeks != this.DisplayWeeks)
          this.SetWeeks(weeks, false);
      }
      else
        dateTime = Utils.GetWeekStart(dateTime);
      this.Load(new DateTime?(dateTime));
      HabitSyncService.SyncHabitCheckInCalendar(dateTime);
      return dateTime;
    }

    private void TryReload()
    {
      if (!this.IsVisible)
        return;
      this.Load(new DateTime?());
    }

    public void Reload(bool force = false, int delay = 50, bool setWeekend = false)
    {
      if (!this.IsVisible)
        return;
      DelayActionHandlerCenter.TryDoAction(this._uid, new EventHandler(this.TryDelayReload), delay);
    }

    public DateTime Today()
    {
      DateTime today = DateTime.Today;
      DateTime weekStart = Utils.GetWeekStart(!this._isMultiWeek || this.DisplayWeeks >= 5 ? DateUtils.GetMonthStart(today) : today);
      this.Load(new DateTime?(weekStart));
      HabitSyncService.SyncHabitCheckInCalendar(weekStart);
      return weekStart;
    }

    public DateTime Next()
    {
      return this._isMultiWeek ? this.GoTo(this.GetStartDate().AddDays((double) (this.DisplayWeeks * 7)), false, false) : this.GoTo(this.GetCurrentMonthDate().AddMonths(1), true, false);
    }

    public DateTime Last()
    {
      return this._isMultiWeek ? this.GoTo(this.GetStartDate().AddDays((double) (this.DisplayWeeks * -7)), false, false) : this.GoTo(this.GetCurrentMonthDate().AddMonths(-1), true, false);
    }

    public async void NextWeek()
    {
      MultiWeekControl multiWeekControl = this;
      multiWeekControl._touchScrolling = true;
      double diff = multiWeekControl.WeekHeight;
      diff /= 6.0;
      int times = 6;
      while (times > 0)
      {
        foreach (MultiWeekWeekControl weekControl in multiWeekControl._weekControls)
        {
          if (weekControl.RenderTransform is TranslateTransform renderTransform)
          {
            double num = renderTransform.Y - diff;
            renderTransform.Y = num;
            weekControl.Offset = num;
          }
        }
        --times;
        await Task.Delay(10);
      }
      MultiWeekWeekControl weekControl1 = multiWeekControl._weekControls[0];
      multiWeekControl._weekControls.Remove(weekControl1);
      multiWeekControl._weekControls.Add(weekControl1);
      for (int index = 0; index < multiWeekControl._weekControls.Count; ++index)
      {
        MultiWeekWeekControl weekControl2 = multiWeekControl._weekControls[index];
        if (weekControl2.RenderTransform is TranslateTransform renderTransform)
        {
          renderTransform.Y = (double) (index - 1) * multiWeekControl.WeekHeight;
          weekControl2.Offset = (double) (index - 1) * multiWeekControl.WeekHeight;
        }
      }
      multiWeekControl.LoadOnScroll(multiWeekControl._model.StartDate.AddDays(7.0), true);
      multiWeekControl._touchScrolling = false;
      DelayActionHandlerCenter.TryDoAction("CalendarInputScroll", new EventHandler(multiWeekControl.TryStopTouchScroll), 50);
    }

    public async void LastWeek()
    {
      MultiWeekControl multiWeekControl = this;
      multiWeekControl._touchScrolling = true;
      double diff = -1.0 * multiWeekControl.WeekHeight;
      diff /= 6.0;
      int times = 6;
      while (times > 0)
      {
        for (int index = 0; index < multiWeekControl._weekControls.Count; ++index)
        {
          MultiWeekWeekControl weekControl = multiWeekControl._weekControls[index];
          if (weekControl.RenderTransform is TranslateTransform renderTransform)
          {
            double num = renderTransform.Y - diff;
            renderTransform.Y = num;
            weekControl.Offset = num;
          }
        }
        --times;
        await Task.Delay(10);
      }
      MultiWeekWeekControl weekControl1 = multiWeekControl._weekControls[multiWeekControl._weekControls.Count - 1];
      multiWeekControl._weekControls.Remove(weekControl1);
      multiWeekControl._weekControls.Insert(0, weekControl1);
      for (int index = 0; index < multiWeekControl._weekControls.Count; ++index)
      {
        MultiWeekWeekControl weekControl2 = multiWeekControl._weekControls[index];
        if (weekControl2.RenderTransform is TranslateTransform renderTransform)
        {
          renderTransform.Y = (double) (index - 1) * multiWeekControl.WeekHeight;
          weekControl2.Offset = (double) (index - 1) * multiWeekControl.WeekHeight;
        }
      }
      multiWeekControl.LoadOnScroll(multiWeekControl._model.StartDate.AddDays(-7.0), false);
      multiWeekControl._touchScrolling = false;
      DelayActionHandlerCenter.TryDoAction("CalendarInputScroll", new EventHandler(multiWeekControl.TryStopTouchScroll), 50);
    }

    public CalendarControl GetParent()
    {
      this._calendarControl = this._calendarControl ?? Utils.FindParent<CalendarControl>((DependencyObject) this);
      return this._calendarControl;
    }

    private Window GetParentWindow()
    {
      this._parentWindow = this._parentWindow ?? Utils.GetParentWindow((DependencyObject) this);
      return this._parentWindow;
    }

    public event EventHandler<(DateTime, DateTime)> DateRangeChanged;

    private void UnbindEvents()
    {
      CalendarEventChangeNotifier.TitleChanged -= new EventHandler<TextExtra>(this.NotifyEventTitleChanged);
      LocalSettings.Settings.PropertyChanged -= new PropertyChangedEventHandler(this.OnSettingsChanged);
    }

    private void BindEvents()
    {
      CalendarEventChangeNotifier.TitleChanged += new EventHandler<TextExtra>(this.NotifyEventTitleChanged);
      LocalSettings.Settings.PropertyChanged += new PropertyChangedEventHandler(this.OnSettingsChanged);
    }

    private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!this.IsVisible || this._model == null || !(e.PropertyName == "StartWeekOfYear") && !(e.PropertyName == "EnableLunar") && !(e.PropertyName == "ShowWeek") && !(e.PropertyName == "EnableHoliday"))
        return;
      this.SetGridModel(this._model.StartDate);
    }

    private void TryDelayReload(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.TryReload));
    }

    private void NotifyEventTitleChanged(object sender, TextExtra extra)
    {
      if (this.Visibility != Visibility.Visible)
        return;
      foreach (CalendarDisplayModel calendarDisplayModel in this._tasks.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (task => task.Id == extra.Id)))
        calendarDisplayModel.SourceViewModel.Title = extra.Text;
      this._rowEvents.Where<WeekEventModel>((Func<WeekEventModel, bool>) (m => m.Data.EventId == extra.Id)).ToList<WeekEventModel>().ForEach((Action<WeekEventModel>) (model => model.Data.SourceViewModel.Title = extra.Text));
    }

    public void ShowAddWindow(DateTime dateTime)
    {
      this._selectedFirstDate = new DateTime?(dateTime);
      foreach (MultiWeekWeekControl weekControl in this._weekControls)
        weekControl.TryShowAddWindow(dateTime);
    }

    public List<CalendarDisplayModel> ExistTasks(HashSet<string> taskIds)
    {
      List<CalendarDisplayModel> tasks = this._tasks;
      return (tasks != null ? tasks.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (model => taskIds.Contains(model.Id))).ToList<CalendarDisplayModel>() : (List<CalendarDisplayModel>) null) ?? new List<CalendarDisplayModel>();
    }

    public void RemoveItem(CalendarDisplayViewModel model)
    {
      if (model == null)
        return;
      this._tasks.RemoveAll((Predicate<CalendarDisplayModel>) (m => m.Id == model.SourceViewModel.Id && Math.Abs(m.repeatDiff - model.RepeatDiff) < 0.1));
      this.SetupEvents();
    }

    public (DateTime, DateTime) GetTimeSpan()
    {
      MonthGridViewModel model = this.GetModel();
      return model != null ? (model.StartDate, model.EndDate) : (DateTime.Today, DateTime.Today);
    }

    private async Task Load(DateTime? startDate)
    {
      MultiWeekControl sender = this;
      if (PopupStateManager.IsInAdd() || sender._touchScrolling)
      {
        await Task.Delay(500);
        if (PopupStateManager.IsInAdd() || sender._touchScrolling)
          return;
      }
      DateTime? nullable = startDate;
      DateTime dateTime;
      if (!nullable.HasValue)
      {
        MonthGridViewModel model = sender.GetModel();
        dateTime = model != null ? model.StartDate : DateTime.Today;
      }
      else
        dateTime = nullable.GetValueOrDefault();
      startDate = new DateTime?(dateTime);
      sender.DayNameView.ResetColumns();
      sender.SetGridModel(startDate.Value);
      EventHandler<(DateTime, DateTime)> dateRangeChanged = sender.DateRangeChanged;
      if (dateRangeChanged != null)
        dateRangeChanged((object) sender, (sender._model.StartDate, sender._model.EndDate));
      sender._currentMonth = DateUtils.GetCurrentMonthDate(sender._model.StartDate, sender._model.EndDate);
      if (!UserDao.IsPro())
        return;
      await sender.LoadRowData();
    }

    private void LoadOnScroll(DateTime startDate, bool isNext)
    {
      this.SetGridModel(startDate);
      EventHandler<(DateTime, DateTime)> dateRangeChanged = this.DateRangeChanged;
      if (dateRangeChanged != null)
        dateRangeChanged((object) this, (this._model.StartDate, this._model.EndDate));
      DateTime currentMonthDate = DateUtils.GetCurrentMonthDate(this._model.StartDate, this._model.EndDate);
      foreach (MultiWeekWeekControl weekControl in this._weekControls)
        weekControl.SetCurrentMonth(currentMonthDate);
      if (this._mode == "0" && this._currentMonth != currentMonthDate)
      {
        this._currentMonth = currentMonthDate;
        DelayActionHandlerCenter.TryDoAction("AddMonthScrollAction", (EventHandler) ((sender, args) => UserActCollectUtils.AddClickEvent("calendar", "month_action", "swipe_vertical")), 3000);
      }
      this.SetupWeekEvents(this._rowData, startDate.AddDays(isNext ? (double) (this.DisplayWeeks * 7) : -7.0));
    }

    private void SetGridModel(DateTime startDate)
    {
      MonthGridViewModel model = MonthGridViewModel.Build(startDate, this.DisplayWeeks);
      this._model = model;
      this.SetupRows(model);
    }

    private async Task LoadRowData()
    {
      await this.GetTasks();
      this.SetupEvents();
    }

    private async Task GetTasks()
    {
      MonthGridViewModel model = this.GetModel();
      int num = this.GetMaxCount();
      if (num < 0)
        num = 2;
      this._depth = num;
      this._tasks = await CalendarDisplayService.GetDisplayModels(model.StartDate.AddDays(-7.0), model.EndDate.AddDays(8.0), true);
      if (this._tasks == null || !this._tasks.Any<CalendarDisplayModel>())
        return;
      this._tasks = this._tasks.OrderBy<CalendarDisplayModel, int>((Func<CalendarDisplayModel, int>) (task => task.Status)).ToList<CalendarDisplayModel>();
    }

    public void LoadVirtualModels()
    {
      MonthGridViewModel model = this.GetModel();
      this._tasks = CalendarDisplayModel.GetVirtualModels(model.StartDate, model.EndDate);
      this.SetupEvents();
    }

    private void SetupEvents()
    {
      if (this.GetModel() == null)
        return;
      this._rowData = this.GetRowData(this._depth, false);
      this.SetupEvents(this._rowData);
      this._rowEvents = this._rowData.Values.SelectMany<List<WeekEventModel>, WeekEventModel>((Func<List<WeekEventModel>, IEnumerable<WeekEventModel>>) (m => (IEnumerable<WeekEventModel>) m)).ToList<WeekEventModel>();
    }

    private List<WeekEventModel> GetRowModels(
      int depth,
      bool isPrint,
      List<CalendarDisplayViewModel> tasks)
    {
      List<WeekEventModel> list1 = tasks.Where<CalendarDisplayViewModel>((Func<CalendarDisplayViewModel, bool>) (item => item.Row <= depth || depth < 0)).Select<CalendarDisplayViewModel, WeekEventModel>((Func<CalendarDisplayViewModel, WeekEventModel>) (item => new WeekEventModel()
      {
        Row = item.Row,
        Column = item.Column,
        ColumnSpan = item.ColumnSpan,
        Data = item
      })).ToList<WeekEventModel>();
      if (depth >= 0)
      {
        Dictionary<int, WeekEventModel> source = new Dictionary<int, WeekEventModel>();
        for (int j = 0; j < 7; j++)
        {
          List<CalendarDisplayViewModel> list2 = tasks.Where<CalendarDisplayViewModel>((Func<CalendarDisplayViewModel, bool>) (data => data.Column <= j && data.Column + data.ColumnSpan > j)).OrderByDescending<CalendarDisplayViewModel, int>((Func<CalendarDisplayViewModel, int>) (task => task.Row)).ToList<CalendarDisplayViewModel>();
          CalendarDisplayViewModel displayViewModel = list2.FirstOrDefault<CalendarDisplayViewModel>();
          if (displayViewModel != null)
          {
            int row = displayViewModel.Row;
            if (row >= depth)
            {
              string text = "+" + (row - depth).ToString();
              double num = 0.0;
              CalendarDisplayViewModel lastOne = list2.FirstOrDefault<CalendarDisplayViewModel>((Func<CalendarDisplayViewModel, bool>) (m => m.Row == depth));
              if (lastOne != null && lastOne.ColumnSpan != 1 || row != depth)
              {
                if (lastOne != null)
                {
                  if (lastOne.ColumnSpan > 1)
                  {
                    WeekEventModel weekEventModel = list1.FirstOrDefault<WeekEventModel>((Func<WeekEventModel, bool>) (v => v.Data == lastOne));
                    if (row == depth)
                    {
                      if (weekEventModel != null)
                      {
                        source.Add(j, weekEventModel);
                        continue;
                      }
                      text = "+1";
                    }
                    else
                    {
                      text = "+" + (row + 1 - depth).ToString();
                      list1.Remove(weekEventModel);
                    }
                  }
                  else if (isPrint)
                  {
                    text = "+" + (row + 1 - depth).ToString();
                    WeekEventModel weekEventModel = list1.FirstOrDefault<WeekEventModel>((Func<WeekEventModel, bool>) (v => v.Data == lastOne));
                    list1.Remove(weekEventModel);
                  }
                  else
                  {
                    num = Utils.MeasureString(text, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 11.0).Width + 8.0;
                    lastOne.LoadMoreWidth = num;
                  }
                }
                list1.Add(new WeekEventModel()
                {
                  Column = j,
                  Row = depth,
                  ColumnSpan = 1,
                  Data = new CalendarDisplayViewModel()
                  {
                    Column = j,
                    Row = depth,
                    ColumnSpan = 1,
                    IsLoadMore = true,
                    LoadMoreWidth = num,
                    SourceViewModel = new TaskBaseViewModel()
                    {
                      Type = DisplayType.LoadMore,
                      Title = text
                    }
                  }
                });
              }
            }
          }
        }
        if (source.Any<KeyValuePair<int, WeekEventModel>>())
        {
          foreach (int key in source.Keys)
          {
            WeekEventModel weekEventModel = source[key];
            if (!list1.Contains(weekEventModel))
              list1.Add(new WeekEventModel()
              {
                Column = key,
                Row = depth,
                ColumnSpan = 1,
                Data = new CalendarDisplayViewModel()
                {
                  Column = key,
                  Row = depth,
                  ColumnSpan = 1,
                  IsLoadMore = true,
                  LoadMoreWidth = 0.0,
                  SourceViewModel = new TaskBaseViewModel()
                  {
                    Type = DisplayType.LoadMore,
                    Title = "+1"
                  }
                }
              });
          }
        }
      }
      return list1;
    }

    private Dictionary<DateTime, List<WeekEventModel>> GetRowData(int depth, bool isPrint)
    {
      int num = !isPrint ? 1 : 0;
      Dictionary<DateTime, List<WeekEventModel>> rowData = new Dictionary<DateTime, List<WeekEventModel>>();
      int displayWeeks = this.DisplayWeeks;
      for (int index = -1 * num; index < displayWeeks + num; ++index)
      {
        DateTime dateTime = this.GetModel().StartDate.AddDays((double) (index * 7));
        List<CalendarDisplayViewModel> list = GridGeoAssembler.AssemblyEvents(this._tasks, dateTime).ToList<CalendarDisplayViewModel>();
        List<WeekEventModel> rowModels = this.GetRowModels(depth, isPrint, list);
        rowData.Add(dateTime, rowModels);
      }
      return rowData;
    }

    private MonthGridViewModel GetModel()
    {
      if (this._model != null)
        return this._model;
      DateTime displayStartDate = MonthGridViewModel.GetDisplayStartDate(DateTime.Today);
      bool flag = displayStartDate.AddDays(35.0).Month == DateTime.Today.Month;
      return MonthGridViewModel.Build(displayStartDate, flag ? 6 : 5);
    }

    private int GetMaxCount()
    {
      int displayWeeks = this.DisplayWeeks;
      double num = this.ActualHeight / (double) displayWeeks;
      if (num == 0.0)
      {
        Window window = Window.GetWindow((DependencyObject) this);
        num = window != null ? (window.ActualHeight - 100.0) / (double) displayWeeks : 0.0;
      }
      double max = num - 24.0;
      return MultiWeekControl.GetCount(GridGeoAssembler.TaskBarHeight, max) - 1;
    }

    private static int GetCount(int item, double max)
    {
      for (int index = 1; index < 100; ++index)
      {
        if ((double) (index * item) > max * 0.9)
          return index - 1;
      }
      return 1;
    }

    public KeyValuePair<DateTime, DateTime> GetSelectDateSpan()
    {
      if (this._model != null)
      {
        List<MonthDayViewModel> list = this._model.Models.Where<MonthDayViewModel>((Func<MonthDayViewModel, bool>) (model => model.State == MonthCellState.Selected)).OrderBy<MonthDayViewModel, DateTime>((Func<MonthDayViewModel, DateTime>) (model => model.Date)).ToList<MonthDayViewModel>();
        MonthDayViewModel monthDayViewModel1 = list.FirstOrDefault<MonthDayViewModel>();
        MonthDayViewModel monthDayViewModel2 = list.LastOrDefault<MonthDayViewModel>();
        if (monthDayViewModel1 != null && monthDayViewModel2 != null)
          return new KeyValuePair<DateTime, DateTime>(monthDayViewModel1.Date, monthDayViewModel2.Date);
      }
      return new KeyValuePair<DateTime, DateTime>();
    }

    public void SetSelection(DateTime date, MultiWeekDayControl dayControl)
    {
      if (!this._selectedFirstDate.HasValue || this._model == null)
        return;
      DateTime start = this._selectedFirstDate.Value;
      DateTime end = date;
      if (start > end)
      {
        end = start;
        start = date;
      }
      this._model.Models.ForEach((Action<MonthDayViewModel>) (model =>
      {
        if (model.Date >= start.Date && model.Date <= end.Date)
          model.State = MonthCellState.Selected;
        else
          model.State = MonthCellState.Normal;
      }));
      this._dayControl = dayControl;
    }

    public void ClearSelection()
    {
      if (!this._selectedFirstDate.HasValue || this._model == null)
        return;
      this._model.Models.Where<MonthDayViewModel>((Func<MonthDayViewModel, bool>) (model => model.State != 0)).ToList<MonthDayViewModel>().ForEach((Action<MonthDayViewModel>) (model => model.State = MonthCellState.Normal));
      this._selectedFirstDate = new DateTime?();
      this._dayControl = (MultiWeekDayControl) null;
    }

    private void SetupRows(MonthGridViewModel model)
    {
      for (int index = 0; index < this._weekControls.Count; ++index)
        this._weekControls[index].SetData(model.ModelDict, index - 1);
    }

    private async Task SetupWeekEvents(
      Dictionary<DateTime, List<WeekEventModel>> rowData,
      DateTime weekStart)
    {
      if (rowData == null)
        return;
      foreach (MultiWeekWeekControl multiWeekWeekControl in this._weekControls.Where<MultiWeekWeekControl>((Func<MultiWeekWeekControl, bool>) (weekControl => weekControl.StartDate == weekStart)))
      {
        List<WeekEventModel> events;
        if (rowData.TryGetValue(multiWeekWeekControl.StartDate, out events))
          multiWeekWeekControl.SetEvents((IEnumerable<WeekEventModel>) events);
        else
          multiWeekWeekControl.SetEvents((IEnumerable<WeekEventModel>) new List<WeekEventModel>());
      }
    }

    private void SetupEvents(Dictionary<DateTime, List<WeekEventModel>> rowData)
    {
      if (rowData == null)
        return;
      for (int index = 0; index < this._weekControls.Count; ++index)
      {
        MultiWeekWeekControl weekControl = this._weekControls[index];
        List<WeekEventModel> events;
        if (rowData.TryGetValue(weekControl.StartDate, out events))
          weekControl.SetEvents((IEnumerable<WeekEventModel>) events);
        else
          weekControl.SetEvents((IEnumerable<WeekEventModel>) new List<WeekEventModel>());
      }
    }

    private DateTime GetStartDate() => this._model != null ? this._model.StartDate : DateTime.Today;

    private DateTime GetCurrentMonthDate()
    {
      return this._model != null ? this._model.GetCurrentMonthDate() : DateTime.Today;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
      if (!this.IsVisible)
        return;
      this.MovePopup(e);
    }

    private void RegisterPanelDrag(bool fromArrange)
    {
      this._dragFromArrange = fromArrange;
      Window parentWindow = this.GetParentWindow();
      parentWindow.CaptureMouse();
      parentWindow.MouseMove -= new MouseEventHandler(this.OnMouseMove);
      parentWindow.MouseMove += new MouseEventHandler(this.OnMouseMove);
      parentWindow.MouseLeftButtonUp -= new MouseButtonEventHandler(this.OnDragDrop);
      parentWindow.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDragDrop);
    }

    private void MovePopup(MouseEventArgs arg)
    {
      if (!this.DragBarPopup.IsOpen || arg.LeftButton != MouseButtonState.Pressed)
        return;
      System.Windows.Point safePopupPosition = this.GetSafePopupPosition(arg);
      this.PopTriangle.Visibility = Visibility.Collapsed;
      if (!this._dragFromArrange)
      {
        CalendarDisplayViewModel draggingModel = this._draggingModel;
        if ((draggingModel != null ? (draggingModel.IsTaskOrNote ? 1 : 0) : 0) != 0)
          this.GetParent()?.ArrangePanel?.OnPopupMove(safePopupPosition, this.ActualWidth);
      }
      if (safePopupPosition.X > this.ActualWidth)
      {
        double y = safePopupPosition.Y;
        CalendarControl parent = this.GetParent();
        if ((parent != null ? (parent.CalendarSide.CheckHoverDayCell(20.0) ? 1 : 0) : 0) != 0)
        {
          this.PopTriangle.Visibility = Visibility.Visible;
          this.DragBarPopup.Height += 6.0;
          y -= 10.0;
        }
        this.DragBarPopup.PlacementRectangle = new Rect(safePopupPosition.X, y, 0.0, 0.0);
        this.ClearDropTarget();
      }
      else
      {
        this.DragBarPopup.PlacementRectangle = new Rect(safePopupPosition.X, safePopupPosition.Y, 0.0, 0.0);
        this.SetDropTarget(arg);
      }
    }

    private System.Windows.Point GetSafePopupPosition(MouseEventArgs arg)
    {
      return arg.GetPosition((IInputElement) this);
    }

    private void ClearDropTarget()
    {
      foreach (MultiWeekWeekControl weekControl in this._weekControls)
        weekControl.ClearDropTarget();
    }

    private void SetDropTarget(MouseEventArgs arg)
    {
      foreach (MultiWeekWeekControl weekControl in this._weekControls)
        weekControl.SetDropTarget(arg);
    }

    private async void OnDragDrop(object sender, MouseEventArgs e)
    {
      MultiWeekControl multiWeekControl = this;
      CalendarControl parent = multiWeekControl.GetParent();
      if (sender is Window window)
      {
        window.ReleaseMouseCapture();
        window.MouseLeftButtonUp -= new MouseButtonEventHandler(multiWeekControl.OnDragDrop);
        window.MouseMove -= new MouseEventHandler(multiWeekControl.OnMouseMove);
      }
      if (multiWeekControl.DragBarPopup.IsOpen && parent != null)
      {
        multiWeekControl.DragBarPopup.IsOpen = false;
        DateTime? dropDate = multiWeekControl.GetDropDate();
        multiWeekControl.ClearDropTarget();
        CalendarDisplayViewModel model = multiWeekControl._draggingModel.Clone();
        if (Utils.IfCtrlPressed())
        {
          string str = await parent.TryCopy(model);
          multiWeekControl._copyId = str;
        }
        DateTime? hoverDate = parent.CalendarSide.DayPicker.GetHoverDate();
        parent.CalendarSide.DayPicker.ClearHoverCell();
        if (hoverDate.HasValue)
          dropDate = hoverDate;
        TimeDataModel original = (TimeDataModel) multiWeekControl.DragBar.Tag;
        if (original.StartDate.HasValue && dropDate.HasValue)
        {
          DateTime dateTime = original.StartDate.Value;
          DateTime date1 = dateTime.Date;
          dateTime = dropDate.Value;
          DateTime date2 = dateTime.Date;
          if (date1 == date2)
          {
            ClearDragging();
            parent = (CalendarControl) null;
            return;
          }
        }
        model = multiWeekControl.TryCopyModel(model, original);
        if (dropDate.HasValue)
        {
          await multiWeekControl.HandleModelDrop(model, dropDate, original);
        }
        else
        {
          int num = model.IsDerivative ? 1 : 0;
        }
        if (model.IsTaskOrNote && !multiWeekControl._dragFromArrange)
          await parent.ArrangePanel.OnDragDrop(original);
        multiWeekControl.SetupEvents();
        dropDate = new DateTime?();
        model = (CalendarDisplayViewModel) null;
        original = (TimeDataModel) null;
      }
      multiWeekControl._dragFromArrange = false;
      ClearDragging();
      parent = (CalendarControl) null;

      async void ClearDragging()
      {
        CalendarDisplayViewModel model = this._draggingModel;
        this._draggingModel = (CalendarDisplayViewModel) null;
        if (model != null)
        {
          await Task.Delay(100);
          model.Dragging = false;
        }
        CalendarControl parent = this.GetParent();
        if (parent == null)
        {
          model = (CalendarDisplayViewModel) null;
        }
        else
        {
          parent.SetEditting(false);
          model = (CalendarDisplayViewModel) null;
        }
      }
    }

    private CalendarDisplayViewModel TryCopyModel(
      CalendarDisplayViewModel model,
      TimeDataModel original)
    {
      if (string.IsNullOrEmpty(this._copyId))
        return model;
      CalendarDisplayViewModel displayViewModel = model.Clone();
      if (model.IsCalendarEvent)
      {
        displayViewModel.SourceViewModel.Id = this._copyId;
        original.EventId = this._copyId;
      }
      else if (!string.IsNullOrEmpty(model.ItemId))
      {
        displayViewModel.SourceViewModel = TaskDetailItemCache.GetCheckItemById(this._copyId);
        original.ItemId = this._copyId;
      }
      else
      {
        displayViewModel.SourceViewModel = TaskCache.GetTaskById(this._copyId);
        original.TaskId = this._copyId;
      }
      this._copyId = (string) null;
      return displayViewModel;
    }

    private async Task HandleModelDrop(
      CalendarDisplayViewModel model,
      DateTime? dropDate,
      TimeDataModel original)
    {
      if (model.IsCalendarEvent)
        await this.NotifyEventDrop(dropDate, original, model);
      else if (!string.IsNullOrEmpty(original.ItemId))
        await this.NotifyCheckItemDrop(dropDate, model);
      else
        await this.NotifyTaskDrop(dropDate, original, model);
    }

    private async Task NotifyTaskDrop(
      DateTime? date,
      TimeDataModel original,
      CalendarDisplayViewModel model)
    {
      TimeData revisedDate;
      if (!date.HasValue)
      {
        revisedDate = (TimeData) null;
      }
      else
      {
        if (!original.StartDate.HasValue)
        {
          original.IsAllDay = new bool?(true);
        }
        else
        {
          DateTime dateTime = original.StartDate.Value;
          DateTime date1 = dateTime.Date;
          dateTime = date.Value;
          DateTime date2 = dateTime.Date;
          if (date1 == date2)
          {
            revisedDate = (TimeData) null;
            return;
          }
        }
        revisedDate = TaskService.GetChangedData(original.StartDate, original.DueDate, original.IsAllDay, date.Value, original.RepeatFlag, original.RepeatFrom);
        if (!string.IsNullOrEmpty(model.RepeatFlag))
        {
          TaskModel task = await TaskDao.GetThinTaskById(model.GetTaskId());
          if (task == null)
            revisedDate = (TimeData) null;
          else if (!model.StartDate.HasValue)
            revisedDate = (TimeData) null;
          else if (!task.startDate.HasValue)
          {
            revisedDate = (TimeData) null;
          }
          else
          {
            List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task.id);
            int num = await ModifyRepeatHandler.TryUpdateDueDateOnlyDate(model.TaskId, model.DisplayStartDate, model.DisplayDueDate, new TimeData()
            {
              StartDate = revisedDate.StartDate,
              DueDate = revisedDate.DueDate,
              IsAllDay = new bool?(!model.StartDate.HasValue || ((int) model.IsAllDay ?? 1) != 0),
              Reminders = remindersByTaskId,
              RepeatFrom = model.RepeatFrom,
              RepeatFlag = model.RepeatFlag,
              ExDates = task.exDate != null ? ((IEnumerable<string>) task.exDate).ToList<string>() : new List<string>()
            }, date.Value, 1, 0) ? 1 : 0;
            await Task.Delay(400);
            revisedDate = (TimeData) null;
          }
        }
        else
        {
          this.Reload();
          this.GetParent()?.ArrangePanel.NotifyModelChanged();
          await TaskService.SetDate(model.TaskId, date.Value, false, isAllDay: revisedDate.IsAllDay);
          if (original.StartDate.HasValue)
          {
            revisedDate = (TimeData) null;
          }
          else
          {
            await TaskService.SaveTaskReminders(new TaskModel()
            {
              id = model.TaskId,
              reminders = ((int) revisedDate.IsAllDay ?? 1) != 0 ? TimeData.GetDefaultAllDayReminders().ToArray() : TimeData.GetDefaultTimeReminders().ToArray()
            });
            revisedDate = (TimeData) null;
          }
        }
      }
    }

    private async Task NotifyEventDrop(
      DateTime? date,
      TimeDataModel original,
      CalendarDisplayViewModel model)
    {
      if (!date.HasValue)
        return;
      TimeData changedData = TaskService.GetChangedData(original.StartDate, original.DueDate, original.IsAllDay, date.Value, original.RepeatFlag, original.RepeatFrom);
      this.Reload();
      await CalendarService.SaveEventTime(model.EventId, new TimeData()
      {
        StartDate = changedData.StartDate,
        DueDate = changedData.DueDate,
        IsAllDay = changedData.IsAllDay,
        RepeatFlag = original.RepeatFlag
      });
    }

    private async Task NotifyCheckItemDrop(DateTime? date, CalendarDisplayViewModel model)
    {
      if (!date.HasValue)
        return;
      this.Reload();
      TaskDetailItemModel taskDetailItemModel = await TaskService.SetSubtaskDate(model.GetTaskId(), model.ItemId, date.Value);
    }

    public bool IsDragging() => this.DragBarPopup.IsOpen;

    private DateTime? GetDropDate()
    {
      MonthGridViewModel model = this.GetModel();
      return (model != null ? model.Models.FirstOrDefault<MonthDayViewModel>((Func<MonthDayViewModel, bool>) (m => m.State == MonthCellState.Drop)) : (MonthDayViewModel) null)?.Date;
    }

    public void SetFirstDate(DateTime vmDate) => this._selectedFirstDate = new DateTime?(vmDate);

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.SetWeeks(this.DisplayWeeks, this._isMultiWeek);
      Size size = e.NewSize;
      double height1 = size.Height;
      size = e.PreviousSize;
      double height2 = size.Height;
      if (Math.Abs(height1 - height2) <= 0.0)
        return;
      this.Reload(false, 50, false);
    }

    public async void PrintMonthCalendar(bool isAll)
    {
      if (this.GetModel() == null)
        return;
      await this.GetTasks();
      new PrintPreviewWindow(this.GetRowData(isAll ? -1 : 8, true).Values.ToList<List<WeekEventModel>>(), this.GetModel().StartDate).Show();
      this.SetupEvents();
    }

    private void OnMouseScroll(object sender, MouseWheelEventArgs e)
    {
      if (this._touchScrolling)
      {
        DelayActionHandlerCenter.TryDoAction("CalendarInputScroll", new EventHandler(this.TryStopTouchScroll), 100);
        this._upScroll = e.Delta < 0;
        this.TouchScroll((double) e.Delta);
      }
      else
      {
        if ((DateTime.Now - this._lastScrollTime).TotalMilliseconds < 30.0)
          return;
        TaskDetailWindow.TryCloseWindow();
        this._lastScrollTime = DateTime.Now;
        foreach (MultiWeekWeekControl weekControl in this._weekControls)
          weekControl.ShowMonthText();
        if (this._touchScrolling)
          return;
        if (e.Delta > 0)
          this.LastWeek();
        else
          this.NextWeek();
      }
    }

    private void TouchScroll(double delta)
    {
      bool flag = false;
      TaskDetailWindow.TryCloseWindow();
      foreach (MultiWeekWeekControl weekControl in this._weekControls)
      {
        if (weekControl.RenderTransform is TranslateTransform renderTransform)
        {
          double num = renderTransform.Y + delta;
          if (num <= -1.5 * this.WeekHeight)
          {
            num += (double) (this.DisplayWeeks + 2) * this.WeekHeight;
            flag = true;
            this._model.StartDate = this._model.StartDate.AddDays(7.0);
            this._model.EndDate = this._model.EndDate.AddDays(7.0);
            weekControl.AddDays((this.DisplayWeeks + 2) * 7);
            this.SetupWeekEvents(this._rowData, weekControl.StartDate);
          }
          if (num >= ((double) this.DisplayWeeks + 0.5) * this.WeekHeight + 2.0)
          {
            num -= (double) (this.DisplayWeeks + 2) * this.WeekHeight;
            flag = true;
            this._model.StartDate = this._model.StartDate.AddDays(-7.0);
            this._model.EndDate = this._model.EndDate.AddDays(-7.0);
            weekControl.AddDays((this.DisplayWeeks + 2) * -7);
            this.SetupWeekEvents(this._rowData, weekControl.StartDate);
          }
          weekControl.Offset = num;
          renderTransform.Y = num;
        }
        weekControl.ShowMonthText();
      }
      if (!flag)
        return;
      this._weekControls.Sort((Comparison<MultiWeekWeekControl>) ((a, b) => a.Offset.CompareTo(b.Offset)));
      EventHandler<(DateTime, DateTime)> dateRangeChanged = this.DateRangeChanged;
      if (dateRangeChanged != null)
        dateRangeChanged((object) this, (this._model.StartDate, this._model.EndDate));
      DateTime currentMonthDate = DateUtils.GetCurrentMonthDate(this._model.StartDate, this._model.EndDate);
      foreach (MultiWeekWeekControl weekControl in this._weekControls)
        weekControl.SetCurrentMonth(currentMonthDate);
      if (!(this._mode == "0") || !(this._currentMonth != currentMonthDate))
        return;
      this._currentMonth = currentMonthDate;
      DelayActionHandlerCenter.TryDoAction("AddMonthScrollAction", (EventHandler) ((sender, args) => UserActCollectUtils.AddClickEvent("calendar", "month_action", "swipe_vertical")), 3000);
    }

    public void ClearItems(bool onlyData = false)
    {
      foreach (MultiWeekWeekControl weekControl in this._weekControls)
      {
        weekControl.ClearItems(onlyData);
        weekControl.DataContext = (object) null;
      }
      this._rowEvents?.Clear();
      this._tasks?.Clear();
    }

    public void OnShowWeekChanged(bool checkWeekStart = false)
    {
      if (this._model == null)
        return;
      if (checkWeekStart)
      {
        DateTime startDate = this._model.StartDate;
        switch (LocalSettings.Settings.WeekStartFrom)
        {
          case "Sunday":
            if (startDate.DayOfWeek == DayOfWeek.Saturday)
              startDate = startDate.AddDays(1.0);
            if (startDate.DayOfWeek == DayOfWeek.Monday)
            {
              startDate = startDate.AddDays(-1.0);
              break;
            }
            break;
          case "Saturday":
            if (startDate.DayOfWeek == DayOfWeek.Sunday)
              startDate = startDate.AddDays(-1.0);
            if (startDate.DayOfWeek == DayOfWeek.Monday)
            {
              startDate = startDate.AddDays(-2.0);
              break;
            }
            break;
          case "Monday":
            if (startDate.DayOfWeek == DayOfWeek.Sunday)
              startDate = startDate.AddDays(1.0);
            if (startDate.DayOfWeek == DayOfWeek.Saturday)
            {
              startDate = startDate.AddDays(2.0);
              break;
            }
            break;
        }
        if (startDate != this._model.StartDate)
        {
          this.SetGridModel(startDate);
          EventHandler<(DateTime, DateTime)> dateRangeChanged = this.DateRangeChanged;
          if (dateRangeChanged != null)
            dateRangeChanged((object) this, (this._model.StartDate, this._model.EndDate));
        }
      }
      this.DayNameView.ResetColumns(true);
      foreach (MultiWeekWeekControl weekControl in this._weekControls)
        weekControl.SetColumns(LocalSettings.Settings.ShowCalWeekend ? 7 : 5);
      this.SetupRows(this._model);
      this.SetupEvents();
    }

    public void OnDoubleFingerTouch()
    {
      this._touchScrolling = true;
      DelayActionHandlerCenter.TryDoAction("CalendarInputScroll", new EventHandler(this.TryStopTouchScroll), 50);
    }

    private void TryStopTouchScroll(object sender, EventArgs e)
    {
      this.Dispatcher.Invoke(new Action(this.OnStopTouchScroll));
    }

    private async void OnStopTouchScroll()
    {
      double diff = 0.0;
      if (this._weekControls[0].RenderTransform is TranslateTransform renderTransform1)
        diff = renderTransform1.Y + this.WeekHeight;
      if (Math.Abs(diff) > 0.1)
      {
        if (Math.Abs(diff) > 12.0)
          diff = diff >= 0.0 ? (this._upScroll ? diff : diff - this.WeekHeight) : (this._upScroll ? diff + this.WeekHeight : diff);
        diff /= 6.0;
        int times = 6;
        while (times > 0)
        {
          this.TouchScroll(-1.0 * diff);
          --times;
          await Task.Delay(10);
        }
      }
      for (int index = 0; index < this._weekControls.Count; ++index)
      {
        if (this._weekControls[index].RenderTransform is TranslateTransform renderTransform2)
          renderTransform2.Y = (double) (index - 1) * this.WeekHeight;
      }
      this._touchScrolling = false;
      this.Reload(delay: 10);
    }

    public async Task FlashNaviDates(List<DateTime> navDates, bool delay = false)
    {
      MultiWeekControl multiWeekControl = this;
      if (!multiWeekControl.IsLoaded)
        await Task.Delay(240);
      foreach (MultiWeekWeekControl weekControl in multiWeekControl._weekControls)
        weekControl.FlashNaviDates(navDates, delay);
    }

    public void SetMode(string mode) => this._mode = mode;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/month/multiweekcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (MultiWeekControl) target;
          this.Root.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
          this.Root.MouseWheel += new MouseWheelEventHandler(this.OnMouseScroll);
          break;
        case 2:
          this.DayNameView = (WeekDayNameView) target;
          break;
        case 3:
          this.WeekPanel = (Grid) target;
          break;
        case 4:
          this.DragBarPopup = (Popup) target;
          break;
        case 5:
          this.PopTriangle = (Grid) target;
          break;
        case 6:
          this.DragBar = (TaskDragBar) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
