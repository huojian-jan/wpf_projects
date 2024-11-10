// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Team.InviteUserModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Team
{
  public class InviteUserModel : BaseViewModel
  {
    private string _id;
    private string _userCode;
    private string _avatarUrl;
    private string _userName;
    private string _email;
    private DateTime _lstTime;
    private bool _inGroup;
    private bool _selected;
    private bool _enable = true;
    private int _siteId;

    public string Id { get; set; }

    public string UserCode { get; set; }

    public string AvatarUrl { get; set; }

    public string UserName { get; set; }

    public string Email { get; set; }

    public DateTime LstTime { get; set; }

    public bool InGroup { get; set; }

    public bool CanDelete { get; set; }

    public BitmapImage Avatar { get; set; }

    public async void SetAvatar()
    {
      InviteUserModel inviteUserModel = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(inviteUserModel.AvatarUrl);
      inviteUserModel.Avatar = avatarByUrlAsync;
      inviteUserModel.OnPropertyChanged("Avatar");
    }

    public bool IsFeiShu => this._siteId == 10 && Utils.IsDida();

    public bool Selected
    {
      get => this._selected;
      set
      {
        this._selected = value;
        this.OnPropertyChanged(nameof (Selected));
      }
    }

    private InviteUserModel()
    {
    }

    public InviteUserModel(ShareContactsModel model)
    {
      this.Id = model.toUserId;
      this.UserCode = model.userCode;
      this.AvatarUrl = model?.avatarUrl ?? "";
      this.Email = model.email;
      this.LstTime = model.lstTime;
      this.InGroup = false;
      this.Selected = false;
      this._siteId = model.siteId.GetValueOrDefault();
      this.SetAvatar();
    }

    public InviteUserModel(TeamContactsModel model)
    {
      this.UserCode = model.userCode;
      this.Email = model.email;
      this.LstTime = model.lstTime;
      this.InGroup = false;
      this.Selected = false;
    }

    private InviteUserModel(ShareUserModel userModel)
    {
      this.Id = userModel.userId.ToString();
      this.UserCode = userModel.userCode;
      this.AvatarUrl = userModel.avatarUrl;
      this.UserName = string.IsNullOrEmpty(userModel.displayName) ? userModel.username : userModel.displayName;
      this.Email = userModel.username;
      this.LstTime = userModel.createdTime ?? DateTime.Now;
      this.InGroup = true;
      this.Selected = false;
      this.SetAvatar();
    }

    public InviteUserModel(TeamMember userMode)
    {
      this.UserCode = userMode.userCode;
      this.AvatarUrl = userMode.avatarUrl;
      this.UserName = userMode.displayName;
      this.Email = userMode.email;
      this.InGroup = false;
      this.Selected = false;
      this.SetAvatar();
    }

    public static List<InviteUserModel> Build(
      List<ShareUserModel> shareUsers,
      List<string> shareUserCodeList)
    {
      List<InviteUserModel> inviteUserModelList = new List<InviteUserModel>();
      foreach (ShareUserModel shareUser in shareUsers)
      {
        if (!shareUserCodeList.Contains(shareUser.userCode))
          inviteUserModelList.Add(new InviteUserModel(shareUser));
      }
      return inviteUserModelList;
    }
  }
}
