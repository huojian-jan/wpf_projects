// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.AttachmentBackgroundConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class AttachmentBackgroundConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return (object) null;
      if (parameter is AttachmentConvertParam attachmentConvertParam && attachmentConvertParam.IsFailed)
      {
        if (attachmentConvertParam.IsDark)
        {
          SolidColorBrush solidColorBrush = new SolidColorBrush(Color.FromRgb(byte.MaxValue, byte.MaxValue, byte.MaxValue));
          solidColorBrush.Opacity = 0.1;
          return (object) solidColorBrush;
        }
        SolidColorBrush solidColorBrush1 = new SolidColorBrush(Color.FromRgb((byte) 19, (byte) 19, (byte) 19));
        solidColorBrush1.Opacity = 0.03;
        return (object) solidColorBrush1;
      }
      string str = value.ToString();
      if (str != null)
      {
        switch (str.Length)
        {
          case 3:
            switch (str[1])
            {
              case 'D':
                if (str == "PDF")
                  return (object) ThemeUtil.GetColor("PdfBackgroundColor");
                break;
              case 'I':
                if (str == "ZIP")
                  return (object) ThemeUtil.GetColor("ZipBackgroundColor");
                break;
              case 'L':
                if (str == "XLS")
                  return (object) ThemeUtil.GetColor("XlsBackgroundColor");
                break;
              case 'O':
                if (str == "DOC")
                  return (object) ThemeUtil.GetColor("DocBackgroundColor");
                break;
              case 'P':
                if (str == "PPT")
                  return (object) ThemeUtil.GetColor("PptBackgroundColor");
                break;
              case 'S':
                if (str == "CSV")
                  return (object) ThemeUtil.GetColor("CsvBackgroundColor");
                break;
            }
            break;
          case 4:
            if (str == "TEXT")
              return (object) ThemeUtil.GetColor("TxtBackgroundColor");
            break;
          case 5:
            switch (str[0])
            {
              case 'A':
                if (str == "AUDIO")
                  return (object) ThemeUtil.GetColor("AudioBackgroundColor");
                break;
              case 'O':
                if (str == "OTHER")
                  return (object) ThemeUtil.GetColor("OtherBackgroundColor");
                break;
              case 'V':
                if (str == "VIDEO")
                  return (object) ThemeUtil.GetColor("VideoBackgroundColor");
                break;
            }
            break;
        }
      }
      return (object) ThemeUtil.GetColor("OtherBackgroundColor");
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
