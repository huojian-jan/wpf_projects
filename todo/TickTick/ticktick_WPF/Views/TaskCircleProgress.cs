// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.TaskCircleProgress
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
namespace ticktick_WPF.Views
{
  public class TaskCircleProgress : Border, IComponentConnector
  {
    public static readonly DependencyProperty PercentageProperty = DependencyProperty.Register(nameof (Percentage), typeof (double), typeof (TaskCircleProgress), new PropertyMetadata((object) 65.0, new PropertyChangedCallback(TaskCircleProgress.OnPercentageChanged)));
    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(nameof (StrokeThickness), typeof (double), typeof (TaskCircleProgress), new PropertyMetadata((object) 5.0));
    public static readonly DependencyProperty SegmentColorProperty = DependencyProperty.Register(nameof (SegmentColor), typeof (Brush), typeof (TaskCircleProgress), new PropertyMetadata((object) new SolidColorBrush(Colors.Red)));
    public static readonly DependencyProperty RadiusProperty = DependencyProperty.Register(nameof (Radius), typeof (double), typeof (TaskCircleProgress), new PropertyMetadata((object) 25.0));
    internal TaskCircleProgress RootBd;
    internal Path PathRoot;
    internal PathFigure ProgressPath;
    internal LineSegment ProgressLine;
    internal ArcSegment ProgressArc;
    internal PathFigure OutsidePath;
    internal LineSegment OutsideLine;
    internal ArcSegment OutsideArc;
    internal PathFigure InsidePath;
    internal LineSegment InsideLine;
    internal ArcSegment InsideArc;
    private bool _contentLoaded;

    public TaskCircleProgress()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private void OnLoaded(object sender, RoutedEventArgs e) => this.RenderArc();

    public double Radius
    {
      get => (double) this.GetValue(TaskCircleProgress.RadiusProperty);
      set => this.SetValue(TaskCircleProgress.RadiusProperty, (object) value);
    }

    public Brush SegmentColor
    {
      get => (Brush) this.GetValue(TaskCircleProgress.SegmentColorProperty);
      set => this.SetValue(TaskCircleProgress.SegmentColorProperty, (object) value);
    }

    public double StrokeThickness
    {
      get => (double) this.GetValue(TaskCircleProgress.StrokeThicknessProperty);
      set => this.SetValue(TaskCircleProgress.StrokeThicknessProperty, (object) value);
    }

    public double Percentage
    {
      get => (double) this.GetValue(TaskCircleProgress.PercentageProperty);
      set => this.SetValue(TaskCircleProgress.PercentageProperty, (object) value);
    }

    private static void OnPercentageChanged(
      DependencyObject sender,
      DependencyPropertyChangedEventArgs args)
    {
      if (!(sender is TaskCircleProgress taskCircleProgress))
        return;
      taskCircleProgress.RenderArc();
    }

    public void RenderArc()
    {
      double num = this.Radius + this.StrokeThickness;
      this.PathRoot.Width = num * 2.0;
      this.PathRoot.Height = num * 2.0;
      this.Width = num * 2.0 + 1.0;
      this.Height = num * 2.0 + 1.0;
      this.OutsidePath.StartPoint = new System.Windows.Point(num, num);
      this.OutsideLine.Point = new System.Windows.Point(num, 0.0);
      System.Windows.Point cartesianCoordinate1 = this.ComputeCartesianCoordinate(359.0, num);
      cartesianCoordinate1.X += num;
      cartesianCoordinate1.Y += num;
      this.OutsideArc.Point = cartesianCoordinate1;
      this.OutsideArc.Size = new Size(num, num);
      this.OutsideArc.IsLargeArc = true;
      this.InsidePath.StartPoint = new System.Windows.Point(num, num);
      this.InsideLine.Point = new System.Windows.Point(num, this.StrokeThickness);
      System.Windows.Point cartesianCoordinate2 = this.ComputeCartesianCoordinate(359.0, this.Radius);
      cartesianCoordinate2.X += num;
      cartesianCoordinate2.Y += num;
      this.InsideArc.Point = cartesianCoordinate2;
      this.InsideArc.Size = new Size(this.Radius, this.Radius);
      this.InsideArc.IsLargeArc = true;
      double angle = this.Percentage >= 100.0 ? 359.0 : this.Percentage * 360.0 / 100.0;
      bool flag = angle > 180.0;
      this.ProgressPath.StartPoint = new System.Windows.Point(num, num);
      this.ProgressLine.Point = new System.Windows.Point(num, this.StrokeThickness);
      System.Windows.Point cartesianCoordinate3 = this.ComputeCartesianCoordinate(angle, this.Radius);
      cartesianCoordinate3.X += num;
      cartesianCoordinate3.Y += num;
      this.ProgressArc.Point = cartesianCoordinate3;
      this.ProgressArc.Size = new Size(this.Radius, this.Radius);
      this.ProgressArc.IsLargeArc = flag;
    }

    private System.Windows.Point ComputeCartesianCoordinate(double angle, double radius)
    {
      double num = Math.PI / 180.0 * (angle - 90.0);
      return new System.Windows.Point(radius * Math.Cos(num), radius * Math.Sin(num));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/taskcircleprogress.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.RootBd = (TaskCircleProgress) target;
          break;
        case 2:
          this.PathRoot = (Path) target;
          break;
        case 3:
          this.ProgressPath = (PathFigure) target;
          break;
        case 4:
          this.ProgressLine = (LineSegment) target;
          break;
        case 5:
          this.ProgressArc = (ArcSegment) target;
          break;
        case 6:
          this.OutsidePath = (PathFigure) target;
          break;
        case 7:
          this.OutsideLine = (LineSegment) target;
          break;
        case 8:
          this.OutsideArc = (ArcSegment) target;
          break;
        case 9:
          this.InsidePath = (PathFigure) target;
          break;
        case 10:
          this.InsideLine = (LineSegment) target;
          break;
        case 11:
          this.InsideArc = (ArcSegment) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
