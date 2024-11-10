// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.MarkdownTest
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
namespace ticktick_WPF.Views.MarkDown
{
  public class MarkdownTest : Window, IComponentConnector
  {
    internal ScrollViewer Scroller;
    private bool _contentLoaded;

    public MarkdownTest() => this.InitializeComponent();

    private void MarkDownEditor_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/markdown/markdowntest.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        this.Scroller = (ScrollViewer) target;
      else
        this._contentLoaded = true;
    }
  }
}
