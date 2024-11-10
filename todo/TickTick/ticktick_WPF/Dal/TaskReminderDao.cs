// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.TaskReminderDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Habit;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class TaskReminderDao
  {
    private static readonly List<string> OnTimeList = new List<string>()
    {
      "TRIGGER:PT0S",
      "TRIGGER:-PT0S"
    };

    public static async Task<ObservableCollection<TaskReminderModel>> GetAllReminders()
    {
      return await Task.Run<ObservableCollection<TaskReminderModel>>((Func<Task<ObservableCollection<TaskReminderModel>>>) (async () => new ObservableCollection<TaskReminderModel>(await App.Connection.Table<TaskReminderModel>().ToListAsync())));
    }

    public static async Task<List<TaskReminderModel>> GetRemindersByTaskId(string taskServerId)
    {
      return await Task.Run<List<TaskReminderModel>>((Func<Task<List<TaskReminderModel>>>) (async () => await App.Connection.Table<TaskReminderModel>().Where((Expression<Func<TaskReminderModel, bool>>) (v => v.taskserverid == taskServerId)).ToListAsync()));
    }

    public static async Task<int> SaveReminders(TaskReminderModel taskReminderModel)
    {
      return await Task.Run<int>((Func<Task<int>>) (async () =>
      {
        try
        {
          List<TaskReminderModel> listAsync = await App.Connection.Table<TaskReminderModel>().Where((Expression<Func<TaskReminderModel, bool>>) (v => v.id.Equals(taskReminderModel.id))).ToListAsync();
          if (listAsync.Count != 0)
          {
            taskReminderModel._Id = listAsync[0]._Id;
            int num = await App.Connection.UpdateAsync((object) taskReminderModel);
            return 0;
          }
          int num1 = await App.Connection.InsertAsync((object) taskReminderModel);
          return taskReminderModel._Id;
        }
        catch (Exception ex)
        {
          return -1;
        }
      }));
    }

    public static async Task InsertTaskReminder(TaskReminderModel taskReminderModel)
    {
      int num = await App.Connection.InsertAsync((object) taskReminderModel);
    }

    public static async Task InsertTaskReminders(List<TaskReminderModel> taskReminderModels)
    {
      int num = await App.Connection.InsertAllAsync((IEnumerable) taskReminderModels);
    }

    public static async Task<bool> DeleteRemindersByTaskId(string taskId)
    {
      return await Task.Run<bool>((Func<Task<bool>>) (async () =>
      {
        List<TaskReminderModel> listAsync = await App.Connection.Table<TaskReminderModel>().Where((Expression<Func<TaskReminderModel, bool>>) (v => v.taskserverid == taskId)).ToListAsync();
        if (listAsync.Count == 0)
          return false;
        foreach (object obj in listAsync)
        {
          int num = await App.Connection.DeleteAsync(obj);
        }
        return true;
      }));
    }

    public static async Task<List<ReminderModel>> LoadNoneFireBindEvents()
    {
      // ISSUE: object of a compiler-generated type is created
      // ISSUE: variable of a compiler-generated type
      TaskReminderDao.\u003C\u003Ec__DisplayClass6_0 cDisplayClass60 = new TaskReminderDao.\u003C\u003Ec__DisplayClass6_0();
      Dictionary<string, ReminderDelayModel> delayDict = (await ReminderDelayDao.GetDelayModelByType("calendar")).ToDictionaryEx<ReminderDelayModel, string, ReminderDelayModel>((Func<ReminderDelayModel, string>) (d => d.ObjId), (Func<ReminderDelayModel, ReminderDelayModel>) (d => d));
      string str = Utils.GetCurrentUserIdInt().ToString();
      // ISSUE: reference to a compiler-generated field
      cDisplayClass60.validReminders = new List<ReminderModel>();
      List<CalendarEventModel> events = await App.Connection.QueryAsync<CalendarEventModel>("select * from CalendarEventModel where (CalendarId in (select Id as CalendarId from BindCalendarModel where Show = 'show') or  CalendarId in (select Id as CalendarId from CalendarSubscribeProfileModel where Show = 'show')) and UserId = '" + str + "' and Deleted = '0'");
      // ISSUE: reference to a compiler-generated field
      cDisplayClass60.skipEvents = await CalendarEventDao.GetSkipEvents();
      // ISSUE: reference to a compiler-generated field
      cDisplayClass60.hiddenKeys = await ArchivedDao.GetArchivedKeys();
      if (events != null && events.Any<CalendarEventModel>())
      {
        foreach (CalendarEventModel calendarEventModel in events)
        {
          // ISSUE: object of a compiler-generated type is created
          // ISSUE: variable of a compiler-generated type
          TaskReminderDao.\u003C\u003Ec__DisplayClass6_1 cDisplayClass61 = new TaskReminderDao.\u003C\u003Ec__DisplayClass6_1();
          // ISSUE: reference to a compiler-generated field
          cDisplayClass61.CS\u0024\u003C\u003E8__locals1 = cDisplayClass60;
          // ISSUE: reference to a compiler-generated field
          cDisplayClass61.eve = calendarEventModel;
          // ISSUE: reference to a compiler-generated field
          // ISSUE: reference to a compiler-generated field
          if (CacheManager.GetBindCalendarById(cDisplayClass61.eve.CalendarId) != null || CacheManager.GetSubscribeCalById(cDisplayClass61.eve.CalendarId) != null)
          {
            // ISSUE: reference to a compiler-generated field
            DateTime? dueStart = cDisplayClass61.eve.DueStart;
            if (dueStart.HasValue)
            {
              // ISSUE: reference to a compiler-generated field
              cDisplayClass61.delay = (ReminderDelayModel) null;
              // ISSUE: reference to a compiler-generated field
              // ISSUE: reference to a compiler-generated field
              if (!string.IsNullOrEmpty(cDisplayClass61.eve.EventId) && delayDict.ContainsKey(cDisplayClass61.eve.EventId))
              {
                // ISSUE: reference to a compiler-generated field
                // ISSUE: reference to a compiler-generated field
                cDisplayClass61.delay = delayDict[cDisplayClass61.eve.EventId];
              }
              // ISSUE: reference to a compiler-generated field
              string eventKey = ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(cDisplayClass61.eve));
              // ISSUE: reference to a compiler-generated field
              // ISSUE: reference to a compiler-generated field
              if (!cDisplayClass61.CS\u0024\u003C\u003E8__locals1.hiddenKeys.Contains(eventKey))
              {
                // ISSUE: reference to a compiler-generated field
                // ISSUE: variable of a compiler-generated type
                TaskReminderDao.\u003C\u003Ec__DisplayClass6_0 cs8Locals1 = cDisplayClass61.CS\u0024\u003C\u003E8__locals1;
                // ISSUE: reference to a compiler-generated field
                CalendarEventModel eve = cDisplayClass61.eve;
                // ISSUE: reference to a compiler-generated field
                dueStart = cDisplayClass61.eve.DueStart;
                DateTime date = dueStart.Value;
                // ISSUE: reference to a compiler-generated field
                ReminderDelayModel delay = cDisplayClass61.delay;
                // ISSUE: reference to a compiler-generated method
                cs8Locals1.\u003CLoadNoneFireBindEvents\u003Eg__AddReminder\u007C2(eve, date, delay);
              }
              // ISSUE: reference to a compiler-generated field
              if (!string.IsNullOrEmpty(cDisplayClass61.eve.RepeatFlag))
              {
                // ISSUE: reference to a compiler-generated field
                cDisplayClass61.exDateList = (List<DateTime>) null;
                // ISSUE: reference to a compiler-generated field
                if (!string.IsNullOrEmpty(cDisplayClass61.eve.ExDates))
                {
                  try
                  {
                    // ISSUE: reference to a compiler-generated field
                    // ISSUE: reference to a compiler-generated field
                    cDisplayClass61.exDateList = JsonConvert.DeserializeObject<List<DateTime>>(cDisplayClass61.eve.ExDates);
                  }
                  catch (Exception ex)
                  {
                  }
                }
                DateTime targetTzTime;
                // ISSUE: reference to a compiler-generated field
                if (!cDisplayClass61.eve.IsAllDay)
                {
                  // ISSUE: reference to a compiler-generated field
                  dueStart = cDisplayClass61.eve.DueStart;
                  // ISSUE: reference to a compiler-generated field
                  targetTzTime = TimeZoneUtils.LocalToTargetTzTime(dueStart.Value, cDisplayClass61.eve.TimeZone);
                }
                else
                {
                  // ISSUE: reference to a compiler-generated field
                  dueStart = cDisplayClass61.eve.DueStart;
                  targetTzTime = dueStart.Value;
                }
                DateTime dateTime = targetTzTime;
                // ISSUE: reference to a compiler-generated field
                string repeatFlag = cDisplayClass61.eve.RepeatFlag;
                DateTime calStart = dateTime;
                DateTime today = DateTime.Today;
                DateTime checkStart = today.AddDays(-1.0);
                today = DateTime.Today;
                DateTime checkEnd = today.AddDays(4.0);
                // ISSUE: reference to a compiler-generated field
                string timeZone = cDisplayClass61.eve.TimeZone;
                // ISSUE: reference to a compiler-generated field
                int num = cDisplayClass61.eve.IsAllDay ? 1 : 0;
                // ISSUE: reference to a compiler-generated method
                RepeatDao.GetEventRepeatDates(repeatFlag, calStart, checkStart, checkEnd, timeZone, num != 0).ForEach(new Action<DateTime>(cDisplayClass61.\u003CLoadNoneFireBindEvents\u003Eb__3));
              }
            }
          }
        }
      }
      // ISSUE: reference to a compiler-generated field
      List<ReminderModel> validReminders = cDisplayClass60.validReminders;
      cDisplayClass60 = (TaskReminderDao.\u003C\u003Ec__DisplayClass6_0) null;
      delayDict = (Dictionary<string, ReminderDelayModel>) null;
      events = (List<CalendarEventModel>) null;
      return validReminders;
    }

    public static async Task<List<ReminderModel>> LoadNonFireHabits()
    {
      List<ReminderModel> validReminders = new List<ReminderModel>();
      List<HabitModel> habits = await HabitDao.GetNeedCheckHabits();
      DateTime start = DateTime.Today.AddDays(-30.0);
      DateTime dateTime1 = DateTime.Today;
      DateTime end = dateTime1.AddDays(1.0);
      List<HabitCheckInModel> monthCheckIns = await HabitCheckInDao.GetCheckInsInSpan(start, end);
      if (habits != null && habits.Any<HabitModel>())
      {
        Dictionary<string, ReminderDelayModel> delayDict = (await ReminderDelayDao.GetDelayModelByType("habit")).ToDictionaryEx<ReminderDelayModel, string, ReminderDelayModel>((Func<ReminderDelayModel, string>) (d => d.ObjId), (Func<ReminderDelayModel, ReminderDelayModel>) (d => d));
        foreach (HabitModel habitModel in habits)
        {
          HabitModel habit = habitModel;
          if (!string.IsNullOrEmpty(habit.Reminder))
          {
            List<HabitCheckInModel> list = monthCheckIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.HabitId == habit.Id)).ToList<HabitCheckInModel>();
            HabitCheckInModel habitCheckInModel = list.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (v => v.HabitId == habit.Id && v.CheckinStamp == DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture)));
            if (habitCheckInModel == null || habitCheckInModel.Value < habitCheckInModel.Goal && habitCheckInModel.CheckStatus != 1)
            {
              if (await HabitUtils.IsHabitValidInToday(habit, list))
              {
                string[] source = habit.Reminder.Split(',');
                if (((IEnumerable<string>) source).Any<string>())
                {
                  foreach (string str in source)
                  {
                    if (str.Contains(":"))
                    {
                      string[] strArray = str.Split(':');
                      int result1;
                      int result2;
                      if (strArray.Length == 2 && int.TryParse(strArray[0], out result1) && int.TryParse(strArray[1], out result2))
                      {
                        dateTime1 = DateTime.Today;
                        dateTime1 = dateTime1.AddHours((double) result1);
                        DateTime dateTime2 = dateTime1.AddMinutes((double) result2);
                        if (!string.IsNullOrEmpty(habit.Id) && delayDict.ContainsKey(habit.Id))
                        {
                          ReminderDelayModel reminderDelayModel = delayDict[habit.Id];
                          DateTime? nullable = reminderDelayModel.RemindTime;
                          dateTime1 = dateTime2;
                          if ((nullable.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == dateTime1 ? 1 : 0) : 1) : 0) != 0)
                          {
                            nullable = reminderDelayModel.NextTime;
                            if (nullable.HasValue)
                            {
                              nullable = reminderDelayModel.NextTime;
                              dateTime2 = nullable.Value;
                            }
                          }
                        }
                        if (dateTime2 > DateTime.Now)
                          validReminders.Add(new ReminderModel()
                          {
                            IsAllDay = new bool?(false),
                            HabitId = habit.Id,
                            Type = 4,
                            StartDate = new DateTime?(dateTime2),
                            ReminderTime = new DateTime?(dateTime2)
                          });
                      }
                    }
                  }
                }
              }
            }
          }
        }
        delayDict = (Dictionary<string, ReminderDelayModel>) null;
      }
      List<ReminderModel> reminderModelList = validReminders;
      validReminders = (List<ReminderModel>) null;
      habits = (List<HabitModel>) null;
      monthCheckIns = (List<HabitCheckInModel>) null;
      return reminderModelList;
    }

    public static async Task<List<ReminderModel>> LoadNonFireReminders()
    {
      List<ReminderModel> validReminders = new List<ReminderModel>();
      List<ReminderModel> reminderModelList1 = await App.Connection.QueryAsync<ReminderModel>("SELECT 0           AS Type,        b.id        AS TaskId,        ''          AS CheckItemId,        b.startdate AS StartDate,        b.isallday  AS IsAllDay,        b.creator   AS Creator,        a.TRIGGER   AS Trigger,        NULL        AS ReminderTime,        b.projectId AS ProjectId,        b.assignee AS Assignee,        b.repeatFlag          AS RepeatFlag FROM   taskremindermodel a,        taskmodel b WHERE  a.taskserverid = b.id        AND b.status = 0        AND b.deleted = 0 AND b.userId = '**' UNION SELECT 0              AS Type,        id             AS TaskId,        ''             AS CheckItemId,        startdate      AS StartDate,        isallday       AS IsAllDay,        creator   AS Creator,        'TRIGGER:PT0S' AS Trigger,        remindtime     AS ReminderTime,        projectId      AS ProjectId,        assignee AS Assignee,        ''          AS RepeatFlag FROM   taskmodel WHERE  remindertime IS NOT NULL AND deleted = 0 AND status = 0 AND userId = '**' UNION SELECT 1                    AS Type,        a.taskserverid       AS TaskId,        a.id                 AS CheckItemId,        a.startdate          AS StartDate,        a.isallday           AS IsAllDay,        b.creator   AS Creator,        'TRIGGER:PT0S'       AS Trigger,        a.snoozeremindertime AS ReminderTime,        b.projectId AS ProjectId,        b.assignee AS Assignee,        ''          AS RepeatFlag FROM   taskdetailitemmodel a, taskmodel b WHERE  a.taskserverid = b.id         AND a.status = 0         AND b.status = 0         AND b.userId = '**'         AND a.startdate is not null         AND b.deleted = 0 ".Replace("**", LocalSettings.Settings.LoginUserId));
      List<string> list = CacheManager.GetProjects().Where<ProjectModel>((Func<ProjectModel, bool>) (p => (p.closed.HasValue && !p.closed.Value || !p.closed.HasValue) && !TeamDao.IsTeamExpired(p.teamId))).Select<ProjectModel, string>((Func<ProjectModel, string>) (p => p.id)).ToList<string>();
      // ISSUE: explicit non-virtual call
      if (reminderModelList1 != null && __nonvirtual (reminderModelList1.Count) > 0)
      {
        // ISSUE: variable of a compiler-generated type
        TaskReminderDao.\u003C\u003Ec__DisplayClass8_0 cDisplayClass80;
        // ISSUE: reference to a compiler-generated field
        cDisplayClass80.taskRepeats = new Dictionary<string, List<DateTime>>();
        foreach (ReminderModel reminderModel1 in reminderModelList1)
        {
          // ISSUE: object of a compiler-generated type is created
          // ISSUE: variable of a compiler-generated type
          TaskReminderDao.\u003C\u003Ec__DisplayClass8_1 cDisplayClass81 = new TaskReminderDao.\u003C\u003Ec__DisplayClass8_1();
          // ISSUE: reference to a compiler-generated field
          cDisplayClass81.reminder = reminderModel1;
          // ISSUE: reference to a compiler-generated field
          if (list.Contains(cDisplayClass81.reminder.ProjectId))
          {
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            bool flag = string.IsNullOrEmpty(cDisplayClass81.reminder.Assignee) || cDisplayClass81.reminder.Assignee == "-1";
            // ISSUE: reference to a compiler-generated field
            // ISSUE: reference to a compiler-generated field
            if (flag && cDisplayClass81.reminder.Creator != LocalSettings.Settings.LoginUserId || !flag && cDisplayClass81.reminder.Assignee != Utils.GetCurrentUserIdInt().ToString())
            {
              // ISSUE: reference to a compiler-generated method
              ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>(new Func<ProjectModel, bool>(cDisplayClass81.\u003CLoadNonFireReminders\u003Eb__3));
              if (projectModel != null && projectModel.muted && projectModel.IsShareList())
                continue;
            }
            // ISSUE: reference to a compiler-generated field
            DateTime? nullable1 = cDisplayClass81.reminder.ReminderTime;
            if (!nullable1.HasValue)
            {
              // ISSUE: reference to a compiler-generated field
              nullable1 = cDisplayClass81.reminder.StartDate;
              // ISSUE: reference to a compiler-generated field
              if (nullable1.HasValue && !Utils.IsEmptyDate(cDisplayClass81.reminder.StartDate))
              {
                // ISSUE: variable of a compiler-generated type
                TaskReminderDao.\u003C\u003Ec__DisplayClass8_2 cDisplayClass82;
                // ISSUE: reference to a compiler-generated field
                // ISSUE: reference to a compiler-generated field
                cDisplayClass82.task = TaskCache.GetTaskById(cDisplayClass81.reminder.TaskId);
                // ISSUE: reference to a compiler-generated field
                if (cDisplayClass82.task != null)
                {
                  // ISSUE: reference to a compiler-generated field
                  DateTime? nullable2 = cDisplayClass81.reminder.StartDate;
                  // ISSUE: reference to a compiler-generated field
                  // ISSUE: reference to a compiler-generated field
                  cDisplayClass82.reminderSpan = TriggerUtils.ParseTrigger(cDisplayClass81.reminder.Trigger);
                  // ISSUE: reference to a compiler-generated field
                  List<DateTime> repeats82 = TaskReminderDao.\u003CLoadNonFireReminders\u003Eg__GetRepeats\u007C8_2(cDisplayClass82.task, nullable2.Value, ref cDisplayClass80);
                  DateTime dateTime1 = nullable2.Value;
                  DateTime date = dateTime1.Date;
                  dateTime1 = DateTime.Today;
                  dateTime1 = dateTime1.Date;
                  DateTime dateTime2 = dateTime1.AddDays(2.0);
                  // ISSUE: reference to a compiler-generated field
                  if (date < dateTime2 && !string.IsNullOrEmpty(cDisplayClass81.reminder.RepeatFlag) && repeats82.Any<DateTime>())
                  {
                    foreach (DateTime dateTime3 in repeats82)
                    {
                      nullable2 = new DateTime?(DateUtils.SetDateOnly(nullable2.Value, dateTime3.Date));
                      DateTime? time = nullable2;
                      nullable1 = new DateTime?();
                      DateTime? reminderTime = nullable1;
                      ref TaskReminderDao.\u003C\u003Ec__DisplayClass8_2 local = ref cDisplayClass82;
                      // ISSUE: reference to a compiler-generated method
                      (bool, DateTime?) tuple = cDisplayClass81.\u003CLoadNonFireReminders\u003Eg__IsReminderTimeValid\u007C4(time, reminderTime, ref local);
                      if (tuple.Item1)
                      {
                        // ISSUE: reference to a compiler-generated field
                        ReminderModel reminderModel2 = cDisplayClass81.reminder.Copy();
                        reminderModel2.ReminderTime = tuple.Item2;
                        validReminders.Add(reminderModel2);
                      }
                    }
                  }
                  // ISSUE: reference to a compiler-generated field
                  // ISSUE: reference to a compiler-generated field
                  // ISSUE: reference to a compiler-generated method
                  (bool, DateTime?) tuple1 = cDisplayClass81.\u003CLoadNonFireReminders\u003Eg__IsReminderTimeValid\u007C4(cDisplayClass81.reminder.StartDate, cDisplayClass81.reminder.ReminderTime, ref cDisplayClass82);
                  if (tuple1.Item1)
                  {
                    // ISSUE: reference to a compiler-generated field
                    cDisplayClass81.reminder.ReminderTime = tuple1.Item2;
                    // ISSUE: reference to a compiler-generated field
                    validReminders.Add(cDisplayClass81.reminder);
                  }
                }
              }
            }
            else
            {
              // ISSUE: reference to a compiler-generated field
              if (TaskReminderDao.CheckReminderValid(cDisplayClass81.reminder.ReminderTime))
              {
                // ISSUE: reference to a compiler-generated field
                validReminders.Add(cDisplayClass81.reminder);
              }
            }
          }
        }
      }
      List<ReminderModel> reminderModelList2 = validReminders;
      validReminders = (List<ReminderModel>) null;
      return reminderModelList2;
    }

    private static bool CheckReminderValid(DateTime? reminderTime)
    {
      if (reminderTime.HasValue)
      {
        DateTime? nullable = reminderTime;
        DateTime now = DateTime.Now;
        if ((nullable.HasValue ? (nullable.GetValueOrDefault() > now ? 1 : 0) : 0) != 0)
        {
          TimeSpan timeSpan = reminderTime.Value - DateTime.Now;
          if (timeSpan.TotalMinutes >= 0.0 && timeSpan.TotalMinutes < 30.0)
            return true;
        }
      }
      return false;
    }

    public static async Task BatchAddTaskReminders(IEnumerable<TaskReminderModel> reminders)
    {
      int num = await App.Connection.InsertAllAsync((IEnumerable) reminders);
    }

    public static bool IsEquals(List<TaskReminderModel> left, List<TaskReminderModel> right)
    {
      return left == null && right == null || left != null && right != null && left.Count == right.Count && left.All<TaskReminderModel>((Func<TaskReminderModel, bool>) (lr => right.Any<TaskReminderModel>((Func<TaskReminderModel, bool>) (rr =>
      {
        if (rr.trigger == lr.trigger)
          return true;
        return TaskReminderDao.OnTimeList.Contains(rr.trigger) && TaskReminderDao.OnTimeList.Contains(lr.trigger);
      }))));
    }
  }
}
