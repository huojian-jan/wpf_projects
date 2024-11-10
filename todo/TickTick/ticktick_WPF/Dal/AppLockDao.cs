// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.AppLockDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Dal
{
  public static class AppLockDao
  {
    private const string PassPrefix = "&";
    private const string PassPosfix = "#";

    public static async Task<AppLockModel> GetLockConfig()
    {
      try
      {
        string userId = LocalSettings.Settings.LoginUserId;
        if (string.IsNullOrEmpty(userId))
          return (AppLockModel) null;
        return await App.Connection.Table<AppLockModel>().Where((Expression<Func<AppLockModel, bool>>) (m => m.UserId == userId)).FirstOrDefaultAsync();
      }
      catch (Exception ex)
      {
        return (AppLockModel) null;
      }
    }

    public static async Task SaveLockConfig(AppLockModel model)
    {
      AppLockModel lockConfig = await AppLockDao.GetLockConfig();
      model.UserId = LocalSettings.Settings.LoginUserId;
      if (lockConfig != null)
      {
        if (string.IsNullOrEmpty(model.Password))
        {
          model.Password = lockConfig.Password;
          model._Id = lockConfig._Id;
        }
        model.Locked = lockConfig.Locked;
        int num = await App.Connection.UpdateAsync((object) model);
      }
      else
      {
        int num1 = await App.Connection.InsertAsync((object) model);
      }
      AppLockCache.SetModel(model);
    }

    public static async Task SetAppLocked(bool locked)
    {
      AppLockModel model = await AppLockDao.GetLockConfig();
      if (model == null)
      {
        model = (AppLockModel) null;
      }
      else
      {
        model.Locked = locked;
        int num = await App.Connection.UpdateAsync((object) model);
        AppLockCache.SetModel(model);
        model = (AppLockModel) null;
      }
    }

    public static async Task ClearPassword()
    {
      await AppLockDao.SaveLockPassword(string.Empty, true);
    }

    public static async Task SaveLockPassword(string password, bool clear = false)
    {
      if (!string.IsNullOrEmpty(password))
        password = Utils.Base64Encode("&" + password + "#");
      AppLockModel config = await AppLockDao.GetLockConfig();
      if (config != null)
      {
        config.Password = password;
        if (clear)
          config.Locked = false;
        int num = await App.Connection.UpdateAsync((object) config);
        AppLockCache.SetModel(config);
        config = (AppLockModel) null;
      }
      else
      {
        AppLockModel model = new AppLockModel()
        {
          UserId = Utils.GetCurrentUserIdInt().ToString(),
          Password = password,
          MinLock = false,
          LockAfter = false,
          LockInterval = 5,
          LockWidget = false,
          Locked = false
        };
        int num = await App.Connection.InsertAsync((object) model);
        AppLockCache.SetModel(model);
        model = (AppLockModel) null;
        config = (AppLockModel) null;
      }
    }
  }
}
