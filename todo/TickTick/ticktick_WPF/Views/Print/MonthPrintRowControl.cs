// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.MonthPrintRowControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Views.Calendar.Month;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class MonthPrintRowControl : UserControl, IComponentConnector
  {
    internal MonthPrintRowControl Root;
    internal Grid Container;
    internal Grid BottomLine;
    internal Grid DayTextGrid;
    internal TextBlock FirstDay;
    internal TextBlock SecondDay;
    internal TextBlock ThirdDay;
    internal TextBlock FourthDay;
    internal TextBlock FifthDay;
    internal TextBlock SixthDay;
    internal TextBlock SeventhDay;
    private bool _contentLoaded;

    public MonthPrintRowControl() => this.InitializeComponent();

    public MonthPrintRowControl(DateTime begin, int month)
    {
      this.InitializeComponent();
      this.Height = 44.0;
      this.DayTextGrid.Visibility = Visibility.Visible;
      this.FirstDay.Text = begin.Day.ToString();
      this.SecondDay.Text = begin.AddDays(1.0).Day.ToString();
      this.ThirdDay.Text = begin.AddDays(2.0).Day.ToString();
      this.FourthDay.Text = begin.AddDays(3.0).Day.ToString();
      this.FifthDay.Text = begin.AddDays(4.0).Day.ToString();
      this.SixthDay.Text = begin.AddDays(5.0).Day.ToString();
      this.SeventhDay.Text = begin.AddDays(6.0).Day.ToString();
      this.FirstDay.Opacity = month == 0 || begin.Month == month ? 1.0 : 0.56;
      this.SecondDay.Opacity = month == 0 || begin.AddDays(1.0).Month == month ? 1.0 : 0.56;
      this.ThirdDay.Opacity = month == 0 || begin.AddDays(2.0).Month == month ? 1.0 : 0.56;
      this.FourthDay.Opacity = month == 0 || begin.AddDays(3.0).Month == month ? 1.0 : 0.56;
      this.FifthDay.Opacity = month == 0 || begin.AddDays(4.0).Month == month ? 1.0 : 0.56;
      this.SixthDay.Opacity = month == 0 || begin.AddDays(5.0).Month == month ? 1.0 : 0.56;
      this.SeventhDay.Opacity = month == 0 || begin.AddDays(6.0).Month == month ? 1.0 : 0.56;
    }

    public MonthPrintRowControl(IEnumerable<WeekEventModel> rowData, bool showBottom)
    {
      this.InitializeComponent();
      if (showBottom)
        this.BottomLine.Visibility = Visibility.Visible;
      this.SetData(rowData, showBottom);
    }

    public void SetData(IEnumerable<WeekEventModel> rowData, bool showBottom, double topMargin = 0.0)
    {
      foreach (WeekEventModel weekEventModel in rowData)
      {
        if (weekEventModel.Data.LoadMoreWidth == 0.0 || weekEventModel.Data.IsLoadMore)
        {
          CalendarPrintCell calendarPrintCell = new CalendarPrintCell();
          calendarPrintCell.Margin = new Thickness(6.0, 3.0 + topMargin, 6.0, 3.0);
          CalendarPrintCell element = calendarPrintCell;
          element.SetValue(Grid.ColumnProperty, (object) weekEventModel.Column);
          element.SetValue(Grid.ColumnSpanProperty, (object) weekEventModel.ColumnSpan);
          element.Height = 25.0;
          this.Height = weekEventModel.Data.IsLoadMore ? 32.0 : 32.0 + topMargin;
          element.DataContext = (object) weekEventModel.Data;
          this.Container.Children.Add((UIElement) element);
        }
      }
      if (!showBottom)
        return;
      this.Height += 3.0;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/monthprintrowcontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (MonthPrintRowControl) target;
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          this.BottomLine = (Grid) target;
          break;
        case 4:
          this.DayTextGrid = (Grid) target;
          break;
        case 5:
          this.FirstDay = (TextBlock) target;
          break;
        case 6:
          this.SecondDay = (TextBlock) target;
          break;
        case 7:
          this.ThirdDay = (TextBlock) target;
          break;
        case 8:
          this.FourthDay = (TextBlock) target;
          break;
        case 9:
          this.FifthDay = (TextBlock) target;
          break;
        case 10:
          this.SixthDay = (TextBlock) target;
          break;
        case 11:
          this.SeventhDay = (TextBlock) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
