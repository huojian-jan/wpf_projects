// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.FilterConditionTitleConverter
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
  public class FilterConditionTitleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return (object) null;
      CondType condType = (CondType) value;
      string empty = string.Empty;
      switch (condType)
      {
        case CondType.Lists:
          empty = Utils.GetString("lists");
          break;
        case CondType.Tag:
          empty = Utils.GetString("tag");
          break;
        case CondType.Date:
          empty = Utils.GetString("date");
          break;
        case CondType.Priority:
          empty = Utils.GetString("priority");
          break;
        case CondType.Assignee:
          empty = Utils.GetString("assignee");
          break;
        case CondType.TaskType:
          empty = Utils.GetString("TaskType");
          break;
      }
      return (object) string.Format(Utils.GetString("LogicOf"), (object) empty);
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
