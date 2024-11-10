// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ReminderTime.TaskReminderCalculator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util.ReminderTime
{
  public class TaskReminderCalculator
  {
    public static async Task OnTasksChanged(TasksChangeEventArgs args)
    {
      BlockingSet<string> blockingSet = new BlockingSet<string>();
      blockingSet.AddRange(args.AddIds);
      blockingSet.AddRange(args.StatusChangedIds);
      blockingSet.AddRange(args.DeletedChangedIds);
      blockingSet.AddRange(args.UndoDeletedIds);
      blockingSet.AddRange(args.KindChangedIds);
      blockingSet.AddRange(args.ProjectChangedIds);
      blockingSet.AddRange(args.DateChangedIds);
      blockingSet.AddRange(args.AssignChangedIds);
      blockingSet.AddRange(args.BatchChangedIds);
      foreach (string taskId in blockingSet.ToList())
      {
        TaskBaseViewModel task = TaskCache.GetTaskById(taskId);
        if (task != null)
        {
          List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(task.Id);
          List<ReminderTimeModel> reminderTimeModels = !string.IsNullOrEmpty(task.RepeatFlag) ? TaskReminderCalculator.GetRepeatTaskReminders(task, remindersByTaskId) : TaskReminderCalculator.GetTaskReminders(task, remindersByTaskId);
          await ReminderTimeDao.DeleteReminderTimeByIdAndType(task.Id, 0);
          await ReminderTimeDao.AddReminderTimes(reminderTimeModels);
          reminderTimeModels = (List<ReminderTimeModel>) null;
        }
        task = (TaskBaseViewModel) null;
      }
    }

    public static List<ReminderTimeModel> GetTaskReminders(
      TaskModel task,
      List<TaskReminderModel> reminderModels)
    {
      return TaskReminderCalculator.GetTaskReminders(TaskCache.GetTaskById(task.id) ?? new TaskBaseViewModel(task), reminderModels);
    }

    public static List<ReminderTimeModel> GetTaskReminders(
      TaskBaseViewModel task,
      List<TaskReminderModel> reminderModels)
    {
      if (task == null || task.Deleted != 0 || task.Status != 0 || reminderModels == null || reminderModels.Count == 0)
        return (List<ReminderTimeModel>) null;
      List<ReminderTimeModel> taskReminders = new List<ReminderTimeModel>();
      if (task.StartDate.HasValue && !Utils.IsEmptyDate(task.StartDate))
      {
        DateTime dateTime1 = task.StartDate.Value;
        foreach (TaskReminderModel reminderModel in reminderModels)
        {
          if (!string.IsNullOrEmpty(reminderModel.trigger))
          {
            TimeSpan trigger = TriggerUtils.ParseTrigger(reminderModel.trigger);
            DateTime dateTime2 = dateTime1 - trigger;
            if ((!task.RemindTime.HasValue || task.RemindTime.Value < dateTime2) && dateTime2 > DateTime.Now)
              taskReminders.Add(new ReminderTimeModel(task.Id, 0, dateTime2.Ticks));
          }
        }
        if (task.RemindTime.HasValue && task.RemindTime.Value > DateTime.Now)
          taskReminders.Add(new ReminderTimeModel(task.Id, 0, task.RemindTime.Value.Ticks));
      }
      return taskReminders;
    }

    public static List<ReminderTimeModel> GetRepeatTaskReminders(
      TaskModel task,
      List<TaskReminderModel> reminderModels)
    {
      return TaskReminderCalculator.GetRepeatTaskReminders(TaskCache.GetTaskById(task.id) ?? new TaskBaseViewModel(task), reminderModels);
    }

    public static List<ReminderTimeModel> GetRepeatTaskReminders(
      TaskBaseViewModel task,
      List<TaskReminderModel> reminderModels)
    {
      if (task == null || task.Deleted != 0 || task.Status != 0 || string.IsNullOrEmpty(task.RepeatFlag) || reminderModels == null || reminderModels.Count == 0)
        return (List<ReminderTimeModel>) null;
      if (CacheManager.GetProjectById(task.ProjectId) == null)
        return (List<ReminderTimeModel>) null;
      List<ReminderTimeModel> repeatTaskReminders = new List<ReminderTimeModel>();
      if (task.StartDate.HasValue && !Utils.IsEmptyDate(task.StartDate))
      {
        DateTime dateTime1 = task.StartDate.Value;
        List<TimeSpan> list = reminderModels.Where<TaskReminderModel>((Func<TaskReminderModel, bool>) (r => !string.IsNullOrEmpty(r.trigger))).Select<TaskReminderModel, TimeSpan>((Func<TaskReminderModel, TimeSpan>) (r => TriggerUtils.ParseTrigger(r.trigger))).ToList<TimeSpan>();
        foreach (TimeSpan timeSpan in list)
        {
          DateTime dateTime2 = dateTime1 - timeSpan;
          if ((!task.RemindTime.HasValue || task.RemindTime.Value < dateTime2) && dateTime2 > DateTime.Now)
            repeatTaskReminders.Add(new ReminderTimeModel(task.Id, 0, dateTime2.Ticks));
        }
        List<DateTime> repeats = TaskReminderCalculator.GetRepeats(task, dateTime1);
        if (repeats.Any<DateTime>())
        {
          foreach (DateTime dateTime3 in repeats)
          {
            if (dateTime3.Date != dateTime1.Date)
            {
              dateTime1 = DateUtils.SetDateOnly(dateTime1, dateTime3.Date);
              foreach (TimeSpan timeSpan in list)
              {
                DateTime dateTime4 = dateTime1 - timeSpan;
                if ((!task.RemindTime.HasValue || task.RemindTime.Value < dateTime4) && dateTime4 > DateTime.Now)
                  repeatTaskReminders.Add(new ReminderTimeModel(task.Id, 0, dateTime4.Ticks));
              }
            }
          }
        }
        if (task.RemindTime.HasValue && task.RemindTime.Value > DateTime.Now)
          repeatTaskReminders.Add(new ReminderTimeModel(task.Id, 0, task.RemindTime.Value.Ticks));
      }
      return repeatTaskReminders;
    }

    private static List<DateTime> GetRepeats(TaskBaseViewModel task, DateTime startDate)
    {
      List<string> exDate = (List<string>) null;
      if (!string.IsNullOrEmpty(task.ExDates))
        exDate = ((IEnumerable<string>) ExDateSerilizer.ToArray(task.ExDates)).ToList<string>();
      string str = task.IsAllDay.HasValue && task.IsAllDay.Value || !task.IsFloating || !(task.TimeZoneName != TimeZoneData.LocalTimeZoneModel?.TimeZoneName) ? TimeZoneData.LocalTimeZoneModel?.TimeZoneName : task.TimeZoneName;
      DateTime targetTzTime = TimeZoneUtils.LocalToTargetTzTime(startDate, str);
      return RepeatUtils.GetValidRepeatDates(task.RepeatFlag, "2", targetTzTime, DateTime.Today.AddDays(-1.0), DateTime.Today.AddDays(4.0), exDate, str);
    }
  }
}
