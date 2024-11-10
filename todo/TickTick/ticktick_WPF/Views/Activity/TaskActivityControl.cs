// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Activity.TaskActivityControl
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Views.Activity
{
  public class TaskActivityControl : UserControl, IComponentConnector, IStyleConnector
  {
    private ScrollViewer _scrollViewer;
    internal ListView Items;
    private bool _contentLoaded;

    public TaskActivityControl() => this.InitializeComponent();

    private void OnMouseWheel(object sender, MouseWheelEventArgs e)
    {
      ScrollViewer scrollViewer = this.GetScrollViewer();
      if (scrollViewer == null)
        return;
      scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - (double) e.Delta / 2.0);
      e.Handled = true;
    }

    private ScrollViewer GetScrollViewer()
    {
      if (this._scrollViewer == null && this.Items.Template.FindName("ScrollViewer", (FrameworkElement) this.Items) is ScrollViewer name)
        this._scrollViewer = name;
      return this._scrollViewer;
    }

    public void SetItems(List<TaskActivityViewModel> vms)
    {
      this.Items.ItemsSource = (IEnumerable) vms;
      this.GetScrollViewer()?.ScrollToTop();
    }

    private void Null_Handler(object sender, RequestBringIntoViewEventArgs e) => e.Handled = true;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/activity/taskactivitycontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
      {
        this.Items = (ListView) target;
        this.Items.PreviewMouseWheel += new MouseWheelEventHandler(this.OnMouseWheel);
      }
      else
        this._contentLoaded = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
        return;
      ((Style) target).Setters.Add((SetterBase) new EventSetter()
      {
        Event = FrameworkElement.RequestBringIntoViewEvent,
        Handler = (Delegate) new RequestBringIntoViewEventHandler(this.Null_Handler)
      });
    }
  }
}
