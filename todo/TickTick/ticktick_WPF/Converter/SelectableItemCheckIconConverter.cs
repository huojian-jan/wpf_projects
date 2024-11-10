// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SelectableItemCheckIconConverter
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
  public class SelectableItemCheckIconConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 6)
      {
        if (values[0] is bool flag1 && !flag1)
          return (object) null;
        if (values[1] is bool flag2 && values[2] is bool flag3 && values[3] is bool flag4 && values[4] is bool flag5 && values[5] is bool flag6)
        {
          if (flag2)
          {
            if (flag3)
            {
              if (flag4)
                return (object) Utils.GetIconData("CheckedBox");
              return flag5 ? (object) Utils.GetIconData("PartCheckedBox") : (object) Utils.GetIconData("UncheckBox");
            }
          }
          else
          {
            if (flag4)
              return (object) Utils.GetIconData("IcCheck");
            if (flag5 && !flag6)
              return (object) Utils.GetIconData("IcPartCheck");
          }
        }
      }
      return (object) null;
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
