// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Update.DownloadWindow
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Util;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Update
{
  public class DownloadWindow : Window, IComponentConnector
  {
    private WebClient _downloader;
    private readonly string _downloadPath;
    private long _pkgSize;
    internal TextBlock IndicatorText;
    internal DownloadProgressBar ProgressBar;
    internal Button CancalBtn;
    internal Button InstallBtn;
    private bool _contentLoaded;

    public DownloadWindow(string downloadPath, long size)
    {
      this.InitializeComponent();
      this._downloadPath = downloadPath;
      this._pkgSize = size;
    }

    public override void OnApplyTemplate()
    {
      this.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void InitBaseEvents(Window window, Func<string, DependencyObject> getTemplateChild)
    {
      if (getTemplateChild("CloseButton") is Button button)
        button.Click += new RoutedEventHandler(this.OnCancelClick);
      if (!(getTemplateChild("DragGrid") is Grid grid))
        return;
      grid.PreviewMouseDown += (MouseButtonEventHandler) ((s, e) =>
      {
        if (Mouse.LeftButton != MouseButtonState.Pressed)
          return;
        window.DragMove();
      });
    }

    private void OnWindowLoaded(object sender, RoutedEventArgs e) => this.StartDownload();

    private async void StartDownload()
    {
      DownloadWindow downloadWindow = this;
      if (!Utils.IsNetworkAvailable())
        return;
      downloadWindow._downloader = new WebClient();
      if (!Directory.Exists(AppPaths.PackageDir))
        Directory.CreateDirectory(AppPaths.PackageDir);
      try
      {
        downloadWindow._downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloadWindow.OnDownloadChanged);
        downloadWindow._downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadWindow.OnDownloadCompleted);
        await downloadWindow._downloader.DownloadFileTaskAsync(downloadWindow._downloadPath, AppPaths.PackageDir + downloadWindow.GetPackageName());
        downloadWindow.NotifyDownloadFinished();
      }
      catch (Exception ex)
      {
        int num = (int) MessageBox.Show(Utils.GetString("DownloadFailed"));
        downloadWindow.OnCancelClick((object) null, (RoutedEventArgs) null);
      }
    }

    private void OnDownloadCompleted(object sender, AsyncCompletedEventArgs e)
    {
      if (!e.Cancelled)
        return;
      this.DeleteBrokenPackage();
    }

    private void OnDownloadChanged(object sender, DownloadProgressChangedEventArgs e)
    {
      this.ProgressBar.Progress = e.ProgressPercentage;
    }

    private string GetPackageName()
    {
      return ((IEnumerable<string>) this._downloadPath.Split('/')).Last<string>();
    }

    private void NotifyDownloadFinished()
    {
      this.CancalBtn.Visibility = Visibility.Collapsed;
      this.InstallBtn.Visibility = Visibility.Visible;
      this.IndicatorText.Text = Utils.GetString("ReadyToInstall");
    }

    private async void OnCancelClick(object sender, RoutedEventArgs e)
    {
      DownloadWindow downloadWindow = this;
      downloadWindow.CancalBtn.Content = (object) Utils.GetString("Canceling");
      downloadWindow.CancalBtn.IsEnabled = false;
      downloadWindow._downloader?.CancelAsync();
      await Task.Delay(1000);
      downloadWindow.Close();
    }

    private void DeleteBrokenPackage()
    {
      string path = AppPaths.PackageDir + this.GetPackageName();
      if (!System.IO.File.Exists(path))
        return;
      try
      {
        System.IO.File.Delete(path);
      }
      catch (Exception ex)
      {
      }
    }

    private async void OnInstallClick(object sender, RoutedEventArgs e)
    {
      DownloadWindow downloadWindow = this;
      string str = AppPaths.PackageDir + downloadWindow.GetPackageName();
      if (System.IO.File.Exists(str))
      {
        try
        {
          Process.Start(str);
        }
        catch (Exception ex)
        {
        }
      }
      await Task.Delay(1000);
      downloadWindow.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/update/downloadwindow.xaml", UriKind.Relative));
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
          this.IndicatorText = (TextBlock) target;
          break;
        case 3:
          this.ProgressBar = (DownloadProgressBar) target;
          break;
        case 4:
          this.CancalBtn = (Button) target;
          this.CancalBtn.Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        case 5:
          this.InstallBtn = (Button) target;
          this.InstallBtn.Click += new RoutedEventHandler(this.OnInstallClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
