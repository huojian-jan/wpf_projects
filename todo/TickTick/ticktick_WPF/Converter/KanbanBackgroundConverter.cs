// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.KanbanBackgroundConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class KanbanBackgroundConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 4)
      {
        if (((!(values[0] is bool flag1) ? 0 : 1) & (flag1 ? 1 : 0)) != 0)
          return (object) ThemeUtil.GetColor("ItemSelectedColor");
        if (((!(values[1] is bool flag2) ? 0 : 1) & (flag2 ? 1 : 0)) != 0 || ((!(values[2] is bool flag3) ? 0 : 1) & (flag3 ? 1 : 0)) != 0)
          return (object) ThemeUtil.GetColor("BaseColorOpacity5");
      }
      return (object) ThemeUtil.GetColor("KanbanTaskItemBackground");
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
