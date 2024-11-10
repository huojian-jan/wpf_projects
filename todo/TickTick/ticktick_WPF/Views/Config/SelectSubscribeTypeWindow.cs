// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.SelectSubscribeTypeWindow
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
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class SelectSubscribeTypeWindow : Window, IComponentConnector
  {
    private readonly SubscribeCalendar _parent;
    internal TextBlock CalDavSummary;
    private bool _contentLoaded;

    public SelectSubscribeTypeWindow(SubscribeCalendar subscribeCalendar)
    {
      this.InitializeComponent();
      this._parent = subscribeCalendar;
      this.Closing += (CancelEventHandler) ((sender, e) => this.Owner?.Activate());
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void OnCancelClick(object sender, MouseButtonEventArgs e) => this.Close();

    private void OnSubscribeUrlClick(object sender, MouseButtonEventArgs e)
    {
      if (!LimitCache.CheckCalendarCount("URL"))
        return;
      AddUrlCalendarWindow urlCalendarWindow = new AddUrlCalendarWindow(this._parent);
      urlCalendarWindow.Owner = Window.GetWindow((DependencyObject) this._parent);
      urlCalendarWindow.Show();
      this.Close();
    }

    private void OnBindCalendarClick(object sender, MouseButtonEventArgs e)
    {
      if (!LimitCache.CheckCalendarCount("Google"))
        return;
      BindGoogleAccount.GetInstance(this._parent).Start();
      this.Close();
    }

    private void OnBindCalDavClick(object sender, MouseButtonEventArgs e)
    {
      if (!LimitCache.CheckCalendarCount("CalDAV"))
        return;
      BindAccountWindow bindAccountWindow = new BindAccountWindow(this._parent, Constants.BindAccountType.CalDAV);
      bindAccountWindow.Owner = Window.GetWindow((DependencyObject) this._parent);
      bindAccountWindow.Show();
      this.Close();
    }

    private void OnBindOutlookClick(object sender, MouseButtonEventArgs e)
    {
      if (!LimitCache.CheckCalendarCount("Outlook"))
        return;
      this.Close();
      CookieHelper.ClearCookie();
      BindOutlookWindow bindOutlookWindow = new BindOutlookWindow(this._parent);
      bindOutlookWindow.Owner = Window.GetWindow((DependencyObject) this._parent);
      bindOutlookWindow.ShowDialog();
    }

    private void OnBindExchangeClick(object sender, MouseButtonEventArgs e)
    {
      if (!LimitCache.CheckCalendarCount("Exchange"))
        return;
      BindAccountWindow bindAccountWindow = new BindAccountWindow(this._parent, Constants.BindAccountType.Exchange);
      bindAccountWindow.Owner = Window.GetWindow((DependencyObject) this._parent);
      bindAccountWindow.Show();
      this.Close();
    }

    private void OnBindICloudClick(object sender, MouseButtonEventArgs e)
    {
      if (!LimitCache.CheckCalendarCount("iCloud"))
        return;
      BindICloudWindow bindIcloudWindow = new BindICloudWindow(this._parent);
      bindIcloudWindow.Owner = Window.GetWindow((DependencyObject) this._parent);
      bindIcloudWindow.Show();
      this.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/selectsubscribetypewindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBindCalendarClick);
          break;
        case 2:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBindOutlookClick);
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBindExchangeClick);
          break;
        case 4:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBindICloudClick);
          break;
        case 5:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnBindCalDavClick);
          break;
        case 6:
          this.CalDavSummary = (TextBlock) target;
          break;
        case 7:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnSubscribeUrlClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
