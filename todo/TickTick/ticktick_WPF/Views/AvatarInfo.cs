// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Views.AvatarInfo
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Views
{
  public class AvatarInfo
  {
    public readonly string AvatarUrl;
    public readonly string UserId;

    public AvatarInfo(string userId, string url)
    {
      this.UserId = userId;
      this.AvatarUrl = url;
    }
  }
}
