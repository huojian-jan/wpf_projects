// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.TeamGroupViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using ticktick_WPF.Models;
using ticktick_WPF.Resource;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class TeamGroupViewModel : PtfAllViewModel
  {
    public readonly TeamModel Team;

    public TeamGroupViewModel(TeamModel team)
      : base(PtfType.Project)
    {
      this.Id = team.id;
      this.Team = team;
      this.Title = team.name;
      this.Open = team.open;
      this.TeamId = team.id;
      this.Expired = team.expired;
    }
  }
}
