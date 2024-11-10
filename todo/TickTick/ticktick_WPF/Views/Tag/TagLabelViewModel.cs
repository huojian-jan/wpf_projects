// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Tag.TagLabelViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Linq;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Tag
{
  public class TagLabelViewModel : BaseViewModel
  {
    private TagLabelViewModel()
    {
    }

    public TagLabelViewModel(string tag)
    {
      this.Tag = tag;
      this.Title = CacheManager.GetTags().FirstOrDefault<TagModel>((Func<TagModel, bool>) (t => t.name == tag))?.GetDisplayName() ?? tag ?? "";
      this.Color = ThemeUtil.GetColorValue("ColorPrimary").ToString();
      this.IsAdd = false;
      this.SortOrder = 0L;
    }

    public TagLabelViewModel(TagModel tag)
    {
      this.Tag = tag.name;
      this.Title = tag.GetDisplayName() ?? "";
      this.Color = !string.IsNullOrEmpty(tag.color) ? tag.color : ThemeUtil.GetColorValue("ColorPrimary").ToString();
      this.IsAdd = false;
      this.SortOrder = tag.sortOrder;
    }

    public bool IsAdd { get; private set; }

    public bool Adding { get; set; }

    public long SortOrder { get; set; }

    public string Color { get; }

    public bool Editable { get; set; } = true;

    public string Tag { get; }

    public string Title { get; } = "";

    public static TagLabelViewModel BuildAddModel()
    {
      return new TagLabelViewModel()
      {
        IsAdd = true,
        SortOrder = long.MaxValue
      };
    }
  }
}
