// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net;
using Ical.Net.DataTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public static class HabitUtils
  {
    public static readonly List<string> HabitIcons = new List<string>()
    {
      "habit_daily_check_in",
      "habit_drink_water",
      "habit_eat_breakfast",
      "habit_eat_fruits",
      "habit_early_to_rise",
      "habit_early_to_bed",
      "habit_learn_words",
      "habit_reading",
      "habit_quit_snacks",
      "habit_exercising",
      "habit_meditating",
      "habit_keep_calm",
      "habit_jogging",
      "habit_stretch",
      "habit_yoga",
      "habit_push_ups",
      "habit_cycling",
      "habit_swimming",
      "habit_learn_language",
      "habit_learn_instrument",
      "habit_do_homework",
      "habit_keep_diary",
      "habit_save_money",
      "habit_self_reflection",
      "habit_eat_dinner",
      "habit_eat_veggies",
      "habit_quit_sugar",
      "habit_deep_breath",
      "habit_control_temper",
      "habit_stop_overthinking",
      "habit_listen_music",
      "habit_watch_movie",
      "habit_no_video_games",
      "habit_beat_phone_addiction",
      "habit_contact",
      "habit_praise_others",
      "habit_help_others",
      "habit_introspect",
      "habit_plan",
      "habit_stay_positive",
      "habit_your_apprearance",
      "habit_take_photos",
      "habit_eye_protection",
      "habit_brush_teeth",
      "habit_take_shower",
      "habit_cleaning",
      "habit_housework",
      "habit_water_flowers",
      "habit_walk_the_dog",
      "habit_cat_keeper",
      "habit_watch_documentary",
      "habit_watch_news",
      "habit_watch_tv_show",
      "habit_watch_soap_opera",
      "habit_not_swear",
      "habit_skincare",
      "habit_take_walk",
      "habit_keep_fit",
      "habit_quit_smoking",
      "habit_quit_drinking",
      "habit_take_medicine",
      "habit_stand",
      "habit_neck_exercises",
      "habit_say_love_u",
      "habit_smile_to_yourself"
    };
    public static readonly Dictionary<string, string> IconColorDict = new Dictionary<string, string>()
    {
      {
        "habit_daily_check_in",
        "#70C362"
      },
      {
        "habit_drink_water",
        "#6FE9BD"
      },
      {
        "habit_eat_breakfast",
        "#ED70A4"
      },
      {
        "habit_eat_fruits",
        "#A1CE61"
      },
      {
        "habit_early_to_rise",
        "#F8D550"
      },
      {
        "habit_early_to_bed",
        "#7B42EB"
      },
      {
        "habit_learn_words",
        "#6FE9BD"
      },
      {
        "habit_reading",
        "#A1CE61"
      },
      {
        "habit_quit_snacks",
        "#CF65F6"
      },
      {
        "habit_exercising",
        "#4AA6EF"
      },
      {
        "habit_meditating",
        "#CF65F6"
      },
      {
        "habit_keep_calm",
        "#5992F8"
      },
      {
        "habit_jogging",
        "#F2B04C"
      },
      {
        "habit_stretch",
        "#ED70A4"
      },
      {
        "habit_yoga",
        "#CF65F6"
      },
      {
        "habit_push_ups",
        "#6FE9BD"
      },
      {
        "habit_cycling",
        "#A1CE61"
      },
      {
        "habit_swimming",
        "#65D7FF"
      },
      {
        "habit_learn_language",
        "#ED70A4"
      },
      {
        "habit_learn_instrument",
        "#5992F8"
      },
      {
        "habit_do_homework",
        "#E5DF2B"
      },
      {
        "habit_keep_diary",
        "#EE8C6F"
      },
      {
        "habit_save_money",
        "#F2B04C"
      },
      {
        "habit_self_reflection",
        "#EE8C6F"
      },
      {
        "habit_eat_dinner",
        "#4AA6EF"
      },
      {
        "habit_eat_veggies",
        "#F8D550"
      },
      {
        "habit_quit_sugar",
        "#52B8D2"
      },
      {
        "habit_deep_breath",
        "#6FE9BD"
      },
      {
        "habit_control_temper",
        "#FFCE77"
      },
      {
        "habit_stop_overthinking",
        "#5992F8"
      },
      {
        "habit_listen_music",
        "#F2B04C"
      },
      {
        "habit_watch_movie",
        "#EE8C6F"
      },
      {
        "habit_no_video_games",
        "#52B8D2"
      },
      {
        "habit_beat_phone_addiction",
        "#7B42EB"
      },
      {
        "habit_contact",
        "#A1CE61"
      },
      {
        "habit_praise_others",
        "#ED70A4"
      },
      {
        "habit_help_others",
        "#EE8C6F"
      },
      {
        "habit_introspect",
        "#F8D550"
      },
      {
        "habit_plan",
        "#52B8D2"
      },
      {
        "habit_stay_positive",
        "#A1CE61"
      },
      {
        "habit_your_apprearance",
        "#CF65F6"
      },
      {
        "habit_take_photos",
        "#4AA6EF"
      },
      {
        "habit_eye_protection",
        "#7B42EB"
      },
      {
        "habit_brush_teeth",
        "#A1CE61"
      },
      {
        "habit_take_shower",
        "#CF65F6"
      },
      {
        "habit_cleaning",
        "#EC6A66"
      },
      {
        "habit_housework",
        "#ED70A4"
      },
      {
        "habit_water_flowers",
        "#4AA6EF"
      },
      {
        "habit_walk_the_dog",
        "#7B42EB"
      },
      {
        "habit_cat_keeper",
        "#EE8C6F"
      },
      {
        "habit_watch_documentary",
        "#ED70A4"
      },
      {
        "habit_watch_news",
        "#A1CE61"
      },
      {
        "habit_watch_tv_show",
        "#5992F8"
      },
      {
        "habit_watch_soap_opera",
        "#F8D550"
      },
      {
        "habit_not_swear",
        "#5992F8"
      },
      {
        "habit_skincare",
        "#CF65F6"
      },
      {
        "habit_take_walk",
        "#A1CE61"
      },
      {
        "habit_keep_fit",
        "#CF65F6"
      },
      {
        "habit_quit_smoking",
        "#52B8D2"
      },
      {
        "habit_quit_drinking",
        "#EC6A66"
      },
      {
        "habit_take_medicine",
        "#F8D550"
      },
      {
        "habit_stand",
        "#FFCE77"
      },
      {
        "habit_neck_exercises",
        "#B0DE81"
      },
      {
        "habit_roll_eyes",
        "#6FE79A"
      },
      {
        "habit_say_love_u",
        "#FA9BB0"
      },
      {
        "habit_smile_to_yourself",
        "#FF7877"
      }
    };
    private static readonly Utils.TupleList<int, string, HabitRealType, HabitCheckInType> CheckInTuple = new Utils.TupleList<int, string, HabitRealType, HabitCheckInType>()
    {
      {
        0,
        "boolean",
        HabitRealType.Auto,
        HabitCheckInType.BoolUnCompleted
      },
      {
        0,
        "boolean",
        HabitRealType.Manual,
        HabitCheckInType.BoolUnCompleted
      },
      {
        0,
        "boolean",
        HabitRealType.CompletedAll,
        HabitCheckInType.BoolUnCompleted
      },
      {
        0,
        "real",
        HabitRealType.Auto,
        HabitCheckInType.AutoUncompleted
      },
      {
        0,
        "real",
        HabitRealType.Manual,
        HabitCheckInType.ManualUnCompleted
      },
      {
        0,
        "real",
        HabitRealType.CompletedAll,
        HabitCheckInType.CompletedAllUnCompleted
      },
      {
        2,
        "boolean",
        HabitRealType.Auto,
        HabitCheckInType.BoolCompleted
      },
      {
        2,
        "boolean",
        HabitRealType.Manual,
        HabitCheckInType.BoolCompleted
      },
      {
        2,
        "boolean",
        HabitRealType.CompletedAll,
        HabitCheckInType.BoolCompleted
      },
      {
        2,
        "real",
        HabitRealType.Auto,
        HabitCheckInType.AutoCompleted
      },
      {
        2,
        "real",
        HabitRealType.Manual,
        HabitCheckInType.ManualCompleted
      },
      {
        2,
        "real",
        HabitRealType.CompletedAll,
        HabitCheckInType.CompletedAllCompleted
      }
    };
    private static DateTime _lastRemindDate;

    public static HabitCheckInType GetHabitCheckInType(int status, string type, double step)
    {
      if (status == -1)
        status = 2;
      HabitRealType habitRealType = step <= 0.0 ? (Math.Abs(step) > 0.0 ? HabitRealType.Manual : HabitRealType.CompletedAll) : HabitRealType.Auto;
      Tuple<int, string, HabitRealType, HabitCheckInType> tuple = HabitUtils.CheckInTuple.FirstOrDefault<Tuple<int, string, HabitRealType, HabitCheckInType>>((Func<Tuple<int, string, HabitRealType, HabitCheckInType>, bool>) (item => item.Item1 == status && item.Item2 == type.ToLower() && item.Item3 == habitRealType));
      return tuple == null ? HabitCheckInType.AutoUncompleted : tuple.Item4;
    }

    public static async Task<bool> IsHabitValidInToday(
      HabitModel habit,
      List<HabitCheckInModel> checkIns)
    {
      if (!await HabitUtils.CheckHabitStartDate(habit, DateTime.Today))
        return false;
      if (!string.IsNullOrEmpty(habit.RepeatRule))
      {
        List<DateTime> source1 = new List<DateTime>();
        List<DateTime> source2 = new List<DateTime>();
        if (checkIns != null)
        {
          foreach (HabitCheckInModel checkIn in checkIns)
          {
            DateTime result;
            if (checkIn.CheckStatus != 0 && DateTime.TryParseExact(checkIn.CheckinStamp, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
              switch (checkIn.CheckStatus)
              {
                case 1:
                  source2.Add(result);
                  continue;
                case 2:
                  source1.Add(result);
                  continue;
                default:
                  continue;
              }
            }
          }
        }
        HabitRepeatInfo habitRepeatInfo = HabitUtils.BuildHabitRepeatInfo(habit.RepeatRule);
        switch (habitRepeatInfo.Type)
        {
          case HabitRepeatType.TimesInWeek:
            DateTime weekStart = Utils.GetWeekStart(DateTime.Today);
            DateTime weekEnd = weekStart.AddDays(6.0) > DateTime.Today ? DateTime.Today : weekStart.AddDays(6.0);
            int num1 = source1 != null ? source1.Count<DateTime>((Func<DateTime, bool>) (d => d >= weekStart && d < DateTime.Today)) : 0;
            int num2 = source1 != null ? source1.Count<DateTime>((Func<DateTime, bool>) (d => d > DateTime.Today && d <= weekEnd)) : 0;
            int num3 = habitRepeatInfo.Count - num1 - num2;
            int num4 = num2 + source2.Count<DateTime>((Func<DateTime, bool>) (d => d > DateTime.Today && d <= weekEnd));
            return (weekEnd - DateTime.Today).Days - num4 < num3;
          case HabitRepeatType.ByDay:
            if (habitRepeatInfo.ByDays.Any<DayOfWeek>() && !habitRepeatInfo.ByDays.Contains(DateTime.Today.DayOfWeek))
              return false;
            break;
          case HabitRepeatType.Daily:
            source1?.Sort((Comparison<DateTime>) ((a, b) => a.CompareTo(b)));
            return (DateTime.Today - source1.LastOrDefault<DateTime>((Func<DateTime, bool>) (d => d.Date <= DateTime.Today)).Date).Days >= habitRepeatInfo.Interval;
        }
      }
      return true;
    }

    public static bool IsReminderValid(string reminderText)
    {
      if (!string.IsNullOrEmpty(reminderText))
      {
        string[] source = reminderText.Split(',');
        if (((IEnumerable<string>) source).Any<string>())
        {
          foreach (string str in source)
          {
            if (str.Contains(":"))
            {
              string[] strArray = str.Split(':');
              int result1;
              int result2;
              if (strArray.Length == 2 && int.TryParse(strArray[0], out result1) && int.TryParse(strArray[1], out result2) && result1 * 60 + result2 > DateTime.Now.Hour * 60 + DateTime.Now.Minute)
                return true;
            }
          }
        }
      }
      return false;
    }

    public static HabitRepeatInfo BuildHabitRepeatInfo(string repeatFlag)
    {
      if (!string.IsNullOrEmpty(repeatFlag))
      {
        RecurrenceModel recurrenceModel = RecurrenceModel.GetRecurrenceModel(repeatFlag);
        if (recurrenceModel.Frequency == FrequencyType.Weekly)
        {
          Match match = new Regex("TT_TIMES=(\\d{0,})").Match(repeatFlag);
          int result;
          if (match.Success && int.TryParse(match.Groups[1].ToString(), out result))
            return new HabitRepeatInfo()
            {
              Type = HabitRepeatType.TimesInWeek,
              Count = result
            };
          if (recurrenceModel.ByDay.Any<WeekDay>())
          {
            List<DayOfWeek> list = recurrenceModel.ByDay.Select<WeekDay, DayOfWeek>((Func<WeekDay, DayOfWeek>) (day => day.DayOfWeek)).ToList<DayOfWeek>();
            return new HabitRepeatInfo()
            {
              Type = HabitRepeatType.ByDay,
              ByDays = list
            };
          }
        }
        else if (recurrenceModel.Frequency == FrequencyType.Daily)
          return new HabitRepeatInfo()
          {
            Type = HabitRepeatType.Daily,
            Interval = recurrenceModel.Interval == 0 ? 1 : recurrenceModel.Interval
          };
      }
      return new HabitRepeatInfo()
      {
        Type = HabitRepeatType.Daily
      };
    }

    public static bool InitShowHint(
      HabitRepeatInfo repeatInfo,
      DateTime date,
      DateTime firstCheckDay,
      List<(DateTime, bool?)> checkedDates)
    {
      return (repeatInfo.Type != HabitRepeatType.Daily ? 0 : (repeatInfo.Interval > 1 ? 1 : 0)) == 0 ? HabitUtils.NeedShowHint(repeatInfo, date, checkedDates) : HabitUtils.NeedShowHintDailyInterval(repeatInfo.Interval, date, firstCheckDay, checkedDates);
    }

    private static bool NeedShowHintDailyInterval(
      int interval,
      DateTime date,
      DateTime firstCheckDay,
      List<(DateTime, bool?)> checkedDates)
    {
      checkedDates?.Sort((Comparison<(DateTime, bool?)>) ((a, b) => a.CompareTo(b)));
      DateTime? date1 = checkedDates != null ? new DateTime?(checkedDates.LastOrDefault<(DateTime, bool?)>((Func<(DateTime, bool?), bool>) (d =>
      {
        bool? nullable = d.Item2;
        bool flag = true;
        return nullable.GetValueOrDefault() == flag & nullable.HasValue && d.Item1.Date < date.Date;
      })).Item1) : new DateTime?();
      if (Utils.IsEmptyDate(date1))
      {
        date1 = new DateTime?(firstCheckDay);
        DateTime? nullable = date1;
        DateTime date2 = date.Date;
        if ((nullable.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == date2 ? 1 : 0) : 1) : 0) != 0)
          return true;
      }
      if (Utils.IsEmptyDate(date1))
        date1 = new DateTime?(DateTime.Today);
      DateTime? nullable1 = date1;
      DateTime date3 = date.Date;
      return (nullable1.HasValue ? (nullable1.GetValueOrDefault() < date3 ? 1 : 0) : 0) != 0 && (date.Date - date1.Value).Days % interval == 0;
    }

    private static bool NeedShowHint(
      HabitRepeatInfo repeatInfo,
      DateTime date,
      List<(DateTime, bool?)> checkedDates)
    {
      switch (repeatInfo.Type)
      {
        case HabitRepeatType.TimesInWeek:
          DateTime weekStart = Utils.GetWeekStart(date);
          DateTime weekEnd = weekStart.AddDays(6.0);
          int num1 = checkedDates != null ? checkedDates.Count<(DateTime, bool?)>((Func<(DateTime, bool?), bool>) (d =>
          {
            bool? nullable = d.Item2;
            bool flag = false;
            return !(nullable.GetValueOrDefault() == flag & nullable.HasValue) && d.Item1 >= weekStart && d.Item1 < date.Date;
          })) : 0;
          int num2 = checkedDates != null ? checkedDates.Count<(DateTime, bool?)>((Func<(DateTime, bool?), bool>) (d =>
          {
            bool? nullable = d.Item2;
            bool flag = false;
            return !(nullable.GetValueOrDefault() == flag & nullable.HasValue) && d.Item1 > date.Date && d.Item1 <= weekEnd;
          })) : 0;
          int num3 = repeatInfo.Count - num1 - num2;
          int num4 = num2 + (checkedDates != null ? checkedDates.Count<(DateTime, bool?)>((Func<(DateTime, bool?), bool>) (d =>
          {
            bool? nullable = d.Item2;
            bool flag = false;
            return nullable.GetValueOrDefault() == flag & nullable.HasValue && d.Item1 > date.Date && d.Item1 <= weekEnd;
          })) : 0);
          return (weekEnd - date.Date).Days - num4 < num3;
        case HabitRepeatType.ByDay:
          if (!repeatInfo.ByDays.Contains(date.DayOfWeek))
            break;
          goto case HabitRepeatType.Daily;
        case HabitRepeatType.Daily:
          return true;
      }
      return false;
    }

    public static string GetHabitHintText(string habitIconRes)
    {
      string[] source = habitIconRes.Split('_');
      return source.Length > 1 && source[0] == "txt" ? source[1] : Utils.GetString(((IEnumerable<string>) source).Where<string>((Func<string, bool>) (str => str.Length >= 1)).Aggregate<string, string>("", (Func<string, string, string>) ((current, str) => current + str.Substring(0, 1).ToUpper() + str.Substring(1).ToLower())));
    }

    public static async Task ShowHabitRecordWindow(
      string habitId,
      string habitName,
      DateTime date,
      bool isEdit = true,
      bool unCompleted = false)
    {
      HabitModel habit = await HabitDao.GetHabitById(habitId);
      if (habit == null)
        ;
      else
      {
        HabitRecordModel record = await HabitRecordDao.GetHabitRecordsByHabitIdAndDate(habitId, int.Parse(date.ToString("yyyyMMdd")));
        HabitRecordModel habitRecordModel1 = record;
        if ((habitRecordModel1 != null ? (habitRecordModel1.Deleted == 1 ? 1 : 0) : 0) != 0)
        {
          App.Connection.DeleteAsync((object) record);
          record = (HabitRecordModel) null;
        }
        Application current = Application.Current;
        if (current == null)
          ;
        else
        {
          Dispatcher dispatcher = current.Dispatcher;
          if (dispatcher == null)
            ;
          else
            dispatcher.Invoke((Action) (() =>
            {
              CheckInLogViewModel model = new CheckInLogViewModel();
              model.HabitId = habitId;
              model.HabitName = habitName;
              model.Content = record?.Content ?? string.Empty;
              model.Date = date;
              HabitRecordModel habitRecordModel2 = record;
              model.Score = habitRecordModel2 != null ? habitRecordModel2.Emoji : 0;
              model.IconRes = habit.IconRes;
              model.Color = habit.Color;
              model.IsBoolHabit = habit.IsBoolHabit();
              model.UnCompleted = unCompleted;
              new EditHabitLogWindow(model, isEdit, record == null).ShowDialog();
            }));
        }
      }
    }

    public static string GetUnitText(string unit)
    {
      return unit == "次" || unit == "Count" ? Utils.GetString("Count") : unit;
    }

    public static async Task<bool> CheckSectionLimit()
    {
      List<HabitSectionModel> sections = HabitSectionCache.GetSections();
      List<HabitSectionModel> list = sections != null ? sections.Where<HabitSectionModel>((Func<HabitSectionModel, bool>) (section => section.SyncStatus != -1)).ToList<HabitSectionModel>() : (List<HabitSectionModel>) null;
      // ISSUE: explicit non-virtual call
      if ((list != null ? (__nonvirtual (list.Count) >= 18 ? 1 : 0) : 0) == 0)
        return true;
      if (UserDao.IsPro())
        new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("ExceedMaxSectionCountMessage"), MessageBoxButton.OK).ShowDialog();
      return false;
    }

    public static async Task<bool> CheckHabitLimit()
    {
      List<HabitModel> allHabits = await HabitDao.GetAllHabits();
      List<HabitModel> list = allHabits != null ? allHabits.Where<HabitModel>((Func<HabitModel, bool>) (habit => habit.SyncStatus != -1 && habit.Status != 1)).ToList<HabitModel>() : (List<HabitModel>) null;
      long userLimit = Utils.GetUserLimit(Constants.LimitKind.HabitNumber);
      int? count = list?.Count;
      long? nullable = count.HasValue ? new long?((long) count.GetValueOrDefault()) : new long?();
      long num = userLimit;
      if (!(nullable.GetValueOrDefault() >= num & nullable.HasValue))
        return true;
      if (UserDao.IsPro())
        new CustomerDialog(Utils.GetString("LimitTips"), Utils.GetString("ProHabitLimit"), MessageBoxButton.OK).ShowDialog();
      else
        ProChecker.ShowUpgradeDialog(ProType.MoreHabits);
      return false;
    }

    public static void WhenCheckInMidnight(string habitReminder, IToastShowWindow window = null)
    {
      if (HabitUtils._lastRemindDate.Date == DateTime.Today)
        return;
      bool flag = true;
      if (!string.IsNullOrEmpty(habitReminder))
      {
        string[] source = habitReminder.Split(',');
        if (((IEnumerable<string>) source).Any<string>())
        {
          foreach (string str in source)
          {
            if (str.Contains(":"))
            {
              string[] strArray = str.Split(':');
              int result;
              if (strArray.Length == 2 && int.TryParse(strArray[0], out result) && result < 3)
                flag = false;
            }
          }
        }
      }
      if (!flag)
        return;
      HabitUtils._lastRemindDate = DateTime.Now;
      string str1 = string.Format(Utils.GetString("NotifyCheckHabitMidnight"), (object) DateTime.Now.ToString("HH:mm"));
      if (window == null)
        Utils.Toast(str1);
      else
        window.TryToastString((object) null, str1);
    }

    public static async Task<bool> CheckHabitStartDate(HabitModel habit, DateTime date)
    {
      return habit.GetStartDate().Date <= date || await HabitCheckInDao.ExistCheckInBefore(habit.Id, date);
    }
  }
}
