// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ProjectFoldIconVisibleConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class ProjectFoldIconVisibleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      return values != null && values.Length == 3 && values[0] is bool flag1 && values[1] is bool flag2 && values[2] is bool flag3 && flag2 ? (object) (Visibility) (!flag1 | flag3 ? 0 : 2) : (object) Visibility.Collapsed;
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
