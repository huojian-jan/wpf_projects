// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.DateParser.HighlightIndexModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Documents;

#nullable disable
namespace ticktick_WPF.Util.DateParser
{
  public class HighlightIndexModel
  {
    public int start { get; set; }

    public int end { get; set; }

    public int length { get; set; }

    public string text { get; set; }

    public TextPointer endPointer { get; set; }

    public TextPointer startPointer { get; set; }

    public int type { get; set; }
  }
}
