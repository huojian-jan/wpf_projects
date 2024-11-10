// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TickDbHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Files;
using ticktick_WPF.Util.Sync.ClosedLoader;
using ticktick_WPF.Util.Twitter;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TickDbHelper
  {
    public static async Task CreateDbAsync()
    {
      string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      if (!Directory.Exists(folderPath + "\\Tick_Tick"))
        Directory.CreateDirectory(folderPath + "\\Tick_Tick");
      if (!Directory.Exists(folderPath + "\\Tick_Tick\\Image"))
        Directory.CreateDirectory(folderPath + "\\Tick_Tick\\Image");
      Connection.DbConnection = new SQLiteAsyncConnection(folderPath + "\\Tick_Tick\\TickTick.db");
      string version = Utils.GetVersion();
      if (File.Exists(folderPath + "\\Tick_Tick\\dbversion" + version + ".txt"))
        return;
      CreateTablesResult tableAsync1 = await App.Connection.CreateTableAsync<UserModel>();
      CreateTablesResult tableAsync2 = await App.Connection.CreateTableAsync<UserInfoModel>();
      CreateTablesResult tableAsync3 = await App.Connection.CreateTableAsync<SettingsModel>();
      CreateTablesResult tableAsync4 = await App.Connection.CreateTableAsync<ProjectModel>();
      CreateTablesResult tableAsync5 = await App.Connection.CreateTableAsync<ProjectGroupModel>();
      CreateTablesResult tableAsync6 = await App.Connection.CreateTableAsync<TaskModel>();
      CreateTablesResult tableAsync7 = await App.Connection.CreateTableAsync<TaskDetailItemModel>();
      CreateTablesResult tableAsync8 = await App.Connection.CreateTableAsync<TaskReminderModel>();
      CreateTablesResult tableAsync9 = await App.Connection.CreateTableAsync<UserPublicProfilesModel>();
      CreateTablesResult tableAsync10 = await App.Connection.CreateTableAsync<TempDataModel>();
      CreateTablesResult tableAsync11 = await App.Connection.CreateTableAsync<LimitsModel>();
      CreateTablesResult tableAsync12 = await App.Connection.CreateTableAsync<SyncStatusModel>();
      CreateTablesResult tableAsync13 = await App.Connection.CreateTableAsync<TaskSyncedJsonModel>();
      CreateTablesResult tableAsync14 = await App.Connection.CreateTableAsync<SectionStatusModel>();
      CreateTablesResult tableAsync15 = await App.Connection.CreateTableAsync<AttachmentModel>();
      CreateTablesResult tableAsync16 = await App.Connection.CreateTableAsync<TagSortTypeModel>();
      CreateTablesResult tableAsync17 = await App.Connection.CreateTableAsync<CommentModel>();
      CreateTablesResult tableAsync18 = await App.Connection.CreateTableAsync<FilterModel>();
      CreateTablesResult tableAsync19 = await App.Connection.CreateTableAsync<HolidayModel>();
      CreateTablesResult tableAsync20 = await App.Connection.CreateTableAsync<PomodoroSummaryModel>();
      CreateTablesResult tableAsync21 = await App.Connection.CreateTableAsync<PomodoroModel>();
      CreateTablesResult tableAsync22 = await App.Connection.CreateTableAsync<WidgetSettingModel>();
      CreateTablesResult tableAsync23 = await App.Connection.CreateTableAsync<CalendarWidgetModel>();
      CreateTablesResult tableAsync24 = await App.Connection.CreateTableAsync<TagModel>();
      CreateTablesResult tableAsync25 = await App.Connection.CreateTableAsync<CalendarSubscribeProfileModel>();
      CreateTablesResult tableAsync26 = await App.Connection.CreateTableAsync<CalendarEventModel>();
      CreateTablesResult tableAsync27 = await App.Connection.CreateTableAsync<BindCalendarAccountModel>();
      CreateTablesResult tableAsync28 = await App.Connection.CreateTableAsync<BindCalendarModel>();
      CreateTablesResult tableAsync29 = await App.Connection.CreateTableAsync<AppLockModel>();
      CreateTablesResult tableAsync30 = await App.Connection.CreateTableAsync<TaskDefaultModel>();
      CreateTablesResult tableAsync31 = await App.Connection.CreateTableAsync<LocationModel>();
      CreateTablesResult tableAsync32 = await App.Connection.CreateTableAsync<AttendModel>();
      CreateTablesResult tableAsync33 = await App.Connection.CreateTableAsync<ColumnModel>();
      CreateTablesResult tableAsync34 = await App.Connection.CreateTableAsync<TaskSortOrderInDateModel>();
      CreateTablesResult tableAsync35 = await App.Connection.CreateTableAsync<TaskSortOrderInPriorityModel>();
      CreateTablesResult tableAsync36 = await App.Connection.CreateTableAsync<TaskSortOrderInProjectModel>();
      CreateTablesResult tableAsync37 = await App.Connection.CreateTableAsync<TeamModel>();
      CreateTablesResult tableAsync38 = await App.Connection.CreateTableAsync<SearchHistoryModel>();
      CreateTablesResult tableAsync39 = await App.Connection.CreateTableAsync<TaskTemplateModel>();
      CreateTablesResult tableAsync40 = await App.Connection.CreateTableAsync<ArchivedEventModel>();
      CreateTablesResult tableAsync41 = await App.Connection.CreateTableAsync<ArrangeSectionStatusModel>();
      CreateTablesResult tableAsync42 = await App.Connection.CreateTableAsync<HabitModel>();
      CreateTablesResult tableAsync43 = await App.Connection.CreateTableAsync<HabitCheckInModel>();
      CreateTablesResult tableAsync44 = await App.Connection.CreateTableAsync<HabitRecordModel>();
      CreateTablesResult tableAsync45 = await App.Connection.CreateTableAsync<SkipHabitModel>();
      CreateTablesResult tableAsync46 = await App.Connection.CreateTableAsync<CommonSettings>();
      CreateTablesResult tableAsync47 = await App.Connection.CreateTableAsync<ProjectSyncedJsonModel>();
      CreateTablesResult tableAsync48 = await App.Connection.CreateTableAsync<PomoTask>();
      CreateTablesResult tableAsync49 = await App.Connection.CreateTableAsync<FilterSyncedJsonModel>();
      CreateTablesResult tableAsync50 = await App.Connection.CreateTableAsync<TagSyncedJsonModel>();
      CreateTablesResult tableAsync51 = await App.Connection.CreateTableAsync<ProjectGroupSyncedJsonModel>();
      CreateTablesResult tableAsync52 = await App.Connection.CreateTableAsync<ShowCountDownModel>();
      CreateTablesResult tableAsync53 = await App.Connection.CreateTableAsync<ClosedLoadStatus>();
      CreateTablesResult tableAsync54 = await App.Connection.CreateTableAsync<HabitSectionModel>();
      CreateTablesResult tableAsync55 = await App.Connection.CreateTableAsync<SyncSortOrderModel>();
      CreateTablesResult tableAsync56 = await App.Connection.CreateTableAsync<TaskCompletionRate>();
      CreateTablesResult tableAsync57 = await App.Connection.CreateTableAsync<CourseScheduleModel>();
      CreateTablesResult tableAsync58 = await App.Connection.CreateTableAsync<CourseModel>();
      CreateTablesResult tableAsync59 = await App.Connection.CreateTableAsync<TaskStickyModel>();
      CreateTablesResult tableAsync60 = await App.Connection.CreateTableAsync<FocusOptionModel>();
      CreateTablesResult tableAsync61 = await App.Connection.CreateTableAsync<WindowModel>();
      CreateTablesResult tableAsync62 = await App.Connection.CreateTableAsync<TimerModel>();
      CreateTablesResult tableAsync63 = await App.Connection.CreateTableAsync<TimerStatisticsModel>();
      CreateTablesResult tableAsync64 = await App.Connection.CreateTableAsync<ReminderDelayModel>();
      CreateTablesResult tableAsync65 = await App.Connection.CreateTableAsync<ReminderTimeModel>();
      await TickDbHelper.SetTaskModelIndex();
      await TickDbHelper.SetOtherModelIndex();
      FileUtils.ResetDbVersionFile();
    }

    private static async Task SetTaskModelIndex()
    {
      await TickDbHelper.AddModelIndex("TaskModel", new Dictionary<string, List<string>>()
      {
        {
          "TaskModel_status_deleted",
          new List<string>() { "status", "deleted" }
        }
      });
      await TickDbHelper.RemoveIndex("TaskModel_userId");
    }

    private static async Task SetOtherModelIndex()
    {
      await TickDbHelper.AddModelIndex("TaskDetailItemModel", new Dictionary<string, List<string>>()
      {
        {
          "TaskDetailItemModel_status_TaskServerId_startDate",
          new List<string>() { "status", "TaskServerId", "startDate" }
        }
      });
      await TickDbHelper.AddModelIndex("HabitCheckInModel", new Dictionary<string, List<string>>()
      {
        {
          "HabitCheckInModel_HabitId_CheckinStamp",
          new List<string>() { "HabitId", "CheckinStamp" }
        }
      });
    }

    private static async Task AddModelIndex(
      string tableName,
      Dictionary<string, List<string>> indexDict)
    {
      foreach (KeyValuePair<string, List<string>> keyValuePair in indexDict)
      {
        string key = keyValuePair.Key;
        List<string> items = keyValuePair.Value;
        if (items != null && items.Count != 0)
        {
          string str = items.Join<string>(",");
          int num = await App.Connection.ExecuteAsync("CREATE INDEX IF NOT EXISTS " + key + " ON " + tableName + "(" + str + ")");
        }
      }
    }

    private static async Task RemoveIndex(string indexName)
    {
      int num = await App.Connection.ExecuteAsync("DROP INDEX IF EXISTS " + indexName);
    }
  }
}
