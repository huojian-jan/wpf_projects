// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.LogicBackgroundColorConverter
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
  public class LogicBackgroundColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && value is LogicType logicType && parameter is string str && logicType.ToString() == str)
      {
        switch (logicType)
        {
          case LogicType.And:
            return (object) ThemeUtil.GetColor("LogicAndBackground");
          case LogicType.Or:
            return (object) ThemeUtil.GetColor("LogicOrBackground");
          case LogicType.Not:
            return (object) ThemeUtil.GetColor("LogicNotBackground");
        }
      }
      return (object) ThemeUtil.GetColor("BaseColorOpacity10");
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
