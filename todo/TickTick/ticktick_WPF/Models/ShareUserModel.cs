// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ShareUserModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using Newtonsoft.Json;
using System;
using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class ShareUserModel : BaseModel
  {
    public string recordId { get; set; }

    public long? userId { get; set; }

    public string avatarUrl { get; set; }

    public string username { get; set; }

    public string displayName { get; set; }

    public bool isOwner { get; set; }

    public bool isProjectShare { get; set; }

    public bool isAccept { get; set; }

    [JsonConverter(typeof (UtcDateTimeConverter))]
    public DateTime? createdTime { get; set; }

    public string userCode { get; set; }

    public int acceptStatus { get; set; }

    public bool deleted { get; set; }

    public string permission { get; set; }

    public int? siteId { get; set; }

    public bool visitor { get; set; }
  }
}
