// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.FilterCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Cache
{
  public class FilterCache : CacheBase<FilterModel>
  {
    public override async Task Load()
    {
      FilterCache filterCache = this;
      List<FilterModel> filters = await FilterDao.GetAllFilters();
      await filterCache.CheckTimelineModel(filters);
      filterCache.AssembleData((IEnumerable<FilterModel>) filters, (Func<FilterModel, string>) (filter => filter.id));
      filters = (List<FilterModel>) null;
    }

    private async Task CheckTimelineModel(List<FilterModel> filters)
    {
      List<FilterModel> filterModelList1 = filters;
      // ISSUE: explicit non-virtual call
      if ((filterModelList1 != null ? (__nonvirtual (filterModelList1.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      List<FilterModel> filterModelList2 = new List<FilterModel>();
      foreach (FilterModel filter in filters)
      {
        if (filter.SyncTimeline.SortType != filter.Timeline.SortType)
        {
          filter.SyncTimeline.SortType = filter.Timeline.SortType;
          filterModelList2.Add(filter);
        }
      }
      if (!filterModelList2.Any<FilterModel>())
        return;
      int num = await App.Connection.UpdateAllAsync((IEnumerable) filterModelList2);
    }
  }
}
