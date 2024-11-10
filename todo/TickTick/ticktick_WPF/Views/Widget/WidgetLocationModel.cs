// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WidgetLocationModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Drawing;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class WidgetLocationModel
  {
    public double Left { get; set; }

    public double Top { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public double ScreenX { get; set; }

    public double ScreenY { get; set; }

    public double ScreenWidth { get; set; }

    public HideType HideType { get; set; }

    public void SetScreen(Rectangle rect, double factorX, double factorY)
    {
      this.ScreenWidth = (double) rect.Width * factorX;
      this.ScreenX = (double) rect.X * factorX;
      this.ScreenY = (double) rect.Y * factorY;
    }

    public double HideLeftIn => this.ScreenX - this.Width;

    public double HideRightIn => this.ScreenX + this.ScreenWidth;

    public double HideTopIn => this.ScreenY - this.Height;

    public double LeftOut => this.ScreenX;

    public double RightOut => this.ScreenX + this.ScreenWidth - this.Width;

    public double TopOut => this.ScreenY;
  }
}
