// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarControl
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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar.Month;
using ticktick_WPF.Views.Calendar.Week;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Widget;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarControl : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty SideDisplayModeProperty = DependencyProperty.Register(nameof (SideDisplayMode), typeof (CalSideBarDisplayMode), typeof (CalendarControl), new PropertyMetadata((object) CalSideBarDisplayMode.Cal));
    private RenderTargetBitmap _bmp;
    private bool _editting;
    private int _lastMouseMoveOffset;
    private DateTime? _lastMouseMoveTime;
    public MultiWeekControl MultiWeek;
    public MultiDayView MultiDay;
    private readonly DelayActionHandler _delayResize = new DelayActionHandler();
    private SettingDialog _settingDialog;
    private readonly bool _inWidget;
    private CalMoreItems _calMorePopup;
    private DateTime? _lastScrollTime;
    internal CalendarControl Root;
    internal BlurEffect CalendarBlurEffect;
    internal CalendarHead HeadView;
    internal Grid CalendarBody;
    internal Grid MonthGrid;
    internal Grid WeekGrid;
    internal Grid SideViewGrid;
    internal CalendarSideBar CalendarSide;
    internal ArrangeTaskPanel ArrangePanel;
    internal Border NotProBorder;
    private bool _contentLoaded;

    public CalSideBarDisplayMode SideDisplayMode
    {
      get => (CalSideBarDisplayMode) this.GetValue(CalendarControl.SideDisplayModeProperty);
      set
      {
        this.SetValue(CalendarControl.SideDisplayModeProperty, (object) value);
        LocalSettings.Settings.CalendarDisplaySettings.SetSideBar(this._inWidget, (int) value);
      }
    }

    public bool IsLocked { get; set; }

    public double TimeScrollVerticalOffset { get; set; }

    public bool InWidget => this._inWidget;

    public CalendarControl()
    {
      this._inWidget = true;
      this.InitializeComponent();
      this.InitEvents();
      this.LoadDefault();
      this.Loaded += new RoutedEventHandler(this.OnControlLoaded);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      UtilLog.Info("CalendarFilter : " + LocalSettings.Settings.CalendarFilterData);
    }

    public CalendarControl(bool inMainWindow)
    {
      this._inWidget = false;
      this.InitializeComponent();
      this.InitEvents();
      this.LoadDefault();
      this.Loaded += new RoutedEventHandler(this.OnControlLoaded);
      this.Unloaded += (RoutedEventHandler) ((s, e) => this.UnbindEvents());
      UtilLog.Info("CalendarFilter : " + LocalSettings.Settings.CalendarFilterData);
    }

    private void InitEvents()
    {
      this.InitHeadEvents();
      this.InitViewEvents();
      this.InitArrangeEvents();
    }

    private void UnbindEvents()
    {
      TasksChangeEventManager.RemoveHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      CalendarEventChangeNotifier.Deleted -= new EventHandler<string>(this.Reload);
      CalendarEventChangeNotifier.Changed -= new EventHandler<CalendarEventModel>(this.Reload);
      CalendarEventChangeNotifier.RemoteChanged -= new EventHandler(this.DelayReloadCalendar);
      DataChangedNotifier.HabitCheckInChanged -= new EventHandler<HabitCheckInModel>(this.OnCheckInChanged);
      DataChangedNotifier.HabitsChanged -= new EventHandler(this.OnHabitsChanged);
      DataChangedNotifier.ThemeModeChanged -= new EventHandler(this.OnIsDarkChanged);
      DataChangedNotifier.CalendarConfigChanged -= new EventHandler<bool>(this.ReloadOnConfigChanged);
      DataChangedNotifier.CalendarProjectFilterChanged -= new EventHandler(this.ReloadOnFilterChanged);
      DataChangedNotifier.CalendarChanged -= new EventHandler(this.OnCalendarChanged);
      DataChangedNotifier.EventArchivedChanged -= new EventHandler(this.Reload);
      DataChangedNotifier.WeekStartFromChanged -= new EventHandler(this.OnWeekStartChanged);
      DataChangedNotifier.YearStartFromChanged -= new EventHandler(this.Reload);
      DataChangedNotifier.ProjectChanged -= new EventHandler(this.Reload);
      DataChangedNotifier.TagDeleted -= new EventHandler(this.Reload);
      DataChangedNotifier.TagChanged -= new EventHandler<TagModel>(this.Reload);
      DataChangedNotifier.ScheduleChanged -= new EventHandler(this.Reload);
      DataChangedNotifier.FilterChanged -= new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      TimeChangeNotifier.DayChanged -= new EventHandler<EventArgs>(this.OnDayChanged);
      DataChangedNotifier.SyncDone -= new EventHandler<SyncResult>(this.OnSyncChanged);
      ticktick_WPF.Notifier.GlobalEventManager.ReloadCalendar -= new EventHandler(this.ForceReload);
      ticktick_WPF.Notifier.GlobalEventManager.ModifyRecurrenceCompleted -= new EventHandler(this.OnRecurrenceModified);
      PomoNotifier.Changed -= new EventHandler<PomoChangeArgs>(this.OnPomoChanged);
      PomoNotifier.ServiceChanged -= new EventHandler(this.OnPomoChanged);
      this._delayResize.StopAndClear();
    }

    private void OnWeekStartChanged(object sender, EventArgs e)
    {
      MultiWeekControl multiWeek = this.MultiWeek;
      if ((multiWeek != null ? (multiWeek.IsVisible ? 1 : 0) : 0) != 0)
        this.MultiWeek?.OnShowWeekChanged(true);
      MultiDayView multiDay = this.MultiDay;
      if ((multiDay != null ? (multiDay.IsVisible ? 1 : 0) : 0) != 0)
        this.SetCalendarSideSelectRange();
      if (this.Visibility != Visibility.Visible)
        return;
      this.Navigate().Reload(true);
    }

    private void OnShowWeekChanged(object sender, PropertyChangedEventArgs e)
    {
      if (this.HeadView.Mode == "1")
        this.MultiDay?.OnShowWeekChanged(new bool?(LocalSettings.Settings.ShowCalWeekend));
      MultiWeekControl multiWeek = this.MultiWeek;
      if ((multiWeek != null ? (multiWeek.IsVisible ? 1 : 0) : 0) == 0)
        return;
      this.MultiWeek?.OnShowWeekChanged();
    }

    private void BindEvents()
    {
      TasksChangeEventManager.AddHandler(TaskChangeNotifier.Notifier, new EventHandler<TasksChangeEventArgs>(this.OnTasksChanged));
      CalendarEventChangeNotifier.Deleted += new EventHandler<string>(this.Reload);
      CalendarEventChangeNotifier.Changed += new EventHandler<CalendarEventModel>(this.Reload);
      CalendarEventChangeNotifier.RemoteChanged += new EventHandler(this.DelayReloadCalendar);
      DataChangedNotifier.HabitCheckInChanged += new EventHandler<HabitCheckInModel>(this.OnCheckInChanged);
      DataChangedNotifier.HabitsChanged += new EventHandler(this.OnHabitsChanged);
      DataChangedNotifier.ThemeModeChanged += new EventHandler(this.OnIsDarkChanged);
      DataChangedNotifier.CalendarConfigChanged += new EventHandler<bool>(this.ReloadOnConfigChanged);
      DataChangedNotifier.CalendarProjectFilterChanged += new EventHandler(this.ReloadOnFilterChanged);
      DataChangedNotifier.CalendarChanged += new EventHandler(this.OnCalendarChanged);
      DataChangedNotifier.EventArchivedChanged += new EventHandler(this.Reload);
      DataChangedNotifier.WeekStartFromChanged += new EventHandler(this.OnWeekStartChanged);
      DataChangedNotifier.YearStartFromChanged += new EventHandler(this.Reload);
      DataChangedNotifier.ProjectChanged += new EventHandler(this.Reload);
      DataChangedNotifier.TagDeleted += new EventHandler(this.Reload);
      DataChangedNotifier.TagChanged += new EventHandler<TagModel>(this.Reload);
      DataChangedNotifier.FilterChanged += new EventHandler<FilterChangeArgs>(this.OnFilterChanged);
      DataChangedNotifier.ScheduleChanged += new EventHandler(this.Reload);
      DataChangedNotifier.SyncDone += new EventHandler<SyncResult>(this.OnSyncChanged);
      ticktick_WPF.Notifier.GlobalEventManager.ReloadCalendar += new EventHandler(this.ForceReload);
      ticktick_WPF.Notifier.GlobalEventManager.ModifyRecurrenceCompleted += new EventHandler(this.OnRecurrenceModified);
      TimeChangeNotifier.DayChanged += new EventHandler<EventArgs>(this.OnDayChanged);
      PomoNotifier.Changed += new EventHandler<PomoChangeArgs>(this.OnPomoChanged);
      PomoNotifier.ServiceChanged += new EventHandler(this.OnPomoChanged);
      PomoNotifier.LinkChanged += new EventHandler<PomoLinkArgs>(this.OnPomoChanged);
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnShowWeekChanged), "ShowCalWeekend");
      PropertyChangedEventManager.AddHandler((INotifyPropertyChanged) LocalSettings.Settings, new EventHandler<PropertyChangedEventArgs>(this.OnShowWeekChanged), "WeekStartFrom");
      this._delayResize.SetAction(new EventHandler(this.Resize));
    }

    private void OnCalendarChanged(object sender, EventArgs e) => this.CalendarSide.ReloadFilter();

    private void OnSyncChanged(object sender, SyncResult e)
    {
      if (!this.IsVisible || !e.RemoteTagChanged)
        return;
      ProjectExtra projectExtra = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      if (LocalSettings.Settings.CellColorType.ToLower() == "tag" || projectExtra.Tags != null && projectExtra.Tags.Any<string>())
        this.Navigate().Reload(delay: 1000);
      else
        this.CalendarSide.ReloadFilter();
      this.ArrangePanel.TryResetTagFilterText();
    }

    private void OnDayChanged(object sender, EventArgs e)
    {
      this.Navigate().Reload(true);
      this.ArrangePanel.TryReloadTasks();
      this.CalendarSide.DayPicker.SetDayCells();
    }

    private async void OnTasksChanged(object sender, TasksChangeEventArgs e)
    {
      CalendarControl calendarControl = this;
      bool flag1 = calendarControl.Visibility == Visibility.Visible;
      if (flag1)
      {
        bool flag2 = e.BatchChangedIds.Any();
        if (flag2)
          flag2 = await calendarControl.CheckChangedTaskMatch(e.BatchChangedIds);
        bool flag3 = flag2;
        if (!flag3)
        {
          bool flag4 = e.DeletedChangedIds.Any();
          if (flag4)
            flag4 = await calendarControl.CheckTaskDeletedIds(e.DeletedChangedIds.Value);
          flag3 = flag4;
        }
        bool flag5 = flag3;
        if (!flag5)
        {
          bool flag6 = e.UndoDeletedIds.Any();
          if (flag6)
            flag6 = await calendarControl.CheckTaskDeletedIds(e.UndoDeletedIds.Value);
          flag5 = flag6;
        }
        bool flag7 = flag5;
        if (!flag7)
        {
          bool flag8 = e.AddIds.Any();
          if (flag8)
            flag8 = await calendarControl.CheckAddTasksMatch(e.AddIds);
          flag7 = flag8;
        }
        bool flag9 = flag7;
        if (!flag9)
        {
          bool flag10 = e.StatusChangedIds.Any();
          if (flag10)
            flag10 = await calendarControl.CheckChangedTaskMatch(e.StatusChangedIds);
          flag9 = flag10;
        }
        bool flag11 = flag9;
        if (!flag11)
        {
          bool flag12 = e.KindChangedIds.Any();
          if (flag12)
            flag12 = await calendarControl.CheckChangedTaskMatch(e.KindChangedIds);
          flag11 = flag12;
        }
        bool flag13 = flag11;
        if (!flag13)
        {
          bool flag14 = e.ProjectChangedIds.Any();
          if (flag14)
            flag14 = await calendarControl.CheckChangedTaskMatch(e.ProjectChangedIds);
          flag13 = flag14;
        }
        bool flag15 = flag13;
        if (!flag15)
        {
          bool flag16 = e.DateChangedIds.Any();
          if (flag16)
            flag16 = await calendarControl.CheckChangedTaskMatch(e.DateChangedIds);
          flag15 = flag16;
        }
        bool flag17 = flag15;
        if (!flag17)
        {
          bool flag18 = e.PriorityChangedIds.Any();
          if (flag18)
            flag18 = await calendarControl.CheckPriorityChangedIds(e.PriorityChangedIds);
          flag17 = flag18;
        }
        bool flag19 = flag17;
        if (!flag19)
        {
          bool flag20 = e.TagChangedIds.Any();
          if (flag20)
            flag20 = await calendarControl.CheckTagChangedIds(e.TagChangedIds);
          flag19 = flag20;
        }
        bool flag21 = flag19;
        if (!flag21)
        {
          bool flag22 = e.AssignChangedIds.Any();
          if (flag22)
            flag22 = await calendarControl.CheckAssignChangedIds(e.AssignChangedIds);
          flag21 = flag22;
        }
        bool flag23 = flag21;
        if (!flag23)
        {
          bool flag24 = e.CheckItemChangedIds.Any();
          if (flag24)
            flag24 = await calendarControl.CheckItemsChangedIds(e.CheckItemChangedIds);
          flag23 = flag24;
        }
        flag1 = flag23;
      }
      if (!flag1)
        return;
      calendarControl.Reload(true, false, false);
    }

    private async Task<bool> CheckItemsChangedIds(BlockingSet<string> ids)
    {
      if (!LocalSettings.Settings.ShowCheckListInCal)
        return false;
      (DateTime startDate, DateTime endDate) = this.Navigate().GetTimeSpan();
      List<CalendarDisplayModel> calendarDisplayModelList = this.Navigate().ExistTasks(ids.Value);
      // ISSUE: explicit non-virtual call
      if ((calendarDisplayModelList != null ? (__nonvirtual (calendarDisplayModelList.Count) > 0 ? 1 : 0) : 0) != 0)
        return true;
      List<TaskBaseViewModel> matchedItemsInCal = TaskViewModelHelper.GetMatchedItemsInCal(startDate, endDate, ids.ToList());
      // ISSUE: explicit non-virtual call
      return matchedItemsInCal != null && __nonvirtual (matchedItemsInCal.Count) > 0;
    }

    private async Task<bool> CheckAddTasksMatch(BlockingSet<string> ids)
    {
      (DateTime, DateTime) timeSpan = this.Navigate().GetTimeSpan();
      return (await TaskViewModelHelper.GetMatchedTasksInCal(timeSpan.Item1, timeSpan.Item2.AddDays(1.0), ids.ToList())).Count > 0;
    }

    private async Task<bool> CheckAssignChangedIds(BlockingSet<string> ids)
    {
      ProjectExtra projectFilter = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      bool flag = projectFilter.IsAll;
      if (!flag && projectFilter.FilterIds.Any<string>())
      {
        FilterModel filterModel = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == projectFilter.FilterIds[0]));
        if (filterModel != null)
        {
          if (Parser.GetFilterRuleType(filterModel.rule) == 0)
          {
            if (Parser.ToNormalModel(filterModel.rule).Assignees.Any<string>())
              flag = true;
          }
          else if (Parser.ToAdvanceModel(filterModel.rule).CardList.Any<CardViewModel>((Func<CardViewModel, bool>) (c => c.ConditionName == "assignee")))
            flag = true;
        }
      }
      return flag && await this.CheckChangedTaskMatch(ids);
    }

    private async Task<bool> CheckTagChangedIds(BlockingSet<string> ids)
    {
      ProjectExtra projectFilter = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      bool flag = projectFilter.Tags.Any<string>();
      if (!flag && projectFilter.FilterIds.Any<string>())
      {
        FilterModel filterModel = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == projectFilter.FilterIds[0]));
        if (filterModel != null)
        {
          List<string> defaultTags = FilterViewModel.CalculateTaskDefault(filterModel.rule).DefaultTags;
          if ((defaultTags != null ? (defaultTags.Any<string>() ? 1 : 0) : 0) != 0)
            flag = true;
        }
      }
      return flag && await this.CheckChangedTaskMatch(ids, false);
    }

    private async Task<bool> CheckPriorityChangedIds(BlockingSet<string> ids)
    {
      ProjectExtra projectFilter = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      if (projectFilter.FilterIds.Any<string>())
      {
        FilterModel filterModel = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == projectFilter.FilterIds[0]));
        if (filterModel != null && FilterViewModel.CalculateTaskDefault(filterModel.rule).Priority.HasValue)
          return await this.CheckChangedTaskMatch(ids, false);
      }
      return false;
    }

    private async Task<bool> CheckChangedTaskMatch(BlockingSet<string> ids, bool checkCount = true)
    {
      (DateTime startDate, DateTime endDate) = this.Navigate().GetTimeSpan();
      List<CalendarDisplayModel> models = this.Navigate().ExistTasks(ids.Value);
      if (checkCount)
      {
        List<CalendarDisplayModel> calendarDisplayModelList = models;
        // ISSUE: explicit non-virtual call
        if ((calendarDisplayModelList != null ? (__nonvirtual (calendarDisplayModelList.Count) > 0 ? 1 : 0) : 0) != 0)
          return true;
      }
      return (await TaskViewModelHelper.GetMatchedTasksInCal(startDate, endDate, ids.ToList())).Count != models.Count;
    }

    private async Task<bool> CheckTaskDeletedIds(HashSet<string> ids)
    {
      (DateTime startDate, DateTime endDate) = this.Navigate().GetTimeSpan();
      List<CalendarDisplayModel> models = this.Navigate().ExistTasks(ids);
      bool flag = models.Count != (await TaskViewModelHelper.GetMatchedTasksInCal(startDate, endDate, ids.ToList<string>())).Count;
      models = (List<CalendarDisplayModel>) null;
      return flag;
    }

    private void OnPomoChanged(object sender, object e)
    {
      if (!LocalSettings.Settings.ShowFocusRecord)
        return;
      this.Reload(reloadArrange: false, reloadFilter: false);
    }

    private async void OnRecurrenceModified(object sender, EventArgs e)
    {
      await Task.Delay(50);
      if (!this._editting || PopupStateManager.IsViewPopOpened())
        return;
      this._editting = false;
    }

    private void ForceReload(object sender, EventArgs e)
    {
      if (!this.IsVisible)
        return;
      this.Reload(true);
    }

    private void OnIsDarkChanged(object sender, EventArgs e)
    {
      if (!this.IsVisible)
        return;
      if (this.NotProBorder.Visibility == Visibility.Visible)
        this.MultiWeek?.LoadVirtualModels();
      this.Reload(true, reloadFilter: false, resetDatePickerColor: true);
    }

    private void OnHabitsChanged(object sender, EventArgs e)
    {
      if (!this.IsVisible)
        return;
      this.Reload(reloadArrange: false, reloadFilter: false);
    }

    private void OnCheckInChanged(object sender, HabitCheckInModel checkIn)
    {
      if (!this.IsVisible)
        return;
      this.Reload(reloadArrange: false, reloadFilter: false);
    }

    private void ReloadOnSubtaskChanged(object sender, string e)
    {
      if (!LocalSettings.Settings.ShowCheckListInCal)
        return;
      this.Navigate()?.Reload();
      this.CalendarSide.DayPicker.LoadTaskIndicator();
    }

    private void ReloadOnFilterChanged(object sender, EventArgs e)
    {
      if (sender != null && !sender.Equals((object) this.CalendarSide))
        this.CalendarSide.ReloadFilter();
      if (!this.IsVisible)
        return;
      this.Reload(reloadFilter: false);
    }

    private void Reload(object sender, TagModel e)
    {
      ProjectExtra projectExtra = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      if (LocalSettings.Settings.CellColorType.ToLower() == "tag" || projectExtra.Tags != null && projectExtra.Tags.Any<string>())
        this.Navigate().Reload(delay: 1000);
      else
        this.CalendarSide.ReloadFilter();
      this.ArrangePanel.TryResetTagFilterText();
    }

    private void OnFilterChanged(object sender, FilterChangeArgs e)
    {
      if (!this.IsVisible || !e.RuleChanged || !ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData)?.FilterIds?.Contains(e.Filter?.id).GetValueOrDefault())
        return;
      this.Navigate()?.Reload(delay: 1000);
    }

    private void DelayReloadCalendar(object sender, EventArgs e)
    {
      if (!this.IsVisible)
        return;
      this.Navigate()?.Reload(delay: 1000);
    }

    private void Reload(object sender, CalendarEventModel e)
    {
      if (!this.IsVisible)
        return;
      this.Reload();
    }

    private void Reload(object sender, string e)
    {
      if (!this.IsVisible)
        return;
      this.Reload(reloadFilter: false);
    }

    private void ReloadOnConfigChanged(object sender, bool e)
    {
      if (!this.IsVisible)
        return;
      this.Navigate()?.Reload(true);
      this.CalendarSide.DayPicker.LoadTaskIndicator();
      if (!e)
        return;
      this.ArrangePanel.TryReloadTasks();
    }

    private void InitArrangeEvents()
    {
      this.ArrangePanel.OnClose -= new EventHandler(this.OnCloseArrangeTask);
      this.ArrangePanel.OnClose += new EventHandler(this.OnCloseArrangeTask);
      this.CalendarSide.DateSelected -= new EventHandler<DateTime>(this.OnDateSelected);
      this.CalendarSide.DateSelected += new EventHandler<DateTime>(this.OnDateSelected);
    }

    private void OnDateSelected(object sender, DateTime date)
    {
      this.HeadView.SelectedDate = new DateTime?(date);
      this.GoTo(date);
      MultiDayView multiDay = this.MultiDay;
      if ((multiDay != null ? (multiDay.IsVisible ? 1 : 0) : 0) != 0)
      {
        this.MultiDay.FlashNaviDate(date);
      }
      else
      {
        MultiWeekControl multiWeek = this.MultiWeek;
        if ((multiWeek != null ? (multiWeek.IsVisible ? 1 : 0) : 0) == 0)
          return;
        this.MultiWeek.FlashNaviDates(new List<DateTime>()
        {
          date
        });
      }
    }

    private void InitHeadEvents()
    {
      this.HeadView.SwitchView -= new WeekMonthSwitch.SwitchModeDelegate(this.OnSwitchView);
      this.HeadView.SwitchView += new WeekMonthSwitch.SwitchModeDelegate(this.OnSwitchView);
      this.HeadView.Next -= new EventHandler(this.MoveNext);
      this.HeadView.Next += new EventHandler(this.MoveNext);
      this.HeadView.Last -= new EventHandler(this.MoveLast);
      this.HeadView.Last += new EventHandler(this.MoveLast);
      this.HeadView.Today -= new EventHandler(this.GotoToday);
      this.HeadView.Today += new EventHandler(this.GotoToday);
      this.HeadView.DateSelect -= new EventHandler<DateTime>(this.GotoDate);
      this.HeadView.DateSelect += new EventHandler<DateTime>(this.GotoDate);
      this.HeadView.Action -= new EventHandler<string>(this.OnHeadAction);
      this.HeadView.Action += new EventHandler<string>(this.OnHeadAction);
    }

    private void OnHeadAction(object sender, string e)
    {
      switch (e)
      {
        case "ArrangeTask":
          this.ShowOrHideArrange();
          break;
        case "DisplaySetting":
          this.ShowDisplaySetting();
          break;
        case "Subscribe":
          this.ShowSubscribe();
          break;
        case "Print":
          this.Print(false);
          break;
        case "PrintDetail":
          this.Print(true);
          break;
      }
    }

    private void InitViewEvents()
    {
    }

    public void OnKeyUp(object sender, KeyEventArgs e)
    {
      this.OnKeyUp(e.Key);
      e.Handled = true;
    }

    private void GotoDate(object sender, DateTime date)
    {
      INavigate navigate = this.Navigate();
      navigate.GoTo(date, navigate.Equals((object) this.MultiWeek));
    }

    private void GoTo(DateTime date) => this.Navigate().GoTo(date);

    private void SetCalendarSideSelectRange()
    {
      this.CalendarSide.SetMonth(this.HeadView.GetCurrentMonthDate());
      if ((this.HeadView.EndDate - this.HeadView.StartDate).TotalDays < 28.0)
        this.CalendarSide.SetSelectedRange(new DateTime?(this.HeadView.StartDate), new DateTime?(this.HeadView.EndDate));
      else
        this.CalendarSide.SetSelectedRange(new DateTime?(), new DateTime?());
    }

    private void Reload(object sender, EventArgs e)
    {
      if (this.Visibility != Visibility.Visible)
        return;
      this.Reload(delay: true);
    }

    public void Reload(
      bool force = false,
      bool reloadArrange = true,
      bool reloadFilter = true,
      bool delay = false,
      bool resetDatePickerColor = false,
      bool setWeekend = false)
    {
      ThreadUtil.DetachedRunOnUiThread((Action) (() =>
      {
        this.Navigate().Reload(force, delay ? 1000 : 50, setWeekend);
        if (reloadArrange)
          this.ArrangePanel.TryReloadTasks();
        if (reloadFilter)
          this.CalendarSide.ReloadFilter();
        if (resetDatePickerColor)
          this.CalendarSide.DayPicker.ReloadColor();
        this.CalendarSide.DayPicker.LoadTaskIndicator();
      }));
    }

    public void GotoDate(DateTime? date, bool scrollToNow = false)
    {
      this.Navigate().GoTo(date ?? DateTime.Today, !date.HasValue, scrollToNow);
      this.SetCalendarSideSelectRange();
    }

    private void GotoToday(object sender, EventArgs eventArgs) => this.Navigate().Today();

    private async void MoveLast(object sender, EventArgs eventArgs)
    {
      if (!UserDao.IsPro())
        return;
      this.Navigate().Last();
    }

    private async void MoveNext(object sender, EventArgs eventArgs)
    {
      if (!UserDao.IsPro())
        return;
      this.Navigate().Next();
    }

    private INavigate Navigate()
    {
      if (!UserDao.IsPro())
        this.SwitchMultiWeeks(true);
      if (this.MonthGrid.Visibility == Visibility.Visible)
      {
        if (this.MultiWeek == null)
          this.InitMonthView(5);
        return (INavigate) this.MultiWeek;
      }
      if (this.MultiDay == null)
        this.InitWeekView(7);
      return (INavigate) this.MultiDay;
    }

    private void InitWeekView(int days, bool showWeekend = true)
    {
      MultiDayView multiDayView = new MultiDayView(this);
      multiDayView.Margin = new Thickness(0.0, 0.0, -1.0, 0.0);
      this.MultiDay = multiDayView;
      this.MultiDay.SetDisplayDays(days, showWeekend);
      this.MultiDay.DateRangeChanged += new EventHandler<(DateTime, DateTime)>(this.OnDateRangeChanged);
      this.WeekGrid.Children.Clear();
      this.WeekGrid.Children.Add((UIElement) this.MultiDay);
    }

    private void OnDateRangeChanged(object sender, (DateTime, DateTime) dateRange)
    {
      (DateTime dateTime1, DateTime dateTime2) = dateRange;
      if (!(dateTime1 != this.HeadView.StartDate) && !(dateTime2 != this.HeadView.EndDate))
        return;
      this.HeadView.EndDate = dateRange.Item2;
      this.HeadView.StartDate = dateRange.Item1;
      this.SetCalendarSideSelectRange();
    }

    private void InitMonthView(int weeks)
    {
      this.MultiWeek = new MultiWeekControl();
      this.MultiWeek.SetWeeks(weeks, false);
      this.MultiWeek.DateRangeChanged -= new EventHandler<(DateTime, DateTime)>(this.OnDateRangeChanged);
      this.MultiWeek.DateRangeChanged += new EventHandler<(DateTime, DateTime)>(this.OnDateRangeChanged);
      this.MonthGrid.Children.Clear();
      this.MonthGrid.Children.Add((UIElement) this.MultiWeek);
    }

    private void OnSwitchView(string from, string to)
    {
      if (from == to)
        return;
      switch (to)
      {
        case "2":
          this.SwitchMultiDay(true, false, 1);
          UserActCollectUtils.AddClickEvent("calendar", "view_switch", "day");
          break;
        case "1":
          if (from == "2")
            this.HeadView.SelectedDate = new DateTime?(this.HeadView.StartDate);
          this.SwitchMultiDay(false, true, 7, from == "2" ? new DateTime?(this.HeadView.StartDate) : new DateTime?());
          UserActCollectUtils.AddClickEvent("calendar", "view_switch", "week");
          break;
        case "0":
          if (from == "2" || from == "1" || to.StartsWith("D"))
            this.HeadView.SelectedDate = new DateTime?(this.HeadView.GetCurrentMonthDate());
          this.SwitchMultiWeeks(true, navDates: from == "1" || from == "2" ? this.HeadView.GetRangeDates() : (List<DateTime>) null);
          UserActCollectUtils.AddClickEvent("calendar", "view_switch", "month");
          break;
      }
      if (to.StartsWith("D"))
      {
        int result;
        this.SwitchMultiDay(false, false, int.TryParse(to.Substring(1), out result) ? result : 3, from == "2" ? new DateTime?(this.HeadView.StartDate) : new DateTime?());
        if (!from.StartsWith("D"))
          UserActCollectUtils.AddClickEvent("calendar", "view_switch", "multi_day");
      }
      if (to.StartsWith("W"))
      {
        int result;
        this.SwitchMultiWeeks(false, int.TryParse(to.Substring(1), out result) ? result : 3, from == "1" || from == "2" ? this.HeadView.GetRangeDates() : (List<DateTime>) null, from == "0" || from.StartsWith("W"));
        if (!from.StartsWith("W"))
          UserActCollectUtils.AddClickEvent("calendar", "view_switch", "multi_week");
      }
      PopupStateManager.Reset();
    }

    public async void SwitchMultiWeeks(
      bool toMonth,
      int weeks = 5,
      List<DateTime> navDates = null,
      bool fromMultiWeeks = false)
    {
      if (this.MultiWeek == null)
        this.InitMonthView(weeks);
      LocalSettings.Settings.CalendarDisplaySettings.SetHeadSwitch(this._inWidget, toMonth ? 0 : 4, weeks);
      this.CalendarSide.DayPicker.ShowSelectedInCal = false;
      this.WeekGrid.Visibility = Visibility.Collapsed;
      this.MultiDay?.ClearItems(true);
      this.MonthGrid.Visibility = Visibility.Visible;
      this.MultiWeek.SetWeeks(weeks, !toMonth);
      this.MultiWeek.SetMode(this.HeadView.Mode);
      this.MultiWeek.GoTo(fromMultiWeeks ? this.HeadView.StartDate : this.HeadView.GetCurrentMonthDate(true), !fromMultiWeeks, false);
      if (navDates != null)
        this.MultiWeek.FlashNaviDates(navDates, true);
      this.SetCalendarSideSelectRange();
    }

    private async void SwitchMultiDay(bool isDay, bool isWeek, int days, DateTime? date = null)
    {
      days = isDay ? 1 : (isWeek ? (LocalSettings.Settings.ShowCalWeekend ? 7 : 5) : days);
      if (this.MultiDay == null)
        this.InitWeekView(days, !isWeek || LocalSettings.Settings.ShowCalWeekend);
      this.CalendarSide.DayPicker.ShowSelectedInCal = days == 1;
      LocalSettings.Settings.CalendarDisplaySettings.SetHeadSwitch(this._inWidget, isDay ? 2 : (isWeek ? 1 : 3), days);
      this.MonthGrid.Visibility = Visibility.Collapsed;
      this.MultiWeek?.ClearItems(true);
      this.WeekGrid.Visibility = Visibility.Visible;
      this.MultiDay.SetMode(this.HeadView.Mode);
      this.MultiDay.SetDisplayDays(days, !isWeek || LocalSettings.Settings.ShowCalWeekend);
      this.MultiDay.GoTo(date ?? this.HeadView.GetCurrentMonthDate(), true, false);
      if (date.HasValue)
        this.MultiDay.FlashNaviDate(date.Value, true);
      this.MultiDay.TryShowTooltip();
      this.SetCalendarSideSelectRange();
    }

    private void LoadDefault()
    {
      switch (LocalSettings.Settings.CalendarDisplaySettings.GetHeadSwitch(this._inWidget))
      {
        case 1:
          this.SwitchMultiDay(false, true, 0);
          break;
        case 2:
          this.SwitchMultiDay(true, false, 1);
          break;
        case 3:
          this.SwitchMultiDay(false, false, Math.Min(14, Math.Max(1, LocalSettings.Settings.CalendarDisplaySettings.GetMultiNum(this._inWidget))));
          break;
        case 4:
          this.SwitchMultiWeeks(false, Math.Min(6, Math.Max(2, LocalSettings.Settings.CalendarDisplaySettings.GetMultiNum(this._inWidget))));
          break;
        default:
          this.SwitchMultiWeeks(true);
          break;
      }
    }

    private void OnControlLoaded(object sender, EventArgs e)
    {
      this.LoadDisplaySettings();
      this.NotifyAllDayHeightChanged();
      this.BindEvents();
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      Size size = e.NewSize;
      double width1 = size.Width;
      size = e.PreviousSize;
      double width2 = size.Width;
      if (Math.Abs(width1 - width2) <= 0.001)
        return;
      this._delayResize?.TryDoAction();
    }

    private void Resize(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.Resize));
    }

    private void Resize()
    {
      if (!this.IsVisible || !this.WeekGrid.IsVisible)
        return;
      this.MultiDay?.Resize();
    }

    private void LoadDisplaySettings()
    {
      this.HeadView.SetMode(LocalSettings.Settings.CalendarDisplaySettings.GetHeadMode(this._inWidget));
      CalSideBarDisplayMode sideBar = (CalSideBarDisplayMode) LocalSettings.Settings.CalendarDisplaySettings.GetSideBar(this._inWidget);
      this.SideDisplayMode = sideBar;
      switch (sideBar)
      {
        case CalSideBarDisplayMode.Arrange:
          this.ArrangePanel.Opacity = 1.0;
          this.ArrangePanel.IsHitTestVisible = true;
          this.ArrangePanel.SetInDisplay(true);
          this.CalendarSide.Opacity = 0.0;
          this.CalendarSide.IsHitTestVisible = false;
          break;
        case CalSideBarDisplayMode.None:
          this.CalendarSide.Opacity = 0.0;
          this.CalendarSide.IsHitTestVisible = false;
          this.SideViewGrid.Visibility = Visibility.Collapsed;
          this.WeekGrid.Margin = new Thickness(0.0, 0.0, 3.0, 0.0);
          break;
      }
      this.HeadView.SetShowCalendarSide(this.SideDisplayMode == CalSideBarDisplayMode.Cal);
    }

    public IDragBarEvent GetDragEvent()
    {
      if (this.MultiWeek != null && this.MonthGrid.Visibility == Visibility.Visible)
        return (IDragBarEvent) this.MultiWeek;
      return this.MultiDay != null && this.WeekGrid.Visibility == Visibility.Visible ? (IDragBarEvent) this.MultiDay : (IDragBarEvent) null;
    }

    public void NavigateDate(DateTime date, CalendarDisplayMode mode)
    {
      if (this.HeadView.Mode == mode.ToString())
        return;
      this.HeadView.SelectedDate = new DateTime?(date);
      this.HeadView.WeekMonthSwitch.NavigateMode(((int) mode).ToString());
      if (mode == CalendarDisplayMode.Day)
      {
        this.HeadView.SetMode("2");
        this.SwitchMultiDay(true, false, 1, new DateTime?(date));
      }
      else
      {
        this.HeadView.SetMode("1");
        this.SwitchMultiDay(false, true, 1, new DateTime?(date));
      }
    }

    public void SetEditting(bool editting) => this._editting = editting;

    public bool IsEditting() => this._editting;

    public void OnTouchScroll(int offset)
    {
      if (!PopupStateManager.CanShowAddPopup() || !(this.Navigate() is MultiDayView multiDayView))
        return;
      if (Math.Abs(offset) > 1)
        offset /= 2;
      multiDayView.OnTouchScroll(-1.0 * (double) offset);
    }

    public void MoveLastOrNext(bool next)
    {
      Utils.RunOnUiThread(this.Dispatcher, (Action) (() =>
      {
        this.HeadView.AddWeekMonthAction();
        if (next)
          this.MoveNext((object) null, (EventArgs) null);
        else
          this.MoveLast((object) null, (EventArgs) null);
      }));
    }

    public void OnKeyUp(Key key)
    {
      if (!PopupStateManager.CanShowAddPopup() || (!this._lastMouseMoveTime.HasValue ? 1000.0 : (DateTime.Now - this._lastMouseMoveTime.Value).TotalMilliseconds) <= 100.0)
        return;
      this._lastMouseMoveTime = new DateTime?(DateTime.Now);
      switch (key)
      {
        case Key.Prior:
        case Key.Left:
          if (Utils.IfCtrlPressed() || Utils.IfShiftPressed())
            break;
          this.MoveLastOrNext(false);
          break;
        case Key.Next:
        case Key.Right:
          if (Utils.IfCtrlPressed() || Utils.IfShiftPressed())
            break;
          this.MoveLastOrNext(true);
          break;
        case Key.Up:
          if (this.MultiWeek == null || this.MonthGrid.Visibility != Visibility.Visible)
            break;
          this.MoveLastOrNext(false);
          break;
        case Key.Down:
          if (this.MultiWeek == null || this.MonthGrid.Visibility != Visibility.Visible)
            break;
          this.MoveLastOrNext(true);
          break;
        case Key.D1:
        case Key.D2:
        case Key.D3:
        case Key.D:
        case Key.M:
        case Key.W:
          if (Utils.IfModifierKeyDown() || Utils.IfWinPressed())
            break;
          this.HeadView.OnSwitchKeyUp(key);
          break;
      }
    }

    public void NotifyAllDayHeightChanged()
    {
      if (this.MultiDay == null)
        return;
      this.MultiDay.SetFirstRowHeight();
    }

    public bool IsFilterAll() => this.CalendarSide.IsAll;

    public void Print(bool isAll)
    {
      if (this.MultiWeek != null && this.MonthGrid.Visibility == Visibility.Visible)
      {
        this.MultiWeek.PrintMonthCalendar(isAll);
      }
      else
      {
        if (this.MultiDay == null || this.WeekGrid.Visibility != Visibility.Visible)
          return;
        this.MultiDay.PrintWeekCalendar(isAll);
      }
    }

    public async Task<string> TryCopy(CalendarDisplayViewModel origin)
    {
      if (origin.IsCalendarEvent)
        return (string) null;
      return !string.IsNullOrEmpty(origin.ItemId) ? (await TaskDetailItemDao.CopyItem(origin.ItemId)).id : (await TaskService.CopyTask(origin.GetTaskId(), true, delayNotify: true)).id;
    }

    public async void GetFocus(bool force = false)
    {
      CalendarControl calendarControl = this;
      await Task.Delay(200);
      if (!force && !PopupStateManager.CanShowAddPopup())
        return;
      Window window = Window.GetWindow((DependencyObject) calendarControl);
      if (window == null || !window.IsActive)
        return;
      FocusManager.SetFocusedElement((DependencyObject) window, (IInputElement) window);
      Keyboard.Focus((IInputElement) window);
    }

    private void OnShortCutChanged(object sender, string key)
    {
      switch (key)
      {
        case "Print":
          KeyBindingManager.SetKeyGesture("Print", this.InputBindings[0] as KeyBinding);
          break;
        case "PrintDetail":
          KeyBindingManager.SetKeyGesture("PrintDetail", this.InputBindings[1] as KeyBinding);
          break;
      }
    }

    public void SetColorOpacity(float opacity)
    {
    }

    private void OnAddTaskClick(object sender, DateTime? date)
    {
      TaskDetailViewModel model = TaskDetailViewModel.BuildInitModel((TaskBaseViewModel) null);
      model.SourceViewModel.StartDate = date;
      string defaultAddProjectId = CalendarUtils.GetCalendarDefaultAddProjectId();
      model.SourceViewModel.ProjectId = defaultAddProjectId;
      TaskDetailPopup taskDetailPopup = new TaskDetailPopup();
      IToastShowWindow parent = Utils.FindParent<IToastShowWindow>((DependencyObject) this);
      if (parent != null)
      {
        if (PopupStateManager.LastTarget == this)
          return;
        taskDetailPopup.DependentWindow = parent;
      }
      taskDetailPopup.Disappear -= new EventHandler<string>(this.OnDetailClosed);
      taskDetailPopup.Disappear += new EventHandler<string>(this.OnDetailClosed);
      Grid target = sender as Grid;
      taskDetailPopup.Show(model, string.Empty, new TaskWindowDisplayArgs((UIElement) target, 40.0, false, 0));
      this.SetEditting(true);
    }

    private string GetWidgetThemeId()
    {
      CalendarWidget parent = Utils.FindParent<CalendarWidget>((DependencyObject) this);
      return parent != null ? parent.ThemeId : string.Empty;
    }

    private void OnDetailClosed(object sender, string e) => this.SetEditting(false);

    private void ShowSubscribe()
    {
      UserActCollectUtils.AddClickEvent("calendar", "toolbar", "subscribe_calendar");
      SettingsHelper.PullRemoteSettings();
      SettingDialog.ShowSettingDialog(SettingsType.Calendar, Window.GetWindow((DependencyObject) this));
    }

    public void ShowDisplaySetting()
    {
      Window parentWindow = Utils.GetParentWindow((DependencyObject) this);
      CalendarDisplaySettingWindow displaySettingWindow = new CalendarDisplaySettingWindow();
      displaySettingWindow.Owner = parentWindow;
      UserActCollectUtils.AddClickEvent("calendar", "toolbar", "view_options");
      displaySettingWindow.ShowDialog();
    }

    public void ShowOrHideArrange()
    {
      this.CalendarSide.IsHitTestVisible = false;
      this.ArrangePanel.IsHitTestVisible = true;
      switch (this.SideDisplayMode)
      {
        case CalSideBarDisplayMode.Cal:
          this.SideDisplayMode = CalSideBarDisplayMode.Arrange;
          this.StartShowAndHideStory((UIElement) this.CalendarSide, (UIElement) this.ArrangePanel, true);
          break;
        case CalSideBarDisplayMode.Arrange:
          this.ShowOrHideArrangePanel(false);
          this.SideDisplayMode = CalSideBarDisplayMode.None;
          break;
        case CalSideBarDisplayMode.None:
          this.ShowOrHideArrangePanel(true);
          this.StartShowAndHideStory((UIElement) this.CalendarSide, (UIElement) this.ArrangePanel, false);
          this.SideDisplayMode = CalSideBarDisplayMode.Arrange;
          break;
      }
      if (this.SideDisplayMode == CalSideBarDisplayMode.Arrange)
        UserActCollectUtils.AddClickEvent("calendar", "toolbar", "arrangement");
      this.ArrangePanel.SetInDisplay(this.SideDisplayMode == CalSideBarDisplayMode.Arrange);
      this.HeadView.SetShowCalendarSide(this.SideDisplayMode == CalSideBarDisplayMode.Cal);
    }

    public void ShowOrHideCalSide()
    {
      this.ArrangePanel.IsHitTestVisible = false;
      this.CalendarSide.IsHitTestVisible = true;
      UserActCollectUtils.AddClickEvent("calendar", "toolbar", "filter");
      switch (this.SideDisplayMode)
      {
        case CalSideBarDisplayMode.Cal:
          this.ShowOrHideArrangePanel(false);
          this.SideDisplayMode = CalSideBarDisplayMode.None;
          break;
        case CalSideBarDisplayMode.Arrange:
          this.SideDisplayMode = CalSideBarDisplayMode.Cal;
          this.StartShowAndHideStory((UIElement) this.ArrangePanel, (UIElement) this.CalendarSide, true);
          break;
        case CalSideBarDisplayMode.None:
          this.ShowOrHideArrangePanel(true);
          this.StartShowAndHideStory((UIElement) this.ArrangePanel, (UIElement) this.CalendarSide, false);
          this.SideDisplayMode = CalSideBarDisplayMode.Cal;
          break;
      }
      this.HeadView.SetShowCalendarSide(this.SideDisplayMode == CalSideBarDisplayMode.Cal);
    }

    private void StartShowAndHideStory(UIElement hideUi, UIElement showUi, bool withStory)
    {
      hideUi.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
      showUi.BeginAnimation(UIElement.OpacityProperty, (AnimationTimeline) null);
      hideUi.Opacity = 0.0;
      showUi.Opacity = 1.0;
    }

    private void OnCloseArrangeTask(object sender, EventArgs e)
    {
      this.ShowOrHideArrangePanel(false);
      this.SideDisplayMode = CalSideBarDisplayMode.None;
    }

    private async void ShowOrHideArrangePanel(bool show)
    {
      CalendarControl calendarControl = this;
      if (show)
      {
        calendarControl.SideViewGrid.Visibility = Visibility.Visible;
        calendarControl.WeekGrid.Margin = new Thickness(0.0, 0.0, 1.0, 0.0);
      }
      else
      {
        calendarControl.SideViewGrid.Visibility = Visibility.Collapsed;
        calendarControl.WeekGrid.Margin = new Thickness(0.0, 0.0, 3.0, 0.0);
      }
      if (calendarControl.MultiDay == null || calendarControl.WeekGrid.Visibility != Visibility.Visible)
        return;
      calendarControl.UpdateLayout();
      calendarControl.Resize();
    }

    private async void OnSideFoldChagned(object sender, EventArgs e)
    {
      if (this.MultiDay == null || this.WeekGrid.Visibility != Visibility.Visible)
        return;
      await Task.Delay(50);
      this.Resize();
    }

    internal void SetBlur()
    {
      this.CalendarBlurEffect.Radius = UserDao.IsPro() ? 0.0 : 8.0;
      this.NotProBorder.Visibility = Visibility.Collapsed;
      this.CalendarBody.IsEnabled = UserDao.IsPro();
      this.HeadView.SetEnable(UserDao.IsPro());
    }

    public void ClearData()
    {
      this.MultiWeek?.ClearItems();
      this.MultiDay?.ClearItems();
      this.ArrangePanel.ClearItems();
    }

    public void RemoveItem(CalendarDisplayViewModel model)
    {
      this.Navigate()?.RemoveItem(model);
      this.ArrangePanel.RemoveItem(model);
    }

    public void ShowProToast()
    {
      if (UserDao.IsPro())
      {
        if (this.CalendarBody.IsEnabled)
          return;
        this.CalendarBody.IsEnabled = true;
        this.HeadView.SetEnable(true);
        this.NotProBorder.Visibility = Visibility.Collapsed;
        this.Reload();
      }
      else
      {
        if (this.MonthGrid.Visibility != Visibility.Visible)
          this.SwitchMultiWeeks(true);
        this.ShowOrHideArrangePanel(false);
        this.SideDisplayMode = CalSideBarDisplayMode.None;
        this.HeadView.SetShowCalendarSide(false);
        this.MultiWeek.Today();
        this.MultiWeek.LoadVirtualModels();
        this.CalendarBody.IsEnabled = false;
        this.HeadView.SetEnable(false);
        this.CalendarBlurEffect.Radius = 0.0;
        this.NotProBorder.Visibility = Visibility.Visible;
      }
    }

    private void ProClick(object sender, MouseButtonEventArgs e)
    {
      Utils.StartUpgrade(this._inWidget ? "calendar_widget" : "calendar_view");
    }

    public void CollectUserEvent()
    {
      UserActCollectUtils.AddClickEvent("calendar", "element_show", this.SideDisplayMode == CalSideBarDisplayMode.Arrange ? "arrangement" : (this.SideDisplayMode == CalSideBarDisplayMode.Cal ? "filter" : "view_show"));
      if (!LocalSettings.Settings.EnableLunar)
        return;
      UserActCollectUtils.AddClickEvent("calendar", "element_show", "lunar_calendar");
    }

    public void OnDoubleFingerTouch()
    {
      if (this.MultiDay != null && this.MultiDay.IsMouseOver)
        this.MultiDay.OnDoubleFingerTouch();
      if (this.MultiWeek == null || !this.MultiWeek.IsMouseOver)
        return;
      this.MultiWeek.OnDoubleFingerTouch();
    }

    public void SetDateText(DateTime? date) => this.HeadView.SetDateText(date);

    public void AddPopupBorder(Border dragPopup)
    {
      dragPopup.SetValue(Grid.ColumnSpanProperty, (object) 2);
      this.CalendarBody.Children.Add((UIElement) dragPopup);
      dragPopup.SetValue(Panel.ZIndexProperty, (object) 1000);
    }

    private void OnScrollChanged(object sender, MouseWheelEventArgs e)
    {
      if (!PopupStateManager.CanShowAddPopup() || !this.CalendarSide.DayPicker.IsMouseOver && !this.HeadView.IsMouseOver || this._lastScrollTime.HasValue && (DateTime.Now - this._lastScrollTime.Value).TotalMilliseconds < 500.0)
        return;
      this._lastScrollTime = new DateTime?(DateTime.Now);
      int delta = e.Delta;
      if (delta > 0)
      {
        this.MoveLast((object) null, (EventArgs) null);
      }
      else
      {
        if (delta >= 0)
          return;
        this.MoveNext((object) null, (EventArgs) null);
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/calendarcontrol.xaml", UriKind.Relative));
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
          this.Root = (CalendarControl) target;
          this.Root.MouseWheel += new MouseWheelEventHandler(this.OnScrollChanged);
          this.Root.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
          break;
        case 2:
          this.CalendarBlurEffect = (BlurEffect) target;
          break;
        case 3:
          this.HeadView = (CalendarHead) target;
          break;
        case 4:
          this.CalendarBody = (Grid) target;
          break;
        case 5:
          this.MonthGrid = (Grid) target;
          break;
        case 6:
          this.WeekGrid = (Grid) target;
          break;
        case 7:
          this.SideViewGrid = (Grid) target;
          break;
        case 8:
          this.CalendarSide = (CalendarSideBar) target;
          break;
        case 9:
          this.ArrangePanel = (ArrangeTaskPanel) target;
          break;
        case 10:
          this.NotProBorder = (Border) target;
          break;
        case 11:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ProClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
