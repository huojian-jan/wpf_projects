// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Config.AccountInfo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Config
{
  public class AccountInfo : UserControl, IComponentConnector
  {
    private readonly SettingDialog _parent;
    internal ImageBrush HeadimgImageBrush;
    internal TextBlock email;
    internal TextBlock NextBillingDateText;
    internal Button RenewButton;
    internal Button LogoutButton;
    private bool _contentLoaded;

    public AccountInfo(SettingDialog settingDialog)
    {
      this.InitializeComponent();
      this.InitData();
      this._parent = settingDialog;
    }

    private void InitData()
    {
      this.InitProStatus();
      this.CheckLoginStatus();
    }

    private async void InitProStatus()
    {
      UserModel userModel = await UserDao.QueryUserModelListDbByIdAsync(LocalSettings.Settings.LoginUserId);
      List<TeamModel> teams = CacheManager.GetTeams();
      TeamModel teamModel = teams != null ? teams.FirstOrDefault<TeamModel>((Func<TeamModel, bool>) (t => t.IsPro())) : (TeamModel) null;
      bool flag = teamModel != null;
      if (userModel != null)
      {
        if (userModel.pro | flag)
        {
          DateTime? nullable = flag ? new DateTime?(teamModel.expiredDate) : userModel.proEndDate;
          this.NextBillingDateText.Visibility = Visibility.Visible;
          this.NextBillingDateText.Text = string.Format(Utils.GetString(flag ? "UsingBusiness" : "NextBillingDate"), (object) nullable?.ToString("yyyy-MM-dd"));
        }
        else
          this.NextBillingDateText.Visibility = Visibility.Collapsed;
      }
      if (!flag)
        return;
      this.RenewButton.Visibility = Visibility.Collapsed;
    }

    private async void CheckLoginStatus()
    {
      AccountInfo accountInfo = this;
      UserInfoModel userInfo = await UserManager.GetUserInfo();
      if (userInfo != null)
      {
        ImageBrush imageBrush = accountInfo.HeadimgImageBrush;
        imageBrush.ImageSource = (ImageSource) await AvatarHelper.GetAvatarByUrlAsync(string.IsNullOrEmpty(userInfo.picture) ? "avatar-new.png" : userInfo.picture, 256);
        imageBrush = (ImageBrush) null;
        if (!userInfo.fakedEmail)
          userInfo.displayEmail = userInfo.username;
        if (userInfo.displayEmail == null)
          accountInfo.email.Visibility = Visibility.Collapsed;
        if (userInfo.fakedEmail && string.IsNullOrEmpty(userInfo.name))
        {
          UserModel userModelByName = await UserDao.GetUserModelByName(userInfo.username);
          if (userModelByName != null)
            userInfo.name = Utils.GetMaskPhone(userModelByName.phone);
        }
        if (string.IsNullOrEmpty(userInfo.displayEmail) && !string.IsNullOrEmpty(userInfo.name))
        {
          UserModel userModelByName = await UserDao.GetUserModelByName(userInfo.username);
          if (userModelByName != null)
          {
            userInfo.displayEmail = Utils.GetMaskPhone(userModelByName.phone);
            if (!string.IsNullOrEmpty(userInfo.displayEmail))
            {
              accountInfo.email.Text = Utils.GetString("PhoneNumber");
              accountInfo.email.Visibility = Visibility.Visible;
            }
          }
        }
        accountInfo.DataContext = (object) userInfo;
        userInfo = (UserInfoModel) null;
      }
      else
      {
        accountInfo.email.Visibility = Visibility.Collapsed;
        accountInfo.HeadimgImageBrush.ImageSource = (ImageSource) new BitmapImage(new Uri("pack://application:,,,/Assets/avatar-new.png"));
        userInfo = (UserInfoModel) null;
      }
    }

    private void OnRenewClick(object sender, RoutedEventArgs e) => Utils.StartUpgrade("settings");

    private void OnLogoutClick(object sender, RoutedEventArgs e)
    {
      this._parent.Close();
      this.LogoutButton.IsEnabled = false;
      App.Window?.Logout();
    }

    private async void GoToWebSetting(object sender, RoutedEventArgs e)
    {
      Utils.TryProcessStartUrlWithToken("/webapp/#settings/profile");
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/config/accountinfo.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.HeadimgImageBrush = (ImageBrush) target;
          break;
        case 2:
          this.email = (TextBlock) target;
          break;
        case 3:
          this.NextBillingDateText = (TextBlock) target;
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.GoToWebSetting);
          break;
        case 5:
          this.RenewButton = (Button) target;
          this.RenewButton.Click += new RoutedEventHandler(this.OnRenewClick);
          break;
        case 6:
          this.LogoutButton = (Button) target;
          this.LogoutButton.Click += new RoutedEventHandler(this.OnLogoutClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
