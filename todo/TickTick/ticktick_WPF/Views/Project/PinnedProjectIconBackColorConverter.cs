﻿// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.PinnedProjectIconBackColorConverter
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
  public class PinnedProjectIconBackColorConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values != null && values.Length == 2 && values[1] is string str)
      {
        string color = values[0] as string;
        if (str == "White" || str == "Blue")
        {
          if (string.IsNullOrEmpty(color) || color == "transparent")
          {
            Color? resource = Application.Current?.FindResource((object) "ColorPrimary") as Color?;
            ref Color? local = ref resource;
            color = (local.HasValue ? local.GetValueOrDefault().ToString() : (string) null) ?? "#000000";
          }
          return (object) ThemeUtil.GetAlphaColor(color, 10);
        }
      }
      return (object) ThemeUtil.GetAlphaColor("#FFFFFF", 80);
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