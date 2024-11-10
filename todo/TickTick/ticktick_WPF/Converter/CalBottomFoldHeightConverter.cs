// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CalBottomFoldHeightConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Views.Calendar;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class CalBottomFoldHeightConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is int num ? (object) (CalendarGeoHelper.MinHeight * (num < 24 ? 2.0 : 1.0)) : (object) null;
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
