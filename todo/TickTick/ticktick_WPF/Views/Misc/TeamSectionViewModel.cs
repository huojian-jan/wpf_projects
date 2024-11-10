// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.TeamSectionViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class TeamSectionViewModel : SelectableItemViewModel
  {
    public TeamSectionViewModel(string id, string title)
    {
      this.Id = id;
      this.Title = title;
      this.Type = "team";
      this.ShowIcon = false;
      this.Selectable = false;
      this.IsSectionGroup = true;
      this.ShowIndent = true;
      this.Open = true;
      this.TeamId = id;
      this.IsTeam = true;
      this.IsParent = true;
    }
  }
}
