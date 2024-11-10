// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.OutlookCalendarModels
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class OutlookCalendarModels
  {
    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime begin { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime end { get; set; }

    public List<string> errorIds { get; set; }

    public List<BindCalendarModel> events { get; set; }
  }
}
