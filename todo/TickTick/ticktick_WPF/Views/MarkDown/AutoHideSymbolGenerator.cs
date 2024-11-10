// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.AutoHideSymbolGenerator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using CommonMark.Syntax;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class AutoHideSymbolGenerator : VisualLineElementGenerator
  {
    private readonly MarkDownEditor _editor;
    private readonly bool _isLeft;

    public AutoHideSymbolGenerator(MarkDownEditor editor, bool isLeft)
    {
      this._editor = editor;
      this._isLeft = isLeft;
    }

    private void GetMatch(int startOffset, out int matchOffset)
    {
      List<Inline> markList = this._editor.GetMarkList();
      int num = this._editor.EditBox.CaretOffset;
      if (this._editor.EditBox.SelectionLength > 0)
        num = this._editor.EditBox.SelectionStart == this._editor.EditBox.CaretOffset ? this._editor.EditBox.SelectionStart + this._editor.EditBox.SelectionLength : this._editor.EditBox.SelectionStart;
      int endOffset = this.CurrentContext.VisualLine.LastDocumentLine.EndOffset;
      if (markList.Any<Inline>())
      {
        markList.Sort((Comparison<Inline>) ((a, b) => !this._isLeft ? (a.SourcePosition + a.SourceLength).CompareTo(b.SourcePosition + b.SourceLength) : a.SourcePosition.CompareTo(b.SourcePosition)));
        try
        {
          foreach (Inline inline in markList)
          {
            if ((!this._isLeft || inline.SourcePosition >= startOffset) && (this._isLeft || inline.SourcePosition <= startOffset))
            {
              if (inline.SourcePosition + inline.SourceLength <= endOffset)
              {
                if (!this._editor.CurrentFocused || inline.SourcePosition > num || inline.SourcePosition + inline.SourceLength < num)
                {
                  string str1 = inline.Emphasis.DelimiterCharacter.ToString();
                  if (!string.IsNullOrEmpty(str1))
                  {
                    if (str1 == "\0")
                    {
                      switch (inline.Tag)
                      {
                        case InlineTag.Emphasis:
                          str1 = "*";
                          break;
                        case InlineTag.UnderLine:
                          str1 = "~";
                          break;
                      }
                    }
                    switch (inline.Tag)
                    {
                      case InlineTag.Strong:
                      case InlineTag.Strikethrough:
                      case InlineTag.HighLight:
                        str1 += str1;
                        break;
                    }
                    if (this._isLeft || inline.SourcePosition + inline.SourceLength - str1.Length >= startOffset)
                    {
                      string str2 = this.CurrentContext.Document.Text.Substring(inline.SourcePosition, inline.SourceLength);
                      if (this._isLeft && str2.StartsWith(str1) || !this._isLeft && str2.EndsWith(str1))
                      {
                        matchOffset = this._isLeft ? inline.SourcePosition : inline.SourcePosition + inline.SourceLength - str1.Length;
                        return;
                      }
                    }
                  }
                }
              }
              else
                break;
            }
          }
        }
        catch (Exception ex)
        {
        }
      }
      matchOffset = -1;
    }

    public override int GetFirstInterestedOffset(int startOffset)
    {
      int matchOffset;
      this.GetMatch(startOffset, out matchOffset);
      return matchOffset;
    }

    public override VisualLineElement ConstructElement(int offset)
    {
      try
      {
        Inline inline1 = (Inline) null;
        List<Inline> markList = this._editor.GetMarkList();
        if (this._isLeft)
        {
          markList.Sort((Comparison<Inline>) ((a, b) => a.SourcePosition.CompareTo(b.SourcePosition)));
          foreach (Inline inline2 in markList)
          {
            if (inline2.SourcePosition == offset)
            {
              inline1 = inline2;
              break;
            }
          }
        }
        else
        {
          markList.Sort((Comparison<Inline>) ((a, b) => (a.SourcePosition + a.SourceLength).CompareTo(b.SourcePosition + b.SourceLength)));
          foreach (Inline inline3 in markList)
          {
            if (inline3.SourcePosition < offset && inline3.SourcePosition + inline3.SourceLength > offset)
            {
              inline1 = inline3;
              break;
            }
          }
        }
        if (inline1 == null)
          return (VisualLineElement) null;
        string str = inline1.Emphasis.DelimiterCharacter.ToString();
        if (string.IsNullOrEmpty(str))
          return (VisualLineElement) null;
        switch (inline1.Tag)
        {
          case InlineTag.Strong:
          case InlineTag.Strikethrough:
          case InlineTag.HighLight:
            str += str;
            break;
        }
        return (VisualLineElement) new HideMarkElement(str.Length, this._isLeft || inline1.Tag != InlineTag.Emphasis ? 0.01 : 3.0, FormattedTextElement.PrepareText(TextFormatter.Create(), string.Empty, (TextRunProperties) new VisualLineElementTextRunProperties(this.CurrentContext.GlobalTextRunProperties)));
      }
      catch (Exception ex)
      {
        return (VisualLineElement) null;
      }
    }
  }
}
