// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Time.MonthPicker
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.Time
{
  public class MonthPicker : UserControl, IComponentConnector
  {
    private TickDatePicker _calendar;
    internal Grid MonthGrid;
    private bool _contentLoaded;

    public event EventHandler<DateTime> MonthSelected;

    public MonthPicker(DateTime date)
    {
      this.InitializeComponent();
      this.InitMonth(date);
    }

    private void InitMonth(DateTime date)
    {
      this._calendar = new TickDatePicker(new DateTime?(date), isStart: false, isMonthMode: true);
      this._calendar.MonthSelected -= new EventHandler<DateTime>(this.OnMonthSelected);
      this._calendar.MonthSelected += new EventHandler<DateTime>(this.OnMonthSelected);
      this.MonthGrid.Children.Add((UIElement) this._calendar);
      this._calendar.NextOrLastGrid.Margin = new Thickness(0.0, 0.0, 4.0, 0.0);
    }

    private void OnMonthSelected(object sender, DateTime date)
    {
      EventHandler<DateTime> monthSelected = this.MonthSelected;
      if (monthSelected == null)
        return;
      monthSelected((object) this, date);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/time/monthpicker.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.MonthGrid = (Grid) target;
      else
        this._contentLoaded = true;
    }
  }
}
