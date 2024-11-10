// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.PathImageConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Files;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class PathImageConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length != 2 || !(values[0] is string path) || !(values[1] is double num))
        return (object) ThemeUtil.GetPlaceholderAttachmentImage();
      bool flag = false;
      int width = ((int) (num * 1.5) / 1000 + 1) * 1000;
      if (!FileUtils.FileEmptyOrNotExists(path))
      {
        try
        {
          return (object) ThemeUtil.GetImage(path, width);
        }
        catch (Exception ex)
        {
          UtilLog.Error(string.Format("PathImageConverter.Convert Read Image Failed, {0}", (object) ex));
          flag = true;
        }
      }
      return (object) ThemeUtil.GetPlaceholderAttachmentImage(path.StartsWith("failed", StringComparison.OrdinalIgnoreCase) | flag);
    }

    public object[] ConvertBack(
      object value,
      Type[] targetTypes,
      object parameter,
      CultureInfo culture)
    {
      return (object[]) null;
    }
  }
}
