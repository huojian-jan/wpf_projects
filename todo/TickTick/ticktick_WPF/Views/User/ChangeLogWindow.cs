// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.User.ChangeLogWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.User
{
  public class ChangeLogWindow : Window, IComponentConnector
  {
    internal Run VersionRun;
    internal TextBlock VersionContent;
    private bool _contentLoaded;

    public ChangeLogWindow(string version, string versionMessage)
    {
      this.InitializeComponent();
      this.VersionRun.Text = version;
      this.VersionContent.Text = versionMessage;
    }

    private void OnGotItClick(object sender, RoutedEventArgs e) => this.Close();

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/user/changelogwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.VersionRun = (Run) target;
          break;
        case 2:
          this.VersionContent = (TextBlock) target;
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnGotItClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
