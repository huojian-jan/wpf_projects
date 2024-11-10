// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ItemIconOpacityConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class ItemIconOpacityConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 3 || !(values[0] is DisplayType displayType) || !(values[1] is int num1) || !(values[2] is int num2))
        return (object) 0.36;
      if (displayType != DisplayType.Task && displayType != DisplayType.CheckItem && displayType != DisplayType.Agenda)
        return (object) 0.36;
      return num2 != 0 ? (object) 0.18 : (object) (num1 != 0 ? 1.0 : 0.36);
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
