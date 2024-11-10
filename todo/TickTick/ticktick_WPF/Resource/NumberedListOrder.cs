// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Resource.NumberedListOrder
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Resource
{
  public class NumberedListOrder
  {
    public NumberedListOrder(int level, int number, int lineNumber)
    {
      this.Level = level;
      this.Number = number;
      this.LineNumber = lineNumber;
    }

    public int Level { get; set; }

    public int Number { get; set; }

    public int LineNumber { get; set; }
  }
}
