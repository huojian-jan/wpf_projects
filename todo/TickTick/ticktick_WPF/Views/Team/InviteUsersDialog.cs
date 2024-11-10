// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Team.InviteUsersDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using ticktick_WPF.Views.Misc;

#nullable disable
namespace ticktick_WPF.Views.Team
{
  public class InviteUsersDialog : UserControl, IComponentConnector
  {
    private readonly bool _isTeam;
    private readonly ObservableCollection<InviteGroupModel> _projectUsersDatas = new ObservableCollection<InviteGroupModel>();
    private readonly List<InviteUserModel> _recentInviteModels;
    internal GroupTitle Titles;
    internal ItemsControl RecentInvite;
    internal ItemsControl SharedProject;
    internal ItemsControl TeamMembers;
    internal StackPanel EmptyPage;
    internal TextBlock EmptyText1;
    internal TextBlock EmptyText2;
    internal Button InviteButton;
    private bool _contentLoaded;

    public InviteUsersDialog(
      List<InviteUserModel> recentInviteModels,
      List<ProjectUsersMode> projectUsersDatas,
      ICollection<string> existUsers)
    {
      this._isTeam = false;
      this.InitializeComponent();
      this.Height = Utils.IsCn() ? 316.0 : 326.0;
      if (recentInviteModels != null)
      {
        recentInviteModels = recentInviteModels.Where<InviteUserModel>((Func<InviteUserModel, bool>) (model => !existUsers.Contains(model.UserCode))).ToList<InviteUserModel>();
        this._recentInviteModels = recentInviteModels.ToList<InviteUserModel>();
        this._recentInviteModels.ForEach((Action<InviteUserModel>) (i => i.CanDelete = true));
        if (recentInviteModels.Count<InviteUserModel>() > 10)
          recentInviteModels.RemoveRange(10, recentInviteModels.Count<InviteUserModel>() - 10);
        this.RecentInvite.ItemsSource = (IEnumerable) recentInviteModels;
      }
      this.SetSharedProjectSource(projectUsersDatas, existUsers);
      this.EmptyPage.Visibility = this.RecentInvite.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
      this.Titles.SetMaxWidth(110.0);
    }

    private async void SetSharedProjectSource(
      List<ProjectUsersMode> projectUsersDatas,
      ICollection<string> existUsers)
    {
      if (projectUsersDatas == null)
        ;
      else
      {
        foreach (ProjectUsersMode projectUsersData in projectUsersDatas)
        {
          InviteGroupModel inviteGroupModel = await InviteGroupModel.Build(projectUsersData, existUsers.ToList<string>());
          if (inviteGroupModel != null)
          {
            inviteGroupModel.Users = inviteGroupModel.Users.Where<InviteUserModel>((Func<InviteUserModel, bool>) (user => !existUsers.Contains(user.UserCode))).ToList<InviteUserModel>();
            this._projectUsersDatas.Add(inviteGroupModel);
          }
        }
        this.SharedProject.ItemsSource = (IEnumerable) this._projectUsersDatas;
      }
    }

    public InviteUsersDialog(
      IEnumerable<InviteUserModel> recentInviteModels,
      IEnumerable<InviteUserModel> teamMemberModels,
      ICollection<string> existUsers)
    {
      this._isTeam = true;
      this.InitializeComponent();
      this.Height = Utils.IsCn() ? 316.0 : 346.0;
      if (recentInviteModels != null)
      {
        recentInviteModels = (IEnumerable<InviteUserModel>) recentInviteModels.Where<InviteUserModel>((Func<InviteUserModel, bool>) (model => !existUsers.Contains(model.UserCode))).ToList<InviteUserModel>();
        this._recentInviteModels = recentInviteModels.ToList<InviteUserModel>();
        this._recentInviteModels.ForEach((Action<InviteUserModel>) (i => i.CanDelete = true));
        if (recentInviteModels.Count<InviteUserModel>() > 10)
          recentInviteModels.ToList<InviteUserModel>().RemoveRange(10, recentInviteModels.Count<InviteUserModel>() - 10);
        this.RecentInvite.ItemsSource = (IEnumerable) recentInviteModels;
      }
      this.TeamMembers.ItemsSource = teamMemberModels != null ? (IEnumerable) teamMemberModels.Where<InviteUserModel>((Func<InviteUserModel, bool>) (model => !existUsers.Contains(model.UserCode))).ToList<InviteUserModel>() : (IEnumerable) null;
      this.EmptyPage.Visibility = this.RecentInvite.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
      this.Titles.Titles = "InviteRecently|TeamMembers";
      this.Titles.SetMaxWidth(110.0);
    }

    public event EventHandler OnCancel;

    public event EventHandler OnSwitch;

    public event EventHandler<List<(string, string)>> OnInvited;

    public void OnProjectItemSelected(List<string> userCodes, bool selected, bool clickAll = true)
    {
      foreach (InviteGroupModel projectUsersData in (Collection<InviteGroupModel>) this._projectUsersDatas)
      {
        InviteGroupModel data = projectUsersData;
        data.Users.ForEach((Action<InviteUserModel>) (user =>
        {
          if (!userCodes.Contains(user.UserCode))
            return;
          user.Selected = selected;
          if (clickAll)
            return;
          data.SelectNum += selected ? 1 : -1;
        }));
        if (clickAll)
          data.SelectNum = data.Users.Where<InviteUserModel>((Func<InviteUserModel, bool>) (user => user.Selected)).ToList<InviteUserModel>().Count;
      }
      this.InviteButton.IsEnabled = selected || this._projectUsersDatas.Any<InviteGroupModel>((Func<InviteGroupModel, bool>) (data => data.SelectNum > 0));
    }

    public void CheckInviteEnable(bool modelSelected)
    {
      List<InviteUserModel> source = new List<InviteUserModel>();
      if (this.RecentInvite.IsVisible)
        source = this.RecentInvite.ItemsSource as List<InviteUserModel>;
      if (this.TeamMembers.IsVisible)
        source = this.TeamMembers.ItemsSource as List<InviteUserModel>;
      this.InviteButton.IsEnabled = modelSelected || source.Any<InviteUserModel>((Func<InviteUserModel, bool>) (mode => mode.Selected));
    }

    private void OnInviteClick(object sender, RoutedEventArgs e)
    {
      List<(string, string)> valueTupleList = new List<(string, string)>();
      if (this.RecentInvite.IsVisible)
      {
        if (this.RecentInvite.ItemsSource is List<InviteUserModel> itemsSource1)
          valueTupleList = itemsSource1.Where<InviteUserModel>((Func<InviteUserModel, bool>) (user => user.Selected)).Select<InviteUserModel, (string, string)>((Func<InviteUserModel, (string, string)>) (user => (user.UserName, user.Email))).ToList<(string, string)>();
      }
      else if (this._isTeam)
      {
        if (this.TeamMembers.ItemsSource is List<InviteUserModel> itemsSource2)
          valueTupleList = itemsSource2.Where<InviteUserModel>((Func<InviteUserModel, bool>) (user => user.Selected)).Select<InviteUserModel, (string, string)>((Func<InviteUserModel, (string, string)>) (user => (user.UserName, user.Email))).ToList<(string, string)>();
      }
      else
      {
        foreach (InviteGroupModel projectUsersData in (Collection<InviteGroupModel>) this._projectUsersDatas)
        {
          foreach (InviteUserModel user1 in projectUsersData.Users)
          {
            InviteUserModel user = user1;
            if (user.Selected && valueTupleList.All<(string, string)>((Func<(string, string), bool>) (u => u.Item2 != user.Email)))
              valueTupleList.Add((user.UserName, user.Email));
          }
        }
      }
      EventHandler<List<(string, string)>> onInvited = this.OnInvited;
      if (onInvited == null)
        return;
      onInvited((object) this, valueTupleList);
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
      EventHandler onCancel = this.OnCancel;
      if (onCancel == null)
        return;
      onCancel((object) this, (EventArgs) null);
    }

    private void OnRecentShow()
    {
      if (!this.RecentInvite.IsVisible)
      {
        if (this.RecentInvite.ItemsSource is List<InviteUserModel> itemsSource)
          itemsSource.ForEach((Action<InviteUserModel>) (data => data.Selected = false));
        this.RecentInvite.Visibility = Visibility.Visible;
        this.TeamMembers.Visibility = Visibility.Collapsed;
        this.SharedProject.Visibility = Visibility.Collapsed;
        this.EmptyPage.Visibility = this.RecentInvite.Items.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
      }
      this.EmptyText1.Text = Utils.GetString("ShareEmpty1");
      this.EmptyText2.Text = Utils.GetString("ShareEmpty2");
      this.InviteButton.IsEnabled = false;
    }

    private void OnSharedOrTeamShow()
    {
      if (this.RecentInvite.IsVisible)
      {
        if (this._isTeam && this.TeamMembers.ItemsSource is List<InviteUserModel> itemsSource)
        {
          itemsSource.ForEach((Action<InviteUserModel>) (data => data.Selected = false));
        }
        else
        {
          EventHandler onSwitch = this.OnSwitch;
          if (onSwitch != null)
            onSwitch((object) null, (EventArgs) null);
        }
        this.RecentInvite.Visibility = Visibility.Collapsed;
        this.TeamMembers.Visibility = this._isTeam ? Visibility.Visible : Visibility.Collapsed;
        this.SharedProject.Visibility = this._isTeam ? Visibility.Collapsed : Visibility.Visible;
        this.EmptyPage.Visibility = this.SharedProject.Items.Count != 0 || this.TeamMembers.Items.Count != 0 ? Visibility.Collapsed : Visibility.Visible;
        this.EmptyText1.Text = this._isTeam ? Utils.GetString("InviteMemberEmpty1") : Utils.GetString("InviteProjectEmpty1");
        this.EmptyText2.Text = this._isTeam ? Utils.GetString("InviteMemberEmpty2") : Utils.GetString("ShareEmpty2");
      }
      this.InviteButton.IsEnabled = false;
    }

    private void OnTitleSelected(object sender, GroupTitleViewModel e)
    {
      if (e.Index == 0)
        this.OnRecentShow();
      else
        this.OnSharedOrTeamShow();
    }

    public async Task DeleteRecentRecord(string email)
    {
      if (!await Communicator.DeleteShareContact(email))
        ;
      else
      {
        InviteHelper.DeleteRecentUser(email);
        this._recentInviteModels.RemoveAll((Predicate<InviteUserModel>) (r => r.Email == email));
        List<InviteUserModel> list = this._recentInviteModels.ToList<InviteUserModel>();
        if (list.Count<InviteUserModel>() > 10)
          list.RemoveRange(10, list.Count<InviteUserModel>() - 10);
        this.RecentInvite.ItemsSource = (IEnumerable) list;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/team/inviteusersdialog.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    internal Delegate _CreateDelegate(Type delegateType, string handler)
    {
      return Delegate.CreateDelegate(delegateType, (object) this, handler);
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          this.Titles = (GroupTitle) target;
          break;
        case 2:
          this.RecentInvite = (ItemsControl) target;
          break;
        case 3:
          this.SharedProject = (ItemsControl) target;
          break;
        case 4:
          this.TeamMembers = (ItemsControl) target;
          break;
        case 5:
          this.EmptyPage = (StackPanel) target;
          break;
        case 6:
          this.EmptyText1 = (TextBlock) target;
          break;
        case 7:
          this.EmptyText2 = (TextBlock) target;
          break;
        case 8:
          this.InviteButton = (Button) target;
          this.InviteButton.Click += new RoutedEventHandler(this.OnInviteClick);
          break;
        case 9:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnCancelClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
