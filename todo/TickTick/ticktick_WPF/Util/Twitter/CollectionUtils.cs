// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Twitter.CollectionUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.ObjectModel;

#nullable disable
namespace ticktick_WPF.Util.Twitter
{
  public static class CollectionUtils
  {
    public static T GetLastItem<T>(Collection<T> source, int fromIndex, Func<T, bool> fun)
    {
      if (source == null)
        return default (T);
      for (int index = Math.Min(fromIndex, source.Count - 1); index >= 0; --index)
      {
        if (fun(source[index]))
          return source[index];
      }
      return default (T);
    }

    public static T GetFirstItem<T>(Collection<T> source, int fromIndex, Func<T, bool> fun)
    {
      if (source == null)
        return default (T);
      for (int index = Math.Max(fromIndex, 0); index < source.Count; ++index)
      {
        if (fun(source[index]))
          return source[index];
      }
      return default (T);
    }
  }
}
