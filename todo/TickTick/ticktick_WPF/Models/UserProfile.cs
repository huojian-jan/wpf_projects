// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.UserProfile
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Models
{
  [Serializable]
  public class UserProfile
  {
    public string userCode { get; set; }

    public string avatarUrl { get; set; }

    public string email { get; set; }

    public string name { get; set; }

    public string username { get; set; }

    public bool? isMyself { get; set; }
  }
}
