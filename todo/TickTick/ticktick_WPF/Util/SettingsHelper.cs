// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.SettingsHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class SettingsHelper
  {
    private static bool _localChecked;
    private static long _lastPushTime;

    public static bool ShortCutChanged { get; set; }

    public static bool GetShowDetail() => LocalSettings.Settings.ShowDetails;

    public static async Task PullRemoteSettings()
    {
      SettingsHelper.PullRemotePreference();
      UserSettingsModel model = await Communicator.GetUserSettings();
      if (model == null)
        ;
      else if (!string.IsNullOrEmpty(model.errorId))
        ;
      else
        ThreadUtil.DetachedRunOnUiBackThread((Action) (async () =>
        {
          bool needPush = false;
          LocalSettings settings = LocalSettings.Settings;
          settings.DateParsing = model.nlpEnabled;
          settings.RemoveTimeText = model.removeDate;
          settings.KeepTagsInText = !model.removeTag;
          TaskDefaultModel defaultSafely = TaskDefaultDao.GetDefaultSafely();
          if (defaultSafely != null)
          {
            bool flag = false;
            if (defaultSafely.Priority != model.defaultPriority)
            {
              defaultSafely.Priority = model.defaultPriority;
              flag = true;
            }
            if (defaultSafely.AddTo != model.defaultToAdd)
            {
              defaultSafely.AddTo = model.defaultToAdd;
              flag = true;
            }
            int result;
            if (int.TryParse(model.defaultTimeMode, out result) && defaultSafely.DateMode != result)
            {
              defaultSafely.DateMode = result;
              flag = true;
            }
            if (defaultSafely.Duration != model.defaultTimeDuration)
            {
              defaultSafely.Duration = model.defaultTimeDuration;
              flag = true;
            }
            if (model.defaultReminds != null && model.defaultReminds.Any<string>())
            {
              string str = string.Join(",", model.defaultReminds.ToArray());
              if (defaultSafely.TimeReminders != str)
              {
                defaultSafely.TimeReminders = str;
                flag = true;
              }
            }
            else if (!string.IsNullOrEmpty(defaultSafely.TimeReminders))
            {
              defaultSafely.TimeReminders = string.Empty;
              flag = true;
            }
            if (model.defaultADReminders != null && model.defaultADReminders.Any<string>())
            {
              string str = string.Join(",", model.defaultADReminders.ToArray());
              if (defaultSafely.AllDayReminders != str)
              {
                defaultSafely.AllDayReminders = str;
                flag = true;
              }
            }
            else if (!string.IsNullOrEmpty(defaultSafely.AllDayReminders))
            {
              defaultSafely.AllDayReminders = string.Empty;
              flag = true;
            }
            string defaultDate = SettingsHelper.GetDefaultDate(model.defaultDueDate);
            if (defaultSafely.Date != defaultDate)
            {
              defaultSafely.Date = defaultDate;
              flag = true;
            }
            if (model.defaultTags != null && TagSerializer.ToJsonContent(model.defaultTags) != defaultSafely.TagString)
            {
              defaultSafely.Tags = model.defaultTags;
              flag = true;
            }
            if (model.defaultProjectId != null && defaultSafely.ProjectId != model.defaultProjectId)
            {
              defaultSafely.ProjectId = model.defaultProjectId;
              flag = true;
            }
            if (flag)
            {
              await TaskDefaultDao.SaveTaskDefault(defaultSafely);
              DataChangedNotifier.NotifyTaskDefaultChanged();
            }
            await ProjectDao.SaveInboxColor(model.inboxColor);
          }
          settings.SetRemoteTaskListDisplay(!model.showCompleted, model.showChecklist);
          settings.WeekStartFrom = SettingsHelper.GetWeekStartFrom(model.startDayOfWeek);
          settings.PosOfOverdue = model.posOfOverdue;
          string[] notificationOptions = model.notificationOptions;
          List<string> list1 = notificationOptions != null ? ((IEnumerable<string>) notificationOptions).ToList<string>() : (List<string>) null;
          list1?.Remove("");
          settings.NotificationOptions = list1 == null ? "" : list1.Join<string>(";");
          settings.EnableLunar = model.lunarEnabled;
          settings.EnableHoliday = model.holidayEnabled;
          settings.ShowWeek = model.weekNumbersEnabled;
          settings.EnableCountDown = model.enableCountdown;
          if (settings.StartWeekOfYear != model.startWeekOfYear)
          {
            settings.StartWeekOfYear = model.startWeekOfYear;
            DataChangedNotifier.NotifyYearStartFromChanged();
          }
          if (model.calendarViewConf != null)
          {
            settings.ShowCheckListInCal = model.calendarViewConf.showChecklist;
            settings.ShowCompletedInCal = model.calendarViewConf.showCompleted;
            settings.ShowRepeatCircles = model.calendarViewConf.showFutureTask;
            settings.ShowFocusRecord = model.calendarViewConf.showFocusRecord;
            settings.CellColorType = model.calendarViewConf.cellColorType;
            settings.HabitInCal = ((int) model.calendarViewConf.showHabit ?? (settings.HabitInCal ? 1 : 0)) != 0;
          }
          SmartProjectConf[] smartProjects = model.smartProjects;
          List<SmartProjectConf> list2 = smartProjects != null ? ((IEnumerable<SmartProjectConf>) smartProjects).ToList<SmartProjectConf>() : (List<SmartProjectConf>) null;
          SmartProjectConf smartProjectConf1 = list2 != null ? list2.FirstOrDefault<SmartProjectConf>((Func<SmartProjectConf, bool>) (p => p.name == "subscribe")) : (SmartProjectConf) null;
          if (smartProjectConf1 != null)
          {
            SmartProjectConf smartProjectConf2 = list2.FirstOrDefault<SmartProjectConf>((Func<SmartProjectConf, bool>) (p => p.name == "subscribedCalendar"));
            if (smartProjectConf2 == null)
            {
              needPush = true;
              smartProjectConf1.name = "subscribedCalendar";
            }
            else
            {
              needPush = true;
              list2.Remove(smartProjectConf1);
              smartProjectConf2.order = smartProjectConf1.order;
            }
          }
          settings.SmartProjects = list2 ?? new List<SmartProjectConf>();
          settings.EnableTimeZone = model.isTimeZoneOptionEnabled;
          if (model.shortcutItemConfPc != null && !string.IsNullOrEmpty(model.shortcutItemConfPc) && !"null".Equals(model.shortcutItemConfPc))
          {
            settings.ShortCutModel.CopyProperties(JsonConvert.DeserializeObject<ShortcutModel>(model.shortcutItemConfPc));
            JObject jobject = JObject.Parse(model.shortcutItemConfPc);
            if (jobject["ShortcutAddTask"] != null)
              LocalSettings.Settings.ShortcutAddTask = jobject["ShortcutAddTask"].ToString();
            if (jobject["ShortcutOpenOrClose"] != null)
              LocalSettings.Settings.ShortcutOpenOrClose = jobject["ShortcutOpenOrClose"].ToString();
            if (jobject["ShortcutPin"] != null)
              LocalSettings.Settings.ShortcutPin = jobject["ShortcutPin"].ToString();
            if (jobject["LockShortcut"] != null)
              LocalSettings.Settings.LockShortcut = jobject["LockShortcut"].ToString();
            if (jobject["PomoShortcut"] != null)
              LocalSettings.Settings.PomoShortcut = jobject["PomoShortcut"].ToString();
            if (jobject["CreateStickyShortcut"] != null)
              LocalSettings.Settings.CreateStickyShortCut = jobject["CreateStickyShortcut"].ToString();
            if (jobject["ShowHideStickyShortCut"] != null)
              LocalSettings.Settings.ShowHideStickyShortCut = jobject["ShowHideStickyShortCut"].ToString();
          }
          settings.Save();
          bool pullTimeZone = false;
          if (!SettingsHelper._localChecked)
          {
            SettingsHelper._localChecked = true;
            if (model.timeZone == null || model.locale == null)
            {
              needPush = true;
              pullTimeZone = true;
            }
          }
          if (!needPush)
          {
            settings = (LocalSettings) null;
          }
          else
          {
            await SettingsHelper.PushLocalSettings(pullTimeZone);
            settings = (LocalSettings) null;
          }
        }));
    }

    public static async Task PushLocalSettings(bool pullTimeZone = false)
    {
      UserSettingsModel model;
      LocalSettings settings;
      if (!Utils.IsNetworkAvailable())
      {
        model = (UserSettingsModel) null;
        settings = (LocalSettings) null;
      }
      else
      {
        model = new UserSettingsModel();
        settings = LocalSettings.Settings;
        settings.Save();
        model.nlpEnabled = settings.DateParsing;
        model.nlpenabled = settings.DateParsing;
        model.removeDate = settings.RemoveTimeText;
        model.removeTag = !settings.KeepTagsInText;
        TaskDefaultModel taskDefault = await TaskDefaultDao.GetTaskDefault();
        if (taskDefault != null)
        {
          model.defaultPriority = taskDefault.Priority;
          model.defaultToAdd = taskDefault.AddTo;
          model.defaultTimeMode = taskDefault.DateMode.ToString();
          model.defaultTimeDuration = taskDefault.Duration;
          UserSettingsModel userSettingsModel1 = model;
          List<string> stringList1;
          if (string.IsNullOrEmpty(taskDefault.TimeReminders))
            stringList1 = new List<string>();
          else
            stringList1 = ((IEnumerable<string>) taskDefault.TimeReminders.Split(',')).ToList<string>();
          userSettingsModel1.defaultReminds = stringList1;
          UserSettingsModel userSettingsModel2 = model;
          List<string> stringList2;
          if (string.IsNullOrEmpty(taskDefault.AllDayReminders))
            stringList2 = new List<string>();
          else
            stringList2 = ((IEnumerable<string>) taskDefault.AllDayReminders.Split(',')).ToList<string>();
          userSettingsModel2.defaultADReminders = stringList2;
          model.defaultDueDate = Constants.DateValue.ToIndex(taskDefault.Date);
          model.defaultTags = taskDefault.Tags;
          model.defaultProjectId = !string.IsNullOrEmpty(taskDefault.ProjectId) ? taskDefault.ProjectId : LocalSettings.Settings.InServerBoxId;
        }
        model.showCompleted = !settings.HideComplete;
        model.showFutureTask = settings.ShowRepeatCircles;
        model.showChecklist = settings.ShowSubtasks;
        model.startDayOfWeek = SettingsHelper.GetWeekStart(settings.WeekStartFrom);
        model.posOfOverdue = settings.PosOfOverdue;
        UserSettingsModel userSettingsModel3 = model;
        string notificationOptions = settings.NotificationOptions;
        string[] strArray;
        if (notificationOptions == null)
          strArray = (string[]) null;
        else
          strArray = notificationOptions.Split(';');
        userSettingsModel3.notificationOptions = strArray;
        UserSettingsModel userSettingsModel = model;
        userSettingsModel.inboxColor = (await ProjectDao.GetProjectById(Utils.GetInboxId()))?.color;
        userSettingsModel = (UserSettingsModel) null;
        model.holidayEnabled = settings.EnableHoliday;
        model.lunarEnabled = settings.EnableLunar;
        model.weekNumbersEnabled = settings.ShowWeek;
        model.enableCountdown = settings.EnableCountDown;
        model.calendarViewConf = new CalendarViewConf()
        {
          showChecklist = settings.ShowCheckListInCal,
          showCompleted = settings.ShowCompletedInCal,
          showFutureTask = settings.ShowRepeatCircles,
          showFocusRecord = settings.ShowFocusRecord,
          cellColorType = settings.CellColorType,
          showHabit = new bool?(settings.HabitInCal)
        };
        model.isTimeZoneOptionEnabled = settings.EnableTimeZone;
        if (pullTimeZone)
        {
          model.timeZone = TimeZoneData.LocalTimeZoneModel?.TimeZoneName;
          model.locale = CultureInfo.InstalledUICulture.Name.Replace("-", "_");
        }
        List<SmartProjectConf> smartProjects = settings.SmartProjects;
        // ISSUE: explicit non-virtual call
        if ((smartProjects != null ? (__nonvirtual (smartProjects.Count) > 0 ? 1 : 0) : 0) != 0)
          model.smartProjects = settings.SmartProjects.ToArray();
        JObject jobject = JObject.FromObject((object) settings.ShortCutModel);
        jobject.Add("ShortcutAddTask", (JToken) LocalSettings.Settings.ShortcutAddTask);
        jobject.Add("ShortcutOpenOrClose", (JToken) LocalSettings.Settings.ShortcutOpenOrClose);
        jobject.Add("ShortcutPin", (JToken) LocalSettings.Settings.ShortcutPin);
        jobject.Add("LockShortcut", (JToken) LocalSettings.Settings.LockShortcut);
        jobject.Add("PomoShortcut", (JToken) LocalSettings.Settings.PomoShortcut);
        jobject.Add("CreateStickyShortcut", (JToken) LocalSettings.Settings.CreateStickyShortCut);
        model.shortcutItemConfPc = jobject.ToString();
        await Communicator.SaveUserSettings(model);
        model = (UserSettingsModel) null;
        settings = (LocalSettings) null;
      }
    }

    public static async Task<UserPreferenceModel> PullRemotePreference(long? checkPoint = null)
    {
      LocalSettings settings = LocalSettings.Settings;
      UserPreferenceModel userPreference = await Communicator.GetUserPreference(checkPoint ?? settings.SettingsModel.PreferrenceMTime);
      LocalSettings.Settings.SetRemotePreference(userPreference);
      SettingsHelper._lastPushTime = settings.SettingsModel.PreferrenceMTime;
      UserPreferenceModel userPreferenceModel = userPreference;
      settings = (LocalSettings) null;
      return userPreferenceModel;
    }

    public static async Task PushLocalPreference()
    {
      LocalSettings settings = LocalSettings.Settings;
      if (settings.UserPreference.matrix != null)
        settings.UserPreference.matrix.show_completed = new bool?(((int) settings.UserPreference.matrix.show_completed ?? 1) != 0);
      settings.UserPreference.mtime = settings.UserPreference.GetMaxMTime();
      if (SettingsHelper._lastPushTime >= settings.UserPreference.mtime)
        settings = (LocalSettings) null;
      else if (!await Communicator.SaveUserPreference(settings.UserPreference))
      {
        settings = (LocalSettings) null;
      }
      else
      {
        SettingsHelper._lastPushTime = settings.UserPreference.mtime;
        settings = (LocalSettings) null;
      }
    }

    private static string GetDefaultDate(int model) => Constants.DateValue.ToValue(model);

    private static string GetWeekStart(string dayOfWeek) => dayOfWeek.Substring(0, 3).ToUpper();

    private static string GetWeekStartFrom(string dayOfWeek)
    {
      switch (dayOfWeek)
      {
        case "SUN":
          return "Sunday";
        case "MON":
          return "Monday";
        case "SAT":
          return "Saturday";
        default:
          return "Sunday";
      }
    }

    public static bool GetShowHoliday() => LocalSettings.Settings.EnableHoliday;
  }
}
