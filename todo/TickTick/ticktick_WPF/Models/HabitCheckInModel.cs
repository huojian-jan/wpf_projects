// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.HabitCheckInModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class HabitCheckInModel : BaseModel
  {
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "habitId")]
    public string HabitId { get; set; }

    [JsonProperty(PropertyName = "checkinStamp")]
    public string CheckinStamp { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    [JsonProperty(PropertyName = "checkinTime")]
    public DateTime? CheckinTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? opTime { get; set; }

    [JsonProperty(PropertyName = "value")]
    public double Value { get; set; }

    [JsonProperty(PropertyName = "goal")]
    public double Goal { get; set; }

    [JsonIgnore]
    public string UserId { get; set; }

    [JsonIgnore]
    [Indexed]
    public int Status { get; set; }

    [JsonProperty(PropertyName = "status")]
    public int CheckStatus { get; set; }

    public bool IsUnComplete() => this.CheckStatus == 1;

    public bool IsComplete() => this.Value >= this.Goal && this.CheckStatus != 1;
  }
}
