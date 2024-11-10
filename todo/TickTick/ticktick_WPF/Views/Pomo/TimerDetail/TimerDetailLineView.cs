// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TimerDetail.TimerDetailLineView
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.TimerDetail
{
  public class TimerDetailLineView : Grid
  {
    private Line _averageLine;
    private TextBlock _averageText;
    private Grid _durTextGrid;
    private List<TimerDayItemViewModel> _items;
    private double _topMargin = 10.0;
    private int _lineNum = 4;

    public TimerDetailLineView()
    {
      this.Height = 160.0 + this._topMargin;
      this.ColumnDefinitions.Add(new ColumnDefinition());
      this.ColumnDefinitions.Add(new ColumnDefinition()
      {
        Width = new GridLength(54.0)
      });
      Line line1 = new Line();
      line1.X1 = 0.0;
      line1.X2 = 1.0;
      line1.Stretch = Stretch.Fill;
      line1.StrokeThickness = 2.0;
      line1.VerticalAlignment = VerticalAlignment.Top;
      line1.Margin = new Thickness(0.0, 120.0 + this._topMargin, 0.0, 0.0);
      this._averageLine = line1;
      this._averageLine.Stroke = (Brush) Brushes.Red;
      this._averageLine.SetValue(Panel.ZIndexProperty, (object) 100);
      this.Children.Add((UIElement) this._averageLine);
      TextBlock textBlock = new TextBlock();
      textBlock.VerticalAlignment = VerticalAlignment.Top;
      textBlock.Margin = new Thickness(8.0, this._averageLine.Margin.Top - 7.0 + this._topMargin, 0.0, 0.0);
      textBlock.Foreground = (Brush) Brushes.Red;
      this._averageText = textBlock;
      this._averageText.SetResourceReference(FrameworkElement.StyleProperty, (object) "Tag03");
      this._averageText.SetValue(Grid.ColumnProperty, (object) 1);
      this.Children.Add((UIElement) this._averageText);
      this._durTextGrid = new Grid();
      this._durTextGrid.SetValue(Grid.ColumnProperty, (object) 1);
      this.Children.Add((UIElement) this._durTextGrid);
      this.SetValueText(1);
      Line line2 = new Line();
      line2.Y1 = 0.0;
      line2.Y2 = 1.0;
      line2.Stretch = Stretch.Fill;
      line2.StrokeThickness = 1.0;
      line2.HorizontalAlignment = HorizontalAlignment.Right;
      line2.VerticalAlignment = VerticalAlignment.Top;
      line2.Margin = new Thickness(0.0, this._topMargin, 0.0, 40.0);
      line2.StrokeDashArray = new DoubleCollection()
      {
        3.0,
        3.0
      };
      Line element = line2;
      element.SetResourceReference(Shape.StrokeProperty, (object) "BaseColorOpacity10");
      this.Children.Add((UIElement) element);
    }

    private void SetLine(int hour) => this._lineNum = hour == 2 || hour == 4 ? 3 : 4;

    private void SetValueText(int hour)
    {
      int num1 = hour == 2 || hour == 4 ? 3 : 4;
      int num2 = hour * 60;
      if (this._durTextGrid.Children.Count == num1)
      {
        for (int index = 0; index < this._durTextGrid.Children.Count; ++index)
        {
          if (this._durTextGrid.Children[index] is TextBlock child)
          {
            Thickness margin = child.Margin;
            double top1 = margin.Top;
            margin = this._averageText.Margin;
            double top2 = margin.Top;
            bool flag = Math.Abs(top1 - top2) >= 14.0;
            child.Text = flag ? this.GetDurText(num2 * index / (num1 - 1)) : string.Empty;
          }
        }
      }
      else
      {
        this._durTextGrid.Children.Clear();
        int num3 = 120 / (num1 - 1);
        for (int index = 0; index < num1; ++index)
        {
          TextBlock textBlock = new TextBlock();
          textBlock.Margin = new Thickness(8.0, 120.0 + this._topMargin - (double) (num3 * index) - 8.0, 0.0, 0.0);
          textBlock.VerticalAlignment = VerticalAlignment.Top;
          TextBlock element = textBlock;
          Thickness margin = element.Margin;
          double top3 = margin.Top;
          margin = this._averageText.Margin;
          double top4 = margin.Top;
          bool flag = Math.Abs(top3 - top4) >= 14.0;
          element.Text = flag ? this.GetDurText(num2 * index / (num1 - 1)) : string.Empty;
          element.SetResourceReference(FrameworkElement.StyleProperty, (object) "Tag03");
          this._durTextGrid.Children.Add((UIElement) element);
        }
      }
    }

    private string GetDurText(int m)
    {
      return m < 60 ? m.ToString() + nameof (m) : (m / 60).ToString() + "h";
    }

    public void SetData(TimerTimelineItemViewModel model, Dictionary<string, long> statisticsData)
    {
      long num1 = statisticsData.Count > 0 ? statisticsData.Values.Max() : 0L;
      long num2 = statisticsData.Count > 0 ? statisticsData.Values.Sum() : 0L;
      long val2 = num1 / 60L + (long) (num1 % 60L > 0L);
      if (val2 > 6L)
        val2 += val2 % 3L > 0L ? 3L - val2 % 3L : 0L;
      long hour = Math.Max(1L, val2);
      long num3 = (long) Math.Max(statisticsData.Count<KeyValuePair<string, long>>((Func<KeyValuePair<string, long>, bool>) (kv => kv.Value > 0L)), 1);
      this.SetAverage((int) (num2 / num3), hour);
      this.SetValueText((int) hour);
      this.SetLine((int) hour);
      List<TimerDayItemViewModel> dayItemViewModelList = new List<TimerDayItemViewModel>();
      if (model.Interval == "year")
      {
        DateTime dateTime1 = model.StartDate;
        dateTime1 = dateTime1.AddMonths(1 - model.StartDate.Month);
        DateTime dateTime2 = dateTime1.AddDays((double) (1 - model.StartDate.Day));
        for (int months = 0; months < 12; ++months)
        {
          DateTime date = dateTime2.AddMonths(months);
          string key = DateUtils.GetDateNum(date).ToString();
          long minutes = statisticsData.ContainsKey(key) ? statisticsData[key] : 0L;
          dayItemViewModelList.Add(new TimerDayItemViewModel(date, minutes, model.Interval, hour * 60L));
        }
      }
      else
      {
        DateTime startDate = model.StartDate;
        int num4;
        if (!(model.Interval == "month"))
        {
          num4 = 7;
        }
        else
        {
          DateTime dateTime = startDate.AddMonths(1);
          dateTime = dateTime.AddDays(-1.0);
          num4 = dateTime.Day;
        }
        int num5 = num4;
        for (int index = 0; index < num5; ++index)
        {
          DateTime date = startDate.AddDays((double) index);
          string key = DateUtils.GetDateNum(date).ToString();
          long minutes = statisticsData.ContainsKey(key) ? statisticsData[key] : 0L;
          dayItemViewModelList.Add(new TimerDayItemViewModel(date, minutes, model.Interval, hour * 60L));
        }
      }
      this._items = dayItemViewModelList;
      this.InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
      base.OnRender(dc);
      Pen pen = new Pen()
      {
        Brush = (Brush) ThemeUtil.GetColor("BaseColorOpacity10"),
        DashStyle = new DashStyle((IEnumerable<double>) new double[2]
        {
          3.0,
          3.0
        }, 0.0),
        Thickness = 1.0
      };
      this.DrawLine(dc);
      SolidColorBrush color = ThemeUtil.GetColor("PrimaryColor");
      List<TimerDayItemViewModel> items = this._items;
      // ISSUE: explicit non-virtual call
      if ((items != null ? (__nonvirtual (items.Count) > 0 ? 1 : 0) : 0) == 0)
        return;
      double num1 = (this.ActualWidth - 56.0) / (double) this._items.Count;
      double num2 = (num1 - (double) this._items[0].ColumnWidth) / 2.0;
      for (int index = 0; index < this._items.Count; ++index)
      {
        TimerDayItemViewModel dayItemViewModel = this._items[index];
        if (dayItemViewModel.LineVisible)
          dc.DrawLine(pen, new System.Windows.Point((double) index * num1, this._topMargin), new System.Windows.Point((double) index * num1, 120.0 + this._topMargin));
        if (dayItemViewModel.ColumnHeight > 0)
        {
          Geometry geometry = this.GetGeometry(num2 + (double) index * num1, dayItemViewModel.ColumnWidth, dayItemViewModel.ColumnHeight);
          dc.DrawGeometry((Brush) color, (Pen) null, geometry);
        }
        if (!string.IsNullOrEmpty(num1 >= 16.0 || !dayItemViewModel.HideText ? dayItemViewModel.DayText : string.Empty))
          dc.DrawText(dayItemViewModel.TextFormat, new System.Windows.Point((double) index * num1 + (num1 - dayItemViewModel.TextFormat.Width) / 2.0, 130.0 + this._topMargin));
      }
    }

    private void DrawLine(DrawingContext dc)
    {
      Pen pen = new Pen()
      {
        Brush = (Brush) ThemeUtil.GetColor("BaseColorOpacity10"),
        Thickness = 1.0
      };
      int num = 120 / (this._lineNum - 1);
      double x = this.ActualWidth - 54.0;
      for (int index = 0; index < this._lineNum; ++index)
        dc.DrawLine(pen, new System.Windows.Point(0.0, this._topMargin + (double) (index * num)), new System.Windows.Point(x, this._topMargin + (double) (index * num)));
    }

    private Geometry GetGeometry(double x, int pathWidth, int pathHeight)
    {
      PathGeometry geometry = new PathGeometry();
      PathFigure pathFigure = new PathFigure()
      {
        StartPoint = new System.Windows.Point(x, 120.0 + this._topMargin),
        IsClosed = true
      };
      int num = Math.Min(pathWidth / 2, Math.Min(4, pathHeight));
      pathFigure.Segments.Add((PathSegment) new LineSegment()
      {
        Point = new System.Windows.Point(x, 120.0 + this._topMargin - (double) pathHeight + (double) num)
      });
      pathFigure.Segments.Add((PathSegment) new ArcSegment()
      {
        Size = new Size((double) num, (double) num),
        Point = new System.Windows.Point(x + (double) num, 120.0 + this._topMargin - (double) pathHeight),
        SweepDirection = SweepDirection.Clockwise
      });
      pathFigure.Segments.Add((PathSegment) new LineSegment()
      {
        Point = new System.Windows.Point(x + (double) pathWidth - (double) num, 120.0 + this._topMargin - (double) pathHeight)
      });
      pathFigure.Segments.Add((PathSegment) new ArcSegment()
      {
        Size = new Size((double) num, (double) num),
        Point = new System.Windows.Point(x + (double) pathWidth, 120.0 + this._topMargin - (double) pathHeight + (double) num),
        SweepDirection = SweepDirection.Clockwise
      });
      pathFigure.Segments.Add((PathSegment) new LineSegment()
      {
        Point = new System.Windows.Point(x + (double) pathWidth, 120.0 + this._topMargin)
      });
      geometry.Figures.Add(pathFigure);
      return (Geometry) geometry;
    }

    private void SetAverage(int avgMin, long hour)
    {
      if (avgMin == 0)
      {
        this._averageLine.Visibility = Visibility.Collapsed;
        this._averageText.Visibility = Visibility.Collapsed;
        this._averageLine.Margin = new Thickness(0.0, 160.0 + this._topMargin, 0.0, 0.0);
        this._averageText.Margin = new Thickness(8.0, 160.0 + this._topMargin, 0.0, 0.0);
      }
      else
      {
        this._averageLine.Visibility = Visibility.Visible;
        this._averageText.Visibility = Visibility.Visible;
        double top = 120.0 + this._topMargin - (double) ((long) (avgMin * 2) / hour);
        this._averageLine.Margin = new Thickness(0.0, top, 0.0, 0.0);
        this._averageText.Margin = new Thickness(8.0, top - 8.0, 0.0, 0.0);
        this._averageText.Text = Utils.GetShortDurationString((long) (avgMin * 60), false);
      }
    }

    public (double, TimerDayItemViewModel) GetHoverItem(MouseEventArgs e)
    {
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      if (position.Y < 120.0 + this._topMargin)
      {
        List<TimerDayItemViewModel> items = this._items;
        // ISSUE: explicit non-virtual call
        if ((items != null ? (__nonvirtual (items.Count) > 0 ? 1 : 0) : 0) != 0)
        {
          double num = (this.ActualWidth - 56.0) / (double) this._items.Count;
          int index = (int) (position.X / num);
          if (this._items.Count > index)
            return (((double) index + 0.5) * num, this._items[index]);
        }
      }
      return (0.0, (TimerDayItemViewModel) null);
    }
  }
}
