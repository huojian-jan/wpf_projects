// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.MultiDayAllDayPrintRowView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar.Month;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class MultiDayAllDayPrintRowView : Grid
  {
    private int _days;

    public MultiDayAllDayPrintRowView(int days, bool showAllDayText)
    {
      this.Height = 40.0;
      this._days = days;
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(48.0)
      });
      SolidColorBrush colorInString = ThemeUtil.GetColorInString("#89191919");
      for (int index = 0; index < days; ++index)
      {
        this.ColumnDefinitions.Add(new ColumnDefinition()
        {
          Width = new GridLength(1.0, GridUnitType.Star)
        });
        Line line = new Line();
        line.Y1 = 0.0;
        line.Y2 = 1.0;
        line.HorizontalAlignment = HorizontalAlignment.Right;
        line.Stretch = Stretch.Fill;
        line.StrokeThickness = 1.0;
        line.Stroke = (Brush) colorInString;
        line.Margin = new Thickness(0.0, 0.0, 0.0, -1.0);
        Line element = line;
        element.SetValue(Grid.ColumnProperty, (object) index);
        this.Children.Add((UIElement) element);
      }
      Line line1 = new Line();
      line1.Y1 = 0.0;
      line1.Y2 = 1.0;
      line1.HorizontalAlignment = HorizontalAlignment.Right;
      line1.Margin = new Thickness(0.0, 0.0, 0.0, -1.0);
      line1.Stretch = Stretch.Fill;
      line1.StrokeThickness = 1.0;
      line1.Stroke = (Brush) colorInString;
      Line element1 = line1;
      element1.SetValue(Grid.ColumnProperty, (object) days);
      this.Children.Add((UIElement) element1);
      if (!showAllDayText)
        return;
      TextBlock element2 = new TextBlock();
      element2.FontSize = 16.0;
      element2.Margin = new Thickness(0.0, 0.0, 0.0, 0.0);
      element2.VerticalAlignment = VerticalAlignment.Center;
      element2.Text = Utils.GetString("AllDay");
      element2.Foreground = (Brush) ThemeUtil.GetColorInString("#E8191919");
      this.Children.Add((UIElement) element2);
    }

    public void SetData(IEnumerable<WeekEventModel> rowData, bool showBottom, double topMargin = 0.0)
    {
      foreach (WeekEventModel weekEventModel in rowData)
      {
        if (weekEventModel.Column < 0)
        {
          weekEventModel.ColumnSpan += weekEventModel.Column;
          weekEventModel.Column = 0;
        }
        if (weekEventModel.Column + weekEventModel.ColumnSpan > this._days)
          weekEventModel.ColumnSpan = this._days - weekEventModel.Column;
        if (weekEventModel.ColumnSpan > 0 && weekEventModel.Column < this._days)
        {
          CalendarPrintCell calendarPrintCell = new CalendarPrintCell();
          calendarPrintCell.Margin = new Thickness(6.0, 3.0 + topMargin, 6.0, 3.0);
          CalendarPrintCell element = calendarPrintCell;
          element.SetValue(Grid.ColumnProperty, (object) (weekEventModel.Column + 1));
          element.SetValue(Grid.ColumnSpanProperty, (object) weekEventModel.ColumnSpan);
          element.Height = 25.0;
          this.Height = weekEventModel.Data.IsLoadMore ? 32.0 : 32.0 + topMargin;
          element.DataContext = (object) weekEventModel.Data;
          this.Children.Add((UIElement) element);
        }
      }
      if (!showBottom)
        return;
      this.Height += 3.0;
    }
  }
}
