// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Converter.MaxWindowMarginConverter
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;

#nullable disable
namespace ticktick_WPF.Converter
{
  public class MaxWindowMarginConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      Rectangle rectangle1 = Screen.PrimaryScreen.Bounds;
      int width1 = rectangle1.Width;
      rectangle1 = Screen.PrimaryScreen.WorkingArea;
      int width2 = rectangle1.Width;
      Rectangle rectangle2;
      if (width1 == width2)
      {
        rectangle2 = Screen.PrimaryScreen.WorkingArea;
        if (rectangle2.Y == 0)
          return (object) new Thickness(0.0, 0.0, 0.0, SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height);
      }
      rectangle2 = Screen.PrimaryScreen.Bounds;
      int height1 = rectangle2.Height;
      rectangle2 = Screen.PrimaryScreen.WorkingArea;
      int height2 = rectangle2.Height;
      if (height1 == height2)
      {
        rectangle2 = Screen.PrimaryScreen.WorkingArea;
        if (rectangle2.X == 0)
          return (object) new Thickness(0.0, 0.0, SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Width, 0.0);
      }
      rectangle2 = Screen.PrimaryScreen.Bounds;
      int width3 = rectangle2.Width;
      rectangle2 = Screen.PrimaryScreen.WorkingArea;
      int width4 = rectangle2.Width;
      if (width3 == width4)
      {
        rectangle2 = Screen.PrimaryScreen.WorkingArea;
        if (rectangle2.Y > 0)
          return (object) new Thickness(0.0, SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Height, 0.0, 0.0);
      }
      rectangle2 = Screen.PrimaryScreen.Bounds;
      int height3 = rectangle2.Height;
      rectangle2 = Screen.PrimaryScreen.WorkingArea;
      int height4 = rectangle2.Height;
      if (height3 == height4)
      {
        rectangle2 = Screen.PrimaryScreen.WorkingArea;
        if (rectangle2.X > 0)
          return (object) new Thickness(SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Width, 0.0, 0.0, 0.0);
      }
      return (object) new Thickness(0.0, 0.0, 0.0, 45.0);
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
