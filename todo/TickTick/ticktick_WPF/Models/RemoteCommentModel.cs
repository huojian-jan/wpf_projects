// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.RemoteCommentModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

#nullable disable
namespace ticktick_WPF.Models
{
  public class RemoteCommentModel
  {
    public string title { get; set; }

    public string taskId { get; set; }

    public string projectId { get; set; }

    public string id { get; set; }

    public string modifiedTime { get; set; }

    public string createdTime { get; set; }

    public string replyCommentId { get; set; }

    public List<Mention> mentions { get; set; }

    public UserProfile userProfile { get; set; }

    public ReplyUserProfile replyUserProfile { get; set; }

    public bool local { get; set; }

    public RemoteCommentModel(CommentModel comment)
    {
      this.id = comment.id;
      this.local = true;
      this.mentions = JsonConvert.DeserializeObject<List<Mention>>(comment.mentionstring ?? "");
      this.replyUserProfile = new ReplyUserProfile();
      this.taskId = comment.taskSid;
      this.projectId = comment.projectSid;
      this.title = comment.title;
      DateTime dateTime = comment.createdTime ?? DateTime.Now;
      dateTime = dateTime.ToUniversalTime();
      this.createdTime = dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.000") + "+0000";
      this.modifiedTime = "";
      this.replyCommentId = comment.replyCommentId;
      this.userProfile = new UserProfile()
      {
        isMyself = new bool?(true)
      };
    }
  }
}
