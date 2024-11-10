// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.MoveCheckModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class MoveCheckModel
  {
    public string id { get; set; }

    public string projectId { get; set; }

    public string etag { get; set; }

    public long etimestamp { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? modifiedTime { get; set; }
  }
}
