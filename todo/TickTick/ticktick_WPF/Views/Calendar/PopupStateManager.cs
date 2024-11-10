// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Calendar.PopupStateManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Calendar
{
  public static class PopupStateManager
  {
    private const int CheckDelay = 150;
    private static bool _addPopupOpen;
    private static bool _viewPopupOpen;
    private static bool _loadMorePopupOpen;
    private static bool _selection;
    private static bool _dragPopupOpen;
    private static bool _showLog = true;
    private static bool _viewPopupClose;
    public static bool CanOpenTimePopup = true;

    public static UIElement LastTarget { get; set; }

    public static void Reset()
    {
      PopupStateManager._addPopupOpen = false;
      PopupStateManager._viewPopupOpen = false;
      PopupStateManager._loadMorePopupOpen = false;
      PopupStateManager._dragPopupOpen = false;
      PopupStateManager._selection = false;
    }

    public static void OnAddPopupOpened()
    {
      PopupStateManager.d("add_open");
      PopupStateManager._addPopupOpen = true;
    }

    public static async void OnAddPopupClosed(bool needWait = true)
    {
      PopupStateManager._viewPopupOpen = false;
      if (needWait)
        await Task.Delay(200);
      PopupStateManager.d("add_close");
      PopupStateManager._addPopupOpen = false;
    }

    public static bool CanShowDragPopup() => !PopupStateManager._dragPopupOpen;

    public static async void OnViewPopupOpened()
    {
      PopupStateManager._viewPopupOpen = true;
      PopupStateManager._viewPopupClose = false;
    }

    public static async void OnViewPopupClosed(bool needWait = true)
    {
      PopupStateManager._viewPopupClose = true;
      if (needWait)
        await Task.Delay(200);
      PopupStateManager._viewPopupOpen = !PopupStateManager._viewPopupClose;
    }

    public static void OnLoadMorePopupOpened()
    {
      PopupStateManager._loadMorePopupOpen = true;
      PopupStateManager.d("load_open");
    }

    public static async void OnLoadMorePopupClosed()
    {
      await Task.Delay(150);
      PopupStateManager._loadMorePopupOpen = false;
      PopupStateManager.d("load_close");
    }

    public static bool CanShowAddPopup()
    {
      PopupStateManager.LogState();
      return !PopupStateManager._addPopupOpen && !PopupStateManager._viewPopupOpen && !PopupStateManager._loadMorePopupOpen;
    }

    public static bool CanShowDetailPopup()
    {
      PopupStateManager.LogState();
      return !PopupStateManager._addPopupOpen && !PopupStateManager._selection;
    }

    public static bool CanStartSelection()
    {
      return !PopupStateManager._addPopupOpen && !PopupStateManager._viewPopupOpen && !PopupStateManager._loadMorePopupOpen;
    }

    public static bool CanShowLoadMorePopup()
    {
      PopupStateManager.LogState();
      return !PopupStateManager._addPopupOpen && !PopupStateManager._viewPopupOpen && !PopupStateManager._loadMorePopupOpen;
    }

    public static void StartSelection() => PopupStateManager._selection = true;

    public static bool CheckAddPopup() => PopupStateManager._addPopupOpen;

    public static async void StopSelection()
    {
      await Task.Delay(150);
      PopupStateManager._selection = false;
    }

    public static bool IsInSelection() => PopupStateManager._selection;

    private static void d(string msg)
    {
      if (!PopupStateManager._showLog)
        return;
      Log.d(msg);
    }

    private static void LogState()
    {
    }

    public static bool IsViewPopOpened() => PopupStateManager._viewPopupOpen;

    public static bool IsInAdd() => PopupStateManager._addPopupOpen;

    public static async void SetCanOpenTimePopup(bool canOpen, bool delay = false)
    {
      if (delay)
        await Task.Delay(100);
      PopupStateManager.CanOpenTimePopup = canOpen;
    }
  }
}
