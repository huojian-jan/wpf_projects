// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalMoreItems
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalMoreItems
  {
    private CustomMenuList _moreMenu;

    public event EventHandler<string> Action;

    public CalMoreItems(UIElement ui)
    {
      EscPopup escPopup1 = new EscPopup();
      escPopup1.StaysOpen = false;
      escPopup1.PopupAnimation = PopupAnimation.Fade;
      escPopup1.Placement = PlacementMode.Left;
      escPopup1.PlacementTarget = ui;
      escPopup1.HorizontalOffset = 35.0;
      escPopup1.VerticalOffset = 15.0;
      EscPopup escPopup2 = escPopup1;
      this._moreMenu = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) new List<CustomMenuItemViewModel>()
      {
        new CustomMenuItemViewModel((object) "DisplaySetting", Utils.GetString("DisplaySetting"), Utils.GetIcon("IcDisplaySetting")),
        new CustomMenuItemViewModel((object) "ArrangeTask", Utils.GetString("ArrangeTask"), Utils.GetIcon("IcArrangeTask")),
        new CustomMenuItemViewModel((object) "Subscribe", Utils.GetString("SubscribeCalendar"), Utils.GetImageSource("SubscribeDrawingLine", (FrameworkElement) ui)),
        new CustomMenuItemViewModel((object) "Print", Utils.GetString("Print"), Utils.GetImageSource("PrintDrawingImage", (FrameworkElement) ui))
        {
          SubActions = new List<CustomMenuItemViewModel>()
          {
            new CustomMenuItemViewModel((object) "PrintCalendar", Utils.GetString("PrintCalendar"), (Geometry) null),
            new CustomMenuItemViewModel((object) "PrintDetail", Utils.GetString("PrintDetail"), (Geometry) null)
          }
        }
      }, (Popup) escPopup2);
      this._moreMenu.Operated += new EventHandler<object>(this.OnSelected);
    }

    public void Show() => this._moreMenu.Show();

    private void OnSelected(object sender, object e)
    {
      switch (e as string)
      {
        case "ArrangeTask":
          EventHandler<string> action1 = this.Action;
          if (action1 == null)
            break;
          action1((object) this, "ArrangeTask");
          break;
        case "DisplaySetting":
          EventHandler<string> action2 = this.Action;
          if (action2 == null)
            break;
          action2((object) this, "DisplaySetting");
          break;
        case "Subscribe":
          EventHandler<string> action3 = this.Action;
          if (action3 == null)
            break;
          action3((object) this, "Subscribe");
          break;
        case "PrintCalendar":
          EventHandler<string> action4 = this.Action;
          if (action4 != null)
            action4((object) this, "Print");
          UserActCollectUtils.AddClickEvent("calendar", "toolbar", "print");
          break;
        case "PrintDetail":
          EventHandler<string> action5 = this.Action;
          if (action5 != null)
            action5((object) this, "PrintDetail");
          UserActCollectUtils.AddClickEvent("calendar", "toolbar", "print");
          break;
      }
    }
  }
}
