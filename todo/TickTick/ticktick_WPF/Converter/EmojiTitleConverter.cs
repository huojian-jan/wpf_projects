// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.EmojiTitleConverter
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
  public class EmojiTitleConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 2 || !(values[0] is string text) || !(values[1] is bool flag))
        return (object) string.Empty;
      if (!flag)
        return (object) string.Empty;
      string emojiIcon = EmojiHelper.GetEmojiIcon(text);
      return !text.StartsWith(emojiIcon) ? (object) string.Empty : (object) emojiIcon;
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
