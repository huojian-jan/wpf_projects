﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.GuideProjectData
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class GuideProjectData
  {
    public string sortType { get; set; }

    public string viewMode { get; set; }

    public List<GuideProjectColumn> columns { get; set; }

    public List<GuideProjectTask> tasks { get; set; }
  }
}