﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ShowHabitIndicatorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class ShowHabitIndicatorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      return values != null && values.Length == 2 && values[0] is HabitModel habitModel && values[1] is HabitCheckInModel habitCheckInModel && habitModel.Type.ToLower() != "boolean" && habitCheckInModel.Value > 0.0 ? (object) Visibility.Visible : (object) Visibility.Collapsed;
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