// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.SpellCheck.SpellCheckProvider
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using ticktick_WPF.Util;
using ticktick_WPF.Views.MarkDown.Colorizer;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.SpellCheck
{
  public class SpellCheckProvider : ISpellCheckProvider
  {
    private readonly Regex _mardownUri = new Regex("\\[([^\\[]+)\\]\\(([^\\)]+)\\)");
    private readonly SpellCheckBackgroundRenderer _spellCheckRenderer;
    private readonly ISpellingService _spellingService;
    private readonly Regex _uriFinderRegex = new Regex("(http|ftp|https|mailto):\\/\\/[\\w\\-_]+(\\.[\\w\\-_]+)+([\\w\\-\\.,@?^=%&amp;:/~\\+#]*[\\w\\-\\@?^=%&amp;/~\\+#])?", RegexOptions.Compiled);
    private readonly Regex _wordSeparatorRegex = new Regex("-[^\\w]+|^'[^\\w]+|[^\\w]+'[^\\w]+|[^\\w]+-[^\\w]+|[^\\w]+'$|[^\\w]+-$|^-$|^'$|[^\\w'-]", RegexOptions.Compiled);
    private ISpellChecker _editor;
    private bool _enabled = true;
    private bool _textChanged;
    private bool _delay;
    private string _uid;

    public SpellCheckProvider(ISpellingService spellingService)
    {
      this._uid = Utils.GetGuid();
      this._spellingService = spellingService;
      this._spellCheckRenderer = new SpellCheckBackgroundRenderer();
    }

    private void TryDoSpell(object sender, EventArgs e)
    {
      Utils.RunOnUiThread(Application.Current?.Dispatcher, new Action(this.DoSpellCheck));
    }

    public void Initialize(ISpellChecker editor)
    {
      this._editor = editor;
      this._editor.GetEditBox().TextArea.TextView.BackgroundRenderers.Add((IBackgroundRenderer) this._spellCheckRenderer);
      this._editor.GetEditBox().TextArea.TextView.VisualLinesChanged += new EventHandler(this.TextViewVisualLinesChanged);
    }

    public bool Enabled
    {
      get => this._enabled;
      set
      {
        this._enabled = value;
        if (this._enabled)
          return;
        this.ClearSpellCheckErrors();
      }
    }

    public string CustomDictionaryFile() => this._spellingService.CustomDictionaryFile();

    public void Disconnect()
    {
      if (this._editor == null)
        return;
      this.ClearSpellCheckErrors();
      this._editor.GetEditBox().TextArea.TextView.BackgroundRenderers.Remove((IBackgroundRenderer) this._spellCheckRenderer);
      this._editor.GetEditBox().TextArea.TextView.VisualLinesChanged -= new EventHandler(this.TextViewVisualLinesChanged);
      this._editor = (ISpellChecker) null;
    }

    public ISpellingService SpellingService() => this._spellingService;

    public IEnumerable<TextSegment> GetSpellCheckErrors()
    {
      return this._spellCheckRenderer != null ? (IEnumerable<TextSegment>) this._spellCheckRenderer.ErrorSegments : Enumerable.Empty<TextSegment>();
    }

    public IEnumerable<string> GetSpellCheckSuggestions(string word)
    {
      return this._spellCheckRenderer != null ? this._spellingService.Suggestions(word) : Enumerable.Empty<string>();
    }

    public void Add(string word)
    {
      if (string.IsNullOrWhiteSpace(word) || this._spellingService == null || !this.Enabled)
        return;
      this._spellingService.Add(word);
    }

    private void TextViewVisualLinesChanged(object sender, EventArgs e)
    {
      if (!this.Enabled)
        return;
      DelayActionHandlerCenter.TryDoAction(this._uid, new EventHandler(this.TryDoSpell), this._delay ? 300 : 100);
    }

    private void DoSpellCheck()
    {
      if (this._editor == null || !this._textChanged || !this._editor.GetEditBox().TextArea.TextView.VisualLinesValid)
        return;
      this._spellCheckRenderer.ErrorSegments.Clear();
      ParallelQuery<VisualLine> parallelQuery = this._editor.GetEditBox().TextArea.TextView.VisualLines.AsParallel<VisualLine>();
      this.ClearSpellCheckErrors();
      foreach (VisualLine visualLine in (IEnumerable<VisualLine>) parallelQuery)
      {
        int startIndex = 0;
        int offset = visualLine.FirstDocumentLine.Offset;
        int length = visualLine.LastDocumentLine.EndOffset - offset;
        string input = Regex.Replace(this._editor.GetEditBox().Document.GetText(offset, length), "[\\u2018\\u2019\\u201A\\u201B\\u2032\\u2035\\u3040-\\u309F\\u30A0-\\u30FF\\u3100-\\u312F\\u3100-\\u312F\\u31A0-\\u31BF\\u3400-\\u4DBF\\u4E00-\\u9FFF\\uAC00-\\uD7AF\\uF900-\\uFAFF\\uFF66-\\uFFDF]", " ");
        string[] array = ((IEnumerable<string>) this._wordSeparatorRegex.Split(this._mardownUri.Replace(this._uriFinderRegex.Replace(input, ""), ""))).Where<string>((Func<string, bool>) (s => !string.IsNullOrEmpty(s))).Where<string>((Func<string, bool>) (w => !Regex.Match(w, "\\d").Success)).ToArray<string>();
        int num1 = 0;
        foreach (string str in (IEnumerable<string>) array)
        {
          if (num1 < 20)
          {
            string word = str.Trim('\'', '_', '-');
            int num2 = visualLine.FirstDocumentLine.Offset + input.IndexOf(word, startIndex, StringComparison.InvariantCultureIgnoreCase);
            if (num2 >= 0)
            {
              if (!this._spellingService.Spell(word))
              {
                this._spellCheckRenderer.ErrorSegments.Add(new TextSegment()
                {
                  StartOffset = num2,
                  Length = str.Length
                });
                ++num1;
              }
              startIndex = input.IndexOf(str, startIndex, StringComparison.InvariantCultureIgnoreCase) + str.Length;
            }
            else
              break;
          }
          else
            break;
        }
      }
      this._editor.GetEditBox().TextArea.TextView.Redraw();
      this._textChanged = false;
    }

    private void ClearSpellCheckErrors() => this._spellCheckRenderer?.ErrorSegments.Clear();

    public void SetTextChanged(bool change, bool delay = true)
    {
      this._spellCheckRenderer.ErrorSegments.Clear();
      this._textChanged = change;
      this._delay = delay;
    }
  }
}
