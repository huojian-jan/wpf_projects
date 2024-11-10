// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ColumnDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using TickTickDao;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class ColumnDao : BaseDao<ColumnModel>
  {
    public static async Task<bool> TryInitColumns(string projectId)
    {
      List<ColumnModel> serverColumns = await Communicator.GetRemoteColumnsByProjectId(projectId);
      if (serverColumns != null && serverColumns.Any<ColumnModel>())
      {
        serverColumns.ForEach((Action<ColumnModel>) (column =>
        {
          column.userId = Utils.GetCurrentUserIdInt().ToString();
          column.syncStatus = "done";
        }));
        int num = await App.Connection.InsertAllAsync((IEnumerable) serverColumns);
        await ProjectDao.SetProjectDefaultColumn(projectId, serverColumns[0].id);
        return false;
      }
      await ColumnDao.InitColumns(projectId);
      return true;
    }

    public static async Task InitColumns(string projectId)
    {
      List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(projectId);
      string defaultColumnId;
      if (columnsByProjectId != null && columnsByProjectId.Any<ColumnModel>())
      {
        defaultColumnId = (string) null;
      }
      else
      {
        defaultColumnId = Utils.GetGuid();
        int num = await App.Connection.InsertAsync((object) new ColumnModel()
        {
          id = defaultColumnId,
          userId = Utils.GetCurrentUserIdInt().ToString(),
          projectId = projectId,
          sortOrder = new long?(0L),
          name = Utils.GetString("NotSectioned"),
          createdTime = new DateTime?(DateTime.Now),
          modifiedTime = new DateTime?(DateTime.Now),
          syncStatus = "init"
        });
        await TaskSortOrderService.OnColumnInit(projectId, defaultColumnId);
        await ProjectDao.SetProjectDefaultColumn(projectId, defaultColumnId);
        defaultColumnId = (string) null;
      }
    }

    public static async Task<ColumnModel> GetColumnById(string columnId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<ColumnModel>().Where((Expression<Func<ColumnModel, bool>>) (column => column.id == columnId && column.userId == userId)).FirstOrDefaultAsync();
    }

    private static async Task<List<ColumnModel>> GetColumnsByIds(List<string> columnIds)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<ColumnModel>().Where((Expression<Func<ColumnModel, bool>>) (model => columnIds.Contains(model.id) && model.deleted != 1 && model.userId == userId)).OrderBy<long?>((Expression<Func<ColumnModel, long?>>) (model => model.sortOrder)).ToListAsync();
    }

    public static async Task<List<ColumnModel>> GetColumnsByProjectId(string projectId)
    {
      string userId = Utils.GetCurrentUserStr();
      List<ColumnModel> columns = await App.Connection.Table<ColumnModel>().Where((Expression<Func<ColumnModel, bool>>) (model => model.projectId == projectId && model.userId == userId)).OrderBy<long?>((Expression<Func<ColumnModel, long?>>) (model => model.sortOrder)).ToListAsync();
      Dictionary<string, ColumnModel> dictionary = new Dictionary<string, ColumnModel>();
      List<ColumnModel> columnModelList1 = columns;
      // ISSUE: explicit non-virtual call
      if ((columnModelList1 != null ? (__nonvirtual (columnModelList1.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        List<ColumnModel> columnModelList2 = new List<ColumnModel>();
        foreach (ColumnModel columnModel1 in columns)
        {
          if (!string.IsNullOrEmpty(columnModel1.id))
          {
            if (dictionary.ContainsKey(columnModel1.id))
            {
              ColumnModel columnModel2 = dictionary[columnModel1.id];
              if (columnModel2.deleted == 1 && columnModel2.deleted != 1)
              {
                dictionary[columnModel1.id] = columnModel1;
                App.Connection.DeleteAsync((object) columnModel2);
                columnModelList2.Remove(columnModel2);
                columnModelList2.Add(columnModel1);
              }
              else
                App.Connection.DeleteAsync((object) columnModel1);
            }
            else
            {
              dictionary.Add(columnModel1.id, columnModel1);
              if (columnModel1.deleted != 1)
                columnModelList2.Add(columnModel1);
            }
          }
        }
        columns = columnModelList2;
      }
      List<ColumnModel> columnModelList3 = columns;
      // ISSUE: explicit non-virtual call
      if ((columnModelList3 != null ? (__nonvirtual (columnModelList3.Count) > 0 ? 1 : 0) : 0) != 0 && new HashSet<long>(columns.Select<ColumnModel, long>((Func<ColumnModel, long>) (c => c.sortOrder.GetValueOrDefault()))).Count < columns.Count)
      {
        long so = 0;
        foreach (ColumnModel column in columns)
        {
          column.sortOrder = new long?(so);
          await ColumnDao.UpdateColumn(column);
          so += 268435456L;
        }
        SyncManager.TryDelaySync();
      }
      List<ColumnModel> columnsByProjectId = columns;
      columns = (List<ColumnModel>) null;
      return columnsByProjectId;
    }

    public static async Task DeleteColumn(string columnId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<ColumnModel> columns = await App.Connection.Table<ColumnModel>().Where((Expression<Func<ColumnModel, bool>>) (m => m.id == columnId && m.userId == userId)).ToListAsync();
      for (int i = 0; i < columns.Count; ++i)
      {
        ColumnModel model = columns[i];
        if (i == 0)
        {
          if (model != null)
          {
            model.syncStatus = "delete";
            model.deleted = 1;
            int num = await App.Connection.UpdateAsync((object) model);
          }
        }
        else
        {
          int num1 = await BaseDao<ColumnModel>.DeleteAsync(model);
        }
      }
      columns = (List<ColumnModel>) null;
    }

    public static async Task SaveColumnName(string name, string columnId, bool isNew = false)
    {
      ColumnModel columnById = await ColumnDao.GetColumnById(columnId);
      if (columnById == null)
        return;
      columnById.name = name;
      if (isNew)
        columnById.syncStatus = "new";
      if (columnById.syncStatus != "new" && columnById.syncStatus != "init")
        columnById.syncStatus = "update";
      int num = await App.Connection.UpdateAsync((object) columnById);
    }

    public static async Task<string> GetProjectDefaultColumnId(string projectId, bool ignoreFirst = false)
    {
      List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(projectId);
      if (columnsByProjectId == null || !columnsByProjectId.Any<ColumnModel>())
        return string.Empty;
      List<ColumnModel> list = columnsByProjectId.OrderBy<ColumnModel, long?>((Func<ColumnModel, long?>) (column => column.sortOrder)).ToList<ColumnModel>();
      if (ignoreFirst)
        list.RemoveAt(0);
      return list.FirstOrDefault<ColumnModel>()?.id;
    }

    public static async Task SaveAsLastSortOrder(string dragId)
    {
      ColumnModel dragColumn = await ColumnDao.GetColumnById(dragId);
      if (dragColumn == null)
      {
        dragColumn = (ColumnModel) null;
      }
      else
      {
        List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(dragColumn.projectId);
        long? nullable1;
        if (columnsByProjectId == null)
        {
          nullable1 = new long?();
        }
        else
        {
          long? nullable2 = columnsByProjectId.Select<ColumnModel, long?>((Func<ColumnModel, long?>) (c => c.sortOrder)).Max();
          nullable1 = nullable2.HasValue ? new long?(nullable2.GetValueOrDefault() + 268435456L) : new long?();
        }
        dragColumn.sortOrder = new long?(nullable1.GetValueOrDefault());
        dragColumn.modifiedTime = new DateTime?(DateTime.Now);
        if (dragColumn.syncStatus != "new")
          dragColumn.syncStatus = "update";
        int num = await App.Connection.UpdateAsync((object) dragColumn);
        dragColumn = (ColumnModel) null;
      }
    }

    public static async Task<long?> SaveSortOrder(string dragId, string dropId, bool left = false)
    {
      ColumnModel dragColumn = await ColumnDao.GetColumnById(dragId);
      if (dragColumn != null)
      {
        List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(dragColumn.projectId);
        if (columnsByProjectId.Any<ColumnModel>())
        {
          ColumnModel columnModel = columnsByProjectId.FirstOrDefault<ColumnModel>((Func<ColumnModel, bool>) (item => item.id == dropId));
          if (columnModel != null)
          {
            long valueOrDefault = columnModel.sortOrder.GetValueOrDefault();
            int num1 = columnsByProjectId.IndexOf(columnModel);
            dragColumn.sortOrder = new long?((left ? 0 : 1) != 0 ? (num1 < columnsByProjectId.Count - 1 ? valueOrDefault / 2L + columnsByProjectId[num1 + 1].sortOrder.GetValueOrDefault() / 2L : valueOrDefault + 268435456L) : (num1 > 0 ? valueOrDefault / 2L + columnsByProjectId[num1 - 1].sortOrder.GetValueOrDefault() / 2L : valueOrDefault - 268435456L));
            dragColumn.modifiedTime = new DateTime?(DateTime.Now);
            if (dragColumn.syncStatus != "new")
              dragColumn.syncStatus = "update";
            int num2 = await App.Connection.UpdateAsync((object) dragColumn);
            return dragColumn.sortOrder;
          }
        }
      }
      return new long?();
    }

    public static async Task<Dictionary<string, string>> GetNeedPostColumns()
    {
      List<ColumnSyncStatusModel> source = await App.Connection.QueryAsync<ColumnSyncStatusModel>("select id as ColumnId, syncStatus as SyncStatus from ColumnModel where projectId is not null and syncStatus is not null and syncStatus <> 'done' and userId = '" + Utils.GetCurrentUserIdInt().ToString() + "'");
      Dictionary<string, string> needPostColumns = new Dictionary<string, string>();
      if (source != null && source.Any<ColumnSyncStatusModel>())
      {
        foreach (ColumnSyncStatusModel columnSyncStatusModel in source)
        {
          if (!needPostColumns.ContainsKey(columnSyncStatusModel.ColumnId))
            needPostColumns.Add(columnSyncStatusModel.ColumnId, columnSyncStatusModel.ColumnId);
        }
      }
      return needPostColumns;
    }

    public static async Task SaveCommitResultBackToDb(
      Dictionary<string, string> id2Etag,
      Dictionary<string, string> id2Error,
      LogModel logModel)
    {
      string serverId;
      if (id2Etag != null)
      {
        logModel.Log += "Id2etag : ";
        foreach (string key in id2Etag.Keys)
        {
          serverId = key;
          ColumnModel columnById = await ColumnDao.GetColumnById(serverId);
          if (columnById != null)
          {
            columnById.syncStatus = "done";
            columnById.etag = id2Etag[serverId];
            int num = await App.Connection.UpdateAsync((object) columnById);
            LogModel logModel1 = logModel;
            logModel1.Log = logModel1.Log + serverId + "  ";
          }
          serverId = (string) null;
        }
      }
      if (id2Error == null)
        return;
      serverId = string.Empty;
      foreach (string serverId1 in id2Error.Keys)
      {
        ColumnModel column = await ColumnDao.GetColumnById(serverId1);
        if (column != null)
        {
          if (id2Error[serverId1] == "NOT_EXISTED")
          {
            column.syncStatus = column.deleted == 0 ? "new" : "done";
            int num = await App.Connection.UpdateAsync((object) column);
          }
          if (id2Error[serverId1] == "EXISTED")
          {
            column.syncStatus = column.deleted == 0 ? "update" : "done";
            int num = await App.Connection.UpdateAsync((object) column);
          }
        }
        serverId = serverId + serverId1 + " : " + id2Error[serverId1];
        column = (ColumnModel) null;
      }
      LogModel logModel2 = logModel;
      logModel2.Log = logModel2.Log + "  error " + serverId;
      serverId = (string) null;
    }

    public static async Task<Dictionary<string, ColumnModel>> GetLocalSyncedColumnMap()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      Dictionary<string, ColumnModel> dict = new Dictionary<string, ColumnModel>();
      List<ColumnModel> listAsync = await App.Connection.Table<ColumnModel>().Where((Expression<Func<ColumnModel, bool>>) (v => v.userId.Equals(userId))).ToListAsync();
      if (listAsync != null && listAsync.Any<ColumnModel>())
      {
        foreach (ColumnModel model in listAsync)
        {
          if (!string.IsNullOrEmpty(model.id) && !dict.ContainsKey(model.id))
            dict.Add(model.id, model);
          else
            BaseDao<ColumnModel>.DeleteAsync(model);
        }
      }
      Dictionary<string, ColumnModel> localSyncedColumnMap = dict;
      dict = (Dictionary<string, ColumnModel>) null;
      return localSyncedColumnMap;
    }

    public static async Task<Dictionary<string, ColumnModel>> GetLocalSyncedColumnMap(
      string projectId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      Dictionary<string, ColumnModel> dict = new Dictionary<string, ColumnModel>();
      List<ColumnModel> listAsync = await App.Connection.Table<ColumnModel>().Where((Expression<Func<ColumnModel, bool>>) (v => v.userId.Equals(userId) && v.projectId == projectId)).ToListAsync();
      if (listAsync != null && listAsync.Any<ColumnModel>())
      {
        foreach (ColumnModel model in listAsync)
        {
          if (!string.IsNullOrEmpty(model.id) && !dict.ContainsKey(model.id))
            dict.Add(model.id, model);
          else
            BaseDao<ColumnModel>.DeleteAsync(model);
        }
      }
      Dictionary<string, ColumnModel> localSyncedColumnMap = dict;
      dict = (Dictionary<string, ColumnModel>) null;
      return localSyncedColumnMap;
    }

    public static async Task DeleteColumns(List<ColumnProjectModel> models)
    {
      List<ColumnModel> columnsByIds = await ColumnDao.GetColumnsByIds(models.Select<ColumnProjectModel, string>((Func<ColumnProjectModel, string>) (model => model.columnId)).ToList<string>());
      if (!columnsByIds.Any<ColumnModel>())
        return;
      foreach (object obj in columnsByIds)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task SaveServerMergeData(
      List<ColumnModel> added,
      List<ColumnModel> updated,
      List<ColumnModel> deleted)
    {
      if (added.Count > 0)
      {
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) added);
      }
      if (updated.Count > 0)
      {
        int num2 = await App.Connection.UpdateAllAsync((IEnumerable) updated);
      }
      if (deleted.Count <= 0)
        return;
      foreach (object obj in deleted)
      {
        int num3 = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task<ColumnModel> AddColumn(
      string name,
      string projectId,
      long sortOrder,
      string id = null)
    {
      ColumnModel column = new ColumnModel()
      {
        id = string.IsNullOrEmpty(id) ? Utils.GetGuid() : id,
        name = name,
        projectId = projectId,
        sortOrder = new long?(sortOrder),
        userId = Utils.GetCurrentUserIdInt().ToString(),
        createdTime = new DateTime?(DateTime.Now),
        deleted = 0,
        syncStatus = "new"
      };
      int num = await App.Connection.InsertAsync((object) column);
      ColumnModel columnModel = column;
      column = (ColumnModel) null;
      return columnModel;
    }

    public static async Task UpdateColumn(ColumnModel column)
    {
      if (column.syncStatus != "new" && column.syncStatus != "init")
        column.syncStatus = "update";
      int num = await App.Connection.UpdateAsync((object) column);
    }

    public static async Task<List<ColumnModel>> GetAllColumnsAsync()
    {
      return await Task.Run<List<ColumnModel>>((Func<Task<List<ColumnModel>>>) (async () => await Connection.DbConnection.Table<ColumnModel>().Where((Expression<Func<ColumnModel, bool>>) (c => c.userId == LocalSettings.Settings.LoginUserId && c.deleted != 1)).ToListAsync()));
    }

    public static async Task<List<ColumnModel>> CheckProjectColumns(string projectId)
    {
      List<ColumnModel> columnsByProjectId = await ColumnDao.GetColumnsByProjectId(projectId);
      if (columnsByProjectId == null || !columnsByProjectId.Any<ColumnModel>())
      {
        int num1 = await ColumnDao.TryInitColumns(projectId) ? 1 : 0;
      }
      else
      {
        int num2 = await ColumnBatchHandler.MergeWithServer(projectId) ? 1 : 0;
      }
      return await ColumnDao.GetColumnsByProjectId(projectId);
    }

    public static async Task UpdateColumnProjectId(string columnId, string projectId)
    {
      int num = await App.Connection.ExecuteAsync("Update ColumnModel SET projectId=? WHERE id=? and userId=?", (object) projectId, (object) columnId, (object) LocalSettings.Settings.LoginUserId);
    }
  }
}
