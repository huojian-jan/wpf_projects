// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.Misc.NotificationListItem
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Util;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView;
using TickTickModels;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Views.Misc
{
  public class NotificationListItem : UserControl, IComponentConnector
  {
    private NotificationViewModel _notificationItem;
    internal TextBlock TitleTextBlock;
    private bool _contentLoaded;

    public NotificationListItem() => this.InitializeComponent();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      this._notificationItem = this.DataContext as NotificationViewModel;
      this.GetTitle(this._notificationItem?.notification);
    }

    private void OnTitleClick(object sender, MouseButtonEventArgs e)
    {
      if ((sender is TextBlock textBlock ? textBlock.Tag : (object) null) == null)
        return;
      string fileName = ((FrameworkElement) sender).Tag.ToString();
      if (string.IsNullOrEmpty(fileName))
        return;
      try
      {
        Process.Start(fileName);
      }
      catch (Exception ex)
      {
      }
    }

    private async void OnNotificationAcceptClick(object sender, RoutedEventArgs e)
    {
      NotificationListItem notificationListItem = this;
      if (notificationListItem._notificationItem == null)
        return;
      notificationListItem._notificationItem.IsButtonEnable = "False";
      bool acceptShareReturn = false;
      List<ProjectModel> projects;
      switch (notificationListItem._notificationItem.actionStatus)
      {
        case 0:
          if (notificationListItem._notificationItem.type == "PayReminder")
          {
            string urlTemple = BaseUrl.GetDomainUrl() + "/sign/autoSignOn?token={0}&dest=/about/upgrade";
            string fileName = string.Format(urlTemple, (object) await Communicator.GetSignOnToken());
            try
            {
              Process.Start(fileName);
              break;
            }
            catch (Exception ex)
            {
              break;
            }
          }
          else
          {
            long projectLimitNumber = Utils.GetUserLimit(Constants.LimitKind.ProjectNumber);
            List<string> teamIds = CacheManager.GetTeams().Select<TeamModel, string>((Func<TeamModel, string>) (t => t.id)).ToList<string>();
            List<ProjectModel> list;
            if (!UserDao.IsPro())
              list = (await ProjectDao.GetAllProjects(false)).ToList<ProjectModel>();
            else
              list = (await ProjectDao.GetAllProjects(false, false)).Where<ProjectModel>((Func<ProjectModel, bool>) (p => !TeamDao.IsTeamExpired(p.teamId))).ToList<ProjectModel>();
            projects = list;
            projects = projects.Where<ProjectModel>((Func<ProjectModel, bool>) (p => string.IsNullOrEmpty(p.teamId) || teamIds.Contains(p.teamId))).ToList<ProjectModel>();
            if (projectLimitNumber <= (long) projects.Count)
            {
              if (UserDao.IsPro())
              {
                string content = string.Format(Utils.GetString("ListsLimitAdd"), (object) projects.Count);
                CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("ProjectLimitTips"), content, MessageBoxButton.OK);
                customerDialog.Owner = Window.GetWindow((DependencyObject) notificationListItem);
                customerDialog.ShowDialog();
              }
              else
                ProChecker.ShowUpgradeDialog(ProType.MoreLists);
              notificationListItem._notificationItem.IsButtonEnable = "True";
              return;
            }
            string str = await Communicator.SetAcceptShare(notificationListItem._notificationItem.projectid, notificationListItem._notificationItem.id, 1);
            if (string.IsNullOrEmpty(str))
            {
              acceptShareReturn = true;
              break;
            }
            TeamErrorDataModel teamErrorDataModel = (TeamErrorDataModel) null;
            ErrorModel errorModel = JsonConvert.DeserializeObject<ErrorModel>(str);
            try
            {
              teamErrorDataModel = JsonConvert.DeserializeObject<TeamErrorDataModel>(errorModel.data.ToString());
            }
            catch
            {
            }
            if (errorModel != null)
            {
              switch (errorModel.errorCode)
              {
                case "user_email_not_verified":
                  notificationListItem.ShowAcceptFailureDialog();
                  break;
                case "exceed_quota":
                  if (UserDao.IsPro())
                  {
                    string content = string.Format(Utils.GetString("ListsLimitAdd"), (object) projects.Count);
                    CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("ProjectLimitTips"), content, MessageBoxButton.OK);
                    customerDialog.Owner = Window.GetWindow((DependencyObject) notificationListItem);
                    customerDialog.ShowDialog();
                    break;
                  }
                  ProChecker.ShowUpgradeDialog(ProType.MoreLists);
                  break;
                case "team_project_exceed_quota":
                  new CustomerDialog(Utils.GetString("AddToFailed"), Utils.GetString("GuestTeamProjectExceedQuota"), Utils.GetString("GotIt"), "").ShowDialog();
                  break;
                case "visitor_exceed_quota":
                  JToken jtoken;
                  if (errorModel.data != null && errorModel.data.TryGetValue("limit", out jtoken))
                  {
                    int result;
                    int num = int.TryParse(jtoken.ToString(), out result) ? result : 29;
                    string content = string.Format(Utils.GetString(num <= 1 ? "FreeLimitedSharingGuests" : "ProLimitedSharingGuests"), (object) num);
                    CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), content, MessageBoxButton.OK);
                    customerDialog.Owner = (Window) App.Window;
                    customerDialog.ShowDialog();
                    return;
                  }
                  break;
                case "no_team_permission":
                  new CustomerDialog(Utils.GetString("AddToFailed"), string.Format(Utils.GetString("NoTeamPermission01"), (object) (teamErrorDataModel?.projectOwnerName ?? " ")), Utils.GetString("GotIt"), "").ShowDialog();
                  break;
                case "exceed_team_max_share_limit":
                  if (UserDao.IsPro())
                  {
                    long userLimit = Utils.GetUserLimit(Constants.LimitKind.ShareUserNumber);
                    string format = TeamDao.IsTeamValid() ? Utils.GetString("RecipientsTeamLimit") : Utils.GetString("RecipientsLimit");
                    new CustomerDialog(Utils.GetString("LimitTips"), string.Format(format, (object) userLimit), MessageBoxButton.OK).ShowDialog();
                    break;
                  }
                  ProChecker.ShowUpgradeDialog(ProType.MoreSharingMembers);
                  break;
                default:
                  new CustomerDialog(Utils.GetString("OperateFailed"), Utils.GetString("OperateFailedMessage"), Utils.GetString("GotIt"), "").ShowDialog();
                  break;
              }
            }
            else
              break;
          }
          break;
        case 1:
          if (notificationListItem._notificationItem.type == "projectPermission")
          {
            string str = await Communicator.AcceptApplyPermission(notificationListItem._notificationItem.id);
            if (string.IsNullOrEmpty(str))
            {
              notificationListItem._notificationItem.ButtonVisibility = "Collapsed";
              notificationListItem._notificationItem.ActionVisibility = "Visible";
              notificationListItem._notificationItem.ActionText = Utils.GetString("Accepted");
              AvatarHelper.ResetProjectAvatars(notificationListItem._notificationItem.notification.notificationUserData.projectId);
              SyncManager.Sync();
              break;
            }
            try
            {
              ErrorModel errorModel = JsonConvert.DeserializeObject<ErrorModel>(str);
              if (errorModel != null)
              {
                string errorCode = errorModel.errorCode;
              }
              new CustomerDialog(Utils.GetString("OperateFailed"), Utils.GetString("OperateFailedMessage"), Utils.GetString("GotIt"), "").ShowDialog();
              break;
            }
            catch (Exception ex)
            {
              Console.WriteLine((object) ex);
              throw;
            }
          }
          else
          {
            string str = await Communicator.AcceptTeamApply(notificationListItem._notificationItem.id);
            if (!string.IsNullOrEmpty(str))
            {
              string errorCode = JsonConvert.DeserializeObject<ErrorModel>(str)?.errorCode;
              if (errorCode == null)
                return;
              switch (errorCode.Length)
              {
                case 12:
                  switch (errorCode[0])
                  {
                    case 'e':
                      if (!(errorCode == "exceed_limit"))
                        return;
                      TeamMessageModel teamMessage = await Communicator.GetTeamMessage(notificationListItem._notificationItem.teamId);
                      if (teamMessage != null && teamMessage.trial)
                      {
                        CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("MemberCountLimit"), string.Format(Utils.GetString("TeamMemberCountLimitText"), (object) notificationListItem._notificationItem.teamName), Utils.GetString("BuyNow"), Utils.GetString("Cancel"));
                        customerDialog.ShowDialog();
                        if (!customerDialog.DialogResult.GetValueOrDefault())
                          return;
                        string signOnToken = await Communicator.GetSignOnToken();
                        Utils.TryProcessStartUrl(string.Format(BaseUrl.GetDomainUrl() + "/sign/autoSignOn?token={0}&dest={1}", (object) signOnToken, (object) string.Format(Utils.GetString("RenewTeamUrl"), (object) notificationListItem._notificationItem.teamId)));
                        return;
                      }
                      CustomerDialog customerDialog1 = new CustomerDialog(Utils.GetString("TeamMemberCountLimit"), string.Format(Utils.GetString("TeamMemberCountLimitText"), (object) notificationListItem._notificationItem.teamName), Utils.GetString("AddLimit"), Utils.GetString("Cancel"));
                      customerDialog1.ShowDialog();
                      if (!customerDialog1.DialogResult.GetValueOrDefault())
                        return;
                      string signOnToken1 = await Communicator.GetSignOnToken();
                      Utils.TryProcessStartUrl(string.Format(BaseUrl.GetDomainUrl() + "/sign/autoSignOn?token={0}&dest={1}", (object) signOnToken1, (object) string.Format(Utils.GetString("manageTeamUrl"), (object) notificationListItem._notificationItem.teamId)));
                      return;
                    case 't':
                      if (!(errorCode == "team_expired") || !new CustomerDialog(Utils.GetString("HandleDefeat"), string.Format(Utils.GetString("TeamExpired"), (object) notificationListItem._notificationItem.teamName), Utils.GetString("GotIt"), "").ShowDialog().GetValueOrDefault())
                        return;
                      string signOnToken2 = await Communicator.GetSignOnToken();
                      Utils.TryProcessStartUrl(string.Format(BaseUrl.GetDomainUrl() + "/sign/autoSignOn?token={0}&dest={1}", (object) signOnToken2, (object) string.Format(Utils.GetString("RenewTeamUrl"), (object) notificationListItem._notificationItem.teamId)));
                      return;
                    default:
                      return;
                  }
                case 13:
                  return;
                case 14:
                  return;
                case 15:
                  return;
                case 16:
                  return;
                case 17:
                  if (!(errorCode == "team_exceed_limit"))
                    return;
                  new CustomerDialog(Utils.GetString("HandleDefeat"), string.Format(Utils.GetString("UserTeamCountLimit"), (object) notificationListItem._notificationItem.UserDisplayName), Utils.GetString("GotIt"), "").ShowDialog();
                  return;
                case 18:
                  if (!(errorCode == "no_team_permission"))
                    return;
                  new CustomerDialog(Utils.GetString("HandleDefeat"), string.Format(Utils.GetString("TeamNotExist"), (object) notificationListItem._notificationItem.teamName), Utils.GetString("GotIt"), "").ShowDialog();
                  return;
                case 19:
                  if (!(errorCode == "no_admin_permission"))
                    return;
                  new CustomerDialog(Utils.GetString("HandleDefeat"), string.Format(Utils.GetString("NoAdminPermission"), (object) notificationListItem._notificationItem.teamName), Utils.GetString("GotIt"), "").ShowDialog();
                  return;
                case 20:
                  if (!(errorCode == "user_already_refused"))
                    return;
                  Utils.Toast(string.Format(Utils.GetString("OtherAdminRefused"), (object) notificationListItem._notificationItem.UserDisplayName, (object) notificationListItem._notificationItem.teamName));
                  notificationListItem._notificationItem.ButtonVisibility = "Collapsed";
                  notificationListItem._notificationItem.ActionVisibility = "Visible";
                  notificationListItem._notificationItem.ActionText = Utils.GetString("Declined");
                  return;
                case 21:
                  if (!(errorCode == "user_already_approved"))
                    return;
                  Utils.Toast(string.Format(Utils.GetString("OtherAdminAccepted"), (object) notificationListItem._notificationItem.UserDisplayName, (object) notificationListItem._notificationItem.teamName));
                  notificationListItem._notificationItem.ButtonVisibility = "Collapsed";
                  notificationListItem._notificationItem.ActionVisibility = "Visible";
                  notificationListItem._notificationItem.ActionText = Utils.GetString("Accepted");
                  return;
                default:
                  return;
              }
            }
            else
            {
              notificationListItem._notificationItem.ButtonVisibility = "Collapsed";
              notificationListItem._notificationItem.ActionVisibility = "Visible";
              notificationListItem._notificationItem.ActionText = Utils.GetString("Accepted");
              SyncManager.Sync();
              return;
            }
          }
        case 9:
          string str1 = await Communicator.HandleShareApplication(notificationListItem._notificationItem.id, "accept");
          if (string.IsNullOrEmpty(str1))
          {
            acceptShareReturn = true;
            break;
          }
          TeamErrorDataModel teamErrorDataModel1 = (TeamErrorDataModel) null;
          ErrorModel errorModel1 = JsonConvert.DeserializeObject<ErrorModel>(str1);
          try
          {
            teamErrorDataModel1 = JsonConvert.DeserializeObject<TeamErrorDataModel>(errorModel1.data.ToString());
          }
          catch
          {
          }
          if (errorModel1 != null)
          {
            switch (errorModel1.errorCode)
            {
              case "no_team_permission":
                new CustomerDialog(Utils.GetString("HandleDefeat"), string.Format(Utils.GetString("NoTeamPermission02"), (object) (teamErrorDataModel1?.applyUserName ?? " "), (object) (teamErrorDataModel1?.teamName ?? " ")), Utils.GetString("GotIt"), "").ShowDialog();
                break;
              case "team_project_exceed_quota":
                JToken jtoken1;
                if (errorModel1.data != null && errorModel1.data.TryGetValue("teamId", out jtoken1))
                {
                  ProChecker.ShowUpgradeDialog(ProType.TeamShareLimit, (Window) App.Window, jtoken1.ToString());
                  return;
                }
                break;
              case "visitor_exceed_quota":
                JToken jtoken2;
                JToken jtoken3;
                if (errorModel1.data != null && errorModel1.data.TryGetValue("teamId", out jtoken2) && errorModel1.data.TryGetValue("limit", out jtoken3))
                {
                  int result;
                  int num = int.TryParse(jtoken3.ToString(), out result) ? result : 29;
                  TeamModel teamById = CacheManager.GetTeamById(jtoken2.ToString());
                  if (num >= 29)
                  {
                    CustomerDialog customerDialog = new CustomerDialog(Utils.GetString("LimitTips"), string.Format(Utils.GetString("ProLimitedSharingGuests"), (object) num), MessageBoxButton.OK);
                    customerDialog.Owner = (Window) App.Window;
                    customerDialog.ShowDialog();
                    return;
                  }
                  ProChecker.ShowUpgradeDialog(ProType.MoreTeamGuest, teamId: teamById?.id);
                  return;
                }
                break;
              case "exceed_quota":
                new CustomerDialog(Utils.GetString("HandleDefeat"), Utils.GetString("InviteeProjectOverLimit"), Utils.GetString("GotIt"), "").ShowDialog();
                break;
              case "exceed_team_max_share_limit":
                if (UserDao.IsPro())
                {
                  long userLimit = Utils.GetUserLimit(Constants.LimitKind.ShareUserNumber);
                  string format = TeamDao.IsTeamValid() ? Utils.GetString("RecipientsTeamLimit") : Utils.GetString("RecipientsLimit");
                  new CustomerDialog(Utils.GetString("LimitTips"), string.Format(format, (object) userLimit), MessageBoxButton.OK).ShowDialog();
                  break;
                }
                ProChecker.ShowUpgradeDialog(ProType.MoreSharingMembers);
                break;
              default:
                new CustomerDialog(Utils.GetString("OperateFailed"), Utils.GetString("OperateFailedMessage"), Utils.GetString("GotIt"), "").ShowDialog();
                break;
            }
          }
          else
            break;
          break;
        case 21:
          if (notificationListItem._notificationItem.type == "team")
          {
            if (string.IsNullOrEmpty(await Communicator.HandleTeamInvite(notificationListItem._notificationItem.id, true)))
            {
              notificationListItem._notificationItem.ButtonVisibility = "Collapsed";
              notificationListItem._notificationItem.ActionVisibility = "Visible";
              notificationListItem._notificationItem.ActionText = Utils.GetString("Accepted");
              SyncManager.Sync();
              break;
            }
            new CustomerDialog(Utils.GetString("OperateFailed"), Utils.GetString("OperateFailedMessage"), Utils.GetString("GotIt"), "").ShowDialog();
            break;
          }
          break;
      }
      projects = (List<ProjectModel>) null;
      if (acceptShareReturn)
      {
        notificationListItem._notificationItem.ButtonVisibility = "Collapsed";
        notificationListItem._notificationItem.ActionVisibility = "Visible";
        notificationListItem._notificationItem.ActionText = Utils.GetString("Accepted");
        if (notificationListItem._notificationItem.actionStatus == 0)
          await ListViewContainer.AcceptShareList(notificationListItem._notificationItem.projectid);
        SyncManager.Sync();
      }
      notificationListItem._notificationItem.IsButtonEnable = "True";
    }

    private async void ShowAcceptFailureDialog()
    {
      bool? nullable = new CustomerDialog(Utils.GetString("SendVerifyEmail"), Utils.GetString("SendVerifyEmailHint"), Utils.GetString("SendVerifyEmail"), Utils.GetString("Cancel")).ShowDialog();
      bool flag = true;
      if (!(nullable.GetValueOrDefault() == flag & nullable.HasValue))
        return;
      await Communicator.ResentVerifyEmail();
    }

    private async void NotificationDeclineClick(object sender, RoutedEventArgs e)
    {
      if (this._notificationItem == null)
        return;
      this._notificationItem.IsButtonEnable = "False";
      bool acceptShareReturn = false;
      switch (this._notificationItem.actionStatus)
      {
        case 0:
          acceptShareReturn = string.IsNullOrEmpty(await Communicator.SetAcceptShare(this._notificationItem.projectid, this._notificationItem.id, 2));
          break;
        case 1:
          if (this._notificationItem.type == "projectPermission")
          {
            if (string.IsNullOrEmpty(await Communicator.RefuseApplyPermission(this._notificationItem.id)))
            {
              this._notificationItem.ButtonVisibility = "Collapsed";
              this._notificationItem.ActionVisibility = "Visible";
              this._notificationItem.ActionText = Utils.GetString("Declined");
              break;
            }
            break;
          }
          string str = await Communicator.RefuseTeamApply(this._notificationItem.id);
          if (!string.IsNullOrEmpty(str))
          {
            switch (JsonConvert.DeserializeObject<ErrorModel>(str)?.errorCode)
            {
              case "no_admin_permission":
                new CustomerDialog(Utils.GetString("HandleDefeat"), string.Format(Utils.GetString("NoAdminPermission"), (object) this._notificationItem.teamName), Utils.GetString("GotIt"), "").ShowDialog();
                break;
              case "team_expired":
                if (new CustomerDialog(Utils.GetString("HandleDefeat"), string.Format(Utils.GetString("TeamExpired"), (object) this._notificationItem.UserDisplayName), Utils.GetString("GotIt"), "").ShowDialog().GetValueOrDefault())
                {
                  string signOnToken = await Communicator.GetSignOnToken();
                  Utils.TryProcessStartUrl(string.Format(BaseUrl.GetDomainUrl() + "/sign/autoSignOn?token={0}&dest={1}", (object) signOnToken, (object) string.Format(Utils.GetString("RenewTeamUrl"), (object) this._notificationItem.teamId)));
                  break;
                }
                break;
              case "user_already_approved":
                Utils.Toast(string.Format(Utils.GetString("OtherAdminAccepted"), (object) this._notificationItem.UserDisplayName, (object) this._notificationItem.teamName));
                this._notificationItem.ButtonVisibility = "Collapsed";
                this._notificationItem.ActionVisibility = "Visible";
                this._notificationItem.ActionText = Utils.GetString("Accepted");
                break;
              case "user_already_refused":
                Utils.Toast(string.Format(Utils.GetString("OtherAdminRefused"), (object) this._notificationItem.UserDisplayName, (object) this._notificationItem.teamName));
                this._notificationItem.ButtonVisibility = "Collapsed";
                this._notificationItem.ActionVisibility = "Visible";
                this._notificationItem.ActionText = Utils.GetString("Declined");
                break;
              case "user_not_exist":
                new CustomerDialog(Utils.GetString("HandleDefeat"), string.Format(Utils.GetString("UserNotExist"), (object) this._notificationItem.UserDisplayName), Utils.GetString("GotIt"), "").ShowDialog();
                break;
              case "no_team_permission":
                new CustomerDialog(Utils.GetString("HandleDefeat"), string.Format(Utils.GetString("TeamNotExist"), (object) this._notificationItem.teamName), Utils.GetString("GotIt"), "").ShowDialog();
                break;
            }
          }
          else
          {
            acceptShareReturn = true;
            break;
          }
          break;
        case 9:
          acceptShareReturn = string.IsNullOrEmpty(await Communicator.HandleShareApplication(this._notificationItem.id, "refuse"));
          break;
        case 21:
          if (this._notificationItem.type == "team")
          {
            if (string.IsNullOrEmpty(await Communicator.HandleTeamInvite(this._notificationItem.id, false)))
            {
              this._notificationItem.ButtonVisibility = "Collapsed";
              this._notificationItem.ActionVisibility = "Visible";
              this._notificationItem.ActionText = Utils.GetString("Declined");
              break;
            }
            new CustomerDialog(Utils.GetString("OperateFailed"), Utils.GetString("OperateFailedMessage"), Utils.GetString("GotIt"), "").ShowDialog();
            break;
          }
          break;
      }
      if (acceptShareReturn)
      {
        this._notificationItem.ButtonVisibility = "Collapsed";
        this._notificationItem.ActionVisibility = "Visible";
        this._notificationItem.ActionText = Utils.GetString("Declined");
      }
      this._notificationItem.IsButtonEnable = "True";
    }

    private void GetTitle(NotificationModel notification)
    {
      if (notification == null)
        return;
      Utils.IsCn();
      string type = notification.type;
      if (type != null)
      {
        switch (type.Length)
        {
          case 4:
            switch (type[1])
            {
              case 'a':
                if (type == "task")
                {
                  string title = (string) null;
                  switch (notification.actionStatus)
                  {
                    case 1:
                      title = Utils.GetString("TeamAddTask");
                      break;
                    case 2:
                      title = Utils.GetString("TeamDoTask");
                      break;
                    case 3:
                      title = Utils.GetString("TeamUnDoTask");
                      break;
                    case 4:
                      title = Utils.GetString("TeamDelTask");
                      break;
                    case 5:
                      title = Utils.GetString("TeamMoveTask");
                      break;
                    case 6:
                      title = Utils.GetString("TeamAbandonedTask");
                      break;
                    case 7:
                      title = Utils.GetString("TeamReopenTask");
                      break;
                  }
                  if (title != null)
                  {
                    this.TitleTextBlock.Inlines.Clear();
                    using (List<string>.Enumerator enumerator = this.GetRunTextList(title).GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        string current = enumerator.Current;
                        switch (current)
                        {
                          case "{0}":
                            continue;
                          case "{1}":
                            this.TitleTextBlock.Inlines.Add((Inline) this.GetProjectRun(notification.notificationUserData.projectName, notification.notificationUserData.projectId));
                            continue;
                          case "{2}":
                            this.TitleTextBlock.Inlines.Add((Inline) this.GetTaskRun(notification.notificationUserData.title, notification.notificationUserData.taskId));
                            continue;
                          default:
                            if (!string.IsNullOrEmpty(current))
                            {
                              string text = current;
                              if (this.TitleTextBlock.Inlines.Count == 0)
                                text = current.TrimStart();
                              this.TitleTextBlock.Inlines.Add((Inline) new Run(text));
                              continue;
                            }
                            continue;
                        }
                      }
                      break;
                    }
                  }
                  else
                    break;
                }
                else
                  break;
              case 'e':
                if (type == "team")
                {
                  switch (notification.actionStatus)
                  {
                    case 1:
                    case 15:
                    case 16:
                    case 20:
                      this._notificationItem.Title = string.Format(Utils.GetString("TeamAttendApply"), (object) notification.notificationUserData.userDisplayName, (object) notification.notificationUserData.teamName);
                      break;
                    case 2:
                      this._notificationItem.Title = string.Format(Utils.GetString("TeamAttendAdminAccepted"), (object) notification.notificationUserData.adminDisplayName, (object) notification.notificationUserData.userDisplayName, (object) notification.notificationUserData.teamName);
                      break;
                    case 3:
                      this._notificationItem.Title = string.Format(Utils.GetString("TeamAttendAdminRefused"), (object) notification.notificationUserData.adminDisplayName, (object) notification.notificationUserData.userDisplayName, (object) notification.notificationUserData.teamName);
                      break;
                    case 4:
                      this._notificationItem.Title = string.Format(Utils.GetString("TeamAttendAccepted"), (object) notification.notificationUserData.teamName);
                      break;
                    case 5:
                      this._notificationItem.Title = string.Format(Utils.GetString("TeamAttendRefused"), (object) notification.notificationUserData.teamName);
                      break;
                    case 6:
                      this._notificationItem.Title = string.Format(Utils.GetString("TeamExit"), (object) notification.notificationUserData.userDisplayName, (object) notification.notificationUserData.teamName);
                      break;
                    case 7:
                      this._notificationItem.Title = string.Format(Utils.GetString("TeamProjectHandle"), (object) notification.notificationUserData.userDisplayName, (object) notification.notificationUserData.teamName, (object) notification.notificationUserData.projectCount);
                      break;
                    case 8:
                      this._notificationItem.Title = string.Format(Utils.GetString("RemovedByAdmin"), (object) notification.notificationUserData.adminDisplayName, (object) notification.notificationUserData.userDisplayName, (object) notification.notificationUserData.teamName);
                      break;
                    case 9:
                      this._notificationItem.Title = string.Format(Utils.GetString("RemovedByAdmin"), (object) notification.notificationUserData.adminDisplayName, (object) Utils.GetString("You"), (object) notification.notificationUserData.teamName);
                      break;
                    case 10:
                      this._notificationItem.Title = string.Format(Utils.GetString("TeamProjectHandle"), (object) Utils.GetString("Admin"), (object) notification.notificationUserData.teamName, (object) notification.notificationUserData.projectCount);
                      break;
                    case 11:
                      this._notificationItem.Title = string.Format(Utils.GetString("BecomeAdmin"), (object) notification.notificationUserData.userDisplayName, (object) notification.notificationUserData.teamName);
                      break;
                    case 12:
                      this._notificationItem.Title = string.Format(Utils.GetString("BecomeNormal"), (object) notification.notificationUserData.userDisplayName, (object) notification.notificationUserData.teamName);
                      break;
                    case 13:
                    case 14:
                      this._notificationItem.Title = string.Format(Utils.GetString("BecomeSuper"), (object) notification.notificationUserData.teamName);
                      break;
                    case 17:
                      this._notificationItem.Title = string.Format(Utils.GetString("TeamDeleted"), (object) notification.notificationUserData.teamName);
                      break;
                    case 21:
                    case 22:
                    case 23:
                      this._notificationItem.Title = string.Format(Utils.GetString("NotificationForTeam21"), (object) notification.notificationUserData.adminDisplayName, (object) notification.notificationUserData.teamName);
                      break;
                    case 24:
                      this._notificationItem.Title = string.Format(Utils.GetString("NotificationForTeam24"), (object) notification.notificationUserData.adminDisplayName, (object) notification.notificationUserData.teamName);
                      break;
                    case 25:
                      this._notificationItem.Title = string.Format(Utils.GetString("NotificationForTeam25"), (object) notification.notificationUserData.adminDisplayName, (object) notification.notificationUserData.teamName);
                      break;
                  }
                }
                else
                  break;
                break;
            }
            break;
          case 5:
            switch (type[1])
            {
              case 'c':
                if (type == "score")
                {
                  string title = Utils.GetString("TicketScoreTitle");
                  if (title != null)
                  {
                    this.TitleTextBlock.Inlines.Clear();
                    using (List<string>.Enumerator enumerator = this.GetRunTextList(title).GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        string current = enumerator.Current;
                        this.TitleTextBlock.Inlines.Add(current == "{0}" ? (Inline) this.GetTicketRun(notification.notificationUserData.ticketDescription, notification.notificationUserData.ticketId) : (Inline) new Run(current));
                      }
                      break;
                    }
                  }
                  else
                    break;
                }
                else
                  break;
              case 'h':
                if (type == "share")
                {
                  string title = (string) null;
                  switch (notification.actionStatus)
                  {
                    case 0:
                    case 2:
                    case 3:
                      this._notificationItem.Title = string.Format(Utils.GetString("NotificationTitleShareYou"), (object) notification.notificationUserData.entityName);
                      break;
                    case 1:
                      title = Utils.GetString("NotificationTitleShareYou");
                      break;
                    case 4:
                      title = Utils.GetString("NotificationTitleJoin");
                      break;
                    case 5:
                      title = Utils.GetString("NotificationTitleDeclineYou");
                      break;
                    case 6:
                      this._notificationItem.Title = string.Format(Utils.GetString("NotificationTitleDelete"), (object) notification.notificationUserData.entityName);
                      break;
                    case 7:
                      this._notificationItem.Title = string.Format(Utils.GetString("NotificationTitleRemoveYou"), (object) notification.notificationUserData.entityName);
                      break;
                    case 8:
                      title = Utils.GetString("NotificationTitleLeave");
                      break;
                    case 9:
                    case 10:
                    case 11:
                      title = Utils.GetString("NotificationTitleWant");
                      break;
                    case 12:
                      title = Utils.GetString("NotificationTitleAgree");
                      break;
                    case 13:
                      this._notificationItem.Title = string.Format(Utils.GetString("NotificationTitleDisAgree"), (object) notification.notificationUserData.entityName);
                      break;
                    case 14:
                      title = Utils.GetString("TransferListNotification");
                      break;
                    case 15:
                      title = Utils.GetString("NotificationForShare15");
                      break;
                  }
                  if (title != null)
                  {
                    this.TitleTextBlock.Inlines.Clear();
                    using (List<string>.Enumerator enumerator = this.GetRunTextList(title).GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        string current = enumerator.Current;
                        this.TitleTextBlock.Inlines.Add(current == "{0}" ? (Inline) this.GetProjectRun(notification.notificationUserData.entityName, notification.notificationUserData.entityId) : (Inline) new Run(current));
                      }
                      break;
                    }
                  }
                  else
                    break;
                }
                else
                  break;
            }
            break;
          case 6:
            switch (type[1])
            {
              case 's':
                if (type == "assign")
                {
                  List<string> runTextList = this.GetRunTextList(Utils.GetString("NotificationTitleAssign"));
                  this.TitleTextBlock.Inlines.Clear();
                  using (List<string>.Enumerator enumerator = runTextList.GetEnumerator())
                  {
                    while (enumerator.MoveNext())
                    {
                      string current = enumerator.Current;
                      switch (current)
                      {
                        case "{0}":
                          this.TitleTextBlock.Inlines.Add((Inline) this.GetProjectRun(notification.notificationUserData.projectName, notification.notificationUserData.projectId));
                          continue;
                        case "{1}":
                          this.TitleTextBlock.Inlines.Add((Inline) this.GetTaskRun(notification.notificationUserData.taskTitle, notification.notificationUserData.taskId));
                          continue;
                        default:
                          this.TitleTextBlock.Inlines.Add((Inline) new Run(current));
                          continue;
                      }
                    }
                    break;
                  }
                }
                else
                  break;
              case 't':
                if (type == "attend")
                {
                  string title = (string) null;
                  switch (notification.actionStatus)
                  {
                    case 1:
                    case 2:
                    case 3:
                      title = Utils.GetString("SaveAgendaNotification");
                      break;
                    case 4:
                    case 5:
                    case 6:
                      title = Utils.GetString("OwnerDeletedSharedAgenda");
                      break;
                  }
                  if (title != null)
                  {
                    this.TitleTextBlock.Inlines.Clear();
                    using (List<string>.Enumerator enumerator = this.GetRunTextList(title).GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        string current = enumerator.Current;
                        switch (current)
                        {
                          case "{0}":
                            this.TitleTextBlock.Inlines.Add((Inline) new Run(notification.notificationUserData.userDisplayName));
                            continue;
                          case "{1}":
                            this.TitleTextBlock.Inlines.Add((Inline) this.GetTaskRun(notification.notificationUserData.title, notification.notificationUserData.entityId));
                            continue;
                          default:
                            this.TitleTextBlock.Inlines.Add((Inline) new Run(current));
                            continue;
                        }
                      }
                      break;
                    }
                  }
                  else
                    break;
                }
                else
                  break;
            }
            break;
          case 7:
            switch (type[0])
            {
              case 'c':
                if (type == "comment")
                {
                  string title = (string) null;
                  switch (notification.notificationUserData.action)
                  {
                    case "MENTION":
                      title = Utils.GetString("NotificationCommentMention");
                      break;
                    case "REPLY":
                      title = Utils.GetString("NotificationCommentReply");
                      break;
                    case "COMMENT":
                      title = Utils.GetString("NotificationCommentAdd");
                      break;
                  }
                  if (title != null)
                  {
                    this.TitleTextBlock.Inlines.Clear();
                    using (List<string>.Enumerator enumerator = this.GetRunTextList(title).GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        string current = enumerator.Current;
                        switch (current)
                        {
                          case "{0}":
                            this.TitleTextBlock.Inlines.Add((Inline) this.GetTaskRun(notification.notificationUserData.taskTitle, notification.notificationUserData.taskId));
                            continue;
                          case "{1}":
                            this.TitleTextBlock.Inlines.Add((Inline) new Run(notification.notificationUserData.title));
                            continue;
                          default:
                            this.TitleTextBlock.Inlines.Add((Inline) new Run(current));
                            continue;
                        }
                      }
                      break;
                    }
                  }
                  else
                    break;
                }
                else
                  break;
              case 's':
                if (type == "support")
                {
                  string title = Utils.GetString("TicketNotificationTitle");
                  if (title != null)
                  {
                    this.TitleTextBlock.Inlines.Clear();
                    using (List<string>.Enumerator enumerator = this.GetRunTextList(title).GetEnumerator())
                    {
                      while (enumerator.MoveNext())
                      {
                        string current = enumerator.Current;
                        this.TitleTextBlock.Inlines.Add(current == "{0}" ? (Inline) this.GetTicketRun(notification.notificationUserData.ticketDescription, notification.notificationUserData.ticketId) : (Inline) new Run(current));
                      }
                      break;
                    }
                  }
                  else
                    break;
                }
                else
                  break;
              case 'u':
                if (type == "upgrade")
                  break;
                break;
            }
            break;
          case 10:
            if (type == "forumTopic")
            {
              this._notificationItem.Title = "The topic '" + notification.notificationUserData.topicTitle + "' has new reply.";
              this._notificationItem.Url = "https://help.ticktick.com" + notification.notificationUserData.topicUrl;
              break;
            }
            break;
          case 11:
            if (type == "PayReminder")
            {
              if (!notification.notificationUserData.expiryDate.HasValue || notification.notificationUserData.expiryDate.Value.Year == 1)
              {
                string title = Utils.GetString("NotificationRemiderToUpgrade");
                this.TitleTextBlock.Inlines.Clear();
                using (List<string>.Enumerator enumerator = this.GetRunTextList(title).GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    string current = enumerator.Current;
                    this.TitleTextBlock.Inlines.Add(current == "{0}" ? (Inline) this.GetProjectRun(notification.notificationUserData.projectName, notification.notificationUserData.projectId) : (Inline) new Run(current));
                  }
                  break;
                }
              }
              else
              {
                this._notificationItem.Title = string.Format(Utils.GetString("YourProWillOutDate"), (object) notification.notificationUserData.expiryDate.Value.ToString("m", (IFormatProvider) App.Ci));
                break;
              }
            }
            else
              break;
          case 15:
            if (type == "TeamPayReminder" && notification.notificationUserData.expiryDate.HasValue)
            {
              DateTime? expiryDate = notification.notificationUserData.expiryDate;
              ref DateTime? local = ref expiryDate;
              DateTime dateTime1;
              int? nullable1;
              if (!local.HasValue)
              {
                nullable1 = new int?();
              }
              else
              {
                dateTime1 = local.GetValueOrDefault();
                nullable1 = new int?(dateTime1.Year);
              }
              int? nullable2 = nullable1;
              dateTime1 = DateTime.Today;
              int year = dateTime1.Year;
              bool flag = nullable2.GetValueOrDefault() == year & nullable2.HasValue;
              DateTime dateTime2 = notification.notificationUserData.expiryDate.Value;
              this._notificationItem.Title = string.Format(Utils.GetString("TeamPayReminder"), (object) notification.notificationUserData.teamName, (object) dateTime2.ToString(flag ? "M" : "D", (IFormatProvider) App.Ci));
              break;
            }
            break;
          case 17:
            if (type == "projectPermission")
            {
              string title = (string) null;
              string text = (string) null;
              switch (notification.actionStatus)
              {
                case 1:
                case 4:
                case 5:
                  title = Utils.GetString("ApplyPermission");
                  text = notification.notificationUserData.applyUserDisplayName;
                  break;
                case 2:
                  title = Utils.GetString("ApplyPermissionAccepted");
                  text = notification.notificationUserData.ownerUserDisplayName;
                  break;
                case 3:
                  title = Utils.GetString("ApplyPermissionRefused");
                  text = notification.notificationUserData.ownerUserDisplayName;
                  break;
              }
              if (title != null)
              {
                this.TitleTextBlock.Inlines.Clear();
                using (List<string>.Enumerator enumerator = this.GetRunTextList(title).GetEnumerator())
                {
                  while (enumerator.MoveNext())
                  {
                    string current = enumerator.Current;
                    switch (current)
                    {
                      case "{0}":
                        this.TitleTextBlock.Inlines.Add((Inline) new Run(text));
                        continue;
                      case "{1}":
                        this.TitleTextBlock.Inlines.Add((Inline) this.GetProjectRun(notification.notificationUserData.projectName, notification.notificationUserData.projectId));
                        continue;
                      default:
                        this.TitleTextBlock.Inlines.Add((Inline) new Run(current));
                        continue;
                    }
                  }
                  break;
                }
              }
              else
                break;
            }
            else
              break;
        }
      }
      this.DataContext = (object) this._notificationItem;
      if (this._notificationItem.Title == null)
        return;
      this.TitleTextBlock.Text = this._notificationItem.Title;
    }

    private Run GetTaskRun(string text, string id)
    {
      if (text.Length > 400)
        text = text.Substring(0, 400) + "...";
      Run taskRun = new Run(text);
      taskRun.Background = (Brush) new SolidColorBrush(Colors.Transparent);
      taskRun.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
      taskRun.Cursor = Cursors.Hand;
      taskRun.MouseLeftButtonUp += (MouseButtonEventHandler) ((sender, e) =>
      {
        App.Window.NavigateTask(id, string.Empty);
        App.Window.LeftTabBar.NotificationPopup.IsOpen = false;
      });
      return taskRun;
    }

    private Run GetProjectRun(string text, string id)
    {
      Run projectRun;
      if (CacheManager.GetProjects().Exists((Predicate<ProjectModel>) (p => p.id == id)))
      {
        Run run = new Run(text);
        run.Background = (Brush) new SolidColorBrush(Colors.Transparent);
        run.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
        run.Cursor = Cursors.Hand;
        projectRun = run;
        projectRun.MouseLeftButtonUp += (MouseButtonEventHandler) ((sender, e) =>
        {
          App.Window.NavigateNormalProject(id);
          App.Window.LeftTabBar.NotificationPopup.IsOpen = false;
        });
      }
      else
        projectRun = new Run(text);
      return projectRun;
    }

    private Run GetTicketRun(string text, string id)
    {
      Run ticketRun = new Run(text);
      ticketRun.Background = (Brush) new SolidColorBrush(Colors.Transparent);
      ticketRun.Foreground = (Brush) ThemeUtil.GetColor("PrimaryColor");
      ticketRun.Cursor = Cursors.Hand;
      ticketRun.MouseLeftButtonUp += (MouseButtonEventHandler) (async (sender, e) =>
      {
        App.Window.LeftTabBar.NotificationPopup.IsOpen = false;
        if (InternetExplorerBrowserEmulation.IsVersion11())
        {
          try
          {
            int dotNetReleaseKey = Utils.GetDotNetReleaseKey();
            UtilLog.Info("db size : " + new FileInfo(AppPaths.TickTickDbPath).Length.ToString());
            UtilLog.Info("dotnet releaseKey : " + dotNetReleaseKey.ToString());
            UtilLog.Info("taskCount : " + TaskCache.LocalTaskViewModels.Count.ToString());
          }
          catch (Exception ex)
          {
          }
          NavigateWebBrowserWindow webBrowserWindow = new NavigateWebBrowserWindow("/v2/tickets/detail?id=" + id);
          webBrowserWindow.Show();
          webBrowserWindow.Topmost = true;
        }
        else
          this.JumpWebFeedBack(id);
      });
      return ticketRun;
    }

    private async void JumpWebFeedBack(string id)
    {
      Utils.TryProcessStartUrlWithToken("/v2/tickets/detail?id=" + id);
    }

    private List<string> GetRunTextList(string title)
    {
      List<string> runTextList = new List<string>();
      if (title.Contains("{"))
      {
        int num = title.IndexOf("{", StringComparison.Ordinal);
        runTextList.Add(title.Substring(0, num));
        runTextList.Add(title.Substring(num, 3));
        runTextList.AddRange((IEnumerable<string>) this.GetRunTextList(title.Substring(num + 3, title.Length - num - 3)));
      }
      else
        runTextList.Add(title);
      return runTextList;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    public void InitializeComponent()
    {
      if (this._contentLoaded)
        return;
      this._contentLoaded = true;
      Application.LoadComponent((object) this, new Uri("/TickTick;component/views/misc/notificationlistitem.xaml", UriKind.Relative));
    }

    [DebuggerNonUserCode]
    [GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    void IComponentConnector.Connect(int connectionId, object target)
    {
      switch (connectionId)
      {
        case 1:
          ((FrameworkElement) target).Loaded += new RoutedEventHandler(this.OnLoaded);
          break;
        case 2:
          this.TitleTextBlock = (TextBlock) target;
          this.TitleTextBlock.MouseLeftButtonUp += new MouseButtonEventHandler(this.OnTitleClick);
          break;
        case 3:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.OnNotificationAcceptClick);
          break;
        case 4:
          ((ButtonBase) target).Click += new RoutedEventHandler(this.NotificationDeclineClick);
          break;
        default:
          this._contentLoaded = true;
          break;
      }
    }
  }
}
