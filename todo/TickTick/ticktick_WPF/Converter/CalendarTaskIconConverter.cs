// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CalendarTaskIconConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class CalendarTaskIconConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 3 && values[0] is DisplayType displayType)
      {
        bool valueOrDefault1 = (values[1] as bool?).GetValueOrDefault();
        bool valueOrDefault2 = (values[2] as bool?).GetValueOrDefault();
        switch (displayType)
        {
          case DisplayType.Task:
            if (valueOrDefault2)
              return (object) Utils.GetIcon("IcCalAbandonedIndicator");
            break;
          case DisplayType.Habit:
            if (valueOrDefault2)
              return (object) Utils.GetIcon("IcHabitUncompletedIndicator");
            return valueOrDefault1 ? (object) Utils.GetIcon("IcHabitCompletedIndicator") : (object) Utils.GetIcon("IcLineHabit");
          case DisplayType.Note:
            return (object) Utils.GetIcon("IcNoteIndicator");
        }
      }
      return (object) null;
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
