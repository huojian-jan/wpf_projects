// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.ImageInlineRun
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Rendering;
using System.Windows;
using System.Windows.Media.TextFormatting;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public sealed class ImageInlineRun : InlineObjectRun
  {
    private readonly double _height;
    private readonly double _width;
    private UIElement _element;
    private readonly double _baselineExtra;

    public ImageInlineRun(
      int length,
      TextRunProperties properties,
      UIElement element,
      double width,
      double height,
      double baselineExtra)
      : base(length, properties, element)
    {
      this._width = width;
      this._height = height;
      this._element = element;
      this._baselineExtra = baselineExtra;
    }

    public override TextEmbeddedObjectMetrics Format(double remainingParagraphWidth)
    {
      return new TextEmbeddedObjectMetrics(this._width, this._height, 14.0 + this._baselineExtra);
    }
  }
}
