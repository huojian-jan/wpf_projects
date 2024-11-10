// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TagModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class TagModel : BaseModel, ITimeline
  {
    private string _color;

    public string id { get; set; }

    [Indexed]
    public string name { get; set; }

    public string label { get; set; }

    public long sortOrder { get; set; }

    public string sortType { get; set; }

    [JsonIgnore]
    public int status { get; set; }

    public string userId { get; set; }

    public string viewMode { get; set; }

    public int type { get; set; }

    [JsonProperty("sortOption")]
    [Ignore]
    public SortOption SortOption { get; set; }

    [JsonIgnore]
    public string SortOptionString
    {
      get
      {
        return this.SortOption != null ? JsonConvert.SerializeObject((object) this.SortOption) : string.Empty;
      }
      set
      {
        this.SortOption = string.IsNullOrEmpty(value) ? (SortOption) null : JsonConvert.DeserializeObject<SortOption>(value);
      }
    }

    [Ignore]
    [JsonProperty("timeline")]
    public TimelineSyncModel SyncTimeline { get; set; }

    [JsonIgnore]
    public string SyncTimelineModel
    {
      get
      {
        return this.SyncTimeline != null ? JsonConvert.SerializeObject((object) this.SyncTimeline) : string.Empty;
      }
      set
      {
        this.SyncTimeline = string.IsNullOrEmpty(value) ? new TimelineSyncModel(Constants.SortType.project.ToString()) : JsonConvert.DeserializeObject<TimelineSyncModel>(value);
      }
    }

    [Ignore]
    [JsonIgnore]
    public ticktick_WPF.Models.TimelineModel Timeline { get; set; }

    [JsonIgnore]
    public string TimelineModel
    {
      get
      {
        return this.Timeline != null ? JsonConvert.SerializeObject((object) this.Timeline) : string.Empty;
      }
      set
      {
        this.Timeline = string.IsNullOrEmpty(value) ? new ticktick_WPF.Models.TimelineModel(Constants.SortType.project.ToString()) : JsonConvert.DeserializeObject<ticktick_WPF.Models.TimelineModel>(value);
      }
    }

    [JsonIgnore]
    public string pinyin { get; set; }

    [JsonIgnore]
    public string inits { get; set; }

    public string etag { get; set; }

    public int deleted { get; set; }

    public string color
    {
      get => this._color;
      set => this._color = value;
    }

    public string parent { get; set; }

    [JsonIgnore]
    public bool collapsed { get; set; }

    public int TypeOrder => this.type != 2 ? 1 : 2;

    public bool IsParent()
    {
      List<TagModel> tags = CacheManager.GetTags();
      return tags != null && tags.Any<TagModel>((Func<TagModel, bool>) (t => t.parent == this.name));
    }

    public bool IsChild() => !string.IsNullOrEmpty(this.parent) && this.parent != this.name;

    public bool IsEquals(TagModel tag)
    {
      return tag.name == this.name && tag.label == this.label && tag.sortOrder == this.sortOrder && tag.color == this.color && this.parent == tag.parent && tag.deleted == this.deleted && tag.viewMode == this.viewMode && object.Equals((object) tag.SortOption, (object) this.SortOption) && object.Equals((object) tag.SyncTimeline, (object) this.SyncTimeline);
    }

    public string GetDisplayName() => string.IsNullOrEmpty(this.label) ? this.name : this.label;

    public SortOption GetSortOption()
    {
      if (this.SortOption != null)
        return this.SortOption;
      if (string.IsNullOrEmpty(this.sortType))
        this.sortType = "project";
      return SortOptionUtils.GetSortOptionBySortType(Utils.LoadSortType(this.sortType), false);
    }

    public void SetUpdateStatus() => this.status = this.status != 0 ? 1 : 0;

    public int GetTagType() => this.type != 0 ? this.type : 1;
  }
}
