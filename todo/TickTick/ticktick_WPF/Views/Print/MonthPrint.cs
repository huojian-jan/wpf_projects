// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.MonthPrint
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
using ticktick_WPF.Views.Calendar.Month;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class MonthPrint : UserControl, IComponentConnector
  {
    private int _month;
    private DateTime _startDate;
    internal MonthPrint Root;
    internal FlowDocument Doc;
    internal Run MonthText;
    internal Grid WeekPanel;
    internal TextBlock Sun;
    internal TextBlock Mon;
    internal TextBlock Tues;
    internal TextBlock Wed;
    internal TextBlock Thur;
    internal TextBlock Fri;
    internal TextBlock Sat;
    private bool _contentLoaded;

    public MonthPrint(List<List<WeekEventModel>> monthRowData, DateTime startDate)
    {
      this.InitializeComponent();
      this.Doc.FontFamily = LocalSettings.Settings.ExtraSettings.AppFontFamily == "SourceHansansSC_CN" ? new FontFamily("Microsoft YaHei UI") : LocalSettings.Settings.FontFamily;
      this._month = startDate.AddDays(7.0).Month;
      this._startDate = startDate;
      this.MonthText.Text = startDate.AddDays(7.0).ToString("y", (IFormatProvider) App.Ci);
      this.InitWeekText();
      this.AddWeekViews(monthRowData);
    }

    private void InitWeekText()
    {
      if (this._startDate.DayOfWeek == DayOfWeek.Monday)
      {
        this.Mon.SetValue(Grid.ColumnProperty, (object) 0);
        this.Tues.SetValue(Grid.ColumnProperty, (object) 1);
        this.Wed.SetValue(Grid.ColumnProperty, (object) 2);
        this.Thur.SetValue(Grid.ColumnProperty, (object) 3);
        this.Fri.SetValue(Grid.ColumnProperty, (object) 4);
        this.Sat.SetValue(Grid.ColumnProperty, (object) 5);
        this.Sun.SetValue(Grid.ColumnProperty, (object) 6);
      }
      else
      {
        if (this._startDate.DayOfWeek != DayOfWeek.Saturday)
          return;
        this.Sat.SetValue(Grid.ColumnProperty, (object) 0);
        this.Sun.SetValue(Grid.ColumnProperty, (object) 1);
        this.Mon.SetValue(Grid.ColumnProperty, (object) 2);
        this.Tues.SetValue(Grid.ColumnProperty, (object) 3);
        this.Wed.SetValue(Grid.ColumnProperty, (object) 4);
        this.Thur.SetValue(Grid.ColumnProperty, (object) 5);
        this.Fri.SetValue(Grid.ColumnProperty, (object) 6);
      }
    }

    private void AddWeekViews(List<List<WeekEventModel>> monthRowData)
    {
      int num = 0;
      foreach (List<WeekEventModel> weekRowData in monthRowData)
      {
        this.AddWeekView(weekRowData, num);
        ++num;
      }
    }

    private void AddWeekView(List<WeekEventModel> weekRowData, int num)
    {
      this.Doc.Blocks.Add((Block) new BlockUIContainer((UIElement) new MonthPrintRowControl(this._startDate.AddDays((double) (num * 7)), this._month)));
      // ISSUE: explicit non-virtual call
      if (weekRowData != null && __nonvirtual (weekRowData.Count) > 0)
      {
        int num1 = weekRowData.Select<WeekEventModel, int>((Func<WeekEventModel, int>) (r => r.Row)).Max();
        for (int index = 0; index <= num1; ++index)
        {
          int row = index;
          List<WeekEventModel> list = weekRowData.Where<WeekEventModel>((Func<WeekEventModel, bool>) (data => data.Row == row)).ToList<WeekEventModel>();
          if (list.Any<WeekEventModel>())
            this.Doc.Blocks.Add((Block) new BlockUIContainer((UIElement) new MonthPrintRowControl((IEnumerable<WeekEventModel>) list, row == num1)));
        }
      }
      else
        this.Doc.Blocks.Add((Block) new BlockUIContainer((UIElement) new MonthPrintRowControl((IEnumerable<WeekEventModel>) new List<WeekEventModel>(), true)));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/monthprint.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (MonthPrint) target;
          break;
        case 2:
          this.Doc = (FlowDocument) target;
          break;
        case 3:
          this.MonthText = (Run) target;
          break;
        case 4:
          this.WeekPanel = (Grid) target;
          break;
        case 5:
          this.Sun = (TextBlock) target;
          break;
        case 6:
          this.Mon = (TextBlock) target;
          break;
        case 7:
          this.Tues = (TextBlock) target;
          break;
        case 8:
          this.Wed = (TextBlock) target;
          break;
        case 9:
          this.Thur = (TextBlock) target;
          break;
        case 10:
          this.Fri = (TextBlock) target;
          break;
        case 11:
          this.Sat = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
