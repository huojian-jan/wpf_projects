// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SelectionHorizonConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class SelectionHorizonConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && value is SelectionMode selectionMode)
      {
        if (selectionMode == SelectionMode.Start)
          return (object) HorizontalAlignment.Right;
        if (selectionMode == SelectionMode.End)
          return (object) HorizontalAlignment.Left;
      }
      return (object) Visibility.Collapsed;
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
