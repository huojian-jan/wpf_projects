// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.BaseUrl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ticktick_WPF.Properties;
using ticktick_WPF.Util.Network;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class BaseUrl
  {
    public static readonly string Domain = "dida365.com";
    public static readonly string ApiDomain = "api.dida365.com";
    public static readonly string PullDomain = "pull.dida365.com";
    public static string Ip;
    public static string ApiIp;
    public static string PullIp;
    private static bool _testApi;

    public static string DomainUrl => "https://" + BaseUrl.Domain;

    public static string ApiDomainUrl => "https://" + BaseUrl.ApiDomain;

    public static bool IsDefault => true;

    static BaseUrl()
    {
      if (!BaseUrl.IsDefault)
        BaseUrl.SetIp();
      BaseUrl._testApi = File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\didaapitest.txt");
    }

    public static string GetDomain()
    {
      return !BaseUrl.IsDefault && !string.IsNullOrEmpty(BaseUrl.Ip) ? BaseUrl.Ip : BaseUrl.Domain;
    }

    public static string GetDomainUrl()
    {
      return !BaseUrl.IsDefault && !string.IsNullOrEmpty(BaseUrl.Ip) ? BaseUrl.Ip : BaseUrl.DomainUrl;
    }

    public static string GetPullDomain()
    {
      return !BaseUrl.IsDefault && !string.IsNullOrEmpty(BaseUrl.PullIp) ? BaseUrl.PullIp : BaseUrl.PullDomain;
    }

    public static void SetIp()
    {
      string ip1 = HttpDnsHandler.GetIp(BaseUrl.Domain);
      if (!string.IsNullOrEmpty(ip1))
        BaseUrl.ApiIp = ip1;
      string ip2 = HttpDnsHandler.GetIp(BaseUrl.ApiDomain);
      if (!string.IsNullOrEmpty(ip2))
        BaseUrl.Ip = ip2;
      string ip3 = HttpDnsHandler.GetIp(BaseUrl.PullDomain);
      if (string.IsNullOrEmpty(ip3))
        return;
      BaseUrl.PullIp = ip3;
    }

    public static string GetApiDomain(bool getDefault = false)
    {
      if (BaseUrl._testApi && !string.IsNullOrEmpty(Settings.Default.ApiTest))
        return Settings.Default.ApiTest;
      return getDefault || BaseUrl.IsDefault || string.IsNullOrEmpty(BaseUrl.ApiIp) ? BaseUrl.ApiDomainUrl : "https://" + BaseUrl.ApiIp;
    }

    public static string GetHostName()
    {
      string apiDomain = BaseUrl.GetApiDomain();
      if (apiDomain.Contains("."))
      {
        string[] source = apiDomain.Split('.');
        if (source.Length >= 3)
          return string.Join(".", (IEnumerable<string>) ((IEnumerable<string>) source).Skip<string>(source.Length - 2).ToList<string>());
      }
      return string.Empty;
    }

    public static string GetUrl(string path) => BaseUrl.GetApiDomain() + path;

    public static string GetReleaseNoteUrl()
    {
      return "https://" + BaseUrl.GetPullDomain() + "/windows/release_note.json";
    }
  }
}
