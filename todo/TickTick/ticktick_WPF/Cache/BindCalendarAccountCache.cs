// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.BindCalendarAccountCache
// Assembly: TickTick, Version=5.3.0.2, Culture=neutral, PublicKeyToken=null
// MVID: 7E33C365-38DF-41BD-A128-B002B0ADD403
// Assembly location: C:\Program Files (x86)\滴答清单\TickTick.exe

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ticktick_WPF.Dal;
using ticktick_WPF.Models;

#nullable disable
namespace ticktick_WPF.Cache
{
  public class BindCalendarAccountCache : CacheBase<BindCalendarAccountModel>
  {
    public override async Task Load()
    {
      this.AssembleData((IEnumerable<BindCalendarAccountModel>) await BindCalendarAccountDao.GetBindCalendarAccounts(), (Func<BindCalendarAccountModel, string>) (account => account.Id));
    }
  }
}
