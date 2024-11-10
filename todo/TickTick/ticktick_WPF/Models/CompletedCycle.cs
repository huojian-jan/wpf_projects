// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CompletedCycle
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class CompletedCycle
  {
    public int StartDate;
    public int EndDate;
    public int TargetDays;
    public List<int> CheckinDays = new List<int>();

    public int Streak => this.CheckinDays.Count;

    public bool isComplete => this.Streak == this.TargetDays;
  }
}
