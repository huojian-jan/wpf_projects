// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.PermissionIconColorConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows.Data;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class PermissionIconColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      int result;
      return value is Constants.ProjectPermission projectPermission && parameter is string s && int.TryParse(s, out result) && projectPermission == (Constants.ProjectPermission) result ? (object) ThemeUtil.GetColor("PrimaryColor") : (object) ThemeUtil.GetColor("BaseColorOpacity40");
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
