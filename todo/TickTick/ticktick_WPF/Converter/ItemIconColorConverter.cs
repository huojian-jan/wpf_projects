// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ItemIconColorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class ItemIconColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 5 || !(values[0] is DisplayType displayType) || !(values[1] is int num1) || !(values[2] is int num2))
        return (object) ThemeUtil.GetColor("BaseColorOpacity40");
      FrameworkElement context = values.Length == 5 ? values[4] as FrameworkElement : (FrameworkElement) null;
      if (num2 == 0 && (displayType == DisplayType.Task || displayType == DisplayType.CheckItem || displayType == DisplayType.Agenda))
      {
        switch (num1)
        {
          case 1:
            return (object) ThemeUtil.GetColor("PriorityLowColor");
          case 3:
            return (object) ThemeUtil.GetColor("PriorityMiddleColor");
          case 5:
            return (object) ThemeUtil.GetColor("PriorityHighColor");
        }
      }
      return num2 != 0 ? (object) ThemeUtil.GetColor((values[3] as bool?).GetValueOrDefault() ? "BaseColorOpacity40" : "BaseColorOpacity20", context) : (object) ThemeUtil.GetColor("BaseColorOpacity40", context);
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
