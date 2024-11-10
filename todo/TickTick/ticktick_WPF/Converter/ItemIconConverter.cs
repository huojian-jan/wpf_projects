// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ItemIconConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class ItemIconConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 4)
        return (object) Utils.GetIconData("IcCheckBox");
      DisplayType type = DisplayType.Task;
      int status = 0;
      string kind = "TEXT";
      if (values[0] is DisplayType)
        type = (DisplayType) values[0];
      if (values[1] is string)
        kind = values[1].ToString();
      if (values[2] is int)
        status = (int) values[2];
      int valueOrDefault = (values[3] as int?).GetValueOrDefault();
      return (object) ThemeUtil.GetTaskIconGeometry(type, status, kind, valueOrDefault);
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
