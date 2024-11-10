// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Timeline.SortOrderImageConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Sort;

#nullable disable
namespace ticktick_WPF.Views.Timeline
{
  public class SortOrderImageConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      Constants.SortType result;
      return values.Length >= 1 && values[0] is string str && Enum.TryParse<Constants.SortType>(str, out result) ? (object) SortOptionHelper.GetSortTypeImage(result, true) : (object) Utils.GetImageSource("TimelineDefaultSortOrderDrawingImage");
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
