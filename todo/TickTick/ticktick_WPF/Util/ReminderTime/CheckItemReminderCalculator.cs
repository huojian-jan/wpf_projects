// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ReminderTime.CheckItemReminderCalculator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.ViewModels;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util.ReminderTime
{
  public class CheckItemReminderCalculator
  {
    public static ReminderTimeModel GetCheckItemReminder(TaskDetailItemModel checkItem)
    {
      return CheckItemReminderCalculator.GetCheckItemReminder(TaskDetailItemCache.GetCheckItemById(checkItem.id) ?? new TaskBaseViewModel(checkItem));
    }

    public static ReminderTimeModel GetCheckItemReminder(TaskBaseViewModel checkItem)
    {
      if (checkItem == null || checkItem.Status != 0 || !checkItem.StartDate.HasValue || Utils.IsEmptyDate(checkItem.StartDate) || ((int) checkItem.IsAllDay ?? 1) != 0)
        return (ReminderTimeModel) null;
      TaskBaseViewModel taskById = TaskCache.GetTaskById(checkItem.ParentId);
      if (taskById == null || taskById.Deleted != 0 || taskById.Status != 0)
        return (ReminderTimeModel) null;
      if (checkItem.StartDate.HasValue && !Utils.IsEmptyDate(checkItem.StartDate))
      {
        DateTime dateTime1 = checkItem.StartDate.Value;
        if (checkItem.RemindTime.HasValue)
        {
          DateTime? remindTime = checkItem.RemindTime;
          DateTime dateTime2 = dateTime1;
          if ((remindTime.HasValue ? (remindTime.GetValueOrDefault() < dateTime2 ? 1 : 0) : 0) == 0)
            goto label_9;
        }
        if (dateTime1 > DateTime.Now)
          return new ReminderTimeModel(checkItem.Id, 1, dateTime1.Ticks);
      }
label_9:
      return checkItem.RemindTime.HasValue && checkItem.RemindTime.Value > DateTime.Now ? new ReminderTimeModel(checkItem.Id, 1, checkItem.RemindTime.Value.Ticks) : (ReminderTimeModel) null;
    }

    public static async Task OnTasksChanged(TasksChangeEventArgs args)
    {
      foreach (string checkItemId in args.CheckItemChangedIds.ToList())
        await CheckItemReminderCalculator.ResetCheckItemReminder(checkItemId);
      BlockingSet<string> blockingSet = new BlockingSet<string>();
      blockingSet.AddRange(args.AddIds);
      blockingSet.AddRange(args.BatchChangedIds);
      foreach (string taskId in blockingSet.ToList())
      {
        TaskBaseViewModel taskById = TaskCache.GetTaskById(taskId);
        if (taskById != null && taskById.Status == 0 && taskById.Deleted == 0 && taskById.Kind == "CHECKLIST")
        {
          BlockingList<TaskBaseViewModel> checkItems = taskById.CheckItems;
          if (checkItems != null && checkItems.Count > 0)
          {
            foreach (TaskBaseViewModel taskBaseViewModel in checkItems.ToList())
              await CheckItemReminderCalculator.ResetCheckItemReminder(taskBaseViewModel?.Id);
          }
        }
      }
    }

    private static async Task ResetCheckItemReminder(string checkItemId)
    {
      await ReminderTimeDao.DeleteReminderTimeByIdAndType(checkItemId, 1);
      TaskBaseViewModel checkItemById = TaskDetailItemCache.GetCheckItemById(checkItemId);
      if (checkItemById == null)
        return;
      ReminderTimeModel checkItemReminder = CheckItemReminderCalculator.GetCheckItemReminder(checkItemById);
      if (checkItemReminder == null)
        return;
      await ReminderTimeDao.AddReminderTimes(new List<ReminderTimeModel>()
      {
        checkItemReminder
      });
    }
  }
}
