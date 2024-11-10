// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.RecentCommentConverter
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
  public class RecentCommentConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value != null && value is CommentViewModel)
      {
        CommentViewModel commentViewModel = (CommentViewModel) value;
        if (parameter.ToString() == "Content")
          return (object) commentViewModel.Content;
        if (parameter.ToString() == "CreateDate")
          return (object) DateUtils.FormatCommentTime(commentViewModel.CreateDate);
        if (parameter.ToString() == "Avatar")
          return (object) commentViewModel.Avatar;
        if (parameter.ToString() == "UserName")
          return (object) commentViewModel.UserName;
      }
      return (object) string.Empty;
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
