// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.Month.WeekEventModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Views.Calendar.Month
{
  public class WeekEventModel
  {
    public int Row { get; set; }

    public int Column { get; set; }

    public int ColumnSpan { get; set; }

    public DateTime TargetDate { get; set; }

    public CalendarDisplayViewModel Data { get; set; }
  }
}
