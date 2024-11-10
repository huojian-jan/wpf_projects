// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ReminderModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class ReminderModel
  {
    public int Type { get; set; }

    public string TaskId { get; set; }

    public string CheckItemId { get; set; }

    public string EventId { get; set; }

    public string HabitId { get; set; }

    public string Creator { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? ReminderTime { get; set; }

    public string Trigger { get; set; }

    public bool? IsAllDay { get; set; }

    public string Assignee { get; set; }

    public string ProjectId { get; set; }

    public string RepeatFlag { get; set; }

    public string Reminders { get; set; }

    public string Id { get; set; }

    public string GroupId { get; set; }

    public string Title { get; set; }

    public string Content { get; set; }

    public ReminderModel Copy() => (ReminderModel) this.MemberwiseClone();

    public string GetObjType()
    {
      string objType = "task";
      switch (this.Type)
      {
        case 1:
          objType = "checklist";
          break;
        case 2:
          objType = "calendar";
          break;
        case 4:
          objType = "habit";
          break;
        case 8:
          objType = "course";
          break;
      }
      return objType;
    }

    public string GetObjId()
    {
      string objId = this.Id;
      switch (this.Type)
      {
        case 0:
          objId = this.TaskId;
          break;
        case 1:
          objId = this.CheckItemId;
          break;
        case 2:
          objId = this.EventId;
          break;
        case 4:
          objId = this.HabitId;
          break;
      }
      return objId;
    }

    public bool CanDelayTomorrow() => this.Type != 8 && this.Type != 4;
  }
}
