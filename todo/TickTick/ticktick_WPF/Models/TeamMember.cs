// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TeamMember
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TeamMember
  {
    public int role { get; set; }

    public string userCode { get; set; }

    public string displayName { get; set; }

    public string avatarUrl { get; set; }

    public bool isMyself { get; set; }

    public string email { get; set; }

    public string nickname { get; set; }

    public DateTime joinedTime { get; set; }

    public DateTime becameAdminTime { get; set; }
  }
}
