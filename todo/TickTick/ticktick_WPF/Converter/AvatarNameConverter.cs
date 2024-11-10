// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.AvatarNameConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class AvatarNameConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || !((IEnumerable<object>) values).Any<object>() || !(values[0] is string assignId) || !(values[1] is string projectId))
        return (object) Utils.GetString("AssignTo");
      return string.IsNullOrEmpty(assignId) || string.IsNullOrEmpty(projectId) || assignId == "-1" ? (object) Utils.GetString("AssignTo") : (object) AvatarHelper.GetCacheUserName(assignId, projectId);
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
