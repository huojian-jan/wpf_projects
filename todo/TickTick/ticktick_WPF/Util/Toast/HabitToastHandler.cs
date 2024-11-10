// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Toast.HabitToastHandler
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
  public class HabitToastHandler : IToastHandler
  {
    private void AddClickEvent(string label)
    {
      UserActCollectUtils.AddClickEvent("reminder", "habit_reminder_system", label);
    }

    public async void Exec(ToastOptionModel optModel, List<KeyValuePair<string, object>> kvs)
    {
      if (optModel.CurrentOptionName == "Delay")
      {
        this.AddClickEvent("snooze");
        this.Delay(optModel, kvs);
      }
      else
      {
        switch (optModel.ToastType)
        {
          case ToastType.BoolHabitCompleted:
            if (optModel.CurrentOptionName == "Complete")
            {
              await this.AutoAddStep(optModel.TargetId);
              this.AddClickEvent("done");
              break;
            }
            if (!(optModel.CurrentOptionName == "Dismiss"))
              break;
            break;
          case ToastType.MultiHabitCompleted:
            if (optModel.CurrentOptionName == "AddStep")
            {
              await this.AutoAddStep(optModel.TargetId);
              this.AddClickEvent("auto_record");
              break;
            }
            if (optModel.CurrentOptionName == "Dismiss" || !(optModel.CurrentOptionName == "Manual"))
              break;
            this.AddClickEvent("manual_record");
            App.NavigateHabit(optModel.TargetId, true);
            break;
        }
      }
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
              goto label_22;
            }
            else
              break;
          case 4:
            if (str == "3hrs")
            {
              reminder.ReminderTime = new DateTime?(DateTime.Now.AddHours(3.0));
              data = "3h";
              goto label_22;
            }
            else
              break;
          case 6:
            switch (str[0])
            {
              case '1':
                if (str == "15mins")
                {
                  reminder.ReminderTime = new DateTime?(DateTime.Now.AddMinutes(15.0));
                  data = "15m";
                  goto label_22;
                }
                else
                  break;
              case '3':
                if (str == "30mins")
                {
                  reminder.ReminderTime = new DateTime?(DateTime.Now.AddMinutes(30.0));
                  data = "30m";
                  goto label_22;
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
              goto label_22;
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
                  goto label_22;
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
                  goto label_22;
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
              goto label_22;
            }
            else
              break;
        }
      }
      reminder.ReminderTime = new DateTime?();
label_22:
      if (!string.IsNullOrEmpty(data))
        UserActCollectUtils.AddClickEvent("reminder_data", "snooze", data);
      await ReminderDelayDao.AddModelAsync(new ReminderDelayModel()
      {
        UserId = LocalSettings.Settings.LoginUserId,
        ObjId = optionModel.TargetId,
        Type = "habit",
        RemindTime = new DateTime?(optionModel.StartTime),
        NextTime = reminder.ReminderTime,
        SyncStatus = 0
      });
      SyncManager.TryDelaySync();
      reminderTime = reminder.ReminderTime;
      if (reminderTime.HasValue)
        Application.Current?.Dispatcher?.Invoke<Task>((Func<Task>) (async () => App.ReminderList.Add(reminder)));
    }

    private async Task AutoAddStep(string habitId)
    {
      Utils.RunOnUiThread(Dispatcher.CurrentDispatcher, (Action) (async () =>
      {
        HabitModel habitById = await HabitDao.GetHabitById(habitId);
        if (habitById == null)
          return;
        Utils.PlayCompletionSound();
        if (habitById.IsBoolHabit())
        {
          await HabitService.CheckInHabit(habitById.Id, DateTime.Today);
        }
        else
        {
          double step = habitById.Step < 0.0 ? 1.0 : habitById.Step;
          await HabitService.CheckInHabit(habitById.Id, DateTime.Today, step);
        }
      }));
    }
  }
}
