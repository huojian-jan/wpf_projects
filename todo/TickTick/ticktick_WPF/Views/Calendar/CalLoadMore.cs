// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalLoadMore
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
using ticktick_WPF.Views.Calendar.Month;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalLoadMore : Border, IComponentConnector
  {
    private bool _pressed;
    internal Border MoreBorder;
    private bool _contentLoaded;

    public CalLoadMore() => this.InitializeComponent();

    private IDragBarEvent GetDragTarget()
    {
      return Utils.FindParent<IDragBarEvent>((DependencyObject) this);
    }

    private void OnClick(object sender, MouseButtonEventArgs e)
    {
      IDragBarEvent dragTarget = this.GetDragTarget();
      if ((dragTarget != null ? (dragTarget.OnSelection() ? 1 : 0) : 0) != 0 || PopupStateManager.IsInSelection() || PopupStateManager.IsInAdd())
        return;
      e.Handled = true;
      if (this.DataContext != null && this.DataContext is CalendarDisplayViewModel dataContext && dataContext.IsLoadMore)
        this.ShowLoadMore(CalendarGeoHelper.GetRealColumn(dataContext.Column), e);
      this._pressed = false;
    }

    private CalendarControl GetCalendarParent()
    {
      return Utils.FindParent<CalendarControl>((DependencyObject) this);
    }

    private void ShowLoadMore(int column, MouseButtonEventArgs e)
    {
      CalendarControl calendarParent = this.GetCalendarParent();
      bool locked = calendarParent != null && calendarParent.IsLocked;
      Utils.FindParent<MultiWeekWeekControl>((DependencyObject) this)?.ShowLoadMore(column, e, locked);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e) => this._pressed = true;

    public void SetMoreBorderMargin(bool isFull)
    {
      this.MoreBorder.Margin = new Thickness(isFull ? 3.0 : -3.0, 1.0, 4.0, 1.0);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/calloadmore.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.MoreBorder = (Border) target;
        else
          this._contentLoaded = true;
      }
      else
      {
        ((UIElement) target).MouseLeftButtonDown += new MouseButtonEventHandler(this.OnMouseDown);
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnClick);
      }
    }
  }
}
