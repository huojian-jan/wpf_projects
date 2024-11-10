// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.DateCardViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.Views.Filter;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class DateCardViewModel : FilterListDisplayModel<string>
  {
    private static readonly Dictionary<string, string> DateMap = new Dictionary<string, string>()
    {
      {
        "nodue",
        Utils.GetString("NoDate")
      },
      {
        "overdue",
        Utils.GetString("overdue")
      },
      {
        "recurring",
        Utils.GetString("RecurringDate")
      },
      {
        "today",
        Utils.GetString("Today")
      },
      {
        "tomorrow",
        Utils.GetString("Tomorrow")
      },
      {
        "thisweek",
        Utils.GetString("ThisWeek")
      },
      {
        "nextweek",
        Utils.GetString("NextWeek")
      },
      {
        "thismonth",
        Utils.GetString("ThisMonth")
      }
    };

    public DateCardViewModel() => this.ConditionName = "dueDate";

    public List<string> Values
    {
      get => this.ValueList;
      set
      {
        this.ValueList = value;
        this.Content = this.ToDisplayList(this.ValueList);
        this.OnPropertyChanged(nameof (Values));
      }
    }

    public string ToDisplayList(List<string> dates)
    {
      return DateCardViewModel.FormatDisplayText(dates, " " + Utils.GetString("or") + " ");
    }

    public static string ToNormalDisplayText(List<string> dates)
    {
      return DateCardViewModel.FormatDisplayText(dates, " ,");
    }

    public static string FormatDisplayText(List<string> dates, string divider)
    {
      string str = string.Empty;
      if (dates != null && dates.Count > 0)
      {
        for (int index = 0; index < dates.Count; ++index)
          str = index >= dates.Count - 1 ? str + DateCardViewModel.GetDateText(dates[index]) : str + DateCardViewModel.GetDateText(dates[index]) + divider;
      }
      return str;
    }

    private static string GetDateText(string key)
    {
      if (DateCardViewModel.DateMap.ContainsKey(key))
        return DateCardViewModel.DateMap[key];
      if (key.EndsWith("days"))
        return string.Format(Utils.GetString("NextNDays"), (object) key.Replace("days", string.Empty));
      if (key.EndsWith("dayslater"))
        return string.Format(Utils.GetString("NDaysLater"), (object) key.Replace("dayslater", string.Empty));
      if (key.EndsWith("daysfromtoday"))
        return key.StartsWith("-") ? string.Format(Utils.GetString("NDaysAgo"), (object) key.Replace("daysfromtoday", string.Empty).Replace("-", string.Empty), (object) "th") : string.Format(Utils.GetString("AfterNDays"), (object) key.Replace("daysfromtoday", string.Empty), (object) "th");
      if (key.StartsWith("offset") && key.Length > 8)
      {
        string str = key.Substring(7);
        switch (str.Substring(0, str.Length - 1))
        {
          case "-1D":
            return Utils.GetString("PublicYesterday");
          case "1M":
            return Utils.GetString("NextMonth");
          case "-1M":
            return Utils.GetString("LastMonth");
          case "1W":
            return Utils.GetString("NextWeek");
          case "-1W":
            return Utils.GetString("LastWeek");
        }
      }
      if (!key.StartsWith("span"))
        return string.Empty;
      (int? nullable1, int? nullable2) = FilterUtils.GetSpanPairInRule(key);
      return (nullable1.HasValue ? FilterUtils.GetNthDayString(nullable1.Value) : "-") + " ~ " + (nullable2.HasValue ? FilterUtils.GetNthDayString(nullable2.Value) : "-");
    }

    public override ConditionEditDialog GenerateEditDialog()
    {
      return (ConditionEditDialog) new DateEditDialog();
    }

    public override Rule<string> GetParseRule() => (Rule<string>) new DueDateRule();

    public override string ToCardText()
    {
      switch (this.ValueList.Count)
      {
        case 0:
          return Utils.GetString("date");
        case 1:
          return this.LogicType == LogicType.Not ? Utils.GetString("Exclude") + " " + DateCardViewModel.GetDateText(this.ValueList[0]) : DateCardViewModel.GetDateText(this.ValueList[0]);
        default:
          return this.LogicType == LogicType.Not ? FilterListDisplayModel<string>.GetLogicString(this.LogicType) + " ( " + DateCardViewModel.FormatDisplayText(this.ValueList, ", ") + " )" : this.ToDisplayList(this.ValueList);
      }
    }
  }
}
