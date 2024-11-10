// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarDetailWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarDetailWindow : EscPopup, IComponentConnector
  {
    private bool _inOperate;
    private UIElement _target;
    internal CalendarDetailControl CalendarDetail;
    private bool _contentLoaded;

    public event EventHandler<bool> EventArchiveChanged;

    public CalendarDetailWindow(string themeId = "")
    {
      if (!string.IsNullOrEmpty(themeId))
        ThemeUtil.SetTheme(themeId, (FrameworkElement) this);
      this.InitializeComponent();
      this.InitEvents();
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding(WindowCommands.EscCommand, new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
      this.Closed += new EventHandler(this.OnClosed);
    }

    public event EventHandler<string> Disappear;

    private void InitEvents()
    {
      CalendarEventChangeNotifier.Deleted -= new EventHandler<string>(this.OnEventDeleted);
      CalendarEventChangeNotifier.Deleted += new EventHandler<string>(this.OnEventDeleted);
      this.CalendarDetail.InOperate -= new EventHandler(this.OnCalendarOperate);
      this.CalendarDetail.InOperate += new EventHandler(this.OnCalendarOperate);
      this.CalendarDetail.EventArchiveChanged -= new EventHandler<bool>(this.OnEventArchivedChanged);
      this.CalendarDetail.EventArchiveChanged += new EventHandler<bool>(this.OnEventArchivedChanged);
      this.CalendarDetail.StopOperate -= new EventHandler(this.OnCalendarStopOperate);
      this.CalendarDetail.StopOperate += new EventHandler(this.OnCalendarStopOperate);
    }

    private void OnEventArchivedChanged(object sender, bool e)
    {
      EventHandler<bool> eventArchiveChanged = this.EventArchiveChanged;
      if (eventArchiveChanged == null)
        return;
      eventArchiveChanged(sender, e);
    }

    private void OnCalendarStopOperate(object sender, EventArgs e) => this._inOperate = false;

    private void OnCalendarOperate(object sender, EventArgs e) => this._inOperate = true;

    private void OnEventDeleted(object sender, string eventId) => this.IsOpen = false;

    public async void Show(
      UIElement target,
      double targetWidth,
      double addHeight,
      bool byMouse,
      CalendarDetailViewModel model)
    {
      CalendarDetailWindow calendarDetailWindow = this;
      PopupStateManager.LastTarget = target;
      calendarDetailWindow._target = target;
      calendarDetailWindow.PlacementTarget = target;
      calendarDetailWindow.CalendarDetail.DataContext = (object) model;
      // ISSUE: explicit non-virtual call
      TaskPopupArgs popupLocation = PopupLocationCalculator.GetPopupLocation(calendarDetailWindow._target, targetWidth, __nonvirtual (calendarDetailWindow.Width), byMouse, addHeight);
      if (!popupLocation.ByMouse)
      {
        calendarDetailWindow.Placement = popupLocation.IsRight ? PlacementMode.Right : PlacementMode.Left;
        calendarDetailWindow.HorizontalOffset = popupLocation.IsRight ? -6.0 : 6.0;
        calendarDetailWindow.VerticalOffset = -8.0;
      }
      else
        calendarDetailWindow.Placement = PlacementMode.Mouse;
      PopupStateManager.OnViewPopupOpened();
      calendarDetailWindow.IsOpen = true;
      await Task.Delay(200);
      if (!calendarDetailWindow.IsOpen)
        return;
      PopupStateManager.OnViewPopupOpened();
    }

    private void OnClosed(object sender, EventArgs e)
    {
      if (PopupStateManager.LastTarget == this._target)
        PopupStateManager.LastTarget = (UIElement) null;
      PopupStateManager.OnViewPopupClosed(false);
      EventHandler<string> disappear = this.Disappear;
      if (disappear != null)
        disappear((object) this, (string) null);
      CalendarEventChangeNotifier.Deleted -= new EventHandler<string>(this.OnEventDeleted);
      this.CalendarDetail.InOperate -= new EventHandler(this.OnCalendarOperate);
      this.CalendarDetail.StopOperate -= new EventHandler(this.OnCalendarStopOperate);
      CalendarEventChangeNotifier.Deleted -= new EventHandler<string>(this.OnEventDeleted);
      this.CalendarDetail.InOperate -= new EventHandler(this.OnCalendarOperate);
      this.CalendarDetail.EventArchiveChanged -= new EventHandler<bool>(this.OnEventArchivedChanged);
      this.CalendarDetail.StopOperate -= new EventHandler(this.OnCalendarStopOperate);
      this.EventArchiveChanged = (EventHandler<bool>) null;
      this.Disappear = (EventHandler<string>) null;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/calendar/calendardetailwindow.xaml", UriKind.Relative));
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
        this.CalendarDetail = (CalendarDetailControl) target;
      else
        this._contentLoaded = true;
    }
  }
}
