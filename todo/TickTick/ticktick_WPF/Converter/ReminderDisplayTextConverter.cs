// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ReminderDisplayTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class ReminderDisplayTextConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      return values == null || values.Length != 2 ? (object) string.Empty : (object) ReminderUtils.GetReminderListDisplayText(values[0] as ICollection<TaskReminderModel>, (values[1] as bool?).GetValueOrDefault());
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
