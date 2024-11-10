// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.WidgetMorePopup
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class WidgetMorePopup
  {
    private EscPopup _popup;
    private System.Action _closeAction;

    public event EventHandler<WidgetMoreAction> MoreAction;

    public event EventHandler<string> Action;

    public WidgetMorePopup()
    {
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.PopupAnimation = PopupAnimation.Fade;
      escPopup.Placement = PlacementMode.Mouse;
      this._popup = escPopup;
      this._popup.Closed += (EventHandler) ((o, e) =>
      {
        System.Action closeAction = this._closeAction;
        if (closeAction == null)
          return;
        closeAction();
      });
    }

    public WidgetMorePopup(System.Action closeAction)
      : this()
    {
      this._closeAction = closeAction;
    }

    public void Show(Point point, bool isCalendar = false)
    {
      this._popup.Placement = PlacementMode.Bottom;
      this._popup.HorizontalOffset = point.X;
      this._popup.VerticalOffset = point.Y;
      List<CustomMenuItemViewModel> types;
      if (!isCalendar)
      {
        List<CustomMenuItemViewModel> menuItemViewModelList = new List<CustomMenuItemViewModel>();
        CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "Sync", Utils.GetString("Sync"), Utils.GetIcon("IcSync"));
        menuItemViewModel1.ShowSelected = false;
        menuItemViewModelList.Add(menuItemViewModel1);
        CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "Settings", Utils.GetString("WidgetSettings"), Utils.GetIcon("IcSettings"));
        menuItemViewModel2.ShowSelected = false;
        menuItemViewModelList.Add(menuItemViewModel2);
        CustomMenuItemViewModel menuItemViewModel3 = new CustomMenuItemViewModel((object) "Lock", Utils.GetString("Lock"), Utils.GetIcon("IcLockWidget"));
        menuItemViewModel3.ShowSelected = false;
        menuItemViewModelList.Add(menuItemViewModel3);
        CustomMenuItemViewModel menuItemViewModel4 = new CustomMenuItemViewModel((object) "Close", Utils.GetString("Close"), Utils.GetIcon("IcExit"));
        menuItemViewModel4.ShowSelected = false;
        menuItemViewModelList.Add(menuItemViewModel4);
        types = menuItemViewModelList;
      }
      else
      {
        types = new List<CustomMenuItemViewModel>();
        CustomMenuItemViewModel menuItemViewModel5 = new CustomMenuItemViewModel((object) "DisplaySetting", Utils.GetString("DisplaySetting"), Utils.GetIcon("IcDisplaySetting"));
        menuItemViewModel5.ShowSelected = false;
        types.Add(menuItemViewModel5);
        CustomMenuItemViewModel menuItemViewModel6 = new CustomMenuItemViewModel((object) "ArrangeTask", Utils.GetString("ArrangeTask"), Utils.GetIcon("IcArrangeTask"));
        menuItemViewModel6.ShowSelected = false;
        types.Add(menuItemViewModel6);
        CustomMenuItemViewModel menuItemViewModel7 = new CustomMenuItemViewModel((object) "Sync", Utils.GetString("Sync"), Utils.GetIcon("IcSync"));
        menuItemViewModel7.ShowSelected = false;
        types.Add(menuItemViewModel7);
        CustomMenuItemViewModel menuItemViewModel8 = new CustomMenuItemViewModel((object) "Settings", Utils.GetString("WidgetSettings"), Utils.GetIcon("IcSettings"));
        menuItemViewModel8.ShowSelected = false;
        types.Add(menuItemViewModel8);
        CustomMenuItemViewModel menuItemViewModel9 = new CustomMenuItemViewModel((object) "Lock", Utils.GetString("Lock"), Utils.GetIcon("IcLockWidget"));
        menuItemViewModel9.ShowSelected = false;
        types.Add(menuItemViewModel9);
        CustomMenuItemViewModel menuItemViewModel10 = new CustomMenuItemViewModel((object) "Close", Utils.GetString("Close"), Utils.GetIcon("IcExit"));
        menuItemViewModel10.ShowSelected = false;
        types.Add(menuItemViewModel10);
      }
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this._popup);
      customMenuList.Operated += new EventHandler<object>(this.OnItemSelected);
      customMenuList.Show();
    }

    private void OnItemSelected(object sender, object e)
    {
      switch (e as string)
      {
        case "Sync":
          EventHandler<WidgetMoreAction> moreAction1 = this.MoreAction;
          if (moreAction1 == null)
            break;
          moreAction1((object) this, WidgetMoreAction.Sync);
          break;
        case "Settings":
          EventHandler<WidgetMoreAction> moreAction2 = this.MoreAction;
          if (moreAction2 == null)
            break;
          moreAction2((object) this, WidgetMoreAction.Setting);
          break;
        case "Lock":
          EventHandler<WidgetMoreAction> moreAction3 = this.MoreAction;
          if (moreAction3 == null)
            break;
          moreAction3((object) this, WidgetMoreAction.Lock);
          break;
        case "Close":
          EventHandler<WidgetMoreAction> moreAction4 = this.MoreAction;
          if (moreAction4 == null)
            break;
          moreAction4((object) this, WidgetMoreAction.Exit);
          break;
        default:
          EventHandler<string> action = this.Action;
          if (action == null)
            break;
          action((object) this, e as string);
          break;
      }
    }

    public void SetResources(ResourceDictionary resources) => this._popup.Resources = resources;

    public void SetPlaceTarget(UIElement moreButton) => this._popup.PlacementTarget = moreButton;
  }
}
