// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.BatchData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class BatchData
  {
    public DateTime? StartDate;
    public DateTime? DueDate;
    public bool? IsAllDay;
    public string RepeatFlag;
    public string RepeatFrom;
    public string TimeZone;
    public string Assign;
    public bool IsFloating;

    public bool IsUnified => this.IsReminderUnified && this.IsRepeatUnified;

    public List<TaskReminderModel> Reminders { get; set; }

    public bool IsDateUnified { get; set; } = true;

    public bool IsTimeUnified { get; set; } = true;

    public bool IsReminderUnified { get; set; } = true;

    public bool IsRepeatUnified { get; set; } = true;

    public BatchData Clone() => (BatchData) this.MemberwiseClone();

    public void SetUnified()
    {
      this.IsTimeUnified = true;
      this.IsReminderUnified = true;
      this.IsRepeatUnified = true;
    }
  }
}
