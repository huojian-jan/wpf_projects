// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Provider.TagListData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Util.Provider
{
  public class TagListData : SortProjectData
  {
    private readonly string _tag;
    private readonly string _title;

    public TagListData(string tag)
    {
      this._tag = tag;
      TagModel tagModel = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == tag));
      this._title = tagModel?.GetDisplayName() ?? tag;
      this.EmptyPath = Utils.GetIconData("IcEmptyTag");
      this.AddTaskHint = tagModel?.GetDisplayName() ?? tag + " ";
      this.ShowAssignSort = false;
      this.ShowCustomSort = false;
      this.ShowLoadMore = false;
      this.ShowProjectSort = true;
    }

    public override string GetTitle() => this._title;

    public override async void SaveSortOption(SortOption sortOption)
    {
      TagModel tagByName = await TagDao.GetTagByName(this._tag);
      if (tagByName == null)
        return;
      tagByName.SortOption = sortOption;
      tagByName.sortType = sortOption.groupBy == "none" ? sortOption.orderBy : sortOption.groupBy;
      tagByName.status = 1;
      await TagDao.UpdateTag(tagByName);
    }

    public override DrawingImage GetEmptyImage()
    {
      return Application.Current?.FindResource((object) "EmptyTagDrawingImage") as DrawingImage;
    }
  }
}
