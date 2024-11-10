// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskListTextPostponeConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Provider;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class TaskListTextPostponeConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 3)
        return (object) Visibility.Collapsed;
      if (values[0] is string str && values[2] is bool flag)
      {
        object obj = values[1] is ProjectIdentity ? values[1] : (object) MainWindowManager.GetSelectedProject();
        int num1;
        if (str == "outdated")
        {
          switch (obj)
          {
            case TodayProjectIdentity _:
            case WeekProjectIdentity _:
              num1 = 1;
              break;
            default:
              num1 = obj is AllProjectIdentity ? 1 : 0;
              break;
          }
        }
        else
          num1 = 0;
        int num2 = flag ? 1 : 0;
        if ((num1 & num2) != 0)
          return (object) Visibility.Visible;
      }
      return (object) Visibility.Collapsed;
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
