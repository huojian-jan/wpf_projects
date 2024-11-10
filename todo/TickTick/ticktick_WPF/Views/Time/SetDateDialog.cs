// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.SetDateDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Misc;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class SetDateDialog : UserControl, ITabControl, IComponentConnector
  {
    private SetDateTimeControl _dateTimePanel;
    private SetDateDurationControl _durationPanel;
    private bool _calendarMode;
    private bool _noteMode;
    private static SetDateDialog _dialog;
    private bool _item;
    private bool _inOperation;
    private EscPopup _popup;
    private TimeData _originData;
    private bool _showQuickDate;
    private int _dateTabIndex;
    private bool _showRemind;
    internal ContentControl Container;
    internal TextBox EmptyBox;
    internal Grid SwitchTitle;
    internal GroupTitle2 DateOrDurationSwitch;
    internal TextBlock Title;
    internal Grid QuickDatePanel;
    internal ColumnDefinition SkipGridColumn;
    internal Border Today;
    internal Border Tomorrow;
    internal Border NextWeek;
    internal Border NextMonth;
    internal Border SkipGrid;
    internal Border DateTimeBorder;
    internal Border DurationBorder;
    internal Button BatchEditButton;
    internal Border ReminderBorder;
    internal SelectReminderOrRepeatControl ReminderOrRepeatControl;
    internal Grid LocalReminderTimeGrid;
    internal TextBlock LocalReminderTime;
    internal Button SaveButton;
    internal Button CancelButton;
    private bool _contentLoaded;

    private void InitDateTimeControl()
    {
      this._dateTimePanel = new SetDateTimeControl();
      this._dateTimePanel.ParentWindow = this;
      this._dateTimePanel.TimePointControl.DropdownPopup.Opened += new EventHandler(this.OnPopupOpened);
      this._dateTimePanel.TimePointControl.DropdownPopup.Closed += new EventHandler(this.OnPopupClosed);
      this._dateTimePanel.DateSelected -= new EventHandler<DateTime>(this.OnDateSelected);
      this._dateTimePanel.DateSelected += new EventHandler<DateTime>(this.OnDateSelected);
      this.DateTimeBorder.Child = (UIElement) this._dateTimePanel;
    }

    private void InitDurationControl()
    {
      this._durationPanel = new SetDateDurationControl();
      this._durationPanel.ParentWindow = this;
      this._durationPanel.PopupOpened += new EventHandler(this.OnPopupOpened);
      this._durationPanel.PopupClosed += new EventHandler(this.OnPopupClosed);
      this.DurationBorder.Child = (UIElement) this._durationPanel;
    }

    private SetDateDialog(EscPopup popup = null)
    {
      this.InitializeComponent();
      this.InitPopupEvent();
      EscPopup escPopup1 = popup;
      if (escPopup1 == null)
      {
        EscPopup escPopup2 = new EscPopup();
        escPopup2.StaysOpen = false;
        escPopup1 = escPopup2;
      }
      this._popup = escPopup1;
      this._popup.Closed += (EventHandler) (async (a, e) =>
      {
        SetDateDialog sender = this;
        EventHandler hided = sender.Hided;
        if (hided != null)
          hided((object) sender, (EventArgs) null);
        await Task.Delay(40);
        sender.ClearEventHandle();
      });
      this._popup.KeyUp += new KeyEventHandler(this.OnKeyUp);
      this._popup.Child = (UIElement) this;
    }

    private void OnKeyUp(object sender, KeyEventArgs e)
    {
      if (this._inOperation || !this.ShowTabSelected)
        return;
      switch (e.Key)
      {
        case Key.Back:
        case Key.Delete:
          switch (this._dateTabIndex)
          {
            case 8:
              if (this.DateTimeBorder.Visibility != Visibility.Visible)
                return;
              this._dateTimePanel?.ClearTime();
              return;
            case 9:
            case 10:
            case 11:
              this.ReminderOrRepeatControl.ClearReminderOrRepeat();
              return;
            default:
              return;
          }
      }
    }

    private void OnTabKeyUp(bool shift)
    {
      if (!this._popup.IsOpen)
        return;
      if (this.ShowTabSelected)
      {
        this._dateTabIndex += 14 + (shift ? -1 : 1);
        this._dateTabIndex %= 14;
      }
      else
      {
        this.ShowTabSelected = true;
        if (this._dateTabIndex < 0)
          this._dateTabIndex = 0;
      }
      if (this._dateTabIndex < 2 && (this.DateOrDurationSwitch.Visibility != Visibility.Visible || this.SwitchTitle.Visibility != Visibility.Visible))
      {
        this.OnTabKeyUp(shift);
      }
      else
      {
        if (this.DateOrDurationSwitch.GetSelectedIndex() == 1)
        {
          SetDateDurationControl durationPanel = this._durationPanel;
          if ((durationPanel != null ? (durationPanel.SetTabIndex(this._dateTabIndex - 2) ? 1 : 0) : 0) == 0)
          {
            this.OnTabKeyUp(shift);
            return;
          }
        }
        else
        {
          if (this.QuickDatePanel.Visibility != Visibility.Visible && this._dateTabIndex >= 2 && this._dateTabIndex <= 6)
          {
            this.OnTabKeyUp(shift);
            return;
          }
          this.SetDateBorderStatus(this._dateTabIndex);
          if (this._dateTimePanel != null && !this._dateTimePanel.SetTabSelect(this._dateTabIndex - 7))
          {
            this.OnTabKeyUp(shift);
            return;
          }
          if (this._dateTabIndex == 6 && !this.SkipGrid.IsVisible)
          {
            this.OnTabKeyUp(shift);
            return;
          }
        }
        if (this._dateTabIndex == 9 && this.BatchEditButton.Visibility == Visibility.Visible)
        {
          UiUtils.SetCancelButtonTabSelected(this.BatchEditButton, true);
        }
        else
        {
          if (!this.ReminderOrRepeatControl.SetTabSelect(this._dateTabIndex - 9))
          {
            this.OnTabKeyUp(shift);
            return;
          }
          UiUtils.SetCancelButtonTabSelected(this.BatchEditButton, false);
        }
        this.DateOrDurationSwitch.TabSelect(this._dateTabIndex);
        UiUtils.SetSaveButtonTabSelected(this.SaveButton, this._dateTabIndex == 12);
        UiUtils.SetCancelButtonTabSelected(this.CancelButton, this._dateTabIndex == 13);
      }
    }

    private void SetDateBorderStatus(int dateTabIndex)
    {
      this.Today.BorderBrush = (Brush) Brushes.Transparent;
      this.Tomorrow.BorderBrush = (Brush) Brushes.Transparent;
      this.NextWeek.BorderBrush = (Brush) Brushes.Transparent;
      this.NextMonth.BorderBrush = (Brush) Brushes.Transparent;
      this.SkipGrid.BorderBrush = (Brush) Brushes.Transparent;
      switch (dateTabIndex)
      {
        case 2:
          this.Today.SetResourceReference(Control.BorderBrushProperty, (object) "TabBorderColor");
          break;
        case 3:
          this.Tomorrow.SetResourceReference(Control.BorderBrushProperty, (object) "TabBorderColor");
          break;
        case 4:
          this.NextWeek.SetResourceReference(Control.BorderBrushProperty, (object) "TabBorderColor");
          break;
        case 5:
          this.NextMonth.SetResourceReference(Control.BorderBrushProperty, (object) "TabBorderColor");
          break;
        case 6:
          this.SkipGrid.SetResourceReference(Control.BorderBrushProperty, (object) "TabBorderColor");
          break;
      }
    }

    private void InitPopupEvent()
    {
      this.ReminderOrRepeatControl.PopupOpened += new EventHandler(this.OnPopupOpened);
      this.ReminderOrRepeatControl.PopupClosed += new EventHandler(this.OnPopupClosed);
    }

    public event EventHandler<TimeData> Save;

    public event EventHandler Clear;

    public event EventHandler Hided;

    public event EventHandler SkipRecurrence;

    private TimeData TimeData => this.DataContext as TimeData;

    public static SetDateDialog GetDialog(bool showTabSelected = false, EscPopup popup = null)
    {
      if (SetDateDialog._dialog == null || popup != null)
        SetDateDialog._dialog = new SetDateDialog(popup);
      SetDateDialog._dialog.ShowTabSelected = showTabSelected;
      SetDateDialog._dialog._inOperation = false;
      SetDateDialog._dialog.InitTabStatus();
      SetDateDialog._dialog._dateTabIndex = -1;
      return SetDateDialog._dialog;
    }

    private void InitTabStatus()
    {
      this.DateOrDurationSwitch.ClearTabSelected();
      this.SetDateBorderStatus(-1);
      this._dateTimePanel?.SetTabSelect(-1);
      this._durationPanel?.SetTabIndex(-1);
      this.ReminderOrRepeatControl.SetTabSelect(-1);
      UiUtils.SetCancelButtonTabSelected(this.CancelButton, false);
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, false);
    }

    public bool ShowTabSelected { get; set; }

    public async void Show(TimeData timeData, SetDateDialogArgs args)
    {
      SetDateDialog setDateDialog = this;
      timeData = TimeData.Clone(timeData);
      setDateDialog.SetPosition(args.Target, args.HOffset, args.VOffset, args.Placement);
      setDateDialog._popup.IsOpen = true;
      setDateDialog._showQuickDate = args.ShowQuickDate;
      if (args.ShowQuickDate)
      {
        bool flag = args.CanSkip && !RepeatUtils.IsNonRepeatTask(timeData.StartDate, timeData.IsAllDay, timeData.RepeatFlag, timeData.RepeatFrom, timeData.TimeZone.TimeZoneName, timeData.TimeZone.IsFloat, timeData.ExDates);
        setDateDialog.SkipGrid.Visibility = flag ? Visibility.Visible : Visibility.Collapsed;
        setDateDialog.SkipGridColumn.Width = flag ? new GridLength(1.0, GridUnitType.Star) : new GridLength(0.0);
      }
      timeData = setDateDialog.HandleTimeData(timeData, args.CalendarMode, args.IsNote, args.ItemMode);
      setDateDialog._originData = timeData;
      setDateDialog.DataContext = (object) TimeData.Clone(timeData);
      setDateDialog._noteMode = args.IsNote;
      setDateDialog._calendarMode = args.CalendarMode;
      setDateDialog._item = args.ItemMode;
      setDateDialog._showRemind = args.ShowRemind;
      setDateDialog.ReminderOrRepeatControl.SetCalendarMode(setDateDialog._calendarMode, args.ShowRemind);
      int num = setDateDialog._noteMode || args.ItemMode ? 0 : (int) SetDateDialog.ModeFromDateData(setDateDialog.TimeData);
      setDateDialog.InitShow(args.ItemMode, args.CalendarMode, args.IsNote);
      if (num == 1 && timeData.IsTimeUnified)
      {
        setDateDialog.DateOrDurationSwitch.SetSelectedIndex(1);
        setDateDialog.ShowDurationDialog();
      }
      else
      {
        setDateDialog.DateOrDurationSwitch.SetSelectedIndex(0);
        setDateDialog.ShowDateDialog();
      }
      PopupStateManager.SetCanOpenTimePopup(true);
    }

    private void OnPopupOpened(object sender, EventArgs e) => this._inOperation = true;

    private async void OnPopupClosed(object sender, EventArgs e)
    {
      this._inOperation = false;
      this.TryFocusEmpty((object) null, (MouseButtonEventArgs) null);
    }

    private void SetPosition(
      UIElement target,
      double hOffset,
      double vOffset,
      PlacementMode placement)
    {
      this._popup.PlacementTarget = target;
      this._popup.Placement = target == null ? PlacementMode.Mouse : placement;
      this._popup.VerticalOffset = vOffset;
      this._popup.HorizontalOffset = hOffset;
    }

    private void InitShow(bool itemModeOrHideRepeat, bool calendarMode, bool noteMode)
    {
      if (itemModeOrHideRepeat)
      {
        this.ReminderOrRepeatControl.Visibility = Visibility.Collapsed;
        this.LocalReminderTimeGrid.Visibility = Visibility.Visible;
        this.ShowSubTaskLocalTime();
      }
      else
      {
        this.ReminderOrRepeatControl.Visibility = Visibility.Visible;
        this.LocalReminderTimeGrid.Visibility = Visibility.Collapsed;
      }
      this.ReminderOrRepeatControl.ShowRepeat(!noteMode);
      this.SwitchTitle.Visibility = itemModeOrHideRepeat | calendarMode ? Visibility.Collapsed : Visibility.Visible;
      this.CancelButton.Content = (object) Utils.GetString(calendarMode ? "Cancel" : "PublicClear");
      this.DateOrDurationSwitch.Visibility = calendarMode || noteMode ? Visibility.Collapsed : Visibility.Visible;
      this.Title.Visibility = noteMode ? Visibility.Visible : Visibility.Collapsed;
      if (!noteMode)
        return;
      this.LocalReminderTimeGrid.Visibility = Visibility.Visible;
      this.ShowSubTaskLocalTime();
    }

    private void ShowDateDialog()
    {
      if (this._dateTimePanel == null)
        this.InitDateTimeControl();
      DateTime? nullable1 = this.TimeData.StartDate;
      TimeData timeData1 = !nullable1.HasValue ? TimeData.InitDefaultTime() : TimeData.Clone(this.TimeData);
      timeData1.TimeZone = this.TimeData.TimeZone;
      timeData1.BatchData = this.TimeData.BatchData;
      this.DataContext = (object) timeData1;
      this.QuickDatePanel.Visibility = this._showQuickDate ? Visibility.Visible : Visibility.Collapsed;
      if (timeData1.ChangedDateOnly || !timeData1.IsTimeUnified)
      {
        this.ReminderBorder.Visibility = Visibility.Collapsed;
        this.BatchEditButton.Visibility = Visibility.Visible;
        this._dateTimePanel.SetTimeInputVisible(false);
      }
      else
      {
        this.ReminderBorder.Visibility = Visibility.Visible;
        this.BatchEditButton.Visibility = Visibility.Collapsed;
        this._dateTimePanel.SetTimeInputVisible(true);
      }
      this._dateTimePanel.TimePointControl.CloseReminderPopup();
      nullable1 = this.TimeData.DueDate;
      if (nullable1.HasValue)
      {
        TimeData timeData2 = this.TimeData;
        nullable1 = new DateTime?();
        DateTime? nullable2 = nullable1;
        timeData2.DueDate = nullable2;
      }
      this._dateTimePanel.TimePointControl.EnableEditTimeZone = !this._item;
      this.DateTimeBorder.Visibility = Visibility.Visible;
      this.DurationBorder.Visibility = Visibility.Collapsed;
      this._dateTimePanel.SetData();
      if (this.ShowTabSelected)
      {
        if (this._dateTabIndex < 0)
        {
          this._dateTabIndex = 0;
          this.DateOrDurationSwitch.TabSelectFirst();
          if (this.DateOrDurationSwitch.Visibility != Visibility.Visible)
            this.OnTabKeyUp(false);
        }
        this.EmptyBox.Focus();
      }
      else
        this.EmptyBox.Focus();
    }

    public void ShowSubTaskLocalTime()
    {
      if ((!this.TimeData.IsAllDay.HasValue || !this.TimeData.IsAllDay.Value) && this.TimeData.StartDate.HasValue)
      {
        this.LocalReminderTime.Visibility = Visibility.Visible;
        DateTime dateTime1 = this.TimeData.StartDate.Value;
        string timeFormatString = DateUtils.GetTimeFormatString();
        if (LocalSettings.Settings.EnableTimeZone && this.TimeData.TimeZone.TimeZoneName != TimeZoneData.LocalTimeZoneModel?.TimeZoneName)
        {
          DateTime dateTime2 = this.TimeData.TimeZone.IsFloat ? this.TimeData.StartDate.Value : TimeZoneUtils.ToLocalTime(this.TimeData.StartDate.Value, this.TimeData.TimeZone.TimeZoneName);
          this.LocalReminderTime.Text = string.Format(Utils.GetString("RemindTime"), (object) dateTime2.ToString("M", (IFormatProvider) App.Ci), (object) dateTime2.ToString(timeFormatString, (IFormatProvider) App.Ci), (object) Utils.GetString("LocalTimeZone"));
        }
        else
          this.LocalReminderTime.Text = string.Format(Utils.GetString("RemindTime"), (object) dateTime1.ToString("M", (IFormatProvider) App.Ci), (object) dateTime1.ToString(timeFormatString, (IFormatProvider) App.Ci), (object) "");
      }
      else
        this.LocalReminderTime.Visibility = Visibility.Collapsed;
    }

    private void OnDateSelected(object sender, DateTime e) => this.ShowSubTaskLocalTime();

    private TimeData HandleTimeData(
      TimeData timeData,
      bool calendarMode,
      bool isNote,
      bool itemMode)
    {
      if (!timeData.IsAllDay.GetValueOrDefault() && !timeData.TimeZone.IsFloat)
      {
        if (LocalSettings.Settings.EnableTimeZone)
          timeData = TimeZoneUtils.TransferToSelectedTimeZoneTime(timeData);
        else
          timeData.TimeZone = TimeZoneData.LocalTimeZoneModel;
      }
      if (timeData.IsAllDay.GetValueOrDefault() && !itemMode)
        timeData.TimeZone = TimeZoneData.LocalTimeZoneModel;
      SetDateDialog.HandleAllDay(timeData, -1);
      if (!calendarMode)
        SetDateDialog.HandleTaskDefault(timeData, itemMode);
      else if (!timeData.DueDate.HasValue)
        timeData.DueDate = timeData.StartDate;
      if (timeData.IsDefault & calendarMode)
        timeData.Reminders = TimeData.GetDefaultAllDayReminders();
      if (isNote | itemMode)
        timeData.DueDate = new DateTime?();
      if (timeData.BatchData != null)
      {
        DateTime? nullable1;
        if (!timeData.IsDefault)
        {
          TimeData timeData1 = timeData;
          DateTime? nullable2;
          if (!timeData.BatchData.IsDateUnified || !timeData.BatchData.StartDate.HasValue)
          {
            nullable1 = TaskDefaultModel.GetDefaultDate(TaskDefaultDao.GetDefaultSafely()?.Date);
            nullable2 = new DateTime?(nullable1 ?? DateTime.Today);
          }
          else
            nullable2 = timeData.BatchData.StartDate;
          timeData1.StartDate = nullable2;
        }
        if (timeData.BatchData.IsTimeUnified && timeData.BatchData.StartDate.HasValue)
        {
          timeData.TimeZone = new TimeZoneViewModel(timeData.BatchData.IsFloating, timeData.BatchData.TimeZone);
          timeData.IsAllDay = timeData.BatchData.IsAllDay;
          bool? isAllDay = timeData.IsAllDay;
          if (((int) isAllDay ?? 1) == 0)
          {
            nullable1 = timeData.StartDate;
            if (nullable1.HasValue)
            {
              TimeData timeData2 = timeData;
              nullable1 = timeData.StartDate;
              int year = nullable1.Value.Year;
              nullable1 = timeData.StartDate;
              DateTime dateTime = nullable1.Value;
              int month = dateTime.Month;
              nullable1 = timeData.StartDate;
              dateTime = nullable1.Value;
              int day = dateTime.Day;
              dateTime = timeData.BatchData.StartDate.Value;
              int hour = dateTime.Hour;
              dateTime = timeData.BatchData.StartDate.Value;
              int minute = dateTime.Minute;
              DateTime? nullable3 = new DateTime?(new DateTime(year, month, day, hour, minute, 0));
              timeData2.StartDate = nullable3;
            }
          }
          if (timeData.BatchData.IsReminderUnified)
          {
            timeData.Reminders = timeData.BatchData.Reminders;
          }
          else
          {
            TimeData timeData3 = timeData;
            isAllDay = timeData.IsAllDay;
            List<TaskReminderModel> taskReminderModelList = ((int) isAllDay ?? 1) != 0 ? TimeData.GetDefaultAllDayReminders() : TimeData.GetDefaultTimeReminders();
            timeData3.Reminders = taskReminderModelList;
          }
        }
        if (timeData.BatchData.IsRepeatUnified)
        {
          timeData.RepeatFlag = timeData.BatchData.RepeatFlag;
          timeData.RepeatFrom = timeData.BatchData.RepeatFrom;
        }
        else
        {
          timeData.RepeatFlag = (string) null;
          timeData.RepeatFrom = (string) null;
        }
      }
      return timeData;
    }

    private static void HandleAllDay(TimeData timeData, int offset)
    {
      if (timeData == null)
        timeData = new TimeData();
      bool? isAllDay = timeData.IsAllDay;
      if (!isAllDay.HasValue)
        return;
      isAllDay = timeData.IsAllDay;
      if (!isAllDay.Value)
        return;
      DateTime? nullable1 = timeData.DueDate;
      if (!nullable1.HasValue)
        return;
      nullable1 = timeData.StartDate;
      if (!nullable1.HasValue)
        return;
      TimeData timeData1 = timeData;
      nullable1 = timeData.DueDate;
      DateTime? nullable2 = new DateTime?(nullable1.Value.AddDays((double) offset));
      timeData1.DueDate = nullable2;
      nullable1 = timeData.DueDate;
      DateTime date1 = nullable1.Value.Date;
      nullable1 = timeData.StartDate;
      DateTime date2 = nullable1.Value.Date;
      if (!(date1 == date2))
        return;
      TimeData timeData2 = timeData;
      nullable1 = new DateTime?();
      DateTime? nullable3 = nullable1;
      timeData2.DueDate = nullable3;
    }

    private static void HandleTaskDefault(TimeData timeData, bool itemMode)
    {
      if (!timeData.IsDefault)
        return;
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      bool? isAllDay;
      if (defaultSafely != null && defaultSafely.DateMode == 1)
      {
        timeData.IsAllDay = new bool?(Constants.DurationValue.IsAllDayValue(defaultSafely.Duration) | itemMode);
        TimeData timeData1 = timeData;
        isAllDay = timeData.IsAllDay;
        DateTime? startDate;
        DateTime? nullable1;
        if (!isAllDay.Value)
        {
          startDate = timeData.StartDate;
          nullable1 = new DateTime?(DateUtils.GetNextHour(startDate ?? defaultSafely.GetDefaultDateTime() ?? DateTime.Today));
        }
        else
          nullable1 = timeData.StartDate ?? defaultSafely.GetDefaultDateTime();
        timeData1.StartDate = nullable1;
        startDate = timeData.StartDate;
        if (startDate.HasValue)
        {
          TimeData timeData2 = timeData;
          startDate = timeData.StartDate;
          DateTime? nullable2 = new DateTime?(startDate.Value.AddMinutes((double) defaultSafely.Duration));
          timeData2.DueDate = nullable2;
        }
      }
      TimeData timeData3 = timeData;
      isAllDay = timeData.IsAllDay;
      List<TaskReminderModel> taskReminderModelList;
      if (isAllDay.HasValue)
      {
        isAllDay = timeData.IsAllDay;
        if (isAllDay.Value)
        {
          taskReminderModelList = TimeData.GetDefaultAllDayReminders();
          goto label_11;
        }
      }
      taskReminderModelList = TimeData.GetDefaultTimeReminders();
label_11:
      timeData3.Reminders = taskReminderModelList;
    }

    private static SetDateDialog.DateMode ModeFromDateData(TimeData timeData)
    {
      if (timeData.DueDate.HasValue && !Utils.IsEmptyDate(timeData.DueDate))
        return SetDateDialog.DateMode.Duration;
      if (!timeData.IsDefault || TaskDefaultDao.GetDefaultSafely().DateMode != 1)
        return SetDateDialog.DateMode.Date;
      if (!timeData.DueDate.HasValue)
        timeData.DueDate = timeData.StartDate;
      return SetDateDialog.DateMode.Duration;
    }

    private void ShowDurationDialog()
    {
      if (this._durationPanel == null)
        this.InitDurationControl();
      DateTime? nullable1 = this.TimeData.StartDate;
      TimeData data = !nullable1.HasValue ? TimeData.InitDefaultDuration() : TimeData.Clone(this.TimeData);
      SetDateDialog.HandleAllDayBeforeSave(data, 0);
      data.BatchData = this.TimeData.BatchData;
      if (data.ChangedDateOnly)
      {
        this.ReminderBorder.Visibility = Visibility.Collapsed;
        this.BatchEditButton.Visibility = Visibility.Visible;
      }
      else
      {
        this.ReminderBorder.Visibility = Visibility.Visible;
        this.BatchEditButton.Visibility = Visibility.Collapsed;
      }
      this.QuickDatePanel.Visibility = Visibility.Collapsed;
      this.DataContext = (object) data;
      if (!this.TimeData.IsDefault)
      {
        bool? isAllDay = this.TimeData.IsAllDay;
        if (isAllDay.HasValue)
        {
          isAllDay = this.TimeData.IsAllDay;
          if (isAllDay.Value)
          {
            nullable1 = this.TimeData.StartDate;
            if (nullable1.HasValue)
            {
              nullable1 = this.TimeData.DueDate;
              if (!nullable1.HasValue)
              {
                DateTime dateTime = DateTime.Now;
                int hour = dateTime.Hour;
                TimeData timeData1 = this.TimeData;
                nullable1 = this.TimeData.StartDate;
                dateTime = DateUtils.SetHourAndMinuteOnly(nullable1.Value, hour, 0);
                DateTime? nullable2 = new DateTime?(dateTime.AddHours(1.0));
                timeData1.StartDate = nullable2;
                TimeData timeData2 = this.TimeData;
                nullable1 = this.TimeData.StartDate;
                dateTime = nullable1.Value;
                DateTime? nullable3 = new DateTime?(dateTime.AddHours(1.0));
                timeData2.DueDate = nullable3;
                goto label_14;
              }
              else
                goto label_14;
            }
            else
              goto label_14;
          }
        }
        nullable1 = this.TimeData.StartDate;
        if (nullable1.HasValue)
        {
          nullable1 = this.TimeData.DueDate;
          if (!nullable1.HasValue)
          {
            TimeData timeData = this.TimeData;
            nullable1 = this.TimeData.StartDate;
            DateTime? nullable4 = new DateTime?(nullable1.Value.AddHours(1.0));
            timeData.DueDate = nullable4;
          }
        }
      }
label_14:
      this.ReminderOrRepeatControl.IsDateSelected = true;
      this.ReminderOrRepeatControl.SelectRepeatButton.Visibility = this._item ? Visibility.Collapsed : Visibility.Visible;
      this.DateTimeBorder.Visibility = Visibility.Collapsed;
      this.DurationBorder.Visibility = Visibility.Visible;
      this._durationPanel.SetData();
      if (this.ShowTabSelected)
      {
        if (this._dateTabIndex < 0)
        {
          this._dateTabIndex = 1;
          this.DateOrDurationSwitch.TabSelectLast();
        }
        this.EmptyBox.Focus();
      }
      else
        this.EmptyBox.Focus();
    }

    private void SwitchDurationClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DurationBorder.IsVisible || !ProChecker.CheckPro(ProType.Duration))
        return;
      bool? isAllDay = this.TimeData.IsAllDay;
      if (isAllDay.HasValue)
      {
        isAllDay = this.TimeData.IsAllDay;
        if (isAllDay.Value)
        {
          int hour = DateTime.Now.Hour;
          int minute = DateTime.Now.Minute;
          DateTime? nullable1 = (DateTime?) this._dateTimePanel?.Calendar.SelectedDate;
          DateTime modify = nullable1 ?? DateTime.Today;
          this.TimeData.StartDate = new DateTime?(modify.Date);
          TimeData timeData1 = this.TimeData;
          nullable1 = this.TimeData.StartDate;
          DateTime? nullable2 = new DateTime?(DateUtils.SetHourAndMinuteOnly(nullable1.Value, hour, 0).AddMinutes(minute >= 30 ? 60.0 : 30.0));
          timeData1.StartDate = nullable2;
          nullable1 = this.TimeData.StartDate;
          if (nullable1.Value.Date != modify.Date && modify.Date != DateTime.Today)
          {
            TimeData timeData2 = this.TimeData;
            nullable1 = this.TimeData.StartDate;
            DateTime? nullable3 = new DateTime?(DateUtils.SetDateOnly(nullable1.Value, modify));
            timeData2.StartDate = nullable3;
          }
          TimeData timeData3 = this.TimeData;
          nullable1 = this.TimeData.StartDate;
          DateTime? nullable4 = new DateTime?(nullable1.Value.AddHours(1.0));
          timeData3.DueDate = nullable4;
          this.TimeData.Reminders = TimeData.GetDefaultTimeReminders();
          goto label_8;
        }
      }
      TimeData timeData = this.TimeData;
      if ((timeData != null ? (timeData.StartDate.HasValue ? 1 : 0) : 0) != 0)
        this.TimeData.DueDate = new DateTime?(this.TimeData.StartDate.Value.AddHours(1.0));
label_8:
      this.TimeData.IsAllDay = new bool?(false);
      this.TimeData.BatchData?.SetUnified();
      this.ShowDurationDialog();
      this._dateTabIndex = 1;
    }

    private void SwitchDateClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DateTimeBorder.IsVisible)
        return;
      this.TimeData.BatchData?.SetUnified();
      this.ShowDateDialog();
      this._dateTabIndex = 0;
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler clear = this.Clear;
      if (clear != null)
        clear((object) this, (EventArgs) null);
      this._popup.IsOpen = false;
    }

    public async void SaveClick(object sender, RoutedEventArgs e)
    {
      SetDateDialog sender1 = this;
      await Task.Delay(50);
      TimeData timeData = TimeData.Clone(sender1.TimeData);
      if (!timeData.StartDate.HasValue && sender1.DateTimeBorder.Visibility == Visibility.Visible)
        timeData.StartDate = sender1._dateTimePanel.Calendar.SelectedDate;
      if (timeData.IsAllDay.GetValueOrDefault())
        timeData.TimeZone = TimeZoneData.LocalTimeZoneModel;
      SetDateDialog.HandleAllDayBeforeSave(timeData, 1);
      if ((!timeData.IsAllDay.HasValue || !timeData.IsAllDay.Value) && !timeData.TimeZone.IsFloat)
        timeData = TimeZoneUtils.TransferToLocalTime(timeData);
      if (!LocalSettings.Settings.EnableTimeZone)
        timeData.TimeZone = TimeZoneData.LocalTimeZoneModel;
      if (!sender1._showRemind)
        timeData.Reminders = new List<TaskReminderModel>();
      EventHandler<TimeData> save = sender1.Save;
      if (save != null)
        save((object) sender1, timeData);
      UtilLog.Info("SetDateDialog.Save " + timeData?.ToString());
      sender1._popup.IsOpen = false;
    }

    private static void HandleAllDayBeforeSave(TimeData data, int addDayDiff)
    {
      if (!data.IsAllDay.HasValue || !data.IsAllDay.Value)
        return;
      if (data.StartDate.HasValue)
        data.StartDate = new DateTime?(DateUtils.CropHourAndMinute(data.StartDate.Value));
      if (!data.DueDate.HasValue)
        return;
      data.DueDate = new DateTime?(data.DueDate.Value.AddDays((double) addDayDiff));
      data.DueDate = new DateTime?(DateUtils.CropHourAndMinute(data.DueDate.Value));
    }

    public void OnSaved(TimeData timeData)
    {
      if (!this._showRemind)
        timeData.Reminders = (List<TaskReminderModel>) null;
      EventHandler<TimeData> save = this.Save;
      if (save == null)
        return;
      save((object) this, timeData);
    }

    public void BindRepeatChangedEvent(EventHandler onRepeatChanged)
    {
      this.ReminderOrRepeatControl.RepeatChanged -= onRepeatChanged;
      this.ReminderOrRepeatControl.RepeatChanged += onRepeatChanged;
    }

    public void ClearEventHandle()
    {
      this.Save = (EventHandler<TimeData>) null;
      this.Clear = (EventHandler) null;
      this.Hided = (EventHandler) null;
      this.SkipRecurrence = (EventHandler) null;
    }

    private void TryFocusEmpty(object sender, MouseButtonEventArgs e)
    {
      if (this._durationPanel != null && (this._durationPanel.DueTimeInput.IsMouseOver || this._durationPanel.StartTimeInput.IsMouseOver) || this._dateTimePanel != null && this._dateTimePanel.TimePointControl.IsMouseOver)
        return;
      this.EmptyBox.Focus();
    }

    private void OnGridClick(object sender, MouseButtonEventArgs e)
    {
      if (sender is FrameworkElement frameworkElement && frameworkElement.Tag is string tag)
        this.QuickSetDate(tag);
      this._popup.IsOpen = false;
    }

    private void QuickSetDate(string tag)
    {
      DateTime dateTime1 = DateTime.Today;
      switch (tag)
      {
        case "Tomorrow":
          dateTime1 = dateTime1.AddDays(1.0);
          break;
        case "NextWeek":
          dateTime1 = dateTime1.AddDays(7.0);
          break;
        case "NextMonth":
          dateTime1 = dateTime1.AddMonths(1);
          break;
        case "SkipCurrentRecurrence":
          EventHandler skipRecurrence = this.SkipRecurrence;
          if (skipRecurrence != null)
            skipRecurrence((object) this, (EventArgs) null);
          this._popup.IsOpen = false;
          return;
      }
      TimeData e = TimeData.Clone(this.TimeData);
      if (e.StartDate.HasValue)
      {
        ref DateTime local1 = ref dateTime1;
        int year = dateTime1.Year;
        int month = dateTime1.Month;
        int day = dateTime1.Day;
        DateTime dateTime2 = e.StartDate.Value;
        int hour = dateTime2.Hour;
        DateTime? startDate = e.StartDate;
        dateTime2 = startDate.Value;
        int minute = dateTime2.Minute;
        local1 = new DateTime(year, month, day, hour, minute, 0);
        DateTime dateTime3 = dateTime1;
        startDate = e.StartDate;
        DateTime dateTime4 = startDate.Value;
        double totalMinutes = (dateTime3 - dateTime4).TotalMinutes;
        e.StartDate = new DateTime?(dateTime1);
        TimeData timeData = e;
        DateTime? dueDate = e.DueDate;
        ref DateTime? local2 = ref dueDate;
        DateTime? nullable = local2.HasValue ? new DateTime?(local2.GetValueOrDefault().AddMinutes(totalMinutes)) : new DateTime?();
        timeData.DueDate = nullable;
      }
      else
        e.StartDate = new DateTime?(dateTime1);
      EventHandler<TimeData> save = this.Save;
      if (save == null)
        return;
      save((object) this, e);
    }

    public bool IsOpen()
    {
      EscPopup popup = this._popup;
      return popup != null && popup.IsOpen;
    }

    public void TryClose()
    {
      if (!this._popup.IsOpen)
        return;
      this._popup.IsOpen = false;
      this.ClearEventHandle();
      SetDateDialog._dialog = (SetDateDialog) null;
    }

    private void BatchSetClick(object sender, RoutedEventArgs e)
    {
      this.TimeData.BatchData?.SetUnified();
      this.ReminderBorder.Visibility = Visibility.Visible;
      this.BatchEditButton.Visibility = Visibility.Collapsed;
      this._dateTimePanel?.SetTimeInputVisible(true);
    }

    private void OnDateOrDurationSwitchChanged(object sender, GroupTitleViewModel e)
    {
      if ("Duration".Equals(e.Title))
        this.SwitchDurationClick((object) null, (MouseButtonEventArgs) null);
      else
        this.SwitchDateClick((object) null, (MouseButtonEventArgs) null);
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
      if (!this.ShowTabSelected)
        return;
      this.ShowTabSelected = false;
      this.InitTabStatus();
    }

    public bool HandleTab(bool shift)
    {
      if (this.ReminderOrRepeatControl.TabHandled(shift) || this._inOperation)
        return true;
      this.OnTabKeyUp(shift);
      this.EmptyBox.Focus();
      return true;
    }

    public bool HandleEnter()
    {
      if (this.ReminderOrRepeatControl.HandleEnter())
        return true;
      if (this._inOperation && !this.ShowTabSelected)
      {
        this._dateTimePanel?.TryClosePopup();
        this._durationPanel?.TryClosePopup();
      }
      else if (this.ShowTabSelected)
      {
        if (this._dateTabIndex >= 2 && this._dateTabIndex <= 7 && this.DateOrDurationSwitch.GetSelectedIndex() == 1)
        {
          this._durationPanel?.HandleEnter();
          return true;
        }
        switch (this._dateTabIndex)
        {
          case 0:
            if (this.DateOrDurationSwitch.IsVisible)
            {
              this.DateOrDurationSwitch.SetSelectedIndex(0);
              this.SwitchDateClick((object) null, (MouseButtonEventArgs) null);
              break;
            }
            break;
          case 1:
            if (this.DateOrDurationSwitch.IsVisible)
            {
              this.DateOrDurationSwitch.SetSelectedIndex(1);
              this.SwitchDurationClick((object) null, (MouseButtonEventArgs) null);
              break;
            }
            break;
          case 2:
            this.QuickSetDate("Today");
            this._popup.IsOpen = false;
            break;
          case 3:
            this.QuickSetDate("Tomorrow");
            this._popup.IsOpen = false;
            break;
          case 4:
            this.QuickSetDate("NextWeek");
            this._popup.IsOpen = false;
            break;
          case 5:
            this.QuickSetDate("NextMonth");
            this._popup.IsOpen = false;
            break;
          case 6:
            this.QuickSetDate("SkipCurrentRecurrence");
            this._popup.IsOpen = false;
            break;
          case 7:
            SetDateTimeControl dateTimePanel1 = this._dateTimePanel;
            if (dateTimePanel1 != null)
            {
              dateTimePanel1.SelectTabItem();
              break;
            }
            break;
          case 8:
            SetDateTimeControl dateTimePanel2 = this._dateTimePanel;
            if (dateTimePanel2 != null)
            {
              dateTimePanel2.SelectTimeInput();
              break;
            }
            break;
          case 9:
            if (this.BatchEditButton.Visibility == Visibility.Visible)
            {
              this.BatchSetClick((object) this.BatchEditButton, (RoutedEventArgs) null);
              if (this.DurationBorder.Visibility == Visibility.Visible)
              {
                this._dateTabIndex = 9;
                this.ReminderOrRepeatControl.SetTabSelect(0);
                break;
              }
              this._dateTabIndex = 8;
              SetDateTimeControl dateTimePanel3 = this._dateTimePanel;
              if (dateTimePanel3 != null)
              {
                dateTimePanel3.SetTabSelect(1);
                break;
              }
              break;
            }
            goto case 10;
          case 10:
          case 11:
            if (this.ReminderOrRepeatControl.IsVisible)
            {
              this.ReminderOrRepeatControl.HandleKeyUp(Key.Return);
              break;
            }
            break;
          case 12:
            this.SaveClick((object) this, (RoutedEventArgs) null);
            break;
          case 13:
            EventHandler clear = this.Clear;
            if (clear != null)
              clear((object) this, (EventArgs) null);
            this._popup.IsOpen = false;
            break;
        }
        return true;
      }
      return false;
    }

    public bool HandleEsc()
    {
      if (this.ReminderOrRepeatControl.HandleEsc())
        return true;
      SetDateTimeControl dateTimePanel = this._dateTimePanel;
      if ((dateTimePanel != null ? (dateTimePanel.HandleEsc() ? 1 : 0) : 0) != 0)
        return true;
      SetDateDurationControl durationPanel = this._durationPanel;
      if ((durationPanel != null ? (durationPanel.HandleEsc() ? 1 : 0) : 0) != 0)
        return true;
      if (this._inOperation)
      {
        this._dateTimePanel?.TryClosePopup();
        this.ReminderOrRepeatControl.TryClosePopup();
        this.EmptyBox.Focus();
      }
      return false;
    }

    public bool UpDownSelect(bool isUp)
    {
      this._durationPanel?.HandleUpDown(isUp);
      if (this.ShowTabSelected && this._dateTabIndex == 7)
        this._dateTimePanel?.MoveTabSelectDate(isUp ? -7 : 7);
      return this.ShowTabSelected && this._dateTabIndex < 9 || this.ReminderOrRepeatControl.HandleUpDownSelect(isUp);
    }

    public bool LeftRightSelect(bool isLeft)
    {
      this._durationPanel?.HandleLeftRight(isLeft);
      if (this.ShowTabSelected && this._dateTabIndex == 7)
      {
        this._dateTimePanel?.MoveTabSelectDate(isLeft ? -1 : 1);
        return true;
      }
      return (!this.ShowTabSelected || this._dateTabIndex >= 9) && this.ReminderOrRepeatControl.HandleLeftRightSelect(isLeft);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/setdatedialog.xaml", UriKind.Relative));
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
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseUp);
          break;
        case 2:
          this.Container = (ContentControl) target;
          this.Container.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.TryFocusEmpty);
          break;
        case 3:
          this.EmptyBox = (TextBox) target;
          break;
        case 4:
          this.SwitchTitle = (Grid) target;
          break;
        case 5:
          this.DateOrDurationSwitch = (GroupTitle2) target;
          break;
        case 6:
          this.Title = (TextBlock) target;
          break;
        case 7:
          this.QuickDatePanel = (Grid) target;
          break;
        case 8:
          this.SkipGridColumn = (ColumnDefinition) target;
          break;
        case 9:
          this.Today = (Border) target;
          this.Today.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnGridClick);
          break;
        case 10:
          this.Tomorrow = (Border) target;
          this.Tomorrow.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnGridClick);
          break;
        case 11:
          this.NextWeek = (Border) target;
          this.NextWeek.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnGridClick);
          break;
        case 12:
          this.NextMonth = (Border) target;
          this.NextMonth.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnGridClick);
          break;
        case 13:
          this.SkipGrid = (Border) target;
          this.SkipGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnGridClick);
          break;
        case 14:
          this.DateTimeBorder = (Border) target;
          break;
        case 15:
          this.DurationBorder = (Border) target;
          break;
        case 16:
          this.BatchEditButton = (Button) target;
          this.BatchEditButton.Click += new RoutedEventHandler(this.BatchSetClick);
          break;
        case 17:
          this.ReminderBorder = (Border) target;
          break;
        case 18:
          this.ReminderOrRepeatControl = (SelectReminderOrRepeatControl) target;
          break;
        case 19:
          this.LocalReminderTimeGrid = (Grid) target;
          break;
        case 20:
          this.LocalReminderTime = (TextBlock) target;
          break;
        case 21:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.SaveClick);
          break;
        case 22:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.CancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    private enum DateMode
    {
      Date,
      Duration,
    }
  }
}
