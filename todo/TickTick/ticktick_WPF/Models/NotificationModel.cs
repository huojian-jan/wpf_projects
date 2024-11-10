// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.NotificationModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;

#nullable disable
namespace ticktick_WPF.Models
{
  public class NotificationModel
  {
    public string id { get; set; }

    public string title { get; set; }

    public string type { get; set; }

    [JsonProperty(PropertyName = "data")]
    public NotificationUserData notificationUserData { get; set; }

    public int actionStatus { get; set; }

    public DateTime createdTime { get; set; }

    public object category { get; set; }

    public object unread { get; set; }
  }
}
