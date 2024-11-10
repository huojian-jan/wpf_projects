// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.DateTimeConfig
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
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class DateTimeConfig : UserControl, IComponentConnector
  {
    internal CustomSimpleComboBox TimeFormatComboBox;
    internal CustomSimpleComboBox WeekStartFromComboBox;
    internal Grid LunarGrid;
    internal CheckBox LunarToggle;
    internal Grid AlternateCalendarGrid;
    internal CustomSimpleComboBox OtherCalendarComboBox;
    internal Grid ShowWeekGrid;
    internal CheckBox ShowWeekNumToggle;
    internal Grid HolidayGrid;
    internal CheckBox EnableHolidayToggle;
    private bool _contentLoaded;

    public DateTimeConfig()
    {
      this.InitializeComponent();
      this.InitData();
    }

    private void InitData()
    {
      this.InitTimeFormat();
      this.InitWeekStartFrom();
      this.InitLunarHolidayPanel();
      this.InitAlternativeCalendar();
    }

    private void InitAlternativeCalendar()
    {
      this.OtherCalendarComboBox.ItemsSource = DateTimeConfig.CalendarDisplayInfo.GetCalendarDisplayNames();
      AlternativeCalendar alternativeCalendar = LocalSettings.Settings.UserPreference.alternativeCalendar;
      if (alternativeCalendar != null)
      {
        int indexByKey = DateTimeConfig.CalendarDisplayInfo.GetIndexByKey(alternativeCalendar.calendar);
        if (indexByKey == -1)
          return;
        this.OtherCalendarComboBox.SelectedIndex = indexByKey;
      }
      else
        this.OtherCalendarComboBox.SelectedIndex = 0;
    }

    private void InitLunarHolidayPanel()
    {
      this.LunarGrid.Visibility = Visibility.Collapsed;
      this.HolidayGrid.Visibility = Visibility.Collapsed;
      if (Utils.IsShowHoliday())
        this.HolidayGrid.Visibility = Visibility.Visible;
      this.AlternateCalendarGrid.Visibility = Visibility.Collapsed;
      this.LunarGrid.Visibility = Visibility.Visible;
    }

    private void InitWeekStartFrom()
    {
      this.WeekStartFromComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("Sunday"),
        Utils.GetString("Monday"),
        Utils.GetString("Saturday")
      };
      switch (LocalSettings.Settings.WeekStartFrom)
      {
        case "Sunday":
          this.WeekStartFromComboBox.SelectedIndex = 0;
          break;
        case "Monday":
          this.WeekStartFromComboBox.SelectedIndex = 1;
          break;
        case "Saturday":
          this.WeekStartFromComboBox.SelectedIndex = 2;
          break;
        default:
          this.WeekStartFromComboBox.SelectedIndex = 0;
          break;
      }
    }

    private void InitTimeFormat()
    {
      this.TimeFormatComboBox.ItemsSource = new List<string>()
      {
        Utils.GetString("SystemDefault"),
        Utils.GetString("12Hour") + " (1:00 PM)",
        Utils.GetString("24Hour") + " (13:00)"
      };
      this.TimeFormatComboBox.SelectedIndex = LocalSettings.Settings.SettingsModel.TimeFormat == "24Hour" ? 2 : (!(LocalSettings.Settings.SettingsModel.TimeFormat == "System") ? 1 : 0);
    }

    private async void OnEnableLunarClick(object sender, MouseButtonEventArgs e)
    {
      DateTimeConfig child = this;
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      LocalSettings.Settings.EnableLunar = !LocalSettings.Settings.EnableLunar;
      Utils.FindParent<SettingDialog>((DependencyObject) child)?.SetChanged();
    }

    private async void OnEnableHolidayClick(object sender, MouseButtonEventArgs e)
    {
      DateTimeConfig child = this;
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      LocalSettings.Settings.EnableHoliday = !LocalSettings.Settings.EnableHoliday;
      if (LocalSettings.Settings.EnableHoliday)
        HolidayManager.ForcePullHolidays();
      Utils.FindParent<SettingDialog>((DependencyObject) child)?.SetChanged();
    }

    private async void OnShowWeekClick(object sender, MouseButtonEventArgs e)
    {
      DateTimeConfig child = this;
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      LocalSettings.Settings.ShowWeek = !LocalSettings.Settings.ShowWeek;
      Utils.FindParent<SettingDialog>((DependencyObject) child)?.SetChanged();
    }

    private void EnableTimeZoneClick(object sender, MouseButtonEventArgs e)
    {
      Mouse.Capture((IInputElement) null);
      e.Handled = true;
      LocalSettings.Settings.EnableTimeZone = !LocalSettings.Settings.EnableTimeZone;
      Utils.FindParent<SettingDialog>((DependencyObject) this)?.SetChanged();
    }

    private async void OtherCalendarSelectionChanged(object sender, SimpleComboBoxViewModel e)
    {
      int selectedIndex = this.OtherCalendarComboBox.SelectedIndex;
      UserPreferenceModel userPreference = LocalSettings.Settings.UserPreference;
      AlternativeCalendar alternativeCalendar = new AlternativeCalendar();
      alternativeCalendar.calendar = DateTimeConfig.CalendarDisplayInfo.GetKeyByIndex(selectedIndex);
      alternativeCalendar.mtime = Utils.GetNowTimeStampInMills();
      userPreference.alternativeCalendar = alternativeCalendar;
      LocalSettings.Settings.UserPreference.mtime = Utils.GetNowTimeStampInMills();
      LocalSettings.Settings.Save();
      await SettingsHelper.PushLocalPreference();
    }

    private void WeekStartFromSelectionChanged(object sender, SimpleComboBoxViewModel e)
    {
      switch (this.WeekStartFromComboBox.SelectedIndex)
      {
        case 0:
          LocalSettings.Settings.WeekStartFrom = "Sunday";
          break;
        case 1:
          LocalSettings.Settings.WeekStartFrom = "Monday";
          break;
        case 2:
          LocalSettings.Settings.WeekStartFrom = "Saturday";
          break;
        default:
          LocalSettings.Settings.WeekStartFrom = "Sunday";
          break;
      }
      LocalSettings.Settings.Save();
      Utils.FindParent<SettingDialog>((DependencyObject) this)?.SetChanged();
    }

    private void OnTimeFormatSelectionChanged(object sender, SimpleComboBoxViewModel e)
    {
      switch (this.TimeFormatComboBox.SelectedIndex)
      {
        case 0:
          LocalSettings.Settings.TimeFormat = "System";
          break;
        case 1:
          LocalSettings.Settings.TimeFormat = "12Hour";
          break;
        case 2:
          LocalSettings.Settings.TimeFormat = "24Hour";
          break;
        default:
          LocalSettings.Settings.TimeFormat = "System";
          break;
      }
      LocalSettings.Settings.Save();
      DataChangedNotifier.NotifyTimeFormatChanged();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/datetimeconfig.xaml", UriKind.Relative));
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
          this.TimeFormatComboBox = (CustomSimpleComboBox) target;
          break;
        case 2:
          this.WeekStartFromComboBox = (CustomSimpleComboBox) target;
          break;
        case 3:
          this.LunarGrid = (Grid) target;
          break;
        case 4:
          this.LunarToggle = (CheckBox) target;
          this.LunarToggle.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnEnableLunarClick);
          break;
        case 5:
          this.AlternateCalendarGrid = (Grid) target;
          break;
        case 6:
          this.OtherCalendarComboBox = (CustomSimpleComboBox) target;
          break;
        case 7:
          this.ShowWeekGrid = (Grid) target;
          break;
        case 8:
          this.ShowWeekNumToggle = (CheckBox) target;
          this.ShowWeekNumToggle.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnShowWeekClick);
          break;
        case 9:
          this.HolidayGrid = (Grid) target;
          break;
        case 10:
          this.EnableHolidayToggle = (CheckBox) target;
          this.EnableHolidayToggle.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnEnableHolidayClick);
          break;
        case 11:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.EnableTimeZoneClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    private class CalendarDisplayInfo
    {
      public string Key { get; set; }

      public string Value { get; set; }

      public static List<DateTimeConfig.CalendarDisplayInfo> GetCalendarDisplayInfo()
      {
        List<DateTimeConfig.CalendarDisplayInfo> list = new List<DateTimeConfig.CalendarDisplayInfo>()
        {
          new DateTimeConfig.CalendarDisplayInfo()
          {
            Key = "korean-lunar",
            Value = Utils.GetString("korean_lunar")
          },
          new DateTimeConfig.CalendarDisplayInfo()
          {
            Key = "hebcal",
            Value = Utils.GetString("hebrew")
          },
          new DateTimeConfig.CalendarDisplayInfo()
          {
            Key = "hijri",
            Value = Utils.GetString("hijri")
          },
          new DateTimeConfig.CalendarDisplayInfo()
          {
            Key = "shaka",
            Value = Utils.GetString("saka_era")
          },
          new DateTimeConfig.CalendarDisplayInfo()
          {
            Key = "lunar",
            Value = Utils.GetString("chinese_lunar")
          }
        }.OrderBy<DateTimeConfig.CalendarDisplayInfo, string>((Func<DateTimeConfig.CalendarDisplayInfo, string>) (it => it.Value)).ToList<DateTimeConfig.CalendarDisplayInfo>();
        list.Insert(0, new DateTimeConfig.CalendarDisplayInfo()
        {
          Key = "",
          Value = Utils.GetString("none")
        });
        return list;
      }

      public static List<string> GetCalendarDisplayNames()
      {
        return DateTimeConfig.CalendarDisplayInfo.GetCalendarDisplayInfo().Select<DateTimeConfig.CalendarDisplayInfo, string>((Func<DateTimeConfig.CalendarDisplayInfo, string>) (it => it.Value)).ToList<string>();
      }

      private static List<string> GetCalendarKeyList()
      {
        return DateTimeConfig.CalendarDisplayInfo.GetCalendarDisplayInfo().Select<DateTimeConfig.CalendarDisplayInfo, string>((Func<DateTimeConfig.CalendarDisplayInfo, string>) (it => it.Key)).ToList<string>();
      }

      public static string GetKeyByIndex(int index)
      {
        List<string> calendarKeyList = DateTimeConfig.CalendarDisplayInfo.GetCalendarKeyList();
        return index >= 0 && index < calendarKeyList.Count ? calendarKeyList[index] : "";
      }

      public static int GetIndexByKey(string key)
      {
        List<string> calendarKeyList = DateTimeConfig.CalendarDisplayInfo.GetCalendarKeyList();
        return !calendarKeyList.Contains(key) ? 0 : calendarKeyList.IndexOf(key);
      }
    }
  }
}
