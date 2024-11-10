// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.MultipleBoolVisibleConverter
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
  public class MultipleBoolVisibleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && ((IEnumerable<object>) values).Any<object>() && parameter != null && parameter is string str)
      {
        char[] chArray = new char[1]{ ',' };
        string[] strArray = str.Split(chArray);
        if (values.Length == strArray.Length)
        {
          bool flag1 = true;
          for (int index = 0; index < strArray.Length; ++index)
          {
            if (values[index] is bool flag2 && bool.Parse(strArray[index]) != flag2)
            {
              flag1 = false;
              break;
            }
          }
          return (object) (Visibility) (flag1 ? 0 : 2);
        }
      }
      return (object) Visibility.Collapsed;
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
