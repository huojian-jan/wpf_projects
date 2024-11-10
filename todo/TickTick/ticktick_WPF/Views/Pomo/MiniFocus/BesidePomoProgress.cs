// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Pomo.MiniFocus.BesidePomoProgress
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

#nullable disable
namespace ticktick_WPF.Views.Pomo.MiniFocus
{
  public class BesidePomoProgress : UserControl, ISideMiniFocus, IComponentConnector
  {
    internal Rectangle TopPomoRect;
    internal RectangleGeometry ClipGeo;
    private bool _contentLoaded;

    public BesidePomoProgress()
    {
      this.InitializeComponent();
      this.OnStatusChanged();
      this.SizeChanged += new SizeChangedEventHandler(this.OnSizeChanged);
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => this.SetPercent();

    public void SetPercent()
    {
      this.ClipGeo.Rect = new Rect(0.0, Math.Max(0.0, (1.0 - TickFocusManager.Config.GetDisplayPercent()) * this.ActualHeight), 4.0, this.ActualHeight);
    }

    public void OnStatusChanged()
    {
      switch (TickFocusManager.Status)
      {
        case PomoStatus.Relaxing:
          this.TopPomoRect.SetResourceReference(Shape.FillProperty, (object) "PomoGreen");
          break;
        case PomoStatus.Pause:
          this.TopPomoRect.SetResourceReference(Shape.FillProperty, (object) "PomoPauseColor");
          break;
        default:
          this.TopPomoRect.SetResourceReference(Shape.FillProperty, (object) "PrimaryColor");
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/pomo/minifocus/besidepomoprogress.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.ClipGeo = (RectangleGeometry) target;
        else
          this._contentLoaded = true;
      }
      else
        this.TopPomoRect = (Rectangle) target;
    }
  }
}
