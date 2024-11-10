// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitReminderWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Remind;
using TickTickDao;
using TickTickModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitReminderWindow : MyWindow, IRemindPop, IComponentConnector
  {
    private HabitModel _habit;
    private ReminderModel _reminder;
    private int _currentIndex = -1;
    internal Border ContainerBorder;
    internal Image WindowIcon;
    internal TextBlock TimeText;
    internal Border FocusIcon;
    internal StackPanel Container;
    internal Image HabitImage;
    internal EmjTextBlock NameText;
    internal StackPanel HabitProgress;
    internal Border BaseBar;
    internal TextBlock ProgressText;
    internal StackPanel ButtonPanel;
    internal Button AutoRecordButton;
    internal Button CompleteAllButton;
    internal Button ManualRecordButton;
    internal Button RecordButton;
    internal Grid ReminderGrid;
    internal CustomComboBox ChooseRemindLaterComboBox;
    internal Popup ManuallyCheckInPopup;
    internal ManualRecordCheckinControl CheckInControl;
    private bool _contentLoaded;

    public HabitReminderWindow()
    {
      this.InitializeComponent();
      this.Left = SystemParameters.WorkArea.Width - 324.0;
      this.Top = SystemParameters.WorkArea.Height - 180.0;
      this.WindowIcon.Source = (ImageSource) AppIconUtils.GetIconImage();
      this.ChooseRemindLaterComboBox.SetPopupPosition(PlacementMode.Top, -5, 2);
      DataChangedNotifier.HabitsSyncDone += new EventHandler(this.OnHabitSyncDone);
      ticktick_WPF.Notifier.GlobalEventManager.RemindHandled += new EventHandler<RemindMessage>(this.OnRemindHandled);
    }

    private void OnRemindHandled(object sender, RemindMessage e)
    {
      if (!this.CheckRemindMatched(this._reminder, e))
        return;
      Utils.RunOnUiThread(this.Dispatcher, new Action(this.TryClose));
    }

    private bool CheckRemindMatched(ReminderModel reminder, RemindMessage remindMessage)
    {
      return !(remindMessage.type != "habit") && reminder.ReminderTime.HasValue && (reminder.ReminderTime.Value - remindMessage.remindTime).Minutes <= 0 && reminder.HabitId == remindMessage.id;
    }

    private async void OnHabitSyncDone(object sender, EventArgs e)
    {
      HabitCheckInModel byHabitIdAndStamp = await HabitCheckInDao.GetHabitCheckInsByHabitIdAndStamp(this._habit?.Id, DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
      if (byHabitIdAndStamp == null || byHabitIdAndStamp.CheckStatus != 1 && byHabitIdAndStamp.Value < byHabitIdAndStamp.Goal)
        return;
      this.TryClose();
    }

    public async Task InitData(ReminderModel reminder)
    {
      HabitReminderWindow habitReminderWindow = this;
      habitReminderWindow._reminder = reminder;
      HabitModel habitById = await HabitDao.GetHabitById(reminder.HabitId);
      HabitDetailViewModel vm;
      if (habitById == null)
      {
        // ISSUE: explicit non-virtual call
        __nonvirtual (habitReminderWindow.TryClose());
        vm = (HabitDetailViewModel) null;
      }
      else
      {
        habitReminderWindow._habit = habitById;
        string key = "";
        DateTime? reminderTime = reminder.ReminderTime;
        DateTime dateTime = reminderTime ?? DateTime.Now;
        int num = dateTime.Hour * 60 + dateTime.Minute;
        if (num <= 540)
          key = "TodayMorning";
        else if (num <= 780)
          key = "TodayAfternoon";
        else if (num <= 1020)
          key = "TodayEvening";
        else if (num <= 1200)
          key = "TodayNight";
        ObservableCollection<ComboBoxViewModel> observableCollection = new ObservableCollection<ComboBoxViewModel>();
        observableCollection.Add(new ComboBoxViewModel((object) "15m", "15 " + Utils.GetString("PublicMinutes"), 32.0));
        observableCollection.Add(new ComboBoxViewModel((object) "30m", "30 " + Utils.GetString("PublicMinutes"), 32.0));
        observableCollection.Add(new ComboBoxViewModel((object) "1h", "1 " + Utils.GetString("PublicHour"), 32.0));
        observableCollection.Add(new ComboBoxViewModel((object) "3h", "3 " + Utils.GetString("PublicHours"), 32.0));
        ObservableCollection<ComboBoxViewModel> items = observableCollection;
        if (!string.IsNullOrEmpty(key))
          items.Add(new ComboBoxViewModel((object) key, Utils.GetString(key), 32.0));
        habitReminderWindow.ChooseRemindLaterComboBox.Init<ComboBoxViewModel>(items);
        vm = new HabitDetailViewModel(habitReminderWindow._habit)
        {
          PivotDate = DateTime.Today
        };
        bool flag1 = LocalSettings.Settings.ExtraSettings.RemindDetail == 2;
        if (!flag1)
        {
          bool flag2 = LocalSettings.Settings.ExtraSettings.RemindDetail == 1;
          if (flag2)
            flag2 = await AppLockCache.GetAppLocked();
          flag1 = flag2;
        }
        bool flag3 = flag1;
        if (flag3)
        {
          vm.Name = Utils.GetString("ReminderWhenLock");
          habitReminderWindow.ButtonPanel.Visibility = Visibility.Collapsed;
        }
        habitReminderWindow.HabitProgress.Visibility = flag3 ? Visibility.Collapsed : Visibility.Visible;
        habitReminderWindow.FocusIcon.Visibility = flag3 || !LocalSettings.Settings.EnableFocus ? Visibility.Collapsed : Visibility.Visible;
        List<HabitCheckInModel> checkInsByHabitId = await HabitCheckInDao.GetHabitCheckInsByHabitId(habitReminderWindow._habit.Id);
        if (checkInsByHabitId.Any<HabitCheckInModel>())
        {
          HabitCheckInModel habitCheckInModel = checkInsByHabitId.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (checkIn => checkIn.CheckinStamp == DateTime.Today.ToString("yyyyMMdd")));
          vm.TodayCheckIn = habitCheckInModel;
          if (habitCheckInModel != null)
          {
            vm.Value = habitCheckInModel.Value;
            vm.CompletedRatio = (int) (habitCheckInModel.Value * 1.0 / habitReminderWindow._habit.Goal * 1.0 * 100.0);
            if (habitCheckInModel.Value >= habitReminderWindow._habit.Goal)
              vm.Status = 2;
          }
        }
        habitReminderWindow.InitOperationButton(habitReminderWindow._habit);
        habitReminderWindow.DataContext = (object) vm;
        TextBlock progressText = habitReminderWindow.ProgressText;
        string str;
        if (!(vm.Type?.ToLower() != "real"))
          str = vm.Value.ToString() + " / " + vm.Goal.ToString() + " " + vm.Unit;
        else
          str = string.Format(Utils.GetString(habitReminderWindow._habit.TotalCheckIns > 1 ? "AchievedStreakDays" : "AchievedStreakDay"), (object) habitReminderWindow._habit.TotalCheckIns);
        progressText.Text = str;
        reminderTime = reminder.ReminderTime;
        DateTime date = reminderTime ?? reminder.StartDate ?? DateTime.Now;
        habitReminderWindow.TimeText.Text = ticktick_WPF.Util.DateUtils.FormatSmartDateShortString(date) + ", " + ticktick_WPF.Util.DateUtils.FormatHourMinute(date);
        habitReminderWindow.HabitImage.Source = (ImageSource) ImageUtils.GetResourceImage(vm.ImageUrl, 40);
        vm = (HabitDetailViewModel) null;
      }
    }

    private void InitOperationButton(HabitModel habit)
    {
      this._habit = habit;
      switch (HabitUtils.GetHabitCheckInType(0, this._habit.Type, this._habit.Step))
      {
        case HabitCheckInType.BoolCompleted:
        case HabitCheckInType.BoolUnCompleted:
        case HabitCheckInType.CompletedAllCompleted:
        case HabitCheckInType.CompletedAllUnCompleted:
          this.CompleteAllButton.Visibility = Visibility.Visible;
          break;
        case HabitCheckInType.AutoCompleted:
        case HabitCheckInType.AutoUncompleted:
          this.RecordButton.Visibility = Visibility.Visible;
          this.AutoRecordButton.Visibility = Visibility.Visible;
          this.ManuallyCheckInPopup.PlacementTarget = (UIElement) this.RecordButton;
          this.AutoRecordButton.Content = (object) ("+" + habit.Step.ToString() + " " + habit.Unit);
          this.ReminderGrid.Width = 85.0;
          break;
        case HabitCheckInType.ManualCompleted:
        case HabitCheckInType.ManualUnCompleted:
          this.ManualRecordButton.Visibility = Visibility.Visible;
          this.ManuallyCheckInPopup.PlacementTarget = (UIElement) this.ManualRecordButton;
          break;
      }
    }

    private async void OnCheckInPopupSave(object sender, double amount)
    {
      this.AddClickEvent("manual_record");
      this.ManuallyCheckInPopup.IsOpen = false;
      Utils.PlayCompletionSound();
      await HabitService.CheckInHabit(this._habit.Id, DateTime.Today, amount);
      this.TryClose();
    }

    private void OnCheckInPopupCancel(object sender, EventArgs e)
    {
      this.ManuallyCheckInPopup.IsOpen = false;
    }

    private void OnDismissClick(object sender, RoutedEventArgs e) => this.TryClose();

    private void OnViewDetailClick(object sender, MouseButtonEventArgs e)
    {
      this.AddClickEvent("click_content");
      if (!string.IsNullOrEmpty(this._habit?.Id))
        App.NavigateHabit(this._habit.Id);
      this.TryClose();
    }

    private void OnCloseIconClick(object sender, MouseButtonEventArgs e)
    {
      this.AddClickEvent("x_btn");
      this.TryClose();
    }

    private void OnFocusClick(object sender, MouseButtonEventArgs e)
    {
      this.AddClickEvent("start_focus");
      if (!string.IsNullOrEmpty(this._habit.Id))
      {
        if (!TickFocusManager.ConfirmSwitch(this._habit.Id, this._habit.Name, Window.GetWindow((DependencyObject) this)))
          return;
        TickFocusManager.StartFocus(this._habit.Id, true, true);
        if (LocalSettings.Settings.PomoLocalSetting.AutoShowWidget)
          TickFocusManager.HideOrShowFocusWidget(true);
      }
      this.TryClose();
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (Mouse.LeftButton != MouseButtonState.Pressed)
        return;
      this.LocationChanged -= new EventHandler(this.OnLocationChanged);
      this.LocationChanged += new EventHandler(this.OnLocationChanged);
      this.DragMove();
    }

    private void OnLocationChanged(object sender, EventArgs e)
    {
      ReminderWindowManager.OnWindowMoved((IRemindPop) this);
    }

    private async void ReminderSelectionChanged(object sender, ComboBoxViewModel e)
    {
      ReminderModel reminderModel = this._reminder.Copy();
      if (e.Value is string str)
      {
        string data = str;
        if (str != null)
        {
          switch (str.Length)
          {
            case 2:
              switch (str[0])
              {
                case '1':
                  if (str == "1h")
                  {
                    reminderModel.ReminderTime = new DateTime?(DateTime.Now.AddHours(1.0));
                    break;
                  }
                  break;
                case '3':
                  if (str == "3h")
                  {
                    reminderModel.ReminderTime = new DateTime?(DateTime.Now.AddHours(3.0));
                    break;
                  }
                  break;
              }
              break;
            case 3:
              switch (str[0])
              {
                case '1':
                  if (str == "15m")
                  {
                    reminderModel.ReminderTime = new DateTime?(DateTime.Now.AddMinutes(15.0));
                    break;
                  }
                  break;
                case '3':
                  if (str == "30m")
                  {
                    reminderModel.ReminderTime = new DateTime?(DateTime.Now.AddMinutes(30.0));
                    break;
                  }
                  break;
              }
              break;
            case 10:
              if (str == "TodayNight")
              {
                data = "smart_time";
                reminderModel.ReminderTime = new DateTime?((reminderModel.ReminderTime ?? DateTime.Now).Date.AddHours(20.0));
                break;
              }
              break;
            case 12:
              switch (str[5])
              {
                case 'E':
                  if (str == "TodayEvening")
                  {
                    data = "smart_time";
                    reminderModel.ReminderTime = new DateTime?((reminderModel.ReminderTime ?? DateTime.Now).Date.AddHours(17.0));
                    break;
                  }
                  break;
                case 'M':
                  if (str == "TodayMorning")
                  {
                    data = "smart_time";
                    reminderModel.ReminderTime = new DateTime?((reminderModel.ReminderTime ?? DateTime.Now).Date.AddHours(9.0));
                    break;
                  }
                  break;
              }
              break;
            case 14:
              if (str == "TodayAfternoon")
              {
                data = "smart_time";
                reminderModel.ReminderTime = new DateTime?((reminderModel.ReminderTime ?? DateTime.Now).Date.AddHours(13.0));
                break;
              }
              break;
          }
        }
        UserActCollectUtils.AddClickEvent("reminder_data", "snooze", data);
      }
      this.AddClickEvent("snooze");
      App.ReminderList.Add(reminderModel);
      await ReminderDelayDao.AddModelAsync(new ReminderDelayModel()
      {
        UserId = LocalSettings.Settings.LoginUserId,
        ObjId = this._reminder.GetObjId(),
        Type = this._reminder.GetObjType(),
        RemindTime = this._reminder.StartDate,
        NextTime = reminderModel.ReminderTime,
        SyncStatus = 0
      });
      SyncManager.TryDelaySync();
      this.TryClose();
    }

    private void RemindLaterButtonClick(object sender, RoutedEventArgs e)
    {
      this.ChooseRemindLaterComboBox.IsOpen = true;
    }

    private async void CompleteAllClick(object sender, RoutedEventArgs e)
    {
      this.AddClickEvent(this._habit.Type.ToLower() == "boolean" ? "done" : "complete_all");
      Utils.PlayCompletionSound();
      await HabitService.CheckInHabit(this._habit.Id, DateTime.Today);
      this.TryClose();
    }

    private void OnManuallyRecordClick(object sender, RoutedEventArgs e)
    {
      this.ManuallyCheckInPopup.IsOpen = true;
      this.CheckInControl.Init(this._habit.Unit);
      this.CheckInControl.Cancel -= new EventHandler(this.OnCheckInPopupCancel);
      this.CheckInControl.Cancel += new EventHandler(this.OnCheckInPopupCancel);
      this.CheckInControl.Save -= new EventHandler<double>(this.OnCheckInPopupSave);
      this.CheckInControl.Save += new EventHandler<double>(this.OnCheckInPopupSave);
    }

    private async void OnAutoRecordClick(object sender, RoutedEventArgs e)
    {
      Utils.PlayCompletionSound();
      this.AddClickEvent("auto_record");
      await HabitService.CheckInHabit(this._habit.Id, DateTime.Today, this._habit.Step);
      this.TryClose();
    }

    public bool IsSameReminder(ReminderModel reminder)
    {
      return string.IsNullOrEmpty(reminder.HabitId) && this._reminder.HabitId == reminder.HabitId && this._reminder.Id == reminder.Id;
    }

    public void SetDisplayStyle(int index)
    {
      bool flag = true;
      if (this._currentIndex < 0 && index == 2)
      {
        this._currentIndex = index - 1;
        flag = false;
      }
      int num1 = this._currentIndex < 0 ? (index > 0 ? index + 1 : index) : this._currentIndex;
      if (this.ContainerBorder.RenderTransform is ScaleTransform renderTransform)
      {
        if (flag)
        {
          DoubleAnimation doubleAnimation = AnimationUtils.GetDoubleAnimation(new double?(1.0 - 0.05 * (double) num1), 1.0 - 0.05 * (double) index, 180);
          renderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, (AnimationTimeline) doubleAnimation);
          renderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, (AnimationTimeline) doubleAnimation);
        }
        else
        {
          renderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, (AnimationTimeline) null);
          renderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, (AnimationTimeline) null);
          renderTransform.ScaleX = 1.0 - 0.05 * (double) index;
          renderTransform.ScaleY = 1.0 - 0.05 * (double) index;
        }
      }
      double num2 = SystemParameters.WorkArea.Height - 180.0;
      DoubleAnimation doubleAnimation1 = AnimationUtils.GetDoubleAnimation(new double?(flag ? num2 - (double) (8 * num1) : num2 - (double) (8 * index) + 2.0), num2 - (double) (8 * index), 180);
      this.BeginAnimation(Window.TopProperty, (AnimationTimeline) doubleAnimation1);
      this._currentIndex = index;
      this.Opacity = 1.0;
    }

    public void TryClose()
    {
      Communicator.NotifyCloseReminder(new RemindMessage(this._reminder));
      this.Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      DataChangedNotifier.HabitsSyncDone -= new EventHandler(this.OnHabitSyncDone);
      ticktick_WPF.Notifier.GlobalEventManager.RemindHandled -= new EventHandler<RemindMessage>(this.OnRemindHandled);
      base.OnClosing(e);
    }

    public void TryHide()
    {
      this._currentIndex = -1;
      this.Opacity = 0.0;
      if (!(this.ContainerBorder.RenderTransform is ScaleTransform renderTransform))
        return;
      renderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, (AnimationTimeline) null);
      renderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, (AnimationTimeline) null);
      renderTransform.ScaleX = 0.9;
      renderTransform.ScaleY = 0.9;
      this.BeginAnimation(Window.TopProperty, (AnimationTimeline) null);
      this.Top = SystemParameters.WorkArea.Height - 196.0;
    }

    private void AddClickEvent(string label)
    {
      UserActCollectUtils.AddClickEvent("reminder", "habit_reminder_dialog", label);
    }

    public void ShowWindow()
    {
      IntPtr handle = new WindowInteropHelper((Window) this).Handle;
      this.Opacity = 1.0;
      this.Visibility = Visibility.Visible;
      if (handle != IntPtr.Zero)
      {
        this.Topmost = false;
        this.Topmost = true;
        NativeUtils.ShowWindow(handle, 4);
      }
      else
        this.Show();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitreminderwindow.xaml", UriKind.Relative));
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
          this.ContainerBorder = (Border) target;
          break;
        case 2:
          this.WindowIcon = (Image) target;
          break;
        case 3:
          this.TimeText = (TextBlock) target;
          break;
        case 4:
          ((UIElement) target).PreviewMouseDown += new MouseButtonEventHandler(this.OnPreviewMouseDown);
          break;
        case 5:
          this.FocusIcon = (Border) target;
          this.FocusIcon.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFocusClick);
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseIconClick);
          break;
        case 7:
          this.Container = (StackPanel) target;
          this.Container.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnViewDetailClick);
          break;
        case 8:
          this.HabitImage = (Image) target;
          break;
        case 9:
          this.NameText = (EmjTextBlock) target;
          break;
        case 10:
          this.HabitProgress = (StackPanel) target;
          break;
        case 11:
          this.BaseBar = (Border) target;
          break;
        case 12:
          this.ProgressText = (TextBlock) target;
          break;
        case 13:
          this.ButtonPanel = (StackPanel) target;
          break;
        case 14:
          this.AutoRecordButton = (Button) target;
          this.AutoRecordButton.Click += new RoutedEventHandler(this.OnAutoRecordClick);
          break;
        case 15:
          this.CompleteAllButton = (Button) target;
          this.CompleteAllButton.Click += new RoutedEventHandler(this.CompleteAllClick);
          break;
        case 16:
          this.ManualRecordButton = (Button) target;
          this.ManualRecordButton.Click += new RoutedEventHandler(this.OnManuallyRecordClick);
          break;
        case 17:
          this.RecordButton = (Button) target;
          this.RecordButton.Click += new RoutedEventHandler(this.OnManuallyRecordClick);
          break;
        case 18:
          this.ReminderGrid = (Grid) target;
          break;
        case 19:
          this.ChooseRemindLaterComboBox = (CustomComboBox) target;
          break;
        case 20:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.RemindLaterButtonClick);
          break;
        case 21:
          this.ManuallyCheckInPopup = (Popup) target;
          break;
        case 22:
          this.CheckInControl = (ManualRecordCheckinControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
