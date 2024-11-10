// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Team.InviteGroupModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Team
{
  public class InviteGroupModel : BaseViewModel
  {
    private string _id;
    private string _projectName;
    private List<InviteUserModel> _users;
    private bool _opened;
    private int _selectNum;
    private bool _selectAll;

    public string Id { get; set; }

    public string ProjectName { get; set; }

    public List<InviteUserModel> Users
    {
      get => this._users;
      set
      {
        this._users = value;
        this.OnPropertyChanged(nameof (Users));
      }
    }

    public bool Opened
    {
      get => this._opened;
      set
      {
        this._opened = value;
        this.OnPropertyChanged(nameof (Opened));
      }
    }

    public int SelectNum
    {
      get => this._selectNum;
      set
      {
        this._selectNum = value;
        this.SelectAll = this._selectNum == this._users.Count;
      }
    }

    public bool SelectAll
    {
      get => this._selectAll;
      set
      {
        this._selectAll = value;
        this.OnPropertyChanged(nameof (SelectAll));
      }
    }

    public static async Task<InviteGroupModel> Build(
      ProjectUsersMode mode,
      List<string> shareUserCodeList)
    {
      string currentUser = LocalSettings.Settings.LoginUserId;
      ProjectModel projectModel = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == mode.projectId));
      List<InviteUserModel> inviteUserModelList = InviteUserModel.Build(mode.shareUsers.Where<ShareUserModel>((Func<ShareUserModel, bool>) (model => model.userId.ToString() != currentUser)).ToList<ShareUserModel>(), shareUserCodeList);
      if (inviteUserModelList.Count == 0)
        return (InviteGroupModel) null;
      return new InviteGroupModel()
      {
        Id = mode.projectId,
        ProjectName = projectModel?.name,
        Opened = false,
        Users = inviteUserModelList
      };
    }
  }
}
