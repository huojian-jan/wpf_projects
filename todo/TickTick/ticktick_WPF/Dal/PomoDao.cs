// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.PomoDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickDao;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class PomoDao : BaseDao<PomodoroModel>
  {
    public static string GetPomoSql(int? type = null, bool withDelete = true)
    {
      string pomoSql = "Select _Id as _Id,Id as Id,TaskSid as TaskSid,Status as Status,StartTime as StartTime,EndTime as EndTime,SyncStatus as SyncStatus , Etag as Etag,UserId as UserId,Added as Added,Type as Type,Note as Note,PauseDuration as PauseDuration from PomodoroModel " + " Where UserId = '" + LocalSettings.Settings.LoginUserId + "'";
      if (!withDelete)
        pomoSql += string.Format(" and SyncStatus != {0} ", (object) -1);
      if (type.HasValue)
        pomoSql += string.Format(" and Type = {0} ", (object) type);
      return pomoSql;
    }

    public static async Task<List<PomodoroModel>> GetAllPomo(bool containDeleted = false)
    {
      return await App.Connection.QueryAsync<PomodoroModel>(PomoDao.GetPomoSql(withDelete: containDeleted));
    }

    public static async Task<List<PomodoroModel>> GetPomoDescByStart(int limit)
    {
      return await App.Connection.QueryAsync<PomodoroModel>(PomoDao.GetPomoSql(withDelete: false) + " order by StartTime  desc " + string.Format(" limit {0} ", (object) limit));
    }

    public static async Task<List<PomodoroModel>> GetPomoDescByStartAndTimerId(
      int limit,
      string timerId,
      string objId,
      bool isHabit,
      DateTime startTime,
      DateTime endTime)
    {
      string str1 = "";
      if (!string.IsNullOrEmpty(objId))
        str1 = isHabit ? " or b.HabitId = '" + objId + "' " : " or b.TaskId = '" + objId + "' ";
      string str2 = startTime.ToString("yyyy'-'MM'-'dd HH':'mm':'ss");
      string str3 = endTime.ToString("yyyy'-'MM'-'dd HH':'mm':'ss");
      string sql = "Select distinct a.Id as Id,a.Status as Status,a.StartTime as StartTime,a.EndTime as EndTime,a.SyncStatus as SyncStatus , a.Type as Type,a.Note as Note,a.PauseDuration as PauseDuration from PomodoroModel a,PomoTask b " + string.Format("where a.Id = b.PomoId and a.UserId = '{0}' and ( b.TimerSid = '{1}' {2} ) and a.SyncStatus != {3} and ", (object) LocalSettings.Settings.LoginUserId, (object) timerId, (object) str1, (object) -1) + "(b.StartTime >= '" + str2 + "' and b.StartTime < '" + str3 + "' or b.EndTime > '" + str2 + "' and b.EndTime <= '" + str3 + "')" + " order by StartTime  desc ";
      if (limit != 0)
        sql += string.Format(" limit {0} ", (object) limit);
      return await App.Connection.QueryAsync<PomodoroModel>(sql);
    }

    public static async Task<List<PomodoroModel>> GetPomoByDateSpan(
      DateTime startTime,
      DateTime endTime,
      bool containDeleted = false)
    {
      if (startTime > endTime)
        return new List<PomodoroModel>();
      string pomoSql = PomoDao.GetPomoSql(withDelete: containDeleted);
      try
      {
        return (await App.Connection.QueryAsync<PomodoroModel>(pomoSql)).Where<PomodoroModel>((Func<PomodoroModel, bool>) (p =>
        {
          if (p.StartTime >= startTime && p.StartTime < endTime || p.EndTime > startTime && p.EndTime <= endTime)
            return true;
          return p.StartTime <= startTime && p.EndTime >= endTime;
        })).ToList<PomodoroModel>();
      }
      catch (Exception ex)
      {
        UtilLog.Info(string.Format("GetPomoByDateSpanError start: {0}, end: {1}, ci: {2}", (object) startTime.ToString((IFormatProvider) App.Ci), (object) endTime.ToString((IFormatProvider) App.Ci), (object) Thread.CurrentThread.CurrentCulture));
        throw;
      }
    }

    public static async Task<PomodoroModel> GetPomoById(string pomoId)
    {
      List<PomodoroModel> source = await App.Connection.QueryAsync<PomodoroModel>(PomoDao.GetPomoSql() + " and Id = '" + pomoId + "'");
      return source != null ? source.FirstOrDefault<PomodoroModel>() : (PomodoroModel) null;
    }

    public static async Task<List<PomodoroModel>> GetPomoInIds(List<string> pomoIds)
    {
      if (pomoIds == null)
        return new List<PomodoroModel>();
      List<PomodoroModel> source = await App.Connection.QueryAsync<PomodoroModel>(PomoDao.GetPomoSql());
      return source != null ? source.Where<PomodoroModel>((Func<PomodoroModel, bool>) (p => pomoIds.Contains(p.Id))).ToList<PomodoroModel>() : (List<PomodoroModel>) null;
    }

    public static async Task<List<PomodoroModel>> GetNeedPostPomos()
    {
      List<PomodoroModel> pomodoroModelList = await App.Connection.QueryAsync<PomodoroModel>(PomoDao.GetPomoSql() + string.Format(" and SyncStatus != {0}", (object) 2));
      Dictionary<string, PomodoroModel> dictionary = new Dictionary<string, PomodoroModel>();
      foreach (PomodoroModel pomodoroModel in pomodoroModelList)
      {
        if (dictionary.ContainsKey(pomodoroModel.Id) || Utils.IsEmptyDate(pomodoroModel.StartTime))
          App.Connection.DeleteAsync((object) pomodoroModel);
        else
          dictionary[pomodoroModel.Id] = pomodoroModel;
      }
      return dictionary.Values.ToList<PomodoroModel>();
    }

    public static async Task<List<PomodoroModel>> GetPomosByType(int? type)
    {
      try
      {
        return await App.Connection.QueryAsync<PomodoroModel>(PomoDao.GetPomoSql(type, false)) ?? new List<PomodoroModel>();
      }
      catch (Exception ex)
      {
        return new List<PomodoroModel>();
      }
    }

    public static async Task UpdateAllAsync(IEnumerable<PomodoroModel> pomos)
    {
      int num = await App.Connection.UpdateAllAsync((IEnumerable) pomos);
    }

    public static async Task UpdateAsync(PomodoroModel pomo)
    {
      int num = await App.Connection.UpdateAsync((object) pomo);
    }

    public static async Task DeleteAllAsync(IEnumerable<PomodoroModel> pomos)
    {
      foreach (BaseModel pomo in pomos)
      {
        int num = await App.Connection.ExecuteAsync(string.Format("DELETE FROM {0} WHERE _Id={1}", (object) "PomodoroModel", (object) pomo._Id));
      }
    }

    public static async Task<int> DeleteByIdAsync(string pomoId)
    {
      return await App.Connection.ExecuteAsync("DELETE FROM PomodoroModel WHERE Id=?", (object) pomoId);
    }

    public static async Task DeleteAsync(PomodoroModel pomo)
    {
      int num = await App.Connection.DeleteAsync((object) pomo);
    }

    public static async Task InsertAllAsync(IEnumerable<PomodoroModel> pomos)
    {
      int num = await App.Connection.InsertAllAsync((IEnumerable) pomos);
    }

    public static async Task InsertAsync(PomodoroModel pomo)
    {
      if (string.IsNullOrEmpty(pomo.UserId))
        pomo.UserId = Utils.GetCurrentUserStr();
      int num = await App.Connection.InsertAsync((object) pomo);
    }

    public static async Task<List<PomoTask>> GetPomoTasksByPomoId(string pomoId)
    {
      return await App.Connection.Table<PomoTask>().Where((Expression<Func<PomoTask, bool>>) (p => p.PomoId == pomoId)).ToListAsync();
    }

    public static async Task<PomoTask> GetPomoTaskByPomoIdAndTime(string pomoId, DateTime start)
    {
      return await App.Connection.Table<PomoTask>().Where((Expression<Func<PomoTask, bool>>) (p => p.PomoId == pomoId && p.StartTime == start)).FirstOrDefaultAsync();
    }

    public static async Task UpdatePomoTask(PomoTask pomoTask)
    {
      int num = await App.Connection.UpdateAsync((object) pomoTask);
    }

    public static async Task UpdatePomo(PomodoroModel pomo)
    {
      if (pomo.SyncStatus == 2)
        pomo.SyncStatus = 1;
      await PomoDao.UpdateAsync(pomo);
    }
  }
}
