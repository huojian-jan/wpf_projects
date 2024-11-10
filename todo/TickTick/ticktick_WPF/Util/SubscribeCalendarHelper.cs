// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.SubscribeCalendarHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Views.Config;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class SubscribeCalendarHelper
  {
    public const int SubscribeCal = 6;
    public const int GoogleCal = 1;
    public const int OutlookCal = 2;
    public const int Exchange = 3;
    public const int ICloud = 4;
    public const int CalDavCal = 5;
    private static readonly Dictionary<string, DateTime> LoadUrlTimeDict = new Dictionary<string, DateTime>();
    private static bool _isParsingCalendar;

    public static async Task UnsubscribeCalendar(string calendarId)
    {
      if (!Utils.IsNetworkAvailable())
      {
        Utils.Toast(Utils.GetString("NoNetwork"));
      }
      else
      {
        if (string.IsNullOrEmpty(calendarId))
          return;
        await Communicator.UnsubscribeCalendar(calendarId);
        await CalendarSubscribeProfileDao.DeleteCalendar(calendarId);
      }
    }

    public static async Task UnbindCalendar(string accountId)
    {
      if (!Utils.IsNetworkAvailable())
      {
        Utils.Toast(Utils.GetString("NoNetwork"));
      }
      else
      {
        if (string.IsNullOrEmpty(accountId))
          return;
        await Communicator.UnbindCalendar(accountId);
        await BindCalendarAccountDao.DeleteBindAccountById(accountId);
        ticktick_WPF.Notifier.GlobalEventManager.NotifyReloadCalendar();
      }
    }

    public static async Task<CalendarSubscribeProfileModel> SubscribeCalendar(
      string remotePath,
      string color = null,
      string show = "show")
    {
      CalendarSubscribeProfileModel profile = await Communicator.SubscribeCalendar(new CalendarSubscribeProfileModel()
      {
        Name = "Calendar",
        Url = remotePath,
        Show = show,
        CreatedTime = new DateTime?(DateTime.Now),
        Color = color ?? ThemeUtil.GetRandomColor()
      });
      if (profile == null || profile.errorId != null)
        return (CalendarSubscribeProfileModel) null;
      profile.UserId = Utils.GetCurrentUserIdInt().ToString();
      await CalendarSubscribeProfileDao.AddOrUpdateCalendar(profile);
      return profile;
    }

    public static async Task ParseUrlCalendar(string calendarId, string url)
    {
      if (SubscribeCalendarHelper._isParsingCalendar)
        return;
      SubscribeCalendarHelper._isParsingCalendar = true;
      try
      {
        await CalendarService.PullSubscribeEventsByCalIdAsync(calendarId);
        SubscribeCalendarHelper.HandleCalendarUnExpired(calendarId);
      }
      catch (CustomException.CalendarExpiredException ex)
      {
        SubscribeCalendarHelper.HandleCalendarExpired(calendarId);
      }
      SubscribeCalendarHelper._isParsingCalendar = false;
    }

    private static async Task HandleCalendarUnExpired(string calendarId)
    {
      CalendarSubscribeProfileModel calendar = CacheManager.GetSubscribeCalById(calendarId);
      if (calendar == null)
        calendar = (CalendarSubscribeProfileModel) null;
      else if (!calendar.Expired)
      {
        calendar = (CalendarSubscribeProfileModel) null;
      }
      else
      {
        calendar.Expired = false;
        int num = await App.Connection.UpdateAsync((object) calendar);
        CacheManager.UpdateCalendarProfile(calendar);
        calendar = (CalendarSubscribeProfileModel) null;
      }
    }

    private static async Task HandleCalendarExpired(string calendarId)
    {
      CalendarSubscribeProfileModel calendar = CacheManager.GetSubscribeCalById(calendarId);
      if (calendar == null)
        ;
      else if (calendar.Expired)
        ;
      else
      {
        calendar.Expired = true;
        List<CalendarEventModel> listAsync = await App.Connection.Table<CalendarEventModel>().Where((Expression<Func<CalendarEventModel, bool>>) (model => model.UserId == LocalSettings.Settings.LoginUserId && model.CalendarId == calendarId)).ToListAsync();
        if (listAsync.Any<CalendarEventModel>())
        {
          foreach (object obj in listAsync)
          {
            int num = await App.Connection.DeleteAsync(obj);
          }
        }
        int num1 = await App.Connection.UpdateAsync((object) calendar);
        CacheManager.UpdateCalendarProfile(calendar);
        LocalSettings.Settings.ExtraSettings.BindCalendarExpireCheckTime = (long) DateUtils.GetDateNum(DateTime.Today);
        LocalSettings.Settings.Save(true);
        Application.Current.Dispatcher.Invoke((Action) (() => SubscribeExpiredWindow.TryShowWindow((List<BindCalendarAccountModel>) null, new List<CalendarSubscribeProfileModel>()
        {
          calendar
        })));
      }
    }

    private static string UrlToPath(string url)
    {
      string str = Utils.Base64Encode(url);
      return str.Length > 10 ? str.Substring(0, 10) : str;
    }

    public static async Task<CalendarCollection> TryParseCalendar(string url)
    {
      if (!Directory.Exists(AppPaths.CalendarDir))
        Directory.CreateDirectory(AppPaths.CalendarDir);
      string path = SubscribeCalendarHelper.UrlToPath(url);
      string localPath = AppPaths.CalendarDir + path + ".ics";
      string str = AppPaths.CalendarDir + path + "_bak.ics";
      if (System.IO.File.Exists(localPath))
      {
        if (System.IO.File.Exists(str))
          System.IO.File.Delete(str);
        try
        {
          System.IO.File.Move(localPath, str);
        }
        catch (Exception ex)
        {
        }
      }
      string remotePath = url;
      if (remotePath.StartsWith("webcal"))
      {
        remotePath = remotePath.Substring(6);
        remotePath = "https" + remotePath;
      }
      if (remotePath.StartsWith("http") && !remotePath.StartsWith("https"))
        remotePath = remotePath.Replace("http", "https");
      if (!remotePath.StartsWith("https") && !remotePath.StartsWith("http"))
        remotePath = "https://" + remotePath;
      if (remotePath.Contains("mymba.sjtu.edu.cn"))
        remotePath = remotePath.Replace("https", "http");
      if (Utils.IsNetworkAvailable())
      {
        WebClient downloader = new WebClient();
        try
        {
          await downloader.DownloadFileTaskAsync(remotePath, localPath);
        }
        catch (Exception ex1)
        {
          if (remotePath.StartsWith("https"))
          {
            remotePath = remotePath.Replace("https", "http");
            try
            {
              await downloader.DownloadFileTaskAsync(remotePath, localPath);
            }
            catch (Exception ex2)
            {
            }
          }
        }
        finally
        {
          downloader?.Dispose();
        }
        if (System.IO.File.Exists(localPath))
        {
          try
          {
            CalendarCollection calendar = CalendarCollection.Load(localPath);
            if (calendar != null)
              return calendar;
          }
          catch (Exception ex)
          {
            return (CalendarCollection) null;
          }
        }
        downloader = (WebClient) null;
      }
      return (CalendarCollection) null;
    }

    public static async Task SaveBindAccount(BindCalendarAccountModel account)
    {
      await BindCalendarAccountDao.SaveBindCalendarAccount(account);
    }

    public static async Task<BindCalendarAccountModel> BindCalendarAccount(string code)
    {
      BindCalendarAccountModel calendarAccountModel = await Communicator.BindCalendarAccount(code, "google", Utils.IsDida() ? "dida365.com" : "ticktick.com");
      if (calendarAccountModel == null || string.IsNullOrEmpty(calendarAccountModel.Account))
        return (BindCalendarAccountModel) null;
      calendarAccountModel.UserId = Utils.GetCurrentUserIdInt().ToString();
      return calendarAccountModel;
    }

    public static string GetCalendarName(string calendarId)
    {
      if (string.IsNullOrEmpty(calendarId))
        return string.Empty;
      BindCalendarModel bindCalendarModel = CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == calendarId));
      return bindCalendarModel == null ? string.Empty : bindCalendarModel.Name;
    }

    public static BindCalendarModel GetDefaultCalendar(string accountId)
    {
      List<BindCalendarModel> list = CacheManager.GetBindCalendars().Where<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.AccountId == accountId && cal.Accessible)).ToList<BindCalendarModel>();
      return list.Any<BindCalendarModel>() ? list.FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Show != "hidden")) ?? list.First<BindCalendarModel>() : (BindCalendarModel) null;
    }

    public static async void ParseUrlCalendars(List<CalendarSubscribeProfileModel> models)
    {
      foreach (CalendarSubscribeProfileModel model in models)
        await SubscribeCalendarHelper.ParseUrlCalendar(model.Id, model.Url);
      CalendarEventChangeNotifier.NotifyRemoteChanged();
    }

    public static int GetCalendarType(string calendarId)
    {
      return string.IsNullOrEmpty(calendarId) ? 6 : SubscribeCalendarHelper.GetAccountTypeById(CacheManager.GetBindCalendars().FirstOrDefault<BindCalendarModel>((Func<BindCalendarModel, bool>) (cal => cal.Id == calendarId))?.AccountId);
    }

    private static int GetAccountTypeById(string accountId)
    {
      return string.IsNullOrEmpty(accountId) ? 6 : SubscribeCalendarHelper.GetAccountType(CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (ac => ac.Id == accountId)));
    }

    public static int GetAccountType(BindCalendarAccountModel account)
    {
      if (account == null)
        return 6;
      switch (account.Kind)
      {
        case "icloud":
          return 4;
        case "caldav":
          return 5;
        case "exchange":
          return 3;
        default:
          switch (account.Site)
          {
            case "outlook":
              return 2;
            case "google":
              return 1;
            default:
              return 6;
          }
      }
    }

    public static Geometry GetCalendarProjectIconById(string accountId)
    {
      return SubscribeCalendarHelper.GetCalendarProjectIcon(string.IsNullOrEmpty(accountId) ? (BindCalendarAccountModel) null : CacheManager.GetBindCalendarAccounts().FirstOrDefault<BindCalendarAccountModel>((Func<BindCalendarAccountModel, bool>) (ac => ac.Id == accountId)));
    }

    public static Geometry GetCalendarProjectIcon(BindCalendarAccountModel account)
    {
      Geometry icon;
      switch (account?.Kind)
      {
        case "caldav":
          icon = Utils.GetIcon("IcCalDavProject");
          break;
        case "exchange":
          icon = Utils.GetIcon("IcExchangeProject");
          break;
        case "icloud":
          icon = Utils.GetIcon("IcICloudProject");
          break;
        default:
          switch (account?.Site)
          {
            case "outlook":
              icon = Utils.GetIcon("IcOutlookProject");
              break;
            case "google":
              icon = Utils.GetIcon("IcGoogleCalendarProject");
              break;
            default:
              icon = Utils.GetIcon("IcSubscribeCalendar");
              break;
          }
          break;
      }
      return icon;
    }
  }
}
