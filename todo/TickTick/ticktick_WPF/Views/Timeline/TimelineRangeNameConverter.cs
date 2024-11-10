// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.TimelineRangeNameConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class TimelineRangeNameConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return value is string key ? (object) Utils.GetString(TimelineConstants.GetRangeI18nKey(key)) : (object) Utils.GetString(TimelineConstants.GetRangeI18nKey(TimelineConstants.RangeDefault));
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
