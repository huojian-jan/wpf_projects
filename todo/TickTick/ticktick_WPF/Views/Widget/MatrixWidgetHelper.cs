// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Widget.MatrixWidgetHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Notifier;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Widget
{
  public class MatrixWidgetHelper
  {
    public static async Task AddWidget()
    {
      if (MatrixWidgetHelper.Widget == null)
      {
        await SingleWidgetDao.CreateWidget(false);
        WidgetWindow widgetWindow = await MatrixWidgetHelper.TryLoadWidget();
      }
      App.RefreshIconMenu();
      JumpHelper.InitJumpList();
      GlobalEventManager.NotifyWidgetOpenChanged();
    }

    public static WidgetWindow Widget { get; private set; }

    public static async Task<WidgetWindow> TryLoadWidget()
    {
      if (MatrixWidgetHelper.Widget != null)
      {
        MatrixWidgetHelper.Widget.CloseWidget();
        MatrixWidgetHelper.Widget = (WidgetWindow) null;
      }
      CalendarWidgetModel widget = await SingleWidgetDao.TryGetWidget(1);
      if (widget != null)
      {
        MatrixWidgetHelper.Widget = new WidgetWindow(new WidgetViewModel(widget));
        await Task.Delay(200);
        MatrixWidgetHelper.Widget.ShowWidget();
        App.RefreshIconMenu();
        JumpHelper.InitJumpList();
      }
      return MatrixWidgetHelper.Widget;
    }

    public static async Task HideWidget()
    {
      MatrixWidgetHelper.Widget?.CloseWidget();
      MatrixWidgetHelper.Widget = (WidgetWindow) null;
    }

    public static async Task CloseWidget()
    {
      MatrixWidgetHelper.Widget?.CloseWidget();
      MatrixWidgetHelper.Widget = (WidgetWindow) null;
      await SingleWidgetDao.DeleteWidget(1);
      App.RefreshIconMenu();
      JumpHelper.InitJumpList();
      GlobalEventManager.NotifyWidgetOpenChanged();
    }

    public static bool CanAddWidget() => MatrixWidgetHelper.Widget == null;

    public static void CheckProEnable()
    {
      MatrixWidgetHelper.Widget?.MatrixWidget?.CheckProEnable();
    }
  }
}
