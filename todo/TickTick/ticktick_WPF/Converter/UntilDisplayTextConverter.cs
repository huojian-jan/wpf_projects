// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.UntilDisplayTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Ical.Net.DataTypes;
using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class UntilDisplayTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 1)
        return (object) string.Empty;
      string str = values[0] as string;
      string untilText = RRuleUtils.GetUntilText((RecurrencePattern) RecurrenceModel.GetRecurrenceModel(str), str, false);
      return string.IsNullOrEmpty(untilText) ? (object) string.Empty : (object) untilText;
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
