// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.TagViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class TagViewModel : SelectableItemViewModel
  {
    public TagViewModel(TagModel tag, bool showIndent = false, bool inCal = false)
    {
      this.Id = tag.name;
      this.Title = tag.GetDisplayName();
      this.Icon = Utils.GetIconData("IcTagLine");
      this.ShowIndent = showIndent;
      this.IsSubItem = tag.IsChild();
      this.IsParent = tag.IsParent();
      this.Open = !tag.collapsed;
      this.Type = nameof (tag);
      this.ParentId = tag.parent;
      this.InCalFilter = inCal;
    }
  }
}
