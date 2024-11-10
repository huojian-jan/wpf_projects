// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WidgetLocationHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class WidgetLocationHelper
  {
    public static WidgetLocationModel GetLocationSafely(
      double left,
      double top,
      double width,
      double height,
      Matrix? transform)
    {
      WidgetLocationModel locationSafely = new WidgetLocationModel()
      {
        Left = left,
        Top = top,
        Width = width,
        Height = height
      };
      Matrix valueOrDefault;
      double num1;
      if (!transform.HasValue)
      {
        num1 = 1.0;
      }
      else
      {
        valueOrDefault = transform.GetValueOrDefault();
        num1 = valueOrDefault.M11;
      }
      double factorX = num1;
      double num2;
      if (!transform.HasValue)
      {
        num2 = 1.0;
      }
      else
      {
        valueOrDefault = transform.GetValueOrDefault();
        num2 = valueOrDefault.M22;
      }
      double factorY = num2;
      System.Windows.Point point1 = new System.Windows.Point(left / factorX, top / factorY);
      System.Windows.Point point2 = new System.Windows.Point((left + width) / factorX, top / factorY);
      System.Windows.Point point3 = new System.Windows.Point(left / factorX, (top + height) / factorY);
      System.Windows.Point point4 = new System.Windows.Point((left + width) / factorX, (top + height) / factorY);
      Rectangle[] source = new Rectangle[4]
      {
        Rectangle.Empty,
        Rectangle.Empty,
        Rectangle.Empty,
        Rectangle.Empty
      };
      foreach (Screen allScreen in Screen.AllScreens)
      {
        Rectangle bounds = allScreen.Bounds;
        if (point1.X <= (double) (bounds.X + bounds.Width) && point1.X >= (double) bounds.X && point1.Y <= (double) (bounds.Y + bounds.Height) && point1.Y >= (double) bounds.Y)
          source[0] = bounds;
        if (point2.X <= (double) (bounds.X + bounds.Width) && point2.X >= (double) bounds.X && point2.Y <= (double) (bounds.Y + bounds.Height) && point2.Y >= (double) bounds.Y)
          source[1] = bounds;
        if (point3.X <= (double) (bounds.X + bounds.Width) && point3.X >= (double) bounds.X && point3.Y <= (double) (bounds.Y + bounds.Height) && point3.Y >= (double) bounds.Y)
          source[2] = bounds;
        if (point4.X <= (double) (bounds.X + bounds.Width) && point4.X >= (double) bounds.X && point4.Y <= (double) (bounds.Y + bounds.Height) && point4.Y >= (double) bounds.Y)
          source[3] = bounds;
      }
      if (((IEnumerable<Rectangle>) source).All<Rectangle>((Func<Rectangle, bool>) (r => r.IsEmpty)))
      {
        locationSafely.Left = 100.0;
        locationSafely.Top = 100.0;
        locationSafely.HideType = HideType.Reset;
        return locationSafely;
      }
      if (source[0].IsEmpty && source[1].IsEmpty || Math.Abs(point1.Y - (double) source[0].Y) < 1.0 || Math.Abs(point2.Y - (double) source[1].Y) < 1.0)
      {
        if (!source[2].IsEmpty && !source[2].Equals((object) source[3]))
          locationSafely.SetScreen(source[2], factorX, factorY);
        else if (source[2].IsEmpty && !source[3].IsEmpty)
          locationSafely.SetScreen(source[3], factorX, factorY);
        else
          locationSafely.SetScreen(source[2], factorX, factorY);
        locationSafely.HideType = HideType.Top;
        return locationSafely;
      }
      if (source[0].IsEmpty && source[2].IsEmpty || Math.Abs(point1.X - (double) source[0].X) < 1.0 && Math.Abs(point3.X - (double) source[2].X) < 1.0)
      {
        if (!source[1].IsEmpty && !source[1].Equals((object) source[3]))
          locationSafely.SetScreen(source[1], factorX, factorY);
        else if (source[1].IsEmpty && !source[3].IsEmpty)
          locationSafely.SetScreen(source[3], factorX, factorY);
        else
          locationSafely.SetScreen(source[1], factorX, factorY);
        locationSafely.HideType = HideType.Left;
        return locationSafely;
      }
      if (source[1].IsEmpty && source[3].IsEmpty || Math.Abs(point2.X - (double) source[1].X - (double) source[1].Width) < 1.0 && Math.Abs(point4.X - (double) source[3].X - (double) source[3].Width) < 1.0)
      {
        if (!source[0].IsEmpty && !source[0].Equals((object) source[2]))
          locationSafely.SetScreen(source[0], factorX, factorY);
        else if (source[0].IsEmpty && !source[2].IsEmpty)
          locationSafely.SetScreen(source[2], factorX, factorY);
        else
          locationSafely.SetScreen(source[0], factorX, factorY);
        locationSafely.HideType = HideType.Right;
        return locationSafely;
      }
      if (source[0].Equals((object) source[1]))
        locationSafely.SetScreen(source[0], factorX, factorY);
      return locationSafely;
    }

    public static System.Windows.Point GetPomoLocationSafely(
      double left,
      double top,
      double width,
      double height,
      Matrix? transform,
      System.Windows.Point defaultPoint)
    {
      System.Windows.Point pomoLocationSafely = new System.Windows.Point(left, top);
      Matrix valueOrDefault;
      double num1;
      if (!transform.HasValue)
      {
        num1 = 1.0;
      }
      else
      {
        valueOrDefault = transform.GetValueOrDefault();
        num1 = valueOrDefault.M11;
      }
      double num2 = num1;
      double num3;
      if (!transform.HasValue)
      {
        num3 = 1.0;
      }
      else
      {
        valueOrDefault = transform.GetValueOrDefault();
        num3 = valueOrDefault.M22;
      }
      double num4 = num3;
      System.Windows.Point point1 = new System.Windows.Point(left / num2, top / num4);
      System.Windows.Point point2 = new System.Windows.Point((left + width) / num2, top / num4);
      System.Windows.Point point3 = new System.Windows.Point(left / num2, (top + height) / num4);
      System.Windows.Point point4 = new System.Windows.Point((left + width) / num2, (top + height) / num4);
      Rectangle[] source = new Rectangle[4]
      {
        Rectangle.Empty,
        Rectangle.Empty,
        Rectangle.Empty,
        Rectangle.Empty
      };
      foreach (Screen allScreen in Screen.AllScreens)
      {
        Rectangle bounds = allScreen.Bounds;
        if (point1.X <= (double) (bounds.X + bounds.Width) && point1.X >= (double) bounds.X && point1.Y <= (double) (bounds.Y + bounds.Height) && point1.Y >= (double) bounds.Y)
          source[0] = bounds;
        if (point2.X <= (double) (bounds.X + bounds.Width) && point2.X >= (double) bounds.X && point2.Y <= (double) (bounds.Y + bounds.Height) && point2.Y >= (double) bounds.Y)
          source[1] = bounds;
        if (point3.X <= (double) (bounds.X + bounds.Width) && point3.X >= (double) bounds.X && point3.Y <= (double) (bounds.Y + bounds.Height) && point3.Y >= (double) bounds.Y)
          source[2] = bounds;
        if (point4.X <= (double) (bounds.X + bounds.Width) && point4.X >= (double) bounds.X && point4.Y <= (double) (bounds.Y + bounds.Height) && point4.Y >= (double) bounds.Y)
          source[3] = bounds;
      }
      if (((IEnumerable<Rectangle>) source).All<Rectangle>((Func<Rectangle, bool>) (r => r.IsEmpty)))
        return defaultPoint;
      if (source[0].IsEmpty && source[1].IsEmpty || Math.Abs(point1.Y - (double) source[0].Y - (double) source[0].Width) < 1.0 || Math.Abs(point2.Y - (double) source[1].Y - (double) source[1].Width) < 1.0)
      {
        if (!source[2].IsEmpty && !source[2].Equals((object) source[3]))
        {
          pomoLocationSafely.X = (double) (source[2].X + source[2].Width) * num2 - width;
          pomoLocationSafely.Y = (double) source[2].Y * num4;
        }
        else if (source[2].IsEmpty && !source[3].IsEmpty)
        {
          pomoLocationSafely.X = (double) source[3].X * num2;
          pomoLocationSafely.Y = (double) source[3].Y * num4;
        }
        else
        {
          pomoLocationSafely.X = left;
          pomoLocationSafely.Y = (double) source[2].Y * num4;
        }
        return pomoLocationSafely;
      }
      if ((!source[2].IsEmpty || !source[3].IsEmpty) && (Math.Abs(point3.Y - (double) source[2].Y) >= 1.0 || Math.Abs(point4.Y - (double) source[3].Y) >= 1.0))
        return pomoLocationSafely;
      if (!source[0].IsEmpty && !source[0].Equals((object) source[1]))
      {
        pomoLocationSafely.X = (double) (source[0].X + source[0].Width) * num2 - width;
        pomoLocationSafely.Y = (double) (source[0].Y + source[0].Height) * num4 - height - 20.0;
      }
      else if (source[0].IsEmpty && !source[1].IsEmpty)
      {
        pomoLocationSafely.X = (double) source[1].X * num2;
        pomoLocationSafely.Y = (double) (source[1].Y + source[1].Height) * num4 - height - 20.0;
      }
      else
      {
        pomoLocationSafely.X = left;
        pomoLocationSafely.Y = (double) (source[0].Y + source[0].Height) * num4 - height - 20.0;
      }
      return pomoLocationSafely;
    }
  }
}
