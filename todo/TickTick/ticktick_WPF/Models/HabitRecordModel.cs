// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.HabitRecordModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class HabitRecordModel : BaseModel
  {
    [JsonProperty(PropertyName = "content")]
    public string Content { get; set; }

    [JsonProperty(PropertyName = "habitId")]
    public string HabitId { get; set; }

    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "stamp")]
    public int Stamp { get; set; }

    [JsonProperty(PropertyName = "emoji")]
    public int Emoji { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? opTime { get; set; }

    [JsonIgnore]
    public string UserId { get; set; }

    [JsonIgnore]
    public int Status { get; set; }

    [JsonIgnore]
    public int Deleted { get; set; }
  }
}
