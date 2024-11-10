// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ShareContactsModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Models
{
  public class ShareContactsModel
  {
    public ShareContactsModel()
    {
    }

    public ShareContactsModel(TeamMember model)
    {
      this.email = model.email;
      this.userCode = model.email;
      this.avatarUrl = model.avatarUrl;
      this.displayName = string.IsNullOrEmpty(model.nickname) ? model.displayName : model.nickname;
      this.displayEmail = model.email;
    }

    public string toUserId { get; set; }

    public string email { get; set; }

    public string userCode { get; set; }

    public int freq { get; set; }

    public int? siteId { get; set; }

    public DateTime lstTime { get; set; }

    public string avatarUrl { get; set; }

    public string displayName { get; set; }

    public string displayEmail { get; set; }
  }
}
