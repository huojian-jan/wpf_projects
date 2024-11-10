// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.BugsnagHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Bugsnag.Clients;
using System;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF
{
  public static class BugsnagHelper
  {
    private static bool _shutDown;
    private static readonly DelayActionHandler _delaySender = new DelayActionHandler();
    private static Exception _exception;
    private static bool _userSetted;

    static BugsnagHelper()
    {
      if (LocalSettings.Settings?.Common != null && !BugsnagHelper._userSetted)
      {
        BugsnagHelper._userSetted = true;
        WPFClient.Config.SetUser(LocalSettings.Settings.LoginUserId, LocalSettings.Settings.LoginUserName, LocalSettings.Settings.LoginUserName);
      }
      WPFClient.Config.AutoNotify = false;
      BugsnagHelper._delaySender.SetAction((EventHandler) ((sender, args) =>
      {
        if (BugsnagHelper._exception == null)
          return;
        try
        {
          WPFClient.Notify(BugsnagHelper._exception);
        }
        catch (Exception ex)
        {
        }
      }));
    }

    public static void SendException(Exception e)
    {
      if (BugsnagHelper._shutDown)
        return;
      BugsnagHelper._shutDown = true;
      BugsnagHelper._exception = e;
      BugsnagHelper._delaySender.TryDoAction();
    }

    public static void SetUser(UserModel user)
    {
      if (user == null || BugsnagHelper._userSetted)
        return;
      WPFClient.Config.SetUser(user.userId, user.username, user.username);
    }
  }
}
