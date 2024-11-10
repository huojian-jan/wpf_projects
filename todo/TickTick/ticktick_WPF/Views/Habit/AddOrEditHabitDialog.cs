// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.AddOrEditHabitDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using Ical.Net;
using Ical.Net.DataTypes;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MarkDown;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Time;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class AddOrEditHabitDialog : Window, IOkCancelWindow, IComponentConnector
  {
    private HabitModel _habit;
    private bool _isNew;
    private DateTime _popupClosedTime;
    private string _beforeSection;
    private bool _isTargetDayChanged;
    private string _beforeTime;
    private List<HabitSectionModel> _allSection;
    internal Grid IconGrid;
    internal Image IconImage;
    internal EscPopup SetIconPopup;
    internal SetHabitIconControl SetIconControl;
    internal TextBox HabitTitleBox;
    internal Border FrequencySelectBorder;
    internal TextBlock FreqText;
    internal EscPopup SetFrequencyPopup;
    internal SetHabitFrequencyControl SetFrequencyControl;
    internal Border GoalBorder;
    internal TextBlock GoalText;
    internal EscPopup SetGoalPopup;
    internal SetHabitGoalControl SetGoalControl;
    internal Border TargetDayBorder;
    internal TextBlock TargetDayTextBlock;
    internal EscPopup TargetDaySelectPopup;
    internal StackPanel TargetDayList;
    internal OptionCheckBox TargetDayCustomRadioButton;
    internal EscPopup TargetDayCustomPopup;
    internal TextBox TargetDayCustomInput;
    internal TextBlock TargetDayCustomInputDay;
    internal Border TargetStartDateBorder;
    internal TextBlock TargetStartDateTextBlock;
    internal EscPopup TargetStartDatePopup;
    internal TickDatePicker TargetStartDatePicker;
    internal Border SectionBorder;
    internal EmjTextBlock SectionNameTextBlock;
    internal EscPopup SectionPopup;
    internal ListView SectionList;
    internal EscPopup AddSectionPopup;
    internal EmojiEditor AddSectionInput;
    internal TextBlock AddSectionError;
    internal SetHabitReminderControl SetReminderControl;
    internal CheckBox RecordCheckBox;
    internal Button SaveButton;
    internal Grid ToastGrid;
    internal TextBlock ToastTextBlock;
    private bool _contentLoaded;

    public AddOrEditHabitDialog(string sectionId = null)
    {
      this.InitializeComponent();
      this.Title = Utils.GetString("AddHabit");
      this._habit = HabitModel.GetDefaultHabit();
      this._habit.SectionId = sectionId;
      this._isNew = true;
    }

    public AddOrEditHabitDialog(HabitModel habit)
    {
      this.InitializeComponent();
      this.Title = Utils.GetString("EditHabit");
      this._habit = habit;
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      AddOrEditHabitDialog orEditHabitDialog = this;
      if (orEditHabitDialog._habit.RepeatRule == null)
        orEditHabitDialog._habit.RepeatRule = "RRULE:FREQ=DAILY;INTERVAL=1";
      orEditHabitDialog._habit.Unit = HabitUtils.GetUnitText(orEditHabitDialog._habit.Unit);
      orEditHabitDialog.HabitTitleBox.Text = orEditHabitDialog._habit.Name;
      orEditHabitDialog.SetFreqText(orEditHabitDialog._habit.RepeatRule);
      orEditHabitDialog.GoalText.Text = orEditHabitDialog._habit.IsBoolHabit() ? Utils.GetString("CheckInTheDay") : string.Format(Utils.GetString("DailyTimes"), (object) orEditHabitDialog._habit.Goal, (object) orEditHabitDialog._habit.Unit);
      orEditHabitDialog.RecordCheckBox.IsChecked = new bool?(orEditHabitDialog._habit.RecordEnable.GetValueOrDefault());
      orEditHabitDialog.SetRecordCheckBoxEvent();
      orEditHabitDialog._popupClosedTime = DateTime.Now;
      orEditHabitDialog.SetReminderControl.Init(orEditHabitDialog._habit.Reminder);
      orEditHabitDialog.SetIcon();
      await Task.Delay(100);
      orEditHabitDialog.Activate();
      orEditHabitDialog.HabitTitleBox.Focus();
      if (orEditHabitDialog._isNew)
        orEditHabitDialog.HabitTitleBox.SelectAll();
      else
        orEditHabitDialog.HabitTitleBox.Select(orEditHabitDialog.HabitTitleBox.Text.Length, 0);
      foreach (object child in orEditHabitDialog.TargetDayList.Children)
      {
        if (child is OptionCheckBox optionCheckBox && optionCheckBox.Tag is string tag)
        {
          int result = -1;
          if (int.TryParse(tag, out result) && result > 0)
            optionCheckBox.Text = string.Format(Utils.GetString("HabitCycleDay"), (object) tag);
        }
      }
      orEditHabitDialog.UpdateSelectedTargetDay();
      orEditHabitDialog.UpdateTargetStartDate();
      orEditHabitDialog.UpdateSections(orEditHabitDialog._isNew);
    }

    private void SetRecordCheckBoxEvent()
    {
      this.RecordCheckBox.Checked += (RoutedEventHandler) ((o, arg) => UserActCollectUtils.AddClickEvent("habit", "add_edit_habit", "show_auto_habit_log"));
      this.RecordCheckBox.Unchecked += (RoutedEventHandler) ((o, arg) => UserActCollectUtils.AddClickEvent("habit", "add_edit_habit", "hide_auto_habit_log"));
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e) => this.UpdateSaveButton();

    private void UpdateSaveButton()
    {
      this.SaveButton.IsEnabled = !string.IsNullOrEmpty(this.HabitTitleBox.Text) || !string.IsNullOrEmpty(this.HabitTitleBox.Tag as string);
    }

    private void OpenFrequencyPopup(object sender, MouseButtonEventArgs e)
    {
      this.SetFrequencyControl.Init(this._habit.RepeatRule);
      this.SetFrequencyPopup.IsOpen = true;
      HwndHelper.SetFocus((Popup) this.SetFrequencyPopup, false);
    }

    private void OnFreqSaved(object sender, string rule)
    {
      this._habit.RepeatRule = rule;
      this.SetFreqText(rule);
    }

    private void SetFreqText(string rule)
    {
      if (rule == null)
        this.FreqText.Text = Utils.GetString("EveryDay");
      else if (rule.Contains("TT_TIMES"))
      {
        Match match = new Regex("TT_TIMES=(\\d{0,})").Match(rule);
        int result;
        if (match.Success && int.TryParse(match.Groups[1].ToString(), out result))
        {
          this.FreqText.Text = string.Format(Utils.GetString("TimesPerWeekText"), (object) result, (object) Utils.GetString(result > 1 ? "PublicDays" : "PublicDay"));
        }
        else
        {
          this.FreqText.Text = string.Format(Utils.GetString("TimesPerWeekText"), (object) 3, (object) Utils.GetString("PublicDays"));
          this._habit.RepeatRule = "RRULE:FREQ=WEEKLY;INTERVAL=1;TT_TIMES=" + 3.ToString();
        }
      }
      else
      {
        RecurrenceModel recurrenceModel = RecurrenceModel.GetRecurrenceModel(rule);
        this.FreqText.Text = recurrenceModel.Frequency == FrequencyType.Daily && recurrenceModel.Interval == 1 || this.IsWeekDaily(recurrenceModel) ? Utils.GetString("EveryDay") : RRuleUtils.RRule2String("2", rule, new DateTime?(), false);
      }
    }

    private bool IsWeekDaily(RecurrenceModel pattern)
    {
      if (pattern.Frequency != FrequencyType.Weekly)
        return false;
      List<WeekDay> byDay = pattern.ByDay;
      List<DayOfWeek> dayOdWeeks = byDay != null ? byDay.Select<WeekDay, DayOfWeek>((Func<WeekDay, DayOfWeek>) (bd => bd.DayOfWeek)).ToList<DayOfWeek>() : (List<DayOfWeek>) null;
      return dayOdWeeks == null || Enum.GetValues(typeof (DayOfWeek)).Cast<DayOfWeek>().All<DayOfWeek>((Func<DayOfWeek, bool>) (value => dayOdWeeks.Contains(value)));
    }

    private void HideFreqPopup(object sender, EventArgs e) => this.SetFrequencyPopup.IsOpen = false;

    private void OpenGoalPopup(object sender, MouseButtonEventArgs e)
    {
      this.SetGoalControl.Init(this._habit);
      this.SetGoalPopup.IsOpen = true;
    }

    private void HideGoalPopup(object sender, EventArgs e) => this.SetGoalPopup.IsOpen = false;

    private void OnGoalSaved(object sender, HabitGoalModel goal)
    {
      if (goal.Type == HabitType.Real.ToString())
      {
        this._habit.Type = goal.Type;
        this._habit.Goal = goal.Goal;
        this._habit.Step = goal.Step;
        this._habit.Unit = goal.Unit;
      }
      else
      {
        this._habit.Type = HabitType.Boolean.ToString();
        this._habit.Step = 1.0;
        this._habit.Goal = 1.0;
        this._habit.Unit = Utils.GetString("Count");
      }
      this.SetGoalPopup.IsOpen = false;
      this.GoalText.Text = this._habit.IsBoolHabit() ? Utils.GetString("CheckInTheDay") : string.Format(Utils.GetString("DailyTimes"), (object) this._habit.Goal, (object) this._habit.Unit);
    }

    private void OnSelecteIconClick(object sender, MouseButtonEventArgs e)
    {
      this.SetIconPopup.IsOpen = true;
      this.SetIconControl.Reset(this._habit.IconRes, this._habit.Color);
      HwndHelper.SetFocus((Popup) this.SetIconPopup, false);
    }

    private void SetIcon()
    {
      this.IconImage.Source = (ImageSource) HabitService.GetIcon(this._habit.IconRes, this._habit.Color);
      this.HabitTitleBox.Tag = (object) HabitUtils.GetHabitHintText(this._habit.IconRes);
    }

    private void OnIconSelected(object sender, EventArgs e)
    {
      if (sender is SetHabitIconControl habitIconControl)
      {
        if (habitIconControl.IsIcon)
        {
          this._habit.IconRes = habitIconControl.SelectedIcon;
          this._habit.Color = habitIconControl.IconColor;
        }
        else
        {
          this._habit.IconRes = "txt_" + habitIconControl.IconText;
          string color = habitIconControl.TextColor ?? "";
          if (this._habit.Color != color)
          {
            this._habit.Color = color;
            ticktick_WPF.Views.Misc.ColorSelector.ColorSelector.TryAddClickEvent(color);
          }
        }
      }
      this.SetIconPopup.IsOpen = false;
      this.SetIcon();
      this.UpdateSaveButton();
    }

    private void HideIconPopup(object sender, EventArgs e)
    {
      this.SetIconPopup.IsOpen = false;
      this.UpdateSaveButton();
    }

    private void OnStoryCompleted(object sender, EventArgs e)
    {
      this.ToastGrid.Visibility = Visibility.Collapsed;
    }

    private void OnSaveClick(object sender, RoutedEventArgs e) => this.Save();

    private async void Save()
    {
      AddOrEditHabitDialog orEditHabitDialog = this;
      orEditHabitDialog._habit.RecordEnable = new bool?(orEditHabitDialog.RecordCheckBox.IsChecked.GetValueOrDefault());
      orEditHabitDialog._habit.Name = !string.IsNullOrEmpty(orEditHabitDialog.HabitTitleBox.Text) ? orEditHabitDialog.HabitTitleBox.Text : orEditHabitDialog.HabitTitleBox.Tag as string;
      if (orEditHabitDialog._isNew)
      {
        orEditHabitDialog._habit.CreatedTime = DateTime.Now;
        orEditHabitDialog._habit.SyncStatus = 0;
        HabitModel habitModel = orEditHabitDialog._habit;
        habitModel.SortOrder = await HabitDao.GetNewHabitSortOrderBySectionId(orEditHabitDialog._habit.SectionId);
        habitModel = (HabitModel) null;
      }
      else
        orEditHabitDialog._habit.SyncStatus = 1;
      string reminderString = orEditHabitDialog.SetReminderControl.GetReminderString();
      if (orEditHabitDialog._habit.Reminder != reminderString)
        ReminderDelayDao.DeleteByIdAsync(orEditHabitDialog._habit.Id, "habit");
      orEditHabitDialog._habit.Reminder = reminderString;
      if (orEditHabitDialog._isTargetDayChanged)
      {
        if (!await orEditHabitDialog.CheckHabitTargetDay())
          return;
      }
      await HabitService.UpdateHabit(orEditHabitDialog._habit);
      await TimerService.TryUpdateTimerIcon(orEditHabitDialog._habit.Id, orEditHabitDialog._habit.IconRes, orEditHabitDialog._habit.Color);
      DataChangedNotifier.NotifyHabitsChanged();
      if (ABTestManager.IsNewRemindCalculate())
        HabitReminderCalculator.RecalHabitReminder(orEditHabitDialog._habit.Id);
      else
        ReminderCalculator.AssembleReminders();
      orEditHabitDialog.Close();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => this.Close();

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void Toast(object sender, string text)
    {
      this.ToastGrid.Visibility = Visibility.Visible;
      this.ToastTextBlock.Text = text;
      ((Storyboard) this.FindResource((object) "ToastShowAndHide")).Begin();
    }

    public void OnCancel() => this.Close();

    public void Ok()
    {
      if (!this.SaveButton.IsEnabled)
        return;
      this.Save();
    }

    public async Task<bool> CheckHabitTargetDay()
    {
      int? targetDays = this._habit.TargetDays;
      int num = 0;
      if (targetDays.GetValueOrDefault() <= num & targetDays.HasValue)
        return true;
      HabitModel habitModel = this._habit;
      habitModel.CompletedCyclesList = await HabitDao.GetHabitCompletedCycles(this._habit);
      habitModel = (HabitModel) null;
      this._habit.CompletedCycles = new int?(this._habit.CompletedCyclesList.Count<CompletedCycle>((Func<CompletedCycle, bool>) (c => c.isComplete)));
      return new CustomerDialog((string) null, string.Format(Utils.GetString("HabitComfirmEditTargetInfo"), (object) this.GetFormatDateStr(), (object) this._habit.TargetDays), Utils.GetString("Continue"), Utils.GetString("Cancel")).ShowDialog().GetValueOrDefault();
    }

    private async void UpdateSections(bool updateReminder = true)
    {
      AddOrEditHabitDialog orEditHabitDialog = this;
      orEditHabitDialog._allSection = HabitSectionCache.GetSections();
      List<HabitSectionModel> allSection = orEditHabitDialog._allSection;
      // ISSUE: reference to a compiler-generated method
      HabitSectionModel section = allSection != null ? allSection.FirstOrDefault<HabitSectionModel>(new Func<HabitSectionModel, bool>(orEditHabitDialog.\u003CUpdateSections\u003Eb__34_0)) : (HabitSectionModel) null;
      if (section != null)
      {
        string sectionName = orEditHabitDialog.SectionNameTextBlock.Text.ToString();
        orEditHabitDialog.SectionNameTextBlock.Text = section.DisplayName;
        string container = orEditHabitDialog.SetReminderControl.GetReminderString() ?? string.Empty;
        bool flag1 = string.IsNullOrEmpty(container);
        bool flag2 = !flag1 && !container.Contains(",");
        if (flag1 & updateReminder || flag1 | flag2 && orEditHabitDialog._isNew || flag2 && string.IsNullOrEmpty(orEditHabitDialog._habit.Reminder))
        {
          if (container.Contains(","))
          {
            section = (HabitSectionModel) null;
            return;
          }
          string beforeSectionDefaultTime = await HabitService.GetSectionDefaultTime(sectionName);
          string sectionDefaultTime = await HabitService.GetSectionDefaultTime(section.DisplayName);
          if (string.IsNullOrEmpty(sectionDefaultTime))
          {
            section = (HabitSectionModel) null;
            return;
          }
          if (!new List<string>()
          {
            "09:00",
            "13:00",
            "20:00",
            ""
          }.Contains(container) || beforeSectionDefaultTime != orEditHabitDialog._beforeTime && !string.IsNullOrEmpty(orEditHabitDialog._beforeTime))
          {
            orEditHabitDialog._beforeTime = container.ToString();
            section = (HabitSectionModel) null;
            return;
          }
          orEditHabitDialog.SetReminderControl.Init(sectionDefaultTime);
          orEditHabitDialog._beforeTime = orEditHabitDialog.SetReminderControl.GetReminderString() ?? string.Empty;
          beforeSectionDefaultTime = (string) null;
        }
        container = (string) null;
        section = (HabitSectionModel) null;
      }
      else
      {
        orEditHabitDialog.SectionNameTextBlock.Text = Utils.GetString("HabitSectionOthers");
        section = (HabitSectionModel) null;
      }
    }

    private string GetFormatDateStr() => DateUtils.FormatShortDate(this._habit.GetStartDate());

    private void UpdateTargetStartDate()
    {
      int? targetStartDate = this._habit.TargetStartDate;
      ref int? local = ref targetStartDate;
      if (Utils.IsEmptyDate(DateUtils.ParseDateTime(local.HasValue ? local.GetValueOrDefault().ToString() : (string) null)))
        this._habit.TargetStartDate = new int?(Convert.ToInt32(this._habit.CreatedTime.ToString("yyyyMMdd")));
      this.TargetStartDateTextBlock.Text = this.GetFormatDateStr();
    }

    private void UpdateSelectedTargetDay()
    {
      int? targetDays1 = this._habit.TargetDays;
      int num1 = 0;
      if (targetDays1.GetValueOrDefault() < num1 & targetDays1.HasValue || !this._habit.TargetDays.HasValue)
      {
        this._habit.TargetDays = new int?(0);
      }
      else
      {
        int? targetDays2 = this._habit.TargetDays;
        int num2 = 365;
        if (targetDays2.GetValueOrDefault() > num2 & targetDays2.HasValue)
          this._habit.TargetDays = new int?(365);
      }
      TextBlock targetDayTextBlock = this.TargetDayTextBlock;
      int? targetDays3 = this._habit.TargetDays;
      int num3 = 0;
      string str1 = targetDays3.GetValueOrDefault() == num3 & targetDays3.HasValue ? Utils.GetString("Forever") : string.Format(Utils.GetString("HabitCycleDay"), (object) this._habit.TargetDays);
      targetDayTextBlock.Text = str1;
      OptionCheckBox optionCheckBox = (OptionCheckBox) null;
      this.TargetDayCustomPopup.IsOpen = false;
      TextBox targetDayCustomInput = this.TargetDayCustomInput;
      int? targetDays4 = this._habit.TargetDays;
      string str2 = targetDays4.ToString();
      targetDayCustomInput.Text = str2;
      UIElementCollection children = this.TargetDayList.Children;
      int count = children != null ? children.Count : 0;
      int index;
      for (index = 0; index < count; ++index)
      {
        if (this.TargetDayList.Children[index] is OptionCheckBox child && child.Tag is string tag)
        {
          string str3 = tag;
          targetDays4 = this._habit.TargetDays;
          string str4 = targetDays4.ToString();
          if (str3 == str4)
          {
            child.Selected = true;
            break;
          }
          child.Selected = false;
          if (tag == "-1")
            optionCheckBox = child;
        }
      }
      if (index != count)
        return;
      optionCheckBox.Selected = true;
      this.TargetDayCustomPopup.IsOpen = true;
    }

    private void OpenTargetDayPopup(object sender, MouseButtonEventArgs e)
    {
      this.UpdateSelectedTargetDay();
      this.TargetDaySelectPopup.IsOpen = true;
    }

    private void OnTargetDayCustomInputTextChanged(object sender, TextChangedEventArgs e)
    {
      if (!(sender is TextBox textBox))
        return;
      textBox.TextChanged -= new TextChangedEventHandler(this.OnTargetDayCustomInputTextChanged);
      int result = 1;
      if (int.TryParse(textBox.Text, out result))
      {
        if (result < 1)
          result = 1;
        if (result > 365)
          result = 365;
      }
      else
        result = 1;
      textBox.Text = result.ToString();
      textBox.CaretIndex = result.ToString().Length;
      this.TargetDayCustomInputDay.Text = result > 1 ? Utils.GetString("PublicDays") : Utils.GetString("PublicDay");
      textBox.TextChanged += new TextChangedEventHandler(this.OnTargetDayCustomInputTextChanged);
    }

    private void OnTargetDayCustomInputPreviewTextInput(object sender, TextCompositionEventArgs e)
    {
      Regex regex = new Regex("[^0-9.-]+");
      e.Handled = regex.IsMatch(e.Text);
    }

    private void OpenTargetStartDatePopup(object sender, MouseButtonEventArgs e)
    {
      this.TargetStartDatePicker.SetData(new DateTime?(this._habit.GetStartDate()));
      this.TargetStartDatePopup.IsOpen = true;
    }

    private void OnTargetDaySelectCustomOk(object sender, RoutedEventArgs e)
    {
      int result = 0;
      int.TryParse(this.TargetDayCustomInput.Text, out result);
      this._habit.TargetDays = new int?(result);
      this.UpdateSelectedTargetDay();
      this.TargetDayCustomPopup.IsOpen = false;
      this.TargetDaySelectPopup.IsOpen = false;
    }

    private void OnPopupCancelClick(object sender, RoutedEventArgs e) => this.CloseAllPopup();

    private void CloseAllPopup()
    {
      this.TargetDayCustomPopup.IsOpen = false;
      this.TargetDaySelectPopup.IsOpen = false;
      this.TargetStartDatePopup.IsOpen = false;
      this.AddSectionPopup.IsOpen = false;
      this.SectionPopup.IsOpen = false;
    }

    private void OnTargetStartDateOk(object sender, RoutedEventArgs e)
    {
      this._habit.TargetStartDate = new int?(Convert.ToInt32((this.TargetStartDatePicker.SelectedDate ?? DateTime.Today).ToString("yyyyMMdd")));
      this.UpdateTargetStartDate();
      this.TargetStartDatePopup.IsOpen = false;
    }

    private void OnTargetDaySelectClick(object sender, MouseButtonEventArgs e)
    {
      this._isTargetDayChanged = true;
      if (sender is OptionCheckBox optionCheckBox)
      {
        if (optionCheckBox.Tag?.ToString() == "-1")
        {
          this.TargetDayCustomPopup.IsOpen = true;
          return;
        }
        UIElementCollection children = this.TargetDayList.Children;
        int count = children != null ? children.Count : 0;
        int index = 0;
        int result1 = 0;
        if (optionCheckBox.Tag is string tag1)
        {
          if (!int.TryParse(tag1, out result1))
            return;
          if (result1 > 0)
            this._habit.TargetDays = new int?(result1);
          else if (result1 == 0)
            this._habit.TargetDays = new int?(0);
        }
        for (; index < count; ++index)
        {
          if (this.TargetDayList.Children[index] is OptionCheckBox child && child.Selected && child.Tag is string tag2)
          {
            int result2;
            if (!int.TryParse(tag2, out result2))
              return;
            child.Selected = result2 == result1;
          }
        }
        this.UpdateSelectedTargetDay();
        this.TargetDaySelectPopup.IsOpen = false;
      }
      this.TargetDayCustomPopup.IsOpen = false;
    }

    private async void OpenSectionPopup(object sender, MouseButtonEventArgs e)
    {
      if (this._allSection == null || this._allSection.Count == 0)
      {
        await HabitService.InitSections();
        this._allSection = await HabitService.GetHabitSections();
      }
      List<HabitSectionListViewModel> items = new List<HabitSectionListViewModel>();
      bool flag = false;
      foreach (HabitSectionModel habitSectionModel in this._allSection)
      {
        HabitSectionListViewModel sectionListViewModel1 = new HabitSectionListViewModel();
        sectionListViewModel1.Selected = this._habit.SectionId == habitSectionModel.Id;
        sectionListViewModel1.Title = habitSectionModel.DisplayName;
        sectionListViewModel1.Section = habitSectionModel;
        sectionListViewModel1.SortOrder = habitSectionModel.SortOrder;
        HabitSectionListViewModel sectionListViewModel2 = sectionListViewModel1;
        items.Add(sectionListViewModel2);
        flag = flag || sectionListViewModel2.Selected;
      }
      List<HabitSectionListViewModel> sectionListViewModelList1 = items;
      HabitSectionListViewModel sectionListViewModel3 = new HabitSectionListViewModel(HabitSectionModel.GetDefault());
      sectionListViewModel3.Selected = !flag;
      sectionListViewModel3.Title = Utils.GetString("HabitSectionOthers");
      sectionListViewModel3.IsOther = true;
      sectionListViewModelList1.Add(sectionListViewModel3);
      items.Sort((Comparison<HabitSectionListViewModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
      List<HabitSectionListViewModel> sectionListViewModelList2 = items;
      HabitSectionListViewModel sectionListViewModel4 = new HabitSectionListViewModel();
      sectionListViewModel4.Selected = false;
      sectionListViewModel4.Title = Utils.GetString("AddSection");
      sectionListViewModel4.IsAdd = true;
      sectionListViewModelList2.Add(sectionListViewModel4);
      ItemsSourceHelper.SetItemsSource<HabitSectionListViewModel>((ItemsControl) this.SectionList, items);
      this.SectionPopup.IsOpen = true;
    }

    private async void OnSelectSectionClick(object sender, MouseButtonEventArgs e)
    {
      if (this.SectionList.ItemsSource is ObservableCollection<HabitSectionListViewModel> itemsSource1)
      {
        foreach (HabitItemBaseViewModel itemBaseViewModel in (Collection<HabitSectionListViewModel>) itemsSource1)
          itemBaseViewModel.Selected = false;
      }
      if (!(sender is OptionCheckBox optionCheckBox) || !(optionCheckBox.DataContext is HabitSectionListViewModel dataContext))
        return;
      dataContext.Selected = true;
      if (dataContext.IsAdd)
      {
        if (await HabitUtils.CheckSectionLimit())
        {
          this.AddSectionPopup.IsOpen = true;
          this.AddSectionInput.Text = string.Empty;
        }
        this.SectionPopup.IsOpen = true;
      }
      else
      {
        this.AddSectionPopup.IsOpen = false;
        if (this.SectionList.ItemsSource is ObservableCollection<HabitSectionListViewModel> itemsSource2)
        {
          foreach (HabitSectionListViewModel sectionListViewModel in (Collection<HabitSectionListViewModel>) itemsSource2)
          {
            if (sectionListViewModel.Selected)
              this._habit.SectionId = sectionListViewModel.Section?.Id;
          }
        }
        this.AddSectionPopup.IsOpen = false;
        this.SectionPopup.IsOpen = false;
        this.UpdateSections();
      }
    }

    private async void OnAddSectionOkClicked(object sender, RoutedEventArgs e)
    {
      (bool flag, string str) = await HabitService.CheckSectionName(this.AddSectionInput.Text.Trim());
      if (flag)
      {
        HabitSectionModel model = new HabitSectionModel();
        model.Name = this.AddSectionInput.Text;
        await HabitService.AddSections(new List<HabitSectionModel>()
        {
          model
        });
        this._habit.SectionId = model.Id;
        this.AddSectionPopup.IsOpen = false;
        this.SectionPopup.IsOpen = false;
        model = (HabitSectionModel) null;
      }
      else
      {
        this.AddSectionError.Text = str;
        this.AddSectionError.Visibility = Visibility.Visible;
      }
      this.UpdateSections();
    }

    private void OnSectionOkClicked(object sender, RoutedEventArgs e)
    {
      if (this.SectionList.ItemsSource is ObservableCollection<HabitSectionListViewModel> itemsSource)
      {
        foreach (HabitSectionListViewModel sectionListViewModel in (Collection<HabitSectionListViewModel>) itemsSource)
        {
          if (sectionListViewModel.Selected)
            this._habit.SectionId = sectionListViewModel.Section?.Id;
        }
      }
      this.AddSectionPopup.IsOpen = false;
      this.SectionPopup.IsOpen = false;
      this.UpdateSections();
    }

    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
      if (!this.HabitTitleBox.IsMouseOver)
      {
        this.HabitTitleBox.TabIndex = -1;
        Keyboard.ClearFocus();
      }
      if (this.SectionPopup.IsMouseOver || this.AddSectionPopup.IsMouseOver || this.TargetDayCustomPopup.IsMouseOver || this.TargetDaySelectPopup.IsMouseOver || this.TargetStartDatePopup.IsMouseOver)
        return;
      this.CloseAllPopup();
    }

    private async void OnAddSectionInputTextChanged(object sender, EventArgs e)
    {
      if (!NameUtils.IsValidColumnName(this.AddSectionInput.Text) && !string.IsNullOrEmpty(this.AddSectionInput.Text))
      {
        this.AddSectionError.Text = Utils.GetString("SectionNotValid");
        this.AddSectionError.Visibility = Visibility.Visible;
      }
      else
        this.AddSectionError.Visibility = Visibility.Collapsed;
    }

    [Obsolete]
    private void OnWindowLocationChanged(object sender, EventArgs e)
    {
      MethodInfo method = typeof (Popup).GetMethod("UpdatePosition", BindingFlags.Instance | BindingFlags.NonPublic);
      if (method == (MethodInfo) null)
        return;
      if (this.AddSectionPopup.IsOpen)
        method.Invoke((object) this.AddSectionPopup, (object[]) null);
      if (this.SectionPopup.IsOpen)
        method.Invoke((object) this.SectionPopup, (object[]) null);
      if (this.TargetDaySelectPopup.IsOpen)
        method.Invoke((object) this.TargetDaySelectPopup, (object[]) null);
      if (!this.TargetDayCustomPopup.IsOpen)
        return;
      method.Invoke((object) this.TargetDayCustomPopup, (object[]) null);
    }

    private void OnCustomTargetKeyUp(object sender, KeyEventArgs e)
    {
      if (e.Key != Key.Return)
        return;
      this.OnTargetDaySelectCustomOk((object) null, (RoutedEventArgs) null);
      e.Handled = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/addoredithabitdialog.xaml", UriKind.Relative));
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
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnWindowLoaded);
          ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnPreviewMouseDown);
          break;
        case 2:
          ((Timeline) target).Completed += new EventHandler(this.OnStoryCompleted);
          break;
        case 3:
          this.IconGrid = (Grid) target;
          this.IconGrid.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSelecteIconClick);
          break;
        case 4:
          this.IconImage = (Image) target;
          break;
        case 5:
          this.SetIconPopup = (EscPopup) target;
          break;
        case 6:
          this.SetIconControl = (SetHabitIconControl) target;
          break;
        case 7:
          this.HabitTitleBox = (TextBox) target;
          this.HabitTitleBox.TextChanged += new TextChangedEventHandler(this.OnTextChanged);
          break;
        case 8:
          this.FrequencySelectBorder = (Border) target;
          this.FrequencySelectBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OpenFrequencyPopup);
          break;
        case 9:
          this.FreqText = (TextBlock) target;
          break;
        case 10:
          this.SetFrequencyPopup = (EscPopup) target;
          break;
        case 11:
          this.SetFrequencyControl = (SetHabitFrequencyControl) target;
          break;
        case 12:
          this.GoalBorder = (Border) target;
          this.GoalBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OpenGoalPopup);
          break;
        case 13:
          this.GoalText = (TextBlock) target;
          break;
        case 14:
          this.SetGoalPopup = (EscPopup) target;
          break;
        case 15:
          this.SetGoalControl = (SetHabitGoalControl) target;
          break;
        case 16:
          this.TargetDayBorder = (Border) target;
          this.TargetDayBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OpenTargetDayPopup);
          break;
        case 17:
          this.TargetDayTextBlock = (TextBlock) target;
          break;
        case 18:
          this.TargetDaySelectPopup = (EscPopup) target;
          break;
        case 19:
          this.TargetDayList = (StackPanel) target;
          break;
        case 20:
          this.TargetDayCustomRadioButton = (OptionCheckBox) target;
          break;
        case 21:
          this.TargetDayCustomPopup = (EscPopup) target;
          break;
        case 22:
          this.TargetDayCustomInput = (TextBox) target;
          this.TargetDayCustomInput.TextChanged += new TextChangedEventHandler(this.OnTargetDayCustomInputTextChanged);
          this.TargetDayCustomInput.PreviewTextInput += new TextCompositionEventHandler(this.OnTargetDayCustomInputPreviewTextInput);
          this.TargetDayCustomInput.PreviewKeyUp += new KeyEventHandler(this.OnCustomTargetKeyUp);
          break;
        case 23:
          this.TargetDayCustomInputDay = (TextBlock) target;
          break;
        case 24:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnTargetDaySelectCustomOk);
          break;
        case 25:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnPopupCancelClick);
          break;
        case 26:
          this.TargetStartDateBorder = (Border) target;
          this.TargetStartDateBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OpenTargetStartDatePopup);
          break;
        case 27:
          this.TargetStartDateTextBlock = (TextBlock) target;
          break;
        case 28:
          this.TargetStartDatePopup = (EscPopup) target;
          break;
        case 29:
          this.TargetStartDatePicker = (TickDatePicker) target;
          break;
        case 30:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnTargetStartDateOk);
          break;
        case 31:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnPopupCancelClick);
          break;
        case 32:
          this.SectionBorder = (Border) target;
          this.SectionBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OpenSectionPopup);
          break;
        case 33:
          this.SectionNameTextBlock = (EmjTextBlock) target;
          break;
        case 34:
          this.SectionPopup = (EscPopup) target;
          break;
        case 35:
          this.SectionList = (ListView) target;
          break;
        case 36:
          this.AddSectionPopup = (EscPopup) target;
          break;
        case 37:
          this.AddSectionInput = (EmojiEditor) target;
          break;
        case 38:
          this.AddSectionError = (TextBlock) target;
          break;
        case 39:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnAddSectionOkClicked);
          break;
        case 40:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnPopupCancelClick);
          break;
        case 41:
          this.SetReminderControl = (SetHabitReminderControl) target;
          break;
        case 42:
          this.RecordCheckBox = (CheckBox) target;
          break;
        case 43:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 44:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 45:
          this.ToastGrid = (Grid) target;
          break;
        case 46:
          this.ToastTextBlock = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
