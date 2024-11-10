// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.EventArchivePostModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class EventArchivePostModel
  {
    public List<EventArchiveSyncModel> add { get; set; } = new List<EventArchiveSyncModel>();

    public List<string> delete { get; set; } = new List<string>();

    [JsonIgnore]
    public bool Empty => this.add.Count == 0 && this.delete.Count == 0;
  }
}
