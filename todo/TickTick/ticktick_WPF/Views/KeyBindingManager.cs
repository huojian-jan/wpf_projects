// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.KeyBindingManager
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views
{
  public static class KeyBindingManager
  {
    public static ConcurrentDictionary<string, List<KeyBinding>> AppKeyBinding = new ConcurrentDictionary<string, List<KeyBinding>>();

    public static event EventHandler<string> ShortCutChanged;

    public static void TryAddKeyBinding(string key, KeyBinding kb)
    {
      if (kb == null)
        return;
      try
      {
        KeyBindingManager.AddKeyBinding(key, kb);
      }
      catch (Exception ex)
      {
        string propertyValue = new ShortcutModel().GetPropertyValue(key);
        LocalSettings.Settings.ShortCutModel.SetPropertyValue(key, propertyValue);
        KeyBindingManager.AddKeyBinding(key, kb);
      }
    }

    private static void AddKeyBinding(string key, KeyBinding kb)
    {
      KeyBindingManager.SetKeyGesture(key, kb);
      if (KeyBindingManager.AppKeyBinding.ContainsKey(key))
        KeyBindingManager.AppKeyBinding[key].Add(kb);
      else
        KeyBindingManager.AppKeyBinding.TryAdd(key, new List<KeyBinding>()
        {
          kb
        });
    }

    public static void ChangeShortCut(string key, string newShort)
    {
      ThreadUtil.DetachedRunOnUiBackThread((Action) (() =>
      {
        if (KeyBindingManager.AppKeyBinding.ContainsKey(key))
        {
          foreach (KeyBinding keyBinding in KeyBindingManager.AppKeyBinding[key])
          {
            KeyGesture keyGesture = KeyBindingManager.GetKeyGesture(newShort);
            if (keyGesture != null && keyGesture.Modifiers != ModifierKeys.None)
            {
              keyBinding.Key = Key.None;
              keyBinding.Modifiers = ModifierKeys.None;
            }
            keyBinding.Gesture = (InputGesture) (keyGesture ?? new KeyGesture(Key.None));
          }
        }
        EventHandler<string> shortCutChanged = KeyBindingManager.ShortCutChanged;
        if (shortCutChanged == null)
          return;
        shortCutChanged((object) null, key);
      }));
    }

    private static KeyGesture GetKeyGesture(string newShort)
    {
      if (string.IsNullOrEmpty(newShort))
        return (KeyGesture) null;
      string[] source = newShort.Split('+');
      List<string> list = ((IEnumerable<string>) source).Where<string>((Func<string, bool>) (p => p != "Ctrl" && p != "Shift" && p != "Alt" && p != "Windows")).ToList<string>();
      string str = list.Count == 1 ? list[0] : string.Empty;
      if (str == "~")
        str = "Oem3";
      Key result;
      if (!Enum.TryParse<Key>(str, out result))
        return (KeyGesture) null;
      ModifierKeys modifiers = ((IEnumerable<string>) source).Contains<string>("Ctrl") ? ModifierKeys.Control : ModifierKeys.None;
      if (((IEnumerable<string>) source).Contains<string>("Shift"))
        modifiers |= ModifierKeys.Shift;
      if (((IEnumerable<string>) source).Contains<string>("Alt"))
        modifiers |= ModifierKeys.Alt;
      if (((IEnumerable<string>) source).Contains<string>("Windows"))
        modifiers |= ModifierKeys.Windows;
      if (modifiers == ModifierKeys.Shift)
        return (KeyGesture) null;
      return modifiers == ModifierKeys.None ? new KeyGesture(result) : new KeyGesture(result, modifiers);
    }

    public static void SetKeyGesture(string key, KeyBinding kb)
    {
      KeyGesture keyGesture = KeyBindingManager.GetKeyGesture(LocalSettings.Settings.ShortCutModel.GetPropertyValue(key));
      if (keyGesture != null && keyGesture.Modifiers != ModifierKeys.None)
      {
        kb.Key = Key.None;
        kb.Modifiers = ModifierKeys.None;
      }
      kb.Gesture = (InputGesture) (keyGesture ?? new KeyGesture(Key.None));
    }

    public static bool HasCtrlNumKeyBinding()
    {
      foreach (string allPropertyName in LocalSettings.Settings.ShortCutModel.GetAllPropertyNames())
      {
        if (!allPropertyName.StartsWith("Tab0"))
        {
          string propertyValue = LocalSettings.Settings.ShortCutModel.GetPropertyValue(allPropertyName);
          if (propertyValue != null && propertyValue.Length == 7 && propertyValue.ToLower().StartsWith("ctrl+d"))
            return true;
        }
      }
      return false;
    }

    public static void RemoveKeyBinding(string key, KeyBinding keyBinding)
    {
      if (!KeyBindingManager.AppKeyBinding.ContainsKey(key))
        return;
      KeyBindingManager.AppKeyBinding[key].Remove(keyBinding);
    }

    public static void ResetTabShortCut()
    {
      bool flag1 = KeyBindingManager.HasCtrlNumKeyBinding();
      bool flag2 = LocalSettings.Settings.ShortCutModel.Tab01 == "Ctrl+D1";
      for (int index = 1; index <= 9; ++index)
      {
        if (flag2 & flag1)
          LocalSettings.Settings.ShortCutModel.SetPropertyValue(string.Format("Tab0{0}", (object) index), string.Empty);
        if (!flag2 && !flag1)
          LocalSettings.Settings.ShortCutModel.SetPropertyValue(string.Format("Tab0{0}", (object) index), "Ctrl+D" + index.ToString());
      }
    }
  }
}
