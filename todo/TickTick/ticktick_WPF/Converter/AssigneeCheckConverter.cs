// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.AssigneeCheckConverter
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
  public class AssigneeCheckConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (parameter == null)
        return (object) false;
      string str = parameter.ToString();
      List<string> stringList = (List<string>) value;
      return (object) (bool) (stringList == null ? 0 : (stringList.Contains(str) ? 1 : 0));
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
