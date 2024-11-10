// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.KotlinUtils.ModelsDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using KotlinModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.KotlinUtils
{
  public class ModelsDao
  {
    public static DateTime TTCalendarToDateTime(TTCalendar calTime)
    {
      try
      {
        DateTime dateTime = new DateTime(1, 1, 1);
        dateTime = dateTime.AddYears(calTime.year - 1);
        dateTime = dateTime.AddMonths(calTime.month - 1);
        dateTime = dateTime.AddDays((double) (calTime.day - 1));
        dateTime = dateTime.AddHours((double) calTime.hour);
        dateTime = dateTime.AddMinutes((double) calTime.minute);
        dateTime = dateTime.AddSeconds((double) calTime.second);
        return dateTime;
      }
      catch (Exception ex)
      {
        return new DateTime();
      }
    }

    public static TTCalendar NewTTCalendar(DateTime date, string zoneId, bool handleTimezone = false)
    {
      if (handleTimezone)
        date = TimeZoneUtils.LocalToTargetTzTime(date, zoneId);
      return new TTCalendar()
      {
        year = date.Year,
        month = date.Month,
        day = date.Day,
        hour = date.Hour,
        minute = date.Minute,
        second = date.Second,
        milliSecond = date.Millisecond,
        zone = zoneId ?? TimeZoneData.LocalTimeZoneModel.TimeZoneName
      };
    }

    public static DateTime? TTCalendarToLocalDateTime(TTCalendar cal)
    {
      if (cal.year <= 0)
        return new DateTime?();
      try
      {
        DateTime dateTime = new DateTime(1, 1, 1);
        dateTime = dateTime.AddYears(cal.year - 1);
        dateTime = dateTime.AddMonths(cal.month - 1);
        dateTime = dateTime.AddDays((double) (cal.day - 1));
        dateTime = dateTime.AddHours((double) cal.hour);
        dateTime = dateTime.AddMinutes((double) cal.minute);
        return new DateTime?(TimeZoneUtils.ToLocalTime(dateTime.AddSeconds((double) cal.second), cal.zone));
      }
      catch (Exception ex)
      {
        return new DateTime?();
      }
    }

    public static CheckItem NewCheckItem(TaskDetailItemModel item, string timeZone, bool isClone = false)
    {
      CheckItem checkItem = new CheckItem();
      checkItem.uniqueId = (long) item._Id;
      checkItem.id = isClone ? Utils.GetGuid() : item.id;
      DateTime? nullable = item.startDate;
      TTCalendar ttCalendar1;
      if (nullable.HasValue)
      {
        nullable = item.startDate;
        ttCalendar1 = ModelsDao.NewTTCalendar(nullable.Value, timeZone);
      }
      else
        ttCalendar1 = new TTCalendar();
      checkItem.startDate = ttCalendar1;
      nullable = item.completedTime;
      TTCalendar ttCalendar2;
      if (nullable.HasValue)
      {
        nullable = item.completedTime;
        ttCalendar2 = ModelsDao.NewTTCalendar(nullable.Value, timeZone);
      }
      else
        ttCalendar2 = new TTCalendar();
      checkItem.completedDate = ttCalendar2;
      checkItem.isChecked = item.status;
      return checkItem;
    }

    public static Reminder NewReminder(TaskReminderModel reminder, string userId, string taskId = null)
    {
      return new Reminder()
      {
        id = (long) reminder._Id,
        sid = reminder.id,
        userId = userId,
        taskId = 0,
        taskSid = taskId ?? reminder.taskserverid,
        duration = reminder.trigger
      };
    }

    public static DueDataModel NewDueDataModel(
      TaskModel task,
      List<TaskReminderModel> taskReminders,
      DateTime? recurringStart,
      DateTime? recurringEnd)
    {
      DueDataModel dueDataModel1 = new DueDataModel();
      dueDataModel1.repeatFlag = task.repeatFlag;
      dueDataModel1.repeatFrom = task.repeatFlag == null || !task.repeatFlag.Contains("FORGETTINGCURVE") ? task.repeatFrom : "0";
      dueDataModel1.timeZone = task.timeZone ?? TimeZoneData.LocalTimeZoneModel.TimeZoneName;
      dueDataModel1.reminders = (taskReminders != null ? taskReminders.Select<TaskReminderModel, Reminder>((Func<TaskReminderModel, Reminder>) (item => ModelsDao.NewReminder(item, task.userId, task.id))).ToList<Reminder>() : (List<Reminder>) null) ?? new List<Reminder>();
      DueDataModel dueDataModel2 = dueDataModel1;
      bool? nullable1 = task.isAllDay;
      int num1 = ((int) nullable1 ?? 1) != 0 ? 1 : 0;
      dueDataModel2.isAllDay = num1;
      DueDataModel dueDataModel3 = dueDataModel1;
      nullable1 = task.isFloating;
      int num2 = nullable1.GetValueOrDefault() ? 1 : 0;
      dueDataModel3.isFloating = num2;
      dueDataModel1.isClearDate = 0;
      DateTime? nullable2 = recurringStart;
      DateTime? nullable3 = nullable2 ?? task.startDate;
      dueDataModel1.startDate = !nullable3.HasValue ? new TTCalendar() : ModelsDao.NewTTCalendar(nullable3.Value, task.timeZone, true);
      nullable2 = recurringEnd;
      DateTime? nullable4 = nullable2 ?? task.dueDate;
      dueDataModel1.dueDate = !nullable4.HasValue ? new TTCalendar() : ModelsDao.NewTTCalendar(nullable4.Value, task.timeZone, true);
      DueDataModel dueDataModel4 = dueDataModel1;
      nullable2 = task.completedTime;
      TTCalendar ttCalendar1;
      if (nullable2.HasValue)
      {
        nullable2 = task.completedTime;
        ttCalendar1 = ModelsDao.NewTTCalendar(nullable2.Value, task.timeZone, true);
      }
      else
        ttCalendar1 = new TTCalendar();
      dueDataModel4.completedDate = ttCalendar1;
      DueDataModel dueDataModel5 = dueDataModel1;
      nullable2 = task.startDate;
      TTCalendar ttCalendar2;
      if (nullable2.HasValue)
      {
        nullable2 = task.startDate;
        ttCalendar2 = ModelsDao.NewTTCalendar(nullable2.Value, task.timeZone, true);
      }
      else
        ttCalendar2 = new TTCalendar();
      dueDataModel5.repeatOriginStartDate = ttCalendar2;
      List<string> source = string.IsNullOrEmpty(task.exDates) ? (List<string>) null : JsonConvert.DeserializeObject<List<string>>(task.exDates);
      List<TTCalendar> ttCalendarList = new List<TTCalendar>();
      if (source != null)
      {
        foreach (string s in source.Where<string>((Func<string, bool>) (t => !string.IsNullOrEmpty(t))).Select<string, string>((Func<string, string>) (text => text.Replace("\"", ""))).ToList<string>())
        {
          DateTime result;
          if (DateTime.TryParseExact(s, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
          {
            TTCalendar ttCalendar3 = ModelsDao.NewTTCalendar(result, (string) null);
            ttCalendarList.Add(ttCalendar3);
          }
        }
      }
      dueDataModel1.exDate = ttCalendarList;
      return dueDataModel1;
    }

    public static DueDataModel NewDueDataModel(TaskModel task, TimeData timeData)
    {
      DueDataModel dueDataModel1 = new DueDataModel();
      dueDataModel1.repeatFlag = timeData.RepeatFlag;
      dueDataModel1.repeatFrom = timeData.RepeatFrom;
      dueDataModel1.timeZone = timeData.TimeZone?.TimeZoneName ?? TimeZoneData.LocalTimeZoneModel.TimeZoneName;
      DueDataModel dueDataModel2 = dueDataModel1;
      List<TaskReminderModel> reminders = timeData.Reminders;
      List<Reminder> reminderList = (reminders != null ? reminders.Select<TaskReminderModel, Reminder>((Func<TaskReminderModel, Reminder>) (item => ModelsDao.NewReminder(item, task.userId, task.id))).ToList<Reminder>() : (List<Reminder>) null) ?? new List<Reminder>();
      dueDataModel2.reminders = reminderList;
      dueDataModel1.isAllDay = ((int) timeData.IsAllDay ?? 1) != 0 ? 1 : 0;
      DueDataModel dueDataModel3 = dueDataModel1;
      TimeZoneViewModel timeZone = timeData.TimeZone;
      int num = timeZone != null && timeZone.IsFloat ? 1 : 0;
      dueDataModel3.isFloating = num;
      dueDataModel1.isClearDate = 0;
      DueDataModel dueDataModel4 = dueDataModel1;
      DateTime? nullable = timeData.StartDate;
      TTCalendar ttCalendar1;
      if (nullable.HasValue)
      {
        nullable = timeData.StartDate;
        ttCalendar1 = ModelsDao.NewTTCalendar(nullable.Value, task.timeZone, true);
      }
      else
        ttCalendar1 = new TTCalendar();
      dueDataModel4.startDate = ttCalendar1;
      DueDataModel dueDataModel5 = dueDataModel1;
      nullable = timeData.DueDate;
      TTCalendar ttCalendar2;
      if (nullable.HasValue)
      {
        nullable = timeData.DueDate;
        ttCalendar2 = ModelsDao.NewTTCalendar(nullable.Value, task.timeZone, true);
      }
      else
        ttCalendar2 = new TTCalendar();
      dueDataModel5.dueDate = ttCalendar2;
      DueDataModel dueDataModel6 = dueDataModel1;
      nullable = task.completedTime;
      TTCalendar ttCalendar3;
      if (nullable.HasValue)
      {
        nullable = task.completedTime;
        ttCalendar3 = ModelsDao.NewTTCalendar(nullable.Value, task.timeZone, true);
      }
      else
        ttCalendar3 = new TTCalendar();
      dueDataModel6.completedDate = ttCalendar3;
      DueDataModel dueDataModel7 = dueDataModel1;
      nullable = task.startDate;
      TTCalendar ttCalendar4;
      if (nullable.HasValue)
      {
        nullable = task.startDate;
        ttCalendar4 = ModelsDao.NewTTCalendar(nullable.Value, task.timeZone, true);
      }
      else
        ttCalendar4 = new TTCalendar();
      dueDataModel7.repeatOriginStartDate = ttCalendar4;
      List<TTCalendar> ttCalendarList = new List<TTCalendar>();
      if (timeData.ExDates != null)
      {
        timeData.ExDates = timeData.ExDates.Select<string, string>((Func<string, string>) (text => text.Replace("\"", ""))).ToList<string>();
        foreach (string exDate in timeData.ExDates)
        {
          DateTime result;
          if (DateTime.TryParseExact(exDate, "yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
          {
            TTCalendar ttCalendar5 = ModelsDao.NewTTCalendar(result, (string) null);
            ttCalendarList.Add(ttCalendar5);
          }
        }
      }
      dueDataModel1.exDate = ttCalendarList;
      return dueDataModel1;
    }

    public static async Task SaveModels(List<CheckItem> list)
    {
      list?.ForEach((Action<CheckItem>) (async item =>
      {
        TaskDetailItemModel checkItemById = await TaskDetailItemDao.GetCheckItemById(item.id);
        if (checkItemById == null)
          return;
        checkItemById.status = item.isChecked;
        checkItemById.startDate = ModelsDao.TTCalendarToLocalDateTime(item.startDate);
        checkItemById.completedTime = ModelsDao.TTCalendarToLocalDateTime(item.completedDate);
        await TaskDetailItemDao.SaveChecklistItem(checkItemById);
      }));
    }

    public static async Task InsertModels(List<CheckItem> list, string taskId, bool keepStatus = true)
    {
      if (list == null)
        return;
      foreach (CheckItem checkItem in list)
      {
        CheckItem item = checkItem;
        TaskDetailItemModel taskDetailItemModel = await App.Connection.Table<TaskDetailItemModel>().Where((Expression<Func<TaskDetailItemModel, bool>>) (t => (long) t._Id == item.uniqueId)).FirstOrDefaultAsync();
        if (taskDetailItemModel == null)
          break;
        await TaskDetailItemDao.SaveChecklistItem(new TaskDetailItemModel()
        {
          id = item.id,
          title = taskDetailItemModel.title,
          status = keepStatus ? item.isChecked : 0,
          isAllDay = taskDetailItemModel.isAllDay,
          startDate = ModelsDao.TTCalendarToLocalDateTime(item.startDate),
          completedTime = ModelsDao.TTCalendarToLocalDateTime(item.completedDate),
          TaskServerId = taskId,
          sortOrder = taskDetailItemModel.sortOrder
        });
      }
    }

    public static async Task InsertModels(List<Reminder> list, string taskId)
    {
      if (list == null)
        return;
      foreach (Reminder reminder in list)
      {
        TaskReminderModel taskReminderModel = new TaskReminderModel();
        taskReminderModel.Taskid = (int) reminder.taskId;
        taskReminderModel.taskserverid = taskId;
        try
        {
          taskReminderModel.id = reminder.sid;
          taskReminderModel.trigger = reminder.duration;
        }
        catch (Exception ex)
        {
        }
        await TaskReminderDao.InsertTaskReminder(taskReminderModel);
      }
    }
  }
}
