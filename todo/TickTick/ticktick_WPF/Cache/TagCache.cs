// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.TagCache
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
  public class TagCache : CacheBase<TagModel>
  {
    public override async Task Load()
    {
      TagCache tagCache = this;
      List<TagModel> allTags = await TagDao.GetAllTags();
      tagCache.CheckTagSortOrders(allTags);
      tagCache.AssembleData((IEnumerable<TagModel>) allTags, (Func<TagModel, string>) (tag => tag.name));
    }

    private async Task CheckTagSortOrders(List<TagModel> tags)
    {
      List<TagModel> tagModelList = tags;
      // ISSUE: explicit non-virtual call
      if ((tagModelList != null ? (__nonvirtual (tagModelList.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      tags = tags.OrderBy<TagModel, long>((Func<TagModel, long>) (p => p.sortOrder)).ToList<TagModel>();
      long? nullable1 = new long?();
      long? nullable2 = new long?();
      List<TagModel> needResort = new List<TagModel>();
      foreach (TagModel tag in tags)
      {
        TagModel t = needResort.LastOrDefault<TagModel>();
        if (t != null)
        {
          if (Math.Abs(tag.sortOrder - t.sortOrder) < 4L)
          {
            needResort.Add(tag);
          }
          else
          {
            if (needResort.Count > 1)
            {
              if (!nullable1.HasValue || tag.sortOrder > nullable1.Value)
                nullable2 = new long?(tag.sortOrder);
              long? nullable3 = nullable1;
              nullable1 = new long?(nullable3 ?? nullable2.Value - 268435456L);
              nullable3 = nullable2;
              nullable2 = new long?(nullable3 ?? nullable1.Value + 268435456L);
              long num1 = (nullable2.Value - nullable1.Value) / (long) (needResort.Count + 1);
              for (int index = 0; index < needResort.Count; ++index)
              {
                TagModel tagModel = needResort[index];
                tagModel.sortOrder = nullable1.Value + num1 * (long) (index + 1);
                tagModel.status = tagModel.status == 2 ? 1 : tagModel.status;
              }
              int num2 = await App.Connection.UpdateAllAsync((IEnumerable) needResort);
            }
            needResort.Clear();
            needResort.Add(tag);
            nullable1 = new long?(t.sortOrder);
            nullable2 = new long?();
          }
        }
        else
          needResort.Add(tag);
        t = (TagModel) null;
      }
      if (needResort.Count > 1)
      {
        if (!nullable1.HasValue)
        {
          nullable2 = new long?(needResort[0].sortOrder - 268435456L);
          nullable1 = new long?(needResort[0].sortOrder - 268435456L);
        }
        nullable2 = new long?(nullable2 ?? nullable1.Value + 268435456L);
        long num3 = (nullable2.Value - nullable1.Value) / (long) (needResort.Count + 1);
        for (int index = 0; index < needResort.Count; ++index)
        {
          TagModel tagModel = needResort[index];
          tagModel.sortOrder = nullable1.Value + num3 * (long) (index + 1);
          tagModel.status = tagModel.status == 2 ? 1 : tagModel.status;
        }
        int num4 = await App.Connection.UpdateAllAsync((IEnumerable) needResort);
      }
      needResort = (List<TagModel>) null;
    }
  }
}
