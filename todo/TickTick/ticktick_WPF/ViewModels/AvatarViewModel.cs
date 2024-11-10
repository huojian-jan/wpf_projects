// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.ViewModels.AvatarViewModel
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;

#nullable disable
namespace ticktick_WPF.ViewModels
{
  public class AvatarViewModel : BaseViewModel
  {
    public string AvatarUrl { get; set; }

    public string Name { get; set; }

    public string UserId { get; set; }

    public string UserCode { get; set; }

    public bool IsNoAvatar { get; set; }

    public DateTime Date { get; set; }
  }
}
