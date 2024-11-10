// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.SetDateDurationControl
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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class SetDateDurationControl : UserControl, IComponentConnector
  {
    private List<Popup> _innerPopups;
    private TimeZoneSelectControl _durationTz;
    private SelectDateDialog _setDateDialog;
    public SetDateDialog ParentWindow;
    private bool _showPopupButtonMouseDown;
    private int _tabIndex;
    internal Grid StartDateText;
    internal Border StartDateBorder;
    internal TimeInputControl StartTimeInput;
    internal Grid EndDateText;
    internal Border DueDateBorder;
    internal TimeInputControl DueTimeInput;
    internal Popup SetDatePopup;
    internal CheckBox AllDayCheckBox;
    internal Rectangle AllDayBorder;
    internal Grid TimeZone;
    internal Grid TimeZoneGrid;
    private bool _contentLoaded;

    private TimeData TimeData => (TimeData) this.DataContext;

    public event EventHandler PopupOpened;

    public event EventHandler PopupClosed;

    public SetDateDurationControl()
    {
      this.InitializeComponent();
      this._innerPopups = new List<Popup>()
      {
        this.SetDatePopup,
        this.StartTimeInput.DropdownPopup,
        this.DueTimeInput.DropdownPopup
      };
      foreach (Popup innerPopup in this._innerPopups)
      {
        innerPopup.Opened += new EventHandler(this.OnPopupOpened);
        innerPopup.Closed += new EventHandler(this.OnPopupClosed);
      }
      this.StartTimeInput.ReminderPopup.HorizontalOffset = -150.0;
      this.DueTimeInput.ReminderPopup.HorizontalOffset = -150.0;
    }

    private void OnPopupOpened(object sender, EventArgs e)
    {
      EventHandler popupOpened = this.PopupOpened;
      if (popupOpened == null)
        return;
      popupOpened((object) this, (EventArgs) null);
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      EventHandler popupClosed = this.PopupClosed;
      if (popupClosed == null)
        return;
      popupClosed((object) this, (EventArgs) null);
    }

    public void SetData()
    {
      if (this._durationTz == null)
      {
        this._durationTz = new TimeZoneSelectControl(this.TimeData.TimeZone);
        this._durationTz.OnTimeZoneChanged += (EventHandler<TimeZoneViewModel>) ((obj, model) => this.TimeData.TimeZone = model);
        this.TimeZoneGrid.Children.Add((UIElement) this._durationTz);
        this._durationTz.OptionPopup.Opened += new EventHandler(this.OnPopupOpened);
        this._durationTz.OptionPopup.Closed += new EventHandler(this.OnPopupClosed);
        this._innerPopups.Add(this._durationTz.OptionPopup);
      }
      else
        this._durationTz.SetData(this.TimeData.TimeZone);
      this.InitDurationTimeControl();
    }

    private void InitDurationTimeControl()
    {
      this.StartTimeInput.SelectedTimeChanged -= new EventHandler<DateTime>(this.OnStartTimeSelected);
      this.StartTimeInput.SelectedTimeChanged += new EventHandler<DateTime>(this.OnStartTimeSelected);
      this.DueTimeInput.SelectedTimeChanged -= new EventHandler<DateTime>(this.OnDueTimeSelected);
      this.DueTimeInput.SelectedTimeChanged += new EventHandler<DateTime>(this.OnDueTimeSelected);
      this.NotifyTimeChanged();
      this.DueTimeInput.BeginTime = this.TimeData.StartDate;
    }

    private void OnDueTimeSelected(object sender, DateTime time)
    {
      if (!this.TimeData.DueDate.HasValue || !this.TimeData.IsAllDay.HasValue || this.TimeData.IsAllDay.Value)
        return;
      DateTime dateTime1 = DateUtils.SetHourAndMinuteOnly(time, time.Hour, time.Minute);
      TimeData timeData = this.TimeData;
      DateTime? startDate = this.TimeData.StartDate;
      DateTime dateTime2 = dateTime1;
      DateTime? nullable = (startDate.HasValue ? (startDate.GetValueOrDefault() > dateTime2 ? 1 : 0) : 0) != 0 ? this.TimeData.StartDate : new DateTime?(dateTime1);
      timeData.DueDate = nullable;
      this.NotifyTimeChanged(true);
    }

    private void OnStartTimeSelected(object sender, DateTime time)
    {
      if (!this.TimeData.StartDate.HasValue || !this.TimeData.DueDate.HasValue)
        return;
      DateTime dateTime1 = this.TimeData.DueDate.Value;
      DateTime? startDate = this.TimeData.StartDate;
      DateTime dateTime2 = startDate.Value;
      double totalMinutes = (dateTime1 - dateTime2).TotalMinutes;
      TimeData timeData1 = this.TimeData;
      startDate = this.TimeData.StartDate;
      DateTime? nullable1 = new DateTime?(DateUtils.SetHourAndMinuteOnly(startDate.Value, time.Hour, time.Minute));
      timeData1.StartDate = nullable1;
      TimeData timeData2 = this.TimeData;
      startDate = this.TimeData.StartDate;
      DateTime? nullable2 = new DateTime?(startDate.Value.AddMinutes(totalMinutes));
      timeData2.DueDate = nullable2;
      this.DueTimeInput.BeginTime = this.TimeData.StartDate;
      this.NotifyTimeChanged(true);
    }

    private void NotifyTimeChanged(bool exceptStart = false, bool exceptDue = false)
    {
      DateTime? nullable;
      if (this.TimeData.StartDate.HasValue && !exceptStart)
      {
        TimeInputControl startTimeInput = this.StartTimeInput;
        nullable = this.TimeData.StartDate;
        DateTime dateTime = nullable.Value;
        startTimeInput.SelectedTime = dateTime;
      }
      nullable = this.TimeData.DueDate;
      if (!nullable.HasValue || exceptDue)
        return;
      TimeInputControl dueTimeInput = this.DueTimeInput;
      nullable = this.TimeData.DueDate;
      DateTime dateTime1 = nullable.Value;
      dueTimeInput.SelectedTime = dateTime1;
    }

    private void StartDateClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (!this._showPopupButtonMouseDown)
        return;
      this.ShowStartDatePopup();
    }

    private void ShowStartDatePopup(bool inTab = false)
    {
      this.PrepareOpenPopup();
      DurationModel model = new DurationModel()
      {
        SelectedDate = this.TimeData.StartDate,
        SelectionStart = this.TimeData.StartDate,
        SelectionEnd = this.TimeData.DueDate
      };
      if (this._setDateDialog == null)
        this._setDateDialog = new SelectDateDialog(this.SetDatePopup, model, true);
      else
        this._setDateDialog.SetData(model, true);
      this._setDateDialog.SetCurrentTab(inTab);
      this._setDateDialog.SelectDate -= new EventHandler<DateTime>(this.SelectDueDate);
      this._setDateDialog.SelectDate -= new EventHandler<DateTime>(this.SelectStartDate);
      this._setDateDialog.SelectDate += new EventHandler<DateTime>(this.SelectStartDate);
      this._setDateDialog.SelectDateAndSave -= new EventHandler<DateTime>(this.SelectSetDateAndSave);
      this._setDateDialog.SelectDateAndSave += new EventHandler<DateTime>(this.SelectSetDateAndSave);
      this.SetDatePopup.PlacementTarget = (UIElement) this.StartDateText;
      this._setDateDialog.Show();
      this.StartDateBorder.BorderThickness = new Thickness(1.0);
      this._showPopupButtonMouseDown = false;
    }

    private void SelectSetDateAndSave(object sender, DateTime e)
    {
      this.OnStartDateSelected(e);
      this.ParentWindow.SaveClick((object) null, (RoutedEventArgs) null);
    }

    private void SelectStartDate(object sender, DateTime date) => this.OnStartDateSelected(date);

    private void OnStartDateSelected(DateTime date)
    {
      this.TimeData.StartDate = new DateTime?(date);
      if (this.TimeData.StartDate.HasValue)
        this.StartTimeInput.SelectedTime = this.TimeData.StartDate.Value;
      DateTime? selectEnd = this._setDateDialog.SelectEnd;
      if (selectEnd.HasValue)
      {
        TimeData timeData = this.TimeData;
        selectEnd = this._setDateDialog.SelectEnd;
        DateTime? nullable = new DateTime?(selectEnd.Value);
        timeData.DueDate = nullable;
      }
      this.NotifyTimeChanged();
    }

    private void PrepareOpenPopup() => this.CloseInnerPopups();

    private bool CloseInnerPopups()
    {
      bool flag = false;
      foreach (Popup innerPopup in this._innerPopups)
      {
        if (innerPopup.IsOpen)
        {
          innerPopup.IsOpen = false;
          flag = true;
        }
      }
      return flag;
    }

    private void StartTimeClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.PrepareOpenPopup();
      this.StartTimeInput.ClearSelection();
    }

    private void EndDateClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      if (!this._showPopupButtonMouseDown)
        return;
      this.ShowEndDatePopup();
    }

    private void ShowEndDatePopup(bool inTab = false)
    {
      this.PrepareOpenPopup();
      DurationModel durationModel = new DurationModel()
      {
        SelectedDate = this.TimeData.DueDate,
        SelectionStart = this.TimeData.StartDate,
        SelectionEnd = this.TimeData.DueDate
      };
      if (this._setDateDialog == null)
      {
        Popup setDatePopup = this.SetDatePopup;
        DurationModel model = durationModel;
        ref DateTime? local = ref durationModel.SelectionStart;
        DateTime? nullable = local.HasValue ? new DateTime?(local.GetValueOrDefault().AddDays(-1.0)) : new DateTime?();
        DateTime? maxDate = new DateTime?();
        DateTime? minDate = nullable;
        this._setDateDialog = new SelectDateDialog(setDatePopup, model, false, maxDate, minDate);
      }
      else
      {
        SelectDateDialog setDateDialog = this._setDateDialog;
        DurationModel model = durationModel;
        ref DateTime? local = ref durationModel.SelectionStart;
        DateTime? nullable = local.HasValue ? new DateTime?(local.GetValueOrDefault().AddDays(-1.0)) : new DateTime?();
        DateTime? maxDate = new DateTime?();
        DateTime? minDate = nullable;
        setDateDialog.SetData(model, false, maxDate, minDate);
      }
      this._setDateDialog.SetCurrentTab(inTab);
      this._setDateDialog.SelectDate -= new EventHandler<DateTime>(this.SelectDueDate);
      this._setDateDialog.SelectDate += new EventHandler<DateTime>(this.SelectDueDate);
      this._setDateDialog.SelectDate -= new EventHandler<DateTime>(this.SelectStartDate);
      this._setDateDialog.SelectDateAndSave -= new EventHandler<DateTime>(this.SelectSetDateAndSave);
      this.SetDatePopup.PlacementTarget = (UIElement) this.EndDateText;
      this._setDateDialog.Show();
      this.DueDateBorder.BorderThickness = new Thickness(1.0);
      this._showPopupButtonMouseDown = false;
    }

    private void SelectDueDate(object sender, DateTime date)
    {
      TimeData timeData1 = this.TimeData;
      DateTime? nullable1 = this.TimeData.DueDate;
      DateTime? nullable2 = new DateTime?(DateUtils.SetDateOnly(nullable1 ?? date, date));
      timeData1.DueDate = nullable2;
      nullable1 = this._setDateDialog.SelectStart;
      if (nullable1.HasValue)
      {
        TimeData timeData2 = this.TimeData;
        nullable1 = this._setDateDialog.SelectStart;
        DateTime? nullable3 = new DateTime?(nullable1.Value);
        timeData2.StartDate = nullable3;
        nullable1 = this.TimeData.StartDate;
        DateTime? dueDate = this.TimeData.DueDate;
        DateTime dateTime;
        if ((nullable1.HasValue & dueDate.HasValue ? (nullable1.GetValueOrDefault() >= dueDate.GetValueOrDefault() ? 1 : 0) : 0) != 0 && this.TimeData.IsAllDay.HasValue && !this.TimeData.IsAllDay.Value)
        {
          TimeData timeData3 = this.TimeData;
          dueDate = this.TimeData.DueDate;
          dateTime = dueDate.Value;
          DateTime? nullable4 = new DateTime?(dateTime.AddHours(-1.0));
          timeData3.StartDate = nullable4;
        }
        dueDate = this.TimeData.DueDate;
        if (dueDate.HasValue && this.TimeData.IsAllDay.HasValue && this.TimeData.IsAllDay.Value)
        {
          dueDate = this.TimeData.DueDate;
          nullable1 = this.TimeData.StartDate;
          if ((dueDate.HasValue & nullable1.HasValue ? (dueDate.GetValueOrDefault() < nullable1.GetValueOrDefault() ? 1 : 0) : 0) != 0)
          {
            TimeData timeData4 = this.TimeData;
            nullable1 = this.TimeData.StartDate;
            dateTime = nullable1.Value;
            DateTime? nullable5 = new DateTime?(dateTime.AddDays(1.0));
            timeData4.DueDate = nullable5;
          }
        }
      }
      this.NotifyTimeChanged();
    }

    private void EndTimeClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.PrepareOpenPopup();
      this.DueTimeInput.ClearSelection();
    }

    private void OnAllDayClick(object sender, RoutedEventArgs e)
    {
      this.CheckAllDay(this.AllDayCheckBox.IsChecked.HasValue && this.AllDayCheckBox.IsChecked.Value);
    }

    private void CheckAllDay(bool check)
    {
      if (check)
      {
        if (this.TimeData.StartDate.HasValue && this.TimeData.DueDate.HasValue && this.TimeData.StartDate.Value.Date == this.TimeData.DueDate.Value.Date)
        {
          this.TimeData.DueDate = new DateTime?(this.TimeData.DueDate.Value.AddDays(1.0));
          this.NotifyTimeChanged();
        }
        this.TimeData.Reminders = TimeData.GetDefaultAllDayReminders();
      }
      else
      {
        this.TrySetNextHour(this.TimeData);
        this.TimeData.Reminders = TimeData.GetDefaultTimeReminders();
        this.NotifyTimeChanged();
      }
    }

    private void TrySetNextHour(TimeData timeData)
    {
      if (!timeData.StartDate.HasValue || !timeData.DueDate.HasValue)
        return;
      DateTime dateTime1 = timeData.StartDate.Value;
      DateTime dateTime2 = timeData.DueDate.Value;
      if (dateTime1.Hour != 0 || dateTime1.Minute != 0 || dateTime2.Hour != 0 || dateTime2.Minute != 0)
        return;
      DateTime dateTime3 = DateTime.Now;
      int hour = dateTime3.Hour;
      TimeData timeData1 = timeData;
      dateTime3 = DateUtils.SetHourAndMinuteOnly(timeData.StartDate.Value, hour, 0);
      DateTime? nullable1 = new DateTime?(dateTime3.AddHours(1.0));
      timeData1.StartDate = nullable1;
      TimeData timeData2 = timeData;
      dateTime3 = DateUtils.SetHourAndMinuteOnly(timeData.DueDate.Value, hour, 0);
      DateTime? nullable2 = new DateTime?(dateTime3.AddHours(2.0));
      timeData2.DueDate = nullable2;
    }

    private void ClosePopup(object sender, MouseButtonEventArgs e)
    {
      if (this._durationTz != null && this._durationTz.OptionPopup.IsOpen)
        return;
      this.CloseInnerPopups();
      this.StartTimeInput.ClearSelection();
      this.DueTimeInput.ClearSelection();
    }

    public void TryClosePopup()
    {
      foreach (Popup innerPopup in this._innerPopups)
        innerPopup.IsOpen = false;
    }

    private void DatePopupClosed(object sender, EventArgs e)
    {
      PopupStateManager.SetCanOpenTimePopup(true);
      this.StartDateBorder.BorderThickness = new Thickness((double) (this._tabIndex == 0 ? 1 : 0));
      this.DueDateBorder.BorderThickness = new Thickness((double) (this._tabIndex == 2 ? 1 : 0));
    }

    private void DatePopupOpened(object sender, EventArgs e)
    {
      PopupStateManager.SetCanOpenTimePopup(false);
    }

    private void OnShowPopupButtonMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (PopupStateManager.CanOpenTimePopup)
        this._showPopupButtonMouseDown = true;
      else
        PopupStateManager.SetCanOpenTimePopup(true);
    }

    public bool SetTabIndex(int index)
    {
      this._tabIndex = index;
      this.StartDateBorder.BorderThickness = new Thickness((double) (this._tabIndex == 0 ? 1 : 0));
      this.StartTimeInput.TabSelected = this._tabIndex == 1;
      this.DueDateBorder.BorderThickness = new Thickness((double) (this._tabIndex == 2 ? 1 : 0));
      this.DueTimeInput.TabSelected = this._tabIndex == 3;
      this.AllDayBorder.Opacity = (double) (this._tabIndex == 4 ? 1 : 0);
      if (this._durationTz != null)
        this._durationTz.TabSelected = this._tabIndex == 5;
      switch (this._tabIndex)
      {
        case 1:
          if (!this.StartTimeInput.IsVisible)
            goto case 6;
          else
            break;
        case 3:
          if (!this.DueTimeInput.IsVisible)
            goto case 6;
          else
            break;
        case 5:
          if (this.TimeZone.IsVisible)
            break;
          goto case 6;
        case 6:
          return false;
      }
      return true;
    }

    public bool HandleEnter()
    {
      switch (this._tabIndex)
      {
        case 0:
          if (this.SetDatePopup.IsOpen)
          {
            SelectDateDialog setDateDialog = this._setDateDialog;
            if (setDateDialog != null)
            {
              setDateDialog.EnterSelect();
              break;
            }
            break;
          }
          this.ShowStartDatePopup(true);
          break;
        case 1:
          this.StartTimeInput.ClearSelection();
          this.StartTimeInput.ToggleReminderPopup();
          this.StartTimeInput.FocusHour();
          break;
        case 2:
          if (this.SetDatePopup.IsOpen)
          {
            SelectDateDialog setDateDialog = this._setDateDialog;
            if (setDateDialog != null)
            {
              setDateDialog.EnterSelect();
              break;
            }
            break;
          }
          this.ShowEndDatePopup(true);
          break;
        case 3:
          this.DueTimeInput.ClearSelection();
          this.DueTimeInput.ToggleReminderPopup();
          this.DueTimeInput.FocusHour();
          break;
        case 4:
          bool check = !this.AllDayCheckBox.IsChecked.GetValueOrDefault();
          this.AllDayCheckBox.IsChecked = new bool?(check);
          this.CheckAllDay(check);
          break;
        case 5:
          if (this.TimeZone.IsVisible)
          {
            TimeZoneSelectControl durationTz = this._durationTz;
            if (durationTz != null)
            {
              durationTz.ShowPopup();
              break;
            }
            break;
          }
          break;
      }
      return this._tabIndex >= 0 && this._tabIndex <= 5;
    }

    public bool HandleEsc() => this.CloseInnerPopups();

    public void HandleLeftRight(bool isLeft)
    {
      if (!this.SetDatePopup.IsOpen)
        return;
      this._setDateDialog?.HandleLeftRight(isLeft);
    }

    public void HandleUpDown(bool isUp)
    {
      if (!this.SetDatePopup.IsOpen)
        return;
      this._setDateDialog?.HandleUpDown(isUp);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/setdatedurationcontrol.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.ClosePopup);
          break;
        case 2:
          this.StartDateText = (Grid) target;
          this.StartDateText.MouseLeftButtonUp += new MouseButtonEventHandler(this.StartDateClick);
          this.StartDateText.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnShowPopupButtonMouseDown);
          break;
        case 3:
          this.StartDateBorder = (Border) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.StartTimeClick);
          break;
        case 5:
          this.StartTimeInput = (TimeInputControl) target;
          break;
        case 6:
          this.EndDateText = (Grid) target;
          this.EndDateText.MouseLeftButtonUp += new MouseButtonEventHandler(this.EndDateClick);
          this.EndDateText.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnShowPopupButtonMouseDown);
          break;
        case 7:
          this.DueDateBorder = (Border) target;
          break;
        case 8:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.EndTimeClick);
          break;
        case 9:
          this.DueTimeInput = (TimeInputControl) target;
          break;
        case 10:
          this.SetDatePopup = (Popup) target;
          this.SetDatePopup.Opened += new EventHandler(this.DatePopupOpened);
          this.SetDatePopup.Closed += new EventHandler(this.DatePopupClosed);
          break;
        case 11:
          this.AllDayCheckBox = (CheckBox) target;
          this.AllDayCheckBox.Click += new RoutedEventHandler(this.OnAllDayClick);
          break;
        case 12:
          this.AllDayBorder = (Rectangle) target;
          break;
        case 13:
          this.TimeZone = (Grid) target;
          break;
        case 14:
          this.TimeZoneGrid = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
