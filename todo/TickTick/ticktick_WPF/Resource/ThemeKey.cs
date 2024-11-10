// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Resource.ThemeKey
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Resource
{
  public static class ThemeKey
  {
    public const string Default = "White";
    public const string Blue = "Blue";
    public const string Pink = "Pink";
    public const string Black = "Black";
    public const string Green = "Green";
    public const string Gray = "Gray";
    public const string Yellow = "Yellow";
    public const string Dark = "Dark";
    public const string Spring = "Spring";
    public const string Summer = "Summer";
    public const string Autumn = "Autumn";
    public const string Winter = "Winter";
    public const string Beijing = "Beijing";
    public const string Hangzhou = "Hangzhou";
    public const string London = "London";
    public const string Moscow = "Moscow";
    public const string SanFrancisco = "Sanfrancisco";
    public const string Seoul = "Seoul";
    public const string Shanghai = "Shanghai";
    public const string Sydney = "Sydney";
    public const string Tokyo = "Tokyo";
    public const string NewYork = "NewYork";
    public const string Desert = "Desert";
    public const string Structure = "Structure";
    public const string Blacksea = "Blacksea";
    public const string Leaves = "Leaves";
    public const string Birds = "Birds";
    public const string Guangzhou = "Guangzhou";
    public const string Shenzhen = "Shenzhen";
    public const string Cairo = "Cairo";
    public const string LosAngeles = "LosAngeles";
    public const string Custom = "Custom";
    public const string Blossom = "Blossom";
    public const string Dawn = "Dawn";
    public const string Frozen = "Frozen";
    public const string Meadow = "Meadow";
    public const string Silence = "Silence";

    public static bool ShowProjectTextShadow(string themeId)
    {
      return ThemeKey.IsProTheme(themeId) && !ThemeKey.IsSeasonTheme(themeId) && themeId != "Frozen" && themeId != "Blossom";
    }

    public static bool IsDarkTheme(string key) => key == "Dark";

    public static bool IsGreenTheme(string key) => key == "Green";

    public static bool IsPinkTheme(string key) => key == "Pink";

    public static bool IsSeasonTheme(string key)
    {
      return key == "Spring" || key == "Summer" || key == "Autumn" || key == "Winter";
    }

    public static bool IsColorTheme(string key)
    {
      return key == "White" || key == "Blue" || key == "Pink" || key == "Black" || key == "Green" || key == "Gray" || key == "Yellow";
    }

    public static bool IsProTheme(string key)
    {
      return !ThemeKey.IsDarkTheme(key) && !ThemeKey.IsColorTheme(key);
    }

    public static bool IsCustomTheme(string key) => key == "Custom";
  }
}
