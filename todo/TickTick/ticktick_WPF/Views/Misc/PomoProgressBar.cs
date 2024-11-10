// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.PomoProgressBar
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class PomoProgressBar : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty TargetAngelProperty = DependencyProperty.Register(nameof (TargetAngel), typeof (double), typeof (PomoProgressBar), new PropertyMetadata((object) 0.0, new PropertyChangedCallback(PomoProgressBar.OnTargetAngelChanged)));
    public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(nameof (Angle), typeof (double), typeof (PomoProgressBar), new PropertyMetadata((object) 0.0, (PropertyChangedCallback) null));
    public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(nameof (StrokeThickness), typeof (double), typeof (PomoProgressBar), new PropertyMetadata((object) 2.0, (PropertyChangedCallback) null));
    public static readonly DependencyProperty IsStrokeModeProperty = DependencyProperty.Register(nameof (IsStrokeMode), typeof (bool), typeof (PomoProgressBar), new PropertyMetadata((object) true, (PropertyChangedCallback) null));
    public static readonly DependencyProperty TimingSizeProperty = DependencyProperty.Register(nameof (TimingSize), typeof (int), typeof (PomoProgressBar), new PropertyMetadata((object) 0, (PropertyChangedCallback) null));
    public static readonly DependencyProperty UnderColorProperty = DependencyProperty.Register(nameof (UnderColor), typeof (SolidColorBrush), typeof (PomoProgressBar), new PropertyMetadata((object) Brushes.Transparent, (PropertyChangedCallback) null));
    public static readonly DependencyProperty TopColorProperty = DependencyProperty.Register(nameof (TopColor), typeof (Brush), typeof (PomoProgressBar), new PropertyMetadata((object) Brushes.Transparent, (PropertyChangedCallback) null));
    private bool _isPrimaryColor;
    private DoubleAnimation _anim;
    internal PomoProgressBar Root;
    internal CycleProgressBar CycleProgress;
    internal Grid TimingGrid;
    internal Path TimingPath;
    private bool _contentLoaded;

    private static void OnTargetAngelChanged(
      DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      if (!(d is PomoProgressBar pomoProgressBar) || !(e.NewValue is double newValue))
        return;
      pomoProgressBar.BeginAnim(newValue);
    }

    private void BeginAnim(double target)
    {
      if (this._anim == null)
      {
        DoubleAnimation doubleAnimation = new DoubleAnimation();
        doubleAnimation.Duration = (Duration) TimeSpan.FromMilliseconds(1000.0);
        this._anim = doubleAnimation;
      }
      if (Math.Abs(this.Angle - target) <= 0.1)
        return;
      this._anim.From = new double?(this.Angle <= 300.0 || target >= 60.0 ? this.Angle : 0.0);
      this._anim.To = new double?(target);
      this.BeginAnimation(PomoProgressBar.AngleProperty, (AnimationTimeline) this._anim);
    }

    public double TargetAngel
    {
      get => (double) this.GetValue(PomoProgressBar.TargetAngelProperty);
      set => this.SetValue(PomoProgressBar.TargetAngelProperty, (object) value);
    }

    public double Angle
    {
      get => (double) this.GetValue(PomoProgressBar.AngleProperty);
      set => this.SetValue(PomoProgressBar.AngleProperty, (object) value);
    }

    public double StrokeThickness
    {
      get => (double) this.GetValue(PomoProgressBar.StrokeThicknessProperty);
      set => this.SetValue(PomoProgressBar.StrokeThicknessProperty, (object) value);
    }

    public SolidColorBrush UnderColor
    {
      get => (SolidColorBrush) this.GetValue(PomoProgressBar.UnderColorProperty);
      set => this.SetValue(PomoProgressBar.UnderColorProperty, (object) value);
    }

    public Brush TopColor
    {
      get => (Brush) this.GetValue(PomoProgressBar.TopColorProperty);
      set => this.SetValue(PomoProgressBar.TopColorProperty, (object) value);
    }

    public int TimingSize
    {
      get => (int) this.GetValue(PomoProgressBar.TimingSizeProperty);
      set => this.SetValue(PomoProgressBar.TimingSizeProperty, (object) value);
    }

    public bool IsStrokeMode
    {
      get => (bool) this.GetValue(PomoProgressBar.IsStrokeModeProperty);
      set
      {
        if (this.IsStrokeMode == value)
          return;
        this.SetValue(PomoProgressBar.IsStrokeModeProperty, (object) value);
        this.TimingPath.Data = !value ? this.GetTimingData() : (Geometry) null;
        if (this.IsStrokeMode)
          this.CycleProgress.SetBinding(CycleProgressBar.AngleProperty, (BindingBase) new Binding("Angle")
          {
            Source = (object) this.Root
          });
        else
          this.CycleProgress.Percent = 0.0;
      }
    }

    private Geometry GetTimingData()
    {
      switch (this.TimingSize)
      {
        case 1:
          return Geometry.Parse(this.GetPath(this.Height / 2.0, this.Height / 2.0 - 4.0, 60, 1.0));
        case 2:
          return Geometry.Parse(this.GetPath(this.Height / 2.0, this.Height / 2.0 - 3.0, 30, 0.5));
        default:
          return Geometry.Parse(this.GetPath(this.Height / 2.0, this.Height / 2.0 - 12.0, 120, 1.5));
      }
    }

    public PomoProgressBar()
    {
      this.InitializeComponent();
      this.CycleProgress.Duration = 1000;
      this.CycleProgress.SetBinding(CycleProgressBar.AngleProperty, (BindingBase) new Binding(nameof (Angle))
      {
        Source = (object) this.Root
      });
    }

    private string GetPath(double oR, double iR, int num, double thick)
    {
      string path = "";
      for (int index = 0; index < num; ++index)
      {
        double angle = (double) index * Math.PI * 2.0 / (double) num;
        path += this.GetRect(angle, oR, iR, thick);
      }
      return path;
    }

    private string GetRect(double angle, double oR, double iR, double thick)
    {
      double num1 = Math.Round(oR + Math.Sin(angle) * oR, 2);
      double num2 = Math.Round(oR - Math.Cos(angle) * oR, 2);
      double num3 = Math.Round(oR + Math.Sin(angle) * iR, 2);
      double num4 = Math.Round(oR - Math.Cos(angle) * iR, 2);
      double num5 = Math.Round(num1 + Math.Cos(angle) * thick, 2);
      double num6 = Math.Round(num2 + Math.Sin(angle) * thick, 2);
      double num7 = Math.Round(num3 + Math.Cos(angle) * thick, 2);
      double num8 = Math.Round(num4 + Math.Sin(angle) * thick, 2);
      return "M" + num1.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo) + "," + num2.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo) + " L" + num1.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo) + "," + num2.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo) + " L" + num3.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo) + "," + num4.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo) + " L" + num7.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo) + "," + num8.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo) + " L" + num5.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo) + "," + num6.ToString((IFormatProvider) NumberFormatInfo.InvariantInfo) + " Z ";
    }

    public void Reset()
    {
      this.BeginAnimation(PomoProgressBar.AngleProperty, (AnimationTimeline) null);
      this.Angle = 0.0;
      this.TimingGrid.OpacityMask = (Brush) this.FindResource((object) "MaskBrush");
    }

    public void HideLeftMask() => this.TimingGrid.OpacityMask = (Brush) null;

    public void SetAngle(double angle)
    {
      this.BeginAnimation(PomoProgressBar.AngleProperty, (AnimationTimeline) null);
      this.Angle = angle;
    }

    public void SetSize(double width)
    {
      this.Height = width;
      this.Width = width;
      this.TimingPath.Data = !this.IsStrokeMode ? this.GetTimingData() : (Geometry) null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/pomoprogressbar.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (PomoProgressBar) target;
          break;
        case 2:
          this.CycleProgress = (CycleProgressBar) target;
          break;
        case 3:
          this.TimingGrid = (Grid) target;
          break;
        case 4:
          this.TimingPath = (Path) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
