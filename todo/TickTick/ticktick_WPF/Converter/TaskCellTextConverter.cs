﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskCellTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class TaskCellTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 3 || !(values[0] is double num1) || !(values[1] is double num2) || !(values[2] is string str))
        return (object) "";
      int length = Math.Max(4, (int) (num1 / 15.4) * (int) num2 / 5);
      if (str.Length > length)
        str = str.Substring(0, length);
      return (object) str;
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