// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.WidgetInfo
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
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Widget;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class WidgetInfo : UserControl, IComponentConnector
  {
    internal StackPanel CalendarWidgetPanel;
    internal StackPanel MatrixWidgetPanel;
    private bool _contentLoaded;

    public WidgetInfo()
    {
      this.InitializeComponent();
      this.CalendarWidgetPanel.IsEnabled = CalendarWidgetHelper.CanAddProject();
      this.MatrixWidgetPanel.IsEnabled = MatrixWidgetHelper.CanAddWidget();
      this.Loaded += (RoutedEventHandler) ((sender, args) => ticktick_WPF.Notifier.GlobalEventManager.WidgetOpenChanged += new EventHandler(this.OnWidgetopenChanged));
      this.Unloaded += (RoutedEventHandler) ((sender, args) => ticktick_WPF.Notifier.GlobalEventManager.WidgetOpenChanged -= new EventHandler(this.OnWidgetopenChanged));
    }

    private void OnWidgetopenChanged(object sender, EventArgs e)
    {
      this.CalendarWidgetPanel.IsEnabled = CalendarWidgetHelper.CanAddProject();
      this.MatrixWidgetPanel.IsEnabled = MatrixWidgetHelper.CanAddWidget();
    }

    private async void OnMatrixWidgetClick(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.MatrixWidget))
        return;
      this.MatrixWidgetPanel.IsEnabled = false;
      if (!MatrixWidgetHelper.CanAddWidget())
        return;
      await MatrixWidgetHelper.AddWidget();
    }

    private async void OnCalendarWidgetClick(object sender, MouseButtonEventArgs e)
    {
      if (!ProChecker.CheckPro(ProType.CalendarWidget))
        return;
      this.CalendarWidgetPanel.IsEnabled = false;
      if (!CalendarWidgetHelper.CanAddProject())
        return;
      await CalendarWidgetHelper.AddWidget();
    }

    private void OnListWidgetClick(object sender, MouseButtonEventArgs e)
    {
      ProjectWidgetsHelper.AddDefaultWidget();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/widgetinfo.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnListWidgetClick);
          break;
        case 2:
          this.CalendarWidgetPanel = (StackPanel) target;
          this.CalendarWidgetPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCalendarWidgetClick);
          break;
        case 3:
          this.MatrixWidgetPanel = (StackPanel) target;
          this.MatrixWidgetPanel.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnMatrixWidgetClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
