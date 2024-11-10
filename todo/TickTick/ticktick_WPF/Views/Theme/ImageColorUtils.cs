// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.ImageColorUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public class ImageColorUtils
  {
    public static PixelColor[,] GetPixels(BitmapSource source)
    {
      if (source.Format != PixelFormats.Bgra32)
        source = (BitmapSource) new FormatConvertedBitmap(source, PixelFormats.Bgra32, (BitmapPalette) null, 0.0);
      PixelColor[,] pixels = new PixelColor[source.PixelHeight, source.PixelWidth];
      int stride = source.PixelWidth * source.Format.BitsPerPixel / 8;
      GCHandle gcHandle = GCHandle.Alloc((object) pixels, GCHandleType.Pinned);
      source.CopyPixels(new Int32Rect(0, 0, source.PixelWidth, source.PixelHeight), gcHandle.AddrOfPinnedObject(), pixels.GetLength(0) * pixels.GetLength(1) * 4, stride);
      gcHandle.Free();
      return pixels;
    }
  }
}
