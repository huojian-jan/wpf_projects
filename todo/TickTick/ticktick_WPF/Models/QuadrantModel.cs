// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.QuadrantModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Models
{
  public class QuadrantModel
  {
    public string id { get; set; }

    public string rule { get; set; }

    public string sortType { get; set; }

    public long? sortOrder { get; set; }

    public string name { get; set; }

    [JsonProperty("sortOption")]
    public SortOption SortOption { get; set; }

    public SortOption GetSortOption()
    {
      return this.SortOption == null ? SortOptionUtils.GetSortOptionBySortType(Utils.LoadSortType(this.sortType), false) : this.SortOption;
    }

    public static QuadrantModel GetDefault(int level)
    {
      return new QuadrantModel()
      {
        id = "quadrant" + level.ToString(),
        sortType = "dueDate",
        name = string.Empty,
        sortOrder = new long?((long) (level - 1) * 268435456L),
        rule = MatrixManager.GetDefaultQuadrantRule(MatrixType.Simple, level)
      };
    }
  }
}
