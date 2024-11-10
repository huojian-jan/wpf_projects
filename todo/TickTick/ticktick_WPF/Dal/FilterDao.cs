// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.FilterDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class FilterDao : BaseDao<FilterModel>
  {
    public static async Task AddFilter(FilterModel filter)
    {
      if (await FilterDao.GetFilterById(filter.id) != null)
        return;
      filter.name = filter.name.Trim();
      if (string.IsNullOrEmpty(filter.userId))
        filter.userId = Utils.GetCurrentUserIdInt().ToString();
      int num = await App.Connection.InsertAsync((object) filter);
      CacheManager.UpdateFilter(filter);
    }

    private static async Task BatchInsertFilter(IReadOnlyCollection<FilterModel> filters)
    {
      if (filters == null || filters.Count <= 0)
        return;
      List<FilterModel> newFilters = new List<FilterModel>();
      foreach (FilterModel filter in (IEnumerable<FilterModel>) filters)
      {
        if (await FilterDao.GetFilterById(filter.id) == null)
        {
          if (string.IsNullOrEmpty(filter.userId))
            filter.userId = Utils.GetCurrentUserIdInt().ToString();
          filter.name = filter.name.Trim();
          if (!newFilters.Select<FilterModel, string>((Func<FilterModel, string>) (f => f.id)).ToList<string>().Contains(filter.id))
            newFilters.Add(filter);
        }
      }
      if (newFilters.Count > 0)
      {
        int num = await App.Connection.InsertAllAsync((IEnumerable) newFilters);
        foreach (FilterModel filter in newFilters)
          CacheManager.UpdateFilter(filter);
      }
      newFilters = (List<FilterModel>) null;
    }

    public static async Task UpdateFilter(FilterModel filter, bool needSync = true)
    {
      filter.name = filter.name.Trim();
      if (needSync)
      {
        filter.syncStatus = filter.syncStatus != 0 ? 1 : 0;
        filter.modifiedTime = DateTime.Now;
      }
      if (string.IsNullOrEmpty(filter.userId))
        filter.userId = Utils.GetCurrentUserIdInt().ToString();
      CacheManager.UpdateFilter(filter);
      await FilterSyncJsonDao.TrySaveFilter(filter.id);
      int num = await App.Connection.UpdateAsync((object) filter);
    }

    private static async Task BatchUpdateFilter(List<FilterModel> filters)
    {
      string str = Utils.GetCurrentUserIdInt().ToString();
      foreach (FilterModel filter in filters)
      {
        filter.name = filter.name.Trim();
        if (string.IsNullOrEmpty(filter.userId))
          filter.userId = str;
      }
      int num = await App.Connection.UpdateAllAsync((IEnumerable) filters);
      foreach (FilterModel filter in filters)
        CacheManager.UpdateFilter(filter);
    }

    public static async Task<Dictionary<string, FilterModel>> GetLocalSyncedFilterMap()
    {
      List<FilterModel> filtersWithDeleted = await FilterDao.GetAllFiltersWithDeleted();
      Dictionary<string, FilterModel> localSyncedFilterMap = new Dictionary<string, FilterModel>();
      foreach (FilterModel filterModel in filtersWithDeleted)
      {
        if (!localSyncedFilterMap.ContainsKey(filterModel.id))
          localSyncedFilterMap.Add(filterModel.id, filterModel);
      }
      return localSyncedFilterMap;
    }

    private static async Task<List<FilterModel>> GetAllFiltersWithDeleted()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<FilterModel>().Where((Expression<Func<FilterModel, bool>>) (x => x.userId == userId)).OrderBy<long>((Expression<Func<FilterModel, long>>) (x => x.sortOrder)).ToListAsync();
    }

    public static async Task<List<FilterModel>> GetAllFilters()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<FilterModel>().Where((Expression<Func<FilterModel, bool>>) (x => x.deleted != 1 && x.userId == userId)).OrderBy<long>((Expression<Func<FilterModel, long>>) (x => x.sortOrder)).ToListAsync();
    }

    public static async Task SaveServerMergeData(
      List<FilterModel> added,
      List<FilterModel> updated,
      List<string> deleted)
    {
      if (added != null && added.Count > 0)
        await FilterDao.BatchInsertFilter((IReadOnlyCollection<FilterModel>) added);
      if (updated != null && updated.Count > 0)
      {
        await FilterDao.BatchUpdateFilter(updated);
        await FilterSyncJsonDao.BatchDeleteFilters(updated.Select<FilterModel, string>((Func<FilterModel, string>) (filter => filter.id)).ToList<string>());
      }
      if (deleted.Count <= 0)
        return;
      await FilterDao.BatchDeleteFilter((IEnumerable<string>) deleted);
    }

    public static async Task<long> GetNextFilterSortOrder()
    {
      List<FilterModel> listAsync = await App.Connection.Table<FilterModel>().Where((Expression<Func<FilterModel, bool>>) (f => f.userId == LocalSettings.Settings.LoginUserId)).OrderBy<long>((Expression<Func<FilterModel, long>>) (f => f.sortOrder)).ToListAsync();
      FilterModel filterModel = listAsync.FirstOrDefault<FilterModel>();
      if (filterModel == null)
        return 0;
      long nextFilterSortOrder = filterModel.sortOrder - 268435456L;
      if (nextFilterSortOrder > filterModel.sortOrder)
      {
        long num = 268435456;
        foreach (FilterModel filter in listAsync)
        {
          filter.sortOrder = num;
          num += 268435456L;
          FilterDao.UpdateFilter(filter);
        }
        nextFilterSortOrder = -268435456L;
      }
      return nextFilterSortOrder;
    }

    private static async Task BatchDeleteFilter(IEnumerable<string> deleted)
    {
      foreach (string id in deleted)
      {
        await FilterDao.DeleteFilter(id);
        CacheManager.DeleteFilter(id);
      }
    }

    public static async Task<List<FilterModel>> GetNeedPostFilter()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<FilterModel>().Where((Expression<Func<FilterModel, bool>>) (x => x.syncStatus != 2 && x.userId == userId)).OrderBy<long>((Expression<Func<FilterModel, long>>) (x => x.sortOrder)).ToListAsync();
    }

    public static async Task SaveCommitResultBackToDb(
      Dictionary<string, string> id2Etag,
      Collection<string> deleted)
    {
      FilterModel filter;
      if (id2Etag != null)
      {
        foreach (string serverId in id2Etag.Keys)
        {
          filter = await FilterDao.GetFilterById(serverId);
          if (filter != null)
          {
            filter.syncStatus = 2;
            filter.etag = id2Etag[serverId];
            int num = await App.Connection.UpdateAsync((object) filter);
            CacheManager.UpdateFilter(filter);
          }
          filter = (FilterModel) null;
        }
      }
      if (deleted == null || deleted.Count <= 0)
        return;
      foreach (string id in deleted)
      {
        filter = await FilterDao.GetFilterById(id);
        if (filter != null)
        {
          filter.deleted = 1;
          filter.syncStatus = 2;
          int num = await App.Connection.UpdateAsync((object) filter);
          CacheManager.UpdateFilter(filter);
        }
        filter = (FilterModel) null;
      }
    }

    private static async Task DeleteFilter(string id)
    {
      List<FilterModel> filtersById = await FilterDao.GetFiltersById(id);
      if (filtersById == null || filtersById.Count < 0)
        return;
      foreach (FilterModel filter in filtersById)
      {
        filter.deleted = 1;
        filter.syncStatus = 1;
        await FilterDao.UpdateFilter(filter);
      }
    }

    private static async Task<List<FilterModel>> GetFiltersById(string id)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<FilterModel>().Where((Expression<Func<FilterModel, bool>>) (v => v.id == id && v.userId == userId)).ToListAsync();
    }

    public static async Task<FilterModel> GetFilterById(string id)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<FilterModel> listAsync = await App.Connection.Table<FilterModel>().Where((Expression<Func<FilterModel, bool>>) (v => v.id == id && v.userId == userId)).ToListAsync();
      return listAsync == null || listAsync.Count <= 0 ? (FilterModel) null : listAsync[0];
    }

    public static async Task BatchUpdateAffectedFilterOnTagChanged(string original, string revised)
    {
      List<FilterModel> allFilters = await FilterDao.GetAllFilters();
      List<FilterModel> filters = new List<FilterModel>();
      foreach (FilterModel filterModel in allFilters)
      {
        if (filterModel != null)
        {
          string oldValue = "\"" + original + "\"";
          string newValue = "\"" + revised?.ToLower() + "\"";
          if (filterModel.rule != null && filterModel.rule.Contains(oldValue) && !filterModel.rule.Contains(newValue))
          {
            filterModel.rule = filterModel.rule.Replace(oldValue, newValue);
            if (filterModel.syncStatus != 0)
              filterModel.syncStatus = 1;
            filters.Add(filterModel);
          }
        }
      }
      if (filters.Count <= 0)
        return;
      await FilterDao.BatchUpdateFilter(filters);
    }

    public static async Task BatchUpdateFilterOnTagDeleted(string tag)
    {
      List<FilterModel> allFilters = await FilterDao.GetAllFilters();
      List<FilterModel> filters = new List<FilterModel>();
      foreach (FilterModel filterModel in allFilters)
      {
        FilterViewModel filterViewModel = new FilterViewModel(filterModel);
        switch (filterViewModel.Mode)
        {
          case FilterMode.Normal:
            NormalFilterViewModel normalViewModel = filterViewModel.NormalViewModel;
            if (normalViewModel.Tags.Contains(tag))
            {
              normalViewModel.Tags.Remove(tag);
              filterModel.rule = normalViewModel.ToRule(false);
              filters.Add(filterModel);
              continue;
            }
            continue;
          case FilterMode.Advanced:
            AdvancedFilterViewModel advancedViewModel = filterViewModel.AdvancedViewModel;
            if (advancedViewModel.CardList != null && advancedViewModel.CardList.Count > 0)
            {
              using (List<CardViewModel>.Enumerator enumerator = advancedViewModel.CardList.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  if (enumerator.Current is TagCardViewModel current && current.Values.Contains(tag))
                  {
                    current.Values.Remove(tag);
                    if (current.Values.Count == 0)
                      advancedViewModel.CardList.Remove((CardViewModel) current);
                    filterModel.rule = advancedViewModel.ToRule(false);
                    filters.Add(filterModel);
                    break;
                  }
                }
                continue;
              }
            }
            else
              continue;
          default:
            continue;
        }
      }
      if (filters.Count <= 0)
        return;
      await FilterDao.BatchUpdateFilter(filters);
    }

    public static bool CheckNameValid(string name, string id)
    {
      name = name.Trim();
      if (string.IsNullOrEmpty(name) || name.StartsWith("#") || !NameUtils.IsValidNameNoCheckSharp(name, false))
        return false;
      FilterModel filterModel = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.name == name));
      if (string.IsNullOrEmpty(id))
      {
        if (filterModel != null)
          return false;
      }
      else if (filterModel != null && filterModel.id != id)
        return false;
      return true;
    }

    public static async Task SaveFilterFromPreview(FilterModel filter)
    {
      if (string.IsNullOrEmpty(filter.id))
      {
        filter.id = Utils.GetGuid();
        filter.createdTime = DateTime.Now;
        filter.modifiedTime = DateTime.Now;
        filter.syncStatus = 0;
        FilterModel filterModel = filter;
        filterModel.sortOrder = await FilterDao.GetNextFilterSortOrder();
        filterModel = (FilterModel) null;
        await FilterDao.AddFilter(filter);
      }
      else
      {
        filter.modifiedTime = DateTime.Now;
        if (filter.syncStatus != 0)
          filter.syncStatus = 1;
        await FilterSyncJsonDao.TrySaveFilter(filter.id);
        await FilterDao.UpdateFilter(filter);
      }
      MainWindowManager.TrySelectFilter(filter);
    }

    internal static async Task SwitchViewModel(FilterModel filter, string vm)
    {
      filter.viewMode = vm;
      if (filter.syncStatus != 0)
        filter.syncStatus = 1;
      await FilterSyncJsonDao.TrySaveFilter(filter.id);
      await FilterDao.UpdateFilter(filter);
    }
  }
}
