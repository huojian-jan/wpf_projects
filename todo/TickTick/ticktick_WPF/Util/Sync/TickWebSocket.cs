// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.TickWebSocket
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using TickTickUtils.Lang;
using WebSocket4Net;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class TickWebSocket
  {
    private const int AutoSendPingInterval = 540;
    private const int OpenTimeout = 10000;
    protected WebSocket _webSocket;
    private readonly System.Timers.Timer HeartTimer = new System.Timers.Timer();
    private readonly SemaphoreSlim Locker = new SemaphoreSlim(1);
    protected volatile bool _stopped;
    private string _pushUrl;

    public TickWebSocket(string pushUrl)
    {
      this._pushUrl = pushUrl;
      this.HeartTimer.Interval = 55000.0;
      this.HeartTimer.Elapsed += (ElapsedEventHandler) ((sender, args) => this.SendHello());
    }

    public async Task InitAsync()
    {
      TickWebSocket tickWebSocket = this;
      UtilLog.Info(tickWebSocket.GetType().Name + ".InitAsync()");
      tickWebSocket._stopped = false;
      tickWebSocket.HeartTimer.Start();
      WebSocket webSocket = await tickWebSocket.OpenAsync();
    }

    public virtual void SendHello()
    {
      this.SendTextAsync("hello").ContinueWith(new Action<Task>(UtilRun.LogTask));
    }

    public async Task SendTextAsync(string text)
    {
      if (text == null)
        return;
      (await this.OpenAsync())?.Send(text);
    }

    private async Task<WebSocket> OpenAsync()
    {
      TickWebSocket tickWebSocket = this;
      if (tickWebSocket is FocusWebSocket && (!UserDao.IsPro() || !LocalSettings.Settings.EnableFocus || !LocalSettings.Settings.FocusKeepInSync))
        return (WebSocket) null;
      Stopwatch watch = Stopwatch.StartNew();
      await tickWebSocket.Locker.WaitAsync();
      try
      {
        WebSocket webSocket = tickWebSocket._webSocket;
        if (!tickWebSocket._stopped && (webSocket != null ? (webSocket.State != WebSocketState.Open ? 1 : 0) : 1) != 0)
        {
          long waitTime = watch.ElapsedMilliseconds;
          if (webSocket != null)
          {
            UtilLog.Info(tickWebSocket.GetType().Name + ".OpenAsync() dispose ws: " + webSocket?.ToString() + ", stopped: " + tickWebSocket._stopped.ToString());
            UtilRun.Dispose((IDisposable) webSocket);
            tickWebSocket._webSocket = (WebSocket) null;
          }
          webSocket = await Task.Run<WebSocket>(new Func<WebSocket>(tickWebSocket.CreateOpenedWebSocket));
          tickWebSocket._webSocket = webSocket;
          UtilLog.Info(tickWebSocket.GetType().Name + ".OpenAsync() elapsed: " + watch.Elapsed.ToString() + "(wait: " + waitTime.ToString() + "ms, open: " + (watch.ElapsedMilliseconds - waitTime).ToString() + "ms) Opened ws: " + webSocket?.ToString() + ", stopped: " + tickWebSocket._stopped.ToString());
        }
        if (tickWebSocket._stopped)
        {
          UtilRun.Dispose((IDisposable) tickWebSocket._webSocket);
          tickWebSocket._webSocket = (WebSocket) null;
          throw new Exception(tickWebSocket.GetType().Name + " is stopped");
        }
        return webSocket;
      }
      catch (Exception ex)
      {
        watch.Stop();
        UtilLog.Debug(tickWebSocket.GetType().Name + ".OpenAsync() elapsed: " + watch.Elapsed.ToString() + ", ex: " + ex?.ToString() + ", stopped: " + tickWebSocket._stopped.ToString());
        UtilLog.Warn(tickWebSocket.GetType().Name + ".OpenAsync() Error: " + ex.Message);
        return (WebSocket) null;
      }
      finally
      {
        tickWebSocket.Locker.Release();
      }
    }

    private WebSocket CreateOpenedWebSocket()
    {
      WebSocket ws = new WebSocket(this._pushUrl, customHeaderItems: this.GetHeaders(), sslProtocols: SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12);
      ws.EnableAutoSendPing = true;
      ws.AutoSendPingInterval = 540;
      ws.Security.EnabledSslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12;
      ws.MessageReceived += new EventHandler<MessageReceivedEventArgs>(this.OnMessageReceived);
      Exception cause = (Exception) null;
      try
      {
        using (CountdownEvent openedLocker = new CountdownEvent(1))
        {
          ws.Error += new EventHandler<ErrorEventArgs>(OnOpenError);
          ws.Opened += new EventHandler(OnOpened);
          ws.Open();
          openedLocker.Wait(10000);
          if (ws.State == WebSocketState.Open)
            return ws;

          void OnOpenError(object sender, ErrorEventArgs args)
          {
            WebSocket webSocket = (WebSocket) sender;
            webSocket.Error -= new EventHandler<ErrorEventArgs>(OnOpenError);
            UtilLog.Error(this.GetType().Name + ".CreateOpenedWebSocket()  OpenError ws: " + webSocket?.ToString() + ", state: " + webSocket?.State.ToString() + ", ex: " + args.Exception?.ToString());
            cause = args.Exception;
            openedLocker.Signal();
          }

          void OnOpened(object sender, EventArgs e)
          {
            UtilLog.Info(this.GetType().Name + ".CreateOpenedWebSocket() Opened ws:" + sender?.ToString() + ", event: " + e?.ToString());
            ws.Error -= new EventHandler<ErrorEventArgs>(OnOpenError);
            ws.Opened -= new EventHandler(OnOpened);
            ws.Error += new EventHandler<ErrorEventArgs>(this.OnError);
            ws.Closed += new EventHandler(this.OnClosed);
            openedLocker.Signal();
          }
        }
        if (cause == null)
          cause = new Exception(this.GetType().Name + ".open error timeout: " + 10000.ToString() + "ms");
      }
      catch (Exception ex)
      {
        cause = ex;
      }
      UtilRun.Dispose((IDisposable) ws);
      throw cause ?? new Exception();
    }

    protected virtual List<KeyValuePair<string, string>> GetHeaders()
    {
      return (List<KeyValuePair<string, string>>) null;
    }

    private async void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
      TickWebSocket tickWebSocket = this;
      try
      {
        string message = e.Message;
        if (message != null && !message.Contains("pong"))
          UtilLog.Info(tickWebSocket.GetType().Name + ".OnMessageReceived(): " + message);
        if (string.IsNullOrEmpty(e.Message) || tickWebSocket._stopped)
          return;
        if (tickWebSocket.IsConnectId(message))
          await tickWebSocket.Register(message);
        else
          tickWebSocket.HandleMessage(message);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(tickWebSocket.GetType().Name + ".OnMessageReceived() ex: " + ex?.ToString());
      }
    }

    protected virtual bool IsConnectId(string msg) => false;

    protected virtual async Task Register(string msg)
    {
    }

    private void OnError(object sender, ErrorEventArgs error)
    {
      WebSocket webSocket = (WebSocket) sender;
      UtilLog.Error(this.GetType().Name + ".OnError() ws: " + webSocket?.ToString() + ", state: " + webSocket?.State.ToString() + ", ex: " + error.Exception?.ToString());
    }

    private void OnClosed(object sender, EventArgs e)
    {
      if (sender is WebSocket webSocket)
      {
        webSocket.Error -= new EventHandler<ErrorEventArgs>(this.OnError);
        webSocket.Closed -= new EventHandler(this.OnClosed);
      }
      UtilLog.Info(this.GetType().Name + ".OnClosed() ws: " + webSocket?.ToString());
    }

    protected virtual async Task HandleMessage(string message)
    {
    }

    public void DisposeWs()
    {
      this._stopped = true;
      this.HeartTimer.Stop();
      WebSocket webSocket = this._webSocket;
      UtilLog.Info(this.GetType().Name + ".DisposeWs() dispose ws: " + webSocket?.ToString() + ", stopped: " + this._stopped.ToString());
      if (webSocket == null)
        return;
      this._webSocket = (WebSocket) null;
      UtilRun.Dispose((IDisposable) webSocket);
    }
  }
}
