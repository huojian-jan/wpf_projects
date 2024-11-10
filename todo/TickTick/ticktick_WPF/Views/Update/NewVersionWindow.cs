// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Update.NewVersionWindow
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
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Update
{
  public class NewVersionWindow : Window, IComponentConnector
  {
    private bool _contentLoaded;

    public NewVersionWindow() => this.InitializeComponent();

    public NewVersionWindow(NewVersonViewModel model)
    {
      this.InitializeComponent();
      this.DataContext = (object) model;
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      App.Window?.LeftTabBar.SetUpgradeDisplay(0);
      base.OnClosing(e);
    }

    private void OnSkipClick(object sender, RoutedEventArgs e)
    {
      if (this.DataContext == null || !(this.DataContext is NewVersonViewModel dataContext))
        return;
      string skipVersion = LocalSettings.Settings.SkipVersion;
      LocalSettings.Settings.SkipVersion = !string.IsNullOrEmpty(skipVersion) ? skipVersion + ";" + dataContext.NewVersion : dataContext.NewVersion;
      this.Close();
    }

    private void OnInstallClick(object sender, RoutedEventArgs e)
    {
      if (this.DataContext != null && this.DataContext is NewVersonViewModel dataContext)
      {
        string str = AppPaths.PackageDir + ((IEnumerable<string>) dataContext.DownloadPath.Split('/')).Last<string>();
        try
        {
          long valueOrDefault = (dataContext.DownloadPath.Contains("x64") ? dataContext.SizeModel?.x64 : dataContext.SizeModel?.x86).GetValueOrDefault();
          if (File.Exists(str) && new FileInfo(str).Length >= valueOrDefault)
            Process.Start(str);
          else
            new DownloadWindow(dataContext.DownloadPath, valueOrDefault).Show();
        }
        catch (Exception ex)
        {
        }
      }
      this.Close();
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/update/newversionwindow.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 1)
      {
        if (connectionId == 2)
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnInstallClick);
        else
          this._contentLoaded = true;
      }
      else
        ((ButtonBase) target).Click += new RoutedEventHandler(this.OnSkipClick);
    }
  }
}
