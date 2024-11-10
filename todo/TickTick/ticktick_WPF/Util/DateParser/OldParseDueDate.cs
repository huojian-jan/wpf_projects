// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.OldParseDueDate
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  public class OldParseDueDate : IPaserDueDate
  {
    public string RepeatFlag;
    public DateTime DueDate;
    private string _reminder;
    public List<string> RecognizeStrings = new List<string>();

    public TickTickDuration Duration { private get; set; }

    public void SetDueDate(DateTime dueDate) => this.DueDate = dueDate;

    public void SetReminder(string reminder) => this._reminder = reminder;

    public TimeData ToTimeData(bool addDefaultReminder)
    {
      if (Utils.IsEmptyDate(this.DueDate))
        this.DueDate = DateTime.Today;
      TimeData timeData1 = new TimeData();
      if (this.Duration != null)
      {
        if (Utils.IsEmptyDate(this.DueDate) && !string.IsNullOrEmpty(this._reminder))
          this.DueDate = DateTime.Now.Date;
        if (!this.Duration.IsPositive())
          this.SetReminder(this.Duration.ToString());
      }
      if (!Utils.IsEmptyDate(this.DueDate))
      {
        timeData1.StartDate = new DateTime?(this.DueDate);
        timeData1.IsAllDay = new bool?(this._reminder == null);
      }
      if (!string.IsNullOrEmpty(this.RepeatFlag))
        timeData1.RepeatFlag = this.RepeatFlag;
      if (this._reminder != null)
      {
        TimeData timeData2 = timeData1;
        List<TaskReminderModel> taskReminderModelList;
        if (!(this._reminder == "TRIGGER:PT0S"))
          taskReminderModelList = new List<TaskReminderModel>()
          {
            new TaskReminderModel()
            {
              trigger = this._reminder,
              id = Utils.GetGuid()
            }
          };
        else
          taskReminderModelList = TimeData.GetDefaultTimeReminders();
        timeData2.Reminders = taskReminderModelList;
      }
      else
        timeData1.Reminders = TimeData.GetDefaultAllDayReminders();
      if (addDefaultReminder)
        timeData1.Reminders = !timeData1.IsAllDay.HasValue || !timeData1.IsAllDay.Value ? TimeData.GetDefaultTimeReminders() : TimeData.GetDefaultAllDayReminders();
      return timeData1;
    }

    public List<string> GetRecognizeStrings() => this.RecognizeStrings;

    public bool IsTimeSeted() => !Utils.IsEmptyDate(this.DueDate);
  }
}
