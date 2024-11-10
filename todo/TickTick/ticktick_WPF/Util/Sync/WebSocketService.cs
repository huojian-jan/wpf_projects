// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.WebSocketService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Timers;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public static class WebSocketService
  {
    private const int AutoSendPingInterval = 30;
    private const int OpenTimeout = 10000;
    private static int _reconnectCount;
    private static string PushUrl = "wss://wss.dida365.com/pc";
    private static string FocusPushUrl = "wss://wssp.dida365.com/pc";
    private static SyncWebSocket _webSocket;
    private static FocusWebSocket _focusSocket;
    private static readonly Timer SyncTimer = new Timer();

    public static async Task InitAsync()
    {
      string apiDomain = BaseUrl.GetApiDomain(true);
      WebSocketService.PushUrl = apiDomain.Replace("https", "wss").Replace("api", "wss") + "/pc";
      if (apiDomain.Contains("-"))
        WebSocketService.FocusPushUrl = apiDomain.Replace("https", "wss").Replace("api", "wss-pomodoro") + "/pc";
      WebSocketService.SyncTimer.Interval = 600000.0;
      WebSocketService.SyncTimer.Elapsed += (ElapsedEventHandler) ((sender, args) => UtilRun.Watch("websocket.SyncTimer() watch sync", (Action) (() => Utils.RunOnUiThread(App.Window.Dispatcher, (Action) (() =>
      {
        SyncManager.Sync();
        ReminderCalculator.AssembleReminders();
      }))), true));
      WebSocketService.SyncTimer.Start();
      WebSocketService._webSocket = new SyncWebSocket(WebSocketService.PushUrl);
      WebSocketService._focusSocket = new FocusWebSocket(WebSocketService.FocusPushUrl);
      WebSocketService._webSocket.InitAsync();
      WebSocketService._focusSocket.InitAsync();
      NetworkChange.NetworkAvailabilityChanged += (NetworkAvailabilityChangedEventHandler) ((sender, args) =>
      {
        bool flag = args != null && args.IsAvailable;
        UtilLog.Info("WebSocketService NetworkAvailabilityChanged.IsAvailable=" + flag.ToString());
        if (!flag)
          return;
        WebSocketService._webSocket.SendHello();
        WebSocketService._focusSocket.SendHello();
      });
    }

    public static void DisposeWs()
    {
      WebSocketService.SyncTimer.Stop();
      WebSocketService._webSocket.DisposeWs();
      WebSocketService._focusSocket.DisposeWs();
    }

    public static void CheckFocusSocket() => WebSocketService._focusSocket?.CheckConnected();
  }
}
