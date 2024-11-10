// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.KanbanItemShadowConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class KanbanItemShadowConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 2 || !(values[0] is bool flag1) || !(values[1] is bool flag2))
        return (object) new Thickness(6.0);
      if (!flag1 && !flag2)
        return (object) new Thickness(6.0, 0.0, 6.0, 0.0);
      if (flag1 && !flag2)
        return (object) new Thickness(6.0, 6.0, 6.0, 0.0);
      return !flag1 ? (object) new Thickness(6.0, 0.0, 6.0, 6.0) : (object) new Thickness(6.0);
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
