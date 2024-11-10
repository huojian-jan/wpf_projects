// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.HabitProgressTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class HabitProgressTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 2 || !(values[0] is HabitModel habitModel))
        return (object) string.Empty;
      return values[1] is HabitCheckInModel habitCheckInModel ? (object) (habitCheckInModel.Value.ToString() + "/" + habitModel.Goal.ToString() + " " + habitModel.Unit) : (object) (0.ToString() + "/" + habitModel.Goal.ToString() + " " + habitModel.Unit);
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
