// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.UserActionModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class UserActionModel
  {
    public string kind { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime time { get; set; }

    public string platform { get; set; } = "windows";

    public string did { get; set; }

    public string id { get; set; }

    public string userCode { get; set; }

    public string type { get; set; }

    public string ctype { get; set; }

    public object data { get; set; }
  }
}
