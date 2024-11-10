// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.PtfAddButtonVisibleConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class PtfAddButtonVisibleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 4)
      {
        if (((!(values[1] is bool flag) ? 0 : 1) & (flag ? 1 : 0)) != 0)
          return (object) Visibility.Collapsed;
        if (values[0] is TeamGroupViewModel teamGroupViewModel && values[3] is string str)
          return teamGroupViewModel.TeamId == str ? (object) Visibility.Visible : (object) Visibility.Collapsed;
        if (values[0] is PtfAllViewModel ptfAllViewModel && values[2] is PtfType ptfType && ptfAllViewModel.Type == ptfType)
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
