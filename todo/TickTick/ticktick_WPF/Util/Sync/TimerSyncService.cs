// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.TimerSyncService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Notifier;
using ticktick_WPF.Service;
using TickTickDao;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class TimerSyncService
  {
    private static readonly TTAsyncLocker TimerPullLocker = new TTAsyncLocker(1, 1);
    private static readonly HashSet<string> PulledKey = new HashSet<string>();

    public static async Task PullRemoteTimers()
    {
      await TimerSyncService.TimerPullLocker.RunAsync(new Func<Task>(TimerSyncService.PullTimers));
    }

    private static async Task PullTimers()
    {
      List<TimerModel> remotes = await Communicator.PullRemoteTimers();
      if (remotes == null)
      {
        remotes = (List<TimerModel>) null;
      }
      else
      {
        if (await TimerSyncService.MergeRemoteTimers(remotes))
          PomoNotifier.NotifyTimerChanged();
        UtilLog.Info(string.Format("PullRemoteTimers : Count {0}", (object) remotes.Count));
        remotes = (List<TimerModel>) null;
      }
    }

    private static async Task<bool> MergeRemoteTimers(List<TimerModel> remoteTimers)
    {
      List<TimerModel> source = await BaseDao<TimerModel>.GetAllAsync() ?? new List<TimerModel>();
      List<TimerModel> added = new List<TimerModel>();
      List<TimerModel> updated = new List<TimerModel>();
      List<TimerModel> deleted = new List<TimerModel>();
      if (remoteTimers.Any<TimerModel>())
      {
        foreach (TimerModel remoteTimer in remoteTimers)
        {
          TimerModel remote = remoteTimer;
          TimerModel timerModel1 = source.FirstOrDefault<TimerModel>((Func<TimerModel, bool>) (timer => timer.Id == remote.Id));
          int currentUserIdInt;
          if (timerModel1 != null)
          {
            if (timerModel1.Etag != remote.Etag && timerModel1.SyncStatus == 2)
            {
              remote._Id = timerModel1._Id;
              TimerModel timerModel2 = remote;
              currentUserIdInt = Utils.GetCurrentUserIdInt();
              string str = currentUserIdInt.ToString();
              timerModel2.UserId = str;
              remote.SyncStatus = 2;
              updated.Add(remote);
            }
            source.Remove(timerModel1);
          }
          else
          {
            TimerModel timerModel3 = remote;
            currentUserIdInt = Utils.GetCurrentUserIdInt();
            string str = currentUserIdInt.ToString();
            timerModel3.UserId = str;
            remote.SyncStatus = 2;
            added.Add(remote);
          }
        }
        if (source.Any<TimerModel>())
          deleted.AddRange(source.Where<TimerModel>((Func<TimerModel, bool>) (local => local.SyncStatus == 2)));
        if (added.Any<TimerModel>())
        {
          int num1 = await App.Connection.InsertAllAsync((IEnumerable) added);
        }
        if (updated.Any<TimerModel>())
        {
          int num2 = await App.Connection.UpdateAllAsync((IEnumerable) updated);
        }
        if (deleted.Any<TimerModel>())
        {
          foreach (object obj in deleted)
          {
            int num3 = await App.Connection.DeleteAsync(obj);
          }
        }
      }
      else if (source.Any<TimerModel>())
      {
        foreach (TimerModel timerModel in source)
        {
          if (timerModel.SyncStatus != 0)
          {
            int num = await App.Connection.DeleteAsync((object) timerModel);
          }
        }
        return true;
      }
      return added.Any<TimerModel>() || updated.Any<TimerModel>() || deleted.Any<TimerModel>();
    }

    public static async Task PostTimers()
    {
      List<TimerModel> needPostTimers = await TimerDao.GetNeedPostTimers();
      ModelSyncBean<TimerModel> syncBean = new ModelSyncBean<TimerModel>();
      BatchUpdateResult result;
      if (needPostTimers == null)
      {
        syncBean = (ModelSyncBean<TimerModel>) null;
        result = (BatchUpdateResult) null;
      }
      else if (!needPostTimers.Any<TimerModel>())
      {
        syncBean = (ModelSyncBean<TimerModel>) null;
        result = (BatchUpdateResult) null;
      }
      else
      {
        foreach (TimerModel timerModel in needPostTimers)
        {
          switch (timerModel.SyncStatus)
          {
            case -1:
              syncBean.Deleted.Add(timerModel.Id);
              continue;
            case 0:
              syncBean.Add.Add(timerModel);
              continue;
            case 1:
              syncBean.Update.Add(timerModel);
              continue;
            default:
              continue;
          }
        }
        if (syncBean.Empty())
        {
          syncBean = (ModelSyncBean<TimerModel>) null;
          result = (BatchUpdateResult) null;
        }
        else
        {
          result = await Communicator.UpdateTimers(syncBean);
          BatchUpdateResult batchUpdateResult1 = result;
          if ((batchUpdateResult1 != null ? (batchUpdateResult1.Id2etag.Count > 0 ? 1 : 0) : 0) != 0)
          {
            try
            {
              foreach (string key in result.Id2etag.Keys)
              {
                string etag = result.Id2etag[key];
                await TimerService.SaveEtag(key, etag);
              }
            }
            catch (Exception ex)
            {
            }
          }
          BatchUpdateResult batchUpdateResult2 = result;
          if ((batchUpdateResult2 != null ? (batchUpdateResult2.Id2error.Count > 0 ? 1 : 0) : 0) != 0)
          {
            try
            {
              foreach (string key in result.Id2error.Keys)
              {
                switch (result.Id2error[key])
                {
                  case "OBJ_NOT_EXISTED":
                    TimerDao.DeleteByIds(new List<string>()
                    {
                      key
                    });
                    PomoNotifier.NotifyTimerChanged();
                    continue;
                  case "OBJ_BOUND":
                    PomoService.RemovePomoTaskTimerId(key);
                    TimerDao.DeleteByIds(new List<string>()
                    {
                      key
                    });
                    PomoNotifier.NotifyTimerChanged();
                    continue;
                  case "EXISTED":
                    TimerDao.SetTimerSyncStatus(key, 1);
                    continue;
                  case "NOT_EXISTED":
                    TimerDao.SetTimerSyncStatus(key, 0);
                    continue;
                  default:
                    continue;
                }
              }
            }
            catch (Exception ex)
            {
            }
          }
          if (result == null)
          {
            syncBean = (ModelSyncBean<TimerModel>) null;
            result = (BatchUpdateResult) null;
          }
          else
          {
            TimerDao.DeleteByIds(syncBean.Deleted);
            syncBean = (ModelSyncBean<TimerModel>) null;
            result = (BatchUpdateResult) null;
          }
        }
      }
    }

    public static async Task<TimerOverviewModel> PullTimerOverview(string timerId)
    {
      TimerModel timer = await TimerDao.GetTimerById(timerId);
      if (timer == null)
        return (TimerOverviewModel) null;
      TimerOverviewModel overview = await Communicator.PullTimerOverview(timerId);
      if (overview == null && timer.Overview == null)
      {
        Dictionary<DateTime, double> timerRecord = await TimerService.GetTimerRecord(timerId, timer.ObjId, timer.ObjType);
        overview = new TimerOverviewModel()
        {
          Days = timerRecord.Keys.Count,
          Today = timerRecord.ContainsKey(DateTime.Today) ? (long) timerRecord[DateTime.Today] : 0L,
          Total = (long) timerRecord.Values.ToList<double>().Sum()
        };
      }
      if (overview != null)
      {
        overview.Date = DateUtils.GetDateNum(DateTime.Today);
        await TimerDao.SaveOverview(timerId, JsonConvert.SerializeObject((object) overview));
      }
      return overview;
    }

    public static async Task<bool> PullTimerStatistics(
      string timerId,
      DateTime start,
      DateTime end,
      string interval)
    {
      TimerModel timer = await TimerDao.GetTimerById(timerId);
      if (timer == null)
        return false;
      string key = TimerService.GetTimerStatisticsKey(timerId, start, end, interval);
      string statisticsString = await Communicator.PullTimerStatistics(timerId, start, end, interval);
      TimerStatisticsModel statistics = await TimerDao.GetStatistics(key);
      Dictionary<string, long> dictionary;
      try
      {
        dictionary = JsonConvert.DeserializeObject<Dictionary<string, long>>(statisticsString);
      }
      catch (Exception ex)
      {
      }
      if (dictionary == null)
      {
        if (statistics == null)
        {
          await TimerDao.SaveStatistics(key, JsonConvert.SerializeObject((object) await TimerService.CalTimerStatistics(timer, start, end, interval)));
          return true;
        }
      }
      else if (statistics == null || statistics.Value != statisticsString)
      {
        await TimerDao.SaveStatistics(key, statisticsString);
        return true;
      }
      return false;
    }

    public static async Task<bool> TryPullTimerStatistics(
      string timerId,
      DateTime start,
      DateTime end,
      string interval)
    {
      string timerStatisticsKey = TimerService.GetTimerStatisticsKey(timerId, start, end, interval);
      if (TimerSyncService.PulledKey.Contains(timerStatisticsKey))
        return false;
      TimerSyncService.PulledKey.Add(timerStatisticsKey);
      return await TimerSyncService.PullTimerStatistics(timerId, start, end, interval);
    }

    public static void ClearPulledKey() => TimerSyncService.PulledKey.Clear();
  }
}
