// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.RepeatEndEnableConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class RepeatEndEnableConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 2)
        return (object) true;
      return values[0] is string repeatFrom && values[1] is string repeatFlag ? (object) (RepeatUtils.GetRepeatType(repeatFrom, repeatFlag) != RepeatFromType.Custom) : (object) true;
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
