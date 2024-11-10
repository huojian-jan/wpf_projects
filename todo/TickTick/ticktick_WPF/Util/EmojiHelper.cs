// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.EmojiHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.Text.RegularExpressions;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class EmojiHelper
  {
    public static bool StartWithEmoji(string text)
    {
      string emojiIcon = EmojiHelper.GetEmojiIcon(text);
      return !string.IsNullOrEmpty(emojiIcon) && text.StartsWith(emojiIcon);
    }

    public static string GetEmojiIcon(string text)
    {
      try
      {
        if (string.IsNullOrEmpty(text))
          return string.Empty;
        Match match = EmojiData.MatchOne.Match(text);
        return match.Success ? match.Groups[0].Value : string.Empty;
      }
      catch (Exception ex)
      {
        return string.Empty;
      }
    }
  }
}
