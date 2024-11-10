// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.AttachmentIconConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class AttachmentIconConverter : IValueConverter
  {
    private static readonly Dictionary<string, string> AttachmentIconDict = new Dictionary<string, string>()
    {
      {
        "AUDIO",
        "pack://application:,,,/Assets/Attachment_Icon/Attachment_audio"
      },
      {
        "VIDEO",
        "pack://application:,,,/Assets/Attachment_Icon/Attachment_video"
      },
      {
        "PDF",
        "pack://application:,,,/Assets/Attachment_Icon/Attachment_pdf"
      },
      {
        "DOC",
        "pack://application:,,,/Assets/Attachment_Icon/Attachment_doc"
      },
      {
        "TEXT",
        "pack://application:,,,/Assets/Attachment_Icon/Attachment_txt"
      },
      {
        "XLS",
        "pack://application:,,,/Assets/Attachment_Icon/Attachment_xls"
      },
      {
        "PPT",
        "pack://application:,,,/Assets/Attachment_Icon/Attachment_ppt"
      },
      {
        "CSV",
        "pack://application:,,,/Assets/Attachment_Icon/Attachment_csv"
      },
      {
        "ZIP",
        "pack://application:,,,/Assets/Attachment_Icon/Attachment_zip"
      },
      {
        "OTHER",
        "pack://application:,,,/Assets/Attachment_Icon/Attachment_unknown"
      }
    };

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value == null)
        return (object) null;
      string str = AttachmentIconConverter.AttachmentIconDict["OTHER"];
      string key = value.ToString();
      if (AttachmentIconConverter.AttachmentIconDict.ContainsKey(key))
        str = AttachmentIconConverter.AttachmentIconDict[key];
      if (parameter is AttachmentConvertParam attachmentConvertParam && attachmentConvertParam.IsFailed)
      {
        str += "_failed";
        if (attachmentConvertParam.IsDark)
          str += "_dark";
      }
      return (object) (str + ".png");
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
