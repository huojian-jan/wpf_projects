// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MiniFocus.BesideTiming
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
using System.Windows.Shapes;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Pomo.MiniFocus
{
  public class BesideTiming : UserControl, ISideMiniFocus, IComponentConnector
  {
    internal Grid Container;
    internal Path BottomPath;
    internal Path TopPath;
    internal RectangleGeometry ClipGeo;
    private bool _contentLoaded;

    public BesideTiming()
    {
      this.InitializeComponent();
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
      this.SetPercent();
      this.BottomPath.Data = Utils.GetIcon(e.NewSize.Height < 100.0 ? "FocusSideTimingPath01" : (e.NewSize.Height > 150.0 ? "FocusSideTimingPath03" : "FocusSideTimingPath02"));
    }

    public void SetPercent()
    {
      this.ClipGeo.Rect = new Rect(0.0, Math.Max(0.0, (1.0 - TickFocusManager.Config.GetDisplayPercent()) * this.ActualHeight), 4.0, this.ActualHeight);
    }

    public void OnStatusChanged()
    {
      if (TickFocusManager.Status == PomoStatus.Pause)
        this.TopPath.SetResourceReference(Shape.FillProperty, (object) "PomoPauseColor");
      else
        this.TopPath.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor");
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/minifocus/besidetiming.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Container = (Grid) target;
          break;
        case 2:
          this.BottomPath = (Path) target;
          break;
        case 3:
          this.TopPath = (Path) target;
          break;
        case 4:
          this.ClipGeo = (RectangleGeometry) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
