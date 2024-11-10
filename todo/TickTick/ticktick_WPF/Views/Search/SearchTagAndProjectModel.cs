// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Search.SearchTagAndProjectModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Search
{
  public class SearchTagAndProjectModel : SearchItemBaseModel
  {
    public bool IsIcon { get; set; } = true;

    public bool IsTag { get; set; }

    public bool IsFilter { get; set; }

    public bool IsProject { get; set; }

    public Geometry IconData { get; set; }

    public long SortOrder { get; set; }

    public string Name { get; set; }

    public string Emoji { get; set; } = string.Empty;

    public Pinyin Pinyin { get; set; }

    public bool IsInbox { get; set; }

    public override sealed string Id { get; set; }

    public override bool CanSelect { get; set; } = true;

    public long? ParentSortOrder { get; set; }

    public bool TeamProject { get; set; }

    public SearchTagAndProjectModel(ProjectModel project)
    {
      this.IsProject = true;
      string emojiIcon = EmojiHelper.GetEmojiIcon(project.name);
      if (!string.IsNullOrEmpty(emojiIcon) && project.name.StartsWith(emojiIcon))
      {
        this.IsIcon = false;
        this.Name = project.name.Remove(0, emojiIcon.Length);
        this.Emoji = emojiIcon;
      }
      else
      {
        this.IconData = project.IsNote ? Utils.GetIconData(project.IsShareList() ? "IcShareNoteProject" : "IcNoteProject") : Utils.GetIconData(project.IsShareList() ? "IcSharedProject" : "IcNormalProject");
        this.Name = project.name;
        this.Emoji = string.Empty;
      }
      this.SortOrder = project.sortOrder;
      this.Id = project.id;
      this.Pinyin = PinyinUtils.ToPinyin(project.name);
      this.IsInbox = project.Isinbox;
      this.TeamProject = !string.IsNullOrEmpty(project.teamId);
    }

    public void UpdateGroupSortOrder()
    {
      if (!this.IsProject)
        return;
      ProjectModel projectById = CacheManager.GetProjectById(this.Id);
      if (projectById == null)
        return;
      ProjectGroupModel groupById = CacheManager.GetGroupById(projectById.groupId);
      if (groupById != null)
        this.ParentSortOrder = groupById.sortOrder;
      else
        this.ParentSortOrder = new long?();
    }

    public SearchTagAndProjectModel(FilterModel filter)
    {
      this.IsFilter = true;
      string emojiIcon = EmojiHelper.GetEmojiIcon(filter.name);
      if (!string.IsNullOrEmpty(emojiIcon) && filter.name.StartsWith(emojiIcon))
      {
        this.IsIcon = false;
        this.Name = filter.name.Remove(0, emojiIcon.Length);
        this.Emoji = emojiIcon;
      }
      else
      {
        this.IconData = Utils.GetIconData("IcFilterProject");
        this.Name = filter.name;
        this.Emoji = string.Empty;
      }
      this.SortOrder = filter.sortOrder;
      this.Id = filter.id;
      this.Pinyin = PinyinUtils.ToPinyin(filter.name);
    }

    public SearchTagAndProjectModel(TagModel tag)
    {
      this.IsTag = true;
      this.IconData = Utils.GetIconData("IcTagLine");
      this.Name = tag.GetDisplayName();
      this.SortOrder = tag.sortOrder;
      this.Id = tag.name;
      this.Pinyin = PinyinUtils.ToPinyin(tag.name);
    }

    public bool Contains(string text) => this.Pinyin.Contains(text);
  }
}
