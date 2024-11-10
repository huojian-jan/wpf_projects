// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.TaskCommentCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Cache
{
  public static class TaskCommentCache
  {
    private static DateTime _lastCacheTime;
    private static ConcurrentDictionary<string, List<CommentModel>> _cache = new ConcurrentDictionary<string, List<CommentModel>>();

    public static async Task<ConcurrentDictionary<string, List<CommentModel>>> GetTaskCommentDict()
    {
      if ((DateTime.Now - TaskCommentCache._lastCacheTime).TotalSeconds < 300.0)
        return TaskCommentCache._cache;
      TaskCommentCache._lastCacheTime = DateTime.Now;
      List<CommentModel> allComments = await CommentDao.GetAllComments();
      if (allComments != null)
      {
        ConcurrentDictionary<string, List<CommentModel>> concurrentDictionary = new ConcurrentDictionary<string, List<CommentModel>>();
        foreach (CommentModel commentModel in allComments)
        {
          string taskSid = commentModel?.taskSid;
          if (!string.IsNullOrEmpty(taskSid))
          {
            if (concurrentDictionary.ContainsKey(taskSid))
              concurrentDictionary[taskSid].Add(commentModel);
            else
              concurrentDictionary[taskSid] = new List<CommentModel>()
              {
                commentModel
              };
          }
        }
        TaskCommentCache._cache = concurrentDictionary;
      }
      return TaskCommentCache._cache;
    }

    public static void OnCommentChanged(List<CommentModel> comments, bool isAdd = false)
    {
      TaskCommentCache._lastCacheTime = DateTime.Now.AddMinutes(-10.0);
    }

    public static void OnCommentDeleted(CommentModel comment)
    {
      TaskCommentCache._lastCacheTime = DateTime.Now.AddMinutes(-10.0);
    }

    public static List<CommentModel> GetTaskComments(string taskId)
    {
      return TaskCommentCache._cache.ContainsKey(taskId) ? TaskCommentCache._cache[taskId] : new List<CommentModel>();
    }
  }
}
