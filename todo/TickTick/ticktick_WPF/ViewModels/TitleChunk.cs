// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TitleChunk
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public struct TitleChunk
  {
    public string Text;
    public bool IsNum;
    public int RealLength;
    public double Num;

    public TitleChunk(string text, bool isNum, int realLength)
    {
      this.Text = text;
      this.IsNum = isNum;
      this.RealLength = realLength;
      this.Num = 0.0;
      double result;
      if (!isNum || !double.TryParse(text, out result))
        return;
      this.Num = result;
    }
  }
}
