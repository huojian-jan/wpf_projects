// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TimeDataModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TimeDataModel
  {
    public string TaskId { get; set; }

    public string ItemId { get; set; }

    public string EventId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? DueDate { get; set; }

    public bool? IsAllDay { get; set; }

    public string RepeatFrom { get; set; }

    public string RepeatFlag { get; set; }

    public ReminderMode HandleReminderMode { get; set; }
  }
}
