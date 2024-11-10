// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.ShareContactsViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Media.Imaging;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.CustomControl;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class ShareContactsViewModel : ComboBoxViewModel
  {
    private int _siteId;

    public ShareContactsViewModel(string addEmail)
    {
      this.IsNewAdd = true;
      this.displayName = string.Format(Utils.GetString("InviteUserJoinString"), addEmail.Length > 30 ? (object) (addEmail.Substring(0, 30) + "...") : (object) addEmail);
      this.email = addEmail;
    }

    public ShareContactsViewModel(ShareContactsModel shareContactsModel)
    {
      this.toUserId = shareContactsModel.toUserId;
      this.email = shareContactsModel.email;
      this.userCode = shareContactsModel.userCode;
      this.freq = shareContactsModel.freq;
      this.lstTime = shareContactsModel.lstTime;
      this.avatarUrl = shareContactsModel.avatarUrl;
      this.displayName = shareContactsModel.displayName;
      this.displayEmail = shareContactsModel.displayEmail;
      this._siteId = shareContactsModel.siteId.GetValueOrDefault();
      this.CanSelect = false;
      this.SetAvatar();
    }

    public string toUserId { get; set; }

    public string email { get; set; }

    public string avatarUrl { get; set; }

    public string userCode { get; set; }

    public int freq { get; set; }

    public DateTime lstTime { get; set; }

    public string displayName { get; set; }

    public string displayEmail { get; set; }

    public bool isFeishu => this._siteId == 10 && Utils.IsDida();

    public BitmapImage avatar { get; set; }

    public bool IsNewAdd { get; set; }

    private async void SetAvatar()
    {
      ShareContactsViewModel contactsViewModel = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(contactsViewModel.avatarUrl);
      contactsViewModel.avatar = avatarByUrlAsync;
      contactsViewModel.OnPropertyChanged("avatar");
    }
  }
}
