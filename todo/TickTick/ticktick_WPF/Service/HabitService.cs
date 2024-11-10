// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.HabitService
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.ReminderTime;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Views;
using ticktick_WPF.Views.Habit;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service
{
  public static class HabitService
  {
    private static DateTime _lastGetSectionsTime;

    public static BitmapSource GetIcon(string iconRes, string color)
    {
      if (iconRes == null)
        return (BitmapSource) null;
      if (iconRes.StartsWith("habit"))
        return (BitmapSource) new BitmapImage(new Uri("pack://application:,,,/Assets/Habits/" + iconRes.ToLower() + ".png"));
      Border element = new Border();
      element.Width = 36.0;
      element.Height = 36.0;
      element.Background = string.IsNullOrEmpty(color) ? (Brush) Brushes.Transparent : (Brush) ThemeUtil.GetColorInString(color);
      element.CornerRadius = new CornerRadius(18.0);
      Border border = element;
      TextBlock textBlock1 = new TextBlock();
      textBlock1.FontSize = 16.0;
      textBlock1.Text = ((IEnumerable<string>) iconRes.Split('_')).LastOrDefault<string>();
      textBlock1.Foreground = string.IsNullOrEmpty(color) ? (Brush) ThemeUtil.GetColor("PrimaryColor") : (Brush) Brushes.White;
      textBlock1.VerticalAlignment = VerticalAlignment.Center;
      textBlock1.HorizontalAlignment = HorizontalAlignment.Center;
      TextBlock textBlock2 = textBlock1;
      border.Child = (UIElement) textBlock2;
      return (BitmapSource) ThemeUtil.SaveAsWriteableBitmap((FrameworkElement) element);
    }

    public static async Task<(bool, string)> CheckSectionName(string newName)
    {
      return string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName) ? (false, Utils.GetString("SectionNameCannotBeEmpty")) : (!(await HabitService.GetHabitSections()).Exists((Predicate<HabitSectionModel>) (h => h.DisplayName == newName || h.Name == newName)) ? (newName == Utils.GetString("HabitSectionOthers") || newName.ToLower() == "other" ? (false, Utils.GetString("SectionNameExisted")) : (string.IsNullOrEmpty(newName) || string.IsNullOrWhiteSpace(newName) || !NameUtils.IsValidColumnName(newName) ? (false, Utils.GetString("SectionNotValid")) : (true, ""))) : (false, Utils.GetString("SectionNameExisted")));
    }

    public static async Task<List<HabitModel>> GetHabitsbyStatus(int status)
    {
      return await HabitDao.GetHabitsbyStatus(status);
    }

    public static async Task<List<HabitSectionModel>> GetHabitSections()
    {
      List<HabitSectionModel> sections = HabitSectionCache.GetSections();
      if (Utils.IsEmptyDate(HabitService._lastGetSectionsTime))
        HabitService._lastGetSectionsTime = DateTime.Today.AddDays(-1.0);
      TimeSpan timeSpan = DateTime.Now - HabitService._lastGetSectionsTime;
      bool flag = false;
      if (timeSpan.TotalSeconds > 59.0)
      {
        HabitService._lastGetSectionsTime = DateTime.Now;
        flag = true;
      }
      if (((sections == null ? 1 : (!sections.Any<HabitSectionModel>() ? 1 : 0)) & (flag ? 1 : 0)) != 0)
      {
        await HabitSectionsSyncService.PullHabitSections();
        sections = HabitSectionCache.GetSections();
      }
      return sections.Where<HabitSectionModel>((Func<HabitSectionModel, bool>) (section => section.SyncStatus != -1)).ToList<HabitSectionModel>();
    }

    public static async Task UpdateSection(HabitSectionModel model, bool needsNotify = true)
    {
      if (model.Id == HabitSectionModel.GetDefault().Id)
      {
        HabitSectionModel.GetDefault().IsOpen = model.IsOpen;
      }
      else
      {
        model.SyncStatus = 1;
        await HabitSectionDao.UpdateAsync(model);
      }
      DataChangedNotifier.NotifyHabitSectionChanged();
      HabitSectionsSyncService.BatchUpdateHabitSections();
    }

    public static async void UpdateSectionSortOrderById(string id, long order)
    {
      if (id == HabitSectionModel.GetDefault().Id)
      {
        HabitSectionModel.GetDefault().SortOrder = order;
        DataChangedNotifier.NotifyHabitsChanged();
        LocalSettings.Settings.HabitDefaultOrder = order;
        LocalSettings.Settings.Save();
        HabitService.PushHabitSettings();
      }
      else
      {
        HabitSectionModel habitSectionById = await HabitSectionDao.GetHabitSectionById(id);
        if (habitSectionById == null)
          return;
        habitSectionById.SortOrder = order;
        habitSectionById.SyncStatus = 1;
        await HabitSectionDao.UpdateAsync(habitSectionById);
        await HabitSectionCache.SetSections();
        DataChangedNotifier.NotifyHabitsChanged();
        HabitSectionsSyncService.BatchUpdateHabitSections();
      }
    }

    public static async Task OnPomoTaskChanged(string habitId, long second, DateTime date)
    {
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById == null || habitById.IsBoolHabit())
        return;
      float num = 0.0f;
      if (FocusConstance.HourUnit.Contains(habitById.Unit.Trim().ToLower()))
        num = (float) second / 3600f;
      if (FocusConstance.MinuteUnit.Contains(habitById.Unit.Trim().ToLower()))
        num = (float) second / 60f;
      if ((double) num <= 0.0)
        return;
      HabitService.CheckInHabit(habitById.Id, date, Math.Round((double) num, 2));
    }

    public static async void DeleteSection(HabitSectionModel model)
    {
      List<HabitModel> habits = await HabitDao.GetHabitsBySectionId(model.Id);
      string otherSectionId = HabitSectionModel.GetDefault().Id;
      List<HabitModel> habitsBySectionId = await HabitDao.GetHabitsBySectionId(otherSectionId);
      long num1 = 268435456;
      if (habitsBySectionId != null && habitsBySectionId.Any<HabitModel>())
        num1 = habitsBySectionId.Select<HabitModel, long>((Func<HabitModel, long>) (h => h.SortOrder)).Min();
      foreach (HabitModel habitModel in habits)
      {
        habitModel.SyncStatus = habitModel.SyncStatus != 0 ? 1 : 0;
        habitModel.SectionId = otherSectionId;
        habitModel.SortOrder = num1 - 268435456L;
        num1 -= 268435456L;
      }
      int num2 = await HabitDao.UpdateAllAsync(habits);
      HabitSyncService.CommitHabits();
      model.SyncStatus = -1;
      await HabitSectionDao.UpdateAsync(model);
      await HabitSectionCache.SetSections();
      HabitSectionsSyncService.BatchUpdateHabitSections();
      DataChangedNotifier.NotifyHabitsChanged();
      habits = (List<HabitModel>) null;
      otherSectionId = (string) null;
    }

    public static async void DeleteSectionById(string sectionId)
    {
      HabitSectionModel habitSectionById = await HabitSectionDao.GetHabitSectionById(sectionId);
      if (habitSectionById == null)
        return;
      HabitService.DeleteSection(habitSectionById);
    }

    public static async Task AddSections(List<HabitSectionModel> models)
    {
      List<HabitSectionModel> sections = HabitSectionCache.GetSections();
      sections.Add(HabitSectionModel.GetDefault());
      models.ForEach((Action<HabitSectionModel>) (model =>
      {
        model.SyncStatus = 0;
        model.ModifiedTime = DateTime.Now;
        model.SortOrder = sections == null || !sections.Any<HabitSectionModel>() ? 0L : sections.Select<HabitSectionModel, long>((Func<HabitSectionModel, long>) (item => item.SortOrder)).Max() + 268435456L;
        sections.Add(model);
      }));
      await HabitSectionDao.InsertAllAsync(models);
      await HabitSectionCache.SetSections();
      HabitSectionsSyncService.BatchUpdateHabitSections();
      DataChangedNotifier.NotifyHabitsChanged();
    }

    public static async Task<string> GetSectionDefaultTime(string sectionName)
    {
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          Utils.GetString("HabitSectionMorning"),
          "09:00"
        },
        {
          Utils.GetString("HabitSectionAfternoon"),
          "13:00"
        },
        {
          Utils.GetString("HabitSectionNight"),
          "20:00"
        }
      };
      dictionary["morning"] = "09:00";
      dictionary["afternoon"] = "13:00";
      dictionary["night"] = "20:00";
      string lower = sectionName.TrimEnd().TrimStart().ToLower();
      return dictionary.ContainsKey(lower) ? dictionary[lower] : string.Empty;
    }

    public static async Task InitSections()
    {
      await HabitService.AddSections(new List<HabitSectionModel>()
      {
        new HabitSectionModel("_morning"),
        new HabitSectionModel("_afternoon"),
        new HabitSectionModel("_night")
      });
    }

    public static async Task<bool> PullRemoteCheckinAndRecordData(HabitModel habit)
    {
      if (habit == null)
        return false;
      int checkStamp = habit.CheckStamp;
      int lastSeasonStamp = ticktick_WPF.Util.DateUtils.GetDateNum(DateTime.Today.AddDays(-91.0));
      if (habit.CheckStamp > 0)
        checkStamp = Math.Min(lastSeasonStamp, checkStamp);
      bool checkInChanged = await HabitService.PullRemoteCheckIn(habit.Id, checkStamp);
      bool recordChanged = await HabitService.PullRemoteHabitRecord(habit.Id, checkStamp);
      bool localDirty = await HabitCheckInDao.HandleLocalDirtyCheckIns(habit.Id);
      await HabitDao.SaveHabitCheckStamp(habit.Id, lastSeasonStamp);
      return checkInChanged | recordChanged | localDirty;
    }

    public static async Task<bool> PullAndMergeRemoteHabitSections()
    {
      List<HabitSectionModel> remoteSections = await Communicator.PullRemoteHabitSections();
      return remoteSections != null && await HabitService.MergeRemoteHabitSections(remoteSections);
    }

    public static async Task<List<HabitModel>> PullRemoteHabits(bool needMerge = true)
    {
      List<HabitModel> habits = await Communicator.PullRemoteHabits();
      if (habits != null)
      {
        if (needMerge)
        {
          if (await HabitService.MergeRemoteHabits(habits))
            DataChangedNotifier.NotifyHabitsChanged();
        }
        UtilLog.Info(string.Format("PullRemoteHabits : Count {0}", (object) habits.Count));
      }
      List<HabitModel> habitModelList = habits;
      habits = (List<HabitModel>) null;
      return habitModelList;
    }

    public static async Task CheckInHabit(
      string habitId,
      DateTime date,
      double step = -1.0,
      bool increment = true,
      bool checkShowRecord = true,
      int delay = 0,
      IToastShowWindow window = null,
      object sender = null)
    {
      window = window ?? Utils.GetToastWindow();
      bool isRealArchived = false;
      HabitModel habit = await HabitDao.GetHabitById(habitId);
      if (habit == null)
      {
        habit = (HabitModel) null;
      }
      else
      {
        bool needRecord = true;
        if (!increment)
        {
          HabitCheckInModel byHabitIdAndStamp = await HabitCheckInDao.GetHabitCheckInsByHabitIdAndStamp(habitId, date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
          if (byHabitIdAndStamp != null && byHabitIdAndStamp.Value >= byHabitIdAndStamp.Goal)
            needRecord = false;
        }
        HabitCheckInModel checkIn = await HabitService.CheckInHabit(habit, date, step, increment);
        if (checkIn != null && DateTime.Now.Hour < 3)
          HabitUtils.WhenCheckInMidnight(habit.Reminder, window);
        UtilLog.Info("CheckInHabit " + habitId + "," + ticktick_WPF.Util.DateUtils.GetDateNum(date).ToString() + "," + checkIn?.Value.ToString() + "," + checkIn?.Goal.ToString());
        if (checkIn != null && checkIn.Value >= checkIn.Goal)
        {
          if (habit.RecordEnable.GetValueOrDefault() & needRecord & checkShowRecord)
            await HabitUtils.ShowHabitRecordWindow(habitId, habit.Name, date, false);
          DateTime dateTime1 = ticktick_WPF.Util.DateUtils.ParseDateTime(checkIn.CheckinStamp);
          int? targetStartDate = habit.TargetStartDate;
          ref int? local = ref targetStartDate;
          int num;
          string str;
          if (!local.HasValue)
          {
            str = (string) null;
          }
          else
          {
            num = local.GetValueOrDefault();
            str = num.ToString();
          }
          DateTime dateTime2 = ticktick_WPF.Util.DateUtils.ParseDateTime(str);
          if (dateTime1 >= dateTime2)
          {
            int? targetDays1 = habit.TargetDays;
            num = 0;
            if (targetDays1.GetValueOrDefault() > num & targetDays1.HasValue)
            {
              List<CompletedCycle> completedCyclesList = habit.CompletedCyclesList;
              int? nullable1 = completedCyclesList != null ? completedCyclesList.LastOrDefault<CompletedCycle>()?.Streak : new int?();
              int? targetDays2 = habit.TargetDays;
              int? nullable2 = targetDays2.HasValue ? new int?(targetDays2.GetValueOrDefault() - 1) : new int?();
              if (nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() & nullable1.HasValue == nullable2.HasValue)
              {
                CustomerDialog customerDialog = new CustomerDialog((string) null, string.Format(Utils.GetString("HabitCompleteCycleInfo"), (object) habit.TargetDays), Utils.GetString("HabitContinue"), Utils.GetString("EndAndArchive"));
                try
                {
                  customerDialog.Owner = window is FrameworkElement frameworkElement ? Window.GetWindow((DependencyObject) frameworkElement) : (Window) App.Window;
                }
                catch (Exception ex)
                {
                }
                bool isClosed = false;
                customerDialog.CloseClick += (EventHandler) ((_, e) => isClosed = true);
                bool? nullable3 = customerDialog.ShowDialog();
                if ((nullable3.HasValue ? new bool?(!nullable3.GetValueOrDefault()) : new bool?()).GetValueOrDefault() && !isClosed)
                {
                  isRealArchived = true;
                  (window ?? (IToastShowWindow) App.Window).TryToastString((object) null, Utils.GetString("Archived"));
                  DataChangedNotifier.NotifyHabitArchived(habitId);
                  await Task.Delay(100);
                  await HabitDao.ChangeHabitArchiveStatus(habitId, true);
                }
              }
            }
          }
        }
        await HabitDao.UpdateHabitInfo(habitId);
        if (delay > 0)
          await Task.Delay(delay);
        DataChangedNotifier.NotifyHabitCyclesChanged(habit.Id);
        DataChangedNotifier.NotifyHabitCheckInChanged(checkIn, sender);
        if (isRealArchived)
          DataChangedNotifier.NotifyHabitsChanged();
        HabitSyncService.CommitHabitCheckIns();
        HabitSyncService.CommitHabits();
        checkIn = (HabitCheckInModel) null;
        habit = (HabitModel) null;
      }
    }

    public static async Task<bool> ResetCheckInHabit(
      string habitId,
      DateTime date,
      string type = "boolean",
      object sender = null)
    {
      HabitModel habit = await HabitDao.GetHabitById(habitId);
      if (habit != null)
      {
        if (!string.Equals(type, HabitType.Boolean.ToString(), StringComparison.CurrentCultureIgnoreCase) && (int) habit.Step != 0)
        {
          bool? nullable = new CustomerDialog(Utils.GetString("ResetCheckIn"), string.Format(Utils.GetString("ResetCheckInHint"), (object) date.ToString("m", (IFormatProvider) App.Ci)), Utils.GetString("Reset"), Utils.GetString("Cancel")).ShowDialog();
          if ((!nullable.HasValue ? 0 : (nullable.Value ? 1 : 0)) == 0)
            return false;
        }
        await HabitService.ResetCheckInHabit(habitId, date);
        await HabitDao.UpdateHabitInfo(habitId);
        DataChangedNotifier.NotifyHabitCyclesChanged(habit.Id);
        DataChangedNotifier.NotifyHabitCheckInChanged(new HabitCheckInModel()
        {
          HabitId = habitId,
          CheckinStamp = date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture),
          Goal = habit.Goal,
          Value = 0.0
        }, sender);
        HabitSyncService.CommitHabitCheckIns();
        HabitSyncService.CommitHabits();
      }
      return true;
    }

    public static async Task<bool> PullRemoteHabitRecord(string habitId, int stamp)
    {
      List<HabitRecordModel> records = await Communicator.PullHabitRecords(habitId, stamp);
      return records != null && await HabitService.MergeRemoteHabitRecords(habitId, (IEnumerable<HabitRecordModel>) records, stamp);
    }

    public static async Task<bool> PullRemoteHabitsCheckIn(List<string> habitIds, int checkStamp = 0)
    {
      bool changed = false;
      CheckInCollection collection = await Communicator.PullHabitsCheckIns(habitIds, checkStamp);
      if (collection?.CheckIns != null && collection.CheckIns.Any<KeyValuePair<string, List<HabitCheckInModel>>>())
      {
        foreach (string key in collection.CheckIns.Keys)
          changed |= await HabitService.MergeRemoteHabitCheckIns(key, collection.CheckIns[key], checkStamp);
        UtilLog.Info(nameof (PullRemoteHabitsCheckIn) + checkStamp.ToString() + "   " + collection.CheckIns.Count.ToString() + "   " + changed.ToString());
      }
      bool flag = changed;
      collection = (CheckInCollection) null;
      return flag;
    }

    public static async Task PullRemoteHabitsRecord(List<string> habitIds, int checkStamp = 0)
    {
      await HabitService.MergeRemoteRecords(await Communicator.PullHabitsRecords(habitIds, checkStamp), checkStamp);
    }

    public static async Task<bool> PullRemoteCheckIn(string habitId, int stamp)
    {
      return await HabitService.MergeRemoteHabitCheckIns(habitId, await Communicator.PullHabitCheckIns(habitId, stamp), stamp);
    }

    public static async Task DeleteCheckInRecord(string logId)
    {
      await HabitRecordDao.DeleteHabitRecord(logId);
      SyncManager.TryDelaySync();
      DataChangedNotifier.NotifyHabitLogChanged();
    }

    public static async Task SaveHabitCheckinRecord(string logId, string text, int score)
    {
      await HabitRecordDao.SaveHabitCheckinRecord(logId, text, score);
      SyncManager.TryDelaySync();
      DataChangedNotifier.NotifyHabitLogChanged();
    }

    public static async Task AddCheckInRecord(CheckInLogViewModel vm)
    {
      await HabitRecordDao.AddCheckInRecord(vm);
      SyncManager.TryDelaySync();
      DataChangedNotifier.NotifyHabitLogChanged();
    }

    public static async Task PullRemoteConfig()
    {
      HabitSettingsModel config = await Communicator.GetRemoteHabitConfig();
      if (config == null)
        config = (HabitSettingsModel) null;
      else if (!string.IsNullOrEmpty(config.errorId))
      {
        config = (HabitSettingsModel) null;
      }
      else
      {
        LocalSettings.Settings.HabitInToday = config.ShowInToday.GetValueOrDefault();
        HabitDefaultSection defaultSection1 = config.DefaultSection;
        long num1;
        if (defaultSection1 != null)
          num1 = defaultSection1.SortOrder;
        else
          num1 = await HabitSectionDao.GetMaxSortOrder();
        LocalSettings.Settings.HabitDefaultOrder = num1;
        LocalSettings.Settings.Save();
        HabitSectionModel habitSectionModel = HabitSectionModel.GetDefault();
        HabitDefaultSection defaultSection2 = config.DefaultSection;
        long num2 = defaultSection2 != null ? defaultSection2.SortOrder : HabitSectionModel.GetDefault().SortOrder;
        habitSectionModel.SortOrder = num2;
        config = (HabitSettingsModel) null;
      }
    }

    public static async Task PushHabitSettings()
    {
      Communicator.PushHabitConfig(new HabitSettingsModel()
      {
        ShowInCalendar = new bool?(LocalSettings.Settings.HabitInCal),
        ShowInToday = new bool?(LocalSettings.Settings.HabitInToday),
        DefaultSection = new HabitDefaultSection()
        {
          SortOrder = LocalSettings.Settings.HabitDefaultOrder
        },
        Enabled = new bool?(LocalSettings.Settings.ShowHabit)
      });
    }

    public static async Task UncompleteHabit(string habitId, DateTime date, object sender = null)
    {
      HabitModel habit = await HabitDao.GetHabitById(habitId);
      if (habit == null)
      {
        habit = (HabitModel) null;
      }
      else
      {
        HabitCheckInModel checkIn = await HabitCheckInDao.GetHabitCheckInByHabitIdAndStamp(habitId, date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
        if (checkIn == null || checkIn.Status == -1)
        {
          checkIn = new HabitCheckInModel()
          {
            Id = Utils.GetGuid(),
            HabitId = habit.Id,
            UserId = Utils.GetCurrentUserIdInt().ToString(),
            CheckinStamp = date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture),
            Value = 0.0,
            Status = 0,
            CheckStatus = 1,
            CheckinTime = new DateTime?(DateTime.Now),
            Goal = habit.Goal
          };
          checkIn.opTime = new DateTime?(DateTime.Now);
          await HabitCheckInDao.InsertAsync(checkIn);
        }
        else
        {
          checkIn.CheckStatus = 1;
          checkIn.Status = checkIn.Status != 0 ? 1 : 0;
          checkIn.opTime = new DateTime?(DateTime.Now);
          await HabitCheckInDao.UpdateAsync(checkIn);
        }
        if (DateTime.Now.Hour < 3)
          HabitUtils.WhenCheckInMidnight(habit.Reminder);
        if (habit.RecordEnable.GetValueOrDefault())
          HabitUtils.ShowHabitRecordWindow(habit.Id, habit.Name, date, false, true);
        await HabitDao.UpdateHabitInfo(habitId);
        DataChangedNotifier.NotifyHabitCyclesChanged(habit.Id);
        DataChangedNotifier.NotifyHabitCheckInChanged(checkIn, sender);
        HabitSyncService.CommitHabitCheckIns();
        HabitSyncService.CommitHabits();
        checkIn = (HabitCheckInModel) null;
        habit = (HabitModel) null;
      }
    }

    public static async Task<HabitCheckInModel> CheckInHabit(
      HabitModel habit,
      DateTime date,
      double step = -1.0,
      bool increment = true)
    {
      HabitCheckInModel habitCheckInModel = (HabitCheckInModel) null;
      if (habit != null)
      {
        if (habit.Type.ToLower() == "boolean")
          habitCheckInModel = await HabitService.CheckInBoolHabit(habit, date);
        else
          habitCheckInModel = await HabitService.CheckInRealHabit(habit, date, step, increment);
        UtilLog.Info(string.Format("CheckInHabit {0},{1},type:{2},step:{3},{4}", (object) habit.Id, (object) date, (object) habit.Type, (object) step, (object) increment));
      }
      return habitCheckInModel;
    }

    private static async Task<HabitCheckInModel> CheckInRealHabit(
      HabitModel habit,
      DateTime date,
      double step,
      bool increment)
    {
      HabitCheckInType type = HabitUtils.GetHabitCheckInType(0, habit.Type, habit.Step);
      HabitCheckInModel checkIn = await HabitCheckInDao.GetHabitCheckInByHabitIdAndStamp(habit.Id, date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
      if (checkIn == null || checkIn.Status == -1)
      {
        double num = step;
        if (type == HabitCheckInType.CompletedAllUnCompleted)
          num = !increment || step > 0.0 ? step : habit.Goal;
        bool flag = num >= habit.Goal;
        HabitCheckInModel model = new HabitCheckInModel()
        {
          Id = Utils.GetGuid(),
          HabitId = habit.Id,
          UserId = Utils.GetCurrentUserIdInt().ToString(),
          CheckinStamp = date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture),
          Value = num,
          Status = 0,
          CheckStatus = flag ? 2 : 0,
          Goal = habit.Goal
        };
        model.CheckinTime = !flag ? new DateTime?() : new DateTime?(DateTime.Now);
        model.opTime = new DateTime?(DateTime.Now);
        await HabitCheckInDao.InsertAsync(model);
        return model;
      }
      if (increment)
      {
        if (type == HabitCheckInType.CompletedAllUnCompleted && step <= 0.0)
          checkIn.Value = checkIn.Goal;
        else
          checkIn.Value += step;
      }
      else
      {
        checkIn.Value = step;
        if (date.Date == DateTime.Today && Math.Abs(checkIn.Value) < 1E-05)
          checkIn.Goal = habit.Goal;
      }
      checkIn.Status = checkIn.Status != 0 ? 1 : 0;
      if (checkIn.Value >= checkIn.Goal)
      {
        checkIn.CheckinTime = new DateTime?(DateTime.Now);
        checkIn.CheckStatus = 2;
      }
      else
      {
        checkIn.CheckinTime = new DateTime?();
        checkIn.CheckStatus = 0;
      }
      checkIn.opTime = new DateTime?(DateTime.Now);
      await HabitCheckInDao.UpdateAsync(checkIn);
      return checkIn;
    }

    private static async Task<HabitCheckInModel> CheckInBoolHabit(HabitModel habit, DateTime date)
    {
      HabitCheckInModel checkIn = await HabitCheckInDao.GetHabitCheckInByHabitIdAndStamp(habit.Id, date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
      if (checkIn == null || checkIn.Status == -1)
      {
        HabitCheckInModel model = new HabitCheckInModel()
        {
          Id = Utils.GetGuid(),
          HabitId = habit.Id,
          UserId = Utils.GetCurrentUserIdInt().ToString(),
          CheckinStamp = date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture),
          CheckinTime = new DateTime?(DateTime.Now),
          Value = 1.0,
          Status = 0,
          Goal = 1.0,
          CheckStatus = 2
        };
        model.opTime = new DateTime?(DateTime.Now);
        await HabitCheckInDao.InsertAsync(model);
        return model;
      }
      if (Math.Abs(checkIn.Value) < 0.0001)
        checkIn.Value = checkIn.Goal;
      else
        checkIn.Goal = checkIn.Value;
      checkIn.CheckStatus = 2;
      checkIn.Status = 1;
      checkIn.CheckinTime = new DateTime?(DateTime.Now);
      checkIn.opTime = new DateTime?(DateTime.Now);
      await HabitCheckInDao.UpdateAsync(checkIn);
      return checkIn;
    }

    public static async Task ResetCheckInHabit(string habitId, DateTime date)
    {
      HabitCheckInModel byHabitIdAndStamp = await HabitCheckInDao.GetHabitCheckInByHabitIdAndStamp(habitId, date.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture));
      if (byHabitIdAndStamp == null)
        return;
      byHabitIdAndStamp.Value = 0.0;
      byHabitIdAndStamp.CheckStatus = 0;
      byHabitIdAndStamp.Status = -1;
      byHabitIdAndStamp.opTime = new DateTime?(DateTime.Now);
      await HabitCheckInDao.UpdateAsync(byHabitIdAndStamp);
    }

    public static async Task<bool> MergeRemoteHabitSections(List<HabitSectionModel> remoteSections)
    {
      List<HabitSectionModel> added = new List<HabitSectionModel>();
      List<HabitSectionModel> updated = new List<HabitSectionModel>();
      List<HabitSectionModel> deleted = new List<HabitSectionModel>();
      List<HabitSectionModel> source = await HabitSectionDao.GetAllHabitSections() ?? new List<HabitSectionModel>();
      if (remoteSections.Any<HabitSectionModel>())
      {
        foreach (HabitSectionModel remoteSection in remoteSections)
        {
          HabitSectionModel remote = remoteSection;
          HabitSectionModel habitSectionModel1 = source.FirstOrDefault<HabitSectionModel>((Func<HabitSectionModel, bool>) (habit => habit.Id == remote.Id));
          int currentUserIdInt;
          if (habitSectionModel1 != null)
          {
            if (habitSectionModel1.Etag != remote.Etag && habitSectionModel1.SyncStatus == 2)
            {
              remote._Id = habitSectionModel1._Id;
              HabitSectionModel habitSectionModel2 = remote;
              currentUserIdInt = Utils.GetCurrentUserIdInt();
              string str = currentUserIdInt.ToString();
              habitSectionModel2.UserId = str;
              remote.SyncStatus = 2;
              updated.Add(remote);
            }
            source.Remove(habitSectionModel1);
          }
          else
          {
            HabitSectionModel habitSectionModel3 = remote;
            currentUserIdInt = Utils.GetCurrentUserIdInt();
            string str = currentUserIdInt.ToString();
            habitSectionModel3.UserId = str;
            remote.SyncStatus = 2;
            added.Add(remote);
          }
        }
        if (source.Any<HabitSectionModel>())
          deleted.AddRange(source.Where<HabitSectionModel>((Func<HabitSectionModel, bool>) (local => local.SyncStatus == 2)));
        if (added.Any<HabitSectionModel>())
        {
          int num1 = await App.Connection.InsertAllAsync((IEnumerable) added);
        }
        if (updated.Any<HabitSectionModel>())
        {
          int num2 = await App.Connection.UpdateAllAsync((IEnumerable) updated);
        }
        if (deleted.Any<HabitSectionModel>())
        {
          foreach (object obj in deleted)
          {
            int num3 = await App.Connection.DeleteAsync(obj);
          }
        }
      }
      else if (source.Any<HabitSectionModel>())
      {
        foreach (HabitSectionModel habitSectionModel in source)
        {
          if (habitSectionModel.SyncStatus != 0)
          {
            int num = await App.Connection.DeleteAsync((object) habitSectionModel);
          }
        }
        return true;
      }
      return added.Any<HabitSectionModel>() || updated.Any<HabitSectionModel>() || deleted.Any<HabitSectionModel>();
    }

    public static async Task MergeRemoteRecords(
      HabitRecordCheckResult remoteRecords,
      int checkStamp)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<HabitRecordModel> habitRecords = await HabitRecordDao.GetHabitRecords(checkStamp);
      List<HabitRecordModel> habitRecordModelList1 = new List<HabitRecordModel>();
      List<HabitRecordModel> updated = new List<HabitRecordModel>();
      List<HabitRecordModel> deleted = new List<HabitRecordModel>();
      if (remoteRecords?.HabitRecords != null && remoteRecords.HabitRecords.Any<KeyValuePair<string, List<HabitRecordModel>>>())
      {
        foreach (List<HabitRecordModel> habitRecordModelList2 in remoteRecords.HabitRecords.Values)
        {
          foreach (HabitRecordModel habitRecordModel1 in habitRecordModelList2)
          {
            HabitRecordModel record = habitRecordModel1;
            HabitRecordModel habitRecordModel2 = habitRecords.FirstOrDefault<HabitRecordModel>((Func<HabitRecordModel, bool>) (model => model.Stamp == record.Stamp && model.HabitId == record.HabitId));
            if (habitRecordModel2 != null)
            {
              if (habitRecordModel2.Status == 0 || habitRecordModel2.Status == 1)
              {
                habitRecords.Remove(habitRecordModel2);
              }
              else
              {
                if ((habitRecordModel2.Content != record.Content || habitRecordModel2.Emoji != record.Emoji) && habitRecordModel2.Deleted != 1)
                {
                  habitRecordModel2.Content = record.Content;
                  habitRecordModel2.Emoji = record.Emoji;
                  updated.Add(habitRecordModel2);
                }
                habitRecords.Remove(habitRecordModel2);
              }
            }
            else
            {
              record.UserId = userId;
              record.Status = 2;
              record.Deleted = 0;
              habitRecordModelList1.Add(record);
            }
          }
        }
      }
      if (habitRecords.Any<HabitRecordModel>())
        deleted.AddRange(habitRecords.Where<HabitRecordModel>((Func<HabitRecordModel, bool>) (record => record.Status == 2)));
      if (habitRecordModelList1.Any<HabitRecordModel>())
      {
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) habitRecordModelList1);
      }
      if (updated.Any<HabitRecordModel>())
      {
        int num2 = await App.Connection.UpdateAllAsync((IEnumerable) updated);
      }
      if (!deleted.Any<HabitRecordModel>())
      {
        userId = (string) null;
        updated = (List<HabitRecordModel>) null;
        deleted = (List<HabitRecordModel>) null;
      }
      else
      {
        foreach (object obj in deleted)
        {
          int num3 = await App.Connection.DeleteAsync(obj);
        }
        userId = (string) null;
        updated = (List<HabitRecordModel>) null;
        deleted = (List<HabitRecordModel>) null;
      }
    }

    public static async Task HandleTodayCheckIn(HabitModel current, HabitModel modify, bool isPull = false)
    {
      if (!(current.Type != modify.Type) && Math.Abs(current.Goal - modify.Goal) <= 0.0001)
        ;
      else
      {
        string userId = Utils.GetCurrentUserIdInt().ToString();
        string stamp = DateTime.Today.ToString("yyyyMMdd");
        HabitCheckInModel todayCheckIn = await App.Connection.Table<HabitCheckInModel>().Where((Expression<Func<HabitCheckInModel, bool>>) (v => v.UserId == userId && v.HabitId == modify.Id && v.CheckinStamp == stamp)).FirstOrDefaultAsync();
        if (todayCheckIn != null)
        {
          bool needLog = todayCheckIn.Value < todayCheckIn.Goal;
          if (modify.IsBoolHabit() && !current.IsBoolHabit())
          {
            bool flag = todayCheckIn.Value >= todayCheckIn.Goal;
            todayCheckIn.Goal = 1.0;
            if (todayCheckIn.Value > 0.0)
            {
              todayCheckIn.Value = 1.0;
              if (!flag)
                todayCheckIn.CheckinTime = new DateTime?(DateTime.Now);
              todayCheckIn.CheckStatus = 2;
            }
          }
          else if (!modify.IsBoolHabit() && current.IsBoolHabit())
          {
            if (todayCheckIn.Value >= todayCheckIn.Goal)
              return;
            todayCheckIn.Goal = modify.Goal;
            todayCheckIn.Value = 0.0;
            todayCheckIn.CheckinTime = new DateTime?();
          }
          else if (!modify.IsBoolHabit() && !current.IsBoolHabit())
          {
            bool flag = todayCheckIn.Value >= todayCheckIn.Goal;
            todayCheckIn.Goal = modify.Goal;
            if (todayCheckIn.Value >= todayCheckIn.Goal)
            {
              todayCheckIn.CheckinTime = flag ? todayCheckIn.CheckinTime : new DateTime?(DateTime.Now);
              todayCheckIn.CheckStatus = 2;
            }
            else
            {
              todayCheckIn.CheckStatus = todayCheckIn.CheckStatus == 2 ? 0 : todayCheckIn.CheckStatus;
              if (todayCheckIn.CheckStatus != 1)
                todayCheckIn.CheckinTime = new DateTime?();
            }
          }
          if (!isPull)
          {
            todayCheckIn.Status = todayCheckIn.Status != 0 ? 1 : 0;
            int num = await App.Connection.UpdateAsync((object) todayCheckIn);
            if (modify.RecordEnable.GetValueOrDefault() & needLog && todayCheckIn.Value >= todayCheckIn.Goal)
              HabitUtils.ShowHabitRecordWindow(modify.Id, modify.Name, DateTime.Today);
          }
          else
          {
            int num1 = await App.Connection.UpdateAsync((object) todayCheckIn);
          }
        }
        todayCheckIn = (HabitCheckInModel) null;
      }
    }

    public static async Task UpdateHabit(HabitModel habit)
    {
      HabitModel exist = await HabitDao.GetHabitById(habit.Id);
      if (exist != null)
      {
        await HabitService.HandleTodayCheckIn(exist, habit);
        exist.Name = habit.Name;
        exist.IconRes = habit.IconRes;
        exist.Color = habit.Color;
        exist.RepeatRule = habit.RepeatRule;
        exist.Type = habit.Type;
        exist.Goal = habit.Goal;
        exist.Step = habit.Step;
        exist.Unit = habit.Unit;
        exist.Reminder = habit.Reminder;
        exist.Status = habit.Status;
        exist.SyncStatus = exist.SyncStatus != 0 ? 1 : 0;
        exist.RecordEnable = habit.RecordEnable;
        exist.ModifiedTime = DateTime.Now;
        HabitModel habitModel = exist;
        habitModel.TotalCheckIns = await HabitDao.GetHabitTotalCheckCount(exist.Id);
        habitModel = (HabitModel) null;
        exist.TargetDays = habit.TargetDays;
        exist.TargetStartDate = habit.TargetStartDate;
        habitModel = exist;
        habitModel.CompletedCyclesList = await HabitDao.GetHabitCompletedCycles(habit);
        habitModel = (HabitModel) null;
        exist.CompletedCycles = new int?(exist.CompletedCyclesList.Count<CompletedCycle>((Func<CompletedCycle, bool>) (c => c.isComplete)));
        exist.SectionId = habit.SectionId;
        int num = await App.Connection.UpdateAsync((object) exist);
        HabitSyncService.CommitHabits();
      }
      else
      {
        habit.UserId = LocalSettings.Settings.LoginUserId;
        habit.Id = string.IsNullOrEmpty(habit.Id) ? Utils.GetGuid() : habit.Id;
        habit.SyncStatus = habit.SyncStatus;
        habit.ModifiedTime = DateTime.Now;
        int num = await App.Connection.InsertAsync((object) habit);
        HabitSyncService.CommitHabits(true);
      }
      HabitSyncService.CommitHabitCheckIns();
      exist = (HabitModel) null;
    }

    public static async Task<bool> MergeRemoteHabits(List<HabitModel> remoteHabits)
    {
      List<HabitModel> source = await HabitDao.GetAllHabits() ?? new List<HabitModel>();
      foreach (HabitModel habitModel in source)
      {
        HabitModel local = habitModel;
        HabitModel modify = remoteHabits.FirstOrDefault<HabitModel>((Func<HabitModel, bool>) (habit => habit.Id == local.Id));
        if (modify != null)
          HabitService.HandleTodayCheckIn(local, modify, true);
      }
      List<HabitModel> added = new List<HabitModel>();
      List<HabitModel> updated = new List<HabitModel>();
      List<HabitModel> deleted = new List<HabitModel>();
      if (remoteHabits.Any<HabitModel>())
      {
        foreach (HabitModel remoteHabit in remoteHabits)
        {
          HabitModel remote = remoteHabit;
          HabitModel habitModel1 = source.FirstOrDefault<HabitModel>((Func<HabitModel, bool>) (habit => habit.Id == remote.Id));
          int currentUserIdInt;
          if (habitModel1 != null)
          {
            if (habitModel1.Etag != remote.Etag && habitModel1.SyncStatus == 2)
            {
              remote._Id = habitModel1._Id;
              HabitModel habitModel2 = remote;
              currentUserIdInt = Utils.GetCurrentUserIdInt();
              string str = currentUserIdInt.ToString();
              habitModel2.UserId = str;
              remote.SyncStatus = 2;
              if (remote.Type != habitModel1.Type)
                UtilLog.Info("HabitTypeChanged " + remote.Id + " remote :" + remote.Type + " local :" + habitModel1.Type);
              updated.Add(remote);
            }
            source.Remove(habitModel1);
          }
          else
          {
            HabitModel habitModel3 = remote;
            currentUserIdInt = Utils.GetCurrentUserIdInt();
            string str = currentUserIdInt.ToString();
            habitModel3.UserId = str;
            remote.SyncStatus = 2;
            if (remote.Type != "Boolean" && remote.Type != "Real")
              UtilLog.Info("HabitTypeError " + remote.Id + " remote :" + remote.Type);
            added.Add(remote);
          }
        }
        if (source.Any<HabitModel>())
          deleted.AddRange(source.Where<HabitModel>((Func<HabitModel, bool>) (local => local.SyncStatus == 2)));
        if (added.Any<HabitModel>())
        {
          int num1 = await App.Connection.InsertAllAsync((IEnumerable) added);
        }
        if (updated.Any<HabitModel>())
        {
          int num2 = await App.Connection.UpdateAllAsync((IEnumerable) updated);
        }
        if (deleted.Any<HabitModel>())
        {
          foreach (object obj in deleted)
          {
            int num3 = await App.Connection.DeleteAsync(obj);
          }
        }
      }
      else if (source.Any<HabitModel>())
      {
        foreach (HabitModel habitModel in source)
        {
          if (habitModel.SyncStatus != 0)
          {
            int num = await App.Connection.DeleteAsync((object) habitModel);
          }
        }
        return true;
      }
      return added.Any<HabitModel>() || updated.Any<HabitModel>() || deleted.Any<HabitModel>();
    }

    public static async Task<bool> MergeRemoteHabitCheckIns(
      string habitId,
      List<HabitCheckInModel> checkIns,
      int afterStamp)
    {
      if (checkIns == null)
        return false;
      List<HabitCheckInModel> added = new List<HabitCheckInModel>();
      List<HabitCheckInModel> updated = new List<HabitCheckInModel>();
      List<HabitCheckInModel> deleted = new List<HabitCheckInModel>();
      List<HabitCheckInModel> byHabitIdAndStamp = await HabitCheckInDao.GetHabitCheckInsByHabitIdAndStamp(habitId, afterStamp);
      string str = Utils.GetCurrentUserIdInt().ToString();
      List<string> stringList = new List<string>();
      for (int i = checkIns.Count - 1; i >= 0; --i)
      {
        if (stringList.Contains(checkIns[i].CheckinStamp))
        {
          HabitCheckInModel habitCheckInModel = byHabitIdAndStamp.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (model => model.Id == checkIns[i].Id));
          if (habitCheckInModel != null)
          {
            habitCheckInModel.Status = -1;
            updated.Add(habitCheckInModel);
            byHabitIdAndStamp.Remove(habitCheckInModel);
          }
          else
          {
            checkIns[i].UserId = str;
            checkIns[i].Status = -1;
            added.Add(checkIns[i]);
          }
        }
        else
        {
          stringList.Add(checkIns[i].CheckinStamp);
          HabitCheckInModel habitCheckInModel = byHabitIdAndStamp.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (model => model.Id == checkIns[i].Id));
          if (habitCheckInModel != null)
          {
            if (habitCheckInModel.Status != 2)
            {
              byHabitIdAndStamp.Remove(habitCheckInModel);
            }
            else
            {
              if (Math.Abs(habitCheckInModel.Value - checkIns[i].Value) > 0.0 || Math.Abs(habitCheckInModel.Goal - checkIns[i].Goal) > 0.0 || habitCheckInModel.CheckStatus != checkIns[i].CheckStatus)
              {
                habitCheckInModel.Value = checkIns[i].Value;
                habitCheckInModel.Goal = checkIns[i].Goal;
                habitCheckInModel.CheckinTime = checkIns[i].CheckinTime;
                habitCheckInModel.CheckStatus = checkIns[i].CheckStatus;
                updated.Add(habitCheckInModel);
              }
              byHabitIdAndStamp.Remove(habitCheckInModel);
            }
          }
          else
          {
            checkIns[i].UserId = str;
            checkIns[i].Status = 2;
            added.Add(checkIns[i]);
          }
        }
      }
      if (byHabitIdAndStamp.Any<HabitCheckInModel>())
        deleted.AddRange(byHabitIdAndStamp.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (record => record.Status == 2)));
      string log = "MergeHCI id:" + habitId;
      if (added.Any<HabitCheckInModel>())
      {
        int num = await App.Connection.InsertAllAsync((IEnumerable) added);
        log += " ,add :";
        log = added.Aggregate<HabitCheckInModel, string>(log, (Func<string, HabitCheckInModel, string>) ((current, checkIn) => current + string.Format(" {0}-{1},", (object) checkIn.CheckinStamp, (object) checkIn.CheckStatus)));
      }
      if (updated.Any<HabitCheckInModel>())
      {
        int num = await App.Connection.UpdateAllAsync((IEnumerable) updated);
        log += " ,update :";
        log = updated.Aggregate<HabitCheckInModel, string>(log, (Func<string, HabitCheckInModel, string>) ((current, checkIn) => current + string.Format(" {0}-{1},", (object) checkIn.CheckinStamp, (object) checkIn.CheckStatus)));
      }
      if (deleted.Any<HabitCheckInModel>())
      {
        log += " ,delete :";
        foreach (HabitCheckInModel delete in deleted)
        {
          int num = await App.Connection.DeleteAsync((object) delete);
          log = log + delete.CheckinStamp + ", ";
        }
      }
      bool isModify = added.Any<HabitCheckInModel>() || updated.Any<HabitCheckInModel>() || deleted.Any<HabitCheckInModel>();
      if (isModify)
      {
        await HabitDao.UpdateHabitInfo(habitId, false);
        UtilLog.Info(log);
      }
      return isModify;
    }

    public static async Task<bool> MergeRemoteHabitRecords(
      string habitId,
      IEnumerable<HabitRecordModel> records,
      int stamp)
    {
      List<HabitRecordModel> added = new List<HabitRecordModel>();
      List<HabitRecordModel> updated = new List<HabitRecordModel>();
      List<HabitRecordModel> deleted = new List<HabitRecordModel>();
      List<HabitRecordModel> recordsByHabitId = await HabitRecordDao.GetHabitRecordsByHabitId(habitId, stamp);
      string str = Utils.GetCurrentUserIdInt().ToString();
      foreach (HabitRecordModel record1 in records)
      {
        HabitRecordModel record = record1;
        HabitRecordModel habitRecordModel = recordsByHabitId.FirstOrDefault<HabitRecordModel>((Func<HabitRecordModel, bool>) (model => model.Stamp == record.Stamp));
        if (habitRecordModel != null)
        {
          if (habitRecordModel.Status == 0 || habitRecordModel.Status == 1)
          {
            recordsByHabitId.Remove(habitRecordModel);
          }
          else
          {
            if (habitRecordModel.Content != record.Content && habitRecordModel.Deleted != 1)
            {
              habitRecordModel.Content = record.Content;
              updated.Add(habitRecordModel);
            }
            recordsByHabitId.Remove(habitRecordModel);
          }
        }
        else
        {
          record.UserId = str;
          record.Status = 2;
          record.Deleted = 0;
          added.Add(record);
        }
      }
      if (recordsByHabitId.Any<HabitRecordModel>())
        deleted.AddRange(recordsByHabitId.Where<HabitRecordModel>((Func<HabitRecordModel, bool>) (record => record.Status == 2)));
      if (added.Any<HabitRecordModel>())
      {
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) added);
      }
      if (updated.Any<HabitRecordModel>())
      {
        int num2 = await App.Connection.UpdateAllAsync((IEnumerable) updated);
      }
      if (deleted.Any<HabitRecordModel>())
      {
        foreach (object obj in deleted)
        {
          int num3 = await App.Connection.DeleteAsync(obj);
        }
      }
      bool flag = added.Any<HabitRecordModel>() || updated.Any<HabitRecordModel>() || deleted.Any<HabitRecordModel>();
      added = (List<HabitRecordModel>) null;
      updated = (List<HabitRecordModel>) null;
      deleted = (List<HabitRecordModel>) null;
      return flag;
    }

    public static async Task SaveHabitSectionEtag(string sectionId, string etag)
    {
      HabitSectionModel habitSectionById = await HabitSectionDao.GetHabitSectionById(sectionId);
      if (habitSectionById == null)
        return;
      habitSectionById.Etag = etag;
      habitSectionById.SyncStatus = 2;
      int num = await App.Connection.UpdateAsync((object) habitSectionById);
    }

    public static async Task<bool> IsHabitDeleted(string habitId)
    {
      if (string.IsNullOrEmpty(habitId))
        return true;
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      return habitById == null || habitById.Status == -1;
    }

    public static async Task SkipHabit(string habitId)
    {
      HabitModel habit = await HabitDao.GetHabitById(habitId);
      if (habit == null)
      {
        habit = (HabitModel) null;
      }
      else
      {
        string[] exDates = habit.ExDates;
        List<string> stringList1 = (exDates != null ? ((IEnumerable<string>) exDates).ToList<string>() : (List<string>) null) ?? new List<string>();
        List<string> stringList2 = stringList1;
        DateTime today = DateTime.Today;
        string str1 = today.ToString("yyyyMMdd");
        if (stringList2.Contains(str1))
        {
          habit = (HabitModel) null;
        }
        else
        {
          DateTime result;
          stringList1.RemoveAll((Predicate<string>) (e => DateTime.TryParseExact(e, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result) && result <= DateTime.Today.AddDays(-30.0)));
          List<string> stringList3 = stringList1;
          today = DateTime.Today;
          string str2 = today.ToString("yyyyMMdd");
          stringList3.Add(str2);
          habit.ExDates = stringList1.ToArray();
          habit.SyncStatus = habit.SyncStatus != 0 ? 1 : 0;
          int num = await HabitDao.UpdateAsync(habit);
          HabitReminderCalculator.RecalHabitReminder(habit.Id);
          SyncManager.TryDelaySync();
          habit = (HabitModel) null;
        }
      }
    }

    public static async Task SaveHabitIcon(string habitId, string icon, string color)
    {
      HabitModel habitById = await HabitDao.GetHabitById(habitId);
      if (habitById == null || !(habitById.IconRes != icon) && !(habitById.Color != color))
        return;
      habitById.IconRes = icon;
      habitById.Color = color;
      habitById.ModifiedTime = DateTime.Now;
      habitById.SyncStatus = habitById.SyncStatus != 0 ? 1 : 0;
      int num = await HabitDao.UpdateAsync(habitById);
    }
  }
}
