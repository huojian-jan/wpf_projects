// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineBackground
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineBackground : Canvas
  {
    public static readonly DependencyProperty LineCountProperty = DependencyProperty.Register(nameof (LineCount), typeof (int), typeof (TimelineBackground), new PropertyMetadata((object) 0));
    public static readonly DependencyProperty GroupDictModelsProperty = DependencyProperty.Register(nameof (GroupDictModels), typeof (Dictionary<int, bool>), typeof (TimelineBackground), new PropertyMetadata((object) null, new PropertyChangedCallback(TimelineBackground.OnDependencyChanged)));
    public static readonly DependencyProperty OneLineHeightProperty = DependencyProperty.Register(nameof (OneLineHeight), typeof (double), typeof (TimelineBackground), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(TimelineBackground.OnDependencyChanged)));
    public static readonly DependencyProperty OneDayWidthProperty = DependencyProperty.Register(nameof (OneDayWidth), typeof (double), typeof (TimelineBackground), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(TimelineBackground.OnDependencyChanged)));
    public static readonly DependencyProperty StartEndTupleProperty = DependencyProperty.Register(nameof (StartEndTuple), typeof (Tuple<DateTime, DateTime>), typeof (TimelineBackground), new PropertyMetadata((object) null, new PropertyChangedCallback(TimelineBackground.OnDependencyChanged)));
    public static readonly DependencyProperty TimelineDayWidthIndexProperty = DependencyProperty.Register(nameof (TimelineDayWidthIndex), typeof (int), typeof (TimelineBackground), new PropertyMetadata((object) 4, new PropertyChangedCallback(TimelineBackground.OnDependencyChanged)));

    public int LineCount
    {
      get => (int) this.GetValue(TimelineBackground.LineCountProperty);
      set => this.SetValue(TimelineBackground.LineCountProperty, (object) value);
    }

    public Dictionary<int, bool> GroupDictModels
    {
      get => (Dictionary<int, bool>) this.GetValue(TimelineBackground.GroupDictModelsProperty);
      set => this.SetValue(TimelineBackground.GroupDictModelsProperty, (object) value);
    }

    public double OneLineHeight
    {
      get => (double) this.GetValue(TimelineBackground.OneLineHeightProperty);
      set => this.SetValue(TimelineBackground.OneLineHeightProperty, (object) value);
    }

    public double OneDayWidth
    {
      get => (double) this.GetValue(TimelineBackground.OneDayWidthProperty);
      set => this.SetCurrentValue(TimelineBackground.OneDayWidthProperty, (object) value);
    }

    public Tuple<DateTime, DateTime> StartEndTuple
    {
      get => (Tuple<DateTime, DateTime>) this.GetValue(TimelineBackground.StartEndTupleProperty);
      set => this.SetCurrentValue(TimelineBackground.StartEndTupleProperty, (object) value);
    }

    public int TimelineDayWidthIndex
    {
      get => (int) this.GetValue(TimelineBackground.TimelineDayWidthIndexProperty);
      set => this.SetCurrentValue(TimelineBackground.TimelineDayWidthIndexProperty, (object) value);
    }

    private static void OnDependencyChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      ((UIElement) d).InvalidateVisual();
    }

    public TimelineBackground()
    {
      this.ClipToBounds = true;
      this.IsHitTestVisible = false;
      this.Loaded += (RoutedEventHandler) ((s, e) => LocalSettings.Settings.PropertyChanged += new PropertyChangedEventHandler(this.OnSettingsChanged));
      this.Unloaded += (RoutedEventHandler) ((s, e) => LocalSettings.Settings.PropertyChanged -= new PropertyChangedEventHandler(this.OnSettingsChanged));
    }

    private void OnSettingsChanged(object sender, PropertyChangedEventArgs e)
    {
      if (!(e.PropertyName == "ThemeId"))
        return;
      this.InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
      base.OnRender(dc);
      if (this.ActualWidth <= 0.0 || this.StartEndTuple == null)
        return;
      if (LocalSettings.Settings.ThemeId == "White" || LocalSettings.Settings.ThemeId == "Blue" || LocalSettings.Settings.ThemeId == "Gray")
        dc.DrawRectangle((Brush) ThemeUtil.GetColor("BaseColorOpacity3"), (Pen) null, new Rect(0.0, 0.0, this.ActualWidth, this.ActualHeight));
      else if (ThemeKey.IsProTheme(LocalSettings.Settings.ThemeId))
        dc.DrawRectangle((Brush) TimelineThemes.BackgroundBrush, (Pen) null, new Rect(0.0, 0.0, this.ActualWidth, this.ActualHeight));
      this.DrawDay(dc);
      this.DrawGroupHideModels(dc);
      this.DrawToday(dc);
    }

    private void DrawGroupHideModels(DrawingContext dc)
    {
      Dictionary<int, bool> groupDictModels = this.GroupDictModels;
      double oneLineHeight = this.OneLineHeight;
      SolidColorBrush solidColorBrush1 = new SolidColorBrush();
      solidColorBrush1.Color = Colors.White;
      solidColorBrush1.Opacity = 0.6;
      SolidColorBrush solidColorBrush2 = solidColorBrush1;
      if (LocalSettings.Settings.ThemeId == "Pink" || LocalSettings.Settings.ThemeId == "Green" || LocalSettings.Settings.ThemeId == "Yellow")
        solidColorBrush2.Opacity = 0.4;
      else if (ThemeKey.IsDarkTheme(LocalSettings.Settings.ThemeId))
        solidColorBrush2.Opacity = 0.05;
      Pen pen = new Pen()
      {
        Brush = (Brush) ThemeUtil.GetColor("BaseColorOpacity10")
      };
      int num1 = this.LineCount - 1 - 3;
      if (groupDictModels == null)
        return;
      foreach (KeyValuePair<int, bool> keyValuePair in groupDictModels)
      {
        int key = keyValuePair.Key;
        int num2 = keyValuePair.Value ? 1 : 0;
        double y = (double) key * oneLineHeight + 0.5;
        if (key != 0)
          dc.DrawLine(pen, new System.Windows.Point(0.0, y), new System.Windows.Point(this.ActualWidth, y));
        if (num2 == 0)
        {
          int num3 = key > 0 ? 1 : 0;
          dc.DrawRectangle((Brush) solidColorBrush2, (Pen) null, key == num1 ? new Rect(0.0, y + (double) num3, this.ActualWidth, this.ActualHeight - y - (double) num3) : new Rect(0.0, y + (double) num3, this.ActualWidth, oneLineHeight - (double) num3));
        }
      }
    }

    private void DrawToday(DrawingContext dc)
    {
      DateTime dateTime;
      this.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime, out DateTime _);
      double x = (double) (DateTime.Today - dateTime).Days * this.OneDayWidth + this.OneDayWidth / 2.0;
      Pen pen1 = new Pen();
      SolidColorBrush solidColorBrush = new SolidColorBrush(TimelineThemes.DayLineColor);
      solidColorBrush.Opacity = 0.2;
      pen1.Brush = (Brush) solidColorBrush;
      pen1.Thickness = 2.0;
      Pen pen2 = pen1;
      dc.DrawLine(pen2, new System.Windows.Point(x, 0.0), new System.Windows.Point(x, this.ActualHeight));
    }

    private void DrawDay(DrawingContext dc)
    {
      DateTime dateTime1;
      DateTime dateTime2;
      this.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime1, out dateTime2);
      DateTime startDate = dateTime1;
      DateTime dateTime3 = dateTime2;
      double oneDayWidth = this.OneDayWidth;
      int num = 0;
      while (startDate <= dateTime3)
      {
        double x = (double) num * oneDayWidth;
        if (DateUtils.IsWeekEnds(startDate))
          dc.DrawRectangle((Brush) TimelineThemes.WeekendBrush, (Pen) null, new Rect(x, 0.0, oneDayWidth, this.ActualHeight));
        startDate = startDate.AddDays(1.0);
        ++num;
      }
    }

    private void DrawMonth(DrawingContext dc)
    {
      Pen pen = new Pen()
      {
        Brush = (Brush) TimelineThemes.GridBrush
      };
      DateTime dateTime1;
      DateTime dateTime2;
      this.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime1, out dateTime2);
      DateTime dateTime3 = dateTime1;
      DateTime dateTime4 = dateTime2;
      double oneDayWidth = this.OneDayWidth;
      double x = 0.0;
      for (; dateTime3 <= dateTime4; dateTime3 = dateTime3.AddMonths(1))
      {
        double num = (double) DateTime.DaysInMonth(dateTime3.Year, dateTime3.Month) * oneDayWidth;
        dc.DrawLine(pen, new System.Windows.Point(x, 0.0), new System.Windows.Point(x, this.ActualHeight));
        x += num;
      }
    }

    private void DrawWeek(DrawingContext dc)
    {
      DateTime dateTime1;
      DateTime dateTime2;
      this.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime1, out dateTime2);
      DateTime dateTime3 = dateTime1;
      DateTime dateTime4 = dateTime2;
      double num1 = 0.0;
      double oneDayWidth = this.OneDayWidth;
      double num2 = oneDayWidth * 7.0;
      double num3 = oneDayWidth * 5.0;
      double width = oneDayWidth * 2.0;
      for (; dateTime3 <= dateTime4; dateTime3 = dateTime3.AddDays(7.0))
      {
        dc.DrawRectangle((Brush) TimelineThemes.WeekendBrush, (Pen) null, new Rect(num1 + num3, 0.0, width, this.ActualHeight));
        num1 += num2;
      }
    }
  }
}
