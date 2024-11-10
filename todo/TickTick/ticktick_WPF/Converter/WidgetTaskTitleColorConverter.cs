// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.WidgetTaskTitleColorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class WidgetTaskTitleColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 3)
      {
        int? nullable1 = values[0] as int?;
        string str = values[1] as string;
        if (values[2] is ResourceDictionary dict)
        {
          if (nullable1.HasValue)
          {
            int? nullable2 = nullable1;
            int num = 0;
            if (!(nullable2.GetValueOrDefault() == num & nullable2.HasValue))
              goto label_5;
          }
          if (!string.IsNullOrEmpty(str))
            return (object) ThemeUtil.GetColor(dict, "BaseColorOpacity100_80");
label_5:
          return (object) ThemeUtil.GetColor(dict, "BaseColorOpacity40");
        }
      }
      return (object) new SolidColorBrush(Colors.White);
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
