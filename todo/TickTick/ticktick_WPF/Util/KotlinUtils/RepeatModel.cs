// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.KotlinUtils.RepeatModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Util.KotlinUtils
{
  public class RepeatModel
  {
    public List<(DateTime, DateTime)> Ranges = new List<(DateTime, DateTime)>();
    public HashSet<DateTime> RepeatDates = new HashSet<DateTime>();

    public void AddRange(DateTime start, DateTime end, List<DateTime> dates)
    {
      List<(DateTime, DateTime)> list = this.Ranges.ToList<(DateTime, DateTime)>();
      list.Add((start, end));
      list.Sort((Comparison<(DateTime, DateTime)>) ((a, b) => a.Item1 != b.Item1 ? a.Item1.CompareTo(b.Item1) : b.Item2.CompareTo(a.Item2)));
      List<(DateTime, DateTime)> valueTupleList = new List<(DateTime, DateTime)>();
      (DateTime, DateTime) valueTuple = list[0];
      foreach ((DateTime, DateTime) tuple in list)
      {
        if (tuple.Item1 <= valueTuple.Item2 && tuple.Item2 > valueTuple.Item2)
          valueTuple.Item2 = tuple.Item2;
        else if (tuple.Item1 > valueTuple.Item2)
        {
          valueTupleList.Add(valueTuple);
          valueTuple = tuple;
        }
      }
      valueTupleList.Add(valueTuple);
      this.Ranges = valueTupleList;
      foreach (DateTime date in dates)
        this.RepeatDates.Add(date);
    }

    public (DateTime, DateTime) GetNotExistRange(DateTime start, DateTime end)
    {
      (DateTime, DateTime) notExistRange = (start, end);
      foreach ((DateTime, DateTime) range in this.Ranges)
      {
        if (range.Item1 <= notExistRange.Item1 && range.Item2 >= notExistRange.Item2)
          notExistRange.Item2 = notExistRange.Item1;
        else if (range.Item1 <= notExistRange.Item1 && range.Item2 <= notExistRange.Item2 && range.Item2 >= notExistRange.Item1)
          notExistRange.Item1 = range.Item2;
        else if (range.Item1 >= notExistRange.Item1 && notExistRange.Item2 >= range.Item1 && range.Item2 >= notExistRange.Item2)
          notExistRange.Item2 = range.Item1;
      }
      return notExistRange;
    }

    public List<DateTime> GetRepeatDates(DateTime spanStart, DateTime spanEnd)
    {
      return this.RepeatDates.ToList<DateTime>().Where<DateTime>((Func<DateTime, bool>) (d => d >= spanStart && d <= spanEnd)).ToList<DateTime>();
    }
  }
}
