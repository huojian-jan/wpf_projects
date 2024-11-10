// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ReminderCalculator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ReminderCalculator
  {
    private static bool _assembling;
    private static bool _needRedo;

    public static void AssembleReminders()
    {
      if (ABTestManager.IsNewRemindCalculate())
        return;
      DelayActionHandlerCenter.TryDoAction("ReminderCalculator.AssembleReminders", (EventHandler) ((o, e) => ThreadUtil.DetachedRunOnUiBackThread(new Action(ReminderCalculator.Assemble))), 3000);
    }

    public static async void Assemble()
    {
      List<ReminderModel> reminders;
      if (ReminderCalculator._assembling)
      {
        ReminderCalculator._needRedo = true;
        reminders = (List<ReminderModel>) null;
      }
      else
      {
        ReminderCalculator._needRedo = false;
        ReminderCalculator._assembling = true;
        reminders = new List<ReminderModel>();
        if (UserDao.IsPro() && LocalSettings.Settings.UserPreference?.TimeTable?.isEnabled.GetValueOrDefault())
        {
          List<ReminderModel> collection = await ScheduleService.LoadCourseReminders();
          if (collection != null && collection.Count > 0)
            reminders.AddRange((IEnumerable<ReminderModel>) collection);
        }
        if (LocalSettings.Settings.ShowHabit)
        {
          List<ReminderModel> collection = await TaskReminderDao.LoadNonFireHabits();
          if (collection != null && collection.Count > 0)
            reminders.AddRange((IEnumerable<ReminderModel>) collection);
        }
        if (!LocalSettings.Settings.ExtraSettings.DoNotDisturbInCalendar)
        {
          List<ReminderModel> collection = await TaskReminderDao.LoadNoneFireBindEvents();
          if (collection != null && collection.Count > 0)
            reminders.AddRange((IEnumerable<ReminderModel>) collection);
        }
        List<ReminderModel> collection1 = await TaskReminderDao.LoadNonFireReminders();
        if (collection1 != null && collection1.Count > 0)
          reminders.AddRange((IEnumerable<ReminderModel>) collection1);
        App.ReminderList = reminders;
        ReminderCalculator._assembling = false;
        if (!ReminderCalculator._needRedo)
        {
          reminders = (List<ReminderModel>) null;
        }
        else
        {
          ReminderCalculator.AssembleReminders();
          reminders = (List<ReminderModel>) null;
        }
      }
    }

    public static async Task<bool> IsReminderExpired(ReminderModel reminder)
    {
      if (!string.IsNullOrEmpty(reminder.CheckItemId))
      {
        TaskDetailItemModel checklistItemById = await TaskDetailItemDao.GetChecklistItemById(reminder.CheckItemId);
        if (checklistItemById != null && checklistItemById.status == 0 && !checklistItemById.snoozeReminderTime.HasValue)
        {
          DateTime? startDate1 = checklistItemById.startDate;
          DateTime? startDate2 = reminder.StartDate;
          if ((startDate1.HasValue == startDate2.HasValue ? (startDate1.HasValue ? (startDate1.GetValueOrDefault() != startDate2.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0)
          {
            if (!string.IsNullOrEmpty(reminder.TaskId))
            {
              TaskModel thinTaskById = await TaskDao.GetThinTaskById(reminder.TaskId);
              if (thinTaskById == null || thinTaskById.deleted != 0)
                return true;
            }
            return false;
          }
        }
        return true;
      }
      if (!string.IsNullOrEmpty(reminder.TaskId))
      {
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(reminder.TaskId);
        if (thinTaskById != null && thinTaskById.deleted == 0 && thinTaskById.status == 0 && (!thinTaskById.remindTime.HasValue || !(thinTaskById.remindTime.Value > DateTime.Now)))
        {
          DateTime? startDate3 = reminder.StartDate;
          DateTime? startDate4 = thinTaskById.startDate;
          if ((startDate3.HasValue == startDate4.HasValue ? (startDate3.HasValue ? (startDate3.GetValueOrDefault() != startDate4.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0)
            goto label_12;
        }
        return true;
      }
label_12:
      return false;
    }
  }
}
