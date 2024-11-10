// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.MoveColumnProjectBatchHandler
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
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using TickTickDao;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class MoveColumnProjectBatchHandler : BatchHandler
  {
    public MoveColumnProjectBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
    }

    public static async Task<List<MoveColumnArgs>> GetCommits()
    {
      List<SyncStatusModel> columnStatuses = await SyncStatusDao.GetSyncStatusByType(32);
      columnStatuses = columnStatuses.DistinctEx<SyncStatusModel, string>((Func<SyncStatusModel, string>) (t => t.EntityId));
      List<SyncStatusModel> source = await SyncStatusDao.GetSyncStatusByType(2) ?? new List<SyncStatusModel>();
      if (columnStatuses == null || columnStatuses.Count <= 0)
        return (List<MoveColumnArgs>) null;
      Dictionary<string, List<SyncStatusModel>> taskMoveDict = source.GroupEx<SyncStatusModel, string, SyncStatusModel>((Func<SyncStatusModel, string>) (t => t.MoveFromId), (Func<SyncStatusModel, SyncStatusModel>) (t => t));
      List<MoveColumnArgs> args = new List<MoveColumnArgs>();
      foreach (SyncStatusModel syncStatusModel in columnStatuses)
      {
        SyncStatusModel columnStatus = syncStatusModel;
        ColumnModel column = await ColumnDao.GetColumnById(columnStatus.EntityId);
        if (column != null && column.deleted != 1)
        {
          MoveColumnArgs moveColumnArgs = new MoveColumnArgs()
          {
            columnId = column.id,
            fromProjectId = columnStatus.MoveFromId,
            sortOrder = column.sortOrder.GetValueOrDefault(),
            toProjectId = column.projectId
          };
          if (moveColumnArgs.fromProjectId == moveColumnArgs.toProjectId)
          {
            int num = await BaseDao<SyncStatusModel>.DeleteAsync(columnStatus);
          }
          else
          {
            List<SyncStatusModel> syncStatusModelList;
            List<SyncStatusModel> list = (taskMoveDict.TryGetValue(columnStatus.MoveFromId, out syncStatusModelList) ? (IEnumerable<SyncStatusModel>) syncStatusModelList : (IEnumerable<SyncStatusModel>) new List<SyncStatusModel>()).Where<SyncStatusModel>((Func<SyncStatusModel, bool>) (task =>
            {
              TaskBaseViewModel taskById = TaskCache.GetTaskById(task.EntityId);
              return taskById != null && !(taskById.ColumnId != column.id) && !(taskById.ProjectId != column.projectId);
            })).ToList<SyncStatusModel>();
            moveColumnArgs.taskIds = list.Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (t => t.EntityId)).ToList<string>();
            moveColumnArgs.tasks = list;
            args.Add(moveColumnArgs);
            columnStatus = (SyncStatusModel) null;
          }
        }
      }
      return args;
    }

    public static async Task<bool> HandleCommitResult(
      BatchUpdateResult result,
      List<MoveColumnArgs> moveStatuses)
    {
      bool needReCommit = false;
      if (moveStatuses != null)
      {
        foreach (MoveColumnArgs moveStatus in moveStatuses)
        {
          if (result?.Id2etag != null && result.Id2etag.Count != 0 && result.Id2etag.ContainsKey(moveStatus.columnId))
            await DeleteStatus(moveStatus);
          if (result?.Id2error != null && result.Id2error.Count != 0 && result.Id2error.ContainsKey(moveStatus.columnId))
          {
            UtilLog.Info("big sync: moveColumn id2error \\" + result.Id2error[moveStatus.columnId]);
            ProjectModel projectById = CacheManager.GetProjectById(moveStatus.fromProjectId);
            if (projectById == null || moveStatus.fromProjectId != Utils.GetInboxId() && projectById.delete_status && projectById.sync_status != Constants.SyncStatus.SYNC_DONE.ToString())
            {
              switch (result.Id2error[moveStatus.columnId])
              {
                case "FROM_PROJECT_NOT_EXISTED":
                  await RestoreColumn(moveStatus, Utils.GetInboxId());
                  await DeleteStatus(moveStatus);
                  continue;
                case "COLUMN_NOT_EXISTED":
                  int num1 = await SyncStatusDao.DeleteSyncStatus(moveStatus.columnId, 32) ? 1 : 0;
                  continue;
                case "TO_PROJECT_NO_PROJECT_PERMISSION":
                case "TO_PROJECT_NOT_EXISTED":
                case "COLUMN_EXCEED_QUOTA":
                  await RestoreColumn(moveStatus, Utils.GetInboxId());
                  int num2 = await SyncStatusDao.DeleteSyncStatus(moveStatus.columnId, 32) ? 1 : 0;
                  await SyncStatusDao.AddColumnMoveProjectStatus(moveStatus.columnId, Utils.GetInboxId());
                  needReCommit = true;
                  continue;
              }
            }
            await RestoreColumn(moveStatus, moveStatus.fromProjectId);
            await DeleteStatus(moveStatus);
          }
        }
      }
      return needReCommit;

      static async Task RestoreColumn(MoveColumnArgs status, string projectId)
      {
        await ColumnDao.UpdateColumnProjectId(status.columnId, projectId);
        List<TaskModel> tasksByIds = await TaskDao.GetTasksByIds(status.taskIds);
        if (tasksByIds == null)
          ;
        else
        {
          // ISSUE: explicit non-virtual call
          if (__nonvirtual (tasksByIds.Count) <= 0)
            ;
          else
            tasksByIds.ForEach((Action<TaskModel>) (t =>
            {
              t.projectId = projectId;
              TaskService.UpdateTaskProject(t);
            }));
        }
      }

      static async Task DeleteStatus(MoveColumnArgs status)
      {
        int num1 = await SyncStatusDao.DeleteSyncStatus(status.columnId, 32) ? 1 : 0;
        foreach (SyncStatusModel task in status.tasks)
        {
          int num2 = await SyncStatusDao.DeleteSyncStatus(task.EntityId, task.Type) ? 1 : 0;
        }
      }
    }
  }
}
