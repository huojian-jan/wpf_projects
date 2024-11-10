// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.GridGeoAssembler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public static class GridGeoAssembler
  {
    private const int MaxDepth = 256;
    private static bool[,] _matrix;
    public static int TaskBarHeight = 18;

    public static IEnumerable<CalendarDisplayViewModel> AssemblyMultiDayEvents(
      List<CalendarDisplayModel> tasks,
      DateTime startDate,
      bool showWeekend)
    {
      List<CalendarDisplayViewModel> models = new List<CalendarDisplayViewModel>();
      Dictionary<int, Dictionary<int, bool>> matrixDict = new Dictionary<int, Dictionary<int, bool>>();
      if (tasks != null && tasks.Any<CalendarDisplayModel>())
      {
        List<CalendarDisplayModel> tasks1 = new List<CalendarDisplayModel>();
        List<CalendarDisplayModel> points = new List<CalendarDisplayModel>();
        foreach (CalendarDisplayModel task in tasks)
        {
          if (GridGeoAssembler.IsValidSpanModel(task, false))
            tasks1.Add(task);
          else
            points.Add(task);
        }
        GridGeoAssembler.AssemblyMultiDaySpanEvents(tasks1, models, matrixDict, startDate, showWeekend);
        GridGeoAssembler.AssemblyMultiDayPointEvents(points, models, matrixDict, startDate, showWeekend);
      }
      return (IEnumerable<CalendarDisplayViewModel>) models;
    }

    private static void AssemblyMultiDaySpanEvents(
      List<CalendarDisplayModel> tasks,
      List<CalendarDisplayViewModel> models,
      Dictionary<int, Dictionary<int, bool>> matrixDict,
      DateTime startDate,
      bool showWeekend)
    {
      if (tasks == null || !tasks.Any<CalendarDisplayModel>())
        return;
      tasks.Sort(new Comparison<CalendarDisplayModel>(GridGeoAssembler.CompareSpanTasks));
      foreach (CalendarDisplayModel task in tasks)
      {
        int spanLength = GridGeoAssembler.GetSpanLength(startDate, task, false);
        if (spanLength >= 1)
        {
          DateTime? nullable = task.DisplayStartDate;
          if (nullable.HasValue)
          {
            nullable = task.DisplayDueDate;
            if (nullable.HasValue)
            {
              nullable = task.DisplayStartDate;
              DateTime date1 = nullable.Value;
              int totalDays = (int) (date1.Date - startDate.Date).TotalDays;
              if (!showWeekend)
              {
                nullable = task.DisplayDueDate;
                DateTime end = nullable ?? task.DisplayStartDate.Value;
                DateTime date2 = end.Date;
                nullable = task.DisplayStartDate;
                date1 = nullable.Value;
                DateTime date3 = date1.Date;
                if (date2 > date3 && (end - end.Date).TotalHours <= 5.0)
                {
                  date1 = end.Date;
                  end = date1.AddDays(-1.0);
                }
                nullable = task.DisplayStartDate;
                int weekendsCountInSpan1 = DateUtils.GetWeekendsCountInSpan(nullable.Value, end, true);
                spanLength -= weekendsCountInSpan1;
                if (spanLength > 0)
                {
                  nullable = task.DisplayStartDate;
                  int weekendsCountInSpan2 = DateUtils.GetWeekendsCountInSpan(nullable.Value, startDate, false);
                  totalDays += weekendsCountInSpan2;
                }
                else
                  continue;
              }
              for (int row = 0; row < 10000; ++row)
              {
                if (CheckRowEnoughSpace(row, totalDays, spanLength))
                {
                  SetOccupied(row, totalDays, spanLength);
                  CalendarDisplayViewModel displayViewModel = CalendarDisplayViewModel.Build(task);
                  displayViewModel.Row = row;
                  displayViewModel.Column = totalDays;
                  displayViewModel.ColumnSpan = spanLength;
                  models.Add(displayViewModel);
                  break;
                }
              }
            }
          }
        }
      }

      void SetOccupied(int row, int offset, int length)
      {
        if (!matrixDict.ContainsKey(row))
          matrixDict[row] = new Dictionary<int, bool>();
        Dictionary<int, bool> dictionary = matrixDict[row];
        for (int index = 0; index < length; ++index)
          dictionary[index + offset] = true;
      }

      bool CheckRowEnoughSpace(int row, int offset, int length)
      {
        if (!matrixDict.ContainsKey(row))
          return true;
        Dictionary<int, bool> dictionary = matrixDict[row];
        for (int index = 0; index < length; ++index)
        {
          if (row >= 256 || dictionary.ContainsKey(offset + index))
            return false;
        }
        return true;
      }
    }

    private static void AssemblyMultiDayPointEvents(
      List<CalendarDisplayModel> points,
      List<CalendarDisplayViewModel> models,
      Dictionary<int, Dictionary<int, bool>> matrixDict,
      DateTime startDate,
      bool showWeekend)
    {
      if (points == null || !points.Any<CalendarDisplayModel>())
        return;
      points.Sort(new Comparison<CalendarDisplayModel>(GridGeoAssembler.CompareTasks));
      foreach (CalendarDisplayModel point in points)
      {
        DateTime? nullable = point.DisplayStartDate;
        if (!nullable.HasValue)
        {
          nullable = point.CompletedTime;
          if (!nullable.HasValue)
            continue;
        }
        nullable = point.DisplayStartDate;
        DateTime dateTime = nullable ?? point.CompletedTime.Value;
        if (showWeekend || !DateUtils.IsWeekEnds(dateTime))
        {
          int key1 = 0;
          nullable = point.DisplayStartDate;
          if (nullable.HasValue)
          {
            nullable = point.DisplayStartDate;
            key1 = (int) (nullable.Value.Date - startDate.Date).TotalDays;
          }
          else
          {
            nullable = point.CompletedTime;
            if (nullable.HasValue)
            {
              nullable = point.CompletedTime;
              key1 = (int) (nullable.Value.Date - startDate.Date).TotalDays;
            }
          }
          if (!showWeekend)
          {
            int weekendsCountInSpan = DateUtils.GetWeekendsCountInSpan(dateTime, startDate, false);
            key1 += weekendsCountInSpan;
          }
          for (int key2 = 0; key2 < 10000; ++key2)
          {
            bool flag = false;
            if (!matrixDict.ContainsKey(key2))
            {
              matrixDict[key2] = new Dictionary<int, bool>()
              {
                {
                  key1,
                  true
                }
              };
              flag = true;
            }
            else
            {
              Dictionary<int, bool> dictionary = matrixDict[key2];
              if (!dictionary.ContainsKey(key1))
              {
                dictionary[key1] = true;
                flag = true;
              }
            }
            if (flag)
            {
              CalendarDisplayViewModel displayViewModel = CalendarDisplayViewModel.Build(point);
              displayViewModel.Row = key2;
              displayViewModel.Column = key1;
              displayViewModel.ColumnSpan = 1;
              models.Add(displayViewModel);
              break;
            }
          }
        }
      }
    }

    public static IEnumerable<CalendarDisplayViewModel> AssemblyEvents(
      List<CalendarDisplayModel> tasks,
      DateTime startDate)
    {
      List<CalendarDisplayViewModel> models = new List<CalendarDisplayViewModel>();
      GridGeoAssembler._matrix = new bool[7, 256];
      if (tasks != null && tasks.Any<CalendarDisplayModel>())
      {
        List<CalendarDisplayModel> tasks1 = new List<CalendarDisplayModel>();
        List<CalendarDisplayModel> points = new List<CalendarDisplayModel>();
        foreach (CalendarDisplayModel task in tasks)
        {
          if (GridGeoAssembler.IsValidSpanModel(task, false))
            tasks1.Add(task);
          else
            points.Add(task);
        }
        GridGeoAssembler.AssemblySpanEvents(tasks1, models, startDate);
        GridGeoAssembler.AssemblyPointEvents(points, models, startDate);
      }
      return (IEnumerable<CalendarDisplayViewModel>) models;
    }

    private static void AssemblyPointEvents(
      List<CalendarDisplayModel> points,
      List<CalendarDisplayViewModel> models,
      DateTime startDate)
    {
      if (points == null || !points.Any<CalendarDisplayModel>())
        return;
      points.Sort(new Comparison<CalendarDisplayModel>(GridGeoAssembler.CompareTasks));
      foreach (CalendarDisplayModel point in points)
      {
        DateTime? nullable = point.DisplayStartDate;
        if (!nullable.HasValue)
        {
          nullable = point.CompletedTime;
          if (!nullable.HasValue)
            continue;
        }
        if (!GridGeoAssembler.IsValidSpanModel(point, false))
        {
          int index1 = 0;
          nullable = point.DisplayStartDate;
          if (nullable.HasValue)
          {
            nullable = point.DisplayStartDate;
            index1 = GridGeoAssembler.GetStartOffset(nullable.Value, startDate);
          }
          else
          {
            nullable = point.CompletedTime;
            if (nullable.HasValue)
            {
              nullable = point.CompletedTime;
              index1 = GridGeoAssembler.GetStartOffset(nullable.Value, startDate);
            }
          }
          if (index1 >= 0 && index1 < 7)
          {
            for (int index2 = 0; index2 < GridGeoAssembler._matrix.Length; ++index2)
            {
              if (index2 < 256 && !GridGeoAssembler._matrix[index1, index2])
              {
                GridGeoAssembler._matrix[index1, index2] = true;
                CalendarDisplayViewModel displayViewModel = CalendarDisplayViewModel.Build(point);
                displayViewModel.Row = index2;
                displayViewModel.Column = index1;
                displayViewModel.ColumnSpan = 1;
                models.Add(displayViewModel);
                break;
              }
            }
          }
        }
      }
    }

    private static void AssemblySpanEvents(
      List<CalendarDisplayModel> tasks,
      List<CalendarDisplayViewModel> models,
      DateTime startDate)
    {
      if (tasks == null || !tasks.Any<CalendarDisplayModel>())
        return;
      tasks.Sort(new Comparison<CalendarDisplayModel>(GridGeoAssembler.CompareSpanTasks));
      foreach (CalendarDisplayModel task in tasks)
      {
        if (GridGeoAssembler.IsValidSpanModel(task, false))
        {
          int spanLength = GridGeoAssembler.GetSpanLength(startDate, task);
          if (spanLength >= 1 && task.DisplayStartDate.HasValue && task.DisplayDueDate.HasValue)
          {
            int offset = GridGeoAssembler.GetStartOffset(task.DisplayStartDate.Value, startDate);
            if (offset < 0)
              offset = 0;
            for (int row = 0; row < GridGeoAssembler._matrix.Length; ++row)
            {
              if (GridGeoAssembler.CheckRowHasEnoughSpace(row, offset, spanLength))
              {
                GridGeoAssembler.SetRowOccupied(row, offset, spanLength);
                CalendarDisplayViewModel displayViewModel = CalendarDisplayViewModel.Build(task);
                displayViewModel.Row = row;
                displayViewModel.Column = offset;
                displayViewModel.ColumnSpan = spanLength;
                models.Add(displayViewModel);
                break;
              }
            }
          }
        }
      }
    }

    public static bool IsValidSpanModelInDay(CalendarDisplayModel model, DateTime date)
    {
      return !model.IsAllDay.HasValue || model.IsAllDay.Value || !model.DisplayStartDate.HasValue || !model.DisplayDueDate.HasValue || !(model.DisplayStartDate.Value.Date != date.Date) || !(model.DisplayDueDate.Value.Date == date.Date) || (model.DisplayDueDate.Value - date.Date).TotalHours > 5.0;
    }

    public static bool IsValidSpanModel(CalendarDisplayModel model, bool weekMode)
    {
      if (weekMode)
      {
        if (model.DisplayStartDate.HasValue && model.DisplayDueDate.HasValue && (model.IsAllDay.HasValue && model.IsAllDay.Value && model.DisplayStartDate.Value.Date != model.DisplayDueDate.Value.Date || (model.DisplayDueDate.Value - model.DisplayStartDate.Value).TotalHours > 24.0) || !model.DisplayStartDate.HasValue && model.CompletedTime.HasValue)
          return true;
      }
      else if (model.DisplayStartDate.HasValue && model.DisplayDueDate.HasValue)
      {
        int totalDays = (int) (model.DisplayDueDate.Value.Date - model.DisplayStartDate.Value.Date).TotalDays;
        if (totalDays > 1 || totalDays == 1 && (model.DisplayDueDate.Value - model.DisplayDueDate.Value.Date).TotalHours > 5.0)
          return true;
      }
      return false;
    }

    private static void SetRowOccupied(int row, int offset, int length)
    {
      for (int index = 0; index < length; ++index)
      {
        if (offset + index < 7)
          GridGeoAssembler._matrix[index + offset, row] = true;
      }
    }

    private static bool CheckRowHasEnoughSpace(int row, int offset, int length)
    {
      if (offset < 0)
        return false;
      for (int index = 0; index < length; ++index)
      {
        if (offset + index < 7 && (offset + index >= 7 || row >= 256 || GridGeoAssembler._matrix[offset + index, row]))
          return false;
      }
      return true;
    }

    private static int GetStartOffset(DateTime modelStart, DateTime start)
    {
      return (int) (modelStart.Date - start.Date).TotalDays;
    }

    private static int GetSpanLength(
      DateTime startDate,
      CalendarDisplayModel model,
      bool checkDate = true)
    {
      if (model.DisplayStartDate.HasValue && model.DisplayDueDate.HasValue)
      {
        DateTime dateTime1 = model.DisplayStartDate.Value.Date;
        DateTime dateTime2 = model.DisplayDueDate.Value.Date;
        bool? isAllDay = model.IsAllDay;
        DateTime? displayDueDate;
        if (((int) isAllDay ?? 1) == 0)
        {
          DateTime dateTime3 = model.DisplayDueDate.Value;
          displayDueDate = model.DisplayDueDate;
          DateTime date = displayDueDate.Value.Date;
          if ((dateTime3 - date).TotalHours <= 5.0)
            dateTime2 = dateTime2.AddDays(-1.0);
        }
        isAllDay = model.IsAllDay;
        int num;
        if (isAllDay.HasValue)
        {
          isAllDay = model.IsAllDay;
          num = isAllDay.Value ? 1 : 0;
        }
        else
          num = 0;
        if (num == 0)
        {
          displayDueDate = model.DisplayDueDate;
          if (displayDueDate.Value > dateTime2)
            dateTime2 = dateTime2.AddDays(1.0);
        }
        if (dateTime1 < startDate & checkDate)
          dateTime1 = startDate;
        if (dateTime2 > startDate.AddDays(7.0) & checkDate)
          dateTime2 = startDate.AddDays(7.0);
        return (int) (dateTime2 - dateTime1).TotalDays;
      }
      return model.DisplayStartDate.HasValue && model.IsAllDay.HasValue && model.IsAllDay.Value || !model.DisplayStartDate.HasValue && model.CompletedTime.HasValue ? 1 : 0;
    }

    public static int CompareSpanTasks(CalendarDisplayModel left, CalendarDisplayModel right)
    {
      if (left.Type == DisplayType.Habit || right.Type == DisplayType.Habit)
      {
        int num = left.Type.CompareTo((object) right.Type);
        return num != 0 ? num : left.SortOrder.CompareTo(right.SortOrder);
      }
      DateTime? nullable = left.DisplayStartDate;
      if (nullable.HasValue)
      {
        nullable = right.DisplayStartDate;
        if (nullable.HasValue)
        {
          bool? isAllDay1 = left.IsAllDay;
          if (isAllDay1.HasValue)
          {
            isAllDay1 = right.IsAllDay;
            if (isAllDay1.HasValue)
            {
              isAllDay1 = left.IsAllDay;
              bool? isAllDay2 = right.IsAllDay;
              if (!(isAllDay1.GetValueOrDefault() == isAllDay2.GetValueOrDefault() & isAllDay1.HasValue == isAllDay2.HasValue))
              {
                bool? isAllDay3 = left.IsAllDay;
                if (isAllDay3.Value)
                {
                  isAllDay3 = right.IsAllDay;
                  if (!isAllDay3.Value)
                    return 1;
                }
                return -1;
              }
            }
          }
          nullable = left.DisplayStartDate;
          if (nullable.HasValue)
          {
            nullable = right.DisplayStartDate;
            if (nullable.HasValue)
            {
              nullable = left.DisplayStartDate;
              DateTime dateTime1 = nullable.Value;
              nullable = right.DisplayStartDate;
              DateTime dateTime2 = nullable.Value;
              if (dateTime1 != dateTime2)
              {
                nullable = left.DisplayStartDate;
                DateTime dateTime3 = nullable.Value;
                ref DateTime local = ref dateTime3;
                nullable = right.DisplayStartDate;
                DateTime dateTime4 = nullable.Value;
                return local.CompareTo(dateTime4);
              }
            }
          }
          nullable = left.DisplayDueDate;
          if (nullable.HasValue)
          {
            nullable = right.DisplayDueDate;
            if (nullable.HasValue)
              goto label_25;
          }
          nullable = left.DisplayDueDate;
          if (!nullable.HasValue)
          {
            nullable = right.DisplayDueDate;
            if (nullable.HasValue)
              return 1;
          }
          nullable = right.DisplayDueDate;
          if (!nullable.HasValue)
          {
            nullable = left.DisplayDueDate;
            if (nullable.HasValue)
              return -1;
          }
label_25:
          nullable = left.DisplayDueDate;
          if (nullable.HasValue)
          {
            nullable = right.DisplayDueDate;
            if (nullable.HasValue)
            {
              nullable = left.DisplayDueDate;
              DateTime dateTime5 = nullable.Value;
              nullable = right.DisplayDueDate;
              DateTime dateTime6 = nullable.Value;
              if (dateTime5 != dateTime6)
              {
                nullable = right.DisplayDueDate;
                DateTime dateTime7 = nullable.Value;
                ref DateTime local = ref dateTime7;
                nullable = left.DisplayDueDate;
                DateTime dateTime8 = nullable.Value;
                return local.CompareTo(dateTime8);
              }
            }
          }
          if (left.Priority != right.Priority)
            return right.Priority.CompareTo(left.Priority);
          if (left.Type != right.Type)
            return left.Type < right.Type ? 1 : -1;
        }
      }
      return left.SortOrder != right.SortOrder ? left.SortOrder.CompareTo(right.SortOrder) : string.Compare(left.Title, right.Title, StringComparison.Ordinal);
    }

    public static int CompareTasks(CalendarDisplayModel left, CalendarDisplayModel right)
    {
      if (left.Type == DisplayType.Pomo && right.Type != DisplayType.Pomo)
        return 1;
      if (left.Type != DisplayType.Pomo && right.Type == DisplayType.Pomo || left.Status == 0 && right.Status != 0)
        return -1;
      if (left.Status != 0 && right.Status == 0 || left.Type == DisplayType.Habit && right.Type != DisplayType.Habit)
        return 1;
      if (left.Type != DisplayType.Habit && right.Type == DisplayType.Habit)
        return -1;
      if (left.Type == DisplayType.Habit && right.Type == DisplayType.Habit)
        return left.SortOrder.CompareTo(right.SortOrder);
      DateTime? nullable;
      if (left.Status != 0 && right.Status != 0)
      {
        nullable = right.CompletedTime;
        if (nullable.HasValue)
        {
          nullable = left.CompletedTime;
          if (nullable.HasValue)
          {
            nullable = right.CompletedTime;
            DateTime dateTime1 = nullable.Value;
            ref DateTime local = ref dateTime1;
            nullable = left.CompletedTime;
            DateTime dateTime2 = nullable.Value;
            return local.CompareTo(dateTime2);
          }
        }
      }
      nullable = left.DisplayStartDate;
      if (nullable.HasValue)
      {
        nullable = right.DisplayStartDate;
        if (nullable.HasValue)
        {
          bool? isAllDay = left.IsAllDay;
          if (isAllDay.HasValue)
          {
            isAllDay = left.IsAllDay;
            if (isAllDay.Value)
            {
              isAllDay = right.IsAllDay;
              if (isAllDay.HasValue)
              {
                isAllDay = right.IsAllDay;
                if (isAllDay.Value)
                  goto label_21;
              }
              return 1;
            }
          }
label_21:
          isAllDay = right.IsAllDay;
          if (isAllDay.HasValue)
          {
            isAllDay = right.IsAllDay;
            if (isAllDay.Value)
            {
              isAllDay = left.IsAllDay;
              if (isAllDay.HasValue)
              {
                isAllDay = left.IsAllDay;
                if (isAllDay.Value)
                  goto label_26;
              }
              return -1;
            }
          }
label_26:
          nullable = left.DisplayStartDate;
          DateTime dateTime3 = nullable.Value;
          nullable = right.DisplayStartDate;
          DateTime dateTime4 = nullable.Value;
          if (dateTime3 < dateTime4)
            return -1;
          nullable = left.DisplayStartDate;
          DateTime dateTime5 = nullable.Value;
          nullable = right.DisplayStartDate;
          DateTime dateTime6 = nullable.Value;
          if (dateTime5 > dateTime6)
            return 1;
          nullable = left.DisplayDueDate;
          if (nullable.HasValue)
          {
            nullable = right.DisplayDueDate;
            if (!nullable.HasValue)
              return -1;
          }
          nullable = left.DisplayDueDate;
          if (!nullable.HasValue)
          {
            nullable = right.DisplayDueDate;
            if (nullable.HasValue)
              return 1;
          }
          nullable = left.DisplayDueDate;
          if (nullable.HasValue)
          {
            nullable = right.DisplayDueDate;
            if (nullable.HasValue)
            {
              nullable = right.DisplayDueDate;
              DateTime dateTime7 = nullable.Value;
              ref DateTime local = ref dateTime7;
              nullable = left.DisplayDueDate;
              DateTime dateTime8 = nullable.Value;
              return local.CompareTo(dateTime8);
            }
          }
          if (left.Type != right.Type)
            return left.Type == DisplayType.Task && right.Type != DisplayType.Task ? -1 : 1;
          if (left.Priority != right.Priority)
            return left.Priority < right.Priority ? 1 : -1;
        }
      }
      return string.Compare(left.Title, right.Title, StringComparison.Ordinal);
    }
  }
}
