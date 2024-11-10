// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.CalendarEventDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Search;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class CalendarEventDao
  {
    private static List<CalendarEventModel> _skipEvents;

    public static async Task SaveEvents(List<CalendarEventModel> models, string calendarId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<CalendarEventModel> listAsync = await App.Connection.Table<CalendarEventModel>().Where((Expression<Func<CalendarEventModel, bool>>) (model => model.UserId == userId && model.CalendarId == calendarId)).ToListAsync();
      if (listAsync.Any<CalendarEventModel>())
      {
        foreach (object obj in listAsync)
        {
          int num = await App.Connection.DeleteAsync(obj);
        }
      }
      CalendarEventDao._skipEvents = (List<CalendarEventModel>) null;
      int num1 = await App.Connection.InsertAllAsync((IEnumerable) models);
    }

    public static async Task AddEvent(CalendarEventModel model)
    {
      int num = await App.Connection.InsertAsync((object) model);
    }

    public static async Task SaveEvent(CalendarEventModel model)
    {
      int num = await App.Connection.UpdateAsync((object) model);
    }

    public static async Task<CalendarEventModel> GetEventByEventIdOrId(string eventId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      CalendarEventModel calEvent = await App.Connection.Table<CalendarEventModel>().Where((Expression<Func<CalendarEventModel, bool>>) (model => model.UserId == userId && (model.EventId == eventId || model.Id == eventId))).FirstOrDefaultAsync();
      if (calEvent != null)
        CalendarEventDao.AssemblyCalendarEvent(calEvent);
      return calEvent;
    }

    public static async Task<CalendarEventModel> GetEventByEventId(string eventId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      CalendarEventModel calEvent = await App.Connection.Table<CalendarEventModel>().Where((Expression<Func<CalendarEventModel, bool>>) (model => model.UserId == userId && model.EventId == eventId)).FirstOrDefaultAsync();
      if (calEvent != null)
        CalendarEventDao.AssemblyCalendarEvent(calEvent);
      return calEvent;
    }

    public static async Task<CalendarEventModel> GetEventById(string eventId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      CalendarEventModel calEvent = await App.Connection.Table<CalendarEventModel>().Where((Expression<Func<CalendarEventModel, bool>>) (model => model.UserId == userId && model.Id == eventId)).FirstOrDefaultAsync();
      if (calEvent != null)
        CalendarEventDao.AssemblyCalendarEvent(calEvent);
      return calEvent;
    }

    private static void AssemblyCalendarEvent(CalendarEventModel calEvent)
    {
      bool flag = false;
      if (UserDao.IsUserValid())
      {
        BindCalendarModel bindCalendarModel = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == calEvent.CalendarId));
        if (bindCalendarModel != null && bindCalendarModel.Accessible)
          flag = true;
      }
      calEvent.Editable = flag;
      if (!string.IsNullOrEmpty(calEvent.AttendeeInfo))
        calEvent.Attendees = JsonConvert.DeserializeObject<List<CalendarAttendeeModel>>(calEvent.AttendeeInfo);
      if (!string.IsNullOrEmpty(calEvent.Reminders))
      {
        try
        {
          calEvent.ReminderList = JsonConvert.DeserializeObject<List<int>>(calEvent.Reminders);
        }
        catch (Exception ex)
        {
          calEvent.ReminderList = (List<int>) null;
        }
      }
      if (!string.IsNullOrEmpty(calEvent.ExDates))
      {
        try
        {
          calEvent.ExDateList = JsonConvert.DeserializeObject<List<DateTime>>(calEvent.ExDates);
        }
        catch (Exception ex)
        {
          calEvent.ExDateList = (List<DateTime>) null;
        }
      }
      if (!string.IsNullOrEmpty(calEvent.TimeZone))
        return;
      calEvent.TimeZone = Utils.GetLocalTimeZone();
    }

    private static async Task<List<CalendarEventModel>> GetEventsByCalendarId(
      string calendarId,
      int type = 1)
    {
      string str1 = Utils.GetCurrentUserIdInt().ToString();
      DateTime dateTime = DateTime.Today.AddSeconds(-1.0);
      string str2 = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
      dateTime = DateTime.Today;
      dateTime = dateTime.AddMonths(3);
      string str3 = dateTime.ToString("yyyy-MM-dd 23:59:59");
      return await App.Connection.QueryAsync<CalendarEventModel>(string.Format("select * from CalendarEventModel where Deleted = 0 and userId = '{0}' and Type = '{1}' and CalendarId = '{2}' and ", (object) str1, (object) type, (object) calendarId) + "(((DueStart is not null and DueStart >= '" + str2 + "' and DueStart < '" + str3 + "')) or ((DueEnd is not null and DueEnd > '" + str2 + "' and DueEnd < '" + str3 + "')) or (DueStart is not null and DueEnd is not null and DueStart < '" + str2 + "' and DueEnd > '" + str3 + "') or  (repeatFlag is not null and repeatFlag != '' )) ");
    }

    private static List<string> GetVisibleCalendarIds()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<string> list = CacheManager.GetSubscribeCalendars().Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => cal.Show == "show" && cal.UserId == userId)).ToList<CalendarSubscribeProfileModel>().Select<CalendarSubscribeProfileModel, string>((Func<CalendarSubscribeProfileModel, string>) (cal => cal.Id)).ToList<string>();
      List<string> accounts = CacheManager.GetBindCalendarAccounts().Select<BindCalendarAccountModel, string>((Func<BindCalendarAccountModel, string>) (cal => cal.Id)).ToList<string>();
      list.AddRange((IEnumerable<string>) CacheManager.GetBindCalendars().Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => accounts.Contains(cal.AccountId) && cal.Show == "show" && cal.UserId == userId)).Select<BindCalendarModel, string>((Func<BindCalendarModel, string>) (cal => cal.Id)).ToList<string>());
      return list;
    }

    public static async Task<List<CalendarEventModel>> GetEventDerivatives(string id, string uid)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<CalendarEventModel>().Where((Expression<Func<CalendarEventModel, bool>>) (calEvent => calEvent.Uid == uid && calEvent.Id != id && calEvent.UserId == userId)).ToListAsync();
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayEventsInCalendar(
      string calendarId,
      int type,
      DateTime start,
      DateTime end)
    {
      List<CalendarEventModel> events = await CalendarEventDao.GetEventsByCalendarId(calendarId, type);
      bool editable = true;
      BindCalendarModel bindCalendarModel = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == calendarId));
      if (bindCalendarModel != null && !bindCalendarModel.Accessible)
        editable = false;
      List<TaskBaseViewModel> list = new List<TaskBaseViewModel>();
      if (events.Any<CalendarEventModel>())
      {
        List<string> hiddenKeys = await ArchivedDao.GetArchivedKeys();
        List<CalendarEventModel> skipEvents = await CalendarEventDao.GetSkipEvents();
        foreach (CalendarEventModel calendarEventModel1 in events)
        {
          CalendarEventModel calendarEvent = calendarEventModel1;
          if (!string.IsNullOrEmpty(calendarEvent.ExDates))
          {
            try
            {
              calendarEvent.ExDateList = JsonConvert.DeserializeObject<List<DateTime>>(calendarEvent.ExDates);
            }
            catch (Exception ex)
            {
              calendarEvent.ExDateList = (List<DateTime>) null;
            }
          }
          calendarEvent.Editable = editable;
          DateTime? nullable1 = calendarEvent.DueStart;
          if (nullable1.HasValue)
          {
            nullable1 = calendarEvent.DueEnd;
            if (!nullable1.HasValue)
            {
              CalendarEventModel calendarEventModel2 = calendarEvent;
              nullable1 = calendarEvent.DueStart;
              DateTime? nullable2 = new DateTime?(nullable1.Value.AddDays(1.0));
              calendarEventModel2.DueEnd = nullable2;
            }
            DateTime? dueStart = calendarEvent.DueStart;
            DateTime? dueEnd = calendarEvent.DueEnd;
            if (calendarEvent.IsAllDay)
            {
              ref DateTime? local1 = ref dueStart;
              nullable1 = calendarEvent.DueStart;
              DateTime date1 = nullable1.Value.Date;
              local1 = new DateTime?(date1);
              ref DateTime? local2 = ref dueEnd;
              nullable1 = calendarEvent.DueEnd;
              DateTime date2 = nullable1.Value.Date;
              local2 = new DateTime?(date2);
            }
            nullable1 = dueStart;
            DateTime date3 = start.Date;
            if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() >= date3 ? 1 : 0) : 0) == 0)
            {
              nullable1 = dueEnd;
              DateTime date4 = start.Date;
              if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() > date4 ? 1 : 0) : 0) == 0)
                goto label_21;
            }
            nullable1 = dueEnd;
            DateTime date5 = end.Date;
            if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() <= date5 ? 1 : 0) : 0) != 0 && (string.IsNullOrEmpty(calendarEvent.RepeatFlag) ? 0 : (calendarEvent.IsSkipped(skipEvents, dueStart) ? 1 : 0)) == 0)
            {
              string eventKey = ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(calendarEvent));
              if (!hiddenKeys.Contains(eventKey))
                list.Add(new TaskBaseViewModel(calendarEvent));
            }
label_21:
            if (!string.IsNullOrEmpty(calendarEvent.RepeatFlag))
            {
              DateTime targetTzTime;
              if (!calendarEvent.IsAllDay)
              {
                nullable1 = calendarEvent.DueStart;
                targetTzTime = TimeZoneUtils.LocalToTargetTzTime(nullable1.Value, calendarEvent.TimeZone);
              }
              else
              {
                nullable1 = calendarEvent.DueStart;
                targetTzTime = nullable1.Value;
              }
              DateTime calStart = targetTzTime;
              double diff = 0.0;
              nullable1 = calendarEvent.DueEnd;
              if (nullable1.HasValue)
              {
                nullable1 = calendarEvent.DueEnd;
                DateTime dateTime1 = nullable1.Value;
                nullable1 = calendarEvent.DueStart;
                DateTime dateTime2 = nullable1.Value;
                diff = (dateTime1 - dateTime2).TotalMinutes;
              }
              List<DateTime> eventRepeatDates = RepeatDao.GetEventRepeatDates(calendarEvent.RepeatFlag, calStart, start.AddDays((double) (-2 - (int) diff / 1440)), end.AddDays(1.0), calendarEvent.TimeZone, calendarEvent.IsAllDay);
              string deriveId = calendarEvent.Id;
              int num = 0;
              Action<DateTime> action = (Action<DateTime>) (time =>
              {
                calendarEvent.Id = deriveId + "_time_" + num++.ToString();
                calendarEvent.DueStart = new DateTime?(time);
                if (calendarEvent.DueEnd.HasValue)
                {
                  DateTime dateTime = calendarEvent.IsAllDay ? time : TimeZoneUtils.LocalToTargetTzTime(time, calendarEvent.TimeZone);
                  calendarEvent.DueEnd = new DateTime?(calendarEvent.IsAllDay ? dateTime.AddMinutes(diff) : TimeZoneUtils.ToLocalTime(dateTime.AddMinutes(diff), calendarEvent.TimeZone));
                }
                bool flag1 = calendarEvent.IsSkipped(skipEvents);
                bool flag2 = calendarEvent.ExDateList != null && calendarEvent.ExDateList.Any<DateTime>() && calendarEvent.ExDateList.Contains(time.Date);
                DateTime? nullable3 = calendarEvent.DueStart;
                DateTime today1 = DateTime.Today;
                if ((nullable3.HasValue ? (nullable3.GetValueOrDefault() >= today1 ? 1 : 0) : 0) == 0)
                {
                  nullable3 = calendarEvent.DueEnd;
                  DateTime today2 = DateTime.Today;
                  if ((nullable3.HasValue ? (nullable3.GetValueOrDefault() > today2 ? 1 : 0) : 0) == 0)
                    return;
                }
                if (flag1 || flag2 || hiddenKeys.Contains(ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(calendarEvent))))
                  return;
                list.Add(new TaskBaseViewModel(calendarEvent));
              });
              eventRepeatDates.ForEach(action);
            }
          }
        }
      }
      List<TaskBaseViewModel> eventsInCalendar = list;
      events = (List<CalendarEventModel>) null;
      return eventsInCalendar;
    }

    public static async Task<List<CalendarEventModel>> GetSkipEvents()
    {
      if (CalendarEventDao._skipEvents == null)
        CalendarEventDao._skipEvents = await App.Connection.QueryAsync<CalendarEventModel>("select Id,Uid,EventId,OriginalStartTime,IsAllDay,DueStart from CalendarEventModel where Deleted = 0 and userId = '" + Utils.GetCurrentUserIdInt().ToString() + "' and OriginalStartTime is not null") ?? new List<CalendarEventModel>();
      List<CalendarEventModel> skipEvents = CalendarEventDao._skipEvents;
      return skipEvents != null ? skipEvents.ToList<CalendarEventModel>() : (List<CalendarEventModel>) null;
    }

    public static async Task<List<TaskBaseViewModel>> GetDisplayEventsInBindAccount(
      string accountId,
      DateTime start,
      DateTime end)
    {
      List<TaskBaseViewModel> models = new List<TaskBaseViewModel>();
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<BindCalendarModel> listAsync = await App.Connection.Table<BindCalendarModel>().Where((Expression<Func<BindCalendarModel, bool>>) (calendar => calendar.UserId == userId && calendar.AccountId == accountId && calendar.Show != "hidden")).ToListAsync();
      if (listAsync.Any<BindCalendarModel>())
      {
        foreach (BindCalendarModel bindCalendarModel in listAsync)
        {
          List<TaskBaseViewModel> eventsInCalendar = await CalendarEventDao.GetDisplayEventsInCalendar(bindCalendarModel.Id, 0, start, end);
          if (eventsInCalendar != null && eventsInCalendar.Any<TaskBaseViewModel>())
            models.AddRange((IEnumerable<TaskBaseViewModel>) eventsInCalendar);
        }
      }
      List<TaskBaseViewModel> eventsInBindAccount = models;
      models = (List<TaskBaseViewModel>) null;
      return eventsInBindAccount;
    }

    public static async Task<List<CalendarEventModel>> GetEventsBetweenSpan(
      DateTime start,
      DateTime end,
      bool inCalendar = true)
    {
      DateTime dateTime1 = start;
      DateTime dateTime2;
      if (!inCalendar && dateTime1 < DateTime.Today)
      {
        dateTime2 = DateTime.Today;
        dateTime1 = dateTime2.AddMonths(-1);
      }
      string str1 = Utils.GetCurrentUserIdInt().ToString();
      List<string> visibleCalendarIds = CacheManager.GetSubscribeCalendars().Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (calendar => !inCalendar ? calendar.Show == "show" : calendar.Show != "hidden")).Select<CalendarSubscribeProfileModel, string>((Func<CalendarSubscribeProfileModel, string>) (calendar => calendar.Id)).ToList<string>();
      visibleCalendarIds.AddRange(CacheManager.GetBindCalendars().Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (calendar => !inCalendar ? calendar.Show == "show" : calendar.Show != "hidden")).Select<BindCalendarModel, string>((Func<BindCalendarModel, string>) (calendar => calendar.Id)));
      if (dateTime1 > end)
        return new List<CalendarEventModel>();
      string str2 = dateTime1.ToString("yyyy-MM-dd HH:mm:ss");
      dateTime2 = end.AddDays(-1.0);
      string str3 = dateTime2.ToString("yyyy-MM-dd 23:59:59");
      List<CalendarEventModel> models = await App.Connection.QueryAsync<CalendarEventModel>(string.Concat(new string[15]
      {
        "select * from CalendarEventModel where userId = '",
        str1,
        "' and deleted = '0' and ((DueStart is not null and DueStart >= '",
        str2,
        "' and DueStart < '",
        str3,
        "') or (DueEnd is not null and DueEnd > '",
        str2,
        "' and DueEnd < '",
        str3,
        "')) or (DueStart is not null and DueEnd is not null and DueStart < '",
        str2,
        "' and DueEnd > '",
        str3,
        "')"
      }));
      if (models == null || !models.Any<CalendarEventModel>())
        return new List<CalendarEventModel>();
      List<CalendarEventModel> list = models.Where<CalendarEventModel>((Func<CalendarEventModel, bool>) (model =>
      {
        if (!model.DueStart.HasValue)
          return visibleCalendarIds.Contains(model.CalendarId);
        DateTime? nullable1 = model.DueStart;
        DateTime date = nullable1.Value;
        DateTime? nullable2 = model.DueEnd;
        if (model.IsAllDay)
        {
          nullable1 = model.DueStart;
          date = nullable1.Value.Date;
          nullable1 = model.DueEnd;
          ref DateTime? local = ref nullable1;
          nullable2 = local.HasValue ? new DateTime?(local.GetValueOrDefault().Date) : new DateTime?();
        }
        bool flag = models.Any<CalendarEventModel>((Func<CalendarEventModel, bool>) (cal =>
        {
          if (!cal.DueStart.HasValue || !string.IsNullOrEmpty(cal.EventId) || !string.IsNullOrEmpty(model.EventId) || !(cal.EventId != model.EventId) || !cal.EventId.StartsWith(model.EventId))
            return false;
          DateTime dateTime4 = cal.DueStart.Value;
          DateTime? dueStart = model.DueStart;
          return dueStart.HasValue && dateTime4 == dueStart.GetValueOrDefault();
        }));
        if (visibleCalendarIds.Contains(model.CalendarId))
        {
          if (!(date >= start) || !(date <= end))
          {
            if (nullable2.HasValue)
            {
              nullable1 = nullable2;
              DateTime dateTime5 = start;
              if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() > dateTime5 ? 1 : 0) : 0) != 0)
              {
                nullable1 = nullable2;
                DateTime dateTime6 = end;
                if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() <= dateTime6 ? 1 : 0) : 0) != 0)
                  goto label_10;
              }
            }
            if (nullable2.HasValue)
            {
              nullable1 = nullable2;
              DateTime dateTime7 = end;
              if ((nullable1.HasValue ? (nullable1.GetValueOrDefault() > dateTime7 ? 1 : 0) : 0) == 0 || !(date < start))
                goto label_11;
            }
            else
              goto label_11;
          }
label_10:
          return !flag;
        }
label_11:
        return false;
      })).ToList<CalendarEventModel>();
      foreach (CalendarEventModel calendarEventModel1 in list.Where<CalendarEventModel>((Func<CalendarEventModel, bool>) (calEvent => !calEvent.DueEnd.HasValue && calEvent.IsAllDay)))
      {
        DateTime? dueStart = calendarEventModel1.DueStart;
        if (dueStart.HasValue)
        {
          CalendarEventModel calendarEventModel2 = calendarEventModel1;
          dueStart = calendarEventModel1.DueStart;
          dateTime2 = dueStart.Value;
          DateTime? nullable = new DateTime?(dateTime2.AddDays(1.0));
          calendarEventModel2.DueEnd = nullable;
        }
      }
      return list;
    }

    private static async Task<(int, List<TaskBaseViewModel>)> GetRepeatEventModels(
      List<CalendarEventModel> events,
      DateTime start,
      DateTime end,
      List<string> archived,
      List<string> dates = null,
      LogicType logicType = LogicType.Or,
      bool onlyCount = false)
    {
      List<TaskBaseViewModel> list = new List<TaskBaseViewModel>();
      int count = 0;
      if (events.Any<CalendarEventModel>())
      {
        List<CalendarEventModel> skipEvents = await CalendarEventDao.GetSkipEvents();
        foreach (CalendarEventModel model in events)
        {
          if (!string.IsNullOrEmpty(model.RepeatFlag) && model.DueStart.HasValue)
          {
            double num1 = 0.0;
            if (model.DueStart.HasValue && model.DueEnd.HasValue)
              num1 = (model.DueEnd.Value - model.DueStart.Value).TotalMinutes;
            List<DateTime> repeatFlagDate = RepeatDao.GetRepeatFlagDate(new TaskBaseViewModel()
            {
              IsAllDay = new bool?(model.IsAllDay),
              StartDate = model.DueStart,
              DueDate = model.DueEnd,
              RepeatFlag = model.RepeatFlag,
              RepeatFrom = "0",
              TimeZoneName = model.TimeZone
            }, start.AddDays((double) (-2 - (int) num1 / 1440)), end.AddDays(1.0));
            string id1 = model.Id;
            int num2 = 0;
            if (!string.IsNullOrEmpty(model.ExDates))
            {
              try
              {
                model.ExDateList = JsonConvert.DeserializeObject<List<DateTime>>(model.ExDates);
              }
              catch (Exception ex)
              {
                model.ExDateList = (List<DateTime>) null;
              }
            }
            List<DateTime> source = model.ExDateList ?? new List<DateTime>();
            foreach (DateTime time in repeatFlagDate)
            {
              if (time < end)
              {
                model.DueStart = new DateTime?(time);
                DateTime dateTime1 = model.IsAllDay ? time : TimeZoneUtils.LocalToTargetTzTime(time, model.TimeZone);
                model.DueEnd = new DateTime?(model.IsAllDay ? dateTime1.AddMinutes(num1) : TimeZoneUtils.ToLocalTime(dateTime1.AddMinutes(num1), model.TimeZone));
                if (CalendarEventDao.CheckEventMatchDateRules(model, dates, logicType) && (!source.Any<DateTime>() || !source.Contains(time.Date)) && !model.IsSkipped(skipEvents, new DateTime?(time)))
                {
                  DateTime? dueStart1 = model.DueStart;
                  DateTime dateTime2 = start;
                  if ((dueStart1.HasValue ? (dueStart1.GetValueOrDefault() >= dateTime2 ? 1 : 0) : 0) != 0)
                  {
                    DateTime? dueStart2 = model.DueStart;
                    DateTime dateTime3 = end;
                    if ((dueStart2.HasValue ? (dueStart2.GetValueOrDefault() < dateTime3 ? 1 : 0) : 0) != 0)
                      goto label_19;
                  }
                  DateTime? dueEnd1 = model.DueEnd;
                  DateTime dateTime4 = start;
                  if ((dueEnd1.HasValue ? (dueEnd1.GetValueOrDefault() > dateTime4 ? 1 : 0) : 0) != 0)
                  {
                    DateTime? dueEnd2 = model.DueEnd;
                    DateTime dateTime5 = end;
                    if ((dueEnd2.HasValue ? (dueEnd2.GetValueOrDefault() <= dateTime5 ? 1 : 0) : 0) != 0)
                      goto label_19;
                  }
                  DateTime? dueEnd3 = model.DueEnd;
                  DateTime dateTime6 = end;
                  if ((dueEnd3.HasValue ? (dueEnd3.GetValueOrDefault() > dateTime6 ? 1 : 0) : 0) != 0)
                  {
                    DateTime? dueStart3 = model.DueStart;
                    DateTime dateTime7 = start;
                    if ((dueStart3.HasValue ? (dueStart3.GetValueOrDefault() < dateTime7 ? 1 : 0) : 0) == 0)
                      continue;
                  }
                  else
                    continue;
label_19:
                  string id2 = id1 + "_time_" + num2++.ToString();
                  if (!archived.Any<string>() || !archived.Contains(ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(model))))
                  {
                    if (!onlyCount)
                      list.Add(new TaskBaseViewModel(model, id2));
                    ++count;
                  }
                }
              }
            }
          }
        }
      }
      (int, List<TaskBaseViewModel>) repeatEventModels = (count, list);
      list = (List<TaskBaseViewModel>) null;
      return repeatEventModels;
    }

    public static async Task<List<CalendarEventModel>> GetRepeatEvents()
    {
      return await App.Connection.QueryAsync<CalendarEventModel>("select * from calendarEventModel where repeatFlag is not null and repeatFlag !='' and deleted = '0' and userId ='" + Utils.GetCurrentUserIdInt().ToString() + "'");
    }

    public static async Task<(int, List<TaskBaseViewModel>)> GetTaskDisplayModelsBetweenSpan(
      DateTime start,
      DateTime end,
      bool inCal = false,
      List<string> dates = null,
      string keyWord = null,
      LogicType logicType = LogicType.Or,
      bool onlyCount = false)
    {
      List<CalendarEventModel> events = await CalendarEventDao.GetEventsBetweenSpan(start, end, inCal);
      foreach (CalendarEventModel calendarEventModel1 in events.Where<CalendarEventModel>((Func<CalendarEventModel, bool>) (calEvent => calEvent.IsAllDay && !calEvent.DueEnd.HasValue)))
      {
        DateTime? dueStart = calendarEventModel1.DueStart;
        if (dueStart.HasValue)
        {
          CalendarEventModel calendarEventModel2 = calendarEventModel1;
          dueStart = calendarEventModel1.DueStart;
          DateTime? nullable = new DateTime?(dueStart.Value.AddDays(1.0));
          calendarEventModel2.DueEnd = nullable;
        }
      }
      List<string> archived = await ArchivedDao.GetArchivedKeys();
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<string> subCalIds = CacheManager.GetSubscribeCalendars().Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => (cal.Show == "show" || inCal && cal.Show != "hidden") && cal.UserId == userId)).ToList<CalendarSubscribeProfileModel>().Select<CalendarSubscribeProfileModel, string>((Func<CalendarSubscribeProfileModel, string>) (cal => cal.Id)).ToList<string>();
      List<string> accounts = CacheManager.GetBindCalendarAccounts().Select<BindCalendarAccountModel, string>((Func<BindCalendarAccountModel, string>) (cal => cal.Id)).ToList<string>();
      subCalIds.AddRange((IEnumerable<string>) CacheManager.GetBindCalendars().Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => accounts.Contains(cal.AccountId) && (cal.Show == "show" || inCal && cal.Show != "hidden") && cal.UserId == userId)).Select<BindCalendarModel, string>((Func<BindCalendarModel, string>) (cal => cal.Id)).ToList<string>());
      keyWord = keyWord?.Trim();
      List<string> keys = (List<string>) null;
      if (!string.IsNullOrEmpty(keyWord))
      {
        keys = ((IEnumerable<string>) keyWord.Split(' ')).ToList<string>();
        keys.RemoveAll((Predicate<string>) (k => k == ""));
      }
      (int num1, List<TaskBaseViewModel> collection) = await CalendarEventDao.GetRepeatEventModels((await CalendarEventDao.GetRepeatEvents()).Where<CalendarEventModel>((Func<CalendarEventModel, bool>) (calEvent =>
      {
        if (!subCalIds.Contains(calEvent.CalendarId))
          return false;
        return keys == null || SearchHelper.KeyWordMatched(calEvent.Title + "\r\n" + calEvent.Content, keyWord, keys);
      })).ToList<CalendarEventModel>(), start, end, archived, dates, logicType, onlyCount);
      List<TaskBaseViewModel> models = new List<TaskBaseViewModel>();
      int num = 0;
      if (events.Any<CalendarEventModel>())
      {
        List<CalendarEventModel> skipEvents = await CalendarEventDao.GetSkipEvents();
        foreach (CalendarEventModel model in events)
        {
          if (model.DueStart.HasValue && (keys == null || SearchHelper.KeyWordMatched(model.Title + "\r\n" + model.Content, keyWord, keys)) && CalendarEventDao.CheckEventMatchDateRules(model, dates, logicType) && !model.IsSkipped(skipEvents) && !archived.Contains(ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(model))))
          {
            if (!onlyCount)
              models.Add(new TaskBaseViewModel(model));
            ++num;
          }
        }
      }
      models.AddRange((IEnumerable<TaskBaseViewModel>) collection);
      num += num1;
      (int, List<TaskBaseViewModel>) modelsBetweenSpan = (num, models);
      events = (List<CalendarEventModel>) null;
      archived = (List<string>) null;
      collection = (List<TaskBaseViewModel>) null;
      models = (List<TaskBaseViewModel>) null;
      return modelsBetweenSpan;
    }

    public static bool CheckEventMatchDateRules(
      CalendarEventModel model,
      List<string> dates,
      LogicType logicType)
    {
      if (dates == null || dates.Count == 0)
        return true;
      bool flag = false;
      foreach (string date in dates)
      {
        switch (date)
        {
          case "all":
            flag = true;
            goto label_54;
          case "recurring":
            if (!string.IsNullOrEmpty(model.RepeatFlag))
            {
              flag = true;
              goto label_54;
            }
            else
              break;
        }
        DateTime dateTime1 = DateTime.Today;
        DateTime dateTime2 = DateTime.Today;
        DateTime dateTime3 = dateTime2.AddDays(181.0);
        if (date != null)
        {
          switch (date.Length)
          {
            case 5:
              switch (date[0])
              {
                case 'n':
                  if (date == "nodue")
                    continue;
                  break;
                case 't':
                  if (date == "today")
                  {
                    dateTime2 = DateTime.Today;
                    dateTime3 = dateTime2.AddDays(1.0);
                    goto label_40;
                  }
                  else
                    break;
              }
              break;
            case 7:
              if (date == "overdue")
                continue;
              break;
            case 8:
              switch (date[1])
              {
                case 'e':
                  if (date == "nextweek")
                  {
                    int nextWeekDayDiff = DateUtils.GetNextWeekDayDiff();
                    dateTime2 = DateTime.Today;
                    dateTime1 = dateTime2.AddDays((double) nextWeekDayDiff);
                    dateTime2 = DateTime.Today;
                    dateTime3 = dateTime2.AddDays((double) (nextWeekDayDiff + 7));
                    goto label_40;
                  }
                  else
                    break;
                case 'h':
                  if (date == "thisweek")
                  {
                    int nextWeekDayDiff = DateUtils.GetNextWeekDayDiff();
                    dateTime2 = DateTime.Today;
                    dateTime3 = dateTime2.AddDays((double) nextWeekDayDiff);
                    goto label_40;
                  }
                  else
                    break;
                case 'o':
                  if (date == "tomorrow")
                  {
                    dateTime2 = DateTime.Today;
                    dateTime1 = dateTime2.AddDays(1.0);
                    dateTime2 = DateTime.Today;
                    dateTime3 = dateTime2.AddDays(2.0);
                    goto label_40;
                  }
                  else
                    break;
              }
              break;
            case 9:
              if (date == "thismonth")
              {
                dateTime2 = DateTime.Today;
                dateTime3 = dateTime2.AddDays((double) ((DateTime.Now.Day - 1) * -1)).AddMonths(1);
                goto label_40;
              }
              else
                break;
          }
        }
        if (date.EndsWith("days"))
        {
          int result;
          int.TryParse(date.Replace("days", string.Empty), out result);
          dateTime2 = DateTime.Today;
          dateTime3 = dateTime2.AddDays((double) result);
        }
        else if (date.EndsWith("dayslater"))
        {
          int result;
          int.TryParse(date.Replace("dayslater", string.Empty), out result);
          dateTime2 = DateTime.Today;
          dateTime1 = dateTime2.AddDays((double) result);
        }
        else if (date.EndsWith("daysfromtoday"))
        {
          int result;
          int.TryParse(date.Replace("daysfromtoday", string.Empty), out result);
          dateTime2 = DateTime.Today;
          dateTime1 = dateTime2.AddDays((double) result);
          dateTime3 = dateTime1.AddDays(1.0);
        }
        else if (date.StartsWith("span"))
        {
          (int? nullable1, int? nullable2) = FilterUtils.GetSpanPairInRule(date);
          DateTime dateTime4;
          if (nullable1.HasValue)
          {
            dateTime2 = DateTime.Today;
            dateTime4 = dateTime2.AddDays((double) nullable1.Value);
          }
          else
            dateTime4 = dateTime1;
          dateTime1 = dateTime4;
          DateTime dateTime5;
          if (nullable2.HasValue)
          {
            dateTime2 = DateTime.Today;
            dateTime5 = dateTime2.AddDays((double) (nullable2.Value + 1));
          }
          else
            dateTime5 = dateTime3;
          dateTime3 = dateTime5;
        }
        else if (date.StartsWith("offset"))
        {
          (DateTime dateTime6, DateTime dateTime7) = FilterUtils.GetDateOffsetPair(date);
          dateTime1 = dateTime6;
          dateTime3 = dateTime7;
        }
        else
          continue;
label_40:
        if (model.DueEnd.HasValue)
        {
          DateTime? dueStart1 = model.DueStart;
          dateTime2 = dateTime1;
          if ((dueStart1.HasValue ? (dueStart1.GetValueOrDefault() >= dateTime2 ? 1 : 0) : 0) != 0)
          {
            DateTime? dueStart2 = model.DueStart;
            dateTime2 = dateTime3;
            if ((dueStart2.HasValue ? (dueStart2.GetValueOrDefault() < dateTime2 ? 1 : 0) : 0) != 0)
              goto label_47;
          }
          DateTime? dueEnd1 = model.DueEnd;
          dateTime2 = dateTime1;
          if ((dueEnd1.HasValue ? (dueEnd1.GetValueOrDefault() > dateTime2 ? 1 : 0) : 0) != 0)
          {
            DateTime? dueEnd2 = model.DueEnd;
            dateTime2 = dateTime3;
            if ((dueEnd2.HasValue ? (dueEnd2.GetValueOrDefault() <= dateTime2 ? 1 : 0) : 0) != 0)
              goto label_47;
          }
          DateTime? dueStart3 = model.DueStart;
          dateTime2 = dateTime1;
          if ((dueStart3.HasValue ? (dueStart3.GetValueOrDefault() < dateTime2 ? 1 : 0) : 0) != 0)
          {
            DateTime? dueEnd3 = model.DueEnd;
            dateTime2 = dateTime3;
            if ((dueEnd3.HasValue ? (dueEnd3.GetValueOrDefault() > dateTime2 ? 1 : 0) : 0) == 0)
              goto label_48;
          }
          else
            goto label_48;
label_47:
          flag = true;
          break;
        }
label_48:
        if (!model.DueEnd.HasValue)
        {
          DateTime? dueStart4 = model.DueStart;
          dateTime2 = dateTime1;
          if ((dueStart4.HasValue ? (dueStart4.GetValueOrDefault() >= dateTime2 ? 1 : 0) : 0) != 0)
          {
            DateTime? dueStart5 = model.DueStart;
            dateTime2 = dateTime3;
            if ((dueStart5.HasValue ? (dueStart5.GetValueOrDefault() < dateTime2 ? 1 : 0) : 0) != 0)
            {
              flag = true;
              break;
            }
          }
        }
      }
label_54:
      return logicType != LogicType.Or ? !flag : flag;
    }

    public static async Task DeleteBindEvents()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<CalendarEventModel> listAsync = await App.Connection.Table<CalendarEventModel>().Where((Expression<Func<CalendarEventModel, bool>>) (model => model.UserId == userId && model.Type == 0)).ToListAsync();
      if (!listAsync.Any<CalendarEventModel>())
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task MergeCalendarEvents(
      string calendarId,
      IEnumerable<CalendarEventModel> events,
      DateTime start,
      DateTime end)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<CalendarEventModel> listAsync = await App.Connection.Table<CalendarEventModel>().Where((Expression<Func<CalendarEventModel, bool>>) (model => model.UserId == userId && model.CalendarId == calendarId)).ToListAsync();
      BindCalendarAccountModel account = CacheManager.GetAccountCalById(CacheManager.GetBindCalendarById(calendarId)?.AccountId);
      Dictionary<string, CalendarEventModel> localEventsDict = new Dictionary<string, CalendarEventModel>();
      List<CalendarEventModel> localRepeatEventList = new List<CalendarEventModel>();
      foreach (CalendarEventModel calendarEventModel in listAsync)
      {
        if (!string.IsNullOrEmpty(calendarEventModel.Id))
        {
          if (!localEventsDict.ContainsKey(calendarEventModel.Id))
          {
            if (!string.IsNullOrEmpty(calendarEventModel.RepeatFlag) && account?.Site == "google" && RepeatUtils.GetValidRepeatDates(calendarEventModel.RepeatFlag, "2", start, start.Date, end.Date, timeZone: calendarEventModel.TimeZone, toLocalTime: !calendarEventModel.IsAllDay).Any<DateTime>())
              localRepeatEventList.Add(calendarEventModel);
            localEventsDict.Add(calendarEventModel.Id, calendarEventModel);
          }
          else
          {
            int num = await App.Connection.DeleteAsync((object) calendarEventModel);
          }
        }
      }
      List<CalendarEventModel> added = new List<CalendarEventModel>();
      List<CalendarEventModel> updated = new List<CalendarEventModel>();
      foreach (CalendarEventModel calendarEventModel1 in events)
      {
        CalendarEventModel calEvent = calendarEventModel1;
        calEvent.EventId = calEvent.Id;
        calEvent.CalendarId = calendarId;
        calEvent.Id = calEvent.Id + "@" + calendarId;
        calEvent.UserId = userId;
        calEvent.Type = 0;
        if (calEvent.Attendees != null && calEvent.Attendees.Any<CalendarAttendeeModel>())
          calEvent.AttendeeInfo = JsonConvert.SerializeObject((object) calEvent.Attendees);
        if (calEvent.ReminderList != null && calEvent.ReminderList.Any<int>())
          calEvent.Reminders = JsonConvert.SerializeObject((object) calEvent.ReminderList);
        if (calEvent.IsAllDay)
        {
          DateTime? nullable1 = calEvent.DueStart;
          if (nullable1.HasValue)
          {
            CalendarEventModel calendarEventModel2 = calEvent;
            nullable1 = calEvent.DueStart;
            DateTime? nullable2 = new DateTime?(nullable1.Value.ToUniversalTime());
            calendarEventModel2.DueStart = nullable2;
          }
          nullable1 = calEvent.DueEnd;
          if (nullable1.HasValue)
          {
            CalendarEventModel calendarEventModel3 = calEvent;
            nullable1 = calEvent.DueEnd;
            DateTime? nullable3 = new DateTime?(nullable1.Value.ToUniversalTime());
            calendarEventModel3.DueEnd = nullable3;
          }
        }
        if (calEvent.ExDateList != null && calEvent.ExDateList.Any<DateTime>())
          calEvent.ExDates = JsonConvert.SerializeObject((object) calEvent.ExDateList.Select<DateTime, DateTime>((Func<DateTime, DateTime>) (date => TimeZoneUtils.LocalToTargetTzTime(date, calEvent.TimeZone).Date)));
        localRepeatEventList.RemoveAll((Predicate<CalendarEventModel>) (cal => cal.Id == calEvent.Id));
        if (localEventsDict.ContainsKey(calEvent.Id))
        {
          CalendarEventModel calendarEventModel4 = localEventsDict[calEvent.Id];
          if (calendarEventModel4 != null)
          {
            calEvent._Id = calendarEventModel4._Id;
            updated.Add(calEvent);
          }
          localEventsDict.Remove(calEvent.Id);
        }
        else
          added.Add(calEvent);
      }
      if (added.Any<CalendarEventModel>())
      {
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) added);
      }
      if (updated.Any<CalendarEventModel>())
      {
        int num2 = await App.Connection.UpdateAllAsync((IEnumerable) updated);
      }
      bool needDelete = false;
      if (localEventsDict.Any<KeyValuePair<string, CalendarEventModel>>())
      {
        foreach (CalendarEventModel calendarEventModel in localEventsDict.Values)
        {
          DateTime? nullable = calendarEventModel.DueStart;
          if (nullable.HasValue)
          {
            nullable = calendarEventModel.DueEnd;
            if (nullable.HasValue)
            {
              nullable = calendarEventModel.DueStart;
              if (!(nullable.Value <= start))
              {
                nullable = calendarEventModel.DueStart;
                if (nullable.Value >= end)
                  continue;
              }
              else
                continue;
            }
          }
          needDelete = true;
          int num3 = await App.Connection.DeleteAsync((object) calendarEventModel);
        }
      }
      foreach (CalendarEventModel calendarEventModel in localRepeatEventList)
      {
        needDelete = true;
        int num4 = await App.Connection.DeleteAsync((object) calendarEventModel);
      }
      if (((added.Any<CalendarEventModel>() ? 1 : (updated.Any<CalendarEventModel>() ? 1 : 0)) | (needDelete ? 1 : 0)) == 0)
      {
        account = (BindCalendarAccountModel) null;
        localEventsDict = (Dictionary<string, CalendarEventModel>) null;
        localRepeatEventList = (List<CalendarEventModel>) null;
        added = (List<CalendarEventModel>) null;
        updated = (List<CalendarEventModel>) null;
      }
      else
      {
        CalendarEventDao._skipEvents = (List<CalendarEventModel>) null;
        CalendarEventChangeNotifier.NotifyRemoteChanged();
        account = (BindCalendarAccountModel) null;
        localEventsDict = (Dictionary<string, CalendarEventModel>) null;
        localRepeatEventList = (List<CalendarEventModel>) null;
        added = (List<CalendarEventModel>) null;
        updated = (List<CalendarEventModel>) null;
      }
    }

    public static async Task MergeCalendarEvents(
      List<BindCalendarModel> calendars,
      DateTime start,
      DateTime end)
    {
      if (calendars == null || !calendars.Any<BindCalendarModel>())
        return;
      foreach (BindCalendarModel calendar in calendars)
        await CalendarEventDao.MergeCalendarEvents(calendar.Id, (IEnumerable<CalendarEventModel>) calendar.Events, start, end);
    }

    public static async Task DeleteByCalendarId(string calendarId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<CalendarEventModel> listAsync = await App.Connection.Table<CalendarEventModel>().Where((Expression<Func<CalendarEventModel, bool>>) (model => model.UserId == userId && model.CalendarId == calendarId)).ToListAsync();
      if (!listAsync.Any<CalendarEventModel>())
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task HandleTimeZoneChanged(TimeZoneInfo newTz, TimeZoneInfo oldTz)
    {
      List<CalendarEventModel> listAsync = await App.Connection.Table<CalendarEventModel>().ToListAsync();
      List<CalendarEventModel> items = new List<CalendarEventModel>();
      foreach (CalendarEventModel calendarEventModel1 in listAsync)
      {
        if (calendarEventModel1 != null && !calendarEventModel1.IsAllDay)
        {
          DateTime? nullable1 = calendarEventModel1.DueStart;
          if (nullable1.HasValue)
          {
            CalendarEventModel calendarEventModel2 = calendarEventModel1;
            nullable1 = calendarEventModel1.DueStart;
            DateTime? nullable2 = new DateTime?(TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(nullable1.Value, DateTimeKind.Unspecified), oldTz, newTz));
            calendarEventModel2.DueStart = nullable2;
            nullable1 = calendarEventModel1.DueEnd;
            if (nullable1.HasValue)
            {
              CalendarEventModel calendarEventModel3 = calendarEventModel1;
              nullable1 = calendarEventModel1.DueEnd;
              DateTime? nullable3 = new DateTime?(TimeZoneInfo.ConvertTime(DateTime.SpecifyKind(nullable1.Value, DateTimeKind.Unspecified), oldTz, newTz));
              calendarEventModel3.DueEnd = nullable3;
            }
            items.Add(calendarEventModel1);
          }
        }
      }
      if (items.Count <= 0)
        return;
      int num = await App.Connection.UpdateAllAsync((IEnumerable) items);
    }

    public static BindCalendarAccountModel GetCalendarAccount(string calendarId)
    {
      BindCalendarModel bindCalendarById = CacheManager.GetBindCalendarById(calendarId);
      return bindCalendarById != null ? CacheManager.GetAccountCalById(bindCalendarById.AccountId) : (BindCalendarAccountModel) null;
    }

    public static async Task<List<CalendarEventModel>> GetAllShowEvents()
    {
      return await App.Connection.QueryAsync<CalendarEventModel>("select * from CalendarEventModel where (CalendarId in (select Id as CalendarId from BindCalendarModel where Show = 'show') or  CalendarId in (select Id as CalendarId from CalendarSubscribeProfileModel where Show = 'show')) and UserId = '" + LocalSettings.Settings.LoginUserId + "' and Deleted = 0 ");
    }

    public static DateTime? GetEventNextRepeat(
      CalendarEventModel model,
      List<CalendarEventModel> skipEvents,
      List<string> hiddenKeys)
    {
      if (!model.DueStart.HasValue)
        return new DateTime?();
      if (!string.IsNullOrEmpty(model.ExDates))
      {
        try
        {
          model.ExDateList = JsonConvert.DeserializeObject<List<DateTime>>(model.ExDates);
        }
        catch (Exception ex)
        {
          model.ExDateList = (List<DateTime>) null;
        }
      }
      double num1 = 0.0;
      DateTime? nullable;
      DateTime targetTzTime;
      if (!model.IsAllDay)
      {
        targetTzTime = TimeZoneUtils.LocalToTargetTzTime(model.DueStart.Value, model.TimeZone);
      }
      else
      {
        nullable = model.DueStart;
        targetTzTime = nullable.Value;
      }
      DateTime start1 = targetTzTime;
      nullable = model.DueEnd;
      if (nullable.HasValue)
      {
        nullable = model.DueEnd;
        DateTime dateTime1 = nullable.Value;
        nullable = model.DueStart;
        DateTime dateTime2 = nullable.Value;
        num1 = (dateTime1 - dateTime2).TotalMinutes;
      }
      DateTime eventNextRepeatDate = RepeatDao.GetEventNextRepeatDate(model.RepeatFlag, start1, model.TimeZone);
      int num2 = 20;
      while (num2 > 0)
      {
        --num2;
        model.DueStart = new DateTime?(eventNextRepeatDate);
        DateTime start2 = model.IsAllDay ? eventNextRepeatDate : TimeZoneUtils.LocalToTargetTzTime(eventNextRepeatDate, model.TimeZone);
        nullable = model.DueEnd;
        if (nullable.HasValue)
          model.DueEnd = new DateTime?(model.IsAllDay ? start2.AddMinutes(num1) : eventNextRepeatDate.AddMinutes(num1));
        bool flag1 = false;
        bool flag2 = false;
        if (model.ExDateList != null && model.ExDateList.Any<DateTime>() && model.ExDateList.Contains(start2.Date))
          flag1 = true;
        if (skipEvents.Any<CalendarEventModel>())
          flag2 = model.IsSkipped(skipEvents, new DateTime?(eventNextRepeatDate));
        if (!flag1 && !flag2)
        {
          string eventKey = ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(model));
          if (!hiddenKeys.Contains(eventKey))
            return new DateTime?(eventNextRepeatDate);
        }
        eventNextRepeatDate = RepeatDao.GetEventNextRepeatDate(model.RepeatFlag, start2, model.TimeZone);
      }
      return new DateTime?(eventNextRepeatDate);
    }
  }
}
