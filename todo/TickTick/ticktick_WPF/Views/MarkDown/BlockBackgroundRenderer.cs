// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.BlockBackgroundRenderer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using CommonMark.Syntax;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class BlockBackgroundRenderer : IBackgroundRenderer
  {
    private MarkDownEditor _editor;
    private Block _abstractSyntaxTree;

    public BlockBackgroundRenderer(MarkDownEditor editor) => this._editor = editor;

    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
      Block abstractSyntaxTree = this._abstractSyntaxTree;
      if (abstractSyntaxTree == null)
        return;
      SolidColorBrush solidColorBrush1 = (SolidColorBrush) null;
      SolidColorBrush solidColorBrush2 = (SolidColorBrush) null;
      SolidColorBrush solidColorBrush3 = (SolidColorBrush) null;
      int num1 = this._editor.EditBox.CaretOffset;
      if (this._editor.EditBox.SelectionLength > 0)
        num1 = this._editor.EditBox.SelectionStart == this._editor.EditBox.CaretOffset ? this._editor.EditBox.SelectionStart + this._editor.EditBox.SelectionLength : this._editor.EditBox.SelectionStart;
      int num2 = 0;
      foreach (Block block in AbstractSyntaxTree.EnumerateBlocks(abstractSyntaxTree.FirstChild).ToList<Block>())
      {
        if (block.SourcePosition + block.SourceLength <= this._editor.Text.Length)
        {
          if (block.Tag == BlockTag.BlockQuote)
          {
            DocumentLine lineByOffset1 = textView.Document.GetLineByOffset(block.SourcePosition);
            DocumentLine lineByOffset2 = textView.Document.GetLineByOffset(block.SourcePosition + block.SourceLength);
            double? nullable = new double?();
            double num3 = 0.0;
            for (int lineNumber = lineByOffset1.LineNumber; lineNumber <= lineByOffset2.LineNumber; ++lineNumber)
            {
              VisualLine visualLine = textView.GetVisualLine(lineNumber);
              if (visualLine.Elements.Count >= 1 && visualLine.Elements[0] is QuoteItemElement)
              {
                Rect rect = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, visualLine, 0, 1000).FirstOrDefault<Rect>();
                if (!(rect == new Rect()))
                {
                  if (!nullable.HasValue)
                    nullable = new double?(rect.Top);
                  num3 += visualLine.Height;
                }
              }
              else if (num3 > 0.0)
              {
                drawingContext.DrawRectangle((Brush) ThemeUtil.GetColor("MarkDownQuoteColor", (FrameworkElement) this._editor), (Pen) null, new Rect(1.0, nullable.GetValueOrDefault(), 2.0, num3 - 6.0));
                num3 = 0.0;
                nullable = new double?();
              }
            }
            if (num3 > 0.0)
              drawingContext.DrawRectangle((Brush) ThemeUtil.GetColor("MarkDownQuoteColor", (FrameworkElement) this._editor), (Pen) null, new Rect(1.0, nullable.GetValueOrDefault(), 2.0, num3 - 6.0));
          }
          else if (block.Tag == BlockTag.FencedCode)
          {
            DocumentLine lineByOffset3 = textView.Document.GetLineByOffset(block.SourcePosition);
            VisualLine visualLine1 = textView.GetVisualLine(lineByOffset3.LineNumber);
            Rect rect1 = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, visualLine1, block.SourcePosition, block.SourcePosition).FirstOrDefault<Rect>();
            DocumentLine lineByOffset4 = textView.Document.GetLineByOffset(block.SourcePosition + block.SourceLength);
            VisualLine visualLine2 = textView.GetVisualLine(lineByOffset4.LineNumber);
            Rect rect2 = BackgroundGeometryBuilder.GetRectsFromVisualSegment(textView, visualLine2, block.SourcePosition, block.SourcePosition).FirstOrDefault<Rect>();
            RectInfo rectInfo = new RectInfo()
            {
              Top = rect1.Top,
              Bottom = rect2.Top - 6.0
            };
            if (rectInfo.Top >= 0.0 && rectInfo.Bottom >= 0.0 && rectInfo.Bottom > rectInfo.Top)
            {
              solidColorBrush1 = solidColorBrush1 ?? ThemeUtil.GetColor("CodeBackground", (FrameworkElement) this._editor);
              solidColorBrush2 = solidColorBrush2 ?? ThemeUtil.GetColor("MarkDownCodeDivider", (FrameworkElement) this._editor);
              drawingContext.DrawRoundedRectangle((Brush) solidColorBrush1, new Pen((Brush) solidColorBrush2, 0.0), new Rect(0.0, rectInfo.Top, textView.ActualWidth, rectInfo.Bottom - rectInfo.Top), 2.0, 2.0);
            }
          }
          else if (block.Tag == BlockTag.Paragraph || block.Tag == BlockTag.SetextHeading || block.Tag == BlockTag.AtxHeading || block.Tag == BlockTag.IndentedCode)
          {
            foreach (Inline enumerateInline in AbstractSyntaxTree.EnumerateInlines(block.InlineContent))
            {
              if (enumerateInline.Tag == InlineTag.Code)
              {
                TextSegment textSegment = new TextSegment()
                {
                  StartOffset = enumerateInline.SourcePosition,
                  Length = enumerateInline.SourceLength
                };
                foreach (Rect rectangle in BackgroundGeometryBuilder.GetRectsForSegment(textView, (ISegment) textSegment))
                {
                  if (rectangle.Width > 3.0)
                  {
                    solidColorBrush1 = solidColorBrush1 ?? ThemeUtil.GetColor("CodeBackground", (FrameworkElement) this._editor);
                    drawingContext.DrawRoundedRectangle((Brush) solidColorBrush1, (Pen) null, rectangle, 2.0, 2.0);
                  }
                }
              }
              if (enumerateInline.Tag == InlineTag.HighLight)
              {
                TextSegment textSegment1;
                if (num1 < enumerateInline.SourcePosition || num1 > enumerateInline.SourcePosition + enumerateInline.SourceLength)
                {
                  textSegment1 = new TextSegment();
                  textSegment1.StartOffset = enumerateInline.SourcePosition + 2;
                  textSegment1.Length = enumerateInline.SourceLength - 2;
                }
                else
                  textSegment1 = new TextSegment()
                  {
                    StartOffset = enumerateInline.SourcePosition,
                    Length = enumerateInline.SourceLength
                  };
                TextSegment textSegment2 = textSegment1;
                foreach (Rect rectangle in BackgroundGeometryBuilder.GetRectsForSegment(textView, (ISegment) textSegment2))
                {
                  if (rectangle.Width > 3.0)
                  {
                    ++num2;
                    solidColorBrush3 = solidColorBrush3 ?? ThemeUtil.GetColor("MarkdownHighlightColor", (FrameworkElement) this._editor);
                    drawingContext.DrawRoundedRectangle((Brush) solidColorBrush3, (Pen) null, rectangle, 2.0, 2.0);
                  }
                }
              }
            }
          }
        }
      }
    }

    public void UpdateAbstractSyntaxTree(Block ast) => this._abstractSyntaxTree = ast;
  }
}
