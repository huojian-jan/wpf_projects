// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.NotificationViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Windows.Media.Imaging;
using ticktick_WPF.Models;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class NotificationViewModel : BaseViewModel
  {
    private string acceptButtonContent = Utils.GetString("Accept");

    public NotificationViewModel(NotificationModel notification)
    {
      this.id = notification.id;
      this.teamId = notification.notificationUserData.teamId;
      this.teamName = notification.notificationUserData.teamName;
      this.actionStatus = notification.actionStatus;
      this.type = notification.type;
      this.projectid = notification.notificationUserData.entityId;
      this.CreatedTime = notification.createdTime;
      this.userCode = notification.notificationUserData.userCode ?? notification.notificationUserData.fromUserCode;
      this.GetActionStatus(notification);
      this.UserDisplayName = this.userCode == null ? Utils.GetAppName() : notification.notificationUserData.userDisplayName ?? notification.notificationUserData.name ?? notification.notificationUserData.fromUserDisplayName;
      this.notification = notification;
    }

    public int actionStatus { get; set; }

    public string type { get; set; }

    public string teamId { get; set; }

    public string teamName { get; set; }

    private string userDisplayName { get; set; }

    private string title { get; set; }

    private string avatarUrl { get; set; }

    private DateTime createdTime { get; set; }

    private string actionText { get; set; }

    private string actionVisibility { get; set; }

    private string buttonVisibility { get; set; }

    private string acceptButtonVisibility { get; set; }

    private string declineButtonVisibility { get; set; }

    private string isButtonEnable { get; set; }

    public string userCode { get; set; }

    public string projectid { get; set; }

    public string id { get; set; }

    public string url { get; set; }

    public NotificationModel notification { get; set; }

    public string UserDisplayName
    {
      get => this.userDisplayName;
      set
      {
        this.userDisplayName = value;
        this.OnPropertyChanged(nameof (UserDisplayName));
      }
    }

    public string Title
    {
      get => this.title;
      set
      {
        this.title = value;
        this.OnPropertyChanged(nameof (Title));
      }
    }

    public string AvatarUrl
    {
      get
      {
        return !string.IsNullOrEmpty(this.avatarUrl) && !(this.avatarUrl == "-1") ? this.avatarUrl : "avatar-new.png";
      }
      set
      {
        this.avatarUrl = value;
        this.SetAvatar();
      }
    }

    public BitmapImage Avatar { get; set; }

    private async void SetAvatar()
    {
      NotificationViewModel notificationViewModel = this;
      BitmapImage avatarByUrlAsync = await AvatarHelper.GetAvatarByUrlAsync(notificationViewModel.AvatarUrl);
      notificationViewModel.Avatar = avatarByUrlAsync;
      notificationViewModel.OnPropertyChanged("Avatar");
    }

    public DateTime CreatedTime
    {
      get => this.createdTime;
      set
      {
        this.createdTime = value;
        this.OnPropertyChanged(nameof (CreatedTime));
      }
    }

    public string ActionText
    {
      get => this.actionText;
      set
      {
        this.actionText = value;
        this.OnPropertyChanged(nameof (ActionText));
      }
    }

    public string ActionVisibility
    {
      get => this.actionVisibility == null ? "Collapsed" : this.actionVisibility;
      set
      {
        this.actionVisibility = value;
        this.OnPropertyChanged(nameof (ActionVisibility));
      }
    }

    public string ButtonVisibility
    {
      get => this.buttonVisibility == null ? "Collapsed" : this.buttonVisibility;
      set
      {
        this.buttonVisibility = value;
        this.OnPropertyChanged(nameof (ButtonVisibility));
      }
    }

    public string AcceptButtonVisibility
    {
      get => this.acceptButtonVisibility == null ? "Collapsed" : this.acceptButtonVisibility;
      set
      {
        this.acceptButtonVisibility = value;
        this.OnPropertyChanged(nameof (AcceptButtonVisibility));
      }
    }

    public string DeclineButtonVisibility
    {
      get => this.declineButtonVisibility == null ? "Collapsed" : this.declineButtonVisibility;
      set
      {
        this.declineButtonVisibility = value;
        this.OnPropertyChanged(nameof (DeclineButtonVisibility));
      }
    }

    public string AcceptButtonContent
    {
      get => this.acceptButtonContent;
      set
      {
        this.acceptButtonContent = value;
        this.OnPropertyChanged(nameof (AcceptButtonContent));
      }
    }

    public string IsButtonEnable
    {
      get => this.isButtonEnable == null ? "True" : this.isButtonEnable;
      set
      {
        this.isButtonEnable = value;
        this.OnPropertyChanged(nameof (IsButtonEnable));
      }
    }

    public string Url
    {
      get => this.url;
      set
      {
        this.url = value;
        this.OnPropertyChanged(nameof (Url));
      }
    }

    private void GetActionStatus(NotificationModel notification)
    {
      switch (notification.type)
      {
        case "attend":
        case "task":
          this.SetVisible(0);
          break;
        case "team":
          switch (notification.actionStatus)
          {
            case 1:
            case 20:
            case 21:
              this.SetVisible(1);
              return;
            case 15:
            case 22:
              this.SetVisible(2);
              return;
            case 16:
            case 23:
              this.SetVisible(3);
              return;
            default:
              this.SetVisible(0);
              return;
          }
        case "projectPermission":
          switch (notification.actionStatus)
          {
            case 1:
              this.SetVisible(1);
              this.AcceptButtonContent = Utils.GetString("Agree");
              return;
            case 4:
              this.SetVisible(2);
              return;
            case 5:
              this.SetVisible(3);
              return;
            default:
              this.SetVisible(0);
              return;
          }
        default:
          switch (notification.actionStatus)
          {
            case 0:
            case 9:
              if (notification.type == "share")
              {
                this.SetVisible(1);
                return;
              }
              if (!(notification.type == "PayReminder"))
                return;
              this.actionVisibility = "Collapsed";
              this.ButtonVisibility = "Visible";
              this.AcceptButtonVisibility = "Visible";
              this.DeclineButtonVisibility = "Collapsed";
              if (!notification.notificationUserData.expiryDate.HasValue || notification.notificationUserData.expiryDate.Value.Year == 1)
              {
                this.AcceptButtonContent = Utils.GetString("UpdateNow");
                return;
              }
              this.AcceptButtonContent = Utils.GetString("RenewNow");
              return;
            case 1:
            case 10:
              this.SetVisible(2);
              return;
            case 2:
            case 11:
              this.SetVisible(3);
              return;
            case 3:
              this.SetVisible(4);
              return;
            default:
              this.SetVisible(0);
              return;
          }
      }
    }

    private void SetVisible(int num)
    {
      switch (num)
      {
        case 0:
          this.actionVisibility = "Collapsed";
          this.ButtonVisibility = "Collapsed";
          this.AcceptButtonVisibility = "Collapsed";
          this.DeclineButtonVisibility = "Collapsed";
          break;
        case 1:
          this.actionVisibility = "Collapsed";
          this.ButtonVisibility = "Visible";
          this.AcceptButtonVisibility = "Visible";
          this.DeclineButtonVisibility = "Visible";
          break;
        case 2:
          this.actionVisibility = "Visible";
          this.ButtonVisibility = "Collapsed";
          this.AcceptButtonVisibility = "Collapsed";
          this.DeclineButtonVisibility = "Collapsed";
          this.actionText = Utils.GetString("Accepted");
          break;
        case 3:
          this.actionVisibility = "Visible";
          this.ButtonVisibility = "Collapsed";
          this.AcceptButtonVisibility = "Collapsed";
          this.DeclineButtonVisibility = "Collapsed";
          this.actionText = Utils.GetString("Declined");
          break;
        case 4:
          this.actionVisibility = "Visible";
          this.ButtonVisibility = "Collapsed";
          this.AcceptButtonVisibility = "Collapsed";
          this.DeclineButtonVisibility = "Collapsed";
          this.actionText = Utils.GetString("Cancelled");
          break;
      }
    }
  }
}
