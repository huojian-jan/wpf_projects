// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.DisplayIconConverter
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
  public class DisplayIconConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 3)
        return (object) Utils.GetIconData("IcCheckbox");
      if (values[2] is int num && num != 0)
        return (object) Utils.GetIconData("IcCompletedProject");
      if (values[0] is DisplayType displayType)
      {
        switch (displayType)
        {
          case DisplayType.CheckItem:
            return (object) null;
          case DisplayType.Agenda:
            return (object) Utils.GetIconData("IcAgenda");
          case DisplayType.Event:
            return (object) Utils.GetIconData("IcCalendarEvent");
        }
      }
      return values[1] is "CHECKLIST" ? (object) Utils.GetIconData("IcCheckTask") : (object) Utils.GetIconData("IcCheckbox");
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
