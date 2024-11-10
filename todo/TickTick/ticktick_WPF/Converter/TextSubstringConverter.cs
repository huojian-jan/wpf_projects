// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TextSubstringConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class TextSubstringConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is string str) || !(parameter is string s))
        return (object) null;
      int result;
      if (!int.TryParse(s, out result))
        return (object) str;
      return str.Length <= result ? (object) str : (object) str.Substring(0, result);
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
