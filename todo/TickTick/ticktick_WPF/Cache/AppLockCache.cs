// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.AppLockCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Cache
{
  public class AppLockCache
  {
    private const string PassPrefix = "&";
    private const string PassPosfix = "#";
    public static AppLockModel LockModel;

    public static async Task<AppLockModel> GetModel()
    {
      if (AppLockCache.LockModel == null || AppLockCache.LockModel.UserId != LocalSettings.Settings.LoginUserId)
        AppLockCache.LockModel = await AppLockDao.GetLockConfig();
      return AppLockCache.LockModel;
    }

    public static void SetModel(AppLockModel model) => AppLockCache.LockModel = model;

    public static async Task<bool> IsLockValid()
    {
      return !string.IsNullOrEmpty(await AppLockCache.GetLockPassword());
    }

    public static async Task<string> GetLockPassword()
    {
      return AppLockCache.DecodePassword((await AppLockCache.GetModel())?.Password);
    }

    private static string DecodePassword(string password)
    {
      if (string.IsNullOrEmpty(password))
        return string.Empty;
      string str = Utils.Base64Decode(password);
      return str != null && str.StartsWith("&") && str.EndsWith("#") ? str.Substring(1, str.Length - 2) : string.Empty;
    }

    public static async Task<bool> GetAppLocked(bool isStart = false)
    {
      AppLockModel model = await AppLockCache.GetModel();
      if (isStart)
      {
        bool flag = model != null;
        if (flag)
          flag = await AppLockCache.IsLockValid();
        if (flag && model.MinLock && !model.Locked)
        {
          model.Locked = true;
          int num = await App.Connection.UpdateAsync((object) model);
          UtilLog.Info("AppLockStart " + model.Locked.ToString());
        }
      }
      bool appLocked = model != null && model.Locked;
      model = (AppLockModel) null;
      return appLocked;
    }

    public static async Task CheckLockStart()
    {
      AppLockModel model = await AppLockCache.GetModel();
    }

    public static async Task<bool> IsMinLock()
    {
      AppLockModel model = await AppLockCache.GetModel();
      return model != null && model.MinLock && !string.IsNullOrEmpty(AppLockCache.DecodePassword(model.Password));
    }
  }
}
