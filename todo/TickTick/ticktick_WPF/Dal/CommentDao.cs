// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Dal.CommentDao
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ticktick_WPF.Cache;
using ticktick_WPF.Models;
using TickTickDao;

#nullable disable
namespace ticktick_WPF.Dal
{
  public class CommentDao : BaseDao<CommentModel>
  {
    public static async Task<CommentModel> SaveComment(CommentModel comment)
    {
      int num = await App.Connection.InsertAsync((object) comment);
      TaskCommentCache.OnCommentChanged(new List<CommentModel>()
      {
        comment
      }, true);
      return comment;
    }

    public static async Task SaveComments(List<CommentModel> comments)
    {
      if (comments == null)
        return;
      int num = await App.Connection.InsertAllAsync((IEnumerable) comments);
      TaskCommentCache.OnCommentChanged(comments, true);
    }

    public static async Task UpdateComments(List<CommentModel> comments)
    {
      if (comments == null)
        return;
      int num = await App.Connection.UpdateAllAsync((IEnumerable) comments);
      TaskCommentCache.OnCommentChanged(comments);
    }

    public static async Task UpdateComment(CommentModel comment)
    {
      int num = await App.Connection.UpdateAsync((object) comment);
      TaskCommentCache.OnCommentChanged(new List<CommentModel>()
      {
        comment
      });
    }

    public static async Task<List<CommentModel>> GetCommentsByTaskId(string taskSid)
    {
      return await App.Connection.Table<CommentModel>().Where((Expression<Func<CommentModel, bool>>) (v => v.taskSid == taskSid && v.deleted == 0)).ToListAsync();
    }

    public static async Task<List<CommentModel>> GetCommentsByTaskSidWithDeleted(string taskSid)
    {
      return await App.Connection.Table<CommentModel>().Where((Expression<Func<CommentModel, bool>>) (v => v.taskSid == taskSid)).ToListAsync();
    }

    public static async Task DeleteComment(CommentModel model)
    {
      int num = await App.Connection.DeleteAsync((object) model);
      TaskCommentCache.OnCommentDeleted(model);
    }

    public static async void ChangeCommentProjectId(TaskModel task, string projectId)
    {
      List<CommentModel> taskSidWithDeleted = await CommentDao.GetCommentsByTaskSidWithDeleted(task.id);
      if (taskSidWithDeleted == null)
        ;
      else
      {
        taskSidWithDeleted.ForEach((Action<CommentModel>) (c => c.projectSid = projectId));
        int num = await App.Connection.UpdateAllAsync((IEnumerable) taskSidWithDeleted);
      }
    }

    public static async Task<List<CommentModel>> GetAllComments()
    {
      return await App.Connection.Table<CommentModel>().Where((Expression<Func<CommentModel, bool>>) (v => v.deleted == 0)).ToListAsync();
    }

    public static async Task<List<CommentModel>> GetCommentsById(string modelId)
    {
      return await App.Connection.Table<CommentModel>().Where((Expression<Func<CommentModel, bool>>) (v => v.id == modelId && v.deleted == 0)).ToListAsync();
    }
  }
}
