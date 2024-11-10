// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.UiTest
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
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views
{
  public class UiTest : UserControl, IComponentConnector
  {
    internal PathFigure OutsidePath;
    internal LineSegment OutsideLine;
    internal ArcSegment OutsideArc;
    internal PathFigure InsidePath;
    internal LineSegment InsideLine;
    internal ArcSegment InsideArc;
    private bool _contentLoaded;

    public UiTest() => this.InitializeComponent();

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/uitest.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.OutsidePath = (PathFigure) target;
          break;
        case 2:
          this.OutsideLine = (LineSegment) target;
          break;
        case 3:
          this.OutsideArc = (ArcSegment) target;
          break;
        case 4:
          this.InsidePath = (PathFigure) target;
          break;
        case 5:
          this.InsideLine = (LineSegment) target;
          break;
        case 6:
          this.InsideArc = (ArcSegment) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
