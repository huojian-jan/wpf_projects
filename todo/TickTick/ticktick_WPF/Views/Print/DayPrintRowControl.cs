// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.DayPrintRowControl
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
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Views.Print
{
  public class DayPrintRowControl : UserControl, IComponentConnector
  {
    internal TextBlock LeftText;
    internal CalendarPrintCell TaskPrintCell;
    private bool _contentLoaded;

    public DayPrintRowControl() => this.InitializeComponent();

    public DayPrintRowControl(string text, CalendarDisplayViewModel allDayTask)
    {
      this.InitializeComponent();
      if (text == null)
        this.LeftText.Visibility = Visibility.Collapsed;
      else
        this.LeftText.Text = text;
      if (allDayTask == null)
        this.TaskPrintCell.Visibility = Visibility.Collapsed;
      else
        this.TaskPrintCell.DataContext = (object) allDayTask;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/dayprintrowcontrol.xaml", UriKind.Relative));
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
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.TaskPrintCell = (CalendarPrintCell) target;
        else
          this._contentLoaded = true;
      }
      else
        this.LeftText = (TextBlock) target;
    }
  }
}
