// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarDisplaySettingWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarDisplaySettingWindow : Window, IComponentConnector
  {
    private const string List = "list";
    private const string Tag = "tag";
    private const string Priority = "priority";
    internal CustomComboBox CalendarColorComboBox;
    internal CheckBox ShowComplete;
    internal CheckBox ShowSubtask;
    internal CheckBox ShowRecycle;
    internal StackPanel ShowHabitPanel;
    internal CheckBox ShowHabit;
    internal CheckBox ShowFocus;
    internal StackPanel ShowCoursePanel;
    internal CheckBox ShowCourse;
    internal StackPanel ShowWeekendPanel;
    internal CheckBox ShowWeekend;
    private bool _contentLoaded;

    public CalendarDisplaySettingWindow()
    {
      this.InitializeComponent();
      string lower = LocalSettings.Settings.CellColorType.ToLower();
      CustomComboBox calendarColorComboBox = this.CalendarColorComboBox;
      ObservableCollection<ComboBoxViewModel> items = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel1 = new ComboBoxViewModel((object) "list", Utils.GetString("lists"), 32.0);
      comboBoxViewModel1.Selected = lower == "list";
      items.Add(comboBoxViewModel1);
      ComboBoxViewModel comboBoxViewModel2 = new ComboBoxViewModel((object) "tag", Utils.GetString("Tags"), 32.0);
      comboBoxViewModel2.Selected = lower == "tag";
      items.Add(comboBoxViewModel2);
      ComboBoxViewModel comboBoxViewModel3 = new ComboBoxViewModel((object) "priority", Utils.GetString("priority"), 32.0);
      comboBoxViewModel3.Selected = lower == "priority";
      items.Add(comboBoxViewModel3);
      calendarColorComboBox.Init<ComboBoxViewModel>(items, (ComboBoxViewModel) null);
      this.ShowComplete.IsChecked = new bool?(LocalSettings.Settings.ShowCompletedInCal);
      this.ShowSubtask.IsChecked = new bool?(LocalSettings.Settings.ShowCheckListInCal);
      this.ShowRecycle.IsChecked = new bool?(LocalSettings.Settings.ShowRepeatCircles);
      this.ShowFocus.IsChecked = new bool?(LocalSettings.Settings.ShowFocusRecord);
      if (LocalSettings.Settings.ShowHabit)
      {
        this.ShowHabitPanel.Visibility = Visibility.Visible;
        this.ShowHabit.IsChecked = new bool?(LocalSettings.Settings.HabitInCal);
      }
      TimeTableModel timeTable = LocalSettings.Settings.UserPreference.TimeTable;
      if ((timeTable != null ? (timeTable.isEnabled ? 1 : 0) : 0) != 0)
      {
        this.ShowCoursePanel.Visibility = Visibility.Visible;
        this.ShowCourse.IsChecked = new bool?(LocalSettings.Settings.UserPreference.TimeTable.displayInCalendar);
      }
      this.ShowWeekend.IsChecked = new bool?(LocalSettings.Settings.ShowCalWeekend);
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding(WindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
      this.Loaded += new RoutedEventHandler(this.SetEvents);
      this.Closing += (CancelEventHandler) ((sender, e) => this.Owner?.Activate());
    }

    private void SetEvents(object sender, RoutedEventArgs e)
    {
      this.ShowComplete.Checked += new RoutedEventHandler(this.OnShowCompletedChanged);
      this.ShowComplete.Unchecked += new RoutedEventHandler(this.OnShowCompletedChanged);
      this.ShowSubtask.Checked += new RoutedEventHandler(this.OnShowCheckListChanged);
      this.ShowSubtask.Unchecked += new RoutedEventHandler(this.OnShowCheckListChanged);
      this.ShowRecycle.Checked += new RoutedEventHandler(this.OnShowRepeatChanged);
      this.ShowRecycle.Unchecked += new RoutedEventHandler(this.OnShowRepeatChanged);
      this.ShowFocus.Checked += new RoutedEventHandler(this.OnShowFocusChanged);
      this.ShowFocus.Unchecked += new RoutedEventHandler(this.OnShowFocusChanged);
      this.ShowHabit.Checked += new RoutedEventHandler(this.OnShowHabitChanged);
      this.ShowHabit.Unchecked += new RoutedEventHandler(this.OnShowHabitChanged);
      this.ShowWeekend.Checked += new RoutedEventHandler(this.OnShowWeekendChanged);
      this.ShowWeekend.Unchecked += new RoutedEventHandler(this.OnShowWeekendChanged);
      this.ShowCourse.Checked += new RoutedEventHandler(this.OnShowCourseChanged);
      this.ShowCourse.Unchecked += new RoutedEventHandler(this.OnShowCourseChanged);
    }

    private void OnShowWeekendChanged(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!(sender is CheckBox checkBox))
        return;
      LocalSettings.Settings.ShowCalWeekend = checkBox.IsChecked.GetValueOrDefault();
      LocalSettings.Settings.Save();
    }

    private void OnShowHabitChanged(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!(sender is CheckBox checkBox))
        return;
      LocalSettings.Settings.HabitInCal = checkBox.IsChecked.GetValueOrDefault();
      LocalSettings.Settings.Save();
      DataChangedNotifier.NotifyCalendarConfigChanged(false);
      HabitService.PushHabitSettings();
      UserActCollectUtils.AddClickEvent("calendar", "view_options", LocalSettings.Settings.HabitInCal ? "show_habit" : "hide_habit");
    }

    private void OnShowFocusChanged(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!(sender is CheckBox checkBox))
        return;
      LocalSettings.Settings.ShowFocusRecord = checkBox.IsChecked.GetValueOrDefault();
      LocalSettings.Settings.Save();
      DataChangedNotifier.NotifyCalendarConfigChanged(false);
      UserActCollectUtils.AddClickEvent("calendar", "view_options", LocalSettings.Settings.ShowFocusRecord ? "show_focus" : "hide_focus");
    }

    private void OnShowFocusClick(object sender, MouseButtonEventArgs e)
    {
      CheckBox showFocus = this.ShowFocus;
      bool? isChecked = this.ShowFocus.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      showFocus.IsChecked = nullable;
    }

    private void OnShowRecycleClick(object sender, MouseButtonEventArgs e)
    {
      CheckBox showRecycle = this.ShowRecycle;
      bool? isChecked = this.ShowRecycle.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      showRecycle.IsChecked = nullable;
    }

    private void OnShowCompleteClick(object sender, MouseButtonEventArgs e)
    {
      CheckBox showComplete = this.ShowComplete;
      bool? isChecked = this.ShowComplete.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      showComplete.IsChecked = nullable;
    }

    private void OnShowSubtaskClick(object sender, MouseButtonEventArgs e)
    {
      CheckBox showSubtask = this.ShowSubtask;
      bool? isChecked = this.ShowSubtask.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      showSubtask.IsChecked = nullable;
    }

    private void OnShowWeekendClick(object sender, MouseButtonEventArgs e)
    {
      CheckBox showWeekend = this.ShowWeekend;
      bool? isChecked = this.ShowWeekend.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      showWeekend.IsChecked = nullable;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnColorSelected(object sender, ComboBoxViewModel e)
    {
      if (!(e.Value is string str))
        return;
      LocalSettings.Settings.CellColorType = str;
      LocalSettings.Settings.Save();
      UserActCollectUtils.AddClickEvent("calendar", "view_options", LocalSettings.Settings.CellColorType);
      DataChangedNotifier.NotifyCalendarConfigChanged(true);
    }

    private void OnShowCompletedChanged(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!(sender is CheckBox checkBox))
        return;
      LocalSettings.Settings.ShowCompletedInCal = checkBox.IsChecked.GetValueOrDefault();
      LocalSettings.Settings.Save();
      UserActCollectUtils.AddClickEvent("calendar", "view_options", LocalSettings.Settings.ShowCompletedInCal ? "show_completed" : "hide_completed");
      DataChangedNotifier.NotifyCalendarConfigChanged(false);
    }

    private void OnShowCheckListChanged(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!(sender is CheckBox checkBox))
        return;
      LocalSettings.Settings.ShowCheckListInCal = checkBox.IsChecked.GetValueOrDefault();
      LocalSettings.Settings.Save();
      UserActCollectUtils.AddClickEvent("calendar", "view_options", LocalSettings.Settings.ShowCheckListInCal ? "show_check_item" : "hide_check_item");
      DataChangedNotifier.NotifyCalendarConfigChanged(false);
    }

    private void OnShowRepeatChanged(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!(sender is CheckBox checkBox))
        return;
      LocalSettings.Settings.ShowRepeatCircles = checkBox.IsChecked.GetValueOrDefault();
      LocalSettings.Settings.Save();
      UserActCollectUtils.AddClickEvent("calendar", "view_options", LocalSettings.Settings.ShowRepeatCircles ? "show_repeats" : "hide_repeats");
      DataChangedNotifier.NotifyCalendarConfigChanged(false);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      this.Owner.Activate();
      SettingsHelper.PushLocalSettings();
      SettingsHelper.PushLocalPreference();
      base.OnClosing(e);
    }

    private void OnShowHabitClick(object sender, MouseButtonEventArgs e)
    {
      CheckBox showHabit = this.ShowHabit;
      bool? isChecked = this.ShowHabit.IsChecked;
      bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
      showHabit.IsChecked = nullable;
    }

    private async void OnShowCourseClick(object sender, MouseButtonEventArgs e)
    {
      CalendarDisplaySettingWindow owner = this;
      e.Handled = true;
      CourseScheduleModel scheduleByIdAsync = await ScheduleDao.GetScheduleByIdAsync(LocalSettings.Settings.UserPreference.TimeTable.currentTimetableId);
      bool? isChecked = owner.ShowCourse.IsChecked;
      if (!isChecked.GetValueOrDefault() && (scheduleByIdAsync == null || !scheduleByIdAsync.AllTimeSet()))
      {
        UtilLog.Info(string.Format("ShowCourseClick ,schedule is null = {0}", (object) (scheduleByIdAsync == null)));
        new CustomerDialog(Utils.GetString("ShowCourse"), Utils.GetString("ShowCourseMessage"), Utils.GetString("GotIt"), string.Empty, (Window) owner).ShowDialog();
      }
      else
      {
        isChecked = owner.ShowCourse.IsChecked;
        UserActCollectUtils.AddClickEvent("calendar", "view_options", isChecked.GetValueOrDefault() ? "show_course" : "hide_course");
        CheckBox showCourse = owner.ShowCourse;
        isChecked = owner.ShowCourse.IsChecked;
        bool? nullable = isChecked.HasValue ? new bool?(!isChecked.GetValueOrDefault()) : new bool?();
        showCourse.IsChecked = nullable;
      }
    }

    private void OnShowCourseChanged(object sender, RoutedEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      if (!(sender is CheckBox checkBox))
        return;
      LocalSettings.Settings.UserPreference.TimeTable.displayInCalendar = checkBox.IsChecked.GetValueOrDefault();
      LocalSettings.Settings.UserPreference.TimeTable.mtime = Utils.GetNowTimeStampInMills();
      LocalSettings.Settings.Save(true);
      DataChangedNotifier.OnScheduleChanged();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/calendardisplaysettingwindow.xaml", UriKind.Relative));
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
          this.CalendarColorComboBox = (CustomComboBox) target;
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnShowCompleteClick);
          break;
        case 3:
          this.ShowComplete = (CheckBox) target;
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnShowSubtaskClick);
          break;
        case 5:
          this.ShowSubtask = (CheckBox) target;
          break;
        case 6:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnShowRecycleClick);
          break;
        case 7:
          this.ShowRecycle = (CheckBox) target;
          break;
        case 8:
          this.ShowHabitPanel = (StackPanel) target;
          this.ShowHabitPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnShowHabitClick);
          break;
        case 9:
          this.ShowHabit = (CheckBox) target;
          break;
        case 10:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnShowFocusClick);
          break;
        case 11:
          this.ShowFocus = (CheckBox) target;
          break;
        case 12:
          this.ShowCoursePanel = (StackPanel) target;
          this.ShowCoursePanel.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnShowCourseClick);
          break;
        case 13:
          this.ShowCourse = (CheckBox) target;
          break;
        case 14:
          this.ShowWeekendPanel = (StackPanel) target;
          this.ShowWeekendPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnShowWeekendClick);
          break;
        case 15:
          this.ShowWeekend = (CheckBox) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
