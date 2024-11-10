// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ListDiff.ListExtensions
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Util.ListDiff
{
  internal static class ListExtensions
  {
    public static List<T> Splice<T>(this List<T> input, int start, int count, params T[] objects)
    {
      List<T> range = input.GetRange(start, count);
      input.RemoveRange(start, count);
      input.InsertRange(start, (IEnumerable<T>) objects);
      return range;
    }

    public static bool StartsWith<T>(this IReadOnlyList<T> target, IReadOnlyList<T> other)
    {
      return target.Count >= other.Count && target.Take<T>(other.Count).SequenceEqual<T>((IEnumerable<T>) other);
    }

    public static bool EndsWith<T>(this IReadOnlyList<T> target, IReadOnlyList<T> other)
    {
      return target.Count >= other.Count && target.Skip<T>(target.Count - other.Count).Take<T>(other.Count).SequenceEqual<T>((IEnumerable<T>) other);
    }

    public static IReadOnlyList<T> Substring<T>(
      this IReadOnlyList<T> target,
      int start,
      int length = -1)
    {
      if (length == -1)
        length = target.Count - start;
      return (target is List<T> objList ? (IReadOnlyList<T>) objList.GetRange(start, length) : (IReadOnlyList<T>) null) ?? (IReadOnlyList<T>) target.Skip<T>(start).Take<T>(length).ToList<T>();
    }

    private static bool CompareRange<T>(
      IReadOnlyList<T> listA,
      int offsetA,
      IReadOnlyList<T> listB,
      int offsetB,
      int count)
    {
      for (int index = 0; index < count; ++index)
      {
        if (!EqualityComparer<T>.Default.Equals(listA[offsetA + index], listB[offsetB + index]))
          return false;
      }
      return true;
    }

    public static int IndexOf<T>(this IReadOnlyList<T> target, IReadOnlyList<T> other, int start = 0)
    {
      int num = target.Count - other.Count;
      for (int offsetA = start; offsetA < num; ++offsetA)
      {
        if (ListExtensions.CompareRange<T>(target, offsetA, other, 0, other.Count))
          return offsetA;
      }
      return -1;
    }

    public static bool EqualsAt<T>(
      this IReadOnlyList<T> target,
      int targetPos,
      IReadOnlyList<T> other,
      int otherPos)
    {
      return EqualityComparer<T>.Default.Equals(target[targetPos], other[otherPos]);
    }
  }
}
