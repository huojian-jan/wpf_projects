// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Pinyin
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Util
{
  public struct Pinyin
  {
    public string Text;
    public string Inits;
    public List<int> EachStartIndex;

    public Pinyin(string text, string inits, List<int> indexes)
    {
      this.Text = text;
      this.Inits = inits;
      this.EachStartIndex = indexes;
    }

    public bool Contains(string text)
    {
      string inits = this.Inits;
      return (inits != null ? (inits.StartsWith(text) ? 1 : 0) : 0) != 0 || this.PinyinContains(text);
    }

    private bool PinyinContains(string text)
    {
      if (this.Text == text)
        return true;
      if (this.Text == null || this.EachStartIndex == null)
        return false;
      int num1 = this.Text.IndexOf(text, StringComparison.Ordinal);
      if (num1 < 0)
        return false;
      if (num1 == 0)
        return true;
      int num2 = num1 + text.Length;
      for (int index = 0; index < this.EachStartIndex.Count - 1; ++index)
      {
        if (this.EachStartIndex[index] == num1 && this.EachStartIndex[index + 1] < num2)
          return true;
      }
      return false;
    }
  }
}
