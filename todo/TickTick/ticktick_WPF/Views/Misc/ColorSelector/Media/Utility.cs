// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ColorSelector.Media.Utility
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

#nullable disable
namespace ticktick_WPF.Views.Misc.ColorSelector.Media
{
  internal class Utility
  {
    internal static HsbaColor RgbaToHsba(RgbaColor rgba)
    {
      int[] array = new int[3]{ rgba.R, rgba.G, rgba.B };
      Array.Sort<int>(array);
      int num1 = array[2];
      int num2 = array[0];
      double b = (double) num1 / (double) byte.MaxValue;
      double s = num1 == 0 ? 0.0 : (double) (num1 - num2) / (double) num1;
      double h = 0.0;
      if (rgba.R != rgba.G || rgba.R != rgba.B)
      {
        if (num1 == rgba.R && rgba.G >= rgba.B)
          h = (double) (rgba.G - rgba.B) * 60.0 / (double) (num1 - num2) + 0.0;
        else if (num1 == rgba.R && rgba.G < rgba.B)
          h = (double) (rgba.G - rgba.B) * 60.0 / (double) (num1 - num2) + 360.0;
        else if (num1 == rgba.G)
          h = (double) (rgba.B - rgba.R) * 60.0 / (double) (num1 - num2) + 120.0;
        else if (num1 == rgba.B)
          h = (double) (rgba.R - rgba.G) * 60.0 / (double) (num1 - num2) + 240.0;
      }
      return new HsbaColor(h, s, b, (double) rgba.A / (double) byte.MaxValue);
    }

    internal static RgbaColor HsbaToRgba(HsbaColor hsba)
    {
      double num1 = 0.0;
      double num2 = 0.0;
      double num3 = 0.0;
      int num4 = (int) (hsba.H / 60.0 % 6.0);
      double num5 = hsba.H / 60.0 - (double) num4;
      double num6 = hsba.B * (1.0 - hsba.S);
      double num7 = hsba.B * (1.0 - num5 * hsba.S);
      double num8 = hsba.B * (1.0 - (1.0 - num5) * hsba.S);
      switch (num4)
      {
        case 0:
          num1 = hsba.B;
          num2 = num8;
          num3 = num6;
          break;
        case 1:
          num1 = num7;
          num2 = hsba.B;
          num3 = num6;
          break;
        case 2:
          num1 = num6;
          num2 = hsba.B;
          num3 = num8;
          break;
        case 3:
          num1 = num6;
          num2 = num7;
          num3 = hsba.B;
          break;
        case 4:
          num1 = num8;
          num2 = num6;
          num3 = hsba.B;
          break;
        case 5:
          num1 = hsba.B;
          num2 = num6;
          num3 = num7;
          break;
      }
      return new RgbaColor((int) ((double) byte.MaxValue * num1), (int) ((double) byte.MaxValue * num2), (int) ((double) byte.MaxValue * num3), (int) ((double) byte.MaxValue * hsba.A));
    }

    internal static BitmapSource LoadImg(string path)
    {
      try
      {
        BitmapImage bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.StreamSource = (Stream) new MemoryStream(File.ReadAllBytes(path));
        bitmapImage.EndInit();
        return (BitmapSource) bitmapImage;
      }
      catch
      {
        return (BitmapSource) null;
      }
    }

    internal static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
    {
      PixelFormat format = PixelFormat.Format32bppArgb;
      Bitmap bitmap = new Bitmap(bitmapImage.PixelWidth, bitmapImage.PixelHeight, format);
      BitmapData bitmapdata = bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, format);
      bitmapImage.CopyPixels(Int32Rect.Empty, bitmapdata.Scan0, bitmapdata.Height * bitmapdata.Stride, bitmapdata.Stride);
      bitmap.UnlockBits(bitmapdata);
      return bitmap;
    }

    internal static BitmapSource BitmapToBitmapSource(Bitmap bitmap)
    {
      return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }

    internal static int GetBrightness(int r, int g, int b)
    {
      return (int) ((0.2126 * (double) r + 447.0 / 625.0 * (double) g + 0.0722 * (double) b) / 2.55);
    }
  }
}
