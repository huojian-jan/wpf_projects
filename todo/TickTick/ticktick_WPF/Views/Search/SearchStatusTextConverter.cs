// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchStatusTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchStatusTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null)
      {
        switch ((StatusFilter) value)
        {
          case StatusFilter.All:
            return (object) Utils.GetString("AllStatus");
          case StatusFilter.Uncompleted:
            return (object) Utils.GetString("Uncompleted");
          case StatusFilter.Completed:
            return (object) Utils.GetString("Completed");
          case StatusFilter.Abandoned:
            return (object) Utils.GetString("Abandoned");
        }
      }
      return (object) Utils.GetString("AllStatus");
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
