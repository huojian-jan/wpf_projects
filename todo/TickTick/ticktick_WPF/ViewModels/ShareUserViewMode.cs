// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ShareUserViewMode
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class ShareUserViewMode : BaseViewModel
  {
    private string _avatarUrl;
    public int siteId;
    private static string _teamName;

    public ShareUserViewMode()
    {
    }

    public ShareUserViewMode(ShareAddUserMode shareAddUserMode)
    {
      this.recordId = shareAddUserMode.recordId;
      this.userId = shareAddUserMode.toUserId.HasValue ? (long) shareAddUserMode.toUserId.Value : -1L;
      this.avatarUrl = "../Assets/avatar-new.png";
      this.username = Utils.IsFakeEmail(shareAddUserMode.toUsername) ? string.Empty : shareAddUserMode.toUsername;
      this.userCode = shareAddUserMode.toUserCode;
      this.isOwner = false;
      this.isProjectShare = true;
      this.isAccept = false;
      this.acceptStatus = 0;
      this.permission = shareAddUserMode.permission;
    }

    public ShareUserViewMode(ShareUserModel shareUserModel)
    {
      this.recordId = shareUserModel.recordId;
      this.userId = shareUserModel.userId.HasValue ? (long) (int) shareUserModel.userId.Value : -1L;
      this.avatarUrl = shareUserModel.avatarUrl == null || shareUserModel.avatarUrl.Contains("avatar-new") ? "../Assets/avatar-new.png" : shareUserModel.avatarUrl;
      this.username = Utils.IsFakeEmail(shareUserModel.username) ? string.Empty : shareUserModel.username;
      this.displayName = shareUserModel.displayName;
      this.isOwner = shareUserModel.isOwner;
      this.isProjectShare = shareUserModel.isProjectShare;
      this.isAccept = shareUserModel.isOwner || shareUserModel.isAccept || this.userId.ToString() == LocalSettings.Settings.LoginUserId;
      this.createdTime = shareUserModel.createdTime ?? DateTime.Now;
      this.userCode = shareUserModel.userCode;
      this.acceptStatus = shareUserModel.acceptStatus;
      this.deleted = shareUserModel.deleted;
      this.permission = shareUserModel.permission;
      this.siteId = shareUserModel.siteId.GetValueOrDefault();
      this.visitor = shareUserModel.visitor;
    }

    public string avatarUrl
    {
      get => this._avatarUrl;
      set
      {
        this._avatarUrl = value;
        this.SetAvatar();
      }
    }

    public string recordId { get; set; }

    public long userId { get; set; }

    public string username { get; set; }

    public string displayName { get; set; }

    public bool isOwner { get; set; }

    public bool isProjectShare { get; set; }

    public bool isAccept { get; set; }

    public DateTime createdTime { get; set; }

    public string userCode { get; set; }

    public int acceptStatus { get; set; }

    public bool deleted { get; set; }

    public bool editable { get; set; }

    public bool isTeam { get; set; }

    public bool Enable { get; set; } = true;

    public bool openToTeam { get; set; }

    public bool noMembers { get; set; }

    public string permission { get; set; }

    public int userCount { get; set; }

    public bool isFeishu
    {
      get
      {
        return this.userId.ToString() != LocalSettings.Settings.LoginUserId && this.siteId == 10 && Utils.IsDida();
      }
    }

    public ImageSource avatar { get; set; }

    public bool visitor { get; set; }

    private async void SetAvatar()
    {
      ShareUserViewMode shareUserViewMode = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(shareUserViewMode.avatarUrl);
      shareUserViewMode.avatar = (ImageSource) avatarByUrlAsync;
      shareUserViewMode.OnPropertyChanged("avatar");
    }

    public void SetTeamPermissionText()
    {
      this.username = ShareUserViewMode.GetPermissionTextOfTeam(this.permission, ShareUserViewMode._teamName);
      this.OnPropertyChanged("username");
    }

    public static string GetPermissionTextOfTeam(string permit, string teamName)
    {
      ShareUserViewMode._teamName = teamName;
      string key = "MembersCanEdit";
      switch (permit)
      {
        case "read":
          key = "MembersCanRead";
          break;
        case "comment":
          key = "MembersCanComment";
          break;
      }
      return string.Format(Utils.GetString(key), (object) teamName);
    }

    public void SetProperty(ShareUserViewMode teamVModel)
    {
      this.avatar = teamVModel.avatar;
      this.username = teamVModel.username;
      this.permission = teamVModel.permission;
      this.displayName = teamVModel.displayName;
      this.openToTeam = teamVModel.openToTeam;
      this.Enable = teamVModel.Enable;
      this.editable = teamVModel.editable;
      this.OnPropertyChanged("editable");
      this.OnPropertyChanged("displayName");
      this.OnPropertyChanged("username");
      this.OnPropertyChanged("permission");
      this.OnPropertyChanged("avatar");
    }
  }
}
