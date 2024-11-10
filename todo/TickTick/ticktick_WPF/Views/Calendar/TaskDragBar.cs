// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TaskDragBar
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TaskDragBar : UserControl, IComponentConnector
  {
    private bool _contentLoaded;

    public event EventHandler<MouseButtonEventArgs> TaskDrop;

    public event EventHandler<MouseEventArgs> TaskMove;

    public TaskDragBar() => this.InitializeComponent();

    public TaskDragBar(CalendarDisplayViewModel model)
    {
      this.InitializeComponent();
      this.DataContext = (object) model;
    }

    private void OnDrop(object sender, MouseButtonEventArgs e)
    {
      EventHandler<MouseButtonEventArgs> taskDrop = this.TaskDrop;
      if (taskDrop == null)
        return;
      taskDrop((object) this, e);
    }

    private void OnMove(object sender, MouseEventArgs e)
    {
      EventHandler<MouseEventArgs> taskMove = this.TaskMove;
      if (taskMove == null)
        return;
      taskMove((object) this, e);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/taskdragbar.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
      {
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDrop);
        ((UIElement) target).MouseMove += new MouseEventHandler(this.OnMove);
      }
      else
        this._contentLoaded = true;
    }
  }
}
