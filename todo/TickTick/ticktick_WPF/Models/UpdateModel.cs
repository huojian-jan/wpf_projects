// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.UpdateModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Models
{
  public class UpdateModel
  {
    public double versionNum { get; set; }

    public string downLoadUri { get; set; }

    public string fileName { get; set; }

    public DateTime publishDate { get; set; }

    public bool forceUpdate { get; set; }

    public bool greyForced { get; set; }
  }
}
