// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.CalendarService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Config;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service
{
  public static class CalendarService
  {
    public static bool IsPulling;
    private static bool _isPullingEvents;
    private static BlockingSet<string> _loged = new BlockingSet<string>();

    public static async Task PullAccountEvents(string accountId, string kind, string site)
    {
      if (CalendarService._isPullingEvents && UserDao.IsUserValid())
        return;
      CalendarService._isPullingEvents = true;
      try
      {
        switch (kind?.ToLower())
        {
          case "caldav":
          case "icloud":
            await CalendarService.PullBindCalEvents(accountId, new DateTime?(), new DateTime?(), Constants.BindAccountType.CalDAV);
            CalendarService._isPullingEvents = false;
            return;
          case "exchange":
            await CalendarService.PullBindCalEvents(accountId, new DateTime?(), new DateTime?(), Constants.BindAccountType.Exchange);
            CalendarService._isPullingEvents = false;
            return;
          default:
            DateTime dateTime = DateTime.Now;
            dateTime = dateTime.Date;
            ref DateTime local1 = ref dateTime;
            DateTime now = DateTime.Now;
            double num1 = (double) (1 - now.Day);
            DateTime pullStart = local1.AddDays(num1).AddMonths(-1);
            DateTime date = DateTime.Now.Date;
            ref DateTime local2 = ref date;
            now = DateTime.Now;
            double num2 = (double) (1 - now.Day);
            DateTime pullEnd = local2.AddDays(num2).AddMonths(5);
            List<BindCalendarModel> source;
            if (site != null && site.ToLower() == "outlook")
            {
              OutlookCalendarModels outlookCalendarModels = await CalendarService.PullOutlookCalendars(new DateTime?(), new DateTime?());
              if (outlookCalendarModels == null)
              {
                CalendarService._isPullingEvents = false;
                return;
              }
              if (outlookCalendarModels.errorIds.Contains(accountId))
                return;
              source = outlookCalendarModels.events;
              pullStart = outlookCalendarModels.begin;
              pullEnd = outlookCalendarModels.end;
            }
            else
              source = await Communicator.GetBindCalendarEvents(accountId);
            if (source != null && source.Any<BindCalendarModel>())
            {
              foreach (BindCalendarModel calendar in source)
              {
                await BindCalendarAccountDao.SaveCalendarInfo(calendar);
                await CalendarEventDao.MergeCalendarEvents(calendar.Id, (IEnumerable<CalendarEventModel>) calendar.Events, pullStart, pullEnd);
              }
            }
            await CalendarService.HandleCalendarUnExpired(accountId);
            break;
        }
      }
      catch (CustomException.CalendarExpiredException ex)
      {
        await CalendarService.HandleCalendarExpired(accountId);
      }
      if (ABTestManager.IsNewRemindCalculate())
        EventReminderCalculator.InitAllEventsReminderTimes();
      else
        ReminderCalculator.AssembleReminders();
      CalendarService._isPullingEvents = false;
    }

    private static async Task HandleCalendarExpired(string accountId)
    {
      BindCalendarAccountModel accountCalById = CacheManager.GetAccountCalById(accountId);
      if (accountCalById == null || accountCalById.Expired)
        return;
      await BindCalendarAccountDao.HandleAccountExpired(accountCalById);
      DataChangedNotifier.NotifyCalendarChanged();
    }

    private static async Task HandleCalendarUnExpired(string accountId)
    {
      BindCalendarAccountModel accountCalById = CacheManager.GetAccountCalById(accountId);
      if (accountCalById == null || !accountCalById.Expired)
        return;
      accountCalById.Expired = false;
      await BindCalendarAccountDao.SaveBindCalendarAccount(accountCalById, false);
      DataChangedNotifier.NotifyCalendarChanged();
    }

    public static async Task PullAccountCalendarsAndEvents(
      DateTime? start = null,
      DateTime? end = null,
      string accountId = "")
    {
      List<BindCalendarAccountModel> accounts;
      if (!UserDao.IsUserValid())
      {
        CalendarService.IsPulling = false;
        accounts = (List<BindCalendarAccountModel>) null;
      }
      else if (CalendarService.IsPulling && string.IsNullOrEmpty(accountId))
      {
        CalendarService.IsPulling = false;
        accounts = (List<BindCalendarAccountModel>) null;
      }
      else
      {
        CalendarService.IsPulling = true;
        accounts = await Communicator.GetBindCalendarAccounts(start, end);
        if (accounts != null)
        {
          if (accounts.Any<BindCalendarAccountModel>())
          {
            await BindCalendarAccountDao.SaveBindCalendarAccounts(accounts);
            foreach (BindCalendarAccountModel calendarAccountModel in accounts)
            {
              if (string.IsNullOrEmpty(accountId) || !(calendarAccountModel.Id != accountId))
              {
                switch (calendarAccountModel.Kind)
                {
                  case "caldav":
                  case "icloud":
                    await CalendarService.PullBindCalEvents(calendarAccountModel.Id, start, end, Constants.BindAccountType.CalDAV);
                    continue;
                  case "exchange":
                    await CalendarService.PullBindCalEvents(calendarAccountModel.Id, start, end, Constants.BindAccountType.Exchange);
                    continue;
                  default:
                    continue;
                }
              }
            }
            if (!string.IsNullOrEmpty(accountId))
            {
              accounts = (List<BindCalendarAccountModel>) null;
              return;
            }
            await CalendarService.PullOutlookEvents(start, end);
            BindCalendarsCollection bindCalendarEvents;
            if (start.HasValue && end.HasValue)
              bindCalendarEvents = await Communicator.GetBindCalendarEvents(start.Value, end.Value);
            else
              bindCalendarEvents = await Communicator.GetBindCalendarEvents();
            BindCalendarsCollection calendarEvents = bindCalendarEvents;
            if (calendarEvents?.ErrorIds != null && calendarEvents.ErrorIds.Any<string>())
            {
              foreach (string errorId in calendarEvents.ErrorIds)
                CalendarService.HandleCalendarExpired(errorId);
            }
            if (calendarEvents?.Events != null && calendarEvents.Events.Any<BindCalendarModel>())
            {
              DateTime? nullable = start;
              DateTime dateTime1;
              DateTime now;
              DateTime dateTime2;
              if (!nullable.HasValue)
              {
                dateTime1 = DateTime.Now;
                dateTime1 = dateTime1.Date;
                ref DateTime local = ref dateTime1;
                now = DateTime.Now;
                double num = (double) (1 - now.Day);
                dateTime1 = local.AddDays(num);
                dateTime2 = dateTime1.AddMonths(-1);
              }
              else
                dateTime2 = nullable.GetValueOrDefault();
              DateTime start1 = dateTime2;
              nullable = end;
              DateTime dateTime3;
              if (!nullable.HasValue)
              {
                dateTime1 = DateTime.Now;
                dateTime1 = dateTime1.Date;
                ref DateTime local = ref dateTime1;
                now = DateTime.Now;
                double num = (double) (1 - now.Day);
                dateTime1 = local.AddDays(num);
                dateTime3 = dateTime1.AddMonths(5);
              }
              else
                dateTime3 = nullable.GetValueOrDefault();
              DateTime end1 = dateTime3;
              await CalendarEventDao.MergeCalendarEvents(calendarEvents.Events, start1, end1);
              calendarEvents.Events.DistinctEx<BindCalendarModel, string>((Func<BindCalendarModel, string>) (e => e.AccountId)).Select<BindCalendarModel, string>((Func<BindCalendarModel, string>) (e => e.AccountId)).ToList<string>().ForEach((Action<string>) (id => CalendarService.HandleCalendarUnExpired(id)));
            }
            calendarEvents = (BindCalendarsCollection) null;
          }
          else
            await BindCalendarAccountDao.DeleteBindAccounts();
        }
        ThreadUtil.DetachedRunOnUiBackThread((Action) (() =>
        {
          if (ABTestManager.IsNewRemindCalculate())
            EventReminderCalculator.InitAllEventsReminderTimes();
          else
            ReminderCalculator.AssembleReminders();
        }));
        CalendarService.IsPulling = false;
        accounts = (List<BindCalendarAccountModel>) null;
      }
    }

    public static async Task PullBindCalEvents(
      string id,
      DateTime? pullStart,
      DateTime? pullEnd,
      Constants.BindAccountType type)
    {
      try
      {
        List<BindCalendarModel> source = (List<BindCalendarModel>) null;
        switch (type)
        {
          case Constants.BindAccountType.CalDAV:
            source = await Communicator.GetBindCalDavEvents(id, pullStart, pullEnd);
            break;
          case Constants.BindAccountType.Exchange:
            source = await Communicator.GetExchangeEvents(id, pullStart, pullEnd);
            break;
        }
        DateTime? nullable = pullStart;
        DateTime dateTime1;
        DateTime now;
        DateTime dateTime2;
        if (!nullable.HasValue)
        {
          dateTime1 = DateTime.Now;
          dateTime1 = dateTime1.Date;
          ref DateTime local = ref dateTime1;
          now = DateTime.Now;
          double num = (double) (1 - now.Day);
          dateTime1 = local.AddDays(num);
          dateTime2 = dateTime1.AddMonths(-1);
        }
        else
          dateTime2 = nullable.GetValueOrDefault();
        pullStart = new DateTime?(dateTime2);
        nullable = pullEnd;
        DateTime dateTime3;
        if (!nullable.HasValue)
        {
          dateTime1 = DateTime.Now;
          dateTime1 = dateTime1.Date;
          ref DateTime local = ref dateTime1;
          now = DateTime.Now;
          double num = (double) (1 - now.Day);
          dateTime1 = local.AddDays(num);
          dateTime3 = dateTime1.AddMonths(5);
        }
        else
          dateTime3 = nullable.GetValueOrDefault();
        pullEnd = new DateTime?(dateTime3);
        if (source != null && source.Any<BindCalendarModel>())
        {
          foreach (BindCalendarModel calendar in source)
          {
            calendar.AccountId = id;
            await BindCalendarAccountDao.SaveCalendarInfo(calendar);
            await CalendarEventDao.MergeCalendarEvents(calendar.Id, (IEnumerable<CalendarEventModel>) calendar.Events, pullStart.Value, pullEnd.Value);
          }
        }
        BindCalendarAccountModel accountCalById = CacheManager.GetAccountCalById(id);
        if (accountCalById == null || !accountCalById.Expired)
          return;
        accountCalById.Expired = false;
        await BindCalendarAccountDao.SaveBindCalendarAccount(accountCalById, false);
      }
      catch
      {
        CalendarService.HandleCalendarExpired(id);
      }
    }

    private static async Task<OutlookCalendarModels> PullOutlookCalendars(
      DateTime? pullStart,
      DateTime? pullEnd)
    {
      OutlookCalendarModels bindOutlookEvents = await Communicator.GetBindOutlookEvents(pullStart, pullEnd);
      if (bindOutlookEvents != null)
      {
        int? count = bindOutlookEvents.errorIds?.Count;
        int num = 0;
        if (count.GetValueOrDefault() > num & count.HasValue)
        {
          foreach (string errorId in bindOutlookEvents.errorIds)
            CalendarService.HandleCalendarExpired(errorId);
        }
      }
      if (bindOutlookEvents != null && bindOutlookEvents.events.Any<BindCalendarModel>())
        bindOutlookEvents.events.DistinctEx<BindCalendarModel, string>((Func<BindCalendarModel, string>) (e => e.AccountId)).Select<BindCalendarModel, string>((Func<BindCalendarModel, string>) (e => e.AccountId)).ToList<string>().ForEach((Action<string>) (id => CalendarService.HandleCalendarUnExpired(id)));
      return bindOutlookEvents;
    }

    private static async Task PullOutlookEvents(DateTime? pullStart, DateTime? pullEnd)
    {
      OutlookCalendarModels outlook = await CalendarService.PullOutlookCalendars(pullStart, pullEnd);
      List<BindCalendarModel> events = outlook?.events;
      if (events == null)
        outlook = (OutlookCalendarModels) null;
      else if (!events.Any<BindCalendarModel>())
      {
        outlook = (OutlookCalendarModels) null;
      }
      else
      {
        foreach (BindCalendarModel calendar in events)
        {
          await BindCalendarAccountDao.SaveCalendarInfo(calendar);
          await CalendarEventDao.MergeCalendarEvents(calendar.Id, (IEnumerable<CalendarEventModel>) calendar.Events, outlook.begin, outlook.end);
        }
        outlook = (OutlookCalendarModels) null;
      }
    }

    public static async Task PullSubscribeCalendars()
    {
      List<CalendarSubscribeProfileModel> calendarSubscriptions = await Communicator.GetCalendarSubscriptions();
      if (calendarSubscriptions == null)
        ;
      else
      {
        string userId = Utils.GetCurrentUserIdInt().ToString();
        calendarSubscriptions.ForEach((Action<CalendarSubscribeProfileModel>) (sub => sub.UserId = userId));
        await CalendarSubscribeProfileDao.SaveProfiles(calendarSubscriptions);
      }
    }

    public static async Task NotifyTaskDrop(
      TimeDataModel original,
      TaskCellViewModel model,
      TimeDataModel merged)
    {
      if (merged != null)
        await TaskService.SetDateTime(merged);
      if (original.StartDate.HasValue)
      {
        bool? isAllDay1 = original.IsAllDay;
        bool? isAllDay2 = (bool?) merged?.IsAllDay;
        if (isAllDay1.GetValueOrDefault() == isAllDay2.GetValueOrDefault() & isAllDay1.HasValue == isAllDay2.HasValue)
          return;
      }
      TaskModel task = new TaskModel();
      task.id = model.TaskId;
      TimeDataModel timeDataModel = merged;
      bool? isAllDay;
      int num;
      if (timeDataModel == null)
      {
        num = 1;
      }
      else
      {
        isAllDay = timeDataModel.IsAllDay;
        num = !isAllDay.HasValue ? 1 : 0;
      }
      TaskReminderModel[] array;
      if (num == 0)
      {
        isAllDay = merged.IsAllDay;
        if (!isAllDay.Value)
        {
          array = TimeData.GetDefaultTimeReminders().ToArray();
          goto label_12;
        }
      }
      array = TimeData.GetDefaultAllDayReminders().ToArray();
label_12:
      task.reminders = array;
      await TaskService.SaveTaskReminders(task);
    }

    public static async Task NotifyCheckItemDrop(TimeDataModel date, TaskCellViewModel model)
    {
      TimeDataModel timeDataModel = date;
      if ((timeDataModel != null ? (!timeDataModel.StartDate.HasValue ? 1 : 0) : 1) != 0)
        return;
      await TaskService.SetCheckItemDate(model.GetTaskId(), model.ItemId, date);
    }

    private static CalendarService.MoveType GetMoveType(TimeDataModel original, TimeDataModel delta)
    {
      if (!original.StartDate.HasValue)
        return CalendarService.MoveType.Arrange;
      bool? isAllDay = original.IsAllDay;
      bool flag1 = ((int) isAllDay ?? 1) != 0;
      isAllDay = delta.IsAllDay;
      bool flag2 = ((int) isAllDay ?? 1) != 0;
      if (original.StartDate.HasValue && original.DueDate.HasValue && (original.DueDate.Value - original.StartDate.Value).TotalHours > 24.0)
      {
        if (!flag1)
        {
          if (!flag2)
          {
            DateTime? dueDate = delta.DueDate;
            DateTime? startDate = delta.StartDate;
            TimeSpan? nullable = dueDate.HasValue & startDate.HasValue ? new TimeSpan?(dueDate.GetValueOrDefault() - startDate.GetValueOrDefault()) : new TimeSpan?();
            ref TimeSpan? local = ref nullable;
            if ((local.HasValue ? (local.GetValueOrDefault().TotalHours > 24.0 ? 1 : 0) : 0) == 0)
              goto label_7;
          }
          return CalendarService.MoveType.OverPoint2OverPoint;
        }
label_7:
        if (!flag1)
          return CalendarService.MoveType.OverPoint2Point;
      }
      if (flag1 && !flag2)
        return CalendarService.MoveType.Day2Point;
      if (!flag1 & flag2)
        return CalendarService.MoveType.Point2Day;
      return flag1 ? CalendarService.MoveType.Day2Day : CalendarService.MoveType.Point2Point;
    }

    public static TimeDataModel GetMergedData(TimeDataModel original, TimeDataModel delta)
    {
      TimeDataModel mergedData = new TimeDataModel()
      {
        TaskId = original.TaskId,
        ItemId = original.ItemId,
        RepeatFlag = original.RepeatFlag,
        RepeatFrom = original.RepeatFrom
      };
      switch (CalendarService.GetMoveType(original, delta))
      {
        case CalendarService.MoveType.Arrange:
          mergedData.IsAllDay = delta.IsAllDay;
          mergedData.StartDate = delta.StartDate;
          mergedData.DueDate = new DateTime?();
          mergedData.HandleReminderMode = ReminderMode.SetDefault;
          break;
        case CalendarService.MoveType.ArrangeToDay:
          mergedData.IsAllDay = new bool?(false);
          if (original.StartDate.HasValue && delta.StartDate.HasValue)
          {
            TimeDataModel timeDataModel1 = mergedData;
            DateTime? nullable1 = original.StartDate;
            DateTime original1 = nullable1.Value;
            nullable1 = delta.StartDate;
            DateTime modify = nullable1.Value;
            DateTime? nullable2 = new DateTime?(DateUtils.SetDateOnly(original1, modify));
            timeDataModel1.StartDate = nullable2;
            nullable1 = original.DueDate;
            if (nullable1.HasValue)
            {
              nullable1 = original.StartDate;
              if (nullable1.HasValue)
              {
                nullable1 = delta.StartDate;
                if (nullable1.HasValue)
                {
                  nullable1 = original.DueDate;
                  DateTime dateTime1 = nullable1.Value;
                  nullable1 = original.StartDate;
                  DateTime dateTime2 = nullable1.Value;
                  double totalMinutes = (dateTime1 - dateTime2).TotalMinutes;
                  TimeDataModel timeDataModel2 = mergedData;
                  nullable1 = mergedData.StartDate;
                  DateTime? nullable3 = new DateTime?(nullable1.Value.AddMinutes(totalMinutes));
                  timeDataModel2.DueDate = nullable3;
                }
              }
            }
          }
          mergedData.HandleReminderMode = ReminderMode.SetDefault;
          break;
        case CalendarService.MoveType.Day2Point:
          mergedData.IsAllDay = new bool?(false);
          mergedData.StartDate = delta.StartDate;
          mergedData.DueDate = new DateTime?();
          mergedData.HandleReminderMode = ReminderMode.KeepDay;
          break;
        case CalendarService.MoveType.Point2Day:
          mergedData.IsAllDay = new bool?(true);
          mergedData.StartDate = delta.StartDate;
          mergedData.DueDate = new DateTime?();
          mergedData.HandleReminderMode = ReminderMode.SetDefault;
          break;
        case CalendarService.MoveType.Day2Day:
          mergedData.IsAllDay = new bool?(true);
          mergedData.StartDate = delta.StartDate;
          if (original.DueDate.HasValue && original.StartDate.HasValue && delta.StartDate.HasValue)
          {
            double totalDays = (original.DueDate.Value - original.StartDate.Value).TotalDays;
            mergedData.DueDate = new DateTime?(delta.StartDate.Value.AddDays(totalDays));
          }
          mergedData.HandleReminderMode = ReminderMode.None;
          break;
        case CalendarService.MoveType.Point2Point:
          mergedData.IsAllDay = new bool?(false);
          if (original.StartDate.HasValue && delta.StartDate.HasValue)
          {
            TimeDataModel timeDataModel3 = mergedData;
            DateTime? nullable4 = delta.StartDate;
            DateTime? nullable5 = new DateTime?(nullable4.Value);
            timeDataModel3.StartDate = nullable5;
            nullable4 = original.DueDate;
            if (nullable4.HasValue)
            {
              nullable4 = original.StartDate;
              if (nullable4.HasValue)
              {
                nullable4 = delta.StartDate;
                if (nullable4.HasValue)
                {
                  nullable4 = original.DueDate;
                  DateTime dateTime3 = nullable4.Value;
                  nullable4 = original.StartDate;
                  DateTime dateTime4 = nullable4.Value;
                  double totalMinutes = (dateTime3 - dateTime4).TotalMinutes;
                  TimeDataModel timeDataModel4 = mergedData;
                  nullable4 = mergedData.StartDate;
                  DateTime? nullable6 = new DateTime?(nullable4.Value.AddMinutes(totalMinutes));
                  timeDataModel4.DueDate = nullable6;
                }
              }
            }
          }
          mergedData.HandleReminderMode = ReminderMode.None;
          break;
        case CalendarService.MoveType.OverPoint2OverPoint:
          mergedData.IsAllDay = new bool?(false);
          if (original.StartDate.HasValue && delta.StartDate.HasValue)
          {
            TimeDataModel timeDataModel5 = mergedData;
            DateTime? nullable7 = original.StartDate;
            DateTime original2 = nullable7.Value;
            nullable7 = delta.StartDate;
            DateTime modify = nullable7.Value;
            DateTime? nullable8 = new DateTime?(DateUtils.SetDateOnly(original2, modify));
            timeDataModel5.StartDate = nullable8;
            nullable7 = original.DueDate;
            if (nullable7.HasValue)
            {
              nullable7 = original.StartDate;
              if (nullable7.HasValue)
              {
                nullable7 = delta.StartDate;
                if (nullable7.HasValue)
                {
                  nullable7 = original.DueDate;
                  DateTime dateTime5 = nullable7.Value;
                  nullable7 = original.StartDate;
                  DateTime dateTime6 = nullable7.Value;
                  double totalMinutes = (dateTime5 - dateTime6).TotalMinutes;
                  TimeDataModel timeDataModel6 = mergedData;
                  nullable7 = mergedData.StartDate;
                  DateTime? nullable9 = new DateTime?(nullable7.Value.AddMinutes(totalMinutes));
                  timeDataModel6.DueDate = nullable9;
                }
              }
            }
          }
          mergedData.HandleReminderMode = ReminderMode.None;
          break;
        case CalendarService.MoveType.OverPoint2Point:
          mergedData.IsAllDay = new bool?(false);
          mergedData.StartDate = delta.StartDate;
          mergedData.DueDate = new DateTime?();
          mergedData.HandleReminderMode = ReminderMode.None;
          break;
      }
      return mergedData;
    }

    public static async Task SetEventDate(
      string eventId,
      DateTime startDate,
      DateTime endDate,
      bool onlyEnd)
    {
      CalendarEventModel calEvent = await CalendarEventDao.GetEventById(eventId);
      if (calEvent == null)
      {
        calEvent = (CalendarEventModel) null;
      }
      else
      {
        calEvent.DueEnd = new DateTime?(endDate);
        await CalendarEventDao.SaveEvent(calEvent);
        await SyncStatusDao.AddEventModifySyncStatus(calEvent.EventId);
        calEvent = (CalendarEventModel) null;
      }
    }

    public static async Task SaveEventTime(string eventId, TimeData time)
    {
      CalendarEventModel calEvent = await CalendarEventDao.GetEventById(eventId);
      if (calEvent == null)
      {
        calEvent = (CalendarEventModel) null;
      }
      else
      {
        bool isRepeatEvent = !string.IsNullOrEmpty(calEvent.RepeatFlag);
        bool? isAllDay = time.IsAllDay;
        bool flag1 = ((int) isAllDay ?? 1) != 0;
        DateTime? nullable1 = time.StartDate;
        DateTime? nullable2 = calEvent.DueStart;
        int num1;
        if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (nullable1.GetValueOrDefault() != nullable2.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0)
        {
          nullable2 = time.DueDate;
          nullable1 = calEvent.DueEnd;
          if ((nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (nullable2.GetValueOrDefault() != nullable1.GetValueOrDefault() ? 1 : 0) : 0) : 1) == 0 && !(time.RepeatFlag != calEvent.RepeatFlag))
          {
            num1 = flag1 != calEvent.IsAllDay ? 1 : 0;
            goto label_6;
          }
        }
        num1 = 1;
label_6:
        bool flag2 = num1 != 0;
        bool flag3 = false;
        if (time.Reminders.Any<TaskReminderModel>())
        {
          List<int> intList = new List<int>();
          foreach (TaskReminderModel reminder1 in time.Reminders)
          {
            int reminder2 = TriggerUtils.TriggerToReminder(reminder1.trigger);
            intList.Add(reminder2);
          }
          string str = JsonConvert.SerializeObject((object) intList);
          flag3 = str != calEvent.Reminders;
          if (flag3)
            calEvent.Reminders = str;
        }
        else if (calEvent.ReminderList != null && calEvent.ReminderList.Any<int>())
        {
          flag3 = true;
          calEvent.ReminderList = (List<int>) null;
        }
        if (!flag2 && !flag3)
        {
          calEvent = (CalendarEventModel) null;
        }
        else
        {
          CalendarEventModel calendarEventModel1 = calEvent;
          isAllDay = time.IsAllDay;
          int num2 = (int) isAllDay ?? 1;
          calendarEventModel1.IsAllDay = num2 != 0;
          if (calEvent.IsAllDay)
          {
            nullable1 = time.StartDate;
            if (nullable1.HasValue)
            {
              double num3 = DateTimeOffset.Now.Offset.TotalMinutes;
              if (num3 < 0.0)
                num3 = 0.0;
              CalendarEventModel calendarEventModel2 = calEvent;
              nullable1 = time.StartDate;
              DateTime valueOrDefault = nullable1.Value;
              DateTime? nullable3 = new DateTime?(valueOrDefault.AddMinutes(num3));
              calendarEventModel2.DueStart = nullable3;
              CalendarEventModel calendarEventModel3 = calEvent;
              nullable1 = time.DueDate;
              ref DateTime? local = ref nullable1;
              DateTime dateTime;
              if (!local.HasValue)
              {
                nullable2 = calEvent.DueStart;
                valueOrDefault = nullable2.Value;
                dateTime = valueOrDefault.AddDays(1.0);
              }
              else
              {
                valueOrDefault = local.GetValueOrDefault();
                dateTime = valueOrDefault.AddMinutes(num3);
              }
              DateTime? nullable4 = new DateTime?(dateTime);
              calendarEventModel3.DueEnd = nullable4;
              goto label_26;
            }
          }
          calEvent.DueStart = time.StartDate;
          CalendarEventModel calendarEventModel4 = calEvent;
          nullable1 = time.DueDate;
          DateTime? nullable5 = nullable1 ?? time.StartDate;
          calendarEventModel4.DueEnd = nullable5;
label_26:
          calEvent.RepeatFlag = time.RepeatFlag;
          calEvent.ExDates = string.Empty;
          calEvent.ExDateList = (List<DateTime>) null;
          await CalendarEventDao.SaveEvent(calEvent);
          await SyncStatusDao.AddEventModifySyncStatus(calEvent.EventId);
          if (ABTestManager.IsNewRemindCalculate())
            EventReminderCalculator.RecalEventReminders(calEvent);
          else
            ReminderCalculator.AssembleReminders();
          if (isRepeatEvent)
            await CalendarService.HandleDerivativeOnTimeChanged(eventId, calEvent.Uid);
          CalendarEventChangeNotifier.NotifyEventChanged(calEvent);
          calEvent = (CalendarEventModel) null;
        }
      }
    }

    private static async Task HandleDerivativeOnTimeChanged(string id, string uid)
    {
      List<CalendarEventModel> eventDerivatives = await CalendarEventDao.GetEventDerivatives(id, uid);
      if (!eventDerivatives.Any<CalendarEventModel>())
        return;
      foreach (CalendarEventModel calEvent in eventDerivatives)
      {
        await CalendarService.DeleteEvent(calEvent.Id);
        await SyncStatusDao.AddEventDeletedSyncStatus(calEvent.EventId);
      }
    }

    public static async Task MoveCalendar(string eventId, string calendarId)
    {
      CalendarEventModel model = await CalendarEventDao.GetEventById(eventId);
      if (model == null)
      {
        model = (CalendarEventModel) null;
      }
      else
      {
        BindCalendarModel calendar = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (item => item.Id == calendarId));
        if (calendar == null)
          model = (CalendarEventModel) null;
        else if (CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (item => item.Id == calendar.AccountId)).IsCalDav())
        {
          string str1 = Guid.NewGuid().ToString();
          string str2 = str1 + "@" + model.CalendarId;
          CalendarEventModel calEvent = CalendarEventModel.Copy(model);
          calEvent.Id = str2;
          calEvent.EventId = str1;
          calEvent.Uid = str1;
          calEvent.CalendarId = calendarId;
          await CalendarService.AddEvent(calEvent);
          await CalendarService.DeleteEvent(model.Id);
          await SyncStatusDao.AddEventDeletedSyncStatus(model.EventId);
          model = (CalendarEventModel) null;
        }
        else
        {
          string fromCalendarId = model.CalendarId;
          if (fromCalendarId != calendarId)
          {
            string originalId = model.Id;
            model.CalendarId = calendarId;
            model.Id = model.EventId + "@" + calendarId;
            await CalendarEventDao.SaveEvent(model);
            await SyncStatusDao.AddMoveEventStatus(model.EventId, fromCalendarId);
            await TaskSortOrderInDateDao.SyncOnEventCalendarChanged(originalId, model.Id);
            await TaskSortOrderInPriorityDao.SyncOnEventCalendarChanged(originalId, model.Id);
            originalId = (string) null;
          }
          fromCalendarId = (string) null;
          model = (CalendarEventModel) null;
        }
      }
    }

    public static async Task SaveEventContent(string eventId, string content)
    {
      CalendarEventModel calEvent = await CalendarEventDao.GetEventById(eventId);
      if (calEvent == null)
        calEvent = (CalendarEventModel) null;
      else if (!(calEvent.Content != content))
      {
        calEvent = (CalendarEventModel) null;
      }
      else
      {
        calEvent.Content = content;
        await CalendarEventDao.SaveEvent(calEvent);
        await SyncStatusDao.AddEventModifySyncStatus(calEvent.EventId);
        calEvent = (CalendarEventModel) null;
      }
    }

    public static async Task SaveEventTitle(string eventId, string title)
    {
      CalendarEventModel calEvent = await CalendarEventDao.GetEventById(eventId);
      if (calEvent == null)
        calEvent = (CalendarEventModel) null;
      else if (!(calEvent.Title != title))
      {
        calEvent = (CalendarEventModel) null;
      }
      else
      {
        calEvent.Title = title;
        await CalendarEventDao.SaveEvent(calEvent);
        await SyncStatusDao.AddEventModifySyncStatus(calEvent.EventId);
        CalendarEventChangeNotifier.NotifyTitleChanged(eventId, title);
        SyncManager.TryDelaySync(3000);
        calEvent = (CalendarEventModel) null;
      }
    }

    public static async void SaveEventLocation(string eventId, string location)
    {
      CalendarEventModel calEvent = await CalendarEventDao.GetEventById(eventId);
      if (calEvent == null)
        calEvent = (CalendarEventModel) null;
      else if (!(calEvent.Location != location))
      {
        calEvent = (CalendarEventModel) null;
      }
      else
      {
        calEvent.Location = location;
        await CalendarEventDao.SaveEvent(calEvent);
        await SyncStatusDao.AddEventModifySyncStatus(calEvent.EventId);
        calEvent = (CalendarEventModel) null;
      }
    }

    public static async Task AddEvent(CalendarEventModel calEvent)
    {
      await CalendarEventDao.AddEvent(calEvent);
      EventReminderCalculator.RecalEventReminders(calEvent);
      await SyncStatusDao.AddEventCreateSyncStatus(calEvent.EventId);
    }

    public static async Task UndoDeleteEvent(string eventId)
    {
      CalendarEventModel calEvent = await CalendarEventDao.GetEventByEventId(eventId);
      if (calEvent == null)
      {
        calEvent = (CalendarEventModel) null;
      }
      else
      {
        calEvent.Deleted = 0;
        await CalendarEventDao.SaveEvent(calEvent);
        EventReminderCalculator.RecalEventReminders(calEvent);
        CalendarEventChangeNotifier.NotifyEventDeleted(eventId);
        calEvent = (CalendarEventModel) null;
      }
    }

    public static async Task DeleteEvent(string eventId)
    {
      CalendarEventModel model = await CalendarEventDao.GetEventById(eventId);
      if (model == null)
      {
        model = (CalendarEventModel) null;
      }
      else
      {
        model.Deleted = 1;
        await CalendarEventDao.SaveEvent(model);
        EventReminderCalculator.RecalEventReminders(model);
        CalendarEventChangeNotifier.NotifyEventDeleted(eventId);
        model = (CalendarEventModel) null;
      }
    }

    public static async Task PullSubscribeEventsByCalIdAsync(string calendarId)
    {
      SubscribeCalendarModel subscribeEventsByCalId = await Communicator.GetSubscribeEventsByCalId(calendarId);
      if (subscribeEventsByCalId?.events == null)
        return;
      string str = Utils.GetCurrentUserIdInt().ToString();
      foreach (CalendarEventModel calendarEventModel in subscribeEventsByCalId.events)
      {
        CalendarEventModel eve = calendarEventModel;
        eve.UserId = str;
        eve.CalendarId = calendarId;
        eve.Type = 1;
        if (string.IsNullOrEmpty(eve.TimeZone))
          eve.TimeZone = TimeZoneData.LocalTimeZoneModel.TimeZoneName;
        if (eve.ExDateList != null && eve.ExDateList.Any<DateTime>())
          eve.ExDates = JsonConvert.SerializeObject((object) eve.ExDateList.Select<DateTime, DateTime>((Func<DateTime, DateTime>) (date => TimeZoneUtils.LocalToTargetTzTime(date, eve.TimeZone).Date)));
        if (eve.ReminderList != null && eve.ReminderList.Any<int>())
          eve.Reminders = JsonConvert.SerializeObject((object) eve.ReminderList);
        if (eve.IsAllDay)
        {
          eve.DueStart = TimeZoneUtils.LocalToTargetZoneTime(eve.DueStart, eve.TimeZone);
          eve.DueEnd = TimeZoneUtils.LocalToTargetZoneTime(eve.DueEnd, eve.TimeZone);
        }
      }
      await CalendarEventDao.SaveEvents(subscribeEventsByCalId.events, calendarId);
    }

    public static void TryLogResult(string calendarId, string result)
    {
      if (CalendarService._loged.Contains(calendarId))
        return;
      UtilLog.Admin(result);
      CalendarService._loged.Add(calendarId);
    }

    public static async Task CheckExpiredAccounts()
    {
      int dateNum = DateUtils.GetDateNum(DateTime.Today.AddDays(-6.0));
      if (LocalSettings.Settings.ExtraSettings.BindCalendarExpireCheckTime >= (long) dateNum)
        return;
      UtilLog.Info("BeforeNotifyExpired  " + LocalSettings.Settings.ExtraSettings.BindCalendarExpireCheckTime.ToString());
      LocalSettings.Settings.ExtraSettings.BindCalendarExpireCheckTime = (long) DateUtils.GetDateNum(DateTime.Today);
      LocalSettings.Settings.Save(true);
      List<BindCalendarAccountModel> expired = (await BindCalendarAccountDao.GetBindCalendarAccounts()).Where<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (a => a.Expired)).ToList<BindCalendarAccountModel>();
      List<CalendarSubscribeProfileModel> expiredCalendars = (await CalendarSubscribeProfileDao.GetProfiles()).Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (a => a.Expired)).ToList<CalendarSubscribeProfileModel>();
      if (expired.Count > 0 || expiredCalendars.Count > 0)
        Application.Current.Dispatcher.Invoke((Action) (() => SubscribeExpiredWindow.TryShowWindow(expired, expiredCalendars)));
      UtilLog.Info("AfterNotifyExpired  " + LocalSettings.Settings.ExtraSettings.BindCalendarExpireCheckTime.ToString());
    }

    private enum MoveType
    {
      Arrange,
      ArrangeToDay,
      Day2Point,
      Point2Day,
      Day2Day,
      Point2Point,
      OverPoint2OverPoint,
      OverPoint2Point,
    }
  }
}
