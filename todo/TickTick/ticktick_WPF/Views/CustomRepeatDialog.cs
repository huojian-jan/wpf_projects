// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.CustomRepeatDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net;
using Ical.Net.DataTypes;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views
{
  public class CustomRepeatDialog : UserControl, ITabControl, IComponentConnector
  {
    private RecurrencePattern _pattern;
    private Popup _popup;
    private bool _calendarMode;
    private DateTime _startTime;
    private RepeatFromType _repeatFrom;
    private string _repeatFlag = string.Empty;
    private Dictionary<DateTime, bool> _showedDays = new Dictionary<DateTime, bool>();
    private ObservableCollection<ComboBoxViewModel> _repeatModeItems;
    private ObservableCollection<ComboBoxViewModel> _repeatUnitItems;
    private ObservableCollection<ComboBoxViewModel> _workdayItems;
    private ObservableCollection<ComboBoxViewModel> _monthByWeekNumItems;
    private ObservableCollection<ComboBoxViewModel> _monthByWeekDayItems;
    private string _flagCustom;
    private List<DateTime> _customDateList = new List<DateTime>();
    private static readonly string RepeatByDue = Utils.GetString(nameof (RepeatByDue));
    private static readonly string RepeatByComplete = Utils.GetString(nameof (RepeatByComplete));
    private static readonly string RepeatByCustom = Utils.GetString(nameof (RepeatByCustom));
    private static readonly string Day = Utils.GetString("PublicDay");
    private static readonly string Week = Utils.GetString("PublicWeek");
    private static readonly string Month = Utils.GetString("PublicMonth");
    private static readonly string Year = Utils.GetString("PublicYear");
    private static readonly string FirstWorkday = Utils.GetString(nameof (FirstWorkday));
    private static readonly string LastWorkday = Utils.GetString(nameof (LastWorkday));
    private int _tabIndex;
    internal CustomComboBox RepeatModeComboBox;
    internal TextBlock RepeatByDateText;
    internal Grid CustomChooseRepeatUnitGrid;
    internal TextBox CustomChooseRepeatUnitTimePopupTextBox;
    internal TextBox EmptyBox;
    internal CustomComboBox ChooseRepeatUnitComboBox;
    internal Grid CustomChooseRepeatUnitPopupGrid;
    internal WeekdaySelector WeekDaySelector;
    internal Grid CustomChooseRepeatUnitMonthPopupGrid;
    internal GroupTitle2 SwitchGroupTitles;
    internal MonthDaySelector MonthDaySelector;
    internal Grid WorkdayGrid;
    internal CustomComboBox WorkdayCombox;
    internal Border YearMonthGrid;
    internal CustomComboBox YearMonthCombobox;
    internal Grid CustomChooseRepeatUnitMonthByWeekPopupGridGrid;
    internal CustomComboBox CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox;
    internal CustomComboBox CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox;
    internal TickDatePicker CustomDaySelector;
    internal Border SkipHolidayGrid;
    internal CheckBox SkipHolidayCheckbox;
    internal Border SkipWeekendGrid;
    internal CheckBox SkipWeekendCheckbox;
    internal Button CancelButton;
    internal Button SaveButton;
    private bool _contentLoaded;

    private string _realRepeatFrom => this._repeatFrom != RepeatFromType.CompleteTime ? "0" : "1";

    public CustomRepeatDialog()
    {
      this.InitializeComponent();
      this.InitRepeatModeComboBox();
      this.InitRepeatUnitComboBox();
      this.InitWorkdayComboBox();
      this.InitMonthByWeekComboBox();
    }

    private void InitMonthByWeekComboBox()
    {
      string title1 = Utils.GetString("TheFirst");
      string title2 = Utils.GetString("TheSecond");
      string title3 = Utils.GetString("TheThird");
      string title4 = Utils.GetString("TheFourth");
      string title5 = Utils.GetString("TheFifth");
      string title6 = Utils.GetString("TheLast");
      ObservableCollection<ComboBoxViewModel> observableCollection1 = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel1 = new ComboBoxViewModel((object) title1, title1, 32.0);
      comboBoxViewModel1.Selected = true;
      observableCollection1.Add(comboBoxViewModel1);
      observableCollection1.Add(new ComboBoxViewModel((object) title2, title2, 32.0));
      observableCollection1.Add(new ComboBoxViewModel((object) title3, title3, 32.0));
      observableCollection1.Add(new ComboBoxViewModel((object) title4, title4, 32.0));
      observableCollection1.Add(new ComboBoxViewModel((object) title5, title5, 32.0));
      observableCollection1.Add(new ComboBoxViewModel((object) title6, title6, 32.0));
      this._monthByWeekNumItems = observableCollection1;
      this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.Init<ComboBoxViewModel>(this._monthByWeekNumItems, this._monthByWeekNumItems[0]);
      string title7 = Utils.GetString("Sun");
      string title8 = Utils.GetString("Mon");
      string title9 = Utils.GetString("Tues");
      string title10 = Utils.GetString("Wed");
      string title11 = Utils.GetString("Thur");
      string title12 = Utils.GetString("Fri");
      string title13 = Utils.GetString("Sat");
      ObservableCollection<ComboBoxViewModel> observableCollection2 = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel2 = new ComboBoxViewModel((object) title7, title7, 32.0);
      comboBoxViewModel2.Selected = true;
      observableCollection2.Add(comboBoxViewModel2);
      observableCollection2.Add(new ComboBoxViewModel((object) title8, title8, 32.0));
      observableCollection2.Add(new ComboBoxViewModel((object) title9, title9, 32.0));
      observableCollection2.Add(new ComboBoxViewModel((object) title10, title10, 32.0));
      observableCollection2.Add(new ComboBoxViewModel((object) title11, title11, 32.0));
      observableCollection2.Add(new ComboBoxViewModel((object) title12, title12, 32.0));
      observableCollection2.Add(new ComboBoxViewModel((object) title13, title13, 32.0));
      this._monthByWeekDayItems = observableCollection2;
      this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.Init<ComboBoxViewModel>(this._monthByWeekDayItems, this._monthByWeekDayItems[0]);
    }

    private void TryInitYearMonthCombobox()
    {
      ObservableCollection<ComboBoxViewModel> items1 = this.YearMonthCombobox.Items;
      // ISSUE: explicit non-virtual call
      if ((items1 != null ? (__nonvirtual (items1.Count) > 0 ? 1 : 0) : 0) != 0)
        return;
      DateTime dateTime = DateTime.Today.AddMonths(1 - DateTime.Today.Month);
      string title1 = dateTime.ToString("MMM", (IFormatProvider) App.Ci);
      string title2 = dateTime.AddMonths(1).ToString("MMM", (IFormatProvider) App.Ci);
      string title3 = dateTime.AddMonths(2).ToString("MMM", (IFormatProvider) App.Ci);
      string title4 = dateTime.AddMonths(3).ToString("MMM", (IFormatProvider) App.Ci);
      string title5 = dateTime.AddMonths(4).ToString("MMM", (IFormatProvider) App.Ci);
      string title6 = dateTime.AddMonths(5).ToString("MMM", (IFormatProvider) App.Ci);
      string title7 = dateTime.AddMonths(6).ToString("MMM", (IFormatProvider) App.Ci);
      string title8 = dateTime.AddMonths(7).ToString("MMM", (IFormatProvider) App.Ci);
      string title9 = dateTime.AddMonths(8).ToString("MMM", (IFormatProvider) App.Ci);
      string title10 = dateTime.AddMonths(9).ToString("MMM", (IFormatProvider) App.Ci);
      string title11 = dateTime.AddMonths(10).ToString("MMM", (IFormatProvider) App.Ci);
      string title12 = dateTime.AddMonths(11).ToString("MMM", (IFormatProvider) App.Ci);
      ObservableCollection<ComboBoxViewModel> observableCollection = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel((object) 1, title1, 32.0);
      comboBoxViewModel.Selected = true;
      observableCollection.Add(comboBoxViewModel);
      observableCollection.Add(new ComboBoxViewModel((object) 2, title2, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) 3, title3, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) 4, title4, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) 5, title5, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) 6, title6, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) 7, title7, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) 8, title8, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) 9, title9, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) 10, title10, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) 11, title11, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) 12, title12, 32.0));
      ObservableCollection<ComboBoxViewModel> items2 = observableCollection;
      this.YearMonthCombobox.Init<ComboBoxViewModel>(items2, items2[0]);
    }

    private void InitWorkdayComboBox()
    {
      ObservableCollection<ComboBoxViewModel> observableCollection = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel((object) CustomRepeatDialog.FirstWorkday, CustomRepeatDialog.FirstWorkday, 32.0);
      comboBoxViewModel.Selected = true;
      observableCollection.Add(comboBoxViewModel);
      observableCollection.Add(new ComboBoxViewModel((object) CustomRepeatDialog.LastWorkday, CustomRepeatDialog.LastWorkday, 32.0));
      this._repeatUnitItems = observableCollection;
      this.WorkdayCombox.Init<ComboBoxViewModel>(this._repeatUnitItems, this._repeatUnitItems[0]);
    }

    private void InitRepeatUnitComboBox()
    {
      ObservableCollection<ComboBoxViewModel> observableCollection = new ObservableCollection<ComboBoxViewModel>();
      observableCollection.Add(new ComboBoxViewModel((object) CustomRepeatDialog.Day, CustomRepeatDialog.Day, 32.0));
      ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel((object) CustomRepeatDialog.Week, CustomRepeatDialog.Week, 32.0);
      comboBoxViewModel.Selected = true;
      observableCollection.Add(comboBoxViewModel);
      observableCollection.Add(new ComboBoxViewModel((object) CustomRepeatDialog.Month, CustomRepeatDialog.Month, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) CustomRepeatDialog.Year, CustomRepeatDialog.Year, 32.0));
      this._repeatUnitItems = observableCollection;
      this.ChooseRepeatUnitComboBox.Init<ComboBoxViewModel>(this._repeatUnitItems, this._repeatUnitItems[1]);
    }

    private void InitRepeatModeComboBox()
    {
      ObservableCollection<ComboBoxViewModel> observableCollection = new ObservableCollection<ComboBoxViewModel>();
      ComboBoxViewModel comboBoxViewModel = new ComboBoxViewModel((object) CustomRepeatDialog.RepeatByDue, CustomRepeatDialog.RepeatByDue, 32.0);
      comboBoxViewModel.Selected = true;
      observableCollection.Add(comboBoxViewModel);
      observableCollection.Add(new ComboBoxViewModel((object) CustomRepeatDialog.RepeatByComplete, CustomRepeatDialog.RepeatByComplete, 32.0));
      observableCollection.Add(new ComboBoxViewModel((object) CustomRepeatDialog.RepeatByCustom, CustomRepeatDialog.RepeatByCustom, 32.0));
      this._repeatModeItems = observableCollection;
      this.RepeatModeComboBox.Init<ComboBoxViewModel>(this._repeatModeItems, this._repeatModeItems[0]);
    }

    public void Init(
      Popup popup,
      RepeatFromType repeatFrom,
      string repeatFlag,
      bool calendarMode,
      DateTime? startTime)
    {
      this._popup = popup;
      this._repeatFrom = repeatFrom;
      if (this._repeatFrom == RepeatFromType.Custom)
      {
        this._flagCustom = repeatFlag;
      }
      else
      {
        this._repeatFlag = repeatFlag;
        this._flagCustom = string.Empty;
      }
      this._pattern = (RecurrencePattern) RecurrenceModel.GetRecurrenceModel(repeatFlag);
      this._calendarMode = calendarMode;
      this._startTime = startTime ?? DateTime.Now;
      this._startTime = this._startTime.Date;
      if (calendarMode)
      {
        this.SkipHolidayGrid.Visibility = Visibility.Collapsed;
        this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
        this.SwitchGroupTitles.Titles = "EachByDate|EachByWeek";
        this.RepeatByDateText.Visibility = Visibility.Visible;
        this.RepeatModeComboBox.Visibility = Visibility.Collapsed;
      }
      else
      {
        this.RepeatByDateText.Visibility = Visibility.Collapsed;
        this.RepeatModeComboBox.Visibility = Visibility.Visible;
      }
      this.MonthDaySelector.SelectedDays = new List<int>()
      {
        DateTime.Today.Day
      };
      this.MonthDaySelector.NotifySelectedChanged();
      this.WeekDaySelector.SelectedDays = new List<DayOfWeek>()
      {
        DateTime.Today.DayOfWeek
      };
      this.WeekDaySelector.NotifySelectedChanged();
      this._showedDays.Clear();
      this._customDateList = this.GetCustomRepeatDateList();
      this.WorkdayCombox.SetSelected((object) CustomRepeatDialog.FirstWorkday);
      this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.SetSelected(this._monthByWeekNumItems[0].Value);
      this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.SetSelected(this._monthByWeekDayItems[0].Value);
      this.SkipHolidayCheckbox.IsChecked = new bool?(false);
      this.SkipWeekendCheckbox.IsChecked = new bool?(false);
      this.InitView();
    }

    public event CustomRepeatDialog.CustomRepeatSetDelegate OnCustomRepeatSet;

    private void InitView()
    {
      this.CustomDaySelector.Visibility = Visibility.Collapsed;
      this.SkipHolidayGrid.Visibility = Visibility.Visible;
      this.SkipWeekendGrid.Visibility = Visibility.Visible;
      this.CustomChooseRepeatUnitGrid.Visibility = Visibility.Visible;
      this.ChooseRepeatUnitComboBox.Visibility = Visibility.Visible;
      if (!string.IsNullOrEmpty(this._repeatFlag))
      {
        if (this._repeatFlag.Contains("HOLIDAY"))
          this.SkipHolidayCheckbox.IsChecked = new bool?(true);
        if (this._repeatFlag.Contains("WEEKEND"))
          this.SkipWeekendCheckbox.IsChecked = new bool?(true);
      }
      if (this._pattern != null)
      {
        if (this._pattern.Interval > 0)
          this.CustomChooseRepeatUnitTimePopupTextBox.Text = this._pattern.Interval.ToString();
        switch (this._pattern.Frequency)
        {
          case FrequencyType.Daily:
            this.SetRepeatUnitView((object) CustomRepeatDialog.Day);
            this.ChooseRepeatUnitComboBox.SetSelected((object) CustomRepeatDialog.Day);
            break;
          case FrequencyType.Weekly:
            this.SetRepeatUnitView((object) CustomRepeatDialog.Week);
            this.ChooseRepeatUnitComboBox.SetSelected((object) CustomRepeatDialog.Week);
            if (this._pattern.ByDay != null && this._pattern.ByDay.Count > 0)
            {
              this.WeekDaySelector.SelectedDays.Clear();
              foreach (WeekDay weekDay in this._pattern.ByDay)
                this.WeekDaySelector.SelectedDays.Add(weekDay.DayOfWeek);
              this.WeekDaySelector.NotifySelectedChanged();
              break;
            }
            break;
          case FrequencyType.Monthly:
            this.SetRepeatUnitView((object) CustomRepeatDialog.Month);
            this.ChooseRepeatUnitComboBox.SetSelected((object) CustomRepeatDialog.Month);
            if (this._pattern.ByMonthDay != null && this._pattern.ByMonthDay.Count > 0)
            {
              this.SwitchGroupTitles.SetSelectedIndex(0);
              this.OnSelectDayClick((object) null, (MouseButtonEventArgs) null);
              this.MonthDaySelector.Visibility = Visibility.Visible;
              this.MonthDaySelector.SelectedDays.Clear();
              foreach (int num in this._pattern.ByMonthDay)
                this.MonthDaySelector.SelectedDays.Add(num);
              this.MonthDaySelector.NotifySelectedChanged();
              break;
            }
            if (this._pattern.ByDay != null && this._pattern.ByDay.Count == 1)
            {
              this.SwitchGroupTitles.SetSelectedIndex(1);
              this.OnSelectWeekClick((object) null, (MouseButtonEventArgs) null);
              int offset = this._pattern.ByDay[0].Offset;
              int index = offset <= 0 ? 5 : offset - 1;
              this.MonthDaySelector.Visibility = Visibility.Collapsed;
              this.CustomChooseRepeatUnitMonthByWeekPopupGridGrid.Visibility = Visibility.Visible;
              this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.SetSelected(this._monthByWeekNumItems[index].Value);
              this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.SetSelected(this._monthByWeekDayItems[(int) this._pattern.ByDay[0].DayOfWeek].Value);
              break;
            }
            break;
          case FrequencyType.Yearly:
            this.SetRepeatUnitView((object) CustomRepeatDialog.Year);
            this.ChooseRepeatUnitComboBox.SetSelected((object) CustomRepeatDialog.Year);
            List<int> byMonth = this._pattern.ByMonth;
            // ISSUE: explicit non-virtual call
            this.YearMonthCombobox.SetSelected((object) Math.Max(1, Math.Min(12, (byMonth != null ? (__nonvirtual (byMonth.Count) > 0 ? 1 : 0) : 0) != 0 ? this._pattern.ByMonth.FirstOrDefault<int>() : 1)));
            List<WeekDay> byDay1 = this._pattern.ByDay;
            // ISSUE: explicit non-virtual call
            int num1 = (byDay1 != null ? (__nonvirtual (byDay1.Count) > 0 ? 1 : 0) : 0) != 0 ? this._pattern.ByDay[0].Offset : 1;
            this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.SetSelected(this._monthByWeekNumItems[num1 <= 0 ? 5 : num1 - 1].Value);
            CustomComboBox popupGridComboBox = this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox;
            ObservableCollection<ComboBoxViewModel> monthByWeekDayItems = this._monthByWeekDayItems;
            List<WeekDay> byDay2 = this._pattern.ByDay;
            // ISSUE: explicit non-virtual call
            int dayOfWeek = (byDay2 != null ? (__nonvirtual (byDay2.Count) > 0 ? 1 : 0) : 0) != 0 ? (int) this._pattern.ByDay[0].DayOfWeek : 0;
            object selected = monthByWeekDayItems[dayOfWeek].Value;
            popupGridComboBox.SetSelected(selected);
            break;
          default:
            this.SetRepeatUnitView((object) CustomRepeatDialog.Week);
            this.ChooseRepeatUnitComboBox.SetSelected((object) CustomRepeatDialog.Week);
            this.OnSelectWeekClick((object) null, (MouseButtonEventArgs) null);
            break;
        }
      }
      if (RepeatUtils.IsMonthlyWorkday(this._repeatFlag))
      {
        this.SwitchGroupTitles.SetSelectedIndex(2);
        this.WorkdayCombox.SetSelected(this._repeatFlag.Contains("TT_WORKDAY=-1") ? (object) CustomRepeatDialog.LastWorkday : (object) CustomRepeatDialog.FirstWorkday);
        this.WorkdayGrid.Visibility = Visibility.Visible;
        this.MonthDaySelector.Visibility = Visibility.Collapsed;
        this.CustomChooseRepeatUnitMonthByWeekPopupGridGrid.Visibility = Visibility.Collapsed;
        this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
        this.SkipHolidayGrid.Visibility = Visibility.Collapsed;
      }
      switch (this._repeatFrom)
      {
        case RepeatFromType.Duedate:
          this.CustomChooseRepeatUnitPopupGrid.Visibility = Visibility.Visible;
          this.RepeatModeComboBox.SetSelected((object) CustomRepeatDialog.RepeatByDue);
          break;
        case RepeatFromType.CompleteTime:
          this.CustomChooseRepeatUnitPopupGrid.Visibility = Visibility.Collapsed;
          this.RepeatModeComboBox.SetSelected((object) CustomRepeatDialog.RepeatByComplete);
          break;
        case RepeatFromType.Custom:
          this.CustomChooseRepeatUnitPopupGrid.Visibility = Visibility.Collapsed;
          this.ChooseRepeatUnitComboBox.Visibility = Visibility.Collapsed;
          this.CustomChooseRepeatUnitGrid.Visibility = Visibility.Collapsed;
          this.SkipHolidayGrid.Visibility = Visibility.Collapsed;
          this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
          this.CustomDaySelector.Visibility = Visibility.Visible;
          this.InitCustomData();
          this.RepeatModeComboBox.SetSelected((object) CustomRepeatDialog.RepeatByCustom);
          break;
      }
      if (!this._calendarMode)
        return;
      this.SkipHolidayGrid.Visibility = Visibility.Collapsed;
      this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
    }

    private void UserControlLoaded(object sender, RoutedEventArgs e)
    {
      this.InitView();
      this.CustomDaySelector.StartDateChanged += (EventHandler<DateTime>) ((picker, dateTime) => this.UpdateCustomData());
    }

    private void InitCustomData()
    {
      if (string.IsNullOrEmpty(this._flagCustom))
        this.CustomDaySelector.SetSelectedDays(new List<DateTime>());
      TickDatePicker customDaySelector = this.CustomDaySelector;
      DateTime? selectedDate = new DateTime?();
      DateTime? selectStart = new DateTime?();
      DateTime? nullable = new DateTime?(this._startTime);
      DateTime? selectEnd = new DateTime?();
      DateTime? maxDate = new DateTime?();
      DateTime? minDate = nullable;
      customDaySelector.SetData(selectedDate, selectStart, selectEnd, maxDate: maxDate, minDate: minDate);
      this.UpdateCustomData();
      this._customDateList = this.GetCustomRepeatDateList();
    }

    private List<DateTime> GetCustomRepeatDateList()
    {
      List<DateTime> customRepeatDateList = new List<DateTime>();
      if (!string.IsNullOrEmpty(this._flagCustom))
      {
        string flagCustom = this._flagCustom;
        char[] chArray1 = new char[1]{ ';' };
        foreach (string str1 in flagCustom.Split(chArray1))
        {
          if (str1.StartsWith("BYDATE="))
          {
            string str2 = str1.Substring(7);
            char[] chArray2 = new char[1]{ ',' };
            foreach (string s in str2.Split(chArray2))
              customRepeatDateList.Add(DateTime.ParseExact(s, "yyyyMMdd", (IFormatProvider) App.Ci));
            return customRepeatDateList;
          }
        }
      }
      return customRepeatDateList;
    }

    private void UpdateCustomData()
    {
      List<DateTime> selectedDays = this.CustomDaySelector.GetSelectedDays();
      foreach (DateTime customDate in this._customDateList)
      {
        if (!this._showedDays.ContainsKey(customDate.Date))
          selectedDays.Add(customDate.Date);
        this._showedDays[customDate] = true;
      }
      this.CustomDaySelector.SetSelectedDays(selectedDays);
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
      this._popup.IsOpen = false;
      RecurrencePattern pattern = new RecurrencePattern()
      {
        Interval = int.Parse(this.CustomChooseRepeatUnitTimePopupTextBox.Text)
      };
      if (this._repeatFrom == RepeatFromType.Custom)
      {
        List<DateTime> selectedDays = this.CustomDaySelector.GetSelectedDays();
        if (!selectedDays.Any<DateTime>())
        {
          CustomRepeatDialog.CustomRepeatSetDelegate onCustomRepeatSet = this.OnCustomRepeatSet;
          if (onCustomRepeatSet == null)
            return;
          onCustomRepeatSet("2", string.Empty);
        }
        else
        {
          StringBuilder stringBuilder = new StringBuilder();
          stringBuilder.Append("ERULE:NAME=CUSTOM;BYDATE=");
          selectedDays.Sort();
          foreach (DateTime dateTime in selectedDays)
          {
            stringBuilder.Append(dateTime.ToString("yyyyMMdd"));
            stringBuilder.Append(",");
          }
          stringBuilder.Remove(stringBuilder.Length - 1, 1);
          CustomRepeatDialog.CustomRepeatSetDelegate onCustomRepeatSet = this.OnCustomRepeatSet;
          if (onCustomRepeatSet == null)
            return;
          onCustomRepeatSet(this._realRepeatFrom, stringBuilder.ToString());
        }
      }
      else
      {
        if (this.ChooseRepeatUnitComboBox.SelectedItem.Value.Equals((object) CustomRepeatDialog.Week))
        {
          pattern.Frequency = FrequencyType.Weekly;
          if (this.WeekDaySelector.SelectedDays.Count != 0)
          {
            List<WeekDay> weekDayList = new List<WeekDay>();
            foreach (DayOfWeek selectedDay in this.WeekDaySelector.SelectedDays)
              weekDayList.Add(new WeekDay(selectedDay));
            pattern.ByDay = weekDayList;
          }
        }
        else if (this.ChooseRepeatUnitComboBox.SelectedItem.Value.Equals((object) CustomRepeatDialog.Day))
          pattern.Frequency = FrequencyType.Daily;
        else if (this.ChooseRepeatUnitComboBox.SelectedItem.Value.Equals((object) CustomRepeatDialog.Month))
        {
          pattern.Frequency = FrequencyType.Monthly;
          if (this.MonthDaySelector.Visibility == Visibility.Visible)
          {
            if (this.MonthDaySelector.SelectedDays.Count != 0)
            {
              List<int> intList = new List<int>((IEnumerable<int>) this.MonthDaySelector.SelectedDays);
              pattern.ByMonthDay = intList;
            }
          }
          else if (this.CustomChooseRepeatUnitMonthByWeekPopupGridGrid.Visibility == Visibility.Visible)
          {
            List<WeekDay> weekDayList = this.GetWeekDayList();
            pattern.ByDay = weekDayList;
          }
          else if (this.WorkdayGrid.Visibility == Visibility.Visible)
          {
            string repeatFlag = "RRULE:FREQ=MONTHLY;INTERVAL=" + int.Parse(this.CustomChooseRepeatUnitTimePopupTextBox.Text).ToString() + ";BYMONTHDAY=1;TT_WORKDAY=1";
            if (this.WorkdayCombox.SelectedItem.Value.Equals((object) CustomRepeatDialog.LastWorkday))
              repeatFlag = "RRULE:FREQ=MONTHLY;INTERVAL=" + int.Parse(this.CustomChooseRepeatUnitTimePopupTextBox.Text).ToString() + ";BYMONTHDAY=1;TT_WORKDAY=-1";
            CustomRepeatDialog.CustomRepeatSetDelegate onCustomRepeatSet = this.OnCustomRepeatSet;
            if (onCustomRepeatSet == null)
              return;
            onCustomRepeatSet(this._realRepeatFrom, repeatFlag);
            return;
          }
        }
        else if (this.ChooseRepeatUnitComboBox.SelectedItem.Value.Equals((object) CustomRepeatDialog.Year))
        {
          pattern.Frequency = FrequencyType.Yearly;
          int num = this.YearMonthCombobox.SelectedIndex + 1;
          pattern.ByMonth = new List<int>() { num };
          List<WeekDay> weekDayList = this.GetWeekDayList();
          pattern.ByDay = weekDayList;
        }
        CustomRepeatDialog.CustomRepeatSetDelegate onCustomRepeatSet1 = this.OnCustomRepeatSet;
        if (onCustomRepeatSet1 == null)
          return;
        onCustomRepeatSet1(this._realRepeatFrom, this.GetRepeatFlag(pattern));
      }
    }

    private List<WeekDay> GetWeekDayList()
    {
      List<WeekDay> weekDayList = new List<WeekDay>();
      int num = 0;
      switch (this._monthByWeekNumItems.IndexOf(this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.SelectedItem))
      {
        case 0:
          num = 1;
          break;
        case 1:
          num = 2;
          break;
        case 2:
          num = 3;
          break;
        case 3:
          num = 4;
          break;
        case 4:
          num = 5;
          break;
        case 5:
          num = -1;
          break;
      }
      switch (this._monthByWeekDayItems.IndexOf(this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.SelectedItem))
      {
        case 0:
          weekDayList.Add(new WeekDay(DayOfWeek.Sunday, num));
          break;
        case 1:
          weekDayList.Add(new WeekDay(DayOfWeek.Monday, num));
          break;
        case 2:
          weekDayList.Add(new WeekDay(DayOfWeek.Tuesday, num));
          break;
        case 3:
          weekDayList.Add(new WeekDay(DayOfWeek.Wednesday, num));
          break;
        case 4:
          weekDayList.Add(new WeekDay(DayOfWeek.Thursday, num));
          break;
        case 5:
          weekDayList.Add(new WeekDay(DayOfWeek.Friday, num));
          break;
        case 6:
          weekDayList.Add(new WeekDay(DayOfWeek.Saturday, num));
          break;
      }
      return weekDayList;
    }

    private string GetRepeatFlag(RecurrencePattern pattern)
    {
      string str1 = pattern.ToString().Replace("İ", "I");
      int num = this.SkipHolidayGrid.Visibility != Visibility.Visible || !this.SkipHolidayCheckbox.IsChecked.HasValue ? 0 : (this.SkipHolidayCheckbox.IsChecked.Value ? 1 : 0);
      bool flag = this.SkipWeekendGrid.Visibility == Visibility.Visible && this.SkipWeekendCheckbox.IsChecked.HasValue && this.SkipWeekendCheckbox.IsChecked.Value;
      string str2 = string.Empty;
      if (num != 0 && !flag)
        str2 = ";TT_SKIP=HOLIDAY";
      if (num == 0 & flag)
        str2 = ";TT_SKIP=WEEKEND";
      if ((num & (flag ? 1 : 0)) != 0)
        str2 = ";TT_SKIP=HOLIDAY,WEEKEND";
      return "RRULE:" + str1 + str2;
    }

    private void OnSelectWeekClick(object sender, MouseButtonEventArgs e)
    {
      this.MonthDaySelector.Visibility = Visibility.Collapsed;
      this.CustomChooseRepeatUnitMonthByWeekPopupGridGrid.Visibility = Visibility.Visible;
      this.WorkdayGrid.Visibility = Visibility.Collapsed;
      this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
      this.SkipHolidayGrid.Visibility = Visibility.Visible;
      this.SetWeekNum(this._startTime);
      if (!this._calendarMode)
        return;
      this.SkipHolidayGrid.Visibility = Visibility.Collapsed;
      this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
    }

    private void OnSelectDayClick(object sender, MouseButtonEventArgs e)
    {
      this.MonthDaySelector.Visibility = Visibility.Visible;
      this.CustomChooseRepeatUnitMonthByWeekPopupGridGrid.Visibility = Visibility.Collapsed;
      this.WorkdayGrid.Visibility = Visibility.Collapsed;
      this.SkipWeekendGrid.Visibility = Visibility.Visible;
      this.SkipHolidayGrid.Visibility = Visibility.Visible;
      if (!this._calendarMode)
        return;
      this.SkipHolidayGrid.Visibility = Visibility.Collapsed;
      this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
    }

    private void OnSelectWorkdayClick(object sender, MouseButtonEventArgs e)
    {
      this.WorkdayGrid.Visibility = Visibility.Visible;
      this.MonthDaySelector.Visibility = Visibility.Collapsed;
      this.CustomChooseRepeatUnitMonthByWeekPopupGridGrid.Visibility = Visibility.Collapsed;
      this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
      this.SkipHolidayGrid.Visibility = Visibility.Collapsed;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this._popup.IsOpen = false;

    private void RepeatUnitComboBoxSelected(object sender, ComboBoxViewModel e)
    {
      if (e == null || this.WeekDaySelector == null)
        return;
      this.SetRepeatUnitView(e.Value);
    }

    private void SetRepeatUnitView(object val)
    {
      if (val.Equals((object) CustomRepeatDialog.Day))
      {
        this.WeekDaySelector.Visibility = Visibility.Collapsed;
        this.CustomChooseRepeatUnitMonthPopupGrid.Visibility = Visibility.Collapsed;
        this.SkipWeekendGrid.Visibility = Visibility.Visible;
        this.SkipHolidayGrid.Visibility = Visibility.Visible;
      }
      else if (val.Equals((object) CustomRepeatDialog.Week))
      {
        this.WeekDaySelector.Visibility = Visibility.Visible;
        this.CustomChooseRepeatUnitMonthPopupGrid.Visibility = Visibility.Collapsed;
        this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
        this.SkipHolidayGrid.Visibility = Visibility.Visible;
      }
      else if (val.Equals((object) CustomRepeatDialog.Month))
      {
        this.WeekDaySelector.Visibility = Visibility.Collapsed;
        this.CustomChooseRepeatUnitMonthPopupGrid.Visibility = Visibility.Visible;
        this.SkipWeekendGrid.Visibility = this.MonthDaySelector.Visibility;
        this.CustomChooseRepeatUnitMonthByWeekPopupGridGrid.Visibility = Visibility.Collapsed;
        this.SkipHolidayGrid.Visibility = this.WorkdayGrid.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        this.YearMonthGrid.Visibility = Visibility.Collapsed;
        switch (this.SwitchGroupTitles.GetSelectedIndex())
        {
          case 1:
            this.OnSelectWeekClick((object) null, (MouseButtonEventArgs) null);
            break;
          case 2:
            this.OnSelectWorkdayClick((object) null, (MouseButtonEventArgs) null);
            break;
          default:
            this.OnSelectDayClick((object) null, (MouseButtonEventArgs) null);
            break;
        }
      }
      else if (val.Equals((object) CustomRepeatDialog.Year))
      {
        this.TryInitYearMonthCombobox();
        this.WeekDaySelector.Visibility = Visibility.Collapsed;
        this.CustomChooseRepeatUnitMonthPopupGrid.Visibility = Visibility.Visible;
        this.SwitchGroupTitles.Visibility = Visibility.Collapsed;
        this.MonthDaySelector.Visibility = Visibility.Collapsed;
        this.WorkdayGrid.Visibility = Visibility.Collapsed;
        this.CustomChooseRepeatUnitMonthByWeekPopupGridGrid.Visibility = Visibility.Visible;
        this.YearMonthGrid.Visibility = Visibility.Visible;
        this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
        this.SkipHolidayGrid.Visibility = Visibility.Visible;
        this.SetWeekNum(this._startTime);
        this.YearMonthCombobox.SetSelected((object) this._startTime.Month);
      }
      if (!this._calendarMode)
        return;
      this.SkipHolidayGrid.Visibility = Visibility.Collapsed;
      this.SkipWeekendGrid.Visibility = Visibility.Collapsed;
    }

    private void SetWeekNum(DateTime date)
    {
      int num = 1;
      while (date.AddDays((double) (num * -7)).Month == date.Month)
        ++num;
      this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.SetSelected((object) (num - 1));
      this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.SetSelected(this._monthByWeekDayItems[(int) date.DayOfWeek].Value);
    }

    private void customChooseRepeatUnitTimePopupTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
      if (this.CustomChooseRepeatUnitTimePopupTextBox == null)
        return;
      try
      {
        if (int.Parse(this.CustomChooseRepeatUnitTimePopupTextBox.Text) != 0)
          return;
        this.CustomChooseRepeatUnitTimePopupTextBox.Text = "1";
      }
      catch (FormatException ex)
      {
        this.CustomChooseRepeatUnitTimePopupTextBox.Text = "1";
      }
    }

    private void RepeatModeSelected(object sender, ComboBoxViewModel model)
    {
      if (this.RepeatModeComboBox.SelectedItem == null || this.CustomChooseRepeatUnitPopupGrid == null)
        return;
      if (model.Value.ToString() == Utils.GetString("RepeatByDue"))
        this._repeatFrom = RepeatFromType.Duedate;
      else if (model.Value.ToString() == Utils.GetString("RepeatByComplete"))
        this._repeatFrom = RepeatFromType.CompleteTime;
      else if (model.Value.ToString() == Utils.GetString("RepeatByCustom"))
        this._repeatFrom = RepeatFromType.Custom;
      this.InitView();
    }

    private void OnRepeatIntervalTextChanged(object sender, TextChangedEventArgs e)
    {
      int result;
      if (!int.TryParse(this.CustomChooseRepeatUnitTimePopupTextBox.Text, out result))
      {
        this.CustomChooseRepeatUnitTimePopupTextBox.Text = "1";
        this.CustomChooseRepeatUnitTimePopupTextBox.SelectAll();
      }
      else
      {
        if (result <= 365)
          return;
        this.CustomChooseRepeatUnitTimePopupTextBox.Text = "365";
        this.CustomChooseRepeatUnitTimePopupTextBox.SelectAll();
      }
    }

    private void OnGroupTitleSelectedTitleChanged(object sender, GroupTitleViewModel e)
    {
      if (e.Index == 1)
        this.OnSelectWeekClick((object) null, (MouseButtonEventArgs) null);
      else if (e.Index == 2)
        this.OnSelectWorkdayClick((object) null, (MouseButtonEventArgs) null);
      else
        this.OnSelectDayClick((object) null, (MouseButtonEventArgs) null);
    }

    public bool HandleTab(bool shift)
    {
      this._tabIndex += 14 + (shift ? -1 : 1);
      this._tabIndex %= 14;
      this.HandleTabIndex(shift);
      return true;
    }

    public bool HandleEnter()
    {
      if (this.RepeatModeComboBox.TabSelected)
        this.RepeatModeComboBox.HandleEnter();
      if (this.ChooseRepeatUnitComboBox.TabSelected)
        this.ChooseRepeatUnitComboBox.HandleEnter();
      if (this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.TabSelected)
        this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.HandleEnter();
      if (this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.TabSelected)
        this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.HandleEnter();
      if (this.CustomDaySelector.Visibility == Visibility.Visible && this._tabIndex == 1)
        this.CustomDaySelector.SelectTabItem();
      if (this.WorkdayCombox.TabSelected)
        this.WorkdayCombox.HandleEnter();
      if (this.SwitchGroupTitles.InTab())
        this.SwitchGroupTitles.SelectTabItem();
      this.WeekDaySelector.HandleEnter();
      this.MonthDaySelector.HandleEnter();
      switch (this._tabIndex)
      {
        case 10:
          this.SkipHolidayCheckbox.IsChecked = new bool?(!this.SkipHolidayCheckbox.IsChecked.GetValueOrDefault());
          break;
        case 11:
          this.SkipWeekendCheckbox.IsChecked = new bool?(!this.SkipWeekendCheckbox.IsChecked.GetValueOrDefault());
          break;
        case 12:
          this.OnSaveClick((object) this, (RoutedEventArgs) null);
          break;
        case 13:
          this._popup.IsOpen = false;
          break;
      }
      return true;
    }

    public bool HandleEsc()
    {
      if (this.RepeatModeComboBox.IsOpen)
      {
        this.RepeatModeComboBox.Close();
        return true;
      }
      if (this.ChooseRepeatUnitComboBox.IsOpen)
      {
        this.ChooseRepeatUnitComboBox.Close();
        return true;
      }
      if (this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.IsOpen)
      {
        this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.Close();
        return true;
      }
      if (this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.IsOpen)
      {
        this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.Close();
        return true;
      }
      if (!this.WorkdayCombox.IsOpen)
        return false;
      this.WorkdayCombox.Close();
      return true;
    }

    public bool UpDownSelect(bool isUp)
    {
      if (this.RepeatModeComboBox.IsOpen)
        this.RepeatModeComboBox.UpDownSelect(isUp);
      if (this.ChooseRepeatUnitComboBox.IsOpen)
        this.ChooseRepeatUnitComboBox.UpDownSelect(isUp);
      if (this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.IsOpen)
        this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.UpDownSelect(isUp);
      if (this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.IsOpen)
        this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.UpDownSelect(isUp);
      if (this.WorkdayCombox.IsOpen)
        this.WorkdayCombox.UpDownSelect(isUp);
      if (this.ChooseRepeatUnitComboBox.SelectedItem.Value.Equals((object) CustomRepeatDialog.Month) && this._tabIndex == 6 && this.MonthDaySelector.Visibility == Visibility.Visible)
        this.MonthDaySelector.SetTabSelected(true, new int?(isUp ? -7 : 7));
      if (this.CustomDaySelector.Visibility == Visibility.Visible && this._tabIndex == 1)
        this.CustomDaySelector.MoveTabSelectDate(isUp ? -7 : 7);
      return true;
    }

    public bool LeftRightSelect(bool isLeft)
    {
      if (this.ChooseRepeatUnitComboBox.SelectedItem.Value.Equals((object) CustomRepeatDialog.Month) && this._tabIndex == 6 && this.MonthDaySelector.Visibility == Visibility.Visible)
      {
        this.MonthDaySelector.SetTabSelected(true, new int?(isLeft ? -1 : 1));
        return true;
      }
      if (this.CustomDaySelector.Visibility != Visibility.Visible || this._tabIndex != 1)
        return false;
      this.CustomDaySelector.MoveTabSelectDate(isLeft ? -1 : 1);
      return true;
    }

    private void HandleTabIndex(bool shift)
    {
      this.RepeatModeComboBox.TabSelected = this._tabIndex == 0;
      if (this.ChooseRepeatUnitComboBox.Visibility == Visibility.Visible)
      {
        if (this._tabIndex == 1)
          this.CustomChooseRepeatUnitTimePopupTextBox.Focus();
        else
          this.EmptyBox.Focus();
        this.ChooseRepeatUnitComboBox.TabSelected = this._tabIndex == 2;
        if (this.ChooseRepeatUnitComboBox.SelectedItem.Value.Equals((object) CustomRepeatDialog.Day))
          this.SetDayTabStatus(shift);
        else if (this.ChooseRepeatUnitComboBox.SelectedItem.Value.Equals((object) CustomRepeatDialog.Week))
        {
          this.SetWeekTabStatus(shift);
        }
        else
        {
          if (!this.ChooseRepeatUnitComboBox.SelectedItem.Value.Equals((object) CustomRepeatDialog.Month))
            return;
          this.SetMonthTabStatus(shift);
        }
      }
      else
      {
        if (this._tabIndex >= 2 && this._tabIndex <= 11)
          this._tabIndex = shift ? 1 : 12;
        if (this._tabIndex == 1)
          this.CustomDaySelector.TabSelectCurrent();
        else
          this.CustomDaySelector.ClearTabSelected();
        UiUtils.SetSaveButtonTabSelected(this.SaveButton, this._tabIndex == 12);
        UiUtils.SetCancelButtonTabSelected(this.CancelButton, this._tabIndex == 13);
      }
    }

    private void SetMonthTabStatus(bool shift)
    {
      if (this.CustomChooseRepeatUnitMonthPopupGrid.IsVisible)
      {
        this.SwitchGroupTitles.TabSelect(this._tabIndex - 3);
        if (this.MonthDaySelector.Visibility == Visibility.Visible)
        {
          this.MonthDaySelector.SetTabSelected(this._tabIndex == 6);
          if (this._tabIndex >= 7 && this._tabIndex <= 9)
          {
            this._tabIndex = shift ? 6 : 10;
            this.HandleTabIndex(shift);
          }
          else
            this.SetBottomTabStatus(shift, 6);
        }
        else if (this.WorkdayGrid.Visibility == Visibility.Visible)
        {
          this.WorkdayCombox.TabSelected = this._tabIndex == 6;
          if (this._tabIndex >= 7 && this._tabIndex <= 9)
          {
            this._tabIndex = shift ? 6 : 10;
            this.HandleTabIndex(shift);
          }
          else
            this.SetBottomTabStatus(shift, 6);
        }
        else if (this.CustomChooseRepeatUnitMonthByWeekPopupGridGrid.Visibility == Visibility.Visible)
        {
          this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox.TabSelected = this._tabIndex == 6;
          this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox.TabSelected = this._tabIndex == 7;
          if (this._tabIndex >= 8 && this._tabIndex <= 9)
          {
            this._tabIndex = shift ? 7 : 10;
            this.HandleTabIndex(shift);
          }
          else
            this.SetBottomTabStatus(shift, 7);
        }
        else
          this.SetDayTabStatus(shift);
      }
      else
        this.SetDayTabStatus(shift);
    }

    private void SetWeekTabStatus(bool shift)
    {
      if (this.WeekDaySelector.IsVisible)
        this.WeekDaySelector.SetTabIndex(this._tabIndex - 3);
      else if (this._tabIndex >= 3 && this._tabIndex <= 9)
      {
        this._tabIndex = shift ? 2 : 10;
        this.HandleTabIndex(shift);
        return;
      }
      this.SetBottomTabStatus(shift, this.WeekDaySelector.IsVisible ? 9 : 2);
    }

    private void SetDayTabStatus(bool shift)
    {
      if (this._tabIndex >= 3 && this._tabIndex <= 9)
      {
        this._tabIndex = shift ? 2 : 10;
        this.HandleTabIndex(shift);
      }
      else
        this.SetBottomTabStatus(shift, 2);
    }

    private void SetBottomTabStatus(bool shift, int start)
    {
      if (this._tabIndex == 10 && this.SkipHolidayGrid.Visibility != Visibility.Visible)
      {
        this._tabIndex = shift ? start : 11;
        this.HandleTabIndex(shift);
      }
      else if (this._tabIndex == 11 && this.SkipWeekendGrid.Visibility != Visibility.Visible)
      {
        this._tabIndex = shift ? 10 : 12;
        this.HandleTabIndex(shift);
      }
      else
      {
        UiUtils.SetBorderTabSelected(this.SkipHolidayGrid, this._tabIndex == 10);
        UiUtils.SetBorderTabSelected(this.SkipWeekendGrid, this._tabIndex == 11);
        UiUtils.SetSaveButtonTabSelected(this.SaveButton, this._tabIndex == 12);
        UiUtils.SetCancelButtonTabSelected(this.CancelButton, this._tabIndex == 13);
      }
    }

    public void SetTabEnter(bool enter)
    {
      if (enter)
      {
        this._tabIndex = 0;
        this.RepeatModeComboBox.TabSelected = true;
      }
      else
        this._tabIndex = -1;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/customrepeatdialog.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.UserControlLoaded);
          break;
        case 2:
          this.RepeatModeComboBox = (CustomComboBox) target;
          break;
        case 3:
          this.RepeatByDateText = (TextBlock) target;
          break;
        case 4:
          this.CustomChooseRepeatUnitGrid = (Grid) target;
          break;
        case 5:
          this.CustomChooseRepeatUnitTimePopupTextBox = (TextBox) target;
          this.CustomChooseRepeatUnitTimePopupTextBox.TextChanged += new TextChangedEventHandler(this.OnRepeatIntervalTextChanged);
          this.CustomChooseRepeatUnitTimePopupTextBox.LostFocus += new RoutedEventHandler(this.customChooseRepeatUnitTimePopupTextBox_LostFocus);
          break;
        case 6:
          this.EmptyBox = (TextBox) target;
          break;
        case 7:
          this.ChooseRepeatUnitComboBox = (CustomComboBox) target;
          break;
        case 8:
          this.CustomChooseRepeatUnitPopupGrid = (Grid) target;
          break;
        case 9:
          this.WeekDaySelector = (WeekdaySelector) target;
          break;
        case 10:
          this.CustomChooseRepeatUnitMonthPopupGrid = (Grid) target;
          break;
        case 11:
          this.SwitchGroupTitles = (GroupTitle2) target;
          break;
        case 12:
          this.MonthDaySelector = (MonthDaySelector) target;
          break;
        case 13:
          this.WorkdayGrid = (Grid) target;
          break;
        case 14:
          this.WorkdayCombox = (CustomComboBox) target;
          break;
        case 15:
          this.YearMonthGrid = (Border) target;
          break;
        case 16:
          this.YearMonthCombobox = (CustomComboBox) target;
          break;
        case 17:
          this.CustomChooseRepeatUnitMonthByWeekPopupGridGrid = (Grid) target;
          break;
        case 18:
          this.CustomChooseRepeatUnitMonthByWeekNumPopupGridComboBox = (CustomComboBox) target;
          break;
        case 19:
          this.CustomChooseRepeatUnitMonthByWeekDayPopupGridComboBox = (CustomComboBox) target;
          break;
        case 20:
          this.CustomDaySelector = (TickDatePicker) target;
          break;
        case 21:
          this.SkipHolidayGrid = (Border) target;
          break;
        case 22:
          this.SkipHolidayCheckbox = (CheckBox) target;
          break;
        case 23:
          this.SkipWeekendGrid = (Border) target;
          break;
        case 24:
          this.SkipWeekendCheckbox = (CheckBox) target;
          break;
        case 25:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 26:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    public delegate void CustomRepeatSetDelegate(string repeatFrom, string repeatFlag);
  }
}
