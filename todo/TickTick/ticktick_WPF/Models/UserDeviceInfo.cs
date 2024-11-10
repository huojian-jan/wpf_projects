// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.UserDeviceInfo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class UserDeviceInfo
  {
    public string did { get; set; }

    public string platform { get; set; } = "windows";

    public string ver { get; set; }

    public string channel { get; set; }

    public string os { get; set; }

    public string osv { get; set; }

    public string device { get; set; }

    public string name { get; set; }

    public string campaign { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime ctime { get; set; }

    public string userCode { get; set; }

    public string hl { get; set; }

    public string tz { get; set; }

    public string locale { get; set; }

    public bool enable_tz { get; set; }
  }
}
