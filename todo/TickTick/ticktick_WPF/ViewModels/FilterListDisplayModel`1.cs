// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.FilterListDisplayModel`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Filter;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public abstract class FilterListDisplayModel<T> : CardViewModel
  {
    protected List<T> ValueList = new List<T>();

    protected FilterListDisplayModel() => this.Type = CardType.Filter;

    public virtual Rule<T> ToRule()
    {
      Rule<T> parseRule = this.GetParseRule();
      this.SetRuleValue(parseRule);
      return parseRule;
    }

    public abstract Rule<T> GetParseRule();

    protected void SetRuleValue(Rule<T> rule)
    {
      if (this.LogicType == LogicType.And)
        rule.and = this.ValueList;
      else if (this.LogicType == LogicType.Or)
        rule.or = this.ValueList;
      else
        rule.not = this.ValueList;
    }

    protected static string GetLogicString(LogicType logic)
    {
      switch (logic)
      {
        case LogicType.And:
          return Utils.GetString("and");
        case LogicType.Or:
          return Utils.GetString("or");
        case LogicType.Not:
          return Utils.GetString("Exclude");
        default:
          return string.Empty;
      }
    }
  }
}
