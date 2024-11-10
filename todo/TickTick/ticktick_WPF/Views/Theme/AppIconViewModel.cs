// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.AppIconViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class AppIconViewModel : BaseViewModel
  {
    private bool _selected;

    public string Key { get; set; }

    public List<AppIconViewModel> Parent { get; set; }

    public bool IsDefault { get; set; }

    public bool IsPro { get; set; }

    public double Width => this.IsDefault ? 40.0 : 50.0;

    public BitmapImage Image { get; set; }

    public Visibility BorderVisible { get; set; }

    public double BorderRadius { get; set; }

    public AppIconViewModel()
    {
    }

    public AppIconViewModel(AppIconKey key, List<AppIconViewModel> parent)
    {
      this.Key = key.ToString();
      this.Parent = parent;
      this.IsPro = this.Key.EndsWith("Pro");
      this.IsDefault = key == AppIconKey.Default;
      string uriString = AppPaths.AppIconDir + "SetIcons\\" + key.ToString() + ".png";
      BitmapImage bitmapImage = new BitmapImage();
      bitmapImage.BeginInit();
      bitmapImage.DecodePixelWidth = 128;
      bitmapImage.UriSource = new Uri(uriString);
      bitmapImage.EndInit();
      bitmapImage.Freeze();
      this.BorderVisible = key < AppIconKey.CClassic || key > AppIconKey.SBlackPro ? Visibility.Collapsed : Visibility.Visible;
      this.BorderRadius = key == AppIconKey.CClassic ? 25.0 : 8.0;
      this.Image = bitmapImage;
      this.Selected = LocalSettings.Settings.Common.AppIconKey == this.Key || string.IsNullOrEmpty(LocalSettings.Settings.Common.AppIconKey) && key == AppIconKey.Default;
    }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public void ClickCommand()
    {
      if (this._selected || this.IsPro && !ProChecker.CheckPro(ProType.AppIcon))
        return;
      LocalSettings.Settings.Common.AppIconKey = this.Key;
      AppIconUtils.SetAppIcons(this.Key);
      this.Parent.Where<AppIconViewModel>((Func<AppIconViewModel, bool>) (item => item.Selected && item.Key != this.Key)).ToList<AppIconViewModel>().ForEach((Action<AppIconViewModel>) (item => item.Selected = false));
      this.Selected = true;
    }
  }
}
