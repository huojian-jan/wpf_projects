// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.UserDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class UserDao
  {
    public static async Task AdjustCheckPointAfterLogout(string userId)
    {
      UserModel userModel = await UserDao.QueryUserModelListDbByIdAsync(userId);
      if (userModel == null || userModel.checkPoint <= 0L)
        return;
      userModel.checkPoint -= 86400000L;
      int num = await App.Connection.UpdateAsync((object) userModel);
    }

    public static async Task SaverUserSyncCheckPoint(string userId, long checkpoint)
    {
      UserModel userModel = await UserDao.QueryUserModelListDbByIdAsync(userId);
      if (userModel == null)
        return;
      userModel.checkPoint = checkpoint;
      int num = await App.Connection.UpdateAsync((object) userModel);
    }

    public static async Task SaveColumnCheckPoint(string userId, long checkPoint)
    {
      UserModel userModel = await UserDao.QueryUserModelListDbByIdAsync(userId);
      if (userModel == null)
        return;
      userModel.columnCheckPoint = checkPoint;
      int num = await App.Connection.UpdateAsync((object) userModel);
    }

    public static async Task<long> GetColumnCheckPoint(string userId)
    {
      UserModel userModel = await UserDao.QueryUserModelListDbByIdAsync(userId);
      return userModel != null ? userModel.columnCheckPoint : 0L;
    }

    public static async Task<long> GetUserSyncCheckPoint(string userId)
    {
      UserModel userModel = await UserDao.QueryUserModelListDbByIdAsync(userId);
      return userModel != null ? userModel.checkPoint : 0L;
    }

    public static async Task UpdateOrInsertUserModelListDbAsync(UserModel user)
    {
      int num1 = await Task.Run<int>((Func<Task<int>>) (async () =>
      {
        try
        {
          List<UserModel> listAsync = await App.Connection.Table<UserModel>().Where((Expression<Func<UserModel, bool>>) (v => v.userId.Equals(user.userId))).ToListAsync();
          if (listAsync.Count != 0)
          {
            user.checkPoint = listAsync[0].checkPoint;
            user.columnCheckPoint = listAsync[0].columnCheckPoint;
            return await App.Connection.UpdateAsync((object) user);
          }
          int num2 = await App.Connection.InsertAsync((object) user);
          return 1;
        }
        catch (Exception ex)
        {
          return -1;
        }
      }));
    }

    public static async Task UpdateOrInsertUserInfoModelListDbAsync(UserInfoModel user)
    {
      int num1 = await Task.Run<int>((Func<Task<int>>) (async () =>
      {
        try
        {
          if ((await App.Connection.Table<UserInfoModel>().Where((Expression<Func<UserInfoModel, bool>>) (v => v.userCode.Equals(user.userCode))).ToListAsync()).Count != 0)
            return await App.Connection.UpdateAsync((object) user);
          int num2 = await App.Connection.InsertAsync((object) user);
          return 1;
        }
        catch (Exception ex)
        {
          return -1;
        }
      }));
    }

    public static async Task<UserModel> QueryUserModelListDbByIdAsync(string userId)
    {
      return string.IsNullOrEmpty(userId) ? (UserModel) null : await Task.Run<UserModel>((Func<Task<UserModel>>) (async () =>
      {
        List<UserModel> listAsync = await App.Connection.Table<UserModel>().Where((Expression<Func<UserModel, bool>>) (v => v.userId.Equals(userId))).ToListAsync();
        return listAsync.Count != 0 ? listAsync[0] : (UserModel) null;
      }));
    }

    public static async Task<UserModel> GetUserModelByName(string userName)
    {
      return await Task.Run<UserModel>((Func<Task<UserModel>>) (async () =>
      {
        List<UserModel> listAsync = await App.Connection.Table<UserModel>().Where((Expression<Func<UserModel, bool>>) (v => v.username.Equals(userName))).ToListAsync();
        return listAsync.Count != 0 ? listAsync[0] : (UserModel) null;
      }));
    }

    public static async Task<UserInfoModel> GetUserByName(string username)
    {
      return string.IsNullOrEmpty(username) ? (UserInfoModel) null : await Task.Run<UserInfoModel>((Func<Task<UserInfoModel>>) (async () =>
      {
        List<UserInfoModel> listAsync = await App.Connection.Table<UserInfoModel>().Where((Expression<Func<UserInfoModel, bool>>) (v => v.username.Equals(username))).ToListAsync();
        return listAsync.Count != 0 ? listAsync[0] : (UserInfoModel) null;
      }));
    }

    public static async Task<string> GetDisplayName(UserInfoModel userInfo)
    {
      string name = string.IsNullOrEmpty(userInfo.name) ? userInfo.username : userInfo.name;
      if (string.IsNullOrEmpty(name))
      {
        UserModel userModel = await UserDao.QueryUserModelListDbByIdAsync(userInfo.username);
        if (userModel != null)
          name = userModel.phone;
      }
      string displayName = name;
      name = (string) null;
      return displayName;
    }

    public static bool IsPro() => LocalSettings.Settings.IsPro;

    public static bool IsPro2() => LocalSettings.Settings.IsPro;

    public static bool IsUserValid() => UserDao.IsPro();
  }
}
