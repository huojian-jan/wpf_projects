// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Activity.ProjectActivityItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Kanban;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Activity
{
  public class ProjectActivityItem : UserControl, IComponentConnector
  {
    internal Run TaskTitle;
    internal EscPopup UnknownPopup;
    private bool _contentLoaded;

    public ProjectActivityItem() => this.InitializeComponent();

    private void OnTaskClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext == null || !(this.DataContext is ProjectActivityViewModel dataContext))
        return;
      if (dataContext.TaskTitle == Utils.GetString("Unknown"))
      {
        Mouse.Capture((IInputElement) null);
        this.UnknownPopup.IsOpen = true;
      }
      else
      {
        App.ShowMainWindow(dataContext.TaskId);
        this.CloseProjectActivityWindow();
      }
    }

    private void CloseProjectActivityWindow()
    {
      Utils.FindParent<ProjectActivityWindow>((DependencyObject) this)?.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/activity/projectactivityitem.xaml", UriKind.Relative));
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
          this.UnknownPopup = (EscPopup) target;
        else
          this._contentLoaded = true;
      }
      else
      {
        this.TaskTitle = (Run) target;
        this.TaskTitle.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTaskClick);
      }
    }
  }
}
