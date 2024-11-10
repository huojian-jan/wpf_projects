// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.CalArrangeDateFilterPopup
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public class CalArrangeDateFilterPopup
  {
    private EscPopup _popup;

    public CalArrangeDateFilterPopup(FrameworkElement target)
    {
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
      menuItemViewModel1.Selected = LocalSettings.Settings.ArrangeTaskDateType == 1;
      menuItemViewModel1.ImageMargin = new Thickness(11.0, 0.0, 2.0, 0.0);
      types.Add(menuItemViewModel1);
      CustomMenuItemViewModel menuItemViewModel2 = new CustomMenuItemViewModel((object) "undated", Utils.GetString("NoDate"), Utils.GetIcon("NodateIcon"));
      menuItemViewModel2.Selected = LocalSettings.Settings.ArrangeTaskDateType == 0;
      menuItemViewModel2.ImageMargin = new Thickness(11.0, 0.0, 2.0, 0.0);
      types.Add(menuItemViewModel2);
      types.Add(new CustomMenuItemViewModel((object) null));
      CustomMenuItemViewModel menuItemViewModel3 = new CustomMenuItemViewModel((object) "showNote", Utils.GetString(LocalSettings.Settings.ExtraSettings.ShowNoteInCalArrange ? "HideNote" : "ShowNote"), Utils.GetImageSource(LocalSettings.Settings.ExtraSettings.ShowNoteInCalArrange ? "HideNoteDrawingImage" : "ShowNoteDrawingImage", this._popup.PlacementTarget as FrameworkElement));
      menuItemViewModel3.ShowSelected = false;
      types.Add(menuItemViewModel3);
      CustomMenuItemViewModel menuItemViewModel4 = new CustomMenuItemViewModel((object) "showParent", Utils.GetString(LocalSettings.Settings.ExtraSettings.ShowParentInCalArrange ? "HideParent" : "ShowParent"), Utils.GetImageSource(LocalSettings.Settings.ExtraSettings.ShowParentInCalArrange ? "HideParentTaskDrawingImage" : "ShowParentTaskDrawingImage", this._popup.PlacementTarget as FrameworkElement));
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
          LocalSettings.Settings.ExtraSettings.ShowNoteInCalArrange = !LocalSettings.Settings.ExtraSettings.ShowNoteInCalArrange;
          break;
        case "showParent":
          LocalSettings.Settings.ExtraSettings.ShowParentInCalArrange = !LocalSettings.Settings.ExtraSettings.ShowParentInCalArrange;
          break;
        default:
          UserActCollectUtils.AddClickEvent("calendar", "arrangement_show", data);
          LocalSettings.Settings.ArrangeTaskDateType = data == "overdue" ? 1 : 0;
          break;
      }
      ticktick_WPF.Notifier.GlobalEventManager.NotifyArrangeTaskDateTypeChanged();
    }
  }
}
