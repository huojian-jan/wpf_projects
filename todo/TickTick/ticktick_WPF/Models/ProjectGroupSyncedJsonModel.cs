﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ProjectGroupSyncedJsonModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class ProjectGroupSyncedJsonModel : BaseModel
  {
    public string GroupId { get; set; }

    public string UserId { get; set; }

    public string JsonString { get; set; }
  }
}