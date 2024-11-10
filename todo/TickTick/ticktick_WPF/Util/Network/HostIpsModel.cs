// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Network.HostIpsModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Util.Network
{
  public class HostIpsModel
  {
    public string host { get; set; }

    public string[] ips { get; set; }

    public string ttl { get; set; }

    public string origin_ttl { get; set; }

    public string client_ip { get; set; }
  }
}
