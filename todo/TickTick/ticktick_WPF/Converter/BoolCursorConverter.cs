// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.BoolCursorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class BoolCursorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is bool flag))
        return (object) Cursors.Arrow;
      return !flag ? (object) Cursors.No : (object) Cursors.Hand;
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
