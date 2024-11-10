// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.ShareAddUserMode
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

#nullable disable
namespace ticktick_WPF.Models
{
  public class ShareAddUserMode
  {
    public string recordId { get; set; }

    public int? toUserId { get; set; }

    public string toUserCode { get; set; }

    public string toUsername { get; set; }

    public int fromUserId { get; set; }

    public string fromUsername { get; set; }

    public bool accepted { get; set; }

    public long createdTime { get; set; }

    public long? acceptedTime { get; set; }

    public string permission { get; set; }
  }
}
