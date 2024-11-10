// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.SwitchTeamTextConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class SwitchTeamTextConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is string str && !string.IsNullOrEmpty(str))
        return (object) string.Format(Utils.GetString("MoveToA"), (object) Utils.GetString("PersonalSpace"));
      List<TeamModel> teams = CacheManager.GetTeams();
      return !teams.Any<TeamModel>() ? (object) null : (object) string.Format(Utils.GetString("MoveToA"), (object) teams[0].name);
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
