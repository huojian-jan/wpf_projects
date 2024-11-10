// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.SpellCheck.ISpellCheckProvider
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Document;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.SpellCheck
{
  public interface ISpellCheckProvider
  {
    bool Enabled { get; set; }

    void Initialize(ISpellChecker editor);

    IEnumerable<TextSegment> GetSpellCheckErrors();

    IEnumerable<string> GetSpellCheckSuggestions(string word);

    void Add(string word);

    string CustomDictionaryFile();

    void Disconnect();

    ISpellingService SpellingService();
  }
}
