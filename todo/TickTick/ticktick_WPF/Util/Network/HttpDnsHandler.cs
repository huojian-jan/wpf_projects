// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Network.HttpDnsHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util.Network
{
  public static class HttpDnsHandler
  {
    public static string GetIp(string host)
    {
      try
      {
        string uriString = "https://203.107.1.1/186062/d?host=" + host;
        using (HttpClient httpClient = new HttpClient())
        {
          HostIpsModel hostIpsModel = JsonConvert.DeserializeObject<HostIpsModel>(httpClient.GetAsync(new Uri(uriString)).Result.Content.ReadAsStringAsync().Result);
          if (hostIpsModel != null)
          {
            int? length = hostIpsModel.ips?.Length;
            int num = 0;
            if (length.GetValueOrDefault() > num & length.HasValue)
              return ((IEnumerable<string>) hostIpsModel.ips).LastOrDefault<string>();
          }
        }
      }
      catch (Exception ex)
      {
        UtilLog.Error(ex);
      }
      return string.Empty;
    }
  }
}
