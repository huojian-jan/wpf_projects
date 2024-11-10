// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.EventArchiveArgs
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class EventArchiveArgs
  {
    public EventArchiveArgs(CalendarEventModel model)
    {
      this.StartTime = model.DueStart;
      this.EndTime = model.DueEnd;
      this.Id = model.Id;
      this.Title = model.Title;
      this.RepeatFlag = model.RepeatFlag;
      this.IsAllDay = model.IsAllDay;
    }

    public EventArchiveArgs(TaskBaseViewModel model)
    {
      this.StartTime = model.StartDate;
      this.EndTime = model.DueDate;
      this.Id = model.Id;
      this.Title = model.Title;
      this.RepeatFlag = model.RepeatFlag;
      this.IsAllDay = ((int) model.IsAllDay ?? 1) != 0;
    }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string Id { get; set; }

    public string Title { get; set; }

    public string RepeatFlag { get; set; }

    public bool IsAllDay { get; set; }
  }
}
