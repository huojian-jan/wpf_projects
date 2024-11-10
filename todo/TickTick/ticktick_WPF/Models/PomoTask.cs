// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.PomoTask
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
  [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
  public class PomoTask : BaseModel
  {
    [JsonProperty("taskId")]
    public string TaskId { get; set; }

    [JsonProperty("habitId")]
    public string HabitId { get; set; }

    [JsonIgnore]
    public string PomoId { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; }

    [JsonProperty("projectName")]
    public string ProjectName { get; set; }

    [Ignore]
    [JsonProperty("tags")]
    public string[] Tags { get; set; }

    [JsonIgnore]
    public string TagString { get; set; }

    [JsonProperty("startTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime StartTime { get; set; }

    [JsonProperty("endTime")]
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime EndTime { get; set; }

    [JsonProperty("timerId")]
    public string TimerSid { get; set; }

    [JsonProperty("timerName")]
    public string TimerName { get; set; }

    public string GetId()
    {
      if (!string.IsNullOrEmpty(this.TimerSid))
        return this.TimerSid;
      if (!string.IsNullOrEmpty(this.TaskId))
        return this.TaskId;
      return !string.IsNullOrEmpty(this.HabitId) ? this.HabitId : (string) null;
    }

    public string GetEntityId()
    {
      if (!string.IsNullOrEmpty(this.TaskId))
        return this.TaskId;
      return !string.IsNullOrEmpty(this.HabitId) ? this.HabitId : (string) null;
    }

    public long GetTodayDuration()
    {
      return (long) (this.EndTime - (this.StartTime > DateTime.Today ? this.StartTime : DateTime.Today)).TotalSeconds;
    }

    public PomoTask Copy() => (PomoTask) this.MemberwiseClone();

    public string GetTitle() => string.IsNullOrEmpty(this.Title) ? this.TimerName : this.Title;
  }
}
