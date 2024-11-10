// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.SyncTaskBeanLimitPropsContractResolver
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class SyncTaskBeanLimitPropsContractResolver : DefaultContractResolver
  {
    private string[] props;
    private bool retain;

    public SyncTaskBeanLimitPropsContractResolver(string[] props, bool retain = true)
    {
      this.props = props;
      this.retain = retain;
    }

    protected override IList<JsonProperty> CreateProperties(
      Type type,
      MemberSerialization memberSerialization)
    {
      return (IList<JsonProperty>) base.CreateProperties(type, memberSerialization).Where<JsonProperty>((Func<JsonProperty, bool>) (p => this.retain ? ((IEnumerable<string>) this.props).Contains<string>(p.PropertyName) : !((IEnumerable<string>) this.props).Contains<string>(p.PropertyName))).ToList<JsonProperty>();
    }
  }
}
