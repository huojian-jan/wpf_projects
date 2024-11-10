// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CommentVisibleConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class CommentVisibleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null || parameter == null)
        return (object) Visibility.Visible;
      Constants.DetailMode detailMode = (Constants.DetailMode) Enum.Parse(typeof (Constants.DetailMode), value.ToString());
      int num = int.Parse(parameter.ToString());
      return (object) (Visibility) (detailMode == Constants.DetailMode.Page && num == 0 || detailMode == Constants.DetailMode.Popup && num == 1 ? 0 : 2);
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
