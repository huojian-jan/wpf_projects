// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.HotKeyUtils
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using NHotkey;
using NHotkey.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ticktick_WPF.Cache;
using ticktick_WPF.Resource;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.Lock;
using ticktick_WPF.Views.Pomo;
using ticktick_WPF.Views.QuickAdd;

#nullable disable
namespace ticktick_WPF.Util
{
  public class HotKeyUtils
  {
    public Key Key;
    public ModifierKeys Modifiers;

    public HotKeyUtils()
    {
    }

    public HotKeyUtils(string hotkeyString)
    {
      bool flag1 = false;
      bool flag2 = false;
      bool flag3 = false;
      int modifierCount = 0;
      string[] source = hotkeyString.Split('+');
      foreach (string str in source)
      {
        switch (str.ToLower())
        {
          case "ctrl":
            if (!flag1)
            {
              flag1 = true;
              ++modifierCount;
              break;
            }
            break;
          case "alt":
            if (!flag2)
            {
              flag2 = true;
              modifierCount += 2;
              break;
            }
            break;
          case "shift":
            if (!flag3)
            {
              flag3 = true;
              modifierCount += 4;
              break;
            }
            break;
        }
      }
      string str1 = ((IEnumerable<string>) source).LastOrDefault<string>();
      if (str1 == "~")
        str1 = "Oem3";
      Key result;
      if (Enum.TryParse<Key>(str1, out result))
        this.Key = result;
      this.GetModifierKeys(modifierCount);
    }

    private void GetModifierKeys(int modifierCount)
    {
      switch (modifierCount)
      {
        case 0:
          this.Modifiers = ModifierKeys.None;
          break;
        case 1:
          this.Modifiers = ModifierKeys.Control;
          break;
        case 2:
          this.Modifiers = ModifierKeys.Alt;
          break;
        case 3:
          this.Modifiers = ModifierKeys.Alt | ModifierKeys.Control;
          break;
        case 4:
          this.Modifiers = ModifierKeys.Shift;
          break;
        case 5:
          this.Modifiers = ModifierKeys.Control | ModifierKeys.Shift;
          break;
        case 6:
          this.Modifiers = ModifierKeys.Alt | ModifierKeys.Shift;
          break;
        case 7:
          this.Modifiers = ModifierKeys.Alt | ModifierKeys.Control | ModifierKeys.Shift;
          break;
      }
    }

    public static void ResignHotKey()
    {
      HotKeyUtils.RegisterOpenAppShortcut();
      HotKeyUtils.RegisterQuickAddShortcut();
      HotKeyUtils.RegisterPinShortcut();
      if (LocalSettings.Settings.EnableFocus)
      {
        HotKeyUtils.RegisterPomoShortcut();
        HotKeyUtils.RegisterPomoBeginShortcut();
        HotKeyUtils.RegisterPomoPauseShortcut();
      }
      HotKeyUtils.RegisterLockShortcut();
      HotKeyUtils.RegisterCreateSticky();
      HotKeyUtils.RegisterShowHideSticky();
    }

    public static void ResignHotKey(string key)
    {
      if (key == null)
        return;
      switch (key.Length)
      {
        case 11:
          if (!(key == "ShortcutPin"))
            break;
          HotKeyUtils.TryRegHotKey("ShortcutPin", LocalSettings.Settings.ShortcutPin, new EventHandler<HotkeyEventArgs>(HotKeyUtils.PinHotKey));
          break;
        case 12:
          switch (key[0])
          {
            case 'L':
              if (!(key == "LockShortcut"))
                return;
              HotKeyUtils.RegisterLockShortcut();
              return;
            case 'P':
              if (!(key == "PomoShortcut") || !LocalSettings.Settings.EnableFocus)
                return;
              HotKeyUtils.RegisterPomoShortcut();
              return;
            case 'S':
              if (!(key == "StartAndDrop") || !LocalSettings.Settings.EnableFocus)
                return;
              HotKeyUtils.RegisterPomoBeginShortcut();
              return;
            default:
              return;
          }
        case 15:
          if (!(key == "ShortcutAddTask"))
            break;
          HotKeyUtils.TryRegHotKey("ShortcutAddTask", LocalSettings.Settings.ShortcutAddTask, new EventHandler<HotkeyEventArgs>(HotKeyUtils.AddTaskHotKey));
          break;
        case 16:
          if (!(key == "ContinueAndPause") || !LocalSettings.Settings.EnableFocus)
            break;
          HotKeyUtils.RegisterPomoPauseShortcut();
          break;
        case 19:
          if (!(key == "ShortcutOpenOrClose"))
            break;
          HotKeyUtils.TryRegHotKey("ShortcutOpenOrClose", LocalSettings.Settings.ShortcutOpenOrClose, new EventHandler<HotkeyEventArgs>(HotKeyUtils.OpenOrCloseWindowHotKey));
          break;
        case 20:
          if (!(key == "CreateStickyShortcut"))
            break;
          HotKeyUtils.RegisterCreateSticky();
          break;
        case 22:
          if (!(key == "ShowHideStickyShortcut"))
            break;
          HotKeyUtils.RegisterShowHideSticky();
          break;
      }
    }

    private static void RegisterCreateSticky()
    {
      HotKeyUtils.TryRegHotKey("CreateStickyShortcut", LocalSettings.Settings.CreateStickyShortCut, new EventHandler<HotkeyEventArgs>(HotKeyUtils.CreateStickyHotKey));
    }

    private static void RegisterShowHideSticky()
    {
      HotKeyUtils.TryRegHotKey("ShowHideStickyShortcut", LocalSettings.Settings.ShowHideStickyShortCut, new EventHandler<HotkeyEventArgs>(HotKeyUtils.ShowHideStickyHotKey));
    }

    private static void RegisterPinShortcut()
    {
      HotKeyUtils.TryRegHotKey("ShortcutPin", LocalSettings.Settings.ShortcutPin, new EventHandler<HotkeyEventArgs>(HotKeyUtils.PinHotKey));
    }

    private static void RegisterQuickAddShortcut()
    {
      HotKeyUtils.TryRegHotKey("ShortcutAddTask", LocalSettings.Settings.ShortcutAddTask, new EventHandler<HotkeyEventArgs>(HotKeyUtils.AddTaskHotKey));
    }

    private static void RegisterOpenAppShortcut()
    {
      HotKeyUtils.TryRegHotKey("ShortcutOpenOrClose", LocalSettings.Settings.ShortcutOpenOrClose, new EventHandler<HotkeyEventArgs>(HotKeyUtils.OpenOrCloseWindowHotKey));
    }

    private static void ShowHideStickyHotKey(object sender, HotkeyEventArgs e)
    {
      UserActCollectUtils.AddShortCutEvent("global_action", "show_hide_all_stickys");
      TaskStickyWindow.ShowHideAllStickyStatic();
    }

    private static void CreateStickyHotKey(object sender, HotkeyEventArgs e)
    {
      UserActCollectUtils.AddShortCutEvent("global_action", "new_sticky_note");
      TaskStickyWindow.CreateNewStickyStatic();
    }

    private static void TryRegHotKey(
      string name,
      string hotKeyString,
      EventHandler<HotkeyEventArgs> handler)
    {
      if (string.IsNullOrWhiteSpace(hotKeyString))
      {
        HotkeyManager.Current.Remove(name);
      }
      else
      {
        HotKeyUtils hotKeyUtils = new HotKeyUtils(hotKeyString.Replace(" ", ""));
        if (hotKeyUtils.Key != Key.None)
        {
          if (hotKeyUtils.Modifiers != ModifierKeys.None)
          {
            try
            {
              HotkeyManager.Current?.AddOrReplace(name, hotKeyUtils.Key, hotKeyUtils.Modifiers, handler);
              return;
            }
            catch (Exception ex)
            {
              return;
            }
          }
        }
        HotkeyManager.Current?.Remove(name);
      }
    }

    public static void RegisterPomoShortcut()
    {
      HotKeyUtils.TryRegHotKey("PomoShortcut", LocalSettings.Settings.PomoShortcut, new EventHandler<HotkeyEventArgs>(HotKeyUtils.ShowOrHidePomoHandler));
    }

    public static void RegisterPomoBeginShortcut()
    {
      HotKeyUtils.TryRegHotKey("StartAndDrop", LocalSettings.Settings.ShortCutModel.StartAndDrop, new EventHandler<HotkeyEventArgs>(HotKeyUtils.StartOrEndPomoHandler));
    }

    public static void RegisterPomoPauseShortcut()
    {
      HotKeyUtils.TryRegHotKey("ContinueAndPause", LocalSettings.Settings.ShortCutModel.ContinueAndPause, new EventHandler<HotkeyEventArgs>(HotKeyUtils.PauseOrContinuePomoHandler));
    }

    private static async void RegisterLockShortcut()
    {
      HotKeyUtils.TryRegHotKey("LockShortcut", LocalSettings.Settings.LockShortcut, new EventHandler<HotkeyEventArgs>(HotKeyUtils.LockAppHandler));
    }

    private static async void LockAppHandler(object sender, HotkeyEventArgs e)
    {
      int num = await AppLockCache.GetAppLocked() ? 1 : 0;
      UserActCollectUtils.AddShortCutEvent("global_action", "lock_unlock_app");
      if (num == 0)
        App.Instance.TryLockApp();
      else
        AppUnlockWindow.TryUnlockApp(new Action(MainWindowManager.Window.ShowWindow));
    }

    private static void ShowOrHidePomoHandler(object sender, HotkeyEventArgs e)
    {
      if (!LocalSettings.Settings.EnableFocus && !string.IsNullOrEmpty(LocalSettings.Settings.PomoShortcut))
        return;
      UserActCollectUtils.AddShortCutEvent("global_action", "open_close_focus_window");
      TickFocusManager.HideOrShowFocusWidget(false, 0);
    }

    private static void StartOrEndPomoHandler(object sender, HotkeyEventArgs e)
    {
      if (!LocalSettings.Settings.EnableFocus)
        return;
      TickFocusManager.StartOrDrop();
    }

    private static void PauseOrContinuePomoHandler(object sender, HotkeyEventArgs e)
    {
      if (!LocalSettings.Settings.EnableFocus)
        return;
      TickFocusManager.ContinueOrPause();
    }

    private static void PinHotKey(object sender, HotkeyEventArgs e)
    {
      UserActCollectUtils.AddShortCutEvent("global_action", "pin_cancelpin_app");
      LocalSettings.Settings.MainWindowTopmost = !LocalSettings.Settings.MainWindowTopmost;
      MainWindowManager.Window.ToggleAppPinOption();
    }

    private static void AddTaskHotKey(object sender, HotkeyEventArgs e)
    {
      if (string.IsNullOrEmpty(LocalSettings.Settings.LoginUserId))
        return;
      UserActCollectUtils.AddShortCutEvent("global_action", "quick_add");
      QuickAddWindow.ShowOrHideQuickAddWindow();
    }

    private static async void OpenOrCloseWindowHotKey(object sender, HotkeyEventArgs e)
    {
      UserActCollectUtils.AddShortCutEvent("global_action", "showhide_main_window");
      MainWindowManager.Window.ShowOrHideWindow();
    }

    public static void RemoveHotkeys()
    {
      HotkeyManager.Current.Remove("ShortcutOpenOrClose");
      HotkeyManager.Current.Remove("ShortcutAddTask");
      HotkeyManager.Current.Remove("PomoShortcut");
      HotkeyManager.Current.Remove("StartAndDrop");
      HotkeyManager.Current.Remove("ContinueAndPause");
      HotkeyManager.Current.Remove("LockShortcut");
      HotkeyManager.Current.Remove("ShortcutPin");
      HotkeyManager.Current.Remove("CreateStickyShortcut");
    }
  }
}
