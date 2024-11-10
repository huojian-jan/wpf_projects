// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineHeader
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineHeader : Canvas
  {
    public static readonly DependencyProperty OneDayWidthProperty = DependencyProperty.Register(nameof (OneDayWidth), typeof (double), typeof (TimelineHeader), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(TimelineHeader.OnDependencyChanged)));
    public static readonly DependencyProperty StartEndTupleProperty = DependencyProperty.Register(nameof (StartEndTuple), typeof (Tuple<DateTime, DateTime>), typeof (TimelineHeader), new PropertyMetadata((object) null, new PropertyChangedCallback(TimelineHeader.OnDependencyChanged)));
    public static readonly DependencyProperty ShowWeekDayProperty = DependencyProperty.Register(nameof (ShowWeekDay), typeof (bool), typeof (TimelineHeader), new PropertyMetadata((object) false, new PropertyChangedCallback(TimelineHeader.OnDependencyChanged)));
    public static readonly DependencyProperty ShowHolidayProperty = DependencyProperty.Register(nameof (ShowHoliday), typeof (bool), typeof (TimelineHeader), new PropertyMetadata((object) false, new PropertyChangedCallback(TimelineHeader.OnDependencyChanged)));
    public static readonly DependencyProperty HoverStartEndTupleTupleProperty = DependencyProperty.Register(nameof (HoverStartEndTuple), typeof (List<Tuple<DateTime, DateTime>>), typeof (TimelineHeader), new PropertyMetadata((object) null, new PropertyChangedCallback(TimelineHeader.OnDependencyChanged)));
    public static readonly DependencyProperty TimelineDayWidthIndexProperty = DependencyProperty.Register(nameof (TimelineDayWidthIndex), typeof (int), typeof (TimelineHeader), new PropertyMetadata((object) 4, new PropertyChangedCallback(TimelineHeader.OnDependencyChanged)));
    private DelayActionHandler _delayRender;

    public double OneDayWidth
    {
      get => (double) this.GetValue(TimelineHeader.OneDayWidthProperty);
      set => this.SetCurrentValue(TimelineHeader.OneDayWidthProperty, (object) value);
    }

    public Tuple<DateTime, DateTime> StartEndTuple
    {
      get => (Tuple<DateTime, DateTime>) this.GetValue(TimelineHeader.StartEndTupleProperty);
      set => this.SetCurrentValue(TimelineHeader.StartEndTupleProperty, (object) value);
    }

    public bool ShowWeekDay
    {
      get => (bool) this.GetValue(TimelineHeader.ShowWeekDayProperty);
      set => this.SetCurrentValue(TimelineHeader.ShowWeekDayProperty, (object) value);
    }

    public bool ShowHoliday
    {
      get => (bool) this.GetValue(TimelineHeader.ShowHolidayProperty);
      set => this.SetCurrentValue(TimelineHeader.ShowHolidayProperty, (object) value);
    }

    public List<Tuple<DateTime, DateTime>> HoverStartEndTuple
    {
      get
      {
        return (List<Tuple<DateTime, DateTime>>) this.GetValue(TimelineHeader.HoverStartEndTupleTupleProperty);
      }
      set => this.SetCurrentValue(TimelineHeader.HoverStartEndTupleTupleProperty, (object) value);
    }

    public int TimelineDayWidthIndex
    {
      get => (int) this.GetValue(TimelineHeader.TimelineDayWidthIndexProperty);
      set => this.SetCurrentValue(TimelineHeader.TimelineDayWidthIndexProperty, (object) value);
    }

    private static void OnDependencyChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      ((UIElement) d).InvalidateVisual();
    }

    public TimelineHeader()
    {
      this.ClipToBounds = true;
      this.IsHitTestVisible = false;
      this._delayRender = new DelayActionHandler(20);
      this._delayRender.DoAction += (EventHandler) ((o, e) => this.Dispatcher.Invoke(new Action(((UIElement) this).InvalidateVisual)));
      this.Loaded += (RoutedEventHandler) ((s, e) => LocalSettings.Settings.PropertyChanged += new PropertyChangedEventHandler(this.OnSettingsChanged));
      this.Unloaded += (RoutedEventHandler) ((s, e) => LocalSettings.Settings.PropertyChanged -= new PropertyChangedEventHandler(this.OnSettingsChanged));
    }

    private void DrawWeekName(
      DrawingContext dc,
      string name,
      double x,
      double width,
      bool isToday,
      string weekDayText = null)
    {
      SolidColorBrush foreground = isToday ? new SolidColorBrush(TimelineThemes.DayLineColor) : TimelineThemes.NormalDayBrush;
      FormattedText formattedText = new FormattedText(name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 12.0, (Brush) foreground);
      dc.DrawText(formattedText, new System.Windows.Point(x + (width - formattedText.Width) / 2.0, this.Height - 14.0 - formattedText.Height / 2.0));
      this.DragWeekText(dc, weekDayText, isToday, x, width);
    }

    private void DragWeekText(
      DrawingContext dc,
      string weekDayText,
      bool isToday,
      double x,
      double width)
    {
      if (string.IsNullOrEmpty(weekDayText))
        return;
      SolidColorBrush foreground = isToday ? new SolidColorBrush(TimelineThemes.DayLineColor) : TimelineThemes.WeekendDayBrush;
      FormattedText formattedText = new FormattedText(weekDayText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 10.0, (Brush) foreground);
      dc.DrawText(formattedText, new System.Windows.Point(x + (width - formattedText.Width) / 2.0, 11.0 - formattedText.Height / 2.0));
    }

    private void DrawDayName(
      DrawingContext dc,
      string name,
      double x,
      double width,
      bool isToday,
      bool isWeekends = false,
      bool drawText = true,
      bool isRest = false,
      bool isWork = false,
      string weekDayText = null)
    {
      if (!drawText && !isRest && !isWork)
        return;
      SolidColorBrush foreground = isToday ? new SolidColorBrush(TimelineThemes.DayLineColor) : (isWeekends ? TimelineThemes.WeekendDayBrush : TimelineThemes.NormalDayBrush);
      FormattedText formattedText = new FormattedText(name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 12.0, (Brush) foreground);
      if (isRest | isWork)
      {
        Rect rectangle = new Rect(x + (width + formattedText.Width + 2.0) / 2.0, this.Height - 28.0 + 3.0, 10.0, 10.0);
        DrawingImage imageSource = Utils.GetImageSource(isRest ? "RestDrawingImage" : "WorkDrawingImage");
        dc.DrawImage((ImageSource) imageSource, rectangle);
      }
      if (!drawText)
        return;
      dc.DrawText(formattedText, new System.Windows.Point(x + (width - formattedText.Width) / 2.0, this.Height - 14.0 - formattedText.Height / 2.0));
      this.DragWeekText(dc, weekDayText, isToday, x, width);
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
      switch (TimelineConstants.GetDayWidthRange(this.TimelineDayWidthIndex))
      {
        case "year":
          break;
        case "month":
          this.DrawWeek(dc);
          break;
        default:
          this.DrawDay(dc);
          break;
      }
    }

    private bool DateInHover(DateTime dateTime)
    {
      if (this.HoverStartEndTuple == null)
        return false;
      foreach (Tuple<DateTime, DateTime> tuple in this.HoverStartEndTuple)
      {
        if (tuple != null && dateTime >= tuple.Item1 && dateTime <= tuple.Item2)
          return true;
      }
      return false;
    }

    private void DrawWeek(DrawingContext dc)
    {
      DateTime dateTime1;
      DateTime dateTime2;
      this.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime1, out dateTime2);
      DateTime dateTime3 = dateTime1;
      DateTime dateTime4 = dateTime2;
      double x1 = 0.0;
      double num1 = this.OneDayWidth * 7.0;
      int day;
      if (!this.DateInHover(DateTime.Today))
      {
        string str = this.ShowWeekDay ? DateUtils.GetShortWeekTextByWeekDay((int) DateTime.Today.DayOfWeek) : string.Empty;
        double num2 = (double) (DateTime.Today - dateTime3).Days * this.OneDayWidth;
        DrawingContext dc1 = dc;
        day = DateTime.Today.Day;
        string name = day.ToString();
        double x2 = num2;
        double oneDayWidth = this.OneDayWidth;
        string weekDayText = str;
        this.DrawWeekName(dc1, name, x2, oneDayWidth, true, weekDayText);
      }
      for (; dateTime3 <= dateTime4; dateTime3 = dateTime3.AddDays(7.0))
      {
        day = dateTime3.Day;
        string name = day.ToString();
        if (this.DateInHover(dateTime3) || dateTime3 == DateTime.Today)
        {
          x1 += num1;
        }
        else
        {
          string weekDayText = this.ShowWeekDay ? DateUtils.GetShortWeekTextByWeekDay(1) : string.Empty;
          this.DrawWeekName(dc, name, x1, this.OneDayWidth, false, weekDayText);
          x1 += num1;
        }
      }
    }

    private void DrawDay(DrawingContext dc)
    {
      DateTime dateTime1;
      DateTime dateTime2;
      this.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime1, out dateTime2);
      DateTime dateTime3 = dateTime1;
      DateTime dateTime4 = dateTime2;
      double oneDayWidth = this.OneDayWidth;
      int num = 0;
      while (dateTime3 <= dateTime4)
      {
        double x = (double) num * oneDayWidth;
        bool isToday = dateTime3.Date == DateTime.Today;
        bool isRest = false;
        bool isWork = false;
        if (this.ShowHoliday && Utils.IsDida())
        {
          HolidayModel holiday = HolidayManager.GetHoliday(dateTime3.Date);
          if (holiday != null)
          {
            isRest = holiday.type == 0;
            isWork = holiday.type == 1;
          }
        }
        bool drawText = !this.DateInHover(dateTime3);
        string name = dateTime3.Day.ToString();
        string weekDayText = this.ShowWeekDay ? DateUtils.GetShortWeekTextByWeekDay((int) dateTime3.DayOfWeek) : string.Empty;
        this.DrawDayName(dc, name, x, oneDayWidth, isToday, DateUtils.IsWeekEnds(dateTime3), drawText, isRest, isWork, weekDayText);
        dateTime3 = dateTime3.AddDays(1.0);
        ++num;
      }
    }

    private void DrawMonth(DrawingContext dc)
    {
      DateTime dateTime1;
      DateTime dateTime2;
      this.StartEndTuple.Deconstruct<DateTime, DateTime>(out dateTime1, out dateTime2);
      DateTime date = dateTime1;
      DateTime dateTime3 = dateTime2;
      double oneDayWidth = this.OneDayWidth;
      double x = 0.0;
      for (; date <= dateTime3; date = date.AddMonths(1))
      {
        double width = (double) DateTime.DaysInMonth(date.Year, date.Month) * oneDayWidth;
        string name = DateUtils.FormatMonth(date);
        int year1 = date.Year;
        DateTime today = DateTime.Today;
        int year2 = today.Year;
        int num;
        if (year1 == year2)
        {
          int month1 = date.Month;
          today = DateTime.Today;
          int month2 = today.Month;
          num = month1 == month2 ? 1 : 0;
        }
        else
          num = 0;
        bool isToday = num != 0;
        this.DrawDayName(dc, name, x, width, isToday);
        x += width;
      }
    }
  }
}
