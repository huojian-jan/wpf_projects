// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineOutlineAvatarVisibleConverter
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
  public class TimelineOutlineAvatarVisibleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 2 && values[0] is double num)
      {
        if (num < 70.0)
          return (object) Visibility.Collapsed;
        if (values[1] == null || values[1] is string str && (str == "" || str == "-1"))
          return (object) Visibility.Collapsed;
      }
      return (object) Visibility.Visible;
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
