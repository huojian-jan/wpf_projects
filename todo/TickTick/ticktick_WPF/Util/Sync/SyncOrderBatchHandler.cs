// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.SyncOrderBatchHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Sync.Model;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class SyncOrderBatchHandler : BatchHandler
  {
    public SyncOrderBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
    }

    public async Task MergeWithServer(SyncOrderBean syncOrderBean)
    {
      SyncOrderBatchHandler orderBatchHandler = this;
      if (syncOrderBean == null)
        return;
      orderBatchHandler.syncResult.TaskSortChanged = true;
      await orderBatchHandler.MergeSyncOrder(syncOrderBean.OrderByType);
    }

    private async Task MergeSyncOrder(
      Dictionary<string, Dictionary<string, SortOrderData>> syncOrderBean)
    {
      SyncOrderBatchHandler orderBatchHandler = this;
      SyncDataBean<SyncSortOrderModel> pullDataBean;
      if (syncOrderBean == null)
        pullDataBean = (SyncDataBean<SyncSortOrderModel>) null;
      else if (!syncOrderBean.Any<KeyValuePair<string, Dictionary<string, SortOrderData>>>())
      {
        pullDataBean = (SyncDataBean<SyncSortOrderModel>) null;
      }
      else
      {
        pullDataBean = new SyncDataBean<SyncSortOrderModel>();
        List<SyncSortOrderModel> localSortOrders = await SyncSortOrderDao.GetLocalSortOrders();
        foreach (string key1 in syncOrderBean.Keys)
        {
          string key = key1;
          foreach (KeyValuePair<string, SortOrderData> keyValuePair in syncOrderBean[key])
          {
            KeyValuePair<string, SortOrderData> group = keyValuePair;
            SortOrderData sortOrderData = group.Value;
            if (sortOrderData.Changed != null)
            {
              foreach (SyncSortOrder syncSortOrder in sortOrderData.Changed)
              {
                SyncSortOrder order = syncSortOrder;
                SyncSortOrderModel syncSortOrderModel1 = localSortOrders.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.SortOrderType == key && o.GroupId == group.Key && o.EntityId == order.Id && o.Type == order.Type));
                if (syncSortOrderModel1 == null || syncSortOrderModel1.SyncStatus == 2 || syncSortOrderModel1.SyncStatus == 3)
                {
                  SyncSortOrderModel syncSortOrderModel2 = new SyncSortOrderModel()
                  {
                    UserId = orderBatchHandler.userId,
                    SortOrderType = key,
                    GroupId = group.Key,
                    EntityId = order.Id,
                    SortOrder = order.Order.GetValueOrDefault(),
                    SyncStatus = 2,
                    Type = order.Type
                  };
                  if (syncSortOrderModel1 == null)
                    pullDataBean.Addeds.Add(syncSortOrderModel2);
                  else if (syncSortOrderModel2.SortOrder != syncSortOrderModel1.SortOrder)
                  {
                    syncSortOrderModel1.SortOrder = syncSortOrderModel2.SortOrder;
                    syncSortOrderModel1.SyncStatus = 2;
                    pullDataBean.Updateds.Add(syncSortOrderModel1);
                  }
                }
              }
            }
            if (sortOrderData.Deleted != null)
            {
              foreach (SyncSortOrder syncSortOrder in sortOrderData.Deleted)
              {
                SyncSortOrder order = syncSortOrder;
                SyncSortOrderModel syncSortOrderModel = localSortOrders.FirstOrDefault<SyncSortOrderModel>((Func<SyncSortOrderModel, bool>) (o => o.SortOrderType == key && o.GroupId == group.Key && o.EntityId == order.Id && o.Type == order.Type));
                if (syncSortOrderModel != null && (syncSortOrderModel.SyncStatus == 2 || syncSortOrderModel.SyncStatus == 3))
                  pullDataBean.Deleteds.Add(syncSortOrderModel);
              }
            }
          }
        }
        await SyncSortOrderDao.SaveRemoteChangesToDb(pullDataBean);
        pullDataBean = (SyncDataBean<SyncSortOrderModel>) null;
      }
    }

    public static async Task<SyncOrderBean> CreateCommitBean()
    {
      SyncOrderBean syncOrderBean1 = new SyncOrderBean();
      SyncOrderBean syncOrderBean2 = syncOrderBean1;
      syncOrderBean2.OrderByType = await SyncOrderBatchHandler.CreateCommitOrderMap();
      SyncOrderBean commitBean = syncOrderBean1;
      syncOrderBean2 = (SyncOrderBean) null;
      syncOrderBean1 = (SyncOrderBean) null;
      return commitBean;
    }

    public static async Task HandleCommitResult(BatchSyncOrderUpdateResult result)
    {
      await SyncOrderBatchHandler.HandleCommitOrderResult(result.orderByType);
    }

    private static async Task HandleCommitOrderResult(BatchUpdateResult result)
    {
      SyncDataBean<SyncSortOrderModel> dateBean = new SyncDataBean<SyncSortOrderModel>();
      List<SyncSortOrderModel> needPostSortOrders = await SyncSortOrderDao.GetNeedPostSortOrders();
      if (needPostSortOrders == null)
        dateBean = (SyncDataBean<SyncSortOrderModel>) null;
      else if (needPostSortOrders.Count <= 0)
      {
        dateBean = (SyncDataBean<SyncSortOrderModel>) null;
      }
      else
      {
        foreach (SyncSortOrderModel syncSortOrderModel in needPostSortOrders)
        {
          if (result.Id2etag.Keys.Contains<string>(syncSortOrderModel.SortOrderType))
          {
            if (syncSortOrderModel.SyncStatus != 2)
              dateBean.Updateds.Add(syncSortOrderModel);
            else
              dateBean.Deleteds.Add(syncSortOrderModel);
          }
        }
        await SyncSortOrderDao.SavePostResult(dateBean);
        dateBean = (SyncDataBean<SyncSortOrderModel>) null;
      }
    }

    private static async Task<Dictionary<string, Dictionary<string, SortOrderData>>> CreateCommitOrderMap()
    {
      Dictionary<string, Dictionary<string, SortOrderData>> commitBeanMap = new Dictionary<string, Dictionary<string, SortOrderData>>();
      Dictionary<string, Dictionary<string, List<SyncSortOrderModel>>> needPostMap = await SyncSortOrderDao.GetNeedPostMap();
      if (needPostMap == null || needPostMap.Count == 0)
        return commitBeanMap;
      foreach (string key1 in needPostMap.Keys)
      {
        Dictionary<string, List<SyncSortOrderModel>> dictionary1 = needPostMap[key1];
        Dictionary<string, SortOrderData> dictionary2 = new Dictionary<string, SortOrderData>();
        foreach (string key2 in dictionary1.Keys)
        {
          List<SyncSortOrder> syncSortOrderList1 = new List<SyncSortOrder>();
          List<SyncSortOrder> syncSortOrderList2 = new List<SyncSortOrder>();
          foreach (SyncSortOrderModel syncSortOrderModel in dictionary1[key2])
          {
            if (syncSortOrderModel.Deleted == 1)
            {
              if (syncSortOrderModel.SyncStatus == 1 || syncSortOrderModel.SyncStatus == 3)
                syncSortOrderList2.Add(new SyncSortOrder()
                {
                  Id = syncSortOrderModel.EntityId,
                  Order = new long?(syncSortOrderModel.SortOrder),
                  Type = syncSortOrderModel.Type
                });
            }
            else if (syncSortOrderModel.SyncStatus == 1 || syncSortOrderModel.SyncStatus == 3)
              syncSortOrderList1.Add(new SyncSortOrder()
              {
                Id = syncSortOrderModel.EntityId,
                Order = new long?(syncSortOrderModel.SortOrder),
                Type = syncSortOrderModel.Type
              });
          }
          if (syncSortOrderList1.Count > 0 || syncSortOrderList2.Count > 0)
          {
            SortOrderData sortOrderData = new SortOrderData()
            {
              Changed = syncSortOrderList1,
              Deleted = syncSortOrderList2
            };
            dictionary2.Add(key2, sortOrderData);
          }
        }
        if (dictionary2.Count > 0)
          commitBeanMap.Add(key1, dictionary2);
      }
      return commitBeanMap;
    }
  }
}
