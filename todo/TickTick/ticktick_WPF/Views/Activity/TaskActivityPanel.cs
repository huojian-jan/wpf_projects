// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Activity.TaskActivityPanel
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
namespace ticktick_WPF.Views.Activity
{
  public class TaskActivityPanel : StackPanel, IComponentConnector
  {
    internal TextBlock ActivitiesName;
    internal TextBlock ActivityCountText;
    internal Border ActivityGrid;
    internal TaskActivityControl ActivityControl;
    private bool _contentLoaded;

    public TaskActivityPanel() => this.InitializeComponent();

    public event EventHandler Closed;

    private void OnCloseActivityClick(object sender, MouseButtonEventArgs e)
    {
      EventHandler closed = this.Closed;
      if (closed == null)
        return;
      closed((object) this, (EventArgs) e);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/activity/taskactivitypanel.xaml", UriKind.Relative));
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
          this.ActivitiesName = (TextBlock) target;
          break;
        case 2:
          this.ActivityCountText = (TextBlock) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseActivityClick);
          break;
        case 4:
          this.ActivityGrid = (Border) target;
          break;
        case 5:
          this.ActivityControl = (TaskActivityControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
