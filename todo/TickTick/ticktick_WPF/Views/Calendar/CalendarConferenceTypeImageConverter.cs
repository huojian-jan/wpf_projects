// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalendarConferenceTypeImageConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalendarConferenceTypeImageConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 2 || !(values[0] is string str) || !(values[1] is ResourceDictionary resourceDictionary))
        return (object) null;
      Dictionary<string, string> dictionary = new Dictionary<string, string>()
      {
        {
          "video",
          "DrawConferenceVideo"
        },
        {
          "phone",
          "DrawConferencePhone"
        },
        {
          "ohter",
          "DrawConferenceOther"
        }
      };
      string lower = str.ToLower();
      string key = dictionary.ContainsKey(lower) ? lower : "other";
      return resourceDictionary[(object) dictionary[key]];
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
