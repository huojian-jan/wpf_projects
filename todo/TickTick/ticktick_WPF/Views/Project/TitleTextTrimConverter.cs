// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.TitleTextTrimConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class TitleTextTrimConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is string str))
        return (object) string.Empty;
      string text1 = str.Substring(0, Math.Min(str.Length, 9));
      double width = Utils.MeasureString(text1, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 10.0).Width;
      if (width < 42.0)
      {
        string text2 = text1;
        for (; width < 42.0 && text1.Length < str.Length; width = Utils.MeasureString(text2, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 10.0).Width)
        {
          text1 = text2;
          if (text2.Length != str.Length)
            text2 = str.Substring(0, text2.Length + 1);
          else
            break;
        }
      }
      else
      {
        for (; width > 42.0 && text1.Length > 0; width = Utils.MeasureString(text1, new Typeface(Utils.GetDefaultFontFamily(), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal), 10.0).Width)
          text1 = text1.Substring(0, text1.Length - 1);
      }
      return (object) text1;
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
