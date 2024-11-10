// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.DayPrint
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
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class DayPrint : UserControl, IComponentConnector
  {
    public WeekDayCellViewModel PointModel;
    public DateTime Date;
    internal FlowDocumentPageViewer PageViewer;
    internal FlowDocument Doc;
    internal Run MonthText;
    internal Run WeekdayText;
    internal BlockUIContainer CenterLine;
    internal BlockUIContainer BottomLine;
    private bool _contentLoaded;

    public List<CalendarDisplayViewModel> AllDayTasks { get; set; }

    public bool IsAll { get; set; }

    public DayPrint() => this.InitializeComponent();

    public DayPrint(
      List<CalendarDisplayViewModel> allDayTasks,
      WeekDayCellViewModel pointModel,
      double docWidth,
      double docHeight,
      DateTime date,
      bool isAll)
    {
      this.InitializeComponent();
      this.Doc.FontFamily = LocalSettings.Settings.ExtraSettings.AppFontFamily == "SourceHansansSC_CN" ? new FontFamily("Microsoft YaHei UI") : LocalSettings.Settings.FontFamily;
      this.Doc.PageHeight = docHeight;
      this.Doc.PageWidth = docWidth;
      this.Doc.ColumnWidth = docWidth;
      this.PageViewer.Height = docHeight;
      this.AllDayTasks = allDayTasks;
      this.PointModel = pointModel;
      this.IsAll = isAll;
      this.Date = date;
      int allDayHeight = 160;
      this.MonthText.Text = DateUtils.FormatFullDate(date);
      this.WeekdayText.Text = "   " + date.ToString("ddd", (IFormatProvider) App.Ci);
      // ISSUE: explicit non-virtual call
      if (allDayTasks != null && __nonvirtual (allDayTasks.Count) > 0)
      {
        for (int index = 0; index < allDayTasks.Count; ++index)
        {
          this.Doc.Blocks.InsertBefore((Block) this.CenterLine, (Block) new BlockUIContainer((UIElement) new DayPrintRowControl(index == 0 ? Utils.GetString("AllDay") : (string) null, allDayTasks[index])));
          allDayHeight += 50;
        }
      }
      else
      {
        this.Doc.Blocks.InsertBefore((Block) this.CenterLine, (Block) new BlockUIContainer((UIElement) new DayPrintRowControl(Utils.GetString("AllDay"), (CalendarDisplayViewModel) null)));
        allDayHeight += 50;
      }
      if (isAll)
        this.DisplayByTimeLine(pointModel, docWidth, docHeight, (double) allDayHeight);
      else
        this.DisplayByStartTime();
    }

    private void DisplayByTimeLine(
      WeekDayCellViewModel pointModel,
      double pageWidth,
      double pageHeight,
      double allDayHeight)
    {
      CalendarGeoHelper.AssemblyCells(pointModel.Cells, pageWidth - 268.0 + 6.0, pointModel.Date);
      int num1;
      if (CalendarGeoHelper.TopFolded)
      {
        List<CalendarDisplayModel> topTasks = pointModel.TopTasks;
        // ISSUE: explicit non-virtual call
        if ((topTasks != null ? (__nonvirtual (topTasks.Count) > 1 ? 1 : 0) : 0) != 0)
        {
          num1 = pointModel.TopTasks.Count * 30 + 20;
          goto label_4;
        }
      }
      num1 = 60;
label_4:
      double num2 = (double) num1;
      int num3;
      if (CalendarGeoHelper.TopFolded)
      {
        List<CalendarDisplayModel> botTasks = pointModel.BotTasks;
        // ISSUE: explicit non-virtual call
        if ((botTasks != null ? (__nonvirtual (botTasks.Count) > 1 ? 1 : 0) : 0) != 0)
        {
          num3 = pointModel.BotTasks.Count * 30 + 20;
          goto label_8;
        }
      }
      num3 = 60;
label_8:
      double num4 = (double) num3;
      double num5 = CalendarGeoHelper.TopFolded ? (double) ((CalendarGeoHelper.GetEndHour() - CalendarGeoHelper.GetStartHour()) * 120) + num2 + num4 : 2880.0;
      double num6 = pageHeight - 197.0 - allDayHeight;
      if (num6 > 100.0)
      {
        DayTimePointPrintPanel timePointPrintPanel = new DayTimePointPrintPanel(pointModel);
        timePointPrintPanel.Height = num6 - 10.0;
        timePointPrintPanel.Container.Height = num5;
        this.Doc.Blocks.InsertBefore((Block) this.BottomLine, (Block) new BlockUIContainer((UIElement) timePointPrintPanel));
      }
      else
      {
        num6 = pageHeight - 197.0;
        BlockCollection blocks = this.Doc.Blocks;
        BlockUIContainer bottomLine = this.BottomLine;
        DayTimePointPrintPanel timePointPrintPanel = new DayTimePointPrintPanel(pointModel);
        timePointPrintPanel.Height = num6 - 10.0;
        timePointPrintPanel.Container.Height = num5;
        BlockUIContainer newItem = new BlockUIContainer((UIElement) timePointPrintPanel);
        blocks.InsertBefore((Block) bottomLine, (Block) newItem);
      }
      double num7;
      for (double val2 = num5 - num6 + 20.0; val2 > 0.0; val2 -= num7 - 20.0)
      {
        num7 = pageHeight - 197.0;
        DayTimePointPrintPanel timePointPrintPanel = new DayTimePointPrintPanel(pointModel);
        timePointPrintPanel.Height = Math.Min(num7 - 10.0, val2);
        timePointPrintPanel.Container.Margin = new Thickness(0.0, val2 - num5, 0.0, 0.0);
        timePointPrintPanel.Container.Height = num5;
        this.Doc.Blocks.InsertBefore((Block) this.BottomLine, (Block) new BlockUIContainer((UIElement) timePointPrintPanel));
      }
    }

    private void DisplayByStartTime()
    {
      WeekDayCellViewModel pointModel = this.PointModel;
      if ((pointModel != null ? (pointModel.Cells.Count > 0 ? 1 : 0) : 0) != 0)
      {
        List<TaskCellViewModel> cells = this.PointModel.Cells;
        cells.Sort((Comparison<TaskCellViewModel>) ((a, b) =>
        {
          if (!a.DisplayStartDate.HasValue || !b.DisplayStartDate.HasValue)
            return 0;
          DateTime dateTime1 = a.DisplayStartDate.Value;
          DateTime? displayStartDate = b.DisplayStartDate;
          DateTime dateTime2 = displayStartDate.Value;
          if (!(dateTime1 == dateTime2))
          {
            displayStartDate = a.DisplayStartDate;
            DateTime dateTime3 = displayStartDate.Value;
            ref DateTime local = ref dateTime3;
            displayStartDate = b.DisplayStartDate;
            DateTime dateTime4 = displayStartDate.Value;
            return local.CompareTo(dateTime4);
          }
          return a.Status != b.Status ? a.Status.CompareTo(b.Status) : b.Priority.CompareTo(a.Priority);
        }));
        DateTime dateTime = this.Date.AddDays(-1.0);
        bool flag = true;
        foreach (TaskCellViewModel allDayTask in cells.Where<TaskCellViewModel>((Func<TaskCellViewModel, bool>) (cell => cell.DisplayStartDate.HasValue)))
        {
          DateTime? displayStartDate = allDayTask.DisplayStartDate;
          if (displayStartDate.Value != dateTime)
          {
            if (!flag)
            {
              BlockCollection blocks = this.Doc.Blocks;
              BlockUIContainer bottomLine = this.BottomLine;
              DayPrintRowControl dayPrintRowControl = new DayPrintRowControl((string) null, (CalendarDisplayViewModel) null);
              dayPrintRowControl.Height = 20.0;
              BlockUIContainer newItem = new BlockUIContainer((UIElement) dayPrintRowControl);
              blocks.InsertBefore((Block) bottomLine, (Block) newItem);
            }
            BlockCollection blocks1 = this.Doc.Blocks;
            BlockUIContainer bottomLine1 = this.BottomLine;
            displayStartDate = allDayTask.DisplayStartDate;
            BlockUIContainer newItem1 = new BlockUIContainer((UIElement) new DayPrintRowControl(displayStartDate.Value.ToString("HH:mm"), (CalendarDisplayViewModel) allDayTask));
            blocks1.InsertBefore((Block) bottomLine1, (Block) newItem1);
            displayStartDate = allDayTask.DisplayStartDate;
            dateTime = displayStartDate.Value;
            flag = false;
          }
          else
            this.Doc.Blocks.InsertBefore((Block) this.BottomLine, (Block) new BlockUIContainer((UIElement) new DayPrintRowControl((string) null, (CalendarDisplayViewModel) allDayTask)));
        }
      }
      else
        this.Doc.Blocks.InsertBefore((Block) this.BottomLine, (Block) new BlockUIContainer((UIElement) new DayPrintRowControl((string) null, (CalendarDisplayViewModel) null)));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/dayprint.xaml", UriKind.Relative));
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
          this.WeekdayText = (Run) target;
          break;
        case 5:
          this.CenterLine = (BlockUIContainer) target;
          break;
        case 6:
          this.BottomLine = (BlockUIContainer) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
