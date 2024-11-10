// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.SyncSortOrderDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service.SortOrder;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync.Model;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class SyncSortOrderDao : BaseModel
  {
    public static async Task<int> InsertAllAsync(List<SyncSortOrderModel> models)
    {
      return await App.Connection.InsertAllAsync((IEnumerable) models);
    }

    public static async Task<int> InsertAsync(SyncSortOrderModel model)
    {
      return await App.Connection.InsertAsync((object) model);
    }

    public static async Task<int> UpdateAllAsync(List<SyncSortOrderModel> models)
    {
      return await App.Connection.UpdateAllAsync((IEnumerable) models);
    }

    public static async Task<int> UpdateAsync(SyncSortOrderModel model)
    {
      return await App.Connection.UpdateAsync((object) model);
    }

    public static async Task<int> DeleteAllAsync(List<SyncSortOrderModel> models)
    {
      return ((IEnumerable<int>) await Task.WhenAll<int>(models.Select<SyncSortOrderModel, Task<int>>(new Func<SyncSortOrderModel, Task<int>>(App.Connection.DeleteAsync)))).Sum();
    }

    public static async Task<List<SyncSortOrderModel>> GetAllOrderBySortOrderType(string type)
    {
      return await App.Connection.QueryAsync<SyncSortOrderModel>("Select * from SyncSortOrderModel where " + string.Format(" UserId = '{0}' and SortOrderType = '{1}' and Deleted = {2}", (object) LocalSettings.Settings.LoginUserId, (object) type, (object) 0));
    }

    public static async Task<SyncSortOrderModel> GetSortOrderByIdAndType(
      string sortType,
      string id,
      int type)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (v => v.SortOrderType == sortType && v.UserId == userId && v.Deleted == 0 && v.EntityId == id && v.Type == type)).FirstOrDefaultAsync();
    }

    public static async Task<SyncSortOrderModel> GetSortOrderByIdAndGroupId(
      string id,
      string groupId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (v => v.UserId == userId && v.Deleted == 0 && v.EntityId == id && v.GroupId == groupId)).FirstOrDefaultAsync();
    }

    public static async Task<List<SyncSortOrderModel>> SetAllTaskOrderInTag()
    {
      return await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (v => v.UserId == LocalSettings.Settings.LoginUserId && v.SortOrderType.StartsWith("taskByTag") && v.Deleted == 0)).ToListAsync();
    }

    public static async Task<int> SetAllDeletedByGroupId(SortOption option, string id)
    {
      return await App.Connection.ExecuteAsync("UPDATE SyncSortOrderModel" + string.Format(" SET {0}={1}", (object) "Deleted", (object) 1) + string.Format(", {0}={1}", (object) "SyncStatus", (object) 1) + " WHERE GroupId=? AND UserId='" + LocalSettings.Settings.LoginUserId + "'  AND SortOrderType LIKE 'taskBy#" + option.groupBy + "|%" + option.orderBy + "'", (object) id);
    }

    public static async Task<int> CountTagSortOrderByGroupId(string id)
    {
      return await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (model => model.SortOrderType.StartsWith("taskByTag") && model.GroupId == id && model.Deleted == 0)).CountAsync();
    }

    public static async Task<List<SyncSortOrderModel>> GetLocalSortOrders()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (v => v.UserId == userId)).ToListAsync();
    }

    public static async Task SaveRemoteChangesToDb(SyncDataBean<SyncSortOrderModel> bean)
    {
      int num1 = await SyncSortOrderDao.InsertAllAsync(bean.Addeds);
      int num2 = await SyncSortOrderDao.UpdateAllAsync(bean.Updateds);
      int num3 = await SyncSortOrderDao.DeleteAllAsync(bean.Deleteds);
      HashSet<string> ids = new HashSet<string>();
      bean.Addeds?.ForEach((Action<SyncSortOrderModel>) (item =>
      {
        if (!(item.SortOrderType != "projectPinned"))
          return;
        ids.Add(item.EntityId);
      }));
      bean.Updateds?.ForEach((Action<SyncSortOrderModel>) (item =>
      {
        if (!(item.SortOrderType != "projectPinned"))
          return;
        ids.Add(item.EntityId);
      }));
      bean.Deleteds?.ForEach((Action<SyncSortOrderModel>) (item =>
      {
        if (!(item.SortOrderType != "projectPinned"))
          return;
        ids.Add(item.EntityId);
      }));
      if (ids.Any<string>())
      {
        await TaskSortOrderService.FetchAllAsync();
        TaskChangeNotifier.NotifyTaskBatchChanged(ids.ToList<string>());
      }
      DataChangedNotifier.NotifyProjectPinRemoteChanged();
    }

    public static async Task<Dictionary<string, Dictionary<string, List<SyncSortOrderModel>>>> GetNeedPostMap()
    {
      Dictionary<string, Dictionary<string, List<SyncSortOrderModel>>> map = new Dictionary<string, Dictionary<string, List<SyncSortOrderModel>>>();
      foreach (SyncSortOrderModel needPostSortOrder in await SyncSortOrderDao.GetNeedPostSortOrders())
      {
        Dictionary<string, List<SyncSortOrderModel>> dictionary1 = (Dictionary<string, List<SyncSortOrderModel>>) null;
        if (map.ContainsKey(needPostSortOrder.SortOrderType))
          dictionary1 = map[needPostSortOrder.SortOrderType];
        if (dictionary1 == null)
        {
          Dictionary<string, List<SyncSortOrderModel>> dictionary2 = new Dictionary<string, List<SyncSortOrderModel>>();
          List<SyncSortOrderModel> syncSortOrderModelList = new List<SyncSortOrderModel>()
          {
            needPostSortOrder
          };
          dictionary2.Add(needPostSortOrder.GroupId, syncSortOrderModelList);
          map.Add(needPostSortOrder.SortOrderType, dictionary2);
        }
        else if (dictionary1.ContainsKey(needPostSortOrder.GroupId))
        {
          dictionary1[needPostSortOrder.GroupId].Add(needPostSortOrder);
        }
        else
        {
          List<SyncSortOrderModel> syncSortOrderModelList = new List<SyncSortOrderModel>()
          {
            needPostSortOrder
          };
          dictionary1.Add(needPostSortOrder.GroupId, syncSortOrderModelList);
        }
      }
      Dictionary<string, Dictionary<string, List<SyncSortOrderModel>>> needPostMap = map;
      map = (Dictionary<string, Dictionary<string, List<SyncSortOrderModel>>>) null;
      return needPostMap;
    }

    public static async Task<List<SyncSortOrderModel>> GetNeedPostSortOrders()
    {
      return await Task.Run<List<SyncSortOrderModel>>((Func<Task<List<SyncSortOrderModel>>>) (async () => await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (v => v.UserId == LocalSettings.Settings.LoginUserId && v.SyncStatus != 2)).ToListAsync()));
    }

    public static async Task SavePostResult(SyncDataBean<SyncSortOrderModel> dateBean)
    {
      if (dateBean != null)
      {
        if (dateBean.Deleteds != null && dateBean.Deleteds.Count > 0)
        {
          foreach (object deleted in dateBean.Deleteds)
          {
            int num = await App.Connection.DeleteAsync(deleted);
          }
        }
        if (dateBean.Updateds != null && dateBean.Updateds.Count > 0)
        {
          foreach (SyncSortOrderModel updated in dateBean.Updateds)
            updated.SyncStatus = 2;
          int num = await App.Connection.UpdateAllAsync((IEnumerable) dateBean.Updateds);
        }
      }
      await SyncSortOrderDao.ClearDeletedOrders();
    }

    private static async Task ClearDeletedOrders()
    {
      string userId = LocalSettings.Settings.LoginUserId;
      List<SyncSortOrderModel> listAsync = await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (v => v.Deleted != 0 && v.SyncStatus == 2 && v.UserId == userId)).ToListAsync();
      if (listAsync == null || listAsync.Count <= 0)
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task<int> InsertOrUpdate(SyncSortOrderModel model)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      SyncSortOrderModel model1 = await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (o => o.SortOrderType == model.SortOrderType && o.GroupId == model.GroupId && o.EntityId == model.EntityId && o.Type == model.Type && o.UserId == userId)).FirstOrDefaultAsync();
      int num;
      if (model1 != null)
      {
        model1.SortOrder = model.SortOrder;
        model1.SyncStatus = model.SyncStatus;
        model1.Deleted = model.Deleted;
        num = await SyncSortOrderDao.UpdateAsync(model1);
      }
      else
        num = await SyncSortOrderDao.InsertAsync(model);
      SyncManager.TryDelaySync();
      return num;
    }

    public static async Task DeleteAsync(SyncSortOrderModel model)
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
      SyncSortOrderDao.\u003C\u003Ec__DisplayClass18_0 cDisplayClass180 = new SyncSortOrderDao.\u003C\u003Ec__DisplayClass18_0();
      // ISSUE: reference to a compiler-generated field
      cDisplayClass180.model = model;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass180.userId = LocalSettings.Settings.LoginUserId;
      ParameterExpression parameterExpression;
      // ISSUE: reference to a compiler-generated field
      // ISSUE: method reference
      // ISSUE: field reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: field reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: field reference
      // ISSUE: method reference
      // ISSUE: method reference
      // ISSUE: field reference
      SyncSortOrderModel model1 = await App.Connection.Table<SyncSortOrderModel>().Where(Expression.Lambda<Func<SyncSortOrderModel, bool>>((Expression) Expression.AndAlso((Expression) Expression.AndAlso((Expression) Expression.AndAlso((Expression) Expression.AndAlso(o.SortOrderType == cDisplayClass180.model.SortOrderType, (Expression) Expression.Equal((Expression) Expression.Call(o.GroupId, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.ToLower)), Array.Empty<Expression>()), (Expression) Expression.Call((Expression) Expression.Property((Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass180, typeof (SyncSortOrderDao.\u003C\u003Ec__DisplayClass18_0)), FieldInfo.GetFieldFromHandle(__fieldref (SyncSortOrderDao.\u003C\u003Ec__DisplayClass18_0.model))), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (SyncSortOrderModel.get_GroupId))), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (string.ToLower)), Array.Empty<Expression>()))), (Expression) Expression.Equal((Expression) Expression.Property((Expression) parameterExpression, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (SyncSortOrderModel.get_EntityId))), (Expression) Expression.Property((Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass180, typeof (SyncSortOrderDao.\u003C\u003Ec__DisplayClass18_0)), FieldInfo.GetFieldFromHandle(__fieldref (SyncSortOrderDao.\u003C\u003Ec__DisplayClass18_0.model))), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (SyncSortOrderModel.get_EntityId))))), (Expression) Expression.Equal((Expression) Expression.Property((Expression) parameterExpression, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (SyncSortOrderModel.get_Type))), (Expression) Expression.Property((Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass180, typeof (SyncSortOrderDao.\u003C\u003Ec__DisplayClass18_0)), FieldInfo.GetFieldFromHandle(__fieldref (SyncSortOrderDao.\u003C\u003Ec__DisplayClass18_0.model))), (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (SyncSortOrderModel.get_Type))))), (Expression) Expression.Equal((Expression) Expression.Property((Expression) parameterExpression, (MethodInfo) MethodBase.GetMethodFromHandle(__methodref (SyncSortOrderModel.get_UserId))), (Expression) Expression.Field((Expression) Expression.Constant((object) cDisplayClass180, typeof (SyncSortOrderDao.\u003C\u003Ec__DisplayClass18_0)), FieldInfo.GetFieldFromHandle(__fieldref (SyncSortOrderDao.\u003C\u003Ec__DisplayClass18_0.userId))))), parameterExpression)).FirstOrDefaultAsync();
      if (model1 != null)
      {
        model1.SyncStatus = 1;
        model1.Deleted = 1;
        int num = await SyncSortOrderDao.UpdateAsync(model1);
      }
      SyncManager.TryDelaySync();
    }

    public static async Task DealTagRenamedTask(string originalName, string newName)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      string originalGroupId = "#" + originalName;
      string newGroupId = "#" + newName;
      List<SyncSortOrderModel> listAsync = await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (o => o.UserId == userId)).Where((Expression<Func<SyncSortOrderModel, bool>>) (o => o.GroupId == originalGroupId)).ToListAsync();
      // ISSUE: explicit non-virtual call
      if (listAsync != null && __nonvirtual (listAsync.Count) > 0)
      {
        foreach (SyncSortOrderModel local in listAsync)
        {
          local.SyncStatus = 1;
          local.Deleted = 1;
          int num1 = await App.Connection.UpdateAsync((object) local);
          SyncSortOrderModel syncSortOrderModel = new SyncSortOrderModel();
          SyncSortOrderModel.Copy(syncSortOrderModel, local);
          syncSortOrderModel.GroupId = newGroupId;
          syncSortOrderModel.Deleted = 0;
          syncSortOrderModel.SyncStatus = 1;
          int num2 = await SyncSortOrderDao.InsertAsync(syncSortOrderModel);
        }
      }
      await TaskSortOrderService.FetchAllAsync();
      SyncManager.TryDelaySync();
      newGroupId = (string) null;
    }

    public static async Task DealTagRenameProject(string originalName, string newName)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      List<SyncSortOrderModel> listAsync = await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (o => o.EntityId == originalName && o.Type == 7 && o.UserId == userId)).ToListAsync();
      // ISSUE: explicit non-virtual call
      if (listAsync != null && __nonvirtual (listAsync.Count) > 0)
      {
        foreach (SyncSortOrderModel local in listAsync)
        {
          local.SyncStatus = 1;
          local.Deleted = 1;
          int num1 = await SyncSortOrderDao.UpdateAsync(local);
          int num2 = await SyncSortOrderDao.InsertAsync(new SyncSortOrderModel()
          {
            UserId = local.UserId,
            EntityId = newName,
            SortOrderType = local.SortOrderType,
            SortOrder = local.SortOrder,
            SyncStatus = 1,
            Deleted = 0,
            GroupId = local.GroupId
          });
        }
      }
      SyncManager.TryDelaySync();
    }

    public static async Task OnTagRenamed(string originalName, string newName)
    {
      await SyncSortOrderDao.DealTagRenameProject(originalName, newName);
      await SyncSortOrderDao.DealTagRenamedTask(originalName, newName);
    }

    public static async Task DealTagDeletedTask(string tagName)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      string typeTagName = "taskByTag_" + tagName;
      List<SyncSortOrderModel> listAsync = await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (o => o.UserId == userId)).Where((Expression<Func<SyncSortOrderModel, bool>>) (o => o.GroupId == tagName || o.SortOrderType == typeTagName)).ToListAsync();
      // ISSUE: explicit non-virtual call
      if (listAsync != null && __nonvirtual (listAsync.Count) > 0)
      {
        foreach (SyncSortOrderModel model in listAsync)
        {
          model.SyncStatus = 1;
          model.Deleted = 1;
          int num = await SyncSortOrderDao.UpdateAsync(model);
        }
      }
      await TaskSortOrderService.FetchAllAsync();
      SyncManager.TryDelaySync();
    }

    public static async Task DealTagDeletedProject(string tagName)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      List<SyncSortOrderModel> listAsync = await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (o => o.EntityId == tagName && o.Type == 7 && o.UserId == userId)).ToListAsync();
      // ISSUE: explicit non-virtual call
      if (listAsync != null && __nonvirtual (listAsync.Count) > 0)
      {
        foreach (SyncSortOrderModel model in listAsync)
        {
          model.SyncStatus = 1;
          model.Deleted = 1;
          int num = await SyncSortOrderDao.UpdateAsync(model);
        }
      }
      SyncManager.TryDelaySync();
    }

    public static async Task OnTagDeleted(string tagName)
    {
      await SyncSortOrderDao.DealTagDeletedProject(tagName);
      await SyncSortOrderDao.DealTagDeletedTask(tagName);
    }

    public static async Task OnFilterDeleted(string filterId)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      List<SyncSortOrderModel> listAsync = await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (o => o.EntityId == filterId && o.Type == 8 && o.UserId == userId)).ToListAsync();
      // ISSUE: explicit non-virtual call
      if (listAsync != null && __nonvirtual (listAsync.Count) > 0)
      {
        foreach (SyncSortOrderModel model in listAsync)
        {
          model.SyncStatus = 1;
          model.Deleted = 1;
          int num = await SyncSortOrderDao.UpdateAsync(model);
        }
      }
      SyncManager.TryDelaySync();
    }

    public static async Task OnProjectDeletedOrClosed(string projectId)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      List<SyncSortOrderModel> listAsync = await App.Connection.Table<SyncSortOrderModel>().Where((Expression<Func<SyncSortOrderModel, bool>>) (o => o.EntityId == projectId && o.Type == 5 && o.UserId == userId)).ToListAsync();
      // ISSUE: explicit non-virtual call
      if (listAsync != null && __nonvirtual (listAsync.Count) > 0)
      {
        foreach (SyncSortOrderModel model in listAsync)
        {
          model.SyncStatus = 1;
          model.Deleted = 1;
          int num = await SyncSortOrderDao.UpdateAsync(model);
        }
      }
      SyncManager.TryDelaySync();
    }

    public static async Task<bool> ExistTaskListSortOrder(SortOption option, string catId)
    {
      List<int> intList = await App.Connection.QueryAsync<int>("Select _Id from SyncSortOrderModel WHERE GroupId=? AND Deleted != 1 AND UserId='" + LocalSettings.Settings.LoginUserId + "'  AND SortOrderType LIKE 'taskBy#" + option.groupBy + "|%" + option.orderBy + "' limit 1  ", (object) catId);
      // ISSUE: explicit non-virtual call
      return intList != null && __nonvirtual (intList.Count) > 0;
    }

    public static async Task<List<SyncSortOrderModel>> GetVersion2Models()
    {
      return await App.Connection.QueryAsync<SyncSortOrderModel>("Select * from SyncSortOrderModel  WHERE UserId='" + LocalSettings.Settings.LoginUserId + "' AND Deleted != 1  AND SortOrderType LIKE 'taskBy#%'");
    }

    public static async Task<List<SyncSortOrderModel>> GetNoColumnOrderInProject(string catId)
    {
      return await App.Connection.QueryAsync<SyncSortOrderModel>("Select * from SyncSortOrderModel WHERE GroupId='" + catId + "' AND Deleted != 1 AND UserId='" + LocalSettings.Settings.LoginUserId + "'  AND SortOrderType LIKE 'taskBy#sortOrder|none%'");
    }
  }
}
