// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.PageTurn
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
using System.Windows.Media.Media3D;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class PageTurn : UserControl, IComponentConnector
  {
    private DoubleAnimation _da;
    private AxisAngleRotation3D _aar;
    private int _num;
    public static readonly DependencyProperty FontProperty = DependencyProperty.Register(nameof (Font), typeof (FontFamily), typeof (PageTurn), new PropertyMetadata((PropertyChangedCallback) null));
    public static readonly DependencyProperty CurrentTenProperty = DependencyProperty.Register(nameof (CurrentTen), typeof (int), typeof (PageTurn), new PropertyMetadata((object) 0));
    public static readonly DependencyProperty CurrentSingleProperty = DependencyProperty.Register(nameof (CurrentSingle), typeof (int), typeof (PageTurn), new PropertyMetadata((object) 0));
    public static readonly DependencyProperty OriginTenProperty = DependencyProperty.Register(nameof (OriginTen), typeof (int), typeof (PageTurn), new PropertyMetadata((object) 0));
    public static readonly DependencyProperty OriginSingleProperty = DependencyProperty.Register(nameof (OriginSingle), typeof (int), typeof (PageTurn), new PropertyMetadata((object) 0));
    internal PageTurn Root;
    internal Grid View;
    internal Grid ShowGrid;
    internal Viewport3D Viewport;
    internal AxisAngleRotation3D aar;
    private bool _contentLoaded;

    public PageTurn()
    {
      this.InitializeComponent();
      this._da = new DoubleAnimation();
      this._da.Duration = new Duration(TimeSpan.FromSeconds(1.0));
      DoubleAnimation da = this._da;
      CubicEase cubicEase = new CubicEase();
      cubicEase.EasingMode = EasingMode.EaseInOut;
      da.EasingFunction = (IEasingFunction) cubicEase;
      this._da.SetValue(Timeline.DesiredFrameRateProperty, (object) 60);
      this._da.From = new double?(0.0);
      this._da.To = new double?(180.0);
      this._da.Completed += new EventHandler(this.OnCompleted);
      this.Font = new FontFamily(AppPaths.ExeDir + "\\#D-DIN DIN");
      this._aar = this.FindName(nameof (aar)) as AxisAngleRotation3D;
    }

    public void SetText(int num, bool withAnimation = true)
    {
      if (this._num == num)
        return;
      this._num = num;
      int num1 = num / 10;
      int num2 = num % 10;
      this.OriginSingle = this.CurrentSingle;
      this.OriginTen = this.CurrentTen;
      this.CurrentSingle = num2;
      this.CurrentTen = num1;
      if (!this.VisibleAlways)
      {
        this.ShowGrid.Visibility = Visibility.Collapsed;
        this.Viewport.Visibility = Visibility.Visible;
      }
      if (withAnimation)
      {
        this._aar.BeginAnimation(AxisAngleRotation3D.AngleProperty, (AnimationTimeline) this._da);
      }
      else
      {
        this.OriginSingle = this.CurrentSingle;
        this.OriginTen = this.CurrentTen;
      }
    }

    public FontFamily Font
    {
      get => (FontFamily) this.GetValue(PageTurn.FontProperty);
      set => this.SetValue(PageTurn.FontProperty, (object) value);
    }

    public int CurrentTen
    {
      get => (int) this.GetValue(PageTurn.CurrentTenProperty);
      set => this.SetValue(PageTurn.CurrentTenProperty, (object) value);
    }

    public int CurrentSingle
    {
      get => (int) this.GetValue(PageTurn.CurrentSingleProperty);
      set => this.SetValue(PageTurn.CurrentSingleProperty, (object) value);
    }

    public int OriginTen
    {
      get => (int) this.GetValue(PageTurn.OriginTenProperty);
      set => this.SetValue(PageTurn.OriginTenProperty, (object) value);
    }

    public int OriginSingle
    {
      get => (int) this.GetValue(PageTurn.OriginSingleProperty);
      set => this.SetValue(PageTurn.OriginSingleProperty, (object) value);
    }

    public bool VisibleAlways { get; internal set; } = true;

    public void SetVisibleAlways(bool visible)
    {
      this.VisibleAlways = visible;
      this.ShowGrid.Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
      this.Viewport.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnCompleted(object sender, EventArgs e)
    {
      if (this.VisibleAlways)
        return;
      this.ShowGrid.Visibility = Visibility.Visible;
      this.Viewport.Visibility = Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/pageturn.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (PageTurn) target;
          break;
        case 2:
          this.View = (Grid) target;
          break;
        case 3:
          this.ShowGrid = (Grid) target;
          break;
        case 4:
          this.Viewport = (Viewport3D) target;
          break;
        case 5:
          this.aar = (AxisAngleRotation3D) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
