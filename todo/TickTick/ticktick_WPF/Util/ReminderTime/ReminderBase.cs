// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ReminderTime.ReminderBase
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Remind;
using TickTickDao;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.ReminderTime
{
  public class ReminderBase
  {
    public static TTAsyncLocker Locker = new TTAsyncLocker(1, 1);

    public static bool? UseNewReminderCalculate { get; set; }

    public static async Task InitReminders(bool force)
    {
      bool flag = ABTestManager.IsNewRemindCalculate();
      ReminderBase.UseNewReminderCalculate = new bool?(flag);
      if (!flag || !force && LocalSettings.Settings.ExtraSettings.ReminderCalculated)
      {
        if (!flag)
          return;
        await ReminderBase.LoadAllRepeatReminders();
      }
      else
      {
        LocalSettings.Settings.ExtraSettings.ReminderCalculated = true;
        await ReminderBase.Locker.RunAsync((Func<Task>) (async () =>
        {
          await ReminderTimeDao.DeleteAllReminderTime();
          List<TaskBaseViewModel> tasks = TaskCache.GetAllTask();
          Dictionary<string, List<TaskReminderModel>> reminderDict = (await TaskReminderDao.GetAllReminders()).GroupEx<TaskReminderModel, string, TaskReminderModel>((Func<TaskReminderModel, string>) (r => r.taskserverid), (Func<TaskReminderModel, TaskReminderModel>) (r => r));
          foreach (TaskBaseViewModel task in tasks)
          {
            List<TaskReminderModel> reminderModels;
            if (string.IsNullOrEmpty(task.RepeatFlag) && !string.IsNullOrEmpty(task.Id) && reminderDict.TryGetValue(task.Id, out reminderModels))
              await ReminderTimeDao.AddReminderTimes(TaskReminderCalculator.GetTaskReminders(task, reminderModels));
          }
          foreach (TaskBaseViewModel allCheckItem in TaskDetailItemCache.GetAllCheckItems())
          {
            ReminderTimeModel checkItemReminder = CheckItemReminderCalculator.GetCheckItemReminder(allCheckItem);
            if (checkItemReminder != null)
              await ReminderTimeDao.AddReminderTimes(new List<ReminderTimeModel>()
              {
                checkItemReminder
              });
          }
          tasks = (List<TaskBaseViewModel>) null;
          reminderDict = (Dictionary<string, List<TaskReminderModel>>) null;
        }));
        await ReminderBase.LoadAllRepeatReminders();
      }
    }

    public static async Task LoadAllRepeatReminders()
    {
      await ReminderBase.Locker.RunAsync((Func<Task>) (async () =>
      {
        foreach (TaskBaseViewModel taskBaseViewModel in TaskCache.GetAllTask())
        {
          TaskBaseViewModel task = taskBaseViewModel;
          if (task != null && task.Deleted == 0 && task.Status == 0 && !string.IsNullOrEmpty(task.RepeatFlag))
          {
            List<ReminderTimeModel> reminderTimeModels = TaskReminderCalculator.GetRepeatTaskReminders(task, await TaskReminderDao.GetRemindersByTaskId(task.Id));
            await ReminderTimeDao.DeleteReminderTimeByIdAndType(task.Id, 0);
            await ReminderTimeDao.AddReminderTimes(reminderTimeModels);
            reminderTimeModels = (List<ReminderTimeModel>) null;
            task = (TaskBaseViewModel) null;
          }
        }
        await EventReminderCalculator.InitAllEventsReminderTimes();
        await HabitReminderCalculator.InitHabitReminders();
        await CourseReminderCalculator.InitCourseReminders();
      }));
    }

    public static async Task TryRemind(DateTime time)
    {
      if (!ABTestManager.IsNewRemindCalculate() || !LocalSettings.Settings.ShowReminder)
        return;
      List<ReminderTimeModel> modelBetweenTime = await ReminderTimeDao.GetReminderTimeModelBetweenTime(time.AddSeconds(-30.0), time.AddSeconds(30.0));
      if (modelBetweenTime.Any<ReminderTimeModel>())
      {
        foreach (ReminderTimeModel reminderTimeModel in modelBetweenTime)
        {
          ReminderTimeModel r = reminderTimeModel;
          if (Math.Abs(r.ReminderTime - time.Ticks) <= 50000000L)
          {
            App.Connection.DeleteAsync((object) r);
            ReminderModel reminder = (ReminderModel) null;
            switch (r.Type)
            {
              case 0:
                TaskBaseViewModel taskById = TaskCache.GetTaskById(r.EntityId);
                if (taskById.Status == 0 && taskById.Deleted == 0)
                {
                  if (taskById != null)
                  {
                    reminder = new ReminderModel()
                    {
                      Type = r.Type,
                      TaskId = taskById.Id,
                      StartDate = taskById.StartDate,
                      ReminderTime = new DateTime?(new DateTime(r.ReminderTime)),
                      ProjectId = taskById.ProjectId,
                      Creator = taskById.Creator,
                      IsAllDay = taskById.IsAllDay,
                      Assignee = taskById.Assignee
                    };
                    break;
                  }
                  break;
                }
                continue;
              case 1:
                TaskBaseViewModel checkItemById = TaskDetailItemCache.GetCheckItemById(r.EntityId);
                if (checkItemById != null)
                {
                  TaskBaseViewModel ownerTask = checkItemById.OwnerTask;
                  if (ownerTask.Kind == "CHECKLIST" && ownerTask.Status == 0 && ownerTask.Deleted == 0 && checkItemById.Status == 0 && ((int) checkItemById.IsAllDay ?? 1) == 0 && checkItemById.StartDate.HasValue)
                  {
                    reminder = new ReminderModel()
                    {
                      Type = r.Type,
                      TaskId = ownerTask.Id,
                      CheckItemId = checkItemById.Id,
                      StartDate = checkItemById.StartDate,
                      ReminderTime = new DateTime?(new DateTime(r.ReminderTime)),
                      ProjectId = ownerTask.ProjectId,
                      Creator = ownerTask.Creator,
                      IsAllDay = new bool?(false),
                      Assignee = ownerTask.Assignee
                    };
                    break;
                  }
                  break;
                }
                break;
              case 2:
                CalendarEventModel eventByEventIdOrId = await CalendarEventDao.GetEventByEventIdOrId(r.EntityId);
                if (eventByEventIdOrId != null)
                {
                  reminder = new ReminderModel()
                  {
                    Type = r.Type,
                    EventId = r.EntityId,
                    StartDate = eventByEventIdOrId.DueStart,
                    ReminderTime = new DateTime?(new DateTime(r.ReminderTime)),
                    ProjectId = eventByEventIdOrId.CalendarId
                  };
                  break;
                }
                break;
              case 4:
                if (await HabitDao.GetHabitById(r.EntityId) != null)
                {
                  DateTime dateTime = new DateTime(r.ReminderTime);
                  reminder = new ReminderModel()
                  {
                    Type = r.Type,
                    HabitId = r.EntityId,
                    StartDate = new DateTime?(dateTime),
                    ReminderTime = new DateTime?(dateTime)
                  };
                  break;
                }
                break;
            }
            if (reminder != null)
              ReminderBase.ShowReminderWindow(reminder);
            reminder = (ReminderModel) null;
            r = (ReminderTimeModel) null;
          }
        }
      }
      if (!UserDao.IsPro() || !LocalSettings.Settings.UserPreference?.TimeTable?.isEnabled.GetValueOrDefault())
        return;
      List<ReminderModel> courseReminders = CourseReminderCalculator.GetCourseReminders(time.AddSeconds(-30.0), time.AddSeconds(30.0));
      // ISSUE: explicit non-virtual call
      if (courseReminders == null || __nonvirtual (courseReminders.Count) <= 0)
        return;
      foreach (ReminderModel reminder in courseReminders)
        ReminderBase.ShowReminderWindow(reminder);
    }

    public static async Task ShowReminderWindow(ReminderModel reminder)
    {
      if (ReminderBase.IsTaskProjectMuted(reminder))
        return;
      UtilLog.Info("TimeToReminder HabitId " + reminder.HabitId + " TaskId " + reminder.TaskId + " EventId " + reminder.EventId + string.Format(" inClient {0} showreminder", (object) LocalSettings.Settings.ShowReminderInClient) + LocalSettings.Settings.ShowReminder.ToString());
      if (!LocalSettings.Settings.ShowReminder || string.IsNullOrEmpty(LocalSettings.Settings.LoginUserId))
        return;
      if (!LocalSettings.Settings.ShowReminderInClient && !Utils.IsWindows7() && SystemToastUtils.CheckSystemToastEnable())
      {
        string remindSound = LocalSettings.Settings.ExtraSettings.RemindSound;
        bool playSound = remindSound == "Harp" || string.IsNullOrEmpty(remindSound);
        try
        {
          UserActCollectUtils.AddClickEvent("reminder_data", "style", "remind_with_system_notification");
          switch (reminder.Type)
          {
            case 4:
              UserActCollectUtils.AddClickEvent("reminder_data", "type", "habit_reminder");
              await SystemToastUtils.ToastHabit(reminder, playSound);
              break;
            case 8:
              UserActCollectUtils.AddClickEvent("reminder_data", "type", "timetable_reminder");
              await SystemToastUtils.ToastCourse(reminder, playSound);
              break;
            default:
              await SystemToastUtils.SystemToast(reminder, playSound);
              break;
          }
          RemindSoundPlayer.PlayTaskRemindSound(false);
        }
        catch (Exception ex)
        {
          await ReminderBase.ShowReminderPopup(reminder);
          UtilLog.Warn(ex.Message);
        }
      }
      else
        await ReminderBase.ShowReminderPopup(reminder);
    }

    private static async Task ShowReminderPopup(ReminderModel reminder)
    {
      UserActCollectUtils.AddClickEvent("reminder_data", "style", "remind_me_on_client");
      switch (reminder.Type)
      {
        case 0:
        case 1:
          UserActCollectUtils.AddClickEvent("reminder_data", "type", "task_reminder");
          break;
        case 2:
          CalendarEventModel calendarEventModel1 = await CalendarEventDao.GetEventById(reminder.EventId);
          if (calendarEventModel1 == null)
            calendarEventModel1 = await CalendarEventDao.GetEventByEventId(reminder.EventId);
          CalendarEventModel calendarEventModel2 = calendarEventModel1;
          if (calendarEventModel2 != null)
          {
            reminder.Title = calendarEventModel2.Title;
            reminder.Content = calendarEventModel2.Content;
          }
          UserActCollectUtils.AddClickEvent("reminder_data", "type", "calendar_reminder");
          break;
        case 4:
          if (!LocalSettings.Settings.ShowHabit)
            return;
          UserActCollectUtils.AddClickEvent("reminder_data", "type", "habit_reminder");
          break;
        case 8:
          UserActCollectUtils.AddClickEvent("reminder_data", "type", "timetable_reminder");
          break;
      }
      DelayActionHandlerCenter.TryDoAction("ShowCustomRemindPopup", (EventHandler) ((sender, args) => ReminderBase.PlayReminderSound()), 200);
      await Application.Current.Dispatcher.InvokeAsync((Action) (() => ReminderWindowManager.TryAddNewWindow(reminder)));
      await Task.Delay(1000);
    }

    private static void PlayReminderSound()
    {
      if (!LocalSettings.Settings.EnableRingtone)
        return;
      RemindSoundPlayer.PlayTaskRemindSound(false);
    }

    private static bool IsTaskProjectMuted(ReminderModel reminder)
    {
      if (reminder.Type == 0 || reminder.Type == 1)
      {
        TaskBaseViewModel taskById = TaskCache.GetTaskById(reminder.TaskId);
        ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == reminder.ProjectId));
        if (projectModel == null || !projectModel.IsValid())
          return true;
        bool flag = string.IsNullOrEmpty(taskById.Assignee) || taskById.Assignee == "-1";
        if ((flag && taskById.Creator != LocalSettings.Settings.LoginUserId || !flag && reminder.Assignee != Utils.GetCurrentUserIdInt().ToString()) && projectModel.muted && projectModel.IsShareList())
          return true;
      }
      return false;
    }
  }
}
