﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.MaxButtonConverter
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
  public class MaxButtonConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return parameter.ToString() == "Max" ? ((WindowState) value != WindowState.Maximized ? (object) "Visible" : (object) "Collapsed") : ((WindowState) value != WindowState.Maximized ? (object) "Collapsed" : (object) "Visible");
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