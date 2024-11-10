// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.TimerDetail.TimerDayItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Pomo.TimerDetail
{
  public class TimerDayItemViewModel : BaseViewModel
  {
    private double _itemWidth;
    private int _itemMinWidth;
    private int _columnWidth;
    private int _columnHeight;
    private string _dayText;
    public bool HideText;

    public double ItemWidth
    {
      get => this._itemWidth;
      set
      {
        if (Math.Abs(this._itemWidth - value) <= 0.001)
          return;
        this._itemWidth = value;
        this.OnPropertyChanged(nameof (ItemWidth));
        this.OnPropertyChanged("DayText");
      }
    }

    public int ItemMinWidth
    {
      get => this._itemMinWidth;
      set
      {
        if (this._itemMinWidth == value)
          return;
        this._itemMinWidth = value;
        this.OnPropertyChanged(nameof (ItemMinWidth));
      }
    }

    public int ColumnWidth
    {
      get => this._columnWidth;
      set
      {
        if (this._columnWidth == value)
          return;
        this._columnWidth = value;
        this.OnPropertyChanged(nameof (ColumnWidth));
      }
    }

    public int ColumnHeight
    {
      get => this._columnHeight;
      set
      {
        if (this._columnHeight == value)
          return;
        this._columnHeight = value;
        this.OnPropertyChanged(nameof (ColumnHeight));
      }
    }

    public string DayText
    {
      get => this._dayText;
      set
      {
        this._dayText = value;
        this.OnPropertyChanged(nameof (DayText));
      }
    }

    public bool LineVisible { get; set; } = true;

    public TimerDayItemViewModel(DateTime date, long minutes, string interval, long maxMinutes)
    {
      this.Date = date;
      this.Minutes = minutes;
      this.Interval = interval;
      switch (interval)
      {
        case "year":
          this._dayText = date.Month.ToString();
          this._columnWidth = 12;
          break;
        case "month":
          this._dayText = date.Day.ToString();
          this._columnWidth = 5;
          this.HideText = date.Day % 2 == 0;
          this.LineVisible = date.Day % 7 == 1;
          break;
        case "week":
          this._dayText = DateUtils.GetShortWeekTextByWeekDay((int) date.DayOfWeek);
          this._columnWidth = 20;
          break;
      }
      this.TextFormat = new FormattedText(this.DayText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(LocalSettings.Settings.FontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 12.0, (Brush) ThemeUtil.GetColor("BaseColorOpacity40"));
      this._columnHeight = (int) (minutes * 120L / maxMinutes);
    }

    public DateTime Date { get; set; }

    public long Minutes { get; set; }

    public string Interval { get; set; }

    public FormattedText TextFormat { get; set; }

    public string GetDateText()
    {
      return !(this.Interval == "year") ? DateUtils.FormatShortDate(this.Date) : DateUtils.FormatYearMonth(this.Date);
    }
  }
}
