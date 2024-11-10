// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.Colorizer.CodeColorResource
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.Colorizer
{
  public class CodeColorResource
  {
    private static readonly Dictionary<string, string> CodeColorsLight = new Dictionary<string, string>()
    {
      {
        "Comment",
        "B8B8B8"
      },
      {
        "String",
        "14C257"
      },
      {
        "Atom",
        "EF42B5"
      },
      {
        "Keyword",
        "FF871D"
      },
      {
        "Variable",
        "3867E4"
      }
    };
    private static readonly Dictionary<string, string> CodeColorsDark = new Dictionary<string, string>()
    {
      {
        "Comment",
        "#535353"
      },
      {
        "String",
        "#319A59"
      },
      {
        "Atom",
        "#B4317E"
      },
      {
        "Keyword",
        "#C4742E"
      },
      {
        "Variable",
        "#3F62BF"
      }
    };
    private static readonly Dictionary<string, string> JavaDict = new Dictionary<string, string>()
    {
      {
        "",
        ""
      }
    };

    public static SolidColorBrush GetColorByType(string lang, string type)
    {
      string key;
      if (!CodeColorResource.JavaDict.TryGetValue(type, out key))
        key = type;
      return ThemeKey.IsDarkTheme(LocalSettings.Settings.ThemeId) ? (!CodeColorResource.CodeColorsDark.ContainsKey(key) ? ThemeUtil.GetColor("BaseColorOpacity60") : ThemeUtil.GetColor(CodeColorResource.CodeColorsDark[key])) : (!CodeColorResource.CodeColorsLight.ContainsKey(key) ? ThemeUtil.GetColor("BaseColorOpacity60") : ThemeUtil.GetColorInString(CodeColorResource.CodeColorsDark[key]));
    }
  }
}
