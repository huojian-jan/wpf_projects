// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Print.TimePointLine
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
namespace ticktick_WPF.Views.Print
{
  public class TimePointLine : UserControl, IComponentConnector
  {
    internal TextBlock TimeText;
    internal Grid BottomLine;
    private bool _contentLoaded;

    public TimePointLine(string timeText, bool showBottom = true)
    {
      this.InitializeComponent();
      this.TimeText.Text = timeText;
      if (showBottom)
        return;
      this.BottomLine.Visibility = Visibility.Collapsed;
      this.TimeText.Visibility = Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/print/timepointline.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.BottomLine = (Grid) target;
        else
          this._contentLoaded = true;
      }
      else
        this.TimeText = (TextBlock) target;
    }
  }
}
