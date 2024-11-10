// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.Colorizer.DeleteLineColorizer
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.MarkDown.Colorizer
{
  public class DeleteLineColorizer : DocumentColorizingTransformer
  {
    public static TextDecorationCollection Strikethrough;
    public static TextDecorationCollection HintStrikethrough;
    private static readonly TextDecoration TextDecoration = new TextDecoration()
    {
      Location = TextDecorationLocation.Strikethrough
    };
    private static readonly TextDecoration HintTextDecoration;

    static DeleteLineColorizer()
    {
      Pen pen1 = new Pen((Brush) ThemeUtil.GetColor("BaseColorOpacity20_40"), 2.0);
      DeleteLineColorizer.TextDecoration.Pen = pen1;
      DeleteLineColorizer.Strikethrough = new TextDecorationCollection()
      {
        DeleteLineColorizer.TextDecoration
      };
      DeleteLineColorizer.HintTextDecoration = new TextDecoration()
      {
        Location = TextDecorationLocation.Strikethrough
      };
      Pen pen2 = new Pen((Brush) ThemeUtil.GetColor("BaseColorOpacity10_20"), 2.0);
      DeleteLineColorizer.HintTextDecoration.Pen = pen2;
      DeleteLineColorizer.HintStrikethrough = new TextDecorationCollection()
      {
        DeleteLineColorizer.HintTextDecoration
      };
    }

    public static void OnThemeChanged()
    {
      DeleteLineColorizer.TextDecoration.Pen = new Pen((Brush) ThemeUtil.GetColor("BaseColorOpacity20_40"), 2.0);
      DeleteLineColorizer.HintTextDecoration.Pen = new Pen((Brush) ThemeUtil.GetColor("BaseColorOpacity10_20"), 2.0);
    }

    protected override void ColorizeLine(DocumentLine line)
    {
      foreach (VisualLineElement currentElement in (IEnumerable<VisualLineElement>) this.CurrentElements)
        currentElement.TextRunProperties.SetTextDecorations(DeleteLineColorizer.Strikethrough);
    }
  }
}
