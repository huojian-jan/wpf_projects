// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.WeekPrint
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Calendar.Month;
using ticktick_WPF.Views.Calendar.Week;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class WeekPrint : UserControl, IComponentConnector
  {
    public double Left;
    public double Right;
    public double Top;
    public double Bottom;
    internal FlowDocumentPageViewer PageViewer;
    internal FlowDocument Doc;
    internal Run MonthText;
    internal BlockUIContainer Head;
    private bool _contentLoaded;

    public DateTime StartDate { get; }

    public List<CalendarTimelineDayViewModel> PointModels { get; }

    public List<WeekEventModel> AllDayModels { get; }

    public int Days { get; set; }

    public bool ShowWeekends { get; set; }

    public WeekPrint() => this.InitializeComponent();

    public WeekPrint(
      List<WeekEventModel> allDayModels,
      List<CalendarTimelineDayViewModel> pointModels,
      double pageWidth,
      double pageHeight,
      DateTime startDate,
      int days,
      bool showWeekends)
    {
      this.InitializeComponent();
      this.Doc.FontFamily = LocalSettings.Settings.ExtraSettings.AppFontFamily == "SourceHansansSC_CN" ? new FontFamily("Microsoft YaHei UI") : LocalSettings.Settings.FontFamily;
      this.AllDayModels = allDayModels;
      this.PointModels = pointModels;
      this.StartDate = startDate;
      this.Days = days;
      this.ShowWeekends = showWeekends;
      this.Doc.PageHeight = pageHeight;
      this.Doc.PageWidth = pageWidth;
      this.PageViewer.Height = pageHeight;
      double allDayHeight = 210.0;
      this.MonthText.Text = this.GetDateText(startDate);
      DateTime end = startDate.AddDays((double) (days + (showWeekends ? 0 : 2)));
      int month = DateUtils.GetCurrentMonthDate(startDate, end).Month;
      this.Head.Child = (UIElement) new MultiDayPrintHeadView(startDate, days, month, showWeekends);
      allDayModels = (allDayModels != null ? allDayModels.Where<WeekEventModel>((Func<WeekEventModel, bool>) (r => r.Column + r.ColumnSpan > 0 && r.Column < days)).ToList<WeekEventModel>() : (List<WeekEventModel>) null) ?? new List<WeekEventModel>();
      List<int> list1 = allDayModels.Select<WeekEventModel, int>((Func<WeekEventModel, int>) (r => r.Row)).ToList<int>();
      int num = list1.Any<int>() ? list1.Max() : 0;
      for (int index = 0; index <= num; ++index)
      {
        int row = index;
        List<WeekEventModel> list2 = allDayModels.Where<WeekEventModel>((Func<WeekEventModel, bool>) (data => data.Row == row)).ToList<WeekEventModel>();
        if (list2.Any<WeekEventModel>())
        {
          if (row == 0)
          {
            MultiDayAllDayPrintRowView allDayPrintRowView = new MultiDayAllDayPrintRowView(days, true);
            allDayPrintRowView.SetData((IEnumerable<WeekEventModel>) list2, row == num);
            this.Doc.Blocks.Add((Block) new BlockUIContainer((UIElement) allDayPrintRowView));
          }
          else
          {
            MultiDayAllDayPrintRowView allDayPrintRowView = new MultiDayAllDayPrintRowView(days, false);
            allDayPrintRowView.SetData((IEnumerable<WeekEventModel>) list2, row == num);
            this.Doc.Blocks.Add((Block) new BlockUIContainer((UIElement) allDayPrintRowView));
            double height = allDayPrintRowView.Height;
            if (allDayHeight + height > pageHeight - 197.0)
              allDayHeight = height;
            else
              allDayHeight += height;
          }
        }
      }
      this.AddPointModels(pointModels, pageWidth, pageHeight, allDayHeight);
    }

    private string GetDateText(DateTime startDate)
    {
      DateTime dateTime = startDate.AddDays(6.0);
      return startDate.Month != dateTime.Month ? startDate.ToString("Y", (IFormatProvider) App.Ci) + " - " + dateTime.ToString("Y", (IFormatProvider) App.Ci) : startDate.ToString("Y", (IFormatProvider) App.Ci);
    }

    private void AddPointModels(
      List<CalendarTimelineDayViewModel> pointModels,
      double pageWidth,
      double pageHeight,
      double allDayHeight)
    {
      int val2_1 = 0;
      int val2_2 = 0;
      foreach (CalendarTimelineDayViewModel pointModel in pointModels)
      {
        if (pointModel.TopTasks != null)
          val2_1 = Math.Max(pointModel.TopTasks.Count, val2_1);
        if (pointModel.BotTasks != null)
          val2_2 = Math.Max(pointModel.BotTasks.Count, val2_2);
        CalendarGeoHelper.AssemblyCells(pointModel.Cells, (pageWidth - 268.0) / (double) pointModels.Count + 6.0, pointModel.Date);
      }
      int height = CalendarGeoHelper.TopFolded ? (CalendarGeoHelper.GetEndHour() - CalendarGeoHelper.GetStartHour() + (CalendarGeoHelper.GetEndHour() == 24 ? 1 : 2)) * 120 : 2880;
      double num1 = pageHeight - 197.0 - allDayHeight;
      if (num1 > 30.0)
      {
        if (num1 < 100.0)
        {
          num1 = 25.0;
          this.Doc.Blocks.Add((Block) new BlockUIContainer((UIElement) new WeekTimelinePrintView((List<CalendarTimelineDayViewModel>) null, 15.0)));
        }
        else
        {
          Border border = new Border();
          border.Height = num1 - 10.0;
          border.ClipToBounds = true;
          border.Child = (UIElement) new WeekTimelinePrintView(pointModels, (double) height);
          this.Doc.Blocks.Add((Block) new BlockUIContainer((UIElement) border));
        }
      }
      else
      {
        num1 = pageHeight - 197.0;
        BlockCollection blocks = this.Doc.Blocks;
        Border border = new Border();
        border.Height = num1 - 10.0;
        border.ClipToBounds = true;
        border.Child = (UIElement) new WeekTimelinePrintView(pointModels, (double) height);
        BlockUIContainer blockUiContainer = new BlockUIContainer((UIElement) border);
        blocks.Add((Block) blockUiContainer);
      }
      double num2;
      for (double num3 = (double) height - num1 + 20.0; num3 > 0.0; num3 -= num2 - 20.0)
      {
        num2 = pageHeight - 197.0;
        Border border = new Border();
        border.Height = num2 - 10.0;
        border.ClipToBounds = true;
        WeekTimelinePrintView timelinePrintView = new WeekTimelinePrintView(pointModels, (double) height);
        timelinePrintView.VerticalAlignment = VerticalAlignment.Top;
        timelinePrintView.Margin = new Thickness(0.0, num3 - (double) height, 0.0, 0.0);
        border.Child = (UIElement) timelinePrintView;
        this.Doc.Blocks.Add((Block) new BlockUIContainer((UIElement) border));
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/weekprint.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.PageViewer = (FlowDocumentPageViewer) target;
          break;
        case 2:
          this.Doc = (FlowDocument) target;
          break;
        case 3:
          this.MonthText = (Run) target;
          break;
        case 4:
          this.Head = (BlockUIContainer) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
