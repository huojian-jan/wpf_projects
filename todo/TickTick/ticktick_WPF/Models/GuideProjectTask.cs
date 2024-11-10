// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.GuideProjectTask
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
  public class GuideProjectTask
  {
    public string id { get; set; }

    public string title { get; set; }

    public string columnId { get; set; }

    public string content { get; set; }

    public string desc { get; set; }

    public string parentId { get; set; }

    public string repeatFlag { get; set; }

    public string repeatFrom { get; set; }

    public string timeZone { get; set; }

    public string label { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? startDate { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? dueDate { get; set; }

    public bool? floating { get; set; }

    public bool? allDay { get; set; }

    public int priority { get; set; }

    public List<string> tags { get; set; }

    public List<string> reminders { get; set; }

    public List<GuideProjectTaskItem> items { get; set; }

    public List<GuideProjectTaskAction> actions { get; set; }

    public List<GuideProjectTaskResource> resources { get; set; }
  }
}
