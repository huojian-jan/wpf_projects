// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.TagService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Service
{
  public static class TagService
  {
    public static async Task<TagModel> TryCreateTag(
      string tag,
      string color = "",
      string parent = "",
      bool randomColor = true)
    {
      return await TagDao.TryCreateTag(tag, color, parent, randomColor);
    }

    public static async Task SaveTag(
      string id,
      string original,
      string newLabel,
      string color,
      string parent)
    {
      LocalSettings.Settings.OnTagNameChanged(original, newLabel);
      TagModel tag = await TagDao.UpdateTag(original, newLabel, color, parent);
      if (original != newLabel.ToLower())
      {
        await TaskDao.BatchUpdateAffectedTaskOnTagChanged(original, newLabel);
        await TaskSortOrderInPriorityDao.BatchUpdateAffectedTaskOnTagChanged(original, newLabel);
        await FilterDao.BatchUpdateAffectedFilterOnTagChanged(original, newLabel);
        await MatrixManager.UpdateAffectedQuadrantsOnTagChanged(original, newLabel);
        await TaskDefaultDao.UpdateDefaultTagsOnTagChanged(original, newLabel);
      }
      if (tag == null)
      {
        tag = (TagModel) null;
      }
      else
      {
        DataChangedNotifier.NotifyTagChanged(tag, original);
        tag = (TagModel) null;
      }
    }

    public static async Task MergeTag(string original, string merged)
    {
      await TagDao.MergeParentTag(original, merged.ToLower());
      await TaskDao.BatchUpdateAffectedTaskOnTagChanged(original, merged);
      await TaskSortOrderInPriorityDao.BatchUpdateAffectedTaskOnTagChanged(original, merged);
      await TagDao.DeleteTag(original);
      await TaskDefaultDao.UpdateDefaultTagsOnTagChanged(original, (string) null);
      await Communicator.MergeTag(original, merged.ToLower());
    }

    public static async Task DeleteTag(string tag)
    {
      await TagDao.DeleteTag(tag);
      await TaskDefaultDao.UpdateDefaultTagsOnTagChanged(tag, (string) null);
      await TaskDao.BatchUpdateTaskOnTagDeleted(tag);
      await FilterDao.BatchUpdateFilterOnTagDeleted(tag);
      DataChangedNotifier.NotifyTagDeleted();
    }

    public static async Task CheckTaskTags(List<TaskModel> tasks, bool isShare = false)
    {
      List<string> source = new List<string>();
      if (tasks == null || tasks.Count <= 0)
        return;
      foreach (TaskModel task in tasks)
      {
        if (task.tags != null && task.tags.Length != 0)
          source.AddRange((IEnumerable<string>) ((IEnumerable<string>) task.tags).ToList<string>());
      }
      if (source.Count <= 0)
        return;
      await TagService.CheckTagsExist(source.Distinct<string>().OrderBy<string, string>((Func<string, string>) (tag => tag)).ToList<string>(), isShare);
    }

    public static async Task CheckTagsExist(List<string> tags, bool isShare = false)
    {
      if (tags == null || tags.Count == 0)
        return;
      HashSet<string> localTags = new HashSet<string>((IEnumerable<string>) CacheManager.GetTags().Select<TagModel, string>((Func<TagModel, string>) (t => t.name)).ToList<string>());
      List<string> list = tags.Where<string>((Func<string, bool>) (t => !localTags.Contains(t.ToLower()))).ToList<string>();
      if (list.Count <= 0)
        return;
      foreach (string tag1 in list)
      {
        TagModel tag2 = await TagDao.CreateTag(tag1);
        if (tag2 != null)
        {
          tag2.type = isShare ? 2 : 1;
          await TagDao.SaveNewTag(tag2);
        }
        await Task.Delay(10);
      }
    }

    public static bool ExistShareTag()
    {
      return CacheManager.GetTags().Any<TagModel>((Func<TagModel, bool>) (t => t.type == 2));
    }

    public static async void SwitchTagType(string tagName)
    {
      TagModel tag = CacheManager.GetTagByName(tagName);
      if (tag == null)
        ;
      else
      {
        int type = tag.type == 2 ? 1 : 2;
        if (!string.IsNullOrEmpty(tag.parent))
        {
          TagModel tagByName = CacheManager.GetTagByName(tag.parent);
          if (tagByName != null)
            tag = tagByName;
        }
        tag.type = type;
        tag.SetUpdateStatus();
        await TagDao.UpdateTag(tag, notify: false);
        foreach (TagModel tag1 in CacheManager.GetTags().Where<TagModel>((Func<TagModel, bool>) (t => t.parent == tag.name)).ToList<TagModel>())
        {
          tag1.type = type;
          tag1.SetUpdateStatus();
          await TagDao.UpdateTag(tag1, notify: false);
        }
        DataChangedNotifier.NotifyTagTypeChanged();
        SyncManager.Sync();
      }
    }
  }
}
