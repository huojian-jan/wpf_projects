// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.Model.CalendarAccountBean
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util.Sync.Model
{
  public class CalendarAccountBean
  {
    [JsonProperty(PropertyName = "accountId")]
    public string AccountId { get; set; }

    [JsonProperty(PropertyName = "calendarEventBeans")]
    public List<CalendarEventBean> CalendarBeans { get; set; }
  }
}
