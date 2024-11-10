// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.MatrixRuleModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Models
{
  public class MatrixRuleModel
  {
    public QuadrantRulesModel Simple { get; set; }

    public QuadrantRulesModel Complex { get; set; }

    public QuadrantRulesModel Version0 { get; set; }

    public QuadrantRulesModel GetQuadrantRules(MatrixType type)
    {
      if (type == MatrixType.Simple)
        return this.Simple;
      return type == MatrixType.Version0 ? this.Version0 : this.Complex;
    }
  }
}
