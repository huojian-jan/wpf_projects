﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.LineX1Converter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class LineX1Converter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value != null && (double) value > 20.0 ? (object) 7 : (object) 10;
    }

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      return (object) null;
    }
  }
}