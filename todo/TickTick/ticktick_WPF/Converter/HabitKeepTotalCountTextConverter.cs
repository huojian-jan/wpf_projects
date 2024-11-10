// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.HabitKeepTotalCountTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class HabitKeepTotalCountTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 3 || !(values[0] is bool flag))
        return (object) string.Empty;
      if (!flag)
        return (object) (values[2] as string);
      int valueOrDefault = (values[1] as int?).GetValueOrDefault();
      return (object) (valueOrDefault.ToString() + " " + Utils.GetString(valueOrDefault > 1 ? "PublicDays" : "PublicDay"));
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
