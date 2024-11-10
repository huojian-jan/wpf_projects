// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarGeoHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ticktick_WPF.Dal;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarGeoHelper
  {
    private static bool _topFolded = true;
    public const double CellMargin = 1.0;
    public const double TimelineWidth = 60.0;
    public const double CellDragMargin = 16.0;
    public const double SameLineCellDiff = 32.0;
    public const double CellRightMargin = 9.0;
    public const double CellLeftMargin = 2.0;
    public static readonly List<double> HourZoomHeight = new List<double>()
    {
      44.0,
      48.0,
      56.0,
      64.0,
      80.0,
      96.0,
      112.0,
      128.0
    };
    private static int _startHour = LocalSettings.Settings.CollapsedStart;
    private static int _endHour = LocalSettings.Settings.CollapsedEnd;

    public static event EventHandler TopFoldedChanged;

    public static event EventHandler<bool> ExpandTop;

    public static double MinHeight => 11.0;

    public static double MinPointHeight => 16.0;

    public static bool TopFolded
    {
      get => CalendarGeoHelper._topFolded;
      set
      {
        CalendarGeoHelper._topFolded = value;
        EventHandler topFoldedChanged = CalendarGeoHelper.TopFoldedChanged;
        if (topFoldedChanged == null)
          return;
        topFoldedChanged((object) null, (EventArgs) null);
      }
    }

    public static event EventHandler<double> CalendarHourHeightChanged;

    public static double HourHeight => LocalSettings.Settings.CalendarHourHeight;

    public static double QuarterHourHeight => LocalSettings.Settings.CalendarHourHeight / 4.0;

    public static double HalfHourHeight => LocalSettings.Settings.CalendarHourHeight / 2.0;

    public static double MinMinute => LocalSettings.Settings.CalendarMinMinute;

    public static int CalColumns => !LocalSettings.Settings.ShowCalWeekend ? 5 : 7;

    public static void SetCalendarHourHeight(double height)
    {
      double validHeight = LocalSettings.GetValidHeight(height);
      LocalSettings.Settings.CalendarHourHeight = validHeight;
      LocalSettings.Settings.CalendarMinMinute = 60.0 / ((double) (int) validHeight / CalendarGeoHelper.MinHeight);
      EventHandler<double> hourHeightChanged = CalendarGeoHelper.CalendarHourHeightChanged;
      if (hourHeightChanged == null)
        return;
      hourHeightChanged((object) null, validHeight);
    }

    private static double TransVerticalOffset(DateTime startDate)
    {
      return ((startDate - startDate.Date).TotalHours - (double) CalendarGeoHelper.GetStartHourForTask()) * CalendarGeoHelper.HourHeight - CalendarGeoHelper.GetTopFoldDiff();
    }

    public static int GetCollapsedHours()
    {
      return CalendarGeoHelper._startHour + (24 - CalendarGeoHelper._endHour);
    }

    private static double CalculateCellHeight(DateTime startDate, DateTime? dueDate)
    {
      double hourHeight = CalendarGeoHelper.HourHeight;
      double minMinute = CalendarGeoHelper.MinMinute;
      if (dueDate.HasValue)
      {
        DateTime? nullable = dueDate;
        DateTime dateTime1 = startDate;
        if ((nullable.HasValue ? (nullable.HasValue ? (nullable.GetValueOrDefault() == dateTime1 ? 1 : 0) : 1) : 0) == 0)
        {
          DateTime dateTime2 = startDate.Date.AddHours((double) (CalendarGeoHelper.GetEndHour() + (!CalendarGeoHelper.TopFolded ? 1 : 0)));
          if (dueDate.Value < dateTime2)
            dateTime2 = dueDate.Value;
          TimeSpan timeSpan = dateTime2 - startDate;
          return timeSpan.TotalMinutes > minMinute ? (double) (int) (timeSpan.TotalHours * hourHeight) : CalendarGeoHelper.MinHeight;
        }
      }
      return CalendarGeoHelper.MinPointHeight;
    }

    public static int TranslateMinute(double height)
    {
      double num1 = CalendarGeoHelper.HourHeight / 60.0;
      int num2 = (int) (height * 1.0 / num1);
      int num3 = num2 % 15;
      return num3 >= 7 ? num2 + (15 - num3) : num2 - num3;
    }

    public static DateTime TranslateVerticalOffset(DateTime date, double offset)
    {
      int num = (int) (offset / CalendarGeoHelper.QuarterHourHeight);
      DateTime dateTime = date.Date;
      dateTime = dateTime.AddMinutes((double) (num * 15));
      return dateTime.AddHours((double) CalendarGeoHelper.GetStartHourForTask());
    }

    public static DateTime TranslateVerticalOffsetToStart(DateTime date, double offset)
    {
      int num = (int) (offset / CalendarGeoHelper.QuarterHourHeight);
      DateTime dateTime = date.Date;
      dateTime = dateTime.AddMinutes((double) (num * 15));
      return dateTime.AddHours((double) CalendarGeoHelper.GetStartHourForTask());
    }

    public static TaskCellViewModel BuildAddTaskModel(DateTime date, double verticalOffset)
    {
      verticalOffset += CalendarGeoHelper.GetTopFoldDiff();
      int num1 = (int) (verticalOffset / CalendarGeoHelper.QuarterHourHeight);
      double num2 = (double) num1 * CalendarGeoHelper.QuarterHourHeight;
      DateTime dateTime1 = date.Date;
      dateTime1 = dateTime1.AddMinutes((double) (num1 * 15));
      DateTime dateTime2 = dateTime1.AddHours((double) CalendarGeoHelper.GetStartHourForTask());
      TaskBaseViewModel taskBaseViewModel = new TaskBaseViewModel()
      {
        Id = Utils.GetGuid(),
        Type = DisplayType.Task,
        Kind = "TEXT",
        Priority = TaskDefaultDao.GetDefaultSafely().Priority,
        Tag = TaskDefaultDao.GetDefaultSafely().TagString,
        Tags = TaskDefaultDao.GetDefaultSafely().Tags?.ToArray(),
        StartDate = new DateTime?(dateTime2),
        Status = 0,
        Title = string.Empty,
        IsAllDay = new bool?(false)
      };
      TaskCellViewModel taskCellViewModel = new TaskCellViewModel();
      taskCellViewModel.SourceViewModel = taskBaseViewModel;
      taskCellViewModel.Height = CalendarGeoHelper.MinPointHeight;
      taskCellViewModel.Color = "transparent";
      taskCellViewModel.Selected = true;
      taskCellViewModel.HorizontalOffset = 0.0;
      taskCellViewModel.VerticalOffset = num2 - CalendarGeoHelper.GetTopFoldDiff();
      taskCellViewModel.NewAdd = true;
      return taskCellViewModel;
    }

    public static void AssemblyCells(
      List<TaskCellViewModel> cells,
      double width,
      DateTime date,
      DateTime? spanStart = null,
      double? hourHeight = null)
    {
      if (cells == null || cells.Count == 0)
        return;
      IEnumerable<CalendarGeoHelper.CellGeo> cellGeos = CalendarGeoHelper.AssemblyCellGeoInfo((IReadOnlyCollection<TaskCellViewModel>) cells, width, date, spanStart, hourHeight);
      Dictionary<string, CalendarGeoHelper.CellGeo> dictionary = new Dictionary<string, CalendarGeoHelper.CellGeo>();
      foreach (CalendarGeoHelper.CellGeo cellGeo in cellGeos)
      {
        if (cellGeo.Id != null && !dictionary.ContainsKey(cellGeo.Id))
          dictionary.Add(cellGeo.Id, cellGeo);
      }
      if (dictionary.Count <= 0)
        return;
      foreach (TaskCellViewModel cell in cells)
      {
        if (!string.IsNullOrEmpty(cell.Identity))
        {
          if (dictionary.ContainsKey(cell.Identity))
          {
            CalendarGeoHelper.CellGeo cellGeo = dictionary[cell.Identity];
            if (cellGeo != null)
            {
              cell.Width = cellGeo.Width;
              cell.Height = cellGeo.Height;
              cell.HorizontalOffset = cellGeo.Left;
              cell.VerticalOffset = cellGeo.Top;
              cell.Tips = cellGeo.Tips;
              cell.ZIndex = cellGeo.Index;
            }
          }
          else
            UtilLog.Info("CalendarGeoHelper.AssemblyCells, " + cell.Identity + " not assembly");
        }
      }
    }

    private static IEnumerable<CalendarGeoHelper.CellGeo> AssemblyCellGeoInfoTemp(
      IReadOnlyCollection<TaskCellViewModel> cells,
      double width,
      DateTime date,
      DateTime? spanStart,
      double? hourHeight = null)
    {
      if (cells == null || cells.Count <= 0)
        return (IEnumerable<CalendarGeoHelper.CellGeo>) null;
      List<TaskCellViewModel> list1 = cells.Where<TaskCellViewModel>((Func<TaskCellViewModel, bool>) (c => c != null)).ToList<TaskCellViewModel>();
      list1.Sort((Comparison<TaskCellViewModel>) ((a, b) =>
      {
        if (a.Status == 0 && b.Status != 0)
          return -1;
        if (a.Status != 0 && b.Status == 0)
          return 1;
        return a.Type != b.Type ? a.Type.CompareTo((object) b.Type) : a.SourceViewModel.SortOrder.CompareTo(b.SourceViewModel.SortOrder);
      }));
      List<CalendarGeoHelper.CellGeo> list2 = list1.Select<TaskCellViewModel, CalendarGeoHelper.CellGeo>((Func<TaskCellViewModel, CalendarGeoHelper.CellGeo>) (m => spanStart.HasValue ? CalendarGeoHelper.CellGeo.Build(m, hourHeight ?? 60.0, spanStart.Value) : CalendarGeoHelper.CellGeo.Build(m, date))).Where<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, bool>) (x => x?.Id != null)).OrderBy<CalendarGeoHelper.CellGeo, double>((Func<CalendarGeoHelper.CellGeo, double>) (x => x.Top)).ThenByDescending<CalendarGeoHelper.CellGeo, double>((Func<CalendarGeoHelper.CellGeo, double>) (x => x.Height)).ToList<CalendarGeoHelper.CellGeo>();
      List<List<List<CalendarGeoHelper.CellGeo>>> cellGeoListListList = CalendarGeoHelper.AssemblyGroupsTemp(list2.ToList<CalendarGeoHelper.CellGeo>());
      double num1 = width * 0.05;
      foreach (List<List<CalendarGeoHelper.CellGeo>> cellGeoListList in cellGeoListListList)
      {
        for (int index1 = 0; index1 < cellGeoListList.Count; ++index1)
        {
          List<CalendarGeoHelper.CellGeo> source1 = cellGeoListList[index1];
          if (index1 == 0 || cellGeoListList[index1 - 1].Count == 0)
          {
            double num2 = (width - 9.0) / (double) source1.Count;
            for (int index2 = 0; index2 < source1.Count; ++index2)
            {
              CalendarGeoHelper.CellGeo cellGeo = source1[index2];
              cellGeo.Left = num2 * (double) index2;
              cellGeo.Width = num2 - 2.0;
              cellGeo.Index = 0;
            }
          }
          else
          {
            int index3 = index1 - 1;
            List<CalendarGeoHelper.CellGeo> source2;
            do
            {
              source2 = cellGeoListList[index3];
              if (source2.Count != 0)
              {
                double minTop = source2.Min<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, double>) (m => m.Top));
                double maxBottom = source2.Max<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, double>) (m => m.Bottom));
                if (!source1.Any<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, bool>) (c => c.Top >= minTop && c.Top < maxBottom)))
                  --index3;
                else
                  break;
              }
              else
                break;
            }
            while (index3 >= 0);
            if (source2.Count == 0)
            {
              double num3 = (width - 9.0) / (double) source1.Count;
              for (int index4 = 0; index4 < source1.Count; ++index4)
              {
                CalendarGeoHelper.CellGeo cellGeo = source1[index4];
                cellGeo.Left = num3 * (double) index4;
                cellGeo.Width = num3 - 2.0;
                cellGeo.Index = 0;
              }
            }
            else
            {
              int count = source1.Count;
              bool flag = count <= source2.Count;
              int num4 = count / source2.Count;
              int num5 = count % source2.Count;
              int num6 = 0;
              for (int index5 = 0; index5 < source2.Count; ++index5)
              {
                CalendarGeoHelper.CellGeo cellGeo1 = source2[index5];
                double num7 = cellGeo1.Width - num1;
                if (flag)
                {
                  if (source1.Count > index5)
                  {
                    CalendarGeoHelper.CellGeo cellGeo2 = source1[index5];
                    cellGeo2.Left = cellGeo1.Left + num1;
                    cellGeo2.Width = num7;
                    cellGeo2.Index = index1;
                    if (!cellGeo1.ChildTop.HasValue)
                      cellGeo1.ChildTop = new double?(cellGeo2.Top);
                  }
                }
                else
                {
                  int num8 = num4 + (index5 + num5 >= source2.Count ? 1 : 0);
                  double num9 = num7 / (double) num8;
                  for (int index6 = num6; index6 < num6 + num8; ++index6)
                  {
                    CalendarGeoHelper.CellGeo cellGeo3 = source1[index6];
                    cellGeo3.Left = cellGeo1.Left + num1 + num9 * (double) (index6 - num6);
                    cellGeo3.Width = num9;
                    cellGeo3.Index = index1;
                    if (!cellGeo1.ChildTop.HasValue)
                      cellGeo1.ChildTop = new double?(cellGeo3.Top);
                  }
                  num6 += num8;
                }
              }
            }
          }
        }
      }
      return (IEnumerable<CalendarGeoHelper.CellGeo>) list2;
    }

    private static IEnumerable<CalendarGeoHelper.CellGeo> AssemblyCellGeoInfo(
      IReadOnlyCollection<TaskCellViewModel> cells,
      double width,
      DateTime date,
      DateTime? spanStart,
      double? hourHeight = null)
    {
      if (cells == null || cells.Count <= 0)
        return (IEnumerable<CalendarGeoHelper.CellGeo>) null;
      List<TaskCellViewModel> list = cells.Where<TaskCellViewModel>((Func<TaskCellViewModel, bool>) (c => c != null)).ToList<TaskCellViewModel>();
      list.Sort((Comparison<TaskCellViewModel>) ((a, b) =>
      {
        if (a.Status == 0 && b.Status != 0)
          return -1;
        if (a.Status != 0 && b.Status == 0)
          return 1;
        return a.Type != b.Type ? a.Type.CompareTo((object) b.Type) : a.SourceViewModel.SortOrder.CompareTo(b.SourceViewModel.SortOrder);
      }));
      List<(CalendarGeoHelper.CellGeo, int)> tupleList = CalendarGeoHelper.AssemblyGroups(list.Select<TaskCellViewModel, CalendarGeoHelper.CellGeo>((Func<TaskCellViewModel, CalendarGeoHelper.CellGeo>) (m => spanStart.HasValue ? CalendarGeoHelper.CellGeo.Build(m, hourHeight ?? 60.0, spanStart.Value) : CalendarGeoHelper.CellGeo.Build(m, date))).Where<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, bool>) (x => x?.Id != null)).OrderBy<CalendarGeoHelper.CellGeo, double>((Func<CalendarGeoHelper.CellGeo, double>) (x => x.Top)).ThenByDescending<CalendarGeoHelper.CellGeo, double>((Func<CalendarGeoHelper.CellGeo, double>) (x => x.Height)).ToList<CalendarGeoHelper.CellGeo>());
      ConcurrentBag<CalendarGeoHelper.CellGeo> concurrentBag = new ConcurrentBag<CalendarGeoHelper.CellGeo>();
      foreach ((CalendarGeoHelper.CellGeo node, int fullDeep) in tupleList)
      {
        double oneWidth = (width - 9.0 - 2.0) / (double) fullDeep;
        CalendarGeoHelper.TraversalCellGeo(node, oneWidth, (double) fullDeep, concurrentBag);
      }
      return (IEnumerable<CalendarGeoHelper.CellGeo>) concurrentBag.ToList<CalendarGeoHelper.CellGeo>();
    }

    private static void TraversalCellGeo(
      CalendarGeoHelper.CellGeo node,
      double oneWidth,
      double fullDeep,
      ConcurrentBag<CalendarGeoHelper.CellGeo> solvedBag)
    {
      if (solvedBag.Contains<CalendarGeoHelper.CellGeo>(node))
        return;
      solvedBag.Add(node);
      if (node.LeftParent == null && !node.LeftSibling.Any<CalendarGeoHelper.CellGeo>())
      {
        node.Left = 0.0;
        node.Width = node.RightParent == null || Math.Abs(node.Top - node.RightParent.Top) <= 0.01 ? (double) (node.Column + 1) * oneWidth - 1.0 : node.RightParent.Left - 1.0;
      }
      else if (node.RightParent == null && !node.RightSibling.Any<CalendarGeoHelper.CellGeo>())
      {
        node.Left = oneWidth * (double) node.Column;
        node.Width = (fullDeep - (double) node.Column) * oneWidth - 1.0;
      }
      else if (node.RightParent != null && node.LeftParent != null)
      {
        node.Left = oneWidth * (double) (node.LeftParent.Column + 1);
        node.Width = (double) (node.RightParent.LeftParent.Column - node.LeftParent.Column) * oneWidth - 1.0;
      }
      else
      {
        node.Left = oneWidth * (double) node.Column;
        node.Width = oneWidth - 1.0;
      }
      if (node.RightSibling.Any<CalendarGeoHelper.CellGeo>())
      {
        foreach (CalendarGeoHelper.CellGeo node1 in node.RightSibling)
          CalendarGeoHelper.TraversalCellGeo(node1, oneWidth, fullDeep, solvedBag);
      }
      if (!node.LeftSibling.Any<CalendarGeoHelper.CellGeo>())
        return;
      foreach (CalendarGeoHelper.CellGeo node2 in node.LeftSibling)
        CalendarGeoHelper.TraversalCellGeo(node2, oneWidth, fullDeep, solvedBag);
    }

    private static void AssembleChildren(
      CalendarGeoHelper.CellGeo node,
      List<CalendarGeoHelper.CellGeo> restNodes,
      int column,
      HashSet<CalendarGeoHelper.CellGeo> reloved)
    {
      if (reloved.Contains(node))
        return;
      reloved.Add(node);
      node.Column = column;
      List<CalendarGeoHelper.CellGeo> list1 = restNodes.Where<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, bool>) (n =>
      {
        if (n.Id == node.Id || n.Id == node.LeftParent?.Id || n.Id == node.RightParent?.Id)
          return false;
        if (n.RightParent != null && node.LeftParent != null)
        {
          for (CalendarGeoHelper.CellGeo leftParent = node.LeftParent; leftParent != null; leftParent = leftParent.LeftParent)
          {
            if (leftParent == n.RightParent)
              return false;
          }
        }
        return (node.RightParent == null || n.Top >= node.RightParent.Bottom) && CalendarGeoHelper.StartTimeContains(node, n);
      })).ToList<CalendarGeoHelper.CellGeo>();
      if (!list1.Any<CalendarGeoHelper.CellGeo>())
        return;
      List<CalendarGeoHelper.CellGeo> list2 = list1.Where<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, bool>) (n =>
      {
        if (column == 0 || n.RightParent != null && n.RightParent.Column > node.Column || n.LeftParent == null && n.RightParent == null && node.Bottom <= n.Top || node.Column <= n.Column && (n.LeftParent == null || n.LeftParent.Column >= node.Column))
          return false;
        CalendarGeoHelper.CellGeo cellGeo = node;
        while (cellGeo.LeftParent != null)
        {
          cellGeo = cellGeo.LeftParent;
          if (cellGeo.LeftParent == null || cellGeo.LeftParent == n.LeftParent)
            return cellGeo.Bottom <= n.Top;
          if (cellGeo.LeftParent.RightSibling.Max<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, double>) (child => child.Bottom)) > n.Top)
            return false;
        }
        return cellGeo.Bottom <= n.Top;
      })).ToList<CalendarGeoHelper.CellGeo>();
      if (list2.Any<CalendarGeoHelper.CellGeo>())
      {
        list2 = list2.OrderBy<CalendarGeoHelper.CellGeo, double>((Func<CalendarGeoHelper.CellGeo, double>) (time => time.Top)).ToList<CalendarGeoHelper.CellGeo>();
        CalendarGeoHelper.CellGeo node1 = list2.First<CalendarGeoHelper.CellGeo>();
        foreach (CalendarGeoHelper.CellGeo node2 in list2)
        {
          if (!CalendarGeoHelper.StartTimeContains(node1, node2))
            node1 = node2;
          node1.RightParent = node;
        }
      }
      node.LeftSibling = list2.Where<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, bool>) (child => child.RightParent == node)).ToList<CalendarGeoHelper.CellGeo>();
      list1.RemoveAll((Predicate<CalendarGeoHelper.CellGeo>) (child => child.RightParent == node));
      List<CalendarGeoHelper.CellGeo> list3 = list1.Where<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, bool>) (item =>
      {
        if (item.LeftParent == null)
          return true;
        return item.LeftParent != null && item.LeftParent.Column < node.Column;
      })).OrderBy<CalendarGeoHelper.CellGeo, double>((Func<CalendarGeoHelper.CellGeo, double>) (time => time.Top)).ToList<CalendarGeoHelper.CellGeo>();
      if (list3.Any<CalendarGeoHelper.CellGeo>())
      {
        CalendarGeoHelper.CellGeo node1 = list3.First<CalendarGeoHelper.CellGeo>();
        if (Math.Abs(node1.Top - node.Top) < 0.01)
          node.RightParent = node1;
        foreach (CalendarGeoHelper.CellGeo node2 in list3)
        {
          if (!CalendarGeoHelper.StartTimeContains(node1, node2))
            node1 = node2;
          node1.LeftParent = node;
        }
      }
      node.RightSibling = list1.Where<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, bool>) (child => child.LeftParent == node)).ToList<CalendarGeoHelper.CellGeo>();
      foreach (CalendarGeoHelper.CellGeo node1 in node.RightSibling)
        CalendarGeoHelper.AssembleChildren(node1, restNodes, column + 1, reloved);
      foreach (CalendarGeoHelper.CellGeo node2 in node.LeftSibling)
        CalendarGeoHelper.AssembleChildren(node2, restNodes, column - 1, reloved);
    }

    private static bool StartTimeContains(
      CalendarGeoHelper.CellGeo node1,
      CalendarGeoHelper.CellGeo node2)
    {
      return MathUtil.DoubleLe(node1.Top, node2.Top, 0.1) && MathUtil.DoubleLt(node2.Top, node1.Bottom, 0.1);
    }

    private static List<List<List<CalendarGeoHelper.CellGeo>>> AssemblyGroupsTemp(
      List<CalendarGeoHelper.CellGeo> geos)
    {
      List<List<List<CalendarGeoHelper.CellGeo>>> cellGeoListListList = new List<List<List<CalendarGeoHelper.CellGeo>>>();
      List<List<CalendarGeoHelper.CellGeo>> cellGeoListList = new List<List<CalendarGeoHelper.CellGeo>>();
      List<CalendarGeoHelper.CellGeo> source1 = new List<CalendarGeoHelper.CellGeo>();
      cellGeoListListList.Add(cellGeoListList);
      cellGeoListList.Add(source1);
      List<CalendarGeoHelper.CellGeo> source2 = new List<CalendarGeoHelper.CellGeo>();
      while (geos.Any<CalendarGeoHelper.CellGeo>())
      {
        CalendarGeoHelper.CellGeo current = geos.First<CalendarGeoHelper.CellGeo>();
        geos.Remove(current);
        if (source2.Count == 0 || source2.Any<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, bool>) (o => o.IsCross(current))))
        {
          if (source1.Count != 0 && source1.All<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, bool>) (c => Math.Abs(current.Top - c.Top) >= 32.0)))
          {
            source1 = new List<CalendarGeoHelper.CellGeo>();
            cellGeoListList.Add(source1);
          }
          source2.Add(current);
        }
        else
        {
          source2.Clear();
          source2.Add(current);
          cellGeoListList = new List<List<CalendarGeoHelper.CellGeo>>();
          source1 = new List<CalendarGeoHelper.CellGeo>();
          cellGeoListListList.Add(cellGeoListList);
          cellGeoListList.Add(source1);
        }
        source1.Add(current);
      }
      return cellGeoListListList;
    }

    private static List<(CalendarGeoHelper.CellGeo, int)> AssemblyGroups(
      List<CalendarGeoHelper.CellGeo> geos)
    {
      List<(CalendarGeoHelper.CellGeo, int)> valueTupleList = new List<(CalendarGeoHelper.CellGeo, int)>();
      while (geos.Any<CalendarGeoHelper.CellGeo>())
      {
        CalendarGeoHelper.CellGeo node = geos.First<CalendarGeoHelper.CellGeo>();
        geos.Remove(node);
        HashSet<CalendarGeoHelper.CellGeo> cellGeoSet = new HashSet<CalendarGeoHelper.CellGeo>();
        CalendarGeoHelper.AssembleChildren(node, geos, 0, cellGeoSet);
        int num = cellGeoSet.Max<CalendarGeoHelper.CellGeo>((Func<CalendarGeoHelper.CellGeo, int>) (item => item.Column)) + 1;
        valueTupleList.Add((node, num));
        foreach (CalendarGeoHelper.CellGeo cellGeo in cellGeoSet)
          geos.Remove(cellGeo);
      }
      return valueTupleList;
    }

    public static double GetWeekColumnWidth(double sectionWidth, int count)
    {
      return (sectionWidth - 60.0) / (double) count;
    }

    public static double CalHorizontalOffset(double x, double width)
    {
      int num = (int) ((x - 60.0) / width);
      return num >= 0 ? 60.0 + (double) num * width + 1.0 : x - 12.0;
    }

    public static double GetBarWidth(double dayWidth, int column, int range)
    {
      double barWidth = 0.0;
      for (int column1 = column; column1 < column + range; ++column1)
      {
        if (!CalendarGeoHelper.IsColumnHide(column1))
          barWidth += dayWidth;
      }
      return barWidth;
    }

    public static double GetBarLeft(double dayWidth, int column)
    {
      double barLeft = 0.0;
      for (int column1 = 0; column1 < column; ++column1)
      {
        if (!CalendarGeoHelper.IsColumnHide(column1))
          barLeft += dayWidth;
      }
      return barLeft;
    }

    public static int GetRealColumn(int column)
    {
      if (!LocalSettings.Settings.ShowCalWeekend)
      {
        switch (LocalSettings.Settings.WeekStartFrom)
        {
          case "Sunday":
            --column;
            break;
          case "Saturday":
            column -= 2;
            break;
        }
      }
      return column;
    }

    private static bool IsColumnHide(int column)
    {
      if (!LocalSettings.Settings.ShowCalWeekend)
      {
        switch (LocalSettings.Settings.WeekStartFrom)
        {
          case "Sunday":
            return column == 0 || column == 6;
          case "Saturday":
            return column == 0 || column == 1;
          case "Monday":
            return column == 5 || column == 6;
        }
      }
      return false;
    }

    public static double GetMonthDragBarWidth(double width, int count)
    {
      return width / (double) count + 1.0;
    }

    public static void SetStartHour(int hour)
    {
      if (hour >= CalendarGeoHelper._endHour)
        return;
      CalendarGeoHelper._startHour = hour;
      LocalSettings.Settings.CollapsedStart = hour;
    }

    public static int GetStartHourForTask()
    {
      return !CalendarGeoHelper.TopFolded ? 0 : CalendarGeoHelper._startHour - 1;
    }

    public static int GetStartHour(bool display = true)
    {
      return !display || CalendarGeoHelper.TopFolded ? CalendarGeoHelper._startHour : 0;
    }

    public static int GetEndHour(bool display = true)
    {
      return !display || CalendarGeoHelper.TopFolded ? CalendarGeoHelper._endHour : 24;
    }

    public static void SetEndHour(int hour)
    {
      if (hour <= CalendarGeoHelper._startHour)
        return;
      CalendarGeoHelper._endHour = hour;
      LocalSettings.Settings.CollapsedEnd = hour;
    }

    public static int GetTimeBlockCount()
    {
      return !CalendarGeoHelper.TopFolded ? 24 : CalendarGeoHelper._endHour - CalendarGeoHelper._startHour + 1;
    }

    public static int GetPointOffset()
    {
      int hour = DateTime.Now.Hour;
      int startHourForTask = CalendarGeoHelper.GetStartHourForTask();
      int endHour = CalendarGeoHelper.GetEndHour();
      return hour >= startHourForTask && hour < endHour ? (int) ((DateTime.Now - DateTime.Today.AddHours((double) startHourForTask)).TotalMinutes * (CalendarGeoHelper.HourHeight * 1.0 / 60.0)) - (int) CalendarGeoHelper.GetTopFoldDiff() : -1;
    }

    public static double GetRoundOffset(double currentY)
    {
      double quarterHourHeight = CalendarGeoHelper.QuarterHourHeight;
      int num1 = (int) (currentY / quarterHourHeight);
      double num2 = quarterHourHeight * (double) (num1 - 1);
      double num3 = quarterHourHeight * (double) num1;
      double num4 = quarterHourHeight * (double) (num1 + 1);
      double val1 = Math.Abs(currentY - num3);
      double num5 = Math.Abs(currentY - num2);
      double val2_1 = Math.Abs(currentY - num4);
      double roundOffset = num3;
      double val2_2 = num5;
      double num6 = Math.Min(Math.Min(val1, val2_2), val2_1);
      if (Math.Abs(num6 - val2_1) <= 0.001)
        roundOffset = num4;
      else if (Math.Abs(num6 - num5) <= 0.001)
        roundOffset = num2;
      return roundOffset;
    }

    public static void NotifyTopUnFolded(bool isTop)
    {
      EventHandler<bool> expandTop = CalendarGeoHelper.ExpandTop;
      if (expandTop == null)
        return;
      expandTop((object) null, isTop);
    }

    public static void GetCellPosition(TaskCellViewModel cellModel)
    {
      if (!cellModel.BaseDate.HasValue)
        return;
      CalendarGeoHelper.CellGeo cellGeo = CalendarGeoHelper.CellGeo.Build(cellModel, cellModel.BaseDate.Value.Date);
      cellModel.Height = cellGeo.Height;
      cellModel.HorizontalOffset = cellGeo.Left;
      cellModel.VerticalOffset = cellGeo.Top;
      cellModel.SourceViewModel.IsAllDay = new bool?(false);
    }

    public static void ResetCellHeightAndVertical(TaskCellViewModel cellModel, DateTime? baseDate)
    {
      if (!baseDate.HasValue)
        return;
      CalendarGeoHelper.CellGeo cellGeo = CalendarGeoHelper.CellGeo.Build(cellModel, baseDate.Value.Date);
      cellModel.Height = cellGeo.Height;
      cellModel.VerticalOffset = cellGeo.Top;
    }

    public static double GetTopFoldDiff()
    {
      return !CalendarGeoHelper.TopFolded ? 0.0 : CalendarGeoHelper.HourHeight - 32.0;
    }

    private class CellGeo
    {
      public int Column;
      public string Id;
      public List<CalendarGeoHelper.CellGeo> LeftSibling = new List<CalendarGeoHelper.CellGeo>();
      public List<CalendarGeoHelper.CellGeo> RightSibling = new List<CalendarGeoHelper.CellGeo>();
      public CalendarGeoHelper.CellGeo LeftParent;
      public CalendarGeoHelper.CellGeo RightParent;
      public double Top;
      public double Height;
      public double? ChildTop;
      public string Tips;
      public double Width;
      public double Left;

      public double Bottom => this.Top + this.Height;

      public int Index { get; set; }

      public static CalendarGeoHelper.CellGeo Build(TaskCellViewModel model, DateTime date)
      {
        DateTime? displayStartDate = model.DisplayStartDate;
        if (!displayStartDate.HasValue)
          return (CalendarGeoHelper.CellGeo) null;
        DateTime startDate1 = displayStartDate.GetValueOrDefault();
        if (startDate1.Date != date.Date)
          startDate1 = date.Date;
        DateTime? dueDate = model.BarMode ? new DateTime?() : model.DisplayDueDate;
        CalendarGeoHelper.CellGeo cellGeo = new CalendarGeoHelper.CellGeo()
        {
          Id = model.Identity,
          Top = CalendarGeoHelper.TransVerticalOffset(startDate1),
          Height = CalendarGeoHelper.CalculateCellHeight(startDate1, dueDate)
        };
        if (dueDate.HasValue && dueDate.Value.Date != date.Date)
          dueDate = new DateTime?(date.Date.AddDays(1.0));
        if (CalendarGeoHelper.TopFolded)
        {
          int hour = startDate1.Hour;
          int startHourForTask = CalendarGeoHelper.GetStartHourForTask();
          if (hour >= CalendarGeoHelper.GetEndHour())
          {
            cellGeo.Top = -1.0 * CalendarGeoHelper.HourHeight;
            cellGeo.Height = 0.0;
          }
          else if (hour <= startHourForTask)
          {
            if (dueDate.HasValue && (dueDate.Value - date.Date).TotalHours > (double) (startHourForTask + 1))
            {
              DateTime startDate2 = startDate1.Date.AddHours((double) (startHourForTask + 1));
              cellGeo.Top = CalendarGeoHelper.TransVerticalOffset(startDate2);
              cellGeo.Height = CalendarGeoHelper.CalculateCellHeight(startDate2, dueDate);
            }
            else
            {
              cellGeo.Top = -1.0 * CalendarGeoHelper.HourHeight;
              cellGeo.Height = 0.0;
            }
          }
        }
        return cellGeo;
      }

      public static CalendarGeoHelper.CellGeo Build(
        TaskCellViewModel model,
        double hourHeight,
        DateTime spanStart)
      {
        DateTime? displayStartDate = model.DisplayStartDate;
        if (!displayStartDate.HasValue)
          return (CalendarGeoHelper.CellGeo) null;
        DateTime dateTime = displayStartDate.GetValueOrDefault();
        if (dateTime < spanStart)
          dateTime = spanStart;
        DateTime? nullable = model.DisplayDueDate;
        if (nullable.HasValue && nullable.Value > spanStart.AddHours(4.0))
          nullable = new DateTime?(spanStart.AddHours(4.0));
        return new CalendarGeoHelper.CellGeo()
        {
          Id = model.Identity,
          Top = (dateTime - spanStart).TotalHours * hourHeight,
          Height = !nullable.HasValue ? 16.0 : Math.Max(16.0, (nullable.Value - dateTime).TotalHours * hourHeight)
        };
      }

      public bool IsCross(CalendarGeoHelper.CellGeo geo)
      {
        if (this.Top <= geo.Top && this.Bottom > geo.Top)
          return true;
        return geo.Top <= this.Top && geo.Bottom > this.Top;
      }
    }
  }
}
