// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.FilterSyncJsonDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class FilterSyncJsonDao
  {
    public static async Task TrySaveFilter(string id)
    {
      FilterModel localFilter = await FilterDao.GetFilterById(id);
      if (localFilter == null)
        localFilter = (FilterModel) null;
      else if (await FilterSyncJsonDao.GetSavedJson(id) != null)
      {
        localFilter = (FilterModel) null;
      }
      else
      {
        int num = await App.Connection.InsertAsync((object) new FilterSyncedJsonModel()
        {
          UserId = Utils.GetCurrentUserIdInt().ToString(),
          JsonString = JsonConvert.SerializeObject((object) localFilter),
          FilterId = id
        });
        localFilter = (FilterModel) null;
      }
    }

    public static async Task<FilterSyncedJsonModel> GetSavedJson(string filterId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<FilterSyncedJsonModel>().Where((Expression<Func<FilterSyncedJsonModel, bool>>) (model => model.UserId == userId && model.FilterId == filterId)).FirstOrDefaultAsync();
    }

    public static async Task BatchDeleteFilters(List<string> filterIds)
    {
      if (filterIds == null || !filterIds.Any<string>())
        return;
      foreach (string filterId in filterIds)
      {
        FilterSyncedJsonModel savedJson = await FilterSyncJsonDao.GetSavedJson(filterId);
        if (savedJson != null)
        {
          int num = await App.Connection.DeleteAsync((object) savedJson);
        }
      }
    }
  }
}
