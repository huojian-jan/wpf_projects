// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.InnerRadiusConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class InnerRadiusConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      string str = parameter as string;
      double num1 = value as double? ?? 8.0;
      double num2;
      switch (str)
      {
        case null:
          return (object) (20.0 - num1);
        case "1":
          num2 = 20.0 - num1;
          break;
        default:
          num2 = num1 / 2.0;
          break;
      }
      return (object) new Size(num2, num2);
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
