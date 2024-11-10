// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.SpellCheck.ISpellingService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.SpellCheck
{
  public interface ISpellingService
  {
    void ClearLanguage();

    string Language { get; set; }

    bool Spell(string word);

    IEnumerable<string> Suggestions(string word);

    void Add(string word);

    string CustomDictionaryFile();

    string[] Languages();
  }
}
