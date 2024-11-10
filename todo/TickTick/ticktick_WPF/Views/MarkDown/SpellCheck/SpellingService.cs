// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.SpellCheck.SpellingService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using NHunspell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.SpellCheck
{
  public class SpellingService : ISpellingService
  {
    private string _language;
    private Hunspell _speller;
    public static SpellingService CommonSpellingService = (SpellingService) null;
    private static readonly string _dir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Tick_Tick";

    public SpellingService()
    {
      this.Language = LocalSettings.Settings.SpellCheckLanguage;
      LocalSettings.SpellCheckChanged += new EventHandler(this.OnSpellLanguageChanged);
    }

    private void OnSpellLanguageChanged(object sender, EventArgs e)
    {
      if (!LocalSettings.Settings.SpellCheckEnable)
        return;
      this.Language = LocalSettings.Settings.SpellCheckLanguage;
    }

    public bool Spell(string word) => this._speller == null || this._speller.Spell(word);

    public IEnumerable<string> Suggestions(string word)
    {
      return (IEnumerable<string>) this._speller.Suggest(word);
    }

    public void Add(string word)
    {
      if (this._speller == null)
        return;
      this._speller.Add(word);
      this.UpdateCustomDictionary(word);
    }

    public void ClearLanguage() => this._speller = (Hunspell) null;

    public string Language
    {
      get => this._language;
      set => this.SetLanguage(value);
    }

    public string CustomDictionaryFile()
    {
      string path = Path.Combine(SpellingService._dir, "custom_dictionary.txt");
      if (!File.Exists(path))
      {
        if (!Directory.Exists(SpellingService._dir))
          Directory.CreateDirectory(SpellingService._dir);
        Directory.CreateDirectory(SpellingService._dir);
        File.WriteAllText(path, string.Empty);
      }
      return path;
    }

    public string[] Languages()
    {
      return ((IEnumerable<string>) Directory.GetFiles(SpellingService.SpellCheckFolder(), "*.dic")).Select<string, string>(new Func<string, string>(Path.GetFileNameWithoutExtension)).ToArray<string>();
    }

    private static string SpellCheckFolder() => SpellingService._dir + "\\Dictionaries";

    private void SetLanguage(string language)
    {
      if (string.IsNullOrEmpty(language))
        return;
      try
      {
        this.ClearLanguage();
        Hunspell speller = new Hunspell();
        string path1 = SpellingService.SpellCheckFolder();
        string str1 = Path.Combine(path1, language + ".aff");
        string str2 = Path.Combine(path1, language + ".dic");
        if (!File.Exists(str1) || !File.Exists(str2))
          return;
        speller.Load(str1, str2);
        this.LoadCustomDictonary(speller);
        this._speller = speller;
        this._language = language;
      }
      catch (Exception ex)
      {
      }
    }

    private void LoadCustomDictonary(Hunspell speller)
    {
      try
      {
        foreach (string readAllLine in File.ReadAllLines(this.CustomDictionaryFile()))
          speller.Add(readAllLine);
      }
      catch (Exception ex)
      {
      }
    }

    private void UpdateCustomDictionary(string word)
    {
      try
      {
        File.AppendAllLines(this.CustomDictionaryFile(), (IEnumerable<string>) new string[1]
        {
          word
        });
      }
      catch (Exception ex)
      {
      }
    }
  }
}
