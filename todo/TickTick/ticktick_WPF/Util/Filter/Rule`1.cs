// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Filter.Rule`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.Filter
{
  public class Rule<T>
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<T> and;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public object conditionType = (object) 1;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<T> not;
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public List<T> or;

    public virtual string conditionName { get; set; }
  }
}
