// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TimeLineBlock
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TimeLineBlock : UserControl, IComponentConnector
  {
    public static readonly DependencyProperty CheckNowProperty = DependencyProperty.Register(nameof (CheckNow), typeof (bool), typeof (TimeLineBlock), new PropertyMetadata((object) false));
    private bool _bottomPressed;
    private bool _topPressed;
    private bool _handleEnter;
    private bool _mouseDown;
    private TimeLine _timeLine;
    internal TimeLineBlock Root;
    internal Border CalendarGrid;
    private bool _contentLoaded;

    public bool CheckNow
    {
      get => (bool) this.GetValue(TimeLineBlock.CheckNowProperty);
      set
      {
        if (this.CheckNow == value)
          return;
        this.SetValue(TimeLineBlock.CheckNowProperty, (object) value);
      }
    }

    public TimeLineBlock() => this.InitializeComponent();

    private void OnHandleMouseLeave(object sender, MouseEventArgs e) => this._handleEnter = false;

    private void OnHandleMouseEnter(object sender, MouseEventArgs e)
    {
      this._handleEnter = true;
      this.NotifyHandleDragged(e);
    }

    private void NotifyHandleDragged(MouseEventArgs e)
    {
      if (e.LeftButton != MouseButtonState.Pressed || this.DataContext == null || !(this.DataContext is TimeOffset dataContext))
        return;
      TimeLine parent = Utils.FindParent<TimeLine>((DependencyObject) this);
      if (parent == null)
        return;
      if (parent.TopHandleDragging && dataContext.Offset < parent.GetBottomOffset())
        parent.NotifyHandleDragged(dataContext.Offset, true);
      if (!parent.BottomHandleDragging || dataContext.Offset <= parent.GetTopOffset() || dataContext.Offset >= 24)
        return;
      parent.NotifyHandleDragged(dataContext.Offset, false);
    }

    private TimeLine GetParent()
    {
      if (this._timeLine == null)
        this._timeLine = Utils.FindParent<TimeLine>((DependencyObject) this);
      return this._timeLine;
    }

    private void OnBottomHandlePressed(object sender, MouseButtonEventArgs e)
    {
      this._bottomPressed = true;
      this.GetParent()?.SetTryDrag(true);
    }

    private void OnStartHandlePressed(object sender, MouseButtonEventArgs e)
    {
      this._topPressed = true;
      this.GetParent()?.SetTryDrag(true);
    }

    private async void OnBlockMouseMove(object sender, MouseEventArgs e)
    {
      if (e.LeftButton == MouseButtonState.Pressed)
      {
        TimeLine parent = this.GetParent();
        if (parent == null)
          return;
        if (this._bottomPressed)
          parent.BottomHandleDragging = true;
        if (!this._topPressed)
          return;
        parent.TopHandleDragging = true;
      }
      else
      {
        this._mouseDown = false;
        this._bottomPressed = false;
        this._topPressed = false;
        this.GetParent()?.SetTryDrag(false);
      }
    }

    private void OnBlockMouseUp(object sender, MouseButtonEventArgs e)
    {
      this._bottomPressed = false;
      this._topPressed = false;
      this.GetParent()?.SetTryDrag(false);
      if (this.DataContext != null && this.DataContext is TimeOffset dataContext)
        Utils.FindParent<TimeLine>((DependencyObject) this)?.OnHandleDrop(dataContext, this._mouseDown);
      this._mouseDown = false;
    }

    private void OnCollapsedClick(object sender, MouseButtonEventArgs e)
    {
      if (this.DataContext == null || !(this.DataContext is TimeOffset dataContext))
        return;
      Utils.FindParent<TimeLine>((DependencyObject) this)?.ExpandTimeline(dataContext);
      e.Handled = true;
    }

    private void OnBlockMouseDown(object sender, MouseButtonEventArgs e) => this._mouseDown = true;

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/timelineblock.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Root = (TimeLineBlock) target;
          break;
        case 2:
          ((UIElement) target).MouseMove += new MouseEventHandler(this.OnBlockMouseMove);
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBlockMouseUp);
          ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnBlockMouseDown);
          break;
        case 3:
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnCollapsedClick);
          break;
        case 4:
          this.CalendarGrid = (Border) target;
          break;
        case 5:
          ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnHandleMouseEnter);
          ((UIElement) target).MouseLeave += new MouseEventHandler(this.OnHandleMouseLeave);
          break;
        case 6:
          ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnBottomHandlePressed);
          break;
        case 7:
          ((UIElement) target).PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.OnStartHandlePressed);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
