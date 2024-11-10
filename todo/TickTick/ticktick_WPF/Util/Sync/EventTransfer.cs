// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.EventTransfer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Sync.Model;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public static class EventTransfer
  {
    public static async Task<SyncEventBean> DescribeSyncTaskBean(
      List<SyncStatusModel> addedEvents,
      List<SyncStatusModel> updatedEvents,
      List<SyncStatusModel> movedEvents,
      List<SyncStatusModel> deletedEvents,
      bool isCalDav = false)
    {
      SyncEventBean bean = new SyncEventBean();
      List<BindCalendarModel> bindCalendars = CacheManager.GetBindCalendars();
      List<BindCalendarAccountModel> source1 = !isCalDav ? CacheManager.GetBindCalendarAccounts().FindAll((Predicate<BindCalendarAccountModel>) (account => !account.IsCalDav())) : CacheManager.GetBindCalendarAccounts().FindAll((Predicate<BindCalendarAccountModel>) (account => account.IsCalDav()));
      if (source1.Any<BindCalendarAccountModel>())
      {
        foreach (BindCalendarAccountModel calendarAccountModel in source1)
        {
          BindCalendarAccountModel account = calendarAccountModel;
          CalendarAccountBean calendarAccountBean = new CalendarAccountBean()
          {
            AccountId = account.Id
          };
          List<BindCalendarModel> list = bindCalendars.Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.AccountId == account.Id && cal.Accessible)).ToList<BindCalendarModel>();
          List<CalendarEventBean> calendarEventBeanList = new List<CalendarEventBean>();
          if (list.Any<BindCalendarModel>())
          {
            foreach (BindCalendarModel bindCalendarModel in list)
              calendarEventBeanList.Add(new CalendarEventBean()
              {
                CalendarId = bindCalendarModel.Id
              });
          }
          calendarAccountBean.CalendarBeans = calendarEventBeanList;
          bean.CalendarAccountBeans.Add(calendarAccountBean);
        }
      }
      if (movedEvents.Any<SyncStatusModel>())
      {
        foreach (SyncStatusModel movedEvent in movedEvents)
          await EventTransfer.AddUpdateEvent(bean, movedEvent, EventTransfer.ModifyType.Move);
      }
      if (addedEvents.Any<SyncStatusModel>())
      {
        foreach (SyncStatusModel addedEvent in addedEvents)
          await EventTransfer.AddUpdateEvent(bean, addedEvent, EventTransfer.ModifyType.Add);
      }
      if (updatedEvents.Any<SyncStatusModel>())
      {
        foreach (SyncStatusModel updatedEvent in updatedEvents)
          await EventTransfer.AddUpdateEvent(bean, updatedEvent, EventTransfer.ModifyType.Modify);
      }
      if (deletedEvents.Any<SyncStatusModel>())
      {
        foreach (SyncStatusModel deletedEvent in deletedEvents)
          await EventTransfer.AddUpdateEvent(bean, deletedEvent, EventTransfer.ModifyType.Delete);
      }
      SyncEventBean syncEventBean1 = new SyncEventBean();
      foreach (CalendarAccountBean calendarAccountBean in bean.CalendarAccountBeans)
      {
        List<CalendarEventBean> source2 = new List<CalendarEventBean>();
        if (calendarAccountBean.CalendarBeans.Any<CalendarEventBean>())
        {
          foreach (CalendarEventBean calendarBean in calendarAccountBean.CalendarBeans)
          {
            if (calendarBean.Add.Any<CalendarEventModel>() || calendarBean.Move.Any<EventMoveModel>() || calendarBean.Delete.Any<string>() || calendarBean.Update.Any<CalendarEventModel>())
              source2.Add(calendarBean);
          }
        }
        if (source2.Any<CalendarEventBean>())
        {
          calendarAccountBean.CalendarBeans = source2;
          syncEventBean1.CalendarAccountBeans.Add(calendarAccountBean);
        }
      }
      SyncEventBean syncEventBean2 = syncEventBean1;
      bean = (SyncEventBean) null;
      return syncEventBean2;
    }

    private static async Task AddUpdateEvent(
      SyncEventBean eventBean,
      SyncStatusModel model,
      EventTransfer.ModifyType type)
    {
      CalendarEventModel eventByEventId = await CalendarEventDao.GetEventByEventId(model.EntityId);
      if (eventByEventId == null)
        return;
      eventByEventId.Id = eventByEventId.EventId;
      DateTime? nullable1 = eventByEventId.DueStart;
      if (!nullable1.HasValue)
        eventByEventId.DueStart = new DateTime?(DateTime.Today);
      nullable1 = eventByEventId.DueEnd;
      if (!nullable1.HasValue)
        eventByEventId.DueEnd = new DateTime?(DateTime.Today);
      if (eventByEventId.IsAllDay)
      {
        double totalMinutes = DateTimeOffset.Now.Offset.TotalMinutes;
        CalendarEventModel calendarEventModel1 = eventByEventId;
        nullable1 = eventByEventId.DueStart;
        DateTime date = nullable1.Value;
        date = date.Date;
        DateTime? nullable2 = new DateTime?(date.AddMinutes(totalMinutes));
        calendarEventModel1.DueStart = nullable2;
        CalendarEventModel calendarEventModel2 = eventByEventId;
        nullable1 = eventByEventId.DueEnd;
        date = nullable1.Value;
        date = date.Date;
        DateTime? nullable3 = new DateTime?(date.AddMinutes(totalMinutes));
        calendarEventModel2.DueEnd = nullable3;
      }
      if (eventByEventId.ReminderList != null && eventByEventId.ReminderList.Any<int>())
        eventByEventId.ReminderList = eventByEventId.ReminderList.Where<int>((Func<int, bool>) (reminder => reminder >= 0)).ToList<int>();
      foreach (CalendarAccountBean calendarAccountBean in eventBean.CalendarAccountBeans)
      {
        foreach (CalendarEventBean calendarBean in calendarAccountBean.CalendarBeans)
        {
          if (type == EventTransfer.ModifyType.Move)
          {
            if (calendarBean.CalendarId == model.MoveFromId)
            {
              calendarBean.Move.Add(new EventMoveModel()
              {
                EventId = eventByEventId.EventId,
                Destination = eventByEventId.CalendarId
              });
              break;
            }
          }
          else if (calendarBean.CalendarId == eventByEventId.CalendarId)
          {
            switch (type)
            {
              case EventTransfer.ModifyType.Add:
                calendarBean.Add.Add(eventByEventId);
                continue;
              case EventTransfer.ModifyType.Modify:
                calendarBean.Update.Add(eventByEventId);
                continue;
              case EventTransfer.ModifyType.Delete:
                calendarBean.Delete.Add(eventByEventId.Id);
                continue;
              default:
                continue;
            }
          }
        }
      }
    }

    private enum ModifyType
    {
      Add,
      Modify,
      Move,
      Delete,
    }
  }
}
