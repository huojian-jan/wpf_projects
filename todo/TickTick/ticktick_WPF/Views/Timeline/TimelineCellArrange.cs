// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineCellArrange
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

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineCellArrange : TimelineCellBase, IComponentConnector
  {
    private System.Windows.Point? _point;
    internal Grid InfoTile;
    private bool _contentLoaded;

    public TimelineCellArrange() => this.InitializeComponent();

    private void OnCellMouseLeftDown(object sender, MouseButtonEventArgs e)
    {
      this._point = new System.Windows.Point?(e.GetPosition((IInputElement) this));
    }

    private void OnCellMouseMove(object sender, MouseEventArgs e)
    {
      if (!this._point.HasValue)
        return;
      System.Windows.Point position = e.GetPosition((IInputElement) this);
      double x1 = position.X;
      System.Windows.Point point = this._point.Value;
      double x2 = point.X;
      if (Math.Abs(x1 - x2) < 4.0)
      {
        double y1 = position.Y;
        point = this._point.Value;
        double y2 = point.Y;
        if (Math.Abs(y1 - y2) < 4.0)
          return;
      }
      this._point = new System.Windows.Point?();
      if (this.DataContext is TimelineViewModel dataContext1 && (!dataContext1.ProjectEnable || dataContext1.TimelineSortOption.groupBy == "tag") || !(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is TimelineCellViewModel dataContext2) || !dataContext2.DisplayModel.Editable)
        return;
      this.Container?.TimelineFloating.OpenArrange(dataContext2);
    }

    private void OnCellMouseLeftUp(object sender, MouseButtonEventArgs e)
    {
      System.Windows.Point? point = this._point;
      if (point.HasValue && point.GetValueOrDefault() == e.GetPosition((IInputElement) this) && sender is FrameworkElement)
        this.OpenDetailWindow(true);
      this._point = new System.Windows.Point?();
    }

    private void OnFoldClick(object sender, MouseButtonEventArgs e)
    {
      if (!(this.DataContext is TimelineCellViewModel dataContext))
        return;
      this._point = new System.Windows.Point?();
      dataContext.ToggleArrangeItemOpen();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/timeline/timelinecellarrange.xaml", UriKind.Relative));
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
      if (connectionId != 1)
      {
        if (connectionId == 2)
          ((UIElement) target).PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.OnFoldClick);
        else
          this._contentLoaded = true;
      }
      else
      {
        this.InfoTile = (Grid) target;
        this.InfoTile.MouseLeftButtonDown += new MouseButtonEventHandler(this.OnCellMouseLeftDown);
        this.InfoTile.MouseMove += new MouseEventHandler(this.OnCellMouseMove);
        this.InfoTile.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCellMouseLeftUp);
      }
    }
  }
}
