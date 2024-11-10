// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Twitter.HelperExtensions
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable
namespace ticktick_WPF.Util.Twitter
{
  public static class HelperExtensions
  {
    public static string Join<T>(this IEnumerable<T> items, string separator)
    {
      return string.Join<T>(separator, (IEnumerable<T>) items.ToArray<T>());
    }

    public static IEnumerable<T> Concat<T>(this IEnumerable<T> items, T value)
    {
      return items.Concat<T>((IEnumerable<T>) new T[1]
      {
        value
      });
    }

    public static string EncodeRFC3986(this string value)
    {
      return string.IsNullOrEmpty(value) ? string.Empty : Regex.Replace(Uri.EscapeDataString(value), "(%[0-9a-f][0-9a-f])", (MatchEvaluator) (c => c.Value.ToUpper())).Replace("(", "%28").Replace(")", "%29").Replace("$", "%24").Replace("!", "%21").Replace("*", "%2A").Replace("'", "%27").Replace("%7E", "~");
    }
  }
}
