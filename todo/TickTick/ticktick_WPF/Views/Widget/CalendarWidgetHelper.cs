// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.CalendarWidgetHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public static class CalendarWidgetHelper
  {
    public static async Task AddWidget()
    {
      if (CalendarWidgetHelper.Widget == null)
      {
        await SingleWidgetDao.CreateWidget(true);
        WidgetWindow widgetWindow = await CalendarWidgetHelper.TryLoadWidget();
      }
      App.RefreshIconMenu();
      JumpHelper.InitJumpList();
      GlobalEventManager.NotifyWidgetOpenChanged();
    }

    public static WidgetWindow Widget { get; private set; }

    public static async Task<WidgetWindow> TryLoadWidget()
    {
      CalendarWidgetHelper.Widget?.CloseWidget();
      CalendarWidgetModel widget1 = await SingleWidgetDao.TryGetWidget(0);
      if (widget1 != null)
      {
        WidgetWindow widget = new WidgetWindow(new WidgetViewModel(widget1));
        await Task.Delay(200);
        try
        {
          widget.ShowWidget();
        }
        catch (Exception ex)
        {
          widget = (WidgetWindow) null;
        }
        CalendarWidgetHelper.Widget = widget;
        App.RefreshIconMenu();
        JumpHelper.InitJumpList();
        widget = (WidgetWindow) null;
      }
      else
        CalendarWidgetHelper.Widget = (WidgetWindow) null;
      return CalendarWidgetHelper.Widget;
    }

    public static async Task HideWidget()
    {
      CalendarWidgetHelper.Widget?.CloseWidget();
      CalendarWidgetHelper.Widget = (WidgetWindow) null;
    }

    public static async Task CloseWidget()
    {
      CalendarWidgetHelper.Widget?.CloseWidget();
      CalendarWidgetHelper.Widget = (WidgetWindow) null;
      await SingleWidgetDao.DeleteWidget(0);
      App.RefreshIconMenu();
      JumpHelper.InitJumpList();
      GlobalEventManager.NotifyWidgetOpenChanged();
    }

    public static bool CanAddProject() => CalendarWidgetHelper.Widget == null;

    public static void CheckProEnable()
    {
      CalendarWidgetHelper.Widget?.CalendarWidget?.CheckCalendarProEnable();
    }

    public static void ReloadCal() => CalendarWidgetHelper.Widget?.CalendarWidget?.Reload();
  }
}
