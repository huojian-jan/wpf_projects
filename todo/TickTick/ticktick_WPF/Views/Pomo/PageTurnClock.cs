// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.PageTurnClock
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Pomo
{
  public class PageTurnClock : UserControl, IComponentConnector
  {
    private int _second;
    private double _columnWidth;
    internal ColumnDefinition HourColumn;
    internal ColumnDefinition MinuteColumn;
    internal ColumnDefinition SecondColumn;
    internal PageTurn HourPage;
    internal PageTurn MinutePage;
    internal PageTurn SecondPage;
    private bool _contentLoaded;

    public PageTurnClock()
    {
      this.InitializeComponent();
      this.HourPage.SetVisibleAlways(false);
      this.MinutePage.SetVisibleAlways(false);
    }

    public async void SetSecond(int second, bool withAnimation = true)
    {
      if (this._second == second)
        return;
      this._second = second;
      int num1 = second / 60 % 60;
      int hour = second / 3600;
      int num2 = second % 3600 % 60;
      this.MinutePage.SetText(num1, withAnimation);
      this.SecondPage.SetText(num2, withAnimation);
      if (hour > 0 && this.HourPage.Visibility != Visibility.Visible)
      {
        this.MinuteColumn.Width = new GridLength(this._columnWidth * 1.1);
        this.SecondColumn.Width = this.MinuteColumn.Width;
        this.HourColumn.Width = this.MinuteColumn.Width;
        this.HourPage.Visibility = Visibility.Visible;
        this.HourPage.SetText(hour, withAnimation);
      }
      else if (hour == 0 && this.HourPage.Visibility == Visibility.Visible)
      {
        this.HourPage.SetText(hour, withAnimation);
        await Task.Delay(900);
        this.MinuteColumn.Width = new GridLength(this._columnWidth * 1.25);
        this.SecondColumn.Width = this.MinuteColumn.Width;
        this.HourColumn.Width = new GridLength(0.0);
        this.HourPage.Visibility = Visibility.Collapsed;
      }
      if (this.HourPage.Visibility != Visibility.Visible)
        return;
      this.HourPage.SetText(hour, withAnimation);
    }

    public void SetWidth(double width)
    {
      this._columnWidth = width;
      if (this.HourPage.Visibility == Visibility.Visible)
      {
        this.MinuteColumn.Width = new GridLength(this._columnWidth * 1.1);
        this.SecondColumn.Width = this.MinuteColumn.Width;
        this.HourColumn.Width = this.MinuteColumn.Width;
      }
      else
      {
        this.MinuteColumn.Width = new GridLength(width * 1.25);
        this.SecondColumn.Width = this.MinuteColumn.Width;
      }
      this.Height = width;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/pageturnclock.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.HourColumn = (ColumnDefinition) target;
          break;
        case 2:
          this.MinuteColumn = (ColumnDefinition) target;
          break;
        case 3:
          this.SecondColumn = (ColumnDefinition) target;
          break;
        case 4:
          this.HourPage = (PageTurn) target;
          break;
        case 5:
          this.MinutePage = (PageTurn) target;
          break;
        case 6:
          this.SecondPage = (PageTurn) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
