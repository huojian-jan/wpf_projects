// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.EnumerableExtensions
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Util
{
  internal static class EnumerableExtensions
  {
    public static Dictionary<TKey, TValue> ToDictionaryEx<TElement, TKey, TValue>(
      this IEnumerable<TElement> source,
      Func<TElement, TKey> keyGetter,
      Func<TElement, TValue> valueGetter)
    {
      Dictionary<TKey, TValue> dictionaryEx = new Dictionary<TKey, TValue>();
      foreach (TElement element in source)
      {
        TKey key = keyGetter(element);
        if ((object) key != null && !dictionaryEx.ContainsKey(key))
          dictionaryEx.Add(key, valueGetter(element));
      }
      return dictionaryEx;
    }

    public static Dictionary<TKey, List<TValue>> GroupEx<TElement, TKey, TValue>(
      this IEnumerable<TElement> source,
      Func<TElement, TKey> keyGetter,
      Func<TElement, TValue> valueGetter)
    {
      Dictionary<TKey, List<TValue>> dictionary = new Dictionary<TKey, List<TValue>>();
      foreach (TElement element in source)
      {
        TKey key = keyGetter(element);
        if ((object) key != null)
        {
          TValue obj = valueGetter(element);
          if (dictionary.ContainsKey(key))
            dictionary[key].Add(obj);
          else
            dictionary[key] = new List<TValue>() { obj };
        }
      }
      return dictionary;
    }

    public static List<TElement> DistinctEx<TElement, TValue>(
      this IEnumerable<TElement> source,
      Func<TElement, TValue> valueGetter)
    {
      Dictionary<TValue, TElement> dictionary = new Dictionary<TValue, TElement>();
      foreach (TElement element in source)
      {
        if ((object) element != null)
        {
          TValue key = valueGetter(element);
          if ((object) key != null && !dictionary.ContainsKey(key))
            dictionary.Add(key, element);
        }
      }
      return dictionary.Values.ToList<TElement>();
    }
  }
}
