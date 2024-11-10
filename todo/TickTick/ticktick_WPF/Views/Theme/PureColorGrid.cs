// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.PureColorGrid
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
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class PureColorGrid : UserControl, IComponentConnector
  {
    private bool _contentLoaded;

    public PureColorGrid() => this.InitializeComponent();

    private async void OnGridClick(object sender, MouseButtonEventArgs e)
    {
      PureColorGrid pureColorGrid = this;
      if (pureColorGrid.DataContext == null || !(pureColorGrid.DataContext is ThemeBaseModel dataContext))
        return;
      dataContext.SetSelected();
      string themeId = LocalSettings.Settings.ThemeId;
      if (!(themeId != dataContext.Key))
        return;
      try
      {
        App.Instance.LoadTheme(themeId, dataContext.Key);
        LocalSettings.Settings.ExtraSettings.AppTheme = dataContext.Key;
        LocalSettings.Settings.ThemeId = dataContext.Key;
      }
      catch (Exception ex)
      {
        UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/theme/purecolorgrid.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      if (connectionId == 1)
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnGridClick);
      else
        this._contentLoaded = true;
    }
  }
}
