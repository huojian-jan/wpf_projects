// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.SortOrder.TaskSortOrderService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service.SortOrder
{
  public class TaskSortOrderService
  {
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>>> CachedOrders = new ConcurrentDictionary<string, ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>>>();

    public static async Task<List<SyncSortOrderModel>> GetAsyncList(
      string sortOrderType,
      string groupId,
      bool force = false)
    {
      return (await TaskSortOrderService.GetAsync(sortOrderType, groupId, force))?.ToList();
    }

    private static async Task<BlockingList<SyncSortOrderModel>> GetAsync(
      string sortOrderType,
      string groupId,
      bool force = false)
    {
      if (force)
      {
        int num1 = await TaskSortOrderService.FetchAsync(sortOrderType);
      }
      if (string.IsNullOrEmpty(groupId) || string.IsNullOrEmpty(sortOrderType))
        return (BlockingList<SyncSortOrderModel>) null;
      ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>> concurrentDictionary1;
      BlockingList<SyncSortOrderModel> async1;
      if (TaskSortOrderService.CachedOrders.TryGetValue(sortOrderType, out concurrentDictionary1) && concurrentDictionary1.TryGetValue(groupId, out async1))
        return async1;
      int num2 = await TaskSortOrderService.FetchAsync(sortOrderType);
      ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>> concurrentDictionary2;
      BlockingList<SyncSortOrderModel> async2;
      if (TaskSortOrderService.CachedOrders.TryGetValue(sortOrderType, out concurrentDictionary2) && concurrentDictionary2.TryGetValue(groupId, out async2))
        return async2;
      BlockingList<SyncSortOrderModel> addValue = new BlockingList<SyncSortOrderModel>();
      if (!TaskSortOrderService.CachedOrders.ContainsKey(sortOrderType))
      {
        ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>> concurrentDictionary3 = new ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>>();
        concurrentDictionary3.TryAdd(groupId, addValue);
        TaskSortOrderService.CachedOrders.TryAdd(sortOrderType, concurrentDictionary3);
      }
      else
        TaskSortOrderService.CachedOrders[sortOrderType].AddOrUpdate(groupId, addValue, (Func<string, BlockingList<SyncSortOrderModel>, BlockingList<SyncSortOrderModel>>) ((key, oldValue) => oldValue));
      return addValue;
    }

    public static async Task<int> FetchAsync(string sortOrderType)
    {
      int updateCount = 0;
      List<SyncSortOrderModel> orderBySortOrderType = await SyncSortOrderDao.GetAllOrderBySortOrderType(sortOrderType);
      if (orderBySortOrderType.Any<SyncSortOrderModel>())
      {
        Dictionary<string, Dictionary<string, SyncSortOrderModel>> dictionary1 = new Dictionary<string, Dictionary<string, SyncSortOrderModel>>();
        foreach (SyncSortOrderModel model in orderBySortOrderType)
        {
          if (!dictionary1.ContainsKey(model.GroupId))
            dictionary1[model.GroupId] = new Dictionary<string, SyncSortOrderModel>();
          Dictionary<string, SyncSortOrderModel> dictionary2 = dictionary1[model.GroupId];
          if (dictionary2.ContainsKey(model.EntityId))
            SyncSortOrderDao.DeleteAsync(model);
          else
            dictionary2[model.EntityId] = model;
        }
        ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>> concurrentDictionary;
        if (!TaskSortOrderService.CachedOrders.TryGetValue(sortOrderType, out concurrentDictionary))
          concurrentDictionary = new ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>>();
        foreach (KeyValuePair<string, Dictionary<string, SyncSortOrderModel>> keyValuePair in dictionary1)
        {
          string key = keyValuePair.Key;
          List<SyncSortOrderModel> list = keyValuePair.Value.Values.ToList<SyncSortOrderModel>();
          BlockingList<SyncSortOrderModel> blockingList1;
          if (concurrentDictionary.TryGetValue(key, out blockingList1))
          {
            blockingList1.Clear();
            blockingList1.AddRange((IEnumerable<SyncSortOrderModel>) list);
          }
          else
          {
            BlockingList<SyncSortOrderModel> blockingList2 = new BlockingList<SyncSortOrderModel>((IEnumerable<SyncSortOrderModel>) list);
            concurrentDictionary.TryAdd(key, blockingList2);
          }
          TaskSortOrderService.CachedOrders.TryAdd(sortOrderType, concurrentDictionary);
          ++updateCount;
        }
      }
      else
        TaskSortOrderService.CachedOrders.TryRemove(sortOrderType, out ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>> _);
      return updateCount;
    }

    public static async Task FetchAllAsync()
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
      TaskSortOrderService.\u003C\u003Ec__DisplayClass4_0 cDisplayClass40 = new TaskSortOrderService.\u003C\u003Ec__DisplayClass4_0();
      TaskSortOrderService.CachedOrders.Clear();
    }

    public static async Task DeleteAllSortOrderBySortOptionInListId(SortOption option, string id)
    {
      Regex regex = new Regex("taskBy#" + option.groupBy + ".+_" + option.orderBy);
      foreach (string str in TaskSortOrderService.CachedOrders.Keys.ToList<string>())
      {
        ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>> concurrentDictionary;
        if (regex.IsMatch(str) && TaskSortOrderService.CachedOrders.TryGetValue(str, out concurrentDictionary))
        {
          foreach (string key in (IEnumerable<string>) concurrentDictionary.Keys)
          {
            if (key == id)
              concurrentDictionary[key]?.Clear();
          }
        }
      }
      if (await SyncSortOrderDao.SetAllDeletedByGroupId(option, id) <= 0)
        return;
      SyncManager.TryDelaySync();
    }

    public static async Task<SyncSortOrderModel> InsertOrUpdateAsync(
      string sortOrderType,
      string groupId,
      string entityId,
      int type = 1,
      long? sortOrder = null)
    {
      BlockingList<SyncSortOrderModel> async = await TaskSortOrderService.GetAsync(sortOrderType, groupId);
      if (async == null)
        return (SyncSortOrderModel) null;
      List<long> source = async.SelectList<long>((Func<SyncSortOrderModel, long>) (m => m.SortOrder));
      SyncSortOrderModel orderModel = async.FirstOrDefault((Func<SyncSortOrderModel, bool>) (item => item.EntityId == entityId));
      if (orderModel == null)
      {
        orderModel = new SyncSortOrderModel(sortOrderType)
        {
          EntityId = entityId,
          SortOrder = sortOrder ?? (source.Any<long>() ? source.Min() - 268435456L : 0L),
          GroupId = groupId,
          Type = type,
          SyncStatus = 1
        };
        async.Add(orderModel);
      }
      else
      {
        orderModel.SortOrder = sortOrder ?? (source.Any<long>() ? source.Min() - 268435456L : 0L);
        orderModel.SyncStatus = 1;
        orderModel.Type = type;
      }
      int num = await SyncSortOrderDao.InsertOrUpdate(orderModel);
      SyncManager.TryDelaySync();
      return orderModel;
    }

    public static async Task<SyncSortOrderModel> InsertAfterAsync(
      string sortOrderType,
      string groupId,
      string entityId,
      int type = 1,
      string columnId = "",
      string targetId = "")
    {
      if (string.IsNullOrWhiteSpace(columnId))
        columnId = "";
      List<SyncSortOrderModel> list = (await TaskSortOrderService.GetAsync(sortOrderType, groupId))?.ToList();
      if (list == null)
        return (SyncSortOrderModel) null;
      list.Sort((Comparison<SyncSortOrderModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
      SyncSortOrderModel syncSortOrderModel = list.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (item => item.EntityId == targetId));
      long num1;
      if (syncSortOrderModel != null)
      {
        int num2 = list.IndexOf(syncSortOrderModel);
        num1 = num2 < list.Count - 1 ? syncSortOrderModel.SortOrder / 2L + list[num2 + 1].SortOrder / 2L : syncSortOrderModel.SortOrder + 268435456L;
      }
      else
        num1 = list.Any<SyncSortOrderModel>() ? list.Min<SyncSortOrderModel>((Func<SyncSortOrderModel, long>) (o => o.SortOrder)) - 268435456L : 0L;
      SyncSortOrderModel pinModel = list.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (item => item.EntityId == entityId));
      if (pinModel == null)
      {
        pinModel = new SyncSortOrderModel(sortOrderType)
        {
          EntityId = entityId,
          GroupId = groupId,
          Type = type,
          SyncStatus = 1,
          SortOrder = num1
        };
        list.Add(pinModel);
      }
      else
      {
        pinModel.SyncStatus = 1;
        pinModel.SortOrder = num1;
      }
      TaskSortOrderService.CachedOrders[sortOrderType][groupId] = new BlockingList<SyncSortOrderModel>((IEnumerable<SyncSortOrderModel>) list);
      int num3 = await SyncSortOrderDao.InsertOrUpdate(pinModel);
      SyncManager.TryDelaySync();
      return pinModel;
    }

    public static async Task SaveInits(
      Dictionary<string, Dictionary<string, Dictionary<string, SyncSortOrderModel>>> dict)
    {
      if (dict == null)
        return;
      foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, SyncSortOrderModel>>> keyValuePair1 in dict)
      {
        string sortOrderType = keyValuePair1.Key;
        foreach (KeyValuePair<string, Dictionary<string, SyncSortOrderModel>> keyValuePair2 in keyValuePair1.Value)
        {
          string key = keyValuePair2.Key;
          List<SyncSortOrderModel> models = keyValuePair2.Value.Values.ToList<SyncSortOrderModel>();
          List<SyncSortOrderModel> list = (await TaskSortOrderService.GetAsync(sortOrderType, key))?.ToList();
          if (list != null && list.Count > 0)
          {
            HashSet<string> existEntities = new HashSet<string>(list.Select<SyncSortOrderModel, string>((Func<SyncSortOrderModel, string>) (e => e.EntityId)));
            models.RemoveAll((Predicate<SyncSortOrderModel>) (m => existEntities.Contains(m.EntityId)));
          }
          int num = await SyncSortOrderDao.InsertAllAsync(models);
          models = (List<SyncSortOrderModel>) null;
        }
        sortOrderType = (string) null;
      }
    }

    public static async Task DeleteAllAsync(
      string sortOrderType,
      string groupId,
      List<string> ids,
      string columnId = "")
    {
      if (ids == null)
        ;
      else if (!ids.Any<string>())
        ;
      else
      {
        ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>> concurrentDictionary;
        if (TaskSortOrderService.CachedOrders.TryGetValue(sortOrderType, out concurrentDictionary))
        {
          BlockingList<SyncSortOrderModel> list;
          if (concurrentDictionary.TryGetValue(groupId, out list))
          {
            foreach (string id in list.Where((Predicate<SyncSortOrderModel>) (item => !ids.Contains(item.EntityId))).Select<SyncSortOrderModel, string>((Func<SyncSortOrderModel, string>) (item => item.EntityId)).ToList<string>())
            {
              SyncSortOrderModel orderByIdAndGroupId = await SyncSortOrderDao.GetSortOrderByIdAndGroupId(id, groupId);
              if (orderByIdAndGroupId != null)
                await SyncSortOrderDao.DeleteAsync(orderByIdAndGroupId);
            }
            list.RemoveAll((Predicate<SyncSortOrderModel>) (item => ids.Contains(item.EntityId) && item.ColumnId == columnId));
          }
          list = (BlockingList<SyncSortOrderModel>) null;
        }
        SyncManager.TryDelaySync();
      }
    }

    public static async Task<List<SyncSortOrderModel>> BatchResetSortOrder(
      string sortOrderType,
      string catId,
      List<(string, int)> ids,
      string columnId = "")
    {
      List<SyncSortOrderModel> result = new List<SyncSortOrderModel>();
      if (ids == null)
        return result;
      await TaskSortOrderService.DeleteAllAsync(sortOrderType, catId, ids.Select<(string, int), string>((Func<(string, int), string>) (i => i.Item1)).ToList<string>());
      BlockingList<SyncSortOrderModel> items = await TaskSortOrderService.GetAsync(sortOrderType, catId);
      int count = ids.Count;
      for (int i = 0; i < count; ++i)
      {
        (string entityId, int type) = ids[i];
        SyncSortOrderModel item = items?.FirstOrDefault((Func<SyncSortOrderModel, bool>) (m => m.EntityId == entityId));
        long num = 268435456L * (long) i;
        if (item != null)
        {
          item.SortOrder = num;
          SyncSortOrderModel syncSortOrderModel = await TaskSortOrderService.InsertOrUpdateAsync(sortOrderType, catId, entityId, type, new long?(num));
        }
        else
          item = await TaskSortOrderService.InsertOrUpdateAsync(sortOrderType, catId, entityId, type, new long?(num));
        result.Add(item);
        item = (SyncSortOrderModel) null;
      }
      if (result.Any<SyncSortOrderModel>())
        SyncManager.TryDelaySync();
      return result;
    }

    public static async Task NewTaskAdded(
      string taskId,
      string catId,
      SortOption sortOption,
      string frontId = null)
    {
      string key;
      if (string.IsNullOrEmpty(taskId))
        key = (string) null;
      else if (sortOption == null)
      {
        key = (string) null;
      }
      else
      {
        if (catId.Contains("_special_id_"))
        {
          UtilLog.Info("NewTaskAdded in _special_id_");
          catId = "all";
        }
        key = sortOption.GetSortKey();
        List<SyncSortOrderModel> asyncList = await TaskSortOrderService.GetAsyncList(key, catId);
        if (asyncList == null)
          key = (string) null;
        else if (asyncList.Count == 0)
        {
          key = (string) null;
        }
        else
        {
          long num1 = long.MaxValue;
          if (!string.IsNullOrEmpty(frontId))
          {
            SyncSortOrderModel syncSortOrderModel1 = asyncList.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (m => m.EntityId == frontId));
            if (syncSortOrderModel1 != null)
            {
              asyncList.Sort((Comparison<SyncSortOrderModel>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
              int num2 = asyncList.IndexOf(syncSortOrderModel1);
              if (num2 >= asyncList.Count - 1)
              {
                num1 = syncSortOrderModel1.SortOrder + 268435456L;
              }
              else
              {
                SyncSortOrderModel syncSortOrderModel2 = asyncList[num2 + 1];
                num1 = syncSortOrderModel1.SortOrder + (syncSortOrderModel2.SortOrder - syncSortOrderModel1.SortOrder) / 2L;
              }
              UtilLog.Info(string.Format("NewTaskAdded in {0},id {1},sort {2},front {3}", (object) "_special_id_", (object) taskId, (object) num1, (object) syncSortOrderModel1.SortOrder));
            }
          }
          if (num1 == long.MaxValue)
            num1 = TaskDefaultDao.GetDefaultSafely().AddTo != 0 ? Math.Max(0L, asyncList.Select<SyncSortOrderModel, long>((Func<SyncSortOrderModel, long>) (o => o.SortOrder)).Max()) + 268435456L : Math.Min(0L, asyncList.Select<SyncSortOrderModel, long>((Func<SyncSortOrderModel, long>) (o => o.SortOrder)).Min()) - 268435456L;
          SyncSortOrderModel syncSortOrderModel = await TaskSortOrderService.InsertOrUpdateAsync(key, catId, taskId, sortOrder: new long?(num1));
          key = (string) null;
        }
      }
    }

    public static async Task BatchResetOrders(
      List<DisplayItemModel> models,
      string sortKey,
      string cId)
    {
      List<SyncSortOrderModel> sortOrders;
      if (models == null)
        sortOrders = (List<SyncSortOrderModel>) null;
      else if (models.Count == 0)
      {
        sortOrders = (List<SyncSortOrderModel>) null;
      }
      else
      {
        long max = long.MinValue;
        long min = long.MaxValue;
        foreach (DisplayItemModel model in models)
        {
          if (model.SpecialOrder != long.MaxValue && model.SpecialOrder != long.MinValue)
          {
            if (model.SpecialOrder >= max)
              max = model.SpecialOrder;
            if (model.SpecialOrder <= min)
              min = model.SpecialOrder;
          }
        }
        if (max == long.MinValue)
        {
          max = 1L + 268435456L * (long) (models.Count - 1);
          min = 1L;
        }
        if (Math.Abs(max - min) < (long) (models.Count * 256))
        {
          max = Math.Max(9223372036586340351L, min + 268435456L);
          min = max - 268435456L;
        }
        sortOrders = await TaskSortOrderService.GetAsyncList(sortKey, cId, true) ?? new List<SyncSortOrderModel>();
        long sortOrder = min;
        long step = models.Count <= 1 ? 0L : (max - min) / (long) (models.Count - 1);
        foreach (DisplayItemModel model in models)
        {
          DisplayItemModel m = model;
          List<SyncSortOrderModel> source = sortOrders;
          SyncSortOrderModel sortModel = source != null ? source.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (s => s.EntityId == m.EntityId)) : (SyncSortOrderModel) null;
          int entityTypeNum = EntityType.GetEntityTypeNum(m.Type);
          if (sortModel == null)
          {
            sortModel = new SyncSortOrderModel(sortKey)
            {
              EntityId = m.EntityId,
              SortOrder = sortOrder,
              GroupId = cId,
              Type = entityTypeNum,
              SyncStatus = 1
            };
            int num = await SyncSortOrderDao.InsertAsync(sortModel);
            sortOrders.Add(sortModel);
          }
          else
          {
            sortModel.SortOrder = sortOrder;
            sortModel.SyncStatus = 1;
            sortModel.Type = entityTypeNum;
            int num = await SyncSortOrderDao.UpdateAsync(sortModel);
          }
          m.SpecialOrder = sortOrder;
          sortOrder += step;
          sortModel = (SyncSortOrderModel) null;
        }
        TaskSortOrderService.SetSortModels(sortKey, cId, sortOrders);
        sortOrders = (List<SyncSortOrderModel>) null;
      }
    }

    public static void SetSortModels(string sortKey, string cId, List<SyncSortOrderModel> result)
    {
      if (string.IsNullOrEmpty(sortKey) || string.IsNullOrEmpty(cId) || result == null)
        return;
      if (!TaskSortOrderService.CachedOrders.ContainsKey(sortKey))
      {
        ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>> concurrentDictionary = new ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>>();
        concurrentDictionary.TryAdd(cId, new BlockingList<SyncSortOrderModel>((IEnumerable<SyncSortOrderModel>) result));
        TaskSortOrderService.CachedOrders.TryAdd(sortKey, concurrentDictionary);
      }
      else
        TaskSortOrderService.CachedOrders[sortKey][cId] = new BlockingList<SyncSortOrderModel>((IEnumerable<SyncSortOrderModel>) result);
    }

    public static void AddSortModels(string sortKey, string cId, List<SyncSortOrderModel> result)
    {
      if (string.IsNullOrEmpty(sortKey) || string.IsNullOrEmpty(cId) || result == null)
        return;
      if (!TaskSortOrderService.CachedOrders.ContainsKey(sortKey))
      {
        ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>> concurrentDictionary = new ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>>();
        concurrentDictionary.TryAdd(cId, new BlockingList<SyncSortOrderModel>((IEnumerable<SyncSortOrderModel>) result));
        TaskSortOrderService.CachedOrders.TryAdd(sortKey, concurrentDictionary);
      }
      else
      {
        ConcurrentDictionary<string, BlockingList<SyncSortOrderModel>> cachedOrder = TaskSortOrderService.CachedOrders[sortKey];
        if (cachedOrder.ContainsKey(cId) && cachedOrder[cId] != null)
          cachedOrder[cId].AddRange((IEnumerable<SyncSortOrderModel>) result);
        else
          cachedOrder[cId] = new BlockingList<SyncSortOrderModel>((IEnumerable<SyncSortOrderModel>) result);
      }
    }

    public static async Task OnColumnInit(string projectId, string defaultColumnId)
    {
      List<SyncSortOrderModel> columnOrderInProject = await SyncSortOrderDao.GetNoColumnOrderInProject(projectId);
      List<SyncSortOrderModel> newOrders;
      if (columnOrderInProject.Count == 0)
      {
        newOrders = (List<SyncSortOrderModel>) null;
      }
      else
      {
        newOrders = new List<SyncSortOrderModel>();
        foreach (SyncSortOrderModel from in columnOrderInProject)
        {
          SyncSortOrderModel dest = new SyncSortOrderModel();
          SyncSortOrderModel.Copy(dest, from);
          dest.SyncStatus = 1;
          dest.SortOrderType = from.SortOrderType.Replace("none", defaultColumnId);
          newOrders.Add(dest);
          from.SyncStatus = 1;
          from.Deleted = 1;
        }
        int num1 = await SyncSortOrderDao.UpdateAllAsync(columnOrderInProject);
        int num2 = await SyncSortOrderDao.InsertAllAsync(newOrders);
        newOrders = (List<SyncSortOrderModel>) null;
      }
    }
  }
}
