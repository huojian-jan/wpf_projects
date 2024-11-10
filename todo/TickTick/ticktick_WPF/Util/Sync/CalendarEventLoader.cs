// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.CalendarEventLoader
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public static class CalendarEventLoader
  {
    private static DateTime? _loadedStart;
    private static DateTime? _loadedEnd;
    private static bool _inited;

    public static void Reset() => CalendarEventLoader._inited = false;

    private static void Init(DateTime? start = null, DateTime? end = null)
    {
      CalendarEventLoader._loadedStart = start;
      CalendarEventLoader._loadedEnd = end;
      CalendarEventLoader._inited = true;
    }

    public static async void PullRemoteEvent(DateTime start, DateTime end)
    {
      if (!CalendarEventLoader._inited)
        CalendarEventLoader.Init(new DateTime?(start), new DateTime?(end));
      List<BindCalendarAccountModel> calendarAccounts = await BindCalendarAccountDao.GetBindCalendarAccounts();
      if (calendarAccounts == null)
        ;
      else if (calendarAccounts.Count == 0)
        ;
      else
        DelayActionHandlerCenter.TryDoAction("PullEventInCalendar", (EventHandler) (async (o, e) =>
        {
          DateTime dateTime1 = start;
          DateTime? nullable = CalendarEventLoader._loadedStart;
          if ((nullable.HasValue ? (dateTime1 >= nullable.GetValueOrDefault() ? 1 : 0) : 0) != 0)
          {
            DateTime dateTime2 = end;
            nullable = CalendarEventLoader._loadedEnd;
            if ((nullable.HasValue ? (dateTime2 <= nullable.GetValueOrDefault() ? 1 : 0) : 0) != 0)
              return;
          }
          if (start < CalendarEventLoader._loadedStart.Value && (CalendarEventLoader._loadedStart.Value - start).TotalDays <= 100.0)
          {
            if (end < CalendarEventLoader._loadedEnd.Value)
              end = CalendarEventLoader._loadedStart.Value;
            else
              CalendarEventLoader._loadedEnd = new DateTime?(end);
            CalendarEventLoader._loadedStart = new DateTime?(start);
          }
          else if (end > CalendarEventLoader._loadedEnd.Value && (end - CalendarEventLoader._loadedEnd.Value).TotalDays <= 100.0)
          {
            if (start > CalendarEventLoader._loadedStart.Value)
              start = CalendarEventLoader._loadedEnd.Value;
            else
              CalendarEventLoader._loadedStart = new DateTime?(start);
            CalendarEventLoader._loadedEnd = new DateTime?(end);
          }
          for (int times = 0; CalendarService.IsPulling && times <= 120; ++times)
            await Task.Delay(500);
          CalendarService.PullAccountCalendarsAndEvents(new DateTime?(start), new DateTime?(end));
        }), 600);
    }
  }
}
