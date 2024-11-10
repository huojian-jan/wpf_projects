// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TagDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Drag;
using ticktick_WPF.Views.Tag;
using TickTickDao;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class TagDao : BaseDao<TagModel>
  {
    public static async Task<TagModel> TryCreateTag(
      string tag,
      string color = "",
      string parent = null,
      bool randomColor = true)
    {
      TagModel model = await TagDao.CreateTag(tag, color, parent, randomColor);
      if (model != null)
        await TagDao.SaveNewTag(model);
      TagModel tag1 = model;
      model = (TagModel) null;
      return tag1;
    }

    public static async Task SaveNewTag(TagModel tag)
    {
      int num = await App.Connection.InsertAsync((object) tag);
      CacheManager.UpdateTag(tag);
      DataChangedNotifier.NotifyTagChanged(tag);
      SyncManager.TryDelaySync(1000);
    }

    public static async Task<TagModel> CreateTag(
      string tag,
      string color = "",
      string parent = "",
      bool randomColor = true)
    {
      string tagName = tag.ToLower().Trim();
      List<TagModel> allTags = await TagDao.GetAllTags();
      if (allTags.FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == tagName)) != null)
        return (TagModel) null;
      long num = string.IsNullOrEmpty(parent) ? TagDao.GetNextTagSortOrder((IReadOnlyCollection<TagModel>) allTags) : ProjectDragHelper.CalculateInsertTagSortOrder(true, parent);
      if (string.IsNullOrEmpty(color) & randomColor)
        color = ThemeUtil.GetRandomColor();
      Pinyin pinyin = PinyinUtils.ToPinyin(tagName);
      return new TagModel()
      {
        id = Utils.GetGuid(),
        name = tagName,
        label = tag,
        sortOrder = num,
        sortType = Constants.SortType.project.ToString(),
        SortOption = SortOptionUtils.GetSortOptionBySortType(Constants.SortType.project, false),
        userId = Utils.GetCurrentUserIdInt().ToString(),
        status = 0,
        pinyin = pinyin.Text,
        inits = pinyin.Inits,
        deleted = 0,
        color = color,
        parent = parent
      };
    }

    private static long GetNextTagSortOrder(IReadOnlyCollection<TagModel> tags)
    {
      return tags != null && tags.Count > 0 ? tags.OrderBy<TagModel, long>((Func<TagModel, long>) (tag => tag.sortOrder)).First<TagModel>().sortOrder - 268435456L : 0L;
    }

    public static async Task DeleteTag(string tagName)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      foreach (TagModel tag in CacheManager.GetTags().Where<TagModel>((Func<TagModel, bool>) (tag => (tag.name == tagName || tag.parent == tagName) && tag.userId == userId)).ToList<TagModel>())
      {
        if (tag.name == tagName)
        {
          int num = await App.Connection.DeleteAsync((object) tag);
          CacheManager.DeleteTag(tagName);
          if (tag.IsChild())
            await TagDao.CheckParent(tag.parent);
        }
        if (tag.name != tagName && tag.parent == tagName)
        {
          tag.parent = "";
          await TagDao.UpdateTag(tag);
        }
      }
    }

    public static async Task CheckParent(string name)
    {
      TagModel tag = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == name));
      if (tag == null || tag.IsParent())
        return;
      Constants.SortType sortType;
      if (!tag.collapsed)
      {
        string groupBy1 = tag.SortOption?.groupBy;
        sortType = Constants.SortType.tag;
        string str1 = sortType.ToString();
        if (!(groupBy1 == str1))
        {
          string groupBy2 = tag.Timeline?.sortOption?.groupBy;
          sortType = Constants.SortType.tag;
          string str2 = sortType.ToString();
          if (!(groupBy2 == str2))
            return;
        }
      }
      tag.collapsed = false;
      string groupBy3 = tag.SortOption?.groupBy;
      sortType = Constants.SortType.tag;
      string str3 = sortType.ToString();
      if (groupBy3 == str3)
      {
        SortOption sortOption = tag.SortOption;
        sortType = Constants.SortType.project;
        string str4 = sortType.ToString();
        sortOption.groupBy = str4;
        TagModel tagModel = tag;
        sortType = Constants.SortType.project;
        string str5 = sortType.ToString();
        tagModel.sortType = str5;
        tag.status = tag.status != 0 ? 1 : 0;
      }
      string groupBy4 = tag.Timeline?.sortOption?.groupBy;
      sortType = Constants.SortType.tag;
      string str6 = sortType.ToString();
      if (groupBy4 == str6)
      {
        SortOption sortOption = tag.Timeline.sortOption;
        sortType = Constants.SortType.project;
        string str7 = sortType.ToString();
        sortOption.groupBy = str7;
        TimelineModel timeline = tag.Timeline;
        sortType = Constants.SortType.project;
        string str8 = sortType.ToString();
        timeline.SortType = str8;
        tag.SyncTimeline.SortOption = tag.Timeline.sortOption;
        tag.SyncTimeline.SortType = tag.Timeline.SortType;
        tag.status = tag.status != 0 ? 1 : 0;
      }
      await TagDao.UpdateTag(tag);
    }

    public static async Task<Dictionary<string, TagModel>> GetLocalSyncTagDict()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<TagModel> listAsync = await App.Connection.Table<TagModel>().Where((Expression<Func<TagModel, bool>>) (tag => tag.userId == userId && tag.status != 0)).ToListAsync();
      Dictionary<string, TagModel> localSyncTagDict = new Dictionary<string, TagModel>();
      if (listAsync != null && listAsync.Count > 0)
      {
        foreach (TagModel tagModel in listAsync)
        {
          if (!localSyncTagDict.ContainsKey(tagModel.name))
            localSyncTagDict.Add(tagModel.name, tagModel);
        }
      }
      return localSyncTagDict;
    }

    public static async Task<List<TagModel>> GetAllTags()
    {
      if (App.Connection == null)
        return (List<TagModel>) null;
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TagModel>().Where((Expression<Func<TagModel, bool>>) (tag => tag.userId == userId)).OrderBy<long>((Expression<Func<TagModel, long>>) (tag => tag.sortOrder)).ToListAsync();
    }

    public static async Task UpdateTag(TagModel tag, string originName = null, bool notify = true)
    {
      CacheManager.UpdateTag(tag, !notify);
      await TagSyncedJsonDao.TrySaveTag(tag, originName);
      int num = await App.Connection.UpdateAsync((object) tag);
    }

    public static async Task<TagModel> UpdateTag(
      string originalName,
      string newLabel,
      string color,
      string parent)
    {
      string tagName = newLabel.ToLower().Trim();
      TagModel tag = (TagModel) null;
      if (!string.IsNullOrEmpty(originalName))
      {
        List<TagModel> tags = await TagDao.GetTagsByName(originalName);
        if (tags.Count >= 1)
        {
          tag = tags[0];
          for (int i = 1; i < tags.Count; ++i)
          {
            int num = await App.Connection.DeleteAsync((object) tags[i]);
          }
        }
        tags = (List<TagModel>) null;
      }
      if (tag == null)
      {
        tag = await TagDao.TryCreateTag(newLabel);
      }
      else
      {
        Pinyin pinyin = PinyinUtils.ToPinyin(tagName);
        tag.name = tagName;
        tag.label = newLabel;
        tag.pinyin = pinyin.Text;
        tag.inits = pinyin.Inits;
        tag.userId = Utils.GetCurrentUserIdInt().ToString();
        if (tag.parent != parent && (!string.IsNullOrEmpty(tag.parent) || !string.IsNullOrEmpty(parent)))
        {
          if (string.IsNullOrEmpty(parent))
          {
            TagModel tagModel = CacheManager.GetTags().LastOrDefault<TagModel>((Func<TagModel, bool>) (t => t.parent == tag.parent));
            if (tagModel != null && tagModel.id != tag.id)
              tag.sortOrder = ProjectDragHelper.CalculateInsertTagSortOrder(true, tagModel.name);
          }
          else
            tag.sortOrder = ProjectDragHelper.CalculateInsertTagSortOrder(true, parent);
          tag.parent = parent;
        }
        if (tag.color != color)
          tag.color = color;
        if (tag.status != 0)
          tag.status = 1;
        CacheManager.DeleteTag(originalName);
        await TagDao.UpdateTag(tag, originalName);
      }
      TagModel tagModel1 = tag;
      tagName = (string) null;
      return tagModel1;
    }

    public static async Task<TagModel> GetTagByName(string tagId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TagModel>().Where((Expression<Func<TagModel, bool>>) (tag => tag.name == tagId && tag.userId == userId)).FirstOrDefaultAsync();
    }

    public static async Task<List<TagModel>> GetTagsByName(string name)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TagModel>().Where((Expression<Func<TagModel, bool>>) (tag => tag.name == name && tag.userId == userId)).ToListAsync();
    }

    public static async Task<TagModel> GetTagById(string tagId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TagModel>().Where((Expression<Func<TagModel, bool>>) (tag => tag.id == tagId && tag.userId == userId)).FirstOrDefaultAsync();
    }

    public static async Task TryBatchAddTags(List<string> tags, bool isShare = false)
    {
      if (tags == null || tags.Count <= 0)
        return;
      List<TagModel> allTags = await TagDao.GetAllTags();
      List<string> list = allTags.Select<TagModel, string>((Func<TagModel, string>) (tag => tag.name)).ToList<string>();
      List<TagModel> notExistedTags = new List<TagModel>();
      string str = Utils.GetCurrentUserIdInt().ToString();
      long nextTagSortOrder = TagDao.GetNextTagSortOrder((IReadOnlyCollection<TagModel>) allTags);
      foreach (string tag in tags)
      {
        string lower = tag.Trim().Replace("#", string.Empty).Replace("＃", string.Empty).ToLower();
        if (!list.Contains(lower))
        {
          Pinyin pinyin = PinyinUtils.ToPinyin(lower);
          notExistedTags.Add(new TagModel()
          {
            id = Utils.GetGuid(),
            name = lower,
            sortType = Constants.SortType.project.ToString(),
            SortOption = SortOptionUtils.GetSortOptionBySortType(Constants.SortType.project, false),
            pinyin = pinyin.Text,
            inits = pinyin.Inits,
            status = 0,
            userId = str,
            deleted = 0,
            sortOrder = nextTagSortOrder,
            color = ThemeUtil.GetRandomColor(),
            type = isShare ? 2 : 1
          });
          nextTagSortOrder += 268435456L;
        }
      }
      if (notExistedTags.Count > 0)
      {
        int num = await App.Connection.InsertAllAsync((IEnumerable) notExistedTags);
        foreach (TagModel tag in notExistedTags)
          CacheManager.UpdateTag(tag);
      }
      notExistedTags = (List<TagModel>) null;
    }

    public static async Task SaveServerMergeData(
      List<TagModel> added,
      List<TagModel> updated,
      IEnumerable<TagModel> deleted)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<string> list = (await TagDao.GetAllTags()).Select<TagModel, string>((Func<TagModel, string>) (tag => tag.name)).ToList<string>();
      List<TagModel> addtags = new List<TagModel>();
      foreach (TagModel tagModel in added)
      {
        if (!list.Contains(tagModel.name))
        {
          if (string.IsNullOrEmpty(tagModel.id))
          {
            tagModel.id = Utils.GetGuid();
            tagModel.userId = userId;
          }
          addtags.Add(tagModel);
        }
      }
      if (added.Count > 0)
      {
        int num = await App.Connection.InsertAllAsync((IEnumerable) addtags);
        foreach (TagModel tag in addtags)
          CacheManager.UpdateTag(tag);
      }
      if (updated != null && updated.Count > 0)
      {
        int num = await App.Connection.UpdateAllAsync((IEnumerable) updated);
        await TagSyncedJsonDao.BatchDeleteTags(updated.Select<TagModel, string>((Func<TagModel, string>) (tag => tag.name)).ToList<string>());
        foreach (TagModel tagModel in updated)
        {
          TagModel tag = tagModel;
          TagModel cacheTag = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == tag.name));
          if (cacheTag != null && string.IsNullOrEmpty(cacheTag.parent) && !string.IsNullOrEmpty(tag.parent))
            CacheManager.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => t.parent == cacheTag.name)).ToList<TagModel>().ForEach((Action<TagModel>) (async t =>
            {
              t.parent = tag.parent;
              await TagDao.UpdateTag(t);
            }));
          if (cacheTag == null || !tag.IsEquals(cacheTag))
          {
            tag.id = cacheTag?.id ?? tag.id ?? Utils.GetGuid();
            CacheManager.UpdateTag(tag);
          }
        }
      }
      foreach (TagModel tag in deleted)
      {
        int num = await App.Connection.DeleteAsync((object) tag);
        CacheManager.DeleteTag(tag.name);
      }
      userId = (string) null;
      addtags = (List<TagModel>) null;
    }

    public static async Task<List<TagModel>> GetNeedPostTags()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TagModel>().Where((Expression<Func<TagModel, bool>>) (x => x.status != 2 && x.userId == userId)).ToListAsync();
    }

    public static async Task<bool> SaveCommitResultBackToDb(
      Dictionary<string, string> id2Etag,
      Dictionary<string, string> id2Error,
      LogModel logModel)
    {
      bool needRecommit = false;
      string serverId;
      if (id2Etag != null)
      {
        logModel.Log += "Id2etag : ";
        foreach (string key in id2Etag.Keys)
        {
          serverId = key;
          TagModel tag = await TagDao.GetTagByName(serverId);
          if (tag != null)
          {
            tag.status = 2;
            tag.etag = id2Etag[serverId];
            int num = await App.Connection.UpdateAsync((object) tag);
            CacheManager.UpdateTag(tag, true);
          }
          LogModel logModel1 = logModel;
          logModel1.Log = logModel1.Log + "  " + serverId + "  ";
          tag = (TagModel) null;
          serverId = (string) null;
        }
        await TagSyncedJsonDao.BatchDeleteTags(id2Etag.Keys.ToList<string>());
      }
      if (id2Error != null && id2Error.Count > 0)
      {
        serverId = string.Empty;
        foreach (string id in id2Error.Keys)
        {
          List<TagModel> tags = await TagDao.GetTagsByName(id);
          List<TagModel> tagModelList = tags;
          // ISSUE: explicit non-virtual call
          if ((tagModelList != null ? (__nonvirtual (tagModelList.Count) > 0 ? 1 : 0) : 0) != 0 && id2Error[id] == "EXISTED")
          {
            if (tags[0].status == 0)
            {
              needRecommit = true;
              tags[0].status = 1;
              int num = await App.Connection.UpdateAsync((object) tags[0]);
            }
            if (tags.Count > 1)
            {
              for (int i = 1; i < tags.Count; ++i)
              {
                int num = await App.Connection.DeleteAsync((object) tags[i]);
              }
            }
          }
          serverId = serverId + id + " : " + id2Error[id];
          tags = (List<TagModel>) null;
        }
        LogModel logModel2 = logModel;
        logModel2.Log = logModel2.Log + "  error:  " + serverId;
        serverId = (string) null;
      }
      return needRecommit;
    }

    public static async Task MergeParentTag(string original, string target)
    {
      List<TagModel> tags = TagDataHelper.GetTags();
      List<TagModel> list = tags.Where<TagModel>((Func<TagModel, bool>) (t => t.name == original || t.parent == original)).ToList<TagModel>();
      if (list.Count <= 1)
        return;
      long num1 = 1L >> 38 / list.Count;
      TagModel tagModel = tags.FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == target));
      if (tagModel == null)
        return;
      bool flag = tags.Any<TagModel>((Func<TagModel, bool>) (t => t.parent == target));
      int num2 = tags.IndexOf(tagModel);
      if (num2 >= 0 && tags.Count >= num2 + 2)
        num1 = (tags[num2 + 1].sortOrder - tagModel.sortOrder) / (long) list.Count;
      int num3 = 1;
      foreach (TagModel tag in list.Where<TagModel>((Func<TagModel, bool>) (tag => tag.name != original)))
      {
        tag.parent = flag ? tagModel.name : "";
        tag.status = tag.status != 0 ? 1 : 0;
        tag.sortOrder = tagModel.sortOrder + num1 * (long) num3++;
        TagDao.UpdateTag(tag);
      }
    }

    public static void UpdateParent(string name)
    {
      if (string.IsNullOrEmpty(name))
        return;
      TagModel tag = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t?.name == name));
      try
      {
        if (tag == null || tag.IsChild() || tag.IsParent() || !(tag.SortOption?.groupBy != Constants.SortType.tag.ToString()) && !(tag.Timeline?.sortOption?.groupBy != Constants.SortType.tag.ToString()))
          return;
        tag.sortType = Constants.SortType.tag.ToString();
        Constants.SortType sortType;
        if (tag.SortOption != null)
        {
          SortOption sortOption = tag.SortOption;
          sortType = Constants.SortType.tag;
          string str = sortType.ToString();
          sortOption.groupBy = str;
        }
        if (tag.Timeline?.sortOption != null)
        {
          SortOption sortOption = tag.Timeline.sortOption;
          sortType = Constants.SortType.tag;
          string str1 = sortType.ToString();
          sortOption.groupBy = str1;
          TimelineModel timeline = tag.Timeline;
          sortType = Constants.SortType.tag;
          string str2 = sortType.ToString();
          timeline.SortType = str2;
          if (tag.SyncTimeline != null)
          {
            tag.SyncTimeline.SortOption = tag.Timeline.sortOption;
            tag.SyncTimeline.SortType = tag.Timeline.SortType;
          }
          else
            tag.SyncTimeline = new TimelineSyncModel()
            {
              SortOption = tag.Timeline.sortOption,
              SortType = tag.Timeline.SortType
            };
        }
        tag.status = tag.status != 0 ? 1 : 0;
        TagDao.UpdateTag(tag);
      }
      catch (Exception ex)
      {
        UtilLog.Info("TagDao.UpdateParent timeline == null ? " + (tag?.Timeline == null).ToString() + ", parent.SyncTimeline == null ? " + (tag?.SyncTimeline == null).ToString());
      }
    }

    public static async Task<bool> HandleDropTaskTags(TaskModel fromTask, TaskModel dropTask)
    {
      List<string> tags = TagSerializer.ToTags(fromTask.tag);
      // ISSUE: explicit non-virtual call
      if (tags == null || __nonvirtual (tags.Count) <= 0)
        return false;
      await TaskService.SetTags(dropTask.id, (TagSerializer.ToTags(dropTask.tag) ?? new List<string>()).Union<string>((IEnumerable<string>) tags).ToList<string>());
      return true;
    }

    public static List<string> GetValidAndChildren(List<string> tagNames)
    {
      if (tagNames == null)
        return new List<string>();
      List<TagModel> tags = CacheManager.GetTags();
      ConcurrentDictionary<string, TagModel> tagDict = CacheManager.GetTagDict();
      tagNames = tagNames.Where<string>((Func<string, bool>) (t =>
      {
        if (t == "*withtags" || t == "!tag")
          return true;
        return !string.IsNullOrEmpty(t) && tagDict.ContainsKey(t);
      })).ToList<string>();
      Func<TagModel, bool> predicate = (Func<TagModel, bool>) (t => t.IsChild() && tagNames.Contains(t.parent));
      List<string> list = tags.Where<TagModel>(predicate).Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>();
      return tagNames.Union<string>((IEnumerable<string>) list).ToList<string>();
    }

    public static bool IsParentTag(string tagName)
    {
      return CacheManager.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => t.name == tagName || t.parent == tagName)).ToList<TagModel>().Count > 1;
    }

    public static async Task SwitchViewModel(TagModel tag, string viewMode)
    {
      if (tag == null)
        return;
      tag.viewMode = viewMode;
      switch (viewMode)
      {
        case "kanban":
          if (tag.SortOption?.groupBy == "none")
          {
            tag.SortOption.groupBy = "project";
            break;
          }
          break;
        case "timeline":
          TimelineModel.CheckTimelineEmpty((ITimeline) tag, Constants.SortType.project);
          break;
      }
      tag.status = tag.status != 0 ? 1 : 0;
      TagDao.UpdateTag(tag);
    }
  }
}
