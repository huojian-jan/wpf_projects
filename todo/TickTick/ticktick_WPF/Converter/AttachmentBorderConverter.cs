// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.AttachmentBorderConverter
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
  public class AttachmentBorderConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return (object) null;
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
                  return (object) ThemeUtil.GetColor("PdfBorderColor");
                break;
              case 'I':
                if (str == "ZIP")
                  return (object) ThemeUtil.GetColor("ZipBorderColor");
                break;
              case 'L':
                if (str == "XLS")
                  return (object) ThemeUtil.GetColor("XlsBorderColor");
                break;
              case 'O':
                if (str == "DOC")
                  return (object) ThemeUtil.GetColor("DocBorderColor");
                break;
              case 'P':
                if (str == "PPT")
                  return (object) ThemeUtil.GetColor("PptBorderColor");
                break;
              case 'S':
                if (str == "CSV")
                  return (object) ThemeUtil.GetColor("CsvBorderColor");
                break;
            }
            break;
          case 4:
            if (str == "TEXT")
              return (object) ThemeUtil.GetColor("TxtBorderColor");
            break;
          case 5:
            switch (str[0])
            {
              case 'A':
                if (str == "AUDIO")
                  return (object) ThemeUtil.GetColor("AudioBorderColor");
                break;
              case 'O':
                if (str == "OTHER")
                  return (object) ThemeUtil.GetColor("OtherBorderColor");
                break;
              case 'V':
                if (str == "VIDEO")
                  return (object) ThemeUtil.GetColor("VideoBorderColor");
                break;
            }
            break;
        }
      }
      return (object) ThemeUtil.GetColor("OtherBorderColor");
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
