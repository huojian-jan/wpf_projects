// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineToolTip
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
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineToolTip : UserControl, IComponentConnector
  {
    internal StackPanel TitleTip;
    internal TextBlock DateTip;
    private bool _contentLoaded;

    public TimelineToolTip() => this.InitializeComponent();

    public void ShowTip(bool isDate)
    {
      this.TitleTip.Visibility = isDate ? Visibility.Collapsed : Visibility.Visible;
      this.DateTip.Visibility = !isDate ? Visibility.Collapsed : Visibility.Visible;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/timeline/timelinetooltip.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.DateTip = (TextBlock) target;
        else
          this._contentLoaded = true;
      }
      else
        this.TitleTip = (StackPanel) target;
    }
  }
}
