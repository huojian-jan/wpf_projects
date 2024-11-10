// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.HabitSectionDao
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
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class HabitSectionDao
  {
    public static async Task<long> GetMaxSortOrder()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      HabitSectionModel habitSectionModel = await App.Connection.Table<HabitSectionModel>().Where((Expression<Func<HabitSectionModel, bool>>) (v => v.UserId == userId && v.SyncStatus != -1)).OrderByDescending<long>((Expression<Func<HabitSectionModel, long>>) (item => item.SortOrder)).FirstOrDefaultAsync();
      return habitSectionModel != null ? habitSectionModel.SortOrder : 0L;
    }

    public static async Task<List<HabitSectionModel>> GetAllHabitSections()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitSectionModel>().Where((Expression<Func<HabitSectionModel, bool>>) (v => v.UserId == userId)).ToListAsync();
    }

    public static async Task<List<HabitSectionModel>> GetNeedPostHabitSection()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitSectionModel>().Where((Expression<Func<HabitSectionModel, bool>>) (v => v.UserId == userId && (v.SyncStatus == 0 || v.SyncStatus == 1 || v.SyncStatus == -1))).ToListAsync();
    }

    public static async Task<HabitSectionModel> GetHabitSectionById(string sectionId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitSectionModel>().Where((Expression<Func<HabitSectionModel, bool>>) (v => v.UserId == userId && v.Id == sectionId && v.SyncStatus != -1)).FirstOrDefaultAsync();
    }

    public static async Task UpdateAsync(HabitSectionModel model)
    {
      int num = await App.Connection.UpdateAsync((object) model);
    }

    public static async Task InsertAsync(HabitSectionModel model)
    {
      int num = await App.Connection.InsertAsync((object) model);
    }

    public static async Task InsertAllAsync(List<HabitSectionModel> model)
    {
      int num = await App.Connection.InsertAllAsync((IEnumerable) model);
    }

    public static async Task DeleteAllAsync(List<string> modelIds)
    {
      if (modelIds == null || modelIds.Count == 0)
        return;
      int[] numArray = await Task.WhenAll<int>(modelIds.Select<string, Task<int>>((Func<string, Task<int>>) (modelId => App.Connection.ExecuteAsync("DELETE FROM HabitSectionModel WHERE Id=?", (object) modelId))));
    }
  }
}
