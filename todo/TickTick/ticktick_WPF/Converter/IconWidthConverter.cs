// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.IconWidthConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shapes;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class IconWidthConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value != null ? (object) ((Application.Current?.FindResource((object) value.ToString()) is Path resource ? new double?(resource.Width) : new double?()) ?? 16.0) : (object) 16;
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
