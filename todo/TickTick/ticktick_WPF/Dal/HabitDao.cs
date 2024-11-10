// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.HabitDao
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
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class HabitDao
  {
    public static async Task<List<HabitModel>> GetNeedCheckHabits()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<HabitModel> list = (await App.Connection.Table<HabitModel>().Where((Expression<Func<HabitModel, bool>>) (v => v.UserId == userId && v.Status == 0)).OrderBy<long>((Expression<Func<HabitModel, long>>) (v => v.SortOrder)).ToListAsync()).Where<HabitModel>((Func<HabitModel, bool>) (habit => !habit.IsSkipToday())).ToList<HabitModel>();
      foreach (HabitModel habit in list)
        HabitDao.HandleHabitValue(habit);
      return list;
    }

    private static void HandleHabitValue(HabitModel habit)
    {
      if (!(habit.Unit == "Count") || !Utils.IsCn())
        return;
      habit.Unit = Utils.GetString("Count");
    }

    public static async Task<List<HabitModel>> GetAllHabits()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitModel>().Where((Expression<Func<HabitModel, bool>>) (v => v.UserId == userId)).OrderBy<long>((Expression<Func<HabitModel, long>>) (it => it.SortOrder)).ToListAsync();
    }

    public static async Task<List<HabitModel>> GetHabitsbyStatus(int status)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<HabitModel>().Where((Expression<Func<HabitModel, bool>>) (v => v.UserId == userId && v.Status == status)).ToListAsync();
    }

    public static async Task<List<HabitModel>> GetHabitsBySectionId(string sectionId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<HabitModel> listAsync;
      if (sectionId == HabitSectionModel.GetDefault().Id || string.IsNullOrEmpty(sectionId))
        listAsync = await App.Connection.Table<HabitModel>().Where((Expression<Func<HabitModel, bool>>) (v => v.UserId == userId && (v.SectionId == sectionId || v.SectionId == default (string) || v.SectionId == "-1"))).OrderBy<long>((Expression<Func<HabitModel, long>>) (v => v.SortOrder)).ToListAsync();
      else
        listAsync = await App.Connection.Table<HabitModel>().Where((Expression<Func<HabitModel, bool>>) (v => v.UserId == userId && v.SectionId == sectionId)).OrderBy<long>((Expression<Func<HabitModel, long>>) (v => v.SortOrder)).ToListAsync();
      if (listAsync != null && listAsync.Any<HabitModel>())
      {
        foreach (HabitModel habit in listAsync)
          HabitDao.HandleHabitValue(habit);
      }
      return listAsync;
    }

    public static async Task<long> GetNewHabitSortOrderBySectionId(string sectionId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      HabitModel habitModel;
      if (sectionId == HabitSectionModel.GetDefault().Id || string.IsNullOrEmpty(sectionId))
        habitModel = await App.Connection.Table<HabitModel>().Where((Expression<Func<HabitModel, bool>>) (v => v.UserId == userId && (v.SectionId == sectionId || v.SectionId == default (string) || v.SectionId == "-1"))).OrderBy<long>((Expression<Func<HabitModel, long>>) (v => v.SortOrder)).FirstOrDefaultAsync();
      else
        habitModel = await App.Connection.Table<HabitModel>().Where((Expression<Func<HabitModel, bool>>) (v => v.UserId == userId && v.SectionId == sectionId)).OrderBy<long>((Expression<Func<HabitModel, long>>) (v => v.SortOrder)).FirstOrDefaultAsync();
      return habitModel == null ? 0L : habitModel.SortOrder - 268435456L;
    }

    public static async Task<HabitModel> GetHabitById(string habitId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      HabitModel habit = await App.Connection.Table<HabitModel>().Where((Expression<Func<HabitModel, bool>>) (v => v.UserId == userId && v.Id == habitId)).FirstOrDefaultAsync();
      if (habit != null)
      {
        HabitDao.HandleHabitValue(habit);
        if (string.IsNullOrEmpty(habit.Type) || habit.Type == "1")
          habit.Type = Math.Abs(habit.Goal - 1.0) > 0.01 || Math.Abs(habit.Step - 1.0) > 0.01 ? "Real" : "Boolean";
      }
      return habit;
    }

    public static async Task SaveHabitCheckStamp(string habitId, int stamp)
    {
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById == null)
        return;
      habitById.CheckStamp = stamp;
      int num = await HabitDao.UpdateAsync(habitById);
    }

    public static async Task<int> GetHabitTotalCheckCount(string habitId)
    {
      List<HabitCheckInModel> checkInsByHabitId = await HabitCheckInDao.GetHabitCheckInsByHabitId(habitId);
      return checkInsByHabitId != null ? checkInsByHabitId.Select<HabitCheckInModel, string>((Func<HabitCheckInModel, string>) (checkIn => checkIn.CheckinStamp)).Distinct<string>().Count<string>() : 0;
    }

    public static async Task UpdateHabitInfo(string habitId, bool isUpdateSyncStatus = true)
    {
      HabitModel habit = await HabitDao.GetHabitById(habitId);
      if (habit == null)
      {
        habit = (HabitModel) null;
      }
      else
      {
        HabitModel habitModel = habit;
        habitModel.CompletedCyclesList = await HabitDao.GetHabitCompletedCycles(habit);
        habitModel = (HabitModel) null;
        HabitModel habitModel1 = habit;
        List<CompletedCycle> completedCyclesList = habit.CompletedCyclesList;
        int? nullable = new int?(completedCyclesList != null ? completedCyclesList.Count<CompletedCycle>((Func<CompletedCycle, bool>) (cycle => cycle.isComplete)) : 0);
        habitModel1.CompletedCycles = nullable;
        habitModel = habit;
        habitModel.TotalCheckIns = await HabitDao.GetHabitTotalCheckCount(habit.Id);
        habitModel = (HabitModel) null;
        if (isUpdateSyncStatus)
        {
          habit.ModifiedTime = DateTime.Now;
          habit.SyncStatus = habit.SyncStatus != 0 ? 1 : 0;
        }
        int num = await HabitDao.UpdateAsync(habit);
        habit = (HabitModel) null;
      }
    }

    public static async Task<List<CompletedCycle>> GetHabitCompletedCycles(HabitModel habit)
    {
      List<CompletedCycle> cycle = new List<CompletedCycle>();
      List<HabitCheckInModel> checkInsByHabitId = await HabitCheckInDao.GetHabitCheckInsByHabitId(habit.Id);
      int? nullable = habit.TargetDays;
      int num1 = 0;
      if (nullable.GetValueOrDefault() > num1 & nullable.HasValue)
      {
        nullable = habit.TargetStartDate;
        if (nullable.HasValue)
        {
          CompletedCycle completedCycle1 = new CompletedCycle();
          nullable = habit.TargetStartDate;
          completedCycle1.StartDate = nullable.GetValueOrDefault();
          nullable = habit.TargetDays;
          completedCycle1.TargetDays = nullable.GetValueOrDefault();
          CompletedCycle completedCycle2 = completedCycle1;
          int num2 = DateUtils.DateTimeToInt(DateTime.Today);
          if (checkInsByHabitId != null && checkInsByHabitId.Any<HabitCheckInModel>())
          {
            foreach (int num3 in (IEnumerable<int>) checkInsByHabitId.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (checkIn => checkIn.IsComplete())).Select<HabitCheckInModel, int>((Func<HabitCheckInModel, int>) (checkIn => int.Parse(checkIn.CheckinStamp))).Distinct<int>().OrderBy<int, int>((Func<int, int>) (checkinStamp => checkinStamp)))
            {
              int num4 = num3;
              nullable = habit.TargetStartDate;
              int valueOrDefault = nullable.GetValueOrDefault();
              if (num4 >= valueOrDefault & nullable.HasValue && num3 <= num2)
              {
                completedCycle2.CheckinDays.Add(num3);
                if (completedCycle2.Streak == completedCycle2.TargetDays)
                {
                  int timeInt = num3;
                  completedCycle2.EndDate = timeInt;
                  cycle.Add(completedCycle2);
                  if (timeInt != num2)
                  {
                    DateTime time = DateUtils.IntToDateTime(timeInt).AddDays(1.0);
                    completedCycle2 = new CompletedCycle()
                    {
                      StartDate = DateUtils.DateTimeToInt(time),
                      TargetDays = completedCycle2.TargetDays
                    };
                  }
                  else
                    break;
                }
              }
            }
          }
          if (completedCycle2.EndDate != num2)
          {
            completedCycle2.EndDate = num2;
            cycle.Add(completedCycle2);
          }
        }
      }
      List<CompletedCycle> habitCompletedCycles = cycle;
      cycle = (List<CompletedCycle>) null;
      return habitCompletedCycles;
    }

    public static async Task<HabitModel> SaveHabitTotalCount(string habitId)
    {
      HabitModel habit = await HabitDao.GetHabitById(habitId);
      if (habit == null)
        return (HabitModel) null;
      habit.TotalCheckIns = await HabitDao.GetHabitTotalCheckCount(habitId);
      HabitModel habitModel = habit;
      habitModel.CompletedCyclesList = await HabitDao.GetHabitCompletedCycles(habit);
      habitModel = (HabitModel) null;
      HabitModel habitModel1 = habit;
      List<CompletedCycle> completedCyclesList = habit.CompletedCyclesList;
      int? nullable = completedCyclesList != null ? new int?(completedCyclesList.Count<CompletedCycle>((Func<CompletedCycle, bool>) (item => item.isComplete))) : habit.CompletedCycles;
      habitModel1.CompletedCycles = nullable;
      int num = await HabitDao.UpdateAsync(habit);
      return habit;
    }

    public static async Task SaveHabitEtag(string habitId, string etag)
    {
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById == null)
        return;
      habitById.Etag = etag;
      habitById.SyncStatus = 2;
      int num = await HabitDao.UpdateAsync(habitById);
    }

    public static async Task SaveAllHabitCheckStamp(int stamp)
    {
      List<HabitModel> allHabits = await HabitDao.GetAllHabits();
      if (!allHabits.Any<HabitModel>())
        ;
      else
      {
        allHabits.ForEach((Action<HabitModel>) (habit => habit.CheckStamp = stamp));
        int num = await HabitDao.UpdateAllAsync(allHabits);
      }
    }

    public static async Task<List<long>> GetAllSortOrder(string sectionId)
    {
      List<HabitModel> habitsBySectionId = await HabitDao.GetHabitsBySectionId(sectionId);
      return habitsBySectionId != null ? habitsBySectionId.Select<HabitModel, long>((Func<HabitModel, long>) (h => h.SortOrder)).ToList<long>() : (List<long>) null;
    }

    public static async Task ChangeHabitArchiveStatus(string habitId, bool isArchive)
    {
      HabitModel habit = await HabitDao.GetHabitById(habitId);
      if (habit == null)
      {
        habit = (HabitModel) null;
      }
      else
      {
        habit.Status = isArchive ? 1 : 0;
        habit.ModifiedTime = DateTime.Now;
        habit.ArchivedTime = isArchive ? new DateTime?(DateTime.Now) : new DateTime?();
        List<long> allSortOrder = await HabitDao.GetAllSortOrder(habit.SectionId);
        // ISSUE: explicit non-virtual call
        long num1 = allSortOrder == null || __nonvirtual (allSortOrder.Count) <= 0 ? 0L : allSortOrder.Min();
        if (habit.SortOrder != num1)
          habit.SortOrder = num1 - 268435456L;
        habit.SyncStatus = habit.SyncStatus != 0 ? 1 : 0;
        int num2 = await HabitDao.UpdateAsync(habit);
        habit = (HabitModel) null;
      }
    }

    public static async Task DeleteHabit(string habitId)
    {
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById == null)
        return;
      UtilLog.Info("HabitDao.DeleteHabit  " + habitId);
      habitById.Status = -1;
      habitById.ModifiedTime = DateTime.Now;
      habitById.SyncStatus = -1;
      int num = await HabitDao.UpdateAsync(habitById);
      await HabitCheckInDao.DeleteCheckInsByHabitId(habitId);
      await HabitRecordDao.DeleteRecordsByHabitId(habitId);
      DataChangedNotifier.NotifyHabitsChanged();
      HabitSyncService.CommitHabits();
    }

    public static async void SaveSortOrderAndSection(
      string habitId,
      long sortOrder,
      string sectionId)
    {
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById == null)
        return;
      habitById.SortOrder = sortOrder;
      habitById.ModifiedTime = DateTime.Now;
      habitById.SectionId = sectionId;
      habitById.SyncStatus = habitById.SyncStatus != 0 ? 1 : 0;
      int num = await HabitDao.UpdateAsync(habitById);
      DataChangedNotifier.NotifyHabitsChanged();
      HabitSyncService.CommitHabits();
    }

    public static async Task HabitUnableRecord(string habitId)
    {
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById == null)
        return;
      habitById.RecordEnable = new bool?(false);
      habitById.ModifiedTime = DateTime.Now;
      habitById.SyncStatus = habitById.SyncStatus != 0 ? 1 : 0;
      int num = await HabitDao.UpdateAsync(habitById);
      HabitSyncService.CommitHabits();
    }

    public static async Task<int> InsertAllAsync(List<HabitModel> model)
    {
      return await App.Connection.InsertAllAsync((IEnumerable) model);
    }

    public static async Task<int> UpdateAsync(HabitModel model)
    {
      return await App.Connection.UpdateAsync((object) model);
    }

    public static async Task<int> UpdateAllAsync(List<HabitModel> model)
    {
      return await App.Connection.UpdateAllAsync((IEnumerable) model);
    }

    public static async Task<List<HabitModel>> GetNeedPostHabits()
    {
      string userId = LocalSettings.Settings.LoginUserId;
      return await App.Connection.Table<HabitModel>().Where((Expression<Func<HabitModel, bool>>) (v => v.UserId == userId && v.SyncStatus != 2)).ToListAsync();
    }

    public static async Task CheckHabitValid(string habitId)
    {
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById != null && habitById.SyncStatus == 0 && habitById.Status != -1)
        return;
      await HabitCheckInDao.DeleteCheckInsByHabitId(habitId);
      await HabitRecordDao.DeleteRecordsByHabitId(habitId);
    }

    public static async Task<List<SkipHabitModel>> GetAllSkipHabitModels()
    {
      return await App.Connection.Table<SkipHabitModel>().Where((Expression<Func<SkipHabitModel, bool>>) (model => model.UserId == LocalSettings.Settings.LoginUserId)).ToListAsync();
    }

    public static async Task<List<HabitModel>> GetHabitByIds(List<string> ids)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      return await App.Connection.Table<HabitModel>().Where((Expression<Func<HabitModel, bool>>) (v => v.UserId == userId && ids.Contains(v.Id))).ToListAsync();
    }

    public static async Task<string> GetHabitNameById(string id)
    {
      List<StringModel> source = await App.Connection.QueryAsync<StringModel>("select Name as Text from HabitModel where Id = '" + id + "' limit 1");
      StringModel stringModel = source != null ? source.FirstOrDefault<StringModel>() : (StringModel) null;
      return stringModel != null ? stringModel.Text ?? string.Empty : (string) null;
    }
  }
}
