// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.TypeContentConverter
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
  public class TypeContentConverter : IMultiValueConverter
  {
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      if (values == null || values.Length != 2)
        return (object) string.Empty;
      CardType cardType = (CardType) values[0];
      string empty = string.Empty;
      if (values[1] != null)
        empty = values[1].ToString();
      switch (cardType)
      {
        case CardType.InitFilter:
          return !string.IsNullOrEmpty(empty) ? (object) empty : (object) Utils.GetString("filterCondition");
        case CardType.InitLogic:
          return (object) Utils.GetString("logic");
        case CardType.AddMore:
          return (object) Utils.GetString("AddMore");
        case CardType.LogicAnd:
          return (object) Utils.GetString("and");
        case CardType.LogicOr:
          return (object) Utils.GetString("or");
        default:
          return (object) empty;
      }
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
