// Decompiled with JetBrains decompiler
// Type: PopupLocationInfo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using ticktick_WPF.Util;
using TickTickUtils.Lang;

#nullable disable
internal class PopupLocationInfo
{
  private bool _enable = true;
  private bool _popupRight = true;
  private Popup _popup;

  private Point Trigger { get; set; }

  private Point Top { get; set; }

  private Point Bottom { get; set; }

  public bool IsSafeShowing()
  {
    return this._enable && this._popup != null && this._popup.IsOpen && this.IsInSafeArea();
  }

  public async void Mark()
  {
    if (Utils.IsWindows7())
    {
      this._enable = false;
    }
    else
    {
      Point position = PopupLocationCalculator.GetMousePoint(false);
      if (position.X > this.Trigger.X && this._popupRight || position.X < this.Trigger.X && !this._popupRight)
        await Task.Delay(100);
      this.Trigger = position;
    }
  }

  public bool IsInSafeArea()
  {
    return this._enable && Utils.IsPointInTriangle(PopupLocationCalculator.GetMousePoint(false), this.Trigger, this.Top, this.Bottom);
  }

  public async Task Bind(Popup popup, bool delay = false)
  {
    PopupLocationInfo popupLocationInfo1 = this;
    try
    {
      if (Utils.IsWindows7())
      {
        popupLocationInfo1._enable = false;
      }
      else
      {
        popupLocationInfo1._popup = popup;
        if (popupLocationInfo1._popup?.Child == null)
        {
          popupLocationInfo1._enable = false;
        }
        else
        {
          popupLocationInfo1.Trigger = PopupLocationCalculator.GetMousePoint(false);
          if (delay)
            await Task.Delay(80);
          Point screen1 = popup.Child.PointToScreen(new Point(0.0, 0.0));
          Point screen2 = popup.Child.PointToScreen(new Point(0.0, 0.0));
          double pointDpi = ScreenPositionUtils.GetPointDpi(screen1);
          screen2.Y += popup.Child.RenderSize.Height * pointDpi;
          if (popupLocationInfo1.Trigger.X < screen1.X)
          {
            popupLocationInfo1.Top = popup.Child.PointToScreen(new Point(0.0, 0.0));
            popupLocationInfo1.Bottom = screen2;
            popupLocationInfo1._popupRight = true;
          }
          else
          {
            PopupLocationInfo popupLocationInfo2 = popupLocationInfo1;
            double x1 = screen1.X;
            Size renderSize = popup.Child.RenderSize;
            double width1 = renderSize.Width;
            Point point1 = new Point(x1 + width1, screen1.Y);
            popupLocationInfo2.Top = point1;
            PopupLocationInfo popupLocationInfo3 = popupLocationInfo1;
            double x2 = screen2.X;
            renderSize = popup.Child.RenderSize;
            double width2 = renderSize.Width;
            Point point2 = new Point(x2 + width2, screen2.Y);
            popupLocationInfo3.Bottom = point2;
            popupLocationInfo1._popupRight = false;
          }
        }
      }
    }
    catch (Exception ex)
    {
      UtilLog.Error("PopupLocationInfo Init error" + ex?.ToString());
    }
  }
}
