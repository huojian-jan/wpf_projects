// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.FontFamilyViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class FontFamilyViewModel : BaseViewModel
  {
    private bool _loading;
    private bool _showCopyRight;
    private bool _selected;

    public string Title { get; set; }

    public string Id { get; set; }

    public string FontImageUri { get; set; }

    public FontFamily Font { get; set; }

    public bool ShowText { get; set; }

    public bool NeedPro { get; set; }

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public bool Loading
    {
      get => this._loading;
      set
      {
        this._loading = value;
        this.OnPropertyChanged(nameof (Loading));
      }
    }

    public bool ShowCopyRight
    {
      get => this._showCopyRight;
      set
      {
        this._showCopyRight = value;
        this.OnPropertyChanged(nameof (ShowCopyRight));
      }
    }

    public FontFamilyViewModel(string id, bool isPro = true, bool showText = true)
    {
      this.Id = id;
      this.Title = FontFamilyUtils.GetFontName(id);
      this.ShowText = showText;
      this.NeedPro = isPro;
      if (showText)
        this.Font = FontFamilyUtils.GetFontFamilyByKey(id);
      string str;
      if (!showText)
        str = "../../Assets/Theme/FontFamily/" + (LocalSettings.Settings.ThemeId == "Dark" ? "dark" : "light") + "/" + id + ".png";
      else
        str = string.Empty;
      this.FontImageUri = str;
      this.Selected = LocalSettings.Settings.ExtraSettings.AppFontFamily == id;
    }

    public static List<FontFamilyViewModel> GetChineseFontViewModel()
    {
      List<FontFamilyViewModel> chineseFontViewModel = new List<FontFamilyViewModel>()
      {
        new FontFamilyViewModel("Default_CN", false)
        {
          Selected = LocalSettings.Settings.ExtraSettings.AppFontFamily == "Default_CN" || string.IsNullOrEmpty(LocalSettings.Settings.ExtraSettings.AppFontFamily)
        },
        new FontFamilyViewModel("HarmonyOS", false, false),
        new FontFamilyViewModel("LXGW_CN", showText: false),
        new FontFamilyViewModel("Yozai_CN", showText: false),
        new FontFamilyViewModel("975Maru_CN", showText: false)
      };
      if (!Utils.IsWindows7())
        chineseFontViewModel.Insert(2, new FontFamilyViewModel("SourceHansansSC_CN", false, false));
      return chineseFontViewModel;
    }

    public static List<FontFamilyViewModel> GetEnFontViewModel()
    {
      return new List<FontFamilyViewModel>()
      {
        new FontFamilyViewModel("Default_EN", false)
        {
          Selected = LocalSettings.Settings.ExtraSettings.AppFontFamily == "Default_EN" || string.IsNullOrEmpty(LocalSettings.Settings.ExtraSettings.AppFontFamily)
        },
        new FontFamilyViewModel("Roboto", false),
        new FontFamilyViewModel("Arial", false),
        new FontFamilyViewModel("Inter"),
        new FontFamilyViewModel("Poppins"),
        new FontFamilyViewModel("Nunito")
      };
    }

    public void ChangeImage()
    {
      this.FontImageUri = !(LocalSettings.Settings.ThemeId == "Dark") ? this.FontImageUri?.Replace("dark", "light") : this.FontImageUri?.Replace("light", "dark");
      this.OnPropertyChanged("FontImageUri");
    }

    public bool ShowAuthor() => this.Id != "Roboto" && this.Id != "HarmonyOS";
  }
}
