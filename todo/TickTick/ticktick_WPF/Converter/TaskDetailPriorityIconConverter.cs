// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TaskDetailPriorityIconConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class TaskDetailPriorityIconConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      ResourceDictionary context = (ResourceDictionary) null;
      if (parameter is string str)
      {
        Application current = Application.Current;
        List<ResourceDictionary> list = current != null ? current.Resources.MergedDictionaries.ToList<ResourceDictionary>() : (List<ResourceDictionary>) null;
        string requestedCulture = "Resource\\icons_" + str + ".xaml";
        Func<ResourceDictionary, bool> predicate = (Func<ResourceDictionary, bool>) (d => d.Source.OriginalString.Equals(requestedCulture));
        context = list.FirstOrDefault<ResourceDictionary>(predicate);
      }
      if (values != null && values.Length == 2 && values[0] is int num)
      {
        switch (num)
        {
          case 0:
            return (object) Utils.GetImageSource("NonePriorityDrawingImage", context);
          case 1:
            return (object) Utils.GetImageSource("LowPriorityDrawingImage");
          case 3:
            return (object) Utils.GetImageSource("MidPriorityDrawingImage");
          case 5:
            return (object) Utils.GetImageSource("HighPriorityDrawingImage");
        }
      }
      return (object) Utils.GetImageSource("NonePriorityDrawingImage", context);
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
