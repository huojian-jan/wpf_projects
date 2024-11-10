// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Project.ShareProjectDialog
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.Util;
using ticktick_WPF.Util.Twitter;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.CustomControl;
using ticktick_WPF.Views.Detail;
using ticktick_WPF.Views.MainListView;
using ticktick_WPF.Views.Misc;
using ticktick_WPF.Views.Team;
using TickTickModels;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Project
{
  public class ShareProjectDialog : Window, IComponentConnector, IStyleConnector
  {
    private readonly string _projectId = "";
    private readonly ObservableCollection<ShareUserViewMode> _shareUserList = new ObservableCollection<ShareUserViewMode>();
    private readonly List<InviteUserModel> _teamMemberModels = new List<InviteUserModel>();
    private List<string> _invitedUserCodeList = new List<string>();
    private bool _isTeam;
    private bool _needSync;
    private List<string> _notifications;
    private ShareUserModel _owner;
    private ProjectModel _project;
    private List<string> _projectNotifications;
    private ObservableCollection<ShareContactsModel> _shareContactsList = new ObservableCollection<ShareContactsModel>();
    private string _teamId;
    private TeamModel _team;
    private bool _canInputDelete;
    private int? _quota;
    internal Grid EmailInviteGrid;
    internal CustomComboBox userContactsComboBox;
    internal Border InputBorder;
    internal System.Windows.Controls.TextBox addUserTextBox;
    internal HoverIconButton InviteImage;
    internal EscPopup InvitePopup;
    internal ProjectPermissionSetControl EmailJoinPermission;
    internal ScrollViewer SelectedUsersViewer;
    internal ItemsControl SelectedUsers;
    internal System.Windows.Controls.ProgressBar addUserProgressBar;
    internal StackPanel ApplyEditGrid;
    internal TextBlock PermissionNotEditText;
    internal TextBlock ApplyEditText;
    internal Grid BodyGrid;
    internal TextBlock NotifyButton;
    internal ScrollViewer MemberList;
    internal ItemsControl shareUserListView;
    internal Grid MemberEmpty;
    internal EscPopup MessagePopup;
    internal TeamNotification NotificationSetting;
    internal Grid PersonalPage;
    internal Grid LinkShareGrid;
    internal RowDefinition LinkFirstRow;
    internal System.Windows.Controls.CheckBox ShareLinkCheckBox;
    internal TextBlock EnableLinkText;
    internal ProjectPermissionSetControl LinkJoinPermission;
    internal TextBlock CopyLink;
    internal Border ToastBorder;
    internal TextBlock ToastText;
    internal Grid ButtonGrid;
    internal System.Windows.Controls.Button SaveButton;
    private bool _contentLoaded;

    public static async void TryShowShareDialog(string projectId, Window owner = null)
    {
      ProjectModel projectById = CacheManager.GetProjectById(projectId);
      if (projectById != null && !string.IsNullOrEmpty(projectById.teamId) && !projectById.IsShareList())
      {
        if (!await TeamService.CheckFreeTeamShareCount(projectById.teamId))
          return;
      }
      UserInfoModel userInfo = await UserManager.GetUserInfo(true);
      if (userInfo == null)
        return;
      if (userInfo.verifiedEmail || userInfo.fakedEmail)
      {
        ShareProjectDialog shareProjectDialog = new ShareProjectDialog(projectId);
        shareProjectDialog.Owner = owner ?? (Window) App.Window;
        shareProjectDialog.ShowDialog();
      }
      else
      {
        bool? nullable = new CustomerDialog(Utils.GetString("VerifiedEmail"), Utils.GetString("SendVerifyEmailHint"), Utils.GetString("SendVerifyEmail"), Utils.GetString("Cancel")).ShowDialog();
        bool flag = true;
        if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
          return;
        await Communicator.ResentVerifyEmail();
      }
    }

    public ShareProjectDialog(string projectId)
    {
      this.InitializeComponent();
      this._projectId = projectId;
      this.SetProject();
      this.shareUserListView.ItemsSource = (IEnumerable) this._shareUserList;
      this.InitTeam();
      this.InitLoad();
      InputBindingCollection inputBindings = this.InputBindings;
      KeyBinding keyBinding = new KeyBinding((ICommand) new RelayCommand((Action<object>) (o => ((ShareProjectDialog) o).OnEsc())), new KeyGesture(Key.Escape));
      keyBinding.CommandParameter = (object) this;
      inputBindings.Add((InputBinding) keyBinding);
    }

    private void OnEsc()
    {
      if (this.InvitePopup.IsOpen)
        this.InvitePopup.IsOpen = false;
      else if (this.SelectedUsersViewer.IsVisible)
      {
        if (this.SelectedUsers.ItemsSource is ObservableCollection<InviteSelectViewModel> itemsSource)
        {
          // ISSUE: explicit non-virtual call
          __nonvirtual (itemsSource.Clear());
        }
        this.SetInviteView();
      }
      else
        this.Close();
    }

    private async void InitLoad()
    {
      ShareProjectDialog shareProjectDialog = this;
      shareProjectDialog.InitData();
      shareProjectDialog.InitLink();
      await shareProjectDialog.LoadLocalUsers();
      shareProjectDialog.userContactsComboBox.ListView.ItemTemplate = (DataTemplate) shareProjectDialog.FindResource((object) "UserComboboxItemTemplate");
      shareProjectDialog.userContactsComboBox.ListPopup.StaysOpen = true;
      shareProjectDialog.userContactsComboBox.ListPopup.NeedFocus = false;
      shareProjectDialog.SelectedUsers.ItemsSource = (IEnumerable) new ObservableCollection<InviteSelectViewModel>();
      if (shareProjectDialog.EmailInviteGrid.IsVisible)
        shareProjectDialog.addUserTextBox.Focus();
      await shareProjectDialog.LoadRemoteUsers();
    }

    private async Task InitLink()
    {
      if (this._project != null && !this._project.IsProjectPermit())
        this.SetNoPermission();
      else
        await this.GetLink(true);
    }

    private void SetNoPermission()
    {
      this.EmailInviteGrid.Visibility = Visibility.Collapsed;
      this.ApplyEditGrid.Visibility = Visibility.Visible;
      string str = "";
      switch (this._project.permission)
      {
        case "read":
          str = Utils.GetString("ReadOnly");
          break;
        case "comment":
          str = Utils.GetString("CanComment");
          break;
      }
      this.PermissionNotEditText.Text = string.Format(Utils.GetString("PermissionIsNotEdit"), (object) str);
      this.LinkShareGrid.Visibility = Visibility.Collapsed;
    }

    private async void InitData()
    {
      ShareProjectDialog window = this;
      window.GetShareContacts();
      if (window._isTeam)
      {
        await InviteHelper.SetTeamContacts(window._teamId);
        await InviteHelper.SetTeamMembers(window._teamId, window._projectId, window);
        window.GetTeamMember();
      }
      else
        await InviteHelper.SetProjectUsers();
      await InviteHelper.SetShareContacts();
      window.SetCheckBox();
      window.InitCheckEvent();
      window.LinkJoinPermission.PermissionSelect += new EventHandler<int>(window.SetLinkJoinPermission);
      window.LinkJoinPermission.NeedAuditChange += new EventHandler<bool>(window.OnNeedAuditChanged);
      window.LinkJoinPermission.SetShowNeedAuditPanel(window._project.isOwner, window._project.needAudit, window._isTeam);
      window.InviteImage.Visibility = Visibility.Visible;
    }

    private void InitTeam()
    {
      this._teamId = this._project?.teamId;
      this._team = CacheManager.GetTeamById(this._teamId);
      this._isTeam = this._team != null;
      if (string.IsNullOrEmpty(this._teamId))
        return;
      this.LinkFirstRow.Height = new GridLength(0.0);
      this.ShareLinkCheckBox.Visibility = Visibility.Collapsed;
    }

    protected override async void OnClosed(EventArgs e)
    {
      if (this._project != null)
      {
        string[] notificationOptions = this._project.notificationOptions;
        string str1 = notificationOptions != null ? ((IEnumerable<string>) notificationOptions).Join<string>(";") : (string) null;
        List<string> projectNotifications = this._projectNotifications;
        string str2 = projectNotifications != null ? projectNotifications.Join<string>(";") : (string) null;
        if (str1 != str2)
        {
          this._project.notificationOptions = this._projectNotifications?.ToArray();
          this._project.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
          NotificationHelper.SaveNtfOptions(this._projectId, this._project.notificationOptions);
          int num = await ProjectDao.TryUpdateProject(this._project);
          SyncManager.Sync();
          goto label_6;
        }
      }
      if (this._needSync)
        SyncManager.Sync();
label_6:
      base.OnClosed(e);
    }

    private void SetProject()
    {
      this._project = CacheManager.GetProjectById(this._projectId);
      if (this._project == null)
        return;
      this.Title = this.Title + " \"" + this._project.name + "\"";
      string[] notificationOptions = this._project.notificationOptions;
      this._projectNotifications = notificationOptions != null ? ((IEnumerable<string>) notificationOptions).ToList<string>() : (List<string>) null;
    }

    protected override void OnDeactivated(EventArgs e)
    {
      this.Topmost = false;
      base.OnDeactivated(e);
    }

    protected override async void OnActivated(EventArgs e)
    {
      ShareProjectDialog shareProjectDialog = this;
      shareProjectDialog.Topmost = true;
      await Task.Delay(1);
      shareProjectDialog.Topmost = false;
      // ISSUE: reference to a compiler-generated method
      shareProjectDialog.\u003C\u003En__1(e);
    }

    private void GetShareContacts()
    {
      List<ShareContactsModel> shareContacts = InviteHelper.GetShareContacts();
      if (shareContacts == null || shareContacts.Count == 0)
        return;
      this._shareContactsList = new ObservableCollection<ShareContactsModel>(shareContacts.ToDictionaryEx<ShareContactsModel, string, ShareContactsModel>((Func<ShareContactsModel, string>) (c => c.email), (Func<ShareContactsModel, ShareContactsModel>) (c => c)).Values.ToList<ShareContactsModel>());
    }

    private async void GetTeamMember()
    {
      IEnumerable<TeamMember> teamMembers = InviteHelper.GetTeamMembers(this._teamId);
      List<TeamMember> list = teamMembers != null ? teamMembers.ToList<TeamMember>() : (List<TeamMember>) null;
      this._teamMemberModels.Clear();
      if (list == null || list.Count <= 0)
        return;
      foreach (TeamMember userMode in list)
      {
        if (!userMode.isMyself)
          this._teamMemberModels.Add(new InviteUserModel(userMode));
      }
    }

    private async Task GetLink(bool isInviteUrl = false, Constants.ProjectPermission permission = Constants.ProjectPermission.person)
    {
      ShareProjectDialog shareProjectDialog = this;
      try
      {
        string shareLink = await Communicator.GetShareLink(shareProjectDialog._projectId, isInviteUrl, permission);
        if (string.IsNullOrEmpty(shareLink))
          return;
        try
        {
          ShareLinkModel shareLinkModel = JsonConvert.DeserializeObject<ShareLinkModel>(shareLink);
          if (shareLinkModel == null)
            return;
          if (!string.IsNullOrEmpty(shareLinkModel.inviteUrl))
          {
            string str = BaseUrl.GetDomainUrl() + shareLinkModel.inviteUrl;
            Constants.ProjectPermission result;
            if (Enum.TryParse<Constants.ProjectPermission>(shareLinkModel.permission, true, out result))
              shareProjectDialog.LinkJoinPermission.SwitchPermission((int) result);
            shareProjectDialog.ShareLinkCheckBox.IsChecked = new bool?(true);
            shareProjectDialog.CopyLink.Tag = (object) str;
          }
          else
            shareProjectDialog.ShareLinkCheckBox.IsChecked = new bool?(false);
        }
        catch (Exception ex)
        {
          switch (JObject.Parse(shareLink)["errorCode"].ToString())
          {
            case "no_project_permission":
              CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("EnableLinkShareFailedTitle"), Utils.GetString("EnableLinkShareFailedContent"), MessageBoxButton.OK);
              customerDialog.Owner = Window.GetWindow((DependencyObject) shareProjectDialog);
              customerDialog.ShowDialog();
              shareProjectDialog.ShareLinkCheckBox.IsChecked = new bool?(false);
              break;
            default:
              new CustomerDialog(Utils.GetString("OperateFailed"), Utils.GetString("OperateFailedMessage"), Utils.GetString("GotIt"), "").ShowDialog();
              break;
          }
        }
      }
      catch (Exception ex)
      {
      }
    }

    private void SetLinkJoinPermission(object sender, int e)
    {
      this.GetLink(permission: (Constants.ProjectPermission) e);
    }

    private async Task LoadRemoteUsers(bool checkAll = false)
    {
      await AvatarHelper.ResetProjectAvatars(this._projectId, true, checkAll);
      await this.LoadLocalUsers();
    }

    private async Task LoadLocalUsers()
    {
      ShareProjectDialog shareProjectDialog = this;
      List<ShareUserViewMode> shareUserList = new List<ShareUserViewMode>();
      List<ShareUserModel> projectUsersAsync = await AvatarHelper.GetProjectUsersAsync(shareProjectDialog._projectId, true, checkUserList: false);
      bool flag = projectUsersAsync != null && projectUsersAsync.All<ShareUserModel>((Func<ShareUserModel, bool>) (u => u.isOwner || u.visitor));
      if (projectUsersAsync != null && projectUsersAsync.Count != 0)
      {
        foreach (ShareUserModel shareUserModel in projectUsersAsync)
        {
          List<ShareUserViewMode> shareUserViewModeList = shareUserList;
          ShareUserViewMode shareUserViewMode = new ShareUserViewMode(shareUserModel);
          long? userId = shareUserModel.userId;
          ref long? local = ref userId;
          shareUserViewMode.displayName = (local.HasValue ? local.GetValueOrDefault().ToString() : (string) null) == LocalSettings.Settings.LoginUserId ? Utils.GetString("Me") : (shareUserModel.deleted ? Utils.GetString("AccountNotExist") : shareUserModel.displayName);
          ProjectModel project1 = shareProjectDialog._project;
          shareUserViewMode.openToTeam = project1 != null && project1.OpenToTeam;
          shareUserViewMode.noMembers = flag;
          ProjectModel project2 = shareProjectDialog._project;
          shareUserViewMode.editable = project2 != null && project2.IsProjectPermit();
          ProjectModel project3 = shareProjectDialog._project;
          shareUserViewMode.userCount = project3 != null ? project3.userCount : 2;
          shareUserViewModeList.Add(shareUserViewMode);
          if (shareUserModel.userCode != null)
            UserPublicProfilesDao.InsertUser(new UserPublicProfilesModel(shareUserModel));
        }
        shareProjectDialog._owner = projectUsersAsync.FirstOrDefault<ShareUserModel>((Func<ShareUserModel, bool>) (user => user.isOwner));
        shareProjectDialog._invitedUserCodeList = projectUsersAsync.Select<ShareUserModel, string>((Func<ShareUserModel, string>) (mode => mode.userCode)).ToList<string>();
      }
      else
      {
        UserInfoModel userInfo = await UserManager.GetUserInfo();
        long result;
        if (!long.TryParse(LocalSettings.Settings.LoginUserId, out result))
          result = -1L;
        shareUserList.Add(new ShareUserViewMode()
        {
          userId = result,
          avatarUrl = userInfo == null || string.IsNullOrEmpty(userInfo.picture) || userInfo.picture.Contains("avatar-new.png") ? "../Assets/avatar-new.png" : userInfo.picture,
          username = Utils.IsFakeEmail(userInfo?.username) ? string.Empty : userInfo?.username,
          displayName = Utils.GetString("Me"),
          isOwner = true,
          isProjectShare = true,
          isAccept = true,
          noMembers = true,
          userCode = userInfo?.userCode,
          acceptStatus = 1,
          userCount = shareProjectDialog._project.userCount
        });
        shareProjectDialog._owner = new ShareUserModel()
        {
          userId = new long?(result),
          isOwner = true
        };
      }
      if (shareProjectDialog._team != null)
      {
        ImageSource teamLogo = TeamService.GetTeamLogo(shareProjectDialog._team);
        ShareUserViewMode teamVModel = new ShareUserViewMode()
        {
          userId = 0,
          avatar = teamLogo,
          username = ShareUserViewMode.GetPermissionTextOfTeam(shareProjectDialog._project.teamMemberPermission, shareProjectDialog._team.name),
          permission = shareProjectDialog._project.teamMemberPermission,
          displayName = shareProjectDialog._team.name,
          openToTeam = shareProjectDialog._project.OpenToTeam,
          isTeam = true,
          editable = shareProjectDialog._project.IsProjectPermit(),
          isAccept = true
        };
        if (shareProjectDialog._shareUserList.Count > 0 && shareProjectDialog._shareUserList[0].isTeam)
          shareProjectDialog._shareUserList[0].SetProperty(teamVModel);
        else
          shareProjectDialog._shareUserList.Insert(0, teamVModel);
      }
      // ISSUE: reference to a compiler-generated method
      shareProjectDialog._shareUserList.Where<ShareUserViewMode>((Func<ShareUserViewMode, bool>) (m => !m.isTeam)).ToList<ShareUserViewMode>().ForEach(new Action<ShareUserViewMode>(shareProjectDialog.\u003CLoadLocalUsers\u003Eb__32_4));
      shareUserList.ForEach(new Action<ShareUserViewMode>(((Collection<ShareUserViewMode>) shareProjectDialog._shareUserList).Add));
      shareProjectDialog.ShowOrHidePages();
      shareUserList = (List<ShareUserViewMode>) null;
    }

    private void ShowOrHidePages()
    {
      if (this._shareUserList.Count > 1)
      {
        this.MemberList.Visibility = Visibility.Visible;
        this.MemberEmpty.Visibility = Visibility.Collapsed;
        this.PersonalPage.Visibility = Visibility.Collapsed;
        this.NotifyButton.Visibility = this._project.IsProjectPermit() ? Visibility.Visible : Visibility.Collapsed;
      }
      else if (this.ShareLinkCheckBox.IsChecked.GetValueOrDefault() && this._shareUserList.Count == 1)
      {
        this.MemberList.Visibility = Visibility.Visible;
        this.MemberEmpty.Visibility = Visibility.Visible;
        this.PersonalPage.Visibility = Visibility.Collapsed;
        this.NotifyButton.Visibility = Visibility.Visible;
      }
      else
      {
        this.MemberList.Visibility = Visibility.Collapsed;
        this.MemberEmpty.Visibility = Visibility.Visible;
        this.PersonalPage.Visibility = Visibility.Visible;
        this.NotifyButton.Visibility = Visibility.Collapsed;
      }
    }

    private async void ShareLinkCheckBoxClick(object sender, MouseButtonEventArgs e)
    {
      bool? isChecked = this.ShareLinkCheckBox.IsChecked;
      bool flag = false;
      if (isChecked.GetValueOrDefault() == flag & isChecked.HasValue)
        await this.ShowShareLink();
      else
        this.HideShareLink();
      this.ShowOrHidePages();
      e.Handled = true;
    }

    private async Task ShowShareLink()
    {
      await this.GetLink(permission: Constants.ProjectPermission.write);
      this.EnableLinkText.Text = Utils.GetString("EnableLinkShare");
    }

    private async void HideShareLink()
    {
      ShareProjectDialog shareProjectDialog = this;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("StopLinkShare"), Utils.GetString("StopLinkShareContent"), MessageBoxButton.OKCancel);
      customerDialog.Owner = Window.GetWindow((DependencyObject) shareProjectDialog);
      bool? nullable = customerDialog.ShowDialog();
      bool flag = true;
      if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        return;
      shareProjectDialog.EnableLinkText.Text = Utils.GetString("DisableLinkShare");
      shareProjectDialog.ShareLinkCheckBox.IsChecked = new bool?(false);
      string str = await Communicator.DeleteShareLink(shareProjectDialog._projectId);
    }

    private async void InviteUserOnEnter(System.Windows.Controls.TextBox input)
    {
      if (!this.userContactsComboBox.IsOpen)
        return;
      this.userContactsComboBox.HandleEnter();
      this.userContactsComboBox.IsOpen = false;
      input.Text = "";
    }

    private void addUserTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      System.Windows.Controls.TextBox input1 = sender as System.Windows.Controls.TextBox;
      switch (e.Key)
      {
        case Key.Return:
          this.InviteUserOnEnter(input1);
          break;
        case Key.Up:
          this.userContactsComboBox.UpDownSelect(true);
          break;
        case Key.Down:
          this.userContactsComboBox.UpDownSelect(false);
          break;
        default:
          if (e.Key != Key.Return && !string.IsNullOrWhiteSpace(input1?.Text))
          {
            ObservableCollection<ShareContactsViewModel> items = new ObservableCollection<ShareContactsViewModel>();
            if (this._shareContactsList == null || this._shareContactsList.Count == 0)
              this.GetShareContacts();
            string input = input1.Text.Trim();
            ObservableCollection<ShareContactsModel> shareContactsList = this._shareContactsList;
            List<ShareContactsModel> list = shareContactsList != null ? shareContactsList.Where<ShareContactsModel>((Func<ShareContactsModel, bool>) (p =>
            {
              string displayName = p.displayName;
              if ((displayName != null ? (displayName.Contains(input.ToLower()) ? 1 : 0) : 0) == 0)
              {
                string displayEmail = p.displayEmail;
                if ((displayEmail != null ? (displayEmail.Contains(input.ToLower()) ? 1 : 0) : 0) == 0)
                {
                  string email = p.email;
                  return email != null && email.Contains(input.ToLower());
                }
              }
              return true;
            })).ToList<ShareContactsModel>() : (List<ShareContactsModel>) null;
            if (list != null && list.Count != 0)
            {
              foreach (ShareContactsModel shareContactsModel in list)
              {
                if (shareContactsModel.email != LocalSettings.Settings.LoginUserName)
                  items.Add(new ShareContactsViewModel(shareContactsModel));
              }
              if (items.Count > 0)
              {
                items[0].HoverSelected = true;
                this.userContactsComboBox.Init<ShareContactsViewModel>(items, (ShareContactsViewModel) null);
                this.userContactsComboBox.IsOpen = true;
                break;
              }
              this.userContactsComboBox.IsOpen = false;
              break;
            }
            if ((!Utils.IsValidEmail(input) ? 0 : (input != UserManager.GetUserEmail() ? 1 : 0)) != 0)
            {
              ObservableCollection<ShareContactsViewModel> observableCollection = items;
              ShareContactsViewModel contactsViewModel = new ShareContactsViewModel(input);
              contactsViewModel.HoverSelected = true;
              observableCollection.Add(contactsViewModel);
              this.userContactsComboBox.Init<ShareContactsViewModel>(items, (ShareContactsViewModel) null);
              this.userContactsComboBox.IsOpen = true;
              break;
            }
            this.userContactsComboBox.IsOpen = false;
            break;
          }
          this.userContactsComboBox.IsOpen = false;
          break;
      }
    }

    private async Task<bool> InviteUser(string toUsername, int index)
    {
      Constants.ProjectPermission selectedPermission = this.EmailJoinPermission.SelectedPermission;
      ShareAddUserMode shareAddUserMode = await Communicator.AddShareUser(this._projectId, toUsername, selectedPermission);
      if (shareAddUserMode?.toUsername != null && shareAddUserMode.toUsername != "")
      {
        if (!string.IsNullOrEmpty(shareAddUserMode.toUserCode))
        {
          UserPublicProfilesModel userInfoByUserCode = await UserPublicProfilesDao.GetUserInfoByUserCode(shareAddUserMode.toUserCode);
          this._shareUserList.Insert(index, new ShareUserViewMode(shareAddUserMode)
          {
            displayName = string.IsNullOrEmpty(userInfoByUserCode?.nickName) ? userInfoByUserCode?.displayName : userInfoByUserCode.nickName,
            avatarUrl = userInfoByUserCode != null ? (string.IsNullOrEmpty(userInfoByUserCode.avatarUrl) || userInfoByUserCode.avatarUrl.Contains("avatar-new") ? "../Assets/avatar-new.png" : userInfoByUserCode.avatarUrl) : "../Assets/avatar-new.png",
            editable = true,
            siteId = ((int?) userInfoByUserCode?.siteId).GetValueOrDefault()
          });
          this._invitedUserCodeList.Add(shareAddUserMode.toUserCode);
        }
        return true;
      }
      if (shareAddUserMode != null)
      {
        int num = shareAddUserMode.fromUsername != "" ? 1 : 0;
      }
      return false;
    }

    public async void TransferProjectOwner()
    {
      ShareProjectDialog shareProjectDialog = this;
      TransferProjectWindow transferProjectWindow = new TransferProjectWindow(shareProjectDialog._projectId);
      transferProjectWindow.Owner = (Window) shareProjectDialog;
      bool? nullable = transferProjectWindow.ShowDialog();
      if (!nullable.HasValue || !nullable.Value)
        return;
      shareProjectDialog._needSync = true;
      shareProjectDialog._project.isOwner = false;
      shareProjectDialog.LinkJoinPermission.SetShowNeedAuditPanel(false);
      await shareProjectDialog.LoadRemoteUsers();
    }

    public async void DeleteUser(object sender, EventArgs e)
    {
      ShareProjectDialog shareProjectDialog = this;
      try
      {
        bool? nullable = new CustomerDialog(Utils.GetString("RemoveShareUser"), string.Format(Utils.GetString("RemoveShareUserContent"), sender is ShareUserViewMode shareUserViewMode1 ? (object) shareUserViewMode1.displayName : (object) (string) null), Utils.GetString("RemoveMember"), Utils.GetString("Cancel"), shareProjectDialog.Owner).ShowDialog();
        bool flag = true;
        if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
          return;
        ShareUserViewMode temp = shareProjectDialog._shareUserList.FirstOrDefault<ShareUserViewMode>((Func<ShareUserViewMode, bool>) (p => p.userCode == (sender is ShareUserViewMode shareUserViewMode2 ? shareUserViewMode2.userCode : (string) null)));
        if (temp != null)
        {
          string json = await Communicator.DeleteShareUser(shareProjectDialog._projectId, temp.recordId);
          if (!string.IsNullOrEmpty(json))
          {
            try
            {
              int num = JObject.Parse(json)["errorCode"].ToString() == "project_access_permission" ? 1 : 0;
              new CustomerDialog(Utils.GetString("OperateFailed"), Utils.GetString("OperateFailedMessage"), Utils.GetString("GotIt"), "").ShowDialog();
              return;
            }
            catch (Exception ex)
            {
            }
          }
          shareProjectDialog._shareUserList.Remove(temp);
          shareProjectDialog._invitedUserCodeList.Remove(temp.userCode);
          await TaskService.ClearAssignByProjectId(shareProjectDialog._projectId, temp.userId.ToString());
          if (shareProjectDialog._project != null)
          {
            --shareProjectDialog._project.userCount;
            if (shareProjectDialog._project.userCount < 1)
              shareProjectDialog._project.userCount = 1;
            if (shareProjectDialog._project.userCount == 1)
              await TaskService.ClearAssignByProjectId(shareProjectDialog._projectId, LocalSettings.Settings["LoginUserId"].ToString());
            AvatarHelper.RemoveProjectUser(shareProjectDialog._project.id, temp.userId);
          }
          int num1 = await ProjectDao.TryUpdateProject(shareProjectDialog._project);
          SyncManager.Sync();
        }
        shareProjectDialog.ShowOrHidePages();
        temp = (ShareUserViewMode) null;
      }
      catch (Exception ex)
      {
      }
    }

    public override void OnApplyTemplate()
    {
      Utils.InitBaseEvents((Window) this, new Func<string, DependencyObject>(((FrameworkElement) this).GetTemplateChild));
      base.OnApplyTemplate();
    }

    private void CopyShareLink(object sender, RoutedEventArgs e)
    {
      string tag = this.CopyLink.Tag as string;
      if (string.IsNullOrEmpty(tag))
        return;
      try
      {
        System.Windows.Forms.Clipboard.SetText(tag);
        Utils.Toast(Utils.GetString("Copied"));
      }
      catch (Exception ex)
      {
      }
    }

    private async void ShowInvitePopup(object sender, MouseButtonEventArgs e)
    {
      ShareProjectDialog shareProjectDialog = this;
      bool flag = !shareProjectDialog._isTeam;
      if (flag)
        flag = !await shareProjectDialog.CheckShareLimit(1);
      if (flag)
        return;
      if (shareProjectDialog._isTeam)
        shareProjectDialog.GetTeamMember();
      InviteUsersDialog inviteUsersDialog = shareProjectDialog._isTeam ? new InviteUsersDialog((IEnumerable<InviteUserModel>) InviteHelper.GetRecentInvite(), (IEnumerable<InviteUserModel>) shareProjectDialog._teamMemberModels, (ICollection<string>) shareProjectDialog._invitedUserCodeList) : new InviteUsersDialog(InviteHelper.GetRecentInvite(), InviteHelper.GetProjectUsers(), (ICollection<string>) shareProjectDialog._invitedUserCodeList);
      // ISSUE: reference to a compiler-generated method
      inviteUsersDialog.OnCancel += new EventHandler(shareProjectDialog.\u003CShowInvitePopup\u003Eb__44_0);
      inviteUsersDialog.OnInvited += new EventHandler<List<(string, string)>>(shareProjectDialog.OnInviteUsersSelect);
      shareProjectDialog.InvitePopup.Child = (UIElement) inviteUsersDialog;
      shareProjectDialog.InvitePopup.IsOpen = true;
    }

    private async Task<bool> CheckShareLimit(int inviteNum)
    {
      ShareProjectDialog shareProjectDialog = this;
      if (shareProjectDialog._team != null)
        return shareProjectDialog.CheckTeamLimit(inviteNum);
      if (shareProjectDialog._team == null && !string.IsNullOrEmpty(shareProjectDialog._project.teamId))
        return true;
      int? quota1 = shareProjectDialog._quota;
      int num1;
      if (quota1.HasValue)
        num1 = quota1.GetValueOrDefault();
      else
        num1 = await Communicator.CheckShareProjectQuota(shareProjectDialog._projectId);
      shareProjectDialog._quota = new int?(num1);
      int? quota2 = shareProjectDialog._quota;
      int num2 = inviteNum;
      if (!(quota2.GetValueOrDefault() < num2 & quota2.HasValue))
        return true;
      string content = (string) null;
      long limitNumber = 0;
      if (shareProjectDialog._owner?.userId.ToString() == LocalSettings.Settings.LoginUserId)
      {
        if (UserDao.IsPro())
        {
          limitNumber = Utils.GetUserLimit(Constants.LimitKind.ShareUserNumber);
          content = Utils.GetString("RecipientsLimit");
        }
        else
          ProChecker.ShowUpgradeDialog(shareProjectDialog._isTeam ? ProType.MoreTeamGuest : ProType.MoreSharingMembers, teamId: shareProjectDialog._isTeam ? shareProjectDialog._team?.id : (string) null);
      }
      else
      {
        limitNumber = (long) (shareProjectDialog._shareUserList.Count + shareProjectDialog._quota.Value);
        if (limitNumber > 2L)
        {
          content = Utils.GetString("ShareListNotOwnerProLimit");
        }
        else
        {
          CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), string.Format(Utils.GetString("ShareListNotOwnerFreeLimit"), (object) shareProjectDialog._owner?.displayName), MessageBoxButton.OKCancel);
          customerDialog.Owner = Window.GetWindow((DependencyObject) shareProjectDialog);
          bool? nullable = customerDialog.ShowDialog();
          if (nullable.HasValue && nullable.Value)
          {
            string pay = await Communicator.ReminderToPay(shareProjectDialog._projectId);
            Utils.Toast(Utils.GetString("AlreadyReminded"));
          }
        }
      }
      if (content != null)
      {
        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), string.Format(content, (object) limitNumber), MessageBoxButton.OK);
        customerDialog.Owner = Window.GetWindow((DependencyObject) shareProjectDialog);
        customerDialog.ShowDialog();
      }
      return false;
    }

    private bool CheckTeamLimit(int inviteNum)
    {
      long limit = this._team.IsPro() ? LimitCache.GetProLimitByKey(Constants.LimitKind.VisitorNumber) : LimitCache.GetFreeLimitByKey(Constants.LimitKind.VisitorNumber);
      int num = this._shareUserList.Count<ShareUserViewMode>((Func<ShareUserViewMode, bool>) (u => u.visitor));
      if (limit >= (long) (num + inviteNum))
        return true;
      this.ShowTeamGuestDialog(limit);
      return false;
    }

    private async void ShowTeamGuestDialog(long limit)
    {
      ShareProjectDialog shareProjectDialog = this;
      string content = "";
      if (shareProjectDialog._team.IsPro())
        content = string.Format(Utils.GetString("ProLimitedSharingGuests"), (object) limit);
      else
        ProChecker.ShowUpgradeDialog(ProType.MoreTeamGuest, teamId: shareProjectDialog._team?.id);
      if (string.IsNullOrEmpty(content))
        return;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), content, MessageBoxButton.OK);
      customerDialog.Owner = Window.GetWindow((DependencyObject) shareProjectDialog);
      customerDialog.ShowDialog();
    }

    private async void ShowGuestLimitDialog(long limit)
    {
      ShareProjectDialog shareProjectDialog = this;
      string content = string.Format(Utils.GetString(limit <= 1L ? "FreeLimitedSharingGuests" : "ProLimitedSharingGuests"), (object) limit);
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), content, MessageBoxButton.OK);
      customerDialog.Owner = Window.GetWindow((DependencyObject) shareProjectDialog);
      customerDialog.ShowDialog();
    }

    private void OnInviteUsersSelect(object sender, List<(string, string)> userEmails)
    {
      this.OnSelectedUserChanged(userEmails, true);
      this.InvitePopup.IsOpen = false;
    }

    private async void InviteUserByEmails(List<string> userEmails)
    {
      ShareProjectDialog shareProjectDialog = this;
      if (userEmails == null || userEmails.Count <= 0)
        return;
      shareProjectDialog.InvitePopup.IsOpen = false;
      // ISSUE: reference to a compiler-generated method
      int inviteNum = userEmails.Count<string>(new Func<string, bool>(shareProjectDialog.\u003CInviteUserByEmails\u003Eb__50_0));
      if (await shareProjectDialog.CheckShareLimit(inviteNum))
      {
        await shareProjectDialog.BatchInviteUsersAsync(userEmails);
        shareProjectDialog.ShowOrHidePages();
        shareProjectDialog._quota = new int?();
      }
      shareProjectDialog.InvitePopup.IsOpen = false;
      AvatarHelper.ResetPullTime(shareProjectDialog._projectId);
    }

    private async Task BatchInviteUsersAsync(List<string> userEmails)
    {
      ShareProjectDialog owner = this;
      // ISSUE: reference to a compiler-generated method
      userEmails = userEmails.Where<string>(new Func<string, bool>(owner.\u003CBatchInviteUsersAsync\u003Eb__51_0)).ToList<string>();
      ShareAddUsersMode shareAddUsersMode = await Communicator.AddShareUsers(owner._projectId, userEmails, owner.EmailJoinPermission.SelectedPermission);
      if (shareAddUsersMode?.errors != null && shareAddUsersMode.errors.Count > 0)
      {
        if (userEmails.Count == 1)
        {
          string errorCode = shareAddUsersMode.errors[0].errorCode;
          if (errorCode != null)
          {
            switch (errorCode.Length)
            {
              case 11:
                if (errorCode == "share_exist")
                  break;
                goto label_38;
              case 13:
                if (errorCode == "illegal_share")
                  break;
                goto label_38;
              case 16:
                if (errorCode == "invalidate_email")
                {
                  new CustomerDialog(Utils.GetString("AddFailed"), Utils.GetString("InvalidateEmail"), Utils.GetString("OK"), "", owner.Owner).ShowDialog();
                  goto label_42;
                }
                else
                  goto label_38;
              case 17:
                if (errorCode == "unknown_exception")
                {
                  new CustomerDialog(Utils.GetString("AddFailed"), Utils.GetString("UnknownException"), Utils.GetString("OK"), "", owner.Owner).ShowDialog();
                  goto label_42;
                }
                else
                  goto label_38;
              case 18:
                if (errorCode == "no_team_permission")
                  goto label_36;
                else
                  goto label_38;
              case 20:
                if (errorCode == "visitor_exceed_quota")
                {
                  long result = 1;
                  JToken jtoken;
                  if (shareAddUsersMode.errors[0].data != null && shareAddUsersMode.errors[0].data.TryGetValue("limit", out jtoken))
                    long.TryParse(jtoken.ToString(), out result);
                  if (owner._team != null)
                  {
                    owner.ShowTeamGuestDialog(result);
                    goto label_42;
                  }
                  else
                  {
                    owner.ShowGuestLimitDialog(result);
                    goto label_42;
                  }
                }
                else
                  goto label_38;
              case 22:
                if (errorCode == "exceed_max_share_limit")
                {
                  long? userId = (long?) owner._owner?.userId;
                  long num = (long) int.Parse(LocalSettings.Settings["LoginUserId"].ToString());
                  if (userId.GetValueOrDefault() == num & userId.HasValue)
                  {
                    ProChecker.ShowUpgradeDialog(ProType.MoreSharingMembers);
                    goto label_42;
                  }
                  else if (owner._shareUserList.Count + userEmails.Count > 2)
                  {
                    CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), string.Format(Utils.GetString("ShareListNotOwnerProLimit"), (object) "29"), MessageBoxButton.OK);
                    customerDialog.Owner = Window.GetWindow((DependencyObject) owner);
                    customerDialog.ShowDialog();
                    goto label_42;
                  }
                  else
                  {
                    CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), string.Format(Utils.GetString("ShareListNotOwnerFreeLimit"), (object) owner._owner?.displayName), MessageBoxButton.OKCancel);
                    customerDialog.Owner = Window.GetWindow((DependencyObject) owner);
                    bool? nullable = customerDialog.ShowDialog();
                    if (nullable.HasValue && nullable.Value)
                    {
                      string pay = await Communicator.ReminderToPay(owner._projectId);
                      Utils.Toast(Utils.GetString("AlreadyReminded"));
                      goto label_42;
                    }
                    else
                      goto label_42;
                  }
                }
                else
                  goto label_38;
              case 23:
                if (errorCode == "user_email_not_verified")
                {
                  UserInfoModel userInfo = await UserManager.GetUserInfo();
                  if (userInfo != null)
                  {
                    new CustomerDialog(Utils.GetString("VerifiedEmail"), string.Format(Utils.GetString("ShareEmailNotVerified"), string.IsNullOrWhiteSpace(userInfo.email) ? (object) userInfo.username : (object) userInfo.email), Utils.GetString("OK"), "", owner.Owner).ShowDialog();
                    goto label_42;
                  }
                  else
                    goto label_42;
                }
                else
                  goto label_38;
              case 25:
                switch (errorCode[0])
                {
                  case 'p':
                    if (!(errorCode == "project_access_permission") || owner._team == null)
                      goto label_38;
                    else
                      goto label_36;
                  case 't':
                    if (errorCode == "team_project_exceed_quota")
                    {
                      ProChecker.ShowUpgradeDialog(ProType.TeamShareLimit, (Window) owner, owner._project.teamId);
                      goto label_42;
                    }
                    else
                      goto label_38;
                  default:
                    goto label_38;
                }
              case 26:
                if (errorCode == "exceed_pro_max_share_limit")
                {
                  long userLimit = Utils.GetUserLimit(Constants.LimitKind.ShareUserNumber);
                  string format = Utils.GetString("RecipientsLimit");
                  new CustomerDialog(Utils.GetString("LimitTips"), string.Format(format, (object) userLimit), Utils.GetString("OK"), "", owner.Owner).ShowDialog();
                  goto label_42;
                }
                else
                  goto label_38;
              case 27:
                if (errorCode == "exceed_team_max_share_limit")
                {
                  long num = Utils.GetUserLimit(Constants.LimitKind.ShareUserNumber) + 1L;
                  new CustomerDialog(Utils.GetString("LimitTips"), string.Format(Utils.GetString("RecipientsTeamLimit"), (object) num), Utils.GetString("OK"), "", owner.Owner).ShowDialog();
                  goto label_42;
                }
                else
                  goto label_38;
              default:
                goto label_38;
            }
            new CustomerDialog(Utils.GetString("AddFailed"), Utils.GetString("ShareExist"), Utils.GetString("OK"), "", owner.Owner).ShowDialog();
            goto label_42;
label_36:
            new CustomerDialog(Utils.GetString("OperateFailed"), Utils.GetString("CannotInvitePeopleOutsideTeam"), Utils.GetString("OK"), "", owner.Owner).ShowDialog();
            goto label_42;
          }
label_38:
          new CustomerDialog(Utils.GetString("OperateFailed"), Utils.GetString("OperateFailedMessage"), Utils.GetString("GotIt"), "").ShowDialog();
        }
        else
        {
          CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("InvitesSend"), string.Format(Utils.GetString("InviteUsersError"), (object) userEmails.Count, (object) shareAddUsersMode.records?.Count), Utils.GetString("GotIt"), "", owner.Owner);
          if (owner._team != null)
            customerDialog.SetEmphasizeText(Utils.GetString("CannotInvitePeopleOutsideTeam"));
          customerDialog.ShowDialog();
        }
      }
label_42:
      if (shareAddUsersMode?.records != null && shareAddUsersMode.records.Count > 0)
        owner.LoadRemoteUsers();
      if (owner.SelectedUsers.ItemsSource is ObservableCollection<InviteSelectViewModel> itemsSource)
      {
        // ISSUE: explicit non-virtual call
        __nonvirtual (itemsSource.Clear());
      }
      owner.SetInviteView();
      shareAddUsersMode = (ShareAddUsersMode) null;
    }

    private void InitCheckEvent()
    {
      this.NotificationSetting.DoCheck += new EventHandler<bool>(this.OnDoCheck);
      this.NotificationSetting.CreateCheck += new EventHandler<bool>(this.OnCreateCheck);
      this.NotificationSetting.DelCheck += new EventHandler<bool>(this.OnDelCheck);
    }

    private void OnDelCheck(object sender, bool Checked)
    {
      if (this._notifications == null)
        return;
      this._notifications.Remove("DELETE");
      this._notifications.Remove("MOVE_OUT");
      if (Checked)
      {
        this._notifications.Add("DELETE");
        this._notifications.Add("MOVE_OUT");
      }
      else
      {
        this._notifications.Remove("DELETE");
        this._notifications.Remove("MOVE_OUT");
      }
      this._projectNotifications = this._notifications;
    }

    private void OnCreateCheck(object sender, bool Checked)
    {
      if (this._notifications == null)
        return;
      this._notifications.Remove("CREATE");
      if (Checked)
        this._notifications.Add("CREATE");
      else
        this._notifications.Remove("CREATE");
      this._projectNotifications = this._notifications;
    }

    private void OnDoCheck(object sender, bool Checked)
    {
      if (this._notifications == null)
        return;
      this._notifications.Remove("DONE");
      this._notifications.Remove("UNDONE");
      if (Checked)
      {
        this._notifications.Add("DONE");
        this._notifications.Add("UNDONE");
      }
      else
      {
        this._notifications.Remove("DONE");
        this._notifications.Remove("UNDONE");
      }
      this._projectNotifications = this._notifications;
    }

    private void SetCheckBox()
    {
      string notificationOptions = LocalSettings.Settings.NotificationOptions;
      string[] source = this._project?.notificationOptions;
      if (source == null)
        source = (notificationOptions ?? "").Split(';');
      this._notifications = ((IEnumerable<string>) source).ToList<string>();
      if (this._notifications == null)
        return;
      this._notifications.Remove("");
      this.NotificationSetting.DoOrUndo.IsChecked = new bool?(this._notifications.Contains("DONE") || this._notifications.Contains("UNDONE"));
      this.NotificationSetting.Create.IsChecked = new bool?(this._notifications.Contains("CREATE"));
      this.NotificationSetting.Delete.IsChecked = new bool?(this._notifications.Contains("DELETE") || this._notifications.Contains("MOVE_OUT"));
    }

    public async void ExitProject(object sender, EventArgs e)
    {
      ShareProjectDialog shareProjectDialog = this;
      CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("ExitProjectTitle"), string.Format(Utils.GetString("ExitProjectSummary"), (object) shareProjectDialog._project?.name), MessageBoxButton.OKCancel);
      customerDialog.Owner = Window.GetWindow((DependencyObject) shareProjectDialog);
      bool? nullable = customerDialog.ShowDialog();
      bool flag = true;
      if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        return;
      if (TaskStickyWindow.TryCloseInProject(shareProjectDialog._project?.id))
        Utils.Toast(Utils.GetString("StickyClosed"));
      await ProjectDao.DeleteProjectById(shareProjectDialog._project?.id);
      await TaskService.DeleteAllTasksInProject(shareProjectDialog._project?.id);
      ListViewContainer.ReloadProjectData();
      SyncManager.Sync();
      shareProjectDialog.Close();
    }

    public async void SetUserPermission(ShareUserViewMode mode, int e)
    {
      if (mode.isTeam)
      {
        this._project.teamMemberPermission = ((Constants.ProjectPermission) e).ToString();
        mode.permission = this._project.teamMemberPermission;
        mode.SetTeamPermissionText();
        await this.UploadProject();
        this.LoadRemoteUsers();
      }
      else
      {
        string json = await Communicator.SetPermission(this._projectId, (Constants.ProjectPermission) e, mode.recordId);
        if (string.IsNullOrEmpty(json))
          return;
        try
        {
          int num = JObject.Parse(json)["errorCode"].ToString() == "project_access_permission" ? 1 : 0;
          new CustomerDialog(Utils.GetString("OperateFailed"), Utils.GetString("OperateFailedMessage"), Utils.GetString("GotIt"), "").ShowDialog();
        }
        catch (Exception ex)
        {
        }
      }
    }

    private async Task UploadProject()
    {
      ProjectModel projectById = CacheManager.GetProjectById(this._project.id);
      projectById.teamMemberPermission = this._project.teamMemberPermission;
      projectById.openToTeam = this._project.openToTeam;
      projectById.needAudit = this._project.needAudit;
      projectById.sync_status = Constants.SyncStatus.SYNC_UPDATE.ToString();
      int num = await ProjectDao.TryUpdateProject(projectById);
      SyncManager.Sync();
    }

    private async void ApplyEdit(object sender, RoutedEventArgs e)
    {
      ShareProjectDialog shareProjectDialog = this;
      if (!(sender is System.Windows.Controls.Button button))
        return;
      button.Click -= new RoutedEventHandler(shareProjectDialog.ApplyEdit);
      button.Cursor = System.Windows.Input.Cursors.Arrow;
      shareProjectDialog.ApplyEditText.Foreground = (Brush) ThemeUtil.GetColor("BaseColorOpacity60");
      shareProjectDialog.ApplyEditText.MouseEnter -= new System.Windows.Input.MouseEventHandler(shareProjectDialog.ShowUnderLine);
      shareProjectDialog.ApplyEditText.TextDecorations = (TextDecorationCollection) null;
      await Communicator.ApplyPermission(shareProjectDialog._projectId, Constants.ProjectPermission.write);
      Utils.Toast(Utils.GetString("ApplySent"));
    }

    private void ShowUnderLine(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (!(sender is TextBlock textBlock))
        return;
      textBlock.TextDecorations = TextDecorations.Underline;
    }

    private void HideUnderLine(object sender, System.Windows.Input.MouseEventArgs e)
    {
      if (!(sender is TextBlock textBlock))
        return;
      textBlock.TextDecorations = (TextDecorationCollection) null;
    }

    private async void OnLinkBoxClick(object sender, RoutedEventArgs e)
    {
      if (!(sender is System.Windows.Controls.TextBox box))
      {
        box = (System.Windows.Controls.TextBox) null;
      }
      else
      {
        await Task.Delay(50);
        box.SelectAll();
        box = (System.Windows.Controls.TextBox) null;
      }
    }

    private void OnSaveClick(object sender, RoutedEventArgs e) => this.Close();

    private void OnInviteClick(object sender, RoutedEventArgs e)
    {
      if (!(this.SelectedUsers.ItemsSource is ObservableCollection<InviteSelectViewModel> itemsSource))
        return;
      List<string> list = itemsSource.Where<InviteSelectViewModel>((Func<InviteSelectViewModel, bool>) (m => !string.IsNullOrWhiteSpace(m.Email))).Select<InviteSelectViewModel, string>((Func<InviteSelectViewModel, string>) (m => m.Email)).ToList<string>();
      if (list.Count <= 0)
        return;
      this.InviteUserByEmails(list);
    }

    private void ReturnClick(object sender, RoutedEventArgs e)
    {
      if (this.SelectedUsers.ItemsSource is ObservableCollection<InviteSelectViewModel> itemsSource)
      {
        // ISSUE: explicit non-virtual call
        __nonvirtual (itemsSource.Clear());
      }
      this.SetInviteView();
    }

    private void OnInputKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
      this._canInputDelete = false;
      if (e.Key == Key.Back && sender is System.Windows.Controls.TextBox textBox && textBox.CaretIndex == 0 && string.IsNullOrEmpty(textBox.Text))
      {
        this._canInputDelete = true;
      }
      else
      {
        if (!(this.SelectedUsers.ItemsSource is ObservableCollection<InviteSelectViewModel> itemsSource))
          return;
        foreach (InviteSelectViewModel inviteSelectViewModel in (Collection<InviteSelectViewModel>) itemsSource)
        {
          if (inviteSelectViewModel.PreDelete)
            inviteSelectViewModel.PreDelete = false;
        }
      }
    }

    private void OnInputKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
    {
      if (e.Key == Key.Back && this._canInputDelete)
      {
        if (!(this.SelectedUsers.ItemsSource is ObservableCollection<InviteSelectViewModel> itemsSource) || itemsSource.Count < 2)
          return;
        InviteSelectViewModel inviteSelectViewModel = itemsSource[itemsSource.Count - 2];
        if (inviteSelectViewModel.PreDelete)
          this.OnSelectedUserChanged(new List<(string, string)>()
          {
            ((string) null, inviteSelectViewModel.Email)
          }, false);
        else
          inviteSelectViewModel.PreDelete = true;
      }
      else
        this.addUserTextBox_KeyUp(sender, e);
    }

    private void OnDeleteSelectClick(object sender, MouseButtonEventArgs e)
    {
      if (!(sender is Border border) || !(border.DataContext is InviteSelectViewModel dataContext))
        return;
      this.OnSelectedUserChanged(new List<(string, string)>()
      {
        (dataContext.DisplayName, dataContext.Email)
      }, false);
      e.Handled = true;
    }

    private async void OnSelectedUserChanged(List<(string, string)> userEmails, bool isAdd)
    {
      ShareProjectDialog shareProjectDialog = this;
      if (shareProjectDialog.SelectedUsers.ItemsSource is ObservableCollection<InviteSelectViewModel> selected)
      {
        // ISSUE: reference to a compiler-generated method
        int num1 = selected.Count<InviteSelectViewModel>(new Func<InviteSelectViewModel, bool>(shareProjectDialog.\u003COnSelectedUserChanged\u003Eb__70_0));
        // ISSUE: reference to a compiler-generated method
        int num2 = userEmails.Count<(string, string)>(new Func<(string, string), bool>(shareProjectDialog.\u003COnSelectedUserChanged\u003Eb__70_1));
        bool flag = isAdd;
        if (flag)
          flag = !await shareProjectDialog.CheckShareLimit(num2 + num1);
        if (flag)
        {
          selected = (ObservableCollection<InviteSelectViewModel>) null;
          return;
        }
        if (isAdd)
        {
          if (selected.Count == 0)
            selected.Add(new InviteSelectViewModel()
            {
              IsInput = true
            });
          foreach ((_, _) in userEmails)
          {
            (string, string) user;
            if (selected.All<InviteSelectViewModel>((Func<InviteSelectViewModel, bool>) (m => m.Email != user.Item2)))
              selected.Insert(selected.Count - 1, new InviteSelectViewModel()
              {
                Email = user.Item2,
                DisplayName = string.IsNullOrEmpty(user.Item1) ? user.Item2 : user.Item1
              });
          }
        }
        else
        {
          foreach ((_, _) in userEmails)
          {
            (string, string) email;
            selected.Remove(selected.FirstOrDefault<InviteSelectViewModel>((Func<InviteSelectViewModel, bool>) (m => m.Email == email.Item2)));
          }
          if (selected.Count == 1 && selected[0].IsInput)
            selected.Clear();
        }
      }
      shareProjectDialog.SetInviteView();
      selected = (ObservableCollection<InviteSelectViewModel>) null;
    }

    private void SetInviteView()
    {
      // ISSUE: explicit non-virtual call
      this.SelectedUsersViewer.Visibility = (this.SelectedUsers.ItemsSource is ObservableCollection<InviteSelectViewModel> itemsSource ? (__nonvirtual (itemsSource.Count) > 0 ? 1 : 0) : 0) != 0 ? Visibility.Visible : Visibility.Collapsed;
      if (!this.SelectedUsersViewer.IsVisible)
      {
        this.addUserTextBox.Visibility = Visibility.Visible;
        this.BodyGrid.Visibility = Visibility.Visible;
        this.addUserTextBox.Focus();
        this.LinkShareGrid.Visibility = Visibility.Visible;
      }
      else
      {
        this.addUserTextBox.Visibility = Visibility.Collapsed;
        this.BodyGrid.Visibility = Visibility.Collapsed;
        this.LinkShareGrid.Visibility = Visibility.Collapsed;
      }
    }

    private void OnInputLoaded(object sender, RoutedEventArgs e)
    {
      if (!(sender is System.Windows.Controls.TextBox textBox))
        return;
      textBox.Focus();
    }

    private void OnUserComboBoxItemSelect(object sender, ComboBoxViewModel e)
    {
      if (!(e is ShareContactsViewModel contactsViewModel))
        return;
      if (!this.CheckInvited(contactsViewModel.email))
        this.OnSelectedUserChanged(new List<(string, string)>()
        {
          (contactsViewModel.IsNewAdd ? contactsViewModel.email : contactsViewModel.displayName, contactsViewModel.email)
        }, true);
      else
        this.Toast(Utils.GetString("ShareUserExist"));
      this.addUserTextBox.Text = string.Empty;
      System.Windows.Controls.TextBox singleVisualChildren = Utils.FindSingleVisualChildren<System.Windows.Controls.TextBox>((DependencyObject) this.SelectedUsersViewer);
      if (singleVisualChildren == null)
        return;
      singleVisualChildren.Text = string.Empty;
    }

    private bool CheckInvited(string email)
    {
      return this._shareUserList.Any<ShareUserViewMode>((Func<ShareUserViewMode, bool>) (m => m.username == email));
    }

    public async void OpenToTeamChanged(bool isChecked)
    {
      ShareProjectDialog owner = this;
      (string, bool) team = await Communicator.SetProjectOpenToTeam(owner._project.id, isChecked);
      string str = team.Item1;
      if (team.Item2)
      {
        owner._project.openToTeam = new bool?(isChecked);
        ProjectModel cacheProject = CacheManager.GetProjectById(owner._project.id);
        if (cacheProject != null)
          cacheProject.openToTeam = new bool?(isChecked);
        await owner.LoadRemoteUsers(isChecked);
        ProjectModel project = owner._project;
        ProjectModel projectModel = cacheProject;
        int num1 = projectModel != null ? projectModel.userCount : owner._project.userCount;
        project.userCount = num1;
        int num2 = await ProjectDao.TryUpdateProject(owner._project);
        ListViewContainer.ReloadProjectData(false);
        if (string.IsNullOrEmpty(owner._project.teamMemberPermission))
        {
          owner._project.teamMemberPermission = "write";
          await owner.UploadProject();
        }
        else
        {
          if (!isChecked)
          {
            ShareUserViewMode shareUserViewMode = owner._shareUserList.FirstOrDefault<ShareUserViewMode>((Func<ShareUserViewMode, bool>) (m => m.userId == (long) Utils.GetCurrentUserIdInt()));
            if (shareUserViewMode != null && !shareUserViewMode.isOwner && shareUserViewMode.permission != "write")
            {
              owner._project.permission = shareUserViewMode.permission;
              owner.SetNoPermission();
            }
          }
          SyncManager.Sync();
        }
        cacheProject = (ProjectModel) null;
      }
      else
      {
        try
        {
          if (JsonConvert.DeserializeObject<ErrorModel>(str).errorCode == "team_project_exceed_quota")
            ProChecker.ShowUpgradeDialog(ProType.TeamShareLimit, (Window) owner, owner._project.teamId);
        }
        catch
        {
        }
        UtilLog.Info("SetProjectOpenToTeamFail " + str);
      }
    }

    private async void OnNeedAuditChanged(object sender, bool e)
    {
      this._project.needAudit = new bool?(e);
      UserActCollectUtils.AddClickEvent("collaborate", "approval_by_owner_is_required", e ? "enable" : "disable");
      await this.UploadProject();
    }

    private void OnNotifyClick(object sender, MouseButtonEventArgs e)
    {
      this.MessagePopup.IsOpen = true;
    }

    private void OnTextBoxFocus(object sender, RoutedEventArgs e)
    {
      this.InputBorder.SetResourceReference(Border.BorderBrushProperty, (object) "PrimaryColor");
    }

    private void OnTextBoxLostFocus(object sender, RoutedEventArgs e)
    {
      this.InputBorder.SetResourceReference(Border.BorderBrushProperty, (object) "BaseColorOpacity10");
    }

    private void OnScrollMouseUp(object sender, MouseButtonEventArgs e)
    {
      Utils.FindSingleVisualChildren<System.Windows.Controls.TextBox>((DependencyObject) this.SelectedUsersViewer)?.Focus();
    }

    private void OnInputBorderSizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (!this.userContactsComboBox.IsOpen)
        return;
      this.userContactsComboBox.IsOpen = false;
      this.userContactsComboBox.IsOpen = true;
    }

    public void Toast(string toastText)
    {
      this.ToastBorder.Visibility = Visibility.Visible;
      this.ToastText.Text = toastText;
      this.ToastBorder.BeginStoryboard((Storyboard) this.FindResource((object) "ShowToast"));
    }

    private void OnToasted(object sender, EventArgs e)
    {
      this.ToastBorder.Visibility = Visibility.Collapsed;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      System.Windows.Application.LoadComponent((object) this, new Uri("/TickTick;component/views/project/shareprojectdialog.xaml", UriKind.Relative));
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
          ((Timeline) target).Completed += new EventHandler(this.OnToasted);
          break;
        case 4:
          this.EmailInviteGrid = (Grid) target;
          break;
        case 5:
          this.userContactsComboBox = (CustomComboBox) target;
          break;
        case 6:
          this.InputBorder = (Border) target;
          this.InputBorder.SizeChanged += new SizeChangedEventHandler(this.OnInputBorderSizeChanged);
          break;
        case 7:
          this.addUserTextBox = (System.Windows.Controls.TextBox) target;
          this.addUserTextBox.GotFocus += new RoutedEventHandler(this.OnTextBoxFocus);
          this.addUserTextBox.LostFocus += new RoutedEventHandler(this.OnTextBoxLostFocus);
          this.addUserTextBox.KeyUp += new System.Windows.Input.KeyEventHandler(this.addUserTextBox_KeyUp);
          break;
        case 8:
          this.InviteImage = (HoverIconButton) target;
          break;
        case 9:
          this.InvitePopup = (EscPopup) target;
          break;
        case 10:
          this.EmailJoinPermission = (ProjectPermissionSetControl) target;
          break;
        case 11:
          this.SelectedUsersViewer = (ScrollViewer) target;
          this.SelectedUsersViewer.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnScrollMouseUp);
          break;
        case 12:
          this.SelectedUsers = (ItemsControl) target;
          break;
        case 13:
          this.addUserProgressBar = (System.Windows.Controls.ProgressBar) target;
          break;
        case 14:
          this.ApplyEditGrid = (StackPanel) target;
          break;
        case 15:
          this.PermissionNotEditText = (TextBlock) target;
          break;
        case 16:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.ApplyEdit);
          break;
        case 17:
          this.ApplyEditText = (TextBlock) target;
          this.ApplyEditText.MouseEnter += new System.Windows.Input.MouseEventHandler(this.ShowUnderLine);
          this.ApplyEditText.MouseLeave += new System.Windows.Input.MouseEventHandler(this.HideUnderLine);
          break;
        case 18:
          this.BodyGrid = (Grid) target;
          break;
        case 19:
          this.NotifyButton = (TextBlock) target;
          this.NotifyButton.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnNotifyClick);
          break;
        case 20:
          this.MemberList = (ScrollViewer) target;
          break;
        case 21:
          this.shareUserListView = (ItemsControl) target;
          break;
        case 22:
          this.MemberEmpty = (Grid) target;
          break;
        case 23:
          this.MessagePopup = (EscPopup) target;
          break;
        case 24:
          this.NotificationSetting = (TeamNotification) target;
          break;
        case 25:
          this.PersonalPage = (Grid) target;
          break;
        case 26:
          this.LinkShareGrid = (Grid) target;
          break;
        case 27:
          this.LinkFirstRow = (RowDefinition) target;
          break;
        case 28:
          this.ShareLinkCheckBox = (System.Windows.Controls.CheckBox) target;
          this.ShareLinkCheckBox.PreviewMouseLeftButtonDown += new MouseButtonEventHandler(this.ShareLinkCheckBoxClick);
          break;
        case 29:
          this.EnableLinkText = (TextBlock) target;
          break;
        case 30:
          this.LinkJoinPermission = (ProjectPermissionSetControl) target;
          break;
        case 31:
          this.CopyLink = (TextBlock) target;
          this.CopyLink.PreviewMouseLeftButtonUp += new MouseButtonEventHandler(this.CopyShareLink);
          break;
        case 32:
          this.ToastBorder = (Border) target;
          break;
        case 33:
          this.ToastText = (TextBlock) target;
          break;
        case 34:
          this.ButtonGrid = (Grid) target;
          break;
        case 35:
          ((System.Windows.Controls.Primitives.ButtonBase) target).Click += new RoutedEventHandler(this.ReturnClick);
          break;
        case 36:
          this.SaveButton = (System.Windows.Controls.Button) target;
          this.SaveButton.Click += new RoutedEventHandler(this.OnInviteClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IStyleConnector.Connect(int connectionId, object target)
    {
      if (connectionId != 2)
      {
        if (connectionId != 3)
          return;
        ((UIElement) target).GotFocus += new RoutedEventHandler(this.OnTextBoxFocus);
        ((UIElement) target).LostFocus += new RoutedEventHandler(this.OnTextBoxLostFocus);
        ((UIElement) target).PreviewKeyDown += new System.Windows.Input.KeyEventHandler(this.OnInputKeyDown);
        ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnInputLoaded);
        ((UIElement) target).KeyUp += new System.Windows.Input.KeyEventHandler(this.OnInputKeyUp);
      }
      else
        ((UIElement) target).MouseLeftButtonUp += new MouseButtonEventHandler(this.OnDeleteSelectClick);
    }
  }
}
