// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SelectionBackgroundConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Views.Time;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class SelectionBackgroundConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && value is SelectionMode selectionMode)
      {
        switch (selectionMode)
        {
          case SelectionMode.Start:
            return (object) new CornerRadius(15.0, 0.0, 0.0, 15.0);
          case SelectionMode.Middle:
            return (object) new CornerRadius(0.0, 0.0, 0.0, 0.0);
          case SelectionMode.End:
            return (object) new CornerRadius(0.0, 15.0, 15.0, 0.0);
          case SelectionMode.Full:
            return (object) new CornerRadius(15.0, 15.0, 15.0, 15.0);
        }
      }
      return (object) new CornerRadius(15.0, 15.0, 15.0, 15.0);
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
