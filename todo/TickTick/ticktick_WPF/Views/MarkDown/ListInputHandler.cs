// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.ListInputHandler
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public static class ListInputHandler
  {
    public static readonly Regex UnorderedListCheckboxPattern = new Regex("^\\s{0,40}[ ]{0,3}[-\\*\\+][ ]{1,3}\\[[ xX]\\] ", RegexOptions.Compiled);
    public static readonly Regex OrderedListPattern = new Regex("^\\s{0,40}[ ]{0,3}(\\d+)\\. ", RegexOptions.Compiled);
    public static readonly Regex UnorderedListPattern = new Regex("^\\s{0,40}[ ]{0,3}[-\\*\\+] ", RegexOptions.Compiled);
    public static readonly Regex BlockQuotePattern = new Regex("^\\s{0,40}[ ]{0,3}> ", RegexOptions.Compiled);
    public static readonly Regex ImageRegex = new Regex("!\\[([^\\)]+)\\]\\(([^\\)]+)\\)");

    public static void TryRemoveEmptyLineOnDelete(TextEditor editor)
    {
      if (editor.CaretOffset != 0)
        return;
      ListInputHandler.TryRemoveEmptyChar(editor);
      ListInputHandler.TryRemoveEmptyChar(editor);
    }

    private static void TryRemoveEmptyChar(TextEditor editor)
    {
      TextDocument document = editor.Document;
      DocumentLine lineByOffset = document.GetLineByOffset(editor.SelectionStart);
      if (lineByOffset == null || lineByOffset.NextLine == null || !string.IsNullOrEmpty(document.GetText(0, lineByOffset.Length).Trim()))
        return;
      editor.Document.Remove(editor.CaretOffset, lineByOffset.Length + 1);
    }

    public static void AdjustList(TextEditor editor, bool lineCountIncreased)
    {
      TextDocument document = editor.Document;
      DocumentLine line = document.GetLineByOffset(editor.SelectionStart)?.PreviousLine ?? document.GetLineByOffset(editor.SelectionStart);
      if (line == null)
        return;
      string text = document.GetText(line.Offset, line.Length);
      DocumentLine nextLine = line.NextLine;
      string nextText = string.Empty;
      if (nextLine != null)
        nextText = document.GetText(nextLine.Offset, nextLine.Length);
      if (!string.IsNullOrEmpty(nextText) && ListInputHandler.ImageRegex.Match(nextText).Value == nextText)
        return;
      Action<Action> ifIncreased = (Action<Action>) (func =>
      {
        if (!lineCountIncreased)
          return;
        func();
      });
      document.BeginUpdate();
      ((Action<string, IEnumerable<Tuple<Regex, Action<Match>>>>) ((txt, patterns) =>
      {
        var data = patterns.Select(pattern => new
        {
          Match = pattern.Item1.Match(txt),
          Action = pattern.Item2
        }).FirstOrDefault(ma => ma.Match.Success);
        if (data == null)
          return;
        data.Action(data.Match);
      }))(text, (IEnumerable<Tuple<Regex, Action<Match>>>) new Tuple<Regex, Action<Match>>[4]
      {
        new Tuple<Regex, Action<Match>>(ListInputHandler.UnorderedListCheckboxPattern, (Action<Match>) (m => ifIncreased((Action) (() =>
        {
          string str = m.Groups[0].Value.TrimStart();
          if (!(text.TrimStart() == str) || !string.IsNullOrEmpty(nextText.Trim()))
          {
            if (document.GetLineByOffset(editor.SelectionStart).Offset + nextText.Length - nextText.TrimStart().Length != editor.SelectionStart)
              return;
            document.Insert(editor.SelectionStart, m.Groups[0].Value.Replace("x", " ").TrimStart());
          }
          else
            ListInputHandler.RemoveEmptyLine(editor, document, line);
        })))),
        new Tuple<Regex, Action<Match>>(ListInputHandler.UnorderedListPattern, (Action<Match>) (m => ifIncreased((Action) (() =>
        {
          string str = m.Groups[0].Value.TrimStart();
          if (!(text.TrimStart() == str) || !string.IsNullOrEmpty(nextText.Trim()))
            document.Insert(editor.SelectionStart, m.Groups[0].Value.TrimStart());
          else
            ListInputHandler.RemoveEmptyLine(editor, document, line);
        })))),
        new Tuple<Regex, Action<Match>>(ListInputHandler.OrderedListPattern, (Action<Match>) (m =>
        {
          int result;
          if (!int.TryParse(m.Groups[1].Value, out result))
            return;
          if (!(text.TrimStart() == result.ToString() + ". ") || !string.IsNullOrEmpty(nextText.Trim()))
          {
            if (!lineCountIncreased)
              return;
            document.Insert(editor.SelectionStart, (result + 1).ToString() + ". ");
            line = line.NextLine;
          }
          else
            ListInputHandler.RemoveEmptyLine(editor, document, line);
        })),
        new Tuple<Regex, Action<Match>>(ListInputHandler.BlockQuotePattern, (Action<Match>) (m => ifIncreased((Action) (() =>
        {
          string str = m.Groups[0].Value.TrimStart();
          if (!(text.TrimStart() == str) || !string.IsNullOrEmpty(nextText.Trim()))
            document.Insert(editor.SelectionStart, m.Groups[0].Value.TrimStart());
          else
            ListInputHandler.RemoveEmptyLine(editor, document, line);
        }))))
      });
      editor.AdjustNumberedList(false);
      document.EndUpdate();
    }

    private static void RemoveEmptyLine(
      TextEditor editor,
      TextDocument document,
      DocumentLine line)
    {
      document.Remove((ISegment) line);
      int num = editor.CaretOffset - 2;
      if (num < 0)
        return;
      editor.CaretOffset = num;
      editor.Document.Remove(editor.CaretOffset, 2);
    }

    private static void RenumberOrderedList(IDocument document, DocumentLine line, int number)
    {
      if (line == null)
        return;
      while ((line = line.NextLine) != null)
      {
        ++number;
        string text = document.GetText(line.Offset, line.Length);
        Match match = ListInputHandler.OrderedListPattern.Match(text);
        if (!match.Success)
          break;
        Group group = match.Groups[1];
        if (int.Parse(group.Value) != number)
          document.Replace(line.Offset + group.Index, group.Length, number.ToString());
      }
    }
  }
}
