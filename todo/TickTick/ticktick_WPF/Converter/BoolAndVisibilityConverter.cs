﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.BoolAndVisibilityConverter
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
  public class BoolAndVisibilityConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && ((IEnumerable<object>) values).Any<object>())
      {
        List<int> intList = (List<int>) null;
        if (parameter != null)
          intList = ((IEnumerable<string>) parameter.ToString().Split(',')).Select<string, int>(new Func<string, int>(int.Parse)).ToList<int>();
        for (int index = 0; index < values.Length; ++index)
        {
          if (values[index] is bool flag)
          {
            if (intList != null && intList.Contains(index))
              flag = !flag;
            if (!flag)
              return (object) Visibility.Collapsed;
          }
        }
      }
      return (object) Visibility.Visible;
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