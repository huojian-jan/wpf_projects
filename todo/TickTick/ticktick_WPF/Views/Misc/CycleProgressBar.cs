// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.CycleProgressBar
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
  public class CycleProgressBar : Grid, IComponentConnector
  {
    public static readonly DependencyProperty TargetPercentProperty = DependencyProperty.Register(nameof (TargetPercent), typeof (double), typeof (CycleProgressBar), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(CycleProgressBar.OnTargetPercentChanged)));
    public int Duration = 180;
    public static readonly DependencyProperty PercentProperty = DependencyProperty.Register(nameof (Percent), typeof (double), typeof (CycleProgressBar), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(CycleProgressBar.OnPercentChanged)));
    public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(nameof (Angle), typeof (double), typeof (CycleProgressBar), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(CycleProgressBar.OnAngleChanged)));
    public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(nameof (Thickness), typeof (double), typeof (CycleProgressBar), new PropertyMetadata((object) 8.0, (PropertyChangedCallback) null));
    public static readonly DependencyProperty UnderColorProperty = DependencyProperty.Register(nameof (UnderColor), typeof (SolidColorBrush), typeof (CycleProgressBar), new PropertyMetadata((object) Brushes.Transparent, (PropertyChangedCallback) null));
    public static readonly DependencyProperty TopColorProperty = DependencyProperty.Register(nameof (TopColor), typeof (Brush), typeof (CycleProgressBar), new PropertyMetadata((object) Brushes.Transparent, (PropertyChangedCallback) null));
    private DoubleAnimation _anim;
    internal CycleProgressBar Root;
    internal Path UnderPath;
    internal Ellipse TopEll;
    private bool _contentLoaded;

    private static void OnTargetPercentChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is CycleProgressBar cycleProgressBar) || !(e.NewValue is double newValue) || Math.Abs(cycleProgressBar.Percent - newValue) <= 0.1)
        return;
      DoubleAnimation doubleAnimation = new DoubleAnimation();
      doubleAnimation.From = new double?(cycleProgressBar.Percent);
      doubleAnimation.To = new double?(newValue);
      doubleAnimation.Duration = (System.Windows.Duration) TimeSpan.FromMilliseconds((double) cycleProgressBar.Duration);
      DoubleAnimation animation = doubleAnimation;
      cycleProgressBar.BeginAnimation(CycleProgressBar.PercentProperty, (AnimationTimeline) animation);
    }

    private static void OnPercentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is CycleProgressBar cycleProgressBar) || !(e.NewValue is double _))
        return;
      cycleProgressBar.DrawTopPath();
    }

    private static void OnAngleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      if (!(d is CycleProgressBar cycleProgressBar) || !(e.NewValue is double newValue))
        return;
      cycleProgressBar.Percent = newValue / 3.6;
    }

    public double Angle
    {
      get => (double) this.GetValue(CycleProgressBar.AngleProperty);
      set => this.SetValue(CycleProgressBar.AngleProperty, (object) value);
    }

    public double TargetPercent
    {
      get => (double) this.GetValue(CycleProgressBar.TargetPercentProperty);
      set => this.SetValue(CycleProgressBar.TargetPercentProperty, (object) value);
    }

    public double Percent
    {
      get => (double) this.GetValue(CycleProgressBar.PercentProperty);
      set => this.SetValue(CycleProgressBar.PercentProperty, (object) value);
    }

    public double Thickness
    {
      get => (double) this.GetValue(CycleProgressBar.ThicknessProperty);
      set => this.SetValue(CycleProgressBar.ThicknessProperty, (object) value);
    }

    public SolidColorBrush UnderColor
    {
      get => (SolidColorBrush) this.GetValue(CycleProgressBar.UnderColorProperty);
      set => this.SetValue(CycleProgressBar.UnderColorProperty, (object) value);
    }

    public Brush TopColor
    {
      get => (Brush) this.GetValue(CycleProgressBar.TopColorProperty);
      set => this.SetValue(CycleProgressBar.TopColorProperty, (object) value);
    }

    public CycleProgressBar()
    {
      this.InitializeComponent();
      this.SizeChanged += (SizeChangedEventHandler) ((o, e) =>
      {
        this.DrawUnderPath();
        this.DrawTopPath();
      });
    }

    private void DrawUnderPath()
    {
      this.UnderPath.Data = (Geometry) new CombinedGeometry()
      {
        GeometryCombineMode = GeometryCombineMode.Exclude,
        Geometry1 = (Geometry) new EllipseGeometry()
        {
          RadiusX = (this.Width / 2.0),
          RadiusY = (this.Height / 2.0),
          Center = new System.Windows.Point(this.Width / 2.0, this.Height / 2.0)
        },
        Geometry2 = (Geometry) new EllipseGeometry()
        {
          RadiusX = (this.Width / 2.0 - this.Thickness),
          RadiusY = (this.Height / 2.0 - this.Thickness),
          Center = new System.Windows.Point(this.Width / 2.0, this.Height / 2.0)
        }
      };
    }

    private void DrawTopPath()
    {
      if (this.Percent <= 0.1)
      {
        this.TopEll.Clip = (Geometry) new RectangleGeometry()
        {
          Rect = new Rect(0.0, 0.0, 0.0, 0.0)
        };
      }
      else
      {
        CombinedGeometry combinedGeometry = new CombinedGeometry()
        {
          GeometryCombineMode = GeometryCombineMode.Union
        };
        PathGeometry pathGeometry1 = new PathGeometry();
        System.Windows.Point point = new System.Windows.Point(this.Width / 2.0, this.Thickness);
        PathFigure pathFigure1 = new PathFigure()
        {
          StartPoint = point
        };
        pathFigure1.Segments.Add((PathSegment) new ArcSegment()
        {
          Point = new System.Windows.Point(this.Width / 2.0, 0.0),
          Size = new Size(this.Thickness / 2.0, this.Thickness / 2.0),
          SweepDirection = SweepDirection.Clockwise
        });
        pathFigure1.Segments.Add((PathSegment) new ArcSegment()
        {
          Point = new System.Windows.Point(this.Width / 2.0, this.Thickness),
          Size = new Size(this.Thickness / 2.0, this.Thickness / 2.0),
          SweepDirection = SweepDirection.Clockwise
        });
        pathGeometry1.Figures.Add(pathFigure1);
        PathGeometry pathGeometry2 = new PathGeometry();
        PathFigure pathFigure2 = new PathFigure()
        {
          StartPoint = new System.Windows.Point(this.Width / 2.0, this.Thickness)
        };
        pathFigure2.Segments.Add((PathSegment) new LineSegment()
        {
          Point = new System.Windows.Point(this.Width / 2.0, 0.0)
        });
        System.Windows.Point endPoint1 = this.GetEndPoint(true);
        System.Windows.Point endPoint2 = this.GetEndPoint(false);
        pathFigure2.Segments.Add((PathSegment) new ArcSegment()
        {
          Size = new Size(this.Width / 2.0, this.Height / 2.0),
          Point = endPoint1,
          SweepDirection = SweepDirection.Clockwise,
          IsLargeArc = (this.Percent > 50.0)
        });
        pathFigure2.Segments.Add((PathSegment) new ArcSegment()
        {
          Size = new Size(this.Thickness / 2.0, this.Thickness / 2.0),
          Point = endPoint2,
          SweepDirection = SweepDirection.Clockwise
        });
        pathFigure2.Segments.Add((PathSegment) new ArcSegment()
        {
          Size = new Size(this.Width / 2.0 - this.Thickness, this.Height / 2.0 - this.Thickness),
          Point = point,
          SweepDirection = SweepDirection.Counterclockwise,
          IsLargeArc = (this.Percent > 50.0)
        });
        pathGeometry2.Figures.Add(pathFigure2);
        combinedGeometry.Geometry1 = (Geometry) pathGeometry1;
        combinedGeometry.Geometry2 = (Geometry) pathGeometry2;
        this.TopEll.Clip = (Geometry) combinedGeometry;
      }
    }

    private System.Windows.Point GetEndPoint(bool outSide)
    {
      double num = 2.0 * Math.PI * (this.Percent >= 100.0 ? 99.9 : this.Percent) / 100.0;
      return new System.Windows.Point(this.Width / 2.0 + Math.Sin(num) * (this.Width / 2.0 - (outSide ? 0.0 : this.Thickness)), this.Width / 2.0 - Math.Cos(num) * (this.Height / 2.0 - (outSide ? 0.0 : this.Thickness)));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/cycleprogressbar.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (CycleProgressBar) target;
          break;
        case 2:
          this.UnderPath = (Path) target;
          break;
        case 3:
          this.TopEll = (Ellipse) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
