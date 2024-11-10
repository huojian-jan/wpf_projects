// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SelectedListConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class SelectedListConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      List<string> stringList = (List<string>) value;
      if (stringList != null && stringList.Count == 0)
        return (object) Utils.GetString("All");
      if (parameter != null && parameter.ToString() == "Tag")
        return (object) TagCardViewModel.ToNormalDisplayText(stringList);
      if (parameter != null && parameter.ToString() == "DueDate")
        return (object) DateCardViewModel.ToNormalDisplayText(stringList);
      return parameter != null && parameter.ToString() == "Assignee" ? (object) AssigneeCardViewModel.ToNormalDisplayText(stringList) : (object) string.Empty;
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
