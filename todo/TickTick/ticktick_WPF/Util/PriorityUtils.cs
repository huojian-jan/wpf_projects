// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.PriorityUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util
{
  public class PriorityUtils
  {
    public static Dictionary<string, int> Priorities = new Dictionary<string, int>()
    {
      {
        Utils.GetString("PriorityHigh"),
        5
      },
      {
        Utils.GetString("PriorityMedium"),
        3
      },
      {
        Utils.GetString("PriorityLow"),
        1
      },
      {
        Utils.GetString("PriorityNull"),
        0
      }
    };
  }
}
