// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineHeaderHover
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineHeaderHover : Canvas
  {
    public static readonly DependencyProperty OneDayWidthProperty = DependencyProperty.Register(nameof (OneDayWidth), typeof (double), typeof (TimelineHeaderHover), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(TimelineHeaderHover.OnDependencyChanged)));
    public static readonly DependencyProperty StartEndTupleProperty = DependencyProperty.Register(nameof (StartEndTuple), typeof (Tuple<DateTime, DateTime>), typeof (TimelineHeaderHover), new PropertyMetadata((object) null, new PropertyChangedCallback(TimelineHeaderHover.OnDependencyChanged)));
    public static readonly DependencyProperty ShowWeekDayProperty = DependencyProperty.Register(nameof (ShowWeekDay), typeof (bool), typeof (TimelineHeaderHover), new PropertyMetadata((object) false, new PropertyChangedCallback(TimelineHeaderHover.OnDependencyChanged)));
    public static readonly DependencyProperty HoverStartEndTupleTuplesProperty = DependencyProperty.Register(nameof (HoverStartEndTuples), typeof (List<Tuple<DateTime, DateTime>>), typeof (TimelineHeaderHover), new PropertyMetadata((object) null, new PropertyChangedCallback(TimelineHeaderHover.OnDependencyChanged)));
    public static readonly DependencyProperty TimelineDayWidthIndexProperty = DependencyProperty.Register(nameof (TimelineDayWidthIndex), typeof (int), typeof (TimelineHeaderHover), new PropertyMetadata((object) 4, new PropertyChangedCallback(TimelineHeaderHover.OnDependencyChanged)));

    public double OneDayWidth
    {
      get => (double) this.GetValue(TimelineHeaderHover.OneDayWidthProperty);
      set => this.SetCurrentValue(TimelineHeaderHover.OneDayWidthProperty, (object) value);
    }

    public Tuple<DateTime, DateTime> StartEndTuple
    {
      get => (Tuple<DateTime, DateTime>) this.GetValue(TimelineHeaderHover.StartEndTupleProperty);
      set => this.SetCurrentValue(TimelineHeaderHover.StartEndTupleProperty, (object) value);
    }

    public bool ShowWeekDay
    {
      get => (bool) this.GetValue(TimelineHeaderHover.ShowWeekDayProperty);
      set => this.SetCurrentValue(TimelineHeaderHover.ShowWeekDayProperty, (object) value);
    }

    public List<Tuple<DateTime, DateTime>> HoverStartEndTuples
    {
      get
      {
        return (List<Tuple<DateTime, DateTime>>) this.GetValue(TimelineHeaderHover.HoverStartEndTupleTuplesProperty);
      }
      set
      {
        this.SetCurrentValue(TimelineHeaderHover.HoverStartEndTupleTuplesProperty, (object) value);
      }
    }

    public int TimelineDayWidthIndex
    {
      get => (int) this.GetValue(TimelineHeaderHover.TimelineDayWidthIndexProperty);
      set
      {
        this.SetCurrentValue(TimelineHeaderHover.TimelineDayWidthIndexProperty, (object) value);
      }
    }

    private static void OnDependencyChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      ((UIElement) d).InvalidateVisual();
    }

    public TimelineHeaderHover()
    {
      this.ClipToBounds = true;
      this.IsHitTestVisible = false;
      this.Loaded += (RoutedEventHandler) ((s, e) => LocalSettings.Settings.PropertyChanged += new PropertyChangedEventHandler(this.OnSettingsChanged));
      this.Unloaded += (RoutedEventHandler) ((s, e) => LocalSettings.Settings.PropertyChanged -= new PropertyChangedEventHandler(this.OnSettingsChanged));
    }

    private void DrawDayName(DrawingContext dc, string name, double x, double width)
    {
      FormattedText formattedText = new FormattedText(name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 12.0, (Brush) TimelineThemes.PrimaryColorBrush);
      dc.DrawText(formattedText, new System.Windows.Point(x + (width - formattedText.Width) / 2.0, this.Height - 14.0 - formattedText.Height / 2.0));
    }

    private void DrawWeekText(DrawingContext dc, string weekText, double x, double width)
    {
      FormattedText formattedText = new FormattedText(weekText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 10.0, (Brush) TimelineThemes.PrimaryColorBrush);
      dc.DrawText(formattedText, new System.Windows.Point(x + (width - formattedText.Width) / 2.0, 11.0 - formattedText.Height / 2.0));
    }

    private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "ThemeId"))
        return;
      this.InvalidateVisual();
    }

    private static void DrawRoundedRectangle(
      DrawingContext dc,
      Brush brush,
      Pen pen,
      Rect rect,
      CornerRadius cornerRadius)
    {
      StreamGeometry streamGeometry = new StreamGeometry();
      using (StreamGeometryContext streamGeometryContext1 = streamGeometry.Open())
      {
        bool isStroked = pen != null;
        streamGeometryContext1.BeginFigure(rect.TopLeft + new Vector(0.0, cornerRadius.TopLeft), brush != null, true);
        StreamGeometryContext streamGeometryContext2 = streamGeometryContext1;
        System.Windows.Point point1 = rect.TopLeft;
        double x1 = point1.X + cornerRadius.TopLeft;
        point1 = rect.TopLeft;
        double y1 = point1.Y;
        System.Windows.Point point2 = new System.Windows.Point(x1, y1);
        Size size1 = new Size(cornerRadius.TopLeft, cornerRadius.TopLeft);
        int num1 = isStroked ? 1 : 0;
        streamGeometryContext2.ArcTo(point2, size1, 90.0, false, SweepDirection.Clockwise, num1 != 0, true);
        streamGeometryContext1.LineTo(rect.TopRight - new Vector(cornerRadius.TopRight, 0.0), isStroked, true);
        StreamGeometryContext streamGeometryContext3 = streamGeometryContext1;
        point1 = rect.TopRight;
        double x2 = point1.X;
        point1 = rect.TopRight;
        double y2 = point1.Y + cornerRadius.TopRight;
        System.Windows.Point point3 = new System.Windows.Point(x2, y2);
        Size size2 = new Size(cornerRadius.TopRight, cornerRadius.TopRight);
        int num2 = isStroked ? 1 : 0;
        streamGeometryContext3.ArcTo(point3, size2, 90.0, false, SweepDirection.Clockwise, num2 != 0, true);
        streamGeometryContext1.LineTo(rect.BottomRight - new Vector(0.0, cornerRadius.BottomRight), isStroked, true);
        StreamGeometryContext streamGeometryContext4 = streamGeometryContext1;
        point1 = rect.BottomRight;
        double x3 = point1.X - cornerRadius.BottomRight;
        point1 = rect.BottomRight;
        double y3 = point1.Y;
        System.Windows.Point point4 = new System.Windows.Point(x3, y3);
        Size size3 = new Size(cornerRadius.BottomRight, cornerRadius.BottomRight);
        int num3 = isStroked ? 1 : 0;
        streamGeometryContext4.ArcTo(point4, size3, 90.0, false, SweepDirection.Clockwise, num3 != 0, true);
        streamGeometryContext1.LineTo(rect.BottomLeft + new Vector(cornerRadius.BottomLeft, 0.0), isStroked, true);
        StreamGeometryContext streamGeometryContext5 = streamGeometryContext1;
        point1 = rect.BottomLeft;
        double x4 = point1.X;
        point1 = rect.BottomLeft;
        double y4 = point1.Y - cornerRadius.BottomLeft;
        System.Windows.Point point5 = new System.Windows.Point(x4, y4);
        Size size4 = new Size(cornerRadius.BottomLeft, cornerRadius.BottomLeft);
        int num4 = isStroked ? 1 : 0;
        streamGeometryContext5.ArcTo(point5, size4, 90.0, false, SweepDirection.Clockwise, num4 != 0, true);
        streamGeometryContext1.Close();
      }
      dc.DrawGeometry(brush, pen, (Geometry) streamGeometry);
    }

    protected override void OnRender(DrawingContext dc)
    {
      base.OnRender(dc);
      if (this.ActualWidth <= 0.0 || this.ActualHeight <= 0.0 || this.StartEndTuple == null || this.HoverStartEndTuples == null)
        return;
      DateTime dateTime1;
      DateTime dateTime2;
      this.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime1, out dateTime2);
      DateTime dateTime3 = dateTime1;
      foreach (Tuple<DateTime, DateTime> tuple in this.HoverStartEndTuples.Where<Tuple<DateTime, DateTime>>((Func<Tuple<DateTime, DateTime>, bool>) (tuple => tuple != null)))
      {
        tuple.Deconstruct<DateTime, DateTime>(out dateTime2, out dateTime1);
        DateTime date = dateTime2;
        DateTime dateTime4 = dateTime1;
        date = date.Date;
        dateTime4 = dateTime4.Date;
        TimeSpan timeSpan = dateTime4 - date;
        int num = Math.Max(timeSpan.Days + 1, 1);
        double oneDayWidth = this.OneDayWidth;
        timeSpan = date - dateTime3;
        double x1 = (double) timeSpan.Days * oneDayWidth;
        TimelineHeaderHover.DrawRoundedRectangle(dc, (Brush) TimelineThemes.PrimaryColorBrush, (Pen) null, new Rect(x1, this.ActualHeight - 2.0, (double) num * oneDayWidth, 2.0), new CornerRadius(2.0));
        switch (TimelineConstants.GetDayWidthRange(this.TimelineDayWidthIndex))
        {
          case "month":
            if (num > 5)
            {
              double x2 = x1 + oneDayWidth / 2.0;
              string name = DateUtils.FormatMonthAndDay(date);
              this.DrawDayName(dc, name, x2 + 10.0, 0.0);
              string weekText = this.ShowWeekDay ? DateUtils.GetShortWeekTextByWeekDay((int) date.DayOfWeek) : string.Empty;
              if (!string.IsNullOrEmpty(weekText))
                this.DrawWeekText(dc, weekText, x2, 0.0);
              string str1 = DateUtils.FormatMonthAndDay(date.AddDays((double) (num - 1)));
              double width1 = Utils.MeasureString(str1, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 12.0).Width;
              this.DrawDayName(dc, str1, x2 + (double) num * oneDayWidth - width1 - oneDayWidth / 2.0, width1);
              string str2;
              if (!this.ShowWeekDay)
              {
                str2 = string.Empty;
              }
              else
              {
                dateTime1 = date.AddDays((double) (num - 1));
                str2 = DateUtils.GetShortWeekTextByWeekDay((int) dateTime1.DayOfWeek);
              }
              string str3 = str2;
              if (!string.IsNullOrEmpty(str3))
              {
                double width2 = Utils.MeasureString(str3, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 10.0).Width;
                this.DrawWeekText(dc, str3, x2 + (double) num * oneDayWidth - width2 - oneDayWidth / 2.0 - 5.0, width2);
                continue;
              }
              continue;
            }
            for (int index = 0; index < num; ++index)
            {
              this.DrawDayName(dc, date.Day.ToString(), x1, oneDayWidth);
              string weekText = this.ShowWeekDay ? DateUtils.GetShortWeekTextByWeekDay((int) date.DayOfWeek) : string.Empty;
              if (!string.IsNullOrEmpty(weekText))
                this.DrawWeekText(dc, weekText, x1, oneDayWidth);
              date = date.AddDays(1.0);
              x1 += oneDayWidth;
            }
            continue;
          default:
            for (int index = 0; index < num; ++index)
            {
              this.DrawDayName(dc, date.Day.ToString(), x1, oneDayWidth);
              string weekText = this.ShowWeekDay ? DateUtils.GetShortWeekTextByWeekDay((int) date.DayOfWeek) : string.Empty;
              if (!string.IsNullOrEmpty(weekText))
                this.DrawWeekText(dc, weekText, x1, oneDayWidth);
              date = date.AddDays(1.0);
              x1 += oneDayWidth;
            }
            continue;
        }
      }
    }
  }
}
