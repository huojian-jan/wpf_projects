// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.SelectReminderOrRepeatControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class SelectReminderOrRepeatControl : UserControl, IComponentConnector
  {
    private bool _calendarMode;
    private SetRepeatDialog _repeatDialog;
    public bool IsDateSelected;
    private bool _showRemind = true;
    private bool _showPopupButtonMouseDown;
    public static readonly DependencyProperty TabSelectedIndexProperty = DependencyProperty.Register(nameof (TabSelectedIndex), typeof (int), typeof (SelectReminderOrRepeatControl), new PropertyMetadata((object) -1, (PropertyChangedCallback) null));
    internal SelectReminderOrRepeatControl Root;
    internal Border SelectReminderBtn;
    internal TextBlock EmptyRemindText;
    internal TextBlock ReminderText;
    internal Path ReminderRightPath;
    internal EscPopup SelectReminderPopup;
    internal EscPopup SelectRepeatPopup;
    internal Border SelectRepeatButton;
    internal TextBlock EmptyRepeatText;
    internal TextBlock RepeatText;
    internal Path RepeatRightPath;
    internal Border RepeatEndBt;
    internal Grid RepeatEndGrid;
    internal TextBlock EmptyRepeatEndText;
    internal TextBlock RepeatEndText;
    internal Grid ClearPanel;
    internal Path RepeatEndRightPath;
    internal EscPopup SetRepeatEndPopup;
    internal SetRepeatEndControl SetRepeatEndControl;
    private bool _contentLoaded;

    public event EventHandler PopupOpened;

    public event EventHandler PopupClosed;

    public bool ShowRepeatEnd => this.RepeatEndGrid.IsVisible;

    public int TabSelectedIndex
    {
      get => (int) this.GetValue(SelectReminderOrRepeatControl.TabSelectedIndexProperty);
      set => this.SetValue(SelectReminderOrRepeatControl.TabSelectedIndexProperty, (object) value);
    }

    public SelectReminderOrRepeatControl() => this.InitializeComponent();

    private TimeData TimeData => (TimeData) this.DataContext;

    public event EventHandler RepeatChanged;

    private void SelectReminderClick(object sender, MouseButtonEventArgs eventArgs)
    {
      if (!this._showPopupButtonMouseDown)
        return;
      this.ShowReminderPopup(false);
    }

    private void ShowReminderPopup(bool enterSelect)
    {
      this.DismissInnerPopups();
      SetReminderControl setReminderControl = new SetReminderControl(((int) this.TimeData.IsAllDay ?? 1) != 0, (IEnumerable<TaskReminderModel>) (this.TimeData.Reminders ?? new List<TaskReminderModel>()), this.TimeData.StartDate ?? DateTime.Today, false, enterSelect ? 0 : -1);
      setReminderControl.OnCancel += (EventHandler) ((e, arg) => this.SelectReminderPopup.IsOpen = false);
      setReminderControl.OnSelected += new EventHandler<List<string>>(this.OnReminderSelect);
      this.SelectReminderPopup.Child = (UIElement) setReminderControl;
      this.SelectReminderPopup.IsOpen = true;
      this._showPopupButtonMouseDown = false;
    }

    private void OnReminderSelect(object sender, List<string> reminders)
    {
      this.DismissInnerPopups();
      List<TaskReminderModel> taskReminderModelList = new List<TaskReminderModel>();
      if (reminders != null && reminders.Count > 0)
        taskReminderModelList.AddRange(reminders.Select<string, TaskReminderModel>((Func<string, TaskReminderModel>) (reminder => new TaskReminderModel()
        {
          trigger = reminder,
          id = Utils.GetGuid()
        })));
      this.TimeData.Reminders = taskReminderModelList;
    }

    public void SetCalendarMode(bool calendarMode, bool showRemind)
    {
      this._calendarMode = calendarMode;
      this._showRemind = showRemind;
      this.SelectReminderBtn.Visibility = showRemind ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SelectRepeatClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._showPopupButtonMouseDown)
        return;
      this.ShowRepeatPopup();
    }

    private void ShowRepeatPopup()
    {
      EscPopup selectRepeatPopup = this.SelectRepeatPopup;
      RepeatExtra repeat = new RepeatExtra();
      repeat.DefaultDate = this.TimeData.StartDate ?? DateTime.Now;
      repeat.RepeatFrom = this.TimeData.RepeatFrom;
      repeat.RepeatFlag = this.TimeData.RepeatFlag;
      int num = this._calendarMode ? 1 : 0;
      this._repeatDialog = new SetRepeatDialog((Popup) selectRepeatPopup, repeat, num != 0);
      this._repeatDialog.RepeatSelect += new EventHandler<RepeatExtra>(this.OnRepeatSelect);
      this.SelectRepeatPopup.IsOpen = true;
      this._showPopupButtonMouseDown = false;
    }

    private void OnRepeatSelect(object sender, RepeatExtra repeat)
    {
      List<HolidayModel> cacheHolidays = HolidayManager.GetCacheHolidays();
      if (!this.IsDateSelected)
      {
        DateTime? nullable = this.TimeData.StartDate;
        DateTime dateTime = nullable ?? DateTime.Today;
        if (sender is CustomRepeatDialog)
        {
          string repeatFlag = repeat.RepeatFlag;
          List<HolidayModel> holidays = cacheHolidays;
          DateTime? date = new DateTime?(DateTime.Today.AddDays(-1.0));
          nullable = new DateTime?();
          DateTime? completeDate = nullable;
          string repeatFrom = repeat.RepeatFrom;
          dateTime = RepeatUtils.RRule2NextDateTime(repeatFlag, holidays, date, completeDate, repeatFrom).AddHours((double) dateTime.Hour).AddMinutes((double) dateTime.Minute);
        }
        this.TimeData.StartDate = new DateTime?(dateTime);
      }
      this.TimeData.RepeatFrom = repeat.RepeatFrom;
      this.TimeData.RepeatFlag = repeat.RepeatFlag;
      EventHandler repeatChanged = this.RepeatChanged;
      if (repeatChanged == null)
        return;
      repeatChanged(sender, (EventArgs) null);
    }

    public void DismissInnerPopups()
    {
      this.SelectRepeatPopup.IsOpen = false;
      this.SelectReminderPopup.IsOpen = false;
    }

    private void OnPopupClick(object sender, MouseButtonEventArgs e) => e.Handled = true;

    private void SetRepeatEndClick(object sender, MouseButtonEventArgs e)
    {
      if (!this._showPopupButtonMouseDown)
        return;
      this.ShowRepeatEndPopup(false);
    }

    private void ShowRepeatEndPopup(bool enter)
    {
      this.SetRepeatEndPopup.IsOpen = true;
      this.SetRepeatEndControl.Init(enter);
      this._showPopupButtonMouseDown = false;
    }

    private void OnClearRepeatClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.TimeData.RepeatFlag = string.Empty;
      EventHandler repeatChanged = this.RepeatChanged;
      if (repeatChanged == null)
        return;
      repeatChanged((object) this, (EventArgs) null);
    }

    private void OnClearRemindClick(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      this.TimeData.Reminders = new List<TaskReminderModel>();
    }

    private void OnClearRepeatEndClick(object sender, RoutedEventArgs e)
    {
      e.Handled = true;
      this.ClearRepeatEnd();
    }

    private void ClearRepeatEnd()
    {
      this.SetRepeatEndPopup.IsOpen = false;
      this.TimeData.RepeatFlag = RepeatUtils.GetRepeatFlag(this.TimeData.RepeatFlag, new DateTime());
      EventHandler repeatChanged = this.RepeatChanged;
      if (repeatChanged == null)
        return;
      repeatChanged((object) this, (EventArgs) null);
    }

    public void ShowRepeat(bool show)
    {
      if (show)
      {
        this.SelectRepeatButton.Visibility = Visibility.Visible;
        this.RepeatEndBt.Visibility = Visibility.Visible;
      }
      else
      {
        this.SelectRepeatButton.Visibility = Visibility.Collapsed;
        this.RepeatEndBt.Visibility = Visibility.Collapsed;
      }
    }

    private void OnReminderPopupOpened(object sender, EventArgs e)
    {
      this.OnPopupOpened(sender, e);
      this.ReminderRightPath.RenderTransform = (Transform) new RotateTransform(0.0);
    }

    private void OnReminderPopupClosed(object sender, EventArgs e)
    {
      this.OnPopupClosed(sender, e);
      this.ReminderRightPath.RenderTransform = (Transform) new RotateTransform(-90.0);
    }

    private void OnRepeatPopupOpened(object sender, EventArgs e)
    {
      this.OnPopupOpened(sender, e);
      this.RepeatRightPath.RenderTransform = (Transform) new RotateTransform(0.0);
    }

    private void OnRepeatPopupClosed(object sender, EventArgs e)
    {
      this.OnPopupClosed(sender, e);
      this.RepeatRightPath.RenderTransform = (Transform) new RotateTransform(-90.0);
    }

    private void OnRepeatEndPopupOpened(object sender, EventArgs e)
    {
      this.OnPopupOpened(sender, e);
      this.RepeatEndRightPath.RenderTransform = (Transform) new RotateTransform(0.0);
    }

    private void OnRepeatEndPopupClosed(object sender, EventArgs e)
    {
      this.OnPopupClosed(sender, e);
      this.RepeatEndRightPath.RenderTransform = (Transform) new RotateTransform(-90.0);
    }

    private void OnPopupOpened(object sender, EventArgs e)
    {
      PopupStateManager.SetCanOpenTimePopup(false);
      EventHandler popupOpened = this.PopupOpened;
      if (popupOpened == null)
        return;
      popupOpened((object) this, (EventArgs) null);
    }

    private void OnPopupClosed(object sender, EventArgs e)
    {
      PopupStateManager.SetCanOpenTimePopup(true, true);
      EventHandler popupClosed = this.PopupClosed;
      if (popupClosed != null)
        popupClosed((object) this, (EventArgs) null);
      this.SelectRepeatPopup.Child = (UIElement) null;
      this._repeatDialog = (SetRepeatDialog) null;
    }

    public void TryClosePopup()
    {
      this.SelectReminderPopup.IsOpen = false;
      this.SetRepeatEndPopup.IsOpen = false;
      this.SelectRepeatPopup.IsOpen = false;
      this._repeatDialog?.ClosePopup();
    }

    private void OnShowPopupButtonMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (PopupStateManager.CanOpenTimePopup)
        this._showPopupButtonMouseDown = true;
      else
        PopupStateManager.SetCanOpenTimePopup(true);
    }

    public bool SetTabSelect(int index)
    {
      if (index > 2 || index < 0)
        index = -1;
      else if (!this.IsVisible)
        return false;
      if (index != this.TabSelectedIndex)
        this.TabSelectedIndex = index;
      switch (this.TabSelectedIndex)
      {
        case 0:
          return this.SelectReminderBtn.Visibility == Visibility.Visible;
        case 1:
          return this.SelectRepeatButton.Visibility == Visibility.Visible;
        case 2:
          return this.ShowRepeatEnd;
        default:
          return true;
      }
    }

    public bool HandleKeyUp(Key key)
    {
      if (key == Key.Return)
      {
        switch (this.TabSelectedIndex)
        {
          case 0:
            if (this.SelectReminderBtn.Visibility == Visibility.Visible)
            {
              this.ShowReminderPopup(true);
              break;
            }
            break;
          case 1:
            if (this.SelectRepeatButton.Visibility == Visibility.Visible)
            {
              this.ShowRepeatPopup();
              break;
            }
            break;
          case 2:
            if (this.ShowRepeatEnd)
            {
              this.ShowRepeatEndPopup(true);
              break;
            }
            break;
        }
      }
      return false;
    }

    public bool TabHandled(bool shift)
    {
      if (this.SelectReminderPopup.IsOpen)
        this.SelectReminderPopup.HandleTab(shift);
      if (this.SelectRepeatPopup.IsOpen)
        this.SelectRepeatPopup.HandleTab(shift);
      if (this.SetRepeatEndPopup.IsOpen)
        this.SetRepeatEndPopup.HandleTab(shift);
      return this.SelectReminderPopup.IsOpen || this.SelectRepeatPopup.IsOpen || this.SetRepeatEndPopup.IsOpen;
    }

    public bool HandleEnter()
    {
      if (this.SelectReminderPopup.IsOpen)
        this.SelectReminderPopup.HandleEnter();
      if (this.SelectRepeatPopup.IsOpen)
        this.SelectRepeatPopup.HandleEnter();
      if (this.SetRepeatEndPopup.IsOpen)
        this.SetRepeatEndPopup.HandleEnter();
      return this.SelectReminderPopup.IsOpen || this.SelectRepeatPopup.IsOpen || this.SetRepeatEndPopup.IsOpen;
    }

    public bool HandleUpDownSelect(bool isUp)
    {
      if (this.SelectReminderPopup.IsOpen)
        return this.SelectReminderPopup.HandleUpDown(isUp);
      if (this.SelectRepeatPopup.IsOpen)
        return this.SelectRepeatPopup.HandleUpDown(isUp);
      return !this.SetRepeatEndPopup.IsOpen || this.SetRepeatEndPopup.HandleUpDown(isUp);
    }

    public bool HandleLeftRightSelect(bool isLeft)
    {
      if (this.SelectReminderPopup.IsOpen)
        return this.SelectReminderPopup.HandleLeftRight(isLeft);
      if (this.SelectRepeatPopup.IsOpen)
        return this.SelectRepeatPopup.HandleLeftRight(isLeft);
      return !this.SetRepeatEndPopup.IsOpen || this.SetRepeatEndPopup.HandleLeftRight(isLeft);
    }

    public bool HandleEsc()
    {
      if (this.SelectReminderPopup.IsOpen)
        this.SelectReminderPopup.HandleEsc();
      if (this.SelectRepeatPopup.IsOpen)
        this.SelectRepeatPopup.HandleEsc();
      if (this.SetRepeatEndPopup.IsOpen)
        this.SetRepeatEndPopup.HandleEsc();
      return this.SelectReminderPopup.IsOpen || this.SelectRepeatPopup.IsOpen || this.SetRepeatEndPopup.IsOpen;
    }

    private void OnRepeatEndCancel(object sender, EventArgs e)
    {
      this.SetRepeatEndPopup.IsOpen = false;
    }

    private void OnRepeatEndChanged(int count, DateTime until)
    {
      if (count < 0 && until == new DateTime())
      {
        this.ClearRepeatEnd();
      }
      else
      {
        this.TimeData.RepeatFlag = RepeatUtils.GetRepeatFlag(this.TimeData.RepeatFlag, until, count < 0 ? -1 : Math.Min(200, Math.Max(1, count)));
        EventHandler repeatChanged = this.RepeatChanged;
        if (repeatChanged != null)
          repeatChanged((object) this, (EventArgs) null);
        this.SetRepeatEndPopup.IsOpen = false;
      }
    }

    public void ClearReminderOrRepeat()
    {
      if (this.SelectReminderPopup.IsOpen || this.SelectRepeatPopup.IsOpen || this.SetRepeatEndPopup.IsOpen)
        return;
      switch (this.TabSelectedIndex)
      {
        case 0:
          this.TimeData.Reminders = new List<TaskReminderModel>();
          break;
        case 1:
          this.TimeData.RepeatFlag = string.Empty;
          EventHandler repeatChanged = this.RepeatChanged;
          if (repeatChanged == null)
            break;
          repeatChanged((object) this, (EventArgs) null);
          break;
        case 2:
          this.ClearRepeatEnd();
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/selectreminderorrepeatcontrol.xaml", UriKind.Relative));
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
          this.Root = (SelectReminderOrRepeatControl) target;
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnPopupClick);
          break;
        case 3:
          this.SelectReminderBtn = (Border) target;
          this.SelectReminderBtn.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnShowPopupButtonMouseDown);
          this.SelectReminderBtn.MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectReminderClick);
          break;
        case 4:
          this.EmptyRemindText = (TextBlock) target;
          break;
        case 5:
          this.ReminderText = (TextBlock) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClearRemindClick);
          break;
        case 7:
          this.ReminderRightPath = (Path) target;
          break;
        case 8:
          this.SelectReminderPopup = (EscPopup) target;
          break;
        case 9:
          this.SelectRepeatPopup = (EscPopup) target;
          break;
        case 10:
          this.SelectRepeatButton = (Border) target;
          this.SelectRepeatButton.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnShowPopupButtonMouseDown);
          this.SelectRepeatButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.SelectRepeatClick);
          break;
        case 11:
          this.EmptyRepeatText = (TextBlock) target;
          break;
        case 12:
          this.RepeatText = (TextBlock) target;
          break;
        case 13:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClearRepeatClick);
          break;
        case 14:
          this.RepeatRightPath = (Path) target;
          break;
        case 15:
          this.RepeatEndBt = (Border) target;
          break;
        case 16:
          this.RepeatEndGrid = (Grid) target;
          this.RepeatEndGrid.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnShowPopupButtonMouseDown);
          this.RepeatEndGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.SetRepeatEndClick);
          break;
        case 17:
          this.EmptyRepeatEndText = (TextBlock) target;
          break;
        case 18:
          this.RepeatEndText = (TextBlock) target;
          break;
        case 19:
          this.ClearPanel = (Grid) target;
          break;
        case 20:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClearRepeatEndClick);
          break;
        case 21:
          this.RepeatEndRightPath = (Path) target;
          break;
        case 22:
          this.SetRepeatEndPopup = (EscPopup) target;
          break;
        case 23:
          this.SetRepeatEndControl = (SetRepeatEndControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
