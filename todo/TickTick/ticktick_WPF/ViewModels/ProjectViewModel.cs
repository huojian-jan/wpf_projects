// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ProjectViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  [Serializable]
  public class ProjectViewModel : BaseViewModel
  {
    public ProjectViewModel()
    {
    }

    public ProjectViewModel(ProjectModel project)
    {
      this.originalProject = project;
      this._Id = project._Id;
      this.id = project.id;
      this.color = project.color != "transparent" ? project.color : "#00ffffff";
      this.name = project.name;
      this.inAll = project.inAll;
      this.Muted = project.muted;
      this.ViewMode = project.viewMode;
      this.groupId = project.groupId;
      this.num = 0;
      this.sortOrder = project.sortOrder;
      this.userCount = project.userCount == 0 ? 1 : project.userCount;
      this.sortType = project.sortType;
      this.closed = project.closed;
      this.sync_status = project.sync_status;
      this._teamId = project.teamId;
      this.Kind = project.kind;
      this.needAudit = ((int) project.needAudit ?? 1) != 0;
      this.openToTeam = project.openToTeam;
      this.teamMemberPermission = project.teamMemberPermission;
      this.SortOption = project.GetSortOption();
    }

    public ProjectModel originalProject { get; set; }

    public int _Id { get; set; }

    public string id { get; set; }

    public string groupId { get; set; }

    private string color { get; set; }

    private int colorindex { get; set; }

    private string name { get; set; }

    private bool inAll { get; set; }

    public bool? closed { get; set; }

    private int num { get; set; }

    public long sortOrder { get; set; }

    public int userCount { get; set; } = 1;

    public string sortType { get; set; }

    public string sync_status { get; set; }

    private string _teamId { get; set; }

    private bool _muted { get; set; } = true;

    public string Kind { get; set; }

    public bool IsNew { get; set; }

    public bool needAudit { get; set; } = true;

    public bool? openToTeam { get; set; }

    public string teamMemberPermission { get; set; }

    public SortOption SortOption { get; set; }

    public string Color
    {
      get => this.color ?? "transparent";
      set
      {
        this.color = value;
        this.OnPropertyChanged(nameof (Color));
      }
    }

    public string Name
    {
      get => this.name;
      set
      {
        this.name = value;
        this.OnPropertyChanged(nameof (Name));
      }
    }

    public bool InAll
    {
      get => this.inAll;
      set
      {
        this.inAll = value;
        this.OnPropertyChanged(nameof (InAll));
      }
    }

    public bool Muted
    {
      get => this._muted;
      set
      {
        this._muted = value;
        this.OnPropertyChanged("Disturb");
      }
    }

    public int Num
    {
      get => this.num;
      set
      {
        this.num = value;
        this.OnPropertyChanged(nameof (Num));
      }
    }

    public string TeamId
    {
      get => this._teamId;
      set
      {
        this._teamId = value == "" ? (string) null : value;
        this.OnPropertyChanged(nameof (TeamId));
      }
    }

    public string ViewMode { get; set; } = "list";

    public void SetName(string newName) => this.name = newName;

    public bool IsShareList()
    {
      if (this.userCount > 1)
        return true;
      return !string.IsNullOrEmpty(this._teamId) && this.openToTeam.GetValueOrDefault();
    }
  }
}
