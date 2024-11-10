// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.UserPublicProfilesDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class UserPublicProfilesDao
  {
    public static async Task<UserPublicProfilesModel> GetUserInfoByUserCode(string userCode)
    {
      UserPublicProfilesModel userInfoByUserCode = await UserPublicProfilesDao.GetUserInfoByUsercode(userCode);
      if (userInfoByUserCode == null)
        userInfoByUserCode = await UserPublicProfilesDao.UpdateUser(userCode);
      return userInfoByUserCode;
    }

    private static async Task<UserPublicProfilesModel> UpdateUser(string userCode)
    {
      UserPublicProfilesModel userPublicProfiles = await Communicator.GetUserPublicProfiles(userCode);
      if (userPublicProfiles != null)
        UserPublicProfilesDao.UpdateOrInsertUser(userPublicProfiles);
      return userPublicProfiles;
    }

    public static async Task<List<UserPublicProfilesModel>> GetUsersInfoByUserCodes(
      List<string> userCodes)
    {
      return await UserPublicProfilesDao.UpdateUsers(userCodes);
    }

    private static async Task<List<UserPublicProfilesModel>> UpdateUsers(List<string> userCodes)
    {
      List<UserPublicProfilesModel> usersPublicProfiles = await Communicator.GetUsersPublicProfiles(userCodes);
      if (usersPublicProfiles != null)
        UserPublicProfilesDao.UpdateOrInsertUsers(usersPublicProfiles);
      return usersPublicProfiles;
    }

    private static async Task<UserPublicProfilesModel> GetUserInfoByUsercode(string userCode)
    {
      return await Task.Run<UserPublicProfilesModel>((Func<Task<UserPublicProfilesModel>>) (async () =>
      {
        List<UserPublicProfilesModel> listAsync = await App.Connection.Table<UserPublicProfilesModel>().Where((Expression<Func<UserPublicProfilesModel, bool>>) (v => v.userCode == userCode)).ToListAsync();
        return listAsync.Count == 0 ? (UserPublicProfilesModel) null : listAsync[0];
      }));
    }

    public static async Task<UserPublicProfilesModel> GetUserInfoById(long userId)
    {
      return await Task.Run<UserPublicProfilesModel>((Func<Task<UserPublicProfilesModel>>) (async () =>
      {
        List<UserPublicProfilesModel> listAsync = await App.Connection.Table<UserPublicProfilesModel>().Where((Expression<Func<UserPublicProfilesModel, bool>>) (v => v.userId == (long?) userId)).ToListAsync();
        return listAsync.Count == 0 ? (UserPublicProfilesModel) null : listAsync[0];
      }));
    }

    public static async Task UpdateOrInsertUser(UserPublicProfilesModel user)
    {
      try
      {
        List<UserPublicProfilesModel> listAsync = await App.Connection.Table<UserPublicProfilesModel>().Where((Expression<Func<UserPublicProfilesModel, bool>>) (v => v.userCode.Equals(user.userCode))).ToListAsync();
        if (listAsync.Count != 0)
        {
          user._Id = listAsync[0]._Id;
          user.userId = listAsync[0].userId.HasValue ? listAsync[0].userId : user.userId;
          int num = await App.Connection.UpdateAsync((object) user);
        }
        else
        {
          int num1 = await App.Connection.InsertAsync((object) user);
        }
      }
      catch (Exception ex)
      {
      }
    }

    public static async Task InsertUser(UserPublicProfilesModel user)
    {
      try
      {
        if ((await App.Connection.Table<UserPublicProfilesModel>().Where((Expression<Func<UserPublicProfilesModel, bool>>) (v => v.userCode.Equals(user.userCode))).ToListAsync()).Count != 0)
          ;
        else
        {
          int num = await App.Connection.InsertAsync((object) user);
        }
      }
      catch (Exception ex)
      {
      }
    }

    private static async Task UpdateOrInsertUsers(List<UserPublicProfilesModel> users)
    {
      foreach (UserPublicProfilesModel user in users)
        UserPublicProfilesDao.UpdateOrInsertUser(user);
    }
  }
}
