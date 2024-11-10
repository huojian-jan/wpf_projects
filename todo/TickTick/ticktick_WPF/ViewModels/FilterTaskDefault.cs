// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.FilterTaskDefault
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class FilterTaskDefault
  {
    public ProjectModel ProjectModel { get; set; }

    public int? Priority { get; set; }

    public List<string> DefaultTags { get; set; } = new List<string>();

    public DateTime? DefaultDate { get; set; }

    public bool IsNote { get; set; }

    public bool OnlyNote { get; set; }
  }
}
