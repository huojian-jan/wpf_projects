// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.SetReminderControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views
{
  public class SetReminderControl : UserControl, ITabControl, IComponentConnector
  {
    private readonly DateTime _date;
    private readonly bool _isAllDay;
    private List<string> _recentTriggers = new List<string>();
    private bool _withNull;
    private int _tabIndex = -1;
    internal Grid Container;
    internal UpDownSelectListView ReminderlistView;
    internal EscPopup CustomChooseRemindPopup;
    internal Button CancelButton;
    internal Button SaveButton;
    private bool _contentLoaded;

    public SetReminderControl(
      bool isAllDay,
      IEnumerable<TaskReminderModel> reminders,
      DateTime date,
      bool withNull = true,
      int tabIndex = -1)
    {
      this.InitializeComponent();
      this.ReminderlistView.ItemTemplateSelector = (DataTemplateSelector) new ReminderItemTemplateSelector();
      this._isAllDay = isAllDay;
      this._date = date;
      this._withNull = withNull;
      this._tabIndex = tabIndex;
      this.InitData(reminders);
    }

    public event EventHandler OnCancel;

    public event EventHandler<List<string>> OnSelected;

    private void InitData(IEnumerable<TaskReminderModel> reminders)
    {
      if (this._isAllDay)
        this.InitAllDayData((IEnumerable<string>) reminders.Select<TaskReminderModel, string>((Func<TaskReminderModel, string>) (reminder => reminder.trigger)).ToList<string>());
      else
        this.InitTimeData((IEnumerable<string>) reminders.Select<TaskReminderModel, string>((Func<TaskReminderModel, string>) (reminder => reminder.trigger)).ToList<string>());
    }

    private void InitTimeData(IEnumerable<string> reminders)
    {
      List<AdvanceDateModel> source = AdvanceDateModel.BuildTimeDayModels();
      if (!this._withNull)
        source.RemoveAt(0);
      List<string> list1 = reminders.ToList<string>();
      source[0].Selected = !list1.ToList<string>().Any<string>() && this._withNull;
      foreach (string str in list1)
      {
        string trigger = str;
        AdvanceDateModel advanceDateModel1 = source.FirstOrDefault<AdvanceDateModel>((Func<AdvanceDateModel, bool>) (m => SetReminderControl.EqualRule(m.Trigger, trigger)));
        if (advanceDateModel1 == null)
        {
          List<AdvanceDateModel> advanceDateModelList = source;
          AdvanceDateModel advanceDateModel2 = new AdvanceDateModel(trigger, false);
          advanceDateModel2.Selected = true;
          advanceDateModelList.Add(advanceDateModel2);
        }
        else
          advanceDateModel1.Selected = true;
      }
      List<string> list2 = ((IEnumerable<string>) LocalSettings.Settings.TimeCustomRemind.Split(';')).ToList<string>();
      list2.Remove("");
      this._recentTriggers = list2.Except<string>((IEnumerable<string>) list1).ToList<string>();
      if (this._recentTriggers.Count > 0)
      {
        List<AdvanceDateModel> advanceDateModelList = source;
        AdvanceDateModel advanceDateModel3 = new AdvanceDateModel(true, false);
        advanceDateModel3.IsEnable = false;
        advanceDateModel3.IsAllDay = true;
        advanceDateModel3.DisplayText = Utils.GetString("RecentUsed");
        advanceDateModel3.Order = 200000;
        advanceDateModelList.Add(advanceDateModel3);
        AdvanceDateModel advanceDateModel4 = new AdvanceDateModel(this._recentTriggers[0], false);
        advanceDateModel4.Order += 200000;
        source.Add(advanceDateModel4);
        if (this._recentTriggers.Count > 1)
        {
          AdvanceDateModel advanceDateModel5 = new AdvanceDateModel(this._recentTriggers[1], false);
          advanceDateModel5.Order += 200000;
          source.Add(advanceDateModel5);
        }
      }
      List<AdvanceDateModel> list3 = source.OrderBy<AdvanceDateModel, int>((Func<AdvanceDateModel, int>) (model => model.Order)).ToList<AdvanceDateModel>();
      if (this._tabIndex == 0)
        list3[0].HoverSelected = true;
      this.ReminderlistView.ItemsSource = (IEnumerable) list3;
    }

    private void InitAllDayData(IEnumerable<string> reminders, bool withToday = true)
    {
      List<AdvanceDateModel> source = AdvanceDateModel.BuildAllDayModels(withToday);
      if (!this._withNull)
        source.RemoveAt(0);
      List<string> list1 = reminders.ToList<string>();
      source[0].Selected = !list1.ToList<string>().Any<string>() && this._withNull;
      foreach (string str in list1)
      {
        string trigger = str;
        AdvanceDateModel advanceDateModel1 = source.FirstOrDefault<AdvanceDateModel>((Func<AdvanceDateModel, bool>) (m => SetReminderControl.EqualRule(m.Trigger, trigger)));
        if (advanceDateModel1 == null)
        {
          List<AdvanceDateModel> advanceDateModelList = source;
          AdvanceDateModel advanceDateModel2 = new AdvanceDateModel(trigger, true);
          advanceDateModel2.Selected = true;
          advanceDateModelList.Add(advanceDateModel2);
        }
        else
          advanceDateModel1.Selected = true;
      }
      List<string> list2 = ((IEnumerable<string>) LocalSettings.Settings.AllDayCustomRemind.Split(';')).ToList<string>();
      list2.Remove("");
      this._recentTriggers = list2.Except<string>((IEnumerable<string>) list1).ToList<string>();
      if (this._recentTriggers.Count > 0)
      {
        List<AdvanceDateModel> advanceDateModelList = source;
        AdvanceDateModel advanceDateModel3 = new AdvanceDateModel(true, true);
        advanceDateModel3.IsEnable = false;
        advanceDateModel3.IsAllDay = true;
        advanceDateModel3.DisplayText = Utils.GetString("RecentUsed");
        advanceDateModel3.Order = 200000;
        advanceDateModelList.Add(advanceDateModel3);
        AdvanceDateModel advanceDateModel4 = new AdvanceDateModel(this._recentTriggers[0], true);
        advanceDateModel4.Order += 200000;
        source.Add(advanceDateModel4);
        if (this._recentTriggers.Count > 1)
        {
          AdvanceDateModel advanceDateModel5 = new AdvanceDateModel(this._recentTriggers[1], true);
          advanceDateModel5.Order += 200000;
          source.Add(advanceDateModel5);
        }
      }
      List<AdvanceDateModel> list3 = source.OrderBy<AdvanceDateModel, int>((Func<AdvanceDateModel, int>) (model => model.Order)).ToList<AdvanceDateModel>();
      if (this._tabIndex == 0)
        list3[0].HoverSelected = true;
      this.ReminderlistView.ItemsSource = (IEnumerable) list3;
    }

    private static bool EqualRule(string rule1, string rule2)
    {
      Regex regex = new Regex("(-)?P((\\d{0,})Y)?((\\d{0,})M)?((\\d{0,})W)?((\\d{0,})D)?T?((\\d{0,})H)?((\\d{0,})M)?((\\d{0,})S)?");
      if (!string.IsNullOrEmpty(rule1) && !string.IsNullOrEmpty(rule2))
      {
        Match match1 = regex.Match(rule1);
        Match match2 = regex.Match(rule2);
        if (match1.Success && match2.Success)
        {
          bool flag1 = match1.Groups[1].ToString() == "-";
          int result1;
          int.TryParse(match1.Groups[7].ToString(), out result1);
          int result2;
          int.TryParse(match1.Groups[9].ToString(), out result2);
          int result3;
          int.TryParse(match1.Groups[11].ToString(), out result3);
          int result4;
          int.TryParse(match1.Groups[13].ToString(), out result4);
          bool flag2 = match2.Groups[1].ToString() == "-";
          int result5;
          int.TryParse(match2.Groups[7].ToString(), out result5);
          int result6;
          int.TryParse(match2.Groups[9].ToString(), out result6);
          int result7;
          int.TryParse(match2.Groups[11].ToString(), out result7);
          int result8;
          int.TryParse(match2.Groups[13].ToString(), out result8);
          int num1 = result1 * 10080 + result2 * 1440 + result3 * 60 + result4;
          int num2 = result5 * 10080 + result6 * 1440 + result7 * 60 + result8;
          if (num1 == num2 && flag1 == flag2 || num1 == 0 && num2 == 0)
            return true;
        }
      }
      return false;
    }

    private void OnReminderClick(bool enter, UpDownSelectViewModel e)
    {
      if (!(e is AdvanceDateModel reminder))
        return;
      this.SelectReminder(reminder);
      if (!this._withNull)
        return;
      this.SetEmptyToggled();
    }

    private bool CheckOutOfLimits(int add = 0)
    {
      int num = this.GetSelectedReminder().Count + add;
      if (!UserDao.IsPro())
      {
        if (num > 2)
        {
          SetReminderControl.ShowUpgradeProDialog();
          return true;
        }
      }
      else if (num > 5)
      {
        this.ShowProLimitDialog();
        return true;
      }
      return false;
    }

    private void SelectReminder(AdvanceDateModel reminder)
    {
      if (reminder == null || !reminder.IsEnable)
        return;
      if (reminder.IsEmpty && this.ReminderlistView.ItemsSource is List<AdvanceDateModel> itemsSource)
        itemsSource.ForEach((Action<AdvanceDateModel>) (model => model.Selected = false));
      if (reminder.IsCustom)
      {
        reminder.Selected = false;
        if (this.CheckOutOfLimits(1))
          return;
        this.ShowCustomReminder();
      }
      else
      {
        if (!reminder.Selected)
          return;
        this.CheckOutOfLimits();
      }
    }

    private void SetEmptyToggled()
    {
      if (!(this.ReminderlistView.ItemsSource is List<AdvanceDateModel> itemsSource))
        return;
      int count = this.GetSelectedReminder().Count;
      itemsSource[0].Selected = count == 0;
    }

    private async void ShowProLimitDialog()
    {
      SetReminderControl sender = this;
      EventHandler onCancel = sender.OnCancel;
      if (onCancel != null)
        onCancel((object) sender, (EventArgs) null);
      await Task.Delay(100);
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("RemindersLimit"), MessageBoxButton.OK);
      customerDialog.Owner = Window.GetWindow((DependencyObject) sender);
      customerDialog.ShowDialog();
    }

    private static void ShowUpgradeProDialog()
    {
      ProChecker.ShowUpgradeDialog(ProType.MoreReminders);
    }

    private async void ShowCustomReminder()
    {
      if (!this._isAllDay)
        this.ShowCustomTimeReminderDialog();
      else
        this.ShowAllDayReminderDialog();
    }

    private async void ShowAllDayReminderDialog()
    {
      SetReminderControl setReminderControl = this;
      PopupStateManager.SetCanOpenTimePopup(true, true);
      await Task.Delay(50);
      CustomAllDayReminderControl dayReminderControl = new CustomAllDayReminderControl(new AdvanceDateModel("-PT15H", true)
      {
        Date = setReminderControl._date.Date
      }, setReminderControl.CustomChooseRemindPopup, setReminderControl._tabIndex == 0);
      dayReminderControl.Save += new EventHandler<string>(setReminderControl.OnSaveClick);
      dayReminderControl.Show();
    }

    private void OnSaveClick(object sender, string rule)
    {
      List<string> e = this.OnReminderSelected(rule);
      this.StoreCustomAllDayReminder(rule);
      EventHandler<List<string>> onSelected = this.OnSelected;
      if (onSelected == null)
        return;
      onSelected((object) this, e);
    }

    private void StoreCustomAllDayReminder(string rule)
    {
      if (new List<string>()
      {
        "TRIGGER:P0DT9H0M0S",
        "TRIGGER:-P0DT15H0M0S",
        "TRIGGER:-P2DT15H0M0S",
        "TRIGGER:-P6DT15H0M0S"
      }.Contains(rule))
        return;
      List<string> list = ((IEnumerable<string>) LocalSettings.Settings.AllDayCustomRemind.Split(';')).ToList<string>();
      list.Remove("");
      if (list.Contains(rule))
        list.Remove(rule);
      StringBuilder stringBuilder = new StringBuilder(rule);
      for (int index = 0; index < (list.Count > 6 ? 6 : list.Count); ++index)
        stringBuilder.Append(";").Append(list[index]);
      LocalSettings.Settings.AllDayCustomRemind = stringBuilder.ToString();
      stringBuilder.Clear();
    }

    private void StoreCustomTimeReminder(string rule)
    {
      List<string> list = ((IEnumerable<string>) LocalSettings.Settings.TimeCustomRemind.Split(';')).ToList<string>();
      list.Remove("");
      if (list.Contains(rule))
        list.Remove(rule);
      StringBuilder stringBuilder = new StringBuilder();
      if (!new List<string>()
      {
        "TRIGGER:-PT0S",
        "TRIGGER:-PT5M",
        "TRIGGER:-PT30M",
        "TRIGGER:-PT60M",
        "TRIGGER:-PT1440M"
      }.Contains(rule))
        stringBuilder.Append(rule);
      for (int index = 0; index < (list.Count > 6 ? 6 : list.Count); ++index)
        stringBuilder.Append(";").Append(list[index]);
      LocalSettings.Settings.TimeCustomRemind = stringBuilder.ToString();
      stringBuilder.Clear();
    }

    private List<string> OnReminderSelected(string rule)
    {
      List<string> selectedReminder = this.GetSelectedReminder();
      if (!selectedReminder.Contains(rule))
        selectedReminder.Add(rule);
      return selectedReminder;
    }

    private async void ShowCustomTimeReminderDialog()
    {
      SetReminderControl setReminderControl = this;
      await Task.Delay(50);
      CustomReminderControl customReminderControl = new CustomReminderControl(setReminderControl._date);
      // ISSUE: reference to a compiler-generated method
      customReminderControl.OnCancel += new EventHandler(setReminderControl.\u003CShowCustomTimeReminderDialog\u003Eb__28_0);
      // ISSUE: reference to a compiler-generated method
      customReminderControl.OnSave += new EventHandler<string>(setReminderControl.\u003CShowCustomTimeReminderDialog\u003Eb__28_1);
      setReminderControl.CustomChooseRemindPopup.Child = (UIElement) customReminderControl;
      setReminderControl.CustomChooseRemindPopup.IsOpen = true;
    }

    private void CancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler onCancel = this.OnCancel;
      if (onCancel == null)
        return;
      onCancel((object) this, (EventArgs) null);
    }

    private void SaveClick(object sender, RoutedEventArgs e)
    {
      List<string> selectedReminder = this.GetSelectedReminder();
      for (int index = this._recentTriggers.Count - 1; index >= 0; --index)
      {
        if (selectedReminder.Contains(this._recentTriggers[index]))
        {
          if (this._isAllDay)
            this.StoreCustomAllDayReminder(this._recentTriggers[index]);
          else
            this.StoreCustomTimeReminder(this._recentTriggers[index]);
        }
      }
      EventHandler<List<string>> onSelected = this.OnSelected;
      if (onSelected == null)
        return;
      onSelected((object) this, this.GetSelectedReminder());
    }

    private List<string> GetSelectedReminder()
    {
      return this.ReminderlistView.Items.Cast<AdvanceDateModel>().Where<AdvanceDateModel>((Func<AdvanceDateModel, bool>) (reminder => reminder.Selected && !string.IsNullOrEmpty(reminder.Trigger))).OrderBy<AdvanceDateModel, int>((Func<AdvanceDateModel, int>) (m => m.Order)).Select<AdvanceDateModel, string>((Func<AdvanceDateModel, string>) (m => m.Trigger)).ToList<string>();
    }

    public bool HandleTab(bool shift)
    {
      if (this.CustomChooseRemindPopup.IsOpen)
        return this.CustomChooseRemindPopup.HandleTab(shift);
      this._tabIndex += 3 + (shift ? -1 : 1);
      this._tabIndex %= 3;
      UiUtils.SetSaveButtonTabSelected(this.SaveButton, this._tabIndex == 1);
      UiUtils.SetCancelButtonTabSelected(this.CancelButton, this._tabIndex == 2);
      return true;
    }

    public bool HandleEnter()
    {
      this.EnterSelect();
      return true;
    }

    public bool HandleEsc()
    {
      if (!this.CustomChooseRemindPopup.IsOpen)
        return false;
      this.CustomChooseRemindPopup.HandleEsc();
      return true;
    }

    public bool UpDownSelect(bool isUp)
    {
      if (this.CustomChooseRemindPopup.IsOpen)
        return this.CustomChooseRemindPopup.HandleUpDown(isUp);
      this.ReminderlistView.UpDownSelect(isUp);
      return true;
    }

    public bool LeftRightSelect(bool isLeft) => false;

    private void EnterSelect()
    {
      switch (this._tabIndex)
      {
        case 1:
          this.SaveClick((object) null, (RoutedEventArgs) null);
          break;
        case 2:
          EventHandler onCancel = this.OnCancel;
          if (onCancel == null)
            break;
          onCancel((object) this, (EventArgs) null);
          break;
        default:
          if (this.CustomChooseRemindPopup.IsOpen)
            break;
          this.ReminderlistView.HandleEnter();
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
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setremindercontrol.xaml", UriKind.Relative));
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
          this.Container = (Grid) target;
          break;
        case 2:
          this.ReminderlistView = (UpDownSelectListView) target;
          break;
        case 3:
          this.CustomChooseRemindPopup = (EscPopup) target;
          break;
        case 4:
          this.CancelButton = (Button) target;
          this.CancelButton.Click += new RoutedEventHandler(this.CancelClick);
          break;
        case 5:
          this.SaveButton = (Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.SaveClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
