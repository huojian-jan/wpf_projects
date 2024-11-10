// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.UserPublicProfilesModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using TickTickModels;

#nullable disable
namespace ticktick_WPF.Models
{
  public class UserPublicProfilesModel : BaseModel
  {
    public UserPublicProfilesModel()
    {
    }

    public UserPublicProfilesModel(ShareUserModel shareUserModel)
    {
      this.userId = shareUserModel.userId;
      this.userCode = shareUserModel.userCode;
      this.displayName = shareUserModel.displayName;
      this.avatarUrl = shareUserModel.avatarUrl;
      this.email = shareUserModel.username;
    }

    public long? userId { get; set; }

    public string userCode { get; set; }

    public string displayName { get; set; }

    public string nickName { get; set; }

    public string avatarUrl { get; set; }

    public string email { get; set; }

    public int? siteId { get; set; }
  }
}
