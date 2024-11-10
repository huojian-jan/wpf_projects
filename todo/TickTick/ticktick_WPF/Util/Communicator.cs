// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.Communicator
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.Service;
using ticktick_WPF.SyncServices.Models;
using ticktick_WPF.Util.Network;
using ticktick_WPF.Util.Sync;
using ticktick_WPF.Util.Sync.Model;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.Search;
using ticktick_WPF.Views.Tag;
using TickTickModels;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class Communicator
  {
    public static async Task<UserModel> SignOn(string username, string password)
    {
      bool flag = Utils.IsMobilePhone(username);
      return await ApiClient.PostJsonAsync<UserModel>(BaseUrl.GetUrl("/api/v2/user/signon"), (object) new UserLoginModel()
      {
        username = (!flag ? username : string.Empty),
        phone = (flag ? username : string.Empty),
        password = password
      });
    }

    public static async Task<bool> IsNewUser()
    {
      try
      {
        return await ApiClient.GetJsonAsync<bool>(BaseUrl.GetUrl("/api/v2/user/isJustRegistered"));
      }
      catch (Exception ex)
      {
      }
      return false;
    }

    public static async Task SignOut()
    {
      try
      {
        string jsonAsync = await ApiClient.GetJsonAsync<string>(BaseUrl.GetUrl("/api/v2/user/signout"));
      }
      catch (Exception ex)
      {
      }
    }

    public static async Task<UserModel> GetUserStatus()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/status", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        UserModel userStatus = JsonConvert.DeserializeObject<UserModel>(httpWebRequest);
        if (userStatus != null && userStatus.userId != null && userStatus.userId != "")
        {
          userStatus.token = LocalSettings.Settings.LoginUserAuth;
          return userStatus;
        }
        return new UserModel() { username = httpWebRequest };
      }
      catch (Exception ex)
      {
        return new UserModel() { username = httpWebRequest };
      }
    }

    public static async Task<string> GetInviteCode()
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/signup/inviteCode", mode: "GET");
    }

    public static async Task<UserModel> Signup(string name, string username, string password)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/signup", JsonConvert.SerializeObject((object) new UserSignModel()
      {
        name = name,
        username = username,
        password = password
      }), isNeedErrorReturn: true);
      try
      {
        UserModel userModel = JsonConvert.DeserializeObject<UserModel>(httpWebRequest);
        if (userModel != null && userModel.userId != null && userModel.userId != "")
          return userModel;
        return new UserModel() { username = httpWebRequest };
      }
      catch (Exception ex)
      {
        return new UserModel() { username = httpWebRequest };
      }
    }

    public static async Task<UserInfoModel> GetUserInfo()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/profile", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<UserInfoModel>(httpWebRequest);
      }
      catch (Exception ex)
      {
        string msg = ExceptionUtils.BuildExceptionMessage(ex);
        if (App.IsAdmin)
          msg = msg + "\r\n" + httpWebRequest;
        UtilLog.Warn(msg);
        return new UserInfoModel();
      }
    }

    public static async Task<(string, List<TaskModel>)> BatchCheckTrash(
      long? next,
      int limit,
      int type = 1)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/project/all/trash/page?next={0}&limit={1}&type={2}", (object) next, (object) limit, (object) type), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (string.IsNullOrEmpty(httpWebRequest))
        return ((string) null, new List<TaskModel>());
      string empty1 = string.Empty;
      string empty2 = string.Empty;
      string str1;
      string str2;
      try
      {
        str1 = JObject.Parse(httpWebRequest)["tasks"].ToString();
        str2 = JObject.Parse(httpWebRequest)[nameof (next)].ToString();
      }
      catch (Exception ex)
      {
        UtilLog.Info(ex.Message);
        return ((string) null, (List<TaskModel>) null);
      }
      try
      {
        List<TaskModel> taskModelList = JsonConvert.DeserializeObject<List<TaskModel>>(str1);
        return (str2, taskModelList);
      }
      catch (Exception ex)
      {
        UtilLog.Info(ex.Message);
        return ((string) null, (List<TaskModel>) null);
      }
    }

    public static async Task<SyncBean> BatchCheck(long point)
    {
      BatchCheckCollectModel collectModel = new BatchCheckCollectModel()
      {
        point = point,
        time = DateTime.Now
      };
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/check/" + point.ToString(), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET", isNeedErrorReturn: true);
      if (string.IsNullOrEmpty(httpWebRequest))
        return (SyncBean) null;
      collectModel.endTime = DateTime.Now;
      try
      {
        ErrorModel errorModel = JsonConvert.DeserializeObject<ErrorModel>(httpWebRequest);
        if (errorModel != null)
        {
          if (errorModel.errorId != null)
          {
            collectModel.errorId = errorModel.errorId;
            return (SyncBean) null;
          }
        }
      }
      catch (Exception ex)
      {
      }
      JsonSerializerSettings settings = new JsonSerializerSettings();
      List<string> stringList = new List<string>();
      JObject jobject1 = (JObject) null;
      try
      {
        jobject1 = JObject.Parse(httpWebRequest);
      }
      catch (Exception ex)
      {
      }
      string str = (string) null;
      try
      {
        JToken jtoken1 = (JToken) null;
        JToken jtoken2 = (JToken) null;
        if (jobject1 != null)
        {
          JToken jtoken3;
          if (jobject1.TryGetValue("syncTaskBean", out jtoken3))
          {
            JObject jobject2 = JObject.Parse(jtoken3.ToString());
            JToken jtoken4;
            if (jobject2.TryGetValue("deletedForever", out jtoken4))
              jtoken1 = jtoken4;
            JToken jtoken5;
            if (jobject2.TryGetValue("deletedInTrash", out jtoken5))
              jtoken2 = jtoken5;
          }
          JToken jtoken6;
          if (jobject1.TryGetValue("checks", out jtoken6))
            str = jtoken6.ToString();
        }
        if (jtoken1 == null)
          stringList.Add("deletedForever");
        if (jtoken2 == null)
          stringList.Add("deletedInTrash");
      }
      catch (JsonReaderException ex)
      {
      }
      settings.ContractResolver = (IContractResolver) new SyncTaskBeanLimitPropsContractResolver(stringList.ToArray(), false);
      try
      {
        SyncBean syncBean = TaskService.HandleTaskBeforeSave(JsonConvert.DeserializeObject<SyncBean>(httpWebRequest, settings));
        if (syncBean != null)
        {
          syncBean.checks = str;
          collectModel.checkPoint = syncBean.checkPoint;
          collectModel.sync = true;
        }
        return syncBean;
      }
      catch (Exception ex)
      {
        string msg = ExceptionUtils.BuildExceptionMessage(ex);
        if (App.IsAdmin)
          msg = msg + "\r\n" + httpWebRequest;
        UtilLog.Warn(msg);
      }
      return (SyncBean) null;
    }

    public static async Task<bool> DeleteProjectGroup(string groupId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/projectGroup/" + groupId, auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
      return true;
    }

    public static async Task<string> UpdatePutProject(ProjectModel project)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + project.id, JsonConvert.SerializeObject((object) project), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT");
    }

    public static async Task<string> UpdateProject(string projectId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId, auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<ObservableCollection<TaskModel>> PullServerTasksByProjectId(string id)
    {
      string httpWebRequest;
      if (!string.IsNullOrEmpty(id))
        httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + id + "/tasks", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      else
        httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/tasks", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        ObservableCollection<TaskModel> observableCollection = JsonConvert.DeserializeObject<ObservableCollection<TaskModel>>(httpWebRequest);
        UtilLog.Info(string.Format("PullServerTasksByProjectId count:  {0}", (object) observableCollection?.Count));
        return observableCollection;
      }
      catch (Exception ex)
      {
        string msg = ExceptionUtils.BuildExceptionMessage(ex);
        if (App.IsAdmin)
          msg = msg + "\r\n" + httpWebRequest;
        UtilLog.Warn(msg);
      }
      return (ObservableCollection<TaskModel>) null;
    }

    public static async Task<BatchUpdateResult> BatchUpdateProjectGroup(
      SyncProjectGroupBean syncBean)
    {
      return JsonConvert.DeserializeObject<BatchUpdateResult>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/projectGroup", JsonConvert.SerializeObject((object) syncBean), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<BatchTaskOrderUpdateResult> BatchUpdateTaskOrder(
      SyncTaskOrderBean taskOrderBean)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/taskOrder", JsonConvert.SerializeObject((object) taskOrderBean), auth: LocalSettings.Settings.LoginUserAuth);
      try
      {
        return JsonConvert.DeserializeObject<BatchTaskOrderUpdateResult>(httpWebRequest);
      }
      catch (Exception ex)
      {
        string msg = ExceptionUtils.BuildExceptionMessage(ex);
        if (App.IsAdmin)
          msg = msg + "\r\n" + httpWebRequest;
        UtilLog.Warn(msg);
        return (BatchTaskOrderUpdateResult) null;
      }
    }

    public static async Task<BatchSyncOrderUpdateResult> BatchUpdateSyncOrder(
      SyncOrderBean syncOrderBean)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/order", JsonConvert.SerializeObject((object) syncOrderBean), auth: LocalSettings.Settings.LoginUserAuth);
      try
      {
        return JsonConvert.DeserializeObject<BatchSyncOrderUpdateResult>(httpWebRequest);
      }
      catch (Exception ex)
      {
        string msg = ExceptionUtils.BuildExceptionMessage(ex);
        if (App.IsAdmin)
          msg = msg + "\r\n" + httpWebRequest;
        UtilLog.Warn(msg);
        return (BatchSyncOrderUpdateResult) null;
      }
    }

    public static async Task<BatchSyncOrderUpdateResult> BatchUpdateTaskSyncOrder(
      SyncOrderBean syncOrderBean)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v3/batch/order", JsonConvert.SerializeObject((object) syncOrderBean), auth: LocalSettings.Settings.LoginUserAuth);
      try
      {
        return JsonConvert.DeserializeObject<BatchSyncOrderUpdateResult>(httpWebRequest);
      }
      catch (Exception ex)
      {
        string msg = ExceptionUtils.BuildExceptionMessage(ex);
        if (App.IsAdmin)
          msg = msg + "\r\n" + httpWebRequest;
        UtilLog.Warn(msg);
        return (BatchSyncOrderUpdateResult) null;
      }
    }

    public static async Task<ShareAddUserMode> AddShareUser(
      string projectId,
      string toUsername,
      Constants.ProjectPermission permission)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + "/share", "{\"toUsername\": \"" + toUsername + "\"" + (permission == Constants.ProjectPermission.person ? "}" : ",\"permission\":\"" + permission.ToString() + "\"}"), auth: LocalSettings.Settings.LoginUserAuth, isNeedErrorReturn: true);
      try
      {
        ShareAddUserMode shareAddUserMode = JsonConvert.DeserializeObject<ShareAddUserMode>(httpWebRequest);
        if (shareAddUserMode != null && shareAddUserMode.toUsername != null && shareAddUserMode.toUsername != "")
          return shareAddUserMode;
        return new ShareAddUserMode()
        {
          fromUsername = httpWebRequest
        };
      }
      catch (Exception ex)
      {
        return new ShareAddUserMode()
        {
          fromUsername = httpWebRequest
        };
      }
    }

    public static async Task<ShareAddUsersMode> AddShareUsers(
      string projectId,
      List<string> toUsernames,
      Constants.ProjectPermission permission)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + "/shares", Communicator.UserNames2Json(toUsernames, permission), auth: LocalSettings.Settings.LoginUserAuth, isNeedErrorReturn: true);
      try
      {
        return JsonConvert.DeserializeObject<ShareAddUsersMode>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return (ShareAddUsersMode) null;
      }
    }

    private static string UserNames2Json(
      List<string> userList,
      Constants.ProjectPermission permission)
    {
      string str = "" + "[";
      bool flag = true;
      foreach (string user in userList)
      {
        if (!flag)
        {
          str = str + ",{\"toUsername\": \"" + user + "\"" + (permission == Constants.ProjectPermission.person ? "}" : ",\"permission\":\"" + permission.ToString() + "\"}");
        }
        else
        {
          str = str + "{\"toUsername\": \"" + user + "\"" + (permission == Constants.ProjectPermission.person ? "}" : ",\"permission\":\"" + permission.ToString() + "\"}");
          flag = false;
        }
      }
      return str + "]";
    }

    public static async Task<string> DeleteShareUser(string projectId, string recordId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + "/share/" + recordId, auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task<string> GetShareLink(
      string projectId,
      bool isInviteUrl = false,
      Constants.ProjectPermission permission = Constants.ProjectPermission.person)
    {
      string httpWebRequest;
      if (isInviteUrl)
      {
        httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + "/collaboration/invite-url", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET", isNeedErrorReturn: true);
      }
      else
      {
        string str = permission == Constants.ProjectPermission.person ? (string) null : permission.ToString();
        httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + "/collaboration/invite" + (string.IsNullOrEmpty(str) ? "" : "?permission=" + str), auth: LocalSettings.Settings.LoginUserAuth, isNeedErrorReturn: true);
      }
      return httpWebRequest;
    }

    public static async Task<string> DeleteShareLink(string projectId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + "/collaboration/invite", auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task<string> GetShareUserList(string projectId, bool openToTeam)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + (openToTeam ? "/users" : "/shares"), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
    }

    public static async Task<string> GetRecentProjectUsersList()
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/share/recentProjectUsers", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
    }

    public static async Task<List<ShareContactsModel>> GetShareContactsList()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/share/shareContacts", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<List<ShareContactsModel>>(JObject.Parse(httpWebRequest)["contacts"].ToString());
      }
      catch (Exception ex)
      {
        return (List<ShareContactsModel>) null;
      }
    }

    public static async Task<bool> DeleteShareContact(string email)
    {
      return await ApiClient.DeleteAsync(BaseUrl.GetApiDomain() + "/api/v2/share/shareContacts?toEmail=" + email);
    }

    public static async Task<string> GetAllShareContacts()
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/share/contacts", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
    }

    public static async Task<List<TeamContactsModel>> GetTeamContactsList(string teamId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/team/{0}/share/shareContacts", (object) teamId), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<List<TeamContactsModel>>(JObject.Parse(httpWebRequest)["contacts"].ToString());
      }
      catch (Exception ex)
      {
        return (List<TeamContactsModel>) null;
      }
    }

    public static async Task<UserPublicProfilesModel> GetUserPublicProfiles(string userCode)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/pub/api/v2/userPublicProfiles", "[\"" + userCode + "\"]", auth: LocalSettings.Settings.LoginUserAuth);
      if (string.IsNullOrEmpty(httpWebRequest))
        return (UserPublicProfilesModel) null;
      try
      {
        return JsonConvert.DeserializeObject<UserPublicProfilesModel>(httpWebRequest.Substring(1, httpWebRequest.Length - 2));
      }
      catch (Exception ex)
      {
        return (UserPublicProfilesModel) null;
      }
    }

    public static async Task<List<UserPublicProfilesModel>> GetUsersPublicProfiles(
      List<string> userCodes)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("[");
      for (int index = 0; index < userCodes.Count; ++index)
      {
        stringBuilder.Append("\"" + userCodes[index] + "\"");
        if (index < userCodes.Count - 1)
          stringBuilder.Append(",");
      }
      stringBuilder.Append("]");
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/pub/api/v2/userPublicProfiles", stringBuilder.ToString(), auth: LocalSettings.Settings.LoginUserAuth);
      if (string.IsNullOrEmpty(httpWebRequest))
        return (List<UserPublicProfilesModel>) null;
      try
      {
        return JsonConvert.DeserializeObject<List<UserPublicProfilesModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return (List<UserPublicProfilesModel>) null;
      }
    }

    public static async Task<string> GetNotificationList()
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/notification?autoMarkRead=false", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
    }

    public static async Task<string> SetAcceptShare(string projectId, string id, int actionStatus)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/project/{0}/share/accept/{1}?actionStatus={2}", (object) projectId, (object) id, (object) actionStatus), auth: LocalSettings.Settings.LoginUserAuth, isNeedErrorReturn: true);
    }

    public static async Task<NotificationUnreadCount> GetNotificationCount()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/notification/unread", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<NotificationUnreadCount>(httpWebRequest) ?? new NotificationUnreadCount();
      }
      catch (Exception ex)
      {
        return new NotificationUnreadCount();
      }
    }

    public static async Task<string> GetTencentOpenId(string accessToken)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("https://graph.qq.com/oauth2.0/me?access_token={0}", (object) accessToken), mode: "GET", fulluri: true);
      try
      {
        return JObject.Parse(httpWebRequest.Substring(httpWebRequest.IndexOf("{", StringComparison.Ordinal), httpWebRequest.IndexOf("}", StringComparison.Ordinal) - httpWebRequest.IndexOf("{", StringComparison.Ordinal) + 1))["openid"].ToString();
      }
      catch (Exception ex)
      {
        return "";
      }
    }

    public static async Task<string> ThirdChangePassword(
      string code,
      string password1,
      string password2,
      string userAuth)
    {
      JObject jobject = new JObject();
      jobject.Add(nameof (code), (JToken) code);
      jobject.Add("newPassword1", (JToken) password1);
      jobject.Add("newPassword2", (JToken) password2);
      try
      {
        string auth = userAuth;
        return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/third/changePassword", jobject.ToString(), auth: auth, isNeedErrorReturn: true);
      }
      catch (Exception ex)
      {
        return (string) null;
      }
    }

    public static async Task<UserModel> ThirdSignon(
      string mode = "",
      string code = "",
      string site = "",
      string accessToken = "",
      string uId = "",
      string accessTokenSecret = "")
    {
      string str = "";
      switch (mode)
      {
        case "login_Google":
          str = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/user/sign/OAuth2?site={0}&accessToken={1}&uId={2}", (object) site, (object) accessToken, (object) uId), mode: "GET", isNeedErrorReturn: true);
          break;
        case "login_Facebook":
          str = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/user/sign/facebook/validate?access_token={0}", (object) accessToken), mode: "GET", isNeedErrorReturn: true);
          break;
        case "login_Twitter":
          str = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/user/sign/twitter?accessToken={0}&accessTokenSecret={1}", (object) accessToken, (object) accessTokenSecret), mode: "GET", isNeedErrorReturn: true);
          break;
        case "login_Weibo":
          str = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/user/sign/weibo/validate?code={0}", (object) code), mode: "GET", isNeedErrorReturn: true);
          break;
        case "login_QQ":
          str = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/user/sign/OAuth2?site={0}&accessToken={1}&uId={2}", (object) site, (object) accessToken, (object) uId), mode: "GET", isNeedErrorReturn: true);
          break;
        case "login_Wechat":
          str = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/user/sign/wechat/validate?code={0}&state=", (object) code), mode: "GET", isNeedErrorReturn: true);
          break;
      }
      try
      {
        UserModel userModel = JsonConvert.DeserializeObject<UserModel>(str);
        if (userModel != null && userModel.userId != null && userModel.userId != "")
          return userModel;
        return new UserModel() { username = str };
      }
      catch (Exception ex)
      {
        return new UserModel() { username = str };
      }
    }

    public static async Task<ObservableCollection<LimitsModel>> GetLimits()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/configs/limits", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      ObservableCollection<LimitsModel> limits = new ObservableCollection<LimitsModel>();
      try
      {
        JObject jobject = JObject.Parse(httpWebRequest);
        LimitsModel limitsModel1 = JsonConvert.DeserializeObject<LimitsModel>(jobject["free"].ToString());
        limitsModel1.type = "FREE";
        LimitsModel limitsModel2 = JsonConvert.DeserializeObject<LimitsModel>(jobject["pro"].ToString());
        limitsModel2.type = "PRO";
        LimitsModel limitsModel3 = JsonConvert.DeserializeObject<LimitsModel>(jobject["team"].ToString());
        limitsModel3.type = "TEAM";
        limits.Add(limitsModel1);
        limits.Add(limitsModel2);
        limits.Add(limitsModel3);
      }
      catch (Exception ex)
      {
      }
      return limits;
    }

    public static async Task<BatchUpdateResult> BatchUpdateProject(SyncProjectBean syncBean)
    {
      return Communicator.SafeConvertResult<BatchUpdateResult>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/project", JsonConvert.SerializeObject((object) syncBean), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<BatchUpdateResult> BatchUpdateFilter(SyncFilterBean syncBean)
    {
      return Communicator.SafeConvertResult<BatchUpdateResult>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/filter", JsonConvert.SerializeObject((object) syncBean), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<BatchUpdateResult> BatchUpdateTag(SyncTagBean syncBean)
    {
      return Communicator.SafeConvertResult<BatchUpdateResult>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/tag", JsonConvert.SerializeObject((object) syncBean), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<BatchUpdateResult> BatchUpdateTaskProject(
      List<MoveOrRestoreProject> data)
    {
      return Communicator.SafeConvertResult<BatchUpdateResult>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/taskProject", JsonConvert.SerializeObject((object) data), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<BatchUpdateResult> BatchRestoreTask(List<MoveOrRestoreProject> data)
    {
      return Communicator.SafeConvertResult<BatchUpdateResult>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/trash/restore", JsonConvert.SerializeObject((object) data), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<BatchUpdateResult> BatchUpdateTask(SyncTaskBean syncBean)
    {
      return Communicator.SafeConvertResult<BatchUpdateResult>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/task", JsonConvert.SerializeObject((object) syncBean), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<BatchUpdateResult> BatchDeleteTask(List<TaskProject> deleteFromTrash)
    {
      string result;
      if (App.Instance.TimeZoneChangeHandling)
        result = (string) null;
      else
        result = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/task?deleteforever=true", JsonConvert.SerializeObject((object) deleteFromTrash), auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
      return Communicator.SafeConvertResult<BatchUpdateResult>(result);
    }

    public static async Task<bool> DeletePomo(string pomoId, bool isPomo)
    {
      return !string.IsNullOrEmpty(pomoId) && string.IsNullOrEmpty(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/pomodoro" + (isPomo ? "" : "/timing") + "/" + pomoId, string.Empty, auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE", isNeedErrorReturn: true));
    }

    public static async Task<BatchUpdateResult> BatchUpdatePomos(
      BaseSyncBean<PomodoroModel> pomos,
      bool isPomo)
    {
      if (pomos == null || !pomos.Any())
        return (BatchUpdateResult) null;
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/pomodoro" + (isPomo ? "" : "/timing"), JsonConvert.SerializeObject((object) pomos), auth: LocalSettings.Settings.LoginUserAuth);
      try
      {
        string str1 = "add:";
        foreach (PomodoroModel pomodoroModel in pomos.Add)
          str1 = str1 + pomodoroModel.Id + string.Format("_{0}_{1},", (object) pomodoroModel.StartTime, (object) pomodoroModel.EndTime);
        string str2 = str1 + " update:";
        foreach (PomodoroModel pomodoroModel in pomos.Update)
          str2 = str2 + pomodoroModel.Id + string.Format("_{0}_{1},", (object) pomodoroModel.StartTime, (object) pomodoroModel.EndTime);
        UtilLog.Info("BatchUpdatePomos: " + (str2 + " isPomo:" + isPomo.ToString()) + "\r\n--" + httpWebRequest);
      }
      catch (Exception ex)
      {
      }
      return Communicator.SafeConvertResult<BatchUpdateResult>(httpWebRequest);
    }

    private static List<EventUpdateResult> SafeConvertCalendarResult(string result)
    {
      if (string.IsNullOrEmpty(result))
        return new List<EventUpdateResult>();
      try
      {
        return JsonConvert.DeserializeObject<List<EventUpdateResult>>(result);
      }
      catch (Exception ex)
      {
        return new List<EventUpdateResult>();
      }
    }

    private static T SafeConvertResult<T>(string result)
    {
      if (string.IsNullOrEmpty(result))
        return default (T);
      try
      {
        return JsonConvert.DeserializeObject<T>(result);
      }
      catch (Exception ex)
      {
        string msg = ExceptionUtils.BuildExceptionMessage(ex);
        if (App.IsAdmin)
          msg = msg + "\r\n" + result;
        UtilLog.Warn(msg);
        return default (T);
      }
    }

    public static async Task<string> GetSignOnToken()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/requestSignOnToken", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      return string.IsNullOrEmpty(httpWebRequest) ? LocalSettings.Settings.LoginUserAuth : (httpWebRequest.IndexOf("token", StringComparison.Ordinal) != -1 ? JObject.Parse(httpWebRequest)["token"].ToString() : "");
    }

    public static async Task<List<TaskModel>> GetClosedTasks(
      string prjId,
      DateTime? from,
      DateTime? to,
      int limit)
    {
      if (to.HasValue)
      {
        DateTime universalTime;
        string str1;
        if (from.HasValue)
        {
          universalTime = from.Value;
          universalTime = universalTime.ToUniversalTime();
          str1 = universalTime.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
          str1 = "";
        string str2 = str1;
        universalTime = to.Value;
        universalTime = universalTime.ToUniversalTime();
        string str3 = universalTime.ToString("yyyy-MM-dd HH:mm:ss");
        string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/project/{0}/closed/?from={1}&to={2}&limit={3}", (object) prjId, (object) str2, (object) str3, (object) limit), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
        if (!string.IsNullOrEmpty(httpWebRequest))
        {
          try
          {
            List<TaskModel> closedTasks = JsonConvert.DeserializeObject<List<TaskModel>>(httpWebRequest);
            if (closedTasks != null && closedTasks.Count > 0)
            {
              foreach (TaskModel task in closedTasks)
              {
                TaskService.ClearDueDate(task);
                if (string.IsNullOrEmpty(task.userId))
                  task.userId = LocalSettings.Settings.LoginUserId;
                if (task.tags != null)
                  task.tag = TagSerializer.ToJsonContent(((IEnumerable<string>) task.tags).ToList<string>());
                if (task.exDate != null)
                  task.exDates = ExDateSerilizer.ToString(task.exDate);
              }
            }
            return closedTasks;
          }
          catch (Exception ex)
          {
            return (List<TaskModel>) null;
          }
        }
      }
      return (List<TaskModel>) null;
    }

    public static async Task<string> ReminderToPay(string projectId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/project/{0}/reminderToPay", (object) projectId), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<string> HandleShareApplication(string notificationId, string type)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/project/collaboration/{0}?notificationId={1}", (object) type, (object) notificationId), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT");
    }

    public static async Task<string> CheckUpdate()
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpRealUrlRequest(BaseUrl.DomainUrl + "/static/getApp/download?type=" + (Environment.Is64BitOperatingSystem ? "win64" : "win"), LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<List<CommentModel>> GetComments(string projectSid, string taskSid)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/project/{0}/task/{1}/comments", (object) projectSid, (object) taskSid), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<List<CommentModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return (List<CommentModel>) null;
      }
    }

    public static async Task<string> DeleteComment(
      string projectSid,
      string taskSid,
      string commentSid)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/project/{0}/task/{1}/comment/{2}", (object) projectSid, (object) taskSid, (object) commentSid), auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task<string> AddComment(RemoteCommentModel comment, bool isUpdate = false)
    {
      string api = string.Format("/api/v2/project/{0}/task/{1}/comment", (object) (TaskCache.GetTaskById(comment.taskId)?.ProjectId ?? comment.projectId), (object) comment.taskId);
      if (isUpdate)
        api = api + "/" + comment.id;
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(api, JsonConvert.SerializeObject((object) comment, Formatting.None, new JsonSerializerSettings()
      {
        NullValueHandling = NullValueHandling.Ignore
      }), auth: LocalSettings.Settings.LoginUserAuth, mode: isUpdate ? "PUT" : "POST");
    }

    public static async Task<List<HolidayModel>> GetRecentHoliday()
    {
      string currentRegion = HolidayRegion.cn;
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("https://pull.dida365.com/common/calendar/holiday_cn.json", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET", fulluri: true);
      try
      {
        List<HolidayModel> recentHoliday = JsonConvert.DeserializeObject<List<HolidayModel>>(httpWebRequest);
        recentHoliday.ForEach((Action<HolidayModel>) (item => item.region = currentRegion));
        return recentHoliday;
      }
      catch (Exception ex)
      {
        return new List<HolidayModel>();
      }
    }

    public static async Task<List<PomodoroModel>> PullRemotePomos(DateTime from, DateTime to)
    {
      try
      {
        List<PomodoroModel> pomodoroModelList = JsonConvert.DeserializeObject<List<PomodoroModel>>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/pomodoros?from={0}&to={1}", (object) Utils.GetTimeStamp(new DateTime?(from)), (object) Utils.GetTimeStamp(new DateTime?(to))), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET"));
        return pomodoroModelList == null || pomodoroModelList.Count <= 0 ? (List<PomodoroModel>) null : pomodoroModelList;
      }
      catch (Exception ex)
      {
        return (List<PomodoroModel>) null;
      }
    }

    public static async Task<List<PomodoroModel>> PullRemoteTimings(DateTime from, DateTime to)
    {
      try
      {
        List<PomodoroModel> pomodoroModelList = JsonConvert.DeserializeObject<List<PomodoroModel>>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/pomodoros/timing?from={0}&to={1}", (object) Utils.GetTimeStamp(new DateTime?(from)), (object) Utils.GetTimeStamp(new DateTime?(to))), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET"));
        return pomodoroModelList == null || pomodoroModelList.Count <= 0 ? (List<PomodoroModel>) null : pomodoroModelList;
      }
      catch (Exception ex)
      {
        return (List<PomodoroModel>) null;
      }
    }

    public static async Task<PomoConfigModel> GetRemotePomoConfig()
    {
      try
      {
        return JsonConvert.DeserializeObject<PomoConfigModel>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/preferences/pomodoro", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET"));
      }
      catch (Exception ex)
      {
        return (PomoConfigModel) null;
      }
    }

    public static async Task UpdateRemotePomoConfig(PomoConfigModel config)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/preferences/pomodoro", JsonConvert.SerializeObject((object) config), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT");
    }

    public static async Task<List<TaskModel>> SearchRemoteTasks(string key)
    {
      try
      {
        List<TaskModel> taskModelList = JsonConvert.DeserializeObject<List<TaskModel>>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/search/task?keywords={0}", (object) key), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET"));
        return taskModelList == null || taskModelList.Count <= 0 ? new List<TaskModel>() : taskModelList;
      }
      catch (Exception ex)
      {
        return new List<TaskModel>();
      }
    }

    public static async Task<string> GetTaskActivities(string projectId, string taskId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v1/project/{0}/task/{1}/activity", (object) projectId, (object) taskId), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
    }

    public static async Task<string> GetProjectActivities(string projectId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v1/project/{0}/activity", (object) projectId), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
    }

    public static async Task<string> RenameTag(string original, string revised)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/tag/rename", JsonConvert.SerializeObject((object) new TagModifyModel()
      {
        name = original,
        newName = revised
      }), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT");
    }

    public static async Task DeleteTag(string tag)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/tag?name=" + Utils.UrlEncode(tag), auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task<CalendarSubscribeProfileModel> SubscribeCalendar(
      CalendarSubscribeProfileModel profile)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/subscribe", JsonConvert.SerializeObject((object) profile), auth: LocalSettings.Settings.LoginUserAuth);
      return !string.IsNullOrEmpty(httpWebRequest) ? JsonConvert.DeserializeObject<CalendarSubscribeProfileModel>(httpWebRequest) : (CalendarSubscribeProfileModel) null;
    }

    public static async Task UnsubscribeCalendar(string id)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/unsubscribe/" + id, auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task<List<CalendarSubscribeProfileModel>> GetCalendarSubscriptions()
    {
      try
      {
        return JsonConvert.DeserializeObject<List<CalendarSubscribeProfileModel>>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/subscription", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET"));
      }
      catch (Exception ex)
      {
        return (List<CalendarSubscribeProfileModel>) null;
      }
    }

    public static async Task<List<BindCalendarAccountModel>> GetBindCalendarAccounts(
      DateTime? start,
      DateTime? end)
    {
      try
      {
        if (!start.HasValue || !end.HasValue)
          return JsonConvert.DeserializeObject<List<BindCalendarAccountModel>>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/bind/accounts", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET"));
        return JsonConvert.DeserializeObject<List<BindCalendarAccountModel>>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/bind/accounts", "{\"begin\" : \"" + start.Value.ToString() + "\",\"end\": \"" + end.Value.ToString() + "\"}", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET"));
      }
      catch (Exception ex)
      {
        return (List<BindCalendarAccountModel>) null;
      }
    }

    public static async Task<BindCalendarsCollection> GetBindCalendarEvents()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/bind/events/all", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<BindCalendarsCollection>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return new BindCalendarsCollection();
      }
    }

    public static async Task<BindCalendarsCollection> GetBindCalendarEvents(
      DateTime startTime,
      DateTime endTime)
    {
      try
      {
        return JsonConvert.DeserializeObject<BindCalendarsCollection>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/bind/events/all", "{\"begin\": \"" + startTime.ToString(UtcDateTimeConverter.GetConverterValue(startTime)) + "\",\"end\": \"" + endTime.ToString(UtcDateTimeConverter.GetConverterValue(endTime)) + "\"}", auth: LocalSettings.Settings.LoginUserAuth));
      }
      catch (Exception ex)
      {
        return new BindCalendarsCollection();
      }
    }

    public static async Task<BindCalendarAccountModel> BindCalendarAccount(
      string code,
      string site,
      string url)
    {
      return JsonConvert.DeserializeObject<BindCalendarAccountModel>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/bind?code=" + code + "&state=" + site + "&channel=win&redirectUrl=" + url, auth: LocalSettings.Settings.LoginUserAuth, mode: "GET"));
    }

    public static async Task<BindCalendarAccountModel> BindCalDavAccount(BindAccountModel model)
    {
      string loginUserAuth = LocalSettings.Settings.LoginUserAuth;
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v4/calendar/bind", JsonConvert.SerializeObject((object) model), auth: loginUserAuth);
      if (httpWebRequest.Contains("caldav_bind_faild"))
        throw new CustomException.CalendarBindException("calendar_bind_faild");
      if (httpWebRequest.Contains("caldav_bind_duplicate"))
        throw new CustomException.CalendarBindException("calendar_bind_duplicate");
      return !httpWebRequest.Contains("caldav_bind_exceed") ? JsonConvert.DeserializeObject<BindCalendarAccountModel>(httpWebRequest) : throw new CustomException.CalendarBindException("calendar_bind_exceed");
    }

    public static async Task<string> UpdateCalDavAccount(string accoundId, BindAccountModel model)
    {
      string api = "/api/v4/calendar/update/" + accoundId;
      string loginUserAuth = LocalSettings.Settings.LoginUserAuth;
      string content = JsonConvert.SerializeObject((object) model);
      string auth = loginUserAuth;
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(api, content, auth: auth);
    }

    public static async Task UnbindCalendar(string calendarId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/bind/" + calendarId, auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task<List<BindCalendarModel>> GetBindCalDavEvents(
      string calendarId,
      DateTime? pullStart,
      DateTime? pullEnd)
    {
      ref DateTime? local1 = ref pullStart;
      DateTime valueOrDefault;
      string str1;
      if (!local1.HasValue)
      {
        str1 = (string) null;
      }
      else
      {
        valueOrDefault = local1.GetValueOrDefault();
        str1 = valueOrDefault.ToString(UtcDateTimeConverter.GetConverterValue(pullStart.Value));
      }
      string str2 = str1;
      ref DateTime? local2 = ref pullEnd;
      string str3;
      if (!local2.HasValue)
      {
        str3 = (string) null;
      }
      else
      {
        valueOrDefault = local2.GetValueOrDefault();
        str3 = valueOrDefault.ToString(UtcDateTimeConverter.GetConverterValue(pullEnd.Value));
      }
      string str4 = str3;
      string content = "{ \"begin\": \"" + str2 + "\",\"end\": \"" + str4 + "\" }";
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v4/calendar/bind/events/{0}", (object) calendarId), content, auth: LocalSettings.Settings.LoginUserAuth);
      if (!pullStart.HasValue && !pullEnd.HasValue)
        CalendarService.TryLogResult(calendarId, httpWebRequest);
      if (httpWebRequest.Contains("calendar_authorization_invalid"))
        throw new CustomException.CalendarExpiredException();
      try
      {
        return JsonConvert.DeserializeObject<List<BindCalendarModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return new List<BindCalendarModel>();
      }
    }

    public static async Task<List<BindCalendarModel>> GetBindCalendarEvents(string calendarId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/calendar/bind/events/{0}", (object) calendarId), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (httpWebRequest.Contains("calendar_authorization_invalid"))
        throw new CustomException.CalendarExpiredException();
      CalendarService.TryLogResult(calendarId, httpWebRequest);
      try
      {
        return JsonConvert.DeserializeObject<List<BindCalendarModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return new List<BindCalendarModel>();
      }
    }

    public static async Task<List<BindCalendarModel>> GetExchangeEvents(
      string calendarId,
      DateTime? startTime,
      DateTime? endTime)
    {
      ref DateTime? local1 = ref startTime;
      DateTime valueOrDefault;
      string str1;
      if (!local1.HasValue)
      {
        str1 = (string) null;
      }
      else
      {
        valueOrDefault = local1.GetValueOrDefault();
        str1 = valueOrDefault.ToString(UtcDateTimeConverter.GetConverterValue(startTime.Value));
      }
      string str2 = str1;
      ref DateTime? local2 = ref endTime;
      string str3;
      if (!local2.HasValue)
      {
        str3 = (string) null;
      }
      else
      {
        valueOrDefault = local2.GetValueOrDefault();
        str3 = valueOrDefault.ToString(UtcDateTimeConverter.GetConverterValue(endTime.Value));
      }
      string str4 = str3;
      string content = "{ \"begin\": \"" + str2 + "\",\"end\": \"" + str4 + "\" }";
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/calendar/bind/events/exchange/{0}", (object) calendarId), content, auth: LocalSettings.Settings.LoginUserAuth);
      if (!startTime.HasValue && !endTime.HasValue)
        CalendarService.TryLogResult(calendarId, httpWebRequest);
      if (httpWebRequest.Contains("calendar_authorization_invalid"))
        throw new CustomException.CalendarExpiredException();
      try
      {
        return JsonConvert.DeserializeObject<List<BindCalendarModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return new List<BindCalendarModel>();
      }
    }

    public static async Task<ErrorModel> CheckExchangeAccount(string account)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/calendar/exchange/check?account={0}", (object) account), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<ErrorModel>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return (ErrorModel) null;
      }
    }

    public static async Task<BindCalendarAccountModel> BindExchangeAccount(BindAccountModel model)
    {
      string loginUserAuth = LocalSettings.Settings.LoginUserAuth;
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/bind/exchange", JsonConvert.SerializeObject((object) model), auth: loginUserAuth);
      if (httpWebRequest.Contains("calendar_bind_faild"))
        throw new CustomException.CalendarBindException("calendar_bind_faild");
      if (httpWebRequest.Contains("calendar_bind_duplicate"))
        throw new CustomException.CalendarBindException("calendar_bind_duplicate");
      return !httpWebRequest.Contains("calendar_bind_exceed") ? JsonConvert.DeserializeObject<BindCalendarAccountModel>(httpWebRequest) : throw new CustomException.CalendarBindException("calendar_bind_exceed");
    }

    public static async Task<string> UpdateExchangeAccount(string accoundId, BindAccountModel model)
    {
      string api = "/api/v2/calendar/exchange/update/" + accoundId;
      string loginUserAuth = LocalSettings.Settings.LoginUserAuth;
      string content = JsonConvert.SerializeObject((object) model);
      string auth = loginUserAuth;
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(api, content, auth: auth);
    }

    public static async Task<BindCalendarAccountModel> BindICloudAccount(BindAccountModel model)
    {
      string loginUserAuth = LocalSettings.Settings.LoginUserAuth;
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v4/calendar/bind/icloud", JsonConvert.SerializeObject((object) model), auth: loginUserAuth);
      if (httpWebRequest.Contains("caldav_bind_faild"))
        throw new CustomException.CalendarBindException("calendar_bind_faild");
      if (httpWebRequest.Contains("caldav_bind_duplicate"))
        throw new CustomException.CalendarBindException("calendar_bind_duplicate");
      return !httpWebRequest.Contains("caldav_bind_exceed") ? JsonConvert.DeserializeObject<BindCalendarAccountModel>(httpWebRequest) : throw new CustomException.CalendarBindException("calendar_bind_exceed");
    }

    public static async Task<string> UpdateICloudAccount(string accoundId, BindAccountModel model)
    {
      string api = "/api/v4/calendar/update/icloud/" + accoundId;
      string loginUserAuth = LocalSettings.Settings.LoginUserAuth;
      string content = JsonConvert.SerializeObject((object) model);
      string auth = loginUserAuth;
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(api, content, auth: auth);
    }

    public static async Task<string> GetICloudGuideUrl()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v4/calendar/icloud/support/url", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JObject.Parse(httpWebRequest)["url"]?.ToString();
      }
      catch (Exception ex)
      {
        return (string) null;
      }
    }

    public static async Task MergeTag(string tag, string newTag)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/tag/merge", JsonConvert.SerializeObject((object) new TagModifyModel()
      {
        name = tag,
        newName = newTag
      }), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT");
    }

    public static async Task<bool> CheckTagInShareTask(string tag)
    {
      try
      {
        return await ApiClient.PostJsonAsync<bool>(BaseUrl.GetUrl("/api/v2/tag/check/shareTask"), (object) tag);
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public static async Task<UserSettingsModel> GetUserSettings()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/preferences/settings?includeWeb=true", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return !string.IsNullOrEmpty(httpWebRequest) ? JsonConvert.DeserializeObject<UserSettingsModel>(httpWebRequest) : (UserSettingsModel) null;
      }
      catch (Exception ex)
      {
        return (UserSettingsModel) null;
      }
    }

    public static async Task SaveUserSettings(UserSettingsModel model)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/preferences/settings?includeWeb=true", JsonConvert.SerializeObject((object) model), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT");
    }

    public static async Task<UserPreferenceModel> GetUserPreference(long mtime)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/preferences/ext" + string.Format("?mtime={0}", (object) mtime), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return !string.IsNullOrEmpty(httpWebRequest) ? JsonConvert.DeserializeObject<UserPreferenceModel>(httpWebRequest) : (UserPreferenceModel) null;
      }
      catch (Exception ex)
      {
        return (UserPreferenceModel) null;
      }
    }

    public static async Task<bool> SaveUserPreference(UserPreferenceModel model)
    {
      JObject jobject1 = JObject.FromObject((object) model);
      JToken jtoken1;
      if (jobject1.TryGetValue("smartProjectsOption", out jtoken1) && jtoken1.SelectToken("smartProjects") is JArray jarray)
      {
        foreach (JToken jtoken2 in jarray)
        {
          if (jtoken2.SelectToken("timeline") is JObject jobject2)
          {
            jobject2.Remove("range");
            jobject2.Remove("DayWidthIndex");
            jobject2.Remove("sortType");
          }
        }
      }
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequestSuccess("/api/v2/user/preferences/ext", jobject1.ToString(), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<ProjectIdentityModel> DuplicateProject(
      string projectId,
      ProjectModel project,
      int type = 3)
    {
      type = Math.Max(1, Math.Min(3, type));
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/project/{0}/duplicate", (object) projectId) + "?option=" + type.ToString(), JsonConvert.SerializeObject((object) project), auth: LocalSettings.Settings.LoginUserAuth);
      return string.IsNullOrEmpty(httpWebRequest) ? (ProjectIdentityModel) null : JsonConvert.DeserializeObject<ProjectIdentityModel>(httpWebRequest);
    }

    public static async Task ResentVerifyEmail()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/resentVerifyEmail", auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<TaskAttendModel> LoadTaskAttendInfo(string attendId, string copyTaskId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/task-attend/{0}/attendees?taskId={1}", (object) attendId, (object) copyTaskId), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return !string.IsNullOrEmpty(httpWebRequest) ? JsonConvert.DeserializeObject<TaskAttendModel>(httpWebRequest) : (TaskAttendModel) null;
      }
      catch (Exception ex)
      {
        return (TaskAttendModel) null;
      }
    }

    public static async Task DeleteTaskAttend(string projectId, string taskId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/task-attend/{0}/delete/{1}", (object) projectId, (object) taskId), auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task<string> CreateTaskAttend(string projectId, string taskId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/task-attend/invitation/create?projectId={0}&taskId={1}", (object) projectId, (object) taskId), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      return !string.IsNullOrEmpty(httpWebRequest) ? httpWebRequest : string.Empty;
    }

    public static async Task<string> OpenInvitation(string attendId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/pub/api/v2/task-attend/invitation/{0}/open", (object) attendId), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      return !string.IsNullOrEmpty(httpWebRequest) ? httpWebRequest : string.Empty;
    }

    public static async Task<string> EchoInvitation(string attendId, string status)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/task-attend/invitation/{0}/response?status={1}", (object) attendId, (object) status), auth: LocalSettings.Settings.LoginUserAuth);
      return !string.IsNullOrEmpty(httpWebRequest) ? httpWebRequest : string.Empty;
    }

    public static async Task DeleteAttachment(string projectId, string taskId, string attachmentId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v1/attachment/{0}/{1}/{2}", (object) projectId, (object) taskId, (object) attachmentId), auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task RemoveAttendee(string projectId, string taskId, string userCode)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/task-attend/{0}/attendees/{1}?userCode={2}", (object) projectId, (object) taskId, (object) userCode), auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task RemoveAgenda(string projectId, string taskId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/task-attend/{0}/attend/{1}", (object) projectId, (object) taskId), auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task<BatchUpdateResult> BatchUpdateColumns(SyncColumnBean syncBean)
    {
      return Communicator.SafeConvertResult<BatchUpdateResult>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/column", JsonConvert.SerializeObject((object) syncBean), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<List<ColumnModel>> GetRemoteColumnsByProjectId(string projectId)
    {
      try
      {
        string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/column/project/{0}", (object) projectId), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
        return !string.IsNullOrEmpty(httpWebRequest) ? JsonConvert.DeserializeObject<List<ColumnModel>>(httpWebRequest) : new List<ColumnModel>();
      }
      catch (Exception ex)
      {
        return new List<ColumnModel>();
      }
    }

    public static async Task<SyncColumnBean> CheckRemoteColumnChanged(long checkPoint)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/column?from={0}", (object) checkPoint), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return !string.IsNullOrEmpty(httpWebRequest) ? JsonConvert.DeserializeObject<SyncColumnBean>(httpWebRequest) : new SyncColumnBean();
      }
      catch (Exception ex)
      {
        string msg = ExceptionUtils.BuildExceptionMessage(ex);
        if (App.IsAdmin)
          msg = msg + "\r\n" + httpWebRequest;
        UtilLog.Warn(msg);
      }
      return (SyncColumnBean) null;
    }

    public static async Task ClearTrash()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/trash/cleanUp", auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task<List<TeamModel>> GetAllTeams()
    {
      try
      {
        return JsonConvert.DeserializeObject<List<TeamModel>>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/teams", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET"));
      }
      catch (Exception ex)
      {
        return (List<TeamModel>) null;
      }
    }

    public static async Task<string> CreateTeam(string name)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/team", JsonConvert.SerializeObject((object) new ServerTeamModel()
      {
        name = name
      }), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<TeamMessageModel> GetTeamMessage(string teamId)
    {
      return JsonConvert.DeserializeObject<TeamMessageModel>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/team/" + teamId, auth: LocalSettings.Settings.LoginUserAuth, mode: "GET"));
    }

    public static async Task<string> GetTeamMember(string teamId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/team/{0}/members", (object) teamId), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET", isNeedErrorReturn: true);
    }

    public static async Task<ErrorModel> TransferProject(string projectId, string toUserCode)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/project/{0}/transfer?toUserCode={1}", (object) projectId, (object) toUserCode), auth: LocalSettings.Settings.LoginUserAuth);
      return !string.IsNullOrEmpty(httpWebRequest) ? JsonConvert.DeserializeObject<ErrorModel>(httpWebRequest) : (ErrorModel) null;
    }

    public static async Task<string> AcceptTeamApply(string id)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/team/collaboration/accept?notificationId={0}", (object) id), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT", isNeedErrorReturn: true);
      return !string.IsNullOrEmpty(httpWebRequest) ? httpWebRequest : string.Empty;
    }

    public static async Task<string> RefuseTeamApply(string notificationItemId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/team/collaboration/refuse?notificationId={0}", (object) notificationItemId), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT", isNeedErrorReturn: true);
      return string.IsNullOrEmpty(httpWebRequest) ? string.Empty : httpWebRequest;
    }

    public static async Task QueryMember(string teamId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/team/" + teamId + "/members", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
    }

    public static async Task<ErrorModel> UpgradeTeamProject(string projectId, string teamId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + "/upgrade?teamId=" + teamId, auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT", isNeedErrorReturn: true);
      return string.IsNullOrEmpty(httpWebRequest) ? (ErrorModel) null : JsonConvert.DeserializeObject<ErrorModel>(httpWebRequest);
    }

    public static async Task<ErrorModel> DegradeTeamProject(string projectId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + "/degrade", auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT", isNeedErrorReturn: true);
      return string.IsNullOrEmpty(httpWebRequest) ? (ErrorModel) null : JsonConvert.DeserializeObject<ErrorModel>(httpWebRequest);
    }

    public static async Task<int> CheckShareProjectQuota(string projectId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/project/{0}/share/check-quota", (object) projectId), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<int>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return 0;
      }
    }

    public static async Task<string> SetPermission(
      string projectId,
      Constants.ProjectPermission permission,
      string recordId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + "/permission?permission=" + permission.ToString() + "&recordId=" + recordId, auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT", isNeedErrorReturn: true);
    }

    public static async Task ApplyPermission(string projectId, Constants.ProjectPermission write)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/" + projectId + "/permission/apply?permission=" + write.ToString(), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT", isNeedErrorReturn: true);
    }

    public static async Task<string> AcceptApplyPermission(string notificationId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/permission/accept?notificationId=" + notificationId, auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT", isNeedErrorReturn: true);
    }

    public static async Task<string> RefuseApplyPermission(string notificationId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/project/permission/refuse?notificationId=" + notificationId, auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT", isNeedErrorReturn: true);
    }

    public static async Task<List<EventUpdateResult>> BatchUpdateCalendarEvents(SyncEventBean bean)
    {
      return Communicator.SafeConvertCalendarResult(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/bind/events/batch", JsonConvert.SerializeObject((object) bean), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<List<EventUpdateResult>> BatchUpdateCalDavEvents(SyncEventBean bean)
    {
      return Communicator.SafeConvertCalendarResult(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v4/calendar/bind/events/batch", JsonConvert.SerializeObject((object) bean), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<TemplatesModel> PullTemplates()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/templates", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<TemplatesModel>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return new TemplatesModel();
      }
    }

    public static async Task<BatchUpdateResult> PushTemplates(
      TaskTemplateSyncBean syncBean,
      bool isNote = false)
    {
      string str = JsonConvert.SerializeObject((object) syncBean);
      string api = "/api/v2/templates" + (isNote ? "/note" : "/task");
      string loginUserAuth = LocalSettings.Settings.LoginUserAuth;
      string content = str;
      string auth = loginUserAuth;
      return Communicator.SafeConvertResult<BatchUpdateResult>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(api, content, auth: auth, isNeedErrorReturn: true));
    }

    public static void UploadLogs(string content, string ticketId, string tt)
    {
      string host = "support.dida365.com";
      if (!BaseUrl.IsDefault)
      {
        string ip = HttpDnsHandler.GetIp(host);
        if (string.IsNullOrEmpty(ip))
          host = ip;
      }
      ticktick_WPF.Util.Network.Network.PostLog("https://" + host + "/api/v1/ticket/" + ticketId + "/attachment", content, tt);
    }

    public static void MarkReadNotification(string category)
    {
      ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/notification/markRead?category=" + category, auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT");
    }

    public static async Task<List<HabitSectionModel>> PullRemoteHabitSections()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/habitSections", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (!string.IsNullOrEmpty(httpWebRequest))
      {
        try
        {
          return JsonConvert.DeserializeObject<List<HabitSectionModel>>(httpWebRequest);
        }
        catch (Exception ex)
        {
        }
      }
      return (List<HabitSectionModel>) null;
    }

    public static async Task<List<HabitModel>> PullRemoteHabits()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/habits", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      List<HabitModel> habitModelList;
      if (!string.IsNullOrEmpty(httpWebRequest))
      {
        try
        {
          habitModelList = JsonConvert.DeserializeObject<List<HabitModel>>(httpWebRequest);
        }
        catch (Exception ex)
        {
          string msg = ExceptionUtils.BuildExceptionMessage(ex);
          if (App.IsAdmin)
            msg = msg + "\r\n" + httpWebRequest;
          UtilLog.Warn(msg);
        }
      }
      return habitModelList;
    }

    public static async Task<TaskModel> GetTask(string taskId, string projectId, bool withChildren = false)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/task/{0}?projectId={1}&withChildren={2}", (object) taskId, (object) projectId, (object) withChildren), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (string.IsNullOrEmpty(httpWebRequest))
        return (TaskModel) null;
      try
      {
        TaskModel task = JsonConvert.DeserializeObject<TaskModel>(httpWebRequest);
        if (task?.id == null)
          UtilLog.Warn(httpWebRequest);
        return task;
      }
      catch (Exception ex)
      {
        return (TaskModel) null;
      }
    }

    public static async Task<string> CommitHabitCheckIns(SyncHabitCheckInBean bean)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/habitCheckins/batch", JsonConvert.SerializeObject((object) bean), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<string> CommitHabitRecords(SyncHabitRecordBean bean)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/habitRecords", JsonConvert.SerializeObject((object) bean), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<List<HabitRecordModel>> PullHabitRecords(string habitId, int stamp)
    {
      if (!Utils.IsNetworkAvailable())
        return (List<HabitRecordModel>) null;
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/habitRecords/" + habitId + (stamp > 0 ? "?afterStamp=" + stamp.ToString() : string.Empty), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET", isNeedErrorReturn: true);
      if (!string.IsNullOrEmpty(httpWebRequest))
      {
        try
        {
          return JsonConvert.DeserializeObject<List<HabitRecordModel>>(httpWebRequest);
        }
        catch (Exception ex)
        {
        }
      }
      return (List<HabitRecordModel>) null;
    }

    public static async Task<HabitRecordCheckResult> PullHabitsRecords(
      List<string> habitIds,
      int stamp)
    {
      if (!Utils.IsNetworkAvailable())
        return (HabitRecordCheckResult) null;
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/habitRecords/" + Communicator.BuildHabitArgIds((IEnumerable<string>) habitIds) + (stamp > 0 ? "&afterStamp=" + stamp.ToString() : string.Empty), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (!string.IsNullOrEmpty(httpWebRequest))
      {
        try
        {
          return JsonConvert.DeserializeObject<HabitRecordCheckResult>(httpWebRequest);
        }
        catch (Exception ex)
        {
        }
      }
      return (HabitRecordCheckResult) null;
    }

    public static async Task<CheckInCollection> PullHabitsCheckIns(List<string> habitIds, int stamp)
    {
      if (!Utils.IsNetworkAvailable())
        return (CheckInCollection) null;
      JObject jobject = new JObject();
      JArray jarray = new JArray();
      foreach (string habitId in habitIds)
        jarray.Add((JToken) habitId);
      jobject.Add(nameof (habitIds), (JToken) jarray);
      jobject.Add("afterStamp", (JToken) stamp);
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/habitCheckins/query", JsonConvert.SerializeObject((object) jobject), auth: LocalSettings.Settings.LoginUserAuth);
      if (!string.IsNullOrEmpty(httpWebRequest))
      {
        try
        {
          return JsonConvert.DeserializeObject<CheckInCollection>(httpWebRequest);
        }
        catch (Exception ex)
        {
          string msg = ExceptionUtils.BuildExceptionMessage(ex);
          if (App.IsAdmin)
            msg = msg + "\r\n" + httpWebRequest;
          UtilLog.Warn(msg);
        }
      }
      return (CheckInCollection) null;
    }

    private static string BuildHabitArgIds(IEnumerable<string> habitIds)
    {
      StringBuilder stringBuilder = new StringBuilder();
      stringBuilder.Append("?");
      foreach (string habitId in habitIds)
        stringBuilder.Append("habitIds=" + habitId + "&");
      string str = stringBuilder.ToString();
      if (str.EndsWith("&"))
        str = str.Substring(0, str.Length - 1);
      return str;
    }

    public static async Task<List<HabitCheckInModel>> PullHabitCheckIns(string habitId, int stamp)
    {
      if (!Utils.IsNetworkAvailable())
        return (List<HabitCheckInModel>) null;
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/habitCheckins/" + habitId + (stamp > 0 ? "?afterStamp=" + stamp.ToString() : string.Empty), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (string.IsNullOrEmpty(httpWebRequest))
        return (List<HabitCheckInModel>) null;
      try
      {
        return JsonConvert.DeserializeObject<List<HabitCheckInModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return (List<HabitCheckInModel>) null;
      }
    }

    public static async Task<HabitSettingsModel> GetRemoteHabitConfig()
    {
      try
      {
        string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/preferences/habit?platform=windows", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
        return string.IsNullOrEmpty(httpWebRequest) ? (HabitSettingsModel) null : JsonConvert.DeserializeObject<HabitSettingsModel>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return (HabitSettingsModel) null;
      }
    }

    public static async Task PushHabitConfig(HabitSettingsModel config)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/preferences/habit?platform=windows", JsonConvert.SerializeObject((object) config), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT");
    }

    public static async Task<string> UpdateHabit(SyncHabitBean habitBean)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/habits/batch", JsonConvert.SerializeObject((object) habitBean), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<BatchUpdateResult> UpdateHabitSections(
      SyncHabitSectionsBean sectionBean)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/habitSections/batch", JsonConvert.SerializeObject((object) sectionBean), auth: LocalSettings.Settings.LoginUserAuth);
      return httpWebRequest == null || !httpWebRequest.Contains("id2error") && !httpWebRequest.Contains("id2etag") ? (BatchUpdateResult) null : JsonConvert.DeserializeObject<BatchUpdateResult>(httpWebRequest);
    }

    public static async Task<string> RegNewUser(string guid)
    {
      string str1 = "\"event_type\":\"user-activation\"";
      string str2 = "\"device_id\":\"" + guid + "\"";
      string str3 = "\"timestamp\":\"" + DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss'" + DateTime.Now.ToString("zzz").Replace(":", "") + "'") + "\"";
      string str4 = "\"device_type\":\"Windows_" + Utils.GetWindowsVersion() + "\"";
      string str5 = "\"version\":\"" + Utils.GetVersion() + "\"";
      string str6 = "\"user_id\":null";
      string str7 = "\"channel\":\"win_app\"";
      string str8 = "\"ip\":\"192.168.1.143\"";
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("https://" + (Utils.IsDida() ? "a.dida365.com" : "a.ticktick.com") + "/data/api/v1", "{" + str1 + "," + str2 + "," + str3 + "," + str4 + "," + str5 + "," + str6 + "," + str7 + "," + str8 + "}", fulluri: true);
    }

    public static async Task<PushModel> RegisterPushService(string id, string pushToken)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/push/register", JsonConvert.SerializeObject((object) new PushModel()
      {
        id = id,
        pushToken = pushToken,
        hl = Utils.GetLanguage()
      }), auth: LocalSettings.Settings.LoginUserAuth);
      try
      {
        return JsonConvert.DeserializeObject<PushModel>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return (PushModel) null;
      }
    }

    public static async Task NotifyCloseReminder(RemindMessage model)
    {
      UtilLog.Warn("close remider :" + await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/task/closeRemind", JsonConvert.SerializeObject((object) model), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task NotifyPushArrive(PushArriveModel model)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/push/stats/arrive", JsonConvert.SerializeObject((object) new List<PushArriveModel>()
      {
        model
      }), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<SyncTaskParentRes> BatchPushTaskParent(List<SyncTaskParentModel> models)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/batch/taskParent", JsonConvert.SerializeObject((object) models), auth: LocalSettings.Settings.LoginUserAuth);
      if (string.IsNullOrEmpty(httpWebRequest))
        return new SyncTaskParentRes();
      try
      {
        return JsonConvert.DeserializeObject<SyncTaskParentRes>(httpWebRequest);
      }
      catch (Exception ex)
      {
        string msg = ExceptionUtils.BuildExceptionMessage(ex);
        if (App.IsAdmin)
          msg = msg + "\r\n" + httpWebRequest;
        UtilLog.Warn(msg);
        return new SyncTaskParentRes();
      }
    }

    public static async Task SetTaskAssign(List<AssignModel> models)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/task/assign", JsonConvert.SerializeObject((object) models), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<SearchResultModel> GetSearchedResult(
      string searchKey,
      SearchFilterModel filterModel)
    {
      string str = string.Format("/api/v2/search/all?keywords={0}", (object) searchKey);
      if (filterModel.ProjectIds != null)
        str = filterModel.ProjectIds.Aggregate<string, string>(str, (Func<string, string, string>) ((current, projectId) => current + "&projectId=" + projectId));
      if (filterModel.Tags != null)
        str = filterModel.Tags.Aggregate<string, string>(str, (Func<string, string, string>) ((current, tag) => current + "&tags=" + tag));
      if (filterModel.Status.HasValue)
        str = str + "&status=" + filterModel.Status.Value.ToString();
      if (filterModel.Start.HasValue)
        str = str + "&dueFrom=" + filterModel.Start.Value.ToString(UtcDateTimeConverter.GetConverterValue(filterModel.Start.Value));
      if (filterModel.End.HasValue)
        str = str + "&dueTo=" + filterModel.End.Value.ToString(UtcDateTimeConverter.GetConverterValue(filterModel.End.Value));
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(str, auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (!string.IsNullOrEmpty(httpWebRequest))
      {
        try
        {
          SearchResultModel searchedResult = JsonConvert.DeserializeObject<SearchResultModel>(httpWebRequest);
          if (searchedResult != null)
          {
            if (searchedResult.Tasks.Count > 0)
            {
              foreach (TaskModel task in searchedResult.Tasks)
              {
                TaskService.ClearDueDate(task);
                if (string.IsNullOrEmpty(task.userId))
                  task.userId = Utils.GetCurrentUserIdInt().ToString();
                task.GetStringFromArray();
              }
            }
            return searchedResult;
          }
        }
        catch (Exception ex)
        {
          return (SearchResultModel) null;
        }
      }
      return (SearchResultModel) null;
    }

    public static async Task<List<TaskModel>> GetSearchedTasks(
      string searchKey,
      SearchFilterModel filterModel)
    {
      string str = string.Format("/api/v2/search/task?keywords={0}", (object) searchKey);
      if (filterModel.ProjectIds != null)
        str = filterModel.ProjectIds.Aggregate<string, string>(str, (Func<string, string, string>) ((current, projectId) => current + "&projectId=" + projectId));
      if (filterModel.Tags != null)
        str = filterModel.Tags.Aggregate<string, string>(str, (Func<string, string, string>) ((current, tag) => current + "&tags=" + tag));
      if (filterModel.Status.HasValue)
        str = str + "&status=" + filterModel.Status.Value.ToString();
      if (filterModel.Start.HasValue)
        str = str + "&dueFrom=" + filterModel.Start.Value.ToString(UtcDateTimeConverter.GetConverterValue(filterModel.Start.Value));
      if (filterModel.End.HasValue)
        str = str + "&dueTo=" + filterModel.End.Value.ToString(UtcDateTimeConverter.GetConverterValue(filterModel.End.Value));
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(str, auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (!string.IsNullOrEmpty(httpWebRequest))
      {
        try
        {
          List<TaskModel> searchedTasks = JsonConvert.DeserializeObject<List<TaskModel>>(httpWebRequest);
          if (searchedTasks != null)
          {
            if (searchedTasks.Count > 0)
            {
              foreach (TaskModel task in searchedTasks)
              {
                TaskService.ClearDueDate(task);
                if (string.IsNullOrEmpty(task.userId))
                  task.userId = Utils.GetCurrentUserIdInt().ToString();
                if (task.tags != null)
                  task.tag = TagSerializer.ToJsonContent(((IEnumerable<string>) task.tags).ToList<string>());
                if (task.exDate != null)
                  task.exDates = ExDateSerilizer.ToString(task.exDate);
              }
              return searchedTasks;
            }
          }
        }
        catch (Exception ex)
        {
          return new List<TaskModel>();
        }
      }
      return new List<TaskModel>();
    }

    public static async Task<AttachmentModel> UpdateAttachment(
      AttachmentModel attachment,
      string taskId,
      string projectId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v1/attachment/{0}/{1}/{2}", (object) projectId, (object) taskId, (object) attachment.id) + "/status?status=" + attachment.status.ToString(), auth: LocalSettings.Settings.LoginUserAuth);
      try
      {
        AttachmentModel attachmentModel = JsonConvert.DeserializeObject<AttachmentModel>(httpWebRequest);
        if (attachmentModel?.id == attachment.id)
          return attachmentModel;
      }
      catch (Exception ex)
      {
      }
      return (AttachmentModel) null;
    }

    public static async Task<UserModel> LoginApple(
      string accessToken,
      string uId,
      string email,
      string username)
    {
      string api = BaseUrl.GetApiDomain() + "/api/v2/user/sign/OAuth2/apple?accessToken=" + accessToken + "&uId=" + uId + "&platform=windows";
      JObject jobject = new JObject();
      if (!string.IsNullOrEmpty(email))
        jobject.Add(nameof (email), (JToken) email);
      if (!string.IsNullOrEmpty(username))
        jobject.Add("name", (JToken) username);
      string loginReturn = string.Empty;
      try
      {
        loginReturn = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(api, jobject.ToString(), fulluri: true, isNeedErrorReturn: true);
        UserModel userModel = JsonConvert.DeserializeObject<UserModel>(loginReturn);
        if (userModel != null && userModel.userId != null && userModel.userId != "")
          return userModel;
        return new UserModel() { username = loginReturn };
      }
      catch (Exception ex)
      {
        return new UserModel() { username = loginReturn };
      }
    }

    public static async Task PullAppConfig(bool fromLogin = false)
    {
      LocalSettings settings = LocalSettings.Settings;
      string uri = BaseUrl.GetApiDomain() + "/pub/api/v1/app/config";
      if (fromLogin)
        uri += "?from=login";
      string appConfigEtag = settings.ExtraSettings.AppConfigEtag;
      if (!string.IsNullOrEmpty(appConfigEtag))
        uri = uri + "?etag=" + appConfigEtag;
      Dictionary<string, object> jsonAsync = await ApiClient.GetJsonAsync<Dictionary<string, object>>(uri);
      if (jsonAsync == null || jsonAsync.Count <= 0)
        return;
      foreach (string key in jsonAsync.Keys)
      {
        if (string.Equals(key, "etag"))
        {
          LocalSettings.Settings.ExtraSettings.AppConfigEtag = jsonAsync[key].ToString();
          LocalSettings.Settings.Save();
        }
        else if (string.Equals(key, "interval"))
        {
          int num = int.Parse(jsonAsync[key].ToString());
          int appConfigInterval = LocalSettings.Settings.ExtraSettings.AppConfigInterval;
          if (num != appConfigInterval)
          {
            LocalSettings.Settings.ExtraSettings.AppConfigInterval = num;
            LocalSettings.Settings.Save();
            AppConfigManager.StartPolling();
          }
        }
        else if (string.Equals(key, "ab"))
        {
          AbBestData abBestData = JsonConvert.DeserializeObject<AbBestData>(jsonAsync[key].ToString());
          if (abBestData?.best != null && abBestData.best.Count > 0)
            ABTestManager.HandleBestTest(abBestData.best);
        }
      }
    }

    public static async Task<TabPlanData> ApplyABTestGroupResult(TestInfo testInfo)
    {
      string uri = "https://xapi.dida365.com/datacollect/pub/v1/ab/group";
      try
      {
        return await ApiClient.PostJsonStrAsync<TabPlanData>(uri, JsonConvert.SerializeObject((object) testInfo));
      }
      catch (Exception ex)
      {
        Console.WriteLine((object) ex);
        return new TabPlanData();
      }
    }

    public static async Task<TestCode2PlanCodeInfo> GetABTestGroupResult(BatchTestInfo testInfo)
    {
      string uri = "https://xapi.dida365.com/datacollect/pub/v1/ab/group/result";
      try
      {
        return await ApiClient.PostJsonStrAsync<TestCode2PlanCodeInfo>(uri, JsonConvert.SerializeObject((object) testInfo));
      }
      catch (Exception ex)
      {
        Console.WriteLine((object) ex);
        return new TestCode2PlanCodeInfo();
      }
    }

    public static async Task<bool> PostUserDevice(string json)
    {
      return await ticktick_WPF.Util.Network.Network.PostDataCollect("https://xapi.dida365.com/datacollect/device/update", json);
    }

    public static async Task PostException(Exception e, ExceptionType type)
    {
      string uri = "https://xapi.dida365.com/datacollect/log/ex";
      try
      {
        string json = JsonConvert.SerializeObject((object) new ExceptionCollectModel(e, type));
        int num = await ticktick_WPF.Util.Network.Network.PostDataCollect(uri, json) ? 1 : 0;
      }
      catch (Exception ex)
      {
      }
    }

    public static async Task<bool> PostUserEvents(List<string> postFiles)
    {
      return await ticktick_WPF.Util.Network.Network.PostFiles("https://xapi.dida365.com/datacollect/event/upload", postFiles);
    }

    [Obsolete]
    public static async void PostUserLogs()
    {
      List<string> logs = TicketLogUtils.GetUnPostLogs();
      string uri = "https://xapi.dida365.com/datacollect/log/upload";
      if (logs == null)
        logs = (List<string>) null;
      else if (logs.Count == 0)
        logs = (List<string>) null;
      else if (!await ticktick_WPF.Util.Network.Network.PostFiles(uri, logs))
      {
        logs = (List<string>) null;
      }
      else
      {
        using (List<string>.Enumerator enumerator = logs.GetEnumerator())
        {
          while (enumerator.MoveNext())
          {
            string current = enumerator.Current;
            if (File.Exists(current))
            {
              string str = Path.GetFileName(current).Replace("log", "postlog");
              string destFileName = Path.GetDirectoryName(current) + "\\" + str;
              FileInfo fileInfo = new FileInfo(current);
              fileInfo.Attributes = FileAttributes.Normal;
              try
              {
                fileInfo.MoveTo(destFileName);
              }
              catch
              {
                fileInfo.Delete();
              }
            }
          }
          logs = (List<string>) null;
        }
      }
    }

    public static async Task<YearPromoModel> GetYearPromo()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/pub/api/v1/promo/year2021", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<YearPromoModel>(httpWebRequest) ?? new YearPromoModel();
      }
      catch (Exception ex)
      {
        return (YearPromoModel) null;
      }
    }

    public static async Task<SubscribeCalendarModel> GetSubscribeEventsByCalId(string calendarId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/subscribe/events/" + calendarId, auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (httpWebRequest != null && httpWebRequest.Contains("calendar_fetch"))
        throw new CustomException.CalendarExpiredException();
      try
      {
        CalendarService.TryLogResult(calendarId, httpWebRequest);
        return JsonConvert.DeserializeObject<SubscribeCalendarModel>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return (SubscribeCalendarModel) null;
      }
    }

    public static async Task<List<CourseScheduleModel>> GetAllCourseSchedules()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v1/course/timetable", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<List<CourseScheduleModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return (List<CourseScheduleModel>) null;
      }
    }

    public static async Task<BindCalendarAccountModel> SubscribeOutlook(string code)
    {
      return await Communicator.BindCalendarAccount(code, "outlook", Utils.UrlEncode("https://" + BaseUrl.GetDomain() + "/calendar/bind/outlook"));
    }

    public static async Task<OutlookCalendarModels> GetBindOutlookEvents(
      DateTime? pullStart,
      DateTime? pullEnd)
    {
      ref DateTime? local1 = ref pullStart;
      DateTime valueOrDefault;
      string str1;
      if (!local1.HasValue)
      {
        str1 = (string) null;
      }
      else
      {
        valueOrDefault = local1.GetValueOrDefault();
        str1 = valueOrDefault.ToString(UtcDateTimeConverter.GetConverterValue(pullStart.Value));
      }
      string str2 = str1;
      ref DateTime? local2 = ref pullEnd;
      string str3;
      if (!local2.HasValue)
      {
        str3 = (string) null;
      }
      else
      {
        valueOrDefault = local2.GetValueOrDefault();
        str3 = valueOrDefault.ToString(UtcDateTimeConverter.GetConverterValue(pullEnd.Value));
      }
      string str4 = str3;
      string content = "{ \"begin\": \"" + str2 + "\",\"end\": \"" + str4 + "\" }";
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/calendar/bind/events/outlook"), content, auth: LocalSettings.Settings.LoginUserAuth);
      if (!pullStart.HasValue && !pullEnd.HasValue)
        CalendarService.TryLogResult("outlook", httpWebRequest);
      try
      {
        return JsonConvert.DeserializeObject<OutlookCalendarModels>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return (OutlookCalendarModels) null;
      }
    }

    public static async Task<bool> PostArchivedCourses(CourseArchivePostModel model)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequestSuccess("/api/v1/course/archived", JsonConvert.SerializeObject((object) model), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<List<CourseArchiveSyncModel>> GetArchivedCourses()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v1/course/archived", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<List<CourseArchiveSyncModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return (List<CourseArchiveSyncModel>) null;
      }
    }

    public static async Task<bool> PostArchivedEvents(EventArchivePostModel model)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequestSuccess("/api/v2/calendar/archivedEvent", JsonConvert.SerializeObject((object) model), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<List<EventArchiveSyncModel>> GetArchivedEvents()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/archivedEvent", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<List<EventArchiveSyncModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        return (List<EventArchiveSyncModel>) null;
      }
    }

    public static async Task<FocusSyncResultBean> UploadFocusOptions(List<FocusOptionModel> options)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/focus/batch/focusOp", JsonConvert.SerializeObject((object) new FocusOptionBean()
      {
        opList = options,
        lastPoint = LocalSettings.Settings.FocusPushPoint
      }), auth: LocalSettings.Settings.LoginUserAuth, useMs: true);
      try
      {
        return JsonConvert.DeserializeObject<FocusSyncResultBean>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return (FocusSyncResultBean) null;
      }
    }

    public static async Task<StatisticsModel> GetStatistics()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/pomodoros/statistics/generalForDesktop", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<StatisticsModel>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return (StatisticsModel) null;
      }
    }

    public static async Task<List<PomoTask>> GetRecentFocusTasks()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/pomodoro/bind/recent", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<List<PomoTask>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return (List<PomoTask>) null;
      }
    }

    public static async Task<List<PomodoroModel>> GetFocusTimeline(long point)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/pomodoros/timeline?to={0}", (object) point), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<List<PomodoroModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return (List<PomodoroModel>) null;
      }
    }

    public static async Task<List<TimerModel>> PullRemoteTimers()
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/timer", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      List<TimerModel> timerModelList;
      if (!string.IsNullOrEmpty(httpWebRequest))
      {
        try
        {
          timerModelList = JsonConvert.DeserializeObject<List<TimerModel>>(httpWebRequest);
        }
        catch (Exception ex)
        {
          string msg = ExceptionUtils.BuildExceptionMessage(ex);
          if (App.IsAdmin)
            msg = msg + "\r\n" + httpWebRequest;
          UtilLog.Warn(msg);
        }
      }
      return timerModelList;
    }

    public static async Task<BatchUpdateResult> UpdateTimers(ModelSyncBean<TimerModel> timerBean)
    {
      return Communicator.SafeConvertResult<BatchUpdateResult>(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/timer", JsonConvert.SerializeObject((object) timerBean), auth: LocalSettings.Settings.LoginUserAuth));
    }

    public static async Task<TimerOverviewModel> PullTimerOverview(string id)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/timer/overview/" + id, auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (!string.IsNullOrEmpty(httpWebRequest))
      {
        if (!httpWebRequest.Contains("error"))
        {
          try
          {
            return JsonConvert.DeserializeObject<TimerOverviewModel>(httpWebRequest);
          }
          catch (Exception ex)
          {
            return (TimerOverviewModel) null;
          }
        }
      }
      return (TimerOverviewModel) null;
    }

    public static async Task<string> PullTimerStatistics(
      string id,
      DateTime start,
      DateTime end,
      string interval)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/timer/statistics/{0}/{1}/{2}/{3}", (object) id, (object) DateUtils.GetDateNum(start), (object) DateUtils.GetDateNum(end), (object) interval), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      if (string.IsNullOrEmpty(httpWebRequest))
        return (string) null;
      try
      {
        return httpWebRequest;
      }
      catch (Exception ex)
      {
        return (string) null;
      }
    }

    public static async Task<List<PomodoroModel>> GetTimerFocusTimeline(string timerId, long point)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/timer/timeline/{0}?to={1}", (object) timerId, (object) point), auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        return JsonConvert.DeserializeObject<List<PomodoroModel>>(httpWebRequest);
      }
      catch (Exception ex)
      {
        UtilLog.Warn(httpWebRequest);
        return (List<PomodoroModel>) null;
      }
    }

    public static async Task<string> HandleTeamInvite(string notificationId, bool accept)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest(string.Format("/api/v2/team/accept/invite?notificationId={0}&actionStatus={1}", (object) notificationId, (object) (accept ? 22 : 23)), auth: LocalSettings.Settings.LoginUserAuth, isNeedErrorReturn: true);
    }

    public static async Task<long> GetTeamShareCount(string teamId)
    {
      string httpWebRequest = await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/team/" + teamId + "/shareProjectNum", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
      try
      {
        if (httpWebRequest.Contains("current"))
        {
          if (httpWebRequest.Contains("max"))
          {
            JObject jobject = JObject.Parse(httpWebRequest);
            JToken jtoken1;
            if (jobject.TryGetValue("current", out jtoken1))
            {
              JToken jtoken2;
              if (jobject.TryGetValue("max", out jtoken2))
                return jtoken2.Value<long>() - jtoken1.Value<long>();
            }
          }
        }
      }
      catch (Exception ex)
      {
        return 1;
      }
      return 1;
    }

    public static async Task<(string, bool)> SetProjectOpenToTeam(string projectId, bool isChecked)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequestResult("/api/v2/project", new JObject()
      {
        {
          "id",
          (JToken) projectId
        },
        {
          "openToTeam",
          (JToken) isChecked
        }
      }.ToString(), auth: LocalSettings.Settings.LoginUserAuth, mode: "PUT", isNeedErrorReturn: true);
    }

    public static async Task<bool> IsTwitterUser()
    {
      try
      {
        JToken jtoken1;
        if (JObject.Parse(await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/user/userBindingInfo", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET")).TryGetValue("thirdSiteBinds", out jtoken1))
        {
          if (jtoken1 is JArray jarray)
          {
            foreach (JToken jtoken2 in jarray)
            {
              JToken jtoken3;
              if (jtoken2 is JObject jobject && jobject.TryGetValue("siteId", out jtoken3) && jtoken3.ToString() == "7")
                return true;
            }
          }
        }
      }
      catch (Exception ex)
      {
      }
      return false;
    }

    public static async Task<bool> UpdateReminderDelayModels(ReminderDelaySyncModel syncModel)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequestSuccess("/api/v2/reminder/delay", JsonConvert.SerializeObject((object) syncModel), auth: LocalSettings.Settings.LoginUserAuth);
    }

    public static async Task<string> GetProjectFeedsCode(string projectId)
    {
      (string, bool) webRequestResult = await ticktick_WPF.Util.Network.Network.GetHttpWebRequestResult("/api/v2/calendar/feeds/code/new/" + projectId, auth: LocalSettings.Settings.LoginUserAuth);
      return !webRequestResult.Item2 ? (string) null : webRequestResult.Item1;
    }

    public static async Task<string> GetAllProjectFeedsCode()
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequest("/api/v2/calendar/feeds/code", auth: LocalSettings.Settings.LoginUserAuth, mode: "GET");
    }

    public static async Task<bool> DeleteProjectFeedsCode(string projectId)
    {
      return await ticktick_WPF.Util.Network.Network.GetHttpWebRequestSuccess("/api/v2/calendar/feeds/cancel/" + projectId, auth: LocalSettings.Settings.LoginUserAuth, mode: "DELETE");
    }

    public static async Task<BatchUpdateResult> MoveColumnAsync(List<MoveColumnArgs> args)
    {
      return await ApiClient.PostJsonAsync<BatchUpdateResult>(BaseUrl.GetUrl("/api/v2/batch/columnProject"), (object) args);
    }

    public static async Task<GuideProjectModel> GetGuideProject(string plan)
    {
      return await ApiClient.GetJsonAsync<GuideProjectModel>(BaseUrl.GetUrl(string.Format("/pub/api/v2/guide/project?p={0}", (object) plan)));
    }

    public static async Task<List<TaskModel>> GetGuideTask()
    {
      return await ApiClient.GetJsonAsync<List<TaskModel>>(BaseUrl.GetUrl(string.Format("/pub/api/v2/guide/task")));
    }
  }
}
