// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.LogicVisibleConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class LogicVisibleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value != null && value is List<LogicType> source && parameter is string str && source.Select<LogicType, string>((Func<LogicType, string>) (type => type.ToString())).ToList<string>().Contains(str) ? (object) Visibility.Visible : (object) Visibility.Collapsed;
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
