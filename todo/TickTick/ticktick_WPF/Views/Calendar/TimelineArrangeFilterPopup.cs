// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.TimelineArrangeFilterPopup
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Timeline;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class TimelineArrangeFilterPopup
  {
    private readonly TimelineViewModel _model;
    private EscPopup _popup;

    public TimelineArrangeFilterPopup(TimelineViewModel model, FrameworkElement target)
    {
      this._model = model;
      EscPopup escPopup = new EscPopup();
      escPopup.StaysOpen = false;
      escPopup.PopupAnimation = PopupAnimation.Fade;
      escPopup.Placement = target == null ? PlacementMode.Mouse : PlacementMode.Bottom;
      escPopup.PlacementTarget = (UIElement) target;
      escPopup.VerticalOffset = -5.0;
      escPopup.HorizontalOffset = -140.0;
      escPopup.MinWidth = 180.0;
      this._popup = escPopup;
    }

    public void Show()
    {
      List<CustomMenuItemViewModel> types = new List<CustomMenuItemViewModel>();
      CustomMenuItemViewModel menuItemViewModel1 = new CustomMenuItemViewModel((object) "overdue", Utils.GetString("OverdueTask"), Utils.GetIcon("OverDateIcon"));
      menuItemViewModel1.Selected = this._model.IsOverDue;
      menuItemViewModel1.ImageMargin = new Thickness(11.0, 0.0, 2.0, 0.0);
      types.Add(menuItemViewModel1);
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "undated", Utils.GetString("NoDate"), Utils.GetIcon("NodateIcon"));
      menuItemViewModel2.Selected = !this._model.IsOverDue;
      menuItemViewModel2.ImageMargin = new Thickness(11.0, 0.0, 2.0, 0.0);
      types.Add(menuItemViewModel2);
      types.Add(new CustomMenuItemViewModel((object) null));
      CustomMenuItemViewModel menuItemViewModel3 = new CustomMenuItemViewModel((object) "showNote", Utils.GetString(this._model.ShowNoteInArrange ? "HideNote" : "ShowNote"), Utils.GetImageSource(this._model.ShowNoteInArrange ? "HideNoteDrawingImage" : "ShowNoteDrawingImage", this._popup.PlacementTarget as FrameworkElement));
      menuItemViewModel3.ShowSelected = false;
      types.Add(menuItemViewModel3);
      CustomMenuItemViewModel menuItemViewModel4 = new CustomMenuItemViewModel((object) "showParent", Utils.GetString(this._model.ShowParentInArrange ? "HideParent" : "ShowParent"), Utils.GetImageSource(this._model.ShowParentInArrange ? "HideParentTaskDrawingImage" : "ShowParentTaskDrawingImage", this._popup.PlacementTarget as FrameworkElement));
      menuItemViewModel4.ShowSelected = false;
      types.Add(menuItemViewModel4);
      CustomMenuList customMenuList = new CustomMenuList((IEnumerable<CustomMenuItemViewModel>) types, (Popup) this._popup);
      TextBlock textBlock1 = new TextBlock();
      textBlock1.Margin = new Thickness(12.0, -2.0, 0.0, 4.0);
      TextBlock textBlock2 = textBlock1;
      textBlock2.SetResourceReference(TextBlock.TextProperty, (object) nameof (Show));
      textBlock2.SetResourceReference(FrameworkElement.StyleProperty, (object) "Tag03");
      customMenuList.TopItem.Content = (object) textBlock2;
      customMenuList.Operated += new EventHandler<object>(this.OnDateSelected);
      customMenuList.Show();
    }

    private void OnDateSelected(object sender, object e)
    {
      if (!(e is string data))
        return;
      switch (data)
      {
        case "showNote":
          this._model.ShowNoteInArrange = !this._model.ShowNoteInArrange;
          break;
        case "showParent":
          this._model.ShowParentInArrange = !this._model.ShowParentInArrange;
          break;
        default:
          UserActCollectUtils.AddClickEvent("calendar", "arrangement_show", data);
          this._model.IsOverDue = data == "overdue";
          break;
      }
    }
  }
}
