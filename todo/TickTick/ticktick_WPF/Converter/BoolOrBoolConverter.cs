// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.BoolOrBoolConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class BoolOrBoolConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      List<int> intList = (List<int>) null;
      if (parameter != null)
        intList = ((IEnumerable<string>) parameter.ToString().Split(',')).Select<string, int>(new Func<string, int>(int.Parse)).ToList<int>();
      if (values.Length != 0)
      {
        for (int index = 0; index < values.Length; ++index)
        {
          // ISSUE: explicit non-virtual call
          if (values[index] is bool flag && (flag && (intList == null || !intList.Contains(index)) || !flag && intList != null && __nonvirtual (intList.Contains(index))))
            return (object) true;
        }
      }
      return (object) false;
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
