// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CourseDetailWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CourseDetailWindow : EscPopup, IComponentConnector
  {
    internal CourseDetailWindow Root;
    internal ContentControl Control;
    internal ItemsControl CourseItems;
    private bool _contentLoaded;

    public CourseDetailWindow(CourseDetailViewModel viewModel)
    {
      this.InitializeComponent();
      this.DataContext = (object) viewModel;
      this.Closed += (EventHandler) ((sender, args) => PopupStateManager.OnViewPopupClosed());
    }

    public async void Show(UIElement target, double targetWidth, bool showByMouse)
    {
      CourseDetailWindow courseDetailWindow = this;
      courseDetailWindow.PlacementTarget = target;
      TaskPopupArgs popupLocation = PopupLocationCalculator.GetPopupLocation(target, targetWidth, 380.0, showByMouse, 0.0);
      if (!popupLocation.ByMouse)
      {
        courseDetailWindow.Placement = popupLocation.IsRight ? PlacementMode.Right : PlacementMode.Left;
        courseDetailWindow.HorizontalOffset = popupLocation.IsRight ? -6.0 : 6.0;
        courseDetailWindow.VerticalOffset = -8.0;
      }
      else
        courseDetailWindow.Placement = PlacementMode.Mouse;
      PopupStateManager.OnViewPopupOpened();
      courseDetailWindow.IsOpen = true;
      await Task.Delay(200);
      if (!courseDetailWindow.IsOpen)
        return;
      PopupStateManager.OnViewPopupOpened();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/coursedetailwindow.xaml", UriKind.Relative));
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
          this.Root = (CourseDetailWindow) target;
          break;
        case 2:
          this.Control = (ContentControl) target;
          break;
        case 3:
          this.CourseItems = (ItemsControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
