// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.FontFamilyUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;
using System.Windows.Media;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class FontFamilyUtils
  {
    public const string FFMicrosoftYH = "Microsoft YaHei UI";
    public const string FFDefault_CN = "Default_CN";
    public const string FFSource_CN = "SourceHansansSC_CN";
    public const string FFLXGW_CN = "LXGW_CN";
    public const string FFYozai_CN = "Yozai_CN";
    public const string FF975Maru_CN = "975Maru_CN";
    public const string FFHarmony_CN = "HarmonyOS";
    public const string FFDefault_EN = "Default_EN";
    public const string FFRoboto_EN = "Roboto";
    public const string FFArial_EN = "Arial";
    public const string FFInter_EN = "Inter";
    public const string FFPoppins_EN = "Poppins";
    public const string FFNunito_EN = "Nunito";
    public static readonly Dictionary<string, string> FontFamilyNameDict = new Dictionary<string, string>()
    {
      {
        "Default_CN",
        "默认字体"
      },
      {
        "HarmonyOS",
        "鸿蒙字体"
      },
      {
        "SourceHansansSC_CN",
        "思源黑体"
      },
      {
        "LXGW_CN",
        "霞鹜文楷"
      },
      {
        "Yozai_CN",
        "悠哉字体"
      },
      {
        "975Maru_CN",
        "975圆体"
      }
    };
    public static readonly Dictionary<string, string> FontFamilyAuthDict = new Dictionary<string, string>()
    {
      {
        "LXGW_CN",
        "落霞孤鹜"
      },
      {
        "Yozai_CN",
        "落霞孤鹜"
      },
      {
        "975Maru_CN",
        "落霞孤鹜"
      },
      {
        "Roboto",
        "Copyright Christian Robertson"
      },
      {
        "HarmonyOS",
        "HarmonyOS Sans"
      }
    };
    public static readonly Dictionary<string, string> FontUserEventDict = new Dictionary<string, string>()
    {
      {
        "LXGW_CN",
        "fonts_lxgw"
      },
      {
        "SourceHansansSC_CN",
        "fonts_source_han_sans"
      },
      {
        "Yozai_CN",
        "fonts_yozai"
      },
      {
        "975Maru_CN",
        "fonts_975_maru"
      },
      {
        "HarmonyOS",
        "fonts_harmony"
      },
      {
        "Inter",
        "fonts_inter"
      },
      {
        "Poppins",
        "fonts_poppins"
      },
      {
        "Nunito",
        "fonts_nunito"
      },
      {
        "Arial",
        "fonts_arial"
      },
      {
        "Roboto",
        "fonts_roboto"
      }
    };
    public static readonly Dictionary<string, string> FontFamilyCopyRightLinkDict = new Dictionary<string, string>()
    {
      {
        "LXGW_CN",
        "https://github.com/lxgw/LxgwWenKai"
      },
      {
        "Yozai_CN",
        "https://github.com/lxgw/yozai-font"
      },
      {
        "975Maru_CN",
        "https://github.com/lxgw/975maru"
      },
      {
        "HarmonyOS",
        "https://developer.harmonyos.com/cn/docs/design/font-0000001157868583"
      },
      {
        "Roboto",
        "https://fonts.google.com/specimen/Roboto/about?sort=popularity"
      }
    };
    public static readonly Dictionary<string, string> FontFamilyFileNameDict = new Dictionary<string, string>()
    {
      {
        "SourceHansansSC_CN",
        "SourceHanSansSC-VF.ttf"
      },
      {
        "HarmonyOS",
        "HarmonyOS_Sans_SC_Regular.ttf|HarmonyOS_Sans_SC_Bold.ttf"
      },
      {
        "LXGW_CN",
        "LXGW.ttf"
      },
      {
        "Yozai_CN",
        "Yozai.ttf"
      },
      {
        "975Maru_CN",
        "975Maru.ttf"
      }
    };
    public static readonly Dictionary<string, string> FontFamilyDict = new Dictionary<string, string>()
    {
      {
        "Default_EN",
        "Microsoft YaHei UI"
      },
      {
        "Default_CN",
        "Microsoft YaHei UI"
      },
      {
        "Inter",
        AppPaths.ENTTFDir + "/#Inter,Microsoft YaHei UI"
      },
      {
        "Arial",
        "Arial,Microsoft YaHei UI"
      },
      {
        "Poppins",
        AppPaths.ENTTFDir + "/#Poppins,Microsoft YaHei UI"
      },
      {
        "Nunito",
        AppPaths.ENTTFDir + "/#Roboto,Microsoft YaHei UI"
      },
      {
        "Roboto",
        AppPaths.ENTTFDir + "/#Nunito,Microsoft YaHei UI"
      },
      {
        "HarmonyOS",
        AppPaths.TTFDir + "/#HarmonyOS Sans SC"
      },
      {
        "SourceHansansSC_CN",
        AppPaths.TTFDir + "/#Source Han Sans SC VF"
      },
      {
        "LXGW_CN",
        AppPaths.TTFDir + "/#霞鹜文楷 屏幕阅读版"
      },
      {
        "Yozai_CN",
        AppPaths.TTFDir + "/#悠哉字体"
      },
      {
        "975Maru_CN",
        AppPaths.TTFDir + "/#975 圆体"
      }
    };
    public static HashSet<string> NewDownloadFont = new HashSet<string>();

    public static string SourceHanSansFont => FontFamilyUtils.GetFontFamily("SourceHansansSC_CN");

    public static string SourceHarmony => FontFamilyUtils.GetFontFamily("HarmonyOS");

    public static string SourceRoboto => FontFamilyUtils.GetFontFamily("Roboto");

    public static string SourceYahei => FontFamilyUtils.GetFontFamily("Default_CN");

    public static string SourceInter => FontFamilyUtils.GetFontFamily("Inter");

    public static string SourcePoppins => FontFamilyUtils.GetFontFamily("Poppins");

    public static string SourceNunito => FontFamilyUtils.GetFontFamily("Nunito");

    public static string SourceArial => FontFamilyUtils.GetFontFamily("Arial");

    public static string SourceInterNumber => AppPaths.ENTTFDir + "/#Inter number";

    public static string GetFontFamily(string fontKey)
    {
      string fontValue = FontFamilyUtils.GetFontValue(fontKey, FontFamilyUtils.FontFamilyDict);
      return !string.IsNullOrEmpty(fontValue) ? fontValue : FontFamilyUtils.GetFontValue("Default_CN", FontFamilyUtils.FontFamilyDict);
    }

    public static FontFamily GetFontFamilyByKey(string fontKey)
    {
      FontFamily fontFamilyByKey;
      if (!(fontKey == "") && fontKey != null)
      {
        if (fontKey != null)
        {
          switch (fontKey.Length)
          {
            case 5:
              switch (fontKey[0])
              {
                case 'A':
                  if (fontKey == "Arial")
                  {
                    fontFamilyByKey = (FontFamily) App.Instance.FindResource((object) "FontArial");
                    goto label_24;
                  }
                  else
                    break;
                case 'I':
                  if (fontKey == "Inter")
                  {
                    fontFamilyByKey = (FontFamily) App.Instance.FindResource((object) "FontInter");
                    goto label_24;
                  }
                  else
                    break;
              }
              break;
            case 6:
              switch (fontKey[0])
              {
                case 'N':
                  if (fontKey == "Nunito")
                  {
                    fontFamilyByKey = (FontFamily) App.Instance.FindResource((object) "FontNunito");
                    goto label_24;
                  }
                  else
                    break;
                case 'R':
                  if (fontKey == "Roboto")
                  {
                    fontFamilyByKey = (FontFamily) App.Instance.FindResource((object) "FontRoboto");
                    goto label_24;
                  }
                  else
                    break;
              }
              break;
            case 7:
              if (fontKey == "Poppins")
              {
                fontFamilyByKey = (FontFamily) App.Instance.FindResource((object) "FontPoppins");
                goto label_24;
              }
              else
                break;
            case 9:
              if (fontKey == "HarmonyOS")
              {
                fontFamilyByKey = (FontFamily) App.Instance.FindResource((object) "FontHarmony");
                goto label_24;
              }
              else
                break;
            case 10:
              switch (fontKey[8])
              {
                case 'C':
                  if (fontKey == "Default_CN")
                    goto label_15;
                  else
                    break;
                case 'E':
                  if (fontKey == "Default_EN")
                    goto label_15;
                  else
                    break;
              }
              break;
            case 18:
              if (fontKey == "SourceHansansSC_CN")
              {
                fontFamilyByKey = (FontFamily) App.Instance.FindResource((object) "FontSourceHanSans");
                goto label_24;
              }
              else
                break;
          }
        }
        fontFamilyByKey = new FontFamily(FontFamilyUtils.GetFontFamily(fontKey));
        goto label_24;
      }
label_15:
      fontFamilyByKey = (FontFamily) App.Instance.FindResource((object) "FontYahei");
label_24:
      return fontFamilyByKey;
    }

    public static string GetFontName(string fontKey)
    {
      return FontFamilyUtils.GetFontValue(fontKey, FontFamilyUtils.FontFamilyNameDict);
    }

    public static string GetFontFileName(string fontKey)
    {
      return FontFamilyUtils.GetFontValue(fontKey, FontFamilyUtils.FontFamilyFileNameDict);
    }

    public static string GetFontAuth(string fontKey)
    {
      return FontFamilyUtils.GetFontValue(fontKey, FontFamilyUtils.FontFamilyAuthDict);
    }

    public static string GetFontCopyRightLink(string fontKey)
    {
      return FontFamilyUtils.GetFontValue(fontKey, FontFamilyUtils.FontFamilyCopyRightLinkDict);
    }

    public static string GetFontValue(string fontKey, Dictionary<string, string> dict)
    {
      return !string.IsNullOrEmpty(fontKey) && dict.ContainsKey(fontKey) ? dict[fontKey] : string.Empty;
    }

    public static string GetFontEventName(string fontKey)
    {
      return FontFamilyUtils.GetFontValue(fontKey, FontFamilyUtils.FontUserEventDict);
    }

    public static FontFamily GetFocusFontFamily()
    {
      if (!FontFamilyUtils.CanSetFontFamily())
        return FontFamilyUtils.GetFontFamilyByKey("Arial");
      string appFontFamily = LocalSettings.Settings.ExtraSettings.AppFontFamily;
      return appFontFamily == "Inter" || appFontFamily == "Poppins" ? (FontFamily) App.Instance.FindResource((object) "FontInterNumber") : FontFamilyUtils.GetFontFamilyByKey(LocalSettings.Settings.ExtraSettings.AppFontFamily);
    }

    private static bool CanSetFontFamily()
    {
      if (Utils.IsDida() && Utils.IsZhCn())
        return true;
      return !Utils.IsDida() && Utils.IsEn();
    }
  }
}
