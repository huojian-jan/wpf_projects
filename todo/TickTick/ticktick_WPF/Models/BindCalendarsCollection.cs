// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.BindCalendarsCollection
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
  [Serializable]
  public class BindCalendarsCollection : BaseModel
  {
    [JsonProperty("events")]
    public List<BindCalendarModel> Events { get; set; }

    [JsonProperty("errorIds")]
    public List<string> ErrorIds { get; set; }
  }
}
