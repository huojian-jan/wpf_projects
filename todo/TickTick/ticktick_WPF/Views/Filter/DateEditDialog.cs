// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Filter.DateEditDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Filter
{
  public class DateEditDialog : ConditionEditDialogBase<string>
  {
    private const string DaysLater = "dayslater";
    private const string Days = "days";
    private const string DaysFromToday = "daysfromtoday";
    private const string Span = "span";
    private List<string> _originDates;
    private int _version = 1;

    public DateEditDialog(int version = 1)
      : this(false, version)
    {
    }

    private DateEditDialog(bool showAll, int version = 1)
      : this(showAll, new List<string>(), LogicType.Or, version)
    {
    }

    public DateEditDialog(
      bool showAll,
      List<string> selectedDates,
      LogicType logicType,
      int version)
    {
      this.InitializeComponent();
      this._originDates = selectedDates.ToList<string>();
      this._version = version;
      this.MinWidth = 348.0;
      this.ViewModel = new FilterConditionViewModel()
      {
        Type = CondType.Date,
        ShowAll = showAll,
        ItemsSource = this.InitDateItems(selectedDates),
        IsAllSelected = selectedDates.Count == 0,
        Logic = logicType,
        SupportedLogic = new List<LogicType>()
        {
          LogicType.Not,
          LogicType.Or
        }
      };
      this.OnSelectedValueChanged += (EventHandler<FilterConditionViewModel>) ((sender, e) =>
      {
        EventHandler<List<string>> selectedDateChanged = this.OnSelectedDateChanged;
        if (selectedDateChanged == null)
          return;
        selectedDateChanged((object) this, this.ViewModel.IsAllSelected ? new List<string>() : this.GetSelectedValues());
      });
    }

    public event EventHandler<List<string>> OnSelectedDateChanged;

    private ObservableCollection<FilterListItemViewModel> InitDateItems(List<string> selectedDates)
    {
      ObservableCollection<FilterListItemViewModel> observableCollection1 = new ObservableCollection<FilterListItemViewModel>();
      observableCollection1.Add(FilterListItemViewModel.BuildDateItem(Utils.GetString("NoDate"), "nodue", "IcNoDate"));
      observableCollection1.Add(FilterListItemViewModel.BuildDateItem(Utils.GetString("overdue"), "overdue", "IcOverDue"));
      observableCollection1.Add(FilterListItemViewModel.BuildDateItem(Utils.GetString("RecurringDate"), "recurring", "IcRepeat"));
      observableCollection1.Add(FilterListItemViewModel.BuildDateItem(Utils.GetString("Today"), "today", "CalDayIcon" + DateTime.Today.Day.ToString()));
      observableCollection1.Add(FilterListItemViewModel.BuildDateItem(Utils.GetString("Tomorrow"), "tomorrow", "IcTomorrowProject"));
      observableCollection1.Add(FilterListItemViewModel.BuildDateItem(Utils.GetString("ThisWeek"), "thisweek", "CalWeekIcon" + DateTime.Today.DayOfWeek.ToString().Substring(0, 2)));
      observableCollection1.Add(FilterListItemViewModel.BuildDateItem(Utils.GetString("NextWeek"), "nextweek", "IcNext7Day"));
      observableCollection1.Add(FilterListItemViewModel.BuildDateItem(Utils.GetString("ThisMonth"), "thismonth", "IcMonth"));
      observableCollection1.Add(FilterListItemViewModel.BuildDateItem(Utils.GetString("NextMonth"), string.Format("offset({0})", (object) "1M"), "IcNextMonth"));
      ObservableCollection<FilterListItemViewModel> observableCollection2 = observableCollection1;
      FilterListItemViewModel model = FilterListItemViewModel.BuildDateItem(Utils.GetString("Duration"), string.Format("span({0}~{1})", (object) 0, (object) 7), (string) null, "IcDateTime");
      model.DisplayType = FilterItemDisplayType.Span;
      DateEditDialog.GetDaysSpanValue(model, selectedDates);
      observableCollection2.Add(model);
      foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) observableCollection2)
      {
        if (!listItemViewModel.Selected && ((IEnumerable<object>) selectedDates).Contains<object>(listItemViewModel.Value))
          listItemViewModel.Selected = true;
      }
      return observableCollection2;
    }

    private static void GetDaysSpanValue(FilterListItemViewModel model, List<string> selectedDates)
    {
      model.DaysFrom = new int?(0);
      model.DaysTo = new int?(7);
      string rule1 = selectedDates.FirstOrDefault<string>((Func<string, bool>) (date => date.StartsWith("span")));
      if (rule1 != null)
      {
        model.Selected = true;
        model.Value = (object) rule1;
        (int? nullable1, int? nullable2) = FilterUtils.GetSpanPairInRule(rule1);
        model.DaysFrom = nullable1;
        model.DaysTo = nullable2;
      }
      else
      {
        int? nullable3 = new int?();
        int? nullable4 = new int?();
        string rule2 = selectedDates.FirstOrDefault<string>((Func<string, bool>) (date => date.EndsWith("daysfromtoday") && date.StartsWith("-")));
        if (rule2 != null)
        {
          int num = DateEditDialog.GetDaysValue(rule2, FilterItemDisplayType.NDaysAgo) * -1;
          nullable3 = new int?(num);
          nullable4 = new int?(num);
        }
        string rule3 = selectedDates.FirstOrDefault<string>((Func<string, bool>) (date => date.EndsWith("daysfromtoday") && !date.StartsWith("-")));
        if (rule3 != null)
        {
          int daysValue = DateEditDialog.GetDaysValue(rule3, FilterItemDisplayType.AfterNDays);
          nullable3 = new int?(daysValue);
          nullable4 = new int?(daysValue);
        }
        string rule4 = selectedDates.FirstOrDefault<string>((Func<string, bool>) (date => date.EndsWith("dayslater")));
        if (rule4 != null)
        {
          nullable3 = new int?(DateEditDialog.GetDaysValue(rule4, FilterItemDisplayType.NDaysLater));
          nullable4 = new int?();
        }
        string rule5 = selectedDates.FirstOrDefault<string>((Func<string, bool>) (date => date.EndsWith("days")));
        if (rule5 != null)
        {
          int daysValue = DateEditDialog.GetDaysValue(rule5, FilterItemDisplayType.NextNDays);
          nullable3 = new int?(0);
          nullable4 = new int?(daysValue - 1);
        }
        List<string> items = new List<string>();
        items.Add(rule5);
        items.Add(rule4);
        items.Add(rule3);
        items.Add(rule2);
        items.RemoveAll((Predicate<string>) (s => s == null));
        string str = items.Join<string>(";");
        if (string.IsNullOrEmpty(str))
          return;
        model.Selected = true;
        model.Value = (object) str;
        model.DaysFrom = nullable3;
        model.DaysTo = nullable4;
      }
    }

    private static int GetDaysValue(string rule, FilterItemDisplayType type)
    {
      string oldValue = string.Empty;
      switch (type)
      {
        case FilterItemDisplayType.NextNDays:
          oldValue = "days";
          break;
        case FilterItemDisplayType.NDaysLater:
          oldValue = "dayslater";
          break;
        case FilterItemDisplayType.AfterNDays:
          oldValue = "daysfromtoday";
          break;
        case FilterItemDisplayType.NDaysAgo:
          oldValue = "daysfromtoday";
          rule = rule.Replace("-", string.Empty);
          break;
      }
      if (rule == null)
        return 7;
      int result;
      int.TryParse(rule.Replace(oldValue, string.Empty), out result);
      return result;
    }

    protected override void SyncOriginal()
    {
      this._originDates = this.GetSelectedValues().ToList<string>();
    }

    protected override bool CanSave() => this.GetSelectedValues().Count > 0 || base.CanSave();

    public override void Restore()
    {
      this.RestoreSetSelected();
      EventHandler<List<string>> selectedDateChanged = this.OnSelectedDateChanged;
      if (selectedDateChanged == null)
        return;
      selectedDateChanged((object) this, this._originDates);
    }

    private void RestoreSetSelected()
    {
      foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this.ViewModel.ItemsSource)
        listItemViewModel.Selected = this._originDates.Contains(listItemViewModel.Value.ToString());
    }

    protected override List<string> GetSelectedValues()
    {
      if (this.ViewModel == null)
        return new List<string>();
      if (this.ViewModel.IsAllSelected)
        return new List<string>();
      List<string> selectedValues = new List<string>();
      foreach (FilterListItemViewModel listItemViewModel in (Collection<FilterListItemViewModel>) this.ViewModel.ItemsSource)
      {
        if (listItemViewModel.Selected)
        {
          string str = (string) listItemViewModel.Value;
          if (str.Contains(";"))
          {
            IEnumerable<string> collection = ((IEnumerable<string>) str.Split(';')).Where<string>((Func<string, bool>) (p => !string.IsNullOrEmpty(p)));
            selectedValues.AddRange(collection);
          }
          else
            selectedValues.Add(str);
        }
      }
      return selectedValues;
    }
  }
}
