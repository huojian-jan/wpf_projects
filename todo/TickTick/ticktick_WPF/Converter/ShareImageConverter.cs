// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.ShareImageConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class ShareImageConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values.Length == 2 && values[0] is string projectId)
      {
        ProjectModel projectById = CacheManager.GetProjectById(projectId);
        if (projectById != null && projectById.IsShareList())
        {
          switch (projectById.permission)
          {
            case "read":
              return (object) (DrawingImage) Application.Current?.FindResource((object) "ReadOnlyDrawingImage");
            case "comment":
              return (object) (DrawingImage) Application.Current?.FindResource((object) "CanCommentDrawingImage");
            default:
              return (object) (DrawingImage) Application.Current?.FindResource((object) "CanEditDrawingImage");
          }
        }
      }
      return (object) (DrawingImage) Application.Current?.FindResource((object) "CanEditDrawingImage");
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
