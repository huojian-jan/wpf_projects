// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.TagBatchHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.Util.Twitter;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class TagBatchHandler : BatchHandler
  {
    public TagBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
    }

    public async Task MergeWithServer(List<TagModel> serverTags, LogModel logModel)
    {
      TagBatchHandler tagBatchHandler = this;
      logModel.Log += string.Format("  |  MergeTags num :{0}", (object) serverTags?.Count);
      if (serverTags == null)
        return;
      string localUserId = Utils.GetCurrentUserIdInt().ToString();
      List<TagModel> added = new List<TagModel>();
      List<TagModel> updated = new List<TagModel>();
      Dictionary<string, TagModel> localTags = await TagDao.GetLocalSyncTagDict();
      foreach (TagModel delta in serverTags)
      {
        if (localTags.ContainsKey(delta.name))
        {
          TagModel revised = localTags[delta.name];
          localTags.Remove(revised.name);
          if (revised.etag == delta.etag && Utils.IsEqualString(revised.color, delta.color))
          {
            if (revised.type == 0 && delta.type == 2)
            {
              revised.type = delta.type;
              updated.Add(revised);
            }
          }
          else if (revised.deleted != 1)
          {
            bool merged = false;
            if (revised.status == 1)
            {
              TagSyncedJsonModel savedJson = await TagSyncedJsonDao.GetSavedJson(revised.name);
              if (savedJson != null)
              {
                merged = true;
                MergeUtils.Merge(JsonConvert.DeserializeObject<TagModel>(savedJson.JsonString), delta, revised);
              }
            }
            Pinyin pinyin = PinyinUtils.ToPinyin(delta.name);
            delta._Id = revised._Id;
            delta.id = revised.id;
            delta.userId = localUserId;
            delta.inits = pinyin.Inits;
            delta.pinyin = pinyin.Text;
            if (!merged)
            {
              delta.status = 2;
              delta.Timeline = revised.Timeline ?? new TimelineModel(Constants.SortType.project.ToString());
              delta.Timeline.SortType = delta.SyncTimeline?.SortType ?? delta.Timeline.SortType;
              delta.Timeline.sortOption = delta.SyncTimeline?.SortOption ?? delta.Timeline.sortOption;
            }
            else
            {
              delta.sortType = revised.sortType;
              delta.sortOrder = revised.sortOrder;
              delta.color = revised.color;
              delta.status = revised.status;
              delta.parent = revised.parent;
              delta.Timeline = delta.SyncTimeline == null ? (TimelineModel) null : new TimelineModel(delta.SyncTimeline.SortType, delta.SyncTimeline.SortOption);
            }
            delta.deleted = 0;
            delta.collapsed = revised.collapsed;
            updated.Add(delta);
            revised = (TagModel) null;
          }
        }
        else
        {
          Pinyin pinyin = PinyinUtils.ToPinyin(delta.name);
          delta.userId = localUserId;
          delta.inits = pinyin.Inits;
          delta.pinyin = pinyin.Text;
          delta.status = 2;
          delta.deleted = 0;
          added.Add(delta);
        }
      }
      List<TagModel> deleted = localTags.Values.ToList<TagModel>();
      LogModel logModel1 = logModel;
      logModel1.Log = logModel1.Log + "  a:" + added.Select<TagModel, string>((Func<TagModel, string>) (a => a.name)).Join<string>(",") + " u:" + updated.Select<TagModel, string>((Func<TagModel, string>) (a => a.name)).Join<string>(",") + " d:" + deleted.Select<TagModel, string>((Func<TagModel, string>) (a => a.name)).Join<string>(",");
      if (added.Count > 0 || updated.Count > 0 || deleted.Count > 0)
      {
        await TagDao.SaveServerMergeData(added, updated, (IEnumerable<TagModel>) deleted);
        if (tagBatchHandler.syncResult == null)
          tagBatchHandler.syncResult = new SyncResult();
        tagBatchHandler.syncResult.AddedTags = added;
        tagBatchHandler.syncResult.UpdatedTags = updated;
        tagBatchHandler.syncResult.DeletedTags = deleted;
      }
      localUserId = (string) null;
      added = (List<TagModel>) null;
      updated = (List<TagModel>) null;
      localTags = (Dictionary<string, TagModel>) null;
      deleted = (List<TagModel>) null;
    }

    public static async Task<SyncTagBean> CommitToServer()
    {
      List<TagModel> needPostTags = await TagDao.GetNeedPostTags();
      if (needPostTags == null || needPostTags.Count <= 0)
        return (SyncTagBean) null;
      SyncTagBean server = new SyncTagBean();
      foreach (TagModel tagModel in needPostTags)
      {
        TagModel tag = tagModel;
        if (string.IsNullOrEmpty(tag.name))
        {
          App.Connection.DeleteAsync((object) tag);
        }
        else
        {
          if (tag.parent == null)
            tag.parent = "";
          switch (tag.status)
          {
            case 0:
              if (server.Add.Any<TagModel>((Func<TagModel, bool>) (t => t.name == tag.name)))
              {
                App.Connection.DeleteAsync((object) tag);
                continue;
              }
              server.Add.Add(tag);
              continue;
            case 1:
              if (tag.deleted != 1)
                break;
              continue;
            case 3:
              if (tag.deleted == 1)
                continue;
              break;
            default:
              continue;
          }
          if (server.Update.Any<TagModel>((Func<TagModel, bool>) (t => t.name == tag.name)))
            App.Connection.DeleteAsync((object) tag);
          else
            server.Update.Add(tag);
        }
      }
      return server;
    }

    public static async Task<bool> HandleCommitResult(
      Dictionary<string, string> id2Etag,
      Dictionary<string, string> id2Error,
      LogModel logModel)
    {
      return await TagDao.SaveCommitResultBackToDb(id2Etag, id2Error, logModel);
    }
  }
}
