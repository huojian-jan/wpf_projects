// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.RemindMessage
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class RemindMessage
  {
    public RemindMessage(ReminderModel reminderModel)
    {
      if (reminderModel.Type == 8)
      {
        this.id = reminderModel.Id;
        this.type = "course";
      }
      else if (!string.IsNullOrEmpty(reminderModel.HabitId))
      {
        this.id = reminderModel.HabitId;
        this.type = "habit";
      }
      else if (!string.IsNullOrEmpty(reminderModel.EventId))
      {
        this.id = reminderModel.EventId;
        this.type = "calendar";
      }
      else if (!string.IsNullOrEmpty(reminderModel.CheckItemId))
      {
        this.id = reminderModel.CheckItemId;
        this.type = "checklist";
      }
      else
      {
        this.id = reminderModel.TaskId;
        this.type = "task";
      }
      if (!reminderModel.ReminderTime.HasValue)
        return;
      this.remindTime = reminderModel.ReminderTime.Value;
    }

    public RemindMessage()
    {
    }

    public string id { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime remindTime { get; set; }

    public string type { get; set; }
  }
}
