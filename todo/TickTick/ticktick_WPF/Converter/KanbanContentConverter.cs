﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.KanbanContentConverter
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
  public class KanbanContentConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (!(value is string text))
        return (object) null;
      if (text.Length > 200)
        text = text.Substring(0, 200);
      return (object) TaskUtils.ReplaceAttachmentTextInString(text).Replace("\r\n", " ").Replace("\n", "  ");
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