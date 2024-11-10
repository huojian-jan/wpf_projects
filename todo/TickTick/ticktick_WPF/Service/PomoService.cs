// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.PomoService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickDao;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service
{
  public class PomoService
  {
    public static async Task DeleteById(string pomoId)
    {
      PomodoroModel pomo = await PomoDao.GetPomoById(pomoId);
      if (pomo == null)
      {
        pomo = (PomodoroModel) null;
      }
      else
      {
        if (LocalSettings.Settings.StatisticsModel != null)
        {
          int num1 = (int) ((pomo.EndTime - pomo.StartTime).TotalSeconds - (double) pomo.PauseDuration) / 60;
          int num2 = pomo.Type != 1 ? 1 : 0;
          StatisticsModel extra = new StatisticsModel();
          extra.totalPomoCount -= num2;
          extra.totalPomoDuration -= (long) num1;
          if (pomo.EndTime > DateTime.Today)
          {
            extra.todayPomoCount -= num2;
            extra.todayPomoDuration -= (long) num1;
          }
          LocalSettings.Settings.StatisticsModel.Add(extra);
        }
        pomo.SyncStatus = -1;
        await PomoDao.UpdateAsync(pomo);
        await TaskService.DeletePomodoro(pomoId, ((IEnumerable<PomoTask>) await PomoTaskDao.GetByPomoId(pomo.Id)).ToList<PomoTask>(), pomo.Type == 0, true);
        PomoNotifier.NotifyChanged(pomo.Id, PomoChangeType.Deleted);
        SyncManager.TryDelaySync();
        pomo = (PomodoroModel) null;
      }
    }

    public static async Task<PomoStat> LoadStatistics(bool isPomo)
    {
      PomoStat statInfo = new PomoStat();
      List<PomodoroModel> pomosByType = await PomoDao.GetPomosByType(new int?());
      List<PomodoroModel> list1 = pomosByType.Where<PomodoroModel>((Func<PomodoroModel, bool>) (p => p.EndTime.Date == DateTime.Today)).ToList<PomodoroModel>();
      DateTime weekStart = Utils.GetWeekStart(DateTime.Today);
      List<PomodoroModel> list2 = pomosByType.Where<PomodoroModel>((Func<PomodoroModel, bool>) (p => p.EndTime.Date <= DateTime.Today && p.EndTime.Date >= weekStart)).ToList<PomodoroModel>();
      statInfo.TodayPomos = list1.Count<PomodoroModel>((Func<PomodoroModel, bool>) (p => p.Type == 0 && p.StartTime.Date == DateTime.Today));
      statInfo.WeeklyPomos = list2.Count<PomodoroModel>((Func<PomodoroModel, bool>) (p => p.Type == 0 && p.StartTime.Date >= weekStart));
      statInfo.TodayDuration = 0L;
      list1.ForEach((Action<PomodoroModel>) (p =>
      {
        long num = Utils.GetTotalSecond(p.StartTime > DateTime.Today ? p.StartTime : DateTime.Today, p.EndTime) - p.PauseDuration;
        if (num <= 0L)
          return;
        statInfo.TodayDuration += num;
      }));
      statInfo.WeeklyDuration = 0L;
      list2.ForEach((Action<PomodoroModel>) (p =>
      {
        long num = Utils.GetTotalSecond(p.StartTime > weekStart ? p.StartTime : weekStart, p.EndTime) - p.PauseDuration;
        if (num <= 0L)
          return;
        statInfo.WeeklyDuration += num;
      }));
      return statInfo;
    }

    public static async Task RebindTaskIdOfPomoTask(
      string pomoId,
      DateTime startDate,
      string newTaskId,
      string newHabitId,
      TimerModel timer)
    {
      PomodoroModel pomo = await PomoDao.GetPomoById(pomoId);
      List<PomoTask> pomoTasks;
      PomoTask pomoTask;
      TimerModel originTimer;
      if (pomo == null)
      {
        pomo = (PomodoroModel) null;
        pomoTasks = (List<PomoTask>) null;
        pomoTask = (PomoTask) null;
        originTimer = (TimerModel) null;
      }
      else
      {
        pomoTasks = await PomoDao.GetPomoTasksByPomoId(pomoId);
        if (pomoTasks == null || pomoTasks.Count == 0)
        {
          PomoTask task = new PomoTask()
          {
            StartTime = startDate,
            EndTime = pomo.EndTime,
            PomoId = pomoId
          };
          await PomoTaskDao.InsertAsync(task);
          pomoTasks = new List<PomoTask>() { task };
          task = (PomoTask) null;
        }
        List<PomoTask> source = pomoTasks;
        pomoTask = source != null ? source.FirstOrDefault<PomoTask>((Func<PomoTask, bool>) (p => p.StartTime == startDate)) : (PomoTask) null;
        if (pomoTask == null)
        {
          pomo = (PomodoroModel) null;
          pomoTasks = (List<PomoTask>) null;
          pomoTask = (PomoTask) null;
          originTimer = (TimerModel) null;
        }
        else if (!string.IsNullOrEmpty(newTaskId) && TaskCache.GetTaskById(newTaskId) == null)
        {
          pomo = (PomodoroModel) null;
          pomoTasks = (List<PomoTask>) null;
          pomoTask = (PomoTask) null;
          originTimer = (TimerModel) null;
        }
        else
        {
          TimerModel timerModel;
          if (!string.IsNullOrEmpty(pomoTask.TimerSid))
            timerModel = await TimerDao.GetTimerById(pomoTask.TimerSid);
          else
            timerModel = await TimerDao.GetTimerByObjId(string.IsNullOrEmpty(pomoTask.TaskId) ? pomoTask.HabitId : pomoTask.TaskId);
          originTimer = timerModel;
          if (!string.IsNullOrEmpty(newTaskId) || !string.IsNullOrEmpty(pomoTask.TaskId))
            await TaskService.UpdateTaskPomoSummary(pomoTask.TaskId, newTaskId, pomoTask, pomoTasks, pomo);
          if (!string.IsNullOrEmpty(newHabitId))
          {
            HabitModel habitById = await HabitDao.GetHabitById(newHabitId);
            if (habitById == null)
            {
              pomo = (PomodoroModel) null;
              pomoTasks = (List<PomoTask>) null;
              pomoTask = (PomoTask) null;
              originTimer = (TimerModel) null;
              return;
            }
            pomoTask.ProjectName = string.Empty;
            pomoTask.TagString = string.Empty;
            pomoTask.Title = habitById.Name;
            await HabitService.OnPomoTaskChanged(pomoTask.HabitId, Utils.GetTotalSecond(pomoTask.StartTime, pomoTask.EndTime), pomoTask.StartTime.Date);
          }
          pomoTask.TaskId = newTaskId;
          pomoTask.HabitId = newHabitId;
          pomoTask.TimerSid = timer?.Id;
          pomoTask.TimerName = timer?.Name;
          if (string.IsNullOrEmpty(newTaskId) && string.IsNullOrEmpty(newHabitId))
          {
            pomoTask.Tags = (string[]) null;
            pomoTask.TagString = (string) null;
            pomoTask.ProjectName = (string) null;
            pomoTask.Title = timer?.Name;
          }
          await PomoDao.UpdatePomoTask(pomoTask);
          PomoNotifier.NotifyLinkChanged(originTimer?.Id, timer?.Id);
          if (pomo.SyncStatus == 2)
            pomo.SyncStatus = 1;
          if (Utils.IsEmptyDate(pomo.StartTime))
          {
            UtilLog.Info("PomoService.RebindTaskIdOfPomoTask : " + pomo.Id);
            pomo = (PomodoroModel) null;
            pomoTasks = (List<PomoTask>) null;
            pomoTask = (PomoTask) null;
            originTimer = (TimerModel) null;
          }
          else
          {
            await PomoDao.UpdateAsync(pomo);
            SyncManager.TryDelaySync();
            pomo = (PomodoroModel) null;
            pomoTasks = (List<PomoTask>) null;
            pomoTask = (PomoTask) null;
            originTimer = (TimerModel) null;
          }
        }
      }
    }

    public static async Task SaveNote(string pomoId, string note)
    {
      PomodoroModel pomoById = await PomoDao.GetPomoById(pomoId);
      if (pomoById == null || pomoById.Note == note)
        return;
      pomoById.Note = note;
      if (pomoById.SyncStatus == 2)
        pomoById.SyncStatus = 1;
      if (Utils.IsEmptyDate(pomoById.StartTime))
      {
        UtilLog.Info("PomoService.SaveNote : " + pomoById.Id);
      }
      else
      {
        await PomoDao.UpdateAsync(pomoById);
        SyncManager.TryDelaySync();
      }
    }

    public static async Task SaveFocusModel(PomodoroModel pomo)
    {
      PomoTask[] tasks1 = pomo.Tasks;
      if ((tasks1 != null ? (tasks1.Length != 0 ? 1 : 0) : 0) != 0)
      {
        List<PomoTask> tasks = ((IEnumerable<PomoTask>) pomo.Tasks).ToList<PomoTask>();
        int num = await App.Connection.InsertAllAsync((IEnumerable) tasks);
        if (tasks.Count > 0)
          await TaskService.UpdatePomodoro(pomo.Id, tasks.Where<PomoTask>((Func<PomoTask, bool>) (po => !string.IsNullOrEmpty(po.TaskId) || !string.IsNullOrEmpty(po.HabitId))).ToList<PomoTask>(), pomo.Type == 0);
        tasks = (List<PomoTask>) null;
      }
      if (LocalSettings.Settings.StatisticsModel != null)
      {
        int num1 = pomo.Type != 1 ? 1 : 0;
        int num2 = (int) ((pomo.EndTime - pomo.StartTime).TotalSeconds - (double) pomo.PauseDuration) / 60;
        StatisticsModel extra = new StatisticsModel();
        extra.totalPomoCount += num1;
        extra.totalPomoDuration += (long) num2;
        if (pomo.EndTime > DateTime.Today)
        {
          extra.todayPomoCount += num1;
          extra.todayPomoDuration += (long) num2;
        }
        LocalSettings.Settings.StatisticsModel.Add(extra);
      }
      await PomoDao.InsertAsync(pomo);
      UtilLog.Info("SaveFocusModel: " + pomo.Id + "," + pomo.StartTime.ToString() + "," + pomo.EndTime.ToString() + "," + pomo.Tasks?.Length.ToString());
      PomoNotifier.NotifyChanged(pomo.Id, PomoChangeType.Added);
      TaskChangeNotifier.NotifyTaskPomoAdded(new List<string>()
      {
        pomo.Id
      });
      SyncManager.TryDelaySync(1200);
    }

    public static StatisticsModel GetStatisticsModel(List<PomodoroModel> models, bool checkDelete)
    {
      StatisticsModel statisticsModel = new StatisticsModel();
      double num1 = 0.0;
      double num2 = 0.0;
      foreach (PomodoroModel model in models)
      {
        if (model.SyncStatus != 1)
        {
          int num3 = !checkDelete || model.SyncStatus != -1 ? 1 : -1;
          if (model.Type == 0)
          {
            statisticsModel.totalPomoCount += num3;
            if (model.EndTime > DateTime.Today)
              statisticsModel.todayPomoCount += num3;
          }
          double num4 = (double) num3;
          TimeSpan timeSpan = model.EndTime - model.StartTime;
          double totalMinutes1 = timeSpan.TotalMinutes;
          double num5 = num4 * totalMinutes1;
          num1 += num5;
          if (model.EndTime > DateTime.Today)
          {
            double num6 = num2;
            double num7;
            if (!(model.StartTime >= DateTime.Today))
            {
              double num8 = (double) num3;
              timeSpan = model.EndTime - DateTime.Today;
              double totalMinutes2 = timeSpan.TotalMinutes;
              num7 = num8 * totalMinutes2;
            }
            else
              num7 = num5;
            num2 = num6 + num7;
          }
        }
      }
      statisticsModel.todayPomoDuration = (long) (int) Math.Round(num2, 0, MidpointRounding.AwayFromZero);
      statisticsModel.totalPomoDuration = (long) (int) Math.Round(num1, 0, MidpointRounding.AwayFromZero);
      return statisticsModel;
    }

    public static async Task RemovePomoTaskTimerId(string timerId)
    {
      string query = "Update TimerModel Set TimerSid = null where TimerSid = '" + timerId + "'";
      int num = await Connection.DbConnection.ExecuteAsync(query);
    }
  }
}
