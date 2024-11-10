// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ModelSyncBean`1
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class ModelSyncBean<T>
  {
    [JsonProperty(PropertyName = "add")]
    public List<T> Add { get; set; } = new List<T>();

    [JsonProperty(PropertyName = "update")]
    public List<T> Update { get; set; } = new List<T>();

    [JsonProperty(PropertyName = "delete")]
    public List<string> Deleted { get; set; } = new List<string>();

    public bool Empty() => this.Add.Count == 0 && this.Update.Count == 0 && this.Deleted.Count == 0;
  }
}
