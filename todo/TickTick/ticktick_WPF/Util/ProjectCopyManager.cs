// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ProjectCopyManager
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
using ticktick_WPF.Service;
using ticktick_WPF.Service.SortOrder;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class ProjectCopyManager
  {
    public static async Task<TaskModel> CopyTask(
      TaskModel origin,
      string projectId,
      string newId,
      bool checkParent = true,
      bool keepChecklistItemStatus = false,
      int status = 0)
    {
      if (origin == null)
        return (TaskModel) null;
      TaskModel copy = origin.Clone();
      copy.status = status;
      copy.id = newId;
      copy.projectId = projectId;
      copy.assignee = origin.assignee;
      copy.commentCount = "0";
      copy.createdTime = new DateTime?(DateTime.Now);
      copy.modifiedTime = new DateTime?(DateTime.Now);
      if (!keepChecklistItemStatus)
        copy.progress = new int?(0);
      TaskModel taskModel1 = copy;
      taskModel1.content = await ProjectCopyManager.CopyAttachments(origin.id, copy.id, copy.content);
      taskModel1 = (TaskModel) null;
      copy.AddPinnedSecond();
      if (copy.pinnedTimeStamp > 0L)
      {
        SyncSortOrderModel syncSortOrderModel = await TaskSortOrderService.InsertOrUpdateAsync("taskPinned", copy.projectId, newId);
      }
      TaskModel taskModel2 = await TaskDao.InsertTask(copy);
      await ProjectCopyManager.CopyReminders(origin.id, taskModel2.id);
      await ProjectCopyManager.CopyChecklistItems(origin.id, taskModel2.id, keepChecklistItemStatus);
      await ProjectCopyManager.CopyPomodoroSummaries(origin.id, taskModel2.id);
      await SyncStatusDao.AddCreateSyncStatus(copy.id);
      if (checkParent && !string.IsNullOrEmpty(copy.parentId))
      {
        TaskDao.AddOrRemoveTaskChildIds(copy.parentId, new List<string>()
        {
          copy.id
        }, true);
        await SyncStatusDao.AddSetParentSyncStatus(copy.id, "");
      }
      return copy;
    }

    public static async Task<List<string>> TryCopySubTasks(
      string originId,
      string taskId,
      string taskProjectId,
      int status = 0,
      bool isDragCopy = false)
    {
      Dictionary<string, string> dict = new Dictionary<string, string>()
      {
        {
          originId,
          taskId
        }
      };
      Dictionary<string, List<string>> parentChildrenDic = new Dictionary<string, List<string>>();
      List<TaskModel> subTasksByIdAsync = await TaskService.GetAllSubTasksByIdAsync(originId, taskProjectId);
      List<string> copiedTaskIds = new List<string>();
      // ISSUE: explicit non-virtual call
      if (subTasksByIdAsync != null && __nonvirtual (subTasksByIdAsync.Count) > 0)
      {
        foreach (TaskModel taskModel in subTasksByIdAsync)
          dict.Add(taskModel.id, Utils.GetGuid());
        foreach (TaskModel origin in subTasksByIdAsync)
        {
          if (!string.IsNullOrEmpty(origin.parentId) && dict.ContainsKey(origin.parentId))
            origin.parentId = dict[origin.parentId];
          TaskModel taskModel = await ProjectCopyManager.CopyTask(origin, taskProjectId, dict[origin.id], false, status: status);
          if (!string.IsNullOrEmpty(taskModel.parentId))
          {
            if (parentChildrenDic.ContainsKey(taskModel.parentId))
              parentChildrenDic[taskModel.parentId].Add(taskModel.id);
            else
              parentChildrenDic[taskModel.parentId] = new List<string>()
              {
                taskModel.id
              };
          }
          copiedTaskIds.Add(taskModel.id);
        }
        foreach (KeyValuePair<string, List<string>> keyValuePair in parentChildrenDic)
        {
          string key = keyValuePair.Key;
          List<string> stringList = keyValuePair.Value;
          List<string> ids = stringList;
          TaskDao.AddOrRemoveTaskChildIds(key, ids, true);
          foreach (string taskId1 in stringList)
            await SyncStatusDao.AddSetParentSyncStatus(taskId1, "");
        }
      }
      List<string> stringList1 = copiedTaskIds;
      dict = (Dictionary<string, string>) null;
      parentChildrenDic = (Dictionary<string, List<string>>) null;
      copiedTaskIds = (List<string>) null;
      return stringList1;
    }

    private static async Task CopyPomodoroSummaries(string originId, string copyTaskSid)
    {
      await PomoSummaryDao.SavePomoSummaries(((IEnumerable<PomodoroSummaryModel>) (await PomoSummaryDao.GetPomosByTaskId(originId)).ToArray()).Select<PomodoroSummaryModel, PomodoroSummaryModel>((Func<PomodoroSummaryModel, PomodoroSummaryModel>) (item => item.Copy(copyTaskSid, true))).ToList<PomodoroSummaryModel>());
    }

    private static async Task<string> CopyAttachments(
      string originalTaskId,
      string copyTaskSid,
      string taskContent)
    {
      List<AttachmentModel> taskAttachments = await AttachmentDao.GetTaskAttachments(originalTaskId);
      if (taskAttachments != null && taskAttachments.Count > 0)
      {
        foreach (AttachmentModel attachmentModel in taskAttachments)
        {
          AttachmentModel attachment = attachmentModel;
          if (attachment.status != 1)
          {
            AttachmentModel copy = new AttachmentModel()
            {
              id = Utils.GetGuid(),
              refId = attachment.refId,
              path = attachment.path,
              localPath = attachment.localPath,
              size = attachment.size,
              fileName = attachment.fileName,
              fileType = attachment.fileType,
              createdTime = attachment.createdTime,
              taskId = copyTaskSid,
              deleted = attachment.deleted,
              sync_status = attachment.sync_status
            };
            AttachmentCache.SetAttachment(copy);
            await AttachmentDao.InsertOrUpdateAttachment(copy);
            taskContent = taskContent?.Replace(attachment.id, copy.id);
            copy = (AttachmentModel) null;
            attachment = (AttachmentModel) null;
          }
        }
      }
      return taskContent;
    }

    public static async Task CopyReminders(string originalTaskId, string copyTaskSid)
    {
      List<TaskReminderModel> remindersByTaskId = await TaskReminderDao.GetRemindersByTaskId(originalTaskId);
      if (remindersByTaskId == null || remindersByTaskId.Count <= 0)
        return;
      foreach (TaskReminderModel taskReminderModel in remindersByTaskId)
      {
        int num = await TaskReminderDao.SaveReminders(new TaskReminderModel()
        {
          id = Utils.GetGuid(),
          taskserverid = copyTaskSid,
          trigger = taskReminderModel.trigger
        });
      }
    }

    public static async Task CopyChecklistItems(
      string originalTaskId,
      string copyTaskSid,
      bool keepStatus = true)
    {
      List<TaskDetailItemModel> models = await ProjectCopyManager.TaskDetailItemModels(originalTaskId, copyTaskSid, keepStatus);
      if (models == null || models.Count <= 0)
        return;
      await TaskDetailItemDao.BatchInsertChecklists(models);
    }

    private static async Task<List<TaskDetailItemModel>> TaskDetailItemModels(
      string originalTaskId,
      string copyTaskSid,
      bool keepStatus)
    {
      List<TaskDetailItemModel> copylist = new List<TaskDetailItemModel>();
      List<TaskDetailItemModel> checkItemsByTaskId = await TaskDetailItemDao.GetCheckItemsByTaskId(originalTaskId);
      if (checkItemsByTaskId != null && checkItemsByTaskId.Count > 0)
        copylist.AddRange(checkItemsByTaskId.Select<TaskDetailItemModel, TaskDetailItemModel>((Func<TaskDetailItemModel, TaskDetailItemModel>) (item => new TaskDetailItemModel()
        {
          id = Utils.GetGuid(),
          TaskServerId = copyTaskSid,
          title = item.title,
          status = keepStatus ? item.status : 0,
          sortOrder = item.sortOrder,
          completedTime = item.completedTime,
          startDate = item.startDate,
          isAllDay = item.isAllDay,
          snoozeReminderTime = item.snoozeReminderTime
        })));
      List<TaskDetailItemModel> taskDetailItemModelList = copylist;
      copylist = (List<TaskDetailItemModel>) null;
      return taskDetailItemModelList;
    }
  }
}
