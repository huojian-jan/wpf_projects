// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.AbstractSyntaxTree
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using CommonMark;
using CommonMark.Syntax;
using System;
using System.Collections.Generic;
using System.IO;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public static class AbstractSyntaxTree
  {
    private static readonly CommonMarkSettings CommonMarkSettings;
    public static readonly Dictionary<BlockTag, Func<Theme, Highlight>> BlockHighlighter = new Dictionary<BlockTag, Func<Theme, Highlight>>()
    {
      {
        BlockTag.AtxHeading,
        (Func<Theme, Highlight>) (t => t.HighlightHeading)
      },
      {
        BlockTag.SetextHeading,
        (Func<Theme, Highlight>) (t => t.HighlightHeading)
      },
      {
        BlockTag.BlockQuote,
        (Func<Theme, Highlight>) (t => t.HighlightBlockQuote)
      },
      {
        BlockTag.FencedCode,
        (Func<Theme, Highlight>) (t => t.HighlightBlockCode)
      }
    };
    public static readonly Dictionary<InlineTag, Func<Theme, Highlight>> InlineHighlighter = new Dictionary<InlineTag, Func<Theme, Highlight>>()
    {
      {
        InlineTag.Code,
        (Func<Theme, Highlight>) (t => t.HighlightInlineCode)
      },
      {
        InlineTag.Emphasis,
        (Func<Theme, Highlight>) (t => t.HighlightEmphasis)
      },
      {
        InlineTag.Strikethrough,
        (Func<Theme, Highlight>) (t => t.HighlightStrikethrough)
      },
      {
        InlineTag.Strong,
        (Func<Theme, Highlight>) (t => t.HighlightStrongEmphasis)
      },
      {
        InlineTag.RawHtml,
        (Func<Theme, Highlight>) (t => t.HighlightBlockCode)
      },
      {
        InlineTag.HighLight,
        (Func<Theme, Highlight>) (t => t.HighlightHighlight)
      },
      {
        InlineTag.UnderLine,
        (Func<Theme, Highlight>) (t => t.HighlightUnderLine)
      }
    };

    static AbstractSyntaxTree()
    {
      AbstractSyntaxTree.CommonMarkSettings = CommonMarkSettings.Default.Clone();
      AbstractSyntaxTree.CommonMarkSettings.TrackSourcePosition = true;
      AbstractSyntaxTree.CommonMarkSettings.AdditionalFeatures = CommonMarkAdditionalFeatures.StrikethroughTilde;
    }

    public static Block GenerateAbstractSyntaxTree(string text)
    {
      using (StringReader source = new StringReader(AbstractSyntaxTree.Normalize(text)))
      {
        Block document = CommonMarkConverter.ProcessStage1((TextReader) source, AbstractSyntaxTree.CommonMarkSettings);
        CommonMarkConverter.ProcessStage2(document, AbstractSyntaxTree.CommonMarkSettings);
        return document;
      }
    }

    private static string Normalize(string value) => value ?? string.Empty;

    public static IEnumerable<Block> EnumerateSpanningBlocks(
      Block ast,
      int startOffset,
      int endOffset)
    {
      return AbstractSyntaxTree.EnumerateBlocks(ast.FirstChild).Where<Block>((Func<Block, bool>) (b => b.SourcePosition + b.SourceLength > startOffset)).TakeWhile<Block>((Func<Block, bool>) (b => b.SourcePosition < endOffset));
    }

    public static IEnumerable<Block> EnumerateBlocks(Block block)
    {
      if (block != null)
      {
        Stack<Block> stack = new Stack<Block>();
        stack.Push(block);
        while (stack.Any<Block>())
        {
          Block next = stack.Pop();
          yield return next;
          if (next.NextSibling != null)
            stack.Push(next.NextSibling);
          if (next.FirstChild != null)
            stack.Push(next.FirstChild);
          next = (Block) null;
        }
      }
    }

    public static IEnumerable<Inline> EnumerateInlines(Inline inline)
    {
      if (inline != null)
      {
        Stack<Inline> stack = new Stack<Inline>();
        stack.Push(inline);
        while (stack.Any<Inline>())
        {
          Inline next = stack.Pop();
          yield return next;
          if (next.NextSibling != null)
            stack.Push(next.NextSibling);
          if (next.FirstChild != null)
            stack.Push(next.FirstChild);
          next = (Inline) null;
        }
      }
    }

    public static bool PositionSafeForSmartLink(Block ast, int start, int length)
    {
      if (ast == null)
        return true;
      int end = start + length;
      BlockTag[] source = new BlockTag[4]
      {
        BlockTag.FencedCode,
        BlockTag.HtmlBlock,
        BlockTag.IndentedCode,
        BlockTag.ReferenceDefinition
      };
      InlineTag[] inlineTags = new InlineTag[4]
      {
        InlineTag.Code,
        InlineTag.Link,
        InlineTag.RawHtml,
        InlineTag.Image
      };
      BlockTag lastBlockTag = BlockTag.Document;
      foreach (Block enumerateBlock in AbstractSyntaxTree.EnumerateBlocks(ast.FirstChild))
      {
        Block block = enumerateBlock;
        if (block.SourcePosition + block.SourceLength < start)
        {
          lastBlockTag = block.Tag;
        }
        else
        {
          if (block.SourcePosition >= end)
            return ((IEnumerable<BlockTag>) source).All<BlockTag>((Func<BlockTag, bool>) (tag => tag != lastBlockTag));
          return !((IEnumerable<BlockTag>) source).Any<BlockTag>((Func<BlockTag, bool>) (tag => tag == block.Tag)) && !AbstractSyntaxTree.EnumerateInlines(block.InlineContent).TakeWhile<Inline>((Func<Inline, bool>) (il => il.SourcePosition < end)).Where<Inline>((Func<Inline, bool>) (il => il.SourcePosition + il.SourceLength > start)).Any<Inline>((Func<Inline, bool>) (il => ((IEnumerable<InlineTag>) inlineTags).Any<InlineTag>((Func<InlineTag, bool>) (tag => tag == il.Tag))));
        }
      }
      return true;
    }
  }
}
