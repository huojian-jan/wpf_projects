// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineCellOperation
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  [Flags]
  public enum TimelineCellOperation
  {
    None = 1,
    Start = 2,
    Full = 4,
    End = 8,
    Hover = 16, // 0x00000010
    Hide = 32, // 0x00000020
    Fold = 64, // 0x00000040
    Edit = 128, // 0x00000080
    BatchSelect = 256, // 0x00000100
    HoverStart = Hover | Start, // 0x00000012
    HoverFull = Hover | Full, // 0x00000014
    HoverEnd = Hover | End, // 0x00000018
  }
}
