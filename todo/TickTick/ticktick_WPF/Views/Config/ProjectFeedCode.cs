// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.ProjectFeedCode
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  internal class ProjectFeedCode
  {
    public string projectId { get; set; }

    public string code { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? createdTime { get; set; }
  }
}
