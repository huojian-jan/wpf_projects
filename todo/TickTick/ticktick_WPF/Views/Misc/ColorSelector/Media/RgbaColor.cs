// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ColorSelector.Media.RgbaColor
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Misc.ColorSelector.Media
{
  public class RgbaColor
  {
    private int r;
    private int g;
    private int b;
    private int a;

    public int R
    {
      get => this.r;
      set => this.r = value < 0 ? 0 : (value > (int) byte.MaxValue ? (int) byte.MaxValue : value);
    }

    public int G
    {
      get => this.g;
      set => this.g = value < 0 ? 0 : (value > (int) byte.MaxValue ? (int) byte.MaxValue : value);
    }

    public int B
    {
      get => this.b;
      set => this.b = value < 0 ? 0 : (value > (int) byte.MaxValue ? (int) byte.MaxValue : value);
    }

    public int A
    {
      get => this.a;
      set => this.a = value < 0 ? 0 : (value > (int) byte.MaxValue ? (int) byte.MaxValue : value);
    }

    public int Y => Utility.GetBrightness(this.R, this.G, this.B);

    public RgbaColor()
    {
      this.R = (int) byte.MaxValue;
      this.G = (int) byte.MaxValue;
      this.B = (int) byte.MaxValue;
      this.A = (int) byte.MaxValue;
    }

    public RgbaColor(int r, int g, int b, int a = 255)
    {
      this.R = r;
      this.G = g;
      this.B = b;
      this.A = a;
    }

    public RgbaColor(Brush brush)
    {
      if (brush != null)
      {
        this.R = (int) ((SolidColorBrush) brush).Color.R;
        this.G = (int) ((SolidColorBrush) brush).Color.G;
        this.B = (int) ((SolidColorBrush) brush).Color.B;
        this.A = (int) ((SolidColorBrush) brush).Color.A;
      }
      else
        this.R = this.G = this.B = this.A = (int) byte.MaxValue;
    }

    public RgbaColor(double h, double s, double b, double a = 1.0)
    {
      RgbaColor rgba = Utility.HsbaToRgba(new HsbaColor(h, s, b, a));
      this.R = rgba.R;
      this.G = rgba.G;
      this.B = rgba.B;
      this.A = rgba.A;
    }

    public RgbaColor(string hexColor)
    {
      try
      {
        Color color = !(hexColor.Substring(0, 1) == "#") ? (Color) ColorConverter.ConvertFromString("#" + hexColor) : (Color) ColorConverter.ConvertFromString(hexColor);
        this.R = (int) color.R;
        this.G = (int) color.G;
        this.B = (int) color.B;
        this.A = (int) color.A;
      }
      catch
      {
        this.R = 0;
        this.G = 0;
        this.B = 0;
        this.A = (int) byte.MaxValue;
      }
    }

    public Color Color
    {
      get => Color.FromArgb((byte) this.A, (byte) this.R, (byte) this.G, (byte) this.B);
    }

    public Color OpaqueColor
    {
      get => Color.FromArgb(byte.MaxValue, (byte) this.R, (byte) this.G, (byte) this.B);
    }

    public SolidColorBrush SolidColorBrush => new SolidColorBrush(this.Color);

    public SolidColorBrush OpaqueSolidColorBrush => new SolidColorBrush(this.OpaqueColor);

    public string HexString => this.Color.ToString();

    public string RgbaString
    {
      get
      {
        return this.R.ToString() + "," + this.G.ToString() + "," + this.B.ToString() + "," + this.A.ToString();
      }
    }

    public HsbaColor HsbaColor => Utility.RgbaToHsba(this);
  }
}
