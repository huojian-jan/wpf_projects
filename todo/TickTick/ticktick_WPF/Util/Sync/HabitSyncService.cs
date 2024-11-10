// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.HabitSyncService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.Util.Sync.Model;
using TickTickModels;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public static class HabitSyncService
  {
    private static readonly SemaphoreLocker SyncLock = new SemaphoreLocker();
    private static DateTime _lastSyncTime;
    private static DateTime _lastLaunchSyncTime;
    private static readonly DelayActionHandler _commitHabitHandler = new DelayActionHandler(2000);
    private static readonly DelayActionHandler _commitHabitCheckInsHandler = new DelayActionHandler(2000);

    static HabitSyncService()
    {
      HabitSyncService._commitHabitHandler.SetAction((EventHandler) ((sender, args) => HabitSyncService.BatchUpdateHabit()));
      HabitSyncService._commitHabitCheckInsHandler.SetAction((EventHandler) ((sender, args) => HabitSyncService.CommitHabitCheckInsInternal()));
    }

    public static async Task SyncHabitCheckInCalendar(DateTime startDate)
    {
      if (!LocalSettings.Settings.ShowHabit || !(startDate <= DateTime.Today))
        return;
      DateTime dateTime = startDate.AddDays(-1.0) >= DateTime.Today.AddDays(-31.0) ? DateTime.Today.AddDays(-31.0) : startDate.AddDays(-1.0);
      if (!LocalSettings.Settings.HabitInCal)
        return;
      HabitSyncService.PullHabits((int) (DateTime.Today - dateTime).TotalDays);
    }

    public static async Task SyncHabitsOnLogin()
    {
      if (!LocalSettings.Settings.ShowHabit)
        return;
      List<HabitModel> habits = await HabitService.PullRemoteHabits();
      if (habits != null)
      {
        List<string> habitIds = habits.Select<HabitModel, string>((Func<HabitModel, string>) (habit => habit.Id)).ToList<string>();
        if (habitIds.Any<string>())
        {
          int num = await HabitService.PullRemoteHabitsCheckIn(habitIds) ? 1 : 0;
          await HabitService.PullRemoteHabitsRecord(habitIds);
          DateTime dateTime = DateTime.Today;
          dateTime = dateTime.AddDays(-91.0);
          await HabitDao.SaveAllHabitCheckStamp(int.Parse(dateTime.ToString("yyyyMMdd")));
          foreach (HabitModel habitModel in habits)
            await HabitDao.UpdateHabitInfo(habitModel.Id, false);
        }
        habitIds = (List<string>) null;
      }
      habits = (List<HabitModel>) null;
    }

    public static async Task PullHabits(int days)
    {
      if (!LocalSettings.Settings.ShowHabit)
        return;
      List<HabitModel> habits = await HabitService.PullRemoteHabits(false);
      if (habits != null)
      {
        bool changed = await HabitService.MergeRemoteHabits(habits);
        List<string> habitIds = habits.Where<HabitModel>((Func<HabitModel, bool>) (habit => habit.Status == 0)).Select<HabitModel, string>((Func<HabitModel, string>) (habit => habit.Id)).ToList<string>();
        if (habitIds.Any<string>())
        {
          List<string> habitIds1 = habitIds;
          DateTime today = DateTime.Today;
          int dateNum1 = ticktick_WPF.Util.DateUtils.GetDateNum(today.AddDays((double) -days));
          changed = await HabitService.PullRemoteHabitsCheckIn(habitIds1, dateNum1) | changed;
          List<string> habitIds2 = habitIds;
          today = DateTime.Today;
          int dateNum2 = ticktick_WPF.Util.DateUtils.GetDateNum(today.AddDays((double) -days));
          await HabitService.PullRemoteHabitsRecord(habitIds2, dateNum2);
          HabitSyncService.CommitHabitCheckIns();
        }
        if (changed)
        {
          foreach (HabitModel habitModel in habits)
            await HabitDao.UpdateHabitInfo(habitModel.Id, false);
          Application.Current?.Dispatcher?.Invoke((Action) (() =>
          {
            DataChangedNotifier.NotifyHabitsChanged();
            if (ABTestManager.IsNewRemindCalculate())
              HabitReminderCalculator.InitHabitReminders();
            else
              ReminderCalculator.AssembleReminders();
          }));
        }
        habitIds = (List<string>) null;
      }
      habits = (List<HabitModel>) null;
    }

    public static async void SyncHabit()
    {
      HabitSyncService._lastLaunchSyncTime = DateTime.Now;
      HabitSyncService.SyncLock.LockAsync((Func<Task>) (async () =>
      {
        if (HabitSyncService._lastSyncTime >= HabitSyncService._lastLaunchSyncTime)
          return;
        HabitSyncService._lastSyncTime = DateTime.Now;
        try
        {
          await HabitSyncService.PullHabits(91);
        }
        catch (Exception ex)
        {
        }
        finally
        {
          Application.Current?.Dispatcher?.Invoke(new Action(DataChangedNotifier.NotifyHabitsSyncDone));
        }
      }));
    }

    private static async Task BatchUpdateHabit()
    {
      List<HabitModel> needPostHabits = await HabitDao.GetNeedPostHabits();
      SyncHabitBean syncBean = new SyncHabitBean();
      if (needPostHabits == null)
        syncBean = (SyncHabitBean) null;
      else if (!needPostHabits.Any<HabitModel>())
      {
        syncBean = (SyncHabitBean) null;
      }
      else
      {
        foreach (HabitModel habitModel in needPostHabits)
        {
          switch (habitModel.SyncStatus)
          {
            case -1:
              syncBean.Deleted.Add(habitModel.Id);
              continue;
            case 0:
              syncBean.Add.Add(habitModel);
              continue;
            case 1:
              syncBean.Update.Add(habitModel);
              continue;
            default:
              continue;
          }
        }
        if (syncBean.Empty())
        {
          syncBean = (SyncHabitBean) null;
        }
        else
        {
          string str = await Communicator.UpdateHabit(syncBean);
          if (str == null)
            syncBean = (SyncHabitBean) null;
          else if (!str.Contains("id2etag"))
          {
            syncBean = (SyncHabitBean) null;
          }
          else
          {
            try
            {
              BatchUpdateResult model = JsonConvert.DeserializeObject<BatchUpdateResult>(str);
              foreach (string key in model.Id2etag.Keys)
              {
                string etag = model.Id2etag[key];
                await HabitDao.SaveHabitEtag(key, etag);
              }
              foreach (string habitId in syncBean.Deleted)
              {
                HabitModel habitById = await HabitDao.GetHabitById(habitId);
                if (habitById != null)
                  App.Connection.DeleteAsync((object) habitById);
              }
              if (model.Id2error != null)
              {
                foreach (string key in model.Id2error.Keys)
                  ;
              }
              model = (BatchUpdateResult) null;
              syncBean = (SyncHabitBean) null;
            }
            catch (Exception ex)
            {
              syncBean = (SyncHabitBean) null;
            }
          }
        }
      }
    }

    public static async void CommitHabits(bool immediately = false)
    {
      if (immediately)
        HabitSyncService._commitHabitHandler.ImmediatelyDoAction();
      else
        HabitSyncService._commitHabitHandler.TryDoAction();
    }

    public static async Task CommitHabitRecords()
    {
      List<HabitRecordModel> records = await HabitRecordDao.GetNeedPostHabitRecords();
      if (!records.Any<HabitRecordModel>())
      {
        records = (List<HabitRecordModel>) null;
      }
      else
      {
        SyncHabitRecordBean bean = new SyncHabitRecordBean();
        List<HabitModel> allHabits = await HabitDao.GetAllHabits();
        List<HabitRecordModel> commitModels = new List<HabitRecordModel>();
        foreach (HabitRecordModel habitRecordModel in records)
        {
          HabitRecordModel record = habitRecordModel;
          HabitModel habitModel = allHabits != null ? allHabits.FirstOrDefault<HabitModel>((Func<HabitModel, bool>) (h => h.Id == record.HabitId)) : (HabitModel) null;
          if (habitModel == null || habitModel.Status == -1)
          {
            App.Connection.DeleteAsync((object) record);
          }
          else
          {
            commitModels.Add(record);
            switch (record.Status)
            {
              case 0:
                bean.Add.Add(record);
                continue;
              case 1:
                if (record.Deleted == 1)
                {
                  bean.Delete.Add(record);
                  continue;
                }
                bean.Update.Add(record);
                continue;
              default:
                continue;
            }
          }
        }
        string str = await Communicator.CommitHabitRecords(bean);
        if (str != null)
        {
          try
          {
            await HabitRecordDao.HandleRecordCommitResult(JsonConvert.DeserializeObject<BatchUpdateResult>(str), commitModels);
          }
          catch (Exception ex)
          {
            UtilLog.Warn("CommitHabitRecords Failed, " + ExceptionUtils.BuildExceptionMessage(ex));
          }
        }
        bean = (SyncHabitRecordBean) null;
        commitModels = (List<HabitRecordModel>) null;
        records = (List<HabitRecordModel>) null;
      }
    }

    public static void CommitHabitCheckIns(bool immediately = false)
    {
      if (immediately)
        HabitSyncService._commitHabitCheckInsHandler.ImmediatelyDoAction();
      else
        HabitSyncService._commitHabitCheckInsHandler.TryDoAction();
    }

    private static async Task CommitHabitCheckInsInternal()
    {
      List<HabitCheckInModel> checkIns = await HabitCheckInDao.GetNeedPostHabitCheckIns();
      if (checkIns == null)
        checkIns = (List<HabitCheckInModel>) null;
      else if (!checkIns.Any<HabitCheckInModel>())
      {
        checkIns = (List<HabitCheckInModel>) null;
      }
      else
      {
        UtilLog.Info("CommitCheckIns " + checkIns.Aggregate<HabitCheckInModel, string>("", (Func<string, HabitCheckInModel, string>) ((current, checkIn) => current + string.Format("{0},{1},{2},{3}\r\n", (object) checkIn.HabitId, (object) checkIn.Value, (object) checkIn.CheckStatus, (object) checkIn.CheckinStamp))));
        SyncHabitCheckInBean bean = new SyncHabitCheckInBean();
        HashSet<string> stringSet = new HashSet<string>();
        foreach (HabitCheckInModel model in checkIns)
        {
          if (model.Status != -1)
          {
            string str = model.HabitId + model.CheckinStamp;
            if (stringSet.Contains(str))
            {
              HabitCheckInDao.DeleteAsync(model);
              continue;
            }
            stringSet.Add(model.HabitId + model.CheckinStamp);
          }
          switch (model.Status)
          {
            case -1:
              bean.Delete.Add(model);
              continue;
            case 0:
              bean.Add.Add(model);
              continue;
            case 1:
              bean.Update.Add(model);
              continue;
            default:
              continue;
          }
        }
        string result = await Communicator.CommitHabitCheckIns(bean);
        UtilLog.Info("CommitCheckInsResult : " + result);
        if (result != null && result.Contains("id2etag"))
        {
          try
          {
            await HabitCheckInDao.HandleCommitResult(JsonConvert.DeserializeObject<BatchUpdateResult>(result), checkIns);
          }
          catch (Exception ex)
          {
            UtilLog.Warn("BatchUpdateHabitCheckIns Failed, " + ExceptionUtils.BuildExceptionMessage(ex));
          }
        }
        if (result != null && result.Contains("error"))
        {
          try
          {
            if (!string.IsNullOrEmpty(JsonConvert.DeserializeObject<ErrorModel>(result)?.errorId))
            {
              JToken jtoken;
              if (JObject.Parse(result).TryGetValue("data", out jtoken))
              {
                string habitId = jtoken.First?.First?.ToString();
                if (habitId != null)
                  await HabitDao.CheckHabitValid(habitId);
              }
            }
          }
          catch (Exception ex)
          {
            UtilLog.Warn("BatchUpdateHabitCheckIns Failed, " + ExceptionUtils.BuildExceptionMessage(ex));
          }
        }
        result = (string) null;
        checkIns = (List<HabitCheckInModel>) null;
      }
    }

    public static void DelaySync()
    {
      DelayActionHandlerCenter.TryDoAction("DelaySyncHabitsAndSections", (EventHandler) ((sender, args) =>
      {
        HabitSyncService.SyncHabit();
        Utils.RunOnUiThread(App.Window.Dispatcher, new Action(HabitSectionsSyncService.SyncSections));
      }), 3000);
    }

    public static async Task CommitHabitAndCheckIns()
    {
      await HabitSyncService.BatchUpdateHabit();
      HabitSyncService.CommitHabitRecords();
      await HabitSyncService.CommitHabitCheckInsInternal();
    }
  }
}
