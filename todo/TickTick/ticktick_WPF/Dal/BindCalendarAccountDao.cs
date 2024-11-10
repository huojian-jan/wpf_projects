// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.BindCalendarAccountDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Config;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class BindCalendarAccountDao
  {
    public static async Task SaveBindCalendarAccounts(List<BindCalendarAccountModel> models)
    {
      List<BindCalendarAccountModel> list = CacheManager.GetAccountCalDict().Values.ToList<BindCalendarAccountModel>();
      int count = list.Count;
      foreach (BindCalendarAccountModel model in models)
      {
        BindCalendarAccountModel account = model;
        account?.Calendars?.ForEach((Action<BindCalendarModel>) (cal =>
        {
          if (cal == null || cal.Color == null)
            return;
          cal.Color = Utils.RgbaToArgb(cal.Color);
        }));
        BindCalendarAccountDao.SaveBindCalendarAccount(account, updateExpired: false);
        list.RemoveAll((Predicate<BindCalendarAccountModel>) (a => a.Id == account?.Id));
      }
      string logString = string.Format("MergeBindAccount result remote:local {0}:{1}", (object) models.Count, (object) count);
      if (list.Any<BindCalendarAccountModel>())
      {
        logString += " DeleteIds";
        foreach (BindCalendarAccountModel account in list)
        {
          await BindCalendarAccountDao.DeleteBindAccount(account);
          logString = logString + " " + account.Id;
        }
      }
      UtilLog.Info(logString);
      logString = (string) null;
    }

    public static async Task SaveBindCalendarAccount(
      BindCalendarAccountModel model,
      bool handleCalendar = true,
      bool updateExpired = true)
    {
      if (model == null)
        return;
      string userId = Utils.GetCurrentUserIdInt().ToString();
      if (string.IsNullOrEmpty(model.UserId))
        model.UserId = userId;
      BindCalendarAccountModel account = await BindCalendarAccountDao.GetBindCalendarAccount(model.Id);
      if (account != null)
      {
        await BindCalendarAccountDao.DeleteBindAccount(account, false);
        if (!updateExpired)
          model.Expired = account.Expired;
      }
      if (model.Expired && (account == null || !account.Expired))
        BindCalendarAccountDao.ShowAccountExpiredWindow(model);
      int num1 = await App.Connection.InsertAsync((object) model);
      CacheManager.UpdateBindAccount(model);
      if (!handleCalendar)
        return;
      List<BindCalendarModel> notExisted = new List<BindCalendarModel>();
      List<BindCalendarModel> updated = new List<BindCalendarModel>();
      List<BindCalendarModel> localCalendars = account?.Calendars ?? new List<BindCalendarModel>();
      if (model.Calendars == null)
        return;
      foreach (BindCalendarModel calendar in model.Calendars)
      {
        BindCalendarModel cal = calendar;
        List<BindCalendarModel> localCals = localCalendars.Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (local => local.Id == cal.Id)).ToList<BindCalendarModel>();
        if (localCals.Count > 0)
        {
          for (int i = localCals.Count - 1; i >= 0; --i)
          {
            BindCalendarModel model1 = localCals[i];
            localCalendars.Remove(model1);
            if (i == localCals.Count - 1)
            {
              if (model1.Color != cal.Color || model1.LocalCurrentUserPrivilegeSet != cal.LocalCurrentUserPrivilegeSet || model1.AccessRole != cal.AccessRole)
              {
                model1.Color = cal.Color;
                model1.CurrentUserPrivilegeSet = cal.CurrentUserPrivilegeSet;
                model1.AccessRole = cal.AccessRole;
                updated.Add(model1);
                CacheManager.UpdateBindCalendar(model1);
              }
            }
            else
            {
              int num2 = await App.Connection.DeleteAsync((object) model1);
            }
          }
        }
        else
        {
          cal.AccountId = model.Id;
          cal.UserId = userId;
          notExisted.Add(cal);
          CacheManager.UpdateBindCalendar(cal);
        }
        localCals = (List<BindCalendarModel>) null;
      }
      if (localCalendars.Any<BindCalendarModel>())
      {
        foreach (BindCalendarModel model2 in localCalendars)
        {
          CacheManager.DeleteBindCalendar(model2);
          int num3 = await App.Connection.DeleteAsync((object) model2);
        }
      }
      if (updated.Any<BindCalendarModel>())
      {
        int num4 = await App.Connection.UpdateAllAsync((IEnumerable) updated);
      }
      if (notExisted.Any<BindCalendarModel>())
      {
        int num5 = await App.Connection.InsertAllAsync((IEnumerable) notExisted);
      }
      userId = (string) null;
      account = (BindCalendarAccountModel) null;
      notExisted = (List<BindCalendarModel>) null;
      updated = (List<BindCalendarModel>) null;
      localCalendars = (List<BindCalendarModel>) null;
    }

    private static void ShowAccountExpiredWindow(BindCalendarAccountModel model)
    {
      LocalSettings.Settings.ExtraSettings.BindCalendarExpireCheckTime = (long) DateUtils.GetDateNum(DateTime.Today);
      LocalSettings.Settings.Save(true);
      Application.Current.Dispatcher.Invoke((Action) (() => SubscribeExpiredWindow.TryShowWindow(new List<BindCalendarAccountModel>()
      {
        model
      })));
    }

    public static async Task DeleteBindAccountById(string accountId)
    {
      List<BindCalendarAccountModel> calendarAccountById = await BindCalendarAccountDao.GetBindCalendarAccountById(accountId);
      if (calendarAccountById == null || !calendarAccountById.Any<BindCalendarAccountModel>())
        return;
      foreach (BindCalendarAccountModel account in calendarAccountById)
        await BindCalendarAccountDao.DeleteBindAccount(account);
    }

    private static async Task<List<BindCalendarAccountModel>> GetBindCalendarAccountById(
      string accountId)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<BindCalendarAccountModel>().Where((Expression<Func<BindCalendarAccountModel, bool>>) (model => model.UserId == userId && model.Id == accountId)).ToListAsync();
    }

    public static async Task DeleteBindAccounts()
    {
      List<BindCalendarAccountModel> calendarAccounts = await BindCalendarAccountDao.GetBindCalendarAccounts();
      if (calendarAccounts != null && calendarAccounts.Any<BindCalendarAccountModel>())
      {
        foreach (BindCalendarAccountModel account in calendarAccounts)
          await BindCalendarAccountDao.DeleteBindAccount(account);
      }
      await CalendarEventDao.DeleteBindEvents();
    }

    public static async Task HandleAccountExpired(BindCalendarAccountModel account)
    {
      if (account == null)
        return;
      account.Expired = true;
      await BindCalendarAccountDao.SaveBindCalendarAccount(account);
      List<BindCalendarModel> calendarsByAccountId = await BindCalendarAccountDao.GetBindCalendarsByAccountId(account.Id);
      if (calendarsByAccountId == null || !calendarsByAccountId.Any<BindCalendarModel>())
        return;
      foreach (BindCalendarModel bindCalendarModel in calendarsByAccountId)
        await CalendarEventDao.DeleteByCalendarId(bindCalendarModel.Id);
    }

    private static async Task DeleteBindAccount(
      BindCalendarAccountModel account,
      bool deleteCalendars = true)
    {
      if (account == null)
        return;
      int num = await App.Connection.DeleteAsync((object) account);
      CacheManager.DeleteBindAccount(account);
      if (!deleteCalendars)
        return;
      await BindCalendarAccountDao.DeleteCalendarsByAccountId(account.Id);
      DataChangedNotifier.NotifyCalendarChanged();
    }

    public static async Task DeleteCalendarsByAccountId(string accountId)
    {
      List<BindCalendarModel> calendarsByAccountId = await BindCalendarAccountDao.GetBindCalendarsByAccountId(accountId);
      if (calendarsByAccountId == null || !calendarsByAccountId.Any<BindCalendarModel>())
        return;
      foreach (BindCalendarModel calendar in calendarsByAccountId)
      {
        CacheManager.DeleteBindCalendar(calendar);
        int num = await App.Connection.DeleteAsync((object) calendar);
        await CalendarEventDao.DeleteByCalendarId(calendar.Id);
      }
    }

    public static async Task<List<BindCalendarAccountModel>> GetBindCalendarAccounts()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<BindCalendarAccountModel>().Where((Expression<Func<BindCalendarAccountModel, bool>>) (model => model.UserId == userId)).ToListAsync();
    }

    public static async Task<BindCalendarAccountModel> GetBindCalendarAccount(string id)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      BindCalendarAccountModel model = await App.Connection.Table<BindCalendarAccountModel>().Where((Expression<Func<BindCalendarAccountModel, bool>>) (account => account.Id == id && account.UserId == userId)).FirstOrDefaultAsync();
      if (model != null)
      {
        List<BindCalendarModel> calendarsByAccountId = await BindCalendarAccountDao.GetBindCalendarsByAccountId(model.Id);
        if (calendarsByAccountId != null && calendarsByAccountId.Any<BindCalendarModel>())
          model.Calendars = calendarsByAccountId;
      }
      BindCalendarAccountModel bindCalendarAccount = model;
      model = (BindCalendarAccountModel) null;
      return bindCalendarAccount;
    }

    private static async Task<List<BindCalendarModel>> GetBindCalendarsByAccountId(string id)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<BindCalendarModel>().Where((Expression<Func<BindCalendarModel, bool>>) (model => model.UserId == userId && model.AccountId == id)).ToListAsync();
    }

    public static async Task<List<BindCalendarModel>> GetBindCalendars()
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      return await App.Connection.Table<BindCalendarModel>().Where((Expression<Func<BindCalendarModel, bool>>) (model => model.UserId == userId)).ToListAsync();
    }

    public static async Task SaveCalendarShowStatus(string calendarId, string status)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      BindCalendarModel model1 = await App.Connection.Table<BindCalendarModel>().Where((Expression<Func<BindCalendarModel, bool>>) (model => model.UserId == userId && model.Id == calendarId)).FirstOrDefaultAsync();
      if (model1 == null)
        return;
      model1.Show = status;
      CacheManager.UpdateBindCalendar(model1);
      int num = await App.Connection.UpdateAsync((object) model1);
    }

    public static async Task SaveCalendarInfo(BindCalendarModel calendar)
    {
      string argb = Utils.RgbaToArgb(calendar.Color);
      BindCalendarModel model1 = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (model => model.Id == calendar.Id));
      if (model1 == null)
      {
        calendar.UserId = LocalSettings.Settings.LoginUserId;
        calendar.Show = "show";
        CacheManager.UpdateBindCalendar(calendar);
        int num = await App.Connection.InsertAsync((object) calendar);
      }
      else
      {
        if (string.IsNullOrEmpty(calendar.Name) || string.IsNullOrEmpty(argb))
          return;
        model1.Name = calendar.Name;
        model1.Color = argb;
        CacheManager.UpdateBindCalendar(model1);
        int num = await App.Connection.UpdateAsync((object) model1);
      }
    }
  }
}
