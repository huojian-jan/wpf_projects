// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.Display.CalendarDisplayService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.SyncServices;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.Sync.ClosedLoader;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Habit;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Tag;

#nullable disable
namespace ticktick_WPF.Service.Display
{
  public static class CalendarDisplayService
  {
    private static DateTime _previewStartDate;

    public static async Task<List<CalendarDisplayModel>> GetCalendarDisplayModels(
      DateTime startDate,
      DateTime endDate)
    {
      bool showCompletedInCal = LocalSettings.Settings.ShowCompletedInCal;
      bool showCheckListInCal = LocalSettings.Settings.ShowCheckListInCal;
      ProjectExtra projectFilter = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      if (projectFilter.FilterIds.Any<string>())
      {
        FilterModel filterModel = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == projectFilter.FilterIds[0]));
        List<CalendarDisplayModel> source = new List<CalendarDisplayModel>();
        if (filterModel != null)
          source = (await TaskDisplayService.GetDisplayTaskInFilter(filterModel.rule, showCompletedInCal, showCheckListInCal, false, true)).ToList<TaskBaseViewModel>().Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (t =>
          {
            if (t.StartDate.HasValue)
            {
              DateTime? startDate1 = t.StartDate;
              DateTime dateTime1 = startDate;
              if ((startDate1.HasValue ? (startDate1.GetValueOrDefault() >= dateTime1 ? 1 : 0) : 0) != 0)
              {
                DateTime? startDate2 = t.StartDate;
                DateTime dateTime2 = endDate;
                if ((startDate2.HasValue ? (startDate2.GetValueOrDefault() < dateTime2 ? 1 : 0) : 0) != 0)
                  goto label_15;
              }
            }
            if (t.DueDate.HasValue)
            {
              DateTime? dueDate1 = t.DueDate;
              DateTime dateTime3 = startDate;
              if ((dueDate1.HasValue ? (dueDate1.GetValueOrDefault() > dateTime3 ? 1 : 0) : 0) != 0)
              {
                DateTime? dueDate2 = t.DueDate;
                DateTime dateTime4 = endDate;
                if ((dueDate2.HasValue ? (dueDate2.GetValueOrDefault() < dateTime4 ? 1 : 0) : 0) != 0)
                  goto label_15;
              }
            }
            if (t.StartDate.HasValue && t.DueDate.HasValue)
            {
              DateTime? startDate3 = t.StartDate;
              DateTime dateTime5 = startDate;
              if ((startDate3.HasValue ? (startDate3.GetValueOrDefault() < dateTime5 ? 1 : 0) : 0) != 0)
              {
                DateTime? dueDate = t.DueDate;
                DateTime dateTime6 = endDate;
                if ((dueDate.HasValue ? (dueDate.GetValueOrDefault() >= dateTime6 ? 1 : 0) : 0) != 0)
                  goto label_15;
              }
            }
            if (!t.StartDate.HasValue && !t.DueDate.HasValue && t.CompletedTime.HasValue && t.Status != 0)
            {
              DateTime? completedTime1 = t.CompletedTime;
              DateTime dateTime7 = startDate;
              if ((completedTime1.HasValue ? (completedTime1.GetValueOrDefault() >= dateTime7 ? 1 : 0) : 0) != 0)
              {
                DateTime? completedTime2 = t.CompletedTime;
                DateTime dateTime8 = endDate;
                return completedTime2.HasValue && completedTime2.GetValueOrDefault() < dateTime8;
              }
            }
            return false;
label_15:
            return true;
          })).ToList<TaskBaseViewModel>().Select<TaskBaseViewModel, CalendarDisplayModel>((Func<TaskBaseViewModel, CalendarDisplayModel>) (t => new CalendarDisplayModel(t))).ToList<CalendarDisplayModel>();
        CalendarDisplayService.AssemblyEventsColor(source.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (m => m.SourceViewModel.Type == DisplayType.Event)).ToList<CalendarDisplayModel>());
        return source;
      }
      if ((projectFilter.SubscribeCalendars.Any<string>() || projectFilter.BindAccounts.Any<string>()) && projectFilter.GroupIds.Count == 0 && projectFilter.ProjectIds.Count == 0)
        projectFilter.ProjectIds.Add("none");
      List<CalendarDisplayModel> models = TaskViewModelHelper.GetModel(projectFilter.GroupIds, projectFilter.ProjectIds, projectFilter.Tags, new List<FilterDatePair>()
      {
        new FilterDatePair(new DateTime?(startDate), new DateTime?(endDate))
      }, showCompletedInCal, showCheckListInCal, inAll: projectFilter.GroupIds.Count == 0 && projectFilter.ProjectIds.Count == 0 && projectFilter.Tags.Count == 0, inCal: true, orTag: true).ToList<TaskBaseViewModel>().Select<TaskBaseViewModel, CalendarDisplayModel>((Func<TaskBaseViewModel, CalendarDisplayModel>) (t => new CalendarDisplayModel(t))).ToList<CalendarDisplayModel>();
      if (projectFilter.IsAll || projectFilter.SubscribeCalendars.Any<string>() || projectFilter.BindAccounts.Any<string>())
      {
        List<CalendarDisplayModel> modelsBetweenSpan = await CalendarDisplayService.GetCalendarDisplayModelsBetweenSpan(startDate, endDate, projectFilter.SubscribeCalendars, projectFilter.BindAccounts, showCompletedInCal);
        CalendarDisplayService.AssemblyEventsColor(modelsBetweenSpan);
        models.AddRange((IEnumerable<CalendarDisplayModel>) modelsBetweenSpan);
      }
      return models;
    }

    private static void AssemblyEventsColor(List<CalendarDisplayModel> events)
    {
      List<BindCalendarModel> bindCalendars = CacheManager.GetBindCalendars();
      Dictionary<string, string> dict = new Dictionary<string, string>();
      if (bindCalendars.Any<BindCalendarModel>())
      {
        foreach (BindCalendarModel bindCalendarModel in bindCalendars.Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => !dict.ContainsKey(cal.Id))))
          dict.Add(bindCalendarModel.Id, bindCalendarModel.Color);
      }
      List<CalendarSubscribeProfileModel> subscribeCalendars = CacheManager.GetSubscribeCalendars();
      if (subscribeCalendars.Any<CalendarSubscribeProfileModel>())
      {
        foreach (CalendarSubscribeProfileModel subscribeProfileModel in subscribeCalendars.Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => !dict.ContainsKey(cal.Id))))
          dict.Add(subscribeProfileModel.Id, subscribeProfileModel.Color);
      }
      if (events == null || !events.Any<CalendarDisplayModel>())
        return;
      foreach (CalendarDisplayModel calendarDisplayModel in events)
        calendarDisplayModel.SourceViewModel.Color = dict.ContainsKey(calendarDisplayModel.SourceViewModel.CalendarId) ? dict[calendarDisplayModel.SourceViewModel.CalendarId] : "#B1C5CF";
    }

    public static async Task<List<CalendarDisplayModel>> GetRepeatDisplayModel(
      DateTime startDate,
      DateTime endDate)
    {
      ProjectExtra projectFilter = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
      List<TaskBaseViewModel> tasks = TaskCache.GetRepeatTasks(projectFilter == null || projectFilter.FilterIds.Any<string>() || projectFilter.IsAll);
      if (projectFilter != null)
      {
        if (projectFilter.FilterIds.Any<string>())
        {
          FilterModel filterModel = CacheManager.GetFilters().FirstOrDefault<FilterModel>((Func<FilterModel, bool>) (f => f.id == projectFilter.FilterIds[0]));
          if (filterModel != null)
          {
            List<string> filterIds = (await TaskDisplayService.GetDisplayTaskInFilter(filterModel.rule, false, withChild: false)).Select<TaskBaseViewModel, string>((Func<TaskBaseViewModel, string>) (model => model.Id)).ToList<string>();
            tasks = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task => filterIds.Contains(task.Id))).ToList<TaskBaseViewModel>();
          }
        }
        else if (!projectFilter.IsAll)
        {
          List<string> projectIds = projectFilter.ProjectIds;
          List<TaskBaseViewModel> second;
          if (projectIds.Contains("#alllists"))
          {
            second = tasks;
          }
          else
          {
            if (projectFilter.GroupIds != null && projectFilter.GroupIds.Any<string>())
            {
              foreach (string groupId in projectFilter.GroupIds)
              {
                List<string> stringList = projectIds;
                stringList.AddRange((IEnumerable<string>) await ProjectDao.GetProjectIdByGroupId(groupId));
                stringList = (List<string>) null;
              }
            }
            second = tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task => projectIds.Contains(task.ProjectId))).ToList<TaskBaseViewModel>();
          }
          List<TaskBaseViewModel> first = new List<TaskBaseViewModel>();
          if (projectFilter.Tags != null)
          {
            List<string> fullTags = projectFilter.Tags.Select<string, string>((Func<string, string>) (tag => "\"" + tag.ToLower().Trim() + "\"")).ToList<string>();
            if (tasks.Any<TaskBaseViewModel>())
              first = !projectFilter.Tags.Contains("*withtags") ? tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task =>
              {
                string str = task.Tag?.ToLower().Replace(" ", string.Empty).Replace("#", string.Empty).Replace("＃", string.Empty).Trim();
                return !string.IsNullOrEmpty(task.Tag) && fullTags.Any<string>(new Func<string, bool>(str.Contains));
              })).ToList<TaskBaseViewModel>() : tasks.Where<TaskBaseViewModel>((Func<TaskBaseViewModel, bool>) (task =>
              {
                List<string> tags = TagSerializer.ToTags(task.Tag);
                // ISSUE: explicit non-virtual call
                return tags != null && __nonvirtual (tags.Count) > 0;
              })).ToList<TaskBaseViewModel>();
          }
          second.AddRange(first.Except<TaskBaseViewModel>((IEnumerable<TaskBaseViewModel>) second));
          tasks = second;
        }
      }
      List<CalendarDisplayModel> models = new List<CalendarDisplayModel>();
      foreach (TaskBaseViewModel taskBaseViewModel in tasks)
      {
        TaskBaseViewModel task = taskBaseViewModel;
        DateTime? nullable1 = task.StartDate;
        if (nullable1.HasValue)
        {
          double diffMinutes = 0.0;
          nullable1 = task.DueDate;
          if (nullable1.HasValue)
          {
            nullable1 = task.DueDate;
            DateTime dateTime1 = nullable1.Value;
            nullable1 = task.StartDate;
            DateTime dateTime2 = nullable1.Value;
            diffMinutes = (dateTime1 - dateTime2).TotalMinutes;
          }
          bool? isAllDay = task.IsAllDay;
          if (isAllDay.HasValue)
          {
            isAllDay = task.IsAllDay;
            if (isAllDay.Value)
              goto label_29;
          }
          string timeZoneName;
          if (!task.IsFloating && task.TimeZoneName != TimeZoneData.LocalTimeZoneModel?.TimeZoneName)
          {
            timeZoneName = task.TimeZoneName;
            goto label_31;
          }
label_29:
          timeZoneName = TimeZoneData.LocalTimeZoneModel?.TimeZoneName;
label_31:
          string timeZone = timeZoneName;
          RepeatDao.GetRepeatFlagDate(task, startDate.AddDays((double) (-1 - (int) diffMinutes / 1440)), endDate.AddDays(1.0)).ForEach((Action<DateTime>) (date =>
          {
            DateTime dateTime3 = date;
            DateTime? nullable2 = new DateTime?();
            if (task.DueDate.HasValue)
              nullable2 = new DateTime?(TimeZoneUtils.ToLocalTime(TimeZoneUtils.LocalToTargetTzTime(date, timeZone).AddMinutes(diffMinutes), timeZone));
            if (!(dateTime3 >= startDate) || !(dateTime3 < endDate))
            {
              DateTime? nullable3 = nullable2;
              DateTime dateTime4 = startDate;
              if ((nullable3.HasValue ? (nullable3.GetValueOrDefault() > dateTime4 ? 1 : 0) : 0) != 0)
              {
                DateTime? nullable4 = nullable2;
                DateTime dateTime5 = endDate;
                if ((nullable4.HasValue ? (nullable4.GetValueOrDefault() <= dateTime5 ? 1 : 0) : 0) != 0)
                  goto label_6;
              }
              DateTime? nullable5 = nullable2;
              DateTime dateTime6 = endDate;
              if ((nullable5.HasValue ? (nullable5.GetValueOrDefault() > dateTime6 ? 1 : 0) : 0) == 0 || !(dateTime3 < startDate))
                return;
            }
label_6:
            models.Add(new CalendarDisplayModel(task)
            {
              repeatDiff = (date - (task.StartDate ?? date)).TotalDays
            });
          }));
        }
      }
      List<string> expiredProjectIds = CacheManager.GetExpiredProjectIds();
      List<CalendarDisplayModel> repeatDisplayModel = expiredProjectIds.Any<string>() ? models.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (model => !expiredProjectIds.Contains(model.SourceViewModel.ProjectId))).ToList<CalendarDisplayModel>() : models;
      tasks = (List<TaskBaseViewModel>) null;
      return repeatDisplayModel;
    }

    public static async Task<List<CalendarDisplayModel>> GetRepeatDisplayModels(
      DateTime start,
      DateTime end,
      bool allSelected = false)
    {
      List<CalendarEventModel> skipEvents = await CalendarEventDao.GetSkipEvents();
      string userId = Utils.GetCurrentUserIdInt().ToString();
      Dictionary<string, CalendarSubscribeProfileModel> subCal = CacheManager.GetSubscribeCalendars().Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal => cal.Show != "hidden" && cal.UserId == userId)).ToDictionary<CalendarSubscribeProfileModel, string, CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, string>) (cal => cal.Id), (Func<CalendarSubscribeProfileModel, CalendarSubscribeProfileModel>) (cal => cal));
      List<string> subCalIds = subCal.Keys.ToList<string>();
      List<string> accounts = CacheManager.GetBindCalendarAccounts().Select<BindCalendarAccountModel, string>((Func<BindCalendarAccountModel, string>) (cal => cal.Id)).ToList<string>();
      subCalIds.AddRange((IEnumerable<string>) CacheManager.GetBindCalendars().Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => accounts.Contains(cal.AccountId) && cal.Show != "hidden" && cal.UserId == userId)).Select<BindCalendarModel, string>((Func<BindCalendarModel, string>) (cal => cal.Id)).ToList<string>());
      if (!allSelected)
      {
        ProjectExtra projectExtra = ProjectExtra.Deserialize(LocalSettings.Settings.CalendarFilterData);
        if (projectExtra != null)
        {
          List<string> subscribeCalendars = projectExtra.SubscribeCalendars;
          // ISSUE: explicit non-virtual call
          bool flag = subscribeCalendars != null && __nonvirtual (subscribeCalendars.Contains("#allSubscribe"));
          if (!projectExtra.IsAll && !flag)
          {
            List<string> subCalFilter = projectExtra.SubscribeCalendars;
            subCalIds = subCalFilter != null ? subCalIds.Where<string>((Func<string, bool>) (id => subCalFilter.Contains(id))).ToList<string>() : new List<string>();
          }
        }
      }
      List<CalendarEventModel> repeatEvents = await CalendarEventDao.GetRepeatEvents();
      List<CalendarDisplayModel> calModels = new List<CalendarDisplayModel>();
      List<string> hiddenKeys = await ArchivedDao.GetArchivedKeys();
      foreach (CalendarEventModel calendarEventModel1 in repeatEvents)
      {
        CalendarEventModel model = calendarEventModel1;
        CalendarEventModel calendarEventModel2 = model;
        DateTime? nullable1;
        int num1;
        if (calendarEventModel2 == null)
        {
          num1 = 1;
        }
        else
        {
          nullable1 = calendarEventModel2.DueStart;
          num1 = !nullable1.HasValue ? 1 : 0;
        }
        if (num1 == 0 && subCalIds.Contains(model.CalendarId))
        {
          if (!string.IsNullOrEmpty(model.ExDates))
          {
            try
            {
              model.ExDateList = JsonConvert.DeserializeObject<List<DateTime>>(model.ExDates);
            }
            catch (Exception ex)
            {
              model.ExDateList = (List<DateTime>) null;
            }
          }
          double diff = 0.0;
          DateTime targetTzTime;
          if (!model.IsAllDay)
          {
            nullable1 = model.DueStart;
            targetTzTime = TimeZoneUtils.LocalToTargetTzTime(nullable1.Value, model.TimeZone);
          }
          else
          {
            nullable1 = model.DueStart;
            targetTzTime = nullable1.Value;
          }
          DateTime calStart = targetTzTime;
          nullable1 = model.DueEnd;
          if (nullable1.HasValue)
          {
            nullable1 = model.DueEnd;
            DateTime dateTime1 = nullable1.Value;
            nullable1 = model.DueStart;
            DateTime dateTime2 = nullable1.Value;
            diff = (dateTime1 - dateTime2).TotalMinutes;
          }
          RepeatDao.GetEventRepeatDates(model.RepeatFlag, calStart, start.AddDays((double) (-2 - (int) diff / 1440)), end.AddDays(1.0), model.TimeZone, model.IsAllDay).ForEach((Action<DateTime>) (date =>
          {
            if (!(date < end))
              return;
            model.DueStart = new DateTime?(date);
            DateTime dateTime3 = model.IsAllDay ? date : TimeZoneUtils.LocalToTargetTzTime(date, model.TimeZone);
            DateTime? nullable2 = model.DueEnd;
            if (nullable2.HasValue)
              model.DueEnd = new DateTime?(model.IsAllDay ? dateTime3.AddMinutes(diff) : TimeZoneUtils.ToLocalTime(dateTime3.AddMinutes(diff), model.TimeZone));
            bool flag1 = false;
            bool flag2 = false;
            if (model.ExDateList != null && model.ExDateList.Any<DateTime>() && model.ExDateList.Contains(dateTime3.Date))
              flag1 = true;
            if (skipEvents.Any<CalendarEventModel>())
              flag2 = model.IsSkipped(skipEvents, new DateTime?(date));
            if (flag1 || flag2)
              return;
            nullable2 = model.DueStart;
            DateTime dateTime4 = start;
            if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() >= dateTime4 ? 1 : 0) : 0) != 0)
            {
              nullable2 = model.DueStart;
              DateTime dateTime5 = end;
              if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() < dateTime5 ? 1 : 0) : 0) != 0)
                goto label_14;
            }
            nullable2 = model.DueEnd;
            DateTime dateTime6 = start;
            if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() > dateTime6 ? 1 : 0) : 0) != 0)
            {
              nullable2 = model.DueEnd;
              DateTime dateTime7 = end;
              if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() <= dateTime7 ? 1 : 0) : 0) != 0)
                goto label_14;
            }
            nullable2 = model.DueEnd;
            DateTime dateTime8 = end;
            if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() > dateTime8 ? 1 : 0) : 0) == 0)
              return;
            nullable2 = model.DueStart;
            DateTime dateTime9 = start;
            if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() < dateTime9 ? 1 : 0) : 0) == 0)
              return;
label_14:
            int? status = hiddenKeys.Contains(ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(model))) ? new int?(2) : new int?();
            string color = subCal.ContainsKey(model.CalendarId) ? subCal[model.CalendarId].Color : (string) null;
            if (!LocalSettings.Settings.ShowCompletedInCal)
            {
              int? nullable3 = status;
              int num2 = 2;
              if (nullable3.GetValueOrDefault() == num2 & nullable3.HasValue)
                return;
            }
            calModels.Add(new CalendarDisplayModel(model, status, color)
            {
              SourceViewModel = {
                Editable = false
              }
            });
          }));
        }
      }
      CalendarDisplayService.AssemblyEventsColor(calModels);
      List<CalendarDisplayModel> repeatDisplayModels = calModels;
      subCalIds = (List<string>) null;
      repeatEvents = (List<CalendarEventModel>) null;
      return repeatDisplayModels;
    }

    public static async Task<List<CalendarDisplayModel>> GetCalendarDisplayModelsBetweenSpan(
      DateTime start,
      DateTime end,
      List<string> subscribes,
      List<string> accounts,
      bool withArchived = true)
    {
      bool allCal = subscribes.Count == 0 && accounts.Count == 0;
      List<string> calendarIds = CacheManager.GetSubscribeCalendars().Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (cal =>
      {
        if (!(cal.Show != "hidden"))
          return false;
        return allCal || subscribes.Contains(cal.Id);
      })).Select<CalendarSubscribeProfileModel, string>((Func<CalendarSubscribeProfileModel, string>) (c => c.Id)).ToList<string>();
      calendarIds.AddRange((IEnumerable<string>) CacheManager.GetBindCalendars().Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => (allCal || accounts.Contains(cal.AccountId) || subscribes.Contains(cal.Id)) && cal.Show != "hidden")).Select<BindCalendarModel, string>((Func<BindCalendarModel, string>) (cal => cal.Id)).ToList<string>());
      if (!withArchived && start < DateTime.Today)
        start = DateTime.Today;
      List<CalendarEventModel> events = await CalendarEventDao.GetEventsBetweenSpan(start, end);
      List<CalendarEventModel> skipEvents = await CalendarEventDao.GetSkipEvents();
      List<string> archivedKeys = await ArchivedDao.GetArchivedKeys();
      if (!events.Any<CalendarEventModel>())
        return new List<CalendarDisplayModel>();
      List<CalendarDisplayModel> events1 = new List<CalendarDisplayModel>();
      foreach (CalendarEventModel calendarEventModel in events)
      {
        if (!calendarIds.Any<string>() || calendarIds.Contains(calendarEventModel.CalendarId))
        {
          if (!string.IsNullOrEmpty(calendarEventModel.ExDates))
          {
            try
            {
              DateTime? dueStart = calendarEventModel.DueStart;
              ref DateTime? local = ref dueStart;
              DateTime dateTime = local.HasValue ? local.GetValueOrDefault().Date : new DateTime();
              if (JsonConvert.DeserializeObject<List<DateTime>>(calendarEventModel.ExDates).Contains(dateTime.Date))
                continue;
            }
            catch (Exception ex)
            {
            }
          }
          if (!calendarEventModel.IsSkipped(skipEvents))
          {
            bool flag = archivedKeys.Contains(ArchivedEventDao.GenerateEventKey(new EventArchiveArgs(calendarEventModel)));
            if (!(!withArchived & flag))
              events1.Add(new CalendarDisplayModel(calendarEventModel, flag ? new int?(2) : new int?()));
          }
        }
      }
      CalendarDisplayService.AssemblyEventsColor(events1);
      return events1;
    }

    private static async Task<List<CalendarDisplayModel>> GetCalendarDisplayHabits(
      DateTime start,
      DateTime end)
    {
      List<CalendarDisplayModel> result = new List<CalendarDisplayModel>();
      DateTime startDate = start.Date;
      DateTime endDate = end.Date;
      if (endDate.Date == startDate.Date)
        endDate = endDate.AddDays(1.0);
      if (LocalSettings.Settings.ShowHabit && LocalSettings.Settings.HabitInCal)
      {
        List<HabitModel> habits = await HabitDao.GetNeedCheckHabits();
        DateTime today1 = DateTime.Today;
        DateTime start1 = today1.AddDays(-30.0);
        today1 = DateTime.Today;
        DateTime end1 = today1.AddDays(1.0);
        List<HabitCheckInModel> monthCheckIns = await HabitCheckInDao.GetCheckInsInSpan(start1, end1);
        if (DateTime.Today >= startDate && DateTime.Today < endDate)
        {
          foreach (HabitModel habitModel in habits)
          {
            HabitModel habit = habitModel;
            List<HabitCheckInModel> list = monthCheckIns.Where<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (c => c.HabitId == habit.Id)).ToList<HabitCheckInModel>();
            HabitCheckInModel checkIn = list.FirstOrDefault<HabitCheckInModel>((Func<HabitCheckInModel, bool>) (v => v.HabitId == habit.Id && v.CheckinStamp == DateTime.Today.ToString("yyyyMMdd", (IFormatProvider) CultureInfo.InvariantCulture)));
            if (checkIn == null || checkIn.Value < checkIn.Goal && checkIn.CheckStatus != 1)
            {
              if (await HabitUtils.IsHabitValidInToday(habit, list))
              {
                bool flag1 = false;
                bool flag2 = false;
                if (checkIn != null)
                {
                  flag1 = checkIn.Value >= checkIn.Goal;
                  flag2 = checkIn.CheckStatus == 1;
                }
                int num = flag1 ? 2 : (flag2 ? -1 : 0);
                HabitModel habit1 = habit;
                int status = num;
                DateTime today2 = DateTime.Today;
                DateTime? nullable = new DateTime?();
                DateTime? completeTime = nullable;
                CalendarDisplayModel calendarDisplayModel = CalendarDisplayModel.Build(habit1, status, today2, completeTime);
                nullable = calendarDisplayModel.StartDate;
                DateTime dateTime1 = start;
                if ((nullable.HasValue ? (nullable.GetValueOrDefault() >= dateTime1 ? 1 : 0) : 0) != 0)
                {
                  nullable = calendarDisplayModel.StartDate;
                  DateTime dateTime2 = end;
                  if ((nullable.HasValue ? (nullable.GetValueOrDefault() < dateTime2 ? 1 : 0) : 0) != 0)
                    result.Add(calendarDisplayModel);
                }
              }
            }
            checkIn = (HabitCheckInModel) null;
          }
        }
        if (LocalSettings.Settings.ShowCompletedInCal)
        {
          List<HabitCheckInModel> checkInsInSpan = await HabitCheckInDao.GetCheckInsInSpan(startDate, endDate);
          if (checkInsInSpan.Any<HabitCheckInModel>())
          {
            foreach (HabitCheckInModel habitCheckInModel in checkInsInSpan)
            {
              HabitCheckInModel checkIn = habitCheckInModel;
              if (checkIn.Value >= checkIn.Goal || checkIn.CheckStatus == 1)
              {
                HabitModel habit = habits.FirstOrDefault<HabitModel>((Func<HabitModel, bool>) (habitModel => habitModel.Id == checkIn.HabitId));
                if (habit != null)
                {
                  int status = checkIn.Value >= checkIn.Goal ? 2 : (checkIn.CheckStatus == 1 ? -1 : 0);
                  CalendarDisplayModel calendarDisplayModel = CalendarDisplayModel.Build(habit, status, DateTime.ParseExact(checkIn.CheckinStamp, "yyyyMMdd", (IFormatProvider) App.Ci), checkIn.CheckinTime);
                  DateTime? startDate1 = calendarDisplayModel.StartDate;
                  DateTime dateTime3 = start;
                  if ((startDate1.HasValue ? (startDate1.GetValueOrDefault() >= dateTime3 ? 1 : 0) : 0) != 0)
                  {
                    startDate1 = calendarDisplayModel.StartDate;
                    DateTime dateTime4 = end;
                    if ((startDate1.HasValue ? (startDate1.GetValueOrDefault() < dateTime4 ? 1 : 0) : 0) != 0)
                      result.Add(calendarDisplayModel);
                  }
                }
              }
            }
          }
        }
        habits = (List<HabitModel>) null;
        monthCheckIns = (List<HabitCheckInModel>) null;
      }
      List<CalendarDisplayModel> calendarDisplayHabits = result;
      result = (List<CalendarDisplayModel>) null;
      return calendarDisplayHabits;
    }

    private static async Task<List<CalendarDisplayModel>> GetCalendarDisplayPomo(
      DateTime start,
      DateTime end)
    {
      List<PomodoroModel> pomos = await PomoDao.GetPomoByDateSpan(start, end);
      foreach (PomodoroModel pomodoroModel1 in pomos)
      {
        PomodoroModel pomodoroModel = pomodoroModel1;
        pomodoroModel.Tasks = await PomoTaskDao.GetByPomoId(pomodoroModel1.Id);
        pomodoroModel = (PomodoroModel) null;
      }
      List<CalendarDisplayModel> list = pomos.Select<PomodoroModel, CalendarDisplayModel>((Func<PomodoroModel, CalendarDisplayModel>) (p => new CalendarDisplayModel(p))).ToList<CalendarDisplayModel>();
      pomos = (List<PomodoroModel>) null;
      return list;
    }

    public static async Task<List<CalendarDisplayModel>> GetDisplayModels(
      DateTime startDate,
      DateTime endDate,
      bool withCourses = false,
      bool withFocus = false)
    {
      if (CalendarDisplayService._previewStartDate != startDate)
        CalendarDisplayService.PullRemoveData(startDate, endDate);
      List<CalendarDisplayModel> tasks = await CalendarDisplayService.GetCalendarDisplayModels(startDate, endDate);
      CalendarDisplayService._previewStartDate = startDate;
      await Task.WhenAll(CircleTask(), RepeatEvent(), HabitTask(), PomoTask(), GetCourses());
      return tasks;

      async Task CircleTask()
      {
        if (!LocalSettings.Settings.ShowRepeatCircles)
          return;
        List<CalendarDisplayModel> repeatDisplayModel = await CalendarDisplayService.GetRepeatDisplayModel(startDate, endDate);
        if (repeatDisplayModel.Count <= 0)
          return;
        lock (tasks)
          tasks.AddRange((IEnumerable<CalendarDisplayModel>) repeatDisplayModel);
      }

      async Task RepeatEvent()
      {
        List<CalendarDisplayModel> repeatDisplayModels = await CalendarDisplayService.GetRepeatDisplayModels(startDate, endDate);
        if (repeatDisplayModels.Count <= 0)
          return;
        lock (tasks)
          tasks.AddRange((IEnumerable<CalendarDisplayModel>) repeatDisplayModels);
      }

      async Task HabitTask()
      {
        List<CalendarDisplayModel> calendarDisplayHabits = await CalendarDisplayService.GetCalendarDisplayHabits(startDate, endDate);
        if (!calendarDisplayHabits.Any<CalendarDisplayModel>())
          return;
        lock (tasks)
          tasks.AddRange((IEnumerable<CalendarDisplayModel>) calendarDisplayHabits);
      }

      async Task PomoTask()
      {
        if (!withFocus && !LocalSettings.Settings.ShowFocusRecord)
          return;
        PomoSyncService.TryPullRemote(startDate, endDate);
        List<CalendarDisplayModel> pomos = await CalendarDisplayService.GetCalendarDisplayPomo(startDate, endDate);
        if (pomos.Any<CalendarDisplayModel>())
        {
          foreach (CalendarDisplayModel pomo in pomos)
          {
            DateTime? nullable = pomo.StartDate;
            if (nullable.HasValue)
            {
              nullable = pomo.DueDate;
              if (nullable.HasValue)
              {
                nullable = pomo.StartDate;
                DateTime dateTime = nullable.Value;
                DateTime date1 = dateTime.Date;
                nullable = pomo.DueDate;
                dateTime = nullable.Value;
                DateTime date2 = dateTime.Date;
                if (date1 != date2)
                {
                  List<PomoTask> pomoTasksByPomoId = await PomoDao.GetPomoTasksByPomoId(pomo.Id);
                  if (pomoTasksByPomoId != null && pomoTasksByPomoId.Count > 0)
                    pomo.SourceViewModel.DueDate = new DateTime?(pomoTasksByPomoId.Max<PomoTask, DateTime>((Func<PomoTask, DateTime>) (m => m.EndTime)));
                }
              }
            }
          }
          lock (tasks)
            tasks.AddRange((IEnumerable<CalendarDisplayModel>) pomos);
        }
        pomos = (List<CalendarDisplayModel>) null;
      }

      async Task GetCourses()
      {
        if (!withCourses || !LocalSettings.Settings.ShowTimeTableInCal)
          return;
        List<CalendarDisplayModel> displayCourses = await CalendarDisplayService.GetDisplayCourses(startDate, endDate);
        if (displayCourses.Count <= 0)
          return;
        List<CalendarDisplayModel> list = displayCourses.Where<CalendarDisplayModel>((Func<CalendarDisplayModel, bool>) (c =>
        {
          DateTime? nullable;
          if (!LocalSettings.Settings.ShowCompletedInCal)
          {
            nullable = c.DueDate;
            DateTime today = DateTime.Today;
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() <= today ? 1 : 0) : 0) != 0)
              return false;
          }
          nullable = c.StartDate;
          DateTime dateTime1 = startDate;
          if ((nullable.HasValue ? (nullable.GetValueOrDefault() >= dateTime1 ? 1 : 0) : 0) != 0)
          {
            nullable = c.StartDate;
            DateTime dateTime2 = endDate;
            if ((nullable.HasValue ? (nullable.GetValueOrDefault() < dateTime2 ? 1 : 0) : 0) != 0)
              return true;
          }
          nullable = c.DueDate;
          DateTime dateTime3 = startDate;
          if ((nullable.HasValue ? (nullable.GetValueOrDefault() > dateTime3 ? 1 : 0) : 0) == 0)
            return false;
          nullable = c.DueDate;
          DateTime dateTime4 = endDate;
          return nullable.HasValue && nullable.GetValueOrDefault() <= dateTime4;
        })).ToList<CalendarDisplayModel>();
        lock (tasks)
          tasks.AddRange((IEnumerable<CalendarDisplayModel>) list);
      }
    }

    private static async Task PullRemoveData(DateTime startDate, DateTime endDate)
    {
      CalendarEventLoader.PullRemoteEvent(startDate, endDate);
      if (!await ClosedTaskWithFilterLoader.CalClosedLoader.LoadTasksInSpan(startDate, endDate))
        return;
      GlobalEventManager.NotifyReloadCalendar();
    }

    private static async Task<List<CalendarDisplayModel>> GetDisplayCourses(
      DateTime startDate,
      DateTime endDate)
    {
      return (await ScheduleService.GetCoursesInSpan(startDate, endDate, !LocalSettings.Settings.ShowCompletedInCal)).Select<CourseDisplayModel, CalendarDisplayModel>((Func<CourseDisplayModel, CalendarDisplayModel>) (c => new CalendarDisplayModel(c))).ToList<CalendarDisplayModel>();
    }

    public static async Task<List<CalendarDisplayModel>> GetDisplayModelInDay(
      DateTime date,
      bool withCourse = false)
    {
      DateTime startDate = date;
      DateTime endDate = startDate.AddDays(1.0);
      return await CalendarDisplayService.GetDisplayModels(startDate, endDate, withCourse);
    }
  }
}
