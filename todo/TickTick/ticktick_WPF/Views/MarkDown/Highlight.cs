// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.MarkDown.Highlight
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.MarkDown
{
  public class Highlight
  {
    public string Name { get; set; }

    public SolidColorBrush Background { get; set; }

    public SolidColorBrush Foreground { get; set; }

    public string FontWeight { get; set; }

    public string FontStyle { get; set; }

    public bool Underline { get; set; }

    public bool Strikethrough { get; set; }

    public string FontType { get; set; }
  }
}
