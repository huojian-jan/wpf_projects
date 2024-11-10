// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CommentLineVisibleConverter
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
  public class CommentLineVisibleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && value.ToString() == Visibility.Visible.ToString())
        return (object) Visibility.Collapsed;
      if (value == null)
        return (object) Visibility.Visible;
      int num = value.ToString() == Visibility.Collapsed.ToString() ? 1 : 0;
      return (object) Visibility.Visible;
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
