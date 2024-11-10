// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Network.ProxyModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Net;

#nullable disable
namespace ticktick_WPF.Util.Network
{
  public class ProxyModel
  {
    internal readonly string key;
    internal readonly IWebProxy proxy;

    public override bool Equals(object obj)
    {
      return obj is ProxyModel proxyModel && this.key.Equals(proxyModel.key);
    }

    public override int GetHashCode() => this.key.GetHashCode();

    private ProxyModel(string key, IWebProxy proxy)
    {
      this.key = key;
      this.proxy = proxy;
    }

    internal bool IsSame(ProxyModel other) => other != null && this.key.Equals(other.key);

    public override string ToString() => "ProxyModel=" + this.key;

    public static ProxyModel Create(
      int type,
      string addressStr,
      string portStr,
      string user,
      string pwd,
      string domain)
    {
      string key = "type=" + type.ToString();
      IWebProxy proxy = (IWebProxy) null;
      switch (type)
      {
        case 1:
          key = key + ", address=" + addressStr + ":" + portStr;
          string endPoint;
          if (HttpClientHelper.TryGetEndPoint(addressStr, portStr, out endPoint))
          {
            proxy = (IWebProxy) new WebProxy(endPoint);
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pwd))
            {
              if (domain == null)
                domain = string.Empty;
              key = key + ", user=" + user + ", pwd=" + pwd.GetHashCode().ToString() + ", domain=" + domain;
              proxy.Credentials = (ICredentials) new NetworkCredential(user, Utils.Base64Decode(pwd), domain);
              break;
            }
            break;
          }
          break;
        case 2:
          proxy = WebRequest.GetSystemWebProxy();
          break;
      }
      return new ProxyModel(key, proxy);
    }
  }
}
