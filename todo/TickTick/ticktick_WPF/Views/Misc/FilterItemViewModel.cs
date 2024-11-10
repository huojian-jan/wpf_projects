// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.FilterItemViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class FilterItemViewModel : SelectableItemViewModel
  {
    public FilterItemViewModel(string id, string title)
    {
      this.Id = id;
      this.Title = title;
      this.Type = "filter";
      this.CanMultiSelect = false;
      string emojiIcon = EmojiHelper.GetEmojiIcon(title);
      if (!string.IsNullOrEmpty(emojiIcon) && title.StartsWith(emojiIcon))
      {
        this.Title = title.Remove(0, emojiIcon.Length);
        this.Emoji = emojiIcon;
      }
      else
      {
        this.Title = title;
        this.Icon = Utils.GetIconData("IcFilterProject");
      }
    }
  }
}
