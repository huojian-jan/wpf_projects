// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TaskSortOrderInPriorityDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync.Model;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TaskSortOrderInPriorityDao
  {
    public static async Task UpdateAsync(TaskSortOrderInPriorityModel model)
    {
      int num = await App.Connection.UpdateAsync((object) model);
    }

    public static async Task InsertAsync(TaskSortOrderInPriorityModel model)
    {
      int num = await App.Connection.InsertAsync((object) model);
    }

    public static async Task DeleteAsync(TaskSortOrderInPriorityModel model)
    {
      int num = await TaskSortOrderInPriorityDao.DeleteTaskSortOrder(model);
    }

    public static async Task<List<TaskSortOrderInPriorityModel>> GetSortOrders(
      string catId = null,
      int? priority = null,
      string entityId = null,
      bool containDelete = false)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      AsyncTableQuery<TaskSortOrderInPriorityModel> asyncTableQuery = App.Connection.Table<TaskSortOrderInPriorityModel>().Where((Expression<Func<TaskSortOrderInPriorityModel, bool>>) (order => order.UserId == userId));
      if (!string.IsNullOrEmpty(catId))
        asyncTableQuery = asyncTableQuery.Where((Expression<Func<TaskSortOrderInPriorityModel, bool>>) (order => order.CatId == catId));
      if (priority.HasValue)
        asyncTableQuery = asyncTableQuery.Where((Expression<Func<TaskSortOrderInPriorityModel, bool>>) (order => (int?) order.Priority == priority));
      if (!containDelete)
        asyncTableQuery = asyncTableQuery.Where((Expression<Func<TaskSortOrderInPriorityModel, bool>>) (order => order.Deleted == 0));
      if (!string.IsNullOrEmpty(entityId))
        asyncTableQuery = asyncTableQuery.Where((Expression<Func<TaskSortOrderInPriorityModel, bool>>) (order => order.EntityId == entityId));
      return await asyncTableQuery.ToListAsync();
    }

    public static async Task SaveRemoteChanges(SyncDataBean<TaskSortOrderInPriorityModel> bean)
    {
      if (bean.Addeds.Any<TaskSortOrderInPriorityModel>())
      {
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) bean.Addeds);
      }
      if (bean.Updateds.Any<TaskSortOrderInPriorityModel>())
      {
        int num2 = await App.Connection.UpdateAllAsync((IEnumerable) bean.Updateds);
      }
      if (!bean.Deleteds.Any<TaskSortOrderInPriorityModel>())
        return;
      foreach (object deleted in bean.Deleteds)
        App.Connection.DeleteAsync(deleted);
    }

    private static async Task<int> DeleteTaskSortOrder(TaskSortOrderInPriorityModel item)
    {
      Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.ExecuteAsync("DELETE FROM TaskSortOrderInPriorityModel WHERE CatId=? and EntityId=? and UserId=? and Priority=?", (object) item.CatId, (object) item.EntityId, (object) item.UserId, (object) item.Priority);
    }

    public static async Task SavePostResult(
      SyncDataBean<TaskSortOrderInPriorityModel> dateBean)
    {
      if (dateBean != null)
      {
        if (dateBean.Deleteds != null && dateBean.Deleteds.Count > 0)
        {
          foreach (TaskSortOrderInPriorityModel deleted in dateBean.Deleteds)
            await TaskSortOrderInPriorityDao.DeleteAsync(deleted);
        }
        if (dateBean.Updateds != null && dateBean.Updateds.Count > 0)
        {
          foreach (TaskSortOrderInPriorityModel updated in dateBean.Updateds)
            updated.SyncStatus = 2;
          int num = await App.Connection.UpdateAllAsync((IEnumerable) dateBean.Updateds);
        }
      }
      int num1 = await TaskSortOrderInPriorityDao.ClearDeletedOrders();
    }

    private static async Task<int> ClearDeletedOrders()
    {
      return await App.Connection.ExecuteAsync("DELETE FROM TaskSortOrderInPriorityModel WHERE Deleted!=? and SyncStatus=?", (object) 0, (object) 2);
    }

    public static async Task<List<TaskSortOrderInPriorityModel>> GetAllSortOrders()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TaskSortOrderInPriorityModel>().Where((Expression<Func<TaskSortOrderInPriorityModel, bool>>) (v => v.UserId == userId && v.SyncStatus == 2 && v.Deleted == 0)).ToListAsync();
    }

    public static async Task<Dictionary<string, Dictionary<string, List<TaskSortOrderInPriorityModel>>>> GetNeedPostOrdersMap()
    {
      Dictionary<string, Dictionary<string, List<TaskSortOrderInPriorityModel>>> result = new Dictionary<string, Dictionary<string, List<TaskSortOrderInPriorityModel>>>();
      List<TaskSortOrderInPriorityModel> needPostSortOrders = await TaskSortOrderInPriorityDao.GetNeedPostSortOrders();
      if (needPostSortOrders != null && needPostSortOrders.Any<TaskSortOrderInPriorityModel>())
      {
        foreach (TaskSortOrderInPriorityModel orderInPriorityModel in needPostSortOrders)
        {
          string key = orderInPriorityModel.Priority.ToString();
          string catId = orderInPriorityModel.CatId;
          if (!result.ContainsKey(key))
            result.Add(key, new Dictionary<string, List<TaskSortOrderInPriorityModel>>());
          if (!result[key].ContainsKey(catId))
            result[key].Add(catId, new List<TaskSortOrderInPriorityModel>());
          result[key][catId].Add(orderInPriorityModel);
        }
      }
      Dictionary<string, Dictionary<string, List<TaskSortOrderInPriorityModel>>> needPostOrdersMap = result;
      result = (Dictionary<string, Dictionary<string, List<TaskSortOrderInPriorityModel>>>) null;
      return needPostOrdersMap;
    }

    public static async Task<List<TaskSortOrderInPriorityModel>> GetNeedPostSortOrders()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TaskSortOrderInPriorityModel>().Where((Expression<Func<TaskSortOrderInPriorityModel, bool>>) (v => v.UserId == userId && v.SyncStatus != 2)).ToListAsync();
    }

    public static async Task UpdateAsyncByCategoryIdSetToDelete(string categoryId)
    {
      List<TaskSortOrderInPriorityModel> sortOrders = await TaskSortOrderInPriorityDao.GetSortOrders(categoryId);
      if (sortOrders == null || !sortOrders.Any<TaskSortOrderInPriorityModel>())
        return;
      sortOrders.ForEach((Action<TaskSortOrderInPriorityModel>) (order =>
      {
        order.Deleted = 1;
        order.SyncStatus = 1;
      }));
      int num = await App.Connection.UpdateAllAsync((IEnumerable) sortOrders);
    }

    public static async Task BatchUpdateAffectedTaskOnTagChanged(string original, string newLabel)
    {
      if (!(original != newLabel.ToLower()))
        return;
      List<TaskSortOrderInPriorityModel> orders = await TaskSortOrderInPriorityDao.GetSortOrders("#" + original);
      if (orders != null && orders.Any<TaskSortOrderInPriorityModel>())
      {
        List<TaskSortOrderInPriorityModel> items = new List<TaskSortOrderInPriorityModel>();
        foreach (TaskSortOrderInPriorityModel orderInPriorityModel in orders)
          items.Add(new TaskSortOrderInPriorityModel()
          {
            UserId = orderInPriorityModel.UserId,
            CatId = "#" + newLabel.ToLower(),
            EntityId = orderInPriorityModel.EntityId,
            EntityType = orderInPriorityModel.EntityType,
            Priority = orderInPriorityModel.Priority,
            SortOrder = orderInPriorityModel.SortOrder,
            SyncStatus = 0,
            Deleted = orderInPriorityModel.Deleted
          });
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) items);
        orders.ForEach((Action<TaskSortOrderInPriorityModel>) (order =>
        {
          order.Deleted = 1;
          order.SyncStatus = 1;
        }));
        int num2 = await App.Connection.UpdateAllAsync((IEnumerable) orders);
      }
      orders = (List<TaskSortOrderInPriorityModel>) null;
    }

    public static async Task DeleteRemoteDeleted(List<string> projects)
    {
      if (!projects.Any<string>())
        return;
      foreach (string project in projects)
      {
        List<TaskSortOrderInPriorityModel> sortOrders = await TaskSortOrderInPriorityDao.GetSortOrders(project);
        if (sortOrders != null && sortOrders.Any<TaskSortOrderInPriorityModel>())
        {
          foreach (TaskSortOrderInPriorityModel model in sortOrders)
            await TaskSortOrderInPriorityDao.DeleteAsync(model);
        }
      }
    }

    public static async Task SyncOnEventCalendarChanged(string originalId, string revisedId)
    {
      int num = await App.Connection.ExecuteAsync("UPDATE $TaskSortOrderInPriorityModel SET EntityId=?, SyncStatus=? WHERE EntityId=?", (object) revisedId, (object) 1, (object) originalId);
    }

    public static async Task NewTaskAdded(TaskModel task, string catId)
    {
      if (task == null)
        return;
      List<TaskSortOrderInPriorityModel> sortOrders = await TaskSortOrderInPriorityDao.GetSortOrders(catId, new int?(task.priority));
      if (sortOrders == null || sortOrders.Count == 0)
        return;
      TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
      TaskSortOrderInPriorityModel orderInPriorityModel = new TaskSortOrderInPriorityModel()
      {
        EntityId = task.id,
        EntityType = nameof (task),
        Priority = task.priority,
        CatId = catId,
        SyncStatus = 0,
        UserId = LocalSettings.Settings.LoginUserId
      };
      if (defaultSafely.AddTo == 0)
      {
        long num = Math.Min(0L, sortOrders.Select<TaskSortOrderInPriorityModel, long>((Func<TaskSortOrderInPriorityModel, long>) (o => o.SortOrder)).Min());
        orderInPriorityModel.SortOrder = num - 268435456L;
      }
      else
      {
        long num = Math.Max(0L, sortOrders.Select<TaskSortOrderInPriorityModel, long>((Func<TaskSortOrderInPriorityModel, long>) (o => o.SortOrder)).Max());
        orderInPriorityModel.SortOrder = num + 268435456L;
      }
      int num1 = await App.Connection.InsertAsync((object) orderInPriorityModel);
    }

    public static async Task TryAddSplitTaskOrder(
      string projectId,
      int priority,
      string baseTaskId,
      string insertTaskId,
      bool below)
    {
      List<TaskSortOrderInPriorityModel> sortOrders = await TaskSortOrderInPriorityDao.GetSortOrders(projectId, new int?(priority));
      if (sortOrders == null || sortOrders.Count == 0)
        return;
      List<TaskSortOrderInPriorityModel> list = sortOrders.OrderBy<TaskSortOrderInPriorityModel, long>((Func<TaskSortOrderInPriorityModel, long>) (o => o.SortOrder)).ToList<TaskSortOrderInPriorityModel>();
      int index1 = -1;
      for (int index2 = 0; index2 < list.Count; ++index2)
      {
        if (list[index2].EntityId == baseTaskId)
        {
          index1 = index2;
          break;
        }
      }
      if (index1 < 0)
        return;
      long num1 = index1 != 0 || below ? (!(index1 == sortOrders.Count - 1 & below) ? MathUtil.LongAvg(list[index1].SortOrder, list[below ? index1 + 1 : index1 - 1].SortOrder) : list[sortOrders.Count - 1].SortOrder + 268435456L) : list[0].SortOrder - 268435456L;
      int num2 = await App.Connection.InsertAsync((object) new TaskSortOrderInPriorityModel()
      {
        EntityId = insertTaskId,
        EntityType = "task",
        Priority = priority,
        CatId = projectId,
        SyncStatus = 0,
        UserId = LocalSettings.Settings.LoginUserId,
        SortOrder = num1
      });
    }
  }
}
