// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.HotkeyModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class HotkeyModel
  {
    private readonly Dictionary<Key, string> _specialSymbolDictionary = new Dictionary<Key, string>()
    {
      {
        Key.Space,
        "Space"
      },
      {
        Key.Oem3,
        "~"
      }
    };

    public bool Alt { get; set; }

    public bool Shift { get; set; }

    public bool Win { get; set; }

    public bool Ctrl { get; set; }

    public Key CharKey { get; set; }

    public ModifierKeys ModifierKeys
    {
      get
      {
        ModifierKeys modifierKeys = ModifierKeys.None;
        if (this.Alt)
          modifierKeys = ModifierKeys.Alt;
        if (this.Shift)
          modifierKeys |= ModifierKeys.Shift;
        if (this.Win)
          modifierKeys |= ModifierKeys.Windows;
        if (this.Ctrl)
          modifierKeys |= ModifierKeys.Control;
        return modifierKeys;
      }
    }

    public HotkeyModel(string hotkeyString) => this.Parse(hotkeyString);

    public HotkeyModel(bool alt, bool shift, bool win, bool ctrl, Key key)
    {
      this.Alt = alt;
      this.Shift = shift;
      this.Win = win;
      this.Ctrl = ctrl;
      this.CharKey = key;
    }

    private void Parse(string hotkeyString)
    {
      if (string.IsNullOrEmpty(hotkeyString))
        return;
      List<string> list = ((IEnumerable<string>) hotkeyString.Replace(" ", "").Split('+')).ToList<string>();
      if (list.Contains("Alt"))
      {
        this.Alt = true;
        list.Remove("Alt");
      }
      if (list.Contains("Shift"))
      {
        this.Shift = true;
        list.Remove("Shift");
      }
      if (list.Contains("Win"))
      {
        this.Win = true;
        list.Remove("Win");
      }
      if (list.Contains("Ctrl"))
      {
        this.Ctrl = true;
        list.Remove("Ctrl");
      }
      if (list.Count <= 0)
        return;
      string charKey = list[0];
      KeyValuePair<Key, string>? nullable = new KeyValuePair<Key, string>?(this._specialSymbolDictionary.FirstOrDefault<KeyValuePair<Key, string>>((Func<KeyValuePair<Key, string>, bool>) (pair => pair.Value == charKey)));
      if (nullable.Value.Value != null)
      {
        this.CharKey = nullable.Value.Key;
      }
      else
      {
        try
        {
          this.CharKey = (Key) Enum.Parse(typeof (Key), charKey);
        }
        catch (ArgumentException ex)
        {
        }
      }
    }

    public override string ToString()
    {
      string text = string.Empty;
      if (this.Ctrl)
        text += "Ctrl+";
      if (this.Alt)
        text += "Alt+";
      if (this.Shift)
        text += "Shift+";
      if (this.Win)
        text += "Win+";
      if (this.CharKey != Key.None)
        text += this._specialSymbolDictionary.ContainsKey(this.CharKey) ? this._specialSymbolDictionary[this.CharKey] : (this.CharKey < Key.D0 || this.CharKey > Key.D9 ? this.CharKey.ToString() : this.CharKey.ToString().Replace("D", ""));
      else if (!string.IsNullOrEmpty(text))
        text = text.Remove(text.Length - 3);
      return HotkeyModel.HandleUpDownText(text);
    }

    public string ToRealString()
    {
      string realString = string.Empty;
      if (this.Ctrl)
        realString += "Ctrl + ";
      if (this.Alt)
        realString += "Alt + ";
      if (this.Shift)
        realString += "Shift + ";
      if (this.Win)
        realString += "Win + ";
      if (this.CharKey != Key.None)
        realString += this._specialSymbolDictionary.ContainsKey(this.CharKey) ? this._specialSymbolDictionary[this.CharKey] : this.CharKey.ToString();
      else if (!string.IsNullOrEmpty(realString))
        realString = realString.Remove(realString.Length - 3);
      return realString;
    }

    public static string HandleUpDownText(string text)
    {
      if (text.Contains("Up") && !text.Contains("PageUp"))
        text = text.Replace("Up", "↑");
      if (text.Contains("Down") && !text.Contains("PageDown"))
        text = text.Replace("Down", "↓");
      if (text.Contains("Left"))
        text = text.Replace("Left", "←");
      if (text.Contains("Right"))
        text = text.Replace("Right", "→");
      return text;
    }
  }
}
