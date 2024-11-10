// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.GridHelpers
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

#nullable disable
namespace ticktick_WPF.Util
{
  public class GridHelpers
  {
    public static readonly DependencyProperty RowCountProperty = DependencyProperty.RegisterAttached("RowCount", typeof (int), typeof (GridHelpers), new PropertyMetadata((object) -1, new PropertyChangedCallback(GridHelpers.RowCountChanged)));
    public static readonly DependencyProperty ColumnCountProperty = DependencyProperty.RegisterAttached("ColumnCount", typeof (int), typeof (GridHelpers), new PropertyMetadata((object) -1, new PropertyChangedCallback(GridHelpers.ColumnCountChanged)));
    public static readonly DependencyProperty StarRowsProperty = DependencyProperty.RegisterAttached("StarRows", typeof (string), typeof (GridHelpers), new PropertyMetadata((object) string.Empty, new PropertyChangedCallback(GridHelpers.StarRowsChanged)));
    public static readonly DependencyProperty StarColumnsProperty = DependencyProperty.RegisterAttached("StarColumns", typeof (string), typeof (GridHelpers), new PropertyMetadata((object) string.Empty, new PropertyChangedCallback(GridHelpers.StarColumnsChanged)));

    public static int GetRowCount(DependencyObject obj)
    {
      return (int) obj.GetValue(GridHelpers.RowCountProperty);
    }

    public static void SetRowCount(DependencyObject obj, int value)
    {
      obj.SetValue(GridHelpers.RowCountProperty, (object) value);
    }

    public static void RowCountChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      if (!(obj is Grid) || (int) e.NewValue < 0)
        return;
      Grid grid = (Grid) obj;
      grid.RowDefinitions.Clear();
      for (int index = 0; index < (int) e.NewValue; ++index)
        grid.RowDefinitions.Add(new RowDefinition()
        {
          Height = GridLength.Auto
        });
      GridHelpers.SetStarRows(grid);
    }

    public static int GetColumnCount(DependencyObject obj)
    {
      return (int) obj.GetValue(GridHelpers.ColumnCountProperty);
    }

    public static void SetColumnCount(DependencyObject obj, int value)
    {
      obj.SetValue(GridHelpers.ColumnCountProperty, (object) value);
    }

    public static void ColumnCountChanged(
      DependencyObject obj,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(obj is Grid) || (int) e.NewValue < 0)
        return;
      Grid grid = (Grid) obj;
      grid.ColumnDefinitions.Clear();
      for (int index = 0; index < (int) e.NewValue; ++index)
        grid.ColumnDefinitions.Add(new ColumnDefinition()
        {
          Width = GridLength.Auto
        });
      GridHelpers.SetStarColumns(grid);
    }

    public static string GetStarRows(DependencyObject obj)
    {
      return (string) obj.GetValue(GridHelpers.StarRowsProperty);
    }

    public static void SetStarRows(DependencyObject obj, string value)
    {
      obj.SetValue(GridHelpers.StarRowsProperty, (object) value);
    }

    public static void StarRowsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
    {
      if (!(obj is Grid) || string.IsNullOrEmpty(e.NewValue.ToString()))
        return;
      GridHelpers.SetStarRows((Grid) obj);
    }

    public static string GetStarColumns(DependencyObject obj)
    {
      return (string) obj.GetValue(GridHelpers.StarColumnsProperty);
    }

    public static void SetStarColumns(DependencyObject obj, string value)
    {
      obj.SetValue(GridHelpers.StarColumnsProperty, (object) value);
    }

    public static void StarColumnsChanged(
      DependencyObject obj,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(obj is Grid) || string.IsNullOrEmpty(e.NewValue.ToString()))
        return;
      GridHelpers.SetStarColumns((Grid) obj);
    }

    private static void SetStarColumns(Grid grid)
    {
      string[] source = GridHelpers.GetStarColumns((DependencyObject) grid).Split(',');
      for (int index = 0; index < grid.ColumnDefinitions.Count; ++index)
      {
        if (((IEnumerable<string>) source).Contains<string>(index.ToString()))
          grid.ColumnDefinitions[index].Width = new GridLength(1.0, GridUnitType.Star);
      }
    }

    private static void SetStarRows(Grid grid)
    {
      string[] source = GridHelpers.GetStarRows((DependencyObject) grid).Split(',');
      for (int index = 0; index < grid.RowDefinitions.Count; ++index)
      {
        if (((IEnumerable<string>) source).Contains<string>(index.ToString()))
          grid.RowDefinitions[index].Height = new GridLength(1.0, GridUnitType.Star);
      }
    }
  }
}
