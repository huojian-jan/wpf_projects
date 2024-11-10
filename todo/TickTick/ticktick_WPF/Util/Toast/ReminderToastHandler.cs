// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Toast.ReminderToastHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util.Toast
{
  public class ReminderToastHandler : IToastHandler
  {
    public async void Exec(ToastOptionModel optionModel, List<KeyValuePair<string, object>> kvs)
    {
      if (optionModel.CurrentOptionName == "Complete")
      {
        this.Complete(optionModel);
        this.AddClickEvent(optionModel, "done");
      }
      else if (optionModel.CurrentOptionName == "Focus")
      {
        this.OnFocus(optionModel);
        this.AddClickEvent(optionModel, "start_focus");
      }
      else if (optionModel.CurrentOptionName == "Delay")
      {
        await this.Delay(optionModel, kvs);
        this.AddClickEvent(optionModel, "snooze");
      }
      else
      {
        if (!(optionModel.CurrentOptionName == "GotIt"))
          return;
        this.AddClickEvent(optionModel, "got_it_btn");
      }
    }

    private void AddClickEvent(ToastOptionModel optionModel, string label)
    {
      string ctype = "task_reminder_system";
      if (optionModel.ToastType == ToastType.RemindEvent)
        ctype = "calendar_reminder_system";
      if (optionModel.ToastType == ToastType.RemindCourse)
        ctype = "timetable_reminder_system";
      UserActCollectUtils.AddClickEvent("reminder", ctype, label);
    }

    private void OnFocus(ToastOptionModel optionModel)
    {
    }

    public void Complete(ToastOptionModel optionModel)
    {
      Application.Current?.Dispatcher?.Invoke<Task>((Func<Task>) (async () =>
      {
        App.Instance.StopReminderTimer();
        switch (optionModel.ToastType)
        {
          case ToastType.RemindTask:
            TaskModel thinTaskById = await TaskDao.GetThinTaskById(optionModel.TargetId);
            if (thinTaskById != null && thinTaskById.kind == "NOTE")
            {
              SystemToastUtils.TryShowDetail(optionModel.TargetId, "task");
              break;
            }
            if (thinTaskById != null && thinTaskById.status == 0)
            {
              TaskCloseExtra taskCloseExtra = await TaskService.SetTaskStatus(thinTaskById, 2);
              break;
            }
            break;
          case ToastType.RemindCheckItem:
            await TaskService.ReminderCompleteCheckItem(optionModel.TargetId);
            break;
        }
        App.Instance.StartReminderTimer();
        SyncManager.TryDelaySync();
      }));
    }

    public async Task Delay(ToastOptionModel optionModel, List<KeyValuePair<string, object>> kvs)
    {
      List<KeyValuePair<string, object>> keyValuePairList = kvs;
      // ISSUE: explicit non-virtual call
      if ((keyValuePairList != null ? (__nonvirtual (keyValuePairList.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      ReminderModel reminder = new ReminderModel();
      object obj = kvs.FirstOrDefault<KeyValuePair<string, object>>().Value;
      string data = "";
      string str = obj.ToString();
      DateTime? reminderTime;
      if (str != null)
      {
        switch (str.Length)
        {
          case 3:
            if (str == "1hr")
            {
              reminder.ReminderTime = new DateTime?(DateTime.Now.AddHours(1.0));
              data = "1h";
              goto label_27;
            }
            else
              break;
          case 4:
            switch (str[0])
            {
              case '1':
                if (str == "1day")
                {
                  reminder.ReminderTime = new DateTime?(DateTime.Now.AddDays(1.0));
                  data = "tomorrow";
                  goto label_27;
                }
                else
                  break;
              case '3':
                if (str == "3hrs")
                {
                  reminder.ReminderTime = new DateTime?(DateTime.Now.AddHours(3.0));
                  data = "3h";
                  goto label_27;
                }
                else
                  break;
            }
            break;
          case 6:
            switch (str[0])
            {
              case '1':
                if (str == "15mins")
                {
                  reminder.ReminderTime = new DateTime?(DateTime.Now.AddMinutes(15.0));
                  data = "15m";
                  goto label_27;
                }
                else
                  break;
              case '3':
                if (str == "30mins")
                {
                  reminder.ReminderTime = new DateTime?(DateTime.Now.AddMinutes(30.0));
                  data = "30m";
                  goto label_27;
                }
                else
                  break;
            }
            break;
          case 10:
            if (str == "TodayNight")
            {
              ReminderModel reminderModel = reminder;
              reminderTime = reminder.ReminderTime;
              DateTime? nullable = new DateTime?((reminderTime ?? DateTime.Now).Date.AddHours(20.0));
              reminderModel.ReminderTime = nullable;
              data = "smart_time";
              goto label_27;
            }
            else
              break;
          case 12:
            switch (str[5])
            {
              case 'E':
                if (str == "TodayEvening")
                {
                  ReminderModel reminderModel = reminder;
                  reminderTime = reminder.ReminderTime;
                  DateTime? nullable = new DateTime?((reminderTime ?? DateTime.Now).Date.AddHours(17.0));
                  reminderModel.ReminderTime = nullable;
                  data = "smart_time";
                  goto label_27;
                }
                else
                  break;
              case 'M':
                if (str == "TodayMorning")
                {
                  ReminderModel reminderModel = reminder;
                  reminderTime = reminder.ReminderTime;
                  DateTime? nullable = new DateTime?((reminderTime ?? DateTime.Now).Date.AddHours(9.0));
                  reminderModel.ReminderTime = nullable;
                  data = "smart_time";
                  goto label_27;
                }
                else
                  break;
            }
            break;
          case 14:
            if (str == "TodayAfternoon")
            {
              ReminderModel reminderModel = reminder;
              reminderTime = reminder.ReminderTime;
              DateTime? nullable = new DateTime?((reminderTime ?? DateTime.Now).Date.AddHours(13.0));
              reminderModel.ReminderTime = nullable;
              data = "smart_time";
              goto label_27;
            }
            else
              break;
          case 15:
            if (str == "TomorrowMorning")
            {
              ReminderModel reminderModel = reminder;
              reminderTime = reminder.ReminderTime;
              DateTime dateTime = (reminderTime ?? DateTime.Now).Date;
              dateTime = dateTime.AddDays(1.0);
              DateTime? nullable = new DateTime?(dateTime.AddHours(9.0));
              reminderModel.ReminderTime = nullable;
              data = "smart_time";
              goto label_27;
            }
            else
              break;
        }
      }
      reminder.ReminderTime = new DateTime?();
label_27:
      if (!string.IsNullOrEmpty(data))
        UserActCollectUtils.AddClickEvent("reminder_data", "snooze", data);
      switch (optionModel.ToastType)
      {
        case ToastType.RemindTask:
          TaskModel thinTaskById1 = await TaskDao.GetThinTaskById(optionModel.TargetId);
          if (thinTaskById1 != null && thinTaskById1.status == 0 && thinTaskById1.deleted == 0)
          {
            reminder.TaskId = thinTaskById1.id;
            reminder.Type = 0;
            reminder.CheckItemId = "";
            reminder.StartDate = thinTaskById1.startDate;
            reminder.IsAllDay = thinTaskById1.isAllDay;
            reminder.Trigger = "TRIGGER:PT0S";
            reminder.Assignee = thinTaskById1.assignee;
            reminder.ProjectId = thinTaskById1.projectId;
            reminder.RepeatFlag = "";
            reminderTime = reminder.ReminderTime;
            if (reminderTime.HasValue)
              Application.Current?.Dispatcher?.Invoke<Task>((Func<Task>) (async () =>
              {
                App.ReminderList.Add(reminder);
                await TaskService.SetSnoozeTime(reminder.TaskId, reminder.ReminderTime.Value);
                await Task.Delay(100);
                SyncManager.TryDelaySync();
              }));
          }
          ReminderDelayDao.AddModelAsync(new ReminderDelayModel()
          {
            UserId = LocalSettings.Settings.LoginUserId,
            ObjId = optionModel.TargetId,
            Type = "task",
            RemindTime = new DateTime?(optionModel.StartTime),
            NextTime = reminder.ReminderTime,
            SyncStatus = 0
          });
          break;
        case ToastType.RemindCourse:
        case ToastType.RemindEvent:
        case ToastType.RemindHabit:
          await ReminderDelayDao.AddModelAsync(new ReminderDelayModel()
          {
            UserId = LocalSettings.Settings.LoginUserId,
            ObjId = optionModel.TargetId,
            Type = optionModel.ToastType == ToastType.RemindEvent ? "calendar" : (optionModel.ToastType == ToastType.RemindHabit ? "habit" : "course"),
            RemindTime = new DateTime?(optionModel.StartTime),
            NextTime = reminder.ReminderTime,
            SyncStatus = 0
          });
          SyncManager.TryDelaySync();
          reminderTime = reminder.ReminderTime;
          if (reminderTime.HasValue)
          {
            Application current = Application.Current;
            if (current != null)
            {
              Dispatcher dispatcher = current.Dispatcher;
              if (dispatcher != null)
              {
                dispatcher.Invoke<Task>((Func<Task>) (async () => App.ReminderList.Add(reminder)));
                break;
              }
              break;
            }
            break;
          }
          break;
        case ToastType.RemindCheckItem:
          TaskDetailItemModel subtask = await TaskDetailItemDao.GetChecklistItemById(optionModel.TargetId);
          if (subtask != null && subtask.status == 0)
          {
            TaskModel thinTaskById2 = await TaskDao.GetThinTaskById(subtask.TaskServerId);
            if (thinTaskById2 != null && thinTaskById2.status == 0 && thinTaskById2.deleted == 0)
            {
              reminder.TaskId = subtask.TaskServerId;
              reminder.Type = 1;
              reminder.CheckItemId = subtask.id;
              reminder.StartDate = subtask.startDate;
              reminder.IsAllDay = subtask.isAllDay;
              reminder.Trigger = "TRIGGER:PT0S";
              reminder.Assignee = thinTaskById2.assignee;
              reminder.ProjectId = thinTaskById2.projectId;
              reminder.RepeatFlag = "";
              reminderTime = reminder.ReminderTime;
              if (reminderTime.HasValue)
              {
                Application current = Application.Current;
                if (current != null)
                {
                  Dispatcher dispatcher = current.Dispatcher;
                  if (dispatcher != null)
                  {
                    dispatcher.Invoke<Task>((Func<Task>) (async () =>
                    {
                      App.ReminderList.Add(reminder);
                      subtask.snoozeReminderTime = new DateTime?(reminder.ReminderTime.Value);
                      await TaskDetailItemDao.SaveChecklistItem(subtask);
                      await SyncStatusDao.AddSyncStatus(reminder.TaskId, 0);
                      await Task.Delay(100);
                      SyncManager.TryDelaySync();
                    }));
                    break;
                  }
                  break;
                }
                break;
              }
              break;
            }
            break;
          }
          break;
      }
    }
  }
}
