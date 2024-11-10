// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WidgetParentWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class WidgetParentWindow : Window, IComponentConnector
  {
    private bool _topMost;
    private bool _contentLoaded;

    public WidgetParentWindow(string modelOption)
    {
      this._topMost = modelOption == "top";
      this.InitializeComponent();
      this.SourceInitialized += new EventHandler(this.OnWindowSourceInitialized);
      this.Title = Utils.GetAppName();
    }

    private void OnWindowSourceInitialized(object sender, EventArgs e) => this.SetWindowTopStyle();

    private async void SetWindowTopStyle()
    {
      WidgetParentWindow widgetParentWindow = this;
      widgetParentWindow.Topmost = true;
      widgetParentWindow.Activate();
      IntPtr handle = new WindowInteropHelper((Window) widgetParentWindow).Handle;
      int windowLong = NativeUtils.GetWindowLong(handle, -16);
      NativeUtils.SetWindowLong(handle, WinParameter.GWL_STYLE, windowLong & -65537 & -131073);
      if (widgetParentWindow._topMost)
        return;
      IntPtr desktopPtr = NativeUtils.GetDesktopPtr();
      NativeUtils.SetWindowLong(handle, WinParameter.GWL_HWNDPARENT, desktopPtr.ToInt32());
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/widget/widgetparentwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target) => this._contentLoaded = true;
  }
}
