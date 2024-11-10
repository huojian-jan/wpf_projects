// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.FocusWebSocket
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Pomo;
using TickTickModels;
using TickTickUtils.Lang;
using WebSocket4Net;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class FocusWebSocket : TickWebSocket
  {
    public FocusWebSocket(string pushUrl)
      : base(pushUrl)
    {
    }

    protected override bool IsConnectId(string msg) => msg.StartsWith("{\"wsId\"");

    protected override async Task Register(string msg)
    {
    }

    protected override List<KeyValuePair<string, string>> GetHeaders()
    {
      return new List<KeyValuePair<string, string>>()
      {
        new KeyValuePair<string, string>("x-device", Utils.GetDeviceInfo()),
        new KeyValuePair<string, string>("hl", Utils.GetLanguage()),
        new KeyValuePair<string, string>("Authorization", "OAuth " + LocalSettings.Settings.LoginUserAuth)
      };
    }

    protected override async Task HandleMessage(string message)
    {
      try
      {
        FocusWebSocket.WSFocusModel wsFocusModel = JsonConvert.DeserializeObject<FocusWebSocket.WSFocusModel>(message);
        if (wsFocusModel?.data == null || Utils.IsEmptyDate(wsFocusModel.data.time))
          return;
        TickFocusManager.HandleRemoteOption(wsFocusModel.data);
      }
      catch (Exception ex)
      {
      }
    }

    public override void SendHello()
    {
      this.SendTextAsync("{\"type\":\"ping\"}").ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    public void CheckConnected()
    {
      if (this._stopped)
        return;
      WebSocket webSocket = this._webSocket;
      if ((webSocket != null ? (webSocket.State != WebSocketState.Open ? 1 : 0) : 1) == 0 || !UserDao.IsPro() || !LocalSettings.Settings.EnableFocus || !LocalSettings.Settings.FocusKeepInSync)
        return;
      this.SendHello();
    }

    private class WSFocusModel
    {
      public string type { get; set; }

      public FocusOptionModel data { get; set; }

      public long ctime { get; set; }
    }
  }
}
