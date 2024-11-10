// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Models.TeamMessageModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.Models
{
  public class TeamMessageModel
  {
    public string id { get; set; }

    public string name { get; set; }

    public DateTime createdTime { get; set; }

    public DateTime modifiedTime { get; set; }

    public DateTime expireDate { get; set; }

    public int seat { get; set; }

    public int price { get; set; }

    public int planCount { get; set; }

    public bool trial { get; set; }

    public int role { get; set; }
  }
}
