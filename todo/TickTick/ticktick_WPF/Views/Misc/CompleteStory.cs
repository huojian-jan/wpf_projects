// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.CompleteStory
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
using System.Windows.Media.Animation;
using System.Windows.Shapes;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class CompleteStory : Viewbox, IComponentConnector
  {
    public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(nameof (Angle), typeof (double), typeof (CompleteStory), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(CompleteStory.OnAngelChanged)));
    internal CompleteStory Root;
    internal Grid CheckGrid;
    internal Border CheckBorder;
    internal ArcSegment Arc;
    internal Line line;
    private bool _contentLoaded;

    public CompleteStory() => this.InitializeComponent();

    public void PlayStory() => ((Storyboard) this.FindResource((object) "Story")).Begin();

    private static void OnAngelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is CompleteStory completeStory))
        return;
      completeStory.SetAngle((double) e.NewValue);
    }

    private void SetAngle(double val)
    {
      this.Angle = val;
      double num = 2.0 * Math.PI * (val >= 360.0 ? 359.99 : val) / 360.0;
      this.Arc.Point = new System.Windows.Point(24.0 - Math.Sin(num) * 24.0, 24.0 - Math.Cos(num) * 24.0);
      this.Arc.IsLargeArc = val >= 180.0;
    }

    public double Angle
    {
      get => (double) this.GetValue(CompleteStory.AngleProperty);
      set => this.SetValue(CompleteStory.AngleProperty, (object) value);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/completestory.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (CompleteStory) target;
          break;
        case 2:
          this.CheckGrid = (Grid) target;
          break;
        case 3:
          this.CheckBorder = (Border) target;
          break;
        case 4:
          this.Arc = (ArcSegment) target;
          break;
        case 5:
          this.line = (Line) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
