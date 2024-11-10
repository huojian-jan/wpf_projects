// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TempDataDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class TempDataDao : BaseDao<TempDataModel>
  {
    public static async Task<TempDataModel> QueryTempDataModelListDbByTypeAndUserIdAsync(
      string type,
      string userId)
    {
      return await Task.Run<TempDataModel>((Func<Task<TempDataModel>>) (async () =>
      {
        List<TempDataModel> listAsync = await App.Connection.Table<TempDataModel>().Where((Expression<Func<TempDataModel, bool>>) (v => v.User_Id.Equals(userId))).Where((Expression<Func<TempDataModel, bool>>) (v => v.DataType.Equals(type))).ToListAsync();
        return listAsync.Count == 0 ? (TempDataModel) null : listAsync[0];
      }));
    }

    public static async Task<TempDataModel> QueryTempDataModelListDbByTypeAndEntityIdAsync(
      string type,
      string entityId)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      return await Task.Run<TempDataModel>((Func<Task<TempDataModel>>) (async () =>
      {
        List<TempDataModel> listAsync = await App.Connection.Table<TempDataModel>().Where((Expression<Func<TempDataModel, bool>>) (v => v.EntityId.Equals(entityId) && v.User_Id == userId)).Where((Expression<Func<TempDataModel, bool>>) (v => v.DataType.Equals(type))).ToListAsync();
        return listAsync.Count == 0 ? (TempDataModel) null : listAsync[0];
      }));
    }

    public static async Task<int> UpdateOrInsertTempDataModelListDbByTypeAndEntityIdAsync(
      TempDataModel tempDataModel)
    {
      return await Task.Run<int>((Func<Task<int>>) (async () =>
      {
        try
        {
          TempDataModel tempDataModel1 = await TempDataDao.QueryTempDataModelListDbByTypeAndEntityIdAsync(tempDataModel.DataType, tempDataModel.EntityId);
          if (tempDataModel1 != null)
          {
            tempDataModel._Id = tempDataModel1._Id;
            int num = await App.Connection.UpdateAsync((object) tempDataModel);
            return 0;
          }
          int num1 = await App.Connection.InsertAsync((object) tempDataModel);
          return tempDataModel._Id;
        }
        catch (Exception ex)
        {
          return -1;
        }
      }));
    }

    public static async Task<int> UpdateOrInsertTempDataModelListDbByTypeAndUserIdAsync(
      TempDataModel tempDataModel)
    {
      return await Task.Run<int>((Func<Task<int>>) (async () =>
      {
        try
        {
          TempDataModel tempDataModel1 = await TempDataDao.QueryTempDataModelListDbByTypeAndUserIdAsync(tempDataModel.DataType, tempDataModel.User_Id);
          if (tempDataModel1 != null)
          {
            tempDataModel._Id = tempDataModel1._Id;
            int num = await App.Connection.UpdateAsync((object) tempDataModel);
            return 0;
          }
          int num1 = await App.Connection.InsertAsync((object) tempDataModel);
          return tempDataModel._Id;
        }
        catch (Exception ex)
        {
          return -1;
        }
      }));
    }
  }
}
