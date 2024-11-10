// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CustomRepeatDisplayTextConverter
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
  public class CustomRepeatDisplayTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 3)
        return (object) string.Empty;
      string repeatFrom = values[0] as string;
      string repeatFlag = values[1] as string;
      DateTime? startDate = values[2] as DateTime?;
      if (repeatFrom != "2")
        return (object) RRuleUtils.RRule2String(repeatFrom, repeatFlag, startDate);
      return repeatFlag != null && repeatFlag.ToLower().Contains("lunar") && startDate.HasValue ? (object) ("农历 每年" + DateUtils.GetLunarMonthDay(startDate.Value)) : (object) Utils.GetString("Endlessly");
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
