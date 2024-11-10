// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.SmartProjectOption
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using ticktick_WPF.Views.Timeline;

#nullable disable
namespace ticktick_WPF.Models
{
  public class SmartProjectOption
  {
    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("viewMode")]
    public string ViewMode { get; set; }

    [JsonProperty("sortOption")]
    public SortOption SortOption { get; set; }

    [JsonProperty("timeline")]
    public TimelineModel Timeline { get; set; }

    public void SaveRemote(SmartProjectOption remote)
    {
      if (remote == null)
        return;
      this.SortOption = remote.SortOption;
      this.ViewMode = remote.ViewMode;
      if (remote.Timeline == null)
        return;
      if (this.Timeline == null)
      {
        this.Timeline = remote.Timeline;
        this.Timeline.Range = TimelineConstants.RangeDefault;
      }
      else
        this.Timeline.sortOption = remote.Timeline.sortOption;
    }
  }
}
