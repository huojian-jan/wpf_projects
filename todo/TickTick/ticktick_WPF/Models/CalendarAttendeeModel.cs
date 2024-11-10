// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CalendarAttendeeModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class CalendarAttendeeModel : BaseModel
  {
    [JsonProperty("email")]
    public string Email { get; set; }

    [JsonProperty("responseStatus")]
    public string ResponseStatus { get; set; }

    [JsonProperty("self")]
    public bool Self { get; set; }

    [JsonProperty("organizer")]
    public bool Organizer { get; set; }

    [JsonIgnore]
    public bool FirstGuest { get; set; }

    [JsonIgnore]
    public string EventId { get; set; }
  }
}
