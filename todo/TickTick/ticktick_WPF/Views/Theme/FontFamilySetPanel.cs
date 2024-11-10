// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.FontFamilySetPanel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using ticktick_WPF.Notifier;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class FontFamilySetPanel : UserControl, IComponentConnector, IStyleConnector
  {
    private List<FontFamilyViewModel> _models;
    private FontFamilyViewModel _currentHover;
    private bool _switching;
    internal TextBlock Text;
    internal ItemsControl FontFamilyItems;
    internal Popup AuthPopup;
    internal Run AuthRun1;
    internal Run AuthRun2;
    private bool _contentLoaded;

    public FontFamilySetPanel(List<FontFamilyViewModel> models)
    {
      this.InitializeComponent();
      this._models = models;
      this.FontFamilyItems.ItemsSource = (IEnumerable) models;
      DataChangedNotifier.IsDarkChanged += new EventHandler(this.OnIsDarkChanged);
      this.Unloaded += (RoutedEventHandler) ((o, e) => DataChangedNotifier.IsDarkChanged -= new EventHandler(this.OnIsDarkChanged));
    }

    private void OnIsDarkChanged(object sender, EventArgs e)
    {
      this._models.ForEach((Action<FontFamilyViewModel>) (m => m.ChangeImage()));
    }

    private async void OnFontFamilySelect(object sender, MouseButtonEventArgs e)
    {
      if (this.IsSwitching() || !(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is FontFamilyViewModel dataContext))
        return;
      this.TrySetFontFamily(dataContext);
    }

    private bool IsSwitching()
    {
      int num = this._switching ? 1 : 0;
      if (this._switching)
        return num != 0;
      this._switching = true;
      return num != 0;
    }

    private async Task TrySetFontFamily(FontFamilyViewModel model)
    {
      FontFamilySetPanel child = this;
      try
      {
        if (model.Selected || model.NeedPro && !ProChecker.CheckPro(ProType.Font))
          return;
        model.Loading = true;
        string fontFileName = FontFamilyUtils.GetFontFileName(model.Id);
        if (!string.IsNullOrEmpty(fontFileName))
        {
          List<string> list = ((IEnumerable<string>) fontFileName.Split('|')).ToList<string>();
          bool needDownload = false;
          foreach (string fileName in list)
          {
            if (!File.Exists(AppPaths.TTFDir + fileName) || new FileInfo(AppPaths.TTFDir + fileName).Length < 7000000L)
            {
              needDownload = true;
              int num = await IOUtils.CheckResourceExist(AppPaths.TTFDir, fileName, "https://" + BaseUrl.GetPullDomain() + "/windows/font/" + fileName) ? 1 : 0;
              bool flag = !File.Exists(AppPaths.TTFDir + fileName);
              if (!flag && new FileInfo(AppPaths.TTFDir + fileName).Length < 7000000L)
                flag = true;
              if (flag)
              {
                Utils.FindParent<AppearanceControl>((DependencyObject) child)?.Toast(Utils.GetString("DownloadFailed"));
                model.Loading = false;
                return;
              }
            }
          }
          if (needDownload)
          {
            model.Loading = false;
            Utils.FindParent<AppearanceControl>((DependencyObject) child)?.Toast(Utils.GetString("FontDownloadSuccessful"));
            FontFamilyUtils.NewDownloadFont.Add(model.Id);
          }
        }
        LocalSettings.Settings.ExtraSettings.AppFontFamily = model.Id;
        if (!FontFamilyUtils.NewDownloadFont.Contains(model.Id))
        {
          App.Instance.LoadFontResource();
          DataChangedNotifier.NotifyFontFamilyChanged();
        }
        child._models.ForEach((Action<FontFamilyViewModel>) (m => m.Selected = false));
        model.Selected = true;
        string fontEventName = FontFamilyUtils.GetFontEventName(model.Id);
        if (!string.IsNullOrEmpty(fontEventName))
          UserActCollectUtils.AddClickEvent("settings", "fonts", fontEventName);
        model.Loading = false;
      }
      finally
      {
        child._switching = false;
      }
    }

    private bool CheckFileMd5(string path, string fontId)
    {
      return File.Exists(path) && new FileInfo(path).Length > 10000000L;
    }

    private void OnAuthClick(object sender, MouseButtonEventArgs e)
    {
      if (this._currentHover == null)
        return;
      string fontCopyRightLink = FontFamilyUtils.GetFontCopyRightLink(this._currentHover.Id);
      if (string.IsNullOrEmpty(fontCopyRightLink))
        return;
      Utils.TryProcessStartUrl(fontCopyRightLink);
    }

    private void OnCopyRightMouseLeave(object sender, MouseEventArgs e)
    {
      if (this.AuthPopup.IsMouseOver || !this.AuthPopup.IsOpen)
        return;
      this.AuthPopup.IsOpen = false;
      if (this._currentHover == null)
        return;
      if (sender.Equals((object) this.AuthPopup))
        this._currentHover.ShowCopyRight = false;
      this._currentHover = (FontFamilyViewModel) null;
    }

    private void OnCopyRightMouseEnter(object sender, MouseEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is FontFamilyViewModel dataContext))
        return;
      string fontAuth = FontFamilyUtils.GetFontAuth(dataContext.Id);
      if (string.IsNullOrEmpty(fontAuth) || this.AuthPopup.IsOpen)
        return;
      this._currentHover = dataContext;
      this.AuthRun1.Text = dataContext.ShowAuthor() ? Utils.GetString("Author") + ": " : string.Empty;
      this.AuthRun2.Text = fontAuth;
      this.AuthPopup.PlacementTarget = (UIElement) frameworkElement;
      this.AuthPopup.IsOpen = true;
    }

    private void HoverBorderMouseEnter(object sender, MouseEventArgs e)
    {
      if (!(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is FontFamilyViewModel dataContext))
        return;
      dataContext.ShowCopyRight = !string.IsNullOrEmpty(FontFamilyUtils.GetFontAuth(dataContext.Id));
    }

    private void HoverBorderMouseLeave(object sender, MouseEventArgs e)
    {
      if (this.AuthPopup.IsOpen || !(sender is FrameworkElement frameworkElement) || !(frameworkElement.DataContext is FontFamilyViewModel dataContext))
        return;
      dataContext.ShowCopyRight = false;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/theme/fontfamilysetpanel.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Text = (TextBlock) target;
          break;
        case 2:
          this.FontFamilyItems = (ItemsControl) target;
          break;
        case 5:
          this.AuthPopup = (Popup) target;
          this.AuthPopup.MouseLeave += new MouseEventHandler(this.OnCopyRightMouseLeave);
          break;
        case 6:
          this.AuthRun1 = (Run) target;
          break;
        case 7:
          this.AuthRun2 = (Run) target;
          this.AuthRun2.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnAuthClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 3)
      {
        if (connectionId != 4)
          return;
        ((UIElement) target).MouseLeave += new MouseEventHandler(this.OnCopyRightMouseLeave);
        ((UIElement) target).MouseEnter += new MouseEventHandler(this.OnCopyRightMouseEnter);
      }
      else
      {
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnFontFamilySelect);
        ((UIElement) target).MouseEnter += new MouseEventHandler(this.HoverBorderMouseEnter);
        ((UIElement) target).MouseLeave += new MouseEventHandler(this.HoverBorderMouseLeave);
      }
    }
  }
}
