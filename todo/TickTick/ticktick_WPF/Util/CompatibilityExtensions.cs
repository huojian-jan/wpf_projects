// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.CompatibilityExtensions
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util
{
  internal static class CompatibilityExtensions
  {
    public static List<T> Splice<T>(this List<T> input, int start, int count, params T[] objects)
    {
      List<T> range = input.GetRange(start, count);
      input.RemoveRange(start, count);
      input.InsertRange(start, (IEnumerable<T>) objects);
      return range;
    }

    public static string JavaSubstring(this string s, int begin, int end)
    {
      return s.Substring(begin, end - begin);
    }
  }
}
