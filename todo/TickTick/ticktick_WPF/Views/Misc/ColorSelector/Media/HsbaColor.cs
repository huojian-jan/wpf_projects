// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ColorSelector.Media.HsbaColor
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Misc.ColorSelector.Media
{
  public class HsbaColor
  {
    private double h;
    private double s;
    private double b;
    private double a;

    public double H
    {
      get => this.h;
      set => this.h = value < 0.0 ? 0.0 : (value >= 360.0 ? 0.0 : value);
    }

    public double S
    {
      get => this.s;
      set => this.s = value < 0.0 ? 0.0 : (value > 1.0 ? 1.0 : value);
    }

    public double B
    {
      get => this.b;
      set => this.b = value < 0.0 ? 0.0 : (value > 1.0 ? 1.0 : value);
    }

    public double A
    {
      get => this.a;
      set => this.a = value < 0.0 ? 0.0 : (value > 1.0 ? 1.0 : value);
    }

    public int Y => this.RgbaColor.Y;

    public HsbaColor()
    {
      this.H = 0.0;
      this.S = 0.0;
      this.B = 1.0;
      this.A = 1.0;
    }

    public HsbaColor(double h, double s, double b, double a = 1.0)
    {
      this.H = h;
      this.S = s;
      this.B = b;
      this.A = a;
    }

    public HsbaColor(int r, int g, int b, int a = 255)
    {
      HsbaColor hsba = Utility.RgbaToHsba(new RgbaColor(r, g, b, a));
      this.H = hsba.H;
      this.S = hsba.S;
      this.B = hsba.B;
      this.A = hsba.A;
    }

    public HsbaColor(Brush brush)
    {
      HsbaColor hsba = Utility.RgbaToHsba(new RgbaColor(brush));
      this.H = hsba.H;
      this.S = hsba.S;
      this.B = hsba.B;
      this.A = hsba.A;
    }

    public HsbaColor(string hexColor)
    {
      HsbaColor hsba = Utility.RgbaToHsba(new RgbaColor(hexColor));
      this.H = hsba.H;
      this.S = hsba.S;
      this.B = hsba.B;
      this.A = hsba.A;
    }

    public Color Color => this.RgbaColor.Color;

    public Color OpaqueColor => this.RgbaColor.OpaqueColor;

    public SolidColorBrush SolidColorBrush => this.RgbaColor.SolidColorBrush;

    public SolidColorBrush OpaqueSolidColorBrush => this.RgbaColor.OpaqueSolidColorBrush;

    public string HexString => this.Color.ToString();

    public string RgbaString => this.RgbaColor.RgbaString;

    public RgbaColor RgbaColor => Utility.HsbaToRgba(this);
  }
}
