// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.InviteHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Views;
using ticktick_WPF.Views.Project;
using ticktick_WPF.Views.Team;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class InviteHelper
  {
    private static Dictionary<string, IEnumerable<InviteUserModel>> _teamContacts = new Dictionary<string, IEnumerable<InviteUserModel>>();
    private static Dictionary<string, IEnumerable<TeamMember>> _teamMembers = new Dictionary<string, IEnumerable<TeamMember>>();
    private static List<ProjectUsersMode> _projectUsersData = new List<ProjectUsersMode>();
    private static List<ShareContactsModel> _shareContacts = new List<ShareContactsModel>();
    private static List<InviteUserModel> _recentInviteUsers = new List<InviteUserModel>();

    private static async Task PullShareContacts()
    {
      List<ShareContactsModel> contactList = await Communicator.GetShareContactsList();
      if (contactList != null && contactList.Count != 0)
      {
        contactList.Sort((Comparison<ShareContactsModel>) ((a, b) => b.lstTime.CompareTo(a.lstTime)));
        List<InviteUserModel> recentInviteModels = new List<InviteUserModel>();
        List<UserPublicProfilesModel> usersInfoByUserCodes = await UserPublicProfilesDao.GetUsersInfoByUserCodes(contactList.Select<ShareContactsModel, string>((Func<ShareContactsModel, string>) (c => c.userCode)).ToList<string>());
        foreach (ShareContactsModel shareContactsModel in contactList)
        {
          ShareContactsModel item = shareContactsModel;
          if (item != null && !string.IsNullOrWhiteSpace(item.userCode))
          {
            UserPublicProfilesModel publicProfilesModel = usersInfoByUserCodes != null ? usersInfoByUserCodes.FirstOrDefault<UserPublicProfilesModel>((Func<UserPublicProfilesModel, bool>) (u => u.userCode == item.userCode)) : (UserPublicProfilesModel) null;
            item.avatarUrl = publicProfilesModel?.avatarUrl ?? string.Empty;
            string str = string.IsNullOrEmpty(publicProfilesModel?.nickName) ? publicProfilesModel?.displayName : publicProfilesModel.nickName;
            if (!string.IsNullOrEmpty(str))
            {
              item.displayName = str;
              if (!string.IsNullOrEmpty(item.email) && !Utils.IsFakeEmail(item.email))
                item.displayEmail = item.email;
            }
            else if (!string.IsNullOrEmpty(item.email) && !Utils.IsFakeEmail(item.email))
              item.displayName = item.email;
            item.siteId = (int?) publicProfilesModel?.siteId;
            InviteUserModel inviteUserModel = new InviteUserModel(item)
            {
              UserName = str
            };
            if (inviteUserModel.Id != LocalSettings.Settings.LoginUserId)
              recentInviteModels.Add(inviteUserModel);
          }
        }
        InviteHelper._recentInviteUsers = recentInviteModels;
        recentInviteModels = (List<InviteUserModel>) null;
      }
      InviteHelper._shareContacts = contactList;
      contactList = (List<ShareContactsModel>) null;
    }

    private static async Task PullTeamMembers(
      string teamId,
      string projectId = "",
      ShareProjectDialog window = null)
    {
      string teamMember = await Communicator.GetTeamMember(teamId);
      try
      {
        List<TeamMember> teamMemberList = JsonConvert.DeserializeObject<List<TeamMember>>(teamMember);
        teamMemberList?.Sort((Comparison<TeamMember>) ((a, b) => b.joinedTime.CompareTo(a.joinedTime)));
        if (InviteHelper._teamMembers.ContainsKey(teamId))
          InviteHelper._teamMembers[teamId] = (IEnumerable<TeamMember>) teamMemberList;
        else
          InviteHelper._teamMembers.Add(teamId, (IEnumerable<TeamMember>) teamMemberList);
      }
      catch (Exception ex)
      {
        if (window == null)
          ;
        else
        {
          ErrorModel errorModel = JsonConvert.DeserializeObject<ErrorModel>(teamMember);
          if (errorModel == null)
            ;
          else
          {
            ProjectModel project = CacheManager.GetProjects().FirstOrDefault<ProjectModel>((Func<ProjectModel, bool>) (p => p.id == projectId));
            switch (errorModel.errorCode)
            {
              case "team_not_exist":
                CustomerDialog customerDialog1 = new CustomerDialog(Utils.GetString("CanNotGetTeam"), string.Format(Utils.GetString("TeamNotExist"), (object) (CacheManager.GetTeams().FirstOrDefault<TeamModel>((Func<TeamModel, bool>) (team => team.id == project?.teamId))?.name ?? " ")), Utils.GetString("GotIt"), "");
                customerDialog1.Owner = (Window) window;
                if (!customerDialog1.ShowDialog().HasValue)
                  break;
                window.Close();
                break;
              case "no_team_permission":
                CustomerDialog customerDialog2 = new CustomerDialog(Utils.GetString("CanNotGetTeam"), string.Format(Utils.GetString("TeamNotExist"), (object) (CacheManager.GetTeams().FirstOrDefault<TeamModel>((Func<TeamModel, bool>) (team => team.id == project?.teamId))?.name ?? " ")), Utils.GetString("GotIt"), "");
                customerDialog2.Owner = (Window) window;
                if (!customerDialog2.ShowDialog().HasValue)
                  break;
                window.Close();
                break;
              default:
                break;
            }
          }
        }
      }
    }

    private static async Task PullTeamContacts(string teamId)
    {
      List<InviteUserModel> recentInviteModels = new List<InviteUserModel>();
      List<TeamContactsModel> teamContacts = await Communicator.GetTeamContactsList(teamId);
      if (teamContacts != null && teamContacts.Count != 0)
      {
        teamContacts.Sort((Comparison<TeamContactsModel>) ((a, b) => b.lstTime.CompareTo(a.lstTime)));
        List<UserPublicProfilesModel> usersInfoByUserCodes = await UserPublicProfilesDao.GetUsersInfoByUserCodes(teamContacts.Select<TeamContactsModel, string>((Func<TeamContactsModel, string>) (c => c.userCode)).ToList<string>());
        foreach (TeamContactsModel teamContactsModel in teamContacts)
        {
          TeamContactsModel item = teamContactsModel;
          if (item != null && !string.IsNullOrWhiteSpace(item.userCode))
          {
            UserPublicProfilesModel publicProfilesModel = usersInfoByUserCodes != null ? usersInfoByUserCodes.FirstOrDefault<UserPublicProfilesModel>((Func<UserPublicProfilesModel, bool>) (u => u.userCode == item.userCode)) : (UserPublicProfilesModel) null;
            if (publicProfilesModel != null)
            {
              long? userId = publicProfilesModel.userId;
              if (!(userId.ToString() == LocalSettings.Settings.LoginUserId))
              {
                InviteUserModel inviteUserModel1 = new InviteUserModel(item);
                userId = publicProfilesModel.userId;
                inviteUserModel1.Id = userId.ToString();
                inviteUserModel1.AvatarUrl = publicProfilesModel.avatarUrl ?? "";
                inviteUserModel1.UserName = string.IsNullOrEmpty(publicProfilesModel.nickName) ? publicProfilesModel.displayName : publicProfilesModel.nickName;
                InviteUserModel inviteUserModel2 = inviteUserModel1;
                inviteUserModel2.SetAvatar();
                recentInviteModels.Add(inviteUserModel2);
              }
            }
          }
        }
      }
      if (InviteHelper._teamContacts.ContainsKey(teamId))
      {
        InviteHelper._teamContacts[teamId] = (IEnumerable<InviteUserModel>) recentInviteModels;
        recentInviteModels = (List<InviteUserModel>) null;
        teamContacts = (List<TeamContactsModel>) null;
      }
      else
      {
        InviteHelper._teamContacts.Add(teamId, (IEnumerable<InviteUserModel>) recentInviteModels);
        recentInviteModels = (List<InviteUserModel>) null;
        teamContacts = (List<TeamContactsModel>) null;
      }
    }

    private static async Task PullProjectUsers()
    {
      if (!Utils.IsNetworkAvailable())
        return;
      List<ProjectUsersMode> projectUsersList = await Utils.GetRecentProjectUsersList();
      foreach (ProjectUsersMode projectUsersMode in projectUsersList)
      {
        projectUsersMode.shareUsers.RemoveAll((Predicate<ShareUserModel>) (user => user.deleted));
        projectUsersMode.shareUsers.Sort((Comparison<ShareUserModel>) ((a, b) =>
        {
          if (a.isOwner || a.isAccept && !b.isAccept)
            return -1;
          return b.isOwner || !a.isAccept && b.isAccept ? 1 : (b.createdTime ?? DateTime.Now).CompareTo(a.createdTime ?? DateTime.Now);
        }));
      }
      projectUsersList?.Sort((Comparison<ProjectUsersMode>) ((a, b) => a.SortOrder.CompareTo(b.SortOrder)));
      InviteHelper._projectUsersData = projectUsersList;
    }

    public static async Task SetTeamMembers(
      string teamId,
      string projectId = "",
      ShareProjectDialog window = null)
    {
      if (InviteHelper._teamMembers.ContainsKey(teamId))
        InviteHelper.PullTeamMembers(teamId, projectId, window);
      else
        await InviteHelper.PullTeamMembers(teamId, projectId, window);
    }

    public static async Task SetTeamContacts(string teamId)
    {
      if (InviteHelper._teamContacts.ContainsKey(teamId))
        InviteHelper.PullTeamContacts(teamId);
      else
        await InviteHelper.PullTeamContacts(teamId);
    }

    public static async Task SetProjectUsers()
    {
      if (InviteHelper._projectUsersData != null && InviteHelper._projectUsersData.Count > 0)
        InviteHelper.PullProjectUsers();
      else
        await InviteHelper.PullProjectUsers();
    }

    public static async Task SetShareContacts()
    {
      if (InviteHelper._shareContacts != null && InviteHelper._shareContacts.Count > 0)
        InviteHelper.PullShareContacts();
      else
        await InviteHelper.PullShareContacts();
    }

    public static IEnumerable<TeamMember> GetTeamMembers(string teamId)
    {
      return InviteHelper._teamMembers.ContainsKey(teamId) ? InviteHelper._teamMembers[teamId] : (IEnumerable<TeamMember>) null;
    }

    public static IEnumerable<InviteUserModel> GetTeamContacts(string teamId)
    {
      if (!InviteHelper._teamContacts.ContainsKey(teamId))
        return (IEnumerable<InviteUserModel>) null;
      foreach (InviteUserModel inviteUserModel in InviteHelper._teamContacts[teamId])
        inviteUserModel.Selected = false;
      return InviteHelper._teamContacts[teamId];
    }

    public static List<ProjectUsersMode> GetProjectUsers() => InviteHelper._projectUsersData;

    public static List<ShareContactsModel> GetShareContacts() => InviteHelper._shareContacts;

    public static List<InviteUserModel> GetRecentInvite()
    {
      InviteHelper._recentInviteUsers.ForEach((Action<InviteUserModel>) (u => u.Selected = false));
      return InviteHelper._recentInviteUsers;
    }

    public static void Clear()
    {
      InviteHelper._projectUsersData?.Clear();
      InviteHelper._shareContacts?.Clear();
      InviteHelper._recentInviteUsers?.Clear();
    }

    public static void DeleteRecentUser(string email)
    {
      InviteHelper._shareContacts?.RemoveAll((Predicate<ShareContactsModel>) (u => u.email == email));
      InviteHelper._recentInviteUsers?.RemoveAll((Predicate<InviteUserModel>) (u => u.Email == email));
    }
  }
}
