// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.SortOrder.ProjectPinSortOrderService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Service.SortOrder
{
  public class ProjectPinSortOrderService
  {
    public static async Task<List<SyncSortOrderModel>> GetAsync()
    {
      return await SyncSortOrderDao.GetAllOrderBySortOrderType("projectPinned") ?? new List<SyncSortOrderModel>();
    }

    public static async Task<bool> CheckIsProjectPinned(string entityId, int type)
    {
      return await SyncSortOrderDao.GetSortOrderByIdAndType("projectPinned", entityId, type) != null;
    }

    public static async Task<ProjectPinSortOrderModel> Insert(string entityId, int type)
    {
      List<long> list = (await ProjectPinSortOrderService.GetAsync()).Select<SyncSortOrderModel, long>((Func<SyncSortOrderModel, long>) (m => m.SortOrder)).ToList<long>();
      long num1 = list.Any<long>() ? list.Max() + 268435456L : 0L;
      ProjectPinSortOrderModel pinSortOrderModel1 = new ProjectPinSortOrderModel();
      pinSortOrderModel1.EntityId = entityId;
      pinSortOrderModel1.Type = type;
      pinSortOrderModel1.SortOrder = num1;
      pinSortOrderModel1.SyncStatus = 1;
      ProjectPinSortOrderModel pinModel = pinSortOrderModel1;
      int num2 = await SyncSortOrderDao.InsertOrUpdate((SyncSortOrderModel) pinModel);
      ProjectPinSortOrderModel pinSortOrderModel2 = pinModel;
      pinModel = (ProjectPinSortOrderModel) null;
      return pinSortOrderModel2;
    }

    public static async Task Delete(string id, int type)
    {
      ProjectPinSortOrderModel model = new ProjectPinSortOrderModel();
      model.EntityId = id;
      model.Type = type;
      await SyncSortOrderDao.DeleteAsync((SyncSortOrderModel) model);
    }

    public static async Task UpdateProjectPinModel(string entityId, int type, long sortOrder)
    {
      SyncSortOrderModel orderByIdAndType = await SyncSortOrderDao.GetSortOrderByIdAndType("projectPinned", entityId, type);
      if (orderByIdAndType == null)
        return;
      orderByIdAndType.SortOrder = sortOrder;
      orderByIdAndType.SyncStatus = 1;
      int num = await SyncSortOrderDao.InsertOrUpdate(orderByIdAndType);
    }
  }
}
