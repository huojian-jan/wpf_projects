// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.FilterBatchHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sync.Model;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class FilterBatchHandler : BatchHandler
  {
    public FilterBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
    }

    public async Task MergeWithServer(Collection<FilterModel> serverFilters, LogModel logModel)
    {
      FilterBatchHandler filterBatchHandler = this;
      logModel.Log += string.Format("  |  MergeFilters num :{0}", (object) serverFilters?.Count);
      if (serverFilters == null)
        return;
      List<FilterModel> added = new List<FilterModel>();
      List<FilterModel> updated = new List<FilterModel>();
      Dictionary<string, FilterModel> localFilters = await FilterDao.GetLocalSyncedFilterMap();
      foreach (FilterModel delta1 in serverFilters)
      {
        if (localFilters != null && localFilters.ContainsKey(delta1.id))
        {
          FilterModel revised = localFilters[delta1.id];
          localFilters.Remove(revised.id);
          if (!(revised.etag == delta1.etag) && revised.syncStatus != 0 && revised.deleted != 1)
          {
            bool merged = false;
            if (revised.syncStatus == 1 || revised.syncStatus == 3)
            {
              FilterSyncedJsonModel savedJson = await FilterSyncJsonDao.GetSavedJson(revised.id);
              if (savedJson != null)
              {
                merged = true;
                FilterModel original = JsonConvert.DeserializeObject<FilterModel>(savedJson.JsonString);
                revised.syncStatus = 1;
                FilterModel delta2 = delta1;
                FilterModel revised1 = revised;
                MergeUtils.Merge(original, delta2, revised1);
              }
            }
            delta1._Id = revised._Id;
            delta1.deleted = 0;
            if (!merged)
            {
              delta1.syncStatus = 2;
              delta1.Timeline = revised.Timeline ?? new TimelineModel(Constants.SortType.project.ToString());
              delta1.Timeline.SortType = delta1.SyncTimeline?.SortType ?? delta1.Timeline.SortType;
              delta1.Timeline.sortOption = delta1.SyncTimeline?.SortOption ?? delta1.Timeline.sortOption;
              updated.Add(delta1);
            }
            else
              updated.Add(revised);
            revised = (FilterModel) null;
          }
        }
        else
        {
          delta1.deleted = 0;
          delta1.syncStatus = 2;
          delta1.Timeline = delta1.SyncTimeline == null ? (TimelineModel) null : new TimelineModel(delta1.SyncTimeline.SortType, delta1.SyncTimeline.SortOption);
          added.Add(delta1);
        }
      }
      List<string> deleted = new List<string>();
      if (localFilters != null)
        deleted.AddRange(localFilters.Where<KeyValuePair<string, FilterModel>>((Func<KeyValuePair<string, FilterModel>, bool>) (item => item.Value.syncStatus != 0 && item.Value.deleted != 1)).Select<KeyValuePair<string, FilterModel>, string>((Func<KeyValuePair<string, FilterModel>, string>) (item => item.Key)));
      logModel.Log += string.Format(" a:{0} u:{1} d:{2}", (object) added.Count, (object) updated.Count, (object) deleted.Count);
      if (added.Count > 0 || updated.Count > 0 || deleted.Count > 0)
      {
        await FilterDao.SaveServerMergeData(added, updated, deleted);
        if (filterBatchHandler.syncResult == null)
          filterBatchHandler.syncResult = new SyncResult();
        filterBatchHandler.syncResult.AddedFilters = added;
        filterBatchHandler.syncResult.UpdatedFilters = updated;
        filterBatchHandler.syncResult.DeletedFilterIds = deleted;
      }
      added = (List<FilterModel>) null;
      updated = (List<FilterModel>) null;
      localFilters = (Dictionary<string, FilterModel>) null;
      deleted = (List<string>) null;
    }

    public static async Task<SyncFilterBean> CommitToServer()
    {
      List<FilterModel> needPostFilter = await FilterDao.GetNeedPostFilter();
      return needPostFilter == null ? (SyncFilterBean) null : FilterTransfer.DescribeSyncFilterBean(needPostFilter);
    }

    public static async Task HandleCommitResult(
      BatchUpdateResult result,
      Collection<string> deleted,
      LogModel logModel)
    {
      await FilterDao.SaveCommitResultBackToDb(result.Id2etag, deleted);
      if (result.Id2etag != null && result.Id2etag.Keys.Any<string>())
      {
        logModel.Log += "Id2etag : ";
        foreach (string key in result.Id2etag.Keys)
        {
          LogModel logModel1 = logModel;
          logModel1.Log = logModel1.Log + "  " + key + "  ";
        }
        await FilterSyncJsonDao.BatchDeleteFilters(result.Id2etag.Keys.ToList<string>());
      }
      if (deleted.Any<string>())
        await FilterSyncJsonDao.BatchDeleteFilters(deleted.ToList<string>());
      if (result.Id2error == null || !result.Id2error.Keys.Any<string>())
        return;
      string str = string.Empty;
      foreach (string key in result.Id2error.Keys)
        str = str + key + " : " + result.Id2error[key];
      LogModel logModel2 = logModel;
      logModel2.Log = logModel2.Log + "  error:  " + str;
    }
  }
}
