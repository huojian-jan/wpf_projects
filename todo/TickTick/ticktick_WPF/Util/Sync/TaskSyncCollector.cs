// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.TaskSyncCollector
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Provider;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views;
using TickTickDao;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public static class TaskSyncCollector
  {
    public static async Task<TaskSyncModel> CollectTaskSyncBean(SyncTaskBean syncTaskBean, int type)
    {
      TaskSyncModel syncModel = new TaskSyncModel();
      await TaskSyncCollector.CollectDeletedTaskFromRemoteModel(syncTaskBean, syncModel);
      SyncTaskBean syncTaskBean1 = syncTaskBean;
      int num1;
      if (syncTaskBean1 == null)
      {
        num1 = 0;
      }
      else
      {
        int? count = syncTaskBean1.Update?.Count;
        int num2 = 0;
        num1 = count.GetValueOrDefault() > num2 & count.HasValue ? 1 : 0;
      }
      if (num1 != 0)
        await TaskSyncCollector.MergeUpdatedTasksFromServer(syncModel, syncTaskBean?.Update, type);
      TaskSyncModel taskSyncModel = syncModel;
      syncModel = (TaskSyncModel) null;
      return taskSyncModel;
    }

    private static async Task CollectDeletedTaskFromRemoteModel(
      SyncTaskBean syncTaskBean,
      TaskSyncModel syncModel)
    {
      await TaskSyncCollector.CollectDeleteInTrash(syncTaskBean, syncModel);
      await TaskSyncCollector.CollectDeleteForever(syncTaskBean, syncModel);
    }

    private static async Task CollectDeleteInTrash(
      SyncTaskBean syncTaskBean,
      TaskSyncModel syncModel)
    {
      List<TaskProject> trashTasks = syncTaskBean?.DeletedInTrash;
      List<TaskProject> taskProjectList = trashTasks;
      // ISSUE: explicit non-virtual call
      if ((taskProjectList != null ? (__nonvirtual (taskProjectList.Count) > 0 ? 1 : 0) : 0) == 0)
      {
        trashTasks = (List<TaskProject>) null;
      }
      else
      {
        List<string> status = await SyncStatusDao.GetEntityIdsByType(7);
        List<string> stringList = status;
        stringList.AddRange((IEnumerable<string>) await SyncStatusDao.GetEntityIdsByType(6));
        stringList = (List<string>) null;
        foreach (TaskProject task in trashTasks)
        {
          TaskModel thinTaskById = await TaskDao.GetThinTaskById(task.taskId);
          if (thinTaskById != null && thinTaskById.deleted != 2 && !status.Contains(task.taskId))
            syncModel.TaskSyncBean.DeletedInTrash.Add(thinTaskById);
        }
        status = (List<string>) null;
        trashTasks = (List<TaskProject>) null;
      }
    }

    private static async Task CollectDeleteForever(
      SyncTaskBean syncTaskBean,
      TaskSyncModel syncModel)
    {
      List<TaskProject> deletedForever = syncTaskBean?.DeletedForever;
      // ISSUE: explicit non-virtual call
      if (deletedForever == null || __nonvirtual (deletedForever.Count) <= 0)
        return;
      foreach (TaskProject taskProject in deletedForever)
      {
        TaskModel thinTaskById = await TaskDao.GetThinTaskById(taskProject.taskId);
        if (thinTaskById != null)
          syncModel.TaskSyncBean.DeletedForever.Add(thinTaskById);
      }
    }

    public static async Task MergeUpdatedTasksFromServer(
      TaskSyncModel syncModel,
      List<TaskModel> update,
      int type)
    {
      HashSet<string> projectIds = await ProjectDao.GetProjectIdSet();
      HashSet<string> contentChangeTaskIds = await SyncStatusDao.GetEntityIdSetByType(0);
      HashSet<string> deleted = await SyncStatusDao.GetEntityIdSetByType(5);
      HashSet<string> changeParent = await SyncStatusDao.GetEntityIdSetByType(16);
      List<SyncStatusModel> moveStatus = await SyncStatusDao.GetSyncStatusByType(2);
      int sameEtag = 0;
      int ignoreCount = update.Count;
      update = update.Where<TaskModel>((Func<TaskModel, bool>) (t => t != null && projectIds.Contains(t.projectId))).ToList<TaskModel>();
      if (deleted.Any<string>() || UndoHelper.DeletingIds.Any<string>())
        update = update.Where<TaskModel>((Func<TaskModel, bool>) (t => !deleted.Contains(t.id) && !UndoHelper.DeletingIds.Contains(t.id))).ToList<TaskModel>();
      ignoreCount -= update.Count;
      Dictionary<string, TaskEtag> localTasks = await TaskDao.GetTask2SidMap(update.Select<TaskModel, string>((Func<TaskModel, string>) (t => t.id)).ToList<string>());
      int updateCount = 0;
      foreach (TaskModel serverTask in update)
      {
        await MergeServerTask(serverTask);
        ++updateCount;
      }
      UtilLog.Info(string.Format("\r\n\tpull task merge updateCount : {0}, {1} {2}", (object) updateCount, ignoreCount > 0 ? (object) string.Format(" ignoreCount:{0}", (object) ignoreCount) : (object) "", sameEtag > 0 ? (object) string.Format(" sameEtag:{0}", (object) sameEtag) : (object) ""));

      async Task MergeServerTask(TaskModel serverTask)
      {
        if (localTasks.ContainsKey(serverTask.id))
        {
          TaskEtag taskEtag = localTasks[serverTask.id];
          if (type != 1 && taskEtag.Etag == serverTask.etag)
          {
            ++sameEtag;
          }
          else
          {
            await AttachmentSyncCollector.CollectRemoteAttachments(serverTask, syncModel.AttachmentSyncBean);
            if (contentChangeTaskIds.Contains(serverTask.id))
            {
              TaskModel localTask = await TaskDao.GetFullTaskById(serverTask.id);
              TaskModel originalTask = (TaskModel) null;
              TaskSyncedJsonModel jsonByTaskId = await TaskSyncedJsonDao.GetJsonByTaskId(localTask.id);
              if (jsonByTaskId != null)
                originalTask = JsonConvert.DeserializeObject<TaskModel>(jsonByTaskId.jsonString);
              if (originalTask != null)
              {
                try
                {
                  MergeUtils.Merge(originalTask, serverTask, localTask);
                }
                catch (Exception ex)
                {
                  UtilLog.Error("MergeTaskError: " + ExceptionUtils.BuildExceptionMessage(ex));
                }
              }
              if (!changeParent.Contains(localTask.id))
              {
                localTask.parentId = serverTask.parentId;
                localTask.childIds = serverTask.childIds;
              }
              if (moveStatus != null)
              {
                SyncStatusModel model = moveStatus.FirstOrDefault<SyncStatusModel>((Func<SyncStatusModel, bool>) (m => m.EntityId == localTask.id));
                if (model != null)
                {
                  if (model.MoveFromId != serverTask.projectId)
                  {
                    model.MoveFromId = serverTask.projectId;
                    int num = await BaseDao<SyncStatusModel>.UpdateAsync(model);
                  }
                }
                else
                  localTask.projectId = serverTask.projectId;
              }
              syncModel.TaskSyncBean.Updating.Add(localTask);
              originalTask = (TaskModel) null;
            }
            else
            {
              serverTask._Id = taskEtag._Id;
              TaskBaseViewModel taskById = TaskCache.GetTaskById(serverTask.id);
              serverTask.Actions = taskById?.Actions;
              serverTask.Resources = taskById?.Resources;
              serverTask.label = taskById?.Label;
              syncModel.TaskSyncBean.Updated.Add(serverTask);
              syncModel.TaskSyncedJsonBean.Updated.Add(serverTask);
            }
            taskEtag = (TaskEtag) null;
          }
        }
        else
        {
          AttachmentModel[] attachments = serverTask.Attachments;
          if ((attachments != null ? (attachments.Length != 0 ? 1 : 0) : 0) != 0)
          {
            foreach (AttachmentModel attachment in serverTask.Attachments)
            {
              if (attachment != null)
                attachment.fileType = AttachmentProvider.GetFileType(attachment.fileName).ToString();
            }
          }
          syncModel.TaskSyncBean.Added.Add(serverTask);
          syncModel.TaskSyncedJsonBean.Added.Add(serverTask);
        }
      }
    }
  }
}
