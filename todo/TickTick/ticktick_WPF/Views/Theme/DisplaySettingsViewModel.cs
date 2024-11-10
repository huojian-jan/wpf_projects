// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.DisplaySettingsViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class DisplaySettingsViewModel : BaseViewModel
  {
    private bool _selected;
    private string _imageUri;

    public string Title { get; set; }

    public int Type { get; }

    public bool Selected
    {
      get => this._selected;
      set
      {
        if (this._selected == value)
          return;
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    public string ImageUri
    {
      get => this._imageUri;
      set
      {
        if (!(this._imageUri != value))
          return;
        this._imageUri = value;
        this.OnPropertyChanged(nameof (ImageUri));
      }
    }

    public DisplaySettingsViewModel(int type, string id, string title)
    {
      this.Type = type;
      this.Title = title;
      this.ImageUri = "../../Assets/Theme/" + (LocalSettings.Settings.ThemeId == "Dark" ? "dark" : "light") + "/" + id + "_" + (Utils.IsCn() ? "cn" : "en") + ".png";
    }

    public void ChangeImage()
    {
      if (LocalSettings.Settings.ThemeId == "Dark")
        this.ImageUri = this.ImageUri?.Replace("light", "dark");
      else
        this.ImageUri = this.ImageUri?.Replace("dark", "light");
    }
  }
}
