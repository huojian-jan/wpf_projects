// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TimelineSyncModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TimelineSyncModel
  {
    [JsonProperty("sortType")]
    public string SortType { get; set; } = Constants.SortType.sortOrder.ToString();

    [JsonProperty("sortOption")]
    public SortOption SortOption { get; set; }

    public TimelineSyncModel()
    {
    }

    public TimelineSyncModel(string sortType)
    {
      this.SortType = sortType;
      SortOption sortOption;
      if (!(sortType == "dueDate"))
      {
        sortOption = new SortOption()
        {
          groupBy = this.SortType,
          orderBy = "sortOrder"
        };
      }
      else
      {
        sortOption = new SortOption();
        sortOption.groupBy = "none";
        sortOption.orderBy = "dueDate";
      }
      this.SortOption = sortOption;
    }

    public override bool Equals(object obj)
    {
      return obj is TimelineSyncModel timelineSyncModel && object.Equals((object) this.SortOption, (object) timelineSyncModel.SortOption);
    }
  }
}
