// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.HabitRecordDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Habit;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Dal
{
  internal class HabitRecordDao
  {
    public static async Task<List<HabitRecordModel>> GetNeedPostHabitRecords()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitRecordModel>().Where((Expression<Func<HabitRecordModel, bool>>) (v => v.UserId == userId && (v.Status == 0 || v.Status == 1))).ToListAsync();
    }

    public static async Task HandleRecordCommitResult(
      BatchUpdateResult result,
      List<HabitRecordModel> records)
    {
      if (result == null || records == null || !records.Any<HabitRecordModel>())
        return;
      foreach (HabitRecordModel record in records)
      {
        if (result.Id2error != null && result.Id2error.ContainsKey(record.Id))
        {
          if (result.Id2error[record.Id] == "NOHABIT")
            UtilLog.Info("HabitRecord NOHABIT " + record.HabitId);
        }
        else
        {
          record.Status = 2;
          if (result.Id2etag == null || !result.Id2etag.ContainsKey(record.Id))
            UtilLog.Info("HabitRecord NoErrorAndEtag " + record.Id + " , habitId : " + record.HabitId);
        }
      }
      await HabitRecordDao.UpdateAllAsync(records);
    }

    public static async Task<List<HabitRecordModel>> GetHabitRecords(int checkStamp)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitRecordModel>().Where((Expression<Func<HabitRecordModel, bool>>) (v => v.UserId == userId && v.Deleted != 1 && v.Stamp >= checkStamp)).ToListAsync();
    }

    public static async Task<List<HabitRecordModel>> GetHabitRecordsByHabitId(
      string habitId,
      int stamp = 0)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitRecordModel>().Where((Expression<Func<HabitRecordModel, bool>>) (v => v.UserId == userId && v.HabitId == habitId && v.Deleted != 1 && v.Stamp > stamp)).ToListAsync();
    }

    public static async Task<HabitRecordModel> GetHabitRecordsByHabitIdAndDate(
      string habitId,
      int stamp)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitRecordModel>().Where((Expression<Func<HabitRecordModel, bool>>) (v => v.UserId == userId && v.HabitId == habitId && v.Stamp == stamp)).FirstOrDefaultAsync();
    }

    private static async Task<HabitRecordModel> GetHabitRecordByRecordId(string recordId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitRecordModel>().Where((Expression<Func<HabitRecordModel, bool>>) (v => v.UserId == userId && v.Id == recordId)).FirstOrDefaultAsync();
    }

    public static async Task SaveHabitCheckinRecord(string recordId, string content, int emoji)
    {
      HabitRecordModel recordByRecordId = await HabitRecordDao.GetHabitRecordByRecordId(recordId);
      if (recordByRecordId == null)
        return;
      recordByRecordId.Content = content;
      recordByRecordId.Emoji = emoji;
      if (recordByRecordId.Status != 0)
        recordByRecordId.Status = 1;
      recordByRecordId.opTime = new DateTime?(DateTime.Now);
      await HabitRecordDao.UpdateAsync(recordByRecordId);
    }

    private static async Task<HabitRecordModel> GetHabitRecordByHabitIdAndStamp(
      string habitId,
      int stamp)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitRecordModel>().Where((Expression<Func<HabitRecordModel, bool>>) (v => v.UserId == userId && v.HabitId == habitId && v.Stamp == stamp)).FirstOrDefaultAsync();
    }

    public static async Task DeleteHabitRecord(string logId)
    {
      HabitRecordModel recordByRecordId = await HabitRecordDao.GetHabitRecordByRecordId(logId);
      if (recordByRecordId == null)
        return;
      recordByRecordId.Deleted = 1;
      recordByRecordId.Status = 1;
      recordByRecordId.opTime = new DateTime?(DateTime.Now);
      await HabitRecordDao.UpdateAsync(recordByRecordId);
    }

    public static async Task AddCheckInRecord(CheckInLogViewModel vm)
    {
      string habitId = vm.HabitId;
      DateTime date = vm.Date;
      int stamp = int.Parse(date.ToString("yyyyMMdd"));
      HabitRecordModel byHabitIdAndStamp = await HabitRecordDao.GetHabitRecordByHabitIdAndStamp(habitId, stamp);
      if (byHabitIdAndStamp == null)
      {
        HabitRecordModel model = new HabitRecordModel();
        model.Id = Utils.GetGuid();
        model.Content = vm.Content;
        model.HabitId = vm.HabitId;
        date = vm.Date;
        model.Stamp = int.Parse(date.ToString("yyyyMMdd"));
        model.Status = 0;
        model.UserId = Utils.GetCurrentUserIdInt().ToString();
        model.Emoji = vm.Score;
        model.opTime = new DateTime?(DateTime.Now);
        await HabitRecordDao.InsertAsync(model);
      }
      else
        await HabitRecordDao.SaveHabitCheckinRecord(byHabitIdAndStamp.Id, vm.Content, vm.Score);
    }

    public static async Task InsertAsync(HabitRecordModel model)
    {
      int num = await App.Connection.InsertAsync((object) model);
    }

    public static async Task UpdateAsync(HabitRecordModel model)
    {
      int num = await App.Connection.UpdateAsync((object) model);
    }

    public static async Task UpdateAllAsync(List<HabitRecordModel> model)
    {
      int num = await App.Connection.UpdateAllAsync((IEnumerable) model);
    }

    public static async Task DeleteAsync(HabitRecordModel model)
    {
      int num = await App.Connection.DeleteAsync((object) model);
    }

    public static async Task DeleteRecordsByHabitId(string habitId)
    {
      List<HabitRecordModel> listAsync = await App.Connection.Table<HabitRecordModel>().Where((Expression<Func<HabitRecordModel, bool>>) (c => c.HabitId == habitId)).ToListAsync();
      // ISSUE: explicit non-virtual call
      if (listAsync == null || __nonvirtual (listAsync.Count) <= 0)
        return;
      foreach (HabitRecordModel model in listAsync)
        await HabitRecordDao.DeleteAsync(model);
    }
  }
}
