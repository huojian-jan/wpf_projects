// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.TimerService
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
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Pomo;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Service
{
  public class TimerService
  {
    public static async Task UpdateTimer(TimerModel timer)
    {
      if (await TimerDao.GetTimerById(timer.Id) == null)
      {
        int num1 = await BaseDao<TimerModel>.InsertAsync(timer);
      }
      else
      {
        if (timer.SyncStatus != 0)
          timer.SyncStatus = 1;
        int num2 = await BaseDao<TimerModel>.UpdateAsync(timer);
      }
    }

    public static async Task<long> GetNewTimerSortOrder()
    {
      return await TimerDao.GetMinSortOrder() - 134217728L;
    }

    public static async Task SaveSortOrder(string id, long sortOrder)
    {
      await TimerDao.SaveSortOrder(id, sortOrder);
      SyncManager.TryDelaySync(2000);
    }

    public static async Task SetTimerTodayDuration(List<FocusTimerViewModel> models)
    {
      List<PomoTask> todayPomoTasks = await PomoTaskDao.GetTodayPomoTasks();
      Dictionary<string, long> dict = new Dictionary<string, long>();
      // ISSUE: explicit non-virtual call
      if (todayPomoTasks != null && __nonvirtual (todayPomoTasks.Count) > 0)
      {
        foreach (PomoTask pomoTask1 in todayPomoTasks)
        {
          PomoTask pomoTask = pomoTask1;
          PomodoroModel pomoById = await PomoDao.GetPomoById(pomoTask.PomoId);
          if (pomoById != null && pomoById.SyncStatus != -1)
          {
            string entityId = pomoTask.GetEntityId();
            string key = !string.IsNullOrEmpty(entityId) ? entityId : pomoTask.TimerSid;
            if (!string.IsNullOrEmpty(key))
            {
              if (dict.ContainsKey(key))
                dict[key] += pomoTask.GetTodayDuration();
              else
                dict[key] = pomoTask.GetTodayDuration();
            }
            pomoTask = (PomoTask) null;
          }
        }
      }
      foreach (FocusTimerViewModel model in models)
      {
        long duration = 0;
        if (!string.IsNullOrEmpty(model.Id) && dict.ContainsKey(model.Id))
          duration += dict[model.Id];
        if (!string.IsNullOrEmpty(model.ObjId) && dict.ContainsKey(model.ObjId))
          duration += dict[model.ObjId];
        model.DurationText = Utils.GetShortDurationString(duration, false);
      }
      dict = (Dictionary<string, long>) null;
    }

    public static async Task<string> GetObjTitle(string objId, string objType)
    {
      switch (objType)
      {
        case "task":
          TaskBaseViewModel taskById = TaskCache.GetTaskById(objId);
          if (taskById != null)
            return taskById.Title ?? string.Empty;
          break;
        case "habit":
          string habitNameById = await HabitDao.GetHabitNameById(objId);
          if (habitNameById != null)
            return habitNameById;
          break;
      }
      return (string) null;
    }

    public static async Task UpdateName(string id, string title)
    {
      await TimerDao.SaveName(id, title);
    }

    public static async Task ChangeArchiveStatus(string id)
    {
      await TimerDao.ChangeArchiveStatus(id, await TimerService.GetNewTimerSortOrder());
      PomoNotifier.NotifyTimerChanged();
      SyncManager.TryDelaySync(2000);
    }

    public static async Task DeleteTimer(string id)
    {
      await TimerDao.SetDeleteStatus(id);
      PomoNotifier.NotifyTimerChanged();
      SyncManager.TryDelaySync(2000);
    }

    public static async Task SaveEtag(string timerId, string etag)
    {
      await TimerDao.SaveEtag(timerId, etag);
    }

    public static async Task<Dictionary<DateTime, double>> GetTimerRecord(
      string timerId,
      string objId,
      string objType,
      bool isNewAdd = false,
      DateTime? start = null,
      DateTime? end = null,
      bool delete = false)
    {
      List<PomoTask> pomoTaskOfTimer = await PomoTaskDao.GetPomoTaskOfTimer(timerId, objId, objType == "habit", start, end, isNewAdd, delete);
      // ISSUE: variable of a compiler-generated type
      TimerService.\u003C\u003Ec__DisplayClass9_0 cDisplayClass90;
      // ISSUE: reference to a compiler-generated field
      cDisplayClass90.dict = new Dictionary<DateTime, double>();
      if (pomoTaskOfTimer == null || pomoTaskOfTimer.Count == 0)
      {
        // ISSUE: reference to a compiler-generated field
        return cDisplayClass90.dict;
      }
      foreach (PomoTask pomoTask in pomoTaskOfTimer)
      {
        DateTime dateTime1 = pomoTask.StartTime;
        DateTime date1 = dateTime1.Date;
        DateTime endTime1 = pomoTask.EndTime;
        dateTime1 = pomoTask.StartTime;
        dateTime1 = dateTime1.Date;
        DateTime dateTime2 = dateTime1.AddDays(1.0);
        DateTime dateTime3;
        if (!(endTime1 > dateTime2))
        {
          dateTime3 = pomoTask.EndTime;
        }
        else
        {
          dateTime1 = pomoTask.StartTime;
          dateTime1 = dateTime1.Date;
          dateTime3 = dateTime1.AddDays(1.0);
        }
        DateTime startTime = pomoTask.StartTime;
        TimeSpan timeSpan = dateTime3 - startTime;
        double totalMinutes1 = timeSpan.TotalMinutes;
        ref TimerService.\u003C\u003Ec__DisplayClass9_0 local1 = ref cDisplayClass90;
        TimerService.\u003CGetTimerRecord\u003Eg__AddToDict\u007C9_0(date1, totalMinutes1, ref local1);
        DateTime endTime2 = pomoTask.EndTime;
        dateTime1 = pomoTask.StartTime;
        dateTime1 = dateTime1.Date;
        DateTime dateTime4 = dateTime1.AddDays(1.0);
        if (endTime2 > dateTime4)
        {
          dateTime1 = pomoTask.EndTime;
          DateTime date2 = dateTime1.Date;
          DateTime endTime3 = pomoTask.EndTime;
          dateTime1 = pomoTask.StartTime;
          dateTime1 = dateTime1.Date;
          DateTime dateTime5 = dateTime1.AddDays(1.0);
          timeSpan = endTime3 - dateTime5;
          double totalMinutes2 = timeSpan.TotalMinutes;
          ref TimerService.\u003C\u003Ec__DisplayClass9_0 local2 = ref cDisplayClass90;
          TimerService.\u003CGetTimerRecord\u003Eg__AddToDict\u007C9_0(date2, totalMinutes2, ref local2);
        }
      }
      // ISSUE: reference to a compiler-generated field
      return cDisplayClass90.dict;
    }

    public static string GetTimerStatisticsKey(
      string timerId,
      DateTime start,
      DateTime end,
      string interval)
    {
      string str1 = timerId;
      int dateNum = DateUtils.GetDateNum(start);
      string str2 = dateNum.ToString();
      dateNum = DateUtils.GetDateNum(end);
      string str3 = dateNum.ToString();
      string str4 = interval;
      return str1 + str2 + str3 + str4;
    }

    public static async Task<Dictionary<string, long>> CalTimerStatistics(
      TimerModel timer,
      DateTime start,
      DateTime end,
      string interval,
      bool isNewAdd = false)
    {
      Dictionary<DateTime, double> timerRecord = await TimerService.GetTimerRecord(timer.Id, timer.ObjId, timer.ObjType, isNewAdd, new DateTime?(start), new DateTime?(end));
      Dictionary<string, long> dictionary = new Dictionary<string, long>();
      for (int index = 0; index < (end - start).Days; ++index)
      {
        DateTime dateTime = start.AddDays((double) index);
        int dateNum = DateUtils.GetDateNum(dateTime);
        string str1 = dateNum.ToString();
        string str2;
        if (!(interval == "year"))
        {
          str2 = str1;
        }
        else
        {
          dateNum = DateUtils.GetDateNum(dateTime.AddDays((double) (1 - dateTime.Day)));
          str2 = dateNum.ToString();
        }
        string key = str2;
        if (timerRecord.ContainsKey(dateTime))
        {
          if (dictionary.ContainsKey(key))
            dictionary[key] += (long) timerRecord[dateTime];
          else
            dictionary[key] = (long) timerRecord[dateTime];
        }
      }
      return dictionary;
    }

    public static async Task<Dictionary<string, long>> GetTimerStatistics(
      TimerModel timer,
      DateTime start,
      DateTime end,
      string interval)
    {
      string timerStatisticsKey = TimerService.GetTimerStatisticsKey(timer.Id, start, end, interval);
      Dictionary<string, long> statistics;
      try
      {
        statistics = JsonConvert.DeserializeObject<Dictionary<string, long>>((await TimerDao.GetStatistics(timerStatisticsKey))?.Value ?? "") ?? new Dictionary<string, long>();
      }
      catch (Exception ex)
      {
        statistics = new Dictionary<string, long>();
      }
      await HandleDeletedOrAdded(false);
      await HandleDeletedOrAdded(true);
      return statistics;

      async Task HandleDeletedOrAdded(bool delete)
      {
        Dictionary<DateTime, double> timerRecord = await TimerService.GetTimerRecord(timer.Id, timer.ObjId, timer.ObjType, !delete, new DateTime?(start), new DateTime?(end), delete);
        Dictionary<string, long> dictionary = new Dictionary<string, long>();
        foreach (DateTime key1 in timerRecord.Keys)
        {
          int dateNum;
          string str;
          if (!(interval == "year"))
          {
            dateNum = DateUtils.GetDateNum(key1);
            str = dateNum.ToString();
          }
          else
          {
            dateNum = DateUtils.GetDateNum(key1.AddDays((double) (1 - key1.Day)));
            str = dateNum.ToString();
          }
          string key2 = str;
          if (dictionary.ContainsKey(key2))
            dictionary[key2] += (long) timerRecord[key1];
          else
            dictionary[key2] = (long) timerRecord[key1];
        }
        foreach (string key in dictionary.Keys)
        {
          if (statistics.ContainsKey(key))
          {
            long val2 = statistics[key] + (delete ? -1L : 1L) * dictionary[key];
            statistics[key] = Math.Max(0L, val2);
          }
          else if (!delete)
            statistics[key] = dictionary[key];
        }
      }
    }

    public static async Task<TimerOverviewModel> GetTimerOverView(TimerModel timer)
    {
      TimerOverviewModel overview = (string.IsNullOrEmpty(timer.Overview) ? (TimerOverviewModel) null : JsonConvert.DeserializeObject<TimerOverviewModel>(timer.Overview)) ?? new TimerOverviewModel();
      if (overview.Date < DateUtils.GetDateNum(DateTime.Today))
        overview.Today = 0L;
      Dictionary<DateTime, double> timerRecord1 = await TimerService.GetTimerRecord(timer.Id, timer.ObjId, timer.ObjType, true);
      overview.Add(new TimerOverviewModel()
      {
        Days = timerRecord1.Keys.Count,
        Today = timerRecord1.ContainsKey(DateTime.Today) ? (long) timerRecord1[DateTime.Today] : 0L,
        Total = (long) timerRecord1.Values.ToList<double>().Sum()
      });
      Dictionary<DateTime, double> timerRecord2 = await TimerService.GetTimerRecord(timer.Id, timer.ObjId, timer.ObjType, delete: true);
      overview.Cut(new TimerOverviewModel()
      {
        Days = timerRecord2.Keys.Count,
        Today = timerRecord2.ContainsKey(DateTime.Today) ? (long) timerRecord2[DateTime.Today] : 0L,
        Total = (long) timerRecord2.Values.ToList<double>().Sum()
      });
      TimerOverviewModel timerOverView = overview;
      overview = (TimerOverviewModel) null;
      return timerOverView;
    }

    public static async Task TryUpdateTimerIcon(string habitId, string icon, string color)
    {
      TimerModel timerByObjId = await TimerDao.GetTimerByObjId(habitId);
      if (timerByObjId == null || !(timerByObjId.Icon != icon) && !(timerByObjId.Color != color))
        return;
      timerByObjId.Icon = icon;
      timerByObjId.Color = color;
      timerByObjId.ModifiedTime = DateTime.Now;
      timerByObjId.SyncStatus = timerByObjId.SyncStatus != 0 ? 1 : 0;
      int num = await BaseDao<TimerModel>.UpdateAsync(timerByObjId);
      SyncManager.TryDelaySync();
    }
  }
}
