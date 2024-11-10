// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.NavigateWebBrowserWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Navigation;
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views
{
  public class NavigateWebBrowserWindow : Window, IComponentConnector
  {
    private static string _cookie;
    private string _url;
    private string _userAgent;
    public string LogFileName = "FbLog";
    private Action<string> _closeAction;
    private JavaScriptTicketOptions _scriptOption;
    internal Grid Container;
    internal System.Windows.Controls.WebBrowser FeedBackBrowser;
    internal Grid NoNetworkImage;
    private bool _contentLoaded;

    public NavigateWebBrowserWindow(string url, bool addLang = true, Action<string> closeAction = null)
    {
      try
      {
        int dotNetReleaseKey = Utils.GetDotNetReleaseKey();
        UtilLog.Info("LocalTimeZone:" + TimeZoneData.LocalTimeZoneModel.DisplayName + "   " + TimeZoneData.LocalTimeZoneModel.TimeZoneName);
        UtilLog.Info("taskCount : " + TaskCache.LocalTaskViewModels.Count.ToString());
        UtilLog.Info("db size : " + new FileInfo(AppPaths.TickTickDbPath).Length.ToString());
        UtilLog.Info("dotnet releaseKey : " + dotNetReleaseKey.ToString());
      }
      catch (Exception ex)
      {
      }
      this._url = addLang ? UriUtils.AddLangAttribute(url) : url;
      this.InitializeComponent();
      this._closeAction = closeAction;
      this.Closing += (CancelEventHandler) ((sender, e) =>
      {
        this._closeAction = (Action<string>) null;
        this.Owner?.Activate();
        this._scriptOption?.Dispose();
        this._scriptOption = (JavaScriptTicketOptions) null;
        this.FeedBackBrowser.Dispose();
      });
    }

    private static string GetFeedbackDomain() => BaseUrl.GetDomainUrl();

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
      this.InitUrl();
      WebBrowserHelper.SetErrorSilent(this.FeedBackBrowser);
      if (Utils.IsNetworkAvailable())
      {
        WebBrowserZoomInvoker.AddZoomInvoker(this.FeedBackBrowser);
        this.NavigateUrl();
      }
      else
      {
        this.FeedBackBrowser.Visibility = Visibility.Collapsed;
        this.NoNetworkImage.Visibility = Visibility.Visible;
      }
    }

    private void NavigateUrl()
    {
      string source = NavigateWebBrowserWindow.GetFeedbackDomain() + this._url;
      if (this._userAgent != null)
        this.FeedBackBrowser.Navigate(source, "_self", (byte[]) null, "User-Agent:" + this._userAgent);
      else
        this.FeedBackBrowser.Navigate(source);
    }

    private void InitUrl()
    {
      try
      {
        NavigateWebBrowserWindow._cookie = "t=" + UserManager.GetToken();
        CookieHelper.SetCookie(NavigateWebBrowserWindow.GetFeedbackDomain() + "/", NavigateWebBrowserWindow._cookie);
        this._scriptOption = new JavaScriptTicketOptions((Window) this);
        this.FeedBackBrowser.ObjectForScripting = (object) this._scriptOption;
        System.Windows.Forms.WebBrowser webBrowser = new System.Windows.Forms.WebBrowser();
        webBrowser.Navigate("about:blank");
        object domWindow = webBrowser.Document?.Window?.DomWindow;
        object target = domWindow?.GetType()?.InvokeMember("navigator", BindingFlags.GetProperty, (Binder) null, domWindow, new object[0]);
        this._userAgent = target?.GetType()?.InvokeMember("userAgent", BindingFlags.GetProperty, (Binder) null, target, new object[0])?.ToString() + "/TickTick/PC";
      }
      catch (Exception ex)
      {
        this._userAgent = (string) null;
      }
    }

    private void OnNavigating(object sender, NavigatingCancelEventArgs e)
    {
      if (e.Uri.ToString().Contains(NavigateWebBrowserWindow.GetFeedbackDomain()))
        return;
      e.Cancel = true;
    }

    private void ClearCookies(object sender, EventArgs e)
    {
      NavigateWebBrowserWindow._cookie = "t=" + UserManager.GetToken();
      CookieHelper.SetCookieOutDate(NavigateWebBrowserWindow.GetFeedbackDomain() + "/", NavigateWebBrowserWindow._cookie);
    }

    public void DoAction(bool needClose, string arg)
    {
      Action<string> closeAction = this._closeAction;
      if (closeAction != null)
        closeAction(arg);
      if (!needClose)
        return;
      this.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/navigatewebbrowserwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnWindowLoaded);
          break;
        case 2:
          this.Container = (Grid) target;
          break;
        case 3:
          this.FeedBackBrowser = (System.Windows.Controls.WebBrowser) target;
          this.FeedBackBrowser.Navigating += new NavigatingCancelEventHandler(this.OnNavigating);
          break;
        case 4:
          this.NoNetworkImage = (Grid) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
