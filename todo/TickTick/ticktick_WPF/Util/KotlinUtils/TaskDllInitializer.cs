// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.KotlinUtils.TaskDllInitializer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using ticktick_WPF.ViewModels;
using TickTickHandler;

#nullable disable
namespace ticktick_WPF.Util.KotlinUtils
{
  public static class TaskDllInitializer
  {
    static TaskDllInitializer()
    {
      TickTickTask.CreateTaskCallback(new Func<TASK, TASK>(TaskDllInitializer.CreateTaskFun));
      TickTickTask.GetTaskByIdCallback(new Func<string, TASK>(TaskDllInitializer.GetTaskByIdFun));
      TickTickTask.UpdateTaskCallback(new Action<TASK>(TaskDllInitializer.UpdateTaskFun));
      TickTickTask.UpdateTaskAndRemindersCallback(new Action<TASK>(TaskDllInitializer.UpdateTaskAndRemindersFun));
      TickTickTask.CloneTaskCallback(new Func<TASK, int, TASK>(TaskDllInitializer.CloneTaskFun));
      TickTickTask.GetTaskDefaultReminderParamsCallback(new Func<TaskDefaultReminderParams>(TaskDllInitializer.GetTaskDefaultReminderParamsFun));
      TickTickTask.UpdateCheckListItemsCallback(new Action<TASK, List<CheckItem>>(TaskDllInitializer.UpdateCheckListItemsFun));
      TickTickHandler.Utils.TestRemindValidCallback(new Action<TASK>(TaskDllInitializer.TestRemindValidFun));
      TickTickHandler.Utils.TestTaskIsModifiedInEarlyMorningCallback(new Action<TTCalendar, TTCalendar>(TaskDllInitializer.TestTaskIsModifiedInEarlyMorningFun));
      TickTickHandler.Utils.ToastRepeatCountRemainingCallback(new Action<TASK>(TaskDllInitializer.ToastRepeatCountRemainingFun));
      TickTickTask.GetTasksByParentSidCallback(new Func<List<string>, int, List<TASK>>(TaskDllInitializer.GetTasksByParentSidFun));
      TickTickTask.GetTaskInsertOrderCallback(new Func<long, long, int, long>(TaskDllInitializer.GetTaskInsertOrderFun));
      TickTickTask.UpdateTaskParentCallback(new Action<TASK, string, string>(TaskDllInitializer.UpdateTaskParentFun));
      TickTickTask.UnCompletedTaskLocationCallback(new Action<TASK>(TaskDllInitializer.UnCompletedTaskLocationFun));
      TickTickTask.ResetTaskColumnToDefaultIdIfIsEmptyCallback(new Action<TASK>(TaskDllInitializer.ResetTaskColumnToDefaultIdIfIsEmptyFun));
      TickTickTask.TryToScheduleReminderCallback(new Action<TASK, int>(TaskDllInitializer.TryToScheduleReminderFun));
      TickTickTask.ResetPomoDataCallback(new Action<TASK>(TaskDllInitializer.ResetPomoDataFun));
      TickTickTask.CorrectPomoAndStopwatchWhenTaskCompleteCallback(new Action<string, long>(TaskDllInitializer.CorrectPomoAndStopwatchWhenTaskCompleteFun));
    }

    private static void CorrectPomoAndStopwatchWhenTaskCompleteFun(string id, long taskuid)
    {
    }

    private static void ResetPomoDataFun(TASK task)
    {
    }

    private static void TryToScheduleReminderFun(TASK task, int checklistitemreminderchanged)
    {
    }

    private static void ResetTaskColumnToDefaultIdIfIsEmptyFun(TASK task)
    {
    }

    private static void UnCompletedTaskLocationFun(TASK task)
    {
    }

    private static void UpdateTaskParentFun(TASK task, string newparentId, string oldparentId)
    {
      TaskDao.UpdateParent(task.id, newparentId);
    }

    private static long GetTaskInsertOrderFun(long projectuid, long taskuid, int insertbelowInt)
    {
      bool insertbelow = insertbelowInt != 0;
      long result = 0;
      Task.WaitAll(Task.Run((Func<Task>) (async () =>
      {
        TaskModel taskByUid = await TaskDao.GetTaskByUid(taskuid);
        if (taskByUid == null)
          return;
        result = ProjectSortOrderDao.GetNextTaskSortOrderInProject(taskByUid.projectId, taskByUid.sortOrder, taskByUid.parentId, !insertbelow);
      })));
      return result;
    }

    private static List<TASK> GetTasksByParentSidFun(List<string> ids, int withcloseInt)
    {
      List<TASK> tasks = new List<TASK>();
      foreach (string id1 in ids)
      {
        string id = id1;
        TaskBaseViewModel task = TaskCache.GetTaskById(id);
        if (task != null)
          Task.WaitAll(Task.Run((Func<Task>) (async () =>
          {
            foreach (TaskModel child in await TaskService.GetAllSubTasksByIdAsync(id, task.ProjectId))
            {
              List<TaskReminderModel> reminders = await TaskReminderDao.GetRemindersByTaskId(child.id);
              List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(child.id);
              tasks.Add(TASKDao.NewTASK(child, checkItemsByTaskId, reminders));
              reminders = (List<TaskReminderModel>) null;
            }
          })));
      }
      return tasks;
    }

    private static void ToastRepeatCountRemainingFun(TASK task)
    {
    }

    private static void TestTaskIsModifiedInEarlyMorningFun(TTCalendar modify, TTCalendar start)
    {
    }

    private static void TestRemindValidFun(TASK task)
    {
    }

    public static void Init()
    {
    }

    private static async void UpdateCheckListItemsFun(TASK task, List<CheckItem> items)
    {
      List<CheckItem> checkItemList = items;
      // ISSUE: explicit non-virtual call
      if ((checkItemList != null ? (__nonvirtual (checkItemList.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      foreach (CheckItem checkItem in items)
      {
        string id = checkItem.id;
        System.DateTime dateTime1 = ModelsDao.TTCalendarToDateTime(checkItem.startDate);
        System.DateTime dateTime2 = ModelsDao.TTCalendarToDateTime(checkItem.completedDate);
        System.DateTime? startDate = new System.DateTime?(dateTime1);
        System.DateTime? completeDate = new System.DateTime?(dateTime2);
        int isChecked = checkItem.isChecked;
        TaskDetailItemService.SaveModel(id, startDate, completeDate, isChecked);
      }
    }

    private static TaskDefaultReminderParams GetTaskDefaultReminderParamsFun()
    {
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      List<string> stringList1;
      if (string.IsNullOrEmpty(defaultSafely.TimeReminders))
        stringList1 = new List<string>();
      else
        stringList1 = ((IEnumerable<string>) defaultSafely.TimeReminders.Split(',')).ToList<string>();
      List<string> stringList2 = stringList1;
      List<string> stringList3;
      if (string.IsNullOrEmpty(defaultSafely.AllDayReminders))
        stringList3 = new List<string>();
      else
        stringList3 = ((IEnumerable<string>) defaultSafely.AllDayReminders.Split(',')).ToList<string>();
      List<string> stringList4 = stringList3;
      return new TaskDefaultReminderParams()
      {
        defaultReminderDueTime = stringList2,
        defaultReminderAllDay = stringList4
      };
    }

    private static TASK CloneTaskFun(TASK task, int withChildInt)
    {
      bool flag = withChildInt != 0;
      string taskId = task.id;
      TASK clone = (TASK) null;
      Task.WaitAll(Task.Run((Func<Task>) (async () =>
      {
        List<TaskReminderModel> reminders = await TaskReminderDao.GetRemindersByTaskId(taskId);
        TaskModel localTask = await TaskDao.GetTaskById(taskId);
        List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(taskId);
        foreach (TaskDetailItemModel taskDetailItemModel in checkItemsByTaskId)
          taskDetailItemModel.id = ticktick_WPF.Util.Utils.GetGuid();
        clone = TASKDao.NewTASK(localTask, checkItemsByTaskId, reminders, isClone: true);
        reminders = (List<TaskReminderModel>) null;
        localTask = (TaskModel) null;
      })));
      clone.recurringStartDate = task.recurringStartDate;
      clone.recurringDueDate = task.recurringDueDate;
      string newId = clone.id;
      if (flag)
      {
        Thread thread = new Thread((ThreadStart) (async () =>
        {
          List<string> list = (await TaskService.CopySubTasks(taskId, newId)).Where<TaskModel>((Func<TaskModel, bool>) (t => t.parentId == newId)).Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)).ToList<string>();
          clone.childIds = list;
        }));
        thread.Start();
        int num = 0;
        while (thread.IsAlive && num < 50)
        {
          ++num;
          Thread.Sleep(20);
        }
      }
      return clone;
    }

    [Obsolete]
    private static TASK CreateTaskFun(TASK task)
    {
      TASKDao.InsertModel(task).RunSynchronously();
      TaskModel result = TaskDao.GetThinTaskById(task.id).Result;
      task.uniqueId = (long) result._Id;
      return task;
    }

    private static async void UpdateTaskAndRemindersFun(TASK task)
    {
      string taskId = task.id;
      int i = 1;
      while (!TaskCache.ExistTask(taskId) && i <= 5)
        await Task.Delay(i * 20);
      if (TaskCache.ExistTask(taskId))
        await TASKDao.UpdateTask(task, true);
      else
        await TASKDao.InsertModel(task, true);
      await Task.Delay(500);
      taskId = (string) null;
    }

    private static async void UpdateTaskFun(TASK task)
    {
      if (TaskCache.ExistTask(task.id))
        await TASKDao.UpdateTask(task);
      else
        await TASKDao.InsertModel(task);
      await Task.Delay(500);
    }

    private static TASK GetTaskByIdFun(string taskId)
    {
      TASK result = new TASK();
      Task.WaitAll(Task.Run((Func<Task>) (async () =>
      {
        TaskModel task = await TaskDao.GetTaskById(taskId);
        if (task == null)
        {
          task = (TaskModel) null;
        }
        else
        {
          List<TaskReminderModel> reminders = await TaskReminderDao.GetRemindersByTaskId(task.id);
          List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(task.id);
          result = TASKDao.NewTASK(task, checkItemsByTaskId, reminders);
          reminders = (List<TaskReminderModel>) null;
          task = (TaskModel) null;
        }
      })));
      return result;
    }
  }
}
