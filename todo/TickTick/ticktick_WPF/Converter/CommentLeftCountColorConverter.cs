// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.CommentLeftCountColorConverter
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
  public class CommentLeftCountColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return (object) ThemeUtil.GetColor("BaseColorOpacity100").ToString();
      int result;
      int.TryParse(value.ToString(), out result);
      return result > 1024 ? (object) ThemeUtil.GetColor("TextRedColor").ToString() : (object) ThemeUtil.GetColor("BaseColorOpacity100").ToString();
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
