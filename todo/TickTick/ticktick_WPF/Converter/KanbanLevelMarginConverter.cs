// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.KanbanLevelMarginConverter
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
  public class KanbanLevelMarginConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      int result;
      return value is int val2 && parameter is string s && int.TryParse(s, out result) && val2 > 0 ? (object) new Thickness((double) (16 + Math.Min(4, val2) * result), 4.0, 4.0, 4.0) : (object) new Thickness(0.0, 0.0, 0.0, 0.0);
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
