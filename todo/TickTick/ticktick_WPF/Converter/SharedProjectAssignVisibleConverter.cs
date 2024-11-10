// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SharedProjectAssignVisibleConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class SharedProjectAssignVisibleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string projectId)
      {
        ProjectModel projectById = CacheManager.GetProjectById(projectId);
        if ((projectById != null ? (projectById.IsShareList() ? 1 : 0) : 0) != 0)
          return (object) Visibility.Visible;
      }
      return (object) Visibility.Collapsed;
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
