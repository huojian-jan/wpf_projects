// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.GroupIndicatorAngleConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class GroupIndicatorAngleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      int num1 = 0;
      int num2 = 90;
      if (parameter is string str)
      {
        char[] chArray = new char[1]{ ';' };
        string[] strArray = str.Split(chArray);
        int result1;
        int result2;
        if (strArray.Length == 2 && int.TryParse(strArray[0], out result1) && int.TryParse(strArray[1], out result2))
        {
          num1 = result1;
          num2 = result2;
        }
      }
      return value == null || !(bool) value ? (object) num2 : (object) num1;
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
