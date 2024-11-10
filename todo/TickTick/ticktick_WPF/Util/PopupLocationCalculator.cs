// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.PopupLocationCalculator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class PopupLocationCalculator
  {
    public static System.Windows.Point GetMousePoint(bool isDefault)
    {
      if (isDefault)
        return new System.Windows.Point();
      System.Drawing.Point mousePosition = Control.MousePosition;
      return new System.Windows.Point((double) mousePosition.X, (double) mousePosition.Y);
    }

    public static TaskPopupArgs GetPopupLocation(
      UIElement target,
      double targetWidth,
      double width,
      bool byMouse,
      double addHeight)
    {
      TaskPopupArgs popupLocation = new TaskPopupArgs()
      {
        IsRight = true,
        ByMouse = byMouse
      };
      if (byMouse)
        return popupLocation;
      System.Windows.Point point1;
      try
      {
        point1 = target.PointToScreen(new System.Windows.Point(0.0, 0.0));
      }
      catch (Exception ex)
      {
        popupLocation.ByMouse = true;
        return popupLocation;
      }
      try
      {
        CompositionTarget compositionTarget = PresentationSource.FromVisual((Visual) target)?.CompositionTarget;
        if (compositionTarget != null)
        {
          Matrix transformFromDevice = compositionTarget.TransformFromDevice;
          if (addHeight != 0.0)
            addHeight /= transformFromDevice.M22;
          Rectangle rectangle = ScreenPositionUtils.GetPointLocationScreen(point1, new System.Windows.Point?(new System.Windows.Point(point1.X + targetWidth, point1.Y + addHeight)));
          point1 = transformFromDevice.Transform(point1);
          System.Windows.Point point2 = transformFromDevice.Transform(new System.Windows.Point((double) rectangle.X, (double) rectangle.Y));
          System.Windows.Point point3 = transformFromDevice.Transform(new System.Windows.Point((double) rectangle.Width, (double) rectangle.Height));
          rectangle = new Rectangle((int) point2.X, (int) point2.Y, (int) point3.X, (int) point3.Y);
          if (point1.X + targetWidth + width - 10.0 > (double) (rectangle.X + rectangle.Width))
            popupLocation.IsRight = false;
        }
      }
      catch (Exception ex)
      {
      }
      return popupLocation;
    }

    public static double FixVerticalLocation(
      double left,
      double top,
      double height,
      UIElement element)
    {
      Matrix? transformFromDevice = PresentationSource.FromVisual((Visual) element)?.CompositionTarget?.TransformFromDevice;
      Rectangle rectangle = ScreenPositionUtils.GetPointLocationScreen(new System.Windows.Point(left / (transformFromDevice.HasValue ? transformFromDevice.GetValueOrDefault().M11 : 1.0), top / (transformFromDevice.HasValue ? transformFromDevice.GetValueOrDefault().M22 : 1.0)), new System.Windows.Point?());
      if (transformFromDevice.HasValue)
      {
        System.Windows.Point point1 = transformFromDevice.Value.Transform(new System.Windows.Point((double) rectangle.X, (double) rectangle.Y));
        System.Windows.Point point2 = transformFromDevice.Value.Transform(new System.Windows.Point((double) rectangle.Width, (double) rectangle.Height));
        rectangle = new Rectangle((int) point1.X, (int) point1.Y, (int) point2.X, (int) point2.Y);
      }
      return top + height > (double) (rectangle.Y + rectangle.Height) ? (double) (rectangle.Y + rectangle.Height) - height - 35.0 : top;
    }

    public static System.Windows.Point GetPopupLocation(
      Window context,
      double width,
      double height,
      double leftMargin = 0.0,
      double rightMargin = 0.0,
      double topMargin = 0.0)
    {
      System.Drawing.Point mousePosition = Control.MousePosition;
      System.Windows.Point point1 = new System.Windows.Point((double) mousePosition.X, (double) mousePosition.Y);
      CompositionTarget compositionTarget = PresentationSource.FromVisual((Visual) context)?.CompositionTarget;
      if (compositionTarget == null)
        return new System.Windows.Point((double) mousePosition.X, (double) mousePosition.Y);
      System.Windows.Point point2 = compositionTarget.TransformFromDevice.Transform(point1);
      PopupLocationCalculator.AlignDirection direction = PopupLocationCalculator.CalculateDirection(point2.X, point2.Y, width, height, leftMargin, rightMargin);
      double x = point2.X;
      double y = point2.Y;
      switch (direction)
      {
        case PopupLocationCalculator.AlignDirection.TopLeft:
          x = x - width + 10.0 - leftMargin;
          y = y - height + 10.0 + 10.0;
          break;
        case PopupLocationCalculator.AlignDirection.TopRight:
          x = x - 10.0 + rightMargin;
          y = y - height + 10.0 + 10.0;
          break;
        case PopupLocationCalculator.AlignDirection.BottomLeft:
          x = x - width + 10.0 - leftMargin;
          y = y - 10.0 - 10.0 - topMargin;
          break;
        case PopupLocationCalculator.AlignDirection.BottomRight:
          x = x - 10.0 + rightMargin;
          y = y - 10.0 - 10.0 - topMargin;
          break;
      }
      return new System.Windows.Point(x, y);
    }

    private static PopupLocationCalculator.Alignment CalculateAlignment(
      double virtualWidth,
      double virtualHeight,
      double primaryWidth,
      double primaryHeight)
    {
      return virtualHeight == primaryHeight && virtualWidth > primaryWidth || virtualWidth != primaryWidth || virtualHeight <= primaryHeight ? PopupLocationCalculator.Alignment.Horizontal : PopupLocationCalculator.Alignment.Vertical;
    }

    private static PopupLocationCalculator.AlignDirection CalculateDirection(
      double x,
      double y,
      double width,
      double height,
      double lm,
      double rm)
    {
      double virtualScreenWidth = SystemParameters.VirtualScreenWidth;
      double virtualScreenHeight = SystemParameters.VirtualScreenHeight;
      double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
      double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
      switch (PopupLocationCalculator.CalculateAlignment(virtualScreenWidth, virtualScreenHeight, primaryScreenWidth, primaryScreenHeight))
      {
        case PopupLocationCalculator.Alignment.Vertical:
          return PopupLocationCalculator.CalculateVerticalDirection(x, y, width, height, virtualScreenWidth, virtualScreenHeight, primaryScreenWidth, primaryScreenHeight, lm, rm);
        case PopupLocationCalculator.Alignment.Horizontal:
          return PopupLocationCalculator.CalculateHorizontalDirection(x, y, width, height, virtualScreenWidth, virtualScreenHeight, primaryScreenWidth, primaryScreenHeight, lm, rm);
        default:
          return PopupLocationCalculator.AlignDirection.BottomRight;
      }
    }

    private static PopupLocationCalculator.AlignDirection CalculateHorizontalDirection(
      double x,
      double y,
      double width,
      double height,
      double vw,
      double vh,
      double pw,
      double ph,
      double lm,
      double rm)
    {
      double num = vw;
      if (x >= 0.0 && x <= pw)
        num = pw;
      return PopupLocationCalculator.GetAlignDirection(x + width + rm > num, y + height > Math.Min(vh, ph));
    }

    private static PopupLocationCalculator.AlignDirection CalculateVerticalDirection(
      double x,
      double y,
      double width,
      double height,
      double vw,
      double vh,
      double pw,
      double ph,
      double lm,
      double rm)
    {
      double num = vh;
      if (y >= 0.0 && y <= ph)
        num = ph;
      bool top = y + height > num;
      return PopupLocationCalculator.GetAlignDirection(x + width + rm > Math.Min(vw, pw), top);
    }

    private static PopupLocationCalculator.AlignDirection GetAlignDirection(bool left, bool top)
    {
      if (left && !top)
        return PopupLocationCalculator.AlignDirection.BottomLeft;
      if (!left && !top)
        return PopupLocationCalculator.AlignDirection.BottomRight;
      return left ? PopupLocationCalculator.AlignDirection.TopLeft : PopupLocationCalculator.AlignDirection.TopRight;
    }

    public static System.Windows.Point GetTimeDialogLocation(
      UIElement target,
      double width,
      double height,
      double hOffset,
      double vOffset)
    {
      System.Windows.Point timeDialogLocation;
      if (target == null)
      {
        System.Drawing.Point mousePosition = Control.MousePosition;
        timeDialogLocation = new System.Windows.Point((double) mousePosition.X, (double) mousePosition.Y);
      }
      else
      {
        try
        {
          timeDialogLocation = target.PointToScreen(new System.Windows.Point(0.0, 0.0));
        }
        catch (Exception ex)
        {
          System.Drawing.Point mousePosition = Control.MousePosition;
          timeDialogLocation = new System.Windows.Point((double) mousePosition.X, (double) mousePosition.Y);
        }
      }
      try
      {
        Rectangle rectangle = ScreenPositionUtils.GetPointLocationScreen(timeDialogLocation, new System.Windows.Point?(new System.Windows.Point(timeDialogLocation.X, timeDialogLocation.Y)));
        CompositionTarget compositionTarget = PresentationSource.FromVisual((Visual) (target ?? (UIElement) App.Window))?.CompositionTarget;
        if (compositionTarget != null)
        {
          Matrix transformFromDevice = compositionTarget.TransformFromDevice;
          timeDialogLocation = transformFromDevice.Transform(timeDialogLocation);
          System.Windows.Point point1 = transformFromDevice.Transform(new System.Windows.Point((double) rectangle.X, (double) rectangle.Y));
          System.Windows.Point point2 = transformFromDevice.Transform(new System.Windows.Point((double) rectangle.Width, (double) rectangle.Height));
          rectangle = new Rectangle((int) point1.X, (int) point1.Y, (int) point2.X, (int) point2.Y);
        }
        return new System.Windows.Point(timeDialogLocation.X + hOffset + width + 10.0 <= (double) (rectangle.X + rectangle.Width) ? timeDialogLocation.X + hOffset : (double) (rectangle.X + rectangle.Width) - width - 10.0, timeDialogLocation.Y + vOffset + height + 10.0 <= (double) (rectangle.Y + rectangle.Height) ? timeDialogLocation.Y + vOffset : (double) (rectangle.Y + rectangle.Height) - height - 10.0);
      }
      catch (Exception ex)
      {
        return timeDialogLocation;
      }
    }

    private enum Alignment
    {
      Vertical,
      Horizontal,
    }

    private enum AlignDirection
    {
      TopLeft,
      TopRight,
      BottomLeft,
      BottomRight,
    }
  }
}
