// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ReminderTime.EventReminderCalculator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Service;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util.ReminderTime
{
  public class EventReminderCalculator
  {
    private static bool _needReInit;
    private static bool _loading;

    public static async Task InitAllEventsReminderTimes() => EventReminderCalculator.InitAll();

    private static async Task InitAll()
    {
      if (EventReminderCalculator._loading)
      {
        EventReminderCalculator._needReInit = true;
      }
      else
      {
        EventReminderCalculator._loading = true;
        try
        {
          Dictionary<string, ReminderDelayModel> delayDict = (await ReminderDelayDao.GetDelayModelByType("calendar")).ToDictionaryEx<ReminderDelayModel, string, ReminderDelayModel>((Func<ReminderDelayModel, string>) (d => d.ObjId), (Func<ReminderDelayModel, ReminderDelayModel>) (d => d));
          List<CalendarEventModel> events = await App.Connection.QueryAsync<CalendarEventModel>("select * from CalendarEventModel where (CalendarId in (select Id as CalendarId from BindCalendarModel where Show = 'show') or  CalendarId in (select Id as CalendarId from CalendarSubscribeProfileModel where Show = 'show')) and UserId = '" + Utils.GetCurrentUserIdInt().ToString() + "' and Deleted = '0'");
          List<CalendarEventModel> skipEvents = await CalendarEventDao.GetSkipEvents();
          List<string> hiddenKeys = await ArchivedDao.GetArchivedKeys();
          if (events != null && events.Any<CalendarEventModel>())
          {
            foreach (CalendarEventModel eve in events)
            {
              await ReminderTimeDao.DeleteReminderTimeByIdAndType(eve.EventId, 2);
              List<ReminderTimeModel> eventReminders = EventReminderCalculator.GetEventReminders(eve, delayDict, hiddenKeys, skipEvents);
              if (eventReminders != null)
                await ReminderTimeDao.AddReminderTimes(eventReminders);
            }
          }
          delayDict = (Dictionary<string, ReminderDelayModel>) null;
          events = (List<CalendarEventModel>) null;
          skipEvents = (List<CalendarEventModel>) null;
          hiddenKeys = (List<string>) null;
        }
        finally
        {
          EventReminderCalculator._loading = false;
          if (EventReminderCalculator._needReInit)
          {
            EventReminderCalculator._needReInit = false;
            EventReminderCalculator.InitAll();
          }
        }
      }
    }

    public static List<ReminderTimeModel> GetEventReminders(
      CalendarEventModel eve,
      Dictionary<string, ReminderDelayModel> delayDict,
      List<string> hiddenKeys,
      List<CalendarEventModel> skipEvents)
    {
      if (CacheManager.GetBindCalendarById(eve.CalendarId) == null && CacheManager.GetSubscribeCalById(eve.CalendarId) == null)
        return (List<ReminderTimeModel>) null;
      List<ReminderTimeModel> result = new List<ReminderTimeModel>();
      DateTime? dueStart = eve.DueStart;
      if (dueStart.HasValue)
      {
        ReminderDelayModel delay = (ReminderDelayModel) null;
        if (!string.IsNullOrEmpty(eve.EventId) && delayDict.ContainsKey(eve.EventId))
          delay = delayDict[eve.EventId];
        string eventKey = ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(eve));
        List<int> mins = eve.Reminders != null ? JsonConvert.DeserializeObject<List<int>>(eve.Reminders) : new List<int>();
        if (!hiddenKeys.Contains(eventKey))
        {
          List<ReminderTimeModel> reminderTimeModelList = result;
          CalendarEventModel ev = eve;
          dueStart = eve.DueStart;
          DateTime date = dueStart.Value;
          ReminderDelayModel delay1 = delay;
          List<int> mins1 = mins;
          List<ReminderTimeModel> collection = EventReminderCalculator.AddReminder(ev, date, delay1, mins1);
          reminderTimeModelList.AddRange((IEnumerable<ReminderTimeModel>) collection);
        }
        if (!string.IsNullOrEmpty(eve.RepeatFlag))
        {
          List<DateTime> exDateList = (List<DateTime>) null;
          if (!string.IsNullOrEmpty(eve.ExDates))
          {
            try
            {
              exDateList = JsonConvert.DeserializeObject<List<DateTime>>(eve.ExDates);
            }
            catch (Exception ex)
            {
            }
          }
          DateTime targetTzTime;
          if (!eve.IsAllDay)
          {
            dueStart = eve.DueStart;
            targetTzTime = TimeZoneUtils.LocalToTargetTzTime(dueStart.Value, eve.TimeZone);
          }
          else
          {
            dueStart = eve.DueStart;
            targetTzTime = dueStart.Value;
          }
          DateTime dateTime1 = targetTzTime;
          string repeatFlag = eve.RepeatFlag;
          DateTime calStart = dateTime1;
          DateTime today = DateTime.Today;
          DateTime checkStart = today.AddDays(-1.0);
          today = DateTime.Today;
          DateTime checkEnd = today.AddDays(4.0);
          string timeZone = eve.TimeZone;
          int num = eve.IsAllDay ? 1 : 0;
          RepeatDao.GetEventRepeatDates(repeatFlag, calStart, checkStart, checkEnd, timeZone, num != 0).ForEach((Action<DateTime>) (date =>
          {
            CalendarEventModel calendarEventModel = CalendarEventModel.Copy(eve);
            calendarEventModel.DueStart = new DateTime?(date);
            if (eve.DueEnd.HasValue)
            {
              double totalMinutes = (eve.DueEnd.Value - eve.DueStart.Value).TotalMinutes;
              calendarEventModel.DueEnd = new DateTime?(date.AddMinutes(totalMinutes));
            }
            DateTime dateTime2 = calendarEventModel.IsAllDay ? date : TimeZoneUtils.LocalToTargetTzTime(date, calendarEventModel.TimeZone);
            bool flag1 = false;
            bool flag2 = false;
            if (exDateList != null && exDateList.Any<DateTime>() && exDateList.Contains(dateTime2.Date))
              flag1 = true;
            if (skipEvents.Any<CalendarEventModel>())
              flag2 = calendarEventModel.IsSkipped(skipEvents, new DateTime?(date));
            if (flag1 || flag2 || hiddenKeys.Contains(ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(calendarEventModel))))
              return;
            result.AddRange((IEnumerable<ReminderTimeModel>) EventReminderCalculator.AddReminder(calendarEventModel, date, delay, mins));
          }));
        }
      }
      return result;
    }

    private static List<ReminderTimeModel> AddReminder(
      CalendarEventModel ev,
      DateTime date,
      ReminderDelayModel delay,
      List<int> mins)
    {
      List<ReminderTimeModel> reminderTimeModelList = new List<ReminderTimeModel>();
      DateTime dateTime1 = date;
      if (delay != null)
      {
        DateTime? nullable = delay.RemindTime;
        DateTime dateTime2 = date;
        if ((nullable.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == dateTime2 ? 1 : 0) : 1) : 0) != 0)
        {
          nullable = delay.NextTime;
          if (nullable.HasValue)
          {
            nullable = delay.NextTime;
            dateTime1 = nullable.Value;
          }
        }
      }
      if (dateTime1 >= DateTime.Now)
        reminderTimeModelList.Add(new ReminderTimeModel(ev.EventId, 2, dateTime1.Ticks));
      if (!string.IsNullOrEmpty(ev.Reminders) && mins != null && mins.Any<int>())
      {
        foreach (int min in mins)
        {
          if (min != 0)
          {
            DateTime dateTime3 = date.AddMinutes((double) (min * -1));
            if (dateTime3 > DateTime.Now)
              reminderTimeModelList.Add(new ReminderTimeModel(ev.EventId, 2, dateTime3.Ticks));
          }
        }
      }
      return reminderTimeModelList;
    }

    public static async Task RecalEventReminders(CalendarEventModel eve)
    {
      Dictionary<string, ReminderDelayModel> delayDict;
      List<CalendarEventModel> skipEvents;
      if (eve == null)
      {
        delayDict = (Dictionary<string, ReminderDelayModel>) null;
        skipEvents = (List<CalendarEventModel>) null;
      }
      else if (!ABTestManager.IsNewRemindCalculate())
      {
        delayDict = (Dictionary<string, ReminderDelayModel>) null;
        skipEvents = (List<CalendarEventModel>) null;
      }
      else
      {
        await ReminderTimeDao.DeleteReminderTimeByIdAndType(eve.EventId, 2);
        delayDict = (await ReminderDelayDao.GetDelayModelByType("calendar")).ToDictionaryEx<ReminderDelayModel, string, ReminderDelayModel>((Func<ReminderDelayModel, string>) (d => d.ObjId), (Func<ReminderDelayModel, ReminderDelayModel>) (d => d));
        skipEvents = await CalendarEventDao.GetSkipEvents();
        List<ReminderTimeModel> eventReminders = EventReminderCalculator.GetEventReminders(eve, delayDict, await ArchivedDao.GetArchivedKeys(), skipEvents);
        if (eventReminders == null)
        {
          delayDict = (Dictionary<string, ReminderDelayModel>) null;
          skipEvents = (List<CalendarEventModel>) null;
        }
        else
        {
          await ReminderTimeDao.AddReminderTimes(eventReminders);
          delayDict = (Dictionary<string, ReminderDelayModel>) null;
          skipEvents = (List<CalendarEventModel>) null;
        }
      }
    }
  }
}
