// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagDataHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public static class TagDataHelper
  {
    public static List<TagModel> GetTags()
    {
      return CacheManager.GetTags().OrderBy<TagModel, int>((Func<TagModel, int>) (t => t.TypeOrder)).ThenBy<TagModel, long>((Func<TagModel, long>) (tag => tag.sortOrder)).ToList<TagModel>();
    }

    public static Dictionary<string, long> GetTagSortDict()
    {
      List<TagModel> tags = TagDataHelper.GetTags();
      Dictionary<string, long> tagSortDict = new Dictionary<string, long>();
      foreach (TagModel tagModel in tags)
      {
        if (!tagSortDict.ContainsKey(tagModel.name))
          tagSortDict.Add(tagModel.name, tagModel.sortOrder);
      }
      return tagSortDict;
    }

    public static string GetPrimaryTag(
      Dictionary<string, long> tagSortDict,
      IList<string> tags,
      ICollection<string> limits)
    {
      if (tags == null || tags.Count <= 0)
        return string.Empty;
      if (tags.Count == 1)
        return tags[0];
      tagSortDict = tagSortDict ?? TagDataHelper.GetTagSortDict();
      List<string> sortedTags = TagDataHelper.GetSortedTags(tagSortDict, (ICollection<string>) tags);
      if (sortedTags == null || sortedTags.Count <= 0)
        return tags[0];
      return limits == null ? sortedTags[0] : sortedTags.FirstOrDefault<string>(new Func<string, bool>(limits.Contains)) ?? sortedTags[0];
    }

    public static List<string> GetSortedTags(
      Dictionary<string, long> tagSortDict,
      ICollection<string> tags)
    {
      return tagSortDict.Where<KeyValuePair<string, long>>((Func<KeyValuePair<string, long>, bool>) (kv => tags.Contains(kv.Key))).OrderBy<KeyValuePair<string, long>, long>((Func<KeyValuePair<string, long>, long>) (kv => kv.Value)).Select<KeyValuePair<string, long>, string>((Func<KeyValuePair<string, long>, string>) (kv => kv.Key)).ToList<string>();
    }

    public static bool CheckIfTagExisted(string id, string tagName)
    {
      List<TagModel> tags = TagDataHelper.GetTags();
      return string.IsNullOrEmpty(id) ? tags.Exists((Predicate<TagModel>) (tag => string.Equals(tag.name, tagName, StringComparison.CurrentCultureIgnoreCase))) : tags.Exists((Predicate<TagModel>) (tag => tag.id != id && string.Equals(tag.name, tagName, StringComparison.CurrentCultureIgnoreCase)));
    }

    public static TagSelectData GetSelectTagData(List<List<string>> data)
    {
      if (data == null || data.Count <= 0)
        return new TagSelectData();
      List<string> list1 = data[0];
      List<string> first = new List<string>();
      foreach (List<string> second in data)
      {
        list1 = list1.Intersect<string>((IEnumerable<string>) second).ToList<string>();
        first = first.Union<string>((IEnumerable<string>) second).ToList<string>();
      }
      List<string> list2 = first.Except<string>((IEnumerable<string>) list1).ToList<string>();
      return new TagSelectData()
      {
        OmniSelectTags = list1,
        PartSelectTags = list2
      };
    }

    public static bool ShowTagCategoryInAuto()
    {
      List<TagModel> tags = TagDataHelper.GetTags();
      if (tags.Count == 0)
        return true;
      ConcurrentDictionary<string, int> countData = TaskCountCache.CountData;
      bool flag = false;
      foreach (TagModel tagModel in tags)
      {
        if (countData.ContainsKey(tagModel.name))
        {
          flag = true;
          if (countData[tagModel.name] > 0)
            return true;
        }
      }
      return !flag;
    }

    public static string GetFirstValidTag()
    {
      List<TagModel> tags = TagDataHelper.GetTags();
      if (tags.Any<TagModel>())
      {
        if (LocalSettings.Settings.SmartListTag != 2)
          return tags[0].name;
        ConcurrentDictionary<string, int> countData = TaskCountCache.CountData;
        foreach (TagModel tagModel in tags)
        {
          if (countData.ContainsKey(tagModel.name) && countData[tagModel.name] > 0)
            return tagModel.name;
        }
      }
      return string.Empty;
    }

    public static string GetTagDisplayName(string tag)
    {
      TagModel tagModel;
      if (!string.IsNullOrEmpty(tag))
      {
        List<TagModel> tags = CacheManager.GetTags();
        tagModel = tags != null ? tags.FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == tag.ToLower())) : (TagModel) null;
      }
      else
        tagModel = (TagModel) null;
      return tagModel?.GetDisplayName() ?? tag;
    }
  }
}
