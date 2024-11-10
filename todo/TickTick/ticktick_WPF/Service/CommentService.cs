// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Service.CommentService
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;
using ticktick_WPF.Util;
using TickTickDao;
using TickTickUtils.Lang;

#nullable disable
namespace ticktick_WPF.Service
{
  public class CommentService
  {
    private static readonly HashSet<string> PullingTaskIds = new HashSet<string>();

    public static async Task TryPullRemoteComments(string projectSid, string taskSid)
    {
      lock (CommentService.PullingTaskIds)
      {
        if (string.IsNullOrEmpty(taskSid) || CommentService.PullingTaskIds.Contains(taskSid))
          return;
        CommentService.PullingTaskIds.Add(taskSid);
      }
      await CommentService.MergeRemoteComments(projectSid, taskSid);
      lock (CommentService.PullingTaskIds)
        CommentService.PullingTaskIds.Remove(taskSid);
    }

    private static async Task MergeRemoteComments(
      string projectSid,
      string taskSid,
      bool needCheck = true)
    {
      List<CommentModel> localComments;
      List<CommentModel> remoteComments;
      Dictionary<string, CommentModel> localDict;
      UserInfoModel userInfo;
      List<CommentModel> remoteDeletedModelList;
      List<RemoteCommentModel> needPostModelList;
      if (await SyncStatusDao.ExistTaskMoveStatus(taskSid))
      {
        localComments = (List<CommentModel>) null;
        remoteComments = (List<CommentModel>) null;
        localDict = (Dictionary<string, CommentModel>) null;
        userInfo = (UserInfoModel) null;
        remoteDeletedModelList = (List<CommentModel>) null;
        needPostModelList = (List<RemoteCommentModel>) null;
      }
      else
      {
        bool remoteAdded = false;
        bool remoteChanged = false;
        bool remoteDeleted = false;
        localComments = await CommentDao.GetCommentsByTaskSidWithDeleted(taskSid);
        remoteComments = await Communicator.GetComments(projectSid, taskSid);
        UtilLog.Info(string.Format("MergeRemoteComments: P{0} T{1}, local :{2},remote :{3}", (object) projectSid, (object) taskSid, (object) localComments?.Count, (object) remoteComments?.Count));
        if (remoteComments == null)
        {
          localComments = (List<CommentModel>) null;
          remoteComments = (List<CommentModel>) null;
          localDict = (Dictionary<string, CommentModel>) null;
          userInfo = (UserInfoModel) null;
          remoteDeletedModelList = (List<CommentModel>) null;
          needPostModelList = (List<RemoteCommentModel>) null;
        }
        else
        {
          localDict = new Dictionary<string, CommentModel>();
          Dictionary<string, CommentModel> remoteDict = new Dictionary<string, CommentModel>();
          userInfo = await UserManager.GetUserInfo(true);
          remoteDeletedModelList = new List<CommentModel>();
          needPostModelList = new List<RemoteCommentModel>();
          remoteComments?.ForEach((Action<CommentModel>) (remote =>
          {
            if (remoteDict.ContainsKey(remote.id))
              return;
            remoteDict.Add(remote.id, remote);
          }));
          if (localComments != null)
          {
            foreach (CommentModel comment in localComments)
            {
              if (!localDict.ContainsKey(comment.id))
                localDict.Add(comment.id, comment);
              if (comment.syncStatus != 0 && !remoteDict.ContainsKey(comment.id))
                remoteDeletedModelList.Add(comment);
              if (!remoteDict.ContainsKey(comment.id) && comment.syncStatus == 0 && comment.deleted != 1)
                needPostModelList.Add(new RemoteCommentModel(comment));
            }
          }
          bool? nullable1;
          if (remoteComments.Count > 0)
          {
            foreach (CommentModel comment1 in remoteComments)
            {
              if (!localDict.ContainsKey(comment1.id))
              {
                remoteAdded = true;
                comment1.taskSid = taskSid;
                comment1.projectSid = projectSid;
                comment1.deleted = 0;
                comment1.syncStatus = 2;
                if (comment1.userProfile != null)
                {
                  UserProfile userProfile = comment1.userProfile;
                  CommentModel commentModel = comment1;
                  nullable1 = userProfile.isMyself;
                  int num = nullable1.GetValueOrDefault() ? 1 : 0;
                  commentModel.isMySelf = num != 0;
                  if (comment1.isMySelf)
                  {
                    comment1.userName = string.IsNullOrEmpty(userInfo?.name) ? userInfo?.username : userInfo.name;
                    comment1.avatarUrl = userInfo?.picture;
                  }
                  else
                  {
                    comment1.avatarUrl = userProfile.avatarUrl;
                    comment1.userName = string.IsNullOrEmpty(userProfile.name) ? userProfile.username : userProfile.name;
                  }
                }
                CommentModel commentModel1 = await CommentDao.SaveComment(comment1);
              }
              else
              {
                CommentModel comment2 = localDict[comment1.id];
                int num1 = comment2.isMySelf ? 1 : 0;
                UserProfile userProfile1 = comment1.userProfile;
                bool? nullable2;
                if (userProfile1 == null)
                {
                  nullable1 = new bool?();
                  nullable2 = nullable1;
                }
                else
                  nullable2 = userProfile1.isMyself;
                nullable1 = nullable2;
                int num2 = nullable1.GetValueOrDefault() ? 1 : 0;
                int num3;
                if (num1 == num2)
                {
                  DateTime? modifiedTime1 = comment2.modifiedTime;
                  DateTime? modifiedTime2 = comment1.modifiedTime;
                  num3 = modifiedTime1.HasValue == modifiedTime2.HasValue ? (modifiedTime1.HasValue ? (modifiedTime1.GetValueOrDefault() != modifiedTime2.GetValueOrDefault() ? 1 : 0) : 0) : 1;
                }
                else
                  num3 = 1;
                bool flag = num3 != 0;
                remoteChanged |= flag;
                if (comment2.syncStatus == 0 | flag)
                {
                  comment2.syncStatus = 2;
                  comment2.title = comment1.title;
                  CommentModel commentModel = comment2;
                  UserProfile userProfile2 = comment1.userProfile;
                  bool? nullable3;
                  if (userProfile2 == null)
                  {
                    nullable1 = new bool?();
                    nullable3 = nullable1;
                  }
                  else
                    nullable3 = userProfile2.isMyself;
                  nullable1 = nullable3;
                  int num4 = nullable1.GetValueOrDefault() ? 1 : 0;
                  commentModel.isMySelf = num4 != 0;
                  await CommentDao.UpdateComment(comment2);
                }
              }
            }
          }
          if (remoteDeletedModelList.Count > 0)
          {
            remoteDeleted = true;
            foreach (CommentModel commentModel in remoteDeletedModelList)
            {
              await CommentDao.DeleteComment(commentModel);
              Communicator.DeleteComment(commentModel.projectSid, commentModel.taskSid, commentModel.id);
            }
          }
          if (remoteAdded | remoteDeleted | remoteChanged)
            await TaskService.SaveCommentCount(taskSid, false);
          if (!(needPostModelList.Any<RemoteCommentModel>() & needCheck))
          {
            localComments = (List<CommentModel>) null;
            remoteComments = (List<CommentModel>) null;
            localDict = (Dictionary<string, CommentModel>) null;
            userInfo = (UserInfoModel) null;
            remoteDeletedModelList = (List<CommentModel>) null;
            needPostModelList = (List<RemoteCommentModel>) null;
          }
          else
          {
            foreach (RemoteCommentModel comment in needPostModelList)
            {
              string str = await Communicator.AddComment(comment);
            }
            await CommentService.MergeRemoteComments(projectSid, taskSid, false);
            localComments = (List<CommentModel>) null;
            remoteComments = (List<CommentModel>) null;
            localDict = (Dictionary<string, CommentModel>) null;
            userInfo = (UserInfoModel) null;
            remoteDeletedModelList = (List<CommentModel>) null;
            needPostModelList = (List<RemoteCommentModel>) null;
          }
        }
      }
    }

    public static async Task<bool> MergeComments(IEnumerable<CommentModel> comments)
    {
      if (comments == null)
        return false;
      List<CommentModel> newCommentList = new List<CommentModel>();
      List<CommentModel> updateCommentList = new List<CommentModel>();
      List<CommentModel> localComments = await CommentDao.GetAllComments();
      Dictionary<string, CommentModel> localDict = new Dictionary<string, CommentModel>();
      UserInfoModel userInfo = await UserManager.GetUserInfo(true);
      foreach (CommentModel commentModel in localComments)
      {
        if (!localDict.ContainsKey(commentModel.id))
          localDict.Add(commentModel.id, commentModel);
      }
      bool? nullable1;
      foreach (CommentModel remoteComment in comments)
      {
        if (!localDict.ContainsKey(remoteComment.id))
        {
          TaskModel taskById = await TaskDao.GetTaskById(remoteComment.taskSid);
          if (taskById != null)
          {
            remoteComment.projectSid = taskById.projectId;
            remoteComment.deleted = 0;
            remoteComment.syncStatus = 2;
            if (remoteComment.userProfile != null)
            {
              UserProfile userProfile = remoteComment.userProfile;
              CommentModel commentModel = remoteComment;
              nullable1 = userProfile.isMyself;
              int num = nullable1.GetValueOrDefault() ? 1 : 0;
              commentModel.isMySelf = num != 0;
              if (remoteComment.isMySelf)
              {
                remoteComment.userName = string.IsNullOrEmpty(userInfo?.name) ? userInfo?.username : userInfo.name;
                remoteComment.avatarUrl = userInfo?.picture;
              }
              else
              {
                remoteComment.avatarUrl = userProfile.avatarUrl;
                remoteComment.userName = string.IsNullOrEmpty(userProfile.name) ? userProfile.username : userProfile.name;
              }
            }
            newCommentList.Add(remoteComment);
          }
        }
        else
        {
          CommentModel commentModel1 = localDict[remoteComment.id];
          int num1 = commentModel1.isMySelf ? 1 : 0;
          UserProfile userProfile1 = remoteComment.userProfile;
          bool? nullable2;
          if (userProfile1 == null)
          {
            nullable1 = new bool?();
            nullable2 = nullable1;
          }
          else
            nullable2 = userProfile1.isMyself;
          nullable1 = nullable2;
          int num2 = nullable1.GetValueOrDefault() ? 1 : 0;
          int num3;
          if (num1 == num2)
          {
            DateTime? modifiedTime1 = commentModel1.modifiedTime;
            DateTime? modifiedTime2 = remoteComment.modifiedTime;
            num3 = modifiedTime1.HasValue == modifiedTime2.HasValue ? (modifiedTime1.HasValue ? (modifiedTime1.GetValueOrDefault() != modifiedTime2.GetValueOrDefault() ? 1 : 0) : 0) : 1;
          }
          else
            num3 = 1;
          if (num3 != 0)
          {
            commentModel1.syncStatus = 2;
            commentModel1.title = remoteComment.title;
            CommentModel commentModel2 = commentModel1;
            UserProfile userProfile2 = remoteComment.userProfile;
            bool? nullable3;
            if (userProfile2 == null)
            {
              nullable1 = new bool?();
              nullable3 = nullable1;
            }
            else
              nullable3 = userProfile2.isMyself;
            nullable1 = nullable3;
            int num4 = nullable1.GetValueOrDefault() ? 1 : 0;
            commentModel2.isMySelf = num4 != 0;
            updateCommentList.Add(commentModel1);
          }
        }
      }
      if (newCommentList.Count > 0)
        await CommentDao.SaveComments(newCommentList);
      if (updateCommentList.Count > 0)
        await CommentDao.UpdateComments(updateCommentList);
      return newCommentList.Count > 0 || updateCommentList.Count > 0;
    }

    public static async Task AddComment(CommentModel comment)
    {
      CommentModel commentModel = await CommentDao.SaveComment(comment);
      if (!comment.createdTime.HasValue)
        comment.createdTime = new DateTime?(DateTime.Now);
      RemoteCommentModel comment1 = new RemoteCommentModel(comment);
      if (comment.mentions != null && comment.mentions.Count > 0)
      {
        comment1.mentions = new List<Mention>();
        foreach (Mention mention in comment.mentions)
        {
          if (comment.title.Contains(mention.atLabel))
            comment1.mentions.Add(mention);
        }
      }
      string str = await Communicator.AddComment(comment1);
    }

    public static async Task DeleteComment(CommentModel comment)
    {
      comment.deleted = 1;
      await CommentDao.UpdateComment(comment);
      Communicator.DeleteComment(comment.projectSid, comment.taskSid, comment.id);
    }

    public static async Task UpdateComment(CommentModel model)
    {
      if (model == null)
        return;
      List<CommentModel> locals = await CommentDao.GetCommentsById(model.id);
      for (int i = 0; i < locals.Count; ++i)
      {
        CommentModel model1 = locals[i];
        if (i > 0)
        {
          int num = await BaseDao<CommentModel>.DeleteAsync(model1);
        }
        else
        {
          model._Id = model1._Id;
          model.syncStatus = 1;
          await CommentDao.UpdateComment(model);
          string str = await Communicator.AddComment(new RemoteCommentModel(model), true);
        }
      }
      locals = (List<CommentModel>) null;
    }
  }
}
