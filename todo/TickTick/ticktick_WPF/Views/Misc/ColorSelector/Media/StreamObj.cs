// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ColorSelector.Media.StreamObj
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

#nullable disable
namespace ticktick_WPF.Views.Misc.ColorSelector.Media
{
  public class StreamObj
  {
    public static string ToString(Stream stream)
    {
      try
      {
        return new StreamReader(stream).ReadToEnd();
      }
      catch
      {
        return "";
      }
    }

    public static ImageSource ToImageSource(Stream stream)
    {
      try
      {
        BitmapImage imageSource = new BitmapImage();
        imageSource.BeginInit();
        imageSource.CacheOption = BitmapCacheOption.OnLoad;
        imageSource.StreamSource = stream;
        imageSource.EndInit();
        return (ImageSource) imageSource;
      }
      catch
      {
        return (ImageSource) null;
      }
    }
  }
}
