// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.Colorizer.CodeColorizer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.Colorizer
{
  public class CodeColorizer : DocumentColorizingTransformer
  {
    private readonly IHighlightingDefinition _definition;
    private readonly MarkDownEditor _editor;
    private readonly bool _isFixedHighlighter;
    private IHighlighter _highlighter;
    private bool _isInHighlightingGroup;
    private DocumentLine _lastColorizedLine;
    private int _lineNumberBeingColorized;
    private TextView _textView;

    public CodeColorizer(MarkDownEditor editor, IHighlightingDefinition definition)
    {
      this._editor = editor;
      this._definition = definition ?? throw new ArgumentNullException(nameof (definition));
    }

    private void textView_DocumentChanged(object sender, EventArgs e)
    {
      TextView textView = (TextView) sender;
      this.DeregisterServices(textView);
      this.RegisterServices(textView);
    }

    protected virtual void DeregisterServices(TextView textView)
    {
      if (this._highlighter == null)
        return;
      if (this._isInHighlightingGroup)
      {
        this._highlighter.EndHighlighting();
        this._isInHighlightingGroup = false;
      }
      this._highlighter.HighlightingStateChanged -= new HighlightingStateChangedEventHandler(this.OnHighlightStateChanged);
      if (textView.Services.GetService(typeof (IHighlighter)) == this._highlighter)
        textView.Services.RemoveService(typeof (IHighlighter));
      if (this._isFixedHighlighter)
        return;
      this._highlighter?.Dispose();
      this._highlighter = (IHighlighter) null;
    }

    protected virtual void RegisterServices(TextView textView)
    {
      if (textView.Document == null)
        return;
      if (!this._isFixedHighlighter)
        this._highlighter = textView.Document != null ? this.CreateHighlighter(textView, textView.Document) : (IHighlighter) null;
      if (this._highlighter == null || this._highlighter.Document != textView.Document)
        return;
      if (textView.Services.GetService(typeof (IHighlighter)) == null)
        textView.Services.AddService(typeof (IHighlighter), (object) this._highlighter);
      this._highlighter.HighlightingStateChanged += new HighlightingStateChangedEventHandler(this.OnHighlightStateChanged);
    }

    protected virtual IHighlighter CreateHighlighter(TextView textView, TextDocument document)
    {
      return this._definition != null ? (IHighlighter) new DocumentHighlighter(document, this._definition) : throw new NotSupportedException("Cannot create a highlighter because no IHighlightingDefinition was specified, and the CreateHighlighter() method was not overridden.");
    }

    protected override void OnAddToTextView(TextView textView)
    {
      if (this._textView != null)
        throw new InvalidOperationException("Cannot use a HighlightingColorizer instance in multiple text views. Please create a separate instance for each text view.");
      base.OnAddToTextView(textView);
      this._textView = textView;
      textView.DocumentChanged += new EventHandler(this.textView_DocumentChanged);
      textView.VisualLineConstructionStarting += new EventHandler<VisualLineConstructionStartEventArgs>(this.textView_VisualLineConstructionStarting);
      textView.VisualLinesChanged += new EventHandler(this.textView_VisualLinesChanged);
      this.RegisterServices(textView);
    }

    protected override void OnRemoveFromTextView(TextView textView)
    {
      this.DeregisterServices(textView);
      textView.DocumentChanged -= new EventHandler(this.textView_DocumentChanged);
      textView.VisualLineConstructionStarting -= new EventHandler<VisualLineConstructionStartEventArgs>(this.textView_VisualLineConstructionStarting);
      textView.VisualLinesChanged -= new EventHandler(this.textView_VisualLinesChanged);
      base.OnRemoveFromTextView(textView);
      this._textView = (TextView) null;
    }

    private void textView_VisualLineConstructionStarting(
      object sender,
      VisualLineConstructionStartEventArgs e)
    {
      if (this._highlighter == null)
        return;
      this._lineNumberBeingColorized = e.FirstLineInView.LineNumber - 1;
      if (!this._isInHighlightingGroup)
      {
        this._highlighter.BeginHighlighting();
        this._isInHighlightingGroup = true;
      }
      this._highlighter.UpdateHighlightingState(this._lineNumberBeingColorized);
      this._lineNumberBeingColorized = 0;
    }

    private void textView_VisualLinesChanged(object sender, EventArgs e)
    {
      if (this._highlighter == null || !this._isInHighlightingGroup)
        return;
      this._highlighter.EndHighlighting();
      this._isInHighlightingGroup = false;
    }

    protected override void Colorize(ITextRunConstructionContext context)
    {
      this._lastColorizedLine = (DocumentLine) null;
      base.Colorize(context);
      if (this._lastColorizedLine != context.VisualLine.LastDocumentLine && this._highlighter != null)
      {
        this._lineNumberBeingColorized = context.VisualLine.LastDocumentLine.LineNumber;
        this._highlighter.UpdateHighlightingState(this._lineNumberBeingColorized);
        this._lineNumberBeingColorized = 0;
      }
      this._lastColorizedLine = (DocumentLine) null;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
      if (this._highlighter != null)
      {
        this._lineNumberBeingColorized = line.LineNumber;
        HighlightedLine highlightedLine = this._highlighter.HighlightLine(this._lineNumberBeingColorized);
        this._lineNumberBeingColorized = 0;
        if (this.IsCurrentLineValid(line))
        {
          foreach (HighlightedSection section1 in (IEnumerable<HighlightedSection>) highlightedLine.Sections)
          {
            HighlightedSection section = section1;
            if (!CodeColorizer.IsEmptyColor(section.Color))
              this.ChangeLinePart(section.Offset, section.Offset + section.Length, (Action<VisualLineElement>) (visualLineElement => this.ApplyColorToElement(visualLineElement, section.Color)));
          }
        }
      }
      this._lastColorizedLine = line;
    }

    private bool IsCurrentLineValid(DocumentLine line)
    {
      if (this._editor.CodeDict != null && this._editor.CodeDict.Any<KeyValuePair<string, List<Point>>>())
      {
        foreach (string key in this._editor.CodeDict.Keys)
        {
          if (string.Equals(key, this._definition.Name, StringComparison.CurrentCultureIgnoreCase))
          {
            foreach (Point point in this._editor.CodeDict[key])
            {
              int offset = line.Offset;
              int endOffset = line.EndOffset;
              if (point.X <= (double) offset && point.Y >= (double) endOffset)
                return true;
            }
          }
        }
      }
      return false;
    }

    private static bool IsEmptyColor(HighlightingColor color)
    {
      if (color == null)
        return true;
      if (color.Background == null && color.Foreground == null && !color.FontStyle.HasValue && !color.FontWeight.HasValue)
      {
        bool? nullable = color.Underline;
        if (!nullable.HasValue)
        {
          nullable = color.Strikethrough;
          return !nullable.HasValue;
        }
      }
      return false;
    }

    protected virtual void ApplyColorToElement(VisualLineElement element, HighlightingColor color)
    {
      CodeColorizer.ApplyColorToElement(element, color, this.CurrentContext);
    }

    private static void ApplyColorToElement(
      VisualLineElement element,
      HighlightingColor color,
      ITextRunConstructionContext context)
    {
      if (color.Foreground != null)
      {
        Brush brush = color.Foreground.GetBrush(context);
        if (brush != null)
          element.TextRunProperties.SetForegroundBrush(brush);
      }
      if (color.Background != null)
      {
        Brush brush = color.Background.GetBrush(context);
        if (brush != null)
          element.BackgroundBrush = brush;
      }
      FontStyle? fontStyle = color.FontStyle;
      FontWeight? fontWeight;
      if (!fontStyle.HasValue)
      {
        fontWeight = color.FontWeight;
        if (!fontWeight.HasValue && color.FontFamily == null)
          goto label_9;
      }
      Typeface typeface1 = element.TextRunProperties.Typeface;
      VisualLineElementTextRunProperties textRunProperties1 = element.TextRunProperties;
      FontFamily fontFamily = color.FontFamily ?? typeface1.FontFamily;
      fontStyle = color.FontStyle;
      FontStyle style = fontStyle ?? typeface1.Style;
      fontWeight = color.FontWeight;
      FontWeight weight = fontWeight ?? typeface1.Weight;
      FontStretch stretch = typeface1.Stretch;
      Typeface typeface2 = new Typeface(fontFamily, style, weight, stretch);
      textRunProperties1.SetTypeface(typeface2);
label_9:
      bool? nullable = color.Underline;
      if (nullable.GetValueOrDefault())
        element.TextRunProperties.SetTextDecorations(TextDecorations.Underline);
      nullable = color.Strikethrough;
      if (nullable.GetValueOrDefault())
        element.TextRunProperties.SetTextDecorations(TextDecorations.Strikethrough);
      int? fontSize = color.FontSize;
      if (!fontSize.HasValue)
        return;
      VisualLineElementTextRunProperties textRunProperties2 = element.TextRunProperties;
      fontSize = color.FontSize;
      double num = (double) fontSize.Value;
      textRunProperties2.SetFontRenderingEmSize(num);
    }

    private void OnHighlightStateChanged(int fromLineNumber, int toLineNumber)
    {
      if (this._lineNumberBeingColorized != 0 && toLineNumber <= this._lineNumberBeingColorized)
        return;
      if (fromLineNumber == toLineNumber)
      {
        this._textView.Redraw((ISegment) this._textView.Document.GetLineByNumber(fromLineNumber));
      }
      else
      {
        DocumentLine lineByNumber1 = this._textView.Document.GetLineByNumber(fromLineNumber);
        DocumentLine lineByNumber2 = this._textView.Document.GetLineByNumber(toLineNumber);
        int offset = lineByNumber1.Offset;
        this._textView.Redraw(offset, lineByNumber2.EndOffset - offset);
      }
    }
  }
}
