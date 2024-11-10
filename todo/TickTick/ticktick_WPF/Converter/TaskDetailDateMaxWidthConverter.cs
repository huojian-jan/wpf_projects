// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskDetailDateMaxWidthConverter
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
  public class TaskDetailDateMaxWidthConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 2 || !(values[0] is double num) || !(values[1] is Visibility visibility))
        return (object) 250;
      if (num < 50.0)
        return (object) 250;
      return visibility == Visibility.Visible ? (object) (num - 50.0) : (object) (num - 30.0);
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
