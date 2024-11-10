// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.ClosedLoader.ClosedLoadStatus
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util.Sync.ClosedLoader
{
  public class ClosedLoadStatus : BaseModel
  {
    public string UserId { get; set; }

    public string EntityId { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime EarliestPulledDate { get; set; }

    public bool IsAllLoaded { get; set; }

    [Ignore]
    public bool IsNew { get; set; }
  }
}
