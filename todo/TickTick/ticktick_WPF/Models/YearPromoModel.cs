// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.YearPromoModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class YearPromoModel
  {
    public string url { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime startTime { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime endTime { get; set; }
  }
}
