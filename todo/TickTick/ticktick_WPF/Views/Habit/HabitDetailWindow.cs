// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Habit.HabitDetailWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Habit
{
  public class HabitDetailWindow : EscPopup, IComponentConnector
  {
    private bool _inOperate;
    internal HabitDetailControl HabitDetail;
    private bool _contentLoaded;

    public event EventHandler<string> Disappear;

    public HabitDetailWindow()
    {
      this.InitializeComponent();
      this.Closed += (EventHandler) ((sender, args) =>
      {
        EventHandler<string> disappear = this.Disappear;
        if (disappear != null)
          disappear((object) this, (string) null);
        PopupStateManager.OnViewPopupClosed();
        this.Disappear = (EventHandler<string>) null;
      });
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      if (PopupStateManager.IsViewPopOpened())
        return;
      PopupStateManager.OnViewPopupOpened();
    }

    public async void Show(
      UIElement target,
      double targetWidth,
      double addHeight,
      bool byMouse,
      string habitId)
    {
      HabitDetailWindow habitDetailWindow = this;
      PopupStateManager.OnViewPopupOpened();
      habitDetailWindow.HabitDetail.Load(habitId, DateTime.Today);
      habitDetailWindow.PlacementTarget = target;
      // ISSUE: explicit non-virtual call
      TaskPopupArgs popupLocation = PopupLocationCalculator.GetPopupLocation(target, targetWidth, __nonvirtual (habitDetailWindow.Width), byMouse, addHeight);
      if (!popupLocation.ByMouse)
      {
        habitDetailWindow.Placement = popupLocation.IsRight ? PlacementMode.Right : PlacementMode.Left;
        habitDetailWindow.HorizontalOffset = popupLocation.IsRight ? -6.0 : 6.0;
        habitDetailWindow.VerticalOffset = -8.0;
      }
      else
        habitDetailWindow.Placement = PlacementMode.Mouse;
      habitDetailWindow.IsOpen = true;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/habit/habitdetailwindow.xaml", UriKind.Relative));
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
      if (connectionId == 1)
        this.HabitDetail = (HabitDetailControl) target;
      else
        this._contentLoaded = true;
    }
  }
}
