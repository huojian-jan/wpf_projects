// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.CommentModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class CommentModel : BaseModel
  {
    public string id { get; set; }

    public string sId { get; set; }

    [JsonProperty("taskId")]
    public string taskSid { get; set; }

    public string userId { get; set; }

    public string projectSid { get; set; }

    public string title { get; set; }

    public DateTime? createdTime { get; set; }

    public DateTime? modifiedTime { get; set; }

    public int deleted { get; set; }

    public bool candelete { get; set; } = true;

    [NotNull]
    [DefaultValue("2")]
    public int syncStatus { get; set; }

    public string ownerSid { get; set; }

    public bool isMySelf { get; set; }

    public string avatarUrl { get; set; }

    public string replyCommentId { get; set; }

    public string replyUserName { get; set; }

    public string atLabel { get; set; }

    public string userCode { get; set; }

    public string userName { get; set; }

    [Ignore]
    public UserProfile userProfile { get; set; }

    [Ignore]
    public List<Mention> mentions { get; set; }

    public string mentionstring { get; set; }
  }
}
