// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ProjectGroupModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class ProjectGroupModel : BaseModel, ITimeline
  {
    public string id { get; set; }

    public string etag { get; set; }

    public string name { get; set; } = string.Empty;

    public long? sortOrder { get; set; }

    public int deleted { get; set; }

    public int userId { get; set; }

    public string sortType { get; set; }

    public bool open { get; set; }

    public bool showAll { get; set; } = true;

    [JsonIgnore]
    public string sync_status { get; set; }

    public string teamId { get; set; }

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

    public string viewMode { get; set; }

    [JsonProperty("timeline")]
    [Ignore]
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

    public ProjectGroupModel Clone() => this.MemberwiseClone() as ProjectGroupModel;

    public SortOption GetSortOption()
    {
      if (this.SortOption != null)
        return this.SortOption;
      if (string.IsNullOrEmpty(this.sortType))
        this.sortType = "project";
      return SortOptionUtils.GetSortOptionBySortType(Utils.LoadSortType(this.sortType), this.viewMode == "kanban");
    }
  }
}
