// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.SyncServices.PomoSyncService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.SyncServices.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views.Tag;
using TickTickDao;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.SyncServices
{
  public static class PomoSyncService
  {
    private static readonly TTAsyncLocker<DateTime, DateTime, bool> SyncLock = new TTAsyncLocker<DateTime, DateTime, bool>();
    private static DateTime _lastStart;
    private static DateTime _lastEnd;
    private static DateTime _lastPullTime;
    private static long _timelinePullPoint;
    private static long _timerPullPoint;

    public static async Task TryPullRemote(DateTime startTime, DateTime endTime)
    {
      int num = await PomoSyncService.SyncLock.RunAsync(new Func<DateTime, DateTime, Task<bool>>(PomoSyncService.PullRemote), startTime, endTime) ? 1 : 0;
    }

    private static async Task<bool> PullRemote(DateTime startTime, DateTime endTime)
    {
      TimeSpan timeSpan;
      if (startTime == PomoSyncService._lastStart && PomoSyncService._lastEnd == endTime)
      {
        timeSpan = DateTime.Now - PomoSyncService._lastPullTime;
        if (timeSpan.TotalSeconds < 60.0)
          return false;
      }
      PomoSyncService._lastPullTime = DateTime.Now;
      PomoSyncService._lastStart = startTime;
      PomoSyncService._lastEnd = endTime;
      DateTime date = LocalSettings.Settings.PomoLocalSetting.EarliestFetchDate;
      DateTime origin = date;
      DateTime dateTime1 = DateTime.Today.AddDays(1.0);
      if (startTime > endTime)
        return false;
      if (Utils.IsEmptyDate(date))
        date = DateTime.Today.AddDays(1.0);
      DateTime dateTime2;
      if (startTime < date)
      {
        timeSpan = date - startTime;
        if (timeSpan.Days > 100)
        {
          int num1 = await PomoSyncService.Pull(startTime, endTime, false) ? 1 : 0;
          int num2 = await PomoSyncService.Pull(startTime, endTime, true) ? 1 : 0;
          return true;
        }
        if (date.AddDays(-31.0) < startTime)
          startTime = date.AddDays(-31.0);
        int num3 = await PomoSyncService.Pull(startTime, endTime, true) ? 1 : 0;
        int num4 = await PomoSyncService.Pull(startTime, endTime, false) ? 1 : 0;
        dateTime2 = startTime;
      }
      else
      {
        timeSpan = dateTime1 - endTime;
        if (timeSpan.Days > 14 || startTime > dateTime1)
          return false;
        timeSpan = dateTime1 - endTime;
        if (timeSpan.Days < 14)
        {
          ref DateTime local = ref endTime;
          timeSpan = endTime - dateTime1;
          double num = (double) (14 - timeSpan.Days);
          endTime = local.AddDays(num);
        }
        if (dateTime1 < endTime)
          endTime = dateTime1;
        int num5 = await PomoSyncService.Pull(startTime, endTime, false) ? 1 : 0;
        int num6 = await PomoSyncService.Pull(startTime, endTime, true) ? 1 : 0;
        dateTime2 = startTime;
      }
      UtilLog.Info(string.Format("{0}.PullRemote, Param: {1},{2},Set EarliestFetchDate: {3} to {4}", (object) nameof (PomoSyncService), (object) startTime, (object) endTime, (object) origin, (object) dateTime2));
      LocalSettings.Settings.PomoLocalSetting.EarliestFetchDate = dateTime2;
      return true;
    }

    public static async Task<bool> Pull7Days(bool isPomo)
    {
      DateTime today = DateTime.Today;
      DateTime start = today.AddDays(-7.0);
      today = DateTime.Today;
      DateTime end = today.AddDays(1.0);
      int num = isPomo ? 1 : 0;
      return await PomoSyncService.Pull(start, end, num != 0);
    }

    private static async Task<bool> Pull(DateTime start, DateTime end, bool isPomo)
    {
      List<PomodoroModel> pomodoroModelList;
      if (isPomo)
        pomodoroModelList = await Communicator.PullRemotePomos(start, end);
      else
        pomodoroModelList = await Communicator.PullRemoteTimings(start, end);
      List<PomodoroModel> remotes = pomodoroModelList;
      return remotes != null && await PomoSyncService.MergeRemotes(remotes, start, end, new int?(!isPomo ? 1 : 0));
    }

    private static async Task<bool> MergeRemotePomoOfTimer(
      List<PomodoroModel> remotes,
      DateTime start,
      DateTime end,
      string timerId)
    {
      int deleted = 0;
      int updated = 0;
      int added = 0;
      bool flag1 = false;
      TimerModel timerById = await TimerDao.GetTimerById(timerId);
      if (timerById == null)
        return false;
      List<PomodoroModel> locals = await PomoDao.GetPomoDescByStartAndTimerId(0, timerId, timerById.ObjId, timerById.ObjType == "habit", start, end);
      Dictionary<string, PomodoroModel> localDict = new Dictionary<string, PomodoroModel>();
      foreach (PomodoroModel pomodoroModel in locals)
      {
        if (!string.IsNullOrEmpty(pomodoroModel.Id))
          localDict[pomodoroModel.Id] = pomodoroModel;
      }
      PomodoroModel pomo;
      if (remotes.Count > 0)
      {
        foreach (PomodoroModel remote in remotes)
        {
          pomo = remote;
          if (localDict.ContainsKey(pomo.Id))
          {
            PomodoroModel local = localDict[pomo.Id];
            locals.Remove(local);
            if (local.SyncStatus == 2)
            {
              pomo._Id = local._Id;
              pomo.SyncStatus = 2;
              pomo.UserId = local.UserId;
              pomo.Type = local.Type;
              await PomoDao.UpdateAsync(pomo);
              int num = await PomoTaskDao.DeleteAllByPomoId(local.Id);
              ++updated;
              local = (PomodoroModel) null;
            }
            else
              continue;
          }
          else
          {
            pomo.SyncStatus = 2;
            pomo.UserId = Utils.GetCurrentUserStr();
            await PomoDao.InsertAsync(pomo);
            ++added;
          }
          if (pomo.Tasks != null && pomo.Tasks.Length != 0)
          {
            foreach (PomoTask task in pomo.Tasks)
            {
              task.PomoId = pomo.Id;
              if (task.Tags != null)
                task.TagString = TagSerializer.ToJsonContent(((IEnumerable<string>) task.Tags).ToList<string>());
            }
            try
            {
              await PomoTaskDao.InsertAllAsync((IEnumerable<PomoTask>) pomo.Tasks);
            }
            catch (Exception ex)
            {
              UtilLog.Warn("PullTimerTasks" + ex.Message);
            }
            pomo = (PomodoroModel) null;
          }
        }
      }
      foreach (PomodoroModel pomodoroModel in locals)
      {
        pomo = pomodoroModel;
        if (pomo.SyncStatus == 2 && pomo.EndTime <= end)
        {
          int num1 = await PomoDao.DeleteByIdAsync(pomo.Id);
          int num2 = await PomoTaskDao.DeleteAllByPomoId(pomo.Id);
          ++deleted;
          flag1 = true;
        }
        pomo = (PomodoroModel) null;
      }
      bool flag2 = added > 0 || deleted > 0;
      if (flag2)
      {
        TimerSyncService.ClearPulledKey();
        PomoNotifier.NotifyServiceChanged();
        UtilLog.Info(string.Format("PomoSyncService.PullTimerFocus, Param: {0},{1},{2}  Remote:{3} Locals:{4} CURD: ({5}, {6}, 0, {7})", (object) start, (object) end, (object) timerId, (object) remotes.Count, (object) locals.Count, (object) added, (object) updated, (object) deleted));
      }
      return flag2;
    }

    private static async Task<bool> MergeRemotes(
      List<PomodoroModel> remotes,
      DateTime start,
      DateTime end,
      int? type)
    {
      string deleted = "";
      int updated = 0;
      string added = "";
      List<PomodoroModel> locals = await PomoDao.GetPomoByDateSpan(start, end, true);
      locals = !type.HasValue ? locals : locals.Where<PomodoroModel>((Func<PomodoroModel, bool>) (p => p.Type == type.Value)).ToList<PomodoroModel>();
      Dictionary<string, PomodoroModel> localDict = new Dictionary<string, PomodoroModel>();
      foreach (PomodoroModel pomodoroModel in locals)
      {
        if (!string.IsNullOrEmpty(pomodoroModel.Id))
          localDict[pomodoroModel.Id] = pomodoroModel;
      }
      PomodoroModel pomo;
      if (remotes.Count > 0)
      {
        foreach (PomodoroModel remote in remotes)
        {
          pomo = remote;
          int? nullable1;
          if (localDict.ContainsKey(pomo.Id))
          {
            PomodoroModel local = localDict[pomo.Id];
            locals.Remove(local);
            if (local.SyncStatus == 2 || local.SyncStatus == -2 || local.SyncStatus == 0)
            {
              pomo._Id = local._Id;
              pomo.SyncStatus = 2;
              pomo.UserId = local.UserId;
              pomo.Type = local.Type;
              if (local.SyncStatus == -2 || local.SyncStatus == 0)
              {
                pomo.Note = MergeUtils.GetRevisedDiffText(string.Empty, pomo.Note, local.Note);
                pomo.SyncStatus = 1;
              }
              await PomoDao.UpdateAsync(pomo);
              int num = await PomoTaskDao.DeleteAllByPomoId(local.Id);
              nullable1 = pomo.Tasks?.Length;
              int valueOrDefault = nullable1.GetValueOrDefault();
              if (!(num == valueOrDefault & nullable1.HasValue))
              {
                string id = pomo.Id;
                PomoTask[] tasks = pomo.Tasks;
                int? nullable2;
                if (tasks == null)
                {
                  nullable1 = new int?();
                  nullable2 = nullable1;
                }
                else
                  nullable2 = new int?(tasks.Length);
                nullable1 = nullable2;
                string str = nullable1.ToString();
                UtilLog.Warn("MergePomo: " + id + ",RemoteTasks: " + str);
              }
              ++updated;
              local = (PomodoroModel) null;
            }
            else
            {
              local.Note = MergeUtils.GetRevisedDiffText(string.Empty, pomo.Note, local.Note);
              await PomoDao.UpdateAsync(local);
              continue;
            }
          }
          else
          {
            pomo.SyncStatus = 2;
            pomo.UserId = Utils.GetCurrentUserStr();
            PomodoroModel pomodoroModel = pomo;
            nullable1 = type;
            int num = nullable1 ?? pomo.Type;
            pomodoroModel.Type = num;
            await PomoDao.InsertAsync(pomo);
            added += pomo.Id;
          }
          if (pomo.Tasks != null && pomo.Tasks.Length != 0)
          {
            foreach (PomoTask task in pomo.Tasks)
            {
              task.PomoId = pomo.Id;
              if (task.Tags != null)
                task.TagString = TagSerializer.ToJsonContent(((IEnumerable<string>) task.Tags).ToList<string>());
            }
            try
            {
              await PomoTaskDao.InsertAllAsync((IEnumerable<PomoTask>) pomo.Tasks);
            }
            catch (Exception ex)
            {
              UtilLog.Warn("PullPomoTasks" + ex.Message);
            }
            pomo = (PomodoroModel) null;
          }
        }
      }
      foreach (PomodoroModel pomodoroModel in locals)
      {
        pomo = pomodoroModel;
        if (pomo.SyncStatus == 2 && pomo.EndTime <= end)
        {
          await PomoDao.DeleteAsync(pomo);
          int num = await PomoTaskDao.DeleteAllByPomoId(pomo.Id);
          deleted += pomo.Id;
        }
        pomo = (PomodoroModel) null;
      }
      bool flag1 = !string.IsNullOrEmpty(added) || !string.IsNullOrEmpty(deleted);
      if (flag1)
      {
        TimerSyncService.ClearPulledKey();
        PomoNotifier.NotifyServiceChanged();
        UtilLog.Info(string.Format("PomoSyncService.Pull, Param: {0},{1},{2}  Remote:{3} Locals:{4} CURD: ({5} | {6} | {7})", (object) start, (object) end, (object) type, (object) remotes.Count, (object) locals.Count, (object) added, (object) updated, (object) deleted));
      }
      bool flag2 = flag1;
      deleted = (string) null;
      added = (string) null;
      locals = (List<PomodoroModel>) null;
      localDict = (Dictionary<string, PomodoroModel>) null;
      return flag2;
    }

    public static async Task PullStatistics()
    {
      StatisticsModel statistics = await Communicator.GetStatistics();
      if (statistics != null)
      {
        statistics.Add(PomoService.GetStatisticsModel(await PomoDao.GetNeedPostPomos(), true));
        statistics.Date = DateUtils.GetDateNum(DateTime.Today);
        LocalSettings.Settings.StatisticsModel = statistics;
        statistics = (StatisticsModel) null;
      }
      else if (LocalSettings.Settings.StatisticsModel == null)
      {
        List<PomodoroModel> allPomo = await PomoDao.GetAllPomo();
        LocalSettings.Settings.StatisticsModel = PomoService.GetStatisticsModel(allPomo, false);
        LocalSettings.Settings.StatisticsModel.Date = DateUtils.GetDateNum(DateTime.Today);
        statistics = (StatisticsModel) null;
      }
      else if (LocalSettings.Settings.StatisticsModel.Date >= DateUtils.GetDateNum(DateTime.Today))
      {
        statistics = (StatisticsModel) null;
      }
      else
      {
        StatisticsModel statisticsModel = PomoService.GetStatisticsModel(await PomoDao.GetNeedPostPomos(), true);
        LocalSettings.Settings.StatisticsModel.todayPomoCount = statisticsModel.todayPomoCount;
        LocalSettings.Settings.StatisticsModel.todayPomoDuration = statisticsModel.todayPomoDuration;
        LocalSettings.Settings.StatisticsModel.Date = DateUtils.GetDateNum(DateTime.Today);
        statistics = (StatisticsModel) null;
      }
    }

    public static async Task<bool> PullFocusTimelines(bool resetTime = false, string timerId = null)
    {
      PomoSyncService._timelinePullPoint = resetTime || PomoSyncService._timelinePullPoint == 0L ? Utils.GetNowTimeStampInMills() : PomoSyncService._timelinePullPoint;
      long point = PomoSyncService._timelinePullPoint;
      if (point < 0L)
        return false;
      List<PomodoroModel> focusTimeline = await Communicator.GetFocusTimeline(point);
      if (focusTimeline == null)
        return false;
      DateTime dateTime1 = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
      DateTime end = dateTime1.AddMilliseconds((double) point);
      List<PomodoroModel> list = focusTimeline.OrderByDescending<PomodoroModel, DateTime>((Func<PomodoroModel, DateTime>) (f => f.StartTime)).ToList<PomodoroModel>();
      PomodoroModel pomodoroModel = list.LastOrDefault<PomodoroModel>();
      DateTime dateTime2;
      if (pomodoroModel == null)
      {
        dateTime1 = new DateTime();
        dateTime2 = dateTime1;
      }
      else
        dateTime2 = pomodoroModel.StartTime;
      DateTime dateTime3 = dateTime2;
      PomoSyncService._timelinePullPoint = list.Count >= 30 ? Utils.GetTimeStamp(new DateTime?(dateTime3)) : -1L;
      DateTime dateTime4;
      if (list.Count >= 30)
      {
        dateTime4 = dateTime3;
      }
      else
      {
        dateTime1 = new DateTime();
        dateTime4 = dateTime1;
      }
      DateTime start = dateTime4;
      return await PomoSyncService.MergeRemotes(list, start, end, new int?());
    }

    public static async Task<bool> PullTimerTimelines(
      long startPoint,
      bool resetTime = false,
      string timerId = null)
    {
      PomoSyncService._timelinePullPoint = resetTime || PomoSyncService._timelinePullPoint == 0L ? Utils.GetNowTimeStampInMills() : PomoSyncService._timelinePullPoint;
      if (PomoSyncService._timelinePullPoint < 0L || PomoSyncService._timelinePullPoint < startPoint)
        return false;
      long point = PomoSyncService._timelinePullPoint;
      List<PomodoroModel> timerFocusTimeline = await Communicator.GetTimerFocusTimeline(timerId, point);
      if (timerFocusTimeline == null)
        return false;
      DateTime dateTime1 = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
      DateTime end = dateTime1.AddMilliseconds((double) point);
      List<PomodoroModel> list = timerFocusTimeline.OrderByDescending<PomodoroModel, DateTime>((Func<PomodoroModel, DateTime>) (f => f.StartTime)).ToList<PomodoroModel>();
      PomodoroModel pomodoroModel = list.LastOrDefault<PomodoroModel>();
      DateTime dateTime2;
      if (pomodoroModel == null)
      {
        dateTime1 = new DateTime();
        dateTime2 = dateTime1;
      }
      else
        dateTime2 = pomodoroModel.StartTime;
      DateTime dateTime3 = dateTime2;
      PomoSyncService._timelinePullPoint = list.Count >= 30 ? Utils.GetTimeStamp(new DateTime?(dateTime3)) : -1L;
      DateTime dateTime4;
      if (list.Count >= 30)
      {
        dateTime4 = dateTime3;
      }
      else
      {
        dateTime1 = new DateTime();
        dateTime4 = dateTime1;
      }
      DateTime start = dateTime4;
      return await PomoSyncService.MergeRemotePomoOfTimer(list, start, end, timerId);
    }

    private static async Task HandleDeleted(List<string> deleted)
    {
      if (deleted == null || !deleted.Any<string>())
        return;
      foreach (string pomoId in deleted)
      {
        int num1 = await PomoDao.DeleteByIdAsync(pomoId);
        int num2 = await PomoTaskDao.DeleteAllByPomoId(pomoId);
      }
    }

    private static async Task CommitDelete(List<string> delete, bool isPomo)
    {
      List<string> deleted;
      string log;
      if (delete == null)
      {
        deleted = (List<string>) null;
        log = (string) null;
      }
      else if (delete.Count == 0)
      {
        deleted = (List<string>) null;
        log = (string) null;
      }
      else
      {
        deleted = new List<string>();
        log = "delete:";
        foreach (string deletePomoId in delete)
        {
          log = log + deletePomoId + ",";
          if (await Communicator.DeletePomo(deletePomoId, isPomo))
            deleted.Add(deletePomoId);
        }
        UtilLog.Info("CommitDeletePomos: " + log);
        PomoSyncService.HandleDeleted(deleted);
        deleted = (List<string>) null;
        log = (string) null;
      }
    }

    private static async Task CommitChanges(BaseSyncBean<PomodoroModel> pomoSyncBean, bool isPomo)
    {
      BatchUpdateResult result = (BatchUpdateResult) null;
      if (pomoSyncBean != null && pomoSyncBean.Any())
        result = await Communicator.BatchUpdatePomos(pomoSyncBean, isPomo);
      await PomoSyncService.HandleCommitResult(result);
    }

    private static async Task HandleCommitResult(BatchUpdateResult result)
    {
      if (result?.Id2error != null && result.Id2error.Count > 0)
      {
        List<PomodoroModel> pomoInIds = await PomoDao.GetPomoInIds(result.Id2error.Keys.ToList<string>());
        // ISSUE: explicit non-virtual call
        if (pomoInIds != null && __nonvirtual (pomoInIds.Count) > 0)
        {
          foreach (PomodoroModel pomodoroModel in pomoInIds.Where<PomodoroModel>((Func<PomodoroModel, bool>) (p => result.Id2error.ContainsKey(p.Id) && result.Id2error[p.Id] == "EXISTED")))
            pomodoroModel.SyncStatus = -2;
        }
        await PomoDao.UpdateAllAsync((IEnumerable<PomodoroModel>) pomoInIds);
      }
      Dictionary<string, string> id2Etag = result?.Id2etag;
      if (id2Etag == null)
        id2Etag = (Dictionary<string, string>) null;
      else if (id2Etag.Count <= 0)
      {
        id2Etag = (Dictionary<string, string>) null;
      }
      else
      {
        List<PomodoroModel> pomoInIds = await PomoDao.GetPomoInIds(id2Etag.Keys.ToList<string>());
        if (pomoInIds == null)
          id2Etag = (Dictionary<string, string>) null;
        else if (pomoInIds.Count <= 0)
        {
          id2Etag = (Dictionary<string, string>) null;
        }
        else
        {
          foreach (PomodoroModel pomodoroModel in pomoInIds)
          {
            pomodoroModel.SyncStatus = 2;
            if (id2Etag.ContainsKey(pomodoroModel.Id))
              pomodoroModel.Etag = id2Etag[pomodoroModel.Id];
          }
          await PomoDao.UpdateAllAsync((IEnumerable<PomodoroModel>) pomoInIds);
          id2Etag = (Dictionary<string, string>) null;
        }
      }
    }

    public static async Task CommitPomodoros()
    {
      List<PomodoroModel> needPostPomos = await PomoDao.GetNeedPostPomos();
      List<string> pomoDeleteList;
      List<string> timingDeleteList;
      if (needPostPomos == null)
      {
        needPostPomos = (List<PomodoroModel>) null;
        pomoDeleteList = (List<string>) null;
        timingDeleteList = (List<string>) null;
      }
      else if (needPostPomos.Count <= 0)
      {
        needPostPomos = (List<PomodoroModel>) null;
        pomoDeleteList = (List<string>) null;
        timingDeleteList = (List<string>) null;
      }
      else
      {
        UtilLog.Info("CommitPomodorosCount: " + needPostPomos.Count.ToString());
        foreach (PomodoroModel pomodoroModel1 in needPostPomos)
        {
          PomodoroModel pomodoroModel = pomodoroModel1;
          pomodoroModel.Tasks = await PomoTaskDao.GetByPomoId(pomodoroModel1.Id);
          pomodoroModel = (PomodoroModel) null;
        }
        TimerSyncService.ClearPulledKey();
        BaseSyncBean<PomodoroModel> baseSyncBean1;
        (baseSyncBean1, pomoDeleteList) = PomoSyncService.GetSyncArgs(needPostPomos, true);
        await PomoSyncService.CommitChanges(baseSyncBean1, true);
        await PomoSyncService.CommitDelete(pomoDeleteList, true);
        BaseSyncBean<PomodoroModel> baseSyncBean2;
        (baseSyncBean2, timingDeleteList) = PomoSyncService.GetSyncArgs(needPostPomos, false);
        await PomoSyncService.CommitChanges(baseSyncBean2, false);
        await PomoSyncService.CommitDelete(timingDeleteList, false);
        await PomoSyncService.PullStatistics();
        PomoNotifier.NotifyPomoCommit();
        needPostPomos = (List<PomodoroModel>) null;
        pomoDeleteList = (List<string>) null;
        timingDeleteList = (List<string>) null;
      }
    }

    private static (BaseSyncBean<PomodoroModel> syncBean, List<string> deleteList) GetSyncArgs(
      List<PomodoroModel> pomos,
      bool isPomo)
    {
      int pomoType = !isPomo ? 1 : 0;
      IEnumerable<PomodoroModel> source = pomos.Where<PomodoroModel>((Func<PomodoroModel, bool>) (pomo => pomo.Type == pomoType));
      return (new BaseSyncBean<PomodoroModel>(source.Where<PomodoroModel>((Func<PomodoroModel, bool>) (pomo => pomo.SyncStatus == 0)), source.Where<PomodoroModel>((Func<PomodoroModel, bool>) (pomo => pomo.SyncStatus == 1))), source.Where<PomodoroModel>((Func<PomodoroModel, bool>) (pomo => pomo.SyncStatus == -1)).Select<PomodoroModel, string>((Func<PomodoroModel, string>) (pomo => pomo.Id)).ToList<string>());
    }
  }
}
