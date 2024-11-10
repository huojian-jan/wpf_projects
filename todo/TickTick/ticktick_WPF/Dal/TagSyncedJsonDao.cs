// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TagSyncedJsonDao
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
  public static class TagSyncedJsonDao
  {
    public static async Task TrySaveTag(TagModel tag, string originName = null)
    {
      string name;
      if (tag == null)
      {
        name = (string) null;
      }
      else
      {
        name = string.IsNullOrEmpty(originName) ? tag.name : originName;
        if (await TagSyncedJsonDao.GetSavedJson(name) != null)
        {
          name = (string) null;
        }
        else
        {
          TagModel tagByName = await TagDao.GetTagByName(name);
          if (tagByName == null)
          {
            name = (string) null;
          }
          else
          {
            if (!string.IsNullOrEmpty(originName))
            {
              tagByName.name = tag.name;
              tagByName.label = tag.label;
            }
            int num = await App.Connection.InsertAsync((object) new TagSyncedJsonModel()
            {
              TagName = tagByName.name,
              UserId = Utils.GetCurrentUserIdInt().ToString(),
              JsonString = JsonConvert.SerializeObject((object) tagByName)
            });
            name = (string) null;
          }
        }
      }
    }

    public static async Task<TagSyncedJsonModel> GetSavedJson(string tagName)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<TagSyncedJsonModel>().Where((Expression<Func<TagSyncedJsonModel, bool>>) (model => model.UserId == userId && model.TagName == tagName)).FirstOrDefaultAsync();
    }

    public static async Task BatchDeleteTags(List<string> tags)
    {
      if (tags == null || !tags.Any<string>())
        return;
      foreach (string tag in tags)
      {
        TagSyncedJsonModel savedJson = await TagSyncedJsonDao.GetSavedJson(tag);
        if (savedJson != null)
        {
          int num = await App.Connection.DeleteAsync((object) savedJson);
        }
      }
    }
  }
}
