// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskDetailPriorityTextConverter
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
  public class TaskDetailPriorityTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is int num)
      {
        switch (num)
        {
          case 0:
            return (object) Utils.GetString("PriorityNull");
          case 1:
            return (object) Utils.GetString("PriorityLow");
          case 3:
            return (object) Utils.GetString("PriorityMedium");
          case 5:
            return (object) Utils.GetString("PriorityHigh");
        }
      }
      return (object) Utils.GetString("PriorityNull");
    }

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      return (object) null;
    }
  }
}
