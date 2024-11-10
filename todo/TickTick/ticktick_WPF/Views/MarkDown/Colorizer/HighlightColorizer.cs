// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.Colorizer.HighlightColorizer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using CommonMark.Syntax;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.Colorizer
{
  public class HighlightColorizer : DocumentColorizingTransformer
  {
    private readonly MarkDownEditor _editor;
    private Block _abstractSyntaxTree;
    private Theme _theme;
    private static readonly Regex CheckItemRegex = new Regex("^\\s{0,40}- \\[ \\] .*");
    private static readonly Regex CheckedItemRegex = new Regex("^\\s{0,40}- \\[[x|X]\\] .*");

    public HighlightColorizer(MarkDownEditor editor) => this._editor = editor;

    protected override void ColorizeLine(DocumentLine line)
    {
      Block abstractSyntaxTree = this._abstractSyntaxTree;
      if (abstractSyntaxTree == null)
        return;
      if (this._theme == null)
        this._theme = new Theme((FrameworkElement) this._editor);
      Theme theme = this._theme;
      int offset = line.Offset;
      int end = line.EndOffset;
      int leadingSpaces = this.CurrentContext.GetText(offset, end - offset).Text.TakeWhile<char>(new Func<char, bool>(char.IsWhiteSpace)).Count<char>();
      foreach (Block enumerateSpanningBlock in AbstractSyntaxTree.EnumerateSpanningBlocks(abstractSyntaxTree, offset, end))
      {
        double magnify = double.NaN;
        if (enumerateSpanningBlock.Tag == BlockTag.List || enumerateSpanningBlock.Tag == BlockTag.IndentedCode)
        {
          if (enumerateSpanningBlock.SourcePosition < 0 || enumerateSpanningBlock.SourcePosition + enumerateSpanningBlock.SourceLength > this._editor.Text.Length)
            break;
          string str1 = this._editor.Text.Substring(enumerateSpanningBlock.SourcePosition, enumerateSpanningBlock.SourceLength);
          string[] strArray = str1.Split(new string[3]
          {
            Environment.NewLine,
            "\n",
            "\r"
          }, StringSplitOptions.None);
          int startIndex = 0;
          foreach (string input in strArray)
          {
            if (HighlightColorizer.CheckedItemRegex.Match(input).Success)
            {
              string str2 = input.Substring(input.IndexOf("]", StringComparison.Ordinal) + 2);
              int num1 = str1.IndexOf(input, startIndex, StringComparison.Ordinal);
              int num2 = input.IndexOf("]", StringComparison.Ordinal) + 2;
              int num3 = num1 + num2;
              this.ApplyLinePart(this._theme.HighlightCheckedItem, enumerateSpanningBlock.SourcePosition + num3, str2.Length, offset, end, leadingSpaces, magnify);
              if (num1 >= 0)
                startIndex = num1 + input.Length;
            }
            if (HighlightColorizer.CheckItemRegex.Match(input).Success)
            {
              int num = str1.IndexOf(input, startIndex, StringComparison.Ordinal);
              if (num >= 0)
                startIndex = num + input.Length;
            }
          }
        }
        Func<Theme, Highlight> highlighter;
        if (AbstractSyntaxTree.BlockHighlighter.TryGetValue(enumerateSpanningBlock.Tag, out highlighter))
        {
          int sourceLength = enumerateSpanningBlock.Tag == BlockTag.ListItem ? Math.Min(enumerateSpanningBlock.SourceLength, enumerateSpanningBlock.ListData.Padding) : enumerateSpanningBlock.SourceLength;
          if (enumerateSpanningBlock.Tag == BlockTag.BlockQuote && enumerateSpanningBlock.FirstChild?.InlineContent != null)
          {
            Inline inline = (Inline) null;
            using (IEnumerator<Inline> enumerator = AbstractSyntaxTree.EnumerateInlines(enumerateSpanningBlock.FirstChild.InlineContent).GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                Inline current = enumerator.Current;
                if (current.SourcePosition < offset || current.SourcePosition > end)
                {
                  inline = current;
                }
                else
                {
                  if (current.Tag == InlineTag.String && (inline == null || inline.SourcePosition + inline.SourceLength + 2 == current.SourcePosition))
                    this.ApplyLinePart(highlighter(theme), current.SourcePosition - 2, current.SourceLength + 2, offset, end, 0, magnify);
                  inline = current;
                }
              }
              break;
            }
          }
          else
          {
            if (enumerateSpanningBlock.Heading.Level >= (byte) 1)
            {
              string text = this.CurrentContext.GetText(offset, end - offset).Text;
              if (text.Replace("#", string.Empty).TrimEnd().Length > 0)
              {
                if (enumerateSpanningBlock.Heading.Level == (byte) 1)
                {
                  if (!text.StartsWith("#"))
                  {
                    AbstractSyntaxTree.BlockHighlighter.TryGetValue(BlockTag.Document, out highlighter);
                    magnify = 1.0;
                  }
                  else
                    magnify = theme.Header1Height;
                }
                if (enumerateSpanningBlock.Heading.Level == (byte) 2)
                {
                  if (!text.StartsWith("##") && !text.StartsWith("=="))
                  {
                    AbstractSyntaxTree.BlockHighlighter.TryGetValue(BlockTag.Document, out highlighter);
                    magnify = 1.0;
                  }
                  else
                    magnify = theme.Header2Height;
                }
                if (enumerateSpanningBlock.Heading.Level == (byte) 3)
                  magnify = theme.Header3Height;
                if (enumerateSpanningBlock.Heading.Level == (byte) 4)
                  magnify = theme.Header4Height;
                if (enumerateSpanningBlock.Heading.Level == (byte) 5)
                  magnify = theme.Header5Height;
                if (enumerateSpanningBlock.Heading.Level == (byte) 6)
                  magnify = theme.Header6Height;
              }
              else
              {
                highlighter = (Func<Theme, Highlight>) (t => t.HighlightNormal);
                magnify = 1.0;
              }
            }
            if (enumerateSpanningBlock.Tag != BlockTag.ListItem && enumerateSpanningBlock.Tag != BlockTag.List && highlighter != null)
              this.ApplyLinePart(highlighter(theme), enumerateSpanningBlock.SourcePosition, sourceLength, offset, end, leadingSpaces, magnify);
          }
        }
        foreach (Inline inline in AbstractSyntaxTree.EnumerateInlines(enumerateSpanningBlock.InlineContent).TakeWhile<Inline>((Func<Inline, bool>) (il => il.SourcePosition < end)))
        {
          AbstractSyntaxTree.InlineHighlighter.TryGetValue(inline.Tag, out highlighter);
          int sourcePosition = inline.SourcePosition;
          int sourceLength = inline.SourceLength;
          if (inline.Tag == InlineTag.Code)
            magnify = theme.CodeHeight;
          if (inline.Tag == InlineTag.Strong || inline.Tag == InlineTag.Emphasis || inline.Tag == InlineTag.Strikethrough || inline.Tag == InlineTag.UnderLine || inline.Tag == InlineTag.Code || inline.Tag == InlineTag.HighLight)
          {
            int flagLength = 2;
            if (inline.Tag == InlineTag.Emphasis || inline.Tag == InlineTag.UnderLine || inline.Tag == InlineTag.Link || inline.Tag == InlineTag.Placeholder || inline.Tag == InlineTag.Code)
              flagLength = 1;
            if (inline.Tag == InlineTag.HighLight)
              this.HighlightFlagInlineExtra(flagLength, theme, sourcePosition, offset, end, leadingSpaces, highlighter, sourceLength);
            else
              this.HighlightFlagInline(flagLength, theme, sourcePosition, offset, end, leadingSpaces, highlighter, sourceLength);
          }
          else if (highlighter != null)
            this.ApplyLinePart(highlighter(theme), sourcePosition, sourceLength, offset, end, leadingSpaces, magnify);
        }
      }
    }

    private void HighlightFlagInlineExtra(
      int flagLength,
      Theme theme,
      int position,
      int start,
      int end,
      int leadingSpaces,
      Func<Theme, Highlight> highlighter,
      int length)
    {
      this.ApplyLinePart(theme.HighlightFlagExtra, position, flagLength, start, end, leadingSpaces, double.NaN);
      if (highlighter != null)
        this.ApplyLinePart(highlighter(theme), position + flagLength, length - flagLength * 2, start, end, leadingSpaces, double.NaN);
      this.ApplyLinePart(theme.HighlightFlagExtra, position + length - flagLength, flagLength, start, end, leadingSpaces, double.NaN);
    }

    private void HighlightFlagInline(
      int flagLength,
      Theme theme,
      int position,
      int start,
      int end,
      int leadingSpaces,
      Func<Theme, Highlight> highlighter,
      int length)
    {
      this.ApplyLinePart(theme.HighlightFlag, position, flagLength, start, end, leadingSpaces, double.NaN);
      if (highlighter != null)
        this.ApplyLinePart(highlighter(theme), position + flagLength, length - flagLength * 2, start, end, leadingSpaces, double.NaN);
      this.ApplyLinePart(theme.HighlightFlag, position + length - flagLength, flagLength, start, end, leadingSpaces, double.NaN);
    }

    private void ApplyLinePart(
      Highlight highlight,
      int sourceStart,
      int sourceLength,
      int lineStart,
      int lineEnd,
      int leadingSpaces,
      double magnify)
    {
      int startOffset = Math.Max(sourceStart, lineStart + leadingSpaces);
      int endOffset = Math.Min(lineEnd, sourceStart + sourceLength);
      if (startOffset >= endOffset)
        return;
      this.ChangeLinePart(startOffset, endOffset, (Action<VisualLineElement>) (element => HighlightColorizer.ApplyHighlight(element, highlight, magnify)));
    }

    private static void ApplyHighlight(
      VisualLineElement element,
      Highlight highlight,
      double magnify)
    {
      VisualLineElementTextRunProperties textRunProperties = element.TextRunProperties;
      SolidColorBrush foreground = highlight.Foreground;
      if (foreground != null)
        textRunProperties.SetForegroundBrush((Brush) foreground);
      if (!highlight.Name.Contains("Block"))
      {
        SolidColorBrush background = highlight.Background;
        if (background != null)
          textRunProperties.SetBackgroundBrush((Brush) background);
      }
      if (!string.IsNullOrWhiteSpace(highlight.FontWeight) || !string.IsNullOrWhiteSpace(highlight.FontStyle))
      {
        Typeface typeface1 = textRunProperties.Typeface;
        FontWeight weight = HighlightColorizer.ConvertFontWeight(highlight.FontWeight) ?? typeface1.Weight;
        FontStyle style = HighlightColorizer.ConvertFontStyle(highlight.FontStyle) ?? typeface1.Style;
        Typeface typeface2 = new Typeface(typeface1.FontFamily, style, weight, typeface1.Stretch);
        textRunProperties.SetTypeface(typeface2);
      }
      if (!string.IsNullOrEmpty(highlight.FontType))
      {
        Typeface typeface = new Typeface(highlight.FontType);
        textRunProperties.SetTypeface(typeface);
      }
      if (highlight.Underline)
        textRunProperties.SetTextDecorations(TextDecorations.Underline);
      if (highlight.Strikethrough)
        textRunProperties.SetTextDecorations(TextDecorations.Strikethrough);
      if (double.IsNaN(magnify))
        return;
      textRunProperties.SetFontRenderingEmSize(textRunProperties.FontRenderingEmSize * magnify);
    }

    private static FontWeight? ConvertFontWeight(string fontWeight)
    {
      if (string.IsNullOrWhiteSpace(fontWeight))
        return new FontWeight?();
      try
      {
        return new FontWeight?((FontWeight) new FontWeightConverter().ConvertFromString(fontWeight));
      }
      catch (FormatException ex)
      {
        return new FontWeight?();
      }
      catch (NotSupportedException ex)
      {
        return new FontWeight?();
      }
    }

    private static FontStyle? ConvertFontStyle(string fontStyle)
    {
      if (string.IsNullOrWhiteSpace(fontStyle))
        return new FontStyle?();
      try
      {
        return new FontStyle?((FontStyle) new FontStyleConverter().ConvertFromString(fontStyle));
      }
      catch (FormatException ex)
      {
        return new FontStyle?();
      }
      catch (NotSupportedException ex)
      {
        return new FontStyle?();
      }
    }

    public void UpdateAbstractSyntaxTree(Block ast) => this._abstractSyntaxTree = ast;

    public void OnThemeChanged(Theme theme) => this._theme = theme;
  }
}
