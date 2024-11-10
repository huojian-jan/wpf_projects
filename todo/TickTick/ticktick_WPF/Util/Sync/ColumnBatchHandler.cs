// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ColumnBatchHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util.Sync.Model;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class ColumnBatchHandler : BatchHandler
  {
    public ColumnBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
    }

    public static async Task CommitToServer(LogModel logModel)
    {
      Dictionary<string, string> needPostColumns = await ColumnDao.GetNeedPostColumns();
      if (needPostColumns == null || !needPostColumns.Any<KeyValuePair<string, string>>())
        return;
      SyncColumnBean syncBean = await ColumnTransfer.DescribeSyncColumnBean(needPostColumns);
      if (!syncBean.Add.Any<ColumnModel>() && !syncBean.Update.Any<ColumnModel>() && !syncBean.Deleted.Any<ColumnProjectModel>())
        return;
      LogModel logModel1 = logModel;
      logModel1.Log = logModel1.Log + "\r\n" + string.Format("big sync:  update column count : a {0} u {1} d {2}", (object) syncBean.Add.Count, (object) syncBean.Update.Count, (object) syncBean.Deleted.Count);
      BatchUpdateResult batchUpdateResult = await Communicator.BatchUpdateColumns(syncBean);
      if (batchUpdateResult == null)
        return;
      await ColumnTransfer.HandleCommitResult(batchUpdateResult.Id2etag, batchUpdateResult.Id2error, logModel);
    }

    public static async Task<bool> MergeWithServer(SyncColumnBean bean, LogModel syncBeanLog)
    {
      List<ColumnModel> added = new List<ColumnModel>();
      List<ColumnModel> updated = new List<ColumnModel>();
      List<ColumnModel> serverColumns = new List<ColumnModel>();
      serverColumns.AddRange((IEnumerable<ColumnModel>) (bean.Add ?? new List<ColumnModel>()));
      serverColumns.AddRange((IEnumerable<ColumnModel>) (bean.Update ?? new List<ColumnModel>()));
      if (serverColumns.Any<ColumnModel>())
      {
        Dictionary<string, ColumnModel> localSyncedColumnMap = await ColumnDao.GetLocalSyncedColumnMap();
        foreach (ColumnModel columnModel1 in serverColumns)
        {
          if (string.IsNullOrEmpty(columnModel1.userId))
            columnModel1.userId = Utils.GetCurrentUserIdInt().ToString();
          if (localSyncedColumnMap.ContainsKey(columnModel1.id))
          {
            ColumnModel columnModel2 = localSyncedColumnMap[columnModel1.id];
            localSyncedColumnMap.Remove(columnModel1.id);
            if ((string.IsNullOrEmpty(columnModel2.etag) || string.IsNullOrEmpty(columnModel1.etag) || !(columnModel2.etag == columnModel1.etag)) && columnModel2.syncStatus.Equals("done"))
            {
              columnModel1._Id = columnModel2._Id;
              columnModel1.syncStatus = "done";
              updated.Add(columnModel1);
            }
          }
          else
          {
            columnModel1.syncStatus = "done";
            added.Add(columnModel1);
          }
        }
      }
      if (bean.Deleted != null && bean.Deleted.Any<ColumnProjectModel>())
        await ColumnDao.DeleteColumns(bean.Deleted);
      if (added.Count > 0 || updated.Count > 0)
        await ColumnDao.SaveServerMergeData(added, updated, new List<ColumnModel>());
      string empty1 = string.Empty;
      string empty2 = string.Empty;
      LogModel logModel = syncBeanLog;
      logModel.Log = logModel.Log + "\r\n\tmergeColumns u" + updated.Aggregate<ColumnModel, string>(empty1, (Func<string, ColumnModel, string>) ((current, column) => current + column?.id + ",")) + "  a" + added.Aggregate<ColumnModel, string>(empty2, (Func<string, ColumnModel, string>) ((current, column) => current + column?.id + ","));
      bool flag = added.Count > 0 || updated.Count > 0 || bean.Deleted != null && bean.Deleted.Any<ColumnProjectModel>();
      added = (List<ColumnModel>) null;
      updated = (List<ColumnModel>) null;
      serverColumns = (List<ColumnModel>) null;
      return flag;
    }

    public static async Task<bool> MergeWithServer(string projectId)
    {
      ProjectModel projectById = await ProjectDao.GetProjectById(projectId);
      if (projectById == null || projectById.IsNew())
        return false;
      List<ColumnModel> added = new List<ColumnModel>();
      List<ColumnModel> updated = new List<ColumnModel>();
      List<ColumnModel> serverColumns = await Communicator.GetRemoteColumnsByProjectId(projectId);
      List<SyncStatusModel> moveColumnStatus = await SyncStatusDao.GetSyncStatusByType(32);
      if (serverColumns != null && serverColumns.Any<ColumnModel>())
      {
        Dictionary<string, ColumnModel> localSyncedColumnMap = await ColumnDao.GetLocalSyncedColumnMap(projectId);
        foreach (ColumnModel columnModel1 in serverColumns)
        {
          ColumnModel serverColumn = columnModel1;
          if (string.IsNullOrEmpty(serverColumn.userId))
            serverColumn.userId = Utils.GetCurrentUserIdInt().ToString();
          if (localSyncedColumnMap.ContainsKey(serverColumn.id))
          {
            ColumnModel columnModel2 = localSyncedColumnMap[serverColumn.id];
            localSyncedColumnMap.Remove(serverColumn.id);
            if ((string.IsNullOrEmpty(columnModel2.etag) || string.IsNullOrEmpty(serverColumn.etag) || !(columnModel2.etag == serverColumn.etag) || columnModel2.deleted == 1) && columnModel2.syncStatus.Equals("done"))
            {
              serverColumn._Id = columnModel2._Id;
              serverColumn.syncStatus = "done";
              updated.Add(serverColumn);
            }
          }
          else if (!moveColumnStatus.Any<SyncStatusModel>((Func<SyncStatusModel, bool>) (c => c.EntityId == serverColumn.id && c.MoveFromId == serverColumn.projectId)))
          {
            serverColumn.syncStatus = "done";
            added.Add(serverColumn);
          }
        }
        List<ColumnModel> list = localSyncedColumnMap.Values.Where<ColumnModel>((Func<ColumnModel, bool>) (column => (moveColumnStatus.Count == 0 || moveColumnStatus.All<SyncStatusModel>((Func<SyncStatusModel, bool>) (c => c.EntityId != column.id))) && column.syncStatus == "done" || column.syncStatus == "update")).ToList<ColumnModel>();
        if (added.Count > 0 || updated.Count > 0 || list.Count > 0)
        {
          await ColumnDao.SaveServerMergeData(added, updated, list);
          DataChangedNotifier.NotifyColumnChanged(projectId);
          return true;
        }
      }
      return false;
    }
  }
}
