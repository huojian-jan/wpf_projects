// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.LinkTextReadOnlyConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class LinkTextReadOnlyConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      return values.Length == 2 && values[0] is bool flag1 && values[1] is bool flag2 ? (object) (bool) (!flag1 ? 1 : (!flag2 ? 1 : 0)) : (object) false;
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
