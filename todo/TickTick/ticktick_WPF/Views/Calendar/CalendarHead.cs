// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarHead
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Time;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarHead : UserControl, IComponentConnector
  {
    private CalendarControl _calendarControl;
    private int _previousMode;
    private ProjectOrGroupPopup _popup;
    private WidgetMorePopup _morePopup;
    private string _mode = "0";
    private DateTime _startDate;
    internal CalendarHead Root;
    internal Rectangle DragPanel;
    internal StackPanel DatePanel;
    internal TextBlock MonthText;
    internal Run MonthRun;
    internal Run YearRun;
    internal WeekMonthSwitch WeekMonthSwitch;
    internal Border LastBorder;
    internal Path LeftArrow;
    internal Border TodayBorder;
    internal TextBlock TodayText;
    internal Border NextBorder;
    internal Path RightArrow;
    internal StackPanel LockedOptionGrid;
    internal Button UnlockButton;
    internal Button LockedSyncButton;
    internal Image LockedSyncImage;
    internal StackPanel UnlockOptionGrid;
    internal Button ShowFilterButton;
    internal Image ShowCalendarSideImage;
    internal Button MoreButton;
    internal Path UnlockedSyncPath;
    private bool _contentLoaded;

    public event EventHandler<WidgetMoreAction> MoreAction;

    public event EventHandler<string> Action;

    public CalendarHead()
    {
      this.InitializeComponent();
      this.WeekMonthSwitch.SwitchMode += (WeekMonthSwitch.SwitchModeDelegate) ((from, to) =>
      {
        this.Mode = to;
        WeekMonthSwitch.SwitchModeDelegate switchView = this.SwitchView;
        if (switchView == null)
          return;
        switchView(from, to);
      });
      this.SetDateText();
    }

    private void SetDateText()
    {
      if (Utils.IsEmptyDate(this.StartDate))
      {
        this.EndDate = DateTime.Today;
        this._startDate = DateTime.Today;
      }
      DateTime currentMonthDate = this.GetCurrentMonthDate();
      this.MonthRun.Text = DateUtils.FormatMonth(currentMonthDate);
      this.YearRun.Text = DateUtils.FormatYear(currentMonthDate);
    }

    public string Mode
    {
      get => this._mode;
      set
      {
        this._mode = value;
        if (this._mode == "0" || this._mode.StartsWith("W"))
        {
          this.LeftArrow.RenderTransform = (Transform) new RotateTransform(180.0);
          this.RightArrow.RenderTransform = (Transform) new RotateTransform(0.0);
        }
        else
        {
          this.LeftArrow.RenderTransform = (Transform) new RotateTransform(90.0);
          this.RightArrow.RenderTransform = (Transform) new RotateTransform(-90.0);
        }
      }
    }

    public DateTime StartDate
    {
      get => this._startDate;
      set
      {
        this._startDate = value;
        this.SetDateText();
        if (!this.SelectedDate.HasValue)
          return;
        int month = this.GetCurrentMonthDate().Month;
        DateTime? selectedDate = this.SelectedDate;
        ref DateTime? local = ref selectedDate;
        int? nullable = local.HasValue ? new int?(local.GetValueOrDefault().Month) : new int?();
        int valueOrDefault = nullable.GetValueOrDefault();
        if (month == valueOrDefault & nullable.HasValue)
          return;
        this.SelectedDate = new DateTime?();
      }
    }

    public DateTime EndDate { get; set; }

    public DateTime? SelectedDate { get; set; }

    public event WeekMonthSwitch.SwitchModeDelegate SwitchView;

    public event EventHandler Today;

    public event EventHandler Last;

    public event EventHandler Next;

    public event EventHandler<DateTime> DateSelect;

    public CalendarControl GetParent()
    {
      this._calendarControl = this._calendarControl ?? Utils.FindParent<CalendarControl>((DependencyObject) this);
      return this._calendarControl;
    }

    private void OnTodayClick(object sender, RoutedEventArgs e)
    {
      EventHandler today = this.Today;
      if (today != null)
        today(sender, (EventArgs) null);
      UserActCollectUtils.AddClickEvent("calendar", "action", "back_to_today");
    }

    private void OnMoreClick(object sender, RoutedEventArgs e)
    {
      CalendarControl parent = this.GetParent();
      if ((parent != null ? (parent.InWidget ? 1 : 0) : 0) != 0)
      {
        this.GetParent()?.SetEditting(true);
        if (this._morePopup == null)
        {
          this._morePopup = new WidgetMorePopup(new System.Action(this.OnPopupClosed));
          this._morePopup.SetPlaceTarget((UIElement) this.MoreButton);
          this._morePopup.MoreAction += new EventHandler<WidgetMoreAction>(this.OnMoreMoreAction);
          this._morePopup.Action += new EventHandler<string>(this.OnActionSelected);
        }
        this._morePopup.Show(new System.Windows.Point(-5.0, -10.0), true);
      }
      else
      {
        CalMoreItems calMoreItems = new CalMoreItems((UIElement) this.MoreButton);
        calMoreItems.Show();
        calMoreItems.Action += new EventHandler<string>(this.OnActionSelected);
        calMoreItems.Show();
      }
    }

    private void OnActionSelected(object sender, string e)
    {
      EventHandler<string> action = this.Action;
      if (action == null)
        return;
      action((object) this, e);
    }

    private void OnMoreMoreAction(object sender, WidgetMoreAction e)
    {
      EventHandler<WidgetMoreAction> moreAction = this.MoreAction;
      if (moreAction == null)
        return;
      moreAction((object) this, e);
    }

    private void OnLastClick(object sender, RoutedEventArgs e)
    {
      EventHandler last = this.Last;
      if (last != null)
        last(sender, (EventArgs) null);
      UserActCollectUtils.AddClickEvent("calendar", "action", "back_btn");
      this.AddWeekMonthAction();
    }

    public void AddWeekMonthAction()
    {
      if (!(this.Mode == "1") && !(this.Mode == "0"))
        return;
      string mode = this.Mode;
      DelayActionHandlerCenter.TryDoAction("AddCalLastNextClickAction", (EventHandler) ((o, args) => UserActCollectUtils.AddClickEvent("calendar", mode == "1" ? "week_action" : "month_action", mode == "1" ? "swipe_vertical" : "swipe_horizontal")), 3000);
    }

    private void OnNextClick(object sender, RoutedEventArgs e)
    {
      EventHandler next = this.Next;
      if (next != null)
        next(sender, (EventArgs) null);
      UserActCollectUtils.AddClickEvent("calendar", "action", "forward_btn");
      this.AddWeekMonthAction();
    }

    private void OnDateSelect(object sender, MouseButtonEventArgs e)
    {
      MonthPicker monthPicker = new MonthPicker(DateUtils.GetCurrentMonthDate(this.StartDate, this.EndDate));
      this.GetParent()?.SetEditting(true);
      Mouse.GetPosition((IInputElement) this.MonthText);
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.PopupAnimation = PopupAnimation.Fade;
      escPopup.Placement = PlacementMode.Bottom;
      escPopup.PlacementTarget = (UIElement) this.DatePanel;
      escPopup.HorizontalOffset = 10.0;
      escPopup.VerticalOffset = -2.0;
      EscPopup popup = escPopup;
      popup.Child = (UIElement) monthPicker;
      popup.IsOpen = true;
      popup.Closed += (EventHandler) ((o, args) => this.OnPopupClosed());
      monthPicker.MonthSelected += (EventHandler<DateTime>) (async (o, time) =>
      {
        popup.IsOpen = false;
        EventHandler<DateTime> dateSelect = this.DateSelect;
        if (dateSelect != null)
          dateSelect((object) this, time);
        UserActCollectUtils.AddClickEvent("calendar", "action", "switch_month");
      });
    }

    public void SetWidgetMode()
    {
    }

    private void OnCalendarHeadClick(object sender, MouseButtonEventArgs e)
    {
    }

    private void OnPopupClosed() => this.GetParent()?.SetEditting(false);

    public void BeginSyncStory()
    {
      this.LockedSyncButton.IsHitTestVisible = false;
      this.MoreButton.Visibility = Visibility.Collapsed;
      ((Storyboard) this.FindResource(this.LockedOptionGrid.IsVisible ? (object) "LockedSyncStory" : (object) "UnlockedSyncStory")).Begin();
    }

    private void SyncStoryCompleted(object sender, EventArgs e)
    {
      this.MoreButton.Visibility = Visibility.Visible;
      this.LockedSyncButton.IsHitTestVisible = true;
    }

    public void OnSwitchKeyUp(Key key) => this.WeekMonthSwitch.QuickSwitch(key);

    public void SetEnable(bool enable) => this.WeekMonthSwitch.IsEnabled = enable;

    private void OnShowFilterClick(object sender, RoutedEventArgs e)
    {
      this.GetParent()?.ShowOrHideCalSide();
    }

    public void SetMode(string mode)
    {
      this.Mode = mode;
      if (this.WeekMonthSwitch.DataContext is WeekMonthViewModel dataContext)
        dataContext.Mode = mode;
      else
        this.WeekMonthSwitch.DataContext = (object) new WeekMonthViewModel()
        {
          Mode = mode
        };
      this.SetDateText();
    }

    public DateTime GetCurrentMonthDate(bool checkToday = false)
    {
      DateTime currentMonthDate = DateUtils.GetCurrentMonthDate(this.StartDate, this.EndDate);
      if (this.SelectedDate.HasValue && this.SelectedDate.Value.Month == currentMonthDate.Month && this.SelectedDate.Value >= this.StartDate && this.SelectedDate.Value <= this.EndDate)
        return this.SelectedDate.Value;
      if (DateTime.Today.Month == currentMonthDate.Month | checkToday && DateTime.Today >= this.StartDate && DateTime.Today <= this.EndDate)
        return DateTime.Today;
      return currentMonthDate.Month != this.StartDate.Month ? currentMonthDate : this.StartDate;
    }

    public List<DateTime> GetRangeDates()
    {
      int days = (this.EndDate - this._startDate).Days;
      List<DateTime> rangeDates = new List<DateTime>();
      for (int index = 0; index <= days; ++index)
        rangeDates.Add(this._startDate.AddDays((double) index));
      return rangeDates;
    }

    public void SetShowCalendarSide(bool isShow)
    {
      this.ShowCalendarSideImage.SetResourceReference(Image.SourceProperty, isShow ? (object) "HideCalendarFilterDrawingImage" : (object) "ShowCalendarFilterDrawingImage");
      this.ShowCalendarSideImage.ToolTip = (object) Utils.GetString(isShow ? "CollapseCal" : "OpenCal");
    }

    public void SetDateText(DateTime? date)
    {
      if (!date.HasValue)
      {
        this.SetDateText();
      }
      else
      {
        this.MonthRun.Text = DateUtils.FormatMonth(date.Value);
        this.YearRun.Text = DateUtils.FormatYear(date.Value);
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/calendarhead.xaml", UriKind.Relative));
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
          this.Root = (CalendarHead) target;
          break;
        case 2:
          ((Timeline) target).Completed += new EventHandler(this.SyncStoryCompleted);
          break;
        case 3:
          ((Timeline) target).Completed += new EventHandler(this.SyncStoryCompleted);
          break;
        case 4:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCalendarHeadClick);
          break;
        case 5:
          this.DragPanel = (Rectangle) target;
          break;
        case 6:
          this.DatePanel = (StackPanel) target;
          this.DatePanel.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnDateSelect);
          break;
        case 7:
          this.MonthText = (TextBlock) target;
          break;
        case 8:
          this.MonthRun = (Run) target;
          break;
        case 9:
          this.YearRun = (Run) target;
          break;
        case 10:
          this.WeekMonthSwitch = (WeekMonthSwitch) target;
          break;
        case 11:
          this.LastBorder = (Border) target;
          this.LastBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnLastClick);
          break;
        case 12:
          this.LeftArrow = (Path) target;
          break;
        case 13:
          this.TodayBorder = (Border) target;
          this.TodayBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnTodayClick);
          break;
        case 14:
          this.TodayText = (TextBlock) target;
          break;
        case 15:
          this.NextBorder = (Border) target;
          this.NextBorder.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnNextClick);
          break;
        case 16:
          this.RightArrow = (Path) target;
          break;
        case 17:
          this.LockedOptionGrid = (StackPanel) target;
          break;
        case 18:
          this.UnlockButton = (Button) target;
          break;
        case 19:
          this.LockedSyncButton = (Button) target;
          break;
        case 20:
          this.LockedSyncImage = (Image) target;
          break;
        case 21:
          this.UnlockOptionGrid = (StackPanel) target;
          break;
        case 22:
          this.ShowFilterButton = (Button) target;
          this.ShowFilterButton.Click += new RoutedEventHandler(this.OnShowFilterClick);
          break;
        case 23:
          this.ShowCalendarSideImage = (Image) target;
          break;
        case 24:
          this.MoreButton = (Button) target;
          this.MoreButton.Click += new RoutedEventHandler(this.OnMoreClick);
          break;
        case 25:
          this.UnlockedSyncPath = (Path) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
