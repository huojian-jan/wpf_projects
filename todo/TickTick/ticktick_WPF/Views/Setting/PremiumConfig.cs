// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Setting.PremiumConfig
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Emoji.Wpf;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;

#nullable disable
namespace ticktick_WPF.Views.Setting
{
  public class PremiumConfig : UserControl, IComponentConnector
  {
    internal EmjTextBlock TitleText;
    internal TextBlock DescText;
    internal ItemsControl ItemsControl;
    private bool _contentLoaded;

    public PremiumConfig()
    {
      this.InitializeComponent();
      this.ItemsControl.ItemsSource = (IEnumerable) PremiumViewModel.BuildViewModels();
      this.InitData();
    }

    private void InitData() => this.InitProStatus();

    private async void InitProStatus()
    {
      UserModel userModel = await UserDao.QueryUserModelListDbByIdAsync(LocalSettings.Settings.LoginUserId);
      List<TeamModel> teams = CacheManager.GetTeams();
      TeamModel teamModel = teams != null ? teams.FirstOrDefault<TeamModel>((Func<TeamModel, bool>) (t => t.IsPro())) : (TeamModel) null;
      bool flag = teamModel != null;
      if (userModel == null)
        return;
      if (userModel.pro | flag)
      {
        DateTime? nullable = flag ? new DateTime?(teamModel.expiredDate) : userModel.proEndDate;
        this.TitleText.Text = string.Format(Utils.GetString(flag ? "UsingBusiness1" : "YouArePremium"), (object) Utils.GetAppName());
        this.DescText.Text = string.Format(Utils.GetString("ExpiresOn"), (object) nullable?.ToString("yyyy-MM-dd"));
      }
      else
      {
        this.TitleText.Text = Utils.GetString("DoubleYourAchievement");
        this.DescText.Text = string.Format(Utils.GetString("WithPremium"), (object) Utils.GetAppName());
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/setting/premiumconfig.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.TitleText = (EmjTextBlock) target;
          break;
        case 2:
          this.DescText = (TextBlock) target;
          break;
        case 3:
          this.ItemsControl = (ItemsControl) target;
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
