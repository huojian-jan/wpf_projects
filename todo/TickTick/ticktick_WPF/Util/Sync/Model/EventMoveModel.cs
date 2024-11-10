// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.EventMoveModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class EventMoveModel
  {
    [JsonProperty(PropertyName = "destination")]
    public string Destination { get; set; }

    [JsonProperty(PropertyName = "eventId")]
    public string EventId { get; set; }
  }
}
