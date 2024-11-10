// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Util.AvatarHelper
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ticktick_WPF.Cache;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Resource;
using ticktick_WPF.ViewModels;
using ticktick_WPF.Views.MainListView;
using TickTickUtils;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Util
{
  public static class AvatarHelper
  {
    private static readonly ConcurrentDictionary<string, List<ShareUserModel>> ShareListCache = new ConcurrentDictionary<string, List<ShareUserModel>>();
    private static readonly ConcurrentDictionary<string, List<ShareUserModel>> UserListCache = new ConcurrentDictionary<string, List<ShareUserModel>>();
    private static Dictionary<string, ShareUserModel> UserCache = new Dictionary<string, ShareUserModel>();
    private static ConcurrentDictionary<string, BitmapImage> Avatars = new ConcurrentDictionary<string, BitmapImage>();
    private const string AllShares = "allshares";
    private static readonly Dictionary<string, DateTime> LastPullTimeDict = new Dictionary<string, DateTime>();

    public static ConcurrentDictionary<string, List<ShareUserModel>> GetProjectUserCache(
      string projectId,
      bool checkAllUsers = true)
    {
      ProjectModel projectById = CacheManager.GetProjectById(projectId);
      return !checkAllUsers || projectById == null || !projectById.OpenToTeam ? AvatarHelper.ShareListCache : AvatarHelper.UserListCache;
    }

    public static async Task GetAllProjectAvatars()
    {
      string avatarsData;
      if (!CacheManager.GetProjects().Any<ProjectModel>((Func<ProjectModel, bool>) (p => p.IsShareList())))
      {
        avatarsData = (string) null;
      }
      else
      {
        avatarsData = await Communicator.GetAllShareContacts();
        Dictionary<string, List<ShareUserModel>> source;
        try
        {
          source = JsonConvert.DeserializeObject<Dictionary<string, List<ShareUserModel>>>(avatarsData);
        }
        catch (Exception ex)
        {
          source = (Dictionary<string, List<ShareUserModel>>) null;
        }
        bool saveRemote = source != null;
        if (source == null)
        {
          TempDataModel tempDataModel = await TempDataDao.QueryTempDataModelListDbByTypeAndEntityIdAsync("SHAREUSERLIST", "allshares");
          try
          {
            source = JsonConvert.DeserializeObject<Dictionary<string, List<ShareUserModel>>>(tempDataModel.Data);
          }
          catch (Exception ex)
          {
            source = (Dictionary<string, List<ShareUserModel>>) null;
          }
        }
        if (source != null && source.Any<KeyValuePair<string, List<ShareUserModel>>>())
        {
          foreach (KeyValuePair<string, List<ShareUserModel>> keyValuePair in source)
          {
            string key = keyValuePair.Key;
            List<ShareUserModel> shareUserModelList = AvatarHelper.Sort(keyValuePair.Value);
            ConcurrentDictionary<string, List<ShareUserModel>> projectUserCache = AvatarHelper.GetProjectUserCache(key);
            if (!projectUserCache.ContainsKey(key))
              projectUserCache.TryAdd(key, shareUserModelList);
            else
              projectUserCache[key] = shareUserModelList;
          }
          if (saveRemote)
            TempDataDao.UpdateOrInsertTempDataModelListDbByTypeAndEntityIdAsync(new TempDataModel()
            {
              User_Id = Utils.GetCurrentUserIdInt().ToString(),
              DataType = "SHAREUSERLIST",
              ModifyTime = Utils.GetNowTimeStamp(),
              Data = avatarsData,
              EntityId = "allshares"
            });
        }
        AvatarHelper.SetUserCache();
        avatarsData = (string) null;
      }
    }

    public static void SetUserCache()
    {
      List<List<ShareUserModel>> list = AvatarHelper.ShareListCache.Values.ToList<List<ShareUserModel>>();
      Dictionary<string, ShareUserModel> dictionary = new Dictionary<string, ShareUserModel>();
      // ISSUE: explicit non-virtual call
      foreach (IEnumerable<ShareUserModel> source in list.Where<List<ShareUserModel>>((Func<List<ShareUserModel>, bool>) (users => users != null && __nonvirtual (users.Count) > 0)))
      {
        foreach (ShareUserModel shareUserModel in source.Where<ShareUserModel>((Func<ShareUserModel, bool>) (user => user.userId.HasValue)))
        {
          if (!dictionary.ContainsKey(shareUserModel.userId.ToString()))
            dictionary.Add(shareUserModel.userId.ToString(), shareUserModel);
        }
      }
      // ISSUE: explicit non-virtual call
      foreach (IEnumerable<ShareUserModel> source in AvatarHelper.UserListCache.Values.ToList<List<ShareUserModel>>().Where<List<ShareUserModel>>((Func<List<ShareUserModel>, bool>) (users => users != null && __nonvirtual (users.Count) > 0)))
      {
        foreach (ShareUserModel shareUserModel in source.Where<ShareUserModel>((Func<ShareUserModel, bool>) (user => user.userId.HasValue)))
        {
          if (!dictionary.ContainsKey(shareUserModel.userId.ToString()))
            dictionary.Add(shareUserModel.userId.ToString(), shareUserModel);
        }
      }
      AvatarHelper.UserCache = dictionary;
    }

    public static ShareUserModel GetUserModelById(string userId)
    {
      return !string.IsNullOrEmpty(userId) && AvatarHelper.UserCache.ContainsKey(userId) ? AvatarHelper.UserCache[userId] : (ShareUserModel) null;
    }

    public static Dictionary<string, ShareUserModel> GetAllShareUsers() => AvatarHelper.UserCache;

    public static async Task ResetProjectAvatars(string projectId, bool force = false, bool checkAllUsers = true)
    {
      List<ShareUserModel> remoteShareUsers = await AvatarHelper.GetRemoteShareUsers(projectId, force, checkAllUsers);
      if (remoteShareUsers == null || !remoteShareUsers.Any<ShareUserModel>())
        return;
      int num = remoteShareUsers.Count<ShareUserModel>((Func<ShareUserModel, bool>) (c => c.isAccept));
      if (num >= 1)
      {
        ProjectModel projectById = CacheManager.GetProjectById(projectId);
        if (projectById != null && projectById.userCount != num)
        {
          projectById.userCount = num;
          CacheManager.UpdateProject(projectById, false);
          ListViewContainer.ReloadProjectData();
        }
      }
      ConcurrentDictionary<string, List<ShareUserModel>> projectUserCache = AvatarHelper.GetProjectUserCache(projectId, checkAllUsers);
      List<ShareUserModel> shareUserModelList = AvatarHelper.Sort(remoteShareUsers);
      if (!projectUserCache.ContainsKey(projectId))
        projectUserCache.TryAdd(projectId, shareUserModelList);
      else
        projectUserCache[projectId] = shareUserModelList;
    }

    public static List<AvatarViewModel> GetCacheProjectAvatars(string projectId, bool showMyName = true)
    {
      ConcurrentDictionary<string, List<ShareUserModel>> projectUserCache = AvatarHelper.GetProjectUserCache(projectId);
      if (projectId != null && projectUserCache.ContainsKey(projectId))
      {
        List<ShareUserModel> source = projectUserCache[projectId];
        if (source != null && source.Any<ShareUserModel>())
          return source.Where<ShareUserModel>((Func<ShareUserModel, bool>) (user => user.isAccept)).Select<ShareUserModel, AvatarViewModel>((Func<ShareUserModel, AvatarViewModel>) (user => new AvatarViewModel()
          {
            AvatarUrl = user.avatarUrl,
            Name = AvatarHelper.GetDisplayName(user, showMyName),
            UserCode = user.userCode,
            UserId = user.userId.ToString(),
            Date = user.createdTime ?? DateTime.Now
          })).GroupBy<AvatarViewModel, string>((Func<AvatarViewModel, string>) (user => user.UserId)).Select<IGrouping<string, AvatarViewModel>, AvatarViewModel>((Func<IGrouping<string, AvatarViewModel>, AvatarViewModel>) (user => user.FirstOrDefault<AvatarViewModel>())).ToList<AvatarViewModel>();
      }
      return new List<AvatarViewModel>();
    }

    public static ShareUserModel GetUserByIdAndProjectId(string userId, string projectId)
    {
      ConcurrentDictionary<string, List<ShareUserModel>> projectUserCache = AvatarHelper.GetProjectUserCache(projectId);
      if (projectId != null && projectUserCache.ContainsKey(projectId))
      {
        List<ShareUserModel> source = projectUserCache[projectId];
        if (source != null && source.Any<ShareUserModel>())
          return source.FirstOrDefault<ShareUserModel>((Func<ShareUserModel, bool>) (u => u.userId.ToString() == userId));
      }
      return (ShareUserModel) null;
    }

    public static AvatarViewModel GetAvatarByIdAndProjectId(string userId, string projectId)
    {
      ShareUserModel byIdAndProjectId = AvatarHelper.GetUserByIdAndProjectId(userId, projectId);
      if (byIdAndProjectId == null)
        return (AvatarViewModel) null;
      return new AvatarViewModel()
      {
        AvatarUrl = byIdAndProjectId.avatarUrl,
        Name = AvatarHelper.GetDisplayName(byIdAndProjectId, true),
        UserCode = byIdAndProjectId.userCode,
        UserId = byIdAndProjectId.userId.ToString(),
        Date = byIdAndProjectId.createdTime ?? DateTime.Now
      };
    }

    public static async Task<List<AvatarViewModel>> GetProjectAvatars(
      string projectId,
      bool showMyName = true,
      bool fetchRemote = false)
    {
      List<ShareUserModel> projectUsersAsync = await AvatarHelper.GetProjectUsersAsync(projectId, fetchRemote: fetchRemote);
      return projectUsersAsync == null || !projectUsersAsync.Any<ShareUserModel>() ? new List<AvatarViewModel>() : projectUsersAsync.Distinct<ShareUserModel>().Where<ShareUserModel>((Func<ShareUserModel, bool>) (user => user.isAccept)).Select<ShareUserModel, AvatarViewModel>((Func<ShareUserModel, AvatarViewModel>) (user => new AvatarViewModel()
      {
        AvatarUrl = user.avatarUrl,
        Name = AvatarHelper.GetDisplayName(user, showMyName),
        UserCode = user.userCode,
        UserId = user.userId.ToString()
      })).GroupBy<AvatarViewModel, string>((Func<AvatarViewModel, string>) (user => user.UserId)).Select<IGrouping<string, AvatarViewModel>, AvatarViewModel>((Func<IGrouping<string, AvatarViewModel>, AvatarViewModel>) (user => user.FirstOrDefault<AvatarViewModel>())).ToList<AvatarViewModel>();
    }

    public static List<AvatarViewModel> GetProjectAvatarsFromCache(string projectId, bool sort = false)
    {
      ConcurrentDictionary<string, List<ShareUserModel>> projectUserCache = AvatarHelper.GetProjectUserCache(projectId);
      List<ShareUserModel> source = string.IsNullOrEmpty(projectId) || !projectUserCache.ContainsKey(projectId) ? (List<ShareUserModel>) null : projectUserCache[projectId];
      if (source == null || !source.Any<ShareUserModel>())
        return new List<AvatarViewModel>();
      if (sort)
        source.Sort((Comparison<ShareUserModel>) ((a, b) =>
        {
          DateTime? createdTime1 = a.createdTime;
          DateTime? createdTime2 = b.createdTime;
          if ((createdTime1.HasValue == createdTime2.HasValue ? (createdTime1.HasValue ? (createdTime1.GetValueOrDefault() != createdTime2.GetValueOrDefault() ? 1 : 0) : 0) : 1) != 0)
          {
            createdTime2 = a.createdTime;
            if (createdTime2.HasValue)
            {
              createdTime2 = b.createdTime;
              if (createdTime2.HasValue)
              {
                createdTime2 = a.createdTime;
                DateTime dateTime1 = createdTime2.Value;
                ref DateTime local = ref dateTime1;
                createdTime2 = b.createdTime;
                DateTime dateTime2 = createdTime2.Value;
                return local.CompareTo(dateTime2);
              }
            }
          }
          return a.userId.GetValueOrDefault().CompareTo(b.userId.GetValueOrDefault());
        }));
      return source.Distinct<ShareUserModel>().Where<ShareUserModel>((Func<ShareUserModel, bool>) (user => user.isAccept)).Select<ShareUserModel, AvatarViewModel>((Func<ShareUserModel, AvatarViewModel>) (user => new AvatarViewModel()
      {
        AvatarUrl = user.avatarUrl,
        Name = AvatarHelper.GetDisplayName(user, false),
        UserCode = user.userCode,
        UserId = user.userId.ToString()
      })).GroupBy<AvatarViewModel, string>((Func<AvatarViewModel, string>) (user => user.UserId)).Select<IGrouping<string, AvatarViewModel>, AvatarViewModel>((Func<IGrouping<string, AvatarViewModel>, AvatarViewModel>) (user => user.FirstOrDefault<AvatarViewModel>())).ToList<AvatarViewModel>();
    }

    public static List<ShareUserModel> GetProjectUsers(string projectId, bool ownerTop = false)
    {
      if (string.IsNullOrEmpty(projectId))
        return new List<ShareUserModel>();
      ConcurrentDictionary<string, List<ShareUserModel>> concurrentDictionary = CacheManager.GetProjectById(projectId).OpenToTeam ? AvatarHelper.UserListCache : AvatarHelper.ShareListCache;
      List<ShareUserModel> users = (List<ShareUserModel>) null;
      if (concurrentDictionary.ContainsKey(projectId))
        users = concurrentDictionary[projectId];
      return AvatarHelper.Sort(users, ownerTop);
    }

    public static async Task<List<ShareUserModel>> GetProjectUsersAsync(
      string projectId,
      bool ownerTop = false,
      bool fetchRemote = false,
      bool checkUserList = true)
    {
      if (string.IsNullOrEmpty(projectId))
        return new List<ShareUserModel>();
      ProjectModel project = CacheManager.GetProjectById(projectId);
      if (project == null)
        return new List<ShareUserModel>();
      ConcurrentDictionary<string, List<ShareUserModel>> cache = !checkUserList || !project.OpenToTeam ? AvatarHelper.ShareListCache : AvatarHelper.UserListCache;
      List<ShareUserModel> shareUserModelList1;
      if (!fetchRemote && cache.ContainsKey(projectId))
      {
        shareUserModelList1 = cache[projectId];
      }
      else
      {
        if (fetchRemote)
        {
          List<ShareUserModel> shareUserModelList2;
          if (project.IsShareList())
            shareUserModelList2 = await AvatarHelper.GetRemoteShareUsers(projectId, checkAllUsers: checkUserList);
          else
            shareUserModelList2 = new List<ShareUserModel>();
          shareUserModelList1 = shareUserModelList2;
        }
        else
        {
          shareUserModelList1 = await AvatarHelper.GetLocalShareUsers(projectId);
          if (project.IsShareList() && (shareUserModelList1 == null || shareUserModelList1.Count == 0 && !project.OpenToTeam || shareUserModelList1.Count == 1 & checkUserList && project.OpenToTeam))
            shareUserModelList1 = await AvatarHelper.GetRemoteShareUsers(projectId, checkAllUsers: checkUserList);
        }
        if (shareUserModelList1 != null && shareUserModelList1.Any<ShareUserModel>())
        {
          if (!cache.ContainsKey(projectId))
            cache.TryAdd(projectId, shareUserModelList1);
          else
            cache[projectId] = shareUserModelList1;
        }
      }
      return AvatarHelper.Sort(shareUserModelList1, ownerTop);
    }

    private static async Task<List<ShareUserModel>> GetLocalShareUsers(string projectId)
    {
      TempDataModel tempDataModel = await TempDataDao.QueryTempDataModelListDbByTypeAndEntityIdAsync("SHAREUSERLIST", projectId);
      if (tempDataModel == null)
        return (List<ShareUserModel>) null;
      try
      {
        List<ShareUserModel> source = JsonConvert.DeserializeObject<List<ShareUserModel>>(tempDataModel?.Data);
        if (source != null && source.Any<ShareUserModel>())
        {
          foreach (ShareUserModel shareUserModel in source.Where<ShareUserModel>((Func<ShareUserModel, bool>) (user => Utils.IsFakeEmail(user.username))))
            shareUserModel.username = string.Empty;
        }
        return source;
      }
      catch (Exception ex)
      {
        return new List<ShareUserModel>();
      }
    }

    private static List<ShareUserModel> Sort(List<ShareUserModel> users, bool ownerTop = false)
    {
      string userId = Utils.GetCurrentUserIdInt().ToString();
      users?.Sort((Comparison<ShareUserModel>) ((a, b) =>
      {
        if (ownerTop)
        {
          if (a.isOwner && !b.isOwner)
            return -1;
          if (!a.isOwner && b.isOwner)
            return 1;
        }
        if (a.userId.ToString() == userId && b.userId.ToString() != userId)
          return -1;
        if (a.userId.ToString() != userId && b.userId.ToString() == userId)
          return 1;
        if (a.isAccept && !b.isAccept)
          return -1;
        return !a.isAccept && b.isAccept ? 1 : (b.createdTime ?? DateTime.Now).CompareTo(a.createdTime ?? DateTime.Now);
      }));
      return users;
    }

    private static string GetDisplayName(ShareUserModel item, bool showMyName)
    {
      if (showMyName)
      {
        long? userId = item.userId;
        long currentUserIdInt = (long) Utils.GetCurrentUserIdInt();
        if (userId.GetValueOrDefault() == currentUserIdInt & userId.HasValue)
          return Utils.GetString("Me");
      }
      return !string.IsNullOrEmpty(item.displayName) ? item.displayName : item.username;
    }

    public static void ResetPullTime(string projectId)
    {
      if (string.IsNullOrEmpty(projectId))
        return;
      lock (AvatarHelper.LastPullTimeDict)
      {
        if (!AvatarHelper.LastPullTimeDict.ContainsKey(projectId))
          return;
        AvatarHelper.LastPullTimeDict[projectId] = DateTime.Now.AddHours(-1.0);
      }
    }

    private static async Task<List<ShareUserModel>> GetRemoteShareUsers(
      string projectId,
      bool force = false,
      bool checkAllUsers = true)
    {
      if (string.IsNullOrEmpty(projectId))
        return (List<ShareUserModel>) null;
      lock (AvatarHelper.LastPullTimeDict)
      {
        if (!force && AvatarHelper.LastPullTimeDict.ContainsKey(projectId) && (DateTime.Now - AvatarHelper.LastPullTimeDict[projectId]).TotalSeconds < 60.0)
        {
          List<ShareUserModel> shareUserModelList;
          return AvatarHelper.GetProjectUserCache(projectId).TryGetValue(projectId, out shareUserModelList) ? shareUserModelList : new List<ShareUserModel>();
        }
        AvatarHelper.LastPullTimeDict[projectId] = DateTime.Now;
      }
      ProjectModel project = CacheManager.GetProjectById(projectId);
      if (project == null)
        return new List<ShareUserModel>();
      if (project.IsNew())
        await Task.Delay(1500);
      bool openToTeam = checkAllUsers && project.OpenToTeam;
      string dateType = openToTeam ? "USERLIST" : "SHAREUSERLIST";
      if (Utils.IsNetworkAvailable())
      {
        string data = await Communicator.GetShareUserList(projectId, openToTeam);
        int num = await TempDataDao.UpdateOrInsertTempDataModelListDbByTypeAndEntityIdAsync(new TempDataModel()
        {
          User_Id = Utils.GetCurrentUserIdInt().ToString(),
          DataType = dateType,
          ModifyTime = Utils.GetNowTimeStamp(),
          Data = data,
          EntityId = projectId
        });
        try
        {
          return AvatarHelper.HandleUsersFakeEmail(JsonConvert.DeserializeObject<List<ShareUserModel>>(data));
        }
        catch (Exception ex)
        {
          return new List<ShareUserModel>();
        }
      }
      else
      {
        TempDataModel tempDataModel = await TempDataDao.QueryTempDataModelListDbByTypeAndEntityIdAsync(dateType, projectId);
        try
        {
          return AvatarHelper.HandleUsersFakeEmail(JsonConvert.DeserializeObject<List<ShareUserModel>>(tempDataModel.Data));
        }
        catch (Exception ex)
        {
          return new List<ShareUserModel>();
        }
      }
    }

    private static List<ShareUserModel> HandleUsersFakeEmail(List<ShareUserModel> users)
    {
      if (users == null || !users.Any<ShareUserModel>())
        return new List<ShareUserModel>();
      foreach (ShareUserModel shareUserModel in users.Where<ShareUserModel>((Func<ShareUserModel, bool>) (user => Utils.IsFakeEmail(user.username))))
        shareUserModel.username = string.Empty;
      return users;
    }

    private static async Task<ShareUserModel> GetUserModel(string assignId, string projectId)
    {
      List<ShareUserModel> projectUsersAsync = await AvatarHelper.GetProjectUsersAsync(projectId);
      if (projectUsersAsync != null && projectUsersAsync.Any<ShareUserModel>())
      {
        ShareUserModel userModel = projectUsersAsync.FirstOrDefault<ShareUserModel>((Func<ShareUserModel, bool>) (avatar => avatar.userId.ToString() == assignId));
        if (userModel != null)
          return userModel;
      }
      return (ShareUserModel) null;
    }

    public static string GetCacheUrl(string assignId, string projectId)
    {
      List<AvatarViewModel> cacheProjectAvatars = AvatarHelper.GetCacheProjectAvatars(projectId);
      if (cacheProjectAvatars.Any<AvatarViewModel>())
      {
        AvatarViewModel avatarViewModel = cacheProjectAvatars.FirstOrDefault<AvatarViewModel>((Func<AvatarViewModel, bool>) (p => p.UserId == assignId));
        if (avatarViewModel != null)
          return avatarViewModel.AvatarUrl;
      }
      return string.Empty;
    }

    public static async Task<string> GetAvatarUrl(string assignId, string projectId)
    {
      ShareUserModel userModel = await AvatarHelper.GetUserModel(assignId, projectId);
      return userModel != null ? userModel.avatarUrl : string.Empty;
    }

    public static async Task<string> GetUserName(string assignId, string projectId)
    {
      ShareUserModel userModel = await AvatarHelper.GetUserModel(assignId, projectId);
      return userModel != null ? AvatarHelper.GetUserName(userModel) : string.Empty;
    }

    private static string GetUserName(ShareUserModel model)
    {
      if (!string.IsNullOrEmpty(model.displayName))
        return model.displayName;
      return !string.IsNullOrEmpty(model.username) ? model.username : string.Empty;
    }

    public static string GetCacheUserName(string assignId, string projectId)
    {
      if (assignId == Utils.GetCurrentUserIdInt().ToString())
        return Utils.GetString("Me");
      ConcurrentDictionary<string, List<ShareUserModel>> projectUserCache = AvatarHelper.GetProjectUserCache(projectId);
      if (projectUserCache.ContainsKey(projectId))
      {
        ShareUserModel model = projectUserCache[projectId].FirstOrDefault<ShareUserModel>((Func<ShareUserModel, bool>) (avatar => avatar.userId.ToString() == assignId));
        if (model != null)
          return AvatarHelper.GetUserName(model);
      }
      return string.Empty;
    }

    public static BitmapImage GetAvatarByUrl(string avatarUrl, int width = 64, bool getDefault = true)
    {
      if (string.IsNullOrEmpty(avatarUrl) || avatarUrl == "-1")
        return (BitmapImage) null;
      if (avatarUrl.Contains("/Assets/logo.png"))
        return new BitmapImage(new Uri("pack://application:,,,/Assets/logo.png"));
      if (!avatarUrl.Contains("avatar-new"))
      {
        string key = avatarUrl + width.ToString();
        if (AvatarHelper.Avatars.ContainsKey(key))
          return AvatarHelper.Avatars[key];
        if (!getDefault)
          return (BitmapImage) null;
      }
      return AvatarHelper.GetDefaultAvatar();
    }

    public static async Task<BitmapImage> GetAvatarByUrlAsync(string avatarUrl, int width = 64)
    {
      if (string.IsNullOrEmpty(avatarUrl) || avatarUrl == "-1")
        return (BitmapImage) null;
      string key = avatarUrl + width.ToString();
      if (AvatarHelper.Avatars.ContainsKey(key) && AvatarHelper.Avatars[key] != null)
        return AvatarHelper.Avatars[key];
      if (avatarUrl.Contains("/Assets/logo.png"))
        return new BitmapImage(new Uri("pack://application:,,,/Assets/logo.png"));
      string str = ((IEnumerable<string>) avatarUrl.Split('?')).FirstOrDefault<string>();
      if ((str != null ? (str.Contains("avatar-new") ? 1 : 0) : 1) == 0)
      {
        int retry = 0;
        while (AvatarHelper.Avatars.ContainsKey(key))
        {
          BitmapImage avatar = AvatarHelper.Avatars[key];
          if (retry > 20)
            return AvatarHelper.GetDefaultAvatar();
          if (avatar != null)
            return avatar;
          await Task.Delay(100);
          ++retry;
        }
        AvatarHelper.Avatars[key] = (BitmapImage) null;
        try
        {
          if (!Directory.Exists(AppPaths.AvatarDir))
            Directory.CreateDirectory(AppPaths.AvatarDir);
          string path = AppPaths.AvatarDir + avatarUrl.GetHashCode().ToString() + ".png";
          if (!File.Exists(path) || new FileInfo(path).Length == 0L)
          {
            if (!await Utils.TryDownloadFile(avatarUrl, path))
            {
              AvatarHelper.Avatars.TryRemove(key, out BitmapImage _);
              return AvatarHelper.GetDefaultAvatar();
            }
          }
          if (File.Exists(path) && new FileInfo(path).Length > 0L)
          {
            BitmapImage imageByUrl = ImageUtils.GetImageByUrl(path, width);
            AvatarHelper.Avatars[key] = imageByUrl;
            return imageByUrl;
          }
          if (AvatarHelper.Avatars.ContainsKey(key))
            AvatarHelper.Avatars.TryRemove(key, out BitmapImage _);
          path = (string) null;
        }
        catch (Exception ex)
        {
          UtilLog.Info("GetAvatarByUrlAsync Error : " + avatarUrl + " \r\n " + ex.Message);
        }
      }
      return AvatarHelper.GetDefaultAvatar();
    }

    public static BitmapImage GetDefaultAvatar()
    {
      return LocalSettings.Settings.ThemeId == "Dark" ? new BitmapImage(new Uri("pack://application:,,,/Assets/avatar-new-dark.png")) : new BitmapImage(new Uri("pack://application:,,,/Assets/avatar-new.png"));
    }

    public static void Clear()
    {
      AvatarHelper.ShareListCache.Clear();
      AvatarHelper.UserListCache.Clear();
      AvatarHelper.UserCache.Clear();
    }

    public static void RemoveProjectUser(string projectId, long userId)
    {
      if (string.IsNullOrEmpty(projectId))
        return;
      List<ShareUserModel> shareUserModelList1;
      if (AvatarHelper.ShareListCache.TryGetValue(projectId, out shareUserModelList1) && shareUserModelList1 != null)
        shareUserModelList1.RemoveAll((Predicate<ShareUserModel>) (u =>
        {
          long? userId1 = u.userId;
          long num = userId;
          return userId1.GetValueOrDefault() == num & userId1.HasValue;
        }));
      List<ShareUserModel> shareUserModelList2;
      if (!AvatarHelper.UserListCache.TryGetValue(projectId, out shareUserModelList2) || shareUserModelList2 == null)
        return;
      shareUserModelList2.RemoveAll((Predicate<ShareUserModel>) (u =>
      {
        long? userId2 = u.userId;
        long num = userId;
        return userId2.GetValueOrDefault() == num & userId2.HasValue;
      }));
    }

    public static List<AvatarViewModel> GetProjectAvatarsFromCacheInGroup(string groupId)
    {
      List<AvatarViewModel> source = new List<AvatarViewModel>();
      List<ProjectModel> projectsInGroup = CacheManager.GetProjectsInGroup(groupId);
      projectsInGroup.Sort((Comparison<ProjectModel>) ((a, b) => a.sortOrder.CompareTo(b.sortOrder)));
      foreach (ProjectModel projectModel in projectsInGroup)
      {
        foreach (AvatarViewModel avatarViewModel in AvatarHelper.GetProjectAvatarsFromCache(projectModel.id, true))
        {
          AvatarViewModel avatar = avatarViewModel;
          if (!source.Any<AvatarViewModel>((Func<AvatarViewModel, bool>) (u => u.UserId == avatar.UserId)))
            source.Add(avatar);
        }
      }
      return source;
    }
  }
}
