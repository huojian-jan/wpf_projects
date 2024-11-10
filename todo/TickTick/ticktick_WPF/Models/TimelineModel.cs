// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TimelineModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Timeline;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class TimelineModel
  {
    [JsonProperty("range")]
    public string Range { get; set; }

    public int DayWidthIndex { get; set; }

    [JsonProperty("sortType")]
    public string SortType { get; set; }

    [JsonProperty("sortOption")]
    public SortOption sortOption { get; set; }

    public TimelineModel()
    {
      this.Range = TimelineConstants.RangeDefault;
      this.SortType = TimelineConstants.SortDefault;
      this.sortOption = new SortOption()
      {
        groupBy = TimelineConstants.SortDefault,
        orderBy = "sortOrder"
      };
    }

    public TimelineModel(string sortType)
    {
      this.Range = TimelineConstants.RangeDefault;
      this.SortType = sortType;
      this.sortOption = new SortOption()
      {
        groupBy = sortType,
        orderBy = "sortOrder"
      };
    }

    public TimelineModel(string sortType, SortOption option)
    {
      this.Range = TimelineConstants.RangeDefault;
      this.SortType = sortType;
      SortOption sortOption = option;
      if (sortOption == null)
        sortOption = new SortOption()
        {
          groupBy = sortType,
          orderBy = "sortOrder"
        };
      this.sortOption = sortOption;
    }

    public bool IsEquals(TimelineModel model)
    {
      if (model == null || !(this.Range == model.Range) || this.DayWidthIndex != model.DayWidthIndex)
        return false;
      SortOption sortOption = model.sortOption;
      return sortOption != null && sortOption.Equal(this.sortOption);
    }

    public static void CheckTimelineEmpty(ITimeline timeline, Constants.SortType defaultSort)
    {
      if (timeline.Timeline == null)
        timeline.Timeline = new TimelineModel();
      if (timeline.Timeline.Range == null)
        timeline.Timeline.Range = TimelineConstants.RangeDefault;
      if (timeline.Timeline.SortType == null)
        timeline.Timeline.SortType = defaultSort.ToString();
      if (timeline.Timeline.sortOption != null)
        return;
      timeline.Timeline.sortOption = new SortOption()
      {
        groupBy = timeline.Timeline.SortType == "dueDate" ? "none" : timeline.Timeline.SortType,
        orderBy = timeline.Timeline.SortType == "dueDate" ? "dueDate" : "sortOrder"
      };
    }

    public TimelineModel Copy() => (TimelineModel) this.MemberwiseClone();

    public bool SyncPropertyChanged(TimelineModel model)
    {
      return model == null || !this.sortOption.Equal(model.sortOption);
    }

    public SortOption GetSortOption()
    {
      if (this.sortOption == null)
        this.sortOption = new SortOption()
        {
          groupBy = this.SortType == "dueDate" ? "none" : this.SortType,
          orderBy = this.SortType == "dueDate" ? "dueDate" : "sortOrder"
        };
      return new SortOption()
      {
        groupBy = this.sortOption.groupBy,
        orderBy = this.sortOption.orderBy
      };
    }
  }
}
