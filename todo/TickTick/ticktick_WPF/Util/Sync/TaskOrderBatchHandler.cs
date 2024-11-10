// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.TaskOrderBatchHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sync.Model;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class TaskOrderBatchHandler : BatchHandler
  {
    private SyncResult _syncResult;

    public TaskOrderBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
      this._syncResult = syncResult;
    }

    public async Task MergeWithServer(SyncTaskOrderBean syncTaskOrderBean)
    {
      if (syncTaskOrderBean == null || syncTaskOrderBean.taskOrderByDate == null && syncTaskOrderBean.taskOrderByPriority == null && syncTaskOrderBean.taskOrderByProject == null)
        return;
      this._syncResult.TaskSortChanged = true;
      await this.MergeTaskOrderByDate(syncTaskOrderBean.taskOrderByDate);
      await this.MergeTaskOrderByPriority(syncTaskOrderBean.taskOrderByPriority);
      await this.MergeTaskOrderByProject(syncTaskOrderBean.taskOrderByProject);
    }

    private async Task MergeTaskOrderByProject(
      Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>> taskOrderByProject)
    {
      TaskOrderBatchHandler orderBatchHandler = this;
      SyncDataBean<TaskSortOrderInProjectModel> pullDataBean;
      List<string> deletedProjects;
      if (taskOrderByProject == null)
      {
        pullDataBean = (SyncDataBean<TaskSortOrderInProjectModel>) null;
        deletedProjects = (List<string>) null;
      }
      else if (!taskOrderByProject.Any<KeyValuePair<string, Dictionary<string, SyncTaskOrderByDateBean>>>())
      {
        pullDataBean = (SyncDataBean<TaskSortOrderInProjectModel>) null;
        deletedProjects = (List<string>) null;
      }
      else
      {
        pullDataBean = new SyncDataBean<TaskSortOrderInProjectModel>();
        List<TaskSortOrderInProjectModel> localSortOrders = await ProjectSortOrderDao.GetLocalSortOrders();
        deletedProjects = new List<string>();
        foreach (string key1 in taskOrderByProject.Keys)
        {
          string key = key1;
          foreach (KeyValuePair<string, SyncTaskOrderByDateBean> keyValuePair in taskOrderByProject[key])
          {
            SyncTaskOrderByDateBean taskOrderByDateBean = keyValuePair.Value;
            foreach (TaskSortOrder taskSortOrder in taskOrderByDateBean.changed)
            {
              TaskSortOrder order = taskSortOrder;
              TaskSortOrderInProjectModel orderInProjectModel1 = new TaskSortOrderInProjectModel()
              {
                UserId = orderBatchHandler.userId,
                ProjectId = key,
                EntityType = EntityType.GetEntityType(order.type),
                EntityId = order.id,
                SortOrder = order.order.GetValueOrDefault(),
                SyncStatus = 2
              };
              TaskSortOrderInProjectModel orderInProjectModel2 = localSortOrders.FirstOrDefault<TaskSortOrderInProjectModel>((Func<TaskSortOrderInProjectModel, bool>) (o => o.ProjectId == key && o.EntityId == order.id));
              if (orderInProjectModel2 == null)
                pullDataBean.Addeds.Add(orderInProjectModel1);
              else if (orderInProjectModel1.SortOrder != orderInProjectModel2.SortOrder)
              {
                orderInProjectModel2.SortOrder = orderInProjectModel1.SortOrder;
                pullDataBean.Updateds.Add(orderInProjectModel2);
              }
            }
            if (!taskOrderByDateBean.changed.Any<TaskSortOrder>() && !taskOrderByDateBean.deleted.Any<TaskSortOrder>())
              deletedProjects.Add(key);
          }
        }
        await ProjectSortOrderDao.SaveRemoteChangesToDb(pullDataBean);
        if (!deletedProjects.Any<string>())
        {
          pullDataBean = (SyncDataBean<TaskSortOrderInProjectModel>) null;
          deletedProjects = (List<string>) null;
        }
        else
        {
          await ProjectSortOrderDao.DeleteRemoteDeleted(deletedProjects);
          pullDataBean = (SyncDataBean<TaskSortOrderInProjectModel>) null;
          deletedProjects = (List<string>) null;
        }
      }
    }

    private async Task MergeTaskOrderByPriority(
      Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>> taskOrderByPriority)
    {
      TaskOrderBatchHandler orderBatchHandler = this;
      SyncDataBean<TaskSortOrderInPriorityModel> pullDataBean;
      List<string> deletedProjects;
      if (taskOrderByPriority == null)
      {
        pullDataBean = (SyncDataBean<TaskSortOrderInPriorityModel>) null;
        deletedProjects = (List<string>) null;
      }
      else if (!taskOrderByPriority.Any<KeyValuePair<string, Dictionary<string, SyncTaskOrderByDateBean>>>())
      {
        pullDataBean = (SyncDataBean<TaskSortOrderInPriorityModel>) null;
        deletedProjects = (List<string>) null;
      }
      else
      {
        pullDataBean = new SyncDataBean<TaskSortOrderInPriorityModel>();
        List<TaskSortOrderInPriorityModel> allSortOrders = await TaskSortOrderInPriorityDao.GetAllSortOrders();
        deletedProjects = new List<string>();
        foreach (string key1 in taskOrderByPriority.Keys)
        {
          string key = key1;
          foreach (KeyValuePair<string, SyncTaskOrderByDateBean> keyValuePair in taskOrderByPriority[key])
          {
            KeyValuePair<string, SyncTaskOrderByDateBean> group = keyValuePair;
            SyncTaskOrderByDateBean taskOrderByDateBean = group.Value;
            foreach (TaskSortOrder taskSortOrder in taskOrderByDateBean.changed)
            {
              TaskSortOrder order = taskSortOrder;
              TaskSortOrderInPriorityModel orderInPriorityModel1 = new TaskSortOrderInPriorityModel()
              {
                UserId = orderBatchHandler.userId,
                CatId = group.Key,
                Priority = int.Parse(key),
                EntityType = EntityType.GetEntityType(order.type),
                EntityId = order.id,
                SortOrder = order.order.GetValueOrDefault(),
                SyncStatus = 2
              };
              List<TaskSortOrderInPriorityModel> list = allSortOrders.Where<TaskSortOrderInPriorityModel>((Func<TaskSortOrderInPriorityModel, bool>) (o => o.Priority.ToString() == key && o.CatId == group.Key && o.EntityId == order.id)).ToList<TaskSortOrderInPriorityModel>();
              if (list.Count == 0)
              {
                pullDataBean.Addeds.Add(orderInPriorityModel1);
              }
              else
              {
                TaskSortOrderInPriorityModel orderInPriorityModel2 = list[0];
                if (orderInPriorityModel1.SortOrder != orderInPriorityModel2.SortOrder)
                {
                  orderInPriorityModel2.SortOrder = orderInPriorityModel1.SortOrder;
                  pullDataBean.Updateds.Add(orderInPriorityModel2);
                }
                for (int index = 1; index < list.Count; ++index)
                  pullDataBean.Deleteds.Add(list[index]);
              }
            }
            if (!taskOrderByDateBean.changed.Any<TaskSortOrder>() && !taskOrderByDateBean.deleted.Any<TaskSortOrder>())
              deletedProjects.Add(group.Key);
          }
        }
        await TaskSortOrderInPriorityDao.SaveRemoteChanges(pullDataBean);
        if (!deletedProjects.Any<string>())
        {
          pullDataBean = (SyncDataBean<TaskSortOrderInPriorityModel>) null;
          deletedProjects = (List<string>) null;
        }
        else
        {
          await TaskSortOrderInPriorityDao.DeleteRemoteDeleted(deletedProjects);
          pullDataBean = (SyncDataBean<TaskSortOrderInPriorityModel>) null;
          deletedProjects = (List<string>) null;
        }
      }
    }

    private async Task MergeTaskOrderByDate(
      Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>> taskOrderByDate)
    {
      SyncDataBean<TaskSortOrderInDateModel> pullDataBean;
      if (taskOrderByDate == null)
        pullDataBean = (SyncDataBean<TaskSortOrderInDateModel>) null;
      else if (taskOrderByDate.Count == 0)
      {
        pullDataBean = (SyncDataBean<TaskSortOrderInDateModel>) null;
      }
      else
      {
        pullDataBean = new SyncDataBean<TaskSortOrderInDateModel>();
        foreach (string dateStr in taskOrderByDate.Keys)
        {
          Dictionary<string, List<TaskSortOrderInDateModel>> dictionary1 = TaskOrderBatchHandler.AssemblyOrderDict((IEnumerable<TaskSortOrderInDateModel>) await TaskSortOrderInDateDao.GetOrderByDateWithDeleted(dateStr));
          Dictionary<string, SyncTaskOrderByDateBean> dictionary2 = taskOrderByDate[dateStr];
          foreach (string key in dictionary2.Keys)
          {
            if (dictionary1.ContainsKey(key))
            {
              List<TaskSortOrderInDateModel> orderInDateModelList = dictionary1[key];
              List<TaskSortOrder> deleted = dictionary2[key].deleted;
              // ISSUE: explicit non-virtual call
              if ((deleted != null ? (__nonvirtual (deleted.Count) < 1 ? 1 : 0) : 0) != 0)
              {
                List<TaskSortOrder> changed = dictionary2[key].changed;
                // ISSUE: explicit non-virtual call
                // ISSUE: explicit non-virtual call
                if ((changed != null ? (__nonvirtual (changed.Count) < 1 ? 1 : 0) : 0) != 0 && orderInDateModelList != null && __nonvirtual (orderInDateModelList.Count) > 0)
                  pullDataBean.Deleteds.AddRange((IEnumerable<TaskSortOrderInDateModel>) orderInDateModelList);
              }
              TaskOrderBatchHandler.MergeDeletedOrderByDate((IReadOnlyCollection<TaskSortOrder>) dictionary2[key].deleted, orderInDateModelList, pullDataBean);
              this.MergeChangedOrderByDate((IReadOnlyCollection<TaskSortOrder>) dictionary2[key].changed, orderInDateModelList, pullDataBean, dateStr, key);
            }
            else
              this.MergeChangedOrderByDate((IReadOnlyCollection<TaskSortOrder>) dictionary2[key].changed, new List<TaskSortOrderInDateModel>(), pullDataBean, dateStr, key);
          }
        }
        await TaskSortOrderInDateDao.SaveRemoteChangesToDb(pullDataBean);
        pullDataBean = (SyncDataBean<TaskSortOrderInDateModel>) null;
      }
    }

    private static Dictionary<string, List<TaskSortOrderInDateModel>> AssemblyOrderDict(
      IEnumerable<TaskSortOrderInDateModel> orders)
    {
      Dictionary<string, List<TaskSortOrderInDateModel>> dictionary = new Dictionary<string, List<TaskSortOrderInDateModel>>();
      foreach (TaskSortOrderInDateModel order in orders)
      {
        if (!dictionary.ContainsKey(order.projectid))
          dictionary.Add(order.projectid, new List<TaskSortOrderInDateModel>()
          {
            order
          });
        else
          dictionary[order.projectid].Add(order);
      }
      return dictionary;
    }

    public static async Task HandleCommitResult(
      BatchTaskOrderUpdateResult result,
      long lastPostPoint)
    {
      int num = await TaskOrderBatchHandler.HandleCommitOrderByDateResult(result.TaskOrderByDate, lastPostPoint) ? 1 : 0;
      await TaskOrderBatchHandler.HandleCommitOrderByPriorityResult();
      await TaskOrderBatchHandler.HandleCommitOrderByProjectResult();
    }

    private static async Task HandleCommitOrderByProjectResult()
    {
      await TaskOrderBatchHandler.SaveCommitOrderByProjectResult();
    }

    private static async Task SaveCommitOrderByProjectResult()
    {
      SyncDataBean<TaskSortOrderInProjectModel> projectBean = new SyncDataBean<TaskSortOrderInProjectModel>();
      List<TaskSortOrderInProjectModel> sortOrdersInProject = await ProjectSortOrderDao.GetNeedPostSortOrdersInProject();
      if (sortOrdersInProject == null)
        projectBean = (SyncDataBean<TaskSortOrderInProjectModel>) null;
      else if (sortOrdersInProject.Count <= 0)
      {
        projectBean = (SyncDataBean<TaskSortOrderInProjectModel>) null;
      }
      else
      {
        foreach (TaskSortOrderInProjectModel orderInProjectModel in sortOrdersInProject)
        {
          if (orderInProjectModel.SyncStatus != 2)
            projectBean.Updateds.Add(orderInProjectModel);
          else
            projectBean.Deleteds.Add(orderInProjectModel);
        }
        await ProjectSortOrderDao.SavePostResult(projectBean);
        projectBean = (SyncDataBean<TaskSortOrderInProjectModel>) null;
      }
    }

    private static async Task HandleCommitOrderByPriorityResult()
    {
      await TaskOrderBatchHandler.SaveCommitOrderByPriorityResult();
    }

    private static async Task SaveCommitOrderByPriorityResult()
    {
      SyncDataBean<TaskSortOrderInPriorityModel> dateBean = new SyncDataBean<TaskSortOrderInPriorityModel>();
      List<TaskSortOrderInPriorityModel> needPostSortOrders = await TaskSortOrderInPriorityDao.GetNeedPostSortOrders();
      if (needPostSortOrders == null)
        dateBean = (SyncDataBean<TaskSortOrderInPriorityModel>) null;
      else if (needPostSortOrders.Count <= 0)
      {
        dateBean = (SyncDataBean<TaskSortOrderInPriorityModel>) null;
      }
      else
      {
        foreach (TaskSortOrderInPriorityModel orderInPriorityModel in needPostSortOrders)
        {
          if (orderInPriorityModel.SyncStatus != 2)
            dateBean.Updateds.Add(orderInPriorityModel);
          else
            dateBean.Deleteds.Add(orderInPriorityModel);
        }
        await TaskSortOrderInPriorityDao.SavePostResult(dateBean);
        dateBean = (SyncDataBean<TaskSortOrderInPriorityModel>) null;
      }
    }

    private static async Task<bool> HandleCommitOrderByDateResult(
      BatchUpdateResult result,
      long lastPostPoint)
    {
      List<string> errorList = new List<string>();
      if (result.Id2error != null && result.Id2error.Count > 0)
        errorList.AddRange((IEnumerable<string>) result.Id2error.Keys);
      await TaskOrderBatchHandler.SaveCommitOrderByDateResult((ICollection<string>) errorList, lastPostPoint);
      bool flag = errorList.Count == 0;
      errorList = (List<string>) null;
      return flag;
    }

    private static async Task SaveCommitOrderByDateResult(
      ICollection<string> errorIds,
      long lastPostPoint)
    {
      SyncDataBean<TaskSortOrderInDateModel> dateBean = new SyncDataBean<TaskSortOrderInDateModel>();
      List<TaskSortOrderInDateModel> sortOrdersInDate = await TaskSortOrderInDateDao.GetNeedPostSortOrdersInDate(lastPostPoint);
      if (sortOrdersInDate == null)
        dateBean = (SyncDataBean<TaskSortOrderInDateModel>) null;
      else if (sortOrdersInDate.Count <= 0)
      {
        dateBean = (SyncDataBean<TaskSortOrderInDateModel>) null;
      }
      else
      {
        foreach (TaskSortOrderInDateModel orderInDateModel in sortOrdersInDate)
        {
          if (!errorIds.Contains(orderInDateModel.date))
          {
            if (orderInDateModel.status != 2)
              dateBean.Updateds.Add(orderInDateModel);
            else
              dateBean.Deleteds.Add(orderInDateModel);
          }
        }
        await TaskSortOrderInDateDao.SavePostResult(dateBean);
        dateBean = (SyncDataBean<TaskSortOrderInDateModel>) null;
      }
    }

    public static async Task<SyncTaskOrderBean> CreateCommitBean()
    {
      SyncTaskOrderBean syncTaskOrderBean1 = new SyncTaskOrderBean();
      SyncTaskOrderBean syncTaskOrderBean2 = syncTaskOrderBean1;
      syncTaskOrderBean2.taskOrderByDate = await TaskOrderBatchHandler.CreateCommitOrderByDateMap();
      SyncTaskOrderBean syncTaskOrderBean3 = syncTaskOrderBean1;
      syncTaskOrderBean3.taskOrderByPriority = await TaskOrderBatchHandler.CreateCommitOrderByPriorityMap();
      SyncTaskOrderBean syncTaskOrderBean4 = syncTaskOrderBean1;
      syncTaskOrderBean4.taskOrderByProject = await TaskOrderBatchHandler.CreateCommitOrderByProjectMap();
      SyncTaskOrderBean commitBean = syncTaskOrderBean1;
      syncTaskOrderBean2 = (SyncTaskOrderBean) null;
      syncTaskOrderBean3 = (SyncTaskOrderBean) null;
      syncTaskOrderBean4 = (SyncTaskOrderBean) null;
      syncTaskOrderBean1 = (SyncTaskOrderBean) null;
      return commitBean;
    }

    private static async Task<Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>>> CreateCommitOrderByProjectMap()
    {
      Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>> commitBeanMap = new Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>>();
      Dictionary<string, Dictionary<string, List<TaskSortOrderInProjectModel>>> orderInProjectMap = await ProjectSortOrderDao.GetNeedPostSortOrderInProjectMap();
      if (orderInProjectMap == null || orderInProjectMap.Count == 0)
        return commitBeanMap;
      foreach (string key1 in orderInProjectMap.Keys)
      {
        Dictionary<string, List<TaskSortOrderInProjectModel>> dictionary1 = orderInProjectMap[key1];
        Dictionary<string, SyncTaskOrderByDateBean> dictionary2 = new Dictionary<string, SyncTaskOrderByDateBean>();
        foreach (string key2 in dictionary1.Keys)
        {
          List<TaskSortOrder> taskSortOrderList1 = new List<TaskSortOrder>();
          List<TaskSortOrder> taskSortOrderList2 = new List<TaskSortOrder>();
          foreach (TaskSortOrderInProjectModel localOrder in dictionary1[key2])
          {
            if (localOrder.Deleted == 1)
            {
              if (localOrder.SyncStatus == 1)
                taskSortOrderList2.Add(TaskSortOrderTransfer.ConvertLocalToRemote(localOrder));
            }
            else
              taskSortOrderList1.Add(TaskSortOrderTransfer.ConvertLocalToRemote(localOrder));
          }
          if (taskSortOrderList1.Count > 0 || taskSortOrderList2.Count > 0)
          {
            SyncTaskOrderByDateBean taskOrderByDateBean = new SyncTaskOrderByDateBean()
            {
              changed = taskSortOrderList1,
              deleted = taskSortOrderList2
            };
            dictionary2.Add(key2, taskOrderByDateBean);
          }
        }
        if (dictionary2.Count > 0)
          commitBeanMap.Add(key1, dictionary2);
      }
      return commitBeanMap;
    }

    private static async Task<Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>>> CreateCommitOrderByPriorityMap()
    {
      Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>> commitBeanMap = new Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>>();
      Dictionary<string, Dictionary<string, List<TaskSortOrderInPriorityModel>>> needPostOrdersMap = await TaskSortOrderInPriorityDao.GetNeedPostOrdersMap();
      if (needPostOrdersMap == null || needPostOrdersMap.Count == 0)
        return commitBeanMap;
      foreach (string key1 in needPostOrdersMap.Keys)
      {
        Dictionary<string, List<TaskSortOrderInPriorityModel>> dictionary1 = needPostOrdersMap[key1];
        Dictionary<string, SyncTaskOrderByDateBean> dictionary2 = new Dictionary<string, SyncTaskOrderByDateBean>();
        foreach (string key2 in dictionary1.Keys)
        {
          List<TaskSortOrder> taskSortOrderList1 = new List<TaskSortOrder>();
          List<TaskSortOrder> taskSortOrderList2 = new List<TaskSortOrder>();
          foreach (TaskSortOrderInPriorityModel localOrder in dictionary1[key2])
          {
            if (localOrder.Deleted == 1)
            {
              if (localOrder.SyncStatus == 1)
                taskSortOrderList2.Add(TaskSortOrderTransfer.ConvertLocalToRemote(localOrder));
            }
            else
              taskSortOrderList1.Add(TaskSortOrderTransfer.ConvertLocalToRemote(localOrder));
          }
          if (taskSortOrderList1.Count > 0 || taskSortOrderList2.Count > 0)
          {
            SyncTaskOrderByDateBean taskOrderByDateBean = new SyncTaskOrderByDateBean()
            {
              changed = taskSortOrderList1,
              deleted = taskSortOrderList2
            };
            dictionary2.Add(key2, taskOrderByDateBean);
          }
        }
        if (dictionary2.Count > 0)
          commitBeanMap.Add(key1, dictionary2);
      }
      return commitBeanMap;
    }

    private static async Task<Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>>> CreateCommitOrderByDateMap()
    {
      Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>> commitBeanMap = new Dictionary<string, Dictionary<string, SyncTaskOrderByDateBean>>();
      Dictionary<string, Dictionary<string, List<TaskSortOrderInDateModel>>> sortOrdersInDateMap = await TaskSortOrderInDateDao.GetNeedPostSortOrdersInDateMap();
      if (sortOrdersInDateMap == null || sortOrdersInDateMap.Count == 0)
        return commitBeanMap;
      foreach (string key1 in sortOrdersInDateMap.Keys)
      {
        Dictionary<string, List<TaskSortOrderInDateModel>> dictionary1 = sortOrdersInDateMap[key1];
        Dictionary<string, SyncTaskOrderByDateBean> dictionary2 = new Dictionary<string, SyncTaskOrderByDateBean>();
        foreach (string key2 in dictionary1.Keys)
        {
          List<TaskSortOrder> taskSortOrderList1 = new List<TaskSortOrder>();
          List<TaskSortOrder> taskSortOrderList2 = new List<TaskSortOrder>();
          foreach (TaskSortOrderInDateModel localOrder in dictionary1[key2])
          {
            if (localOrder.deleted == 1)
            {
              if (localOrder.status == 1)
                taskSortOrderList2.Add(TaskSortOrderTransfer.ConvertLocalToRemote(localOrder));
            }
            else
              taskSortOrderList1.Add(TaskSortOrderTransfer.ConvertLocalToRemote(localOrder));
          }
          if (taskSortOrderList1.Count > 0 || taskSortOrderList2.Count > 0)
          {
            SyncTaskOrderByDateBean taskOrderByDateBean = new SyncTaskOrderByDateBean()
            {
              changed = taskSortOrderList1,
              deleted = taskSortOrderList2
            };
            dictionary2.Add(key2, taskOrderByDateBean);
          }
        }
        if (dictionary2.Count > 0)
          commitBeanMap.Add(key1, dictionary2);
      }
      return commitBeanMap;
    }

    private void MergeChangedOrderByDate(
      IReadOnlyCollection<TaskSortOrder> changes,
      List<TaskSortOrderInDateModel> localOrders,
      SyncDataBean<TaskSortOrderInDateModel> pullDataBean,
      string dateStr,
      string projectId)
    {
      if (changes == null || changes.Count == 0)
        return;
      foreach (TaskSortOrder change1 in (IEnumerable<TaskSortOrder>) changes)
      {
        TaskSortOrder change = change1;
        TaskSortOrderInDateModel orderInDateModel1 = localOrders.FirstOrDefault<TaskSortOrderInDateModel>((Func<TaskSortOrderInDateModel, bool>) (order => order.taskid == change.id));
        long? order1;
        if (orderInDateModel1 == null)
        {
          TaskSortOrderInDateModel orderInDateModel2 = new TaskSortOrderInDateModel();
          orderInDateModel2.userid = this.userId;
          orderInDateModel2.date = dateStr;
          orderInDateModel2.projectid = projectId;
          orderInDateModel2.taskid = change.id;
          order1 = change.order;
          orderInDateModel2.sortOrder = order1.GetValueOrDefault();
          orderInDateModel2.status = 2;
          TaskSortOrderInDateModel orderInDateModel3 = orderInDateModel2;
          pullDataBean.Addeds.Add(orderInDateModel3);
        }
        else if (orderInDateModel1.status == 2)
        {
          TaskSortOrderInDateModel orderInDateModel4 = orderInDateModel1;
          order1 = change.order;
          long valueOrDefault = order1.GetValueOrDefault();
          orderInDateModel4.sortOrder = valueOrDefault;
          orderInDateModel1.status = 2;
          pullDataBean.Updateds.Add(orderInDateModel1);
        }
      }
    }

    private static void MergeDeletedOrderByDate(
      IReadOnlyCollection<TaskSortOrder> deleteds,
      List<TaskSortOrderInDateModel> localOrders,
      SyncDataBean<TaskSortOrderInDateModel> pullDataBean)
    {
      if (deleteds == null || deleteds.Count == 0 || localOrders == null)
        return;
      foreach (TaskSortOrder deleted1 in (IEnumerable<TaskSortOrder>) deleteds)
      {
        TaskSortOrder deleted = deleted1;
        TaskSortOrderInDateModel orderInDateModel = localOrders.FirstOrDefault<TaskSortOrderInDateModel>((Func<TaskSortOrderInDateModel, bool>) (order => order.taskid == deleted.id));
        if (orderInDateModel != null)
          pullDataBean.Deleteds.Add(orderInDateModel);
      }
    }
  }
}
