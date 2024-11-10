// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.EmptySubViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class EmptySubViewModel : ProjectItemViewModel
  {
    public readonly ProjectGroupModel ProjectGroup;
    public readonly bool IsEmptyTag;
    public readonly string ParentId;

    public EmptySubViewModel(ProjectGroupModel group)
    {
      this.Id = group.id + "empty";
      this.ProjectGroup = group;
      this.ParentId = group.id;
      this.TeamId = group.teamId;
      this.IsEmptySubItem = true;
    }

    public EmptySubViewModel(TagModel tag)
    {
      this.Id = tag.id + "empty";
      this.IsEmptyTag = true;
      this.ParentId = tag.name;
      this.IsEmptySubItem = true;
    }
  }
}
