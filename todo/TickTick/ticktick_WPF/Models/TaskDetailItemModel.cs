// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TaskDetailItemModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using ticktick_WPF.Util;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class TaskDetailItemModel : BaseModel
  {
    [Indexed]
    public string id { get; set; }

    public int TaskId { get; set; }

    [Indexed]
    public string TaskServerId { get; set; }

    public string title { get; set; }

    public int status { get; set; }

    public long sortOrder { get; set; }

    public bool? isAllDay { get; set; }

    [Ignore]
    private DateTime? CompletedTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? completedTime
    {
      get => !Utils.IsEmptyDate(this.CompletedTime) ? this.CompletedTime : new DateTime?();
      set => this.CompletedTime = value;
    }

    [JsonIgnore]
    public DateTime? startDate { get; set; }

    [Ignore]
    [JsonProperty(PropertyName = "startDate")]
    public string serverStartDate { get; set; }

    [Ignore]
    private DateTime? SnoozeReminderTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? snoozeReminderTime
    {
      get
      {
        return !Utils.IsEmptyDate(this.SnoozeReminderTime) ? this.SnoozeReminderTime : new DateTime?();
      }
      set => this.SnoozeReminderTime = value;
    }

    public TaskDetailItemModel Copy() => (TaskDetailItemModel) this.MemberwiseClone();
  }
}
