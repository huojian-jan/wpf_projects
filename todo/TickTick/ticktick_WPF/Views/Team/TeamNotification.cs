// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Team.TeamNotification
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
namespace ticktick_WPF.Views.Team
{
  public class TeamNotification : UserControl, IComponentConnector
  {
    internal TeamNotification Root;
    internal CheckBox DoOrUndo;
    internal CheckBox Create;
    internal CheckBox Delete;
    private bool _contentLoaded;

    public event EventHandler<bool> DoCheck;

    public event EventHandler<bool> CreateCheck;

    public event EventHandler<bool> DelCheck;

    public TeamNotification() => this.InitializeComponent();

    private void OnDoChecked(object sender, RoutedEventArgs e)
    {
      EventHandler<bool> doCheck = this.DoCheck;
      if (doCheck == null)
        return;
      doCheck((object) null, true);
    }

    private void OnDoUnChecked(object sender, RoutedEventArgs e)
    {
      EventHandler<bool> doCheck = this.DoCheck;
      if (doCheck == null)
        return;
      doCheck((object) null, false);
    }

    private void OnCreateChecked(object sender, RoutedEventArgs e)
    {
      EventHandler<bool> createCheck = this.CreateCheck;
      if (createCheck == null)
        return;
      createCheck((object) null, true);
    }

    private void OnCreateUnChecked(object sender, RoutedEventArgs e)
    {
      EventHandler<bool> createCheck = this.CreateCheck;
      if (createCheck == null)
        return;
      createCheck((object) null, false);
    }

    private void OnDelChecked(object sender, RoutedEventArgs e)
    {
      EventHandler<bool> delCheck = this.DelCheck;
      if (delCheck == null)
        return;
      delCheck((object) null, true);
    }

    private void OnDelUnChecked(object sender, RoutedEventArgs e)
    {
      EventHandler<bool> delCheck = this.DelCheck;
      if (delCheck == null)
        return;
      delCheck((object) null, false);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/team/teamnotification.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (TeamNotification) target;
          break;
        case 2:
          this.DoOrUndo = (CheckBox) target;
          this.DoOrUndo.Checked += new RoutedEventHandler(this.OnDoChecked);
          this.DoOrUndo.Unchecked += new RoutedEventHandler(this.OnDoUnChecked);
          break;
        case 3:
          this.Create = (CheckBox) target;
          this.Create.Checked += new RoutedEventHandler(this.OnCreateChecked);
          this.Create.Unchecked += new RoutedEventHandler(this.OnCreateUnChecked);
          break;
        case 4:
          this.Delete = (CheckBox) target;
          this.Delete.Checked += new RoutedEventHandler(this.OnDelChecked);
          this.Delete.Unchecked += new RoutedEventHandler(this.OnDelUnChecked);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
