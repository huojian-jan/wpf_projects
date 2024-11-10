// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineAvatarVisibleConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineAvatarVisibleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 4)
        return (object) Visibility.Collapsed;
      string str = values[3] as string;
      if (string.IsNullOrEmpty(str) || str == "-1")
        return (object) Visibility.Collapsed;
      double? nullable1 = ((int) (values[0] as bool?) ?? 1) != 0 ? values[1] as double? : values[2] as double?;
      if (nullable1.HasValue)
      {
        double? nullable2 = nullable1;
        double num = 70.0;
        if (!(nullable2.GetValueOrDefault() < num & nullable2.HasValue))
          return (object) Visibility.Visible;
      }
      return (object) Visibility.Collapsed;
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
