// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.HabitProgressToolTipConverter
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
  public class HabitProgressToolTipConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value != null && value is HabitCheckInModel habitCheckInModel ? (object) (Math.Min((int) (habitCheckInModel.Value / habitCheckInModel.Goal * 100.0), 100).ToString() + "%") : (object) "0%";
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
