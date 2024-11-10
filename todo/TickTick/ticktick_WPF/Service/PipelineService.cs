// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.PipelineService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service
{
  [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
  internal class PipelineService : IC2SMessages
  {
    private ServiceHost _serverHost;
    private static PipelineService _instance = new PipelineService();
    private readonly List<Guid> _registeredClients = new List<Guid>();

    private PipelineService()
    {
    }

    public static PipelineService GetInstance() => PipelineService._instance;

    public void RegisterServiceHost()
    {
      try
      {
        this._serverHost = new ServiceHost((object) this, Array.Empty<Uri>());
        this._serverHost.AddServiceEndpoint(typeof (IC2SMessages), (Binding) new NetNamedPipeBinding(), "net.pipe://localhost/TickTick");
        this._serverHost.Open();
      }
      catch (Exception ex)
      {
        UtilLog.Warn(ExceptionUtils.BuildExceptionMessage(ex));
      }
    }

    public void CloseServiceHost()
    {
      try
      {
        this._serverHost.Close();
      }
      catch (Exception ex)
      {
      }
    }

    public void Register(Guid clientId)
    {
      UtilLog.Info("Pipeline.Register " + clientId.ToString());
      if (this._registeredClients.Contains(clientId))
        return;
      this._registeredClients.Add(clientId);
    }

    public void DisplayCommandOnServer(string text)
    {
      UtilLog.Info("Pipeline.DisplayCommandOnServer " + text);
      if (text.StartsWith("ticktick://"))
        this.TickTickUri(text);
      else
        App.Window?.HandlePipelineMsg(text);
    }

    private void TickTickUri(string uri)
    {
      uri = uri.Substring(uri.IndexOf("//", StringComparison.Ordinal) + 2);
      uri = HttpUtility.UrlDecode(uri);
      if (!uri.Contains<char>('?'))
        return;
      UriNotifier.NotifyUri(uri.Substring(0, uri.IndexOf('?')), uri.Substring(uri.IndexOf('?') + 1));
    }
  }
}
