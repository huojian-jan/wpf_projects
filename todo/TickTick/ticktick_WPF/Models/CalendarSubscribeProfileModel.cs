// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CalendarSubscribeProfileModel
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
  public class CalendarSubscribeProfileModel : ErrorModel
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("url")]
    public string Url { get; set; }

    [JsonProperty("show")]
    public string Show { get; set; }

    [JsonProperty("createdTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? CreatedTime { get; set; }

    [JsonIgnore]
    public string UserId { get; set; }

    [JsonProperty("color")]
    public string Color { get; set; }

    public bool Expired { get; set; }
  }
}
