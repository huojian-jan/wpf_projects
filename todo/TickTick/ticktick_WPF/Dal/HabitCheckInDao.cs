// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.HabitCheckInDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class HabitCheckInDao
  {
    public static async Task HandleCommitResult(
      BatchUpdateResult result,
      List<HabitCheckInModel> checkIns)
    {
      if (result == null)
        return;
      List<HabitCheckInModel> habitCheckInModelList = checkIns;
      // ISSUE: explicit non-virtual call
      if ((habitCheckInModelList != null ? (__nonvirtual (habitCheckInModelList.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      List<HabitCheckInModel> updates = new List<HabitCheckInModel>();
      List<HabitCheckInModel> deletes = new List<HabitCheckInModel>();
      List<HabitCheckInModel> checkInsByIds = await HabitCheckInDao.GetCheckInsByIds(checkIns.Select<HabitCheckInModel, string>((Func<HabitCheckInModel, string>) (c => c.Id)).ToList<string>());
      if (checkInsByIds == null)
        return;
      foreach (HabitCheckInModel checkIn1 in checkIns)
      {
        HabitCheckInModel checkIn = checkIn1;
        HabitCheckInModel habitCheckInModel = checkInsByIds.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (l => l.Id == checkIn.Id));
        if (habitCheckInModel != null)
        {
          if (habitCheckInModel.Status == checkIn.Status && (checkIn.Status == 0 || checkIn.Status == 1))
          {
            checkIn.Status = 2;
            updates.Add(checkIn);
          }
          if (checkIn.Status == -1)
            deletes.Add(checkIn);
        }
      }
      if (updates.Any<HabitCheckInModel>())
        await HabitCheckInDao.UpdateAllAsync(checkIns);
      if (deletes.Any<HabitCheckInModel>())
      {
        foreach (HabitCheckInModel model in deletes)
          await HabitCheckInDao.DeleteAsync(model);
      }
      updates = (List<HabitCheckInModel>) null;
      deletes = (List<HabitCheckInModel>) null;
    }

    public static async Task<List<HabitCheckInModel>> GetNeedPostHabitCheckIns()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitCheckInModel>().Where((Expression<Func<HabitCheckInModel, bool>>) (v => v.UserId == userId && (v.Status == 0 || v.Status == 1 || v.Status == -1))).ToListAsync();
    }

    public static async Task<List<HabitCheckInModel>> GetCheckInsByIdsInSpan(
      List<string> ids,
      DateTime from,
      DateTime to)
    {
      try
      {
        List<string> stringList = ids;
        // ISSUE: explicit non-virtual call
        if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          List<string> stamps = new List<string>();
          for (int index = 0; (double) index <= (to - from).TotalDays; ++index)
            stamps.Add(from.AddDays((double) index).ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
          return await App.Connection.Table<HabitCheckInModel>().Where((Expression<Func<HabitCheckInModel, bool>>) (check => stamps.Contains(check.CheckinStamp) && check.UserId == LocalSettings.Settings.LoginUserId && ids.Contains(check.HabitId) && check.Status != -1)).ToListAsync();
        }
      }
      catch (Exception ex)
      {
        return (List<HabitCheckInModel>) null;
      }
      return (List<HabitCheckInModel>) null;
    }

    public static async Task<bool> HandleLocalDirtyCheckIns(string habitId)
    {
      List<HabitCheckInStamp> source1 = await App.Connection.QueryAsync<HabitCheckInStamp>("select checkinstamp as CheckInStamp from habitcheckinmodel where userid = '" + Utils.GetCurrentUserIdInt().ToString() + "' and habitid = '" + habitId + "' group by checkinstamp having count(1) > 1 ");
      List<HabitCheckInModel> dirtyCheckIns = new List<HabitCheckInModel>();
      if (source1 != null && source1.Any<HabitCheckInStamp>())
      {
        foreach (HabitCheckInStamp habitCheckInStamp in source1)
        {
          List<HabitCheckInModel> source2 = await App.Connection.QueryAsync<HabitCheckInModel>("select * from habitcheckinmodel where checkinstamp = '" + habitCheckInStamp.CheckInStamp + "' and habitid = '" + habitId + "'");
          if (source2.Any<HabitCheckInModel>() && source2.Count > 1)
          {
            for (int index = 1; index < source2.Count; ++index)
            {
              source2[index].Status = -1;
              dirtyCheckIns.Add(source2[index]);
            }
          }
        }
      }
      if (dirtyCheckIns.Any<HabitCheckInModel>())
      {
        int num = await App.Connection.UpdateAllAsync((IEnumerable) dirtyCheckIns);
      }
      bool flag = dirtyCheckIns.Any<HabitCheckInModel>();
      dirtyCheckIns = (List<HabitCheckInModel>) null;
      return flag;
    }

    private static async Task<List<HabitCheckInModel>> GetCheckInsByIds(List<string> ids)
    {
      try
      {
        List<string> stringList = ids;
        // ISSUE: explicit non-virtual call
        if ((stringList != null ? (__nonvirtual (stringList.Count) > 0 ? 1 : 0) : 0) != 0)
          return await App.Connection.Table<HabitCheckInModel>().Where((Expression<Func<HabitCheckInModel, bool>>) (check => check.UserId == LocalSettings.Settings.LoginUserId && ids.Contains(check.Id))).ToListAsync();
      }
      catch (Exception ex)
      {
        return (List<HabitCheckInModel>) null;
      }
      return (List<HabitCheckInModel>) null;
    }

    public static async Task<List<HabitCheckInModel>> GetHabitCheckInsByHabitIdAndStamp(
      string habitId,
      int stamp)
    {
      string str = Utils.GetCurrentUserIdInt().ToString();
      if (stamp > 0)
        return await App.Connection.QueryAsync<HabitCheckInModel>(string.Format("select * from HabitCheckinModel where userId = '{0}' and habitId = '{1}' and CheckinStamp > {2}", (object) str, (object) habitId, (object) stamp));
      return await App.Connection.QueryAsync<HabitCheckInModel>("select * from HabitCheckinModel where userId = '" + str + "' and habitId = '" + habitId + "'");
    }

    public static async Task<List<HabitCheckInModel>> GetHabitCheckInsByHabitId(string habitId)
    {
      string userId = Utils.GetCurrentUserStr();
      return await App.Connection.Table<HabitCheckInModel>().Where((Expression<Func<HabitCheckInModel, bool>>) (v => v.UserId == userId && v.HabitId == habitId && v.Status != -1)).ToListAsync();
    }

    public static async Task<HabitCheckInModel> GetHabitCheckInsByHabitIdAndStamp(
      string habitId,
      string stamp)
    {
      string userId = Utils.GetCurrentUserStr();
      return await App.Connection.Table<HabitCheckInModel>().Where((Expression<Func<HabitCheckInModel, bool>>) (v => v.UserId == userId && v.HabitId == habitId && v.CheckinStamp == stamp)).FirstOrDefaultAsync();
    }

    public static async Task<HabitCheckInModel> GetHabitCheckInByHabitIdAndStamp(
      string habitId,
      string stamp)
    {
      string userId = Utils.GetCurrentUserStr();
      return await App.Connection.Table<HabitCheckInModel>().Where((Expression<Func<HabitCheckInModel, bool>>) (v => v.UserId == userId && v.HabitId == habitId && v.CheckinStamp == stamp && v.Status != -1)).FirstOrDefaultAsync();
    }

    public static async Task<List<HabitCheckInModel>> GetHabitCheckInsByHabitIdInSpan(
      string habitId,
      DateTime startDate,
      DateTime endDate)
    {
      int num1 = int.Parse(startDate.ToString("yyyyMMdd"));
      int num2 = int.Parse(endDate.ToString("yyyyMMdd"));
      return await App.Connection.QueryAsync<HabitCheckInModel>(string.Format("select * from HabitCheckinModel where userId = '{0}' and habitId = '{1}' and CheckInStamp >= {2} and CheckInStamp <= {3} and Value >= 0 and Status <> -1", (object) Utils.GetCurrentUserIdInt().ToString(), (object) habitId, (object) num1, (object) num2));
    }

    public static async Task<List<HabitCheckInModel>> GetCheckInsInSpan(
      DateTime start,
      DateTime end)
    {
      int num1 = int.Parse(start.ToString("yyyyMMdd"));
      int num2 = int.Parse(end.ToString("yyyyMMdd"));
      return await App.Connection.QueryAsync<HabitCheckInModel>(string.Format("select * from HabitCheckinModel where userId = '{0}' and CheckinStamp >= {1} and CheckInStamp < {2} and Status <> -1", (object) Utils.GetCurrentUserIdInt().ToString(), (object) num1, (object) num2));
    }

    public static async Task<List<HabitCheckInModel>> GetCheckInsInWeek()
    {
      DateTime dateTime = DateTime.Today;
      dateTime = dateTime.AddDays((double) ((int) DateTime.Today.DayOfWeek * -1));
      dateTime = dateTime.AddDays((double) Utils.GetWeekFromDiff());
      string str = dateTime.ToString("yyyyMMdd");
      return await App.Connection.QueryAsync<HabitCheckInModel>("select * from HabitCheckinModel where userId = '" + Utils.GetCurrentUserIdInt().ToString() + "' and CheckinStamp >= " + str + " and Status <> -1");
    }

    public static async Task UpdateAsync(HabitCheckInModel model)
    {
      int num = await App.Connection.UpdateAsync((object) model);
    }

    public static async Task UpdateAllAsync(List<HabitCheckInModel> model)
    {
      int num = await App.Connection.UpdateAllAsync((IEnumerable) model);
    }

    public static async Task InsertAsync(HabitCheckInModel model)
    {
      int num = await App.Connection.InsertAsync((object) model);
    }

    public static async Task DeleteAsync(HabitCheckInModel model)
    {
      int num = await App.Connection.DeleteAsync((object) model);
    }

    public static async Task DeleteCheckInsByHabitId(string habitId)
    {
      List<HabitCheckInModel> listAsync = await App.Connection.Table<HabitCheckInModel>().Where((Expression<Func<HabitCheckInModel, bool>>) (c => c.HabitId == habitId)).ToListAsync();
      // ISSUE: explicit non-virtual call
      if (listAsync == null || __nonvirtual (listAsync.Count) <= 0)
        return;
      foreach (HabitCheckInModel model in listAsync)
        await HabitCheckInDao.DeleteAsync(model);
    }

    public static async Task<bool> ExistCheckInBefore(string habitId, DateTime date)
    {
      List<IdModel> idModelList = await App.Connection.QueryAsync<IdModel>("select id from HabitCheckInModel where CheckinStamp <= '" + date.ToString("yyyyMMdd") + "' and HabitId = '" + habitId + "' and (CheckStatus != 0 or Value > 0) limit 1");
      // ISSUE: explicit non-virtual call
      return idModelList != null && __nonvirtual (idModelList.Count) > 0;
    }
  }
}
