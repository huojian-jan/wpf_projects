// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.EditorUtilities
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Threading;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public static class EditorUtilities
  {
    public static void SelectWordAt(this TextEditor editor, int offset)
    {
      if (offset < 0 || offset >= editor.Document.TextLength || !EditorUtilities.IsWordPart(editor.Document.GetCharAt(offset)))
        return;
      int start = offset;
      int num = offset;
      TextDocument document = editor.Document;
      while (start > 0 && EditorUtilities.IsWordPart(document.GetCharAt(start - 1)))
        --start;
      while (num < document.TextLength - 1 && EditorUtilities.IsWordPart(document.GetCharAt(num + 1)))
        ++num;
      editor.Select(start, num - start + 1);
    }

    private static bool IsWordPart(char ch) => char.IsLetterOrDigit(ch) || ch == '_' || ch == '*';

    public static void SelectHeader(this TextEditor editor, bool next)
    {
      int startat = editor.SelectionStart + (next ? editor.SelectionLength : 0);
      Match match = new Regex("^#{1,6}[^#].*", (RegexOptions) (2 | (next ? 0 : 64))).Match(editor.Text, startat);
      if (!match.Success)
      {
        Notify.Beep();
      }
      else
      {
        editor.Select(match.Index, match.Length);
        TextLocation location = editor.Document.GetLocation(match.Index);
        editor.ScrollTo(location.Line, location.Column);
      }
    }

    public static void ReplaceChar(this TextEditor editor, char origin, char revised)
    {
      editor.Document.Insert(editor.TextArea.Caret.Offset, "x");
    }

    public static void AddHorizontalLine(TextEditor editor)
    {
      DocumentLine lineByOffset = editor.Document.GetLineByOffset(editor.CaretOffset);
      if (lineByOffset == null)
        return;
      string str = editor.Document.GetText(lineByOffset.Offset, lineByOffset.Length).Trim();
      if (string.IsNullOrEmpty(str))
      {
        editor.Document.Remove((ISegment) lineByOffset);
        editor.Document.Insert(lineByOffset.Offset, "---\n");
      }
      else
      {
        editor.Document.Remove(lineByOffset.Offset, str.Length);
        if (lineByOffset.NextLine == null)
          editor.Document.Insert(lineByOffset.Offset, str + "\n---\n");
        else
          editor.Document.Insert(lineByOffset.Offset, str + "\n---");
      }
      editor.TryFocus();
    }

    public static bool IsListTypeText(string content, out int offset, out int length)
    {
      if (!string.IsNullOrEmpty(content))
      {
        if (new Regex("^\\s{0,40}> .*").IsMatch(content))
        {
          offset = content.IndexOf(">", StringComparison.Ordinal);
          length = 2;
          return true;
        }
        if (new Regex("^\\s{0,40}- \\[[ xX]\\] .*").IsMatch(content))
        {
          offset = content.IndexOf("-", StringComparison.Ordinal);
          length = 6;
          return true;
        }
        if (new Regex("^\\s{0,40}[-\\*\\+] .*").IsMatch(content))
        {
          offset = content.IndexOf("-", StringComparison.Ordinal);
          if (offset <= 0)
            offset = content.IndexOf("*", StringComparison.Ordinal);
          if (offset <= 0)
            offset = content.IndexOf("+", StringComparison.Ordinal);
          length = 2;
          return true;
        }
        Match match = new Regex("^\\s{0,40}(\\d+)\\. .*").Match(content);
        if (match.Success)
        {
          string str = match.Groups[1].ToString();
          offset = content.IndexOf(".", StringComparison.Ordinal) - str.Length;
          length = 3 + str.Length - 1;
          return true;
        }
      }
      length = 0;
      offset = 0;
      return false;
    }

    public static async void AddRemoveTextInStart(TextEditor editor, string quote)
    {
      string selectedText = editor.SelectedText;
      if (string.IsNullOrEmpty(selectedText) || !selectedText.Contains("\n") && !selectedText.Contains("\r"))
      {
        DocumentLine lineByOffset = editor.Document.GetLineByOffset(editor.CaretOffset);
        if (lineByOffset == null)
          return;
        EditorUtilities.HandleLineList(editor, quote, lineByOffset);
      }
      else
        EditorUtilities.AddOrRemoveSelectionQuote(editor, quote);
      Task.Delay(100);
      editor.TryFocus();
    }

    private static void HandleLineList(TextEditor editor, string quote, DocumentLine line)
    {
      string text = editor.Document.GetText(line.Offset, line.Length);
      string str = text.Substring(0, text.Length - text.TrimStart().Length);
      int offset;
      int length;
      if (EditorUtilities.IsListTypeText(text, out offset, out length))
      {
        if (offset < 0)
          offset = 0;
        editor.Document.Remove(line.Offset + offset, length);
        if (text.TrimStart().StartsWith(quote))
          return;
        editor.Document.Insert(line.Offset, str + quote);
        editor.Document.Remove(line.Offset + (str + quote).Length, str.Length);
      }
      else
        editor.Document.Insert(line.Offset, quote);
    }

    public static void AddText(this TextEditor editor, string insert, bool focused)
    {
      if (!focused)
      {
        int length = editor.Text.Length;
        DocumentLine lineByOffset = editor.Document.GetLineByOffset(length);
        string text1 = editor.Document.GetText(lineByOffset.Offset, lineByOffset.Length);
        string text2 = insert;
        if (!string.IsNullOrEmpty(text1))
          text2 = "\r\n" + text2;
        editor.Document.Insert(editor.Text.Length, text2);
        editor.CaretOffset = Math.Max(0, length + text2.Length);
        editor.TryFocus();
      }
      else if (string.IsNullOrEmpty(editor.SelectedText))
      {
        editor.Document.Insert(editor.TextArea.Caret.Offset, insert);
        editor.TryFocus();
      }
      else
        editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, insert);
    }

    public static void AddRemoveText(this TextEditor editor, string quote, bool focused = false)
    {
      if (!focused)
      {
        int length = editor.Text.Length;
        DocumentLine lineByOffset = editor.Document.GetLineByOffset(length);
        string text1 = editor.Document.GetText(lineByOffset.Offset, lineByOffset.Length);
        string text2 = quote + quote;
        if (!string.IsNullOrEmpty(text1))
          text2 = "\r\n" + text2;
        editor.Document.Insert(editor.Text.Length, text2);
        editor.CaretOffset = Math.Max(0, length + text2.Length - quote.Length);
        editor.TryFocus();
      }
      else
      {
        string selectedText = editor.SelectedText;
        if (string.IsNullOrEmpty(selectedText))
        {
          editor.SelectWordAt(editor.CaretOffset);
          selectedText = editor.SelectedText;
        }
        if (string.IsNullOrEmpty(selectedText))
        {
          editor.Document.Insert(editor.TextArea.Caret.Offset, quote + quote);
          editor.CaretOffset -= quote.Length;
          editor.TryFocus();
        }
        else
        {
          bool flag = selectedText.StartsWith(quote) && selectedText.EndsWith(quote);
          if (flag)
          {
            if (quote == "*" && selectedText.StartsWith("**") && selectedText.EndsWith("**") && (!selectedText.StartsWith("***") || !selectedText.EndsWith("***")))
              flag = false;
            if (quote == "~" && selectedText.StartsWith("~~") && selectedText.EndsWith("~~") && (!selectedText.StartsWith("~~~") || !selectedText.EndsWith("~~~")))
              flag = false;
          }
          editor.SelectedText = flag ? selectedText.UnSurroundWith(quote) : selectedText.SurroundWith(quote);
        }
      }
    }

    public static bool Find(this TextEditor editor, Regex find)
    {
      try
      {
        bool flag = find.Options.HasFlag((Enum) RegexOptions.RightToLeft);
        int startat = flag ? editor.SelectionStart : editor.SelectionStart + editor.SelectionLength;
        Match match = find.Match(editor.Text, startat);
        if (!match.Success)
          match = find.Match(editor.Text, flag ? editor.Text.Length : 0);
        if (match.Success)
        {
          editor.Select(match.Index, match.Length);
          TextLocation location = editor.Document.GetLocation(match.Index);
          editor.ScrollTo(location.Line, location.Column);
        }
        return match.Success;
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex.ToString());
        return false;
      }
    }

    public static void AdjustNumberedList(this TextEditor editor, bool levelChange = true)
    {
      List<NumberedListOrder> source = new List<NumberedListOrder>();
      int lineCount = editor.LineCount;
      int line = editor.TextArea.Caret.Line;
      Regex regex = new Regex("(\\d+)\\. ");
      for (int index = line; index >= 1; --index)
      {
        DocumentLine lineByNumber = editor.Document.GetLineByNumber(index);
        string text = editor.Document.GetText(lineByNumber.Offset, lineByNumber.Length);
        Match match = regex.Match(text);
        if (match.Success)
        {
          string s = match.Groups[0].Value.Trim().Replace(".", string.Empty);
          int level = text.IndexOf(s, StringComparison.Ordinal) / 4;
          int result;
          if (int.TryParse(s, out result))
            source.Add(new NumberedListOrder(level, result, index));
        }
        else
          break;
      }
      for (int index = line + 1; index <= lineCount; ++index)
      {
        DocumentLine lineByNumber = editor.Document.GetLineByNumber(index);
        string text = editor.Document.GetText(lineByNumber.Offset, lineByNumber.Length);
        Match match = regex.Match(text);
        if (match.Success)
        {
          string s = match.Groups[0].Value.Trim().Replace(".", string.Empty);
          int level = text.IndexOf(s, StringComparison.Ordinal) / 4;
          int result;
          if (int.TryParse(s, out result))
            source.Add(new NumberedListOrder(level, result, index));
        }
        else
          break;
      }
      if (source.Count > 0)
        source = source.OrderBy<NumberedListOrder, int>((Func<NumberedListOrder, int>) (num => num.LineNumber)).ToList<NumberedListOrder>();
      if (source.Count > 0)
      {
        for (int index1 = 0; index1 < source.Count; ++index1)
        {
          NumberedListOrder numberedListOrder = source[index1];
          int num1 = 0;
          bool flag = levelChange && numberedListOrder.LineNumber == line;
          int num2 = flag ? 1 : numberedListOrder.Number;
          for (int index2 = index1 - 1; index2 >= 0; --index2)
          {
            if (source[index2].Level == numberedListOrder.Level)
            {
              if (num2 != 1 || flag)
              {
                flag = false;
                num2 = source[index2].Number;
                ++num1;
              }
              else
                break;
            }
            else if (source[index2].Level < numberedListOrder.Level)
              break;
          }
          numberedListOrder.Number = num2 + num1;
        }
      }
      if (!source.Any<NumberedListOrder>())
        return;
      editor.BeginChange();
      try
      {
        foreach (NumberedListOrder numberedListOrder in source)
        {
          DocumentLine lineByNumber = editor.Document.GetLineByNumber(numberedListOrder.LineNumber);
          string text = editor.Document.GetText(lineByNumber.Offset, lineByNumber.Length);
          Match match = regex.Match(text);
          if (match.Success)
          {
            string old = match.Groups[0].Value.Replace(".", string.Empty).Trim();
            editor.Document.Remove((ISegment) lineByNumber);
            Utils.ReplaceFirst(ref text, old, numberedListOrder.Number.ToString());
            editor.Document.Insert(lineByNumber.Offset, text);
          }
        }
      }
      catch (Exception ex)
      {
      }
      finally
      {
        editor.EndChange();
      }
    }

    public static bool Replace(this TextEditor editor, Regex find, string replace)
    {
      try
      {
        string input = editor.Text.Substring(editor.SelectionStart, editor.SelectionLength);
        Match match = find.Match(input);
        bool flag = false;
        if (match.Success && match.Index == 0 && match.Length == input.Length)
        {
          string text = match.Result(replace);
          editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, text);
          flag = true;
        }
        if (!editor.Find(find) && !flag)
          Notify.Beep();
        return flag;
      }
      catch (Exception ex)
      {
        Trace.WriteLine(ex.ToString());
        return false;
      }
    }

    public static void ReplaceAll(this TextEditor editor, Regex find, string replace)
    {
      try
      {
        int num = 0;
        editor.BeginChange();
        foreach (Match match in find.Matches(editor.Text))
        {
          string text = match.Result(replace);
          editor.Document.Replace(num + match.Index, match.Length, text);
          num += text.Length - match.Length;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
      finally
      {
        editor.EndChange();
      }
    }

    public static bool ErrorBeep()
    {
      Notify.Beep();
      return false;
    }

    public static void MoveSegmentUp(TextEditor textEditor)
    {
      TextArea textArea = textEditor.TextArea;
      TextDocument document = textEditor.Document;
      Selection selection = textArea.Selection;
      int num;
      DocumentLine previousLine;
      if (selection != null && selection.StartPosition.Line > 1)
      {
        int line1 = selection.StartPosition.Line;
        TextViewPosition textViewPosition = selection.EndPosition;
        int line2 = textViewPosition.Line;
        num = Math.Max(line1, line2);
        TextDocument textDocument = document;
        textViewPosition = selection.StartPosition;
        int line3 = textViewPosition.Line;
        textViewPosition = selection.EndPosition;
        int line4 = textViewPosition.Line;
        int number = Math.Min(line3, line4);
        previousLine = textDocument.GetLineByNumber(number).PreviousLine;
      }
      else
      {
        num = textArea.Caret.Line;
        if (num == 1)
          return;
        previousLine = document.GetLineByNumber(num).PreviousLine;
      }
      textEditor.BeginChange();
      try
      {
        string text = document.GetText(previousLine.Offset, previousLine.TotalLength);
        document.Remove(previousLine.Offset, previousLine.TotalLength);
        int offset = num >= document.LineCount ? document.TextLength : document.GetOffset(num, 0);
        document.Insert(offset, text);
        textArea.Caret.Line = num - 1;
        textArea.Caret.BringCaretToView();
      }
      finally
      {
        textEditor.EndChange();
        textArea.TextView.Redraw(DispatcherPriority.ApplicationIdle);
      }
    }

    public static void MoveSegmentDown(TextEditor textEditor)
    {
      TextArea textArea = textEditor.TextArea;
      TextDocument document = textEditor.Document;
      Selection selection = textArea.Selection;
      int num;
      DocumentLine nextLine;
      if (selection != null && selection.StartPosition.Line > 0 && selection.EndPosition.Line < textEditor.LineCount)
      {
        int line1 = selection.StartPosition.Line;
        TextViewPosition textViewPosition = selection.EndPosition;
        int line2 = textViewPosition.Line;
        num = Math.Min(line1, line2);
        TextDocument textDocument = document;
        textViewPosition = selection.StartPosition;
        int line3 = textViewPosition.Line;
        textViewPosition = selection.EndPosition;
        int line4 = textViewPosition.Line;
        int number = Math.Max(line3, line4);
        nextLine = textDocument.GetLineByNumber(number).NextLine;
      }
      else
      {
        num = textArea.Caret.Line;
        if (num >= textEditor.LineCount)
          return;
        nextLine = document.GetLineByNumber(num).NextLine;
      }
      textEditor.BeginChange();
      try
      {
        string text = document.GetText(nextLine.Offset, nextLine.TotalLength);
        document.Remove(nextLine.Offset, nextLine.TotalLength);
        document.Insert(document.GetOffset(num, 0), text);
        textArea.Caret.Line = num + 1;
        textArea.Caret.BringCaretToView();
      }
      finally
      {
        textEditor.EndChange();
        textArea.TextView.Redraw(DispatcherPriority.ApplicationIdle);
      }
    }

    public static void AddOrRemoveHeading(TextEditor textEditor, string quote)
    {
      Regex regex = new Regex("^#{1,6} .*");
      string selectedText = textEditor.SelectedText;
      TextDocument document = textEditor.Document;
      if (string.IsNullOrEmpty(selectedText) || !selectedText.Contains("\r\n"))
      {
        DocumentLine lineByOffset = textEditor.Document.GetLineByOffset(textEditor.CaretOffset);
        if (lineByOffset == null)
          return;
        string text = textEditor.Document.GetText(lineByOffset.Offset, lineByOffset.Length);
        Match match = regex.Match(text);
        if (match.Success)
        {
          document.Remove((ISegment) lineByOffset);
          if (match.Value.Substring(0, match.Value.IndexOf(" ", StringComparison.Ordinal)) != quote.TrimEnd())
            document.Insert(lineByOffset.Offset, quote + text.Substring(text.IndexOf(" ", StringComparison.Ordinal)).TrimStart());
          else
            document.Insert(lineByOffset.Offset, text.Substring(text.IndexOf(" ", StringComparison.Ordinal)).TrimStart());
        }
        else
          textEditor.Document.Insert(lineByOffset.Offset, quote);
        textEditor.TryFocus();
      }
      else
        EditorUtilities.BatchAddHeading(textEditor, quote);
    }

    private static void BatchAddHeading(TextEditor textEditor, string quote)
    {
      TextDocument document = textEditor.Document;
      Regex regex = new Regex("^#{1,6} .*");
      Selection selection = textEditor.TextArea.Selection;
      TextViewPosition textViewPosition1 = selection.StartPosition;
      int line1 = textViewPosition1.Line;
      textViewPosition1 = selection.EndPosition;
      int line2 = textViewPosition1.Line;
      int start = Math.Min(line1, line2);
      TextViewPosition textViewPosition2 = selection.StartPosition;
      int line3 = textViewPosition2.Line;
      textViewPosition2 = selection.EndPosition;
      int line4 = textViewPosition2.Line;
      int num1 = Math.Max(line3, line4);
      if (start == 0)
        return;
      textEditor.BeginChange();
      try
      {
        foreach (int num2 in Enumerable.Range(start, num1 - start + 1))
        {
          DocumentLine lineByNumber = document.GetLineByNumber(num2);
          int offset = lineByNumber.Offset;
          string text = document.GetText((ISegment) lineByNumber);
          if (!string.IsNullOrWhiteSpace(text))
          {
            if (regex.Match(text).Success)
            {
              document.Remove((ISegment) lineByNumber);
              if (!text.StartsWith(quote))
                document.Insert(offset, quote + text.Substring(text.IndexOf(" ", StringComparison.Ordinal)).TrimStart());
              else
                document.Insert(offset, text.Substring(text.IndexOf(" ", StringComparison.Ordinal)).TrimStart());
            }
            else
              document.Insert(document.GetOffset(num2, 0), quote);
          }
        }
      }
      finally
      {
        textEditor.EndChange();
      }
    }

    private static void AddOrRemoveSelectionQuote(TextEditor editor, string quote)
    {
      Selection selection = editor.TextArea.Selection;
      TextViewPosition textViewPosition1 = selection.StartPosition;
      int line1 = textViewPosition1.Line;
      textViewPosition1 = selection.EndPosition;
      int line2 = textViewPosition1.Line;
      int start = Math.Min(line1, line2);
      if (start == 0)
        return;
      TextViewPosition textViewPosition2 = selection.StartPosition;
      int line3 = textViewPosition2.Line;
      textViewPosition2 = selection.EndPosition;
      int line4 = textViewPosition2.Line;
      int num = Math.Max(line3, line4);
      TextDocument document = editor.Document;
      editor.BeginChange();
      try
      {
        foreach (int number in Enumerable.Range(start, num - start + 1))
        {
          if (number <= document.LineCount)
          {
            DocumentLine lineByNumber = document.GetLineByNumber(number);
            EditorUtilities.HandleLineList(editor, quote, lineByNumber);
          }
        }
      }
      finally
      {
        editor.EndChange();
      }
    }

    public static void ConvertSelectionToList(TextEditor textEditor)
    {
      Selection selection = textEditor.TextArea.Selection;
      TextViewPosition textViewPosition1 = selection.StartPosition;
      int line1 = textViewPosition1.Line;
      textViewPosition1 = selection.EndPosition;
      int line2 = textViewPosition1.Line;
      int start = Math.Min(line1, line2);
      if (start == 0)
        return;
      TextViewPosition textViewPosition2 = selection.StartPosition;
      int line3 = textViewPosition2.Line;
      textViewPosition2 = selection.EndPosition;
      int line4 = textViewPosition2.Line;
      int num1 = Math.Max(line3, line4);
      TextDocument document = textEditor.Document;
      textEditor.BeginChange();
      int num2 = 1;
      try
      {
        foreach (int number in Enumerable.Range(start, num1 - start + 1))
        {
          DocumentLine lineByNumber = document.GetLineByNumber(number);
          string text = document.GetText((ISegment) lineByNumber);
          if (!string.IsNullOrWhiteSpace(text))
          {
            int offset;
            int length;
            if (EditorUtilities.IsListTypeText(text, out offset, out length))
            {
              if (offset < 0)
                offset = 0;
              textEditor.Document.Remove(lineByNumber.Offset + offset, length);
              if (!new Regex("^\\s{0,40}(\\d+)\\. .*").Match(text).Success)
                document.Insert(lineByNumber.Offset, string.Format("{0}. ", (object) num2++));
            }
            else
              document.Insert(lineByNumber.Offset, string.Format("{0}. ", (object) num2++));
          }
        }
      }
      finally
      {
        textEditor.EndChange();
      }
    }

    public static void ScrollToOffset(TextEditor editor, int offset)
    {
      DocumentLine lineByOffset = editor.Document.GetLineByOffset(offset);
      if (lineByOffset == null)
        return;
      editor.ScrollToLine(lineByOffset.LineNumber);
      editor.CaretOffset = offset;
    }

    public static void ScrollToLine(TextEditor editor, int line)
    {
      line = Math.Min(Math.Max(1, editor.Document.LineCount), Math.Max(line, 1));
      editor.ScrollToLine(line);
      int offset = editor.Document.GetOffset(line, 0);
      editor.CaretOffset = offset;
    }

    public static void InsertBlockQuote(TextEditor textEditor)
    {
      TextArea textArea = textEditor.TextArea;
      TextDocument document = textEditor.Document;
      Selection selection = textEditor.TextArea.Selection;
      TextViewPosition textViewPosition = selection.StartPosition;
      int line1 = textViewPosition.Line;
      textViewPosition = selection.EndPosition;
      int line2 = textViewPosition.Line;
      int start = Math.Min(line1, line2);
      if (start == 0)
        start = textArea.Caret.Line;
      int num = Math.Max(selection.StartPosition.Line, selection.EndPosition.Line);
      if (num == 0)
        num = textArea.Caret.Line;
      textEditor.BeginChange();
      try
      {
        foreach (int line3 in Enumerable.Range(start, num - start + 1))
        {
          int offset = document.GetOffset(line3, 0);
          document.Insert(offset, "> ");
        }
      }
      finally
      {
        textEditor.EndChange();
      }
    }

    public static void InsertHyperlink(TextEditor editor, string link)
    {
      string linkText = EditorUtilities.GetLinkText(link);
      if (string.IsNullOrEmpty(linkText))
        return;
      editor.Document.Replace(editor.SelectionStart, editor.SelectionLength, linkText);
    }

    private static string GetLinkText(string link)
    {
      if (string.IsNullOrWhiteSpace(link))
        return string.Empty;
      string[] strArray = link.Split(new char[1]{ '"' }, 2);
      if (strArray.Length == 1)
        return strArray[0].Trim() ?? "";
      return "[" + strArray[1].Trim('"', ' ') + "](" + strArray[0].Trim() + ")";
    }

    public static void ReplaceLink(TextEditor editorEditBox, string original, string revised)
    {
      int caretOffset = editorEditBox.CaretOffset;
      editorEditBox.Text = editorEditBox.Text.Replace(original, revised);
      editorEditBox.CaretOffset = caretOffset < editorEditBox.Text.Length ? caretOffset : editorEditBox.Text.Length;
    }

    public static void OutIndentList(TextEditor textEditor)
    {
      TextArea textArea = textEditor.TextArea;
      TextDocument document = textEditor.Document;
      Selection selection = textEditor.TextArea.Selection;
      TextViewPosition textViewPosition = selection.StartPosition;
      int line1 = textViewPosition.Line;
      textViewPosition = selection.EndPosition;
      int line2 = textViewPosition.Line;
      int start = Math.Min(line1, line2);
      if (start == 0)
        start = textArea.Caret.Line;
      int num = Math.Max(selection.StartPosition.Line, selection.EndPosition.Line);
      if (num == 0)
        num = textArea.Caret.Line;
      textEditor.BeginChange();
      try
      {
        foreach (int number in Enumerable.Range(start, num - start + 1))
        {
          DocumentLine lineByNumber = document.GetLineByNumber(number);
          int offset = lineByNumber.Offset;
          string text = document.GetText((ISegment) lineByNumber);
          if (text.StartsWith("    "))
            text = text.Substring(4);
          document.Remove((ISegment) lineByNumber);
          document.Insert(offset, text);
        }
      }
      finally
      {
        textEditor.EndChange();
      }
    }

    public static void IndentList(TextEditor textEditor)
    {
      TextArea textArea = textEditor.TextArea;
      TextDocument document = textEditor.Document;
      Selection selection = textEditor.TextArea.Selection;
      TextViewPosition textViewPosition = selection.StartPosition;
      int line1 = textViewPosition.Line;
      textViewPosition = selection.EndPosition;
      int line2 = textViewPosition.Line;
      int start = Math.Min(line1, line2);
      if (start == 0)
        start = textArea.Caret.Line;
      int num = Math.Max(selection.StartPosition.Line, selection.EndPosition.Line);
      if (num == 0)
        num = textArea.Caret.Line;
      textEditor.BeginChange();
      try
      {
        foreach (int line3 in Enumerable.Range(start, num - start + 1))
        {
          int offset = document.GetOffset(line3, 0);
          document.Insert(offset, "    ");
        }
      }
      finally
      {
        textEditor.EndChange();
      }
    }
  }
}
