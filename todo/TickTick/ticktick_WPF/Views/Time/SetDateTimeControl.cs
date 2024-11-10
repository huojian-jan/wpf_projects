// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.SetDateTimeControl
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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class SetDateTimeControl : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty TimeTabSelectProperty = DependencyProperty.Register(nameof (TimeTabSelect), typeof (bool), typeof (SetDateTimeControl), new PropertyMetadata((object) false));
    public SetDateDialog ParentWindow;
    private bool _showPopupMouseDown;
    private int _index;
    internal SetDateTimeControl Root;
    internal TickDatePicker Calendar;
    internal StackPanel TimeInputPanel;
    internal Border DateBorder;
    internal TimeInputControl TimePointControl;
    internal Path RightIcon;
    private bool _contentLoaded;

    public bool TimeTabSelect
    {
      get => (bool) this.GetValue(SetDateTimeControl.TimeTabSelectProperty);
      set => this.SetValue(SetDateTimeControl.TimeTabSelectProperty, (object) value);
    }

    public TimeData TimeData => (TimeData) this.DataContext;

    public event EventHandler<DateTime> DateSelected;

    public SetDateTimeControl()
    {
      this.InitializeComponent();
      this.TimePointControl.SelectedTimeChanged -= new EventHandler<DateTime>(this.OnTimeSelected);
      this.TimePointControl.SelectedTimeChanged += new EventHandler<DateTime>(this.OnTimeSelected);
      this.TimePointControl.TimeZoneChanged += (EventHandler<TimeZoneViewModel>) ((sender, tzModel) =>
      {
        this.TimeData.TimeZone = tzModel;
        this.Calendar.TimeZone = this.TimeData.IsAllDay.HasValue && !this.TimeData.IsAllDay.Value || !this.TimeData.TimeZone.IsFloat ? this.TimeData.TimeZone.TimeZoneName : (string) null;
      });
      this.TimePointControl.ReminderPopup.HorizontalOffset = -44.0;
      this.TimePointControl.ReminderPopup.VerticalOffset = -4.0;
    }

    public void SetData()
    {
      this.InitTime();
      this.InitSelectedDate();
    }

    private void InitSelectedDate()
    {
      int num = !this.TimeData.StartDate.HasValue || !(this.TimeData.StartDate.Value.Date != DateTime.Today) ? (this.TimeData.IsDefault ? 0 : (this.TimeData.StartDate.HasValue ? 1 : 0)) : 1;
      this.Calendar.TrySetDayCells();
      if (num != 0)
      {
        this.ParentWindow.ReminderOrRepeatControl.IsDateSelected = true;
        this.Calendar.SelectedDate = new DateTime?(this.TimeData.StartDate.Value);
      }
      else
      {
        this.ParentWindow.ReminderOrRepeatControl.IsDateSelected = false;
        this.Calendar.SelectedDate = this.TimeData.IsAllDay.HasValue && !this.TimeData.IsAllDay.Value || !this.TimeData.TimeZone.IsFloat ? new DateTime?(TimeZoneUtils.LocalToTargetTzTime(DateTime.Now, this.TimeData.TimeZone.TimeZoneName).Date) : new DateTime?(DateTime.Today);
      }
      this.Calendar.RepeatFrom = this.TimeData.RepeatFrom;
      this.Calendar.RepeatFlag = this.TimeData.RepeatFlag;
      this.Calendar.ExDates = this.TimeData.ExDates;
      this.Calendar.TimeZone = this.TimeData.IsAllDay.HasValue && !this.TimeData.IsAllDay.Value || !this.TimeData.TimeZone.IsFloat ? this.TimeData.TimeZone.TimeZoneName : (string) null;
      this.Calendar.RenderRepeat();
    }

    private void InitTime()
    {
      if (this.TimeData.IsAllDay.HasValue && !this.TimeData.IsAllDay.Value)
      {
        if (this.TimeData.StartDate.HasValue)
        {
          this.TimePointControl.SelectedTime = this.TimeData.StartDate.Value;
          this.TimePointControl.Visibility = Visibility.Visible;
        }
      }
      else
      {
        this.TimePointControl.SelectedTime = DateUtils.GetNextHour(this.TimeData.TimeZone.IsFloat ? "" : this.TimeData.TimeZone.TimeZoneName);
        this.TimePointControl.Visibility = Visibility.Collapsed;
      }
      this.TimePointControl.TimeZone = this.TimeData.TimeZone;
      this.ParentWindow?.BindRepeatChangedEvent(new EventHandler(this.OnRepeatChanged));
    }

    private void OnTimeSelected(object sender, DateTime time)
    {
      this.TimeData.StartDate = new DateTime?(DateUtils.SetHourAndMinuteOnly(this.TimeData.StartDate ?? DateTime.Now, time.Hour, time.Minute));
      if (this.ParentWindow == null || !this.ParentWindow.LocalReminderTimeGrid.IsVisible)
        return;
      this.ParentWindow.ShowSubTaskLocalTime();
    }

    private void OnRepeatChanged(object sender, EventArgs obj)
    {
      int num = SetDateTimeControl.IsRepeatFlagChanged(this.Calendar.RepeatFlag, this.TimeData.RepeatFlag) ? 1 : (this.Calendar.RepeatFrom != this.TimeData.RepeatFrom ? 1 : 0);
      this.Calendar.RepeatFlag = this.TimeData.RepeatFlag;
      this.Calendar.RepeatFrom = this.TimeData.RepeatFrom;
      this.Calendar.SelectedDateChanged -= new EventHandler<DateTime>(this.OnSelectedDateChanged);
      if (sender is CustomRepeatDialog)
        this.Calendar.SelectedDate = this.TimeData.StartDate;
      if (Utils.IsEmptyDate(this.Calendar.SelectedDate))
      {
        this.TimeData.StartDate = new DateTime?(DateTime.Today);
        this.Calendar.SelectedDate = new DateTime?(DateTime.Today);
      }
      this.Calendar.SelectedDateChanged += new EventHandler<DateTime>(this.OnSelectedDateChanged);
      if (num != 0)
        this.Calendar.ExDates = new List<string>();
      this.Calendar.RenderRepeat();
    }

    private static bool IsRepeatFlagChanged(string original, string revised)
    {
      if (original == null)
        original = string.Empty;
      if (revised == null)
        revised = string.Empty;
      if (original == revised)
        return false;
      if (revised.StartsWith(original))
      {
        string str = revised.Substring(0, original.Length);
        if (str.ToUpper().StartsWith("COUNT") || str.ToUpper().StartsWith("UNTIL"))
          return false;
      }
      return true;
    }

    private void OnSelectedDateChanged(object sender, DateTime dateTime)
    {
      this.OnDateSelected(dateTime);
    }

    private void OnDateSelected(DateTime dateTime)
    {
      this.ParentWindow.ReminderOrRepeatControl.IsDateSelected = true;
      if (this.TimeData.RepeatFlag != null && this.TimeData.RepeatFlag.Contains("LUNAR"))
      {
        this.TimeData.RepeatFrom = "2";
        this.TimeData.RepeatFlag = RepeatUtils.GetLunarRepeatFlag(dateTime);
      }
      DateTime? startDate = this.TimeData.StartDate;
      if (startDate.HasValue)
      {
        TimeData timeData = this.TimeData;
        startDate = this.TimeData.StartDate;
        DateTime? nullable = new DateTime?(DateUtils.SetDateOnly(startDate.Value, dateTime));
        timeData.StartDate = nullable;
      }
      else
        this.TimeData.StartDate = new DateTime?(dateTime);
      EventHandler<DateTime> dateSelected = this.DateSelected;
      if (dateSelected == null)
        return;
      dateSelected((object) this, dateTime);
    }

    private void OnDateSelectedAndSave(object sender, DateTime e)
    {
      this.OnDateSelected(e);
      this.OnSaveClick((object) null, (object) null);
    }

    private async void OnSaveClick(object o, object o1)
    {
      if (this.TimePointControl.IsVisible)
      {
        this.TimePointControl.FocusEmpty();
        await Task.Delay(100);
      }
      if (!this.TimeData.StartDate.HasValue)
        this.TimeData.StartDate = this.Calendar.SelectedDate;
      if (this.Calendar.SelectedDate.HasValue && this.TimeData.StartDate.HasValue)
      {
        this.TimeData.StartDate = new DateTime?(DateUtils.SetDateOnly(this.TimeData.StartDate.Value, this.Calendar.SelectedDate.Value));
        if (this.TimeData.IsAllDay.GetValueOrDefault())
          this.TimeData.StartDate = new DateTime?(DateUtils.CropHourAndMinute(this.TimeData.StartDate.Value));
      }
      this.ParentWindow?.OnSaved(this.TimeData);
    }

    private void SelectTimeClick(object sender, MouseButtonEventArgs e)
    {
      if (this.TimePointControl.IsMouseOver || this.TimePointControl.PopupOpen || !this._showPopupMouseDown)
        return;
      this.TimePointControl.Visibility = Visibility.Visible;
      this.TimePointControl.ToggleReminderPopup();
      this.TimePointControl.FoucusHourText();
      if (!this.TimeData.IsAllDay.HasValue || this.TimeData.IsAllDay.Value)
      {
        this.TimeData.IsAllDay = new bool?(false);
        this.TimeData.Reminders = TimeData.GetDefaultTimeReminders();
        DateTime selectedTime = this.TimePointControl.SelectedTime;
        this.TimeData.StartDate = new DateTime?(DateUtils.SetHourAndMinuteOnly(this.TimeData.StartDate ?? DateTime.Now, selectedTime.Hour, selectedTime.Minute));
      }
      SetDateDialog parentWindow = this.ParentWindow;
      if ((parentWindow != null ? (parentWindow.LocalReminderTimeGrid.IsVisible ? 1 : 0) : 0) != 0)
        this.ParentWindow?.ShowSubTaskLocalTime();
      this._showPopupMouseDown = false;
    }

    private void OnClearTimeClick(object sender, MouseButtonEventArgs e)
    {
      if (((int) this.TimeData.IsAllDay ?? 1) != 0)
        return;
      e.Handled = true;
      this.ClearTime();
    }

    public void ClearTime()
    {
      if (((int) this.TimeData.IsAllDay ?? 1) != 0)
        return;
      this.TimeData.IsAllDay = new bool?(true);
      this.TimeData.Reminders = (List<TaskReminderModel>) null;
      if (this.ParentWindow != null && !this.ParentWindow.LocalReminderTimeGrid.IsVisible)
        this.TimeData.TimeZone = TimeZoneData.LocalTimeZoneModel;
      this.TimePointControl.Visibility = Visibility.Collapsed;
      this.TimePointControl.HidePopup();
    }

    public void TryClosePopup()
    {
      if (this.TimePointControl.TryCloseTimezonePopup())
        return;
      this.TimePointControl.DropdownPopup.IsOpen = false;
    }

    public void SetTimeInputVisible(bool b)
    {
      this.TimeInputPanel.Visibility = b ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnShowPopupButtonMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (PopupStateManager.CanOpenTimePopup)
        this._showPopupMouseDown = true;
      else
        PopupStateManager.SetCanOpenTimePopup(true);
    }

    public bool SetTabSelect(int index)
    {
      if (index != 0)
      {
        if (index == 1)
        {
          this.TimeTabSelect = true;
          this.Calendar.ClearTabSelected();
          return this.TimeInputPanel.Visibility == Visibility.Visible;
        }
        this.TimeTabSelect = false;
        this.Calendar.ClearTabSelected();
        return true;
      }
      this.Calendar.TabSelectCurrent();
      this.TimeTabSelect = false;
      return true;
    }

    public void MoveTabSelectDate(int step = 1) => this.Calendar.MoveTabSelectDate(step);

    public void SelectTabItem() => this.Calendar.SelectTabItem();

    public void SelectTimeInput()
    {
      this._showPopupMouseDown = true;
      this.SelectTimeClick((object) null, (MouseButtonEventArgs) null);
    }

    public bool HandleEsc()
    {
      if (this.TimePointControl.TryCloseTimezonePopup())
        return true;
      if (!this.TimePointControl.ReminderPopup.IsOpen)
        return false;
      this.TimePointControl.ReminderPopup.IsOpen = false;
      return true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/setdatetimecontrol.xaml", UriKind.Relative));
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
          this.Root = (SetDateTimeControl) target;
          break;
        case 2:
          this.Calendar = (TickDatePicker) target;
          break;
        case 3:
          this.TimeInputPanel = (StackPanel) target;
          break;
        case 4:
          this.DateBorder = (Border) target;
          this.DateBorder.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnShowPopupButtonMouseDown);
          this.DateBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectTimeClick);
          break;
        case 5:
          this.TimePointControl = (TimeInputControl) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClearTimeClick);
          break;
        case 7:
          this.RightIcon = (Path) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
