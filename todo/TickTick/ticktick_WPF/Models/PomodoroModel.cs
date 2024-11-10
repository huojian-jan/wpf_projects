// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.PomodoroModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.ComponentModel;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
  [Serializable]
  public class PomodoroModel : BaseModel
  {
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("taskSid")]
    public string TaskSid { get; set; }

    [JsonProperty("status")]
    public int Status { get; set; }

    [JsonProperty("startTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime StartTime { get; set; }

    [JsonProperty("endTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime EndTime { get; set; }

    [JsonIgnore]
    [NotNull]
    [DefaultValue("2")]
    public int SyncStatus { get; set; }

    [JsonProperty("etag")]
    public string Etag { get; set; }

    [JsonProperty("userId")]
    public string UserId { get; set; }

    [Ignore]
    [JsonProperty("tasks")]
    public PomoTask[] Tasks { get; set; }

    [JsonProperty("added")]
    public bool Added { get; set; }

    [JsonProperty("type")]
    public int Type { get; set; }

    [JsonProperty("pauseDuration")]
    public long PauseDuration { get; set; }

    [JsonProperty("note")]
    public string Note { get; set; }
  }
}
