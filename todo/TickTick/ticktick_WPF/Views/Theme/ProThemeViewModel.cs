// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.ProThemeViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class ProThemeViewModel : ThemeBaseModel
  {
    private bool _loading;

    public string Url
    {
      get
      {
        return "https://" + BaseUrl.GetPullDomain() + "/windows/theme/logo/" + this.Key.ToLower() + ".png";
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
  }
}
