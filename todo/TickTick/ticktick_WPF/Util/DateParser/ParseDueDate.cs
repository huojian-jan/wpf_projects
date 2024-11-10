// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.ParseDueDate
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ticktick_WPF.Models;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  public class ParseDueDate : IPaserDueDate
  {
    private static Regex _remindMeRegex = new Regex("remind me|提醒我|リマインドする|알림|me rappeler|lembre-me|hапомнить мне");

    public string repeatFlag { get; set; }

    [JsonConverter(typeof (DateTimeConverter))]
    public DateTime? startDate { get; set; }

    [JsonConverter(typeof (DateTimeConverter))]
    public DateTime? dueDate { get; set; }

    public List<string> reminder { get; set; }

    public List<string> recognizeStrings { get; set; } = new List<string>();

    public bool isAllDay { get; set; }

    public string Text { get; set; }

    public List<string> GetRecognizeStrings() => this.recognizeStrings;

    public TimeData ToTimeData(bool addDefaultReminder)
    {
      DateTime? date = this.dueDate;
      if (this.isAllDay && !Utils.IsEmptyDate(date))
        date = date?.AddDays(1.0);
      TimeData timeData = new TimeData()
      {
        StartDate = this.startDate,
        DueDate = date,
        IsAllDay = new bool?(this.isAllDay),
        RepeatFlag = this.repeatFlag,
        IsDefault = false
      };
      this.reminder?.ForEach((Action<string>) (trigger => timeData.Reminders.Add(new TaskReminderModel()
      {
        trigger = trigger
      })));
      if (this.reminder != null && (this.UseDefaultReminder() && this.reminder.Count == 1 && this.reminder[0] == "TRIGGER:-PT0S" || this.reminder.Count == 0))
        timeData.Reminders = timeData.IsAllDay.Value ? TimeData.GetDefaultAllDayReminders() : TimeData.GetDefaultTimeReminders();
      return timeData;
    }

    private bool UseDefaultReminder()
    {
      if (string.IsNullOrEmpty(this.Text))
        return true;
      return !this.Text.ToLower().Contains("remind me") && !this.Text.Contains("提醒我");
    }

    public bool IsTimeSeted() => !Utils.IsEmptyDate(this.startDate);
  }
}
