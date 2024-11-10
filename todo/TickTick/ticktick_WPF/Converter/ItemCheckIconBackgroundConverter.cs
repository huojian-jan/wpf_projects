// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ItemCheckIconBackgroundConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class ItemCheckIconBackgroundConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 5 || !(values[0] is DisplayType displayType) || !(values[1] is int num1) || !(values[2] is int num2) || !(values[3] is bool flag))
        return (object) Brushes.Transparent;
      if (!flag || displayType == DisplayType.Note || displayType == DisplayType.Event || num2 != 0)
        return (object) Brushes.Transparent;
      ResourceDictionary dict = values.Length == 5 ? values[4] as ResourceDictionary : (ResourceDictionary) null;
      if (displayType == DisplayType.Task || displayType == DisplayType.CheckItem || displayType == DisplayType.Agenda)
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
      return (object) ThemeUtil.GetColor("BaseColorOpacity100", dict);
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
