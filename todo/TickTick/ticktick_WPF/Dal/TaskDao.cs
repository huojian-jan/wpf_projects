// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TaskDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Service.Display;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Summary;
using ticktick_WPF.Views.Tag;
using ticktick_WPF.Views.Undo;
using TickTickDao;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class TaskDao : BaseDao<TaskModel>
  {
    public static async Task<ObservableCollection<TaskModel>> GetTasksInTaskIds(List<string> taskIds)
    {
      if (taskIds == null)
        return (ObservableCollection<TaskModel>) null;
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return new ObservableCollection<TaskModel>((await App.Connection.Table<TaskModel>().Where((Expression<Func<TaskModel, bool>>) (v => v.userId == userId)).Where((Expression<Func<TaskModel, bool>>) (v => v.deleted == 0)).ToListAsync()).Where<TaskModel>((Func<TaskModel, bool>) (v => taskIds.Contains(v.id))));
    }

    public static async Task<ObservableCollection<TaskModel>> GetEmptyColumnTaskInProject(
      string projectId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return new ObservableCollection<TaskModel>(await App.Connection.Table<TaskModel>().Where((Expression<Func<TaskModel, bool>>) (v => v.userId == userId && v.projectId == projectId && v.columnId == "" && v.status != 0)).ToListAsync());
    }

    public static async Task<ObservableCollection<TaskModel>> GetTasksInProjectIds(
      IEnumerable<string> projectIds)
    {
      ObservableCollection<TaskModel> result = new ObservableCollection<TaskModel>();
      foreach (string projectId in projectIds)
      {
        foreach (TaskModel taskModel in await TaskDao.GetTasksInProjectAsync(projectId))
          result.Add(taskModel);
      }
      ObservableCollection<TaskModel> tasksInProjectIds = result;
      result = (ObservableCollection<TaskModel>) null;
      return tasksInProjectIds;
    }

    public static async Task<TaskModel> GetFullTaskById(string id, bool checkTextCount = false)
    {
      TaskModel taskModel = await TaskDao.GetTaskById(id);
      if (taskModel != null)
      {
        taskModel.content = taskModel.content?.Replace("---\r\n", "---\r");
        if (checkTextCount)
        {
          string title = taskModel.title;
          if ((title != null ? (title.Length > 204800 ? 1 : 0) : 0) != 0)
          {
            UserActCollectUtils.SendCustomException("TitleOverLimit " + taskModel.title.Length.ToString());
            taskModel.title = taskModel.title.Substring(0, 204800);
          }
          string content = taskModel.content;
          if ((content != null ? (content.Length > 204800 ? 1 : 0) : 0) != 0)
          {
            UserActCollectUtils.SendCustomException("ContentOverLimit " + taskModel.content.Length.ToString());
            taskModel.content = taskModel.content.Substring(0, 204800);
          }
          string desc = taskModel.desc;
          if ((desc != null ? (desc.Length > 204800 ? 1 : 0) : 0) != 0)
          {
            UserActCollectUtils.SendCustomException("DescOverLimit " + taskModel.desc.Length.ToString());
            taskModel.desc = taskModel.desc.Substring(0, 204800);
          }
        }
        taskModel.exDate = !string.IsNullOrEmpty(taskModel.exDates) ? ExDateSerilizer.ToArray(taskModel.exDates) : new string[0];
        TaskModel taskModel1 = await TaskDao.AssembleFullTask(taskModel, checkTextCount);
      }
      TaskModel fullTaskById = taskModel;
      taskModel = (TaskModel) null;
      return fullTaskById;
    }

    public static async Task<TaskModel> GetTaskById(string taskId)
    {
      if (string.IsNullOrEmpty(taskId))
        return (TaskModel) null;
      string userId = LocalSettings.Settings.LoginUserId;
      return await App.Connection.Table<TaskModel>().Where((Expression<Func<TaskModel, bool>>) (v => v.id.Equals(taskId) && v.userId == userId)).FirstOrDefaultAsync();
    }

    public static async Task<TaskModel> GetThinTaskById(string taskId)
    {
      return await TaskDao.GetTaskById(taskId);
    }

    public static async Task<List<TaskModel>> GetTasksByIds(List<string> taskIds)
    {
      if (taskIds == null || !taskIds.Any<string>())
        return new List<TaskModel>();
      List<string> list = taskIds.ToList<string>();
      List<TaskModel> result = new List<TaskModel>();
      foreach (string taskId in list)
      {
        TaskModel taskById = await TaskDao.GetTaskById(taskId);
        if (taskById != null)
          result.Add(taskById);
      }
      return result;
    }

    public static async Task<List<TaskModel>> GetThinTasksInBatch(List<string> taskIds)
    {
      taskIds = taskIds.ToList<string>();
      return await TaskDao.GetTasksByIds(taskIds);
    }

    public static async Task<Dictionary<string, TaskEtag>> GetTask2SidMap(List<string> ids)
    {
      string sql = "select _Id as _Id , id as TaskId, etag as Etag from TaskModel where id in ({0}) and userId = '" + LocalSettings.Settings.LoginUserId + "'";
      Dictionary<string, TaskEtag> result = new Dictionary<string, TaskEtag>();
      List<string> ids1 = ids;
      foreach (TaskEtag taskEtag in await SqliteUtils.BatchGetModelsAsync<TaskEtag>(sql, ids1))
      {
        if (!string.IsNullOrEmpty(taskEtag.TaskId) && !result.ContainsKey(taskEtag.TaskId))
          result[taskEtag.TaskId] = taskEtag;
      }
      Dictionary<string, TaskEtag> task2SidMap = result;
      result = (Dictionary<string, TaskEtag>) null;
      return task2SidMap;
    }

    private static async Task<List<string>> GetTaskIds() => TaskCache.GetAllTaskIds();

    public static async Task BatchCreateTaskFromRemote(List<TaskModel> tasks, bool needHandleItem)
    {
      List<string> localTaskIds = await TaskDao.GetTaskIds();
      List<TaskModel> list = tasks.Where<TaskModel>((Func<TaskModel, bool>) (task => !localTaskIds.Contains(task.id))).ToList<TaskModel>();
      List<TaskReminderModel> reminderList = new List<TaskReminderModel>();
      List<TaskDetailItemModel> checklistItems = new List<TaskDetailItemModel>();
      List<AttachmentModel> attachmentList = new List<AttachmentModel>();
      List<PomodoroSummaryModel> pomos = new List<PomodoroSummaryModel>();
      List<LocationModel> locations = new List<LocationModel>();
      foreach (TaskModel taskModel1 in list)
      {
        TaskModel taskModel2 = taskModel1;
        int index = Utils.GetCurrentUserIdInt();
        string str1 = index.ToString();
        taskModel2.userId = str1;
        TaskDao.HandleServerDate(taskModel1);
        if (taskModel1.reminders != null)
        {
          TaskReminderModel[] reminders = taskModel1.reminders;
          for (index = 0; index < reminders.Length; ++index)
          {
            TaskReminderModel taskReminderModel = reminders[index];
            taskReminderModel.Taskid = taskModel1._Id;
            taskReminderModel.taskserverid = taskModel1.id;
          }
          reminderList.AddRange((IEnumerable<TaskReminderModel>) taskModel1.reminders);
        }
        if (taskModel1.items != null)
        {
          TaskDetailItemModel[] items = taskModel1.items;
          for (index = 0; index < items.Length; ++index)
          {
            TaskDetailItemModel taskDetailItemModel = items[index];
            taskDetailItemModel.TaskId = taskModel1._Id;
            taskDetailItemModel.TaskServerId = taskModel1.id;
            if (needHandleItem)
              TaskDao.HandleItemServerDate(taskDetailItemModel, taskModel1.timeZone, taskModel1.Floating);
          }
          checklistItems.AddRange((IEnumerable<TaskDetailItemModel>) taskModel1.items);
        }
        if (taskModel1.Attachments != null)
        {
          AttachmentModel[] attachments = taskModel1.Attachments;
          for (index = 0; index < attachments.Length; ++index)
          {
            AttachmentModel attachmentModel = attachments[index];
            attachmentModel.taskId = taskModel1.id;
            attachmentModel.sync_status = 2.ToString();
          }
          attachmentList.AddRange((IEnumerable<AttachmentModel>) taskModel1.Attachments);
        }
        if (taskModel1.FocusSummaries != null)
        {
          PomodoroSummaryModel[] focusSummaries = taskModel1.FocusSummaries;
          for (index = 0; index < focusSummaries.Length; ++index)
          {
            PomodoroSummaryModel pomodoroSummaryModel = focusSummaries[index];
            pomodoroSummaryModel.id = Utils.GetGuid();
            pomodoroSummaryModel.taskId = taskModel1.id;
          }
          pomos.AddRange((IEnumerable<PomodoroSummaryModel>) taskModel1.FocusSummaries);
        }
        if (taskModel1.tags != null)
          taskModel1.tag = TagSerializer.ToJsonContent(((IEnumerable<string>) taskModel1.tags).ToList<string>());
        if (taskModel1.childIds != null)
          taskModel1.childrenString = JsonConvert.SerializeObject((object) taskModel1.childIds);
        if (taskModel1.exDate != null)
          taskModel1.exDates = ExDateSerilizer.ToString(taskModel1.exDate);
        if (taskModel1.location != null)
        {
          taskModel1.hasLocation = 1;
          LocationModel location = taskModel1.location;
          index = Utils.GetCurrentUserIdInt();
          string str2 = index.ToString();
          location.userId = str2;
          taskModel1.location.taskId = taskModel1.id;
          taskModel1.location.latitude = taskModel1.location.loc.latitude;
          taskModel1.location.longitude = taskModel1.location.loc.longitude;
          locations.Add(taskModel1.location);
        }
      }
      await TaskDao.BatchInsertTasks(list);
      if (attachmentList.Count > 0)
      {
        int num = await App.Connection.InsertAllAsync((IEnumerable) attachmentList);
        AttachmentCache.ResetDictItems();
      }
      if (reminderList.Count > 0)
      {
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) reminderList);
      }
      if (checklistItems.Count > 0)
        await TaskDetailItemDao.BatchInsertChecklists(checklistItems);
      if (pomos.Count > 0)
      {
        int num2 = await App.Connection.InsertAllAsync((IEnumerable) pomos);
      }
      if (locations.Count <= 0)
      {
        reminderList = (List<TaskReminderModel>) null;
        checklistItems = (List<TaskDetailItemModel>) null;
        attachmentList = (List<AttachmentModel>) null;
        pomos = (List<PomodoroSummaryModel>) null;
        locations = (List<LocationModel>) null;
      }
      else
      {
        int num3 = await App.Connection.InsertAllAsync((IEnumerable) locations);
        reminderList = (List<TaskReminderModel>) null;
        checklistItems = (List<TaskDetailItemModel>) null;
        attachmentList = (List<AttachmentModel>) null;
        pomos = (List<PomodoroSummaryModel>) null;
        locations = (List<LocationModel>) null;
      }
    }

    private static void HandleItemServerDate(
      TaskDetailItemModel item,
      string timeZone,
      bool isFloating)
    {
      if (((!item.isAllDay.HasValue ? 0 : (item.isAllDay.Value ? 1 : 0)) | (isFloating ? 1 : 0)) == 0 || !item.startDate.HasValue)
        return;
      item.startDate = new DateTime?(DateUtils.ConvertTimeZone(item.startDate.Value, timeZone));
    }

    public static void HandleServerDate(TaskModel item)
    {
      if ((!item.isAllDay.HasValue || !item.isAllDay.Value) && !item.Floating)
        return;
      if (item.startDate.HasValue)
        item.startDate = new DateTime?(DateUtils.ConvertTimeZone(item.startDate.Value, item.timeZone));
      if (!item.dueDate.HasValue)
        return;
      item.dueDate = new DateTime?(DateUtils.ConvertTimeZone(item.dueDate.Value, item.timeZone));
    }

    public static async Task BatchInsertTasksAndItems(List<TaskModel> tasks)
    {
      if (tasks == null || !tasks.Any<TaskModel>())
        return;
      List<TaskDetailItemModel> checkListItems = new List<TaskDetailItemModel>();
      List<AttachmentModel> attachments = new List<AttachmentModel>();
      List<TaskReminderModel> reminderList = new List<TaskReminderModel>();
      foreach (TaskModel task in tasks)
      {
        if (task.items != null)
        {
          foreach (TaskDetailItemModel taskDetailItemModel in task.items)
            taskDetailItemModel.TaskServerId = task.id;
          checkListItems.AddRange((IEnumerable<TaskDetailItemModel>) task.items);
        }
        if (task.Attachments != null)
        {
          foreach (AttachmentModel attachment in task.Attachments)
            attachment.taskId = task.id;
          attachments.AddRange((IEnumerable<AttachmentModel>) task.Attachments);
        }
        if (task.reminders != null)
        {
          foreach (TaskReminderModel reminder in task.reminders)
            reminder.taskserverid = task.id;
          reminderList.AddRange((IEnumerable<TaskReminderModel>) task.reminders);
        }
      }
      int num1 = await App.Connection.InsertAllAsync((IEnumerable) tasks);
      List<TaskBaseViewModel> vms = TaskCache.InsertAll(tasks);
      if (attachments.Count > 0)
      {
        int num2 = await App.Connection.InsertAllAsync((IEnumerable) attachments);
      }
      if (reminderList.Count > 0)
      {
        int num3 = await App.Connection.InsertAllAsync((IEnumerable) reminderList);
      }
      if (checkListItems.Count > 0)
        await TaskDetailItemDao.BatchInsertChecklists(checkListItems);
      ProjectAndTaskIdsCache.OnTasksChanged(vms, CheckMatchedType.All);
      await SyncStatusDao.BatchAddCreateTaskStatus(tasks.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)));
      checkListItems = (List<TaskDetailItemModel>) null;
      attachments = (List<AttachmentModel>) null;
      reminderList = (List<TaskReminderModel>) null;
      vms = (List<TaskBaseViewModel>) null;
    }

    public static async Task BatchInsertTasks(List<TaskModel> tasks)
    {
      if (tasks == null || !tasks.Any<TaskModel>())
        return;
      int num = await App.Connection.InsertAllAsync((IEnumerable) tasks);
      ProjectAndTaskIdsCache.OnTasksChanged(TaskCache.InsertAll(tasks), CheckMatchedType.All);
    }

    public static async Task<TaskModel> InsertTask(TaskModel task)
    {
      DateTime? startDate = task.startDate;
      if (startDate.HasValue)
      {
        TaskModel taskModel = task;
        startDate = task.startDate;
        DateTime? nullable = new DateTime?(startDate.Value);
        taskModel.startDate = nullable;
      }
      if (task.sortOrder == 0L)
        task.sortOrder = ProjectSortOrderDao.GetNewTaskSortOrderInProject(task.projectId, task.parentId);
      if (string.IsNullOrEmpty(task.userId))
        task.userId = Utils.GetCurrentUserIdInt().ToString();
      if (await TaskDao.GetTaskById(task.id) == null)
        await TaskDao.InsertTaskToDb(task);
      return task;
    }

    private static async Task InsertTaskToDb(TaskModel task)
    {
      int num = await App.Connection.InsertAsync((object) task);
      TaskCache.Insert(task);
    }

    public static async Task<List<SyncStatusModel>> GetNeedPostTasks(int type)
    {
      return await App.Connection.QueryAsync<SyncStatusModel>(string.Format("select * from SyncStatusModel where type = {0} and userId = '{1}'", (object) type, (object) LocalSettings.Settings.LoginUserId));
    }

    public static async Task<List<string>> GetDeletedIds() => TaskCache.GetDeletedTaskIds();

    public static async Task<List<SyncStatusModel>> GetNeedPostMovedOrRestoreTasks(
      int movedOrRestore)
    {
      return await TaskDao.GetNeedPostTasks(movedOrRestore);
    }

    public static async Task DeleteTaskToTrash(string taskId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      if (thinTaskById.deleted != 1)
      {
        thinTaskById.deleted = 1;
        thinTaskById.attendId = (string) null;
        thinTaskById.modifiedTime = new DateTime?(DateTime.Now);
      }
      await TaskService.UpdateTaskOnDeletedChanged(thinTaskById);
    }

    public static async Task DeleteTaskForever(string taskId)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      if (thinTaskById.deleted != 2)
      {
        thinTaskById.deleted = 2;
        thinTaskById.modifiedTime = new DateTime?(DateTime.Now);
      }
      await TaskService.UpdateTaskOnDeletedChanged(thinTaskById);
    }

    public static async Task BatchUpdateTasksFromRemote(TaskSyncBean taskSyncBean)
    {
      TaskSyncBean taskSyncBean1 = taskSyncBean;
      int num1;
      if (taskSyncBean1 == null)
      {
        num1 = 0;
      }
      else
      {
        int? count = taskSyncBean1.Updated?.Count;
        int num2 = 0;
        num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
      }
      if (num1 != 0)
      {
        List<string> changeParentIds = await SyncStatusDao.GetEntityIdsByType(16) ?? new List<string>();
        List<SyncStatusModel> moveStatus = await SyncStatusDao.GetSyncStatusByType(2);
        foreach (TaskModel taskModel in taskSyncBean.Updated)
        {
          TaskModel task = taskModel;
          if (task != null)
          {
            TaskModel local = await TaskDao.GetThinTaskById(task.id);
            if (local != null)
            {
              if (changeParentIds.Contains(task.id))
                task.parentId = local.parentId;
              List<SyncStatusModel> source = moveStatus;
              SyncStatusModel model = source != null ? source.FirstOrDefault<SyncStatusModel>((Func<SyncStatusModel, bool>) (m => m.EntityId == local.id)) : (SyncStatusModel) null;
              if (model != null)
              {
                if (model.MoveFromId != task.projectId)
                {
                  model.MoveFromId = task.projectId;
                  int num3 = await BaseDao<SyncStatusModel>.UpdateAsync(model);
                }
                task.projectId = local.projectId;
              }
              if (TaskDao.IsTaskSame(local, task))
                continue;
            }
            await TaskDao.SaveModifiedTask(task);
            task = (TaskModel) null;
          }
        }
        changeParentIds = (List<string>) null;
        moveStatus = (List<SyncStatusModel>) null;
      }
      TaskSyncBean taskSyncBean2 = taskSyncBean;
      int num4;
      if (taskSyncBean2 == null)
      {
        num4 = 0;
      }
      else
      {
        int? count = taskSyncBean2.Updating?.Count;
        int num5 = 0;
        num4 = count.GetValueOrDefault() > num5 & count.HasValue ? 1 : 0;
      }
      if (num4 == 0)
        return;
      foreach (TaskModel task in taskSyncBean.Updating)
        await TaskDao.SaveMergedTask(task);
    }

    private static async Task SaveMergedTask(TaskModel task)
    {
      if (task == null)
        return;
      if (task.tags != null)
      {
        ProjectModel projectById = CacheManager.GetProjectById(task.projectId);
        await TagDao.TryBatchAddTags(((IEnumerable<string>) task.tags).ToList<string>(), projectById != null && projectById.IsShareList());
      }
      await TaskDao.SaveTaskReminders(task);
      await TaskDao.SaveCheckItems(task);
      task.modifiedTime = new DateTime?(DateTime.Now);
      if (task.childIds != null)
        task.childrenString = JsonConvert.SerializeObject((object) task.childIds);
      await TaskService.UpdateTask(task);
      if (await UserDao.GetUserSyncCheckPoint(LocalSettings.Settings.LoginUserId) == 0L)
        return;
      await SyncStatusDao.AddSyncStatus(task.id, 0);
    }

    public static async Task SaveCheckItems(TaskModel task)
    {
      if (task.items == null)
        return;
      List<TaskDetailItemModel> taskDetailItemModelList = await TaskDetailItemDao.DeleteCheckItemsByTaskId(task.id, ((IEnumerable<TaskDetailItemModel>) task.items).Select<TaskDetailItemModel, string>((Func<TaskDetailItemModel, string>) (i => i.id)).ToList<string>());
      TaskDetailItemModel[] taskDetailItemModelArray = task.items;
      for (int index = 0; index < taskDetailItemModelArray.Length; ++index)
      {
        TaskDetailItemModel model = taskDetailItemModelArray[index];
        model.TaskId = task._Id;
        model.TaskServerId = task.id;
        await TaskDetailItemDao.TrySaveChecklistItem(model);
      }
      taskDetailItemModelArray = (TaskDetailItemModel[]) null;
    }

    private static async Task SaveTaskReminders(TaskModel task)
    {
      DateTime? startDate;
      if (task.reminders != null && task.reminders.Length != 0)
      {
        startDate = task.startDate;
        if (startDate.HasValue)
          goto label_3;
      }
      startDate = task.startDate;
      if (startDate.HasValue || task.reminders != null)
      {
        if (task.reminders == null || task.reminders.Length != 0)
          return;
        int num = await TaskReminderDao.DeleteRemindersByTaskId(task.id) ? 1 : 0;
        return;
      }
label_3:
      int num1 = await TaskReminderDao.DeleteRemindersByTaskId(task.id) ? 1 : 0;
      if (task.reminders == null)
        return;
      TaskReminderModel[] taskReminderModelArray = task.reminders;
      for (int index = 0; index < taskReminderModelArray.Length; ++index)
      {
        TaskReminderModel taskReminderModel = taskReminderModelArray[index];
        taskReminderModel.Taskid = task._Id;
        taskReminderModel.taskserverid = task.id;
        int num2 = await TaskReminderDao.SaveReminders(taskReminderModel);
      }
      taskReminderModelArray = (TaskReminderModel[]) null;
    }

    private static bool IsTaskSame(TaskModel local, TaskModel server)
    {
      return !(local.etag != server.etag) && local.status == server.status && !(local.projectId != server.projectId) && !(local.parentId != server.parentId);
    }

    private static async Task SaveModifiedTask(TaskModel task)
    {
      if (task.tags != null)
      {
        task.tag = TagSerializer.ToJsonContent(((IEnumerable<string>) task.tags).ToList<string>());
        ProjectModel projectById = CacheManager.GetProjectById(task.projectId);
        await TagDao.TryBatchAddTags(((IEnumerable<string>) task.tags).ToList<string>(), projectById != null && projectById.IsShareList());
      }
      if (task.exDate != null)
        task.exDates = ExDateSerilizer.ToString(task.exDate);
      if (task.childIds != null)
        task.childrenString = JsonConvert.SerializeObject((object) task.childIds);
      try
      {
        TaskDao.HandleServerDate(task);
      }
      catch (Exception ex)
      {
      }
      await TaskDao.SaveTaskReminders(task);
      await TaskDao.SaveCheckItems(task);
      await TaskDao.SavePomodoroSummaries(task);
      await TaskDao.SaveTaskLocation(task);
      task.modifiedTime = new DateTime?(DateTime.Now);
      await TaskService.UpdateTask(task);
    }

    private static async Task SaveTaskLocation(TaskModel task)
    {
      if (task.location != null)
      {
        task.hasLocation = 1;
        task.location.taskId = task.id;
        task.location.userId = Utils.GetCurrentUserIdInt().ToString();
        task.location.longitude = task.location.loc.longitude;
        task.location.latitude = task.location.loc.latitude;
        await LocationDao.InsertLocation(task.location);
      }
      else
        task.hasLocation = 0;
    }

    public static async Task SavePomodoroSummaries(TaskModel task)
    {
      if (task.FocusSummaries == null)
        return;
      await PomoSummaryDao.DeletePomosByTaskId(task.id);
      foreach (PomodoroSummaryModel focusSummary in task.FocusSummaries)
      {
        focusSummary.id = Utils.GetGuid();
        focusSummary.taskId = task.id;
        focusSummary.userId = LocalSettings.Settings.LoginUserId;
      }
      await PomoSummaryDao.SavePomoSummaries(((IEnumerable<PomodoroSummaryModel>) task.FocusSummaries).ToList<PomodoroSummaryModel>());
    }

    private static string GetDateSql(IEnumerable<string> dates, string start = "c.startDate", string end = "c.dueDate")
    {
      string str1 = string.Empty;
      foreach (string date1 in dates)
      {
        DateTime dateTime;
        if (date1 != null)
        {
          switch (date1.Length)
          {
            case 3:
              if (date1 == "all")
              {
                string str2 = str1;
                dateTime = DateTime.Now;
                DateTime date2 = dateTime.Date;
                dateTime = DateTime.Now;
                dateTime = dateTime.Date;
                DateTime end1 = dateTime.AddDays(180.0);
                string startColum = start;
                string endColumn = end;
                string str3 = TaskDao.AppendDateBetweenSql(date2, end1, startColum, endColumn);
                str1 = str2 + str3;
                continue;
              }
              break;
            case 5:
              switch (date1[0])
              {
                case 'n':
                  if (date1 == "nodue")
                  {
                    str1 = str1 + "(" + start + " is null) or";
                    continue;
                  }
                  break;
                case 't':
                  if (date1 == "today")
                  {
                    string str4 = str1;
                    dateTime = DateTime.Now;
                    DateTime date3 = dateTime.Date;
                    dateTime = DateTime.Now;
                    dateTime = dateTime.Date;
                    DateTime end2 = dateTime.AddDays(1.0);
                    string startColum = start;
                    string endColumn = end;
                    string str5 = TaskDao.AppendDateBetweenSql(date3, end2, startColum, endColumn);
                    str1 = str4 + str5;
                    continue;
                  }
                  break;
              }
              break;
            case 7:
              if (date1 == "overdue")
              {
                str1 += TaskDao.AppendOverdueSql();
                continue;
              }
              break;
            case 8:
              switch (date1[1])
              {
                case 'e':
                  if (date1 == "nextweek")
                  {
                    int nextWeekDayDiff = DateUtils.GetNextWeekDayDiff();
                    string str6 = str1;
                    dateTime = DateTime.Now;
                    dateTime = dateTime.Date;
                    DateTime start1 = dateTime.AddDays((double) nextWeekDayDiff);
                    dateTime = DateTime.Now;
                    dateTime = dateTime.Date;
                    DateTime end3 = dateTime.AddDays((double) (nextWeekDayDiff + 7));
                    string startColum = start;
                    string endColumn = end;
                    string str7 = TaskDao.AppendDateBetweenSql(start1, end3, startColum, endColumn);
                    str1 = str6 + str7;
                    continue;
                  }
                  break;
                case 'h':
                  if (date1 == "thisweek")
                  {
                    int nextWeekDayDiff = DateUtils.GetNextWeekDayDiff();
                    string str8 = str1;
                    dateTime = DateTime.Now;
                    dateTime = dateTime.Date;
                    DateTime start2 = dateTime.AddDays((double) (nextWeekDayDiff - 7));
                    dateTime = DateTime.Now;
                    dateTime = dateTime.Date;
                    DateTime end4 = dateTime.AddDays((double) nextWeekDayDiff);
                    string startColum = start;
                    string endColumn = end;
                    string str9 = TaskDao.AppendDateBetweenSql(start2, end4, startColum, endColumn);
                    str1 = str8 + str9;
                    continue;
                  }
                  break;
                case 'o':
                  if (date1 == "tomorrow")
                  {
                    string str10 = str1;
                    dateTime = DateTime.Now;
                    dateTime = dateTime.Date;
                    DateTime start3 = dateTime.AddDays(1.0);
                    dateTime = DateTime.Now;
                    dateTime = dateTime.Date;
                    DateTime end5 = dateTime.AddDays(2.0);
                    string startColum = start;
                    string endColumn = end;
                    string str11 = TaskDao.AppendDateBetweenSql(start3, end5, startColum, endColumn);
                    str1 = str10 + str11;
                    continue;
                  }
                  break;
              }
              break;
            case 9:
              if (date1 == "thismonth")
              {
                dateTime = DateTime.Now;
                dateTime = dateTime.Date;
                DateTime start4 = dateTime.AddDays((double) ((DateTime.Now.Day - 1) * -1));
                dateTime = start4.Date;
                DateTime end6 = dateTime.AddMonths(1);
                str1 += TaskDao.AppendDateBetweenSql(start4, end6, start, end);
                continue;
              }
              break;
          }
        }
        if (date1.EndsWith("days"))
        {
          int result;
          int.TryParse(date1.Replace("days", string.Empty), out result);
          string str12 = str1;
          dateTime = DateTime.Now;
          DateTime date4 = dateTime.Date;
          dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          DateTime end7 = dateTime.AddDays((double) result);
          string startColum = start;
          string endColumn = end;
          string str13 = TaskDao.AppendDateBetweenSql(date4, end7, startColum, endColumn);
          str1 = str12 + str13;
        }
        else if (date1.EndsWith("dayslater"))
        {
          int result;
          int.TryParse(date1.Replace("dayslater", string.Empty), out result);
          string str14 = str1;
          string str15 = start;
          string str16 = start;
          dateTime = DateTime.Now;
          dateTime = dateTime.Date;
          // ISSUE: variable of a boxed type
          __Boxed<DateTime> local = (ValueType) dateTime.AddDays((double) result);
          string str17 = string.Format("({0} is not null and {1} >= '{2:yyyy-MM-dd 00:00:00}') or", (object) str15, (object) str16, (object) local);
          str1 = str14 + str17;
        }
      }
      if (str1.EndsWith("or"))
        str1 = str1.Substring(0, str1.Length - 2);
      return " (" + str1 + ") ";
    }

    private static string AppendOverdueSql()
    {
      DateTime dateTime = DateTime.Now;
      // ISSUE: variable of a boxed type
      __Boxed<DateTime> date = (ValueType) dateTime.Date;
      dateTime = DateTime.Now;
      dateTime = dateTime.Date;
      // ISSUE: variable of a boxed type
      __Boxed<DateTime> local = (ValueType) dateTime.AddDays(1.0);
      return string.Format(" ((c.startDate is not null and c.dueDate is null and c.startDate < '{0:yyyy-MM-dd 00:00:00}') or  (c.dueDate is not null and c.isAllday = 1 and c.dueDate < '{1:yyyy-MM-dd 00:00:00}' ) or (c.dueDate is not null and c.isAllday = 0 and c.dueDate < '{0:yyyy-MM-dd 00:00:00}' )) or", (object) date, (object) local);
    }

    private static string AppendDateBetweenSql(
      DateTime start,
      DateTime end,
      string startColum = "c.startDate",
      string endColumn = "c.dueDate")
    {
      return string.Format(" ((({2} is not null and {2} >= '{0}' and {2} < '{1}')) or (({3} is not null and {3} > '{0}' and {3} < '{1}')) or ({2} is not null and {3} is not null and {2} < '{0}' and {3} > '{1}')) or", (object) start.ToString("yyyy-MM-dd 00:00:00"), (object) end.AddDays(-1.0).ToString("yyyy-MM-dd 23:59:59"), (object) startColum, (object) endColumn);
    }

    private static string GetAssigneeSql(IEnumerable<string> assignees)
    {
      string str = string.Empty;
      string loginUserId = LocalSettings.Settings.LoginUserId;
      foreach (string assignee in assignees)
      {
        switch (assignee)
        {
          case "me":
            str = str + " c.assignee == '" + loginUserId + "' or";
            continue;
          case "other":
            str = str + " (c.c.assignee <> '" + loginUserId + "' and c.assignee <> '-1' and c.assignee <> '' and c.assignee is not null and d.userCount>1) or";
            continue;
          case "noassignee":
            str += " c.assignee is null or c.assignee = '-1' or c.assignee = '' or";
            continue;
          default:
            continue;
        }
      }
      if (str.EndsWith("or"))
        str = str.Substring(0, str.Length - 2);
      return " (" + str + ") ";
    }

    private static string GetProjectOrGroupSql(
      IReadOnlyList<string> groupIds,
      IReadOnlyList<string> projectIds)
    {
      if (projectIds.Count > 0 && groupIds.Count == 0)
        return TaskDao.GetProjectSql(projectIds);
      if (projectIds.Count == 0 && groupIds.Count > 0)
        return TaskDao.GetGroupSql(groupIds);
      return " (" + TaskDao.GetProjectSql(projectIds) + " or " + TaskDao.GetGroupSql(groupIds) + " ) ";
    }

    private static string GetGroupSql(IReadOnlyList<string> groupIds)
    {
      return TaskDao.GetItemsSql<string>(groupIds, "d.groupId");
    }

    private static string GetProjectSql(IReadOnlyList<string> projectIds)
    {
      return TaskDao.GetItemsSql<string>(projectIds, "c.projectId");
    }

    private static string GetPrioritySql(IReadOnlyList<int> priorities)
    {
      return TaskDao.GetItemsSql<int>(priorities, "c.priority");
    }

    private static string GetItemsSql<T>(IReadOnlyList<T> items, string column)
    {
      string str = string.Empty;
      for (int index = 0; index < items.Count; ++index)
      {
        T obj = items[index];
        str = !((object) obj is int) ? str + "'" + obj?.ToString() + "'" : str + obj?.ToString();
        if (index < items.Count - 1)
          str += ",";
      }
      return " " + column + " in (" + str + ") ";
    }

    public static async Task<List<TaskModel>> GetUncompletedTasksInProject(string projectId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return new List<TaskModel>((IEnumerable<TaskModel>) await App.Connection.Table<TaskModel>().Where((Expression<Func<TaskModel, bool>>) (v => v.userId == userId && v.deleted == 0 && v.status == 0 && v.projectId == projectId)).ToListAsync());
    }

    public static async Task<List<TaskModel>> GetTasksInProjectAsync(string projectId)
    {
      string loginUserId = LocalSettings.Settings.LoginUserId;
      return await App.Connection.QueryAsync<TaskModel>("Select * from TaskModel WHERE projectId = '" + projectId + "' and userId = '" + loginUserId + "'") ?? new List<TaskModel>();
    }

    [Obsolete]
    private static async Task<List<TaskModel>> AssemblyFullTaskList(List<TaskModel> tasks)
    {
      if (tasks.Count > 0)
      {
        Dictionary<string, int> attachCountDict = (await App.Connection.QueryAsync<CountModel>("select taskId as ItemIdentity, count(*) as ItemCount from AttachmentModel where deleted != 1 group by taskId")).Where<CountModel>((Func<CountModel, bool>) (a => !string.IsNullOrEmpty(a.ItemIdentity))).Select<CountModel, KeyValuePair<string, int>>((Func<CountModel, KeyValuePair<string, int>>) (x => new KeyValuePair<string, int>(x.ItemIdentity, x.ItemCount))).ToDictionary<KeyValuePair<string, int>, string, int>((Func<KeyValuePair<string, int>, string>) (x => x.Key), (Func<KeyValuePair<string, int>, int>) (x => x.Value));
        List<CountModel> source = await App.Connection.QueryAsync<CountModel>("select TaskServerId as ItemIdentity, count(*) as ItemCount from TaskReminderModel where TaskServerId != null group by TaskServerId");
        Dictionary<string, int> dictionary = new Dictionary<string, int>();
        if (source != null)
          dictionary = source.Where<CountModel>((Func<CountModel, bool>) (r => !string.IsNullOrEmpty(r.ItemIdentity))).Select<CountModel, KeyValuePair<string, int>>((Func<CountModel, KeyValuePair<string, int>>) (x => new KeyValuePair<string, int>(x.ItemIdentity, x.ItemCount))).ToDictionary<KeyValuePair<string, int>, string, int>((Func<KeyValuePair<string, int>, string>) (x => x.Key), (Func<KeyValuePair<string, int>, int>) (x => x.Value));
        foreach (TaskModel task in tasks)
        {
          TaskDao.AdjustStartAndDueDate(task);
          if (!string.IsNullOrEmpty(task.id))
          {
            if (attachCountDict.ContainsKey(task.id))
              task.attachmentCount = attachCountDict[task.id];
            if (dictionary.ContainsKey(task.id))
              task.reminderCount = dictionary[task.id];
          }
        }
        attachCountDict = (Dictionary<string, int>) null;
      }
      return tasks;
    }

    public static async Task<TaskModel> AssembleFullTask(TaskModel task, bool checkTextCount = false)
    {
      if (task == null)
        return (TaskModel) null;
      DateTime? startDate = task.startDate;
      TaskModel taskModel;
      if (startDate.HasValue)
      {
        taskModel = task;
        taskModel.reminders = (await TaskReminderDao.GetRemindersByTaskId(task.id)).ToArray();
        taskModel = (TaskModel) null;
        task.reminderCount = task.reminders?.Length.Value;
        TaskDao.AdjustLegacyTrigger(task);
      }
      TaskDao.AdjustStartAndDueDate(task);
      if (task.kind == "CHECKLIST")
      {
        taskModel = task;
        taskModel.items = (await TaskDetailItemDao.GetCheckItemsByTaskId(task.id)).ToArray();
        taskModel = (TaskModel) null;
        if (task.items != null && task.items.Length != 0)
        {
          foreach (TaskDetailItemModel taskDetailItemModel1 in task.items)
          {
            TaskDetailItemModel taskDetailItemModel2 = taskDetailItemModel1;
            startDate = taskDetailItemModel1.startDate;
            string str;
            if (!startDate.HasValue)
            {
              str = "-1";
            }
            else
            {
              startDate = taskDetailItemModel1.startDate;
              DateTime dateTime = startDate.Value;
              ref DateTime local = ref dateTime;
              startDate = taskDetailItemModel1.startDate;
              string converterValue = UtcDateTimeConverter.GetConverterValue(startDate.Value);
              str = local.ToString(converterValue);
            }
            taskDetailItemModel2.serverStartDate = str;
            if (checkTextCount)
            {
              string title = taskDetailItemModel1.title;
              if ((title != null ? (title.Length > 204800 ? 1 : 0) : 0) != 0)
              {
                UserActCollectUtils.SendCustomException("CheckItemTextOverLimit " + taskDetailItemModel1.title.Length.ToString());
                taskDetailItemModel1.title = taskDetailItemModel1.title.Substring(0, 204800);
              }
            }
          }
        }
      }
      else
        task.items = (TaskDetailItemModel[]) null;
      taskModel = task;
      taskModel.Attachments = (await AttachmentDao.GetTaskAttachments(task.id)).ToArray();
      taskModel = (TaskModel) null;
      task.attachmentCount = task.Attachments.Length;
      List<PomodoroSummaryModel> pomosByTaskId = await PomoSummaryDao.GetPomosByTaskId(task.id);
      // ISSUE: explicit non-virtual call
      if (pomosByTaskId != null && __nonvirtual (pomosByTaskId.Count) > 0)
      {
        PomodoroSummaryModel pomodoroSummaryModel1 = (PomodoroSummaryModel) null;
        List<PomodoroSummaryModel> list = pomosByTaskId.ToList<PomodoroSummaryModel>();
        foreach (PomodoroSummaryModel pomodoroSummaryModel2 in pomosByTaskId)
        {
          if (!(pomodoroSummaryModel2.userId != LocalSettings.Settings.LoginUserId))
          {
            if (pomodoroSummaryModel1 == null)
            {
              pomodoroSummaryModel1 = pomodoroSummaryModel2;
            }
            else
            {
              pomodoroSummaryModel1.count += pomodoroSummaryModel2.count;
              pomodoroSummaryModel1.PomoDuration += pomodoroSummaryModel2.PomoDuration;
              pomodoroSummaryModel1.estimatedPomo += pomodoroSummaryModel2.estimatedPomo;
              pomodoroSummaryModel1.EstimatedDuration += pomodoroSummaryModel2.EstimatedDuration;
              pomodoroSummaryModel1.StopwatchDuration += pomodoroSummaryModel2.StopwatchDuration;
              List<object[]> focuses = pomodoroSummaryModel2.focuses;
              // ISSUE: explicit non-virtual call
              if ((focuses != null ? (__nonvirtual (focuses.Count) > 0 ? 1 : 0) : 0) != 0)
              {
                foreach (object[] focuse in pomodoroSummaryModel2.focuses)
                  pomodoroSummaryModel1.AddFocuses(focuse, true);
              }
              list.Remove(pomodoroSummaryModel2);
              App.Connection.DeleteAsync((object) pomodoroSummaryModel2);
            }
          }
        }
        task.FocusSummaries = list.ToArray();
      }
      if (!string.IsNullOrEmpty(task.tag))
        task.tags = TagSerializer.ToTags(task.tag).ToArray();
      if (!string.IsNullOrEmpty(task.exDates))
        task.exDate = ExDateSerilizer.ToArray(task.exDates);
      return task;
    }

    private static void AdjustLegacyTrigger(TaskModel task)
    {
      if (!task.isAllDay.HasValue || !task.isAllDay.Value || task.reminders == null || !((IEnumerable<TaskReminderModel>) task.reminders).Any<TaskReminderModel>())
        return;
      foreach (TaskReminderModel reminder in task.reminders)
      {
        if (!string.IsNullOrEmpty(reminder.trigger))
          reminder.trigger = TriggerUtils.ConvertLegacyTrigger(reminder.trigger);
      }
    }

    private static void AdjustStartAndDueDate(TaskModel task)
    {
      if (!task.startDate.HasValue && task.dueDate.HasValue)
      {
        task.startDate = task.dueDate;
        task.dueDate = new DateTime?();
      }
      if (!task.startDate.HasValue)
        return;
      DateTime? startDate = task.startDate;
      DateTime? nullable1 = task.dueDate;
      if ((startDate.HasValue & nullable1.HasValue ? (startDate.GetValueOrDefault() >= nullable1.GetValueOrDefault() ? 1 : 0) : 0) == 0)
        return;
      TaskModel taskModel = task;
      nullable1 = new DateTime?();
      DateTime? nullable2 = nullable1;
      taskModel.dueDate = nullable2;
    }

    public static async Task<List<TaskModel>> GetAllTask()
    {
      string userId = LocalSettings.Settings.LoginUserId;
      return await App.Connection.Table<TaskModel>().Where((Expression<Func<TaskModel, bool>>) (t => t.userId == userId)).ToListAsync() ?? new List<TaskModel>();
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTasksInColumn(
      string projectId,
      string columnId,
      bool isDefaultColumn)
    {
      return await TaskCache.GetModelsInProjectColumn(projectId, columnId, isDefaultColumn);
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayTasksInDate(string dateStamp)
    {
      return await TaskDisplayService.GetDisplayTaskInDate(dateStamp);
    }

    public static async Task<List<TaskDao.TitleModel>> GetTaskTitleInProject()
    {
      return TaskCache.GetTaskTitleModels();
    }

    public static async Task BatchUpdateTaskOnTagDeleted(string tag)
    {
      List<TaskModel> allTask = await TaskDao.GetAllTask();
      if (allTask == null)
        ;
      else if (allTask.Count <= 0)
        ;
      else
      {
        List<TaskModel> list = allTask.Where<TaskModel>((Func<TaskModel, bool>) (task => !string.IsNullOrEmpty(task.tag) && task.tag.Contains(tag))).ToList<TaskModel>();
        if (list.Count <= 0)
          ;
        else
        {
          foreach (TaskModel taskModel in list)
          {
            List<string> tags = TagSerializer.ToTags(taskModel.tag);
            tags.Remove(tag);
            taskModel.tag = TagSerializer.ToJsonContent(tags);
            taskModel.modifiedTime = new DateTime?(DateTime.Now);
          }
          await TaskDao.BatchUpdateTasks(list, false, CheckMatchedType.OnlyFilter);
        }
      }
    }

    public static async Task BatchUpdateAffectedTaskOnTagChanged(string original, string newLabel)
    {
      List<TaskModel> allTask = await TaskDao.GetAllTask();
      if (allTask == null)
        ;
      else if (allTask.Count <= 0)
        ;
      else
      {
        List<TaskModel> list = allTask.Where<TaskModel>((Func<TaskModel, bool>) (task => !string.IsNullOrEmpty(task.tag) && task.tag.ToLower().Contains(TaskDao.AddTagDivider(original)))).ToList<TaskModel>();
        if (list.Count <= 0)
          ;
        else
        {
          foreach (TaskModel taskModel in list)
          {
            string str = TaskDao.AddTagDivider(newLabel);
            if (!taskModel.tag.ToLower().Contains(str))
            {
              taskModel.tag = taskModel.tag.ToLower().Replace(TaskDao.AddTagDivider(original), TaskDao.AddTagDivider(newLabel)).Replace("#", string.Empty).Replace("＃", string.Empty).ToLower();
              taskModel.tags = TagSerializer.ToTags(taskModel.tag).ToArray();
              taskModel.modifiedTime = new DateTime?(DateTime.Now);
            }
          }
          await TaskDao.BatchUpdateTasks(list, false, CheckMatchedType.OnlyFilter);
        }
      }
    }

    private static string AddTagDivider(string tag) => "\"" + tag.ToLower() + "\"";

    public static async Task BatchUpdateTasks(List<TaskBaseViewModel> viewModels)
    {
      if (viewModels == null || viewModels.Count <= 0)
        return;
      foreach (TaskBaseViewModel viewModel in viewModels)
      {
        TaskModel task = await viewModel.ToTask();
        if (task != null)
          await TaskDao.UpdateTask(task);
      }
    }

    public static async Task BatchUpdateTasks(
      List<TaskModel> tasks,
      bool checkUserId = true,
      CheckMatchedType checkMatched = CheckMatchedType.None)
    {
      if (tasks == null || tasks.Count <= 0)
        return;
      List<TaskBaseViewModel> vms = new List<TaskBaseViewModel>();
      foreach (TaskModel task in tasks)
      {
        if (checkUserId && string.IsNullOrEmpty(task.userId))
          task.userId = Utils.GetCurrentUserIdInt().ToString();
        TaskBaseViewModel taskViewModel = TaskCache.SafeGetTaskViewModel(task);
        taskViewModel.SetProperties(new TaskBaseViewModel(task));
        vms.Add(taskViewModel);
      }
      int num = await App.Connection.UpdateAllAsync((IEnumerable) tasks);
      ProjectAndTaskIdsCache.OnTasksChanged(vms, checkMatched);
      vms = (List<TaskBaseViewModel>) null;
    }

    public static async Task UpdateTask(TaskModel task, CheckMatchedType checkMatched = CheckMatchedType.None)
    {
      if (task == null)
        return;
      if (string.IsNullOrEmpty(task.userId))
        task.userId = Utils.GetCurrentUserIdInt().ToString();
      int num = await App.Connection.UpdateAsync((object) task);
      TaskCache.Insert(task);
    }

    public static async Task<List<string>> GetRepeatEventIds()
    {
      return await BaseDao<TaskModel>.GetEntityIds("select Id as Id from calendarEventModel where repeatFlag is not null and repeatFlag !='' and deleted = '0' and userId ='" + Utils.GetCurrentUserIdInt().ToString() + "'");
    }

    public static async Task HandleTimeZoneChanged(TimeZoneInfo newTz, TimeZoneInfo oldTz)
    {
      List<TaskModel> listAsync1 = await App.Connection.Table<TaskModel>().ToListAsync();
      List<TaskModel> items1 = new List<TaskModel>();
      HashSet<string> floatTask = new HashSet<string>();
      foreach (TaskModel taskModel1 in listAsync1)
      {
        if (taskModel1 != null)
        {
          if (taskModel1.Floating)
            floatTask.Add(taskModel1.id);
          else if (!taskModel1.isAllDay.GetValueOrDefault())
          {
            DateTime? nullable1 = taskModel1.startDate;
            if (nullable1.HasValue)
            {
              TaskModel taskModel2 = taskModel1;
              nullable1 = taskModel1.startDate;
              DateTime? nullable2 = new DateTime?(TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(nullable1.Value, DateTimeKind.Unspecified), oldTz, newTz));
              taskModel2.startDate = nullable2;
              nullable1 = taskModel1.dueDate;
              if (nullable1.HasValue)
              {
                TaskModel taskModel3 = taskModel1;
                nullable1 = taskModel1.dueDate;
                DateTime? nullable3 = new DateTime?(TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(nullable1.Value, DateTimeKind.Unspecified), oldTz, newTz));
                taskModel3.dueDate = nullable3;
              }
              items1.Add(taskModel1);
            }
          }
        }
      }
      if (items1.Count > 0)
      {
        int num1 = await App.Connection.UpdateAllAsync((IEnumerable) items1);
      }
      List<TaskDetailItemModel> listAsync2 = await App.Connection.Table<TaskDetailItemModel>().ToListAsync();
      List<TaskDetailItemModel> items2 = new List<TaskDetailItemModel>();
      foreach (TaskDetailItemModel taskDetailItemModel1 in listAsync2)
      {
        bool? isAllDay = taskDetailItemModel1.isAllDay;
        if (isAllDay.HasValue)
        {
          isAllDay = taskDetailItemModel1.isAllDay;
          if (!isAllDay.Value && !floatTask.Contains(taskDetailItemModel1.TaskServerId))
          {
            DateTime? startDate = taskDetailItemModel1.startDate;
            if (startDate.HasValue)
            {
              TaskDetailItemModel taskDetailItemModel2 = taskDetailItemModel1;
              startDate = taskDetailItemModel1.startDate;
              DateTime? nullable = new DateTime?(TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(startDate.Value, DateTimeKind.Unspecified), oldTz, newTz));
              taskDetailItemModel2.startDate = nullable;
              items2.Add(taskDetailItemModel1);
            }
          }
        }
      }
      if (items2.Count <= 0)
      {
        floatTask = (HashSet<string>) null;
      }
      else
      {
        int num2 = await App.Connection.UpdateAllAsync((IEnumerable) items2);
        floatTask = (HashSet<string>) null;
      }
    }

    public static async Task HandleDirtyData()
    {
      List<TaskId> source = await App.Connection.QueryAsync<TaskId>("select userId as UserId, id as Id from taskModel group by userid, id having count(1) > 1");
      if (!source.Any<TaskId>())
        return;
      foreach (TaskId taskId in source)
      {
        TaskId id = taskId;
        List<TaskModel> taskModel = await App.Connection.Table<TaskModel>().Where((Expression<Func<TaskModel, bool>>) (v => v.id.Equals(id.Id) && v.userId == id.UserId)).ToListAsync();
        if (taskModel.Any<TaskModel>() && taskModel.Count > 1)
        {
          for (int i = 1; i < taskModel.Count; ++i)
          {
            int num = await App.Connection.DeleteAsync((object) taskModel[i]);
          }
        }
        taskModel = (List<TaskModel>) null;
      }
    }

    public static async Task<List<TaskBaseViewModel>> GetSummaryTask(SummaryFilterModel filter)
    {
      return !filter.StartDate.HasValue || !filter.EndDate.HasValue ? (List<TaskBaseViewModel>) null : TaskViewModelHelper.GetTaskInSummary(filter);
    }

    public static async Task<List<TaskModel>> GetUnSyncCompletedTasks()
    {
      List<SyncStatusModel> syncStatusByType = await SyncStatusDao.GetSyncStatusByType(4);
      return syncStatusByType != null && syncStatusByType.Any<SyncStatusModel>() ? (await TaskDao.GetThinTasksInBatch(syncStatusByType.Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (t => t.EntityId)).ToList<string>())).Where<TaskModel>((Func<TaskModel, bool>) (task => task.status != 0 && task.deleted == 0 && !string.IsNullOrEmpty(task.repeatTaskId) && task.repeatTaskId != task.id)).ToList<TaskModel>() : (List<TaskModel>) null;
    }

    public static async Task DeleteTaskInDb(string taskId)
    {
      TaskModel taskById = await TaskDao.GetTaskById(taskId);
      if (taskById == null)
        return;
      int num = await App.Connection.DeleteAsync((object) taskById);
      TaskCache.DeleteTask(taskId);
      TaskSyncedJsonDao.DeleteTaskSyncedJsonPhysical(taskId);
    }

    public static async Task<List<TaskModel>> GetAllSubTasksById(string taskId, string projectId)
    {
      List<TaskBaseViewModel> allSubTasksById = TaskCache.GetAllSubTasksById(taskId, projectId);
      return await TaskDao.GetTasksByIds(allSubTasksById != null ? allSubTasksById.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (m => m.Id)).ToList<string>() : (List<string>) null);
    }

    public static async Task<List<TaskModel>> GetTaskAndChildrenInBatch(
      List<string> taskIds,
      bool checkExist = true)
    {
      return await TaskDao.GetTasksByIds(TaskCache.GetTaskAndChildrenInBatch(taskIds, checkExist).Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>());
    }

    public static async Task RemoveTaskParentId(string taskId)
    {
      await TaskDao.UpdateParent(taskId, "");
    }

    public static async Task RemoveTaskParentIdInBatch(List<string> taskIds)
    {
      await TaskDao.BatchUpdateParent(taskIds, "");
    }

    public static Node<DisplayItemModel> GetDisplayItemNode(
      string taskId,
      string projectId,
      bool withCompleted)
    {
      Dictionary<string, Node<TaskBaseViewModel>> taskTreeInProject = TaskCache.GetTaskTreeInProject(projectId, withCompleted);
      return !string.IsNullOrEmpty(taskId) && taskTreeInProject.ContainsKey(taskId) ? taskTreeInProject[taskId].ToDisplayItemNode(taskId) : (Node<DisplayItemModel>) null;
    }

    public static async Task UpdateParent(string taskId, string parentId, bool needSync = true)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null)
        task = (TaskModel) null;
      else if (task.parentId == parentId)
        task = (TaskModel) null;
      else if (string.IsNullOrEmpty(task.parentId) && string.IsNullOrEmpty(parentId))
        task = (TaskModel) null;
      else if (!needSync)
      {
        TaskCache.SafeGetTaskViewModel(task).ParentId = parentId;
        task = (TaskModel) null;
      }
      else
      {
        UtilLog.Info("SetTaskParent t " + taskId + ",old " + task.parentId + ",new " + parentId);
        string oldParent = task.parentId;
        task.parentId = parentId;
        task.modifiedTime = new DateTime?(DateTime.Now);
        await TaskService.UpdateTaskParent(task);
        await SyncStatusDao.AddSetParentSyncStatus(taskId, oldParent);
        if (!string.IsNullOrEmpty(parentId))
          await TaskDao.AddOrRemoveTaskChildIds(parentId, new List<string>()
          {
            task.id
          }, true);
        if (!string.IsNullOrEmpty(oldParent))
          await TaskDao.AddOrRemoveTaskChildIds(oldParent, new List<string>()
          {
            task.id
          }, false);
        oldParent = (string) null;
        task = (TaskModel) null;
      }
    }

    public static async Task AddOrRemoveTaskChildIds(
      string parentId,
      List<string> ids,
      bool addChild)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(parentId);
      if (thinTaskById == null)
        return;
      List<string> stringList = string.IsNullOrEmpty(thinTaskById?.childrenString) ? new List<string>() : JsonConvert.DeserializeObject<List<string>>(thinTaskById.childrenString) ?? new List<string>();
      foreach (string id in ids)
      {
        if (stringList.Contains(id))
        {
          if (!addChild)
            stringList.Remove(id);
        }
        else if (addChild)
          stringList.Add(id);
      }
      thinTaskById.childrenString = stringList.Count > 0 ? JsonConvert.SerializeObject((object) stringList) : (string) null;
      await TaskService.UpdateTaskParent(thinTaskById);
    }

    public static int GetTaskLevel(string taskId, string projectId)
    {
      if (string.IsNullOrEmpty(taskId))
        return 0;
      Dictionary<string, Node<TaskBaseViewModel>> taskTreeInProject = TaskCache.GetTaskTreeInProject(projectId);
      return taskTreeInProject.ContainsKey(taskId) ? taskTreeInProject[taskId].GetLevel(0) : 0;
    }

    public static async Task BatchUpdateParent(
      List<string> ids,
      string parentId,
      string undoId = null,
      bool canUndo = true)
    {
      TaskModel parent = await TaskDao.GetThinTaskById(parentId);
      bool needToast = false;
      string taskName = "";
      int count = 0;
      bool undo = TaskDragUndoModel.CheckUndoIdExist(undoId);
      foreach (string taskId in ids)
      {
        TaskModel task = await TaskDao.GetThinTaskById(taskId);
        if (task != null && !(task.parentId == parentId) && (!string.IsNullOrEmpty(task.parentId) || !string.IsNullOrEmpty(parentId)))
        {
          bool projectChanged = false;
          string oldParent = task.parentId;
          if (parent != null && task.projectId != parent.projectId)
          {
            await TaskService.MoveProject(taskId, parent.projectId);
            projectChanged = true;
            task = await TaskDao.GetThinTaskById(taskId);
            needToast = true;
            taskName = task.title;
            ++count;
          }
          task.parentId = parentId;
          task.modifiedTime = new DateTime?(DateTime.Now);
          if (canUndo && projectChanged | undo)
          {
            TaskDragUndoModel.AddDragModel(undo ? undoId : taskId, taskId, oldParent, parentId, (string) null, (string) null, (string) null, 0L);
          }
          else
          {
            await SyncStatusDao.AddSetParentSyncStatus(taskId, oldParent);
            if (!string.IsNullOrEmpty(oldParent))
              await TaskDao.AddOrRemoveTaskChildIds(oldParent, new List<string>()
              {
                task.id
              }, false);
          }
          await TaskService.UpdateTaskParent(task);
          oldParent = (string) null;
        }
        task = (TaskModel) null;
      }
      if (needToast)
        Utils.GetToastWindow()?.ToastMoveProjectControl(parent.projectId, count > 1 ? string.Empty : taskName);
      if (string.IsNullOrEmpty(parentId))
      {
        parent = (TaskModel) null;
        taskName = (string) null;
      }
      else
      {
        await TaskDao.AddOrRemoveTaskChildIds(parentId, ids, true);
        parent = (TaskModel) null;
        taskName = (string) null;
      }
    }

    public static List<string> GetTreeTopIds(List<string> taskIds, string projectId)
    {
      Dictionary<string, Node<TaskBaseViewModel>> taskNodes = string.IsNullOrEmpty(projectId) ? TaskCache.GetAllTaskTree() : TaskCache.GetTaskTreeInProject(projectId);
      HashSet<string> taskIdSet = new HashSet<string>((IEnumerable<string>) taskIds);
      foreach (string taskId in taskIds)
      {
        if (taskNodes.ContainsKey(taskId) && ExistParentId(taskNodes[taskId], taskIdSet))
          taskIdSet.Remove(taskId);
      }
      return taskIds.Where<string>((Func<string, bool>) (id => taskIdSet.Contains(id) && taskNodes.ContainsKey(id))).ToList<string>();

      static bool ExistParentId(Node<TaskBaseViewModel> node, HashSet<string> set)
      {
        if (!node.HasParent)
          return false;
        return set.Contains(node.ParentId) || ExistParentId(node.Parent, set);
      }
    }

    public static async Task CheckChildrenLevel(string taskId, int newLevel)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      Dictionary<string, Node<TaskBaseViewModel>> taskTreeInProject = TaskCache.GetTaskTreeInProject(thinTaskById.projectId);
      if (!taskTreeInProject.ContainsKey(taskId))
        return;
      await TaskDao.CheckTaskLevel(taskTreeInProject[taskId], newLevel, taskId);
    }

    private static async Task CheckTaskLevel(
      Node<TaskBaseViewModel> taskNode,
      int level,
      string undoId)
    {
      if (level >= 4)
      {
        List<Node<TaskBaseViewModel>> children = taskNode.Children;
        // ISSUE: explicit non-virtual call
        if ((children != null ? (__nonvirtual (children.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          List<TaskBaseViewModel> childTasks = taskNode.GetAllChildrenValue();
          long sortOrderInProject = ProjectSortOrderDao.GetNextTaskSortOrderInProject(taskNode.Value.ProjectId, taskNode.Value.SortOrder, taskNode.Value.ParentId);
          for (int index = 0; index < childTasks.Count; ++index)
            childTasks[index].SortOrder = Math.Min(taskNode.Value.SortOrder, sortOrderInProject) + Math.Abs(sortOrderInProject - taskNode.Value.SortOrder) * (long) (index + 1) / (long) (childTasks.Count + 1);
          await TaskDao.BatchUpdateTasks(childTasks);
          await TaskDao.BatchUpdateParent(childTasks.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>(), taskNode.Value.ParentId, undoId);
          childTasks = (List<TaskBaseViewModel>) null;
          return;
        }
      }
      List<Node<TaskBaseViewModel>> children1 = taskNode.Children;
      // ISSUE: explicit non-virtual call
      if ((children1 != null ? (__nonvirtual (children1.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      foreach (Node<TaskBaseViewModel> child in taskNode.Children)
        await TaskDao.CheckTaskLevel(child, level + 1, undoId);
    }

    public static async Task<bool> UncheckAllCheckItems(string taskId)
    {
      List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(taskId);
      bool changed = false;
      // ISSUE: explicit non-virtual call
      if (checkItemsByTaskId != null && __nonvirtual (checkItemsByTaskId.Count) > 0)
      {
        foreach (TaskDetailItemModel taskDetailItemModel in checkItemsByTaskId)
        {
          if (taskDetailItemModel.status != 0)
          {
            taskDetailItemModel.status = 0;
            changed = true;
          }
        }
        await TaskDetailItemDao.BatchUpdateChecklists(checkItemsByTaskId);
      }
      return changed;
    }

    public static async void UpdateTaskAttachmentCount(string taskId, int count)
    {
      TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId);
      if (thinTaskById == null)
        return;
      thinTaskById.attachmentCount = count;
      TaskDao.UpdateTask(thinTaskById);
    }

    public static async Task FoldOrOpenTasks(List<string> taskIds, bool isOpen)
    {
      List<TaskModel> thinTasksInBatch = await TaskDao.GetThinTasksInBatch(taskIds);
      if (thinTasksInBatch == null)
        return;
      foreach (TaskModel taskModel in thinTasksInBatch)
      {
        if (taskModel.isOpen != isOpen)
          taskModel.isOpen = isOpen;
      }
      await TaskDao.BatchUpdateTasks(thinTasksInBatch);
    }

    public static async Task<List<TaskModel>> GetAssignTasksInProject(
      string projectId,
      string assignee)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TaskModel>().Where((Expression<Func<TaskModel, bool>>) (v => v.projectId.Equals(projectId) && v.userId == userId && v.assignee == assignee)).ToListAsync();
    }

    public static async Task<List<TaskModel>> GetTasksInProjectOrColumn(
      string projectId,
      string columnId,
      bool first)
    {
      List<TaskBaseViewModel> inProjectOrColumn = await TaskCache.GetTasksInProjectOrColumn(projectId, columnId, first);
      return await TaskDao.GetTasksByIds(inProjectOrColumn != null ? inProjectOrColumn.Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (t => t.Id)).ToList<string>() : (List<string>) null);
    }

    public static async Task<TaskModel> GetTaskByUid(long uid)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      return await App.Connection.Table<TaskModel>().Where((Expression<Func<TaskModel, bool>>) (v => (long) v._Id == uid && v.userId == userId)).FirstOrDefaultAsync();
    }

    public static async Task<List<TaskModel>> GetParentsByTaskIdAsync(string taskId)
    {
      TaskModel task = await TaskDao.GetThinTaskById(taskId);
      if (task == null || string.IsNullOrEmpty(task.parentId))
        return (List<TaskModel>) null;
      HashSet<string> parentIds = new HashSet<string>();
      List<TaskModel> result = new List<TaskModel>();
      string taskId1 = task.parentId;
      while (!string.IsNullOrEmpty(taskId1) && !parentIds.Contains(taskId1))
      {
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskId1);
        if (thinTaskById != null && thinTaskById.projectId == task.projectId)
        {
          result.Add(thinTaskById);
          parentIds.Add(thinTaskById.id);
          taskId1 = thinTaskById.parentId;
        }
        else
          taskId1 = (string) null;
      }
      return result;
    }

    public static async void Test()
    {
      List<TaskModel> tasks = await TaskDao.GetAllTask();
      await Task.Delay(1000);
      List<Task> taskList = new List<Task>();
      for (int index = 0; index < 10; ++index)
      {
        taskList.Add((Task) TaskDao.GetAllTask());
        taskList.Add((Task) BaseDao<TaskModel>.UpdateAllAsync(tasks));
      }
      await Task.WhenAll((IEnumerable<Task>) taskList);
      tasks = (List<TaskModel>) null;
    }

    public static async Task<List<TaskModel>> GetUtcTasks()
    {
      return await App.Connection.Table<TaskModel>().Where((Expression<Func<TaskModel, bool>>) (t => t.timeZone == "Etc/UTC")).ToListAsync();
    }

    public static async Task BatchUpdateProjectIdsInDb(List<string> taskIds, string projectId)
    {
      while (taskIds.Count > 0)
      {
        List<string> list;
        if (taskIds.Count > 100)
        {
          list = taskIds.Take<string>(100).ToList<string>();
          taskIds.RemoveRange(0, 100);
        }
        else
        {
          list = taskIds.ToList<string>();
          taskIds.Clear();
        }
        string seed = "";
        string str = list.Aggregate<string, string>(seed, (Func<string, string, string>) ((current, id) => current + "'" + id + "',"));
        int num = await App.Connection.ExecuteAsync("Update TaskModel Set projectId = ? where id in (" + str.Substring(0, str.Length - 1) + ")", (object) projectId);
      }
    }

    private class OrderInDateModel
    {
      public string TaskId { get; set; }

      public long SortOrder { get; set; }

      public string Date { get; set; }
    }

    public class OrderModel
    {
      public string TaskId { get; set; }

      public long SortOrder { get; set; }
    }

    public class TitleModel
    {
      public string TaskId { get; set; }

      public string Kind { get; set; }

      public string Title { get; set; }
    }
  }
}
