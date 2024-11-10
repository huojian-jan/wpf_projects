// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.EventBatchHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util.Sync.Model;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class EventBatchHandler : BatchHandler
  {
    public EventBatchHandler(string userId, SyncResult syncResult)
      : base(userId, syncResult)
    {
    }

    public static async Task<List<SyncStatusModel>> GetLocalModifiedEvents()
    {
      return await TaskDao.GetNeedPostTasks(9);
    }

    public static async Task<List<SyncStatusModel>> GetLocalAddedEvents()
    {
      return await TaskDao.GetNeedPostTasks(10);
    }

    public static async Task<List<SyncStatusModel>> GetLocalDeletedEvents()
    {
      return await TaskDao.GetNeedPostTasks(11);
    }

    public static async Task<List<SyncStatusModel>> GetLocalMoveEvents()
    {
      return await TaskDao.GetNeedPostTasks(12);
    }

    public static async Task HandleCommitResult(
      List<EventUpdateResult> results,
      SyncEventBean bean,
      bool checkEtag = true)
    {
      if (results == null || !results.Any<EventUpdateResult>())
        return;
      foreach (EventUpdateResult result in results)
        await EventBatchHandler.HandleCommitResult(result, bean, checkEtag);
    }

    private static async Task HandleCommitResult(
      EventUpdateResult result,
      SyncEventBean bean,
      bool checkEtag)
    {
      if (result != null && result.Id2error.Any<KeyValuePair<string, Dictionary<string, string>>>())
      {
        foreach (KeyValuePair<string, Dictionary<string, string>> keyValuePair1 in result.Id2error)
        {
          if (keyValuePair1.Value.Any<KeyValuePair<string, string>>())
          {
            foreach (KeyValuePair<string, string> keyValuePair2 in keyValuePair1.Value)
            {
              if (keyValuePair2.Value.ToLower() == "existed")
              {
                int num = await SyncStatusDao.DeleteSyncStatus(keyValuePair2.Key, 10) ? 1 : 0;
              }
            }
          }
        }
      }
      if (!checkEtag)
      {
        bean.CalendarAccountBeans.ForEach((Action<CalendarAccountBean>) (item => item.CalendarBeans.ForEach((Action<CalendarEventBean>) (d => d.Add.ForEach((Action<CalendarEventModel>) (e => SyncStatusDao.DeleteSyncStatus(e.EventId, 10)))))));
        bean.CalendarAccountBeans.ForEach((Action<CalendarAccountBean>) (item => item.CalendarBeans.ForEach((Action<CalendarEventBean>) (d => d.Update.ForEach((Action<CalendarEventModel>) (e => SyncStatusDao.DeleteSyncStatus(e.EventId, 9)))))));
        bean.CalendarAccountBeans.ForEach((Action<CalendarAccountBean>) (item => item.CalendarBeans.ForEach((Action<CalendarEventBean>) (d => d.Move.ForEach((Action<EventMoveModel>) (e => SyncStatusDao.DeleteSyncStatus(e.EventId, 12)))))));
        bean.CalendarAccountBeans.ForEach((Action<CalendarAccountBean>) (item => item.CalendarBeans.ForEach((Action<CalendarEventBean>) (d => d.Delete.ForEach((Action<string>) (e => SyncStatusDao.DeleteSyncStatus(e, 11)))))));
      }
      List<string> entityIdsByType1 = await SyncStatusDao.GetEntityIdsByType(10);
      string eventId;
      if (entityIdsByType1 != null && entityIdsByType1.Any<string>())
      {
        foreach (string str in entityIdsByType1)
        {
          eventId = str;
          if (await EventBatchHandler.UpdateEtag(result, eventId))
          {
            int num = await SyncStatusDao.DeleteSyncStatus(eventId, 10) ? 1 : 0;
          }
          eventId = (string) null;
        }
      }
      List<string> entityIdsByType2 = await SyncStatusDao.GetEntityIdsByType(9);
      if (entityIdsByType2 != null && entityIdsByType2.Any<string>())
      {
        foreach (string str in entityIdsByType2)
        {
          eventId = str;
          if (await EventBatchHandler.UpdateEtag(result, eventId))
          {
            int num = await SyncStatusDao.DeleteSyncStatus(eventId, 9) ? 1 : 0;
          }
          eventId = (string) null;
        }
      }
      List<string> entityIdsByType3 = await SyncStatusDao.GetEntityIdsByType(12);
      if (entityIdsByType3 != null && entityIdsByType3.Any<string>())
      {
        foreach (string str in entityIdsByType3)
        {
          eventId = str;
          if (await EventBatchHandler.UpdateEtag(result, eventId) || !checkEtag)
          {
            int num = await SyncStatusDao.DeleteSyncStatus(eventId, 12) ? 1 : 0;
          }
          eventId = (string) null;
        }
      }
      List<string> entityIdsByType4 = await SyncStatusDao.GetEntityIdsByType(11);
      if (entityIdsByType4 == null || !entityIdsByType4.Any<string>())
        return;
      foreach (string str in entityIdsByType4)
      {
        eventId = str;
        int num1 = await EventBatchHandler.UpdateEtag(result, eventId) ? 1 : 0;
        int num2 = await SyncStatusDao.DeleteSyncStatus(eventId, 11) ? 1 : 0;
        eventId = (string) null;
      }
    }

    private static async Task<bool> UpdateEtag(EventUpdateResult result, string eventId)
    {
      if (!string.IsNullOrWhiteSpace(eventId))
      {
        CalendarEventModel calendarEventModel1 = await CalendarEventDao.GetEventByEventId(eventId);
        if (calendarEventModel1 == null)
          calendarEventModel1 = await CalendarEventDao.GetEventById(eventId);
        CalendarEventModel calendarEventModel2 = calendarEventModel1;
        if (calendarEventModel2 != null && !string.IsNullOrEmpty(calendarEventModel2.CalendarId))
        {
          foreach (Dictionary<string, string> dictionary in result.Id2etag.Values)
          {
            if (dictionary.Keys.Contains<string>(calendarEventModel2.EventId))
            {
              calendarEventModel2.Etag = dictionary[calendarEventModel2.EventId];
              int num = await App.Connection.UpdateAsync((object) calendarEventModel2);
              return true;
            }
          }
        }
      }
      return false;
    }
  }
}
