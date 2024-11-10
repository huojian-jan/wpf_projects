// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Network.ApiMessageProcessingHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using ticktick_WPF.Resource;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Network
{
  internal class ApiMessageProcessingHandler : MessageProcessingHandler
  {
    protected override HttpRequestMessage ProcessRequest(
      HttpRequestMessage request,
      CancellationToken cancellationToken)
    {
      HttpRequestHeaders headers = request.Headers;
      headers.Add("User-Agent", "TickTick/W-" + Utils.GetVersion());
      headers.Add("x-device", Utils.GetDeviceInfo());
      headers.Add("traceid", DateTime.UtcNow.Ticks.ToString() ?? "");
      headers.Add("hl", Utils.GetLanguage());
      string loginUserAuth = LocalSettings.Settings.LoginUserAuth;
      if (!string.IsNullOrEmpty(loginUserAuth))
        headers.Add("Authorization", "OAuth " + loginUserAuth);
      UtilHttp.FixContent(request);
      return request;
    }

    protected override HttpResponseMessage ProcessResponse(
      HttpResponseMessage response,
      CancellationToken cancellationToken)
    {
      return response;
    }
  }
}
