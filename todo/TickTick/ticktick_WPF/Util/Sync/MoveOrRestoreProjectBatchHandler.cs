// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.MoveOrRestoreProjectBatchHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Sync.Model;
using TickTickDao;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class MoveOrRestoreProjectBatchHandler : BatchHandler
  {
    public MoveOrRestoreProjectBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
    }

    public static async Task<List<MoveOrRestoreProject>> GetCommits(int moveOrRestore)
    {
      List<SyncStatusModel> tasks = await TaskDao.GetNeedPostMovedOrRestoreTasks(moveOrRestore);
      if (tasks != null && tasks.Count > 0)
      {
        List<SyncStatusModel> syncStatusByType = await SyncStatusDao.GetSyncStatusByType(moveOrRestore);
        Dictionary<string, string> moveFromDict = new Dictionary<string, string>();
        if (syncStatusByType != null && syncStatusByType.Count > 0)
        {
          foreach (SyncStatusModel syncStatusModel in syncStatusByType)
          {
            if (!string.IsNullOrEmpty(syncStatusModel.EntityId) && !moveFromDict.ContainsKey(syncStatusModel.EntityId))
              moveFromDict.Add(syncStatusModel.EntityId, syncStatusModel.MoveFromId);
          }
        }
        List<MoveOrRestoreProject> projects = new List<MoveOrRestoreProject>();
        int i;
        switch (moveOrRestore)
        {
          case 2:
            for (i = 0; i < tasks.Count; ++i)
            {
              TaskModel taskById = await TaskDao.GetTaskById(tasks[i].EntityId);
              if (taskById != null)
              {
                MoveProject moveProject1 = new MoveProject();
                moveProject1.TaskId = taskById.id;
                moveProject1.FromProjectId = moveFromDict.ContainsKey(taskById.id) ? moveFromDict[taskById.id] : taskById.projectId;
                moveProject1.SortOrder = taskById.sortOrder;
                moveProject1.ToProjectId = taskById.projectId;
                MoveProject moveProject2 = moveProject1;
                if (moveProject2.FromProjectId == moveProject2.ToProjectId)
                {
                  int num = await BaseDao<SyncStatusModel>.DeleteAsync(tasks[i]);
                }
                else
                  projects.Add((MoveOrRestoreProject) moveProject2);
              }
            }
            return projects;
          case 7:
            for (i = 0; i < tasks.Count; ++i)
            {
              TaskModel taskById = await TaskDao.GetTaskById(tasks[i].EntityId);
              if (taskById != null)
              {
                RestoreProject restoreProject1 = new RestoreProject();
                restoreProject1.TaskId = taskById.id;
                restoreProject1.FromProjectId = moveFromDict.ContainsKey(taskById.id) ? moveFromDict[taskById.id] : taskById.projectId;
                restoreProject1.ToProjectId = taskById.projectId;
                RestoreProject restoreProject2 = restoreProject1;
                if (CacheManager.GetProjectById(restoreProject2.FromProjectId) == null)
                  restoreProject2.FromProjectId = Utils.GetInboxId();
                projects.Add((MoveOrRestoreProject) restoreProject2);
              }
            }
            return projects;
          default:
            moveFromDict = (Dictionary<string, string>) null;
            projects = (List<MoveOrRestoreProject>) null;
            break;
        }
      }
      return (List<MoveOrRestoreProject>) null;
    }

    public static async void HandleCommitResult(BatchUpdateResult result, int moveOrRestore)
    {
      List<string> entityIdsByType = await SyncStatusDao.GetEntityIdsByType(moveOrRestore);
      if (entityIdsByType == null)
        return;
      foreach (string taskSid in entityIdsByType)
      {
        if (result?.Id2etag != null && result.Id2etag.Count != 0 && result.Id2etag.ContainsKey(taskSid))
        {
          int num = await SyncStatusDao.DeleteSyncStatus(taskSid, moveOrRestore) ? 1 : 0;
          TaskModel taskById = await TaskDao.GetTaskById(taskSid);
          if (taskById != null)
          {
            taskById.etag = result.Id2etag[taskSid];
            await TaskDao.UpdateTask(taskById);
          }
        }
        if (result?.Id2error != null && result.Id2error.Count != 0 && result.Id2error.ContainsKey(taskSid))
        {
          UtilLog.Info("big sync: move id2error \\" + result.Id2error[taskSid]);
          int num = await SyncStatusDao.DeleteSyncStatus(taskSid, moveOrRestore) ? 1 : 0;
        }
      }
    }
  }
}
