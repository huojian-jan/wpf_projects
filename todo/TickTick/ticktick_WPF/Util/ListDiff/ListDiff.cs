// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.ListDiff.ListDiff
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Util.ListDiff
{
  public class ListDiff
  {
    private readonly TimeSpan _diffTimeout;
    private readonly int _diffDualThreshold;

    private ListDiff(TimeSpan timeout, int dualThreshold)
    {
      this._diffTimeout = timeout;
      this._diffDualThreshold = dualThreshold;
    }

    public static void ApplyDiff<T>(
      ObservableCollection<T> original,
      ObservableCollection<T> revised)
    {
      List<Diff<T>> list = ticktick_WPF.Util.ListDiff.ListDiff.Compare<T>((IReadOnlyList<T>) original, (IReadOnlyList<T>) revised).ToList<Diff<T>>();
      if (!list.Any<Diff<T>>())
        return;
      int index = 0;
      foreach (Diff<T> diff in list)
      {
        switch (diff.Operation)
        {
          case Operation.Delete:
            using (IEnumerator<T> enumerator = diff.Items.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                T current = enumerator.Current;
                original.Remove(current);
              }
              continue;
            }
          case Operation.Insert:
            using (IEnumerator<T> enumerator = diff.Items.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                T current = enumerator.Current;
                original.Insert(index, current);
                ++index;
              }
              continue;
            }
          case Operation.Equal:
            index += diff.Items.Count;
            continue;
          default:
            continue;
        }
      }
    }

    public static IEnumerable<Diff<T>> Compare<T>(
      IReadOnlyList<T> text1,
      IReadOnlyList<T> text2,
      TimeSpan? timeSpan = null,
      int diffDualThreshold = 32)
    {
      if (!timeSpan.HasValue)
        timeSpan = new TimeSpan?(TimeSpan.FromSeconds(1.0));
      return (IEnumerable<Diff<T>>) new ticktick_WPF.Util.ListDiff.ListDiff(timeSpan.Value, diffDualThreshold).CompareUsingOptions<T>(text1, text2);
    }

    private List<Diff<T>> CompareUsingOptions<T>(IReadOnlyList<T> text1, IReadOnlyList<T> text2)
    {
      if (text1.SequenceEqual<T>((IEnumerable<T>) text2))
        return new List<Diff<T>>()
        {
          new Diff<T>(Operation.Equal, text1)
        };
      int commonPrefix = ticktick_WPF.Util.ListDiff.ListDiff.GetCommonPrefix<T>(text1, text2);
      IReadOnlyList<T> items1 = text1.Substring<T>(0, commonPrefix);
      text1 = text1.Substring<T>(commonPrefix);
      text2 = text2.Substring<T>(commonPrefix);
      int commonSuffix = ticktick_WPF.Util.ListDiff.ListDiff.GetCommonSuffix<T>(text1, text2);
      IReadOnlyList<T> items2 = text1.Substring<T>(text1.Count - commonSuffix);
      text1 = text1.Substring<T>(0, text1.Count - commonSuffix);
      text2 = text2.Substring<T>(0, text2.Count - commonSuffix);
      List<Diff<T>> diffs = this.Compute<T>(text1, text2);
      if (items1.Count != 0)
        diffs.Insert(0, new Diff<T>(Operation.Equal, items1));
      if (items2.Count != 0)
        diffs.Add(new Diff<T>(Operation.Equal, items2));
      ticktick_WPF.Util.ListDiff.ListDiff.CleanupMerge<T>(diffs);
      return diffs;
    }

    private List<Diff<T>> Compute<T>(IReadOnlyList<T> text1, IReadOnlyList<T> text2)
    {
      List<Diff<T>> diffList1 = new List<Diff<T>>();
      if (text1.Count == 0)
      {
        diffList1.Add(new Diff<T>(Operation.Insert, text2));
        return diffList1;
      }
      if (text2.Count == 0)
      {
        diffList1.Add(new Diff<T>(Operation.Delete, text1));
        return diffList1;
      }
      IReadOnlyList<T> target = text1.Count > text2.Count ? text1 : text2;
      IReadOnlyList<T> objList = text1.Count > text2.Count ? text2 : text1;
      int length = target.IndexOf<T>(objList);
      if (length != -1)
      {
        Operation operation = text1.Count > text2.Count ? Operation.Delete : Operation.Insert;
        diffList1.Add(new Diff<T>(operation, target.Substring<T>(0, length)));
        diffList1.Add(new Diff<T>(Operation.Equal, objList));
        diffList1.Add(new Diff<T>(operation, target.Substring<T>(length + objList.Count)));
        return diffList1;
      }
      IReadOnlyList<T>[] halfMatch = ticktick_WPF.Util.ListDiff.ListDiff.GetHalfMatch<T>(text1, text2);
      if (halfMatch != null)
      {
        IReadOnlyList<T> text1_1 = halfMatch[0];
        IReadOnlyList<T> text1_2 = halfMatch[1];
        IReadOnlyList<T> text2_1 = halfMatch[2];
        IReadOnlyList<T> text2_2 = halfMatch[3];
        IReadOnlyList<T> items = halfMatch[4];
        List<Diff<T>> diffList2 = this.CompareUsingOptions<T>(text1_1, text2_1);
        List<Diff<T>> collection = this.CompareUsingOptions<T>(text1_2, text2_2);
        List<Diff<T>> diffList3 = diffList2;
        diffList3.Add(new Diff<T>(Operation.Equal, items));
        diffList3.AddRange((IEnumerable<Diff<T>>) collection);
        return diffList3;
      }
      List<Diff<T>> map = this.GetMap<T>(text1, text2);
      if (map != null)
        return map;
      return new List<Diff<T>>()
      {
        new Diff<T>(Operation.Delete, text1),
        new Diff<T>(Operation.Insert, text2)
      };
    }

    private List<Diff<T>> GetMap<T>(IReadOnlyList<T> text1, IReadOnlyList<T> text2)
    {
      DateTime dateTime = DateTime.Now + this._diffTimeout;
      int count1 = text1.Count;
      int count2 = text2.Count;
      int num1 = count1 + count2 - 1;
      bool flag1 = this._diffDualThreshold * 2 < num1;
      List<HashSet<long>> vMap1 = new List<HashSet<long>>();
      List<HashSet<long>> vMap2 = new List<HashSet<long>>();
      Dictionary<int, int> dictionary1 = new Dictionary<int, int>();
      Dictionary<int, int> dictionary2 = new Dictionary<int, int>();
      dictionary1.Add(1, 0);
      dictionary2.Add(1, 0);
      long key1 = 0;
      Dictionary<long, int> dictionary3 = new Dictionary<long, int>();
      bool flag2 = false;
      bool flag3 = (count1 + count2) % 2 == 1;
      for (int index = 0; index < num1; ++index)
      {
        if (DateTime.Now > dateTime)
          return (List<Diff<T>>) null;
        vMap1.Add(new HashSet<long>());
        for (int key2 = -index; key2 <= index; key2 += 2)
        {
          int num2 = key2 == -index || key2 != index && dictionary1[key2 - 1] < dictionary1[key2 + 1] ? dictionary1[key2 + 1] : dictionary1[key2 - 1] + 1;
          int num3 = num2 - key2;
          if (flag1)
          {
            key1 = ticktick_WPF.Util.ListDiff.ListDiff.GetFootprint(num2, num3);
            if (flag3 && dictionary3.ContainsKey(key1))
              flag2 = true;
            if (!flag3)
              dictionary3.Add(key1, index);
          }
          while (!flag2 && num2 < count1 && num3 < count2 && text1.EqualsAt<T>(num2, text2, num3))
          {
            ++num2;
            ++num3;
            if (flag1)
            {
              key1 = ticktick_WPF.Util.ListDiff.ListDiff.GetFootprint(num2, num3);
              if (flag3 && dictionary3.ContainsKey(key1))
                flag2 = true;
              if (!flag3)
                dictionary3.Add(key1, index);
            }
          }
          if (dictionary1.ContainsKey(key2))
            dictionary1[key2] = num2;
          else
            dictionary1.Add(key2, num2);
          vMap1[index].Add(ticktick_WPF.Util.ListDiff.ListDiff.GetFootprint(num2, num3));
          if (num2 == count1 && num3 == count2)
            return ticktick_WPF.Util.ListDiff.ListDiff.DiffPath1<T>((IReadOnlyList<HashSet<long>>) vMap1, text1, text2);
          if (flag2)
          {
            List<HashSet<long>> range = vMap2.GetRange(0, dictionary3[key1] + 1);
            List<Diff<T>> map = ticktick_WPF.Util.ListDiff.ListDiff.DiffPath1<T>((IReadOnlyList<HashSet<long>>) vMap1, text1.Substring<T>(0, num2), text2.Substring<T>(0, num3));
            map.AddRange((IEnumerable<Diff<T>>) ticktick_WPF.Util.ListDiff.ListDiff.DiffPath2<T>((IReadOnlyList<HashSet<long>>) range, text1.Substring<T>(num2), text2.Substring<T>(num3)));
            return map;
          }
        }
        if (flag1)
        {
          vMap2.Add(new HashSet<long>());
          for (int key3 = -index; key3 <= index; key3 += 2)
          {
            int x = key3 == -index || key3 != index && dictionary2[key3 - 1] < dictionary2[key3 + 1] ? dictionary2[key3 + 1] : dictionary2[key3 - 1] + 1;
            int y = x - key3;
            key1 = ticktick_WPF.Util.ListDiff.ListDiff.GetFootprint(count1 - x, count2 - y);
            if (!flag3 && dictionary3.ContainsKey(key1))
              flag2 = true;
            if (flag3)
              dictionary3.Add(key1, index);
            while (!flag2 && x < count1 && y < count2 && text1.EqualsAt<T>(count1 - x - 1, text2, count2 - y - 1))
            {
              ++x;
              ++y;
              key1 = ticktick_WPF.Util.ListDiff.ListDiff.GetFootprint(count1 - x, count2 - y);
              if (!flag3 && dictionary3.ContainsKey(key1))
                flag2 = true;
              if (flag3)
                dictionary3.Add(key1, index);
            }
            if (dictionary2.ContainsKey(key3))
              dictionary2[key3] = x;
            else
              dictionary2.Add(key3, x);
            vMap2[index].Add(ticktick_WPF.Util.ListDiff.ListDiff.GetFootprint(x, y));
            if (flag2)
            {
              List<Diff<T>> map = ticktick_WPF.Util.ListDiff.ListDiff.DiffPath1<T>((IReadOnlyList<HashSet<long>>) vMap1.GetRange(0, dictionary3[key1] + 1), text1.Substring<T>(0, count1 - x), text2.Substring<T>(0, count2 - y));
              map.AddRange((IEnumerable<Diff<T>>) ticktick_WPF.Util.ListDiff.ListDiff.DiffPath2<T>((IReadOnlyList<HashSet<long>>) vMap2, text1.Substring<T>(count1 - x), text2.Substring<T>(count2 - y)));
              return map;
            }
          }
        }
      }
      return (List<Diff<T>>) null;
    }

    private static List<Diff<T>> DiffPath1<T>(
      IReadOnlyList<HashSet<long>> vMap,
      IReadOnlyList<T> text1,
      IReadOnlyList<T> text2)
    {
      LinkedList<Diff<T>> source = new LinkedList<Diff<T>>();
      int count1 = text1.Count;
      int count2 = text2.Count;
      Operation? nullable1 = new Operation?();
label_16:
      for (int index = vMap.Count - 2; index >= 0; --index)
      {
        Operation? nullable2;
        while (!vMap[index].Contains(ticktick_WPF.Util.ListDiff.ListDiff.GetFootprint(count1 - 1, count2)))
        {
          if (vMap[index].Contains(ticktick_WPF.Util.ListDiff.ListDiff.GetFootprint(count1, count2 - 1)))
          {
            --count2;
            nullable2 = nullable1;
            Operation operation = Operation.Insert;
            if (nullable2.GetValueOrDefault() == operation & nullable2.HasValue)
              source.First<Diff<T>>().Items = (IReadOnlyList<T>) text2.Substring<T>(count2, 1).Concat<T>((IEnumerable<T>) source.First<Diff<T>>().Items).ToList<T>();
            else
              source.AddFirst(new Diff<T>(Operation.Insert, text2.Substring<T>(count2, 1)));
            nullable1 = new Operation?(Operation.Insert);
            goto label_16;
          }
          else
          {
            --count1;
            --count2;
            nullable2 = nullable1;
            Operation operation = Operation.Equal;
            if (nullable2.GetValueOrDefault() == operation & nullable2.HasValue)
              source.First<Diff<T>>().Items = (IReadOnlyList<T>) text1.Substring<T>(count1, 1).Concat<T>((IEnumerable<T>) source.First<Diff<T>>().Items).ToList<T>();
            else
              source.AddFirst(new Diff<T>(Operation.Equal, text1.Substring<T>(count1, 1)));
            nullable1 = new Operation?(Operation.Equal);
          }
        }
        --count1;
        nullable2 = nullable1;
        Operation operation1 = Operation.Delete;
        if (nullable2.GetValueOrDefault() == operation1 & nullable2.HasValue)
          source.First<Diff<T>>().Items = (IReadOnlyList<T>) text1.Substring<T>(count1, 1).Concat<T>((IEnumerable<T>) source.First<Diff<T>>().Items).ToList<T>();
        else
          source.AddFirst(new Diff<T>(Operation.Delete, text1.Substring<T>(count1, 1)));
        nullable1 = new Operation?(Operation.Delete);
      }
      return source.ToList<Diff<T>>();
    }

    private static List<Diff<T>> DiffPath2<T>(
      IReadOnlyList<HashSet<long>> vMap,
      IReadOnlyList<T> text1,
      IReadOnlyList<T> text2)
    {
      LinkedList<Diff<T>> source = new LinkedList<Diff<T>>();
      int count1 = text1.Count;
      int count2 = text2.Count;
      Operation? nullable1 = new Operation?();
label_16:
      for (int index = vMap.Count - 2; index >= 0; --index)
      {
        Operation? nullable2;
        while (!vMap[index].Contains(ticktick_WPF.Util.ListDiff.ListDiff.GetFootprint(count1 - 1, count2)))
        {
          if (vMap[index].Contains(ticktick_WPF.Util.ListDiff.ListDiff.GetFootprint(count1, count2 - 1)))
          {
            --count2;
            nullable2 = nullable1;
            Operation operation = Operation.Insert;
            if (nullable2.GetValueOrDefault() == operation & nullable2.HasValue)
              source.Last<Diff<T>>().Items = (IReadOnlyList<T>) source.Last<Diff<T>>().Items.Concat<T>((IEnumerable<T>) text2.Substring<T>(text2.Count - count2 - 1, 1)).ToList<T>();
            else
              source.AddLast(new Diff<T>(Operation.Insert, text2.Substring<T>(text2.Count - count2 - 1, 1)));
            nullable1 = new Operation?(Operation.Insert);
            goto label_16;
          }
          else
          {
            --count1;
            --count2;
            nullable2 = nullable1;
            Operation operation = Operation.Equal;
            if (nullable2.GetValueOrDefault() == operation & nullable2.HasValue)
              source.Last<Diff<T>>().Items = (IReadOnlyList<T>) source.Last<Diff<T>>().Items.Concat<T>((IEnumerable<T>) text1.Substring<T>(text1.Count - count1 - 1, 1)).ToList<T>();
            else
              source.AddLast(new Diff<T>(Operation.Equal, text1.Substring<T>(text1.Count - count1 - 1, 1)));
            nullable1 = new Operation?(Operation.Equal);
          }
        }
        --count1;
        nullable2 = nullable1;
        Operation operation1 = Operation.Delete;
        if (nullable2.GetValueOrDefault() == operation1 & nullable2.HasValue)
          source.Last<Diff<T>>().Items = (IReadOnlyList<T>) source.Last<Diff<T>>().Items.Concat<T>((IEnumerable<T>) text1.Substring<T>(text1.Count - count1 - 1, 1)).ToList<T>();
        else
          source.AddLast(new Diff<T>(Operation.Delete, text1.Substring<T>(text1.Count - count1 - 1, 1)));
        nullable1 = new Operation?(Operation.Delete);
      }
      return source.ToList<Diff<T>>();
    }

    private static long GetFootprint(int x, int y) => ((long) x << 32) + (long) y;

    private static int GetCommonPrefix<T>(IReadOnlyList<T> text1, IReadOnlyList<T> text2)
    {
      int commonPrefix1 = Math.Min(text1.Count, text2.Count);
      for (int commonPrefix2 = 0; commonPrefix2 < commonPrefix1; ++commonPrefix2)
      {
        if (!text1.EqualsAt<T>(commonPrefix2, text2, commonPrefix2))
          return commonPrefix2;
      }
      return commonPrefix1;
    }

    private static int GetCommonSuffix<T>(IReadOnlyList<T> text1, IReadOnlyList<T> text2)
    {
      int count1 = text1.Count;
      int count2 = text2.Count;
      int commonSuffix = Math.Min(text1.Count, text2.Count);
      for (int index = 1; index <= commonSuffix; ++index)
      {
        if (!text1.EqualsAt<T>(count1 - index, text2, count2 - index))
          return index - 1;
      }
      return commonSuffix;
    }

    private static IReadOnlyList<T>[] GetHalfMatch<T>(
      IReadOnlyList<T> text1,
      IReadOnlyList<T> text2)
    {
      IReadOnlyList<T> longtext = text1.Count > text2.Count ? text1 : text2;
      IReadOnlyList<T> shorttext = text1.Count > text2.Count ? text2 : text1;
      if (longtext.Count < 10 || shorttext.Count < 1)
        return (IReadOnlyList<T>[]) null;
      IReadOnlyList<T>[] halfMatchI1 = ticktick_WPF.Util.ListDiff.ListDiff.GetHalfMatchI<T>(longtext, shorttext, (longtext.Count + 3) / 4);
      IReadOnlyList<T>[] halfMatchI2 = ticktick_WPF.Util.ListDiff.ListDiff.GetHalfMatchI<T>(longtext, shorttext, (longtext.Count + 1) / 2);
      if (halfMatchI1 == null && halfMatchI2 == null)
        return (IReadOnlyList<T>[]) null;
      IReadOnlyList<T>[] halfMatch = halfMatchI2 != null ? (halfMatchI1 != null ? (halfMatchI1[4].Count > halfMatchI2[4].Count ? halfMatchI1 : halfMatchI2) : halfMatchI2) : halfMatchI1;
      if (text1.Count > text2.Count)
        return halfMatch;
      return new IReadOnlyList<T>[5]
      {
        halfMatch[2],
        halfMatch[3],
        halfMatch[0],
        halfMatch[1],
        halfMatch[4]
      };
    }

    private static IReadOnlyList<T>[] GetHalfMatchI<T>(
      IReadOnlyList<T> longtext,
      IReadOnlyList<T> shorttext,
      int i)
    {
      IReadOnlyList<T> other = longtext.Substring<T>(i, longtext.Count / 4);
      int num = -1;
      IReadOnlyList<T> objList1 = (IReadOnlyList<T>) EmptyList<T>.Instance;
      IReadOnlyList<T> objList2 = (IReadOnlyList<T>) EmptyList<T>.Instance;
      IReadOnlyList<T> objList3 = (IReadOnlyList<T>) EmptyList<T>.Instance;
      IReadOnlyList<T> objList4 = (IReadOnlyList<T>) EmptyList<T>.Instance;
      IReadOnlyList<T> objList5 = (IReadOnlyList<T>) EmptyList<T>.Instance;
      while (num < shorttext.Count && (num = shorttext.IndexOf<T>(other, num + 1)) != -1)
      {
        int commonPrefix = ticktick_WPF.Util.ListDiff.ListDiff.GetCommonPrefix<T>(longtext.Substring<T>(i), shorttext.Substring<T>(num));
        int commonSuffix = ticktick_WPF.Util.ListDiff.ListDiff.GetCommonSuffix<T>(longtext.Substring<T>(0, i), shorttext.Substring<T>(0, num));
        if (objList1.Count < commonSuffix + commonPrefix)
        {
          objList1 = (IReadOnlyList<T>) shorttext.Substring<T>(num - commonSuffix, commonSuffix).Concat<T>((IEnumerable<T>) shorttext.Substring<T>(num, commonPrefix)).ToList<T>();
          objList2 = longtext.Substring<T>(0, i - commonSuffix);
          objList3 = longtext.Substring<T>(i + commonPrefix);
          objList4 = shorttext.Substring<T>(0, num - commonSuffix);
          objList5 = shorttext.Substring<T>(num + commonPrefix);
        }
      }
      if (objList1.Count < longtext.Count / 2)
        return (IReadOnlyList<T>[]) null;
      return new IReadOnlyList<T>[5]
      {
        objList2,
        objList3,
        objList4,
        objList5,
        objList1
      };
    }

    private static void CleanupMerge<T>(List<Diff<T>> diffs)
    {
      bool flag;
      do
      {
        diffs.Add(new Diff<T>(Operation.Equal, (IReadOnlyList<T>) EmptyList<T>.Instance));
        int index1 = 0;
        int num1 = 0;
        int num2 = 0;
        IReadOnlyList<T> objList1 = (IReadOnlyList<T>) EmptyList<T>.Instance;
        IReadOnlyList<T> objList2 = (IReadOnlyList<T>) EmptyList<T>.Instance;
        while (index1 < diffs.Count)
        {
          switch (diffs[index1].Operation)
          {
            case Operation.Delete:
              ++num1;
              objList1 = (IReadOnlyList<T>) objList1.Concat<T>((IEnumerable<T>) diffs[index1].Items).ToList<T>();
              ++index1;
              continue;
            case Operation.Insert:
              ++num2;
              objList2 = (IReadOnlyList<T>) objList2.Concat<T>((IEnumerable<T>) diffs[index1].Items).ToList<T>();
              ++index1;
              continue;
            case Operation.Equal:
              if (num1 != 0 || num2 != 0)
              {
                if (num1 != 0 && num2 != 0)
                {
                  int commonPrefix = ticktick_WPF.Util.ListDiff.ListDiff.GetCommonPrefix<T>(objList2, objList1);
                  if (commonPrefix != 0)
                  {
                    if (index1 - num1 - num2 > 0 && diffs[index1 - num1 - num2 - 1].Operation == Operation.Equal)
                    {
                      Diff<T> diff = diffs[index1 - num1 - num2 - 1];
                      diff.Items = (IReadOnlyList<T>) diff.Items.Concat<T>((IEnumerable<T>) objList2.Substring<T>(0, commonPrefix)).ToList<T>();
                    }
                    else
                    {
                      diffs.Insert(0, new Diff<T>(Operation.Equal, objList2.Substring<T>(0, commonPrefix)));
                      ++index1;
                    }
                    objList2 = objList2.Substring<T>(commonPrefix);
                    objList1 = objList1.Substring<T>(commonPrefix);
                  }
                  int commonSuffix = ticktick_WPF.Util.ListDiff.ListDiff.GetCommonSuffix<T>(objList2, objList1);
                  if (commonSuffix != 0)
                  {
                    diffs[index1].Items = (IReadOnlyList<T>) objList2.Substring<T>(objList2.Count - commonSuffix).Concat<T>((IEnumerable<T>) diffs[index1].Items).ToList<T>();
                    objList2 = objList2.Substring<T>(0, objList2.Count - commonSuffix);
                    objList1 = objList1.Substring<T>(0, objList1.Count - commonSuffix);
                  }
                }
                if (num1 == 0)
                  diffs.Splice<Diff<T>>(index1 - num1 - num2, num1 + num2, new Diff<T>(Operation.Insert, objList2));
                else if (num2 == 0)
                  diffs.Splice<Diff<T>>(index1 - num1 - num2, num1 + num2, new Diff<T>(Operation.Delete, objList1));
                else
                  diffs.Splice<Diff<T>>(index1 - num1 - num2, num1 + num2, new Diff<T>(Operation.Delete, objList1), new Diff<T>(Operation.Insert, objList2));
                index1 = index1 - num1 - num2 + (num1 != 0 ? 1 : 0) + (num2 != 0 ? 1 : 0) + 1;
              }
              else if (index1 != 0 && diffs[index1 - 1].Operation == Operation.Equal)
              {
                diffs[index1 - 1].Items = (IReadOnlyList<T>) diffs[index1 - 1].Items.Concat<T>((IEnumerable<T>) diffs[index1].Items).ToList<T>();
                diffs.RemoveAt(index1);
              }
              else
                ++index1;
              num2 = 0;
              num1 = 0;
              objList1 = (IReadOnlyList<T>) EmptyList<T>.Instance;
              objList2 = (IReadOnlyList<T>) EmptyList<T>.Instance;
              continue;
            default:
              continue;
          }
        }
        if (diffs[diffs.Count - 1].Items.Count == 0)
          diffs.RemoveAt(diffs.Count - 1);
        flag = false;
        for (int index2 = 1; index2 < diffs.Count - 1; ++index2)
        {
          if (diffs[index2 - 1].Operation == Operation.Equal && diffs[index2 + 1].Operation == Operation.Equal)
          {
            if (diffs[index2].Items.EndsWith<T>(diffs[index2 - 1].Items))
            {
              diffs[index2].Items = (IReadOnlyList<T>) diffs[index2 - 1].Items.Concat<T>((IEnumerable<T>) diffs[index2].Items.Substring<T>(0, diffs[index2].Items.Count - diffs[index2 - 1].Items.Count)).ToList<T>();
              diffs[index2 + 1].Items = (IReadOnlyList<T>) diffs[index2 - 1].Items.Concat<T>((IEnumerable<T>) diffs[index2 + 1].Items).ToList<T>();
              diffs.Splice<Diff<T>>(index2 - 1, 1);
              flag = true;
            }
            else if (diffs[index2].Items.StartsWith<T>(diffs[index2 + 1].Items))
            {
              diffs[index2 - 1].Items = (IReadOnlyList<T>) diffs[index2 - 1].Items.Concat<T>((IEnumerable<T>) diffs[index2 + 1].Items).ToList<T>();
              diffs[index2].Items = (IReadOnlyList<T>) diffs[index2].Items.Substring<T>(diffs[index2 + 1].Items.Count).Concat<T>((IEnumerable<T>) diffs[index2 + 1].Items).ToList<T>();
              diffs.Splice<Diff<T>>(index2 + 1, 1);
              flag = true;
            }
          }
        }
      }
      while (flag);
    }
  }
}
