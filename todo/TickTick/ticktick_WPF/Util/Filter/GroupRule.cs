// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Filter.GroupRule
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;

#nullable disable
namespace ticktick_WPF.Util.Filter
{
  public class GroupRule : StringRule
  {
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public new object conditionType;

    public override string conditionName => "group";
  }
}
