// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CheckTaskTypeConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class CheckTaskTypeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      List<string> stringList = (List<string>) value;
      string str = parameter as string;
      return str == "All" ? (object) (bool) (stringList == null ? 1 : (stringList.Count == 0 ? 1 : 0)) : (object) stringList?.Contains(str);
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
