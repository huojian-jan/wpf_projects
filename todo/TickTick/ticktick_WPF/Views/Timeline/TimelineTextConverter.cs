// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 2 || !(values[0] is string str))
        return (object) string.Empty;
      if (values[1] is double d && !double.IsInfinity(d))
      {
        if (d < 24.0)
          return (object) string.Empty;
        int length = (int) d / 5;
        if (str.Length > length && length > 0)
          str = str.Substring(0, length);
      }
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
