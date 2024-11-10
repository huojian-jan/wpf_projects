// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TagViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class TagViewModel : BaseHidableViewModel
  {
    public TagViewModel(TagModel tag)
    {
      if (tag == null)
        return;
      this.Tag = tag.GetDisplayName();
      this.Extra = tag.GetDisplayName();
      this.Color = !string.IsNullOrEmpty(tag.color) ? tag.color : ThemeUtil.GetColorValue("ColorPrimary").ToString();
      this.SortOrder = tag.sortOrder;
    }

    public TagViewModel(string name, string extra)
    {
      this.Tag = name;
      this.Extra = extra;
      this.IsMore = true;
      this.Color = ThemeUtil.GetColorValue("ColorPrimary").ToString();
    }

    public string Extra { get; } = string.Empty;

    public string Tag { get; } = string.Empty;

    public bool IsMore { get; }

    public string Color { get; } = string.Empty;

    public long SortOrder { get; set; }

    public TagViewModel Copy() => this.MemberwiseClone() as TagViewModel;
  }
}
