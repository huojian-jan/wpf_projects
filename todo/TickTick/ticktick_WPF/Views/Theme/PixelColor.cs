// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Theme.PixelColor
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Theme
{
  public struct PixelColor
  {
    public byte Blue;
    public byte Green;
    public byte Red;
    public byte Alpha;

    public bool IsTooLight() => (int) this.Blue + (int) this.Green + (int) this.Red > 480;

    public SolidColorBrush ToColorBrush()
    {
      string str1 = Convert.ToString(this.Red, 16).ToUpper();
      if (str1.Length == 1)
        str1 = "0" + str1;
      string str2 = Convert.ToString(this.Green, 16).ToUpper();
      if (str2.Length == 1)
        str2 = "0" + str2;
      string str3 = Convert.ToString(this.Blue, 16).ToUpper();
      if (str3.Length == 1)
        str3 = "0" + str3;
      return ThemeUtil.GetColorInString("#" + str1 + str2 + str3);
    }
  }
}
