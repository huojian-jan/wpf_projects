// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.FilterDatePair
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class FilterDatePair
  {
    public DateTime? Start;
    public DateTime? End;
    public bool isRepeat;
    public bool IsNoDate;

    public FilterDatePair(DateTime? start, DateTime? end)
    {
      this.Start = start;
      this.End = end;
    }

    public FilterDatePair(bool isNoDate)
    {
      this.IsNoDate = isNoDate;
      this.isRepeat = !isNoDate;
    }
  }
}
