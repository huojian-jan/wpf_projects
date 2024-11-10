// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.BoolOrVisibilityConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class BoolOrVisibilityConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      bool flag1 = false;
      List<int> intList = (List<int>) null;
      if (parameter != null)
        intList = ((IEnumerable<string>) parameter.ToString().Split(',')).Select<string, int>(new Func<string, int>(int.Parse)).ToList<int>();
      for (int index = 0; index < values.Length; ++index)
      {
        // ISSUE: explicit non-virtual call
        if (values[index] is bool flag2 && (flag2 && (intList == null || !intList.Contains(index)) || !flag2 && intList != null && __nonvirtual (intList.Contains(index))))
        {
          flag1 = true;
          break;
        }
      }
      return (object) (Visibility) (flag1 ? 0 : 2);
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
