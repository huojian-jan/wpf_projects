// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.BindGoogleAccount
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Config;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class BindGoogleAccount
  {
    private SubscribeCalendar _parent;
    private static readonly object _locker = new object();
    private static BindGoogleAccount _instance;
    public EventHandler<EventArgs> Finished;
    private string _accountId;

    private BindGoogleAccount() => App.RegisterURIProtocol();

    public static BindGoogleAccount GetInstance(SubscribeCalendar parent = null)
    {
      if (BindGoogleAccount._instance == null)
      {
        lock (BindGoogleAccount._locker)
        {
          if (BindGoogleAccount._instance == null)
            BindGoogleAccount._instance = new BindGoogleAccount();
        }
      }
      BindGoogleAccount._instance._parent = parent;
      return BindGoogleAccount._instance;
    }

    public void Start(string accountId = null)
    {
      this._accountId = accountId;
      UriNotifier.Uri -= new EventHandler<UriModel>(this.OnUri);
      UriNotifier.Uri += new EventHandler<UriModel>(this.OnUri);
      int num = Utils.IsDida() ? 1 : 0;
      Utils.TryProcessStartUrl("https://accounts.google.com/o/oauth2/v2/auth?scope=https://www.googleapis.com/auth/calendar&redirect_uri=https://" + BaseUrl.Domain + "/pub/calendar/bind&response_type=code&prompt=select_account%20consent&state=ios_google&access_type=offline&client_id=" + (num != 0 ? "713670663672-vbvi5l69rgqo1u1dhg0fcp08u7h60ldn" : "366263775281") + ".apps.googleusercontent.com");
    }

    private async void OnUri(object sender, UriModel e)
    {
      BindGoogleAccount sender1 = this;
      if (!e.Path.StartsWith("v1/calendar/bind"))
        return;
      UriNotifier.Uri -= new EventHandler<UriModel>(sender1.OnUri);
      App.Window.TryShowMainWindow();
      BindCalendarAccountModel account = await SubscribeCalendarHelper.BindCalendarAccount(e.Parmas["code"]);
      if (account != null)
      {
        if (!string.IsNullOrEmpty(sender1._accountId))
        {
          BindCalendarAccountDao.DeleteBindAccountById(account.Id);
          BindCalendarAccountDao.DeleteBindAccountById(sender1._accountId);
          sender1._accountId = string.Empty;
        }
        await SubscribeCalendarHelper.SaveBindAccount(account);
        sender1._parent?.LoadData(false);
        sender1.Toast(Utils.GetString("ThirdLoginSuccess"));
      }
      else
        sender1.Toast(Utils.GetString("VerificationFailedHaveTry"));
      EventHandler<EventArgs> finished = sender1.Finished;
      if (finished == null)
        return;
      finished((object) sender1, (EventArgs) null);
    }

    public void Toast(string msg)
    {
      Task.Delay(300);
      if (this._parent != null)
        Utils.FindParent<SettingDialog>((DependencyObject) this._parent)?.Toast(msg);
      else
        Utils.Toast(msg);
    }
  }
}
