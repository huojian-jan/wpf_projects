// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sync.LoadStatus
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Util.Sync
{
  public class LoadStatus
  {
    public LoadStatus(bool drainOff, DateTime? fromTime, DateTime toTime, int count)
    {
      this.DrainOff = drainOff;
      this.FromTime = fromTime;
      this.ToTime = toTime;
      this.Count = count;
    }

    public bool DrainOff { get; set; }

    public DateTime? FromTime { get; set; }

    public DateTime ToTime { get; set; }

    public int Count { get; set; }

    public string ProjectIds { get; set; }
  }
}
