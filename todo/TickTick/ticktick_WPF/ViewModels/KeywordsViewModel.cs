// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.KeywordsViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using ticktick_WPF.Util.Filter;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class KeywordsViewModel : FilterListDisplayModel<string>
  {
    public bool IsNewAdd;

    public KeywordsViewModel() => this.ConditionName = "keywords";

    public string Keyword
    {
      get
      {
        List<string> valueList = this.ValueList;
        return (valueList != null ? (__nonvirtual (valueList.Count) > 0 ? 1 : 0) : 0) == 0 ? string.Empty : this.ValueList[0];
      }
      set => this.ValueList = new List<string>() { value };
    }

    public override Rule<string> GetParseRule() => (Rule<string>) new KeywordsRule();
  }
}
