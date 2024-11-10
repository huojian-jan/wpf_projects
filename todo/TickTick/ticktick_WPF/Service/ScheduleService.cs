// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.ScheduleService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.ReminderTime;
using TickTickDao;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Service
{
  public static class ScheduleService
  {
    public static ConcurrentDictionary<string, List<CourseDetailModel>> CourseDetailDict = new ConcurrentDictionary<string, List<CourseDetailModel>>();
    public static ConcurrentDictionary<long, BlockingList<CourseDisplayModel>> CourseDisplayModelDict = new ConcurrentDictionary<long, BlockingList<CourseDisplayModel>>();

    public static async Task GetRemoteSchedules()
    {
      List<CourseScheduleModel> remotes = await Communicator.GetAllCourseSchedules();
      List<CourseScheduleModel> locals;
      if (remotes == null)
      {
        remotes = (List<CourseScheduleModel>) null;
        locals = (List<CourseScheduleModel>) null;
      }
      else
      {
        locals = await ScheduleDao.GetAllSchedulesAsync();
        bool flag = false;
        foreach (CourseScheduleModel courseScheduleModel1 in remotes)
        {
          CourseScheduleModel remote = courseScheduleModel1;
          CourseScheduleModel courseScheduleModel2 = locals.FirstOrDefault<CourseScheduleModel>((Func<CourseScheduleModel, bool>) (m => m.Id == remote.Id));
          if (courseScheduleModel2 != null)
          {
            locals.Remove(courseScheduleModel2);
            if (courseScheduleModel2.Etag != remote.Etag)
            {
              remote._Id = courseScheduleModel2._Id;
              remote.SyncStatus = 2;
              remote.UserId = LocalSettings.Settings.LoginUserId;
              await ScheduleDao.UpdateRemoteScheduleAsync(remote);
              flag = true;
            }
          }
          else
          {
            remote.SyncStatus = 2;
            await ScheduleDao.AddRemoteScheduleAsync(remote);
            flag = true;
          }
        }
        foreach (CourseScheduleModel model in locals)
        {
          if (model.SyncStatus == 2)
          {
            await ScheduleDao.DeleteScheduleAsync(model);
            flag = true;
          }
        }
        if (!flag)
        {
          remotes = (List<CourseScheduleModel>) null;
          locals = (List<CourseScheduleModel>) null;
        }
        else
        {
          ScheduleService.CourseDisplayModelDict.Clear();
          TaskCountCache.TryReloadSmartCount();
          DataChangedNotifier.OnScheduleChanged();
          if (ABTestManager.IsNewRemindCalculate())
          {
            CourseReminderCalculator.InitCourseReminders();
            remotes = (List<CourseScheduleModel>) null;
            locals = (List<CourseScheduleModel>) null;
          }
          else
          {
            ReminderCalculator.AssembleReminders();
            remotes = (List<CourseScheduleModel>) null;
            locals = (List<CourseScheduleModel>) null;
          }
        }
      }
    }

    private static async Task<List<CourseDisplayModel>> GetCoursesBetweenDate(
      DateTime start,
      DateTime end)
    {
      int totalDays = (int) (end - start).TotalDays;
      bool flag = true;
      for (int index = 0; index < totalDays; ++index)
      {
        if (!ScheduleService.CourseDisplayModelDict.ContainsKey(start.AddDays((double) index).Ticks))
        {
          flag = false;
          break;
        }
      }
      return flag ? ScheduleService.GetCoursesBetweenDateInDict(start, Math.Max(totalDays, 1)) : await ScheduleService.GetCoursesBetweenDateAsync(start, end);
    }

    public static async Task<List<CourseDisplayModel>> GetCoursesInSpan(
      DateTime start,
      DateTime end,
      bool checkArchive)
    {
      List<CourseDisplayModel> courses = await ScheduleService.GetCoursesBetweenDate(start, end);
      List<CourseDisplayModel> result = new List<CourseDisplayModel>();
      if (courses.Count > 0)
      {
        List<string> archivedKeys = await ArchivedDao.GetArchivedKeys(ArchiveKind.Course);
        foreach (CourseDisplayModel courseDisplayModel in courses)
        {
          if (courseDisplayModel != null)
          {
            courseDisplayModel.Archived = false;
            // ISSUE: explicit non-virtual call
            if (archivedKeys != null && __nonvirtual (archivedKeys.Contains(courseDisplayModel.UniqueId)))
            {
              courseDisplayModel.Archived = true;
              if (checkArchive)
                continue;
            }
            result.Add(courseDisplayModel);
          }
        }
      }
      List<CourseDisplayModel> coursesInSpan = result;
      courses = (List<CourseDisplayModel>) null;
      result = (List<CourseDisplayModel>) null;
      return coursesInSpan;
    }

    private static List<CourseDisplayModel> GetCoursesBetweenDateInDict(DateTime start, int days)
    {
      List<CourseDisplayModel> betweenDateInDict = new List<CourseDisplayModel>();
      for (int index = 0; index < days; ++index)
      {
        ConcurrentDictionary<long, BlockingList<CourseDisplayModel>> displayModelDict = ScheduleService.CourseDisplayModelDict;
        DateTime dateTime = start.Date;
        dateTime = dateTime.AddDays((double) index);
        long ticks = dateTime.Ticks;
        BlockingList<CourseDisplayModel> blockingList;
        ref BlockingList<CourseDisplayModel> local = ref blockingList;
        if (displayModelDict.TryGetValue(ticks, out local))
        {
          List<CourseDisplayModel> list = blockingList.Value.ToList<CourseDisplayModel>();
          betweenDateInDict.AddRange((IEnumerable<CourseDisplayModel>) list);
        }
      }
      return betweenDateInDict;
    }

    private static async Task<List<CourseDisplayModel>> GetCoursesBetweenDateAsync(
      DateTime start,
      DateTime end)
    {
      List<CourseDisplayModel> result = new List<CourseDisplayModel>();
      List<CourseScheduleModel> allSchedulesAsync = await ScheduleDao.GetAllSchedulesAsync();
      if (allSchedulesAsync == null)
        return result;
      int days = (int) (end - start).TotalDays;
      foreach (CourseScheduleModel courseScheduleModel in allSchedulesAsync)
      {
        CourseScheduleModel schedule = courseScheduleModel;
        if (!(schedule.StartDate > end) && !string.IsNullOrEmpty(schedule.LessonTimesStr))
        {
          int startWeek = DateUtils.GetWeekNum(start, schedule.StartDate);
          if (startWeek <= schedule.WeekCount)
          {
            int endWeek = DateUtils.GetWeekNum(end, schedule.StartDate);
            List<CourseModel> byScheduleIdAsync = await ScheduleDao.GetCoursedByScheduleIdAsync(schedule.Id);
            if (byScheduleIdAsync != null && byScheduleIdAsync.Count != 0)
            {
              Dictionary<int, string[]> dictionary = JsonConvert.DeserializeObject<Dictionary<int, string[]>>(schedule.LessonTimesStr);
              if (dictionary != null && dictionary.Count != 0)
              {
                List<string> stringList = string.IsNullOrEmpty(schedule.RemindersStr) ? (List<string>) null : JsonConvert.DeserializeObject<List<string>>(schedule.RemindersStr);
                foreach (CourseModel course in byScheduleIdAsync)
                {
                  if (!string.IsNullOrEmpty(course.ItemsStr))
                  {
                    List<CourseDetailModel> detailModelsFromJson = ScheduleService.GetCourseDetailModelsFromJson(course.ItemsStr);
                    course.Items = detailModelsFromJson;
                    for (int index = 0; index < detailModelsFromJson.Count; ++index)
                    {
                      CourseDetailModel courseDetailModel = detailModelsFromJson[index];
                      if (courseDetailModel.Weeks != null && courseDetailModel.Weeks.Length != 0 && dictionary.ContainsKey(courseDetailModel.StartLesson))
                      {
                        string[] strArray1 = dictionary[courseDetailModel.StartLesson];
                        if ((strArray1 != null ? (strArray1.Length != 2 ? 1 : 0) : 1) == 0 && dictionary.ContainsKey(courseDetailModel.EndLesson))
                        {
                          string[] strArray2 = dictionary[courseDetailModel.EndLesson];
                          if ((strArray2 != null ? (strArray2.Length != 2 ? 1 : 0) : 1) == 0)
                          {
                            (int hours1, int minutes1) = DateUtils.GetHourAndMinuteInString(dictionary[courseDetailModel.StartLesson][0]);
                            (int hours2, int minutes2) = DateUtils.GetHourAndMinuteInString(dictionary[courseDetailModel.EndLesson][1]);
                            foreach (int week in courseDetailModel.Weeks)
                            {
                              if (week >= startWeek && week <= endWeek)
                              {
                                DateTime dateByWeek = DateUtils.GetDateByWeek(schedule.StartDate, week, courseDetailModel.Weekday);
                                if (dateByWeek >= schedule.StartDate && dateByWeek >= start && dateByWeek < end)
                                {
                                  DateTime start1 = DateUtils.SetHourAndMinuteOnly(dateByWeek, hours1, minutes1);
                                  DateTime end1 = DateUtils.SetHourAndMinuteOnly(dateByWeek, hours2, minutes2);
                                  result.Add(new CourseDisplayModel(course, start1, end1)
                                  {
                                    Reminders = stringList,
                                    Room = courseDetailModel.Room,
                                    EndLesson = courseDetailModel.EndLesson,
                                    StartLesson = courseDetailModel.StartLesson,
                                    Teacher = courseDetailModel.Teacher,
                                    ScheduleName = schedule.Name,
                                    Index = index
                                  });
                                }
                              }
                            }
                          }
                        }
                      }
                    }
                  }
                }
                schedule = (CourseScheduleModel) null;
              }
            }
          }
        }
      }
      for (int index = 0; index < days; ++index)
      {
        long ticks = start.AddDays((double) index).Ticks;
        if (!ScheduleService.CourseDisplayModelDict.ContainsKey(ticks))
          ScheduleService.CourseDisplayModelDict[ticks] = new BlockingList<CourseDisplayModel>();
      }
      foreach (CourseDisplayModel courseDisplayModel in result)
      {
        CourseDisplayModel model = courseDisplayModel;
        DateTime dateTime = model.CourseStart;
        dateTime = dateTime.Date;
        long ticks = dateTime.Ticks;
        if (ScheduleService.CourseDisplayModelDict.ContainsKey(ticks))
        {
          BlockingList<CourseDisplayModel> blockingList = ScheduleService.CourseDisplayModelDict[ticks];
          blockingList.RemoveAll((Predicate<CourseDisplayModel>) (m => m.Id == model.Id && m.StartLesson == model.StartLesson));
          blockingList.Add(model);
        }
      }
      return ScheduleService.GetCoursesBetweenDateInDict(start, days);
    }

    public static List<CourseDetailModel> GetCourseDetailModelsFromJson(string itemsStr)
    {
      List<CourseDetailModel> detailModelsFromJson;
      if (ScheduleService.CourseDetailDict.ContainsKey(itemsStr))
      {
        detailModelsFromJson = ScheduleService.CourseDetailDict[itemsStr];
      }
      else
      {
        detailModelsFromJson = JsonConvert.DeserializeObject<List<CourseDetailModel>>(itemsStr);
        ScheduleService.CourseDetailDict[itemsStr] = detailModelsFromJson;
      }
      return detailModelsFromJson;
    }

    public static async Task<List<ReminderModel>> LoadCourseReminders()
    {
      List<ReminderModel> result = new List<ReminderModel>();
      List<CourseDisplayModel> courses = await ScheduleService.GetCoursesInSpan(DateTime.Today, DateTime.Today.AddDays(1.0), true);
      Dictionary<string, ReminderDelayModel> dictionaryEx = (await ReminderDelayDao.GetDelayModelByType("course")).ToDictionaryEx<ReminderDelayModel, string, ReminderDelayModel>((Func<ReminderDelayModel, string>) (d => d.ObjId), (Func<ReminderDelayModel, ReminderDelayModel>) (d => d));
      foreach (CourseDisplayModel courseDisplayModel in courses)
      {
        if (courseDisplayModel.Reminders != null)
        {
          if (!string.IsNullOrEmpty(courseDisplayModel.Id) && dictionaryEx.ContainsKey(courseDisplayModel.Id))
          {
            ReminderDelayModel reminderDelayModel = dictionaryEx[courseDisplayModel.Id];
            DateTime? nullable = reminderDelayModel.RemindTime;
            DateTime courseStart = courseDisplayModel.CourseStart;
            if ((nullable.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == courseStart ? 1 : 0) : 1) : 0) != 0)
            {
              nullable = reminderDelayModel.NextTime;
              if (nullable.HasValue)
              {
                List<ReminderModel> reminderModelList = result;
                ReminderModel reminderModel = new ReminderModel();
                reminderModel.Id = courseDisplayModel.Id;
                reminderModel.GroupId = courseDisplayModel.ScheduleId;
                reminderModel.Type = 8;
                reminderModel.IsAllDay = new bool?(false);
                reminderModel.StartDate = new DateTime?(courseDisplayModel.CourseStart);
                nullable = reminderDelayModel.NextTime;
                reminderModel.ReminderTime = new DateTime?(nullable.Value);
                reminderModel.Title = courseDisplayModel.Title;
                reminderModel.Content = string.Format(Utils.GetString("LessonNum"), courseDisplayModel.StartLesson == courseDisplayModel.EndLesson ? (object) (courseDisplayModel.StartLesson.ToString() ?? "") : (object) (courseDisplayModel.StartLesson.ToString() + " - " + courseDisplayModel.EndLesson.ToString())) + (string.IsNullOrEmpty(courseDisplayModel.Room) ? "" : ", " + courseDisplayModel.Room) + (string.IsNullOrEmpty(courseDisplayModel.Teacher) ? "" : ", " + courseDisplayModel.Teacher);
                reminderModelList.Add(reminderModel);
              }
            }
          }
          foreach (string reminder in courseDisplayModel.Reminders)
          {
            DateTime dateTime = courseDisplayModel.CourseStart - TriggerUtils.ParseTrigger(reminder);
            TimeSpan timeSpan = dateTime - DateTime.Now;
            if (timeSpan.TotalMinutes >= 0.0 && timeSpan.TotalMinutes < 15.0)
              result.Add(new ReminderModel()
              {
                Id = courseDisplayModel.Id,
                GroupId = courseDisplayModel.ScheduleId,
                Type = 8,
                IsAllDay = new bool?(false),
                StartDate = new DateTime?(courseDisplayModel.CourseStart),
                ReminderTime = new DateTime?(dateTime),
                Title = courseDisplayModel.Title,
                Content = string.Format(Utils.GetString("LessonNum"), courseDisplayModel.StartLesson == courseDisplayModel.EndLesson ? (object) (courseDisplayModel.StartLesson.ToString() ?? "") : (object) (courseDisplayModel.StartLesson.ToString() + " - " + courseDisplayModel.EndLesson.ToString())) + (string.IsNullOrEmpty(courseDisplayModel.Room) ? "" : ", " + courseDisplayModel.Room) + (string.IsNullOrEmpty(courseDisplayModel.Teacher) ? "" : ", " + courseDisplayModel.Teacher)
              });
          }
        }
      }
      List<ReminderModel> reminderModelList1 = result;
      result = (List<ReminderModel>) null;
      courses = (List<CourseDisplayModel>) null;
      return reminderModelList1;
    }

    public static void Clear()
    {
      ScheduleService.CourseDetailDict.Clear();
      ScheduleService.CourseDisplayModelDict.Clear();
    }
  }
}
