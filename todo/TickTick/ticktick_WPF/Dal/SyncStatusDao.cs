// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.SyncStatusDao
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
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class SyncStatusDao : BaseDao<SyncStatusModel>
  {
    private static async Task<int> Insert(SyncStatusModel syncStatus)
    {
      if (string.IsNullOrEmpty(syncStatus.UserId))
        syncStatus.UserId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.InsertAsync((object) syncStatus);
    }

    private static async Task<int> Update(SyncStatusModel syncStatus)
    {
      if (string.IsNullOrEmpty(syncStatus.UserId))
        syncStatus.UserId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.UpdateAsync((object) syncStatus);
    }

    public static async Task<List<SyncStatusModel>> GetSyncStatusByType(int type)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (v => v.Type == type && v.UserId == userId)).ToListAsync();
    }

    public static async Task<List<SyncStatusModel>> GetSyncStatusById(string id)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (v => v.EntityId == id && v.UserId == userId)).ToListAsync();
    }

    public static async Task AddCreateSyncStatus(string taskId)
    {
      await SyncStatusDao.AddSyncStatus(taskId, 4);
    }

    public static async Task BatchAddModifySyncStatus(IEnumerable<string> taskIds)
    {
      foreach (string taskId in taskIds)
        await SyncStatusDao.AddModifySyncStatus(taskId);
    }

    public static async Task BatchAddCreateTaskStatus(IEnumerable<string> taskIds)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<SyncStatusModel> list = taskIds.Select<string, SyncStatusModel>((Func<string, SyncStatusModel>) (id => new SyncStatusModel()
      {
        Type = 4,
        EntityId = id,
        UserId = userId,
        ModifyPoint = DateTime.Now.Ticks
      })).ToList<SyncStatusModel>();
      if (!list.Any<SyncStatusModel>())
        return;
      int num = await App.Connection.InsertAllAsync((IEnumerable) list);
    }

    public static async Task BatchRemoveDeleteSyncStatus(List<string> taskIds)
    {
      if (taskIds == null || !taskIds.Any<string>())
        return;
      foreach (string taskId in taskIds)
      {
        int num = await SyncStatusDao.DeleteSyncStatus(taskId, 5) ? 1 : 0;
      }
    }

    public static async Task BatchRemoveAddSyncStatus(List<string> taskIds)
    {
      if (taskIds == null || !taskIds.Any<string>())
        return;
      foreach (string taskId in taskIds)
      {
        int num = await SyncStatusDao.DeleteSyncStatus(taskId, 4) ? 1 : 0;
      }
    }

    public static async Task BatchAddDeleteSyncStatus(IEnumerable<string> taskIds)
    {
      taskIds.ToList<string>().ForEach((Action<string>) (id => SyncStatusDao.AddDeleteSyncStatus(id)));
    }

    public static async Task BatchAddDeleteForeverSyncStatus(IEnumerable<string> taskIds)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<SyncStatusModel> listAsync = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (v => taskIds.Contains<string>(v.EntityId) && v.UserId == userId)).ToListAsync();
      if (listAsync != null && listAsync.Count > 0)
      {
        foreach (object obj in listAsync)
        {
          int num = await App.Connection.DeleteAsync(obj);
        }
      }
      List<SyncStatusModel> list = taskIds.Select<string, SyncStatusModel>((Func<string, SyncStatusModel>) (taskId => new SyncStatusModel()
      {
        Type = 6,
        EntityId = taskId,
        UserId = userId,
        ModifyPoint = DateTime.Now.Ticks
      })).ToList<SyncStatusModel>();
      if (list.Count <= 0)
        ;
      else
      {
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) list);
      }
    }

    public static async Task AddClearTrashSyncStatus()
    {
      await SyncStatusDao.AddSyncStatus("d4ae7f9fedd48aab729c2f9c1bccf46", 8);
    }

    public static async Task AddEventDeletedSyncStatus(string eventId)
    {
      await SyncStatusDao.AddSyncStatus(eventId, 11);
    }

    public static async Task AddEventCreateSyncStatus(string eventId)
    {
      await SyncStatusDao.AddEventSyncStatus(eventId, 10);
    }

    public static async Task AddEventModifySyncStatus(string eventId)
    {
      await SyncStatusDao.AddEventSyncStatus(eventId, 9);
    }

    public static async Task AddModifySyncStatus(string taskId)
    {
      await SyncStatusDao.AddSyncStatus(taskId, 0);
    }

    public static async Task AddDeleteSyncStatus(string taskId)
    {
      TaskModel taskById = await TaskDao.GetTaskById(taskId);
      if (taskById == null || taskById.deleted == 0)
        return;
      await SyncStatusDao.AddSyncStatus(taskId, 5);
    }

    public static async Task<bool> RemoveDeleteSyncStatus(string taskId)
    {
      return await SyncStatusDao.DeleteSyncStatus(taskId, 5);
    }

    public static async Task RemoveDeleteForeverSyncStatus(string taskId)
    {
      int num = await SyncStatusDao.DeleteSyncStatus(taskId, 6) ? 1 : 0;
    }

    private static async Task AddEventSyncStatus(string taskId, int type)
    {
      SyncStatusDao.AddSyncStatus(taskId, type, 10);
    }

    public static async Task AddSyncStatus(string entityId, int type, int create = 4)
    {
      if (string.IsNullOrEmpty(entityId))
        ;
      else
      {
        string userId = Utils.GetCurrentUserIdInt().ToString();
        if (type == create)
        {
          List<SyncStatusModel> listAsync = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (v => v.EntityId == entityId && v.Type == create && v.UserId == userId)).ToListAsync();
          // ISSUE: explicit non-virtual call
          if (listAsync != null && __nonvirtual (listAsync.Count) == 1)
          {
            SyncStatusModel syncStatus = listAsync[0];
            syncStatus.ModifyPoint = DateTime.Now.Ticks;
            int num = await SyncStatusDao.Update(syncStatus);
            return;
          }
        }
        else
        {
          List<SyncStatusModel> listAsync = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (v => v.EntityId == entityId && v.UserId == userId && v.Type != 2 && v.Type != 16)).ToListAsync();
          // ISSUE: explicit non-virtual call
          if (listAsync != null && __nonvirtual (listAsync.Count) == 1)
          {
            SyncStatusModel syncStatusModel = listAsync[0];
            if (syncStatusModel.Type == create || syncStatusModel.Type == type)
            {
              SyncStatusModel syncStatus = listAsync[0];
              syncStatus.ModifyPoint = DateTime.Now.Ticks;
              int num = await SyncStatusDao.Update(syncStatus);
              return;
            }
          }
        }
        int num1 = await SyncStatusDao.Insert(new SyncStatusModel()
        {
          Type = type,
          EntityId = entityId,
          UserId = userId,
          ModifyPoint = DateTime.Now.Ticks
        });
      }
    }

    public static async Task AddMoveProjectStatus(string taskId, string moveFromId)
    {
      await SyncStatusDao.AddMoveOrRestoreProjectStatus(taskId, moveFromId);
    }

    public static async Task AddMoveEventStatus(string eventId, string moveFromId)
    {
      await SyncStatusDao.AddMoveOrRestoreProjectStatus(eventId, moveFromId, 12);
    }

    public static async Task AddMoveOrRestoreProjectStatus(
      string taskId,
      string moveFromId,
      int status)
    {
      if (string.IsNullOrEmpty(taskId))
        ;
      else
      {
        string userId = Utils.GetCurrentUserIdInt().ToString();
        SyncStatusModel syncStatusModel = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (v => v.EntityId == taskId && v.Type == status && v.UserId == userId)).FirstOrDefaultAsync();
        SyncStatusModel syncStatus = new SyncStatusModel()
        {
          Type = status,
          EntityId = taskId,
          MoveFromId = moveFromId,
          UserId = userId,
          ModifyPoint = DateTime.Now.Ticks
        };
        if (syncStatusModel != null)
        {
          syncStatus._Id = syncStatusModel._Id;
          syncStatus.ModifyPoint = DateTime.Now.Ticks;
          syncStatus.MoveFromId = syncStatusModel.MoveFromId;
          int num = await SyncStatusDao.Update(syncStatus);
        }
        else
        {
          int num1 = await SyncStatusDao.Insert(syncStatus);
        }
      }
    }

    public static async Task AddMoveOrRestoreProjectStatus(
      string taskId,
      string moveFromId,
      bool isRestore = false)
    {
      await SyncStatusDao.AddMoveOrRestoreProjectStatus(taskId, moveFromId, isRestore ? 7 : 2);
    }

    public static async Task<bool> ExistTaskMoveStatus(string taskId)
    {
      return await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (v => v.EntityId == taskId && v.Type == 2)).FirstOrDefaultAsync() != null;
    }

    public static async Task AddTrashTaskStatus(string taskId)
    {
      Dictionary<int, SyncStatusModel> dict = await SyncStatusDao.GetSyncStatusMap(taskId);
      if (dict.Count > 0)
      {
        if (dict.ContainsKey(4))
        {
          int num1 = await SyncStatusDao.DeleteSyncStatus(taskId, 4) ? 1 : 0;
        }
        if (dict.ContainsKey(7))
        {
          int num2 = await SyncStatusDao.DeleteSyncStatus(taskId, 7) ? 1 : 0;
        }
        if (dict.ContainsKey(5))
        {
          dict = (Dictionary<int, SyncStatusModel>) null;
          return;
        }
        if (dict.ContainsKey(6))
        {
          dict = (Dictionary<int, SyncStatusModel>) null;
          return;
        }
      }
      int num = await SyncStatusDao.Insert(new SyncStatusModel()
      {
        Type = 5,
        EntityId = taskId,
        UserId = Utils.GetCurrentUserIdInt().ToString()
      });
      dict = (Dictionary<int, SyncStatusModel>) null;
    }

    public static async Task<List<string>> GetEntityIdsByType(int type)
    {
      return await BaseDao<SyncStatusModel>.GetEntityIds(string.Format("select EntityId as Id from SyncStatusModel Where Type = {0} and UserId = '{1}'", (object) type, (object) LocalSettings.Settings.LoginUserId));
    }

    public static async Task<HashSet<string>> GetEntityIdSetByType(int type)
    {
      return await BaseDao<SyncStatusModel>.GetEntityIdSet(string.Format("select EntityId as Id from SyncStatusModel Where Type = {0} and UserId = '{1}'", (object) type, (object) LocalSettings.Settings.LoginUserId));
    }

    public static async Task DeleteEmptySyncStatus()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<SyncStatusModel> listAsync = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (v => string.IsNullOrEmpty(v.EntityId) && v.UserId == userId)).ToListAsync();
      if (listAsync.Count <= 0)
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task<bool> DeleteSyncStatus(string entityId, int type, bool checkRecommit = true)
    {
      long lastSyncStartPoint = LocalSettings.Settings.SyncPoint;
      List<SyncStatusModel> listAsync = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (v => v.EntityId == entityId && v.Type == type)).ToListAsync();
      bool reChanged = false;
      if (listAsync.Count > 0)
      {
        foreach (SyncStatusModel status in listAsync)
        {
          if (status.ModifyPoint > lastSyncStartPoint & checkRecommit)
          {
            SyncService.NeedRecommit = true;
          }
          else
          {
            int num = await App.Connection.DeleteAsync((object) status);
            if (status.ModifyPoint > lastSyncStartPoint)
              reChanged = true;
          }
        }
      }
      return reChanged;
    }

    public static async Task<Dictionary<int, SyncStatusModel>> GetSyncStatusMap(string taskSid)
    {
      Dictionary<int, SyncStatusModel> dict = new Dictionary<int, SyncStatusModel>();
      List<SyncStatusModel> listAsync = await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (v => v.EntityId == taskSid)).ToListAsync();
      if (listAsync.Count > 0)
      {
        foreach (SyncStatusModel syncStatusModel in listAsync)
        {
          if (!dict.ContainsKey(syncStatusModel.Type))
            dict.Add(syncStatusModel.Type, syncStatusModel);
        }
      }
      Dictionary<int, SyncStatusModel> syncStatusMap = dict;
      dict = (Dictionary<int, SyncStatusModel>) null;
      return syncStatusMap;
    }

    public static async Task<List<SyncStatusModel>> GetAllSyncStatus()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (model => model.UserId == userId)).ToListAsync();
    }

    public static async Task<bool> CheckUnSyncItem()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<SyncStatusModel>().Where((Expression<Func<SyncStatusModel, bool>>) (model => model.UserId == userId)).CountAsync() == 0;
    }

    public static async Task BatchAddTaskParentStatus(List<string> ids)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<SyncStatusModel> syncStatusByType = await SyncStatusDao.GetSyncStatusByType(16);
      List<string> existEntities = syncStatusByType != null ? syncStatusByType.Select<SyncStatusModel, string>((Func<SyncStatusModel, string>) (s => s.EntityId)).ToList<string>() : (List<string>) null;
      List<SyncStatusModel> list = ids.Where<string>((Func<string, bool>) (id => existEntities == null || !existEntities.Contains(id))).Select<string, SyncStatusModel>((Func<string, SyncStatusModel>) (id => new SyncStatusModel()
      {
        Type = 16,
        EntityId = id,
        UserId = userId,
        ModifyPoint = DateTime.Now.Ticks
      })).ToList<SyncStatusModel>();
      if (!list.Any<SyncStatusModel>())
        ;
      else
      {
        int num = await App.Connection.InsertAllAsync((IEnumerable) list);
      }
    }

    public static async Task AddSetParentSyncStatus(string taskId, string oldParent)
    {
      List<SyncStatusModel> syncStatusByType = await SyncStatusDao.GetSyncStatusByType(16);
      List<SyncStatusModel> exists = syncStatusByType != null ? syncStatusByType.Where<SyncStatusModel>((Func<SyncStatusModel, bool>) (s => s.EntityId == taskId)).ToList<SyncStatusModel>() : (List<SyncStatusModel>) null;
      List<SyncStatusModel> syncStatusModelList = exists;
      // ISSUE: explicit non-virtual call
      if ((syncStatusModelList != null ? (__nonvirtual (syncStatusModelList.Count) > 0 ? 1 : 0) : 0) != 0)
      {
        for (int i = 0; i < exists.Count; ++i)
        {
          SyncStatusModel syncStatusModel = exists[i];
          if (i != 0)
          {
            int num = await App.Connection.DeleteAsync((object) syncStatusModel);
          }
        }
        exists = (List<SyncStatusModel>) null;
      }
      else
      {
        string str = Utils.GetCurrentUserIdInt().ToString();
        int num = await App.Connection.InsertAsync((object) new SyncStatusModel()
        {
          Type = 16,
          EntityId = taskId,
          UserId = str,
          ModifyPoint = DateTime.Now.Ticks,
          OldParentId = oldParent
        });
        exists = (List<SyncStatusModel>) null;
      }
    }

    public static async Task AddColumnMoveProjectStatus(string columnId, string moveFromId)
    {
      await SyncStatusDao.AddMoveOrRestoreProjectStatus(columnId, moveFromId, 32);
    }
  }
}
