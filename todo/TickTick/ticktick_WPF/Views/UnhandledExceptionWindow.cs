// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.UnhandledExceptionWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views
{
  public class UnhandledExceptionWindow : Window, IComponentConnector
  {
    private const string MailToTemplate = "mailto:{0}?subject={1}&body={2}";
    private static string _exceptionMessage = "";
    private bool _needDownLoad;
    private readonly bool _restart;
    internal TextBlock ExceptionTitle;
    internal TextBox ExceptionMessageTextBlock;
    internal Button sendButton;
    internal Button exitButton;
    private bool _contentLoaded;

    public UnhandledExceptionWindow() => this.InitializeComponent();

    public UnhandledExceptionWindow(Exception e, bool restart = false)
    {
      this.Topmost = true;
      this.InitializeComponent();
      UnhandledExceptionWindow._exceptionMessage = ExceptionUtils.BuildExceptionMessage(e);
      UtilLog.Error(UnhandledExceptionWindow._exceptionMessage);
      this.ExceptionMessageTextBlock.Text = UnhandledExceptionWindow._exceptionMessage;
      this.Title = Utils.GetString("ProgramUnhandledException");
      this._restart = restart;
      this.sendButton.Content = (object) Utils.GetString("Restart");
      string str = (e.InnerException?.ToString() ?? string.Empty) + e.Message;
      if (!str.Contains("Version=") || !str.Contains("Culture=") || !str.Contains("PublicKeyToken="))
        return;
      this.sendButton.Content = (object) Utils.GetString("GoToDownload");
      this.exitButton.Content = (object) Utils.GetString("SendLog");
      this.ExceptionTitle.Text = Utils.GetString("RedownloadMessage");
      this._needDownLoad = true;
    }

    public override async void OnApplyTemplate()
    {
      UnhandledExceptionWindow unhandledExceptionWindow = this;
      if (LocalSettings.Settings != null)
        await LocalSettings.Settings.Save();
      Utils.InitBaseEvents((Window) unhandledExceptionWindow, new Func<string, DependencyObject>(((FrameworkElement) unhandledExceptionWindow).GetTemplateChild));
      // ISSUE: reference to a compiler-generated method
      unhandledExceptionWindow.\u003C\u003En__0();
    }

    private async void sendButton_Click(object sender, RoutedEventArgs e)
    {
      if (this._needDownLoad)
        Utils.TryProcessStartUrl("https://" + BaseUrl.Domain + "/about/windows");
      else if (this._restart)
        App.Instance.Restart();
      else
        this.OpenFeedback();
    }

    private void OpenFeedback()
    {
      try
      {
        int dotNetReleaseKey = Utils.GetDotNetReleaseKey();
        UtilLog.Info("db size : " + new FileInfo(AppPaths.TickTickDbPath).Length.ToString());
        UtilLog.Info("dotnet releaseKey : " + dotNetReleaseKey.ToString());
        UtilLog.Info("taskCount : " + TaskCache.LocalTaskViewModels.Count.ToString());
      }
      catch (Exception ex)
      {
      }
      NavigateWebBrowserWindow webBrowserWindow = new NavigateWebBrowserWindow("/v2/tickets/");
      webBrowserWindow.Topmost = true;
      webBrowserWindow.Closed += (EventHandler) (async (obj, arg) =>
      {
        await LocalSettings.Settings.Save();
        Application.Current?.Shutdown();
      });
      webBrowserWindow.Show();
    }

    private async void exitButton_Click(object sender, RoutedEventArgs e)
    {
      if (this._needDownLoad)
      {
        this.OpenFeedback();
      }
      else
      {
        await LocalSettings.Settings.Save();
        Application.Current?.Shutdown();
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      Application.Current?.Shutdown();
      base.OnClosing(e);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/unhandledexceptionwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.ExceptionTitle = (TextBlock) target;
          break;
        case 2:
          this.ExceptionMessageTextBlock = (TextBox) target;
          break;
        case 3:
          this.sendButton = (Button) target;
          this.sendButton.Click += new RoutedEventHandler(this.sendButton_Click);
          break;
        case 4:
          this.exitButton = (Button) target;
          this.exitButton.Click += new RoutedEventHandler(this.exitButton_Click);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
