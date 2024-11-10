// Decompiled with JetBrains decompiler
// Type: ticktick_WPF.Cache.TeamCache
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
  public class TeamCache : CacheBase<TeamModel>
  {
    private bool _loaded;

    public override async Task Load()
    {
      TeamCache teamCache = this;
      if (teamCache._loaded)
        return;
      teamCache._loaded = true;
      List<TeamModel> allTeams = await TeamDao.GetAllTeams();
      teamCache.AssembleData((IEnumerable<TeamModel>) allTeams, (Func<TeamModel, string>) (team => team.id));
    }
  }
}
