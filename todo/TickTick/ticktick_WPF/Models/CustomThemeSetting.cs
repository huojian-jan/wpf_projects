// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CustomThemeSetting
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Models
{
  public class CustomThemeSetting
  {
    public double ThemeImageOpacity { get; set; } = 1.0;

    public int ThemeImageBlurRadius { get; set; }

    public double ShowAreaOpacity { get; set; } = 0.9;

    public string CustomThemeLocation { get; set; }

    public string CustomThemeColor { get; set; }

    public void Reset()
    {
      this.ThemeImageOpacity = 1.0;
      this.ThemeImageBlurRadius = 0;
      this.ShowAreaOpacity = 0.9;
      this.CustomThemeLocation = string.Empty;
    }
  }
}
