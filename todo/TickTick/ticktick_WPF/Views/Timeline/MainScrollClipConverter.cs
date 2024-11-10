// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.MainScrollClipConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class MainScrollClipConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length < 5 || !(values[0] is double num1) || !(values[1] is double height) || !(values[2] is bool flag1) || !(values[3] is double num2) || !(values[4] is bool flag2))
        return (object) null;
      if (((values.Length != 6 ? 0 : (!(values[5] is bool flag3) ? 0 : 1)) & (flag3 ? 1 : 0)) != 0)
        flag2 = false;
      Point location = new Point(flag1 ? num2 : 0.0, 0.0);
      double num3 = flag1 ? num2 : 0.0;
      Size size = new Size(Math.Max(0.0, num1 - num3 - (flag2 ? 215.0 : 0.0)), height);
      return (object) new RectangleGeometry(new Rect(location, size));
    }

    public object[] ConvertBack(
      object value,
      Type[] targetTypes,
      object parameter,
      CultureInfo culture)
    {
      throw new NotImplementedException();
    }
  }
}
