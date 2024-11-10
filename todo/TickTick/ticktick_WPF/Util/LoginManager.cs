// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.LoginManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Views.Pomo;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public class LoginManager
  {
    private static TTAsyncLocker<bool> _logoutLocker = new TTAsyncLocker<bool>();

    public static async Task<(UserModel, ApiException)> Login(string name, string pw)
    {
      try
      {
        UserModel user = await Communicator.SignOn(name, pw);
        if (user?.userId != null && user.userId != "")
        {
          await UserDao.UpdateOrInsertUserModelListDbAsync(user);
          await LocalSettings.ResetUserSettings(user, "mail_account");
          await UserManager.PullUserInfo();
          await UserActCollectUtils.OnDeviceDataChanged();
          await AppConfigManager.PerformPullAppConfig(true);
          JumpHelper.InitJumpList();
          JumpHelper.InitChangeEvents();
          return (user, (ApiException) null);
        }
        user = (UserModel) null;
      }
      catch (ApiException ex)
      {
        return ((UserModel) null, ex);
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(ExceptionUtils.BuildExceptionMessage(ex));
        UserActCollectUtils.SendException(ex, ExceptionType.Task);
      }
      return ((UserModel) null, (ApiException) null);
    }

    public static async Task<(UserModel, string)> SignUp(string name, string userName, string pw)
    {
      UserModel user = await Communicator.Signup(name, userName, pw);
      if (user?.userId != null && user.userId != "")
      {
        App.IsRegisterSuccess = true;
        await UserDao.UpdateOrInsertUserModelListDbAsync(user);
        await LocalSettings.ResetUserSettings(user, "mail_account");
        await UserManager.PullUserInfo();
        await UserActCollectUtils.OnDeviceDataChanged();
        JumpHelper.InitJumpList();
        JumpHelper.InitChangeEvents();
        return (user, string.Empty);
      }
      try
      {
        return ((UserModel) null, JObject.Parse(user?.username ?? string.Empty)["errorCode"]?.ToString());
      }
      catch (Exception ex)
      {
      }
      return ((UserModel) null, (string) null);
    }

    public static async Task<bool> Logout()
    {
      return await LoginManager._logoutLocker.RunAsync((Func<Task<bool>>) (async () =>
      {
        if (string.IsNullOrEmpty(LocalSettings.Settings.LoginUserAuth))
          return false;
        if (!await TickFocusManager.ExitPomo())
          return false;
        MainWindowManager.HandleLogout();
        JumpHelper.ClearJumpList();
        await LocalSettings.BeforeLogout();
        Utils.CleanAfterLogout();
        await Communicator.SignOut();
        await UserDao.AdjustCheckPointAfterLogout(Utils.GetCurrentUserIdInt().ToString());
        await Utils.LogotEmptyDb();
        UserManager.LastInitTime = DateTime.Now.AddMinutes(-2.0);
        await UserActCollectUtils.OnDeviceDataChanged();
        AppConfigManager.StopPolling();
        LocalSettings.ClearModel();
        return true;
      }));
    }
  }
}
