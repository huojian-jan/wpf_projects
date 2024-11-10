// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.YearPromoControl
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Properties;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class YearPromoControl : Border, IComponentConnector
  {
    private string _url;
    internal YearPromoControl YearPromoBorder;
    internal Image YearPromoImage;
    private bool _contentLoaded;

    public YearPromoControl() => this.InitializeComponent();

    private void OnCloseYearPromoClick(object sender, MouseButtonEventArgs e)
    {
      e.Handled = true;
      this.Visibility = Visibility.Collapsed;
      // ISSUE: variable of a compiler-generated type
      Settings settings = Settings.Default;
      settings.YearPromoClosedUsers = settings.YearPromoClosedUsers + ";" + LocalSettings.Settings.LoginUserId;
      Settings.Default.Save();
    }

    private void OnYearPromoClick(object sender, MouseButtonEventArgs e)
    {
      if (string.IsNullOrEmpty(this._url))
        return;
      UserActCollectUtils.AddClickEvent("2021_report", "click", "");
      Utils.TryProcessStartUrl(this._url);
    }

    public void SetImageAndUrl(BitmapImage bitmapImage, string url)
    {
      this.YearPromoImage.Source = (ImageSource) bitmapImage;
      this._url = url;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/yearpromocontrol.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.YearPromoBorder = (YearPromoControl) target;
          this.YearPromoBorder.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnYearPromoClick);
          break;
        case 2:
          this.YearPromoImage = (Image) target;
          break;
        case 3:
          ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnCloseYearPromoClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
