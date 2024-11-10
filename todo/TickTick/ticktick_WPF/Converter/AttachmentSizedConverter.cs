// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.AttachmentSizedConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class AttachmentSizedConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if ((int) value < 1024)
        return (object) (value?.ToString() + "B");
      return (int) value < 1024000 ? (object) (((double) (int) value / 1024.0).ToString("f2") + "KB") : (object) (((double) (int) value / 1024000.0).ToString("f2") + "MB");
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
