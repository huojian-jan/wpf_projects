// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.LocationDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class LocationDao : BaseDao<LocationModel>
  {
    public static async Task<LocationModel> GetLocationByTaskId(string taskId)
    {
      return await App.Connection.Table<LocationModel>().Where((Expression<Func<LocationModel, bool>>) (loc => loc.taskId == taskId)).FirstOrDefaultAsync();
    }

    private static async Task DeleteLocationByTaskId(string taskId)
    {
      List<LocationModel> listAsync = await App.Connection.Table<LocationModel>().Where((Expression<Func<LocationModel, bool>>) (loc => loc.taskId == taskId)).ToListAsync();
      if (listAsync == null || !listAsync.Any<LocationModel>())
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task InsertLocation(LocationModel model)
    {
      if (string.IsNullOrEmpty(model.taskId))
        return;
      await LocationDao.DeleteLocationByTaskId(model.taskId);
      int num = await App.Connection.InsertAsync((object) model);
    }
  }
}
