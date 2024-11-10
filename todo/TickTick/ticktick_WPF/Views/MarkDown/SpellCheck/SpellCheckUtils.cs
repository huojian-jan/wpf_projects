// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.SpellCheck.SpellCheckUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Input;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using TickTickUtils;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.SpellCheck
{
  public static class SpellCheckUtils
  {
    public static readonly ICommand CorrectSpellingError = (ICommand) new RelayCommand((Action<object>) (o => ((SpellCheckHandler) o).CorrectSpellingError()));
    public static readonly ICommand IgnoreSpellingError = (ICommand) new RelayCommand((Action<object>) (o => ((SpellCheckHandler) o).IgnoreSpellingError()));
    public static readonly ICommand OpenOrClose = (ICommand) new RelayCommand((Action<object>) (o => SpellCheckUtils.OpenOrCloseSpellCheck((bool) o)));
    public static readonly ICommand LanguageSelected = (ICommand) new RelayCommand((Action<object>) (o => SpellCheckUtils.SetSpellCheckLanguage((string) o)));

    public static bool Downloading { get; set; }

    private static void OpenOrCloseSpellCheck(bool enable)
    {
      LocalSettings.Settings.SpellCheckEnable = enable;
    }

    private static async void SetSpellCheckLanguage(string language)
    {
      await SpellCheckUtils.CheckLanguageDicExist(language);
      SpellCheckUtils.TryDeleteCustomDict();
      LocalSettings.Settings.SpellCheckLanguage = language;
    }

    private static void TryDeleteCustomDict()
    {
      if (!System.IO.File.Exists(AppPaths.CustomDictionaryPath))
        return;
      FileInfo fileInfo = new FileInfo(AppPaths.CustomDictionaryPath);
      fileInfo.Attributes = FileAttributes.Normal;
      fileInfo.Delete();
    }

    private static async Task CheckLanguageDicExist(string language)
    {
      SpellCheckUtils.Downloading = true;
      await SpellCheckUtils.TryDownLoadLanguage(language);
      SpellCheckUtils.Downloading = false;
    }

    private static string GetRemotePath() => "https://" + BaseUrl.GetPullDomain() + "/windows/dic/";

    private static async Task TryDownLoadLanguage(string language)
    {
      if (!Directory.Exists(AppPaths.DictionariesDir))
        Directory.CreateDirectory(AppPaths.DictionariesDir);
      WebClient downloader = new WebClient();
      try
      {
        string str1 = Path.Combine(AppPaths.DictionariesDir, language + ".aff");
        if (!System.IO.File.Exists(str1) || new FileInfo(str1).Length == 0L)
          await downloader.DownloadFileTaskAsync(SpellCheckUtils.GetRemotePath() + language + ".aff", str1);
        string str2 = Path.Combine(AppPaths.DictionariesDir, language + ".dic");
        if (System.IO.File.Exists(str2) && new FileInfo(str2).Length != 0L)
        {
          downloader = (WebClient) null;
        }
        else
        {
          await downloader.DownloadFileTaskAsync(SpellCheckUtils.GetRemotePath() + language + ".dic", str2);
          downloader = (WebClient) null;
        }
      }
      catch (Exception ex)
      {
        downloader = (WebClient) null;
      }
    }

    public static void TryInitSpellLanguage()
    {
      string language = LocalSettings.Settings.SpellCheckLanguage;
      if (string.IsNullOrEmpty(language))
      {
        language = "en-US";
        string userChooseLanguage = LocalSettings.Settings.UserChooseLanguage;
        if (userChooseLanguage == "fr-FR" || userChooseLanguage == "ru-RU" || userChooseLanguage == "pt-BR")
          language = LocalSettings.Settings.UserChooseLanguage;
        LocalSettings.Settings.SettingsModel.SpellCheckLanguage = language;
      }
      SpellCheckUtils.TryDownLoadLanguage(language);
    }
  }
}
