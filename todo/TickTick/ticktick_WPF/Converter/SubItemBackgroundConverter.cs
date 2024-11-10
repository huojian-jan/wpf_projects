// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SubItemBackgroundConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class SubItemBackgroundConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 6)
      {
        if (values[5] is bool flag1 && !flag1)
          return (object) Brushes.Transparent;
        if (values[0] is bool flag3 && values[1] is bool flag4 && values[2] is bool flag5)
        {
          if (flag3)
            return (object) ThemeUtil.GetColor("ItemSelectedColor");
          if (flag4 | flag5)
            return ((!(values[4] is bool flag2) ? 0 : 1) & (flag2 ? 1 : 0)) != 0 ? (object) Brushes.Transparent : (object) ThemeUtil.GetColor("BaseColorOpacity5");
        }
      }
      return (object) Brushes.Transparent;
    }

    public object[] ConvertBack(
      object value,
      Type[] targetTypes,
      object parameter,
      CultureInfo culture)
    {
      return (object[]) null;
    }
  }
}
