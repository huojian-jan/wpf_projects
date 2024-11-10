// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.SystemToastUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util.Toast;
using ticktick_WPF.Views.Habit;
using TickTickUtils;
using TickTickUtils.Lang;
using Windows.Foundation.Collections;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class SystemToastUtils
  {
    private static string _logoUrl;
    private static string _appid = "滴答清单";
    private static Dictionary<string, RemindMessage> RemindingDict = new Dictionary<string, RemindMessage>();

    static SystemToastUtils()
    {
      if (Utils.IsWindows7())
        return;
      try
      {
        DesktopNotificationManagerCompat.RegisterAumidAndComServer<MyNotificationActivator>(SystemToastUtils._appid);
        DesktopNotificationManagerCompat.RegisterActivator<MyNotificationActivator>();
        AppIconKey appIconKey1;
        string appIconKey2;
        if (!string.IsNullOrEmpty(LocalSettings.Settings.Common.AppIconKey))
        {
          string appIconKey3 = LocalSettings.Settings.Common.AppIconKey;
          appIconKey1 = AppIconKey.Default;
          string str = appIconKey1.ToString();
          if (!(appIconKey3 == str))
          {
            appIconKey2 = LocalSettings.Settings.Common.AppIconKey;
            goto label_5;
          }
        }
        appIconKey1 = AppIconKey.SClassic;
        appIconKey2 = appIconKey1.ToString();
label_5:
        SystemToastUtils._logoUrl = AppPaths.AppIconDir + appIconKey2 + ".png";
        ToastNotificationManagerCompat.OnActivated += (OnActivated) (toastArgs =>
        {
          ToastArguments toastArguments = ToastArguments.Parse(toastArgs.Argument);
          ValueSet userInput = toastArgs.UserInput;
          List<KeyValuePair<string, object>> list = userInput != null ? ((IEnumerable<KeyValuePair<string, object>>) userInput).ToList<KeyValuePair<string, object>>() : (List<KeyValuePair<string, object>>) null;
          SystemToastUtils.OnToastCallBack(toastArguments["action"], list);
        });
      }
      catch (Exception ex)
      {
        UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
      }
      AppIconUtils.TrySetToastIcon();
    }

    public static void SetLogoUrl()
    {
      AppIconKey appIconKey1;
      string appIconKey2;
      if (!string.IsNullOrEmpty(LocalSettings.Settings.Common.AppIconKey))
      {
        string appIconKey3 = LocalSettings.Settings.Common.AppIconKey;
        appIconKey1 = AppIconKey.Default;
        string str = appIconKey1.ToString();
        if (!(appIconKey3 == str))
        {
          appIconKey2 = LocalSettings.Settings.Common.AppIconKey;
          goto label_4;
        }
      }
      appIconKey1 = AppIconKey.SClassic;
      appIconKey2 = appIconKey1.ToString();
label_4:
      SystemToastUtils._logoUrl = AppPaths.AppIconDir + appIconKey2 + ".png";
    }

    public static async void OnToastCallBack(string arg, List<KeyValuePair<string, object>> kvs)
    {
      List<KeyValuePair<string, object>> source = kvs;
      UtilLog.Info((source != null ? source.Aggregate<KeyValuePair<string, object>, string>("SystemToastCallBack " + arg, (Func<string, KeyValuePair<string, object>, string>) ((current, kv) => string.Format(",{0},{1},{2}", (object) current, (object) kv.Key, kv.Value))) : (string) null) ?? "SystemToastCallBack " + arg);
      ToastOptionModel optionModel = ToastOptionModel.FromJson(arg);
      string key = optionModel?.Id ?? arg;
      RemindMessage remindMessage = (RemindMessage) null;
      if (!string.IsNullOrEmpty(key) && SystemToastUtils.RemindingDict.ContainsKey(key))
      {
        remindMessage = SystemToastUtils.RemindingDict[key];
        if (remindMessage != null)
          Communicator.NotifyCloseReminder(remindMessage);
        SystemToastUtils.RemindingDict.Remove(key);
      }
      if (optionModel == null)
      {
        if (remindMessage == null)
          return;
        Application.Current?.Dispatcher?.Invoke<Task>((Func<Task>) (async () => SystemToastUtils.TryShowDetail(remindMessage.id, remindMessage.type)));
      }
      else
        Application.Current?.Dispatcher?.Invoke<Task>((Func<Task>) (async () => ToastHandlerBuilder.Build(optionModel.ToastType)?.Exec(optionModel, kvs)));
    }

    public static async void TryShowDetail(string id, string type)
    {
      string ctype = "task_reminder_system";
      switch (type)
      {
        case "task":
          if (await TaskDao.GetThinTaskById(id) != null)
          {
            App.ShowMainWindow(id);
            break;
          }
          break;
        case "checklist":
          TaskDetailItemModel checklistItemById = await TaskDetailItemDao.GetChecklistItemById(id);
          if (checklistItemById != null)
          {
            App.ShowMainWindow(checklistItemById.TaskServerId);
            break;
          }
          break;
        case "calendar":
          CalendarEventModel eventByEventId = await CalendarEventDao.GetEventByEventId(id);
          if (eventByEventId != null)
            App.NavigateEvent(eventByEventId.Id);
          ctype = "calendar_reminder_system";
          break;
        case "habit":
          HabitModel habitById = await HabitDao.GetHabitById(id);
          if (habitById != null)
            App.NavigateHabit(habitById.Id);
          ctype = "habit_reminder_system";
          break;
        case "course":
          ctype = "timetable_reminder_system";
          break;
      }
      UserActCollectUtils.AddClickEvent("reminder", ctype, "click_content");
      ctype = (string) null;
    }

    public static async Task SystemToast(ReminderModel reminder, bool playSound)
    {
      ReminderModel reminderModel = reminder;
      if ((reminderModel != null ? (!reminderModel.ReminderTime.HasValue ? 1 : 0) : 1) != 0)
        ;
      else if (reminder.Type == 2)
      {
        await SystemToastUtils.ToastEvent(reminder);
        UserActCollectUtils.AddClickEvent("reminder_data", "type", "calendar_reminder");
      }
      else
      {
        UserActCollectUtils.AddClickEvent("reminder_data", "type", "task_reminder");
        TaskModel task = await TaskDao.GetThinTaskById(reminder.TaskId);
        bool showButton = true;
        if (task == null)
          ;
        else
        {
          string title = task.title;
          string content;
          if (reminder.Type == 0)
          {
            content = TaskUtils.ReplaceAttachmentTextInString(task.kind != "CHECKLIST" ? task.content : task.desc);
          }
          else
          {
            TaskDetailItemModel checklistItemById = await TaskDetailItemDao.GetChecklistItemById(reminder.CheckItemId);
            if (checklistItemById == null)
              return;
            content = "- " + checklistItemById.title;
          }
          if (!string.IsNullOrEmpty(task.attendId) && !AgendaHelper.CanAccessAgenda((AgendaHelper.IAgenda) task))
            showButton = false;
          ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == task.projectId));
          if (projectModel != null && !projectModel.IsEnable())
            showButton = false;
          bool flag1 = LocalSettings.Settings.ExtraSettings.RemindDetail == 2;
          if (!flag1)
          {
            bool flag2 = LocalSettings.Settings.ExtraSettings.RemindDetail == 1;
            if (flag2)
              flag2 = await AppLockCache.GetAppLocked();
            flag1 = flag2;
          }
          if (flag1)
          {
            title = Utils.GetString("ReminderWhenLock");
            content = string.Empty;
            showButton = false;
          }
          SystemToastUtils.TryToast(reminder, SystemToastUtils._logoUrl, title, content, showButton, task.projectId, task.kind != "NOTE", playSound);
          title = (string) null;
          content = (string) null;
        }
      }
    }

    private static async Task ToastEvent(ReminderModel reminder)
    {
      CalendarEventModel model;
      if (reminder == null)
      {
        model = (CalendarEventModel) null;
      }
      else
      {
        CalendarEventModel calendarEventModel = await CalendarEventDao.GetEventById(reminder.EventId);
        if (calendarEventModel == null)
          calendarEventModel = await CalendarEventDao.GetEventByEventId(reminder.EventId);
        model = calendarEventModel;
        bool flag1 = LocalSettings.Settings.ExtraSettings.RemindDetail == 2;
        if (!flag1)
        {
          bool flag2 = LocalSettings.Settings.ExtraSettings.RemindDetail == 1;
          if (flag2)
            flag2 = await AppLockCache.GetAppLocked();
          flag1 = flag2;
        }
        bool flag3 = flag1;
        if (model == null)
          model = (CalendarEventModel) null;
        else if (!reminder.ReminderTime.HasValue)
        {
          model = (CalendarEventModel) null;
        }
        else
        {
          string title = flag3 ? Utils.GetString("ReminderWhenLock") : model.Title;
          string content = flag3 ? string.Empty : model.Content;
          bool showButton = !flag3;
          SystemToastUtils.TryToast(reminder, SystemToastUtils._logoUrl, title, content, showButton, model.CalendarId);
          model = (CalendarEventModel) null;
        }
      }
    }

    public static void ToastPomo(bool isCompleted, bool automatic)
    {
      ToastOptionModel toastOptionModel = new ToastOptionModel()
      {
        ToastType = isCompleted ? ToastType.PomoCompleted : ToastType.PomoRelaxCompleted
      };
      string content1;
      string str;
      if (isCompleted)
      {
        content1 = Utils.GetString("TimeToRelax");
        str = Utils.GetString("Relax");
      }
      else
      {
        content1 = Utils.GetString("TimeToFocus");
        str = Utils.GetString("StartFocus");
      }
      SystemToastUtils.RemovePomoToast();
      if (automatic)
      {
        SystemToastManager.Notify("focus", "focus", SystemToastUtils._logoUrl, string.Empty, content1, silent: true);
        Task.Delay(3000).ContinueWith((Action<Task>) (task => SystemToastUtils.RemovePomoToast()));
      }
      else
      {
        string logoUrl = SystemToastUtils._logoUrl;
        string empty = string.Empty;
        string content2 = content1;
        List<ToastCommands> commands = new List<ToastCommands>();
        ToastCommands toastCommands = new ToastCommands();
        toastCommands.Content = str;
        toastCommands.Argument = toastOptionModel.ToJson("Complete");
        commands.Add(toastCommands);
        toastCommands = new ToastCommands();
        toastCommands.Content = Utils.GetString("Exit");
        toastCommands.Argument = toastOptionModel.ToJson("Exit");
        commands.Add(toastCommands);
        SystemToastManager.Notify("focus", "focus", logoUrl, empty, content2, commands: commands, silent: true);
      }
    }

    public static async Task ToastHabit(ReminderModel reminder, bool playSound)
    {
      HabitModel habitModel = await HabitDao.GetHabitById(reminder.HabitId);
      Action notify;
      if (habitModel == null)
      {
        notify = (Action) null;
      }
      else
      {
        string fileName = AppPaths.DataDir + "Image\\" + habitModel.Id + ".png";
        Application.Current?.Dispatcher.Invoke((Action) (() => ThemeUtil.SaveBitmapToPng(fileName, HabitService.GetIcon(habitModel.IconRes, habitModel.Color))));
        ToastOptionModel toastOptionModel = new ToastOptionModel();
        toastOptionModel.TargetId = habitModel.Id;
        toastOptionModel.Parma = (object) reminder.ReminderTime;
        DateTime? nullable = reminder.StartDate;
        toastOptionModel.StartTime = nullable ?? DateTime.Now;
        ToastOptionModel optModel = toastOptionModel;
        string unit = HabitUtils.GetUnitText(habitModel.Unit);
        List<HabitCheckInModel> checkInsByHabitId = await HabitCheckInDao.GetHabitCheckInsByHabitId(habitModel.Id);
        double todayCheckCount = 0.0;
        if (checkInsByHabitId.Any<HabitCheckInModel>())
        {
          HabitCheckInModel habitCheckInModel = checkInsByHabitId.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (checkIn => checkIn.CheckinStamp == DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture)));
          if (habitCheckInModel != null)
            todayCheckCount = habitCheckInModel.Value;
        }
        HabitCheckInType checkInType = HabitUtils.GetHabitCheckInType(0, habitModel.Type, habitModel.Step);
        notify = (Action) null;
        bool flag1 = LocalSettings.Settings.ExtraSettings.RemindDetail == 2;
        if (!flag1)
        {
          bool flag2 = LocalSettings.Settings.ExtraSettings.RemindDetail == 1;
          if (flag2)
            flag2 = await AppLockCache.GetAppLocked();
          flag1 = flag2;
        }
        if (flag1)
        {
          notify = (Action) (() => SystemToastManager.Notify(optModel.Id, "habit", SystemToastUtils._logoUrl, Utils.GetString("ReminderWhenLock"), string.Empty, silent: !playSound));
        }
        else
        {
          nullable = reminder.ReminderTime;
          Dictionary<string, List<ToastCommands>> reminders = SystemToastUtils.GetRemindersByTime(nullable ?? DateTime.Now, false);
          switch (checkInType)
          {
            case HabitCheckInType.BoolCompleted:
            case HabitCheckInType.BoolUnCompleted:
            case HabitCheckInType.CompletedAllCompleted:
            case HabitCheckInType.CompletedAllUnCompleted:
              optModel.ToastType = ToastType.BoolHabitCompleted;
              notify = (Action) (() =>
              {
                string id = optModel.Id;
                string img = fileName;
                string name = habitModel.Name;
                string empty = string.Empty;
                Dictionary<string, List<ToastCommands>> paras = reminders;
                List<ToastCommands> commands = new List<ToastCommands>();
                commands.Add(new ToastCommands()
                {
                  Content = Utils.GetString("PublicComplete"),
                  Argument = optModel.ToJson("Complete")
                });
                commands.Add(new ToastCommands()
                {
                  Content = Utils.GetString("RemindLater"),
                  Argument = optModel.ToJson("Delay")
                });
                int num = !playSound ? 1 : 0;
                SystemToastManager.Notify(id, "habit", img, name, empty, paras, commands, num != 0);
              });
              break;
            case HabitCheckInType.AutoCompleted:
            case HabitCheckInType.AutoUncompleted:
              optModel.ToastType = ToastType.MultiHabitCompleted;
              notify = (Action) (() =>
              {
                string id = optModel.Id;
                string img = fileName;
                string name = habitModel.Name;
                string content = string.Format("{0} / {1} {2}", (object) todayCheckCount, (object) habitModel.Goal, (object) unit);
                Dictionary<string, List<ToastCommands>> paras = reminders;
                List<ToastCommands> commands = new List<ToastCommands>();
                commands.Add(new ToastCommands()
                {
                  Content = string.Format("+{0} {1}", (object) habitModel.Step, (object) unit),
                  Argument = optModel.ToJson("AddStep")
                });
                commands.Add(new ToastCommands()
                {
                  Content = Utils.GetString("ManuallyRecord"),
                  Argument = optModel.ToJson("Manual")
                });
                commands.Add(new ToastCommands()
                {
                  Content = Utils.GetString("RemindLater"),
                  Argument = optModel.ToJson("Delay")
                });
                int num = !playSound ? 1 : 0;
                SystemToastManager.Notify(id, "habit", img, name, content, paras, commands, num != 0);
              });
              break;
            case HabitCheckInType.ManualCompleted:
            case HabitCheckInType.ManualUnCompleted:
              optModel.ToastType = ToastType.MultiHabitCompleted;
              notify = (Action) (() =>
              {
                string id = optModel.Id;
                string img = fileName;
                string name = habitModel.Name;
                string content = string.Format("{0}/{1} {2}", (object) todayCheckCount, (object) habitModel.Goal, (object) unit);
                Dictionary<string, List<ToastCommands>> paras = reminders;
                List<ToastCommands> commands = new List<ToastCommands>();
                commands.Add(new ToastCommands()
                {
                  Content = Utils.GetString("ManuallyRecord"),
                  Argument = optModel.ToJson("Manual")
                });
                commands.Add(new ToastCommands()
                {
                  Content = Utils.GetString("RemindLater"),
                  Argument = optModel.ToJson("Delay")
                });
                int num = !playSound ? 1 : 0;
                SystemToastManager.Notify(id, "habit", img, name, content, paras, commands, num != 0);
              });
              break;
          }
        }
        if (notify != null)
        {
          SystemToastUtils.RemoveHabitToast(reminder.HabitId);
          notify();
          RemindMessage remindMessage = new RemindMessage(reminder);
          if (!string.IsNullOrEmpty(optModel.Id))
          {
            try
            {
              SystemToastUtils.RemindingDict[optModel.Id] = remindMessage;
            }
            catch (Exception ex)
            {
              UtilLog.Info(string.Format("SystemToast.TryToast() : RemindingDict {0} , optModel.Id {1}", (object) (SystemToastUtils.RemindingDict == null), (object) (optModel.Id == null)));
            }
          }
        }
        Task.Delay(3000).ContinueWith((Action<Task>) (task => File.Delete(fileName)));
        notify = (Action) null;
      }
    }

    private static Dictionary<string, List<ToastCommands>> GetRemindersByTime(
      DateTime date,
      bool withTomorrow = true)
    {
      int num1 = date.Hour * 60 + date.Minute;
      if (num1 > 540 && num1 > 780 && num1 > 1020 && num1 > 1200)
      {
        int num2 = withTomorrow ? 1 : 0;
      }
      List<ToastCommands> toastCommandsList1 = new List<ToastCommands>();
      toastCommandsList1.Add(new ToastCommands()
      {
        Content = Utils.GetString("Snooze15m"),
        Argument = "15mins"
      });
      ToastCommands toastCommands1 = new ToastCommands();
      toastCommands1.Content = Utils.GetString("Snooze30m");
      toastCommands1.Argument = "30mins";
      toastCommandsList1.Add(toastCommands1);
      toastCommands1 = new ToastCommands();
      toastCommands1.Content = Utils.GetString("Snooze1h");
      toastCommands1.Argument = "1hr";
      toastCommandsList1.Add(toastCommands1);
      toastCommands1 = new ToastCommands();
      toastCommands1.Content = Utils.GetString("Snooze3h");
      toastCommands1.Argument = "3hrs";
      toastCommandsList1.Add(toastCommands1);
      List<ToastCommands> toastCommandsList2 = toastCommandsList1;
      Dictionary<string, List<ToastCommands>> remindersByTime = new Dictionary<string, List<ToastCommands>>();
      remindersByTime.Add("Snooze", toastCommandsList2);
      if (!withTomorrow)
        return remindersByTime;
      List<ToastCommands> toastCommandsList3 = toastCommandsList2;
      toastCommands1 = new ToastCommands();
      toastCommands1.Content = string.Format(Utils.GetString("SnoozeUntil"), (object) Utils.GetString("Tomorrow"));
      toastCommands1.Argument = "1day";
      ToastCommands toastCommands2 = toastCommands1;
      toastCommandsList3.Add(toastCommands2);
      return remindersByTime;
    }

    private static void TryToast(
      ReminderModel reminderModel,
      string img,
      string title,
      string content,
      bool showButton,
      string projectId,
      bool isTask = false,
      bool playSound = true,
      bool showTomorrow = true)
    {
      string str1 = title;
      if ((str1 != null ? (str1.Length > 128 ? 1 : 0) : 0) != 0)
        title = title.Substring(0, (int) sbyte.MaxValue);
      string str2 = content;
      if ((str2 != null ? (str2.Length > 256 ? 1 : 0) : 0) != 0)
        content = content.Substring(0, (int) byte.MaxValue);
      ToastOptionModel optModel = new ToastOptionModel(reminderModel);
      Action action;
      if (showButton)
      {
        Dictionary<string, List<ToastCommands>> reminders = SystemToastUtils.GetRemindersByTime(reminderModel.ReminderTime ?? DateTime.Now, showTomorrow);
        action = (Action) (() =>
        {
          string id = optModel.Id;
          string group = projectId;
          string img1 = img;
          string title1 = title ?? "";
          string content1 = content ?? "";
          Dictionary<string, List<ToastCommands>> paras = reminders;
          List<ToastCommands> commands;
          if (!isTask)
          {
            List<ToastCommands> toastCommandsList = new List<ToastCommands>();
            ToastCommands toastCommands = new ToastCommands();
            toastCommands.Content = Utils.GetString("GotIt");
            toastCommands.Argument = optModel.ToJson("GotIt");
            toastCommandsList.Add(toastCommands);
            toastCommands = new ToastCommands();
            toastCommands.Content = Utils.GetString("RemindLater");
            toastCommands.Argument = optModel.ToJson("Delay");
            toastCommandsList.Add(toastCommands);
            commands = toastCommandsList;
          }
          else
          {
            commands = new List<ToastCommands>();
            ToastCommands toastCommands = new ToastCommands();
            toastCommands.Content = Utils.GetString(reminderModel.Type == 1 ? "CompleteCheckItem" : "TaskComplete");
            toastCommands.Argument = optModel.ToJson("Complete");
            commands.Add(toastCommands);
            toastCommands = new ToastCommands();
            toastCommands.Content = Utils.GetString("RemindLater");
            toastCommands.Argument = optModel.ToJson("Delay");
            commands.Add(toastCommands);
          }
          int num = !playSound ? 1 : 0;
          SystemToastManager.Notify(id, group, img1, title1, content1, paras, commands, num != 0);
        });
      }
      else
        action = (Action) (() => SystemToastManager.Notify(optModel.Id, projectId, img, title ?? "", content ?? "", silent: !playSound));
      RemindMessage remindMessage = new RemindMessage(reminderModel);
      SystemToastUtils.RemoveToast(remindMessage.id, projectId);
      action();
      if (string.IsNullOrEmpty(optModel.Id))
        return;
      try
      {
        SystemToastUtils.RemindingDict[optModel.Id] = remindMessage;
      }
      catch (Exception ex)
      {
        UtilLog.Info(string.Format("SystemToast.TryToast() : RemindingDict {0} , optModel.Id {1}", (object) (SystemToastUtils.RemindingDict == null), (object) (optModel.Id == null)));
      }
    }

    public static void ClearToastHistory()
    {
      if (Utils.IsWindows7())
        return;
      try
      {
        ToastNotificationManagerCompat.History.Clear();
      }
      catch (Exception ex)
      {
      }
    }

    public static void RemoveHabitToast(string habitId)
    {
      if (Utils.IsWindows7())
        return;
      try
      {
        foreach (KeyValuePair<string, RemindMessage> keyValuePair in SystemToastUtils.RemindingDict)
        {
          if (keyValuePair.Value.id == habitId)
            ToastNotificationManagerCompat.History.Remove(keyValuePair.Key, "habit");
        }
      }
      catch (Exception ex)
      {
      }
    }

    public static void RemovePomoToast()
    {
      if (Utils.IsWindows7())
        return;
      try
      {
        ToastNotificationManagerCompat.History.RemoveGroup("focus");
      }
      catch (Exception ex)
      {
      }
    }

    public static void RemoveToast(string id, string projectId)
    {
      if (Utils.IsWindows7())
        return;
      try
      {
        foreach (KeyValuePair<string, RemindMessage> keyValuePair in SystemToastUtils.RemindingDict)
        {
          if (keyValuePair.Value.id == id)
            ToastNotificationManagerCompat.History.Remove(keyValuePair.Key, projectId);
        }
      }
      catch (Exception ex)
      {
      }
    }

    public static void RemoveProjectToast(string projectId)
    {
      if (Utils.IsWindows7())
        return;
      try
      {
        ToastNotificationManagerCompat.History.RemoveGroup(projectId);
      }
      catch (Exception ex)
      {
      }
    }

    public static bool CheckSystemToastEnable()
    {
      return !Utils.IsWindows7() && SystemToastUtils.CheckToastEnable();
    }

    private static bool CheckToastEnable()
    {
      try
      {
        return ToastNotificationManagerCompat.CreateToastNotifier().Setting == 0;
      }
      catch (Exception ex)
      {
        UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
      }
      return false;
    }

    public static async Task ToastCourse(ReminderModel reminder, bool playSound, bool showButton = true)
    {
      SystemToastUtils.TryToast(reminder, SystemToastUtils._logoUrl, reminder.Title, reminder.Content, showButton, reminder.GroupId, playSound: playSound, showTomorrow: false);
    }
  }
}
