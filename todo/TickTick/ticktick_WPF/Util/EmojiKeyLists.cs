// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.EmojiKeyLists
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace ticktick_WPF.Util
{
  public class EmojiKeyLists
  {
    public string CnKeys;
    public string EnKeys;
    public string CiKeys;

    public void AddToList(string s, string listName)
    {
      switch (listName)
      {
        case "CnKeys":
          this.CnKeys = s;
          break;
        case "EnKeys":
          this.EnKeys = s;
          break;
        case "CiKeys":
          this.CiKeys = s;
          break;
      }
    }

    public bool CheckKeys(List<string> keys)
    {
      keys.RemoveAll(new Predicate<string>(string.IsNullOrEmpty));
      return CnMatch() || CiMatch() || EnMatch();

      static bool StartWith(string text, string val)
      {
        if (text == null)
          return false;
        int num = text.IndexOf(val, StringComparison.Ordinal);
        if (num < 0)
          return false;
        return num == 0 || text[num - 1] == ',' || text[num - 1] == ' ';
      }

      bool CiMatch()
      {
        return keys.All<string>((Func<string, bool>) (key => this.CiKeys != null && this.CiKeys.Contains(key)));
      }

      bool CnMatch()
      {
        return keys.All<string>((Func<string, bool>) (key => this.CnKeys != null && this.CnKeys.Contains(key)));
      }

      bool EnMatch()
      {
        return keys.All<string>((Func<string, bool>) (key =>
        {
          if (key.Length < 3 && key.All<char>(new Func<char, bool>(CharUtils.IsAbc)))
            return StartWith(this.EnKeys, key);
          return this.EnKeys != null && this.EnKeys.Contains(key);
        }));
      }
    }
  }
}
