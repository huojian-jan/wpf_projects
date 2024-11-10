// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.FilterModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Linq;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;
using ticktick_WPF.ViewModels;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class FilterModel : BaseModel, ITimeline
  {
    [Indexed]
    public string id { get; set; }

    public string name { get; set; }

    public string rule { get; set; }

    public long sortOrder { get; set; }

    public string sortType { get; set; }

    public string etag { get; set; }

    public DateTime createdTime { get; set; }

    public DateTime modifiedTime { get; set; }

    public int syncStatus { get; set; }

    public int deleted { get; set; }

    public string userId { get; set; }

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

    public bool IsAvailable() => this.deleted != 1;

    public bool IsLocalAdded() => this.syncStatus == 0;

    public bool IsLocalUpdated()
    {
      return (this.syncStatus == 1 || this.syncStatus == 3) && this.deleted != 1;
    }

    public bool IsLocalDeleted()
    {
      return (this.syncStatus == 1 || this.syncStatus == 3) && this.deleted == 1;
    }

    public bool ContainsDate()
    {
      if (Parser.GetFilterRuleType(this.rule) != 0)
        return Parser.ToAdvanceModel(this.rule).CardList.Where<CardViewModel>((Func<CardViewModel, bool>) (card => card.Type == CardType.Filter)).ToList<CardViewModel>().Any<CardViewModel>((Func<CardViewModel, bool>) (c => c is DateCardViewModel dateCardViewModel && dateCardViewModel.Values != null && dateCardViewModel.Values.Any<string>((Func<string, bool>) (v => v != "nodue" && v != "recurring"))));
      NormalFilterViewModel normalModel = Parser.ToNormalModel(this.rule);
      return normalModel.DueDates != null && normalModel.DueDates.Any<string>((Func<string, bool>) (d => d != "nodue" && d != "recurring"));
    }

    public FilterModel Copy() => (FilterModel) this.MemberwiseClone();

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
