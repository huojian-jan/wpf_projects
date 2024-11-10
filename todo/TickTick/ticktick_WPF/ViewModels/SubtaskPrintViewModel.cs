// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.SubtaskPrintViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class SubtaskPrintViewModel
  {
    public string Title { get; set; }

    public int Status { get; set; }

    public bool? IsAllDay { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? SnoozeReminderTime { get; set; }

    public int Level { get; set; }

    public string Kind { get; set; }

    public int Priority { get; set; }

    public SubtaskPrintViewModel(TaskDetailItemModel model)
    {
      this.Title = model.title;
      this.Status = model.status;
      this.IsAllDay = model.isAllDay;
      this.StartDate = model.startDate;
      this.SnoozeReminderTime = model.snoozeReminderTime;
      this.Kind = "TEXT";
      this.Level = 1;
    }

    public SubtaskPrintViewModel(DisplayItemModel model)
    {
      this.Title = model.Title;
      this.Status = model.Status;
      this.IsAllDay = model.IsAllDay;
      this.StartDate = model.StartDate;
      this.SnoozeReminderTime = model.RemindTime;
      this.Level = model.Level;
      this.Kind = model.Kind;
      this.Priority = model.Priority;
    }
  }
}
