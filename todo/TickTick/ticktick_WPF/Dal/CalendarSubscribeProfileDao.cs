// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.CalendarSubscribeProfileDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class CalendarSubscribeProfileDao
  {
    public static async Task SaveProfiles(List<CalendarSubscribeProfileModel> models)
    {
      if (models == null)
        return;
      if (!models.Any<CalendarSubscribeProfileModel>())
      {
        CalendarSubscribeProfileDao.DeleteAll();
      }
      else
      {
        List<CalendarSubscribeProfileModel> subscribeCalendars = CacheManager.GetSubscribeCalendars();
        List<CalendarSubscribeProfileModel> notExisted = new List<CalendarSubscribeProfileModel>();
        foreach (CalendarSubscribeProfileModel model1 in models)
        {
          CalendarSubscribeProfileModel model = model1;
          CalendarSubscribeProfileModel subscribeProfileModel = subscribeCalendars.FirstOrDefault<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (m => m.Id == model.Id));
          if (subscribeProfileModel != null)
          {
            model.Expired = subscribeProfileModel.Expired;
            subscribeCalendars.Remove(subscribeProfileModel);
          }
          else if (notExisted.All<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (m => m.Id != model.Id)))
          {
            notExisted.Add(model);
            CacheManager.UpdateCalendarProfile(model);
          }
        }
        foreach (CalendarSubscribeProfileModel local in subscribeCalendars)
        {
          int num = await App.Connection.DeleteAsync((object) local);
          CacheManager.DeleteCalendarProfile(local);
        }
        int num1 = await App.Connection.InsertAllAsync((IEnumerable) notExisted);
        SubscribeCalendarHelper.ParseUrlCalendars(models);
        notExisted = (List<CalendarSubscribeProfileModel>) null;
      }
    }

    public static async Task<CalendarSubscribeProfileModel> GetProfileById(string calendarId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<CalendarSubscribeProfileModel>().Where((Expression<Func<CalendarSubscribeProfileModel, bool>>) (model => model.UserId == userId && model.Id == calendarId)).FirstOrDefaultAsync();
    }

    public static async Task<List<CalendarSubscribeProfileModel>> GetProfiles(bool onlyVisible = false)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<CalendarSubscribeProfileModel> listAsync = await App.Connection.Table<CalendarSubscribeProfileModel>().Where((Expression<Func<CalendarSubscribeProfileModel, bool>>) (model => model.UserId == userId && model.Id != default (string))).ToListAsync();
      List<CalendarSubscribeProfileModel> source = new List<CalendarSubscribeProfileModel>();
      if (listAsync != null)
      {
        foreach (CalendarSubscribeProfileModel subscribeProfileModel in listAsync)
        {
          CalendarSubscribeProfileModel model = subscribeProfileModel;
          if (source.All<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (m => m.Id != model.Id)))
            source.Add(model);
          else
            App.Connection.DeleteAsync((object) model);
        }
      }
      return !onlyVisible || !source.Any<CalendarSubscribeProfileModel>() ? source : source.Where<CalendarSubscribeProfileModel>((Func<CalendarSubscribeProfileModel, bool>) (model => model.Show != "hidden")).ToList<CalendarSubscribeProfileModel>();
    }

    public static async Task DeleteCalendar(string calendarId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      List<CalendarSubscribeProfileModel> listAsync = await App.Connection.Table<CalendarSubscribeProfileModel>().Where((Expression<Func<CalendarSubscribeProfileModel, bool>>) (model => model.UserId == userId && model.Id == calendarId)).ToListAsync();
      if (listAsync == null || !listAsync.Any<CalendarSubscribeProfileModel>())
        return;
      foreach (CalendarSubscribeProfileModel profile in listAsync)
      {
        int num = await App.Connection.DeleteAsync((object) profile);
        await CalendarEventDao.DeleteByCalendarId(profile.Id);
        CacheManager.DeleteCalendarProfile(profile);
      }
    }

    public static async Task DeleteAll()
    {
      List<CalendarSubscribeProfileModel> profiles = await CalendarSubscribeProfileDao.GetProfiles();
      if (profiles != null && profiles.Any<CalendarSubscribeProfileModel>())
      {
        foreach (object obj in profiles)
        {
          int num = await App.Connection.DeleteAsync(obj);
        }
      }
      CacheManager.ClearCalendarProfile();
    }

    public static async Task AddOrUpdateCalendar(CalendarSubscribeProfileModel profile)
    {
      CalendarSubscribeProfileModel profileById = await CalendarSubscribeProfileDao.GetProfileById(profile.Id);
      if (profileById != null)
      {
        profileById.Color = profile.Color;
        profileById.Name = profile.Name;
        profileById.Url = profile.Url;
        profileById.Show = profile.Show;
        int num = await App.Connection.UpdateAsync((object) profileById);
      }
      else
      {
        int num1 = await App.Connection.InsertAsync((object) profile);
      }
      CacheManager.UpdateCalendarProfile(profile);
    }

    public static async Task UpdateShowStatusAndColor(
      string caldndarId,
      string showStatus,
      string color)
    {
      CalendarSubscribeProfileModel profile = await CalendarSubscribeProfileDao.GetProfileById(caldndarId);
      if (profile == null)
        profile = (CalendarSubscribeProfileModel) null;
      else if (!(profile.Show != showStatus) && !(profile.Color != color))
      {
        profile = (CalendarSubscribeProfileModel) null;
      }
      else
      {
        profile.Show = showStatus;
        profile.Color = color;
        int num = await App.Connection.UpdateAsync((object) profile);
        CacheManager.UpdateCalendarProfile(profile);
        profile = (CalendarSubscribeProfileModel) null;
      }
    }
  }
}
