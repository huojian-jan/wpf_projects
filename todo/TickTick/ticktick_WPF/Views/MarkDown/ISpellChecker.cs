// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.ISpellChecker
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ticktick_WPF.Views.MarkDown.SpellCheck;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public interface ISpellChecker
  {
    TextEditor GetEditBox();

    SpellCheckProvider SpellCheckProvider { get; set; }

    void CorrectSpellingError(string correct, TextSegment errorSegment);

    void AddErrorToDict(string error, TextSegment errorSegment);
  }
}
