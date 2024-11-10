// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Filter.DateRule
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.Filter
{
  public class DateRule : StringRule
  {
    public new object conditionType = (object) 1;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<object> or;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<object> not;

    public override string conditionName => "date";
  }
}
