// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.FilterEditVisibleConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class FilterEditVisibleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null)
      {
        switch ((FilterMode) value)
        {
          case FilterMode.Normal:
            return (object) (Visibility) (parameter == null || !(parameter.ToString() == "normal") ? 2 : 0);
          case FilterMode.Advanced:
            return (object) (Visibility) (parameter == null || !(parameter.ToString() == "normal") ? 0 : 2);
        }
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
