// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskDisplayIconConverter
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
  public class TaskDisplayIconConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 3)
      {
        if (values[0] is int num)
        {
          switch (num)
          {
            case -1:
              return (object) Utils.GetIconData("IcAbandoned");
            case 0:
              break;
            default:
              return (object) Utils.GetIconData("IcChecked");
          }
        }
        if (values[2] is string str && !string.IsNullOrEmpty(str))
          return (object) Utils.GetIconData("IcAgendaItem");
        if (values[1] is "CHECKLIST")
          return (object) Utils.GetIconData("IcCheckList");
      }
      return (object) Utils.GetIconData("IcCheckBox");
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
