// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.EmptyStringToVisibilityConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class EmptyStringToVisibilityConverter : MarkupExtension, IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return (object) (Visibility) (string.IsNullOrEmpty(value as string) ? 0 : 2);
    }

    public object ConvertBack(
      object value,
      Type targetType,
      object parameter,
      CultureInfo culture)
    {
      return (object) null;
    }

    public override object ProvideValue(IServiceProvider serviceProvider) => (object) this;
  }
}
