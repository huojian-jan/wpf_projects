// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.PomoSummaryDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickDao;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class PomoSummaryDao : BaseDao<PomodoroSummaryModel>
  {
    public static async Task DeletePomosByTaskId(string taskId)
    {
      List<PomodoroSummaryModel> listAsync = await App.Connection.Table<PomodoroSummaryModel>().Where((Expression<Func<PomodoroSummaryModel, bool>>) (v => v.taskId == taskId)).ToListAsync();
      if (listAsync == null || listAsync.Count <= 0)
        return;
      foreach (object obj in listAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }

    public static async Task CleanPomosByTaskId(string taskId)
    {
      UtilLog.Info("CleanPomos taskId: " + taskId);
      List<PomodoroSummaryModel> listAsync = await App.Connection.Table<PomodoroSummaryModel>().Where((Expression<Func<PomodoroSummaryModel, bool>>) (v => v.taskId == taskId)).ToListAsync();
      if (listAsync == null || listAsync.Count <= 0)
      {
        UtilLog.Info("CleanPomos pomos: null");
      }
      else
      {
        listAsync.ForEach((Action<PomodoroSummaryModel>) (pomo =>
        {
          pomo.count = 0;
          pomo.duration = 0L;
          pomo.PomoDuration = 0L;
          pomo.StopwatchDuration = 0L;
          pomo.focuses = (List<object[]>) null;
        }));
        UtilLog.Info("CleanPomos pomos: " + JsonConvert.SerializeObject((object) listAsync));
        int num = await App.Connection.UpdateAllAsync((IEnumerable) listAsync);
      }
    }

    public static async Task SavePomoSummaries(List<PomodoroSummaryModel> pomos)
    {
      int num = await App.Connection.InsertAllAsync((IEnumerable) pomos);
    }

    public static async Task SavePomoSummary(PomodoroSummaryModel pomo)
    {
      List<PomodoroSummaryModel> listAsync = await App.Connection.Table<PomodoroSummaryModel>().Where((Expression<Func<PomodoroSummaryModel, bool>>) (p => p.id == pomo.id)).ToListAsync();
      if (listAsync != null && listAsync.Count > 0 && !string.IsNullOrEmpty(pomo.id))
      {
        foreach (PomodoroSummaryModel pomodoroSummaryModel in listAsync)
        {
          pomodoroSummaryModel.count = pomo.count;
          pomodoroSummaryModel.PomoDuration = pomo.PomoDuration;
          pomodoroSummaryModel.estimatedPomo = pomo.estimatedPomo;
          pomodoroSummaryModel.EstimatedDuration = pomo.EstimatedDuration;
          pomodoroSummaryModel.StopwatchDuration = pomo.StopwatchDuration;
          pomodoroSummaryModel.taskId = pomo.taskId;
          pomodoroSummaryModel.userId = pomo.userId;
          pomodoroSummaryModel.focuses = pomo.focuses;
        }
        int num = await App.Connection.UpdateAllAsync((IEnumerable) listAsync);
      }
      else
      {
        if (string.IsNullOrEmpty(pomo.id))
          pomo.id = Utils.GetGuid();
        int num = await App.Connection.InsertAsync((object) pomo);
      }
    }

    public static async Task<List<PomodoroSummaryModel>> GetPomosByTaskId(string taskId)
    {
      return await App.Connection.Table<PomodoroSummaryModel>().Where((Expression<Func<PomodoroSummaryModel, bool>>) (v => v.taskId == taskId)).ToListAsync();
    }

    public static async Task<PomodoroSummaryModel> GetPomoByTaskId(string taskId)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      List<PomodoroSummaryModel> listAsync = await App.Connection.Table<PomodoroSummaryModel>().Where((Expression<Func<PomodoroSummaryModel, bool>>) (v => v.taskId == taskId && v.userId == userId)).ToListAsync();
      return listAsync == null || listAsync.Count != 1 ? (PomodoroSummaryModel) null : listAsync[0];
    }

    public static async Task<List<PomodoroSummaryModel>> GetPomosByTaskIds(List<string> taskIds)
    {
      return await App.Connection.Table<PomodoroSummaryModel>().Where((Expression<Func<PomodoroSummaryModel, bool>>) (v => taskIds.Contains(v.taskId))).ToListAsync();
    }
  }
}
