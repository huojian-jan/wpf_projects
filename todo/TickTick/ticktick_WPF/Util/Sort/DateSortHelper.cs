// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Sort.DateSortHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Util.Sort
{
  public static class DateSortHelper
  {
    public static int CompareTaskByDate(
      TaskBaseViewModel left,
      TaskBaseViewModel right,
      bool outDateFirst)
    {
      if (left.StartDate.HasValue && right.StartDate.HasValue)
      {
        DateTime valueOrDefault;
        if (left.DueDate.HasValue && left.DueDate.Value < DateTime.Today || !left.DueDate.HasValue && left.StartDate.Value < DateTime.Today)
        {
          DateTime? dueDate = right.DueDate;
          ref DateTime? local1 = ref dueDate;
          DateTime? nullable1;
          if (!local1.HasValue)
          {
            nullable1 = new DateTime?();
          }
          else
          {
            valueOrDefault = local1.GetValueOrDefault();
            nullable1 = new DateTime?(valueOrDefault.Date);
          }
          DateTime? nullable2 = nullable1;
          DateTime today1 = DateTime.Today;
          if ((nullable2.HasValue ? (nullable2.GetValueOrDefault() >= today1 ? 1 : 0) : 0) == 0)
          {
            DateTime? startDate = right.StartDate;
            ref DateTime? local2 = ref startDate;
            DateTime? nullable3;
            if (!local2.HasValue)
            {
              nullable3 = new DateTime?();
            }
            else
            {
              valueOrDefault = local2.GetValueOrDefault();
              nullable3 = new DateTime?(valueOrDefault.Date);
            }
            DateTime? nullable4 = nullable3;
            DateTime today2 = DateTime.Today;
            if ((nullable4.HasValue ? (nullable4.GetValueOrDefault() >= today2 ? 1 : 0) : 0) == 0)
              goto label_13;
          }
          return !outDateFirst ? 1 : -1;
        }
label_13:
        if (right.DueDate.HasValue && right.DueDate.Value < DateTime.Today || !right.DueDate.HasValue && right.StartDate.Value < DateTime.Today)
        {
          DateTime? dueDate = left.DueDate;
          ref DateTime? local3 = ref dueDate;
          DateTime? nullable5;
          if (!local3.HasValue)
          {
            nullable5 = new DateTime?();
          }
          else
          {
            valueOrDefault = local3.GetValueOrDefault();
            nullable5 = new DateTime?(valueOrDefault.Date);
          }
          DateTime? nullable6 = nullable5;
          DateTime today3 = DateTime.Today;
          if ((nullable6.HasValue ? (nullable6.GetValueOrDefault() >= today3 ? 1 : 0) : 0) == 0)
          {
            DateTime? startDate = left.StartDate;
            ref DateTime? local4 = ref startDate;
            DateTime? nullable7;
            if (!local4.HasValue)
            {
              nullable7 = new DateTime?();
            }
            else
            {
              valueOrDefault = local4.GetValueOrDefault();
              nullable7 = new DateTime?(valueOrDefault.Date);
            }
            DateTime? nullable8 = nullable7;
            DateTime today4 = DateTime.Today;
            if ((nullable8.HasValue ? (nullable8.GetValueOrDefault() >= today4 ? 1 : 0) : 0) == 0)
              goto label_25;
          }
          return !outDateFirst ? -1 : 1;
        }
label_25:
        if ((right.DueDate.HasValue && right.DueDate.Value < DateTime.Today || !right.DueDate.HasValue && right.StartDate.Value < DateTime.Today) && (left.DueDate.HasValue && left.DueDate.Value < DateTime.Today || !left.DueDate.HasValue && left.StartDate.Value < DateTime.Today))
          return DateSortHelper.CompareOutDate(left, right);
        DateTime? dueDate1 = right.DueDate;
        ref DateTime? local5 = ref dueDate1;
        DateTime? nullable9;
        if (!local5.HasValue)
        {
          nullable9 = new DateTime?();
        }
        else
        {
          valueOrDefault = local5.GetValueOrDefault();
          nullable9 = new DateTime?(valueOrDefault.Date);
        }
        DateTime? nullable10 = nullable9;
        DateTime today5 = DateTime.Today;
        if ((nullable10.HasValue ? (nullable10.GetValueOrDefault() >= today5 ? 1 : 0) : 0) == 0)
        {
          DateTime? startDate = right.StartDate;
          ref DateTime? local6 = ref startDate;
          DateTime? nullable11;
          if (!local6.HasValue)
          {
            nullable11 = new DateTime?();
          }
          else
          {
            valueOrDefault = local6.GetValueOrDefault();
            nullable11 = new DateTime?(valueOrDefault.Date);
          }
          DateTime? nullable12 = nullable11;
          DateTime today6 = DateTime.Today;
          if ((nullable12.HasValue ? (nullable12.GetValueOrDefault() >= today6 ? 1 : 0) : 0) == 0)
            goto label_44;
        }
        DateTime? dueDate2 = left.DueDate;
        ref DateTime? local7 = ref dueDate2;
        DateTime? nullable13;
        if (!local7.HasValue)
        {
          nullable13 = new DateTime?();
        }
        else
        {
          valueOrDefault = local7.GetValueOrDefault();
          nullable13 = new DateTime?(valueOrDefault.Date);
        }
        DateTime? nullable14 = nullable13;
        DateTime today7 = DateTime.Today;
        if ((nullable14.HasValue ? (nullable14.GetValueOrDefault() >= today7 ? 1 : 0) : 0) == 0)
        {
          DateTime? startDate = left.StartDate;
          ref DateTime? local8 = ref startDate;
          DateTime? nullable15;
          if (!local8.HasValue)
          {
            nullable15 = new DateTime?();
          }
          else
          {
            valueOrDefault = local8.GetValueOrDefault();
            nullable15 = new DateTime?(valueOrDefault.Date);
          }
          DateTime? nullable16 = nullable15;
          DateTime today8 = DateTime.Today;
          if ((nullable16.HasValue ? (nullable16.GetValueOrDefault() >= today8 ? 1 : 0) : 0) == 0)
            goto label_44;
        }
        return DateSortHelper.CompareDate(left, right);
      }
label_44:
      return 0;
    }

    public static int CompareDate(
      TaskBaseViewModel left,
      TaskBaseViewModel right,
      bool ignoreDuration = false)
    {
      if (left.StartDate.HasValue && right.StartDate.HasValue)
      {
        DateTime date1 = left.StartDate.Value.Date;
        DateTime date2 = right.StartDate.Value.Date;
        if (date1 != date2)
          return date1.CompareTo(date2);
        bool flag1 = DateSortHelper.CheckAllDay(left);
        bool flag2 = DateSortHelper.CheckAllDay(right);
        if (flag1 != flag2)
          return !flag1 ? -1 : 1;
        if (ignoreDuration)
          return left.StartDate.Value.CompareTo(right.StartDate.Value);
        DateTime dateTime1 = left.StartDate.Value.Date;
        DateTime dateTime2 = right.StartDate.Value.Date;
        if (left.DueDate.HasValue)
        {
          dateTime1 = left.DueDate.Value.Date;
          if (flag1)
            dateTime1 = dateTime1.AddDays(-1.0);
        }
        if (right.DueDate.HasValue)
        {
          dateTime2 = right.DueDate.Value.Date;
          if (flag2)
            dateTime2 = dateTime2.AddDays(-1.0);
        }
        if (dateTime1 != dateTime2)
          return dateTime1.CompareTo(dateTime2);
        int num = DateSortHelper.CompareTimeOrDuration(left, right);
        if (num != 0)
          return num;
      }
      return 0;
    }

    private static int CompareOutDate(TaskBaseViewModel left, TaskBaseViewModel right)
    {
      if (!left.StartDate.HasValue || !right.StartDate.HasValue)
        return 0;
      DateTime dateTime1 = left.StartDate.Value.Date;
      DateTime dateTime2 = right.StartDate.Value.Date;
      bool flag1 = DateSortHelper.CheckAllDay(left);
      bool flag2 = DateSortHelper.CheckAllDay(right);
      if (left.DueDate.HasValue)
      {
        dateTime1 = left.DueDate.Value;
        if (flag1)
          dateTime1 = dateTime1.Date.AddDays(-1.0);
      }
      if (right.DueDate.HasValue)
      {
        dateTime2 = right.DueDate.Value;
        if (flag2)
          dateTime2 = dateTime2.Date.AddDays(-1.0);
      }
      if (dateTime1 != dateTime2)
        return dateTime1.CompareTo(dateTime2);
      DateTime date1 = left.StartDate.Value.Date;
      DateTime date2 = right.StartDate.Value.Date;
      if (date1 != date2)
        return date1.CompareTo(date2);
      if (flag1 == flag2)
        return DateSortHelper.CompareTimeOrDuration(left, right);
      return !flag1 ? -1 : 1;
    }

    private static int CompareTimeOrDuration(TaskBaseViewModel left, TaskBaseViewModel right)
    {
      if (left.StartDate.HasValue && right.StartDate.HasValue)
      {
        DateTime date1 = left.StartDate.Value;
        DateTime date2 = right.StartDate.Value;
        if (left.IsAllDay.HasValue && left.IsAllDay.Value)
          date1 = left.StartDate.Value.Date;
        if (right.IsAllDay.HasValue && right.IsAllDay.Value)
          date2 = right.StartDate.Value.Date;
        if (date1 != date2)
          return date1.CompareTo(date2);
        int num1 = 0;
        int num2 = 0;
        DateTime? nullable;
        if (left.DueDate.HasValue)
        {
          DateTime dateTime1 = left.DueDate.Value;
          nullable = left.StartDate;
          DateTime dateTime2 = nullable.Value;
          num1 = (int) (dateTime1 - dateTime2).TotalMinutes;
        }
        nullable = right.DueDate;
        if (nullable.HasValue)
        {
          nullable = right.DueDate;
          DateTime dateTime3 = nullable.Value;
          nullable = right.StartDate;
          DateTime dateTime4 = nullable.Value;
          num2 = (int) (dateTime3 - dateTime4).TotalMinutes;
        }
        if (num1 != num2)
          return num1.CompareTo(num2);
      }
      return 0;
    }

    private static bool CheckAllDay(TaskBaseViewModel model)
    {
      return model.IsAllDay.HasValue && model.IsAllDay.Value;
    }
  }
}
