// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.ScheduleDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class ScheduleDao
  {
    private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public static async Task<List<CourseScheduleModel>> GetAllSchedulesAsync()
    {
      string userId = LocalSettings.Settings.LoginUserId;
      return await App.Connection.Table<CourseScheduleModel>().Where((Expression<Func<CourseScheduleModel, bool>>) (m => m.UserId == userId)).ToListAsync();
    }

    public static async Task InsertScheduleAsync(CourseScheduleModel model)
    {
      if (model == null)
        return;
      if (string.IsNullOrEmpty(model.UserId))
        model.UserId = LocalSettings.Settings.LoginUserId;
      CourseScheduleModel scheduleByIdAsync = await ScheduleDao.GetScheduleByIdAsync(model.Id);
      if (scheduleByIdAsync != null)
      {
        model._Id = scheduleByIdAsync._Id;
        int num = await App.Connection.UpdateAsync((object) model);
      }
      else
      {
        int num1 = await App.Connection.InsertAsync((object) model);
      }
    }

    public static async Task<CourseScheduleModel> GetScheduleByIdAsync(string id)
    {
      string userId = LocalSettings.Settings.LoginUserId;
      return await App.Connection.Table<CourseScheduleModel>().Where((Expression<Func<CourseScheduleModel, bool>>) (m => m.UserId == userId && m.Id == id)).FirstOrDefaultAsync();
    }

    public static async Task UpdateRemoteScheduleAsync(CourseScheduleModel model)
    {
      await ScheduleDao._semaphore.WaitAsync();
      try
      {
        if (model.LessonTimes != null)
          model.LessonTimesStr = JsonConvert.SerializeObject((object) model.LessonTimes);
        if (model.Reminders != null)
          model.RemindersStr = JsonConvert.SerializeObject((object) model.Reminders);
        int num = await App.Connection.UpdateAsync((object) model);
        await ScheduleDao.SaveCoursesAsync(model);
      }
      finally
      {
        ScheduleDao._semaphore.Release();
      }
    }

    public static async Task AddRemoteScheduleAsync(CourseScheduleModel remote)
    {
      if (remote == null)
        return;
      await ScheduleDao._semaphore.WaitAsync();
      try
      {
        if (remote.LessonTimes != null)
          remote.LessonTimesStr = JsonConvert.SerializeObject((object) remote.LessonTimes);
        if (remote.Reminders != null)
          remote.RemindersStr = JsonConvert.SerializeObject((object) remote.Reminders);
        await ScheduleDao.InsertScheduleAsync(remote);
        await ScheduleDao.SaveCoursesAsync(remote);
      }
      finally
      {
        ScheduleDao._semaphore.Release();
      }
    }

    public static async Task DeleteScheduleAsync(CourseScheduleModel model)
    {
      if (model == null)
        return;
      int num = await App.Connection.DeleteAsync((object) model);
      await ScheduleDao.DeleteCoursedByScheduleIdAsync(model.Id);
    }

    private static async Task SaveCoursesAsync(CourseScheduleModel schedule)
    {
      if (schedule == null)
        return;
      await ScheduleDao.DeleteCoursedByScheduleIdAsync(schedule.Id);
      if (schedule.Courses == null || !schedule.Courses.Any<CourseModel>())
        return;
      foreach (CourseModel course in schedule.Courses)
      {
        course.ScheduleId = schedule.Id;
        if (course.Items != null)
          course.ItemsStr = JsonConvert.SerializeObject((object) course.Items);
      }
      int num = await App.Connection.InsertAllAsync((IEnumerable) schedule.Courses);
    }

    public static async Task<List<CourseModel>> GetCoursedByScheduleIdAsync(string scheduleId)
    {
      return await App.Connection.Table<CourseModel>().Where((Expression<Func<CourseModel, bool>>) (m => m.ScheduleId == scheduleId)).ToListAsync();
    }

    public static async Task<CourseModel> GetCoursedByIdAsync(string courseId)
    {
      return await App.Connection.Table<CourseModel>().Where((Expression<Func<CourseModel, bool>>) (m => m.Id == courseId)).FirstOrDefaultAsync();
    }

    private static async Task DeleteCoursedByScheduleIdAsync(string scheduleId)
    {
      List<CourseModel> byScheduleIdAsync = await ScheduleDao.GetCoursedByScheduleIdAsync(scheduleId);
      if (byScheduleIdAsync == null)
        return;
      foreach (object obj in byScheduleIdAsync)
      {
        int num = await App.Connection.DeleteAsync(obj);
      }
    }
  }
}
