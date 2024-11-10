// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ReminderTime.CourseReminderCalculator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Util.ReminderTime
{
  public class CourseReminderCalculator
  {
    private static List<ReminderModel> _courseReminders;

    public static async Task InitCourseReminders()
    {
      await ReminderTimeDao.DeleteReminderTimeByType(8);
      if (!UserDao.IsPro() || !LocalSettings.Settings.UserPreference?.TimeTable?.isEnabled.GetValueOrDefault())
        return;
      CourseReminderCalculator._courseReminders = await ScheduleService.LoadCourseReminders();
    }

    public static List<ReminderModel> GetCourseReminders(DateTime start, DateTime end)
    {
      List<ReminderModel> courseReminders = CourseReminderCalculator._courseReminders;
      return (courseReminders != null ? (courseReminders.Any<ReminderModel>() ? 1 : 0) : 0) != 0 ? CourseReminderCalculator._courseReminders.Where<ReminderModel>((Func<ReminderModel, bool>) (c =>
      {
        if (c.ReminderTime.HasValue)
        {
          DateTime? reminderTime1 = c.ReminderTime;
          DateTime dateTime1 = start;
          if ((reminderTime1.HasValue ? (reminderTime1.GetValueOrDefault() >= dateTime1 ? 1 : 0) : 0) != 0)
          {
            DateTime? reminderTime2 = c.ReminderTime;
            DateTime dateTime2 = end;
            return reminderTime2.HasValue && reminderTime2.GetValueOrDefault() <= dateTime2;
          }
        }
        return false;
      })).ToList<ReminderModel>() : (List<ReminderModel>) null;
    }
  }
}
