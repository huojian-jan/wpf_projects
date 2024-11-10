// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Agenda.UserProfileViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System.Windows.Media.Imaging;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;

#nullable disable
namespace ticktick_WPF.Views.Agenda
{
  public class UserProfileViewModel : BaseViewModel
  {
    public string AvatarUrl { get; set; }

    public BitmapImage Avatar { get; set; }

    public string DisplayName { get; set; }

    public string UserCode { get; set; }

    public string Email { get; set; }

    public bool IsOwner { get; set; }

    public bool IsMySelf { get; }

    public bool ShowDelete { get; set; }

    public UserProfileViewModel(ProfileModel model)
    {
      this.AvatarUrl = model.avatarUrl;
      this.DisplayName = UserProfileViewModel.GetDisplayName(model.name, model.username, model.isMyself);
      this.Email = model.email;
      this.UserCode = model.userCode;
      this.IsMySelf = model.isMyself;
      this.SetAvatar();
    }

    public UserProfileViewModel(UserInfoModel user, bool isMyself)
    {
      this.AvatarUrl = user.picture;
      this.DisplayName = UserProfileViewModel.GetDisplayName(user.name, user.username, isMyself);
      this.Email = user.username;
      this.UserCode = user.userCode;
      this.IsMySelf = isMyself;
      this.SetAvatar();
    }

    public UserProfileViewModel(AttendeeModel model)
    {
      this.AvatarUrl = model.user.avatarUrl;
      this.DisplayName = UserProfileViewModel.GetDisplayName(model.user.name, model.user.username, model.user.isMyself);
      this.Email = model.user.email;
      this.UserCode = model.user.userCode;
      this.IsMySelf = model.user.isMyself;
      this.SetAvatar();
    }

    private async void SetAvatar()
    {
      UserProfileViewModel profileViewModel = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(profileViewModel.AvatarUrl);
      profileViewModel.Avatar = avatarByUrlAsync;
      profileViewModel.OnPropertyChanged("Avatar");
    }

    private static string GetDisplayName(string name, string userName, bool isMyself)
    {
      if (isMyself)
        return Utils.GetString("Me");
      return !string.IsNullOrEmpty(name) ? name : userName;
    }
  }
}
