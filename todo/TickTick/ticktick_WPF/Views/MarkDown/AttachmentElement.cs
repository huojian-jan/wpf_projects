// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.AttachmentElement
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class AttachmentElement : InlineObjectElement
  {
    private readonly double _height;
    private readonly double _width;
    private readonly double _baselineExtra;

    public AttachmentElement(
      int length,
      UIElement element,
      double width,
      double height,
      double baselineExtra,
      TextAlignment alignment)
      : base(length, element)
    {
      this._width = width;
      this._height = height;
      this.TextAlignment = alignment;
      this._baselineExtra = baselineExtra;
    }

    public override TextRun CreateTextRun(
      int startVisualColumn,
      ITextRunConstructionContext context)
    {
      return context != null ? (TextRun) new ImageInlineRun(1, (System.Windows.Media.TextFormatting.TextRunProperties) this.TextRunProperties, this.Element, this._width, this._height, this._baselineExtra) : (TextRun) null;
    }
  }
}
