// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskLevelMarginConverter
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
  public class TaskLevelMarginConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      int result;
      if (values.Length != 2 || !(values[0] is int val2) || !(parameter is string s) || !int.TryParse(s, out result) || val2 < 0)
        return (object) new Thickness(0.0, 0.0, 0.0, 0.0);
      bool valueOrDefault = (values[1] as bool?).GetValueOrDefault();
      return (object) new Thickness((double) (Math.Min(4, val2) * result + (valueOrDefault ? 10 : 0)), 0.0, 0.0, 0.0);
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
