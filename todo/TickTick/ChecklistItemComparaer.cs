// Decompiled with JetBrains decompiler
// Type: ChecklistItemComparaer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using ticktick_WPF.ViewModels;

#nullable disable
public class ChecklistItemComparaer : IComparer<CheckItemViewModel>
{
  public int Compare(CheckItemViewModel x, CheckItemViewModel y)
  {
    if (x != null && y != null)
    {
      int num1 = ChecklistItemComparaer.CompareStatus(x.Status, y.Status);
      if (num1 != 0)
        return num1;
      if (x.Status == 0)
      {
        int num2 = ChecklistItemComparaer.CompareSortOrder(x.SortOrder, y.SortOrder);
        if (num2 != 0)
          return num2;
      }
      int num3 = ChecklistItemComparaer.CompareCompletedTime(x.CompletedTime, y.CompletedTime);
      if (num3 != 0)
        return num3;
      int num4 = ChecklistItemComparaer.CompareSortOrder(x.SortOrder, y.SortOrder);
      if (num4 != 0)
        return num4;
    }
    return 0;
  }

  private static int CompareCompletedTime(DateTime? x, DateTime? y)
  {
    if (x.HasValue && y.HasValue)
    {
      DateTime? nullable1 = x;
      DateTime? nullable2 = y;
      if ((nullable1.HasValue & nullable2.HasValue ? (nullable1.GetValueOrDefault() > nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
        return 1;
      nullable2 = x;
      DateTime? nullable3 = y;
      if ((nullable2.HasValue & nullable3.HasValue ? (nullable2.GetValueOrDefault() < nullable3.GetValueOrDefault() ? 1 : 0) : 0) != 0)
        return -1;
    }
    return 0;
  }

  private static int CompareStatus(int x, int y)
  {
    if (x == y)
      return 0;
    return x == 0 ? -1 : 1;
  }

  private static int CompareSortOrder(long x, long y)
  {
    if (x > y)
      return 1;
    return x < y ? -1 : 0;
  }
}
