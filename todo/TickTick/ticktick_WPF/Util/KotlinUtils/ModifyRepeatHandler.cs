// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.KotlinUtils.ModifyRepeatHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.Time;
using TickTickDao;
using TickTickHandler;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util.KotlinUtils
{
  public static class ModifyRepeatHandler
  {
    private static bool _inited;
    private static IToastShowWindow _toastWindow;
    private static TaskModel _deleteTask;

    static ModifyRepeatHandler()
    {
      TickTickTask.DeletePomodoroSummariesCallback(new Action<string>(ModifyRepeatHandler.DeletePomodoroSummariesFun));
      ModifySingleRepeat.ShowDeleteChoiceDialogCallback(new Func<List<string>, string>(ModifyRepeatHandler.ShowDeleteChoiceDialogFun));
      ModifySingleRepeat.ShowUpdateDateChoiceDialogCallback(new Func<List<string>, string>(ModifyRepeatHandler.ShowUpdateDateChoiceDialogFun));
      ModifySingleRepeat.OnEndDeterminedCallback(new Action<string, List<TASK>, TaskDeletedUndoModel>(ModifyRepeatHandler.OnEndDeterminedFun));
      TickTickHandler.Utils.GetCurrentRemoteUserIdCallback(new Func<long>(ModifyRepeatHandler.GetCurrentRemoteUserIdFun));
      TickTickHandler.Utils.GetUserIdCallback(new Func<string>(ModifyRepeatHandler.GetUserIdFun));
      ModifySingleRepeat.InitModifyRepeat();
      ModifyRepeatHandler._inited = true;
    }

    private static string GetUserIdFun() => LocalSettings.Settings.LoginUserId;

    private static long GetCurrentRemoteUserIdFun()
    {
      long result;
      long.TryParse(LocalSettings.Settings.LoginUserId, out result);
      return result;
    }

    private static async void OnEndDeterminedFun(
      string editotType,
      List<TASK> taskList,
      TaskDeletedUndoModel undoModel)
    {
      List<TASK> taskList1 = taskList;
      // ISSUE: explicit non-virtual call
      if ((taskList1 != null ? (__nonvirtual (taskList1.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        if (editotType.ToLower() != "cancel")
          await Task.Delay(300);
        TaskChangeNotifier.NotifyBatchDateChanged(taskList.Select<TASK, string>((Func<TASK, string>) (t => t.id)).ToList<string>());
        ticktick_WPF.Notifier.GlobalEventManager.NotifyReloadCalendar();
      }
      List<string> list1 = undoModel.deleteSids.ToList<string>();
      if (list1 != null && list1.Any<string>())
        ModifyRepeatHandler.DeleteTask(list1[0]);
      List<TaskDeleteUndoUpdateEntity> list2 = undoModel.updateEntities.ToList<TaskDeleteUndoUpdateEntity>();
      if (list2 == null)
        return;
      List<TaskDeleteRecurrenceUndoEntity> undoModels = new List<TaskDeleteRecurrenceUndoEntity>();
      foreach (TaskDeleteUndoUpdateEntity undo in list2)
        undoModels.Add(new TaskDeleteRecurrenceUndoEntity(undo));
      ModifyRepeatHandler._toastWindow?.ToastDeleteRecUndo(undoModels);
    }

    private static string ShowUpdateDateChoiceDialogFun(List<string> editotTypes)
    {
      ModifyRepeatDialog modifyRepeatDialog = new ModifyRepeatDialog(editotTypes);
      TaskDetailPopup.SetCanClose(false);
      modifyRepeatDialog.ShowDialog();
      TaskDetailPopup.SetCanClose(true);
      if (modifyRepeatDialog.Selected != EditorType.CANCEL)
        TaskDetailPopup.TryCloseWindow();
      return modifyRepeatDialog.Selected.ToString();
    }

    private static string ShowDeleteChoiceDialogFun(List<string> editotTypes)
    {
      ModifyRepeatDialog modifyRepeatDialog = new ModifyRepeatDialog(editotTypes, true);
      TaskDetailPopup.SetCanClose(false);
      modifyRepeatDialog.ShowDialog();
      TaskDetailPopup.SetCanClose(true);
      if (modifyRepeatDialog.Selected != EditorType.CANCEL)
        TaskDetailPopup.TryCloseWindow();
      return modifyRepeatDialog.Selected.ToString();
    }

    private static async void DeleteTask(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
        task = (TaskModel) null;
      else if (string.IsNullOrEmpty(task.attendId))
      {
        List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(task.id, task.projectId);
        // ISSUE: explicit non-virtual call
        if (subTasksByIdAsync != null && __nonvirtual (subTasksByIdAsync.Count) > 0)
        {
          subTasksByIdAsync.Add(task);
          IToastShowWindow toastWindow = ModifyRepeatHandler._toastWindow;
          if (toastWindow == null)
          {
            task = (TaskModel) null;
          }
          else
          {
            toastWindow.BatchDeleteTask(subTasksByIdAsync);
            task = (TaskModel) null;
          }
        }
        else
        {
          IToastShowWindow toastWindow = ModifyRepeatHandler._toastWindow;
          if (toastWindow == null)
          {
            task = (TaskModel) null;
          }
          else
          {
            toastWindow.TaskDeleted(task.id);
            task = (TaskModel) null;
          }
        }
      }
      else if (!await TaskOperationHelper.CheckIfAllowDeleteAgenda(task, (DependencyObject) null))
      {
        task = (TaskModel) null;
      }
      else
      {
        await TaskService.DeleteAgenda(task.id, task.projectId, task.attendId);
        SyncManager.TryDelaySync();
        task = (TaskModel) null;
      }
    }

    private static void DeletePomodoroSummariesFun(string id)
    {
      PomoSummaryDao.DeletePomosByTaskId(id);
    }

    public static async Task<bool> TryUpdateDueDateOnlyDate(
      string taskId,
      System.DateTime? recurringStart,
      System.DateTime? recurringEnd,
      TimeData timeData,
      System.DateTime date,
      int fromCalendar,
      int fromDetail)
    {
      int num = 0;
      System.DateTime? nullable1 = timeData.StartDate;
      if (nullable1.HasValue)
      {
        nullable1 = timeData.DueDate;
        if (nullable1.HasValue)
        {
          nullable1 = timeData.DueDate;
          System.DateTime dateTime1 = nullable1.Value;
          nullable1 = timeData.StartDate;
          System.DateTime dateTime2 = nullable1.Value;
          num = (int) (dateTime1 - dateTime2).TotalMinutes;
        }
        TimeData timeData1 = timeData;
        nullable1 = timeData.StartDate;
        System.DateTime? nullable2 = new System.DateTime?(ticktick_WPF.Util.DateUtils.SetDateOnly(nullable1.Value, date));
        timeData1.StartDate = nullable2;
        if (num > 0)
        {
          TimeData timeData2 = timeData;
          nullable1 = timeData.StartDate;
          System.DateTime? nullable3 = new System.DateTime?(nullable1.Value.AddMinutes((double) num));
          timeData2.DueDate = nullable3;
        }
      }
      else
      {
        timeData.StartDate = new System.DateTime?(date);
        TimeData timeData3 = timeData;
        nullable1 = new System.DateTime?();
        System.DateTime? nullable4 = nullable1;
        timeData3.DueDate = nullable4;
        timeData.IsAllDay = new bool?(true);
      }
      return await ModifyRepeatHandler.TryUpdateDueDate(taskId, recurringStart, recurringEnd, timeData, fromCalendar, fromDetail);
    }

    public static async Task<bool> TryUpdateDueDate(
      string taskId,
      System.DateTime? recurringStart,
      System.DateTime? recurringEnd,
      TimeData reviseData,
      int fromCalendar,
      int fromDetail)
    {
      if (!ModifyRepeatHandler._inited || reviseData == null)
        return false;
      TaskModel task = await TaskDao.GetTaskById(taskId);
      if (task == null)
        return false;
      bool isRepeatChanged = task.repeatFlag != reviseData.RepeatFlag || task.repeatFrom != reviseData.RepeatFrom;
      List<TaskReminderModel> reminders = await TaskReminderDao.GetRemindersByTaskId(task.id);
      bool isReminderChanged = !TaskReminderDao.IsEquals(reminders, reviseData.Reminders);
      System.DateTime? nullable1 = recurringStart;
      System.DateTime? nullable2 = reviseData.StartDate;
      System.DateTime? nullable3;
      int num1;
      if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0)
      {
        nullable2 = recurringEnd;
        nullable3 = reviseData.DueDate;
        if ((nullable2.HasValue == nullable3.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() != nullable3.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0)
        {
          bool? isAllDay = task.isAllDay;
          int num2 = (int) isAllDay ?? 1;
          isAllDay = reviseData.IsAllDay;
          int num3 = (int) isAllDay ?? 1;
          if (num2 == num3 && !(task.timeZone != reviseData.TimeZone.TimeZoneName))
          {
            num1 = task.Floating != reviseData.TimeZone.IsFloat ? 1 : 0;
            goto label_10;
          }
        }
      }
      num1 = 1;
label_10:
      int num4 = isRepeatChanged ? 1 : 0;
      if ((num1 | num4 | (isReminderChanged ? 1 : 0)) == 0)
        return false;
      List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(task.id);
      DueDataResult duedataSetModel = new DueDataResult()
      {
        origin = ModelsDao.NewDueDataModel(task, reminders, recurringStart, recurringEnd),
        revise = ModelsDao.NewDueDataModel(task, reviseData),
        isReminderChanged = isReminderChanged ? 1 : 0,
        onlyDateChanged = isReminderChanged | isRepeatChanged ? 0 : 1
      };
      TaskModel task1 = task;
      List<TaskDetailItemModel> items = checkItemsByTaskId;
      List<TaskReminderModel> reminders1 = reminders;
      nullable3 = recurringStart;
      System.DateTime? repeatStart = nullable3 ?? task.startDate;
      nullable3 = recurringEnd;
      System.DateTime? repeatDue = nullable3 ?? task.dueDate;
      TASK task2 = TASKDao.NewTASK(task1, items, reminders1, repeatStart, repeatDue);
      try
      {
        ModifySingleRepeat.UpdateDueData(task2, duedataSetModel, fromCalendar, fromDetail, 0);
      }
      catch (Exception ex)
      {
        TaskChangeNotifier.NotifyTaskDateChanged(task.id);
      }
      return true;
    }

    public static async void TryDeleteRecurrence(
      string taskId,
      System.DateTime? recurringStart,
      System.DateTime? recurringEnd,
      IToastShowWindow toastWindow)
    {
      if (!ModifyRepeatHandler._inited)
        return;
      TaskModel taskById = await TaskDao.GetTaskById(taskId);
      if (taskById == null)
        return;
      ModifyRepeatHandler._toastWindow = toastWindow;
      ModifyRepeatHandler._deleteTask = taskById;
      List<TASK> tasks = new List<TASK>()
      {
        TASKDao.NewTASK(taskById, (List<TaskDetailItemModel>) null, (List<TaskReminderModel>) null, recurringStart ?? taskById.startDate, recurringEnd ?? taskById.dueDate)
      };
      try
      {
        ModifySingleRepeat.DeleteRecurrence(tasks, 1);
      }
      catch (Exception ex)
      {
        int num = (int) System.Windows.Forms.MessageBox.Show(ExceptionUtils.BuildExceptionMessage(ex));
      }
    }

    public static async Task<bool> CompleteOrSkipRecurrence(
      string taskId,
      System.DateTime? dateTime,
      IToastShowWindow toastWindow = null)
    {
      if (!dateTime.HasValue)
        return true;
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null || task.status != 0 || string.IsNullOrEmpty(task.repeatFlag) || !task.startDate.HasValue)
        return true;
      if (task.remindTime.HasValue)
      {
        task.remindTime = new System.DateTime?();
        await ReminderDelayDao.DeleteByIdAsync(task.id, "task");
      }
      System.DateTime date1 = dateTime.Value.Date;
      System.DateTime? startDate1 = task.startDate;
      System.DateTime date2 = startDate1.Value.Date;
      double totalDays = (date1 - date2).TotalDays;
      System.DateTime targetTzTime;
      if (!task.ParseTz)
      {
        startDate1 = task.startDate;
        targetTzTime = startDate1.Value;
      }
      else
      {
        startDate1 = task.startDate;
        targetTzTime = TimeZoneUtils.LocalToTargetTzTime(startDate1.Value, task.timeZone);
      }
      System.DateTime startDate2 = targetTzTime;
      List<string> list1 = !string.IsNullOrEmpty(task.exDates) ? ((IEnumerable<string>) ExDateSerilizer.ToArray(task.exDates)).ToList<string>() : (List<string>) null;
      List<System.DateTime> list2 = RepeatUtils.GetValidRepeatDates(task.repeatFlag, task.repeatFrom, startDate2, startDate2.Date, startDate2.AddDays(totalDays).Date, list1, task.timeZone, task.ParseTz).Where<System.DateTime>((Func<System.DateTime, bool>) (r => r.Date != dateTime.Value.Date)).ToList<System.DateTime>();
      if (list2.All<System.DateTime>((Func<System.DateTime, bool>) (d => d.Date != task.startDate.Value.Date)))
      {
        List<System.DateTime> dateTimeList = list2;
        startDate1 = task.startDate;
        System.DateTime dateTime1 = startDate1.Value;
        dateTimeList.Insert(0, dateTime1);
      }
      if (list2.Count == 0)
        return true;
      ModifyRepeatDialog modifyRepeatDialog = new ModifyRepeatDialog(list2);
      TaskDetailPopup.SetCanClose(false);
      modifyRepeatDialog.ShowDialog();
      TaskDetailPopup.SetCanClose(true);
      switch (modifyRepeatDialog.Selected)
      {
        case EditorType.COMPLETEALL:
          await ModifyRepeatHandler.CompleteAllTaskRecurrence(task, list2, dateTime.Value);
          break;
        case EditorType.SKIP:
          await ModifyRepeatHandler.SkipAllTaskRecurrence(task, list2, dateTime.Value, toastWindow);
          break;
        default:
          return false;
      }
      TaskChangeNotifier.NotifyBatchDateChanged(new List<string>()
      {
        taskId
      });
      return true;
    }

    private static async Task SkipAllTaskRecurrence(
      TaskModel task,
      List<System.DateTime> dates,
      System.DateTime endTime,
      IToastShowWindow toastWindow)
    {
      if (dates == null || dates.Count == 0)
        return;
      System.DateTime? nullable1 = task.startDate;
      if (!nullable1.HasValue)
        return;
      int num1 = 0;
      List<string> source = new List<string>();
      if (task.exDate != null)
        source = ((IEnumerable<string>) task.exDate).ToList<string>();
      nullable1 = task.startDate;
      if (nullable1.HasValue && source.Count > 0)
      {
        int result;
        List<int> list = source.Select<string, int>((Func<string, int>) (ex => !int.TryParse(ex, out result) ? 0 : result)).ToList<int>();
        nullable1 = task.startDate;
        int startDateNum = ticktick_WPF.Util.DateUtils.GetDateNum(nullable1.Value);
        int nextDateNum = ticktick_WPF.Util.DateUtils.GetDateNum(endTime);
        Func<int, bool> predicate = (Func<int, bool>) (num => num > startDateNum && num < nextDateNum && num != 0);
        num1 = list.Count<int>(predicate);
      }
      dates.Sort((Comparison<System.DateTime>) ((a, b) => a.CompareTo(b)));
      nullable1 = task.startDate;
      System.DateTime originalDate = nullable1.Value;
      System.DateTime date = dates.FirstOrDefault<System.DateTime>((Func<System.DateTime, bool>) (d => d.Date == endTime.Date));
      task.startDate = new System.DateTime?(!ticktick_WPF.Util.Utils.IsEmptyDate(date) ? date : ticktick_WPF.Util.DateUtils.SetHourAndMinuteOnly(endTime, originalDate.Hour, originalDate.Minute));
      nullable1 = task.dueDate;
      if (nullable1.HasValue)
      {
        TaskModel taskModel = task;
        nullable1 = task.startDate;
        System.DateTime dateTime = nullable1.Value;
        ref System.DateTime local = ref dateTime;
        nullable1 = task.dueDate;
        double totalMinutes = (nullable1.Value - originalDate).TotalMinutes;
        System.DateTime? nullable2 = new System.DateTime?(local.AddMinutes(totalMinutes));
        taskModel.dueDate = nullable2;
      }
      if (!string.IsNullOrEmpty(task.repeatFlag))
      {
        int num2 = dates.Count<System.DateTime>((Func<System.DateTime, bool>) (d => d.Date != endTime.Date));
        if (task.repeatFlag.Contains("FORGETTINGCURVE"))
          task.repeatFlag = RepeatUtils.GetNextEbbinghausCycle(task.repeatFlag, num2 + num1);
        if (task.repeatFlag.Contains("COUNT"))
          (toastWindow ?? (IToastShowWindow) App.Window).TryToastString((object) null, ticktick_WPF.Util.Utils.GetString("SkipCountToast"));
      }
      List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(task.id);
      if (checkItemsByTaskId != null && checkItemsByTaskId.Any<TaskDetailItemModel>())
      {
        nullable1 = task.startDate;
        double totalMinutes = (nullable1.Value - originalDate).TotalMinutes;
        List<TaskDetailItemModel> taskDetailItemModelList = new List<TaskDetailItemModel>();
        foreach (TaskDetailItemModel taskDetailItemModel1 in checkItemsByTaskId)
        {
          bool flag = false;
          if (taskDetailItemModel1.status != 0)
          {
            taskDetailItemModel1.status = 0;
            flag = true;
          }
          nullable1 = taskDetailItemModel1.startDate;
          if (nullable1.HasValue)
          {
            TaskDetailItemModel taskDetailItemModel2 = taskDetailItemModel1;
            nullable1 = taskDetailItemModel1.startDate;
            System.DateTime? nullable3 = new System.DateTime?(nullable1.Value.AddMinutes(totalMinutes));
            taskDetailItemModel2.startDate = nullable3;
            flag = true;
          }
          if (flag)
            taskDetailItemModelList.Add(taskDetailItemModel1);
        }
        if (taskDetailItemModelList.Any<TaskDetailItemModel>())
          await TaskDetailItemDao.BatchUpdateChecklists(taskDetailItemModelList);
      }
      await TaskService.UpdateTaskOnTimeChanged(task);
      TaskChangeNotifier.NotifyTaskDateChanged(task.id);
      await SyncStatusDao.AddModifySyncStatus(task.id);
    }

    private static async Task CompleteAllTaskRecurrence(
      TaskModel originalTask,
      List<System.DateTime> dates,
      System.DateTime endTime)
    {
      List<TaskModel> repeatTasks;
      if (dates == null)
        repeatTasks = (List<TaskModel>) null;
      else if (dates.Count == 0)
      {
        repeatTasks = (List<TaskModel>) null;
      }
      else
      {
        System.DateTime? nullable1 = originalTask.startDate;
        if (!nullable1.HasValue)
        {
          repeatTasks = (List<TaskModel>) null;
        }
        else
        {
          int num1 = 0;
          List<string> source = new List<string>();
          if (originalTask.exDate != null)
            source = ((IEnumerable<string>) originalTask.exDate).ToList<string>();
          nullable1 = originalTask.startDate;
          if (nullable1.HasValue && source.Count > 0)
          {
            int result;
            List<int> list = source.Select<string, int>((Func<string, int>) (ex => !int.TryParse(ex, out result) ? 0 : result)).ToList<int>();
            nullable1 = originalTask.startDate;
            int startDateNum = ticktick_WPF.Util.DateUtils.GetDateNum(nullable1.Value);
            int nextDateNum = ticktick_WPF.Util.DateUtils.GetDateNum(endTime);
            Func<int, bool> predicate = (Func<int, bool>) (num => num > startDateNum && num < nextDateNum && num != 0);
            num1 = list.Count<int>(predicate);
          }
          repeatTasks = new List<TaskModel>();
          long num2 = (ProjectSortOrderDao.GetNextTaskSortOrderInProject(originalTask.projectId, originalTask.sortOrder, originalTask.parentId) - originalTask.sortOrder) / (long) dates.Count;
          dates.Sort((Comparison<System.DateTime>) ((a, b) => a.CompareTo(b)));
          double num3 = 0.0;
          nullable1 = originalTask.startDate;
          System.DateTime dateTime1 = nullable1.Value;
          System.DateTime date1 = dates.FirstOrDefault<System.DateTime>((Func<System.DateTime, bool>) (d => d.Date == endTime.Date));
          originalTask.startDate = new System.DateTime?(!ticktick_WPF.Util.Utils.IsEmptyDate(date1) ? date1 : ticktick_WPF.Util.DateUtils.SetHourAndMinuteOnly(endTime, dateTime1.Hour, dateTime1.Minute));
          nullable1 = originalTask.dueDate;
          System.DateTime dateTime2;
          if (nullable1.HasValue)
          {
            nullable1 = originalTask.dueDate;
            num3 = (nullable1.Value - dateTime1).TotalMinutes;
            TaskModel taskModel = originalTask;
            nullable1 = originalTask.startDate;
            dateTime2 = nullable1.Value;
            System.DateTime? nullable2 = new System.DateTime?(dateTime2.AddMinutes(num3));
            taskModel.dueDate = nullable2;
          }
          for (int index = 0; index < dates.Count; ++index)
          {
            System.DateTime date2 = dates[index];
            if (!(date2.Date == endTime.Date))
            {
              TaskModel taskModel = new TaskModel(originalTask);
              taskModel.id = ticktick_WPF.Util.Utils.GetGuid();
              taskModel.status = 2;
              taskModel.repeatFlag = string.Empty;
              taskModel.completedTime = new System.DateTime?(System.DateTime.Now);
              taskModel.repeatTaskId = originalTask.id;
              taskModel.sortOrder = originalTask.sortOrder + (long) index * num2;
              taskModel.completedUserId = ticktick_WPF.Util.Utils.GetCurrentUserIdInt().ToString();
              taskModel.startDate = new System.DateTime?(date2);
              nullable1 = originalTask.dueDate;
              System.DateTime? nullable3;
              if (nullable1.HasValue)
              {
                nullable3 = new System.DateTime?(date2.AddMinutes(num3));
              }
              else
              {
                nullable1 = new System.DateTime?();
                nullable3 = nullable1;
              }
              taskModel.dueDate = nullable3;
              repeatTasks.Add(taskModel);
            }
          }
          if (!string.IsNullOrEmpty(originalTask.repeatFlag))
          {
            int num4 = RepeatUtils.GetRepeatCount(originalTask.repeatFlag) - repeatTasks.Count;
            if (originalTask.repeatFlag.Contains("FORGETTINGCURVE"))
              originalTask.repeatFlag = RepeatUtils.GetNextEbbinghausCycle(originalTask.repeatFlag, repeatTasks.Count + num1);
            if (num4 > 0)
            {
              TaskModel taskModel = originalTask;
              string repeatFlag1 = originalTask.repeatFlag;
              dateTime2 = new System.DateTime();
              System.DateTime until = dateTime2;
              int count = num4;
              string repeatFlag2 = RepeatUtils.GetRepeatFlag(repeatFlag1, until, count);
              taskModel.repeatFlag = repeatFlag2;
            }
          }
          nullable1 = originalTask.remindTime;
          if (nullable1.HasValue)
          {
            TaskModel taskModel = originalTask;
            nullable1 = new System.DateTime?();
            System.DateTime? nullable4 = nullable1;
            taskModel.remindTime = nullable4;
            await ReminderDelayDao.DeleteByIdAsync(originalTask.id, "task");
          }
          originalTask.status = 0;
          originalTask.progress = new int?();
          TaskModel repeatTask;
          for (int i = 0; i < repeatTasks.Count; ++i)
          {
            repeatTask = await TaskDao.InsertTask(repeatTasks[i]);
            repeatTasks[i] = repeatTask;
            SyncStatusDao.AddSyncStatus(repeatTasks[i].id, 4);
            if (!string.IsNullOrEmpty(repeatTask.parentId))
              SyncStatusDao.AddSetParentSyncStatus(repeatTask.id, "");
            await TaskService.TryCloseAndCopySubTask(originalTask, repeatTask, false, 2);
            await ProjectCopyManager.CopyChecklistItems(originalTask.id, repeatTask.id);
            List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(originalTask.id);
            TaskModel taskModel = repeatTask;
            taskModel.reminders = await ticktick_WPF.Util.Utils.CopyReminderItem(repeatTask, remindersByTaskId.ToArray());
            taskModel = (TaskModel) null;
            taskModel = repeatTask;
            taskModel.Attachments = await ticktick_WPF.Util.Utils.CopyAttachmentItem(repeatTask, originalTask.id);
            taskModel = (TaskModel) null;
            if (i == 0)
            {
              taskModel = repeatTask;
              taskModel.FocusSummaries = await ticktick_WPF.Util.Utils.CopyPomoSummaries(repeatTask, originalTask.id);
              taskModel = (TaskModel) null;
              await PomoSummaryDao.CleanPomosByTaskId(originalTask.id);
              await TickFocusManager.NotifyPomoTaskChanged(originalTask.id, repeatTask.id);
            }
            await TaskDao.UpdateTask(repeatTask);
            repeatTask = (TaskModel) null;
          }
          if (!string.IsNullOrEmpty(originalTask.parentId))
            TaskDao.AddOrRemoveTaskChildIds(originalTask.parentId, repeatTasks.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)).ToList<string>(), true);
          if (originalTask.kind == "CHECKLIST")
          {
            repeatTask = originalTask;
            repeatTask.progress = new int?(await TaskService.CalculateProgress(originalTask.id));
            repeatTask = (TaskModel) null;
          }
          await TaskService.UpdateTask(originalTask);
          await SyncStatusDao.AddModifySyncStatus(originalTask.id);
          repeatTasks = (List<TaskModel>) null;
        }
      }
    }

    public static async void DelayCollect(object obj) => await Task.Delay(5000);
  }
}
