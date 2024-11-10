// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.UserManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Util
{
  public class UserManager
  {
    private static UserModel _user;
    private static UserInfoModel _userInfo;
    public static DateTime LastInitTime;

    public static bool IsTeamUser() => UserManager._user != null && UserManager._user.teamUser;

    public static bool IsFreeUser()
    {
      UserModel user = UserManager._user;
      if ((user != null ? (!user.proEndDate.HasValue ? 1 : 0) : 1) != 0)
        return true;
      DateTime? date = UserManager._user?.proEndDate.Value.Date;
      DateTime dateTime = DateTime.Now.AddYears(-20);
      return date.HasValue && date.GetValueOrDefault() < dateTime;
    }

    public static async Task<int> GetProExpireDays()
    {
      List<string> stringList = new List<string>()
      {
        "order",
        "paypal_subscribe",
        "apple",
        "google",
        "stripe_subscribe"
      };
      UserModel user = UserManager._user;
      return (user != null ? (user.proEndDate.HasValue ? 1 : 0) : 0) == 0 ? -1 : (!stringList.Contains(UserManager._user.subscribeType) ? (int) (UserManager._user.proEndDate.Value - DateTime.Today).TotalDays : 30);
    }

    public static bool IsTeamActive()
    {
      if (UserManager._user == null)
        return false;
      return UserManager._user.activeTeamUser || UserManager._user.teamPro;
    }

    public static async Task<UserModel> GetCurrentUser(bool force = false)
    {
      await UserManager.Init();
      return UserManager._user;
    }

    public static async Task Init(bool force = false)
    {
      if (UserManager._user == null && !string.IsNullOrEmpty(LocalSettings.Settings.LoginUserName))
        UserManager._user = await UserDao.QueryUserModelListDbByIdAsync(LocalSettings.Settings.LoginUserId);
      if (force || Utils.IsNetworkAvailable() && (DateTime.Now - UserManager.LastInitTime).TotalMinutes > 1.0)
      {
        UserModel userStatus = await Communicator.GetUserStatus();
        if (!string.IsNullOrEmpty(userStatus.userId))
        {
          UserManager.LastInitTime = DateTime.Now;
          await UserDao.UpdateOrInsertUserModelListDbAsync(userStatus);
        }
        UserManager._user = await UserDao.QueryUserModelListDbByIdAsync(LocalSettings.Settings.LoginUserId);
      }
      else
        UserManager._user = await UserDao.QueryUserModelListDbByIdAsync(LocalSettings.Settings.LoginUserId);
    }

    public static bool ProExpired()
    {
      UserModel user = UserManager._user;
      if ((user != null ? (user.proEndDate.HasValue ? 1 : 0) : 0) != 0)
      {
        DateTime? date = UserManager._user?.proEndDate.Value;
        DateTime now = DateTime.Now;
        if ((date.HasValue ? (date.GetValueOrDefault() < now ? 1 : 0) : 0) != 0)
        {
          date = UserManager._user?.proEndDate.Value.Date;
          DateTime dateTime = DateTime.Now.AddYears(-20);
          return date.HasValue && date.GetValueOrDefault() > dateTime;
        }
      }
      return false;
    }

    public static string GetToken() => UserManager._user?.token;

    public static string GetUserEmail() => UserManager._user?.username;

    public static async Task PullUserInfo()
    {
      UserInfoModel userInfo = await Communicator.GetUserInfo();
      if (userInfo == null)
        return;
      UserManager._userInfo = userInfo;
      if (LocalSettings.Settings.LoginUserName != UserManager._userInfo.username)
        LocalSettings.Settings.LoginUserName = UserManager._userInfo.username;
      await UserDao.UpdateOrInsertUserInfoModelListDbAsync(userInfo);
    }

    public static async Task<string> GetUserCode()
    {
      if (string.IsNullOrEmpty(LocalSettings.Settings.LoginUserId))
        return (string) null;
      if (UserManager._userInfo == null || UserManager._userInfo.username != LocalSettings.Settings.LoginUserName)
        UserManager._userInfo = await UserDao.GetUserByName(LocalSettings.Settings.LoginUserName);
      return UserManager._userInfo?.userCode;
    }

    public static void Clear()
    {
      UserManager._user = (UserModel) null;
      UserManager._userInfo = (UserInfoModel) null;
    }

    public static async Task<UserInfoModel> GetUserInfo(bool force = false)
    {
      if (UserManager._userInfo == null || UserManager._userInfo.username != LocalSettings.Settings.LoginUserName)
      {
        UserManager._userInfo = await UserDao.GetUserByName(LocalSettings.Settings.LoginUserName);
        if (UserManager._userInfo == null & force)
          await UserManager.PullUserInfo();
      }
      return UserManager._userInfo;
    }

    public static async Task CheckUserP(bool force)
    {
      UserModel currentUser = await UserManager.GetCurrentUser(force);
      if (currentUser == null)
        return;
      if (currentUser.pro)
      {
        DateTime? proEndDate = currentUser.proEndDate;
        DateTime now = DateTime.Now;
        if ((proEndDate.HasValue ? (proEndDate.GetValueOrDefault() > now ? 1 : 0) : 0) != 0)
        {
          LocalSettings.Settings.IsPro = true;
          LocalSettings.Settings.DontShowProWindow = false;
          return;
        }
      }
      LocalSettings.Settings.IsPro = CacheManager.GetTeams().Any<TeamModel>((Func<TeamModel, bool>) (t => t.IsPro()));
    }
  }
}
