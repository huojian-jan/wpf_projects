// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.AppConfigManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Timers;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service
{
  public static class AppConfigManager
  {
    private static Timer _timer;

    public static void StartPolling()
    {
      int interval = LocalSettings.Settings.ExtraSettings.AppConfigInterval * 1000;
      if (AppConfigManager._timer == null)
      {
        AppConfigManager._timer = new Timer((double) interval);
      }
      else
      {
        AppConfigManager._timer.Stop();
        AppConfigManager._timer.Interval = (double) interval;
      }
      AppConfigManager._timer.Elapsed -= new ElapsedEventHandler(AppConfigManager.TimerOnElapsed);
      AppConfigManager._timer.Elapsed += new ElapsedEventHandler(AppConfigManager.TimerOnElapsed);
      AppConfigManager._timer.Start();
    }

    private static void TimerOnElapsed(object sender, ElapsedEventArgs e)
    {
      AppConfigManager.PerformPullAppConfig();
    }

    public static void StopPolling() => AppConfigManager._timer?.Stop();

    public static async Task PerformPullAppConfig(bool login = false)
    {
      try
      {
        await Communicator.PullAppConfig(login);
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
    }
  }
}
