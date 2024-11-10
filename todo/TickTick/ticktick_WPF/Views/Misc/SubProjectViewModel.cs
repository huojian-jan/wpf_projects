// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.SubProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class SubProjectViewModel : ProjectViewModel
  {
    public SubProjectViewModel(string id, string parentId, string title, bool share, bool isNote)
    {
      string emojiIcon = EmojiHelper.GetEmojiIcon(title);
      if (!string.IsNullOrEmpty(emojiIcon) && title.StartsWith(emojiIcon))
      {
        this.Title = title.Remove(0, emojiIcon.Length);
        this.Emoji = emojiIcon;
      }
      else
      {
        this.Title = title;
        this.Icon = isNote ? Utils.GetIconData(share ? "IcShareNoteProject" : "IcNoteProject") : Utils.GetIconData(share ? "IcSharedProject" : "IcNormalProject");
      }
      this.Id = id;
      this.ParentId = parentId;
      this.IsSubItem = true;
      this.Type = "normal";
      this.IsNote = isNote;
      this.IsShare = share;
    }
  }
}
