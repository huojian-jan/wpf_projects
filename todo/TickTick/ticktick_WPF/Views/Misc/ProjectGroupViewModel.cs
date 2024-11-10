// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.ProjectGroupViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class ProjectGroupViewModel : SelectableItemViewModel
  {
    public ProjectGroupViewModel(ProjectGroupModel model)
    {
      this.Id = model.id;
      this.Title = model.name;
      string emojiIcon = EmojiHelper.GetEmojiIcon(model.name);
      if (!string.IsNullOrEmpty(emojiIcon) && model.name.StartsWith(emojiIcon))
      {
        this.Title = model.name.Remove(0, emojiIcon.Length);
        this.Emoji = emojiIcon;
      }
      else
      {
        this.Title = model.name;
        this.Icon = Utils.GetIconData(model.open ? "IcOpenedFolder" : "IcClosedFolder");
      }
      this.Type = "group";
      this.IsParent = true;
      this.Open = model.open;
    }

    protected override void SetOpenIcon()
    {
      if (!string.IsNullOrEmpty(this.Emoji))
        return;
      this.Icon = Utils.GetIconData(this.Open ? "IcOpenedFolder" : "IcClosedFolder");
    }
  }
}
