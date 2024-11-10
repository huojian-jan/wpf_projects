// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.KotlinUtils.TASKDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.KotlinUtils
{
  public class TASKDao
  {
    public static TASK NewTASK(
      TaskModel task,
      List<TaskDetailItemModel> items,
      List<TaskReminderModel> reminders,
      DateTime? repeatStart = null,
      DateTime? repeatDue = null,
      bool isClone = false)
    {
      TASK task1 = new TASK();
      ProjectModel projectById = ProjectAndTaskIdsCache.GetProjectById(task.projectId);
      task1.uniqueId = (long) task._Id;
      long result;
      task1.completedUserId = long.TryParse(task.completedUserId, out result) ? result : 0L;
      task1.id = isClone ? Utils.GetGuid() : task.id;
      task1.attendId = task.attendId;
      task1.repeatFlag = task.repeatFlag;
      task1.repeatFrom = task.repeatFrom;
      task1.repeatTaskId = task.repeatTaskId;
      task1.kind = task.kind;
      task1.timeZone = task.timeZone;
      task1.exDate = string.IsNullOrEmpty(task.exDates) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(task.exDates);
      task1.childIds = task.childIds;
      List<Reminder> reminderList;
      if (reminders == null)
      {
        reminderList = (List<Reminder>) null;
      }
      else
      {
        IEnumerable<Reminder> source = reminders.Select<TaskReminderModel, Reminder>((Func<TaskReminderModel, Reminder>) (item => ModelsDao.NewReminder(item, task.userId, task.userId)));
        reminderList = source != null ? source.ToList<Reminder>() : (List<Reminder>) null;
      }
      if (reminderList == null)
        reminderList = new List<Reminder>();
      task1.reminders = reminderList;
      List<CheckItem> checkItemList;
      if (items == null)
      {
        checkItemList = (List<CheckItem>) null;
      }
      else
      {
        IEnumerable<CheckItem> source = items.Select<TaskDetailItemModel, CheckItem>((Func<TaskDetailItemModel, CheckItem>) (item => ModelsDao.NewCheckItem(item, task.timeZone, task.id != item.TaskServerId)));
        checkItemList = source != null ? source.ToList<CheckItem>() : (List<CheckItem>) null;
      }
      if (checkItemList == null)
        checkItemList = new List<CheckItem>();
      task1.items = checkItemList;
      task1.taskStatus = task.status;
      bool? nullable1 = task.isAllDay;
      task1.isAllDay = ((int) nullable1 ?? 1) != 0 ? 1 : 0;
      nullable1 = task.isFloating;
      task1.isFloating = nullable1.GetValueOrDefault() ? 1 : 0;
      task1.progress = task.progress.GetValueOrDefault();
      DateTime? nullable2;
      TTCalendar ttCalendar1;
      if (task.startDate.HasValue)
      {
        nullable2 = task.startDate;
        ttCalendar1 = ModelsDao.NewTTCalendar(nullable2.Value, task.timeZone, true);
      }
      else
        ttCalendar1 = new TTCalendar();
      task1.startDate = ttCalendar1;
      nullable2 = task.dueDate;
      TTCalendar ttCalendar2;
      if (nullable2.HasValue)
      {
        nullable2 = task.dueDate;
        ttCalendar2 = ModelsDao.NewTTCalendar(nullable2.Value, task.timeZone, true);
      }
      else
        ttCalendar2 = new TTCalendar();
      task1.dueDate = ttCalendar2;
      nullable2 = task.completedTime;
      TTCalendar ttCalendar3;
      if (nullable2.HasValue)
      {
        nullable2 = task.completedTime;
        ttCalendar3 = ModelsDao.NewTTCalendar(nullable2.Value, task.timeZone, true);
      }
      else
        ttCalendar3 = new TTCalendar();
      task1.completedDate = ttCalendar3;
      nullable2 = task.repeatFirstDate;
      TTCalendar ttCalendar4;
      if (nullable2.HasValue)
      {
        nullable2 = task.repeatFirstDate;
        ttCalendar4 = ModelsDao.NewTTCalendar(nullable2.Value, task.timeZone, true);
      }
      else
        ttCalendar4 = new TTCalendar();
      task1.repeatFirstDate = ttCalendar4;
      task1.recurringStartDate = !repeatStart.HasValue ? new TTCalendar() : ModelsDao.NewTTCalendar(repeatStart.Value, task.timeZone, true);
      task1.recurringDueDate = !repeatDue.HasValue ? new TTCalendar() : ModelsDao.NewTTCalendar(repeatDue.Value, task.timeZone, true);
      task1.tempDueDate = new TTCalendar();
      task1.tempStartDate = new TTCalendar();
      task1.projectUid = projectById != null ? (long) projectById._Id : 0L;
      task1.projectId = projectById == null ? string.Empty : projectById.id;
      task1.parentId = task.parentId;
      task1.userId = task.userId;
      task1.sortOrder = task.sortOrder;
      nullable2 = task.modifiedTime;
      TTCalendar ttCalendar5;
      if (nullable2.HasValue)
      {
        nullable2 = task.modifiedTime;
        ttCalendar5 = ModelsDao.NewTTCalendar(nullable2.Value, TimeZoneData.LocalTimeZoneModel.TimeZoneName);
      }
      else
        ttCalendar5 = new TTCalendar();
      task1.modifiedTime = ttCalendar5;
      nullable2 = task.remindTime;
      TTCalendar ttCalendar6;
      if (nullable2.HasValue)
      {
        nullable2 = task.remindTime;
        ttCalendar6 = ModelsDao.NewTTCalendar(nullable2.Value, TimeZoneData.LocalTimeZoneModel.TimeZoneName);
      }
      else
        ttCalendar6 = new TTCalendar();
      task1.snoozeRemindTime = ttCalendar6;
      return task1;
    }

    public static async Task InsertModel(TASK temp, bool withReminder = false)
    {
      TaskModel originTask = await App.Connection.Table<TaskModel>().Where((Expression<Func<TaskModel, bool>>) (t => (long) t._Id == temp.uniqueId)).FirstOrDefaultAsync();
      TaskModel task;
      List<string> children;
      if (originTask == null)
      {
        originTask = (TaskModel) null;
        task = (TaskModel) null;
        children = (List<string>) null;
      }
      else
      {
        task = new TaskModel();
        task.title = originTask.title;
        task.content = originTask.content;
        task.desc = originTask.desc;
        try
        {
          task.id = temp.id;
          task.attendId = temp.attendId;
          task.repeatFlag = temp.repeatFlag;
          task.repeatFrom = temp.repeatFrom;
          task.repeatTaskId = temp.repeatTaskId;
          task.kind = temp.kind;
          task.timeZone = temp.timeZone;
          task.userId = temp.userId;
        }
        catch (Exception ex)
        {
        }
        if (string.IsNullOrEmpty(task.id))
        {
          originTask = (TaskModel) null;
          task = (TaskModel) null;
          children = (List<string>) null;
        }
        else
        {
          task.status = temp.taskStatus;
          task.sortOrder = temp.sortOrder;
          task.isAllDay = new bool?(temp.isAllDay == 1);
          task.isFloating = new bool?(temp.isFloating == 1);
          task.startDate = ModelsDao.TTCalendarToLocalDateTime(temp.startDate);
          task.dueDate = ModelsDao.TTCalendarToLocalDateTime(temp.dueDate);
          task.completedTime = ModelsDao.TTCalendarToLocalDateTime(temp.completedDate);
          task.modifiedTime = ModelsDao.TTCalendarToLocalDateTime(temp.modifiedTime);
          task.remindTime = ModelsDao.TTCalendarToLocalDateTime(temp.snoozeRemindTime);
          task.completedUserId = temp.completedUserId.ToString();
          task.projectId = temp.projectId;
          task.parentId = temp.parentId;
          task.priority = originTask.priority;
          task.tag = originTask.tag;
          List<string> list = temp.exDate.ToList<string>();
          task.exDates = JsonConvert.SerializeObject((object) list);
          task.exDate = list.ToArray();
          children = temp.childIds.ToList<string>();
          task.childrenString = JsonConvert.SerializeObject((object) children);
          TaskModel taskModel = await TaskDao.InsertTask(task);
          DateTime? localDateTime = ModelsDao.TTCalendarToLocalDateTime(temp.recurringStartDate);
          int num1;
          if (localDateTime.HasValue)
          {
            DateTime date = localDateTime.Value.Date;
            DateTime? startDate = originTask.startDate;
            ref DateTime? local = ref startDate;
            DateTime? nullable = local.HasValue ? new DateTime?(local.GetValueOrDefault().Date) : new DateTime?();
            num1 = nullable.HasValue ? (date == nullable.GetValueOrDefault() ? 1 : 0) : 0;
          }
          else
            num1 = 0;
          bool modifyFirstRecurrence = num1 != 0;
          if (modifyFirstRecurrence)
          {
            PomodoroSummaryModel[] pomodoroSummaryModelArray = await Utils.CopyPomoSummaries(task, originTask.id);
            await PomoSummaryDao.CleanPomosByTaskId(originTask.id);
            int num2 = await TaskDetailItemService.UncheckDetailItemsByTaskId(originTask.id) ? 1 : 0;
            await TaskService.UncheckAllSubTasks(originTask);
            await SyncStatusDao.AddModifySyncStatus(originTask.id);
          }
          else
          {
            PomodoroSummaryModel[] pomodoroSummaryModelArray1 = await Utils.CopyPomoSummaries(task, originTask.id, true);
          }
          AttachmentModel[] attachmentModelArray = await Utils.CopyAttachmentItem(task, originTask.id);
          await ModelsDao.InsertModels(temp.items, task.id, modifyFirstRecurrence);
          TaskChangeNotifier.NotifyTaskAdded(task);
          if (withReminder)
            await ModelsDao.InsertModels(temp.reminders, task.id);
          await SyncStatusDao.AddCreateSyncStatus(task.id);
          if (!string.IsNullOrEmpty(task.parentId))
          {
            await SyncStatusDao.AddSetParentSyncStatus(task.id, string.Empty);
            await TaskDao.AddOrRemoveTaskChildIds(task.parentId, new List<string>()
            {
              task.id
            }, true);
          }
          List<string> stringList = children;
          // ISSUE: explicit non-virtual call
          if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) == 0)
          {
            originTask = (TaskModel) null;
            task = (TaskModel) null;
            children = (List<string>) null;
          }
          else
          {
            foreach (TaskBaseViewModel child in TaskCache.GetAllSubTasksById(task.id, task.projectId))
            {
              await TaskService.UncheckTaskItem(child.Id);
              await SyncStatusDao.AddCreateSyncStatus(child.Id);
              await SyncStatusDao.AddSetParentSyncStatus(child.Id, string.Empty);
              await TaskDao.AddOrRemoveTaskChildIds(child.ParentId, new List<string>()
              {
                child.Id
              }, true);
            }
            originTask = (TaskModel) null;
            task = (TaskModel) null;
            children = (List<string>) null;
          }
        }
      }
    }

    public static async Task UpdateTask(TASK task, bool withReminder = false)
    {
      string taskId = task.id;
      TaskModel local = await TaskDao.GetTaskById(taskId);
      if (local == null)
      {
        taskId = (string) null;
        local = (TaskModel) null;
      }
      else
      {
        try
        {
          local.attendId = task.attendId;
          local.repeatFlag = task.repeatFlag;
          local.repeatFrom = task.repeatFrom;
          local.repeatTaskId = task.repeatTaskId;
          local.kind = task.kind;
          local.timeZone = task.timeZone;
          local.projectId = task.projectId;
          local.parentId = task.parentId;
          local.userId = task.userId;
        }
        catch (Exception ex)
        {
        }
        local.status = task.taskStatus;
        local.isAllDay = new bool?(task.isAllDay == 1);
        local.isFloating = new bool?(task.isFloating == 1);
        local.startDate = ModelsDao.TTCalendarToLocalDateTime(task.startDate);
        local.dueDate = ModelsDao.TTCalendarToLocalDateTime(task.dueDate);
        local.completedTime = ModelsDao.TTCalendarToLocalDateTime(task.completedDate);
        local.completedUserId = task.completedUserId.ToString();
        local.modifiedTime = new DateTime?(ModelsDao.TTCalendarToDateTime(task.modifiedTime));
        local.remindTime = new DateTime?(ModelsDao.TTCalendarToDateTime(task.snoozeRemindTime));
        local.sortOrder = task.sortOrder;
        List<string> list1 = task.exDate.ToList<string>();
        local.exDates = list1 == null ? string.Empty : JsonConvert.SerializeObject((object) list1);
        local.exDate = list1?.ToArray() ?? new string[0];
        List<string> list2 = task.childIds.ToList<string>();
        local.childrenString = list2 == null ? (string) null : JsonConvert.SerializeObject((object) list2);
        ModelsDao.SaveModels(task.items);
        if (withReminder)
        {
          int num = await TaskReminderDao.DeleteRemindersByTaskId(taskId) ? 1 : 0;
          await ModelsDao.InsertModels(task.reminders, local.id);
          ReminderCalculator.AssembleReminders();
        }
        TaskChangeNotifier.NotifyTaskDateChanged(local.id);
        await TaskService.UpdateTask(local);
        await SyncStatusDao.AddModifySyncStatus(local.id);
        taskId = (string) null;
        local = (TaskModel) null;
      }
    }
  }
}
