// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.TriggerValue
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Util
{
  public class TriggerValue
  {
    public string Value { get; set; }

    public int Length { get; set; }

    public string Unit { get; set; }

    public long Ordinal => (long) (this.Length * TriggerValue.GetUnitWeight(this.Unit));

    private static int GetUnitWeight(string unit)
    {
      switch (unit)
      {
        case "S":
          return 1;
        case "M":
          return 60;
        case "H":
          return 3600;
        case "W":
          return 86400;
        case "D":
          return 604800;
        default:
          return 0;
      }
    }
  }
}
