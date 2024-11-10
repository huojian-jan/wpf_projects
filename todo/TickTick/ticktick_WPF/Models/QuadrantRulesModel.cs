// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.QuadrantRulesModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Models
{
  public class QuadrantRulesModel
  {
    public string Quadrant1 { get; set; }

    public string Quadrant2 { get; set; }

    public string Quadrant3 { get; set; }

    public string Quadrant4 { get; set; }

    public static QuadrantRulesModel GetDefault()
    {
      return new QuadrantRulesModel()
      {
        Quadrant1 = "{\"and\":[{\"or\":[5],\"conditionName\":\"priority\",\"conditionType\":1}],\"type\":0,\"version\":1}",
        Quadrant2 = "{\"and\":[{\"or\":[3],\"conditionName\":\"priority\",\"conditionType\":1}],\"type\":0,\"version\":1}",
        Quadrant3 = "{\"and\":[{\"or\":[1],\"conditionName\":\"priority\",\"conditionType\":1}],\"type\":0,\"version\":1}",
        Quadrant4 = "{\"and\":[{\"or\":[0],\"conditionName\":\"priority\",\"conditionType\":1}],\"type\":0,\"version\":1}"
      };
    }
  }
}
