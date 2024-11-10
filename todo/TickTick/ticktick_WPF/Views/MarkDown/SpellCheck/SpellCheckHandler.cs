// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.SpellCheck.SpellCheckHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Document;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.SpellCheck
{
  public class SpellCheckHandler
  {
    private string _correct;
    private TextSegment _errorSegment;
    private ISpellChecker _editor;
    private string _error;

    public SpellCheckHandler(
      ISpellChecker editor,
      string correct,
      string errorText,
      TextSegment errorSegment)
    {
      this._editor = editor;
      this._correct = correct;
      this._error = errorText;
      this._errorSegment = errorSegment;
    }

    public void CorrectSpellingError()
    {
      this._editor?.CorrectSpellingError(this._correct, this._errorSegment);
    }

    public void IgnoreSpellingError()
    {
      this._editor?.AddErrorToDict(this._error, this._errorSegment);
    }
  }
}
