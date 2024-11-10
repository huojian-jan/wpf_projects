// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ReminderTime.HabitReminderCalculator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Views.Habit;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util.ReminderTime
{
  public class HabitReminderCalculator
  {
    public static async Task InitHabitReminders()
    {
      await ReminderTimeDao.DeleteReminderTimeByType(4);
      List<ReminderTimeModel> validReminders;
      List<HabitModel> habits;
      List<HabitCheckInModel> monthCheckIns;
      if (!LocalSettings.Settings.ShowHabit)
      {
        validReminders = (List<ReminderTimeModel>) null;
        habits = (List<HabitModel>) null;
        monthCheckIns = (List<HabitCheckInModel>) null;
      }
      else
      {
        validReminders = new List<ReminderTimeModel>();
        habits = await HabitDao.GetNeedCheckHabits();
        DateTime dateTime1 = DateTime.Today;
        DateTime start = dateTime1.AddDays(-30.0);
        dateTime1 = DateTime.Today;
        DateTime end = dateTime1.AddDays(1.0);
        monthCheckIns = await HabitCheckInDao.GetCheckInsInSpan(start, end);
        if (habits != null && habits.Any<HabitModel>())
        {
          Dictionary<string, ReminderDelayModel> delayDict = (await ReminderDelayDao.GetDelayModelByType("habit")).ToDictionaryEx<ReminderDelayModel, string, ReminderDelayModel>((Func<ReminderDelayModel, string>) (d => d.ObjId), (Func<ReminderDelayModel, ReminderDelayModel>) (d => d));
          foreach (HabitModel habitModel in habits)
          {
            HabitModel habit = habitModel;
            if (!string.IsNullOrEmpty(habit.Reminder))
            {
              List<HabitCheckInModel> list = monthCheckIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.HabitId == habit.Id)).ToList<HabitCheckInModel>();
              HabitCheckInModel habitCheckInModel = list.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (v => v.HabitId == habit.Id && v.CheckinStamp == DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture)));
              if (habitCheckInModel == null || habitCheckInModel.Value < habitCheckInModel.Goal && habitCheckInModel.CheckStatus != 1)
              {
                if (await HabitUtils.IsHabitValidInToday(habit, list))
                {
                  string[] source = habit.Reminder.Split(',');
                  if (((IEnumerable<string>) source).Any<string>())
                  {
                    foreach (string str in source)
                    {
                      if (str.Contains(":"))
                      {
                        string[] strArray = str.Split(':');
                        int result1;
                        int result2;
                        if (strArray.Length == 2 && int.TryParse(strArray[0], out result1) && int.TryParse(strArray[1], out result2))
                        {
                          dateTime1 = DateTime.Today;
                          dateTime1 = dateTime1.AddHours((double) result1);
                          DateTime dateTime2 = dateTime1.AddMinutes((double) result2);
                          if (!string.IsNullOrEmpty(habit.Id) && delayDict.ContainsKey(habit.Id))
                          {
                            ReminderDelayModel reminderDelayModel = delayDict[habit.Id];
                            DateTime? nullable = reminderDelayModel.RemindTime;
                            dateTime1 = dateTime2;
                            if ((nullable.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == dateTime1 ? 1 : 0) : 1) : 0) != 0)
                            {
                              nullable = reminderDelayModel.NextTime;
                              if (nullable.HasValue)
                              {
                                nullable = reminderDelayModel.NextTime;
                                dateTime2 = nullable.Value;
                              }
                            }
                          }
                          if (dateTime2 > DateTime.Now)
                            validReminders.Add(new ReminderTimeModel()
                            {
                              EntityId = habit.Id,
                              Type = 4,
                              ReminderTime = dateTime2.Ticks,
                              Time = dateTime2
                            });
                        }
                      }
                    }
                  }
                }
              }
            }
          }
          delayDict = (Dictionary<string, ReminderDelayModel>) null;
        }
        await ReminderTimeDao.AddReminderTimes(validReminders);
        validReminders = (List<ReminderTimeModel>) null;
        habits = (List<HabitModel>) null;
        monthCheckIns = (List<HabitCheckInModel>) null;
      }
    }

    public static async Task RecalHabitReminder(string habitId)
    {
      if (!ABTestManager.IsNewRemindCalculate())
        ;
      else
      {
        HabitModel habit = await HabitDao.GetHabitById(habitId);
        if (habit == null)
          ;
        else
        {
          await ReminderTimeDao.DeleteReminderTimeByIdAndType(habit.Id, 4);
          List<ReminderTimeModel> validReminders = new List<ReminderTimeModel>();
          string reminder = habit.Reminder;
          string[] strArray1;
          if (reminder == null)
            strArray1 = (string[]) null;
          else
            strArray1 = reminder.Split(',');
          string[] reminders = strArray1;
          if (reminders == null)
            ;
          else if (reminders.Length == 0)
            ;
          else if (habit.IsSkipToday())
            ;
          else
          {
            string habitId1 = habitId;
            DateTime dateTime1 = DateTime.Today;
            DateTime startDate = dateTime1.AddDays(-30.0);
            dateTime1 = DateTime.Today;
            DateTime endDate = dateTime1.AddDays(1.0);
            List<HabitCheckInModel> habitCheckIns = await HabitCheckInDao.GetHabitCheckInsByHabitIdInSpan(habitId1, startDate, endDate);
            Dictionary<string, ReminderDelayModel> delayDict = (await ReminderDelayDao.GetDelayModelByType("habit")).ToDictionaryEx<ReminderDelayModel, string, ReminderDelayModel>((Func<ReminderDelayModel, string>) (d => d.ObjId), (Func<ReminderDelayModel, ReminderDelayModel>) (d => d));
            HabitCheckInModel habitCheckInModel = habitCheckIns.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (v => v.HabitId == habit.Id && v.CheckinStamp == DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture)));
            if (habitCheckInModel != null)
            {
              if (habitCheckInModel.Value >= habitCheckInModel.Goal)
                return;
              if (habitCheckInModel.CheckStatus == 1)
                return;
            }
            if (!await HabitUtils.IsHabitValidInToday(habit, habitCheckIns))
              ;
            else
            {
              foreach (string str in reminders)
              {
                char[] chArray = new char[1]{ ':' };
                string[] strArray2 = str.Split(chArray);
                int result1;
                int result2;
                if (strArray2.Length == 2 && int.TryParse(strArray2[0], out result1) && int.TryParse(strArray2[1], out result2))
                {
                  dateTime1 = DateTime.Today;
                  dateTime1 = dateTime1.AddHours((double) result1);
                  DateTime dateTime2 = dateTime1.AddMinutes((double) result2);
                  if (!string.IsNullOrEmpty(habit.Id) && delayDict.ContainsKey(habit.Id))
                  {
                    ReminderDelayModel reminderDelayModel = delayDict[habit.Id];
                    DateTime? remindTime = reminderDelayModel.RemindTime;
                    dateTime1 = dateTime2;
                    if ((remindTime.HasValue ? (remindTime.HasValue ? (remindTime.GetValueOrDefault() == dateTime1 ? 1 : 0) : 1) : 0) != 0 && reminderDelayModel.NextTime.HasValue)
                      dateTime2 = reminderDelayModel.NextTime.Value;
                  }
                  if (dateTime2 > DateTime.Now)
                    validReminders.Add(new ReminderTimeModel()
                    {
                      EntityId = habit.Id,
                      Type = 4,
                      ReminderTime = dateTime2.Ticks,
                      Time = dateTime2
                    });
                }
              }
              await ReminderTimeDao.AddReminderTimes(validReminders);
              validReminders = (List<ReminderTimeModel>) null;
              reminders = (string[]) null;
              habitCheckIns = (List<HabitCheckInModel>) null;
              delayDict = (Dictionary<string, ReminderDelayModel>) null;
            }
          }
        }
      }
    }
  }
}
