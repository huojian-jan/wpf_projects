// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.PrintHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public static class PrintHelper
  {
    public static string GetSubTaskTimeText(SubtaskPrintViewModel item)
    {
      if (item.SnoozeReminderTime.HasValue && !Utils.IsEmptyDate(item.SnoozeReminderTime) && item.SnoozeReminderTime.Value > DateTime.Now)
      {
        string str = (item.SnoozeReminderTime.Value.Date == DateTime.Today ? Utils.GetString("Today") : Utils.GetString("Tomorrow")) + DateUtils.FormatHourMinuteText(item.SnoozeReminderTime.Value);
        return Utils.GetString("Today") + "," + string.Format(Utils.GetString("PreviewSnoozeText"), (object) str);
      }
      if (!item.StartDate.HasValue || Utils.IsEmptyDate(item.StartDate))
        return string.Empty;
      DateTime? nullable = item.StartDate;
      DateTime startDate = nullable.Value;
      nullable = new DateTime?();
      DateTime? dueDate = nullable;
      bool? isAllDay = item.IsAllDay;
      return DateUtils.FormatListDateString(startDate, dueDate, isAllDay);
    }
  }
}
