// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.SetHabitFrequencyControl
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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class SetHabitFrequencyControl : UserControl, IComponentConnector
  {
    private bool _needCorrectIntervalText;
    private static readonly string ByDay = Utils.GetString(nameof (ByDay));
    private static readonly string ByWeek = Utils.GetString(nameof (ByWeek));
    private static readonly string ByTimeSpan = Utils.GetString(nameof (ByTimeSpan));
    private ComboBoxViewModel _byDayModel = new ComboBoxViewModel((object) SetHabitFrequencyControl.ByDay, SetHabitFrequencyControl.ByDay, 32.0);
    private ComboBoxViewModel _byWeekModel = new ComboBoxViewModel((object) SetHabitFrequencyControl.ByWeek, SetHabitFrequencyControl.ByWeek, 32.0);
    private ComboBoxViewModel _byTimeSpanModel = new ComboBoxViewModel((object) SetHabitFrequencyControl.ByTimeSpan, SetHabitFrequencyControl.ByTimeSpan, 32.0);
    private ObservableCollection<ComboBoxViewModel> _freqTypeComboBoxModels = new ObservableCollection<ComboBoxViewModel>();
    private ObservableCollection<ComboBoxViewModel> _weekTimesComboBoxModels = new ObservableCollection<ComboBoxViewModel>();
    internal CustomComboBox FreqTypeComboBox;
    internal StackPanel ByDayPanel;
    internal TextBlock DayText;
    internal WeekdaySelector WeekDaySelector;
    internal StackPanel ByWeekPanel;
    internal CustomComboBox WeekTimesComboBox;
    internal TextBlock WeekDayText;
    internal StackPanel ByTimeSpanPanel;
    internal TextBox IntervalText;
    private bool _contentLoaded;

    public SetHabitFrequencyControl()
    {
      this.InitializeComponent();
      this.WeekDaySelector.SelectedDaysChanged += new EventHandler(this.OnSelectedDaysChanged);
      this.WeekTimesComboBox.ItemSelected += new EventHandler<ComboBoxViewModel>(this.OnWeekTimesComboBoxItemSelected);
      ObservableCollection<ComboBoxViewModel> observableCollection = new ObservableCollection<ComboBoxViewModel>();
      observableCollection.Add(this._byDayModel);
      observableCollection.Add(this._byWeekModel);
      observableCollection.Add(this._byTimeSpanModel);
      this._freqTypeComboBoxModels = observableCollection;
      for (int index = 1; index <= 7; ++index)
        this._weekTimesComboBoxModels.Add(new ComboBoxViewModel((object) index, index.ToString(), 24.0));
    }

    public event EventHandler<string> OnFreqRuleSaved;

    public event EventHandler Closed;

    public void Init(string habitRule)
    {
      RecurrenceModel recurrenceModel = RecurrenceModel.GetRecurrenceModel(habitRule);
      this._freqTypeComboBoxModels.ToList<ComboBoxViewModel>().ForEach((Action<ComboBoxViewModel>) (item => item.Selected = false));
      this._weekTimesComboBoxModels.ToList<ComboBoxViewModel>().ForEach((Action<ComboBoxViewModel>) (item => item.Selected = false));
      switch (recurrenceModel.Frequency)
      {
        case FrequencyType.Daily:
          (recurrenceModel.Interval > 1 ? (UpDownSelectViewModel) this._byTimeSpanModel : (UpDownSelectViewModel) this._byDayModel).Selected = true;
          this.IntervalText.Text = Math.Max(2, recurrenceModel.Interval).ToString() + string.Empty;
          break;
        case FrequencyType.Weekly:
          if (habitRule.Contains("TT_TIMES"))
          {
            this._byWeekModel.Selected = true;
            Match match = new Regex("TT_TIMES=(\\d{0,})").Match(habitRule);
            int result;
            if (match.Success && int.TryParse(match.Groups[1].ToString(), out result))
            {
              this._weekTimesComboBoxModels[Math.Min(result - 1, 5)].Selected = true;
              break;
            }
            break;
          }
          this._byDayModel.Selected = true;
          break;
        default:
          this._byDayModel.Selected = true;
          break;
      }
      List<DayOfWeek> dayOfWeekList1 = (List<DayOfWeek>) null;
      if (this._byDayModel.Selected)
      {
        List<DayOfWeek> dayOfWeekList2;
        if (recurrenceModel.Frequency != FrequencyType.Weekly)
        {
          dayOfWeekList2 = (List<DayOfWeek>) null;
        }
        else
        {
          List<WeekDay> byDay = recurrenceModel.ByDay;
          dayOfWeekList2 = byDay != null ? byDay.Select<WeekDay, DayOfWeek>((Func<WeekDay, DayOfWeek>) (d => d.DayOfWeek)).ToList<DayOfWeek>() : (List<DayOfWeek>) null;
        }
        dayOfWeekList1 = dayOfWeekList2;
        this.SetDayRepeatText(recurrenceModel.Frequency == FrequencyType.Daily, habitRule);
      }
      if (dayOfWeekList1 == null)
        dayOfWeekList1 = new List<DayOfWeek>()
        {
          DayOfWeek.Monday,
          DayOfWeek.Tuesday,
          DayOfWeek.Wednesday,
          DayOfWeek.Thursday,
          DayOfWeek.Friday,
          DayOfWeek.Saturday,
          DayOfWeek.Sunday
        };
      this.WeekDaySelector.SelectedDays = dayOfWeekList1;
      this.WeekDaySelector.NotifySelectedChanged();
      this.FreqTypeComboBox.Init<ComboBoxViewModel>(this._freqTypeComboBoxModels, this._freqTypeComboBoxModels.FirstOrDefault<ComboBoxViewModel>((Func<ComboBoxViewModel, bool>) (item => item.Selected)) ?? this._freqTypeComboBoxModels[0]);
      this.ChangeFreqTypePanelVisibility(this.FreqTypeComboBox.SelectedItem.Value as string);
      this.WeekTimesComboBox.Init<ComboBoxViewModel>(this._weekTimesComboBoxModels, this._weekTimesComboBoxModels.FirstOrDefault<ComboBoxViewModel>((Func<ComboBoxViewModel, bool>) (item => item.Selected)) ?? this._weekTimesComboBoxModels[0]);
    }

    private void ChangeFreqTypePanelVisibility(string freqType)
    {
      this.ByDayPanel.Visibility = Visibility.Collapsed;
      this.ByWeekPanel.Visibility = Visibility.Collapsed;
      this.ByTimeSpanPanel.Visibility = Visibility.Collapsed;
      if (SetHabitFrequencyControl.ByDay.Equals(freqType))
        this.ByDayPanel.Visibility = Visibility.Visible;
      else if (SetHabitFrequencyControl.ByWeek.Equals(freqType))
      {
        this.ByWeekPanel.Visibility = Visibility.Visible;
      }
      else
      {
        if (!SetHabitFrequencyControl.ByTimeSpan.Equals(freqType))
          return;
        this.ByTimeSpanPanel.Visibility = Visibility.Visible;
      }
    }

    private void OnSelectedDaysChanged(object sender, EventArgs e) => this.SetByDayRule();

    private void SetByDayRule()
    {
      if (!this._byDayModel.Selected)
        return;
      RecurrencePattern byDayPattern = this.GetByDayPattern();
      this.SetDayRepeatText(byDayPattern.Frequency == FrequencyType.Daily, byDayPattern.ToString().Replace("İ", "I"));
    }

    private void SetDayRepeatText(bool isDaily, string rule)
    {
      this.DayText.Text = isDaily ? Utils.GetString("PickDays") : RRuleUtils.RRule2String("2", rule, new DateTime?(), false);
    }

    private RecurrencePattern GetByDayPattern()
    {
      RecurrencePattern byDayPattern = new RecurrencePattern()
      {
        Interval = 1
      };
      bool flag = Enum.GetValues(typeof (DayOfWeek)).Cast<DayOfWeek>().All<DayOfWeek>((Func<DayOfWeek, bool>) (value => this.WeekDaySelector.SelectedDays.Contains(value)));
      byDayPattern.Frequency = flag ? FrequencyType.Daily : FrequencyType.Weekly;
      if (!flag)
      {
        List<WeekDay> list = this.WeekDaySelector.SelectedDays.Select<DayOfWeek, WeekDay>((Func<DayOfWeek, WeekDay>) (item => new WeekDay(item))).ToList<WeekDay>();
        byDayPattern.ByDay = list;
      }
      return byDayPattern;
    }

    private string GetByWeekRule()
    {
      if (!(this.WeekTimesComboBox.SelectedItem.Value is int num))
        return "";
      return num == 7 ? "RRULE:FREQ=DAILY" : "RRULE:FREQ=WEEKLY;INTERVAL=1;TT_TIMES=" + num.ToString();
    }

    private string GetBySpanRule()
    {
      RecurrencePattern recurrencePattern = new RecurrencePattern()
      {
        Frequency = FrequencyType.Daily
      };
      int result;
      if (int.TryParse(this.IntervalText.Text, out result))
      {
        if (result > 30)
        {
          recurrencePattern.Interval = 30;
          this.IntervalText.Text = "30";
        }
        else if (result < 1)
        {
          recurrencePattern.Interval = 1;
          this.IntervalText.Text = "1";
        }
        else
          recurrencePattern.Interval = result;
      }
      else
      {
        recurrencePattern.Interval = 2;
        this.IntervalText.Text = "2";
      }
      return "RRULE:" + recurrencePattern?.ToString();
    }

    private async void OnSaveClick(object sender, RoutedEventArgs e)
    {
      SetHabitFrequencyControl sender1 = this;
      string rule = "";
      if (sender1.FreqTypeComboBox.SelectedItem.Value is string str)
      {
        if (SetHabitFrequencyControl.ByDay.Equals(str))
          rule = "RRULE:" + sender1.GetByDayPattern()?.ToString();
        else if (SetHabitFrequencyControl.ByWeek.Equals(str))
          rule = sender1.GetByWeekRule();
        else if (SetHabitFrequencyControl.ByTimeSpan.Equals(str))
          rule = sender1.GetBySpanRule();
      }
      await Task.Delay(100);
      if (!string.IsNullOrEmpty(rule))
      {
        EventHandler<string> onFreqRuleSaved = sender1.OnFreqRuleSaved;
        if (onFreqRuleSaved != null)
          onFreqRuleSaved((object) sender1, rule);
      }
      EventHandler closed = sender1.Closed;
      if (closed == null)
      {
        rule = (string) null;
      }
      else
      {
        closed((object) sender1, (EventArgs) null);
        rule = (string) null;
      }
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler closed = this.Closed;
      if (closed == null)
        return;
      closed((object) this, (EventArgs) null);
    }

    private async void OnIntervalTextChanged(object sender, TextChangedEventArgs e)
    {
      int result;
      if (int.TryParse(this.IntervalText.Text, out result))
      {
        if (result > 30)
        {
          this.IntervalText.Text = "30";
          this.IntervalText.SelectAll();
        }
        else
        {
          if (result > 0)
            return;
          this.IntervalText.Text = "1";
          this.IntervalText.SelectAll();
        }
      }
      else
      {
        this.IntervalText.Text = "2";
        this.IntervalText.SelectAll();
      }
    }

    private void HandleNumberInput(object sender, TextCompositionEventArgs e)
    {
      if (e.Text.Length < 1 || char.IsDigit(e.Text, e.Text.Length - 1))
        return;
      e.Handled = true;
    }

    private void OnWeekTimesComboBoxItemSelected(object sender, ComboBoxViewModel e)
    {
      this.WeekDayText.Text = Utils.GetString((int) e.Value == 1 ? "PublicDay" : "PublicDays");
    }

    private void OnFreqTypeChanged(object sender, ComboBoxViewModel e)
    {
      this.ChangeFreqTypePanelVisibility(e.Value as string);
      if (!(e.Value is string str))
        return;
      if (SetHabitFrequencyControl.ByDay.Equals(str))
      {
        this.SetByDayRule();
        this.FreqTypeComboBox.Tag = (object) 0;
        UserActCollectUtils.AddClickEvent("habit", "add_edit_habit", "fq_daily");
      }
      else if (SetHabitFrequencyControl.ByWeek.Equals(str))
      {
        this.FreqTypeComboBox.Tag = (object) 1;
        UserActCollectUtils.AddClickEvent("habit", "add_edit_habit", "fq_weekly");
      }
      else
      {
        if (!SetHabitFrequencyControl.ByTimeSpan.Equals(str))
          return;
        UserActCollectUtils.AddClickEvent("habit", "add_edit_habit", "fq_interval");
        this.IntervalText.Focus();
        this.IntervalText.SelectAll();
        this.FreqTypeComboBox.Tag = (object) 3;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/sethabitfrequencycontrol.xaml", UriKind.Relative));
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
          this.FreqTypeComboBox = (CustomComboBox) target;
          break;
        case 2:
          this.ByDayPanel = (StackPanel) target;
          break;
        case 3:
          this.DayText = (TextBlock) target;
          break;
        case 4:
          this.WeekDaySelector = (WeekdaySelector) target;
          break;
        case 5:
          this.ByWeekPanel = (StackPanel) target;
          break;
        case 6:
          this.WeekTimesComboBox = (CustomComboBox) target;
          break;
        case 7:
          this.WeekDayText = (TextBlock) target;
          break;
        case 8:
          this.ByTimeSpanPanel = (StackPanel) target;
          break;
        case 9:
          this.IntervalText = (TextBox) target;
          this.IntervalText.PreviewTextInput += new TextCompositionEventHandler(this.HandleNumberInput);
          this.IntervalText.TextChanged += new TextChangedEventHandler(this.OnIntervalTextChanged);
          break;
        case 10:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSaveClick);
          break;
        case 11:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
