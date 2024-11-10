// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.DefaultRepeatDisplayTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class DefaultRepeatDisplayTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 3)
        return (object) string.Empty;
      string repeatFrom = values[0] as string;
      string repeatFlag = values[1] as string;
      DateTime? startDate = values[2] as DateTime?;
      if (RepeatUtils.GetRepeatType(repeatFrom, repeatFlag) == RepeatFromType.Custom)
        return (object) Utils.GetString("RepeatByCustom");
      if (string.IsNullOrEmpty(repeatFlag) || repeatFlag == "RRULE:FREQ=NONE")
        return (object) string.Empty;
      return !repeatFlag.Contains("FREQ=DAILY") || !repeatFlag.Contains("TT_SKIP=HOLIDAY,WEEKEND") || repeatFlag.Contains("INTERVAL") && !repeatFlag.Contains("INTERVAL=1") ? (object) RRuleUtils.RRule2String(repeatFrom, repeatFlag, startDate, false, true) : (object) (Utils.GetString("OfficialWorkingDays") + " (" + Utils.GetString("MonToFri") + ")");
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
