// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.AppearanceModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class AppearanceModel
  {
    public string IconKey { get; }

    public string FontKey { get; }

    public string ThemeKey { get; }

    public int FontSize { get; }

    public int NumDisplayType { get; }

    public int ShowCompleteLine { get; }

    public AppearanceModel()
    {
      this.IconKey = LocalSettings.Settings.Common.AppIconKey;
      this.FontKey = LocalSettings.Settings.ExtraSettings.AppFontFamily;
      this.ThemeKey = LocalSettings.Settings.ExtraSettings.AppTheme;
      this.FontSize = LocalSettings.Settings.BaseFontSize;
      this.NumDisplayType = LocalSettings.Settings.ExtraSettings.NumDisplayType;
      this.ShowCompleteLine = LocalSettings.Settings.ExtraSettings.ShowCompleteLine;
    }
  }
}
