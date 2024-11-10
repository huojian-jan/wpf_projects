// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.PinnedProjectIconColorConverter
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
  public class PinnedProjectIconColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 2)
        return (object) ThemeUtil.GetColorInString("#66000000");
      string color = values[0] as string;
      if (string.IsNullOrEmpty(color) || color == "transparent")
      {
        Color? resource = Application.Current?.FindResource((object) "ColorPrimary") as Color?;
        ref Color? local = ref resource;
        color = (local.HasValue ? local.GetValueOrDefault().ToString() : (string) null) ?? "#66000000";
      }
      return (object) ThemeUtil.GetColorInString(color);
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
