// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.ProThemeGrid
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class ProThemeGrid : UserControl, IComponentConnector
  {
    internal Border BackImage;
    private bool _contentLoaded;

    public ProThemeGrid()
    {
      this.InitializeComponent();
      this.Loaded += new RoutedEventHandler(this.OnLoaded);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
      ProThemeGrid proThemeGrid = this;
      if (proThemeGrid.DataContext == null)
        model = (ProThemeViewModel) null;
      else if (!(proThemeGrid.DataContext is ProThemeViewModel model))
      {
        model = (ProThemeViewModel) null;
      }
      else
      {
        string localPath = AppPaths.ThemeDir + "preview\\" + model.Key.ToLower() + ".png";
        if (!System.IO.File.Exists(localPath))
        {
          if (!await IOUtils.DownloadFile(AppPaths.ThemeDir + "preview\\", model.Key.ToLower() + ".png", model.Url))
          {
            Border backImage = proThemeGrid.BackImage;
            ImageBrush imageBrush = new ImageBrush();
            imageBrush.Stretch = Stretch.Uniform;
            imageBrush.ImageSource = (ImageSource) new BitmapImage(new Uri(model.Url));
            backImage.Background = (Brush) imageBrush;
            model = (ProThemeViewModel) null;
            return;
          }
        }
        proThemeGrid.BackImage.Background = (Brush) new ImageBrush()
        {
          ImageSource = (ImageSource) ThemeUtil.GetImage(localPath, option: BitmapCacheOption.OnLoad)
        };
        localPath = (string) null;
        model = (ProThemeViewModel) null;
      }
    }

    private async void OnGridClick(object sender, MouseButtonEventArgs e)
    {
      ProThemeGrid child = this;
      if (child.DataContext == null)
        model = (ProThemeViewModel) null;
      else if (!(child.DataContext is ProThemeViewModel model))
        model = (ProThemeViewModel) null;
      else if (!ProChecker.CheckPro(ProType.PremiumThemes))
      {
        model = (ProThemeViewModel) null;
      }
      else
      {
        model.Loading = true;
        if (!await ProThemeGrid.TryDownloadBackground(model.Key.ToLower() + ".png"))
        {
          Utils.FindParent<AppearanceControl>((DependencyObject) child)?.Toast(Utils.GetString("NetworkDownloadTheme"));
          model.Loading = false;
          model = (ProThemeViewModel) null;
        }
        else
        {
          model.SetSelected();
          string themeId = LocalSettings.Settings.ThemeId;
          if (themeId != model.Key)
          {
            try
            {
              App.Instance.LoadTheme(themeId, model.Key);
              LocalSettings.Settings.ExtraSettings.AppTheme = model.Key;
              LocalSettings.Settings.ThemeId = model.Key;
            }
            catch (Exception ex)
            {
              UtilLog.Info(ExceptionUtils.BuildExceptionMessage(ex));
            }
          }
          model.Loading = false;
          model = (ProThemeViewModel) null;
        }
      }
    }

    private static async Task<bool> TryDownloadBackground(string name)
    {
      if (!Directory.Exists(AppPaths.ThemeDir))
        Directory.CreateDirectory(AppPaths.ThemeDir);
      string localPath = AppPaths.ThemeDir + name;
      if (System.IO.File.Exists(localPath))
        return true;
      string address = "https://" + BaseUrl.GetPullDomain() + "/windows/theme/background/" + name;
      if (!Utils.IsNetworkAvailable())
        return false;
      WebClient downloader = new WebClient();
      try
      {
        await downloader.DownloadFileTaskAsync(address, localPath);
        downloader.Dispose();
        return System.IO.File.Exists(localPath) && new FileInfo(localPath).Length > 0L;
      }
      catch (Exception ex)
      {
        return false;
      }
      finally
      {
        if (System.IO.File.Exists(localPath))
        {
          FileInfo fileInfo = new FileInfo(localPath);
          if (fileInfo.Length == 0L)
          {
            fileInfo.Attributes = FileAttributes.Normal;
            fileInfo.Delete();
          }
        }
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/theme/prothemegrid.xaml", UriKind.Relative));
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
      if (connectionId != 1)
      {
        if (connectionId == 2)
          this.BackImage = (Border) target;
        else
          this._contentLoaded = true;
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnGridClick);
    }
  }
}
