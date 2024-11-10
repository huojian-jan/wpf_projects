// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.BoolTextConverter
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
  public class BoolTextConverter : IValueConverter
  {
    private readonly string _defaultKey;
    private readonly string _negativeKey;
    private readonly string _positiveKey;

    public BoolTextConverter(string positiveKey, string negativeKey, string defaultKey)
    {
      this._positiveKey = positiveKey;
      this._negativeKey = negativeKey;
      this._defaultKey = defaultKey;
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Utils.GetBoolText(value, this._positiveKey, this._negativeKey, this._defaultKey);
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
